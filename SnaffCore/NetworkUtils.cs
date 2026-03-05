using System.Net;
using System.Text.RegularExpressions;

namespace SnaffCore
{
    public static class NetworkUtils
    {
        // Matches network/prefix notation e.g. 10.1.2.0/24 or 2001:db8::/32
        public static readonly Regex CidrRegex = new Regex(@"^\S+/\d+$", RegexOptions.Compiled);
        /// <summary>
        /// Checks whether an IP address falls within a CIDR range.
        /// Supports both IPv4 and IPv6.
        /// </summary>
        public static bool IsInCidr(IPAddress address, string cidr)
        {
            // Split "network/prefixLength" e.g. "10.1.2.0/24" -> ["10.1.2.0", "24"]
            string[] parts = cidr.Split('/');
            if (parts.Length != 2 || !int.TryParse(parts[1], out int prefixLength))
                return false;
            if (!IPAddress.TryParse(parts[0], out IPAddress network))
                return false;

            // GetAddressBytes() returns big-endian bytes: 4 for IPv4, 16 for IPv6
            // Mismatched lengths means comparing IPv4 against IPv6 or vice versa
            byte[] addrBytes = address.GetAddressBytes();
            byte[] netBytes = network.GetAddressBytes();

            if (addrBytes.Length != netBytes.Length)
                return false;

            // Check all bytes that are fully covered by the prefix
            // e.g. /24 covers 3 full bytes, /20 covers 2 full bytes
            int fullBytes = prefixLength / 8;
            int remainingBits = prefixLength % 8;

            for (int i = 0; i < fullBytes; i++)
            {
                if (addrBytes[i] != netBytes[i])
                    return false;
            }

            // Check the partial byte (if the prefix doesn't fall on a byte boundary)
            // e.g. /20 has 4 remaining bits: mask = 11110000 = 0xF0
            // We only compare the masked bits, ignoring the host portion
            if (remainingBits > 0)
            {
                byte mask = (byte)(0xFF << (8 - remainingBits));
                if ((addrBytes[fullBytes] & mask) != (netBytes[fullBytes] & mask))
                    return false;
            }

            return true;
        }
    }
}
