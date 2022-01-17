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
        public SimpleConfig LoadedConfig;

        /// <inheritdoc />
        public string GetHeader()
        {
            return "K9 Config";
        }

        /// <inheritdoc />
        public void Process()
        {
            string configPath = Path.Combine(Config.RootDirectory, Config.K9ConfigName);

            // Write default file out
            if (!File.Exists(configPath))
            {
                Output.LogLine("Creating DEFAULT K9 config ...");
                File.WriteAllText(configPath, Config.K9ConfigDefaultContent);
            }

            // Parse config
            Output.LogLine($"Loading K9 config @ {configPath} ...");
            LoadedConfig = new SimpleConfig(configPath);
            LoadedConfig?.SetEnvironmentVariables();


            string steamworksDirectory = Path.Combine(Config.RootDirectory, "ThirdParty", "Steamworks");
            Program.SetEnvironmentVariable("SteamworksDirectory", steamworksDirectory);
            // ReSharper disable once StringLiteralTypo
            Program.SetEnvironmentVariable("SteamCommand",
                Path.Combine(steamworksDirectory, "sdk", "tools", "ContentBuilder", "builder", "steamcmd.exe"));
        }
    }
}