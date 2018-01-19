using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using MSharp.F7;

namespace MSharp.F7.ToggleHandler
{
    public enum PageOrModule { None, Page, Module }
    public static class Toolbox
    {
        public static PageOrModule State = PageOrModule.None;
        public static bool ContainsAny(this string str, params string[] subStrings)
        {
            foreach (var subString in subStrings)
                if (str.Contains(subString)) return true;

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
            var nameSpaceName = "";
            var nameSpace = uiPageItem.FileCodeModel.CodeElements.OfType<CodeElement>().SingleOrDefault(d => d.Kind == vsCMElement.vsCMElementNamespace);
            if (nameSpace != null)
            {
                nameSpaceName = nameSpace.Name;
            }

            return nameSpaceName.Remove("Root").Remove(".").Remove("_") + uiPageItem.Name + "html";
        }

        public static string GetMvcControllerOfUIPage(this Document document)
        {
            var mvcPagePath = "";
            var curNameSpaceName = "";
            var curClassName = "";

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
            if (cntrlFileName != null)
            {
                mvcPagePath = cntrlFileName.FileNames[0];
            }
            return mvcPagePath;
        }

        public static string GetMvcPageOfWebController(this Document webControlerDoc)
        {
            var mvcPagePath = "";
            var className = webControlerDoc.ProjectItem.FileCodeModel.CodeElements.Item("Controllers").Children.OfType<CodeElement>().SingleOrDefault(p => p.Kind == vsCMElement.vsCMElementClass).Name;
            var mvcPageFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Views").ProjectItems.Item("Pages");
            if (mvcPageFolder != null)
            {
                var mvcPageFile = mvcPageFolder.ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == className.Replace("Controller", ".cshtml").ToUpper());
                if (mvcPageFile != null)
                {
                    mvcPagePath = mvcPageFile.FileNames[0];
                }
            }
            return mvcPagePath;
        }

        public static string GetMvcPageOfWebView(this Document webControlerDoc)
        {
            var mvcPagePath = "";
            var prjName = App.DTE.Application.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name == "@UI").ProjectItems.Item("Pages");
            var uiPageItem = prjName.ProjectItems.GetProjectItems().Where(v => v.Name.ToUpper().Contains(".CS")).FirstOrDefault(p => p.GetMvcPageNameOfUIPage().ToUpper() == webControlerDoc.Name.ToUpper());
            if (uiPageItem != null)
            {
                mvcPagePath = uiPageItem.FileNames[0];
            }
            return mvcPagePath;
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
            var moduleFilePath = "";
            var moduleName = document.ProjectItem.FileCodeModel.CodeElements.Item("Modules").Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            var webCtrlFolder = App.DTE.Application.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers");
            var moduleFile = webCtrlFolder.ProjectItems.Item("Modules").ProjectItems.OfType<ProjectItem>().FirstOrDefault(m => m.Name.ToUpper().EndsWith(".CS") && m.FileCodeModel.CodeElements.OfType<CodeElement>()
            .Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("VIEWMODEL") && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper().Equals(moduleName.ToUpper()))));
            if (moduleFile == null)
            {
                moduleFile = webCtrlFolder.ProjectItems.Item("Pages").ProjectItems.OfType<ProjectItem>().FirstOrDefault(m => m.Name.ToUpper().EndsWith(".CS") && m.FileCodeModel.CodeElements.OfType<CodeElement>()
                .Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("VIEWMODEL") && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper().Equals(moduleName.ToUpper()))));
            }
            if (moduleFile != null)
            {
                moduleFilePath = moduleFile.FileNames[0];
            }
            return moduleFilePath;
        }

        internal static string GetModuleOfWebCtrlPage(this Document document) => GetMvcPageOfWebController(document);

        internal static string GetModuleOfWebCtrlModule(this Document document)
        {
            var moduleFilePath = "";
            var moduleName = document.ProjectItem.FileCodeModel.CodeElements.OfType<CodeElement>().FirstOrDefault(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("VIEWMODEL")).Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            var moduleViewFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Views").ProjectItems.Item("Modules");
            var moduleFile = moduleViewFolder.ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == (moduleName + ".cshtml").ToUpper());
            if (moduleFile != null)
            {
                moduleFilePath = moduleFile.FileNames[0];
            }
            return moduleFilePath;
        }

        internal static string GetModuleOfWebViewPage(this Document document)
        {
            var moduleFilePath = "";
            var ctrlName = document.Name.ToUpper().Replace(".CSHTML", "CONTROLLER");
            var pageFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Pages");
            var moduleCtrlFile = pageFolder.ProjectItems.GetProjectItems().FirstOrDefault(i => i.Name.ToUpper().EndsWith(".CS") && i.FileCodeModel.CodeElements.OfType<CodeElement>().Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper() == "CONTROLLERS" && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper() == ctrlName)));
            var moduleName = moduleCtrlFile.FileCodeModel.CodeElements.OfType<CodeElement>().FirstOrDefault(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("VIEWMODEL")).Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            var uiModuleFiles = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "@UI").ProjectItems.Item("Modules").ProjectItems.GetProjectItems().Where(f => f.Name.ToUpper().EndsWith(".CS"));
            var uiModuleFile = uiModuleFiles.FirstOrDefault(f => f.FileCodeModel.CodeElements.OfType<CodeElement>().Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("MODULES") && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper().Equals(moduleName.ToUpper()))));
            if (uiModuleFile != null)
            {
                moduleFilePath = uiModuleFile.FileNames[0];
            }
            return moduleFilePath;

        }

        internal static string GetModuleOfWebViewModule(this Document document)
        {
            var moduleFilePath = "";
            var moduleFiles = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "@UI").ProjectItems.Item("Modules").ProjectItems.GetProjectItems();
            var moduleFile = moduleFiles.FirstOrDefault(m => m.Name.ToUpper().Remove(".CS") == document.Name.ToUpper().Remove(".CSHTML"));
            if (moduleFile != null)
            {
                moduleFilePath = moduleFile.FileNames[0];
            }
            return moduleFilePath;
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
                return compFolder.ProjectItems.GetProjectItems().Any(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.ToUpper() == document.Name.ToUpper());
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
            var compoFilePath = "";
            var compFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Modules").ProjectItems.OfType<ProjectItem>().FirstOrDefault(c => c.Name.ToUpper() == "COMPONENTS");
            if (compFolder != null)
            {
                var compFile = compFolder.ProjectItems.GetProjectItems().FirstOrDefault(c => c.Name.ToUpper() == document.Name.ToUpper());
                if (compFile != null)
                {
                    compoFilePath = compFile.FileNames[0];
                }
            }
            return compoFilePath;
        }

        internal static string GetComponentFromCtrl(this Document document)
        {
            var compoFilePath = "";
            var compFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Views").ProjectItems.Item("Modules").ProjectItems.OfType<ProjectItem>().FirstOrDefault(c => c.Name.ToUpper() == "COMPONENTS");
            if (compFolder != null)
            {
                var compFolderFile = compFolder.ProjectItems.GetProjectItems().FirstOrDefault(c => c.Name.ToUpper() == document.Name.ToUpper().Remove(".CS"));
                if (compFolderFile != null)
                {
                    var compoFile = compFolderFile.ProjectItems.GetProjectItems().FirstOrDefault(c => c.Name.ToUpper() == "DEFAULT.CSHTML");
                    if (compoFile != null)
                    {
                        compoFilePath = compoFile.FileNames[0];
                    }
                }
            }
            return compoFilePath;
        }

        internal static string GetComponentFromView(this Document document)
        {
            var compoFilePath = "";
            var docName = App.DTE.ActiveDocument.FullName.ToUpper().Replace("\\DEFAULT.CSHTML", ".CS");
            var ix = docName.LastIndexOf("\\");
            var itemName = docName.Substring(++ix);
            var projItem = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "@UI");
            var fileItem = projItem.ProjectItems.Item("Modules").ProjectItems.GetProjectItems().FirstOrDefault(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.ToUpper() == itemName);
            if (fileItem != null)
            {
                compoFilePath = fileItem.FileNames[0];
            }
            return compoFilePath;
        }

        // -------Entities----------------
        internal static bool IsEntityFile(this Document document)
        {
            if (document.Name.ToUpper().EndsWith(".CS"))
            {
                if (document.FullName.ToUpper().Contains("@MODEL\\"))
                {
                    return true;
                }

                if (document.FullName.ToUpper().Contains("DOMAIN\\") && document.FullName.ToUpper().Contains("LOGIC"))
                {
                    return true;
                }

                if (document.FullName.ToUpper().Contains("DOMAIN\\") && document.FullName.ToUpper().Contains("ENTITIES"))
                {
                    return true;
                }
            }

            return false;
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
            if (document.Name.ToUpper().EndsWith(".CS") && document.FullName.ToUpper().Contains(@"DOMAIN\") && document.FullName.ToUpper().Contains("ENTITIES"))
            {
                var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
                var logicFolder = domainProj.ProjectItems.GetProjectItems().FirstOrDefault(f => !f.Name.Contains(".") && f.Name.ToUpper().Contains("LOGIC"));
                if (logicFolder.ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper()))
                    return true;
                var modelProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "@MODEL");
                if (modelProj.ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper()))
                    return true;
            }

            return false;
        }

        internal static bool IsEntityOfDomainLogic(this Document document)
        {
            if (document.Name.ToUpper().EndsWith(".CS") && document.FullName.ToUpper().Contains(@"DOMAIN\") && document.FullName.ToUpper().Contains("LOGIC"))
            {
                var modelProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "@MODEL");
                if (modelProj.ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper()))
                    return true;
                var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
                var entityFolder = domainProj.ProjectItems.GetProjectItems().FirstOrDefault(f => !f.Name.Contains(".") && f.Name.ToUpper().Contains("ENTITIES"));
                if (entityFolder.ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper()))
                    return true;
            }

            return false;
        }

        internal static string GetEntityFromModel(this Document document)
        {
            var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
            var entityFolder = domainProj.ProjectItems.GetProjectItems().FirstOrDefault(f => !f.Name.Contains(".") && f.Name.ToUpper().Contains("ENTITIES"));
            var entityFile = entityFolder.ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            if (entityFile == null)
            {
                var logicFolder = domainProj.ProjectItems.GetProjectItems().FirstOrDefault(f => !f.Name.Contains(".") && f.Name.ToUpper().Contains("LOGIC"));
                entityFile = logicFolder.ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            }

            return entityFile.FileNames[0];
        }

        internal static string GetEntityFromDomainEntity(this Document document)
        {
            var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
            var logicFolder = domainProj.ProjectItems.GetProjectItems().FirstOrDefault(f => !f.Name.Contains(".") && f.Name.ToUpper().Contains("LOGIC"));
            var entityFile = logicFolder.ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
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
                var entityFolder = domainProj.ProjectItems.GetProjectItems().FirstOrDefault(f => !f.Name.Contains(".") && f.Name.ToUpper().Contains("ENTITIES"));
                entityFile = entityFolder.ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            }

            return entityFile.FileNames[0];
        }

        //------------Next File Functions--------------------
        public static bool NextEntityFilePath(Document curDocument,ref string RelatedFilePath)
        {
            if (curDocument.IsEntityOfModel())
            {
                RelatedFilePath = curDocument.GetEntityFromModel();
                return true;
            }

            if (curDocument.IsEntityOfDomainEntity())
            {
                RelatedFilePath = curDocument.GetEntityFromDomainEntity();
                return true;
            }

            if (curDocument.IsEntityOfDomainLogic())
            {
                RelatedFilePath = curDocument.GetEntityFromDomainLogic();
                return true;
            }

            return false;
        }

        public static bool NextComponentFilePath(Document curDocument, ref string RelatedFilePath)
        {
            RelatedFilePath = "";
            if (curDocument.IsComponentOfUI())
            {
                RelatedFilePath = curDocument.GetComponentFromUI();
            }
            else if (curDocument.IsComponentOfWebCtrl())
            {
                RelatedFilePath = curDocument.GetComponentFromCtrl();
            }
            else if (curDocument.IsComponentOfWebView())
            {
                RelatedFilePath = curDocument.GetComponentFromView();
            }

            if (RelatedFilePath.Length > 0)
            {
                return true;
            }
            else
                return false;
        }

        public static bool NextModuleFilePath(Document curDocument,ref string RelatedFilePath,ref PageOrModule State)
        {
            RelatedFilePath = "";
            if (curDocument.IsModuleOfUI())
            {
                RelatedFilePath = curDocument.GetModuleOfUI();
                State = PageOrModule.Module;
            }
            else if (curDocument.IsModuleOfWebCtrlModule())
            {
                RelatedFilePath = curDocument.GetModuleOfWebCtrlModule();
            }
            else if (curDocument.IsModuleOfWebCtrlPage())
            {
                RelatedFilePath = curDocument.GetModuleOfWebCtrlPage();
            }
            else if (curDocument.IsModuleOfWebVeiwModule())
            {
                RelatedFilePath = curDocument.GetModuleOfWebViewModule();
            }
            else if (curDocument.IsModuleOfWebViewPage())
            {
                RelatedFilePath = curDocument.GetModuleOfWebViewPage();
            }

            if (RelatedFilePath.Length > 0)
            {
                return true;
            }
            else
                return false;
        }

        public static bool NextMvcFilePath(Document curDocument, ref string RelatedFilePath, ref PageOrModule State)
        {
            RelatedFilePath = "";
            if (curDocument.IsMvcUIPage())
            {
                RelatedFilePath = curDocument.GetMvcControllerOfUIPage();
                State = PageOrModule.Page;
            }
            else if (curDocument.IsMvcWebController())
            {
                RelatedFilePath = curDocument.GetMvcPageOfWebController();
            }
            else if (curDocument.IsMvcWebView())
            {
                RelatedFilePath = curDocument.GetMvcPageOfWebView();
            }
            if (RelatedFilePath.Length > 0)
            {
                return true;
            }
            else
                return false;
        }
    }
}