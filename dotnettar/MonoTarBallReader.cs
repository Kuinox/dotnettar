using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace dotnettar
{
    public class MonoTarBallStreamReader : Stream
    {
	    readonly FileStream _fileStream;
	    readonly string _header;
	    long _position;
	    public MonoTarBallStreamReader(string filePath)
	    {
		    _fileStream = File.OpenRead(filePath);
		    var header = new TarHeader {Name = Path.GetFileName(filePath), FileSize = _fileStream.Length};
		    _header = header.ToString();
	    }

	    public override void Flush()
	    {
		    throw new NotSupportedException();
	    }

	    public override int Read(byte[] buffer, int offset, int count)
	    {
		    if (Position < TarHeader.BlockSize)
		    {
			    long toRead;
			    if (count > TarHeader.BlockSize - Position)
			    {
				    toRead = TarHeader.BlockSize - Position;
			    }
			    else
			    {
				    toRead = count;
			    }
			    var output = Encoding.ASCII.GetBytes(_header.Substring((int) Position, (int)toRead));
                _position += toRead;
			    output.CopyTo(buffer, offset);
			    return output.Length;
		    }
		    return _fileStream.Read(buffer, offset, count);
	    }

	    public override long Seek(long offset, SeekOrigin origin)
	    {
		    throw new NotSupportedException();
	    }

	    public override void SetLength(long value)
	    {
		    throw new NotSupportedException();
	    }

	    public override void Write(byte[] buffer, int offset, int count)
	    {
		    throw new NotSupportedException();
	    }

        public override bool CanRead
        {
            get
            {
                return _fileStream.CanRead;
            }
        }

        public override bool CanSeek => false;
	    public override bool CanWrite => false;
	    public override long Length => _fileStream.Length + 512;

	    public override long Position
	    {
		    get => _position;
		    set
		    {
			    if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
			    if (value < TarHeader.BlockSize)
			    {
				    _position = 0;
			    }
			    else
			    {
				    _position = value - TarHeader.BlockSize;
			    }
		    }
	    }
    }
}
