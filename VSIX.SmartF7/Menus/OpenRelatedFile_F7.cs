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
                //MessageBox.Show(App.DTE.ActiveDocument.GetEntityFromDomainEntity());
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
                        if (NextEntityFilePath(currentDocument, ref RelatedFilePath))
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

        private bool NextEntityFilePath(Document curDocument, ref string relatedFilePath)
        {
            try
            {
                if (curDocument.IsEntityOfModel())
                {
                    relatedFilePath = curDocument.GetEntityFromModel();
                    return true;
                }
                if (curDocument.IsEntityOfDomainEntity())
                {
                    relatedFilePath = curDocument.GetEntityFromDomainEntity();
                    return true;
                }
                if (curDocument.IsEntityOfDomainLogic())
                {
                    relatedFilePath = curDocument.GetEntityFromDomainLogic();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
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
