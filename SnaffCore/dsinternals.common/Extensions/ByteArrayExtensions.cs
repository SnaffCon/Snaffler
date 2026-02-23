namespace DSInternals.Common
{
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Principal;
    using System.Text;
    using DSInternals.Common.Properties;

    public static class ByteArrayExtensions
    {
        private const string HexDigitsUpper = "0123456789ABCDEF";
        private const string HexDigitsLower = "0123456789abcdef";

        public static void ZeroFill(this byte[] array)
        {
            Array.Clear(array, 0, array.Length);
        }

        public static byte[] HexToBinary(this string hex, int startIndex, int length)
        {
            // Input validation
            Validator.AssertNotNull(hex, nameof(hex));

            if (length % 2 != 0)
            {
                // Each byte in a HEX string must be encoded using 2 characters.
                var exception = new ArgumentException(Resources.NotHexStringMessage, nameof(hex));
                exception.Data.Add("Value", hex);
                throw exception;
            }
            
            if(startIndex < 0 || startIndex >= hex.Length )
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (length < 0 || startIndex + length > hex.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            // Prepare the result
            byte[] bytes = new byte[length / 2];

            // Perform the conversion
            for (int nibbleIndex = 0, byteIndex = 0; nibbleIndex < length; byteIndex = ++nibbleIndex / 2)
            {
                char nibble = hex[startIndex + nibbleIndex];

                if ('0' <= nibble && nibble <= '9')
                {
                    bytes[byteIndex] = (byte)((bytes[byteIndex] << 4) | (nibble - '0'));
                }
                else if ('a' <= nibble && nibble <= 'f')
                {
                    bytes[byteIndex] = (byte)((bytes[byteIndex] << 4) | (nibble - 'a' + 0xA));
                }
                else if ('A' <= nibble && nibble <= 'F')
                {
                    bytes[byteIndex] = (byte)((bytes[byteIndex] << 4) | (nibble - 'A' + 0xA));
                }
                else
                {
                    // Invalid digit
                    var exception = new ArgumentException(Resources.NotHexStringMessage, nameof(hex));
                    exception.Data.Add("Value", hex);
                    throw exception;
                }
            }

            return bytes;
        }

        public static byte[] HexToBinary(this string hex)
        {
            // Trivial case
            if (String.IsNullOrEmpty(hex))
            {
                return null;
            }

            return hex.HexToBinary(0, hex.Length);
        }

        public static string ToHex(this byte[] bytes, bool caps = false)
        {
            if (bytes == null)
            {
                return null;
            }

            string hexDigits = caps ? HexDigitsUpper : HexDigitsLower;

            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach(byte currentByte in bytes)
            {
                hex.Append(hexDigits[(int)(currentByte >> 4)]);
                hex.Append(hexDigits[(int)(currentByte & 0xF)]);
            }

            return hex.ToString();
        }

        public static SecureString ReadSecureWString(this byte[] buffer, int startIndex)
        {
            Validator.AssertNotNull(buffer, nameof(buffer));
            // TODO: Assert startIndex > 0
            int maxLength = buffer.Length - startIndex;

            // Prepare an empty SecureString that will eventually be returned
            var result = new SecureString();

            for (int i = startIndex; i < buffer.Length; i += UnicodeEncoding.CharSize)
            {
                // Convert the next 2 bytes from the byte array into a unicode character
                char c = BitConverter.ToChar(buffer, i);

                if (c == Char.MinValue)
                {
                    // End of string has been reached
                    return result;
                }

                result.AppendChar(c);
            }

            // If we reached this point, the \0 char has not been found, so throw an exception.
            // TODO: Add a reasonable exception message
            throw new ArgumentException();
        }

        public static void SwapBytes(this byte[] bytes, int index1, int index2)
        {
            byte temp = bytes[index1];
            bytes[index1] = bytes[index2];
            bytes[index2] = temp;
        }

        /// <summary>
        /// Encodes an integer into a 4-byte array, in big endian.
        /// </summary>
        /// <param name="number">The integer to encode.</param>
        /// <returns>Array of bytes, in big endian order.</returns>
        public static byte[] GetBigEndianBytes(this uint number)
        {
            byte[] bytes = BitConverter.GetBytes(number);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        public static uint ToUInt32BigEndian(this byte[] bytes, int startIndex = 0)
        {
            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt32(bytes, startIndex);
        }

        public static ushort ToUInt16BigEndian(this byte[] bytes, int startIndex = 0)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt16(bytes, startIndex);
        }

        public static Guid ToGuidBigEndian(this byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                bytes.SwapBytes(0, 3);
                bytes.SwapBytes(1, 2);
                bytes.SwapBytes(4, 5);
                bytes.SwapBytes(6, 7);
            }

            return new Guid(bytes);
        }

        public static SecurityIdentifier ToSecurityIdentifier(this byte[] binarySid, bool bigEndianRid = false)
        {
            if(binarySid == null)
            {
                return null;
            }
            byte[] output = binarySid;
            if (bigEndianRid)
            {
                // Clone the binary SID so we do not perform byte spapping on the original value.
                byte[] binarySidCopy = (byte[])binarySid.Clone();
                int lastByteIndex = binarySidCopy.Length -1;
                // Convert RID from big endian to little endian (Reverse the order of the last 4 bytes)
                binarySidCopy.SwapBytes(lastByteIndex - 3, lastByteIndex);
                binarySidCopy.SwapBytes(lastByteIndex - 2, lastByteIndex - 1);
                output = binarySidCopy;
            }
            return new SecurityIdentifier(output, 0);
        }

        public static byte[] Cut(this byte[] blob, int offset)
        {
            Validator.AssertNotNull(blob, "blob");
            return blob.Cut(offset, blob.Length - offset);
        }

        public static byte[] Cut(this byte[] blob, int offset, int count)
        {
            Validator.AssertNotNull(blob, "blob");
            Validator.AssertMinLength(blob, offset + count, "blob");
            // TODO: Check that offset and count are positive using Validator
            byte[] result = new byte[count];
            Buffer.BlockCopy((Array)blob, offset, (Array)result, 0, count);
            return result;
        }

        public static byte[] ReadToEnd(this MemoryStream stream)
        {
            long remainingBytes = stream.Length - stream.Position;
            if(remainingBytes > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("stream");
            }
            byte[] buffer = new byte[remainingBytes];
            stream.Read(buffer, 0, (int)remainingBytes);
            return buffer;
        }
    }
}
