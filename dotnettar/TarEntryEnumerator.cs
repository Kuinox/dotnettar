using System.Collections.Generic;
using System.Linq;
namespace dotnettar
{
    public class TarEntryEnumerator
    {
        List<TarEntry> _entries;
        public TarEntryEnumerator( List<TarEntry> entries )
        {
            _entries = entries;
        }
        public TarEntry NextEntry()
        {
            var output = _entries.First();
            _entries.Remove( output );
            return output;
        }
    }
}
