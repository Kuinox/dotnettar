using System;
using System.IO;

namespace dotnettar
{
    public class TarFile : Stream
    {
        public delegate TarEntry NextFile();

        readonly NextFile _callback;
        long _position;
        long _startLastStream;
        TarEntry _actualStream;
        bool _fillMode;
        int _positionFiller;
        public TarFile( NextFile callback )
        {
            _callback = callback;
            _actualStream = _callback();
        }
        public TarFile( TarEntry entry )
        {
            _callback = () => null;
            _actualStream = entry;
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

        public override int Read( byte[] buffer, int offset, int count )
        {
            if( _fillMode )
            {
                int fill = 20 * 512;
                int toFill = count;
                if( count > fill - _positionFiller )
                {
                    toFill = fill - _positionFiller;
                }
                _positionFiller += toFill;
                for( int i = 0; i < toFill; i++ )
                {
                    buffer[offset + i] = 0;
                }
                return toFill;
            }
            if( _actualStream == null ) {
                _fillMode = true;
                return Read(buffer, offset, count);
            }
            int readCount = _actualStream.Read( buffer, offset, count );
            _position += readCount;
            if( readCount == 0 )//Stream completly read
            {
                _actualStream = _callback();
                _startLastStream = _position;
                if( _actualStream != null ) return Read( buffer, offset, count );
            }
            return readCount;
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
