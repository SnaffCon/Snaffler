namespace DSInternals.Common.Data
{
    /// <summary>
    /// Key Usage
    /// </summary>
    /// <see>https://msdn.microsoft.com/en-us/library/mt220501.aspx</see>
    public enum KeyUsage : byte
    {
        // Admin key (pin-reset key)
        AdminKey = 0,

        /// <summary>
        /// NGC key attached to a user object (KEY_USAGE_NGC)
        /// </summary>
        NGC = 0x01,

        /// <summary>
        /// Transport key attached to a device object
        /// </summary>
        STK = 0x02,

        /// <summary>
        /// BitLocker recovery key
        /// </summary>
        BitlockerRecovery = 0x03,

        /// <summary>
        /// Unrecognized key usage
        /// </summary>
        Other = 0x04,

        /// <summary>
        /// Fast IDentity Online Key (KEY_USAGE_FIDO)
        /// </summary>
        FIDO = 0x07,

        /// <summary>
        /// File Encryption Key (KEY_USAGE_FEK)
        /// </summary>
        FEK = 0x08,

        /// <summary>
        /// DPAPI Key
        /// </summary>
        DPAPI // TODO: The DPAPI enum needs to be mapped to a proper integer value.
    }
}
