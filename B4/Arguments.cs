// Copyright (c) 2020-2021 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace B4
{
    public class Arguments
    {
        public const string NoLaunchOption = "-no-launch";
        public const string RootDirectoryOption = "-root-directory";
        public const string TeamCityOption = "-teamcity";
        public const string UserEnvironmentOption = "-user-env";
        public const string K9PrebuiltOption = "-k9";
        public const string K9RepositoryOption = "-k9-repo";

        /// <summary>
        /// A list of arguments post processing.
        /// </summary>
        private readonly string[] _cleanedArguments;

        private readonly string[] _rawArguments;

        private readonly int _cleanedArgumentsLength;

        public Arguments(string[] args)
        {
            List<string> processedArguments = new(args.Length);
            List<string> rawArguments = new(args.Length);
            foreach (string s in args)
            {
                if (string.IsNullOrEmpty(s)) continue;
                string argument = s.ToLower();
                if (argument.StartsWith("--"))
                {
                    argument = argument.Substring(1);
                }

                processedArguments.Add(argument);
                rawArguments.Add(s);
            }
            _cleanedArguments = processedArguments.ToArray();
            _cleanedArgumentsLength = _cleanedArguments.Length;
            _rawArguments = rawArguments.ToArray();
        }

        public bool Has(string argument)
        {
            for (int i = 0; i < _cleanedArgumentsLength; i++)
            {
                if (_cleanedArguments[i] == argument)
                {
                    return true;
                }
            }
            return false;
        }

        public void Help()
        {
            Output.SectionHeader("HELP");

            Output.Log("--help", ConsoleColor.Cyan);
            Output.Log("\t\t\t\tOutput helpful information.");
            Output.NextLine();

            Output.Log("--k9 ", ConsoleColor.Cyan);
            Output.Log("<", ConsoleColor.Cyan);
            Output.Log("value");
            Output.Log(">", ConsoleColor.Cyan);
            Output.Log("\t\t\tOverride the K9 prebuilt relative path.");
            Output.NextLine();

            Output.Log("--k9-repo ", ConsoleColor.Cyan);
            Output.Log("<", ConsoleColor.Cyan);
            Output.Log("value");
            Output.Log(">", ConsoleColor.Cyan);
            Output.Log("\t\tOverride the K9 repository location.");
            Output.NextLine();

            Output.Log("--no-launch", ConsoleColor.Cyan);
            Output.Log("\t\t\tDo not launch project in Unity upon execution.");
            Output.NextLine();

            Output.Log("--root-directory ", ConsoleColor.Cyan);
            Output.Log("<", ConsoleColor.Cyan);
            Output.Log("value");
            Output.Log(">", ConsoleColor.Cyan);
            Output.Log("\tOverride the root workspace directory.");
            Output.NextLine();

            Output.Log("--teamcity", ConsoleColor.Cyan);
            Output.Log("\t\t\tSet TeamCity environment variables.");
            Output.NextLine();

            Output.Log("--user-env", ConsoleColor.Cyan);
            Output.Log("\t\t\tSet user environment variables.");
            Output.NextLine();
        }

        public bool TryGetValue(string argument, out string value)
        {
            int argumentIndex = -1;
            for (int i = 0; i < _cleanedArguments.Length; i++)
            {
                if (_cleanedArguments[i] == argument)
                {
                    argumentIndex = i;
                    break;
                }
            }

            int valueIndex = argumentIndex + 1;
            if (argumentIndex != -1 && valueIndex < _cleanedArgumentsLength)
            {
                value = _rawArguments[valueIndex];
                return true;
            }

            value = null;
            return false;
        }
    }
}