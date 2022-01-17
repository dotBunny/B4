// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System.IO;
using B4.Utils;

namespace B4.Steps
{
    public class FindUnity : IStep
    {
        public const string NoArgument = "--no-find-unity";

        /// <summary>
        ///     The determined full path to the desired Unity Editor.
        /// </summary>
        public static string FullPath;

        public FindUnity()
        {
            Program.Args.RegisterHelp("Find Unity", NoArgument,
                "\t\t\tDo not find the Unity installation. This will also force skipping the launch of the editor.");
        }

        /// <inheritdoc />
        public string GetHeader()
        {
            return "Find Unity";
        }

        /// <inheritdoc />
        public void Process()
        {
            if (Program.Args.Has(NoArgument))
            {
                Output.LogLine("Skipped.");
                return;
            }

            // TODO: Shouldnt the UNITY_VERSION be attached to the project folder

            Output.LogLine("Build paths ...");
            string temporaryFile = Path.Combine(Program.RootDirectory, "UNITY_EDITOR.tmp");
            Output.Value("temporaryFile", temporaryFile);
            string k9UnityPath = Path.Combine(K9.FullPath, "K9.Unity.dll");
            Output.Value("k9UnityPath", k9UnityPath);
            string inputPath = Path.Combine(Program.RootDirectory, "UNITY_VERSION");
            Output.Value("inputPath", inputPath);

            Output.LogLine("Launch K9.Unity::FindEditor ...");
            ChildProcess.WaitFor("dotnet", Program.RootDirectory,
                $"{k9UnityPath} FindEditor --input {inputPath} --output {temporaryFile}");

            if (File.Exists(temporaryFile))
            {
                Output.LogLine("Output detected ...");
                string unityEditorPath = File.ReadAllText(temporaryFile);
                FullPath = unityEditorPath;
                Output.Value("FindUnity.FullPath", FullPath);

                Program.SetEnvironmentVariable("UNITY_EDITOR", unityEditorPath);

                Output.LogLine("Remove temporary file.");
                File.Delete(temporaryFile);
            }
            else
            {
                Output.Warning("Unable to find desired Unity editor version.");
            }
        }
    }
}