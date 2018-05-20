using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace dotnettar.Tests
{
	[TestFixture]
	class BasicTest
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
						await header.WriteToStream(newFile, true);
						for (var i = 0; i <= stream.Length - 512; i += 1)
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
		public async Task TarFileHashMatch()
		{
			using (var hashFile = File.OpenText("redis-4.0.0_content.txt"))
			{
				var hashList = new Dictionary<string, string>();
				while (!hashFile.EndOfStream)
				{
					var line = await hashFile.ReadLineAsync();
					if (line.Contains(" ")) hashList.Add(line.Substring(34), line.Substring(0, 32));
					else hashList.Add(line, "");
				}


				using (var tarTest = new TarBall(File.OpenRead("redis-4.0.0.tar")))
				{
					while (true)
					{
						using (var nextFile = await tarTest.GetNextTarFile())
						{
							if (hashList.Count == 566)
							{

							}
							if (nextFile == null) break;
							Assert.That(hashList.ContainsKey(nextFile.Header.Name));
							if (nextFile.Header.FileSize > 0)
							{
								var hasher = MD5.Create();
								hasher.Initialize();
								var hash = BitConverter.ToString(hasher.ComputeHash(nextFile)).Replace("-","").ToLower();
								Trace.WriteLine(hash);
								Assert.That(hashList[nextFile.Header.Name] == hash);
							}
							Assert.That(hashList.Remove(nextFile.Header.Name));
						}
					}
					Assert.That(hashList.Count == 0);
				}
			}
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
}
}
