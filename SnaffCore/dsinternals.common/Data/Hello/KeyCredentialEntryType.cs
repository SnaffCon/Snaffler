namespace DSInternals.Common.Data
{
    /// <summary>
    /// Key Credential Link Entry Identifier
    /// </summary>
    /// <remarks>Describes the data stored in the Value field.</remarks>
    /// <see>https://msdn.microsoft.com/en-us/library/mt220499.aspx</see>
    public enum KeyCredentialEntryType : byte
    {
        /// <summary>
        /// A SHA256 hash of the Value field of the KeyMaterial entry.
        /// </summary>
        KeyID = 0x01,

        /// <summary>
        /// A SHA256 hash of all entries following this entry.
        /// </summary>
        KeyHash = 0x02,

        /// <summary>
        /// Key material of the credential.
        /// </summary>
        KeyMaterial = 0x03,

        /// <summary>
        /// Key Usage
        /// </summary>
        KeyUsage = 0x04,

        /// <summary>
        /// Key Source
        /// </summary>
        KeySource = 0x05,

        /// <summary>
        /// Device Identifier
        /// </summary>
        DeviceId = 0x06,
        
        /// <summary>
        /// Custom key information.
        /// </summary>
        CustomKeyInformation = 0x07,

        /// <summary>
        /// The approximate time this key was last used, in FILETIME format.
        /// </summary>
        KeyApproximateLastLogonTimeStamp = 0x08,

        /// <summary>
        /// The approximate time this key was created, in FILETIME format.
        /// </summary>
        KeyCreationTime = 0x09
    }
}
