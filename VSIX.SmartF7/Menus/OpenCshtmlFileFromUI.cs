using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;
using Geeks.GeeksProductivityTools.Menus.OpenInMSharp;
using System.Windows;
using System.Linq;
using EnvDTE80;
using EnvDTE;

namespace Geeks.GeeksProductivityTools.Menus
{
    public class OpenCshtmlFileFromUI : OpenInMSharpHandler
    {
        OleMenuCommandService Menu;
        String RelatedFilePath;
        public OpenCshtmlFileFromUI(OleMenuCommandService menu)
        {
            Menu = menu;
        }

        public void SetupCommands()
        {
            var menuCommandID = new CommandID(GuidList.GuidOpenRelatedFileInVSCmdSet, (int)PkgCmdIDList.CmdOpenCshtmlFileFromUI);
            var menuItem = new OleMenuCommand(OpenRelatedFileMenuItemCallback, menuCommandID);

            menuItem.BeforeQueryStatus += OpenRelatedFileMenuItem_BeforeQueryStatus;
            Menu.AddCommand(menuItem);
        }

        async void OpenRelatedFileMenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                App.DTE.ItemOperations.OpenFile(RelatedFilePath);
            }
            catch (Exception err)
            {
                System.Windows.Forms.MessageBox.Show(err.Message);
            }
}

        void OpenRelatedFileMenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            var cmd = sender as OleMenuCommand;
            cmd.Visible = false;

            if (null != cmd && App.DTE.ActiveDocument.FullName.IsUIPageFile())
            {
                cmd.Visible = true;
                try
                {
                    var nameSpace = App.DTE.ActiveDocument.ProjectItem.FileCodeModel.CodeElements.OfType<CodeElement>().SingleOrDefault(d => d.Kind == vsCMElement.vsCMElementNamespace).Name;
                    var fileName = nameSpace.Remove(".") + App.DTE.ActiveDocument.ProjectItem.Name + "html";
                    var prjItem = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name == "Website").ProjectItems.Item("Views").ProjectItems.Item("Pages").ProjectItems.Item(fileName);
                    RelatedFilePath = prjItem.FileNames[0];
                    
                }
                catch (Exception)
                {
                    cmd.Visible = false;
                }
            }
        }
    }
}
