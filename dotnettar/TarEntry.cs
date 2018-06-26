using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dotnettar
{
    public class TarEntry : Stream
    {
        private readonly string _header;
        private readonly Stream _stream;
        long _position;
        bool _fillMode;
        public TarEntry( TarHeader header, Stream stream )
        {
            _header = header.ToString();
            _stream = stream;
        }


        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => _position; set => throw new NotSupportedException(); }



        public override int Read( byte[] buffer, int offset, int count )
        {
            if( _fillMode )
            {
                if( _position % 512 == 0 ) return 0;
                int toFill =  512 - ((int)_position % 512);
                int toRead = count;
                if(count>toFill)
                {
                    toRead = toFill;
                }
                for( int i = 0; i < toRead; i++ )
                {
                    buffer[offset + i] = 0;
                }
                return toRead;
            }
            if( Position < TarHeader.BlockSize )//Position is in header
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
            var readed = _stream.Read( buffer, offset, count );
            if(readed == 0 )
            {
                _fillMode = true;
                var readCount = Read( buffer, offset, count );
                _position += readCount;
                return readCount;
            }
            _position += readed;
            return readed;
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
