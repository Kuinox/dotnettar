using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace dotnettar
{
    public class TarBallReader : IDisposable
    {
        Stream _stream;
        public TarBallReader(Stream tarStream)
        {
            _stream = tarStream;
        }

        public async Task<TarFileReader> GetEntryAsync()
        {
            return await TarFileReader.FromStreamAsync( _stream );
        }

        public void Dispose() => _stream.Dispose();
    }
}
