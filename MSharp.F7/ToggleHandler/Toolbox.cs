using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using MSharp.F7;

namespace MSharp.F7.ToggleHandler
{
    public enum PageOrModule { None, Page, Module }
    public static class Toolbox
    {
        public static PageOrModule State = PageOrModule.None;

        public static EnvDTE.Document ActiveDoc(IWpfTextView textView)
        {
            var filePath = GetPath(textView);
            EnvDTE.Document doc = null;
            if (filePath != null)
            {
                doc = App.DTE.Documents.OfType<EnvDTE.Document>().FirstOrDefault(d => d.FullName.ToUpper() == filePath.ToUpper());
            }
            return doc;
        }

        public static string GetPath(IWpfTextView textView)
        {
            textView.TextBuffer.Properties.TryGetProperty(typeof(IVsTextBuffer), out IVsTextBuffer bufferAdapter);
            var persistFileFormat = bufferAdapter as IPersistFileFormat;

            if (persistFileFormat == null)
            {
                return null;
            }
            persistFileFormat.GetCurFile(out string filePath, out _);
            return filePath;
        }
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

        public static string GetMvcControllerOfUIPage(this ProjectItem document)
        {
            var mvcPagePath = "";
            var curNameSpaceName = "";
            var curClassName = "";

            var curNameSpace = document.FileCodeModel.CodeElements.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementNamespace);
            if (curNameSpace != null)
            {
                curNameSpaceName = curNameSpace.Name;
                curClassName = curNameSpace.Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            }
            else
            {
                curClassName = document.FileCodeModel.CodeElements.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
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

        public static void GetSiblingMvcOfUIPage(this ProjectItem document, ref string RelatedFilePath2, ref string RelatedFilePath3)
        {
            RelatedFilePath2 = "";
            var curNameSpaceName = "";
            var curClassName = "";

            var curNameSpace = document.FileCodeModel.CodeElements.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementNamespace);
            if (curNameSpace != null)
            {
                curNameSpaceName = curNameSpace.Name;
                curClassName = curNameSpace.Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            }
            else
            {
                curClassName = document.FileCodeModel.CodeElements.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            }

            var controllerClassName = (curNameSpaceName.Remove("Root") + curClassName).Remove(".").Remove("_").Replace("Page", "Controller").ToUpper();
            var controlerPageFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Pages");
            var cntrlFileName = controlerPageFolder.ProjectItems.GetProjectItems().FirstOrDefault(i => i.Name.ToUpper().EndsWith(".CS") && i.FileCodeModel.CodeElements.OfType<CodeElement>().Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name == "Controllers" && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper() == controllerClassName)));
            if (cntrlFileName != null)
            {
                RelatedFilePath2 = cntrlFileName.FileNames[0];
                RelatedFilePath3 = GetMvcPageOfWebController(cntrlFileName);
            }
        }

        public static string GetMvcPageOfWebController(this ProjectItem webControlerDoc)
        {
            var mvcPagePath = "";
            var className = webControlerDoc.FileCodeModel.CodeElements.Item("Controllers").Children.OfType<CodeElement>().SingleOrDefault(p => p.Kind == vsCMElement.vsCMElementClass).Name;
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

        public static void GetSiblingMvcOfWebController(this ProjectItem document, ref string RelatedFilePath1, ref string RelatedFilePath3)
        {
            RelatedFilePath3 = "";
            var className = document.FileCodeModel.CodeElements.Item("Controllers").Children.OfType<CodeElement>().SingleOrDefault(p => p.Kind == vsCMElement.vsCMElementClass).Name;
            var mvcPageFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Views").ProjectItems.Item("Pages");
            if (mvcPageFolder != null)
            {
                var mvcPageFile = mvcPageFolder.ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == className.Replace("Controller", ".cshtml").ToUpper());
                if (mvcPageFile != null)
                {
                    RelatedFilePath3 = mvcPageFile.FileNames[0];
                    RelatedFilePath1 = GetMvcPageOfWebView(mvcPageFile);
                }
            }

        }

        public static string GetMvcPageOfWebView(this ProjectItem document)
        {
            var mvcPagePath = "";
            var prjName = App.DTE.Application.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name == "#UI").ProjectItems.Item("Pages");
            var uiPageItem = prjName.ProjectItems.GetProjectItems().Where(v => v.Name.ToUpper().Contains(".CS")).FirstOrDefault(p => p.GetMvcPageNameOfUIPage().ToUpper() == document.Name.ToUpper());
            if (uiPageItem != null)
            {
                mvcPagePath = uiPageItem.FileNames[0];
            }
            return mvcPagePath;
        }

        public static void GetSiblingMvcOfWebView(this ProjectItem document, ref string RelatedFilePath1, ref string RelatedFilePath2)
        {
            RelatedFilePath1 = "";
            var prjName = App.DTE.Application.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name == "#UI").ProjectItems.Item("Pages");
            var uiPageItem = prjName.ProjectItems.GetProjectItems().Where(v => v.Name.ToUpper().Contains(".CS")).FirstOrDefault(p => p.GetMvcPageNameOfUIPage().ToUpper() == document.Name.ToUpper());
            if (uiPageItem != null)
            {
                RelatedFilePath1 = uiPageItem.FileNames[0];
                RelatedFilePath2 = GetMvcControllerOfUIPage(uiPageItem);
            }
        }

        public static bool IsMvcUIPage(this ProjectItem document) => document.Name.ToUpper().EndsWith(".CS") && document.FileNames[0].ToUpper().Contains("\\UI\\PAGES\\");

        public static bool IsMvcWebController(this ProjectItem document)
        {
            return document.Name.ToUpper().EndsWith("CONTROLLER.CS") && document.FileNames[0].ToUpper().Contains("\\WEBSITE\\CONTROLLERS\\PAGES\\");
        }

        public static bool IsMvcWebView(this ProjectItem document)
        {
            return document.Name.ToUpper().EndsWith(".CSHTML") && document.FileNames[0].ToUpper().Contains("\\WEBSITE\\VIEWS\\PAGES\\");
        }

        internal static bool IsMvcFile(this ProjectItem document)
        {
            return document.Name.ToUpper().EndsWithAny(".CS", ".CSHTML") && document.FileNames[0].ToUpper().ContainsAny(@"M#\UI\PAGES\", @"WEBSITE\CONTROLLERS\PAGES\", @"WEBSITE\VIEWS\PAGES\");
        }
        // ---------Modules------------
        internal static bool IsModuleFile(this ProjectItem document)
        {
            var docName = document.FileNames[0].ToUpper();

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
                    return document.FileCodeModel.CodeElements.OfType<CodeElement>().Any(p => p.Kind == vsCMElement.vsCMElementNamespace && p.Name == "ViewModel");
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

        internal static bool IsModuleOfUI(this ProjectItem document)
        {
            var docName = document.FileNames[0].ToUpper();

            if (docName.EndsWith(".CS") && docName.Contains("UI\\MODULES\\"))
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

        internal static bool IsModuleOfWebCtrlPage(this ProjectItem document)
        {
            var docName = document.FileNames[0].ToUpper();
            if (docName.EndsWith(".CS") && docName.Contains(@"WEBSITE\CONTROLLERS\PAGES\"))
            {
                return document.FileCodeModel.CodeElements.OfType<CodeElement>().Any(p => p.Kind == vsCMElement.vsCMElementNamespace && p.Name == "ViewModel");
            }

            return false;
        }

        internal static bool IsModuleOfWebViewPage(this ProjectItem document)
        {
            var docName = document.FileNames[0].ToUpper();
            if (docName.EndsWith(".CSHTML") && docName.Contains(@"WEBSITE\VIEWS\PAGES\"))
            {
                var pageFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Pages");
                var foundFile = pageFolder.ProjectItems.GetProjectItems().FirstOrDefault(i => i.Name.ToUpper().EndsWith(".CS") && i.FileCodeModel.CodeElements.OfType<CodeElement>().Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper() == "CONTROLLERS" && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper() == document.Name.ToUpper().Replace(".CSHTML", "CONTROLLER"))));
                return foundFile.FileCodeModel.CodeElements.OfType<CodeElement>().Any(p => p.Kind == vsCMElement.vsCMElementNamespace && p.Name == "ViewModel");
            }

            return false;
        }

        internal static bool IsModuleOfWebCtrlModule(this ProjectItem document)
        {
            return document.Name.ToUpper().EndsWith(".CS") && document.FileNames[0].ToUpper().Contains(@"WEBSITE\CONTROLLERS\MODULES\");
        }

        internal static bool IsModuleOfWebVeiwModule(this ProjectItem document)
        {
            return document.Name.ToUpper().EndsWith(".CSHTML") && document.FileNames[0].ToUpper().Contains(@"WEBSITE\VIEWS\MODULES\");
        }

        internal static string GetModuleOfUI(this ProjectItem document)
        {
            var moduleFilePath = "";
            var moduleName = document.FileCodeModel.CodeElements.Item("Modules").Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
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

        internal static void GetSiblingModuleOfUI(this ProjectItem document, ref string RelatedFilePath2, ref string RelatedFilePath3)
        {
            RelatedFilePath2 = "";
            bool isModule = true;
            var moduleName = document.FileCodeModel.CodeElements.Item("Modules").Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            var webCtrlFolder = App.DTE.Application.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers");
            var moduleFile = webCtrlFolder.ProjectItems.Item("Modules").ProjectItems.OfType<ProjectItem>().FirstOrDefault(m => m.Name.ToUpper().EndsWith(".CS") && m.FileCodeModel.CodeElements.OfType<CodeElement>()
            .Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("VIEWMODEL") && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper().Equals(moduleName.ToUpper()))));
            if (moduleFile == null)
            {
                isModule = false;
                moduleFile = webCtrlFolder.ProjectItems.Item("Pages").ProjectItems.OfType<ProjectItem>().FirstOrDefault(m => m.Name.ToUpper().EndsWith(".CS") && m.FileCodeModel.CodeElements.OfType<CodeElement>()
                .Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("VIEWMODEL") && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper().Equals(moduleName.ToUpper()))));
            }
            if (moduleFile != null)
            {
                RelatedFilePath2 = moduleFile.FileNames[0];

                if (isModule)
                {
                    RelatedFilePath3 = GetModuleOfWebCtrlModule(moduleFile);
                }
                else
                {
                    RelatedFilePath3 = GetModuleOfWebCtrlPage(moduleFile);
                }
            }
        }

        internal static string GetModuleOfWebCtrlPage(this ProjectItem document) => GetMvcPageOfWebController(document);

        internal static void GetSiblingModuleOfWebCtrlPage(this ProjectItem document, ref string RelatedFilePath1, ref string RelatedFilePath3) => GetSiblingMvcOfWebController(document, ref RelatedFilePath1, ref RelatedFilePath3);

        internal static string GetModuleOfWebCtrlModule(this ProjectItem document)
        {
            var moduleFilePath = "";
            var moduleName = document.FileCodeModel.CodeElements.OfType<CodeElement>().FirstOrDefault(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("VIEWMODEL")).Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            var moduleViewFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Views").ProjectItems.Item("Modules");
            var moduleFile = moduleViewFolder.ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == (moduleName + ".cshtml").ToUpper());
            if (moduleFile != null)
            {
                moduleFilePath = moduleFile.FileNames[0];
            }
            return moduleFilePath;
        }

        internal static void GetSiblingModuleOfWebCtrlModule(this ProjectItem document, ref string RelatedFilePath1, ref string RelatedFilePath3)
        {
            RelatedFilePath3 = "";
            var moduleName = document.FileCodeModel.CodeElements.OfType<CodeElement>().FirstOrDefault(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("VIEWMODEL")).Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            var moduleViewFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Views").ProjectItems.Item("Modules");
            var moduleFile = moduleViewFolder.ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == (moduleName + ".cshtml").ToUpper());
            if (moduleFile != null)
            {
                RelatedFilePath3 = moduleFile.FileNames[0];

                RelatedFilePath1 = GetModuleOfWebViewModule(moduleFile);
            }
        }

        internal static string GetModuleOfWebViewPage(this ProjectItem document)
        {
            var moduleFilePath = "";
            var ctrlName = document.Name.ToUpper().Replace(".CSHTML", "CONTROLLER");
            var pageFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Pages");
            var moduleCtrlFile = pageFolder.ProjectItems.GetProjectItems().FirstOrDefault(i => i.Name.ToUpper().EndsWith(".CS") && i.FileCodeModel.CodeElements.OfType<CodeElement>().Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper() == "CONTROLLERS" && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper() == ctrlName)));
            var moduleName = moduleCtrlFile.FileCodeModel.CodeElements.OfType<CodeElement>().FirstOrDefault(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("VIEWMODEL")).Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            var uiModuleFiles = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "#UI").ProjectItems.Item("Modules").ProjectItems.GetProjectItems().Where(f => f.Name.ToUpper().EndsWith(".CS"));
            var uiModuleFile = uiModuleFiles.FirstOrDefault(f => f.FileCodeModel.CodeElements.OfType<CodeElement>().Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("MODULES") && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper().Equals(moduleName.ToUpper()))));
            if (uiModuleFile != null)
            {
                moduleFilePath = uiModuleFile.FileNames[0];
            }
            return moduleFilePath;

        }

        internal static void GetSiblingModuleOfWebViewPage(this ProjectItem document, ref string RelatedFilePath1, ref string RelatedFilePath2)
        {
            RelatedFilePath1 = "";
            var ctrlName = document.Name.ToUpper().Replace(".CSHTML", "CONTROLLER");
            var pageFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Pages");
            var moduleCtrlFile = pageFolder.ProjectItems.GetProjectItems().FirstOrDefault(i => i.Name.ToUpper().EndsWith(".CS") && i.FileCodeModel.CodeElements.OfType<CodeElement>().Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper() == "CONTROLLERS" && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper() == ctrlName)));
            var moduleName = moduleCtrlFile.FileCodeModel.CodeElements.OfType<CodeElement>().FirstOrDefault(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("VIEWMODEL")).Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
            var uiModuleFiles = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "#UI").ProjectItems.Item("Modules").ProjectItems.GetProjectItems().Where(f => f.Name.ToUpper().EndsWith(".CS"));
            var uiModuleFile = uiModuleFiles.FirstOrDefault(f => f.FileCodeModel.CodeElements.OfType<CodeElement>().Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("MODULES") && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper().Equals(moduleName.ToUpper()))));
            if (uiModuleFile != null)
            {
                RelatedFilePath1 = uiModuleFile.FileNames[0];

                var moduleName2 = uiModuleFile.FileCodeModel.CodeElements.Item("Modules").Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
                var webCtrlFolder = App.DTE.Application.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers");
                var moduleFile2 = webCtrlFolder.ProjectItems.Item("Pages").ProjectItems.OfType<ProjectItem>().FirstOrDefault(m => m.Name.ToUpper().EndsWith(".CS") && m.FileCodeModel.CodeElements.OfType<CodeElement>()
                    .Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("VIEWMODEL") && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper().Equals(moduleName.ToUpper()))));
                if (moduleFile2 != null)
                {
                    RelatedFilePath2 = moduleFile2.FileNames[0];
                }
            }

        }

        internal static string GetModuleOfWebViewModule(this ProjectItem document)
        {
            var moduleFilePath = "";
            var moduleFiles = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "#UI").ProjectItems.Item("Modules").ProjectItems.GetProjectItems();
            var moduleFile = moduleFiles.FirstOrDefault(m => m.Name.ToUpper().Remove(".CS") == document.Name.ToUpper().Remove(".CSHTML"));
            if (moduleFile != null)
            {
                moduleFilePath = moduleFile.FileNames[0];
            }
            return moduleFilePath;
        }

        internal static void GetSiblingModuleOfWebViewModule(this ProjectItem document, ref string RelatedFilePath1, ref string RelatedFilePath2)
        {
            RelatedFilePath1 = "";
            var moduleFiles = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "#UI").ProjectItems.Item("Modules").ProjectItems.GetProjectItems();
            var moduleFile = moduleFiles.FirstOrDefault(m => m.Name.ToUpper().Remove(".CS") == document.Name.ToUpper().Remove(".CSHTML"));
            if (moduleFile != null)
            {
                RelatedFilePath1 = moduleFile.FileNames[0];

                var moduleName = moduleFile.FileCodeModel.CodeElements.Item("Modules").Children.OfType<CodeElement>().FirstOrDefault(c => c.Kind == vsCMElement.vsCMElementClass).Name;
                var webCtrlFolder = App.DTE.Application.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers");
                var moduleFile2 = webCtrlFolder.ProjectItems.Item("Modules").ProjectItems.OfType<ProjectItem>().FirstOrDefault(m => m.Name.ToUpper().EndsWith(".CS") && m.FileCodeModel.CodeElements.OfType<CodeElement>()
                .Any(n => n.Kind == vsCMElement.vsCMElementNamespace && n.Name.ToUpper().Equals("VIEWMODEL") && n.Children.OfType<CodeElement>().Any(c => c.Kind == vsCMElement.vsCMElementClass && c.Name.ToUpper().Equals(moduleName.ToUpper()))));
                if (moduleFile2 != null)
                {
                    RelatedFilePath2 = moduleFile2.FileNames[0];
                }
            }
        }

        // --------Components----------
        internal static bool IsComponentFile(this ProjectItem document)
        {
            var docName = document.FileNames[0].ToUpper();
            if (docName.EndsWithAny(".CS", ".CSHTML") && docName.Contains("WEBSITE\\") && docName.Contains("\\COMPONENTS\\"))
            {
                return true;
            }
            else if (docName.EndsWith(".CS") && docName.Contains("UI\\MODULES\\"))
            {
                var compFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Modules").ProjectItems.OfType<ProjectItem>().FirstOrDefault(c => c.Name == "Components");
                if (compFolder != null)
                {
                    return compFolder.ProjectItems.GetProjectItems().Any(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.ToUpper() == document.Name.ToUpper());
                }
            }

            return false;
        }

        internal static bool IsComponentOfUI(this ProjectItem document)
        {
            var docName = document.FileNames[0].ToUpper();
            if (docName.EndsWith(".CS") && docName.Contains("UI\\MODULES\\"))
            {
                var compFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Modules").ProjectItems.OfType<ProjectItem>().FirstOrDefault(f=> f.Name.ToUpper() == "COMPONENTS");
                if (compFolder != null)
                {
                    return compFolder.ProjectItems.GetProjectItems().Any(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.ToUpper() == document.Name.ToUpper());
                }
            }

            return false;
        }

        internal static bool IsComponentOfWebCtrl(this ProjectItem document)
        {
            return document.Name.ToUpper().EndsWith(".CS") && document.FileNames[0].ToUpper().Contains(@"WEBSITE\CONTROLLERS\MODULES\COMPONENTS\");
        }

        internal static bool IsComponentOfWebView(this ProjectItem document)
        {
            return document.Name.ToUpper().EndsWith(".CSHTML") && document.FileNames[0].ToUpper().Contains(@"WEBSITE\VIEWS\MODULES\COMPONENTS\");
        }

        internal static string GetComponentFromUI(this ProjectItem document)
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

        internal static void GetSiblingComponentFromUI(this ProjectItem document, ref string RelatedFilePath2, ref string RelatedFilePath3)
        {
            RelatedFilePath2 = "";
            var compFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Controllers").ProjectItems.Item("Modules").ProjectItems.OfType<ProjectItem>().FirstOrDefault(c => c.Name.ToUpper() == "COMPONENTS");
            if (compFolder != null)
            {
                var compFile = compFolder.ProjectItems.GetProjectItems().FirstOrDefault(c => c.Name.ToUpper() == document.Name.ToUpper());
                if (compFile != null)
                {
                    RelatedFilePath2 = compFile.FileNames[0];
                    RelatedFilePath3 = GetComponentFromCtrl(compFile);
                }
            }

        }

        internal static string GetComponentFromCtrl(this ProjectItem document)
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

        internal static void GetSiblingComponentFromCtrl(this ProjectItem document, ref string RelatedFilePath1, ref string RelatedFilePath3)
        {
            RelatedFilePath3 = "";
            var compFolder = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "WEBSITE").ProjectItems.Item("Views").ProjectItems.Item("Modules").ProjectItems.OfType<ProjectItem>().FirstOrDefault(c => c.Name.ToUpper() == "COMPONENTS");
            if (compFolder != null)
            {
                var compFolderFile = compFolder.ProjectItems.GetProjectItems().FirstOrDefault(c => c.Name.ToUpper() == document.Name.ToUpper().Remove(".CS"));
                if (compFolderFile != null)
                {
                    var compoFile = compFolderFile.ProjectItems.GetProjectItems().FirstOrDefault(c => c.Name.ToUpper() == "DEFAULT.CSHTML");
                    if (compoFile != null)
                    {
                        RelatedFilePath3 = compoFile.FileNames[0];
                        RelatedFilePath1 = GetComponentFromUI(compoFile);
                    }
                }
            }
        }

        internal static string GetComponentFromView(this ProjectItem document)
        {
            var compoFilePath = "";
            var docName = App.DTE.ActiveDocument.FullName.ToUpper().Replace("\\DEFAULT.CSHTML", ".CS");
            var ix = docName.LastIndexOf("\\");
            var itemName = docName.Substring(++ix);
            var projItem = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "#UI");
            var fileItem = projItem.ProjectItems.Item("Modules").ProjectItems.GetProjectItems().FirstOrDefault(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.ToUpper() == itemName);
            if (fileItem != null)
            {
                compoFilePath = fileItem.FileNames[0];
            }
            return compoFilePath;
        }

        internal static void GetSiblingComponentFromView(this ProjectItem document, ref string RelatedFilePath1, ref string RelatedFilePath2)
        {
            RelatedFilePath1 = "";
            var docName = App.DTE.ActiveDocument.FullName.ToUpper().Replace("\\DEFAULT.CSHTML", ".CS");
            var ix = docName.LastIndexOf("\\");
            var itemName = docName.Substring(++ix);
            var projItem = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "#UI");
            var fileItem = projItem.ProjectItems.Item("Modules").ProjectItems.GetProjectItems().FirstOrDefault(i => i.Name.ToUpper().EndsWith(".CS") && i.Name.ToUpper() == itemName);
            if (fileItem != null)
            {
                RelatedFilePath1 = fileItem.FileNames[0];
                RelatedFilePath2 = GetComponentFromCtrl(fileItem);
            }
        }

        // -------Entities----------------
        internal static bool IsEntityFile(this ProjectItem document)
        {
            if (document.Name.ToUpper().EndsWith(".CS"))
            {
                if (document.FileNames[0].ToUpper().Contains("MODEL\\"))
                {
                    return true;
                }

                if (document.FileNames[0].ToUpper().Contains("DOMAIN\\") && document.FileNames[0].ToUpper().Contains("LOGIC"))
                {
                    return true;
                }

                if (document.FileNames[0].ToUpper().Contains("DOMAIN\\") && document.FileNames[0].ToUpper().Contains("ENTITIES"))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsEntityOfModel(this ProjectItem document)
        {
            if (document.Name.ToUpper().EndsWith(".CS") && document.ContainingProject.Name.ToUpper().Equals("#MODEL"))
            {
                var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
                return domainProj.ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper());
            }

            return false;
        }

        internal static bool IsEntityOfDomainEntity(this ProjectItem document)
        {
            if (document.Name.ToUpper().EndsWith(".CS") && document.FileNames[0].ToUpper().Contains(@"DOMAIN\") && document.FileNames[0].ToUpper().Contains("ENTITIES"))
            {
                var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
                var logicFolder = domainProj.ProjectItems.GetProjectItems().FirstOrDefault(f => !f.Name.Contains(".") && f.Name.ToUpper().Contains("LOGIC"));
                if (logicFolder.ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper()))
                    return true;
                var modelProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "#MODEL");
                if (modelProj.ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper()))
                    return true;
            }

            return false;
        }

        internal static bool IsEntityOfDomainLogic(this ProjectItem document)
        {
            if (document.Name.ToUpper().EndsWith(".CS") && document.FileNames[0].ToUpper().Contains(@"DOMAIN\") && document.FileNames[0].ToUpper().Contains("LOGIC"))
            {
                var modelProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "#MODEL");
                if (modelProj.ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper()))
                    return true;
                var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
                var entityFolder = domainProj.ProjectItems.GetProjectItems().FirstOrDefault(f => !f.Name.Contains(".") && f.Name.ToUpper().Contains("ENTITIES"));
                if (entityFolder.ProjectItems.GetProjectItems().Any<ProjectItem>(f => f.Name.ToUpper() == document.Name.ToUpper()))
                    return true;
            }

            return false;
        }

        internal static string GetEntityFromModel(this ProjectItem document)
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

        internal static void GetSiblingEntityOfModel(this ProjectItem document, ref string RelatedFilePath2, ref string RelatedFilePath3)
        {
            var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
            var entityFolder = domainProj.ProjectItems.GetProjectItems().FirstOrDefault(f => !f.Name.Contains(".") && f.Name.ToUpper().Contains("ENTITIES"));
            var entityFile1 = entityFolder.ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            if (entityFile1 != null)
            {
                RelatedFilePath2 = entityFile1.FileNames[0];
            }

            var logicFolder = domainProj.ProjectItems.GetProjectItems().FirstOrDefault(f => !f.Name.Contains(".") && f.Name.ToUpper().Contains("LOGIC"));
            var entityFile2 = logicFolder.ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            if (entityFile2 != null)
            {
                RelatedFilePath3 = entityFile2.FileNames[0];
            }
        }

        internal static string GetEntityFromDomainEntity(this ProjectItem document)
        {
            var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
            var logicFolder = domainProj.ProjectItems.GetProjectItems().FirstOrDefault(f => !f.Name.Contains(".") && f.Name.ToUpper().Contains("LOGIC"));
            var entityFile = logicFolder.ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            if (entityFile == null)
            {
                entityFile = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "#MODEL").ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            }

            return entityFile.FileNames[0];
        }

        internal static void GetSiblingEntityOfDomainEntity(this ProjectItem document, ref string RelatedFilePath1, ref string RelatedFilePath3)
        {
            var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
            var logicFolder = domainProj.ProjectItems.GetProjectItems().FirstOrDefault(f => !f.Name.Contains(".") && f.Name.ToUpper().Contains("LOGIC"));
            var entityFile1 = logicFolder.ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            if (entityFile1 != null)
            {
                RelatedFilePath3 = entityFile1.FileNames[0];
            }
            var entityFile2 = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "#MODEL").ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            if (entityFile2 != null)
            {
                RelatedFilePath1 = entityFile2.FileNames[0];
            }
        }

        internal static string GetEntityFromDomainLogic(this ProjectItem document)
        {
            var entityFile = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "#MODEL").ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            if (entityFile == null)
            {
                var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
                var entityFolder = domainProj.ProjectItems.GetProjectItems().FirstOrDefault(f => !f.Name.Contains(".") && f.Name.ToUpper().Contains("ENTITIES"));
                entityFile = entityFolder.ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            }

            return entityFile.FileNames[0];
        }

        internal static void GetSiblingEntityOfDomainLogic(this ProjectItem document, ref string RelatedFilePath1, ref string RelatedFilePath2)
        {
            var entityFile1 = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "#MODEL").ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            if (entityFile1 != null)
            {
                RelatedFilePath1 = entityFile1.FileNames[0];
            }
            var domainProj = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name.ToUpper() == "DOMAIN");
            var entityFolder = domainProj.ProjectItems.GetProjectItems().FirstOrDefault(f => !f.Name.Contains(".") && f.Name.ToUpper().Contains("ENTITIES"));
            var entityFile2 = entityFolder.ProjectItems.GetProjectItems().FirstOrDefault(f => f.Name.ToUpper() == document.Name.ToUpper());
            if (entityFile2 != null)
            {
                RelatedFilePath2 = entityFile2.FileNames[0];
            }

        }

        //------------Next File Functions--------------------
        public static bool NextEntityFilePath(ProjectItem curDocument,ref string RelatedFilePath)
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

        public static bool NextComponentFilePath(ProjectItem curDocument, ref string RelatedFilePath)
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

        public static bool NextModuleFilePath(ProjectItem curDocument,ref string RelatedFilePath,ref PageOrModule State)
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

        public static bool NextMvcFilePath(ProjectItem curDocument, ref string RelatedFilePath, ref PageOrModule State)
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