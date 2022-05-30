// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;

namespace B4.Utils
{
    public static class Git
    {
        public static string GetLocalCommit(string repositoryDirectory)
        {
            List<string> output = new List<string>();
            ChildProcess.WaitFor("git.exe", repositoryDirectory, "rev-parse HEAD", Line =>
            {
                Output.LogLine(Line);
                output.Add(Line);
            });
            return output[0].Trim();
        }
        public static void GetOrUpdate(string name, string repositoryDirectory, string repositoryURI,
            Action onUpdate = null)
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
                    bool failures = false;
                    Output.LogLine($"Resetting local {name} source ...");
                    if (!ChildProcess.WaitFor("git.exe", repositoryDirectory, "reset --hard"))
                    {
                        Output.Warning($"Unable to reset {name} repository.");
                        failures = true;
                    }

                    Output.LogLine($"Getting latest {name} source ...");
                    if (!ChildProcess.WaitFor("git.exe", repositoryDirectory, "pull"))
                    {
                        Output.Warning($"Unable to pull updates for {name} repository.");
                        failures = true;
                    }


                    if (!failures)
                    {
                        onUpdate?.Invoke();
                    }
                    else
                    {
                        Output.Error("Failures occured while trying to update code.", -1, true);
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
                else
                {
                    onUpdate?.Invoke();
                }
            }
        }
    }
}