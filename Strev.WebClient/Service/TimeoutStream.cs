using System;
using System.IO;

namespace Strev.WebClient.Service
{
    internal class TimeoutStream : Stream
    {
        private long _position;
        private readonly Stream _stream;
        private readonly TimeSpan _timeout;

        public TimeoutStream(Stream internalStream, TimeSpan timeout)
        {
            _position = 0;
            _stream = internalStream;
            _timeout = timeout;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override void Flush() => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position
        {
            get => _position;
            set => throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var reader =
               new AsyncPrimitive<int>(
                   (asc, obj) => _stream.BeginRead(buffer, offset, count, asc, obj),
                   _stream.EndRead
                   );

            int numread = reader.ExecuteSync(_timeout);
            _position += numread;
            return numread;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
    }
}