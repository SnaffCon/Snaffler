using System;
using System.Threading.Tasks;
using SnaffCore.Config;
#if NETFRAMEWORK
using Nett;
#else
using CsToml;
#endif

namespace Snaffler
{
    internal static class TomlCompat
    {
#if NETFRAMEWORK
        private static readonly TomlSettings Settings = TomlSettings.Create(cfg => cfg
            .ConfigureType<LogLevel>(tc => tc.WithConversionFor<TomlString>(conv => conv
                .FromToml(s => (LogLevel)Enum.Parse(typeof(LogLevel), s.Value, ignoreCase: true))
                .ToToml(e => e.ToString()))));

        public static Task WriteFileAsync(Options options, string path)
        {
            Toml.WriteFile(options, path, Settings);
            return Task.FromResult(0);
        }

        public static Task<Options> ReadFileAsync(string path)
        {
            return Task.FromResult(Toml.ReadFile<Options>(path, Settings));
        }

        public static RuleSet ReadString(string content)
        {
            return Toml.ReadString<RuleSet>(content, Settings);
        }
#else
        public static async Task WriteFileAsync(Options options, string path)
        {
            using var result = CsTomlSerializer.Serialize(options);
            await FileCompat.WriteAllBytesAsync(path, result.ByteSpan.ToArray());
        }

        public static async Task<Options> ReadFileAsync(string path)
        {
            return CsTomlSerializer.Deserialize<Options>(await FileCompat.ReadAllBytesAsync(path));
        }

        public static RuleSet ReadString(string content)
        {
            return CsTomlSerializer.Deserialize<RuleSet>(System.Text.Encoding.UTF8.GetBytes(content));
        }
#endif
    }
}
