﻿// Copyright (c) 2022 dotBunny Inc.
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
        private const string NoKey = "no-k9";
        private const string PrebuiltKey = "k9";
        private const string RepositoryKey = "k9-repo";

        /// <summary>
        ///     The determined full path to the root of the built K9.
        /// </summary>
        public static string FullPath;

        public K9()
        {
            Program.Args.RegisterHelp("K9", $"{NoKey}",
                "\t\t\t\tBypass K9 installation and updating.");

            Program.Args.RegisterHelp("K9", $"{PrebuiltKey} <value>",
                "\t\t\tOverride the K9 prebuilt relative path.");

            Program.Args.RegisterHelp("K9", $"{RepositoryKey} <value>",
                "\t\tOverride the K9 repository relative path.");
        }

        /// <inheritdoc />
        public string GetID()
        {
            return "k9";
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
            if (Program.Args.TryGetValue(PrebuiltKey, out string prebuiltOverride))
            {
                if (Directory.Exists(prebuiltOverride))
                {
                    prebuiltDirectory = prebuiltOverride;
                }
            }

            Output.Value("prebuiltDirectory", prebuiltDirectory);

            // TODO: Config?
            string repositoryDirectory = null;
            if (Program.Config.TryGetValue(RepositoryKey, out string defaultRepositoryDirectory))
            {
                repositoryDirectory = defaultRepositoryDirectory;
            }
            if (Program.Args.TryGetValue(RepositoryKey, out string repositoryOverride))
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
                if (!Program.Args.Has(NoKey))
                {
                    Git.GetOrUpdate("K9", repositoryDirectory, "https://github.com/dotBunny/K9", () =>
                    {
                        Output.LogLine("Building K9 (Release) ...");
                        if (!ChildProcess.WaitFor("dotnet.exe", repositoryDirectory,
                                "build K9.sln --configuration Release"))
                        {
                            Output.Error("Unable to build K9", -1, true);
                        }
                    });
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
            Set.EnvironmentVariable("K9", FullPath);
        }
    }
}