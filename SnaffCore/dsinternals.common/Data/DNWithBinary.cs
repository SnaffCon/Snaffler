namespace DSInternals.Common.Data
{
    using System;
    using DSInternals.Common.Properties;

    /// <summary>
    /// The DNWithBinary class represents the DN-Binary LDAP attribute syntax, which contains a binary value and a distinguished name (DN).
    /// </summary>
    public sealed class DNWithBinary
    {
        // String representation of DN-Binary data: B:<char count>:<binary value>:<object DN>
        private const string StringFormat = "B:{0}:{1}:{2}";
        private const string StringFormatPrefix = "B:";
        private const char StringFormatSeparator = ':';

        public string DistinguishedName
        {
            get;
            private set;
        }

        public byte[] Binary
        {
            get;
            private set;
        }

        public DNWithBinary(string dn, byte[] binary)
        {
            Validator.AssertNotNullOrEmpty(dn, nameof(dn));
            Validator.AssertNotNull(binary, nameof(binary));

            this.DistinguishedName = dn;
            this.Binary = binary;
        }

        public static DNWithBinary Parse(string dnWithBinary)
        {
            Validator.AssertNotNullOrEmpty(dnWithBinary, nameof(dnWithBinary));

            bool hasCorrectPrefix = dnWithBinary.StartsWith(StringFormatPrefix);
            int valueLeadingColonIndex = dnWithBinary.IndexOf(StringFormatSeparator, StringFormatPrefix.Length);
            int valueTrailingColonIndex = dnWithBinary.IndexOf(StringFormatSeparator, valueLeadingColonIndex + 1);
            bool has4Parts = valueLeadingColonIndex >= 3 && (valueLeadingColonIndex + 1) < valueTrailingColonIndex;

            if (!hasCorrectPrefix || !has4Parts)
            {
                // We do not need to perform a more thorough validation.
                throw new ArgumentException(Resources.NotDNWithBinaryMessage, nameof(dnWithBinary));
            }

            string dn = dnWithBinary.Substring(valueTrailingColonIndex + 1);
            byte[] binary = dnWithBinary.HexToBinary(valueLeadingColonIndex + 1, valueTrailingColonIndex - valueLeadingColonIndex - 1);
            return new DNWithBinary(dn, binary);
        }

        public override string ToString()
        {
            return String.Format(StringFormat, this.Binary.Length * 2, this.Binary.ToHex(true), this.DistinguishedName);
        }
    }
}
