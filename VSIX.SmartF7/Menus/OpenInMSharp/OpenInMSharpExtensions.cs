using System;
using System.IO;
using GeeksAddin;

namespace Geeks.GeeksProductivityTools.Menus.OpenInMSharp
{
    internal static class FileTypeDetector
    {
        internal static bool IsMvcFile(this string fileName) => fileName.ToUpper().EndsWithAny("CONTROLLER.CS", ".CSHTML");

        internal static bool IsEntityFile(this string fileName) => fileName.ToUpper().EndsWith(".CS") && fileName.ContainsAny("\\@LOGIC\\", "\\LOGIC\\", "\\-LOGIC\\", "\\ENTITIES\\");

        internal static bool IsWebFormsFile(this string fileName) => fileName.ToUpper().EndsWithAny("ASPX", "ASPX.CS", "ASCX", "ASCX.CS");
    }

    internal static class UrlBuilder
    {
        internal static string ToNoneWebFormUrl(this string url, string fileName) => url += "?file=" + fileName.TrimStart(Path.GetDirectoryName(App.DTE.Solution.FullName)).TrimStart("\\");

        internal static string ToWebFormUrl(this string url, string fileName) => url += "?moduleFileName=" + Path.GetFileName(fileName);
    }
}
