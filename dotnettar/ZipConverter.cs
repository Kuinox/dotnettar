using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace dotnettar
{
    public class ZipConverter
    {
        readonly ZipArchive _archive;
        List<ZipArchiveEntry> _entries;

        public ZipConverter(ZipArchive archive)
        {
            _entries = new List<ZipArchiveEntry>( archive.Entries );
        }

        public TarEntry NextEntry()
        {
            if( _entries.Count == 0 ) return null;
            var entry = _entries.First();
            var output = new TarEntry(
                new TarHeader( entry.FullName, entry.Length ),
                entry.Open());
            _entries.Remove( entry );
            return output;
        }
    }
}
