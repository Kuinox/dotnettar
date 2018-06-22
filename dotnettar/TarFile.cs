using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dotnettar
{
    class TarStream : Stream
    {
        public delegate TarFileStream NextFile();

        readonly NextFile _callback;
        long _position;
        long _startLastStream;
        TarFileStream _actualStream;
        public TarStream(NextFile callback)
        {
            _callback = callback;
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
        Stream StreamToRead()
        {

        }

        public override int Read( byte[] buffer, int offset, int count )
        {
            if(_actualStream==null)
            {
                _actualStream = _callback();
            }
            if(_position+count>_startLastStream+_actualStream.Length)
            {

            }
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
