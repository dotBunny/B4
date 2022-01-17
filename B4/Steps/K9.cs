// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using B4.Utils;

namespace B4.Steps
{
    /// <summary>
    ///     Ensure that K9 is present and updated.
    /// </summary>
    public class K9 : IStep
    {
        private const string NoArgument = "--no-k9";
        private const string PrebuiltArgument = "--k9";
        private const string RepositoryArgument = "--k9-repo";

        /// <summary>
        ///     The determined full path to the root of the built K9.
        /// </summary>
        public static string FullPath;

        public K9()
        {
            Program.Args.RegisterHelp("K9", $"{NoArgument}",
                "\t\t\t\tBypass K9 installation and updating.");

            Program.Args.RegisterHelp("K9", $"{PrebuiltArgument} <value>",
                "\t\t\tOverride the K9 prebuilt relative path.");

            Program.Args.RegisterHelp("K9", $"{RepositoryArgument} <value>",
                "\t\tOverride the K9 repository relative path.");
        }

        /// <inheritdoc />
        public string GetHeader()
        {
            return "K9 Installation";
        }

        /// <inheritdoc />
        public void Process()
        {
            // TODO: Config?
            string prebuiltDirectory = Path.Combine(Program.RootDirectory, "K9");
            if (Program.Args.TryGetValue(PrebuiltArgument, out string prebuiltOverride))
            {
                if (Directory.Exists(prebuiltOverride))
                {
                    prebuiltDirectory = prebuiltOverride;
                }
            }

            Output.Value("prebuiltDirectory", prebuiltDirectory);

            // TODO: Config?
            string repositoryDirectory = Path.Combine(Program.RootDirectory, "Projects", "K9");
            if (Program.Args.TryGetValue(RepositoryArgument, out string repositoryOverride))
            {
                if (Directory.Exists(repositoryOverride))
                {
                    repositoryDirectory = repositoryOverride;
                }
            }

            Output.Value("repositoryDirectory", repositoryDirectory);

            if (Directory.Exists(prebuiltDirectory))
            {
                FullPath = prebuiltDirectory;
                Output.LogLine("Found prebuilt K9.", ConsoleColor.Green);
            }
            else if (Program.IsOnline)
            {
                if (!Program.Args.Has(NoArgument))
                {
                    if (Directory.Exists(repositoryDirectory))
                    {
                        Output.LogLine("Fetching repository updates ...");
                        // Grab latest (required really to proceed)
                        if (!ChildProcess.WaitFor("git.exe", repositoryDirectory, "fetch origin"))
                        {
                            Output.Error("Unable to fetch updates for K9.", Environment.ExitCode, true);
                        }

                        // Check if repository is behind
                        Output.LogLine("Checking repository status ...");
                        bool isBehind = false;
                        bool gitStatus = ChildProcess.WaitFor("git.exe", repositoryDirectory, "status -sb", line =>
                        {
                            if (line.Contains("behind"))
                            {
                                isBehind = true;
                            }
                        });

                        if (!gitStatus)
                        {
                            Output.Error("Unable to understand the status of the K9 repository.", Environment.ExitCode,
                                true);
                        }

                        if (isBehind)
                        {
                            Output.LogLine("Resetting local K9 source ...");
                            if (!ChildProcess.WaitFor("git.exe", repositoryDirectory, "reset --hard"))
                            {
                                Output.Warning("Unable to reset K9 repository.");
                            }

                            Output.LogLine("Getting latest K9 source ...");
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
                        if (!ChildProcess.WaitFor("git.exe", Program.RootDirectory,
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
                }
                else
                {
                    Output.Warning("Ignoring K9 installation/upgrading.");
                }
                FullPath = Path.Combine(repositoryDirectory, "Build", "Release");
            }
            else
            {
                FullPath = Path.Combine(repositoryDirectory, "Build", "Release");
                Output.Warning("Skipping K9 updates, unable to reach endpoint.");
            }

            Output.Value("K9", FullPath);
            Program.SetEnvironmentVariable("K9", FullPath);
        }
    }
}