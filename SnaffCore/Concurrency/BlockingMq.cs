using SnaffCore.Classifiers;
using System;
using System.Collections.Concurrent;
using System.Threading.Channels;

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
        private Channel<SnafflerMessage> _channel;
        public ChannelReader<SnafflerMessage> Reader => _channel.Reader;

        private BlockingMq()
        {
            _channel = Channel.CreateUnbounded<SnafflerMessage>();
        }

        private void Enqueue(SnafflerMessage message)
        {
            _channel.Writer.TryWrite(message);
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
            _channel.Writer.TryComplete();
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
            _channel.Writer.TryComplete();
        }
    }
}