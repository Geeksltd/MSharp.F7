using EnvDTE;
using Geeks.GeeksProductivityTools.Menus.OpenInMSharp;
using Geeks.SmartF7.ToggleHandler;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Windows;

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

        void OpenRelatedFileMenuItemCallback(object sender, EventArgs e)
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
                        if (NextEntityFilePath(currentDocument))
                        {
                            cmd.Visible = true;
                            cmd.Text = "Go To Related Entity File";
                            State = PageOrModule.None;
                        }
                    }
                    else if (currentDocument.IsComponentFile())
                    {
                        if (NextComponentFilePath(currentDocument))
                        {
                            cmd.Visible = true;
                            cmd.Text = "Go To Related Component File";
                            State = PageOrModule.None;
                        }
                    }
                    else if (State == PageOrModule.Page && currentDocument.IsModuleOfWebCtrlPage() && currentDocument.IsMvcWebController())
                    {
                        if (NextMvcFilePath(App.DTE.ActiveDocument))
                        { cmd.Visible = true; cmd.Text = "Go To Related MVC Page"; }
                    }
                    else if (State == PageOrModule.Page && currentDocument.IsModuleOfWebViewPage() && currentDocument.IsMvcWebView())
                    {
                        if (NextMvcFilePath(App.DTE.ActiveDocument)) { cmd.Visible = true; cmd.Text = "Go To Related MVC Page"; }
                    }
                    else if (currentDocument.IsModuleFile())
                    {
                        if (NextModuleFilePath(currentDocument))
                        {
                            cmd.Visible = true;
                            cmd.Text = "Go To Related Module File";
                            State = PageOrModule.None;
                        }
                    }
                    else if (currentDocument.IsMvcFile())
                        if (NextMvcFilePath(App.DTE.ActiveDocument))
                        {
                            cmd.Visible = true;
                            cmd.Text = "Go To Related MVC Page";
                        }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        bool NextEntityFilePath(Document curDocument)
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

        bool NextComponentFilePath(Document curDocument)
        {
            if (curDocument.IsComponentOfUI())
            {
                RelatedFilePath = curDocument.GetComponentFromUI();
                return true;
            }
            else if (curDocument.IsComponentOfWebCtrl())
            {
                RelatedFilePath = curDocument.GetComponentFromCtrl();
                return true;
            }
            else if (curDocument.IsComponentOfWebView())
            {
                RelatedFilePath = curDocument.GetComponentFromView();
                return true;
            }

            return false;
        }

        bool NextModuleFilePath(Document curDocument)
        {
            if (curDocument.IsModuleOfUI())
            {
                RelatedFilePath = curDocument.GetModuleOfUI();
                State = PageOrModule.Module;
                return true;
            }
            else if (curDocument.IsModuleOfWebCtrlModule())
            {
                RelatedFilePath = curDocument.GetModuleOfWebCtrlModule();
                return true;
            }
            else if (curDocument.IsModuleOfWebCtrlPage())
            {
                RelatedFilePath = curDocument.GetModuleOfWebCtrlPage();
                return true;
            }
            else if (curDocument.IsModuleOfWebVeiwModule())
            {
                RelatedFilePath = curDocument.GetModuleOfWebViewModule();
                return true;
            }
            else if (curDocument.IsModuleOfWebViewPage())
            {
                RelatedFilePath = curDocument.GetModuleOfWebViewPage();
                return true;
            }

            return false;
        }

        bool NextMvcFilePath(Document curDocument)
        {
            if (curDocument.IsMvcUIPage())
            {
                RelatedFilePath = curDocument.GetMvcControllerOfUIPage();
                State = PageOrModule.Page;
                return true;
            }
            else if (curDocument.IsMvcWebController())
            {
                RelatedFilePath = curDocument.GetMvcPageOfWebController();
                return true;
            }
            else if (curDocument.IsMvcWebView())
            {
                RelatedFilePath = curDocument.GetMvcPageOfWebView();
                return true;
            }

            return false;
        }
    }
}