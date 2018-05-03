using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace dotnettar.Tests
{
	[TestFixture]
	internal class BasicTest
	{
		[Test]
		public void HeaderDoesntThrowOnReadWrite()
		{
			Assert.DoesNotThrowAsync(async () =>
			{
				using (var stream = File.OpenRead("test.tar"))
				{
					using (var newFile = File.Create("testDone.tar"))
					{
						var header = await TarHeader.FromStream(stream);
						await header.WriteToStream(newFile);
						for (int i = 0 ; i <= stream.Length-512; i+=1)
						{
							var buffer = new byte[1];
							await stream.ReadAsync(buffer, 0, 1);
							await newFile.WriteAsync(buffer, 0, 1);
						}
					}
				}
			});
		}

		[Test]
		public async Task HeaderIsValid()
		{
			HeaderDoesntThrowOnReadWrite();//Generating the tar to read.
			using (var original = File.OpenRead("test.tar"))
			{
				using (var created = File.OpenRead("testDone.tar"))
				{
					var bufferOriginal = new byte[512];
					await original.ReadAsync(bufferOriginal, 0, bufferOriginal.Length);
					var bufferDone = new byte[512];
					await created.ReadAsync(bufferDone, 0, bufferDone.Length);
					Trace.WriteLine(Encoding.ASCII.GetString(bufferOriginal).Replace('\0', '$'));
					Trace.WriteLine(Encoding.ASCII.GetString(bufferDone).Replace('\0', '$'));
					Assert.AreEqual(bufferDone, bufferOriginal);
				}
			}
		} 

		[Test]
		public void Basic()
		{

			Assert.DoesNotThrowAsync(async () =>
			{
				using (var stream = File.OpenRead("test.tar"))
				{
					await Test.Open(stream);
				}
			});
		}

		[Test]
		public void Advanced()
		{
			Assert.DoesNotThrowAsync(async () =>
			{
				using (var stream = File.OpenRead("test.tar"))
				{
					await TarHeader.FromStream(stream);
				}
			});
		}
		[Test]
		public void Permissions()
		{
			new UnixPermission();
		}
	}
}
