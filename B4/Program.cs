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
        ///     The processed arguments for the bootstrap.
        /// </summary>
        public static Arguments Args;

        /// <summary>
        ///     Is an internet connection present and able to ping an outside host?
        /// </summary>
        public static bool IsOnline;

        // ReSharper disable once UnusedMember.Local
        private static void Main(string[] args)
        {
            Output.LogLine($"PROJECT {Config.ProjectName} BOOTSTRAP | Copyright (c) 2022 dotBunny Inc.",
                ConsoleColor.Blue);
            Output.LogLine($"Bootstrapped on {DateTime.Now:F}");

            Output.LogLine("Initializing ...");
            Args = new Arguments(args);

            // Current Directory
            Config.RootDirectory = Directory.GetCurrentDirectory();

            // Root Directory Override
            if (Args.TryGetValue(Arguments.RootDirectoryOption, out string rootDirectoryOverride))
            {
                Config.RootDirectory = Path.GetFullPath(rootDirectoryOverride);
            }

            Output.Value("RootDirectory", Config.RootDirectory);

            if (Args.TryGetValue(Arguments.K9PrebuiltOption, out string k9PrebuiltOverride))
            {
                Config.K9PrebuiltDirectory = k9PrebuiltOverride;
            }

            if (Args.TryGetValue(Arguments.K9RepositoryOption, out string k9RepositoryOverride))
            {
                Config.K9RepositoryDirectory = k9RepositoryOverride;
            }

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
                if (IsOnline)
                {
                    Output.LogLine("Online Mode", ConsoleColor.Green);
                }
                else
                {
                    Output.LogLine("Offline Mode", ConsoleColor.Yellow);
                }
            }

            if (Args.Has("-help"))
            {
                Args.Help();
                return;
            }

            // RUN THROUGH STEPS
            IStep[] steps = { new K9(), new K9Config(), new RemotePackages(), new FindUnity(), new LaunchUnity() };

            foreach (IStep step in steps)
            {
                Output.SectionHeader(step.GetHeader());
                step.Process();
            }
        }

        public static void SetEnvironmentVariable(string name, string value)
        {
            if (Args.Has(Arguments.TeamCityOption))
            {
                // Set for TeamCity
                Output.LogLine($"##teamcity[setParameter name='{name}' value='{value}']", ConsoleColor.Yellow);
            }

            // Set for user (no-perm request)
            if (Args.Has(Arguments.UserEnvironmentOption))
            {
                Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
            }
        }
    }
}