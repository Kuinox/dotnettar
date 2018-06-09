using System;
using System.IO;
using System.Threading.Tasks;

namespace dotnettar
{
	public class TarBallReader : IDisposable
    {
	    readonly Stream _stream;
	    public TarBallReader(Stream stream)
	    {
			if(!stream.CanRead) throw new ArgumentException("Can't read the stream");
		    _stream = stream;
	    }

		public async Task<TarFile> GetNextEntry()
	    {
		    return await TarFile.FromTarStream(_stream);
	    }

	    public void Dispose()
	    {
		    _stream?.Dispose();
	    }
    }
}
