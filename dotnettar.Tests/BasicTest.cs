using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using NUnit.Framework;

namespace dotnettar.Tests
{
    [TestFixture]
    class BasicTest
    {
        /*[Test]
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
			}*/


        static int file;
        [Test]
        public async Task WriteTar()
        {
            using( var file1 = File.OpenRead( "redis-4.0.0_content.txt" ) )
            using( var file2 = File.OpenRead( "redis-4 - Copy.0.0_content.txt" ) )
            using( var file3 = File.OpenRead( "redis-4 - Copy (3).0.0_content.txt" ) )
            using( var file4 = File.OpenRead( "redis-4 - Copy (2).0.0_content.txt" ) )
            {

            }
        }

        [Test]
        public async Task ReadTarAndMatchHashAndName()
        {
            using( var hashFile = File.OpenText( "redis-4.0.0_content.txt" ) )
            {
                var hashList = new Dictionary<string, string>();
                while( !hashFile.EndOfStream )
                {
                    var line = await hashFile.ReadLineAsync();
                    if( line.Contains( " " ) ) hashList.Add( line.Substring( 34 ), line.Substring( 0, 32 ) );
                    else hashList.Add( line, "" );
                }
                using( var tarTest = new TarBallReader( File.OpenRead( "redis-4.0.0.tar" ) ) )
                {
                    while( true )
                    {
                        using( var nextFile = await tarTest.GetEntryAsync() )
                        {
                            if( nextFile == null ) break;
                            Assert.That( hashList.ContainsKey( nextFile.Header.Name ) );
                            if( nextFile.Header.FileSize > 0 )
                            {
                                var hasher = MD5.Create();
                                hasher.Initialize();
                                var hash = BitConverter.ToString( hasher.ComputeHash( nextFile ) ).Replace( "-", "" ).ToLower();
                                Assert.That( hashList[nextFile.Header.Name] == hash );
                            }
                            Assert.That( hashList.Remove( nextFile.Header.Name ) );
                        }
                    }
                    Assert.That( hashList.Count == 0 );
                }
            }
        }
    }
}
