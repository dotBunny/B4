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
        private const string RemoteManifestArgument = "--remote-manifest";
        private const string UnityManifestArgument = "--unity-manifest";

        /// <inheritdoc />
        public string GetHeader()
        {
            return "Remote Packages";
        }

        public RemotePackages()
        {
            Program.Args.RegisterHelp("Remote Packages", NoArgument,
                "\t\tDo not process any remote packages.");
            Program.Args.RegisterHelp("Remote Packages", $"{RemoteManifestArgument} <value>",
                "\tThe relative path to the remote packages manifest.");
            Program.Args.RegisterHelp("Remote Packages", $"{UnityManifestArgument} <value>",
                "\tThe relative path to the unity packages manifest. This really should never change.");
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
            if (Program.Args.TryGetValue(Arguments.ProjectDirectoryArgument, out string projectDirectoryOverride))
            {
                projectDirectory = Path.Combine(Program.RootDirectory, projectDirectoryOverride);
            }
            Output.Value("projectDirectory", projectDirectory);

            string remoteManifest = Path.Combine(projectDirectory, "RemotePackages", "manifest.json");
            if (Program.Args.TryGetValue(RemoteManifestArgument, out string remoteManifestOverride))
            {
                remoteManifest = remoteManifestOverride;
            }
            Output.Value("remoteManifest", remoteManifest);

            string packageManifest = Path.Combine(projectDirectory, "Packages", "manifest.json");
            if (Program.Args.TryGetValue(UnityManifestArgument, out string packageManifestOverride))
            {
                packageManifest = packageManifestOverride;
            }
            Output.Value("packageManifest", packageManifest);

            Output.LogLine("Process Remote Manifest ...");
            ChildProcess.WaitFor("dotnet", Program.RootDirectory,
                $"{Path.Combine(K9.FullPath, "K9.Setup.dll")} Checkout --manifest {remoteManifest}");

            Output.LogLine("Update Unity Packages ...");
            ChildProcess.WaitFor("dotnet", Program.RootDirectory,
                $"{Path.Combine(K9.FullPath, "K9.Unity.dll")} RemotePackages --remote {remoteManifest} --unity {packageManifest}");
        }
    }
}