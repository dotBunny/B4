// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Net.NetworkInformation;
using B4.Steps;
using B4.Utils;

namespace B4
{
    /// <summary>
    ///     A unified project bootstrapper.
    /// </summary>
    /// <remarks>No external assembly references should be made.</remarks>
    internal static class Program
    {
        /// <summary>
        ///     The processed arguments for the bootstrapper.
        /// </summary>
        public static Arguments Args;

        /// <summary>
        ///     Is an internet connection present and able to ping an outside host?
        /// </summary>
        public static bool IsOnline;

        /// <summary>
        ///     The full path to the root folder of operations.
        /// </summary>
        /// <remarks>This requires that the bootstrapper live at the root of the project to work by default.</remarks>
        public static string RootDirectory;

        // ReSharper disable once UnusedMember.Local
        private static void Main(string[] args)
        {

            Output.LogLine($"B4 the Bootstrapper | Version {typeof(Program).Assembly.GetName().Version} | Copyright (c) 2022 dotBunny Inc.", ConsoleColor.Green);
            Output.LogLine($"Started on {DateTime.Now:F}", ConsoleColor.DarkGray);

            Output.LogLine("Initializing ...");
            Args = new Arguments(args);

            // Current Directory
            RootDirectory = Directory.GetCurrentDirectory();

            // Root Directory Override
            if (Args.TryGetValue(Arguments.RootDirectoryKey, out string rootDirectoryOverride))
            {
                RootDirectory = Path.GetFullPath(rootDirectoryOverride);
            }

            Output.Value("RootDirectory", RootDirectory);

            // Check Internet Connection
            Ping ping = new();
            try
            {
                PingReply reply = ping.Send(Config.PingHost, 3000);
                if (reply != null)
                {
                    IsOnline = reply.Status == IPStatus.Success;
                }
            }
            catch (Exception e)
            {
                Output.LogLine(e.Message, ConsoleColor.Yellow);
                IsOnline = false;
            }
            finally
            {
                Output.Value("IsOnline", IsOnline.ToString());
            }

            // Initialize our step processors, this will self register content for other systems
            // (like the --help) argument.
            IStep[] steps = { new Steps.B4(), new K9(), new K9Config(), new RemotePackages(), new FindUnity(), new LaunchUnity() };

            // Check for help request
            if (Args.Has(Arguments.HelpKey))
            {
                Args.Help();
                return;
            }

            // Process Setsp
            foreach (IStep step in steps)
            {
                Output.SectionHeader(step.GetHeader());
                step.Process();
            }
        }

        public static void SetEnvironmentVariable(string name, string value)
        {
            if (Args.Has(Arguments.TeamCityKey))
            {
                // Set for TeamCity
                Output.LogLine($"##teamcity[setParameter name='{name}' value='{value}']", ConsoleColor.Yellow);
            }

            // Set for user (no-perm request)
            if (Args.Has(Arguments.UserEnvironmentKey))
            {
                Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
            }
        }
    }
}