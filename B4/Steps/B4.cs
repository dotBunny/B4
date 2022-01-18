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
    public class B4 : IStep
    {
        private const string NoKey = "no-b4";
        private const string RepositoryKey = "b4-repo";

        public B4()
        {
            Program.Args.RegisterHelp("B4", $"{NoKey}",
                "\t\t\t\tBypass B4 source download and updating.");

            Program.Args.RegisterHelp("B4", $"{RepositoryKey} <value>",
                "\t\tOverride the B4 repository relative path.");
        }

        /// <inheritdoc />
        public string GetHeader()
        {
            return "B4 Source Code";
        }

        /// <inheritdoc />
        public void Process()
        {
            // TODO: Config?
            string repositoryDirectory = Path.Combine(Program.RootDirectory, "Projects", "B4");
            if (Program.Args.TryGetValue(RepositoryKey, out string repositoryOverride))
            {
                if (Directory.Exists(repositoryOverride))
                {
                    repositoryDirectory = repositoryOverride;
                }
            }

            Output.Value("repositoryDirectory", repositoryDirectory);

            if (!Program.Args.Has(NoKey))
            {
                Git.GetOrUpdate("B4", repositoryDirectory, "https://github.com/dotBunny/B4");
            }
            else
            {
                Output.Warning("Ignoring B4 download/updating.");
            }
        }
    }
}