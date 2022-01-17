// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Text;

namespace B4.Utils
{
    public static class ChildProcess
    {
        public static bool SpawnHidden(string executablePath, string workingDirectory, string arguments)
        {
            using Process childProcess = new();
            childProcess.StartInfo.FileName = executablePath;
            childProcess.StartInfo.WorkingDirectory = workingDirectory;
            childProcess.StartInfo.Arguments = string.IsNullOrEmpty(arguments) ? "" : arguments;
            childProcess.StartInfo.UseShellExecute = false;
            childProcess.StartInfo.RedirectStandardOutput = true;
            childProcess.StartInfo.RedirectStandardError = true;
            childProcess.StartInfo.CreateNoWindow = true;
            return childProcess.Start();
        }

        public static bool WaitFor(string executablePath, string workingDirectory, string arguments)
        {
            using Process childProcess = new();
            object lockObject = new();

            void OutputHandler(object x, DataReceivedEventArgs y)
            {
                if (y.Data != null)
                {
                    lock (lockObject)
                    {
                        Output.LogLine(y.Data.TrimEnd(), Output.ExternalForegroundColor);
                    }
                }
            }

            childProcess.StartInfo.WorkingDirectory = workingDirectory;
            childProcess.StartInfo.FileName = executablePath;
            childProcess.StartInfo.Arguments = string.IsNullOrEmpty(arguments) ? "" : arguments;
            childProcess.StartInfo.UseShellExecute = false;
            childProcess.StartInfo.RedirectStandardOutput = true;
            childProcess.StartInfo.RedirectStandardError = true;
            childProcess.OutputDataReceived += OutputHandler;
            childProcess.ErrorDataReceived += OutputHandler;
            childProcess.StartInfo.RedirectStandardInput = false;
            childProcess.StartInfo.CreateNoWindow = true;
            childProcess.StartInfo.StandardOutputEncoding = new UTF8Encoding(false, false);
            childProcess.Start();
            childProcess.BeginOutputReadLine();
            childProcess.BeginErrorReadLine();

            // Busy wait for the process to exit so we can get a ThreadAbortException if the thread is terminated.
            // It won't wait until we enter managed code again before it throws otherwise.
            for (;;)
            {
                if (!childProcess.WaitForExit(20))
                {
                    continue;
                }

                childProcess.WaitForExit();
                break;
            }

            if (childProcess.ExitCode == 0)
            {
                return true;
            }

            Environment.ExitCode = childProcess.ExitCode;
            return false;
        }

        public static bool WaitFor(string executablePath, string workingDirectory, string arguments,
            Action<string> outputAction)
        {
            using Process childProcess = new();
            object lockObject = new();

            void OutputHandler(object x, DataReceivedEventArgs y)
            {
                if (y.Data != null)
                {
                    lock (lockObject)
                    {
                        Output.LogLine(y.Data.TrimEnd(), Output.ExternalForegroundColor);
                        outputAction(y.Data.TrimEnd());
                    }
                }
            }

            childProcess.StartInfo.WorkingDirectory = workingDirectory;
            childProcess.StartInfo.FileName = executablePath;
            childProcess.StartInfo.Arguments = string.IsNullOrEmpty(arguments) ? "" : arguments;
            childProcess.StartInfo.UseShellExecute = false;
            childProcess.StartInfo.RedirectStandardOutput = true;
            childProcess.StartInfo.RedirectStandardError = true;
            childProcess.OutputDataReceived += OutputHandler;
            childProcess.ErrorDataReceived += OutputHandler;
            childProcess.StartInfo.RedirectStandardInput = false;
            childProcess.StartInfo.CreateNoWindow = true;
            childProcess.StartInfo.StandardOutputEncoding = new UTF8Encoding(false, false);
            childProcess.Start();
            childProcess.BeginOutputReadLine();
            childProcess.BeginErrorReadLine();

            // Busy wait for the process to exit so we can get a ThreadAbortException if the thread is terminated.
            // It won't wait until we enter managed code again before it throws otherwise.
            for (;;)
            {
                if (!childProcess.WaitForExit(20))
                {
                    continue;
                }

                childProcess.WaitForExit();
                break;
            }

            if (childProcess.ExitCode == 0)
            {
                return true;
            }

            Environment.ExitCode = childProcess.ExitCode;
            return false;
        }
    }
}