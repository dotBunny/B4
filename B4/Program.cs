// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
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
        ///     The B4.ini loaded config instance.
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

        /// <summary>
        ///     An ordered list of the steps to be processed as defined.
        /// </summary>
        private static readonly List<IStep> s_orderedSteps = new();

        /// <summary>
        ///     An instantiated dictionary (by GetID) of possible steps.
        /// </summary>
        private static readonly Dictionary<string, IStep> s_registeredSteps = new();

        /// <summary>
        ///     A cached reference to the programs assembly.
        /// </summary>
        private static Assembly s_assembly;

        /// <summary>
        ///     The defined hostname to use when pinging to determine an outside connection.
        /// </summary>
        private static string s_pingHost;

        // ReSharper disable once UnusedMember.Local
        private static void Main(string[] args)
        {
            // Manual resolve the running directory
            string dllPath = Assembly.GetAssembly(typeof(Program))?.Location;
            if (dllPath != null)
            {
                RootDirectory = Directory.GetParent(dllPath)?.FullName;
            }

            try
            {
                Output.InitLog();

                // Cache reference to local assembly
                s_assembly = typeof(Program).Assembly;

                Output.LogLine(
                    $"B4 the Bootstrapper | Version {s_assembly.GetName().Version} | Copyright (c) 2022 dotBunny Inc.",
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

                string configPath = Path.Combine(RootDirectory, "B4.ini");

                // Output B4 Config
                if (Args.Has(Arguments.DefaultConfigKey))
                {
                    File.WriteAllBytes(configPath, Resources.Get("B4.Configs.B4.ini"));
                }

                // Load B4 Config
                Config = File.Exists(configPath)
                    ? new SimpleConfig(configPath)
                    : new SimpleConfig(Resources.Get("B4.Configs.B4.ini"));


                // ProjectDirectory
                GetParameter(Arguments.ProjectDirectoryKey, "Projects/NightOwl", out ProjectDirectory,
                    s => Path.GetFullPath(Path.Combine(RootDirectory, s)),
                    Directory.Exists);
                Output.Value("ProjectDirectory", ProjectDirectory);

                // PingHost
                GetParameter(Arguments.PingHostKey, "github.com", out s_pingHost);
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

                // Search the assemblies for included IStep's and create an instance of each, using the system activator.
                Type stepInterface = typeof(IStep);
                Type[] types = s_assembly.GetTypes();
                foreach (Type t in types)
                {
                    if (t == stepInterface || !stepInterface.IsAssignableFrom(t))
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
                        if (s_registeredSteps.ContainsKey(targetStepLower))
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
            finally
            {
                Program.SetEnvironmentVariable("B4.DATE", DateTime.Now.ToString("yyyyMMdd"));
                Program.SetEnvironmentVariable("B4.TIME", DateTime.Now.ToString("HHmm"));

                Output.FlushLog();
            }
        }

        /// <summary>
        ///     Get the value for a given parameter from the config, overriden by the arguments, but also have a
        ///     failsafe default value.
        /// </summary>
        /// <param name="key">The argument identifier or config identifier.</param>
        /// <param name="defaultValue">A built-in default value.</param>
        /// <param name="resolvedValue">The resolved parameter value written to the provided <see cref="string"/>.</param>
        /// <param name="processFunction">A function to manipulate the value retrieved.</param>
        /// <param name="validateFunction">A function to validate the working value.</param>
        /// <returns>true/false if value was found.</returns>
        public static bool GetParameter(string key, string defaultValue, out string resolvedValue,
            Func<string, string> processFunction = null, Func<string, bool> validateFunction = null)
        {
            bool success = false;
            resolvedValue = processFunction != null ? processFunction.Invoke(defaultValue) : defaultValue;

            if (Config.TryGetValue(key, out string foundConfig))
            {
                if (processFunction != null)
                {
                    foundConfig = processFunction.Invoke(foundConfig);
                }

                if (validateFunction != null)
                {
                    if (validateFunction.Invoke(foundConfig))
                    {
                        resolvedValue = foundConfig;
                        success = true;
                    }
                }
                else
                {
                    resolvedValue = foundConfig;
                    success = true;
                }
            }
            if (Args.TryGetValue(key, out string foundOverride))
            {
                if (processFunction != null)
                {
                    foundOverride = processFunction.Invoke(foundOverride);
                }

                if (validateFunction != null)
                {
                    if (validateFunction.Invoke(foundOverride))
                    {
                        resolvedValue = foundOverride;
                        success = true;
                    }
                }
                else
                {
                    resolvedValue = foundOverride;
                    success = true;
                }
            }

            return success;
        }

        /// <summary>
        ///     Set a variable for future reference in the running environment.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value of the variable.</param>
        public static void SetEnvironmentVariable(string name, string value)
        {
            if (Args.Has(Arguments.SetTeamCityKey))
            {
                // Set for TeamCity
                Output.LogLine($"##teamcity[setParameter name='{name}' value='{value}']", ConsoleColor.Yellow);
            }

            // Set for user (no-perm request)
            if (Args.Has(Arguments.SetUserEnvironmentKey))
            {
                Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
            }
        }
    }
}