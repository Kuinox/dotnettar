using System;
using System.IO;
using System.Threading.Tasks;

namespace dotnettar
{
	public class TarBallWriter : IDisposable
    {
	    readonly Stream _stream;
	    public TarBallWriter(Stream stream)
	    {
			if(!stream.CanWrite) throw new ArgumentException("Can't write to stream");
		    _stream = stream;
	    }

		public async Task WriteEntry(TarFile tarEntry)
		{
			await tarEntry.AddToTar(_stream);
		}

	    public void Dispose()
	    {
		    _stream?.Dispose();
	    }
    }
}
