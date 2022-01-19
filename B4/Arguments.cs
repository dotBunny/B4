// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using B4.Utils;

namespace B4
{
    public class Arguments
    {
        private const string ArgumentPrefix = "--";
        public const string HelpKey = "help";
        public const string RootDirectoryKey = "root-directory";
        public const string SetTeamCityKey = "set-teamcity";
        public const string SetUserEnvironmentKey = "set-user-env";
        public const string ProjectDirectoryKey = "project-directory";
        public const string PingHostKey = "ping-host";
        public const string DefaultConfigKey = "default-config";

        /// <summary>
        ///     A list of arguments post processing.
        /// </summary>
        private readonly string[] _cleanedArguments;

        private readonly int _cleanedArgumentsLength;

        private readonly string[] _rawArguments;

        private readonly Dictionary<string, Dictionary<string, string>> _registeredHelp = new();

        public Arguments(string[] args)
        {
            List<string> processedArguments = new(args.Length);
            List<string> rawArguments = new(args.Length);
            foreach (string s in args)
            {
                if (string.IsNullOrEmpty(s))
                {
                    continue;
                }

                string p = s.Trim().ToLower();
                if (p.StartsWith(ArgumentPrefix))
                {
                    p = p.Substring(ArgumentPrefix.Length);
                }

                processedArguments.Add(p);
                rawArguments.Add(s);
            }

            _cleanedArguments = processedArguments.ToArray();
            _cleanedArgumentsLength = _cleanedArguments.Length;
            _rawArguments = rawArguments.ToArray();

            StringBuilder argumentChain = new();
            foreach (string s in _rawArguments)
            {
                argumentChain.Append($"{s} ");
            }

            Output.LogLine($"Using arguments {argumentChain.ToString().Trim()}");
        }

        public void RegisterHelp(string section, string arg, string message)
        {
            // Check for section
            if (!_registeredHelp.ContainsKey(section))
            {
                _registeredHelp.Add(section, new Dictionary<string, string>());
            }

            // Add messaging
            if (_registeredHelp[section].ContainsKey(arg))
            {
                _registeredHelp[section][arg] = message;
            }
            else
            {
                _registeredHelp[section].Add(arg, message);
            }
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
            Output.NextLine();
            Output.LogLine(
                "Simple configuration options which lean heavily towards configuring the bootstrap process for CI/CD solutions.",
                ConsoleColor.DarkGray);

            Output.SectionHeader("General");

            Output.Log($"{ArgumentPrefix}{HelpKey}", ConsoleColor.Cyan);
            Output.Log("\t\t\t\tOutput helpful information.");
            Output.NextLine();

            Output.Log($"{ArgumentPrefix}{DefaultConfigKey}", ConsoleColor.Cyan);
            Output.Log("\t\tOutput the default B4.ini in the root directory.");
            Output.NextLine();

            Output.Log($"{ArgumentPrefix}{PingHostKey} ", ConsoleColor.Cyan);
            Output.Log("<", ConsoleColor.Cyan);
            Output.Log("value");
            Output.Log(">", ConsoleColor.Cyan);
            Output.Log("\t\tOverride the host to ping to detect an online connection.");
            Output.NextLine();

            Output.Log($"{ArgumentPrefix}{RootDirectoryKey} ", ConsoleColor.Cyan);
            Output.Log("<", ConsoleColor.Cyan);
            Output.Log("value");
            Output.Log(">", ConsoleColor.Cyan);
            Output.Log("\tOverride the absolute path of the root workspace directory.");
            Output.NextLine();

            Output.Log($"{ArgumentPrefix}{ProjectDirectoryKey} ", ConsoleColor.Cyan);
            Output.Log("<", ConsoleColor.Cyan);
            Output.Log("value");
            Output.Log(">", ConsoleColor.Cyan);
            Output.Log("\tOverride the absolute path to the Unity project.");
            Output.NextLine();

            Output.Log($"{ArgumentPrefix}{SetTeamCityKey}", ConsoleColor.Cyan);
            Output.Log("\t\t\tSet TeamCity environment variables.");
            Output.NextLine();

            Output.Log($"{ArgumentPrefix}{SetUserEnvironmentKey}", ConsoleColor.Cyan);
            Output.Log("\t\t\tSet user environment variables.");
            Output.NextLine();

            Output.NextLine();
            Output.LogLine(
                "The following arguments are meant to provide fine-grain control of the bootstrap steps, overriding any default values and/or values set in the B4.ini file.",
                ConsoleColor.DarkGray);

            foreach (KeyValuePair<string, Dictionary<string, string>> section in _registeredHelp)
            {
                Output.SectionHeader(section.Key);
                foreach (KeyValuePair<string, string> item in section.Value)
                {
                    if (item.Key.EndsWith("<value>"))
                    {
                        Output.Log($"{ArgumentPrefix}{item.Key.Substring(0, item.Key.Length - 7)}", ConsoleColor.Cyan);
                        Output.Log("<", ConsoleColor.Cyan);
                        Output.Log("value");
                        Output.Log(">", ConsoleColor.Cyan);
                    }
                    else
                    {
                        Output.Log($"{ArgumentPrefix}{item.Key}", ConsoleColor.Cyan);
                    }

                    Output.Log(item.Value);
                    Output.NextLine();
                }
            }
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