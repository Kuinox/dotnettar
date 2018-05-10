using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnettar
{
	public class TarHeader
	{
		public const int BlockSize = 512;
		//Pre-Ustar tar header					
		public string Name { get; private set; }
		UnixPermission _fileMode;
		byte _ownerId;
		byte _groupId;
		public long FileSize { get; private set; }
		DateTime _lastModification;

		public int CheckSum => Encoding.ASCII.GetBytes(ToString(true)).Sum(b => b);

		char _typeFlag;
		string _nameOfLinkedFile;
		byte _uStarVersion;
		string _ownerUserName;
		string _ownerGroupName;
		int _deviceMajorNumber;
		int _deviceMinorNumber;
		string _fileNamePrefix;

		TarHeader() { }
		public static async Task<TarHeader> FromStream(Stream stream, bool throwBadCkecksum = false)
		{
			var headerBytes = new byte[512];
			if (await stream.ReadAsync(headerBytes, 0, headerBytes.Length) == 0) throw new EndOfStreamException("Invalid header");
			var headerString = Encoding.ASCII.GetString(headerBytes);

			//Pre-Ustar tar header											|offset	|size	|Description
			var name =              headerString.Substring(0, 100);		//	|0		|100	|File name
			var fileMode =          headerString.Substring(100, 8);		//	|100	|8		|File mode
			var ownerId =           headerString.Substring(108, 8);		//	|108	|8		|Owner's numeric user ID
			var groupId =           headerString.Substring(116, 8);		//	|116	|8		|Group's numeric user ID
			var fileSize =          headerString.Substring(124, 12);	//	|124	|12		|File size in bytes (octal base)
			var lastModification =  headerString.Substring(136, 12);	//	|136	|12		|Last modification time in numeric Unix time format (octal)
			var checkSum =          headerString.Substring(148, 8);		//	|148	|8		|Checksum for header record
			var typeFlag =         headerString[156];					//	|156	|1		|Type flag
			var nameOfLinkedFile =  headerString.Substring(157, 100);	//	|157	|100	|Name of linked file
			//Ustar tar headers												|offset	|size	|Description
			var uStar =             headerString.Substring(257, 6);		//	|257	|6		|UStar indicator "ustar" then NUL
			var uStarVersion =      headerString.Substring(263, 2);		//	|263	|2		|UStar version "00"
			var ownerUserName =     headerString.Substring(265, 32);	//	|265	|32		|Owner user name
			var ownerGroupName =    headerString.Substring(297, 32);	//	|297	|32		|Owner group name
			var deviceMajorNumber = headerString.Substring(329, 8);		//	|329	|8		|Device major number
			var deviceMinorNumber = headerString.Substring(337, 8);		//	|337	|8		|Device minor number
			var fileNamePrefix =    headerString.Substring(345, 155);	//	|345	|8		|Filename prefix
			var filler =            headerString.Substring(500, 12);    //	|500	|12		|Filler up to 512


			if (uStar != "ustar\0") throw new InvalidDataException("Invalid tar file, or non POSIX.1-1988 tar. Only POSIX.1-1988 tar or better are supported.");
			var output = new TarHeader //TODO: implement try catch
			{
				Name = name.Replace("\0", string.Empty),
				_fileMode = new UnixPermission(fileMode),
				_ownerId = OctalToDecimal(byte.Parse(ownerId)),
				_groupId = OctalToDecimal(byte.Parse(groupId)),
				FileSize = OctalToDecimal(long.Parse(fileSize)),
				_lastModification = UnixTimeStampToDateTime(long.Parse(lastModification)),
				_typeFlag = new[] {typeFlag}[0],
				_nameOfLinkedFile = nameOfLinkedFile.Replace("\0", string.Empty),
				_uStarVersion = byte.Parse(uStarVersion),
				_fileNamePrefix = fileNamePrefix.Replace("\0", string.Empty),
				_ownerUserName = ownerUserName.Replace("\0", string.Empty),
				_ownerGroupName = ownerGroupName.Replace("\0", string.Empty)
			};
			if (!int.TryParse(deviceMajorNumber, out output._deviceMajorNumber))
			{
				output._deviceMajorNumber = 0;
			}
			if (!int.TryParse(deviceMinorNumber, out output._deviceMinorNumber))
			{
				output._deviceMinorNumber = 0;
			}
			int checksum = OctalToDecimal(int.Parse(checkSum.Replace("\0", string.Empty)));

			if (output.CheckSum == checksum) return output;

			if (throwBadCkecksum) throw new InvalidDataException("Invalid header's checksum.");
			Trace.WriteLine("Warning, bad checksum");
			return output;
		}

		public override string ToString()
		{
			return ToString(false);
		}

		public string ToString(bool checkSumWhiteSpace)
		{
			var name = Name.PadRight(100, '\0');
			var permissions = _fileMode.ToString() + '\0';
			var ownerId = Convert.ToString(_ownerId, 8).PadLeft(7, '0') + "\0";
			var groupId = Convert.ToString(_groupId, 8).PadLeft(7, '0') + "\0";
			var fileSize = Convert.ToString(FileSize, 8).PadLeft(11, '0') + "\0";
			var timeStamp = Convert.ToString((long)_lastModification.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).PadLeft(11, '0') + "\0";
			string checksum;
			if (checkSumWhiteSpace)
			{
				checksum = "        ";
			}
			else
			{
				checksum = Convert.ToString(CheckSum, 8).PadLeft(7, '0').Substring(1, 6) + "\0 ";
			}
			var nameLinked = _nameOfLinkedFile.PadRight(100, '\0');
			const string ustar = "ustar\0";
			var ustarVersion = Convert.ToString(_uStarVersion, 8).PadLeft(2, '0');
			var ownerName = _ownerUserName.PadRight(12, '\0');
			var groupName = _ownerGroupName.PadRight(12, '\0');
			var deviceMajor = _deviceMajorNumber != 0 ? Convert.ToString(_deviceMajorNumber, 8).PadLeft(7, '0') + "\0" : "\0\0\0\0\0\0\0\0";
			var deviceMinor = _deviceMinorNumber != 0 ? Convert.ToString(_deviceMinorNumber, 8).PadLeft(7, '0') + "\0" : "\0\0\0\0\0\0\0\0";
			var filePrefix = _fileNamePrefix.PadRight(155, '\0');
			const string filler = "\0\0\0\0\0\0\0\0\0\0\0\0";
			var output = name + permissions + ownerId + groupId + fileSize + timeStamp + checksum + _typeFlag + nameLinked + ustar + ustarVersion +
			       ownerName + groupName + deviceMajor + deviceMinor + filePrefix + filler;
			//if(output.Length != 512) throw new InvalidOperationException("Internal error: Incorrect output string computed.");
			return output;
		}

		

		public async Task WriteToStream(Stream stream)
		{
			if(!stream.CanWrite) throw new IOException("Cannot write to given stream");
			var headerString = Encoding.ASCII.GetBytes(ToString());
			await stream.WriteAsync(headerString, 0, headerString.Length);
		}

		static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
		{
			var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
			return dtDateTime;
		}

		internal static long OctalToDecimal(long n)
		{
			long num = n;
			long decValue = 0;
			long Base = 1;
			long temp = num;
			while (temp > 0)
			{
				long lastDigit = temp % 10;
				temp = temp / 10;
				decValue += lastDigit * Base;
				Base = Base * 8;
			}
			return decValue;
		}
		internal static ulong OctalToDecimal(ulong n)
		{
			ulong num = n;
			ulong decValue = 0;
			ulong Base = 1;
			ulong temp = num;
			while (temp > 0)
			{
				ulong lastDigit = temp % 10;
				temp = temp / 10;
				decValue += lastDigit * Base;
				Base = Base * 8;
			}
			return decValue;
		}
		static int OctalToDecimal(int n)
		{
			int num = n;
			int decValue = 0;
			int Base = 1;
			int temp = num;
			while (temp > 0)
			{

				int lastDigit = temp % 10;
				temp = temp / 10;
				decValue += lastDigit * Base;
				Base = Base * 8;
			}
			return decValue;
		}

		static byte OctalToDecimal(byte n)
		{
			byte num = n;
			byte decValue = 0;
			byte Base = 1;
			byte temp = num;
			while (temp > 0)
			{

				byte lastDigit = (byte)(temp % 10);
				temp = (byte)(temp / 10);
				decValue += (byte)(lastDigit * Base);
				Base = (byte)(Base * 8);
			}
			return decValue;
		}
	}
}
