using System.IO;
using System.Threading.Tasks;

namespace Snaffler
{
    internal static class FileCompat
    {
        public static Task<string[]> ReadAllLinesAsync(string path)
        {
#if NETFRAMEWORK
            return Task.FromResult(File.ReadAllLines(path));
#else
            return File.ReadAllLinesAsync(path);
#endif
        }

        public static Task<string> ReadAllTextAsync(string path)
        {
#if NETFRAMEWORK
            return Task.FromResult(File.ReadAllText(path));
#else
            return File.ReadAllTextAsync(path);
#endif
        }

        public static Task<byte[]> ReadAllBytesAsync(string path)
        {
#if NETFRAMEWORK
            return Task.FromResult(File.ReadAllBytes(path));
#else
            return File.ReadAllBytesAsync(path);
#endif
        }

        public static Task WriteAllBytesAsync(string path, byte[] bytes)
        {
#if NETFRAMEWORK
            File.WriteAllBytes(path, bytes);
            return Task.FromResult(0);
#else
            return File.WriteAllBytesAsync(path, bytes);
#endif
        }
    }
}
