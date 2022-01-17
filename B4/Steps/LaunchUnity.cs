// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System.IO;
using B4.Utils;

namespace B4.Steps
{
    public class LaunchUnity : IStep
    {
        private const string NoArgument = "--no-launch-unity";

        public LaunchUnity()
        {
            Program.Args.RegisterHelp("Launch Unity", NoArgument,
                "\t\tDo not launch project in Unity upon execution.");
        }

        /// <inheritdoc />
        public string GetHeader()
        {
            return "Launch Unity";
        }

        /// <inheritdoc />
        public void Process()
        {
            if (Program.Args.Has(NoArgument) ||
                Program.Args.Has(FindUnity.NoArgument))
            {
                Output.LogLine("Skipped.");
                return;
            }

            if (!string.IsNullOrEmpty(FindUnity.FullPath) && File.Exists(FindUnity.FullPath))
            {
                string projectDirectory = Path.Combine(Program.RootDirectory, Config.ProjectRelativePath);
                if (Program.Args.TryGetValue(Arguments.ProjectDirectoryArgument, out string projectDirectoryOverride))
                {
                    projectDirectory = Path.Combine(Program.RootDirectory, projectDirectoryOverride);
                }
                Output.Value("projectDirectory", projectDirectory);
                Output.LogLine("Launching Editor ...");

                //TODO: Addd &?
                if (!ChildProcess.SpawnHidden(FindUnity.FullPath, projectDirectory, $"-projectPath {projectDirectory}"))
                {
                    Output.Error("Failed to launch Unity.", -1);
                }
            }
        }
    }
}