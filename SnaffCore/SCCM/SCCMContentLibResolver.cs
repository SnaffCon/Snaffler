using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SnaffCore.Concurrency;

namespace SnaffCore.SCCM
{
    public class SCCMContentLibResolver
    {
        private BlockingMq Mq { get; set; }
        private bool _debug;
        

        private readonly string[] SCCMSharePatterns = new string[]
        {
            "SCCMContentLib$",
            "SCCMContentLib",
            "SMS_DP$",
            "SMS_DistributionPoint$",
            "ContentLib$",
            "SMSPKGD$",
            "SMSPKGE$",
            "SMSPKGF$",
            "SMSSIG$",
            "SMS_CPSC$",
            "SMS_SITE$",
            "SMS_PKG$",
            "SMSPKG$",
            "SCCMContentLib_",
            "PkgLib$",
            "DataLib$",
            "FileLib$"
        };
        

        private LRUCache<string, string> _fileLibCache;
        

        private const int MAX_RECURSION_DEPTH = 10;
        private const int MAX_CACHE_SIZE = 50000;
        private const long MAX_CACHE_MEMORY_MB = 100;
        private const int MAX_PARALLEL_PACKAGES = 4;
        

        private CancellationTokenSource _cancellationTokenSource;

        public SCCMContentLibResolver(bool debug = false)
        {
            Mq = BlockingMq.GetMq();
            _debug = debug;
            _fileLibCache = new LRUCache<string, string>(MAX_CACHE_SIZE, MAX_CACHE_MEMORY_MB);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public bool IsSCCMShare(string sharePath)
        {

            string shareNameOnly = sharePath;
            

            if (sharePath.StartsWith(@"\\"))
            {
                var parts = sharePath.TrimStart('\\').Split('\\');
                if (parts.Length >= 2)
                {
                    shareNameOnly = parts[1];
                }
            }
            else
            {

                shareNameOnly = Path.GetFileName(sharePath.TrimEnd('\\', '/'));
            }
            

            foreach (var pattern in SCCMSharePatterns)
            {
                if (shareNameOnly.Equals(pattern, StringComparison.OrdinalIgnoreCase) ||
                    shareNameOnly.Equals(pattern.TrimEnd('$'), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            

            if (shareNameOnly.Equals("ADMIN$", StringComparison.OrdinalIgnoreCase))
            {

                return CheckAdminShareForSCCM(sharePath);
            }
            
            return false;
        }

        public List<string> ResolveSCCMContentPaths(string sharePath)
        {
            List<string> resolvedPaths = new List<string>();
            

            LoadFileLibMappings(sharePath);
            
            try
            {
                if (_debug)
                {
                    Mq.Degub($"Resolving SCCM ContentLib: {sharePath}");
                }
                

                string dataLibPath = Path.Combine(sharePath, "DataLib");
                if (!DirectoryExists(dataLibPath))
                {

                    dataLibPath = FindDataLibPath(sharePath);
                    if (string.IsNullOrEmpty(dataLibPath))
                    {
                        Mq.Degub($"[SCCM] No DataLib found in {sharePath}");
                        return resolvedPaths;
                    }
                }
                
                if (_debug)
                {
                    Mq.Degub($"Found DataLib at: {dataLibPath}");
                }
                



                

                var dataLibFiles = GetAllFilesRecursive(dataLibPath, 0);
                int processedCount = 0;
                
                if (_debug)
                {
                    Mq.Degub($"Found {dataLibFiles.Count} files in DataLib");
                }
                

                string fileLibPath = FindFileLibPath(sharePath);
                bool hasFileLib = !string.IsNullOrEmpty(fileLibPath) && DirectoryExists(fileLibPath);
                
                if (_debug && hasFileLib)
                {
                    Mq.Degub($"FileLib found at: {fileLibPath}");
                }
                
                foreach (var file in dataLibFiles)
                {
                    try
                    {
                        string fileName = Path.GetFileName(file);
                        

                        if (fileName.EndsWith(".INI", StringComparison.OrdinalIgnoreCase))
                        {

                            string iniContent = File.ReadAllText(file);


                            string originalFilename = fileName.Substring(0, fileName.Length - 4);

                            // Extract package ID and version from path (DataLib/{PackageID}/{Version}/)
                            string packageId = ExtractPackageIdFromPath(file);
                            string version = ExtractVersionFromPath(file);

                            var hashMatch = System.Text.RegularExpressions.Regex.Match(iniContent, @"Hash[^=]*=([A-F0-9]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            if (hashMatch.Success)
                            {
                                string hashValue = hashMatch.Groups[1].Value;


                                SCCMFileMapping.AddMapping(hashValue, originalFilename);

                                if (hasFileLib)
                                {

                                    string hashFolder = hashValue.Length >= 4 ? hashValue.Substring(0, 4) : hashValue;
                                    string contentPath = Path.Combine(fileLibPath, hashFolder, hashValue);

                                    if (File.Exists(contentPath))
                                    {

                                        SCCMFileMapping.AddPathMapping(contentPath, originalFilename);

                                        // Store metadata for this resolved file
                                        SCCMFileMapping.AddMetadata(contentPath, new SCCMFileMetadata
                                        {
                                            OriginalName = originalFilename,
                                            PackageId = packageId,
                                            Version = version,
                                            Hash = hashValue,
                                            ContentType = "FileLib"
                                        });

                                        resolvedPaths.Add(contentPath);
                                        processedCount++;

                                        if (_debug && processedCount <= 10)
                                        {
                                            Mq.Degub($"Resolved: {originalFilename} -> FileLib/{hashFolder}/{hashValue}");
                                        }
                                    }
                                    else if (_debug && processedCount == 0)
                                    {
                                        Mq.Degub($"Hash {hashValue} not found in FileLib at {contentPath}");
                                    }
                                }
                            }

                            // Also scan the INI file itself for interesting content
                            SCCMFileMapping.AddMetadata(file, new SCCMFileMetadata
                            {
                                OriginalName = originalFilename,
                                PackageId = packageId,
                                Version = version,
                                ContentType = "DataLib-INI"
                            });

                            resolvedPaths.Add(file);
                            processedCount++;
                        }
                        else
                        {


                            resolvedPaths.Add(file);
                            processedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_debug)
                        {
                            Mq.Degub($"Error processing DataLib file {file}: {ex.Message}");
                        }
                    }
                }
                
                if (_debug)
                {
                    Mq.Degub($"Processed {processedCount} items from DataLib");
                }
                

                ProcessPkgLib(sharePath, resolvedPaths);
                

                ProcessAdditionalSCCMLocations(sharePath, resolvedPaths);
                

                if (resolvedPaths.Count == 0)
                {
                    if (_debug)
                    {
                        Mq.Degub($"No files resolved from ContentLib at {sharePath}");
                    }
                }
                else
                {
                    if (_debug)
                    {
                        Mq.Degub($"Resolved {resolvedPaths.Count} files from ContentLib");
                    }
                }
                

                CheckSMSPKGFolders(sharePath, resolvedPaths);
                
            }
            catch (Exception ex)
            {
                Mq.Error($"Error resolving ContentLib: {ex.Message}");
                if (_debug)
                {
                    Mq.Error(ex.ToString());
                }
            }
            
            return resolvedPaths;
        }

        public Dictionary<string, List<string>> GroupFilesByDirectory(List<string> filePaths)
        {
            Dictionary<string, List<string>> directoryMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var filePath in filePaths)
            {
                try
                {
                    string directory = Path.GetDirectoryName(filePath);
                    if (string.IsNullOrEmpty(directory))
                        continue;

                    if (!directoryMap.ContainsKey(directory))
                    {
                        directoryMap[directory] = new List<string>();
                    }

                    directoryMap[directory].Add(filePath);
                }
                catch (Exception ex)
                {
                    if (_debug)
                    {
                        Mq.Degub($"Error grouping file {filePath}: {ex.Message}");
                    }
                }
            }

            return directoryMap;
        }


        private void LoadFileLibMappings(string sharePath)
        {
            try
            {
                string fileLibPath = FindFileLibPath(sharePath);
                if (string.IsNullOrEmpty(fileLibPath))
                {
                    Mq.Degub("[SCCM] No FileLib found for filename resolution");
                    return;
                }
                
                if (_debug)
                {
                    Mq.Degub($"Loading FileLib mappings from: {fileLibPath}");
                }
                

                var hashFolders = GetDirectories(fileLibPath);
                int mappingCount = 0;
                object lockObj = new object();
                

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = MAX_PARALLEL_PACKAGES,
                    CancellationToken = _cancellationTokenSource.Token
                };
                
                try
                {
                    Parallel.ForEach(hashFolders, parallelOptions, hashFolder =>
                    {
                        string hashValue = Path.GetFileName(hashFolder);
                        

                        var iniFiles = Directory.EnumerateFiles(hashFolder, "*.INI").Take(5);
                        
                        foreach (var iniFile in iniFiles)
                        {
                            try
                            {

                                string content = File.ReadAllText(iniFile);
                                

                                string originalName = ExtractOriginalFileNameOptimized(content);
                                
                                if (!string.IsNullOrEmpty(originalName))
                                {
                                    _fileLibCache.Add(hashValue, originalName);
                                    
                                    lock (lockObj)
                                    {
                                        mappingCount++;
                                    }
                                    
                                    if (_debug && mappingCount % 100 == 0)
                                    {
                                        Mq.Degub($"[SCCM] Loaded {mappingCount} mappings so far...");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                if (_debug)
                                {
                                    Mq.Degub($"[SCCM] Error parsing INI file {iniFile}: {ex.Message}");
                                }
                            }
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                    if (_debug)
                    {
                        Mq.Degub("FileLib loading cancelled");
                    }
                }
                
                if (_debug)
                {
                    Mq.Degub($"Loaded {mappingCount} FileLib mappings");
                }
            }
            catch (Exception ex)
            {
                if (_debug)
                {
                    Mq.Degub($"Error loading FileLib mappings: {ex.Message}");
                }
            }
        }

        private string ExtractOriginalFileNameOptimized(string iniContent)
        {

            using (var reader = new StringReader(iniContent))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {

                    if (line.Length < 10) continue;
                    

                    if (line.StartsWith("FileName=", StringComparison.OrdinalIgnoreCase))
                    {
                        return ExtractFileNameFromLine(line, 9);
                    }
                    else if (line.StartsWith("OriginalName=", StringComparison.OrdinalIgnoreCase))
                    {
                        return ExtractFileNameFromLine(line, 13);
                    }
                    else if (line.StartsWith("SourcePath=", StringComparison.OrdinalIgnoreCase))
                    {
                        return ExtractFileNameFromLine(line, 11);
                    }
                    else if (line.StartsWith("RelativePath=", StringComparison.OrdinalIgnoreCase))
                    {
                        return ExtractFileNameFromLine(line, 13);
                    }
                }
            }
            
            return null;
        }
        
        private string ExtractFileNameFromLine(string line, int prefixLength)
        {
            if (line.Length <= prefixLength) return null;
            
            string value = line.Substring(prefixLength).Trim();
            

            if (value.Contains("\\"))
            {
                int lastSlash = value.LastIndexOf('\\');
                if (lastSlash >= 0 && lastSlash < value.Length - 1)
                {
                    value = value.Substring(lastSlash + 1);
                }
            }
            
            return string.IsNullOrEmpty(value) ? null : value;
        }
        
        private string ExtractOriginalFileName(string iniContent)
        {

            string[] patterns = new string[]
            {
                "FileName=",
                "OriginalName=",
                "SourcePath=",
                "RelativePath="
            };
            
            var lines = iniContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                foreach (var pattern in patterns)
                {
                    if (line.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        string value = line.Substring(pattern.Length).Trim();
                        

                        if (value.Contains("\\"))
                        {
                            value = Path.GetFileName(value);
                        }
                        
                        if (!string.IsNullOrEmpty(value))
                        {
                            return value;
                        }
                    }
                }
            }
            
            return null;
        }


        private void ProcessPkgLib(string sharePath, List<string> resolvedPaths)
        {
            try
            {
                string pkgLibPath = FindPkgLibPath(sharePath);
                if (string.IsNullOrEmpty(pkgLibPath))
                {
                    if (_debug)
                    {
                        Mq.Degub("No PkgLib found");
                    }
                    return;
                }
                
                if (_debug)
                {
                    Mq.Degub($"Processing PkgLib at: {pkgLibPath}");
                }
                

                string dataLibPath = FindDataLibPath(sharePath);
                if (string.IsNullOrEmpty(dataLibPath))
                {
                    if (_debug)
                    {
                        Mq.Degub("No DataLib found, skipping PkgLib content resolution");
                    }
                    return;
                }
                


                var iniFiles = Directory.GetFiles(pkgLibPath, "*.INI", SearchOption.TopDirectoryOnly);
                int contentCount = 0;
                
                foreach (var iniFile in iniFiles)
                {
                    try
                    {

                        string iniContent = File.ReadAllText(iniFile);
                        string packageId = Path.GetFileNameWithoutExtension(iniFile);
                        

                        var contentHashes = ExtractContentHashes(iniContent);
                        
                        if (_debug && contentHashes.Count > 0)
                        {
                            Mq.Degub($"Package {packageId} references {contentHashes.Count} content items");
                        }
                        

                        foreach (var hash in contentHashes)
                        {


                            string hashFolder = hash.Length >= 4 ? hash.Substring(0, 4) : hash;
                            string contentPath = Path.Combine(dataLibPath, hashFolder, hash);
                            

                            if (File.Exists(contentPath))
                            {
                                resolvedPaths.Add(contentPath);
                                contentCount++;
                                
                                if (_debug && contentCount <= 5)
                                {
                                    Mq.Degub($"Resolved content: {hash} from package {packageId}");
                                }
                            }
                            else
                            {

                                contentPath = Path.Combine(dataLibPath, hash);
                                if (File.Exists(contentPath))
                                {
                                    resolvedPaths.Add(contentPath);
                                    contentCount++;
                                }
                                else if (_debug && contentCount == 0)
                                {

                                    Mq.Degub($"Content hash {hash} not found in DataLib");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_debug)
                        {
                            Mq.Degub($"Error processing PkgLib INI {iniFile}: {ex.Message}");
                        }
                    }
                }
                
                if (_debug)
                {
                    Mq.Degub($"Resolved {contentCount} files from {iniFiles.Length} PkgLib packages");
                }
            }
            catch (Exception ex)
            {
                if (_debug)
                {
                    Mq.Degub($"Error processing PkgLib: {ex.Message}");
                }
            }
        }

        private void ProcessAdditionalSCCMLocations(string sharePath, List<string> resolvedPaths)
        {
            try
            {

                string parentPath = Path.GetDirectoryName(sharePath);
                if (string.IsNullOrEmpty(parentPath))
                {
                    parentPath = sharePath;
                }
                
                string[] additionalShares = new string[]
                {
                    "SMSSIG$",
                    "SMS_CPSC$",
                    "SMS_SITE$",
                    "SMS_PKG$",
                    "REMINST",
                    "WSUS",
                    "UpdateServicesPackages"
                };
                
                foreach (var share in additionalShares)
                {
                    string additionalPath = Path.Combine(parentPath, share);
                    
                    if (DirectoryExists(additionalPath))
                    {
                        if (_debug)
                        {
                            Mq.Degub($"Found additional share: {share}");
                        }
                        

                        var additionalFiles = GetAllFilesRecursive(additionalPath, 0);


                        // Add all files - let Snaffler's classifiers determine what's interesting
                        foreach (var file in additionalFiles)
                        {
                            resolvedPaths.Add(file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_debug)
                {
                    Mq.Degub($"Error processing additional locations: {ex.Message}");
                }
            }
        }

        private string FindFileLibPath(string sharePath)
        {
            string[] possiblePaths = new string[]
            {
                Path.Combine(sharePath, "FileLib"),
                Path.Combine(sharePath, "SCCMContentLib", "FileLib"),
                Path.Combine(sharePath, "ContentLib", "FileLib"),
                Path.Combine(sharePath, "SMS", "PKG", "FileLib"),
                Path.Combine(sharePath, "SMS_DP", "ContentLib", "FileLib")
            };
            
            foreach (var path in possiblePaths)
            {
                if (DirectoryExists(path))
                {
                    return path;
                }
            }
            
            return null;
        }

        private string FindPkgLibPath(string sharePath)
        {
            string[] possiblePaths = new string[]
            {
                Path.Combine(sharePath, "PkgLib"),
                Path.Combine(sharePath, "SCCMContentLib", "PkgLib"),
                Path.Combine(sharePath, "ContentLib", "PkgLib"),
                Path.Combine(sharePath, "SMS", "PKG", "PkgLib"),
                Path.Combine(sharePath, "SMS_DP", "ContentLib", "PkgLib")
            };
            
            foreach (var path in possiblePaths)
            {
                if (DirectoryExists(path))
                {
                    return path;
                }
            }
            
            return null;
        }

        private string FindDataLibPath(string sharePath)
        {
            string[] possiblePaths = new string[]
            {
                Path.Combine(sharePath, "DataLib"),
                Path.Combine(sharePath, "SCCMContentLib", "DataLib"),
                Path.Combine(sharePath, "SMS", "PKG", "DataLib"),
                Path.Combine(sharePath, "SMS_DP", "ContentLib", "DataLib"),
                Path.Combine(sharePath, "Program Files", "Microsoft Configuration Manager", "CMContentLib", "DataLib")
            };
            
            foreach (var path in possiblePaths)
            {
                if (DirectoryExists(path))
                {
                    return path;
                }
            }
            
            return null;
        }

        private bool CheckAdminShareForSCCM(string sharePath)
        {
            string[] sccmIndicators = new string[]
            {
                "SCCMContentLib",
                "SMS",
                "SMSPKG",
                "Microsoft Configuration Manager"
            };
            
            try
            {
                foreach (var indicator in sccmIndicators)
                {
                    string testPath = Path.Combine(sharePath, indicator);
                    if (DirectoryExists(testPath))
                    {
                        return true;
                    }
                }
            }
            catch
            {

            }
            
            return false;
        }

        private void CheckSMSPKGFolders(string sharePath, List<string> resolvedPaths)
        {
            string[] smspkgPatterns = new string[]
            {
                "SMSPKGD$",
                "SMSPKGE$",
                "SMSPKGF$"
            };
            
            foreach (var pattern in smspkgPatterns)
            {
                string smspkgPath = Path.Combine(sharePath, "..", pattern);
                if (DirectoryExists(smspkgPath))
                {
                    if (_debug)
                    {
                        Mq.Degub($"Found legacy package share: {pattern}");
                    }
                    
                    var packages = GetDirectories(smspkgPath);
                    foreach (var package in packages)
                    {
                        var files = GetAllFilesRecursive(package);
                        resolvedPaths.AddRange(files);
                    }
                }
            }
        }

        private List<string> GetAllFilesRecursive(string path, int currentDepth = 0)
        {
            List<string> files = new List<string>();
            

            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (_debug)
                {
                    Mq.Degub("Operation cancelled by user");
                }
                return files;
            }
            

            if (currentDepth >= MAX_RECURSION_DEPTH)
            {
                if (_debug)
                {
                    Mq.Degub($"Max recursion depth {MAX_RECURSION_DEPTH} reached at: {path}");
                }
                return files;
            }
            
            try
            {

                files.AddRange(Directory.EnumerateFiles(path));
                

                foreach (var subdir in Directory.EnumerateDirectories(path))
                {
                    files.AddRange(GetAllFilesRecursive(subdir, currentDepth + 1));
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                if (_debug)
                {
                    Mq.Degub($"[SCCM] Access denied in recursive enumeration of {path}: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                if (_debug)
                {
                    Mq.Degub($"[SCCM] Error in recursive enumeration of {path}: {ex.Message}");
                }
            }
            
            return files;
        }

        private List<string> GetDirectories(string path)
        {
            List<string> directories = new List<string>();
            
            try
            {

                directories.AddRange(Directory.GetDirectories(path));
            }
            catch (UnauthorizedAccessException ex)
            {
                if (_debug)
                {
                    Mq.Degub($"[SCCM] Access denied enumerating directories in {path}: {ex.Message}");
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                if (_debug)
                {
                    Mq.Degub($"[SCCM] Directory not found {path}: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                if (_debug)
                {
                    Mq.Degub($"[SCCM] Error enumerating directories in {path}: {ex.Message}");
                }
            }
            
            return directories;
        }

        private bool DirectoryExists(string path)
        {
            try
            {

                return Directory.Exists(path);
            }
            catch
            {

                return false;
            }
        }
        
        public void CancelOperations()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                if (_debug)
                {
                    Mq.Degub("Cancellation requested for ongoing operations");
                }
            }
            catch (Exception ex)
            {
                if (_debug)
                {
                    Mq.Degub($"Error during cancellation: {ex.Message}");
                }
            }
        }
        
        public string GetCacheStatistics()
        {
            if (_fileLibCache != null)
            {
                return $"Cache: {_fileLibCache.Count} items, {_fileLibCache.MemoryUsageBytes / 1024}KB memory";
            }
            return "Cache: Not initialized";
        }
        
        private List<string> ExtractContentHashes(string iniContent)
        {
            List<string> hashes = new List<string>();
            
            try
            {


                string[] patterns = new string[]
                {
                    "Content=",
                    "ContentID=",
                    "Hash=",
                    "DataLib=",
                    "ContentHash=",
                    "FileHash="
                };
                
                using (var reader = new StringReader(iniContent))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        foreach (var pattern in patterns)
                        {
                            if (line.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                            {
                                string value = line.Substring(pattern.Length).Trim();
                                

                                if (!string.IsNullOrEmpty(value) && 
                                    value.Length >= 32 && 
                                    value.Length <= 64 &&
                                    IsHexString(value))
                                {
                                    hashes.Add(value);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_debug)
                {
                    Mq.Degub($"Error extracting content hashes: {ex.Message}");
                }
            }
            
            return hashes;
        }
        
        private bool IsHexString(string value)
        {
            foreach (char c in value)
            {
                if (!((c >= '0' && c <= '9') ||
                      (c >= 'a' && c <= 'f') ||
                      (c >= 'A' && c <= 'F')))
                {
                    return false;
                }
            }
            return true;
        }

        private string ExtractPackageIdFromPath(string filePath)
        {
            try
            {
                // Path format: ...DataLib/{PackageID}/{Version}/file.INI
                string dirPath = Path.GetDirectoryName(filePath);
                if (string.IsNullOrEmpty(dirPath))
                    return null;

                // Get parent directory (version)
                string versionDir = Path.GetDirectoryName(dirPath);
                if (string.IsNullOrEmpty(versionDir))
                    return null;

                // Get grandparent directory (package ID)
                string packageDir = Path.GetFileName(versionDir);
                return packageDir;
            }
            catch
            {
                return null;
            }
        }

        private string ExtractVersionFromPath(string filePath)
        {
            try
            {
                // Path format: ...DataLib/{PackageID}/{Version}/file.INI
                string dirPath = Path.GetDirectoryName(filePath);
                if (string.IsNullOrEmpty(dirPath))
                    return null;

                // Get directory name (version)
                string version = Path.GetFileName(dirPath);
                return version;
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _fileLibCache?.Clear();
                _fileLibCache?.Dispose();
            }
            catch (Exception ex)
            {
                if (_debug)
                {
                    Mq.Degub($"Error during disposal: {ex.Message}");
                }
            }
        }

    }
}