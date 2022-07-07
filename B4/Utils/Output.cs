// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace B4.Utils
{
    public static class Output
    {
        public const ConsoleColor ExternalForegroundColor = ConsoleColor.DarkGray;
        public const ConsoleColor ExternalBackgroundColor = ConsoleColor.Black;

        private static ConsoleColor s_stashedForegroundColor;
        private static ConsoleColor s_stashedBackgroundColor;

        private static StringBuilder s_line = new StringBuilder();
        private static List<string> s_buffer = new List<string>(20);
        private static string s_logFile;

        public static void InitLog()
        {
            s_logFile = Path.Combine(Program.RootDirectory, "B4.log");

            // Remove old log file
            if (File.Exists(s_logFile))
            {
                File.Copy(s_logFile, $"{s_logFile}.prev", true);
                File.Delete(s_logFile);
            }
            File.WriteAllText(s_logFile, $"Launched in {Program.RootDirectory} ...");
        }
        public static void FlushLog()
        {
            File.AppendAllLines(s_logFile, s_buffer.ToArray());
            s_buffer.Clear();
        }

        static void AppendLineToBuffer(string line)
        {
            if (s_line.Length > 0)
            {
                s_buffer.Add(s_line.ToString().Trim('\n'));
                s_line.Clear();
                if (string.IsNullOrEmpty(line))
                {
                    return;
                }
            }

            s_buffer.Add(line);
        }

        public static void Error(string message, int errorCode, bool isFatal = false)
        {
            StashState();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;

            string line = $"ERROR {errorCode}: {message}";
            Console.WriteLine(line);
            AppendLineToBuffer(line);

            RestoreState();
            if (isFatal)
            {
                FlushLog();
                Environment.Exit(errorCode);
            }
        }

        public static void Value(string key, string value)
        {
            StashState();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(key);
            s_line.Append(key);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("=");
            s_line.Append("=");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(value);
            s_line.Append(value);
            Console.WriteLine();
            AppendLineToBuffer(string.Empty);
            RestoreState();
        }

        public static void Log(string message, ConsoleColor foregroundColor = ConsoleColor.Gray,
            ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            StashState();
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.Write(message);
            s_line.Append(message);
            RestoreState();
        }

        public static void LogLine(string message,
            ConsoleColor foregroundColor = ConsoleColor.Gray,
            ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            StashState();
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.WriteLine(message);
            AppendLineToBuffer(message);
            RestoreState();
        }

        public static void NextLine()
        {
            Console.WriteLine();
            AppendLineToBuffer(string.Empty);
        }

        public static void SectionHeader(string message)
        {
            StashState();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            AppendLineToBuffer(string.Empty);
            Console.Write("[");
            s_line.Append("[");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(message);
            s_line.Append(message);
            Console.ForegroundColor = ConsoleColor.White;
            s_line.Append("]\n");
            Console.Write("]\n");
            RestoreState();
        }


        public static void Warning(string message)
        {
            StashState();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine($"WARNING: {message}");
            AppendLineToBuffer($"WARNING: {message}");
            RestoreState();
        }

        private static void RestoreState()
        {
            Console.BackgroundColor = s_stashedBackgroundColor;
            Console.ForegroundColor = s_stashedForegroundColor;
        }

        private static void StashState()
        {
            s_stashedBackgroundColor = Console.BackgroundColor;
            s_stashedForegroundColor = Console.ForegroundColor;
        }
    }
}