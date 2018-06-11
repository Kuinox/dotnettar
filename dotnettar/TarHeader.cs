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
        readonly bool _sevenZipMode;

        //Pre-Ustar tar header					
        public string Name { get; set; }
		public UnixPermission FileMode;
		public byte OwnerId;
		public byte GroupId;
		public long FileSize { get; set; }//TODO public set or not ?
		public DateTime LastModification;

		int CheckSum(bool sevenZip = false) => Encoding.ASCII.GetBytes(ToString(true)).Sum(b => b);
		public char TypeFlag;
		public string NameOfLinkedFile;
		public byte UStarVersion;
		public string OwnerUserName;
		public string OwnerGroupName;
		public int DeviceMajorNumber;
		public int DeviceMinorNumber;
		public string FileNamePrefix;

		public TarHeader(bool sevenZipMode = false) {
            _sevenZipMode = sevenZipMode;
        }
		internal static async Task<TarHeader> FromStream(Stream stream, bool throwBadCkecksum = true)
		{
			var headerBytes = new byte[512];
			int numberOfBytesRead = await stream.ReadAsync(headerBytes, 0, headerBytes.Length);
			
			while (headerBytes.All(b => b == 0) && numberOfBytesRead == 512)
			{
				numberOfBytesRead = await stream.ReadAsync(headerBytes, 0, headerBytes.Length);
			}
			if (numberOfBytesRead == 0) return null;
			if (numberOfBytesRead < BlockSize) throw new EndOfStreamException("Invalid header");
			var headerString = Encoding.ASCII.GetString(headerBytes);
			//Pre-Ustar tar header											|offset	|size	|Description
			var name =              headerString.Substring(0, 100);		//	|0		|100	|File name
			var fileMode =          headerString.Substring(100, 8);		//	|100	|8		|File mode
			var ownerId =           headerString.Substring(108, 8);		//	|108	|8		|Owner's numeric user ID
			var groupId =           headerString.Substring(116, 8);		//	|116	|8		|Group's numeric user ID
			var fileSize =          headerString.Substring(124, 12);	//	|124	|12		|File size in bytes (octal base)
			var lastModification =  headerString.Substring(136, 12);	//	|136	|12		|Last modification time in numeric Unix time format (octal)
			var checkSum =          headerString.Substring(148, 8);		//	|148	|8		|Checksum for header record
			var typeFlag =          headerString[156];					//	|156	|1		|Type flag
			var nameOfLinkedFile =  headerString.Substring(157, 100);	//	|157	|100	|Name of linked file
			//Ustar tar headers												|offset	|size	|Description
			var uStar =             headerString.Substring(257, 6);		//	|257	|6		|UStar indicator "ustar" then NUL
			var uStarVersion =      headerString.Substring(263, 2);		//	|263	|2		|UStar version "00"
			var ownerUserName =     headerString.Substring(265, 32);	//	|265	|32		|Owner user name
			var ownerGroupName =    headerString.Substring(297, 32);	//	|297	|32		|Owner group name
			var deviceMajorNumber = headerString.Substring(329, 8);		//	|329	|8		|Device major number
			var deviceMinorNumber = headerString.Substring(337, 8);		//	|337	|8		|Device minor number
			var fileNamePrefix =    headerString.Substring(345, 155);	//	|345	|8		|Filename prefix


			if (uStar != "ustar\0") throw new InvalidDataException("Invalid tar file, or non POSIX.1-1988 tar. Only POSIX.1-1988 tar or better are supported.");
			TarHeader output;
			try
			{
				output = new TarHeader
				{
					Name = name.Replace("\0", string.Empty),
					FileMode = new UnixPermission(fileMode),
					OwnerId = OctalToDecimal(byte.Parse(ownerId)),
					GroupId = OctalToDecimal(byte.Parse(groupId)),
					FileSize = OctalToDecimal(long.Parse(fileSize)),
					LastModification = UnixTimeStampToDateTime(long.Parse(lastModification)),
					TypeFlag = new[] {typeFlag}[0],
					NameOfLinkedFile = nameOfLinkedFile.Replace("\0", string.Empty),
					UStarVersion = byte.Parse(uStarVersion),
					FileNamePrefix = fileNamePrefix.Replace("\0", string.Empty),
					OwnerUserName = ownerUserName.Replace("\0", string.Empty),
					OwnerGroupName = ownerGroupName.Replace("\0", string.Empty)
				};
			}
			catch (FormatException)
			{
				throw new InvalidDataException("Invalid Data Header");
			}
			
			if (!int.TryParse(deviceMajorNumber, out output.DeviceMajorNumber))
			{
				output.DeviceMajorNumber = 0;
			}
			if (!int.TryParse(deviceMinorNumber, out output.DeviceMinorNumber))
			{
				output.DeviceMinorNumber = 0;
			}
			int checksum = OctalToDecimal(int.Parse(checkSum.Replace("\0", string.Empty)));

			if (output.CheckSum() == checksum) return output;
			if (output.CheckSum(true) == checksum) return output;
			if (throwBadCkecksum) throw new InvalidDataException("Invalid header's checksum.");
			return output;
		}

		public override string ToString()
		{
			return ToString(false);
		}

		public string ToString(bool checkSumWhiteSpace)
		{
			var name = Name.PadRight(100, '\0');
			string permissions;
			if(FileMode == null)
			{
				permissions = UnixPermission.DefaultPermission()+"\0";
			} else
			{
				permissions = FileMode.ToString() + '\0';
			}
			var ownerId = Convert.ToString(OwnerId, 8).PadLeft(7, '0') + "\0";
			var groupId = Convert.ToString(GroupId, 8).PadLeft(7, '0') + "\0";
			var fileSize = Convert.ToString(FileSize, 8).PadLeft(11, '0') + "\0";
            if (LastModification == DateTime.MinValue) LastModification = DateTime.Now;
			var timeStamp = Convert.ToString((long)LastModification.Subtract(new DateTime(1970, 1, 1)).TotalSeconds, 8).PadLeft(11, '0') + "\0";
			string checksum;
			if (checkSumWhiteSpace)
			{
				checksum = "        ";
			}
			else
			{
				checksum = Convert.ToString(CheckSum(_sevenZipMode), 8).PadLeft(7, '0').Substring(1, 6) + "\0 ";
			}
            if (NameOfLinkedFile == null) NameOfLinkedFile = "";
            if (OwnerUserName == null) OwnerUserName = "";
            if (OwnerGroupName == null) OwnerGroupName = "";
            if (FileNamePrefix == null) FileNamePrefix = "";
            var nameLinked = NameOfLinkedFile.PadRight(100, '\0');
			const string ustar = "ustar\0";
			var ustarVersion = Convert.ToString(UStarVersion, 8).PadLeft(2, '0');
            var ownerName = OwnerUserName.PadRight(32, '\0'); 
			var groupName = OwnerGroupName.PadRight(32, '\0');
			//var deviceMajor =  Convert.ToString(_deviceMajorNumber, 8).PadLeft(7, '0') + "\0";
			//var deviceMinor =  Convert.ToString(_deviceMinorNumber, 8).PadLeft(7, '0') + "\0";
			var deviceMajor = DeviceMajorNumber != 0 || !_sevenZipMode ? Convert.ToString(DeviceMajorNumber, 8).PadLeft(7, '0') + "\0" : "\0\0\0\0\0\0\0\0";
			var deviceMinor = DeviceMinorNumber != 0 || !_sevenZipMode ? Convert.ToString(DeviceMinorNumber, 8).PadLeft(7, '0') + "\0" : "\0\0\0\0\0\0\0\0";
			var filePrefix = FileNamePrefix.PadRight(155, '\0');
            var output = (name + permissions + ownerId + groupId + fileSize + timeStamp + checksum + TypeFlag + nameLinked + ustar + ustarVersion +
			       ownerName + groupName + deviceMajor + deviceMinor + filePrefix+ "\0\0\0\0\0\0\0\0\0\0\0\0");
			if(output.Length != 512) throw new InvalidOperationException("Internal error: Incorrect output string computed.");
			return output;
		}

		

		public async Task WriteToStream(Stream stream)
		{
			if(!stream.CanWrite) throw new IOException("Cannot write to given stream");
			var headerString = Encoding.ASCII.GetBytes(ToString(false));
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
