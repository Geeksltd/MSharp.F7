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
    public class OpenDomainFileFromMsharpDomain : OpenInMSharpHandler
    {
        OleMenuCommandService Menu;
        String RelatedFilePath;
        public OpenDomainFileFromMsharpDomain(OleMenuCommandService menu)
        {
            Menu = menu;
        }

        public void SetupCommands()
        {
            var menuCommandID = new CommandID(GuidList.GuidOpenRelatedFileInVSCmdSet, (int)PkgCmdIDList.CmdOpenDomainFileFromMsharpDomain);
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
            RelatedFilePath = App.DTE.Solution.FullName.Substring(0, App.DTE.Solution.FullName.LastIndexOf(@"\")) + @"\Domain\Entities\" + App.DTE.ActiveDocument.ProjectItem.Name;
            var cmd = sender as OleMenuCommand;
            cmd.Visible = false;
            var relatedFile = new System.IO.FileInfo(RelatedFilePath);

            if (null != cmd && App.DTE.ActiveDocument.FullName.ToUpper().IsEntityFile() && relatedFile.Exists && App.DTE.ActiveWindow.Project.Name == "@MSharp.Domain")
            {
                cmd.Visible = true;
            }
        }
    }
}
