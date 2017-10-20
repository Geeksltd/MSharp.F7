using System;
using System.Collections.Generic;
using System.Linq;

namespace Geeks.GeeksProductivityTools.TypeScript
{
    public static class TypeScriptHelper
    {
        /// <summary>
        /// Gets the selected typescript files in the project explorer.
        /// </summary>
        /// <param name="App">The application.</param>
        /// <returns></returns>
        public static IEnumerable<string> GetSelectedTypescriptFiles()
        {
            return GeeksAddin.DteExtensions.GetRelativeItemPaths().Where(f => IsValidTsFile(f));
        }

        /// <summary>
        /// Returns all files that should be recompiled due to their relation to
        /// the specific file (the given path).
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static IEnumerable<string> RootFilesToCompileFromFile(string path)
        {
            if (App.Settings.Typescript.GetFullPathFiles().Contains(path))
                return new[] { path };

            var compile = new List<string>();
            foreach (var file in App.Settings.Typescript.GetFullPathFiles())
            {
                if (Geeks.GeeksProductivityTools.TypeScript.FileParser.FindAllReferenceFiles(file).Contains(path.ToLower()))
                    compile.Add(file);
            }

            return compile;
        }

        /// <summary>
        /// Determines whether the given name is a valid TypeScript File Name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static bool IsValidTsFile(string name) => name.EndsWith(".ts") && !name.EndsWith(".d.ts");

        public static string GetFullPathFromRelativePath(string relativePath)
        {
            return System.IO.Path.GetDirectoryName(App.DTE.Solution.FullName) + relativePath;
        }
    }
}
