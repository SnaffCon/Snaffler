using System;
using System.Collections.Generic;
using System.IO;

namespace SnaffCore.SCCM
{
    public class SCCMFileMetadata
    {
        public string OriginalName { get; set; }
        public string PackageId { get; set; }
        public string Version { get; set; }
        public string Hash { get; set; }
        public string ContentType { get; set; }  // "FileLib", "DataLib", "PkgLib", "Legacy"
    }

    public static class SCCMFileMapping
    {
        private static Dictionary<string, string> _hashToFilename = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> _pathToOriginalName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, SCCMFileMetadata> _fileMetadata = new Dictionary<string, SCCMFileMetadata>(StringComparer.OrdinalIgnoreCase);
        private static object _lock = new object();
        
        public static void AddMapping(string hashValue, string originalFilename)
        {
            if (string.IsNullOrEmpty(hashValue) || string.IsNullOrEmpty(originalFilename))
                return;
                
            lock (_lock)
            {
                _hashToFilename[hashValue] = originalFilename;
            }
        }
        
        public static void AddPathMapping(string fullPath, string originalName)
        {
            if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(originalName))
                return;
                
            lock (_lock)
            {
                _pathToOriginalName[fullPath.ToLower()] = originalName;
            }
        }
        
        public static string GetOriginalFilename(string hashValue)
        {
            if (string.IsNullOrEmpty(hashValue))
                return null;
                
            lock (_lock)
            {
                return _hashToFilename.TryGetValue(hashValue, out string filename) ? filename : null;
            }
        }
        
        public static string GetOriginalFilenameFromPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return null;
                
            lock (_lock)
            {
                if (_pathToOriginalName.TryGetValue(fullPath.ToLower(), out string filename))
                    return filename;
            }
            

            string fileName = Path.GetFileName(fullPath);
            if (!string.IsNullOrEmpty(fileName))
            {

                string hashValue = Path.GetFileNameWithoutExtension(fileName);
                return GetOriginalFilename(hashValue);
            }
            
            return null;
        }
        
        public static string FormatPathWithOriginalName(string fullPath)
        {
            string originalName = GetOriginalFilenameFromPath(fullPath);
            
            if (!string.IsNullOrEmpty(originalName))
            {

                return $"{fullPath}({originalName})";
            }
            
            return fullPath;
        }
        
        public static bool IsSCCMHashFile(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return false;
                

            if (!fullPath.Contains("FileLib") && !fullPath.Contains("DataLib"))
                return false;
                

            string fileName = Path.GetFileNameWithoutExtension(fullPath);
            if (fileName.Length >= 32 && fileName.Length <= 64)
            {
                foreach (char c in fileName)
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
            
            return false;
        }
        
        public static void Clear()
        {
            lock (_lock)
            {
                _hashToFilename.Clear();
                _pathToOriginalName.Clear();
                _fileMetadata.Clear();
            }
        }
        
        public static string GetStatistics()
        {
            lock (_lock)
            {
                return $"Hash mappings: {_hashToFilename.Count}, Path mappings: {_pathToOriginalName.Count}, Metadata entries: {_fileMetadata.Count}";
            }
        }

        public static void AddMetadata(string fullPath, SCCMFileMetadata metadata)
        {
            if (string.IsNullOrEmpty(fullPath) || metadata == null)
                return;

            lock (_lock)
            {
                _fileMetadata[fullPath.ToLower()] = metadata;
            }
        }

        public static SCCMFileMetadata GetMetadata(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return null;

            lock (_lock)
            {
                return _fileMetadata.TryGetValue(fullPath.ToLower(), out SCCMFileMetadata metadata) ? metadata : null;
            }
        }
    }
}