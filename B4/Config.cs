// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System.IO;

namespace B4
{
    public static class Config
    {
        /// <summary>
        ///     The filename of the standard K9 config.
        /// </summary>
        public static string K9ConfigName = "K9.ini";

        /// <summary>
        ///     The default content for a K9 config.
        /// </summary>
        public static string K9ConfigDefaultContent = "STEAM_Username=UNDEFINED" + "\n" +
                                                      "STEAM_Password=UNDEFINED";


        public static string PingHost = "github.com";

        public static string ProjectName = "NightOwl";

        public static string ProjectRelativePath = Path.Combine("Projects", "NightOwl");

        //public static string RootDirectory;
    }
}