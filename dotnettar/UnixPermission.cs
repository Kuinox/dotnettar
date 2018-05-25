using System;

namespace dotnettar
{
    public class UnixPermission
    {
	    readonly ushort _permissions;

	    public UnixPermission(string permissionString = "0100777")
	    {
			if (!ulong.TryParse(permissionString, out ulong input)) throw new ArgumentException("Invalid permission string");
			input = TarHeader.OctalToDecimal(input);
		    if (input > ushort.MaxValue) throw new ArgumentException("Invald permission string");
		    _permissions = (ushort) input;
		}

	    public override string ToString()
	    {
		    string output = Convert.ToString(_permissions, 8);
		    return output.PadLeft(7, '0');
	    }
    }
}
