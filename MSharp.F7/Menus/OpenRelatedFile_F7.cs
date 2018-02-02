using System;
using System.ComponentModel.Design;
using EnvDTE;
using MSharp.F7.Menus.OpenInMSharp;
using MSharp.F7.ToggleHandler;
using Microsoft.VisualStudio.Shell;
using System.Diagnostics;

namespace MSharp.F7.Menus
{
    public class OpenRelatedFileF7
    {
        OleMenuCommandService Menu;
        string RelatedFilePath;
        
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
            //string st1 = "", st2 = "";
        //    var currentDocument = App.DTE.ActiveDocument.ProjectItem;
        //    if (currentDocument.IsMvcWebController())
        //    {
        //        currentDocument.GetSiblingPageOfWebController(ref st1,ref st2);
        //        System.Windows.Forms.MessageBox.Show(st1 + Environment.NewLine + st2);
        //    }
        //    if (currentDocument.IsMvcWebView())
        //    {
        //        currentDocument.GetSiblingPageOfWebView(ref st1, ref st2);
        //        System.Windows.Forms.MessageBox.Show(st1 + Environment.NewLine + st2);
        //    }
            try
            {
                App.DTE.ItemOperations.OpenFile(RelatedFilePath);
            }
            catch (Exception err)
            {
                StackTrace st = new StackTrace(err);
                Debug.WriteLine(st.ToString());
            }
        }

        void OpenRelatedFileMenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            var currentDocument = App.DTE.ActiveDocument.ProjectItem;
            var cmd = sender as OleMenuCommand;
            cmd.Visible = false;
            try
            {
                if (null != cmd)
                    if (currentDocument.IsEntityFile())
                    { 
                        if (Toolbox.NextEntityFilePath(currentDocument, ref RelatedFilePath))
                        {
                            cmd.Visible = true;
                            cmd.Text = "Go To Related Entity File";
                            Toolbox.State = PageOrModule.None;
                        }
                    }
                    else if (currentDocument.IsComponentFile())
                    {
                        if (Toolbox.NextComponentFilePath(currentDocument, ref RelatedFilePath))
                        {
                            cmd.Visible = true;
                            cmd.Text = "Go To Related Component File";
                            Toolbox.State = PageOrModule.None;
                        }
                    }
                    else if (Toolbox.State == PageOrModule.Page && currentDocument.IsModuleOfWebCtrlPage() && currentDocument.IsMvcWebController())
                    {
                        if (Toolbox.NextMvcFilePath(currentDocument,ref RelatedFilePath,ref Toolbox.State))
                        { cmd.Visible = true; cmd.Text = "Go To Related MVC Page"; }
                    }
                    else if (Toolbox.State == PageOrModule.Page && currentDocument.IsModuleOfWebViewPage() && currentDocument.IsMvcWebView())
                    {
                        if (Toolbox.NextMvcFilePath(currentDocument, ref RelatedFilePath, ref Toolbox.State)) { cmd.Visible = true; cmd.Text = "Go To Related MVC Page"; }
                    }
                    else if (currentDocument.IsModuleFile())
                    {
                        if (Toolbox.NextModuleFilePath(currentDocument,ref RelatedFilePath,ref Toolbox.State))
                        {
                            cmd.Visible = true;
                            cmd.Text = "Go To Related Module File";
                            Toolbox.State = PageOrModule.None;
                        }
                    }
                    else if (currentDocument.IsMvcFile())
                        if (Toolbox.NextMvcFilePath(currentDocument,ref RelatedFilePath,ref Toolbox.State))
                        {
                            cmd.Visible = true;
                            cmd.Text = "Go To Related MVC Page";
                        }
            }
            catch (Exception err)
            {
                StackTrace st = new StackTrace(err);
                Debug.WriteLine(st.ToString());
            }
        }
    }
}