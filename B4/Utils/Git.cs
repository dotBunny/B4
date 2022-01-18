// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;

namespace B4.Utils
{
    public static class Git
    {
        public static void GetOrUpdate(string name, string repositoryDirectory, string repositoryURI, Action postUpdate)
        {
            if (Directory.Exists(repositoryDirectory))
            {
                Output.LogLine("Fetching repository updates ...");

                // Grab latest (required really to proceed)
                if (!ChildProcess.WaitFor("git.exe", repositoryDirectory, "fetch origin"))
                {
                    Output.Error($"Unable to fetch updates for {name}.", Environment.ExitCode, true);
                }

                // Check if repository is behind
                Output.LogLine("Checking repository status ...");
                bool isBehind = false;
                bool gitStatus = ChildProcess.WaitFor("git.exe", repositoryDirectory, "status -sb", line =>
                {
                    if (line.Contains("behind"))
                    {
                        isBehind = true;
                    }
                });

                if (!gitStatus)
                {
                    Output.Error($"Unable to understand the status of the {name} repository.", Environment.ExitCode,
                        true);
                }

                if (isBehind)
                {
                    Output.LogLine($"Resetting local {name} source ...");
                    if (!ChildProcess.WaitFor("git.exe", repositoryDirectory, "reset --hard"))
                    {
                        Output.Warning($"Unable to reset {name} repository.");
                    }

                    Output.LogLine($"Getting latest {name} source ...");
                    if (!ChildProcess.WaitFor("git.exe", repositoryDirectory, "pull"))
                    {
                        Output.Warning($"Unable to pull updates for {name} repository.");
                    }


                }
                else
                {
                    Output.LogLine($"{name} is up-to-date.");
                }
            }
            else
            {
                Output.LogLine($"Getting latest {name} source ...");
                if (!ChildProcess.WaitFor("git.exe", Program.RootDirectory,
                        $"clone {repositoryURI} {repositoryDirectory}"))
                {
                    Output.Error($"Unable to clone {name}.", -1, true);
                }
                postUpdate?.Invoke();
            }
        }
    }
}