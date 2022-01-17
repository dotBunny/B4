// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System.IO;
using B4.Utils;

namespace B4.Steps
{
    public class RemotePackages : IStep
    {
        /// <inheritdoc />
        public string GetHeader()
        {
            return "Remote Packages";
        }

        /// <inheritdoc />
        public void Process()
        {
            string projectDirectory = Path.Combine(Config.RootDirectory, Config.ProjectRelativePath);

            Output.LogLine("Check Manifest ...");
            ChildProcess.WaitFor("dotnet", Config.RootDirectory,
                $"{Path.Combine(K9.FullPath, "K9.Setup.dll")} Checkout --manifest {Path.Combine(projectDirectory, "RemotePackages", "manifest.json")}");

            Output.LogLine("Update Packages ...");
            ChildProcess.WaitFor("dotnet", Config.RootDirectory,
                $"{Path.Combine(K9.FullPath, "K9.Unity.dll")} RemotePackages --remote {Path.Combine(projectDirectory, "RemotePackages", "manifest.json")} --unity {Path.Combine(projectDirectory, "Packages", "manifest.json")}");
        }
    }
}