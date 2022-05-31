// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Reflection;

namespace B4.Utils
{
    public static class Resources
    {
        public static byte[] Get(string filename)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            using Stream resFilestream = a.GetManifestResourceStream(filename);
            if (resFilestream == null)
            {
                return null;
            }

            byte[] ba = new byte[resFilestream.Length];
            int read = resFilestream.Read(ba, 0, ba.Length);
            return read == ba.Length ? ba : null;
        }
    }
}