// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System.IO;
using B4.Utils;

namespace B4.Steps
{
    public class RemotePackages : IStep
    {
        private const string NoArgument = "--no-remote-packages";

        /// <inheritdoc />
        public string GetHeader()
        {
            return "Remote Packages";
        }

        public RemotePackages()
        {
            Program.Args.RegisterHelp("Remote Packages", NoArgument,
                "\t\tDo not process any remote packages.");
        }

        /// <inheritdoc />
        public void Process()
        {
            if (Program.Args.Has(NoArgument))
            {
                Output.LogLine("Skipped.");
                return;
            }

            string projectDirectory = Path.Combine(Program.RootDirectory, Config.ProjectRelativePath);

            Output.LogLine("Check Manifest ...");
            ChildProcess.WaitFor("dotnet", Program.RootDirectory,
                $"{Path.Combine(K9.FullPath, "K9.Setup.dll")} Checkout --manifest {Path.Combine(projectDirectory, "RemotePackages", "manifest.json")}");

            Output.LogLine("Update Packages ...");
            ChildProcess.WaitFor("dotnet", Program.RootDirectory,
                $"{Path.Combine(K9.FullPath, "K9.Unity.dll")} RemotePackages --remote {Path.Combine(projectDirectory, "RemotePackages", "manifest.json")} --unity {Path.Combine(projectDirectory, "Packages", "manifest.json")}");
        }
    }
}