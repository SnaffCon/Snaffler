using System;
using System.Collections.Concurrent;
using Classifiers;
using SnaffCore.Config;
using SnaffCore.ShareFind;
using SnaffCore.TreeWalk;

namespace SnaffCore.Concurrency
{
    public class BlockingMq
    {
        private static BlockingMq _instance;

        public static void MakeMq()
        {
            _instance = new BlockingMq();
        }

        public static BlockingMq GetMq()
        {
            return _instance;
        }

        // Message Queue
        public BlockingCollection<SnafflerMessage> Q { get; private set; }

        private BlockingMq()
        {
            Q = new BlockingCollection<SnafflerMessage>();
        }

        internal void Terminate()
        {
            // say we did a thing
            Q.Add(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                Type = SnafflerMessageType.Fatal,
                Message = "Terminate was called"
            });
            //this.Q.CompleteAdding();
        }

        internal void Trace(string message)
        {
            // say we did a thing
            Q.Add(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                Type = SnafflerMessageType.Trace,
                Message = message
            });
        }

        internal void Degub(string message)
        {
            // say we did a thing
            Q.Add(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                Type = SnafflerMessageType.Degub,
                Message = message
            });
        }

        internal void Info(string message)
        {
            // say we did a thing
            Q.Add(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                Type = SnafflerMessageType.Info,
                Message = message
            });
        }

        internal void Error(string message)
        {
            Q.Add(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                Type = SnafflerMessageType.Error,
                Message = message
            });
        }

        internal void FileResult(FileResult fileResult)
        {
            // say we did a thing
            Q.Add(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                Type = SnafflerMessageType.FileResult,
                FileResult = fileResult
            });
        }
        internal void DirResult(DirResult dirResult)
        {
            // say we did a thing
            Q.Add(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                DirResult = dirResult,
                Type = SnafflerMessageType.ShareResult
            });
        }
        internal void ShareResult(ShareResult shareResult)
        {
            // say we did a thing
            Q.Add(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                ShareResult = shareResult,
                Type = SnafflerMessageType.ShareResult
            });
        }
    }
}