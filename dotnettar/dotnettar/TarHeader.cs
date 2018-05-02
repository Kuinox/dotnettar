using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace dotnettar
{
	public class TarHeader
	{
		public const int BlockSize = 512;
		//Pre-Ustar tar header					
		string _name;
		UnixPermission _fileMode;
		byte _ownerId;
		byte _groupId;
		public long FileSize { get; private set; }
		DateTime _lastModification;
		public int CheckSum { get; private set; }
		char _typeFlag;
		string _nameOfLinkedFile;
		bool _uStar;
		byte _uStarVersion;
		string _ownerUserName;
		string _ownerGroupName;
		int _deviceMajorNumber;
		int _deviceMinorNumber;
		string _fileNamePrefix;

		TarHeader() { }
		public static async Task<TarHeader> FromStream(Stream stream)
		{
			//Pre-Ustar tar header						|offset	|size	|Description
			var name =              new byte[100];//	|0		|100	|File name
			var fileMode =          new byte[8];  //	|100	|8		|File mode
			var ownerId =           new byte[8];  //	|108	|8		|Owner's numeric user ID
			var groupId =           new byte[8];  //	|116	|8		|Group's numeric user ID
			var fileSize =          new byte[12]; //	|124	|12		|File size in bytes (octal base)
			var lastModification =  new byte[12]; //	|136	|12		|Last modification time in numeric Unix time format (octal)
			var checkSum =          new byte[8];  //	|148	|8		|Checksum for header record
			byte typeFlag;						  //	|156	|1		|Type flag
			var nameOfLinkedFile =  new byte[100];//	|157	|100	|Name of linked file
			//Ustar tar headers							|offset	|size	|Description
			var uStar =             new byte[6];  //	|257	|6		|UStar indicator "ustar" then NUL
			var uStarVersion =      new byte[2];  //	|263	|2		|UStar version "00"
			var ownerUserName =     new byte[32]; //	|265	|32		|Owner user name
			var ownerGroupName =    new byte[32]; //	|297	|32		|Owner group name
			var deviceMajorNumber = new byte[8];  //	|329	|8		|Device major number
			var deviceMinorNumber = new byte[8];  //	|337	|8		|Device minor number
			var fileNamePrefix =    new byte[155];  //	|345	|8		|Filename prefix
			var filler =            new byte[12];  //	|500	|12		|Filler up to 512

			if (await stream.ReadAsync(name             , 0, name.Length             ) == 0) throw new EndOfStreamException("Invalid header");
			if (await stream.ReadAsync(fileMode         , 0, fileMode.Length         ) == 0) throw new EndOfStreamException("Invalid header");
			if (await stream.ReadAsync(ownerId          , 0, ownerId.Length          ) == 0) throw new EndOfStreamException("Invalid header");
			if (await stream.ReadAsync(groupId          , 0, groupId.Length          ) == 0) throw new EndOfStreamException("Invalid header");
			if (await stream.ReadAsync(fileSize         , 0, fileSize.Length         ) == 0) throw new EndOfStreamException("Invalid header");
			if (await stream.ReadAsync(lastModification , 0, lastModification.Length ) == 0) throw new EndOfStreamException("Invalid header");
			if (await stream.ReadAsync(checkSum         , 0, checkSum.Length         ) == 0) throw new EndOfStreamException("Invalid header");
			int temp = stream.ReadByte();
			if(temp==-1) throw new EndOfStreamException("Invalid Header");
			typeFlag = (byte) temp;
			if (await stream.ReadAsync(nameOfLinkedFile , 0, nameOfLinkedFile.Length ) == 0) throw new EndOfStreamException("Invalid header");
			//ustar
			if (await stream.ReadAsync(uStar            , 0, uStar.Length            ) == 0) throw new EndOfStreamException("Invalid header");
			if (await stream.ReadAsync(uStarVersion     , 0, uStarVersion.Length     ) == 0) throw new EndOfStreamException("Invalid header");
			if (await stream.ReadAsync(ownerUserName    , 0, ownerUserName.Length    ) == 0) throw new EndOfStreamException("Invalid header");
			if (await stream.ReadAsync(ownerGroupName   , 0, ownerGroupName.Length   ) == 0) throw new EndOfStreamException("Invalid header");
			if (await stream.ReadAsync(deviceMajorNumber, 0, deviceMajorNumber.Length) == 0) throw new EndOfStreamException("Invalid header");
			if (await stream.ReadAsync(deviceMinorNumber, 0, deviceMinorNumber.Length) == 0) throw new EndOfStreamException("Invalid header");
			if (await stream.ReadAsync(fileNamePrefix   , 0, fileNamePrefix.Length   ) == 0) throw new EndOfStreamException("Invalid header");
			if (await stream.ReadAsync(filler           , 0, filler.Length           ) == 0) throw new EndOfStreamException("Invalid header");
			var output = new TarHeader //TODO: How i do that cleanly ?
			{
				_name = Encoding.ASCII.GetString(name).Replace("\0", string.Empty),
				_fileMode = new UnixPermission(Encoding.ASCII.GetString(fileMode)),
				_ownerId = OctalToDecimal(byte.Parse(Encoding.ASCII.GetString(ownerId))),
				_groupId = OctalToDecimal(byte.Parse(Encoding.ASCII.GetString(groupId))),
				FileSize = OctalToDecimal(long.Parse(Encoding.ASCII.GetString(fileSize))),
				_lastModification = UnixTimeStampToDateTime(long.Parse(Encoding.ASCII.GetString(lastModification))),
				CheckSum = OctalToDecimal(int.Parse(Encoding.ASCII.GetString(checkSum).Replace("\0", string.Empty))),
				_typeFlag = Encoding.ASCII.GetString(new[] {typeFlag})[0],
				_nameOfLinkedFile = Encoding.ASCII.GetString(nameOfLinkedFile),
				_uStar = Encoding.ASCII.GetString(uStar) == "uStar\0",
				_uStarVersion = byte.Parse(Encoding.ASCII.GetString(uStarVersion)),
				_fileNamePrefix = Encoding.ASCII.GetString(fileNamePrefix),
				_ownerUserName = Encoding.ASCII.GetString(ownerUserName),
				_ownerGroupName = Encoding.ASCII.GetString(ownerGroupName)
			};
			if (!int.TryParse(Encoding.ASCII.GetString(deviceMajorNumber), out output._deviceMajorNumber))
			{
				output._deviceMajorNumber = 0;
			}
			if (!int.TryParse(Encoding.ASCII.GetString(deviceMinorNumber), out output._deviceMinorNumber))
			{
				output._deviceMinorNumber = 0;
			}
			return output;
		}

		public async Task WriteToStream(Stream stream)
		{
			if(!stream.CanWrite) throw new IOException("Cannot write to given stream");
			var name = Encoding.ASCII.GetBytes(_name.PadRight(100, '\0'));
			var permissions = Encoding.ASCII.GetBytes(_fileMode.ToString()+'\0');
			var ownerId = Encoding.ASCII.GetBytes(Convert.ToString(_ownerId, 8).PadLeft(7, '0')+"\0");
			var groupId = Encoding.ASCII.GetBytes(Convert.ToString(_groupId, 8).PadLeft(7, '0')+"\0");
			var fileSize = Encoding.ASCII.GetBytes(Convert.ToString(FileSize, 8).PadLeft(11, '0')+"\0");
			var timeStamp = Encoding.ASCII.GetBytes(Convert.ToString((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds, 8).PadLeft(11, '0')+"\0");
			var checksum = Encoding.ASCII.GetBytes(Convert.ToString(CheckSum, 8).PadLeft(7, '0')+"\0");
			var nameLinked = Encoding.ASCII.GetBytes(_nameOfLinkedFile.PadRight(100, '\0'));
			var ustar = Encoding.ASCII.GetBytes("uStar\0");
			var ustarVersion = Encoding.ASCII.GetBytes(Convert.ToString(_uStarVersion, 8).PadLeft(2, '0'));
			var ownerName = Encoding.ASCII.GetBytes(_ownerUserName.PadRight(12, '\0'));
			var groupName = Encoding.ASCII.GetBytes(_ownerGroupName.PadRight(12, '\0'));
			var deviceMajor = Encoding.ASCII.GetBytes(Convert.ToString(_deviceMajorNumber, 8).PadLeft(7, '0') + "\0");
			var deviceMinor = Encoding.ASCII.GetBytes(Convert.ToString(_deviceMajorNumber, 8).PadLeft(7, '0') + "\0");
			var filePrefix = Encoding.ASCII.GetBytes(_fileNamePrefix.PadRight(155, '\0'));
			var filler = Encoding.ASCII.GetBytes("\0\0\0\0\0\0\0\0\0\0\0\0");
			await stream.WriteAsync(name, 0, name.Length);
			await stream.WriteAsync(permissions, 0, permissions.Length);
			await stream.WriteAsync(ownerId, 0, ownerId.Length);
			await stream.WriteAsync(groupId, 0, groupId.Length);
			await stream.WriteAsync(fileSize, 0, fileSize.Length);
			await stream.WriteAsync(timeStamp, 0, timeStamp.Length);
			await stream.WriteAsync(checksum, 0, checksum.Length);
			await stream.WriteAsync(new[] {(byte) _typeFlag}, 0, 1);
			await stream.WriteAsync(nameLinked, 0, nameLinked.Length);
			await stream.WriteAsync(ustar, 0, ustar.Length);
			await stream.WriteAsync(ustarVersion, 0, ustarVersion.Length);
			await stream.WriteAsync(ownerName, 0, ownerName.Length);
			await stream.WriteAsync(groupName, 0, groupName.Length);
			await stream.WriteAsync(deviceMajor, 0, deviceMajor.Length);
			await stream.WriteAsync(deviceMinor, 0, deviceMinor.Length);
			await stream.WriteAsync(filePrefix, 0, filePrefix.Length);
			await stream.WriteAsync(filler, 0, filler.Length);
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
