// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System;

namespace B4.Utils
{
    public static class Set
    {
        public static void EnvironmentVariable(string name, string value)
        {
            if (Program.Args.Has(Arguments.SetTeamCityKey))
            {
                // Set for TeamCity
                Output.LogLine($"##teamcity[setParameter name='{name}' value='{value}']", ConsoleColor.Yellow);
            }

            // Set for user (no-perm request)
            if (Program.Args.Has(Arguments.SetUserEnvironmentKey))
            {
                Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
            }
        }
    }
}