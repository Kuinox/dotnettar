using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace dotnettar
{
	/// <inheritdoc />
	class TarFile : Stream
    {
	    
	    Stream _file;
	    public TarHeader Header { get; private set; }

	    TarFile() { }
	    static async Task<TarFile> FromStream(Stream stream)
	    {
		    var output = new TarFile
		    {
			    Header = await TarHeader.FromStream(stream),
			    _file = stream
		    };
		    
		    return output;
	    }

	    public int FileCheckSum => Header.CheckSum;

	    public override void Flush() => throw new NotSupportedException();

	    public override int Read(byte[] buffer, int offset, int count)
	    {
		    return _file.Read(buffer, offset, count);

	    }

	    public override long Seek(long offset, SeekOrigin origin) => _file.Seek(offset, origin);

	    public override void SetLength(long value) => throw new NotSupportedException();

	    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

	    public override bool CanRead => true;
	    public override bool CanSeek => true;
	    public override bool CanWrite => false;
	    public override long Length => Header.FileSize;
	    public override long Position {
		    get => _file.Position + TarHeader.HeaderSize;
		    set => _file.Position = value + TarHeader.HeaderSize;
	    }
    }
}
