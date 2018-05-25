using System;
using System.IO;
using System.Threading.Tasks;

namespace dotnettar
{
	public class TarBall : IDisposable
    {
	    readonly Stream _stream;
	    public TarBall(Stream stream)
	    {
		    _stream = stream;
	    }

		public async Task<TarFile> GetNextTarFile()
	    {
		    return await TarFile.FromTarStream(_stream);
	    }

	    public void Dispose()
	    {
		    _stream?.Dispose();
	    }
    }
}
