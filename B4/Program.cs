﻿// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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
        ///     The path to the projects folder, relative to <see cref="RootDirectory" />.
        /// </summary>
        public static string ProjectDirectory;

        private static readonly List<IStep> s_orderedSteps = new();

        private static readonly Dictionary<string, IStep> s_registeredSteps = new();

        private static string s_pingHost;

        // ReSharper disable once UnusedMember.Local
        private static void Main(string[] args)
        {
            Output.LogLine(
                $"B4 the Bootstrapper | Version {typeof(Program).Assembly.GetName().Version} | Copyright (c) 2022 dotBunny Inc.",
                ConsoleColor.Green);
            Output.LogLine($"Started on {DateTime.Now:F}", ConsoleColor.DarkGray);

            Output.LogLine("Initializing ...");
            Args = new Arguments(args);

            // Root Directory Override
            if (Args.TryGetValue(Arguments.RootDirectoryKey, out string rootDirectoryOverride))
            {
                RootDirectory = Path.GetFullPath(rootDirectoryOverride);
            }

            Output.Value("RootDirectory", RootDirectory);

            // Load B4 Config
            string configPath = Path.Combine(RootDirectory, "B4.ini");
            Config = File.Exists(configPath)
                ? new SimpleConfig(configPath)
                : new SimpleConfig(Resources.Get("B4.Configs.B4.ini"));

            // ProjectDirectory
            if (Config.TryGetValue(Arguments.ProjectDirectoryKey, out string projectDirectoryDefault))
            {
                ProjectDirectory = Path.GetFullPath(Path.Combine(RootDirectory, projectDirectoryDefault));
            }

            if (Args.TryGetValue(Arguments.ProjectDirectoryKey, out string projectDirectoryOverride))
            {
                ProjectDirectory = Path.GetFullPath(projectDirectoryOverride);
            }

            Output.Value("ProjectDirectory", ProjectDirectory);

            // PingHost
            if (Config.TryGetValue(Arguments.PingHostKey, out string pingHostDefault))
            {
                s_pingHost = pingHostDefault;
            }

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

            // Search the assembly for included IStep's and create an instance of each, using the system activator.
            Type stepInterface = typeof(IStep);
            IEnumerable<Type> foundTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => stepInterface.IsAssignableFrom(p));
            foreach (Type t in foundTypes)
            {
                // Don't try to create the interface
                if (t == stepInterface)
                {
                    continue;
                }

                // Create instance and register instance
                IStep step = (IStep)Activator.CreateInstance(t);
                if (step == null) continue;
                s_registeredSteps.Add(step.GetID().ToLower(), step);
            }

            // Check for help request
            if (Args.Has(Arguments.HelpKey))
            {
                Args.Help();
                return;
            }

            // Build out ordered steps
            if (Config.TryGetValue("steps", out string steps))
            {
                string[] stepSplit = steps.Split(',', StringSplitOptions.TrimEntries);
                foreach (string targetStep in stepSplit)
                {
                    string targetStepLower = targetStep.ToLower();
                    if(s_registeredSteps.ContainsKey(targetStepLower))
                    {
                        s_orderedSteps.Add(s_registeredSteps[targetStepLower]);
                    }
                    else
                    {
                        Output.Log($"Unable to find '{targetStepLower}' step.", ConsoleColor.Yellow);
                    }
                }
            }
            if (s_orderedSteps.Count == 0)
            {
                Output.Error("No steps defined.", -1, true);
            }

            // Process steps
            foreach (IStep step in s_orderedSteps)
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