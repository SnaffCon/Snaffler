using System;
using System.Collections.Concurrent;
using SnaffCore.Config;
using SnaffCore.ShareFind;
using SnaffCore.ShareScan;

namespace SnaffCore.Concurrency
{
    public class BlockingMq
    {
        // Message Queue
        public BlockingCollection<SnafflerMessage> Q { get; private set; }

        public BlockingMq()
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

        internal void FileResult(FileScanner.FileResult fileResult)
        {
            // say we did a thing
            Q.Add(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                Type = SnafflerMessageType.FileResult,
                FileResult = fileResult
            });
        }

        internal void ShareResult(ShareFinder.ShareResult shareResult)
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