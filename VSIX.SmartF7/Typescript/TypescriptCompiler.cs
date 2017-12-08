using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Geeks.GeeksProductivityTools.TypeScript
{
    public class TypescriptCompiler
    {
        const string ERROR_KEY = "TYPESCRIPT_ERRORLIST_KEY";

        static List<string> includedFiles = new List<string>();
        static List<string> visitedFiles = new List<string>();

        public static void Compile(IEnumerable<string> rootPaths)
        {
            Parallel.ForEach(rootPaths, root => Compile(root));
        }

        public static void Compile(string rootPath)
        {
            var file = new FileInfo(rootPath);
            var newFileName = "@all_" + Path.ChangeExtension(file.Name, ".js");

            var argument = "/C tsc --target ES" + App.Settings.Typescript.ECMAScriptVersion;
            if (App.Settings.Typescript.GenerateSourcemap)
            {
                argument += " --sourcemap";
            }

            argument += " --out " + newFileName + " " + file.Name;

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                WorkingDirectory = file.DirectoryName,
                FileName = "cmd.exe",
                Arguments = argument,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var process = System.Diagnostics.Process.Start(startInfo);

            // make a new thread to read the standard error to avoid deadlock
            string errorText = null;
            var stderrThread = new System.Threading.Thread(() => { errorText = process.StandardError.ReadToEnd(); });
            stderrThread.Start();

            process.WaitForExit();
            stderrThread.Join();

            ErrorList.RemoveError(ERROR_KEY);

            if (errorText.Any())
            {
                ErrorList.AddOrOverrideError(ERROR_KEY, new Microsoft.VisualStudio.Shell.ErrorTask
                {
                    ErrorCategory = Microsoft.VisualStudio.Shell.TaskErrorCategory.Error,
                    Category = Microsoft.VisualStudio.Shell.TaskCategory.Html,
                    Priority = Microsoft.VisualStudio.Shell.TaskPriority.Low,
                    Text = string.Format("Geeks: Compiling [{0}] Failed. {1} --------------------------------------------- {1} {2}", file.FullName, Environment.NewLine, errorText)
                });
            }
            else
            {
                App.DTE.StatusBar.Text = "Geeks: Successfully Combined Js Files";
            }

            var newFile = Path.Combine(file.DirectoryName, newFileName);

            if (File.Exists(newFile))
            {
                // include new Js file
                App.DTE.Solution.FindProjectItem(rootPath).ContainingProject.ProjectItems.AddFromFile(newFile);
            }
        }
    }
}