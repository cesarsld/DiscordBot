using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace BHungerGaemsBot
{
    public class ThreadSafeFileBuffer<T> : IDisposable
    {
        private readonly FileStream _fileStream;
        private readonly StreamWriter _writer;
        private readonly ConcurrentQueue<T> _buffer = new ConcurrentQueue<T>();
        private readonly Timer _timer;

        public ThreadSafeFileBuffer(string filePath, int flushPeriodInSeconds = 5)
        {
            _fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(_fileStream);
            var flushPeriod = TimeSpan.FromSeconds(flushPeriodInSeconds);
            _timer = new Timer(FlushBuffer, null, flushPeriod, flushPeriod);
        }

        public void WriteLine(T result)
        {
            _buffer.Enqueue(result);
        }

        public void Dispose()
        {
            _timer.Dispose();
            FlushBuffer(); // flush anything left over in the buffer
            _writer.Dispose();
            _fileStream.Dispose();
        }

        /// <summary>
        /// Since this is only done by one thread at a time (almost always the background flush thread, but one time via Dispose), no need to lock
        /// </summary>
        /// <param name="unused"></param>
        private void FlushBuffer(object unused = null)
        {
            T current;
            while (_buffer.TryDequeue(out current))
            {
                _writer.WriteLine(current);
            }
            _writer.Flush();
        }
    }
}
