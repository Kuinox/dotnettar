using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dotnettar
{
    class Tar
    {
	    Stream _stream;
	    bool _writing;
	    Tar(){}

		/// <summary>
		/// Create a Tar object from a Stream of a Tar to read it's content.
		/// </summary>
		/// <param name="sourceStream"></param>
		/// <returns></returns>
	    public Tar FromTarStream(Stream sourceStream)
		{
			return new Tar
			{
				_stream = sourceStream
			};
		}

	    


    }
}
