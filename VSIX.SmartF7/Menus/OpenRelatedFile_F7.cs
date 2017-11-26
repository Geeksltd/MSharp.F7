using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;
using Geeks.GeeksProductivityTools.Menus.OpenInMSharp;
using System.Windows;
using System.Linq;
using EnvDTE80;
using EnvDTE;
using Geeks.SmartF7.ToggleHandler;

namespace Geeks.GeeksProductivityTools.Menus
{
    public class OpenRelatedFileF7 
    {
        OleMenuCommandService Menu;
        string RelatedFilePath;
        public static PageOrModule State = PageOrModule.None;
        public enum PageOrModule { None, Page, Module }

        public OpenRelatedFileF7(OleMenuCommandService menu)
        {
            Menu = menu;
        }

        public void SetupCommands()
        {
            var menuCommandID = new CommandID(GuidList.GuidOpenRelatedFileF7InVSCmdSet, (int)PkgCmdIDList.CmdOpenRelatedFileF7ID);
            var menuItem = new OleMenuCommand(OpenRelatedFileMenuItemCallback, menuCommandID);
            menuItem.BeforeQueryStatus += OpenRelatedFileMenuItem_BeforeQueryStatus;
            Menu.AddCommand(menuItem);


            var command = App.DTE.Commands.Item("EditorContextMenus.CodeWindow.GoToRelatedFile", -1);
            command.Bindings = "Global::F7";
        }

        async void OpenRelatedFileMenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                App.DTE.ItemOperations.OpenFile(RelatedFilePath);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        void OpenRelatedFileMenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            var currentDocument = App.DTE.ActiveDocument;
            var cmd = sender as OleMenuCommand;
            cmd.Visible = false;

                try
                {
                if (null != cmd)
                    if (currentDocument.IsEntityFile())
                    {
                        if (NextEntityFilePath())
                        {
                            cmd.Visible = true;
                            cmd.Text = "Go To Related Entity File";
                            State = PageOrModule.None;
                        }
                    }
                    else if (currentDocument.IsComponentFile())
                    {
                        if (NextComponentFilePath(currentDocument, ref RelatedFilePath))
                        {
                            cmd.Visible = true;
                            cmd.Text = "Go To Related Component File";
                            State = PageOrModule.None;
                        }
                    }
                    else if (State == PageOrModule.Page && currentDocument.IsModuleOfWebCtrlPage() && currentDocument.IsMvcWebController())
                    {
                        if (NextMvcFilePath(App.DTE.ActiveDocument, ref RelatedFilePath))
                        {
                            cmd.Visible = true;
                            cmd.Text = "Go To Related MVC Page";
                        }
                    }
                    else if (State == PageOrModule.Page && currentDocument.IsModuleOfWebViewPage() && currentDocument.IsMvcWebView())
                    {
                        if (NextMvcFilePath(App.DTE.ActiveDocument, ref RelatedFilePath))
                        {
                            cmd.Visible = true;
                            cmd.Text = "Go To Related MVC Page";
                        }
                    }
                    else if (currentDocument.IsModuleFile())
                    {
                        if (NextModuleFilePath(currentDocument, ref RelatedFilePath))
                        {
                            cmd.Visible = true;
                            cmd.Text = "Go To Related Module File";
                            State = PageOrModule.None;
                        }
                    }
                    else if (currentDocument.IsMvcFile())
                    {
                        if (NextMvcFilePath(App.DTE.ActiveDocument, ref RelatedFilePath))
                        {
                            cmd.Visible = true;
                            cmd.Text = "Go To Related MVC Page";
                        }
                    }
            }
            catch (Exception)
            {
                cmd.Visible = false;
            }
        }

        private bool NextEntityFilePath()
        {
            try
            {
                var solutionDir = App.DTE.Solution.FullName.Substring(0, App.DTE.Solution.FullName.LastIndexOf(@"\"));

                switch (App.DTE.ActiveWindow.Project.Name.ToUpper())
                {
                    case "@MODEL":
                        if ((new System.IO.DirectoryInfo(solutionDir + @"\Domain\Entities\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).Count() > 0)
                        {
                            RelatedFilePath = (new System.IO.DirectoryInfo(solutionDir + @"\Domain\Entities\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).FirstOrDefault().FullName;
                        }
                        else if ((new System.IO.DirectoryInfo(solutionDir + @"\Domain\-Logic\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).Count() > 0)
                        {
                            RelatedFilePath = (new System.IO.DirectoryInfo(solutionDir + @"\Domain\-Logic\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).FirstOrDefault().FullName;
                        }
                        else
                            return false;
                        break;
                    case "DOMAIN":
                        if (App.DTE.ActiveDocument.FullName.ToUpper().Contains("ENTITIES"))
                        {
                            if ((new System.IO.DirectoryInfo(solutionDir + @"\Domain\-Logic\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).Count() > 0)
                            {
                                RelatedFilePath = (new System.IO.DirectoryInfo(solutionDir + @"\Domain\-Logic\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).FirstOrDefault().FullName;
                            }
                            else if ((new System.IO.DirectoryInfo(solutionDir + "\\@M#\\@Model\\Entities\\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).Count() > 0)
                            {
                                RelatedFilePath = (new System.IO.DirectoryInfo(solutionDir + "\\@M#\\@Model\\Entities\\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).FirstOrDefault().FullName;
                            }
                            else
                                return false;
                        }
                        else if (App.DTE.ActiveDocument.FullName.ToUpper().Contains("-LOGIC"))
                        {
                            if ((new System.IO.DirectoryInfo(solutionDir + "\\@M#\\@Model\\Entities\\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).Count() > 0)
                            {
                                RelatedFilePath = (new System.IO.DirectoryInfo(solutionDir + "\\@M#\\@Model\\Entities\\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).FirstOrDefault().FullName;
                            }
                            else if ((new System.IO.DirectoryInfo(solutionDir + @"\Domain\Entities\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).Count() > 0)
                            {
                                RelatedFilePath = (new System.IO.DirectoryInfo(solutionDir + @"\Domain\Entities\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).FirstOrDefault().FullName;
                            }
                            else
                                return false;
                        }
                        break;
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool NextComponentFilePath(Document curDocument, ref string relatedFilePath)
        {
            try
            {
                if (curDocument.IsComponentOfUI())
                {
                    relatedFilePath = curDocument.GetComponentFromUI();
                    return true;
                }
                else if (curDocument.IsComponentOfWebCtrl())
                {
                    relatedFilePath = curDocument.GetComponentFromCtrl();
                    return true;
                }
                else if (curDocument.IsComponentOfWebView())
                {
                    relatedFilePath = curDocument.GetComponentFromView();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool NextModuleFilePath(Document curDocument, ref string relatedFilePath)
        {
            try
            {
                if (curDocument.IsModuleOfUI())
                {
                    relatedFilePath = curDocument.GetModuleOfUI();
                    State = PageOrModule.Module;
                    return true;
                }
                else if (curDocument.IsModuleOfWebCtrlModule())
                {
                    relatedFilePath = curDocument.GetModuleOfWebCtrlModule();
                    //State = PageOrModule.Module;
                    return true;
                }
                else if (curDocument.IsModuleOfWebCtrlPage())
                {
                    RelatedFilePath = curDocument.GetModuleOfWebCtrlPage();
                    //State = PageOrModule.Module;
                    return true;
                }
                else if (curDocument.IsModuleOfWebVeiwModule())
                {
                    relatedFilePath = curDocument.GetModuleOfWebViewModule();
                    //State = PageOrModule.Module;
                    return true;
                }
                else if (curDocument.IsModuleOfWebViewPage())
                {
                    relatedFilePath = curDocument.GetModuleOfWebViewPage();
                    //State = PageOrModule.Module;
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool NextMvcFilePath(Document curDocument, ref string relatedFilePath)
        {
            try
            {
                if (curDocument.IsMvcUIPage())
                {
                    relatedFilePath = curDocument.GetMvcControllerOfUIPage();
                    State = PageOrModule.Page;
                    return true;
                }
                else if (curDocument.IsMvcWebController())
                {
                    relatedFilePath = curDocument.GetMvcPageOfWebController();
                    return true;
                }
                else if (curDocument.IsMvcWebView())
                {
                    relatedFilePath = curDocument.GetMvcPageOfWebView();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }


    }
}
