namespace Sddl.Parser
{
    /// <summary>
    /// A list of securable object types in Windows is defined here https://msdn.microsoft.com/en-us/library/aa379557(VS.85).aspx.
    /// The enum partition object types into groups differing in allowed access rights.
    /// </summary>
    public enum SecurableObjectType
    {
        Unknown,

        // NTFS file system
        File,
        Directory,

        Pipe,

        Process,
        Thread,

        FileMappingObject,

        AccessToken,

        WindowsManagementObject,

        RegistryKey,

        WindowsService,

        LocalOrRemotePrinter,

        NetworkShare,

        // Interprocess synchronization object
        Event,
        Mutex,
        Semaphore,
        Timer,

        JobObject,

        DirectoryServiceObject
    }
}