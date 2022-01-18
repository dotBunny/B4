// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
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
        ///     The B4.ini loaded instance.
        /// </summary>
        public static SimpleConfig Config;

        /// <summary>
        ///     Is an internet connection present and able to ping an outside host?
        /// </summary>
        public static bool IsOnline;

        /// <summary>
        ///     The full path to the root folder of operations.
        /// </summary>
        /// <remarks>This requires that the bootstrapper live at the root to work by default.</remarks>
        public static string RootDirectory = Directory.GetCurrentDirectory();

        /// <summary>
        ///     The path to the projects folder, relative to <see cref="RootDirectory"/>.
        /// </summary>
        public static string ProjectDirectory = Path.Combine(RootDirectory, "Projects", "NightOwl");

        private static string s_pingHost = "github.com";

        private static Dictionary<string, IStep> s_steps = new();

        // ReSharper disable once UnusedMember.Local
        private static void Main(string[] args)
        {
            Output.LogLine($"B4 the Bootstrapper | Version {typeof(Program).Assembly.GetName().Version} | Copyright (c) 2022 dotBunny Inc.", ConsoleColor.Green);
            Output.LogLine($"Started on {DateTime.Now:F}", ConsoleColor.DarkGray);

            Output.LogLine("Initializing ...");
            Args = new Arguments(args);

            // TODO: Handle B4.ini Config

            // Root Directory Override
            if (Args.TryGetValue(Arguments.RootDirectoryKey, out string rootDirectoryOverride))
            {
                RootDirectory = Path.GetFullPath(rootDirectoryOverride);
            }
            Output.Value("RootDirectory", RootDirectory);

            // Project Directory Override
            if (Args.TryGetValue(Arguments.ProjectDirectoryKey, out string projectDirectoryOverride))
            {
                ProjectDirectory = Path.GetFullPath(projectDirectoryOverride);
            }
            Output.Value("ProjectDirectory", ProjectDirectory);

            // Ping Host Override
            if (Args.TryGetValue(Arguments.PingHostKey, out string pingHostOverride))
            {
                s_pingHost = pingHostOverride;
            }
            Output.Value("PingHost", s_pingHost);

            // Check Internet Connection
            Ping ping = new();
            try
            {
                PingReply reply = ping.Send(s_pingHost, 3000);
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

            // Initialize our step processors, this will self register content for other systems in the the right order
            // of operation.
            RegisterStep(new Bootstrapper());
            RegisterStep(new K9());
            RegisterStep(new K9Config());
            RegisterStep(new RemotePackages());
            RegisterStep(new FindUnity());
            RegisterStep(new LaunchUnity());

            // Check for help request
            if (Args.Has(Arguments.HelpKey))
            {
                Args.Help();
                return;
            }

            // TODO: Process steps based on steps in config/cli arg

            // Process Steps
            foreach (KeyValuePair<string, IStep> kvp in s_steps)
            {
                Output.SectionHeader(kvp.Value.GetHeader());
                kvp.Value.Process();
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

        private static void RegisterStep(IStep step)
        {
            s_steps.Add(step.GetID(), step);
        }
    }
}