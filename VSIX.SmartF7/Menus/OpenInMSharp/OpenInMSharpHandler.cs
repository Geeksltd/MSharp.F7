using System.IO;

namespace Geeks.GeeksProductivityTools.Menus.OpenInMSharp
{
    public abstract class OpenInMSharpHandler
    {
        internal bool IsItemVisible(string fileName) => fileName.IsMvcFile() || fileName.IsWebFormsFile() || fileName.IsEntityFile();

        public string BuildCompleteUrl(string url, string fileName)
        {
            if (fileName.IsEntityFile() || fileName.IsMvcFile())
                return url.ToNoneWebFormUrl(fileName);

            return url.ToWebFormUrl(fileName);
        }

        // sometimes the library of EnvDTE gives a full lower case value of a file path which then will preventing resolving it 
        // properly in the M#, that is why, the following method is applied.
        public string GetProperFilePathCapitalization(string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            var dirInfo = fileInfo.Directory;

            return Path.Combine(GetProperDirectoryCapitalization(dirInfo), dirInfo.GetFiles(fileInfo.Name)[0].Name);
        }

        string GetProperDirectoryCapitalization(DirectoryInfo dirInfo)
        {
            var parentDirInfo = dirInfo.Parent;

            if (null == parentDirInfo) return dirInfo.Name;

            return Path.Combine(GetProperDirectoryCapitalization(parentDirInfo), parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
        }
    }
}
