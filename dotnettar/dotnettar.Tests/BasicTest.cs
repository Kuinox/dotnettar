using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using NUnit.Framework;

namespace dotnettar.Tests
{
	[TestFixture]
	internal class BasicTest
	{
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
	}
}
