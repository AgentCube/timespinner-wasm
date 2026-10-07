// ZlibNetStub.cs — Managed drop-in replacement for ZlibNet.dll using .NET 8 System.IO.Compression.ZLibStream.
// Implements RFC 1950 zlib compression and decompression without external C libraries.

using System;
using System.IO;
using System.IO.Compression;

namespace ZlibNet
{
    public class ZStreamException : Exception
    {
        public ZStreamException() { }
        public ZStreamException(string message) : base(message) { }
        public ZStreamException(string message, Exception inner) : base(message, inner) { }
    }

    public class ZInOutStream : Stream
    {
        private readonly Stream _underlyingStream;
        private readonly ZLibStream _zlibStream;
        private readonly bool _isWriting;

        public ZInOutStream(Stream stream)
        {
            _underlyingStream = stream ?? throw new ArgumentNullException(nameof(stream));
            _isWriting = false;
            try
            {
                _zlibStream = new ZLibStream(stream, CompressionMode.Decompress, leaveOpen: true);
            }
            catch (Exception ex)
            {
                throw new ZStreamException("Failed to initialize ZLib decompression stream", ex);
            }
        }

        public ZInOutStream(Stream stream, int level)
        {
            _underlyingStream = stream ?? throw new ArgumentNullException(nameof(stream));
            _isWriting = true;
            try
            {
                _zlibStream = new ZLibStream(stream, CompressionLevel.Optimal, leaveOpen: true);
            }
            catch (Exception ex)
            {
                throw new ZStreamException("Failed to initialize ZLib compression stream", ex);
            }
        }

        public override bool CanRead => !_isWriting && _zlibStream.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => _isWriting && _zlibStream.CanWrite;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
            _zlibStream?.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                return _zlibStream.Read(buffer, offset, count);
            }
            catch (Exception ex)
            {
                throw new ZStreamException("Error reading ZLib stream", ex);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                _zlibStream.Write(buffer, offset, count);
            }
            catch (Exception ex)
            {
                throw new ZStreamException("Error writing ZLib stream", ex);
            }
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _zlibStream?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
