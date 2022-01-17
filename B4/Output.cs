// Copyright (c) 2020-2021 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System;

namespace B4
{
    public static class Output
    {
        public const ConsoleColor ExternalForegroundColor = ConsoleColor.DarkGray;
        public const ConsoleColor ExternalBackgroundColor = ConsoleColor.Black;

        private static ConsoleColor s_stashedForegroundColor;
        private static ConsoleColor s_stashedBackgroundColor;


        public static void Error(string message, int errorCode, bool isFatal = false)
        {
            StashState();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine($"ERROR {errorCode}: {message}");
            RestoreState();
            if (isFatal)
            {
                Environment.Exit(errorCode);
            }
        }

        public static void Value(string key, string value)
        {
            StashState();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(key);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("=");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(value);
            Console.WriteLine();
            RestoreState();
        }

        public static void Log(string message, ConsoleColor foregroundColor = ConsoleColor.Gray,
            ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            if (string.IsNullOrEmpty(message)) return;
            StashState();
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.Write(message);
            RestoreState();
        }

        public static void LogLine(string message,
            ConsoleColor foregroundColor = ConsoleColor.Gray,
            ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            if (string.IsNullOrEmpty(message)) return;

            StashState();
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.WriteLine(message);
            RestoreState();
        }

        public static void NextLine()
        {
            Console.WriteLine();
        }

        public static void SectionHeader(string message)
        {
            StashState();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine();
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(message);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("]\n");
            RestoreState();
        }

        public static void Warning(string message)
        {
            StashState();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine($"WARNING: {message}");
            RestoreState();
        }

        static void RestoreState()
        {
            Console.BackgroundColor = s_stashedBackgroundColor;
            Console.ForegroundColor = s_stashedForegroundColor;
        }

        static void StashState()
        {
            s_stashedBackgroundColor = Console.BackgroundColor;
            s_stashedForegroundColor = Console.ForegroundColor;
        }

    }
}