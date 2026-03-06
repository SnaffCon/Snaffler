using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using SnaffCore.Concurrency;

namespace SnaffCore.Checkpoint
{
    /// <summary>
    /// Thread-safe singleton that tracks scan progress and handles periodic
    /// checkpointing to disk.  Use <see cref="Initialize"/> before starting a
    /// scan and <see cref="GetInstance"/> everywhere else.
    /// </summary>
    public class CheckpointManager
    {
        // ------------------------------------------------------------------ //
        //  Singleton plumbing                                                  //
        // ------------------------------------------------------------------ //

        private static volatile CheckpointManager _instance;
        private static readonly object _createLock = new object();

        public static CheckpointManager GetInstance() => _instance;

        /// <summary>
        /// Create (and optionally restore) the singleton.
        /// Call once during startup before <see cref="SnaffCore.SnaffCon"/> is
        /// constructed.
        /// </summary>
        /// <param name="checkpointFilePath">Path to write / read checkpoint JSON.</param>
        public static CheckpointManager Initialize(string checkpointFilePath)
        {
            lock (_createLock)
            {
                _instance = new CheckpointManager(checkpointFilePath);
                return _instance;
            }
        }

        // ------------------------------------------------------------------ //
        //  State                                                               //
        // ------------------------------------------------------------------ //

        // Use ConcurrentDictionary as a thread-safe HashSet.
        private readonly ConcurrentDictionary<string, byte> _scannedDirectories
            = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, byte> _scannedComputers
            = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        private readonly string _filePath;
        private readonly object _saveLock = new object();

        // ------------------------------------------------------------------ //
        //  Public properties                                                   //
        // ------------------------------------------------------------------ //

        public string FilePath => _filePath;
        public bool IsRestoring { get; private set; }

        /// <summary>How many directories are recorded in this session so far.</summary>
        public int ScannedDirectoryCount => _scannedDirectories.Count;
        /// <summary>How many computers are recorded in this session so far.</summary>
        public int ScannedComputerCount => _scannedComputers.Count;

        // ------------------------------------------------------------------ //
        //  Constructor                                                         //
        // ------------------------------------------------------------------ //

        private CheckpointManager(string filePath)
        {
            _filePath = filePath;

            if (File.Exists(filePath))
            {
                TryLoad();
            }
        }

        // ------------------------------------------------------------------ //
        //  Directory tracking                                                  //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns true if this directory has already been processed in a
        /// previous (or the current) session.
        /// </summary>
        public bool IsDirectoryScanned(string path)
        {
            return !string.IsNullOrWhiteSpace(path) &&
                   _scannedDirectories.ContainsKey(NormalisePath(path));
        }

        /// <summary>Mark a directory as having been entered / processed.</summary>
        public void MarkDirectoryScanned(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
                _scannedDirectories.TryAdd(NormalisePath(path), 0);
        }

        // ------------------------------------------------------------------ //
        //  Computer tracking                                                   //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns true if this computer's shares have already been discovered
        /// in a previous session.
        /// </summary>
        public bool IsComputerScanned(string computer)
        {
            return !string.IsNullOrWhiteSpace(computer) &&
                   _scannedComputers.ContainsKey(NormaliseHost(computer));
        }

        /// <summary>Mark a computer as having had its shares discovered.</summary>
        public void MarkComputerScanned(string computer)
        {
            if (!string.IsNullOrWhiteSpace(computer))
                _scannedComputers.TryAdd(NormaliseHost(computer), 0);
        }

        // ------------------------------------------------------------------ //
        //  Persistence                                                         //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Atomically write a checkpoint file to disk.  Safe to call from any
        /// thread – surplus concurrent calls are serialised by a lock so no
        /// data is lost.
        /// </summary>
        public void SaveCheckpoint()
        {
            lock (_saveLock)
            {
                try
                {
                    var data = new CheckpointData
                    {
                        CheckpointTime = DateTime.UtcNow,
                        ScannedDirectories = new System.Collections.Generic.List<string>(_scannedDirectories.Keys),
                        ScannedComputers   = new System.Collections.Generic.List<string>(_scannedComputers.Keys),
                    };

                    string json = Serialise(data);

                    // Write to a temp file first, then rename – avoids a
                    // corrupt checkpoint if the process is killed mid-write.
                    string tmp = _filePath + ".tmp";
                    File.WriteAllText(tmp, json, Encoding.UTF8);
                    File.Copy(tmp, _filePath, overwrite: true);
                    File.Delete(tmp);

                    BlockingMq.GetMq()?.Info(
                        string.Format("[Checkpoint] Saved checkpoint ({0} dirs, {1} computers) → {2}",
                            data.ScannedDirectories.Count,
                            data.ScannedComputers.Count,
                            _filePath));
                }
                catch (Exception ex)
                {
                    BlockingMq.GetMq()?.Error("[Checkpoint] Failed to save checkpoint: " + ex.Message);
                }
            }
        }

        // ------------------------------------------------------------------ //
        //  Private helpers                                                     //
        // ------------------------------------------------------------------ //

        private void TryLoad()
        {
            try
            {
                string json = File.ReadAllText(_filePath, Encoding.UTF8);
                CheckpointData data = Deserialise(json);
                if (data == null) return;

                foreach (string d in data.ScannedDirectories ?? new System.Collections.Generic.List<string>())
                    _scannedDirectories.TryAdd(NormalisePath(d), 0);

                foreach (string c in data.ScannedComputers ?? new System.Collections.Generic.List<string>())
                    _scannedComputers.TryAdd(NormaliseHost(c), 0);

                // Deduplication: remove child-directory entries whose parent is
                // also in the completed set.  If a parent dir is marked complete,
                // WalkTree will skip it entirely — the child entries are dead weight
                // that will never be checked.  Pruning them here keeps the in-memory
                // set lean and speeds up future IsDirectoryScanned lookups.
                // Example: if both \\srv\share AND \\srv\share\sub are present,
                // \\srv\share\sub is redundant and can be removed.
                var toRemove = new System.Collections.Generic.List<string>();
                foreach (string dir in _scannedDirectories.Keys)
                {
                    // Check if any OTHER entry is a proper prefix of this one.
                    // Use the normalised (uppercased, trimmed) form for comparison.
                    foreach (string other in _scannedDirectories.Keys)
                    {
                        if (other == dir) continue;
                        // 'other' is a parent if 'dir' starts with 'other\' or 'other/'
                        if (dir.StartsWith(other + "\\", StringComparison.OrdinalIgnoreCase) ||
                            dir.StartsWith(other + "/",  StringComparison.OrdinalIgnoreCase))
                        {
                            toRemove.Add(dir);
                            break;
                        }
                    }
                }
                foreach (string redundant in toRemove)
                {
                    byte dummy;
                    _scannedDirectories.TryRemove(redundant, out dummy);
                }

                IsRestoring = true;

                Console.WriteLine(string.Format(
                    "[Checkpoint] Loaded checkpoint from {0} (written {1} UTC).",
                    _filePath,
                    data.CheckpointTime.ToString("u")));
                Console.WriteLine(string.Format(
                    "[Checkpoint] Resuming – will skip {0} directories and {1} computers ({2} redundant dir entries pruned).",
                    _scannedDirectories.Count,
                    _scannedComputers.Count,
                    toRemove.Count));
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Checkpoint] WARNING – could not load checkpoint (" + ex.Message + "). Starting fresh.");
                IsRestoring = false;
            }
        }

        private static string NormalisePath(string p) =>
            p.TrimEnd('\\', '/').ToUpperInvariant();

        private static string NormaliseHost(string h) =>
            h.Trim().ToLowerInvariant();

        // Use DataContractJsonSerializer – no extra NuGet dependency required.
        private static string Serialise(CheckpointData data)
        {
            var ser = new DataContractJsonSerializer(typeof(CheckpointData));
            using (var ms = new MemoryStream())
            {
                ser.WriteObject(ms, data);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private static CheckpointData Deserialise(string json)
        {
            var ser = new DataContractJsonSerializer(typeof(CheckpointData));
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            using (var ms = new MemoryStream(bytes))
            {
                return (CheckpointData)ser.ReadObject(ms);
            }
        }
    }
}
