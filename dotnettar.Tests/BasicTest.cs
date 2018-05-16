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
						for (var i = 0 ; i <= stream.Length-512; i+=1)
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
		public async Task TarFileRead()
		{
			var paths = new List<string>();
			using (var tarTest = new TarBall(File.OpenRead("redis-4.0.0.tar")))
			{
				while (true)
				{
					using (var debug = await tarTest.GetNextTarFile())
					{
						if (debug == null) break;
						paths.Add(debug.Header.Name);
						var hasher = MD5.Create();
						hasher.Initialize();
						var data = new byte[debug.Length];
						await debug.ReadAsync(data, 0, data.Length);
						var hash = hasher.ComputeHash(data);
						
						using (var fileTest = File.OpenRead("redis-4.0.0/pax_global_header"))
						{
							var hasherCorrect = MD5.Create();
							hasherCorrect.Initialize();
							var dataCorrect = new byte[fileTest.Length];
							await fileTest.ReadAsync(dataCorrect, 0, dataCorrect.Length);
							var hashCorrect = hasherCorrect.ComputeHash(dataCorrect);
							Trace.WriteLine("Hash of byte array of");
							Trace.WriteLine("converted file: "+BitConverter.ToString(hash));
							Trace.WriteLine("original file:  "+BitConverter.ToString(hashCorrect));
							Trace.WriteLine("converted in ASCII: "+Encoding.ASCII.GetString(data));
							Trace.WriteLine("original in ASCII:  " +Encoding.ASCII.GetString(dataCorrect));
							Trace.WriteLine("Sequence equal:"+ data.SequenceEqual(dataCorrect));
						}
					}
				}
			}

			
			var correctPaths = (await File.ReadAllLinesAsync("redis-4.0.0_content.txt")).ToList();
			Assert.That(paths.All(s => correctPaths.Remove(s)) && correctPaths.Count == 0);
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
