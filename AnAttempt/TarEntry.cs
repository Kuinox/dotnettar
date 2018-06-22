using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dotnettar
{
    class TarEntry : Stream
    {
        private readonly string _header;
        private readonly Stream _stream;
        long _position;
        public TarEntry( TarHeader header, Stream stream )
        {
            _header = header.ToString();
            _stream = stream;
        }


        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _stream.Length + TarHeader.BlockSize;

        public override long Position { get => _position; set => throw new NotSupportedException(); }



        public override int Read( byte[] buffer, int offset, int count )
        {
            if( Position < TarHeader.BlockSize )
            {
                long toRead;
                if( count > TarHeader.BlockSize - Position )
                {
                    toRead = TarHeader.BlockSize - Position;
                }
                else
                {
                    toRead = count;
                }
                var output = Encoding.ASCII.GetBytes( _header.Substring( (int)Position, (int)toRead ) );
                _position += toRead;
                output.CopyTo( buffer, offset );
                return output.Length;
            }
            else if( Position < _stream.Length + TarHeader.BlockSize )
            {
                var readed = _stream.Read( buffer, offset, count );
                _position += readed;
                return readed;
            }
            long readCount;
            if( count > Length - Position )
            {
                readCount = Length - Position;
            }
            else
            {
                readCount = count;
            }
            _position += readCount;
            for( int i = 0; i < readCount; i++ )
            {
                buffer[offset + i] = 0;
            }
            return (int)readCount;
        }
        public override void Flush()
        {
            throw new NotSupportedException();
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
