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

	    long _offset;
	    long _pointer;

	    TarFile() { }
	    public static async Task<TarFile> FromTarStream(Stream stream, long offset)
	    {
		    var output = new TarFile
		    {
			    Header = await TarHeader.FromStream(stream),
			    _file = stream,
				_pointer = 0,
				_offset = offset + TarHeader.BlockSize
			};
		    
		    return output;
	    }

	    /// <summary>
	    /// Write the content of a TarFile to a Stream
	    /// The tar will be valid if you write only TarFile objects
	    /// </summary>
	    /// <returns></returns>
	    public void AddTarFileToTar(Stream streamToWrite)
	    {
			
	    }


		bool IsPositionInStream()
	    {
		    return _file.Position >= _offset && _file.Position <= _offset + Length;
	    }

	    public int FileCheckSum => Header.CheckSum;

	    public override void Flush() => throw new NotSupportedException();

	    public override int Read(byte[] buffer, int offset, int count)
	    {
		    return _file.Read(buffer, offset, count);
	    }

	    public override long Seek(long offset, SeekOrigin origin)
	    {
		    return _file.Seek(offset, origin);
	    }

	    int GetBlockLength()
	    {
		    return (int)Math.Ceiling((double)Header.FileSize / TarHeader.BlockSize);
	    }

	    public override void SetLength(long value) => throw new NotSupportedException();

	    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

	    public override bool CanRead => true;
	    public override bool CanSeek => false;
	    public override bool CanWrite => false;
	    public override long Length => Header.FileSize;
	    public override long Position {
		    get => _file.Position - _offset;
		    set => _file.Position = value + _offset;
	    }
    }
}
