using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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



        [Test]
        public void TarCopyToMemoryStreamCopyToFile()
        {
            using( var file = File.OpenRead( "ITest.zip" ) )
            using( var archive = new ZipArchive( file ) )
            using( var outputFile = File.OpenWrite( "outputZip.tar" ) )
            using(var memoryStream = new MemoryStream())
            {
                var zipConverted = new ZipConverter( archive );
                var output = new TarFile( zipConverted.NextEntry );
                output.CopyTo( memoryStream );
                memoryStream.CopyTo( outputFile );
            }
        }

        [Test]
        public void EmptyZipCopyToTar()
        {
            using(var newFile = File.OpenRead( "empty.zip" ) )
            using( var emptyZip = new ZipArchive( newFile ) )
            using( var outputFile = File.OpenWrite( "emptyTarOutput.tar" ) )
            {
                var zipConverted = new ZipConverter( emptyZip );
                var output = new TarFile( zipConverted.NextEntry );
                output.CopyTo( outputFile );
            }
        }

        [Test]
        public void TarCopyToFile()
        {
            using( var file = File.OpenRead( "ITest.zip" ) )
            using( var archive = new ZipArchive( file ) )
            using(var outputFile = File.OpenWrite("outputZip.tar"))
            {
                var zipConverted = new ZipConverter( archive );
                var output = new TarFile( zipConverted.NextEntry );
                output.CopyTo( outputFile);
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
