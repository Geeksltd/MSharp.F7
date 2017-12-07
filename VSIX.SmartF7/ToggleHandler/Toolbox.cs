using EnvDTE;
using Geeks.GeeksProductivityTools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Geeks.SmartF7.ToggleHandler
{
    public static class Toolbox
    {
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
        // ----mvc pages--------------
        public static string GetMvcPageNameOfUIPage(this ProjectItem uiPageItem)
        {
            string nameSpaceName = "";
            var nameSpace = uiPageItem.FileCodeModel.CodeElements.OfType<CodeElement>().SingleOrDefault(d => d.Kind == vsCMElement.vsCMElementNamespace);
            if (nameSpace != null)
            {
                nameSpaceName = nameSpace.Name;
            }
            return nameSpaceName.Remove("Root").Remove(".").Remove("_") + uiPageItem.Name + "html";
        }

        public static string GetMvcControllerOfUIPage(this Document document)
        {
            string curNameSpaceName = "";
            string curClassName = "";

            var curNameSpace = document.ProjectItem.FileCodeModel.CodeElements.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementNamespace);
            if (curNameSpace != null)
            {
                curNameSpaceName = curNameSpace.Name;
                curClassName = curNameSpace.Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            }
            else
            {
                curClassName = document.ProjectItem.FileCodeModel.CodeElements.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            }
            var controllerClassName = (curNameSpaceName.Remove("Root") + curClassName).Remove(".").Remove("_").Replace("Page", "Controller").ToUpper();
            var controlerPageFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Pages");
            var cntrlFileName = controlerPageFolder.ProjectItems.GetProjectItems().FirstOrDefault(i => i.Name.ToUpper().EndsWith(".CS") && i.FileCodeModel.CodeElements.OfType<CodeElement>().Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name == "Controllers" && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper() == controllerClassName)));
            return cntrlFileName.FileNames[0];
        }

        public static string GetMvcPageOfWebController(this Document webControlerDoc)
        {
            var className = webControlerDoc.ProjectItem.FileCodeModel.CodeElements.Item("Controllers").Children.OfType<CodeElement>().SingleOrDefault(p => p.Kind == vsCMElement.vsCMElementClass).Name;
            return App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Views").ProjectItems.Item("Pages").FileNames[0] + className.Replace("Controller", ".cshtml"); ;
        }

        public static string GetMvcPageOfWebView(this Document webControlerDoc)
        {
            var prjName = App.DTE.Application.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name == "@UI").ProjectItems.Item("Pages");
            var uiPageItem = prjName.ProjectItems.GetProjectItems().Where(v => v.Name.ToUpper().Contains(".CS")).FirstOrDefault(p => p.GetMvcPageNameOfUIPage().ToUpper() == webControlerDoc.Name.ToUpper());
            return uiPageItem.FileNames[0];
        }

        public static bool IsMvcUIPage(this Document document) => document.Name.ToUpper().EndsWith(".CS") && document.FullName.ToUpper().Contains("\\@UI\\PAGES\\");

        public static bool IsMvcWebController(this Document document)
        {
            return document.Name.ToUpper().EndsWith("CONTROLLER.CS") && document.FullName.ToUpper().Contains("\\WEBSITE\\CONTROLLERS\\PAGES\\");
        }

        public static bool IsMvcWebView(this Document document)
        {
            return document.Name.ToUpper().EndsWith(".CSHTML") && document.FullName.ToUpper().Contains("\\WEBSITE\\VIEWS\\PAGES\\");
        }

        internal static bool IsMvcFile(this Document document)
        {
            return document.Name.ToUpper().EndsWithAny(".CS", ".CSHTML") && document.FullName.ToUpper().ContainsAny(@"@M#\@UI\PAGES\", @"WEBSITE\CONTROLLERS\PAGES\", @"WEBSITE\VIEWS\PAGES\");
        }
        // ---------Modules------------
        internal static bool IsModuleFile(this Document document)
        {
            var docName = document.FullName.ToUpper();

            if (docName.EndsWithAny(".CS", ".CSHTML"))
            {
                if (docName.Contains("\\MODULES\\") && !docName.Contains("\\COMPONENTS\\"))
                {
                    var moduleFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Modules");
                    var compFolder = moduleFolder.ProjectItems.OfType<ProjectItem>().FirstOrDefault(c => c.Name == "Components");
                    if (compFolder != null)
                    {
                        return !compFolder.ProjectItems.GetProjectItems().Any(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.ToUpper() == document.Name.ToUpper());
                    }
                    else
                        return true;
                }
                else if (docName.EndsWith(".CS") && docName.Contains(@"WEBSITE\CONTROLLERS\PAGES\"))
                {
                    return document.ProjectItem.FileCodeModel.CodeElements.OfType<CodeElement>().Any(p => p.Kind == vsCMElement.vsCMElementNamespace && p.Name == "ViewModel");
                }
                else if (docName.EndsWith(".CSHTML") && docName.Contains(@"WEBSITE\VIEWS\PAGES\"))
                {
                    var pageFoder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Pages");
                    var foundFile = pageFoder.ProjectItems.GetProjectItems().FirstOrDefault(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.Replace("Controller.cs", "cshtml").Remove("-").Remove(".").ToUpper() == document.Name.Remove(".").ToUpper());
                    return foundFile.FileCodeModel.CodeElements.OfType<CodeElement>().Any(p => p.Kind == vsCMElement.vsCMElementNamespace && p.Name == "ViewModel");
                }
            }

            return false;
        }

        internal static bool IsModuleOfUI(this Document document)
        {
            var docName = document.FullName.ToUpper();

            if (docName.EndsWith(".CS") && docName.Contains("@UI\\MODULES\\"))
            {
                var moduleFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Modules");
                var compFolder = moduleFolder.ProjectItems.OfType<ProjectItem>().FirstOrDefault(c => c.Name == "Components");
                if (compFolder != null)
                {
                    return !compFolder.ProjectItems.GetProjectItems().Any(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.ToUpper() == document.Name.ToUpper());
                }
                else
                    return true;
            }

            return false;
        }

        internal static bool IsModuleOfWebCtrlPage(this Document document)
        {
            var docName = document.FullName.ToUpper();
            if (docName.EndsWith(".CS") && docName.Contains(@"WEBSITE\CONTROLLERS\PAGES\"))
            {
                return document.ProjectItem.FileCodeModel.CodeElements.OfType<CodeElement>().Any(p => p.Kind == vsCMElement.vsCMElementNamespace && p.Name == "ViewModel");
            }

            return false;
        }

        internal static bool IsModuleOfWebViewPage(this Document document)
        {
            var docName = document.FullName.ToUpper();
            if (docName.EndsWith(".CSHTML") && docName.Contains(@"WEBSITE\VIEWS\PAGES\"))
            {
                var pageFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Pages");
                var foundFile = pageFolder.ProjectItems.GetProjectItems().FirstOrDefault(i => i.Name.ToUpper().EndsWith(".CS") && i.FileCodeModel.CodeElements.OfType<CodeElement>().Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper() == "CONTROLLERS" && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper() == document.Name.ToUpper().Replace(".CSHTML", "CONTROLLER"))));
                return foundFile.FileCodeModel.CodeElements.OfType<CodeElement>().Any(p => p.Kind == vsCMElement.vsCMElementNamespace && p.Name == "ViewModel");
            }

            return false;
        }

        internal static bool IsModuleOfWebCtrlModule(this Document document)
        {
            return document.Name.ToUpper().EndsWith(".CS") && document.FullName.ToUpper().Contains(@"WEBSITE\CONTROLLERS\MODULES\");
        }

        internal static bool IsModuleOfWebVeiwModule(this Document document)
        {
            return document.Name.ToUpper().EndsWith(".CSHTML") && document.FullName.ToUpper().Contains(@"WEBSITE\VIEWS\MODULES\");
        }

        internal static string GetModuleOfUI(this Document document)
        {
            var moduleName = document.ProjectItem.FileCodeModel.CodeElements.Item("Modules").Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            var webCtrlFolder = App.DTE.Application.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers");
            var moduleFile = webCtrlFolder.ProjectItems.Item("Modules").ProjectItems.OfType<ProjectItem>().FirstOrDefault(m => m.Name.ToUpper().EndsWith(".CS") && m.FileCodeModel.CodeElements.OfType<CodeElement>()
            .Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("VIEWMODEL") && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper().Equals(moduleName.ToUpper()))));
            if (moduleFile == null)
            {
                moduleFile = webCtrlFolder.ProjectItems.Item("Pages").ProjectItems.OfType<ProjectItem>().FirstOrDefault(m => m.Name.ToUpper().EndsWith(".CS") && m.FileCodeModel.CodeElements.OfType<CodeElement>()
                .Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("VIEWMODEL") && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper().Equals(moduleName.ToUpper()))));
            }

            return moduleFile.FileNames[0];
        }

        internal static string GetModuleOfWebCtrlPage(this Document document) => GetMvcPageOfWebController(document);

        internal static string GetModuleOfWebCtrlModule(this Document document)
        {
            var SolutionDir = App.DTE.Solution.FullName.Substring(0, App.DTE.Solution.FullName.LastIndexOf(@"\"));
            var moduleName = document.ProjectItem.FileCodeModel.CodeElements.OfType<CodeElement>().FirstOrDefault(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("VIEWMODEL")).Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            return SolutionDir + @"\Website\Views\Modules\" + moduleName + ".cshtml";
        }

        internal static string GetModuleOfWebViewPage(this Document document)
        {
            var ctrlName = document.Name.ToUpper().Replace(".CSHTML", "CONTROLLER");
            var pageFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Pages");
            var moduleCtrlFile = pageFolder.ProjectItems.GetProjectItems().FirstOrDefault(i => i.Name.ToUpper().EndsWith(".CS") && i.FileCodeModel.CodeElements.OfType<CodeElement>().Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper() == "CONTROLLERS" && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper() == ctrlName)));
            var moduleName = moduleCtrlFile.FileCodeModel.CodeElements.OfType<CodeElement>().FirstOrDefault(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("VIEWMODEL")).Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            var uiModuleFiles = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "@UI").ProjectItems.Item("Modules").ProjectItems.GetProjectItems().Where(f => f.Name.ToUpper().EndsWith(".CS"));
            var uiModuleFile = uiModuleFiles.FirstOrDefault(f => f.FileCodeModel.CodeElements.OfType<CodeElement>().Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("MODULES") && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper().Equals(moduleName.ToUpper()))));

            return uiModuleFile.FileNames[0];
        }

        internal static string GetModuleOfWebViewModule(this Document document)
        {
            var moduleFiles = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "@UI").ProjectItems.Item("Modules").ProjectItems.GetProjectItems();
            var moduleFile = moduleFiles.FirstOrDefault(m => m.Name.ToUpper().Remove(".CS") == document.Name.ToUpper().Remove(".CSHTML"));
            return moduleFile.FileNames[0];
        }

        // --------Components----------
        internal static bool IsComponentFile(this Document document)
        {
            var docName = document.FullName.ToUpper();
            if (docName.EndsWithAny(".CS", ".CSHTML") && docName.Contains("WEBSITE\\") && docName.Contains("\\COMPONENTS\\"))
            {
                return true;
            }
            else if (docName.EndsWith(".CS") && docName.Contains("@UI\\MODULES\\"))
            {
                var compFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Modules").ProjectItems.OfType<ProjectItem>().FirstOrDefault(c => c.Name == "Components");
                if (compFolder != null)
                {
                    return compFolder.ProjectItems.GetProjectItems().Any(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.ToUpper() == document.Name.ToUpper());
                }
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

        internal static bool IsComponentOfWebCtrl(this Document document)
        {
            return document.Name.ToUpper().EndsWith(".CS") && document.FullName.ToUpper().Contains(@"WEBSITE\CONTROLLERS\MODULES\COMPONENTS\");
        }

        internal static bool IsComponentOfWebView(this Document document)
        {
            return document.Name.ToUpper().EndsWith(".CSHTML") && document.FullName.ToUpper().Contains(@"WEBSITE\VIEWS\MODULES\COMPONENTS\");
        }

        internal static string GetComponentFromUI(this Document document)
        {
            var SolutionDir = App.DTE.Solution.FullName.Substring(0, App.DTE.Solution.FullName.LastIndexOf(@"\"));
            return SolutionDir + @"\Website\Controllers\Modules\Components\" + document.Name;
        }

        internal static string GetComponentFromCtrl(this Document document)
        {
            var SolutionDir = App.DTE.Solution.FullName.Substring(0, App.DTE.Solution.FullName.LastIndexOf(@"\"));
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

        // -------Entities----------------
        internal static bool IsEntityFile(this Document document)
        {
            return document.Name.ToUpper().EndsWith(".CS") && document.FullName.ToUpper().ContainsAny("@MODEL\\", "DOMAIN\\LOGIC\\", "DOMAIN\\[GEN-ENTITIES]\\");
        }

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
            if (document.Name.ToUpper().EndsWith(".CS") && document.FullName.ToUpper().Contains(@"DOMAIN\[GEN-ENTITIES]\"))
            {
                var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
                if (domainProj.ProjectItems.Item("Logic").ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper()))
                    return true;
                var modelProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "@MODEL");
                if (modelProj.ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper()))
                    return true;
            }

            return false;
        }

        internal static bool IsEntityOfDomainLogic(this Document document)
        {
            if (document.Name.ToUpper().EndsWith(".CS") && document.FullName.ToUpper().Contains(@"DOMAIN\LOGIC\"))
            {
                var modelProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "@MODEL");
                if (modelProj.ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper()))
                    return true;
                var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
                if (domainProj.ProjectItems.Item("[GEN-Entities]").ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper()))
                    return true;
            }

            return false;
        }

        internal static string GetEntityFromModel(this Document document)
        {
            var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
            var entityFile = domainProj.ProjectItems.Item("[GEN-Entities]").ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            if (entityFile == null)
            {
                entityFile = domainProj.ProjectItems.Item("Logic").ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            }

            return entityFile.FileNames[0];
        }

        internal static string GetEntityFromDomainEntity(this Document document)
        {
            var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
            var entityFile = domainProj.ProjectItems.Item("Logic").ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
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
                entityFile = domainProj.ProjectItems.Item("[GEN-Entities]").ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            }

            return entityFile.FileNames[0];
        }
    }
}