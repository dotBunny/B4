// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System.IO;
using B4.Utils;

namespace B4.Steps
{
    public class RemotePackages : IStep
    {
        private const string NoKey = "no-remote-packages";
        private const string RemoteManifestKey = "remote-manifest";
        private const string UnityManifestKey = "unity-manifest";

        public RemotePackages()
        {
            Program.Args.RegisterHelp("Remote Packages", NoKey,
                "\t\tDo not process any remote packages.");
            Program.Args.RegisterHelp("Remote Packages", $"{RemoteManifestKey} <value>",
                "\tThe relative path to the remote packages manifest.");
            Program.Args.RegisterHelp("Remote Packages", $"{UnityManifestKey} <value>",
                "\tThe relative path to the unity packages manifest. This really should never change.");
        }

        /// <inheritdoc />
        public string GetID()
        {
            return "remotepackages";
        }

        /// <inheritdoc />
        public string GetHeader()
        {
            return "Remote Packages";
        }

        /// <inheritdoc />
        public void Process()
        {
            if (Program.Args.Has(NoKey))
            {
                Output.LogLine("Skipped.");
                return;
            }

            string remoteManifest = Path.Combine(Program.ProjectDirectory, "RemotePackages", "manifest.json");
            if (Program.Args.TryGetValue(RemoteManifestKey, out string remoteManifestOverride))
            {
                remoteManifest = remoteManifestOverride;
            }

            Output.Value("remoteManifest", remoteManifest);

            string packageManifest = Path.Combine(Program.ProjectDirectory, "Packages", "manifest.json");
            if (Program.Args.TryGetValue(UnityManifestKey, out string packageManifestOverride))
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