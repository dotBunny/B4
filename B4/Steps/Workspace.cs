// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using B4.Utils;

namespace B4.Steps
{
    public class Workspace : IStep
    {
        public const string NoKey = "no-workspace";

        public Workspace()
        {
            Program.Args.RegisterHelp("Workspace", NoKey,
                "\t\t\tDo not execute workspace configuration.");

        }
        /// <inheritdoc />
        public string GetID()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public string GetHeader()
        {
            throw new System.NotImplementedException();
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

            List<string> output = new List<string>();
            ChildProcess.WaitFor("cm", Program.RootDirectory, "trigger list", s =>
            {
                output.Add(s);
            });

            bool foundExecuteB4 = false;
            int count = output.Count;
            for (int i = 0; i < count; i++)
            {
                string line = output[i].Trim();
                if (line.Contains("Execute B4"))
                {
                    foundExecuteB4 = true;
                }
            }

            if(!foundExecuteB4)
            {
                ChildProcess.WaitFor("cm", Program.RootDirectory,
                    "trigger create after-update \"Execute B4\" \"dotnet @WKSPACE_PATH/B4.dll --no-launch\"");
            }
        }
    }
}