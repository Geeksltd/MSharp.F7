using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Geeks.GeeksProductivityTools.TypeScript
{
    public class FileParser
    {
        const string COMMENT = "///", REFERENCE = "<reference";

        public static IEnumerable<string> FindReferenceFiles(string path, bool includeDeclarations = false)
        {
            var refs = FindReferences(path, includeDeclarations);
            foreach (var item in refs)
                yield return new Uri(new Uri(path), item.TrimStart('/')).LocalPath;

        }

        public static IEnumerable<string> FindReferences(string path, bool includeDeclarations = false)
        {
            var lines = File.ReadAllLines(path);

            foreach (var line in lines)
            {
                var beginningOfComment = line.IndexOf(COMMENT);
                if (beginningOfComment == -1) // Check if line has comment
                    continue;

                var commentedPart = line.Substring(beginningOfComment);

                var beginningOfReference = commentedPart.IndexOf(REFERENCE);
                if (beginningOfReference == -1) // Check if commented part has reference tag
                    continue;

                var xmlText = GetReferenceXML(commentedPart);
                var pathElement = GetPathAttribute(xmlText);

                if (pathElement == null) // Check if element is actually found
                    continue;

                var pathValue = pathElement.Value.ToString();
                if (!includeDeclarations && pathValue.EndsWith(".d.ts")) // Remove .d.ts files
                    continue;

                yield return pathValue;
            }
        }

        public static IEnumerable<string> FindAllReferenceFiles(string path, bool includeDeclarations = false, List<string> collectedFiles = null)
        {
            if (collectedFiles == null)
                collectedFiles = new List<string>();

            var result = new List<string>();

            var allPathsInFile = FindReferenceFiles(path).Select(f => f.ToLower());

            // Skip files if we already contain them
            var staticResult = new List<string>();
            foreach (var item in allPathsInFile)
            {
                if (collectedFiles.Contains(item)) continue;

                collectedFiles.Add(item);
                result.Add(item);
                staticResult.Add(item);
            }

            foreach (var p in staticResult)
            {
                result.AddRange(FindAllReferenceFiles(p, includeDeclarations, collectedFiles));
            }

            return result;
        }

        /// <summary>
        /// Returns a string of the reference part if any (i.e. &lt;reference path="xxxxxxxxx"/>).
        /// </summary>
        /// <param name="referenceString">The reference string.</param>
        /// <returns></returns>
        static string GetReferenceXML(string referenceString)
        {
            var refStart = referenceString.IndexOf("<");
            var refLength = (referenceString.IndexOf("/>") + 2) - refStart;

            var xmlText = referenceString.Substring(refStart, refLength);
            return xmlText;
        }

        static XAttribute GetPathAttribute(string reference) => XElement.Parse(reference).Attribute("path");
    }
}