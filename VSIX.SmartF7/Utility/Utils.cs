using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using EnvDTE;
using EnvDTE80;
using Geeks.GeeksProductivityTools;
using Microsoft.VisualStudio.Shell.Interop;

namespace GeeksAddin
{
    public static class Utils
    {
        public static string GetSolutionName(DTE2 app)
        {
            if (app == null || app.Solution == null || string.IsNullOrEmpty(app.Solution.FullName)) return "";
            return Path.GetFileNameWithoutExtension(app.Solution.FullName);
        }

        public static string[] FindSolutionDirectories(DTE2 app)
        {
            var basePaths = new List<string>();

            if (app.Solution != null)
            {
                for (var i = 1; i <= app.Solution.Projects.Count; i++)
                {
                    var projectItem = app.Solution.Projects.Item(i);
                    AddPathFromProjectItem(basePaths, projectItem);
                }

                return basePaths.ToArray();
            }

            app.StatusBar.Text = "No solution or project is identified. app.Solution is " +
                (app.Solution?.GetType().Name).Or("NULL");

            App.DTE = (DTE2)GeeksProductivityToolsPackage.GetGlobalService(typeof(SDTE));

            return null;
        }

        static void AddPathFromProjectItem(List<string> basePaths, Project projectItem)
        {
            if (projectItem == null) return;

            try
            {
                // Project
                var projectFileName = projectItem.FileName;

                if (!string.IsNullOrWhiteSpace(projectFileName))
                {
                    if (projectItem.Properties.Item("FullPath").Value is string fullPath)
                        basePaths.Add(fullPath);
                }
                else
                {
                    // Folder
                    for (var i = 1; i <= projectItem.ProjectItems.Count; i++)
                        AddPathFromProjectItem(basePaths, projectItem.ProjectItems.Item(i).Object as Project);
                }
            }
            catch (Exception err)
            {
                ErrorNotification.EmailError(err);
            }
        }

        public static IEnumerable<string> SplitCommandLine(string commandLine)
        {
            var inQuotes = false;

            return commandLine.Split(c =>
            {
                if (c == '\"') inQuotes = !inQuotes;
                return !inQuotes && c == ' ';
            }).Select(arg => arg.Trim().TrimMatchingQuotes())
              .Where(arg => !string.IsNullOrEmpty(arg));
        }

        public static IEnumerable<string> Split(this string str, Func<char, bool> controller)
        {
            var nextPiece = 0;

            for (var c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }

        public static string TrimMatchingQuotes(this string input, char quote = '\"')
        {
            if ((input.Length >= 2) &&
                (input[0] == quote) && (input[input.Length - 1] == quote))
                return input.Substring(1, input.Length - 2);

            return input;
        }

        public static async Task<string> FindMSharpProjectUrl(DTE2 app)
        {
            var solutionName = GetSolutionName(app).ToUpper();
            if (solutionName.IsEmpty())
                throw new Exception("No solution is open. Cannot find the M# editor port. Error (on Utils.cs line 120)");

            var openProjectsJson = await new WebClient().DownloadStringTaskAsync("http://localhost:3020/GetProjects");
            var projects = new JavaScriptSerializer().Deserialize<MSharpProject[]>(openProjectsJson);

            if (projects.IsEmpty())
                throw new Exception("No project is open in M#. Error (on Utils.cs line 131)");

            var project = projects.FirstOrDefault(p => p.Name.ToUpper().Contains(solutionName));
            if (project == null)
                throw new Exception("Cannot find any open project in M# to match with solution name:" + solutionName + "Utils.cs line 137");

            return project.MSharpUrl;
        }

        public static bool ContainsAny(this string str, params string[] subStrings)
        {
            foreach (var subString in subStrings)
                if (str.Contains(subString)) return true;

            return false;
        }
    }
}
