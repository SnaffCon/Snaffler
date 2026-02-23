namespace DSInternals.Common.Data
{
    /// <summary>
    /// Key Credential Link Blob Structure Version
    /// </summary>
    /// <see>https://msdn.microsoft.com/en-us/library/mt220501.aspx</see>
    public enum KeyCredentialVersion : uint
    {
        Version0 = 0,
        Version1 = 0x00000100,
        Version2 = 0x00000200,
    }
}