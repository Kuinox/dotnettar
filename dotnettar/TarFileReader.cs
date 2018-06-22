using System;
using System.IO;
using System.Threading.Tasks;

namespace dotnettar
{
    public class TarFileReader: Stream
    {
        readonly Stream _stream;
        public TarHeader Header;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => Header.FileSize;

        public override long Position {
            get => _position;
            set => throw new NotSupportedException();
        }
        long _position;
        public TarFileReader(Stream stream)
        {
            _stream = stream;
        }
        /// <summary>
        /// Read a header and expose a stream of the file entry
        /// </summary>
        /// <param name="tarStream"></param>
        internal static async Task<TarFileReader> FromStreamAsync(Stream tarStream)
        {
            var header = await TarHeader.FromStream( tarStream );
            if( header == null ) return null;
            return new TarFileReader(tarStream)
            {
                Header = header
            };
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read( byte[] buffer, int offset, int count )
        {
            int toRead = count;
            if( count + Position > Length )
            {
                toRead = (int)(Length - Position);
            }
            if( toRead == 0 ) return 0;
            var returnedByteRead = _stream.Read( buffer, offset, toRead );
            _position += returnedByteRead;
            return returnedByteRead;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        public override long Seek( long offset, SeekOrigin origin ) => throw new NotSupportedException();
        /// <summary>
        /// Not supported. This is a read only stream.
        /// </summary>;
        public override void SetLength( long value ) => throw new NotSupportedException();
        /// <summary>
        /// Not supported. This is a read only stream.
        /// </summary>;
        public override void Write( byte[] buffer, int offset, int count ) => throw new NotSupportedException();

        protected override void Dispose( bool disposing )
        {
            long realLength = (long)Math.Ceiling( (double)Length / TarHeader.BlockSize ) * TarHeader.BlockSize;
            long toSkip = realLength - Position;
            if( _stream.CanSeek )
            {
                _stream.Seek( toSkip, SeekOrigin.Current );
            }
            else
            {
                _stream.ReadAsync( new byte[] { }, 0, (int)toSkip );
            }
            base.Dispose( disposing );
        }
    }
}
