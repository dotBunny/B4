// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System.IO;
using B4.Utils;

namespace B4.Steps
{
    public class LaunchUnity : IStep
    {
        /// <inheritdoc />
        public string GetHeader()
        {
            return "Launch Unity";
        }

        /// <inheritdoc />
        public void Process()
        {
            if (Program.Args.Has(Arguments.NoLaunchOption))
            {
                Output.LogLine("Skipped.");
                return;
            }

            if (!string.IsNullOrEmpty(FindUnity.FullPath) && File.Exists(FindUnity.FullPath))
            {
                string projectDirectory = Path.Combine(Config.RootDirectory, Config.ProjectRelativePath);
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