namespace DSInternals.Common.Data
{
    /// <summary>
    /// Key Source
    /// </summary>
    /// <see>https://msdn.microsoft.com/en-us/library/mt220501.aspx</see>
    public enum KeySource : byte
    {
        /// <summary>
        /// On Premises Key Trust
        /// </summary>
        AD = 0x00,

        /// <summary>
        /// Hybrid Azure AD Key Trust
        /// </summary>
        AzureAD = 0x01
    }
}
