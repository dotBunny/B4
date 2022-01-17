// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using B4.Utils;

namespace B4
{
    public class SimpleConfig
    {
        private readonly Dictionary<string, string> _config = new();

        public SimpleConfig(string filePath)
        {
            string[] configLines = File.ReadAllLines(filePath);
            foreach (string s in configLines)
            {
                if (string.IsNullOrEmpty(s))
                {
                    continue;
                }

                string[] split = s.Split('=', 2, StringSplitOptions.TrimEntries);
                if (split.Length == 2)
                {
                    string key = split[0];
                    string value = split[1];

                    if (_config.ContainsKey(key))
                    {
                        _config[key] = value;
                    }
                    else
                    {
                        _config.Add(key, value);
                    }
                }
                else
                {
                    Output.LogLine($"Invalid SimpleConfig line found: {s} in {filePath}", ConsoleColor.Red);
                }
            }
        }

        public Dictionary<string, string>.Enumerator GetEnumerator()
        {
            return _config.GetEnumerator();
        }

        public bool TryGetValue(string key, out string value)
        {
            if (_config.ContainsKey(key))
            {
                value = _config[key];
                return true;
            }

            value = null;
            return false;
        }

        public void SetEnvironmentVariables()
        {
            // Setup environment
            foreach (KeyValuePair<string, string> item in _config)
            {
                Program.SetEnvironmentVariable(item.Key, item.Value);
            }
        }
    }
}