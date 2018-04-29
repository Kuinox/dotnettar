using System;
using System.IO;
using System.IO.Compression;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
			using (var file = File.Open("test.zip", FileMode.Open))
	        {
		        var test = new ZipArchive(file);
		        var toto = test.Entries[0];
		        using (var text = toto.Open())
		        {
			        using (var ms = new MemoryStream())
			        {
				        var buffer = new byte[32768];
						while (true)
				        {
					        var read = text.Read(buffer, 0, buffer.Length);
					        if (read <= 0)
						        break;
					        ms.Write(buffer, 0, read);
				        }

				        var output = new byte[ms.Length];
				        ms.Read(output, 0, (int)ms.Length);
				        Console.WriteLine(System.Text.Encoding.ASCII.GetString(output));
					}

					
					
			        
		        }
	        }

	        Console.ReadKey();
		}
    }
}
