using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SnaffCore.Checkpoint
{
    /// <summary>
    /// Serializable state snapshot for checkpoint/resume support.
    /// Tracks which directories and computers have already been processed so
    /// a resumed run can skip them entirely.
    /// </summary>
    [DataContract]
    public class CheckpointData
    {
        /// <summary>When this checkpoint was written.</summary>
        [DataMember]
        public DateTime CheckpointTime { get; set; }

        /// <summary>
        /// Full UNC / local paths of every directory whose tree-walk has been
        /// fully dispatched.  On resume, any path in this set is skipped by TreeWalker.
        /// </summary>
        [DataMember]
        public List<string> ScannedDirectories { get; set; } = new List<string>();

        /// <summary>
        /// Hostnames / IPs of every computer whose share-discovery has been
        /// completed.  On resume, any computer in this set is skipped by
        /// ShareFinder.
        /// </summary>
        [DataMember]
        public List<string> ScannedComputers { get; set; } = new List<string>();
    }
}
