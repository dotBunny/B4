// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System.IO;
using B4.Utils;

namespace B4.Steps
{
    /// <summary>
    ///     Ensure that K9 is present and updated.
    /// </summary>
    public class Bootstrapper : IStep
    {
        private const string NoKey = "no-b4";
        private const string RepositoryKey = "b4-repo";

        public Bootstrapper()
        {
            Program.Args.RegisterHelp("B4", $"{NoKey}",
                "\t\t\t\tBypass B4 source download and updating.");

            Program.Args.RegisterHelp("B4", $"{RepositoryKey} <value>",
                "\t\tOverride the B4 repository relative path.");
        }

        /// <inheritdoc />
        public string GetID()
        {
            return "bootstrapper";
        }

        /// <inheritdoc />
        public string GetHeader()
        {
            return "B4 Source Code";
        }

        /// <inheritdoc />
        public void Process()
        {
            Program.GetParameter(RepositoryKey, "Projects/B4", out string repositoryDirectory,
                s => Path.GetFullPath(Path.Combine(Program.RootDirectory, s)),
                Directory.Exists);
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