using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace dotnettar
{
	/// <inheritdoc />
	public class TarFile : Stream
    {
	    
	    Stream _file;
	    public TarHeader Header { get; private set; }


	    TarFile() {}
	    public static async Task<TarFile> FromTarStream(Stream stream)
	    {
			var header = await TarHeader.FromStream(stream);
		    if (header == null) return null;
		    var output = new TarFile
		    {
			    Header = header,
			    _file = stream
		    };
		    return output;
	    }

	    /// <summary>
	    /// Write the content of a TarFile to a Stream
	    /// The tar will be valid if you write only TarFile objects
	    /// </summary>
	    /// <returns></returns>
	    public async void AddToTar(Stream streamToWrite)
	    {
		    await Header.WriteToStream(streamToWrite);
		    await _file.CopyToAsync(streamToWrite);
	    }

	    public override void Flush() => throw new NotSupportedException();

	    public override int Read(byte[] buffer, int offset, int count)
	    {
		    Position += count;
		   // if (Position > Length) throw new EndOfStreamException();
			return _file.Read(buffer, offset, count);
	    }

	    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

		public override void SetLength(long value) => throw new NotSupportedException();

	    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

	    public override bool CanRead => true;
	    public override bool CanSeek => false;
	    public override bool CanWrite => false;
	    public override long Length => Header.FileSize;
	    public override long Position { get; set; }

	    protected override void Dispose(bool disposing)
		{
			long realLength = (long)Math.Ceiling((double)Length / TarHeader.BlockSize)*TarHeader.BlockSize;
			long toSkip = realLength - Position;
		    if (_file.CanSeek)
		    {
			    _file.Seek(toSkip, SeekOrigin.Current);
		    }
		    else
		    {
			    _file.ReadAsync(new byte[]{}, 0, (int)toSkip);//TODO read multiple time if size is larger than long, if file size can be biger than 32 bits
		    }
		    base.Dispose(disposing);
	    }

	    public async Task WriteOnFileSystem(string path)
	    {
		    if (!Directory.Exists(path)) throw new ArgumentException("Source path doesn't exist");
		    Directory.CreateDirectory(Path.GetDirectoryName(Header.Name));
		    using (var fileStream = File.Create(Path.GetFileName(Header.Name)))
		    {
			    await _file.CopyToAsync(fileStream);
		    }
	    }
    }
}
