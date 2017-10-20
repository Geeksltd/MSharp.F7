using System.Linq;

namespace Geeks.GeeksProductivityTools
{
    public class TypescriptSettings
    {
        public string[] RootFiles;
        public int ECMAScriptVersion;
        public bool GenerateSourcemap;

        public TypescriptSettings()
        {
            RootFiles = new string[] { };
            ECMAScriptVersion = 5;
            GenerateSourcemap = false;
        }

        public string[] GetFullPathFiles()
        {
            return RootFiles.Select(f => Geeks.GeeksProductivityTools.TypeScript.TypeScriptHelper.GetFullPathFromRelativePath(f)).ToArray();
        }
    }
}
