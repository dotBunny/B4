// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System.IO;
using B4.Utils;

namespace B4.Steps
{
    public class LaunchUnity : IStep
    {
        private const string NoKey = "no-launch-unity";

        public LaunchUnity()
        {
            Program.Args.RegisterHelp("Launch Unity", NoKey,
                "\t\tDo not launch project in Unity upon execution.");
        }

        /// <inheritdoc />
        public string GetID()
        {
            return "launchunity";
        }

        /// <inheritdoc />
        public string GetHeader()
        {
            return "Launch Unity";
        }

        /// <inheritdoc />
        public void Process()
        {
            if (Program.Args.Has(NoKey) ||
                Program.Args.Has(FindUnity.NoKey))
            {
                Output.LogLine("Skipped.");
                return;
            }

            if (!string.IsNullOrEmpty(FindUnity.FullPath) && File.Exists(FindUnity.FullPath))
            {
                Output.LogLine("Launching Editor ...");

                //TODO: Addd &?
                if (!ChildProcess.SpawnHidden(FindUnity.FullPath, Program.ProjectDirectory,
                        $"-projectPath {Program.ProjectDirectory}"))
                {
                    Output.Error("Failed to launch Unity.", -1);
                }
            }
        }
    }
}