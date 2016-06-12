using System;
using System.Diagnostics;
using System.IO;

namespace runProgram
{
    class Program
    {

        static void Main(string[] args)
        {
            StartEXE(@"D:\7-Zip\7zFM.exe", @"D:\7-Zip", @"F:\software\MathType.7z");
            Console.WriteLine();
        }

        static private Process StartProcess(string exePath, string workDir, string arguments)
        {
            Process proc = null;

            ProcessStartInfo info = new ProcessStartInfo();

            info.WorkingDirectory = workDir;
            info.Arguments = arguments;
            info.FileName = exePath;

            if (Environment.GetEnvironmentVariable("AutomationDebug") == "false")
                info.WindowStyle = ProcessWindowStyle.Hidden;

            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;

            try
            {
                proc = Process.Start(info);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            return proc;
        }

        static bool StartEXE(string exePath, string workDir, string arguments)
        {
            Process proc = StartProcess(exePath, workDir, arguments);

            //Get Process Handle to output to console
            if (proc == null)
            {
                System.Console.WriteLine("Create new process failed!");
            }
            else
            {
                while (!proc.StandardOutput.EndOfStream)
                {
                    System.Console.WriteLine(proc.StandardOutput.ReadLine());
                }
                System.Console.WriteLine("Process run finisned with code:{0}", proc.ExitCode);
            }

            return proc.ExitCode == 0;
        }

        static bool FileExists(string dir, string pattern)
        {
            string[] files = Directory.GetFiles(dir, pattern);
            if (files != null && files.Length > 0) { Console.WriteLine("{0} founded", pattern); return true; }
            return false;
        }

        static void SetEnvironmentVariables(string name, string value)
        {
            Environment.SetEnvironmentVariable(name, value);
        }
    }
}