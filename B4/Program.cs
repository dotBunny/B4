// Copyright (c) 2020-2021 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
// ReSharper disable CognitiveComplexity

namespace B4
{
    /// <summary>
    /// A unified project bootstrapper.
    /// </summary>
    /// <remarks>No external assembly references should be made.</remarks>
    internal static class Program
    {
        /// <summary>
        /// The processed arguments for the bootstrap.
        /// </summary>
        private static Arguments s_arguments;

        /// <summary>
        /// A loaded version of the K9 config.
        /// </summary>
        private static readonly Dictionary<string, string> s_config = new();

        /// <summary>
        /// A cached full path to the root of the built K9.
        /// </summary>
        private static string s_k9;

        private static bool s_onlineMode;

        /// <summary>
        /// The known path to the Unity editor.
        /// </summary>
        private static string s_unityEditor;

        // ReSharper disable once UnusedMember.Local
        private static void Main(string[] args)
        {
            Output.LogLine($"PROJECT {Config.ProjectName} BOOTSTRAP | Copyright (c) 2022 dotBunny Inc.", ConsoleColor.Blue);
            Output.LogLine($"Bootstrapped on {DateTime.Now:F}");

            Step1_Initialize(args);

            if (s_arguments.Has("-help"))
            {
                s_arguments.Help();
                return;
            }

            // Process
            Step2_K9();
            Step3_LocalConfig();
            Step4_RemotePackages();
            Step5_FindUnity();

            // Launch
            if (!s_arguments.Has(Arguments.NoLaunchOption) &&
                !string.IsNullOrEmpty(s_unityEditor) &&
                File.Exists(s_unityEditor))
            {
                string projectDirectory = Path.Combine(Config.RootDirectory, Config.ProjectRelativePath);
                Output.LogLine("Launching Unity ...");

                //TODO: Addd &?
                if (!ChildProcess.SpawnHidden(s_unityEditor, projectDirectory, $"-projectPath {projectDirectory}"))
                {
                    Output.Error("Failed to launch Unity.", -1);
                }
            }
        }

        /// <summary>
        /// Initialize base functionality within the bootstrapper.
        /// </summary>
        /// <param name="args">The commandline arguments to be processed.</param>
        static void Step1_Initialize(string[] args)
        {
            Output.LogLine("Initializing ...");
            s_arguments = new Arguments(args);

            // Current Directory
            Config.RootDirectory = Directory.GetCurrentDirectory();

            // Root Directory Override
            if (s_arguments.TryGetValue(Arguments.RootDirectoryOption, out string rootDirectoryOverride))
            {
                Config.RootDirectory = Path.GetFullPath(rootDirectoryOverride);
            }
            Output.Value("RootDirectory", Config.RootDirectory);

            if (s_arguments.TryGetValue(Arguments.K9PrebuiltOption, out string k9PrebuiltOverride))
            {
                Config.K9PrebuiltDirectory = k9PrebuiltOverride;
            }
            if (s_arguments.TryGetValue(Arguments.K9RepositoryOption, out string k9RepositoryOverride))
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
                    s_onlineMode = (reply.Status == IPStatus.Success);
                }
            }
            catch (Exception e)
            {
                Output.LogLine(e.Message, ConsoleColor.Yellow);
                s_onlineMode = false;
            }
            finally
            {
                if (s_onlineMode)
                {
                    Output.LogLine("Online Mode", ConsoleColor.Green);
                }
                else
                {
                    Output.LogLine("Offline Mode", ConsoleColor.Yellow);
                }
            }
        }

        /// <summary>
        /// Ensure that K9 is present and updated.
        /// </summary>
        static void Step2_K9()
        {
            Output.SectionHeader("K9");

            string prebuiltDirectory = Path.Combine(Config.RootDirectory, Config.K9PrebuiltDirectory);
            string repositoryDirectory = Path.Combine(Config.RootDirectory, Config.K9RepositoryDirectory);

            if (Directory.Exists(prebuiltDirectory))
            {
                s_k9 = prebuiltDirectory;
                Output.LogLine($"Found built K9 @ {prebuiltDirectory}", ConsoleColor.Green);
            }
            else if(s_onlineMode)
            {
                if (Directory.Exists(repositoryDirectory))
                {
                    // Grab latest (required really to proceed)
                    if (!ChildProcess.WaitFor("git.exe", repositoryDirectory, "fetch origin"))
                    {
                        Output.Error("Unable to fetch updates for K9.", Environment.ExitCode, true);
                    }

                    // Check if repository is behind
                    bool isBehind = false;
                    bool gitStatus = ChildProcess.WaitFor("git.exe", repositoryDirectory, "status -sb", null, line =>
                    {
                        if (line.Contains("behind"))
                        {
                            isBehind = true;
                        }
                    });

                    if (!gitStatus)
                    {
                        Output.Error("Unable to understand the status of the K9 repository.", Environment.ExitCode, true);
                    }

                    if (isBehind)
                    {
                        Output.LogLine("Getting latest K9 source ...");
                        if (!ChildProcess.WaitFor("git.exe", repositoryDirectory, "reset --hard"))
                        {
                            Output.Warning("Unable to reset K9 repository.");
                        }

                        if (!ChildProcess.WaitFor("git.exe", repositoryDirectory, "pull"))
                        {
                            Output.Warning("Unable to pull updates for K9 repository.");
                        }

                        Output.LogLine("Building K9 (Release) ...");
                        if (!ChildProcess.WaitFor("dotnet.exe", repositoryDirectory,
                                "build K9.sln --configuration Release"))
                        {
                            Output.Error("Unable to build K9", -1, true);
                        }
                    }
                    else
                    {
                        Output.LogLine("K9 is up-to-date.");
                    }
                }
                else
                {
                    Output.LogLine("Getting latest K9 source ...");
                    if (!ChildProcess.WaitFor("git.exe", Config.RootDirectory,
                            $"clone https://github.com/dotBunny/K9 {repositoryDirectory}"))
                    {
                        Output.Error("Unable to clone K9.", -1, true);
                    }

                    Output.LogLine("Building K9 (Release) ...");
                    if (!ChildProcess.WaitFor("dotnet.exe", repositoryDirectory,
                            "build K9.sln --configuration Release"))
                    {
                        Output.Error("Unable to build K9", -1, true);
                    }
                }

                s_k9 = Path.Combine(repositoryDirectory, "Build", "Release");
            }
            else
            {
                s_k9 = Path.Combine(repositoryDirectory, "Build", "Release");
                Output.Warning("Skipping K9 updates, unable to reach endpoint.");
            }

            Output.Value("K9", s_k9);
            SetEnvironmentVariable("K9", s_k9);
        }

        /// <summary>
        /// Apply default config and load any known settings
        /// </summary>
        static void Step3_LocalConfig()
        {
            Output.SectionHeader("LOCAL CONFIG");
            string configPath = Path.Combine(Config.RootDirectory, Config.K9ConfigName);

            // Write default file out
            if (!File.Exists(configPath))
            {
                Output.LogLine("Creating DEFAULT K9 config ...");
                File.WriteAllText(configPath, Config.K9ConfigDefaultContent);
            }

            // Parse config
            Output.LogLine($"Loading K9 config @ {configPath} ...");
            string[] configLines = File.ReadAllLines(configPath);
            foreach (string s in configLines)
            {
                if (string.IsNullOrEmpty(s)) continue;
                string[] split = s.Split('=', 2, StringSplitOptions.TrimEntries);
                if (split.Length == 2)
                {
                    string key = split[0];
                    string value = split[1];

                    if (s_config.ContainsKey(key))
                    {
                        s_config[key] = value;
                    }
                    else
                    {
                        s_config.Add(key, value);
                    }
                }
                else
                {
                    Output.LogLine($"Invalid K9 config line found: {s}", ConsoleColor.Red);
                }
            }

            // Setup environment
            foreach (KeyValuePair<string, string> item in s_config)
            {
                SetEnvironmentVariable(item.Key, item.Value);
            }

            string steamworksDirectory = Path.Combine(Config.RootDirectory, "ThirdParty", "Steamworks");
            SetEnvironmentVariable("SteamworksDirectory", steamworksDirectory);
            // ReSharper disable once StringLiteralTypo
            SetEnvironmentVariable("SteamCommand", Path.Combine(steamworksDirectory, "sdk", "tools", "ContentBuilder", "builder", "steamcmd.exe"));
        }

        static void Step4_RemotePackages()
        {
            Output.SectionHeader("REMOTE PACKAGES");
            string projectDirectory = Path.Combine(Config.RootDirectory, Config.ProjectRelativePath);

            Output.LogLine("Check Manifest ...");
            ChildProcess.WaitFor("dotnet", Config.RootDirectory, $"{Path.Combine(s_k9, "K9.Setup.dll")} Checkout --manifest {Path.Combine(projectDirectory, "RemotePackages", "manifest.json")}");

            Output.LogLine("Update Packages ...");
            ChildProcess.WaitFor("dotnet", Config.RootDirectory, $"{Path.Combine(s_k9, "K9.Unity.dll")} RemotePackages --remote {Path.Combine(projectDirectory, "RemotePackages", "manifest.json")} --unity {Path.Combine(projectDirectory, "Packages", "manifest.json")}");
        }

        static void Step5_FindUnity()
        {
            Output.SectionHeader("UNITY");

            Output.LogLine("Find Unity ...");

            string tempOutput = Path.Combine(Config.RootDirectory, "UNITY_EDITOR.tmp");
            ChildProcess.WaitFor("dotnet", Config.RootDirectory, $"{Path.Combine(s_k9, "K9.Unity.dll")} FindEditor --input {Path.Combine(Config.RootDirectory, "UNITY_VERSION")} --output {tempOutput}");

            if (File.Exists(tempOutput))
            {
                string unityEditorPath = File.ReadAllText(tempOutput);
                s_unityEditor = unityEditorPath;
                SetEnvironmentVariable("UNITY_EDITOR", unityEditorPath);
                File.Delete(tempOutput);
            }
            else
            {
                Output.Warning("Unable to find desired Unity editor version.");
            }
        }

        static void SetEnvironmentVariable(string name, string value)
        {
            if (s_arguments.Has(Arguments.TeamCityOption))
            {
                // Set for TeamCity
                Output.LogLine($"##teamcity[setParameter name='{name}' value='{value}']", ConsoleColor.Yellow);
            }

            // Set for user (no-perm request)
            if (s_arguments.Has(Arguments.UserEnvironmentOption))
            {
                Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
            }
        }
    }
}