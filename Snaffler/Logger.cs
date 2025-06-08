using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using SnaffCore.Config;

#nullable enable
namespace Snaffler
{
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Fatal = 5
    }

    public static class Logger
    {
        private static readonly object _lock = new object();
        private static StreamWriter? _fileWriter;
        private static bool _consoleEnabled;
        private static bool _fileEnabled;
        private static LogLevel _minLevel;
        private static LogType _logType;
        private static char _separator;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static void Configure(bool logToConsole, bool logToFile, string? filePath, LogLevel minLevel, LogType logType, char separator)
        {
            _consoleEnabled = logToConsole;
            _fileEnabled = logToFile && !string.IsNullOrWhiteSpace(filePath);
            _minLevel = minLevel;
            _logType = logType;
            _separator = separator;
            if (_fileEnabled)
            {
                _fileWriter = new StreamWriter(filePath!, append: false, System.Text.Encoding.UTF8)
                {
                    AutoFlush = true
                };
            }
        }

        public static void Close()
        {
            lock (_lock)
            {
                if (_fileWriter != null)
                {
                    _fileWriter.Flush();
                    _fileWriter.Dispose();
                }
                _fileWriter = null;
            }
        }

        private static void WriteHighlighted(string text)
        {
            if (text.IndexOfAny(new[] { '{', '<', '(' }) == -1)
            {
                Console.Write(text);
                return;
            }
            var oldFg = Console.ForegroundColor;
            var oldBg = Console.BackgroundColor;
            bool curlyDone = false;
            bool angleDone = false;
            bool parenDone = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (!curlyDone && c == '{')
                {
                    int end = text.IndexOf('}', i);
                    if (end > i)
                    {
                        string token = text.Substring(i, end - i + 1);
                        Console.ForegroundColor = token switch
                        {
                            "{Green}" => ConsoleColor.DarkGreen,
                            "{Yellow}" => ConsoleColor.DarkYellow,
                            "{Red}" => ConsoleColor.DarkRed,
                            "{Black}" => ConsoleColor.Black,
                            _ => oldFg
                        };
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.Write(token);
                        Console.ForegroundColor = oldFg;
                        Console.BackgroundColor = oldBg;
                        i = end;
                        curlyDone = true;
                        continue;
                    }
                }
                else if (!angleDone && c == '<')
                {
                    int end = text.IndexOf('>', i);
                    if (end > i)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write(text.Substring(i, end - i + 1));
                        Console.ForegroundColor = oldFg;
                        Console.BackgroundColor = oldBg;
                        i = end;
                        angleDone = true;
                        continue;
                    }
                }
                else if (!parenDone && c == '(')
                {
                    int end = text.IndexOf(')', i);
                    if (end > i)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write(text.Substring(i, end - i + 1));
                        Console.ForegroundColor = oldFg;
                        Console.BackgroundColor = oldBg;
                        i = end;
                        parenDone = true;
                        continue;
                    }
                }

                Console.ForegroundColor = oldFg;
                Console.BackgroundColor = oldBg;
                Console.Write(c);
            }
            Console.ForegroundColor = oldFg;
            Console.BackgroundColor = oldBg;
        }

        private static void WriteConsoleInternal(string prefix, string tag, ConsoleColor tagColor, string message)
        {
            lock (_lock)
            {
                var oldFg = Console.ForegroundColor;
                var oldBg = Console.BackgroundColor;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(prefix);
                Console.ForegroundColor = tagColor;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(tag);
                Console.ForegroundColor = oldFg;
                Console.BackgroundColor = oldBg;
                Console.Write(_separator);
                WriteHighlighted(message);
                Console.WriteLine();
            }
        }

        private static void WriteConsole(string prefix, string tag, ConsoleColor tagColor, string message)
        {
            WriteConsoleInternal(prefix, tag, tagColor, message);
        }

        private static void WriteFile(string text)
        {
            lock (_lock)
            {
                if (_fileWriter == null) return;
                _fileWriter.WriteLine(text);
                _fileWriter.Flush();
            }
        }

        private static string BuildJson(LogLevel level, DateTime time, string message, object? data)
        {
            var obj = new
            {
                time = time.ToUniversalTime().ToString("O"),
                level = level.ToString(),
                message,
                eventProperties = data
            };
            return JsonSerializer.Serialize(obj, _jsonOptions);
        }

        private static void Log(LogLevel level, DateTime time, string prefix, string tag, ConsoleColor tagColor, string message, object? data = null)
        {
            if (level < _minLevel) return;

            string plain = string.Concat(prefix, tag, _separator, message);
            string output = _logType == LogType.JSON ? BuildJson(level, time, plain, data) : plain;

            if (_consoleEnabled)
            {
                WriteConsole(prefix, tag, tagColor, message);
            }
            if (_fileEnabled)
            {
                WriteFile(output);
            }
        }

        public static void Trace(DateTime time, string prefix, string message, object? data = null) => Log(LogLevel.Trace, time, prefix, "[Trace]", ConsoleColor.DarkGray, message, data);
        public static void Debug(DateTime time, string prefix, string message, object? data = null) => Log(LogLevel.Debug, time, prefix, "[Debug]", ConsoleColor.Gray, message, data);
        public static void Info(DateTime time, string prefix, string message, object? data = null) => Log(LogLevel.Info, time, prefix, "[Info]", ConsoleColor.White, message, data);
        public static void Warn(DateTime time, string prefix, string message, object? data = null) => Log(LogLevel.Warn, time, prefix, "[Warn]", ConsoleColor.Yellow, message, data);
        public static void Error(DateTime time, string prefix, string message, object? data = null) => Log(LogLevel.Error, time, prefix, "[Error]", ConsoleColor.Magenta, message, data);
        public static void Fatal(DateTime time, string prefix, string message, object? data = null) => Log(LogLevel.Fatal, time, prefix, "[Fatal]", ConsoleColor.Red, message, data);

        public static void File(DateTime time, string prefix, string message, object? data = null) => Log(LogLevel.Warn, time, prefix, "[File]", ConsoleColor.Green, message, data);
        public static void Share(DateTime time, string prefix, string message, object? data = null) => Log(LogLevel.Warn, time, prefix, "[Share]", ConsoleColor.Yellow, message, data);
        public static void Dir(DateTime time, string prefix, string message, object? data = null) => Log(LogLevel.Warn, time, prefix, "[Dir]", ConsoleColor.Green, message, data);
    }
}
