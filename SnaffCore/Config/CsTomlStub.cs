#if NETFRAMEWORK
namespace CsToml
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
    public sealed class TomlSerializedObjectAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
    public sealed class TomlValueOnSerializedAttribute : System.Attribute { }
}
#endif

