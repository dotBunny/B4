﻿// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using B4.Utils;

namespace B4.Steps
{
    public class VCSTriggers : IStep
    {
        public const string NoKey = "no-vcs-triggers";

        enum Provider
        {
            None,
            Plastic,
            Perforce,
            Git
        }

        public VCSTriggers()
        {
            Program.Args.RegisterHelp("VCS Triggers", NoKey,
                "\t\t\tDo not check for vcs commit and update triggers.");

        }
        public string GetID()
        {
            return "vcstriggers";
        }

        /// <inheritdoc />
        public string GetHeader()
        {
            return "VCS Triggers";
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

            switch (GetProvider())
            {
                case Provider.Git:
                    Output.LogLine("Checking GIT hooks ...");
                    break;
                case Provider.Plastic:
                    Output.LogLine("Checking PlasticSCM triggers ...");
                    CheckPlasticSCM();
                    break;
                case Provider.Perforce:
                    Output.LogLine("Checking Perforce triggers ...");
                    break;
            }
        }

        Provider GetProvider()
        {
            string directory = Path.Combine(Program.RootDirectory, ".plastic");
            if (Directory.Exists(directory))
            {
                return Provider.Plastic;
            }

            directory = Path.Combine(Program.RootDirectory, ".git");
            if (Directory.Exists(directory))
            {
                return Provider.Git;
            }

            // TODO: slow perforce check?

            return Provider.None;
        }


        void CheckPlasticSCM()
        {
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
            else
            {
                Output.LogLine("Found existing trigger.");
            }
        }
    }
}