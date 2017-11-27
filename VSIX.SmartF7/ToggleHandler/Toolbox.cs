using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Geeks.GeeksProductivityTools;

namespace Geeks.SmartF7.ToggleHandler
{
    public static class Toolbox
    {
        public static string SolutionDir = App.DTE.Solution.FullName.Substring(0, App.DTE.Solution.FullName.LastIndexOf(@"\"));

        public static bool ContainsAny(this string str, params string[] subStrings)
        {
            foreach (var subString in subStrings)
                if (str.Contains(subString))
                    return true;

            return false;
        }

        public static IEnumerable<ProjectItem> GetProjectItems(this ProjectItems projectItems)
        {
            foreach (ProjectItem item in projectItems)
            {
                yield return item;

                if (item.SubProject != null)
                {
                    foreach (ProjectItem childItem in GetProjectItems(item.SubProject.ProjectItems))
                        yield return childItem;
                }
                else
                {
                    foreach (ProjectItem childItem in GetProjectItems(item.ProjectItems))
                        yield return childItem;
                }
            }

        }
        //----mvc pages--------------
        public static string GetMvcPageNameOfUIPage(this ProjectItem uiPageItem)
        {
            var nameSpace = uiPageItem.FileCodeModel.CodeElements.OfType<CodeElement>().SingleOrDefault(d => d.Kind == vsCMElement.vsCMElementNamespace).Name;
            return nameSpace.Remove("Root").Remove(".") + uiPageItem.Name + "html";
        }

        public static string GetMvcControllerOfUIPage(this Document uiPageDoc)
        {
            var nameSpace = uiPageDoc.ProjectItem.FileCodeModel.CodeElements.OfType<CodeElement>().SingleOrDefault(d => d.Kind == vsCMElement.vsCMElementNamespace).Name;
            var fileName = (nameSpace.Replace(".","-") + "-" + uiPageDoc.ProjectItem.Name.Replace(".cs", ".Controller.cs")).Remove("Root-");
            return App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p=> p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Pages").FileNames[0] + @"\" + fileName;
        }

        public static string GetMvcPageOfWebController(this Document webControlerDoc)
        {
            var className = webControlerDoc.ProjectItem.FileCodeModel.CodeElements.Item("Controllers").Children.OfType<CodeElement>().SingleOrDefault(p => p.Kind == vsCMElement.vsCMElementClass).Name;
            return App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Views").ProjectItems.Item("Pages").FileNames[0] + @"\" + className.Replace("Controller", ".cshtml"); ;
        }

        public static string GetMvcPageOfWebView(this Document webControlerDoc)
        {
            var prjName = App.DTE.Application.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name == "@UI").ProjectItems.Item("Pages");
            var uiPageItem = prjName.ProjectItems.GetProjectItems().Where(v => v.Name.ToUpper().Contains(".CS")).FirstOrDefault(p => p.GetMvcPageNameOfUIPage().ToUpper() == webControlerDoc.Name.ToUpper());
            return uiPageItem.FileNames[0];
        }

        public static bool IsMvcUIPage(this Document document) => document.Name.ToUpper().EndsWith(".CS") && document.FullName.ToUpper().Contains("\\@UI\\PAGES\\");

        public static bool IsMvcWebController(this Document document) => document.Name.ToUpper().EndsWith(".CONTROLLER.CS") && document.FullName.ToUpper().Contains("\\WEBSITE\\CONTROLLERS\\PAGES\\");

        public static bool IsMvcWebView(this Document document) => document.Name.ToUpper().EndsWith(".CSHTML") && document.FullName.ToUpper().Contains("\\WEBSITE\\VIEWS\\PAGES\\");

        internal static bool IsMvcFile(this Document document) => document.Name.ToUpper().EndsWithAny(".CS", ".CSHTML") && document.FullName.ToUpper().ContainsAny(@"@M#\@UI\PAGES\", @"WEBSITE\CONTROLLERS\PAGES\", @"WEBSITE\VIEWS\PAGES\");
        //---------Modules------------
        internal static bool IsModuleFile(this Document document)
        {
            var docName = document.FullName.ToUpper();

            if (docName.EndsWithAny(".CS", ".CSHTML") )
            {
                if (docName.Contains("\\MODULES\\") && !docName.Contains("\\COMPONENTS\\"))
                {
                    var compFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Modules").ProjectItems.Item("Components");
                    var fileNum = compFolder.ProjectItems.GetProjectItems().Count(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.ToUpper() == document.Name.ToUpper());
                    return fileNum == 0;
                    //return true;
                }
                else if (docName.EndsWith(".CS") && docName.Contains(@"WEBSITE\CONTROLLERS\PAGES\"))
                {
                    var nameSpaceNum = document.ProjectItem.FileCodeModel.CodeElements.OfType<CodeElement>().Count(p => p.Kind == vsCMElement.vsCMElementNamespace && p.Name == "ViewModel");
                    return nameSpaceNum > 0;
                }
                else if (docName.EndsWith(".CSHTML") && docName.Contains(@"WEBSITE\VIEWS\PAGES\"))
                {
                    var pageFoder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Pages");
                    var foundFile = pageFoder.ProjectItems.GetProjectItems().FirstOrDefault(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.Replace("Controller.cs", "cshtml").Remove("-").ToUpper() == document.Name.ToUpper());
                    var nameSpaceNum = foundFile.FileCodeModel.CodeElements.OfType<CodeElement>().Count(p => p.Kind == vsCMElement.vsCMElementNamespace && p.Name == "ViewModel");
                    return nameSpaceNum > 0;
                }
            }

            return false;
        }

        internal static bool IsModuleOfUI(this Document document)
        {
            var docName = document.FullName.ToUpper();

            if (docName.EndsWith(".CS") && docName.Contains("@UI\\MODULES\\"))
            {
                var compFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Modules").ProjectItems.Item("Components");
                var fileNum = compFolder.ProjectItems.GetProjectItems().Count(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.ToUpper() == document.Name.ToUpper());
                return fileNum == 0;
            }
            return false;
        }

        internal static bool IsModuleOfWebCtrlPage(this Document document)
        {
            var docName = document.FullName.ToUpper();
            if (docName.EndsWith(".CS") && docName.Contains(@"WEBSITE\CONTROLLERS\PAGES\"))
            {
                var nameSpaceNum = document.ProjectItem.FileCodeModel.CodeElements.OfType<CodeElement>().Count(p => p.Kind == vsCMElement.vsCMElementNamespace && p.Name == "ViewModel");
                return nameSpaceNum > 0;
            }
            return false;
        }

        internal static bool IsModuleOfWebViewPage(this Document document)
        {
            var docName = document.FullName.ToUpper();
            if (docName.EndsWith(".CSHTML") && docName.Contains(@"WEBSITE\VIEWS\PAGES\"))
            {
                var pageFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Pages");
                var foundFile = pageFolder.ProjectItems.GetProjectItems().FirstOrDefault(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.Replace("Controller.cs", "cshtml").Remove("-").ToUpper() == document.Name.ToUpper());
                var nameSpaceNum = foundFile.FileCodeModel.CodeElements.OfType<CodeElement>().Count(p => p.Kind == vsCMElement.vsCMElementNamespace && p.Name == "ViewModel");
                return nameSpaceNum > 0;
            }
            return false;
        }

        internal static bool IsModuleOfWebCtrlModule(this Document document) => document.Name.ToUpper().EndsWith(".CS") && document.FullName.ToUpper().Contains(@"WEBSITE\CONTROLLERS\MODULES\");

        internal static bool IsModuleOfWebVeiwModule(this Document document) => document.Name.ToUpper().EndsWith(".CSHTML") && document.FullName.ToUpper().Contains(@"WEBSITE\VIEWS\MODULES\");

        internal static string GetModuleOfUI(this Document document)
        {
            var webCtrlFolder = App.DTE.Application.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers");
            var moduleFile = webCtrlFolder.ProjectItems.Item("Modules").ProjectItems.OfType<ProjectItem>().FirstOrDefault(m => m.Name.ToUpper() == document.Name.ToUpper().Replace(".CS", "CONTROLLER.CS"));
            if (moduleFile == null)
            {
                var clsFiles = webCtrlFolder.ProjectItems.Item("Pages").ProjectItems.GetProjectItems().Where(v => v.Name.ToUpper().EndsWith(".CS"));
                moduleFile = clsFiles.FirstOrDefault(c => c.FileCodeModel.CodeElements.OfType<CodeElement>().Count(el => el.Kind == vsCMElement.vsCMElementNamespace && el.Name == "ViewModel"
                && el.Children.OfType<CodeElement>().Count(cl => cl.Kind == vsCMElement.vsCMElementClass && cl.Name.ToUpper() == document.Name.ToUpper().Remove(".CS")) > 0) > 0);
            }
            return moduleFile.FileNames[0];
        }

        internal static string GetModuleOfWebCtrlPage(this Document document) => GetMvcPageOfWebController(document);

        internal static string GetModuleOfWebCtrlModule(this Document document) => SolutionDir + @"\Website\Views\Modules\" + document.Name.Replace("Controller.cs", ".cshtml");

        internal static string GetModuleOfWebViewPage(this Document document)
        {
            var pageFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Pages");
            var foundFile = pageFolder.ProjectItems.GetProjectItems().FirstOrDefault(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.Replace("Controller.cs", "cshtml").Remove("-").ToUpper() == document.Name.ToUpper());
            var nameSpaceNum = foundFile.FileCodeModel.CodeElements.OfType<CodeElement>().FirstOrDefault(p => p.Kind == vsCMElement.vsCMElementNamespace && p.Name == "ViewModel");
            var className = nameSpaceNum.Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            var moduleFile = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "@UI").ProjectItems.Item("Modules").ProjectItems.GetProjectItems().FirstOrDefault(m => m.Name.ToUpper().Remove(".CS") == className.ToUpper());
            return moduleFile.FileNames[0];
        }

        internal static string GetModuleOfWebViewModule(this Document document)
        {
            var moduleFiles = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "@UI").ProjectItems.Item("Modules").ProjectItems.GetProjectItems();
            var moduleFile = moduleFiles.FirstOrDefault(m => m.Name.ToUpper().Remove(".CS") == document.Name.ToUpper().Remove(".CSHTML"));
            return moduleFile.FileNames[0];
        }

        //--------Components----------
        internal static bool IsComponentFile(this Document document)
        {
            var docName = document.FullName.ToUpper();
            if (docName.EndsWithAny(".CS", ".CSHTML") && docName.Contains("WEBSITE\\") && docName.Contains("\\COMPONENTS\\"))
            {
                return true;
            }
            else if (docName.EndsWith(".CS") && docName.Contains("@UI\\MODULES\\"))
            {
                var compFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Modules").ProjectItems.Item("Components");
                var fileNum = compFolder.ProjectItems.GetProjectItems().Count(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.ToUpper() == document.Name.ToUpper());
                return fileNum > 0;
            }
            return false;
        }

        internal static bool IsComponentOfUI(this Document document)
        {
            var docName = document.FullName.ToUpper();
            if (docName.EndsWith(".CS") && docName.Contains("@UI\\MODULES\\"))
            {
                var compFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Modules").ProjectItems.Item("Components");
                var fileNum = compFolder.ProjectItems.GetProjectItems().Count(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.ToUpper() == document.Name.ToUpper());
                return fileNum > 0;
            }
            return false;
        }

        internal static bool IsComponentOfWebCtrl(this Document document) => document.Name.ToUpper().EndsWith(".CS") && document.FullName.ToUpper().Contains(@"WEBSITE\CONTROLLERS\MODULES\COMPONENTS\");

        internal static bool IsComponentOfWebView(this Document document) => document.Name.ToUpper().EndsWith(".CSHTML") && document.FullName.ToUpper().Contains(@"WEBSITE\VIEWS\MODULES\COMPONENTS\");

        internal static string GetComponentFromUI(this Document document)
        {
            return SolutionDir + @"\Website\Controllers\Modules\Components\" + document.Name;
        }

        internal static string GetComponentFromCtrl(this Document document)
        {
            return SolutionDir + @"\Website\Views\Modules\Components\" + document.Name.Remove(".cs").Remove(".CS") + "\\Default.cshtml";
        }

        internal static string GetComponentFromView(this Document document)
        {
            var docName = App.DTE.ActiveDocument.FullName.ToUpper().Replace("\\DEFAULT.CSHTML", ".CS");
            var ix = docName.LastIndexOf("\\");
            var itemName = docName.Substring(++ix);
            var projItem = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "@UI");
            var fileItem = projItem.ProjectItems.Item("Modules").ProjectItems.GetProjectItems().FirstOrDefault(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.ToUpper() == itemName);
            return fileItem.FileNames[0];
        }

        //-------Entities----------------
        internal static bool IsEntityFile(this Document document) => document.Name.ToUpper().EndsWith(".CS") && document.FullName.ToUpper().ContainsAny("@MODEL\\", "DOMAIN\\-LOGIC\\", "DOMAIN\\ENTITIES\\");

        internal static bool IsEntityOfModel(this Document document)
        {
            if (document.Name.ToUpper().EndsWith(".CS") && document.ProjectItem.ContainingProject.Name.Equals("@Model"))
            {
                var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
                return domainProj.ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper());
            }
            return false;
        }

        internal static bool IsEntityOfDomainEntity(this Document document)
        {
            if(document.Name.ToUpper().EndsWith(".CS") && document.FullName.ToUpper().Contains(@"DOMAIN\ENTITIES\"))
            {
                var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
                if (domainProj.ProjectItems.Item("-Logic").ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper()))
                    return true;
                var modelProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "@MODEL");
                if (modelProj.ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper()))
                    return true;
            }
            return false;
        }

        internal static bool IsEntityOfDomainLogic(this Document document)
        {
            if (document.Name.ToUpper().EndsWith(".CS") && document.FullName.ToUpper().Contains(@"DOMAIN\-LOGIC\"))
            {
                var modelProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "@MODEL");
                if (modelProj.ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper()))
                    return true;
                var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
                if (domainProj.ProjectItems.Item("-Entities").ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper()))
                    return true;
                
            }
            return false;
        }

        internal static string GetEntityFromModel(this Document document)
        {
            var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
            var entityFile = domainProj.ProjectItems.Item("Entities").ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            if (entityFile == null)
            {
                entityFile = domainProj.ProjectItems.Item("-Logic").ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            }
            return entityFile.FileNames[0];
        }

        internal static string GetEntityFromDomainEntity(this Document document)
        {
            var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
            var entityFile = domainProj.ProjectItems.Item("-Logic").ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            if (entityFile == null)
            {
                entityFile = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "@MODEL").ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            }
            return entityFile.FileNames[0];
        }

        internal static string GetEntityFromDomainLogic(this Document document)
        {
            var entityFile = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "@MODEL").ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            if (entityFile == null)
            {
                var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
                entityFile = domainProj.ProjectItems.Item("Entities").ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            }
            return entityFile.FileNames[0];
        }
    }
}
