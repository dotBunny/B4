// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System.IO;
using B4.Utils;

namespace B4.Steps
{
    /// <summary>
    ///     Apply default config and load any known settings
    /// </summary>
    public class K9Config : IStep
    {
        private SimpleConfig _loadedConfig;

        /// <inheritdoc />
        public string GetHeader()
        {
            return "K9 Config";
        }

        /// <inheritdoc />
        public void Process()
        {
            string configPath = Path.Combine(Program.RootDirectory, "K9.ini");
            Output.Value("configPath", configPath);

            // Write default file out
            if (!File.Exists(configPath))
            {
                Output.LogLine($"Creating default K9.ini config ...");

                byte[] fileData = Resources.Get("Configs\\K9.ini");
                if (fileData != null)
                {
                    File.WriteAllBytes(configPath, fileData);
                }
                else
                {
                    Output.Error("Unable to find default K9.ini data.", -911, false);
                }
            }

            // Parse config
            Output.LogLine($"Loading K9 config ...");
            _loadedConfig = new SimpleConfig(configPath);
            _loadedConfig?.SetEnvironmentVariables();


            // string steamworksDirectory = Path.Combine(Program.RootDirectory, "ThirdParty", "Steamworks");
            // Program.SetEnvironmentVariable("SteamworksDirectory", steamworksDirectory);
            // // ReSharper disable once StringLiteralTypo
            // Program.SetEnvironmentVariable("SteamCommand",
            //     Path.Combine(steamworksDirectory, "sdk", "tools", "ContentBuilder", "builder", "steamcmd.exe"));
        }
    }
}