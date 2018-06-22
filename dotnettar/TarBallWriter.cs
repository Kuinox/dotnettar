using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dotnettar
{
    class TarBallWriter : IDisposable
    {
        private readonly Stream _stream;

        public TarBallWriter(Stream stream)
        {
            _stream = stream;
        }

        public void Dispose() => _stream.Dispose();
        
    }
}
