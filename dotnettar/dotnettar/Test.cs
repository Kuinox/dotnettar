using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace dotnettar
{
    public class Test
    {
	    public static async Task Open(Stream compressedStream)
	    {
			var buffer = new byte[32768];
			using (var localBuffer = new MemoryStream())
			{
				int read;
				await compressedStream.ReadAsync(buffer, 0, buffer.Length);
				do
				{
					await localBuffer.WriteAsync(buffer, 0, buffer.Length);
					read = await compressedStream.ReadAsync(buffer, 0, buffer.Length);
				} while (read != 0);

				var text = new byte[localBuffer.Length];
				localBuffer.Position = 0;
				await localBuffer.ReadAsync(text, 0, text.Length);
				Console.WriteLine(Encoding.ASCII.GetString(text, 0, text.Length));
			}
	    }
    }
}
