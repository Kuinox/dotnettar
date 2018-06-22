using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dotnettar
{
    class TarStream : Stream
    {
        public delegate TarEntry NextFile();

        readonly NextFile _callback;
        long _position;
        long _startLastStream;
        TarEntry _actualStream;
        public TarStream(NextFile callback)
        {
            _callback = callback;
            _actualStream = _callback();
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => _position; set => throw new NotSupportedException(); }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        bool _readingHeader;

        public override int Read( byte[] buffer, int offset, int count )
        {
            long positionInActualStream = _position - _startLastStream;
            long toRead = count;
            if(positionInActualStream+count>_actualStream.Length)//Reading too much
            {
                toRead = _actualStream.Length - positionInActualStream;
            }
            int actualRead = _actualStream.Read(buffer, offset, (int)toRead);
            _position += actualRead;
            positionInActualStream = _position - _startLastStream;
            if(positionInActualStream == Length )//Stream completly read
            {
                _actualStream = _callback();
            }
            return actualRead;
        }

        public override long Seek( long offset, SeekOrigin origin )
        {
            throw new NotSupportedException();
        }

        public override void SetLength( long value )
        {
            throw new NotSupportedException();
        }

        public override void Write( byte[] buffer, int offset, int count )
        {
            throw new NotSupportedException();
        }
    }
}
