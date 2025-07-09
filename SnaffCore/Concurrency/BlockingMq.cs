using SnaffCore.Classifiers;
using System;
using System.Collections.Concurrent;
#if NETFRAMEWORK
#else
using System.Threading.Channels;
#endif

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
#if NETFRAMEWORK
        private BlockingCollection<SnafflerMessage> _collection;
        public BlockingCollection<SnafflerMessage> Collection => _collection;
#else
        private Channel<SnafflerMessage> _channel;
        public ChannelReader<SnafflerMessage> Reader => _channel.Reader;
#endif

        private BlockingMq()
        {
#if NETFRAMEWORK
            _collection = new BlockingCollection<SnafflerMessage>();
#else
            _channel = Channel.CreateUnbounded<SnafflerMessage>();
#endif
        }

        private void Enqueue(SnafflerMessage message)
        {
#if NETFRAMEWORK
            _collection.Add(message);
#else
            _channel.Writer.TryWrite(message);
#endif
        }

        public void Terminate()
        {
            // say we did a thing
            Enqueue(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                Type = SnafflerMessageType.Fatal,
                Message = "Terminate was called"
            });
#if NETFRAMEWORK
            _collection.CompleteAdding();
#else
            _channel.Writer.TryComplete();
#endif
        }

        public void Trace(string message)
        {
            // say we did a thing
            Enqueue(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                Type = SnafflerMessageType.Trace,
                Message = message
            });
        }

        public void Degub(string message)
        {
            // say we did a thing
            Enqueue(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                Type = SnafflerMessageType.Degub,
                Message = message
            });
        }

        public void Info(string message)
        {
            // say we did a thing
            Enqueue(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                Type = SnafflerMessageType.Info,
                Message = message
            });
        }

        public void Error(string message)
        {
            Enqueue(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                Type = SnafflerMessageType.Error,
                Message = message
            });
        }

        public void FileResult(FileResult fileResult)
        {
            // say we did a thing
            Enqueue(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                Type = SnafflerMessageType.FileResult,
                FileResult = fileResult
            });
        }
        public void DirResult(DirResult dirResult)
        {
            // say we did a thing
            Enqueue(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                DirResult = dirResult,
                Type = SnafflerMessageType.DirResult
            });
        }
        public void ShareResult(ShareResult shareResult)
        {
            // say we did a thing
            Enqueue(new SnafflerMessage
            {
                DateTime = DateTime.Now,
                ShareResult = shareResult,
                Type = SnafflerMessageType.ShareResult
            });
        }

        public void Finish()
        {
            Enqueue(new SnafflerMessage()
            {
                DateTime = DateTime.Now,
                Type = SnafflerMessageType.Finish
            });
#if NETFRAMEWORK
            _collection.CompleteAdding();
#else
            _channel.Writer.TryComplete();
#endif
        }
    }
}
