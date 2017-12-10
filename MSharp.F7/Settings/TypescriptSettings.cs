using System.Linq;

namespace MSharp.F7
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
            return RootFiles.Select(f => MSharp.F7.TypeScript.TypeScriptHelper.GetFullPathFromRelativePath(f)).ToArray();
        }
    }
}
