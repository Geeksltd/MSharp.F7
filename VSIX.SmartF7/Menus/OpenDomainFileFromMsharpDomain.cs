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
                App.DTE.ItemOperations.OpenFile(NextRelatedFilePath());
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

            if (null != cmd && App.DTE.ActiveDocument.FullName.ToUpper().IsEntityFile() )
            {
                cmd.Visible = true;
            }
        }

        private string NextRelatedFilePath()
        {
            string result= App.DTE.ActiveDocument.FullName;

            var solutionDir = App.DTE.Solution.FullName.Substring(0, App.DTE.Solution.FullName.LastIndexOf(@"\"));

            switch (App.DTE.ActiveWindow.Project.Name)
            {
                case "@MSharp.Domain":
                    if ((new System.IO.DirectoryInfo(solutionDir + @"\Domain\Entities\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).Count() > 0)
                    {
                        result = (new System.IO.DirectoryInfo(solutionDir + @"\Domain\Entities\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).FirstOrDefault().FullName;
                    }
                    else if ((new System.IO.DirectoryInfo(solutionDir + @"\Domain\-Logic\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).Count() > 0)
                    {
                        result = (new System.IO.DirectoryInfo(solutionDir + @"\Domain\-Logic\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).FirstOrDefault().FullName;
                    }
                   break;
                case "Domain":
                    if (App.DTE.ActiveDocument.FullName.Contains("Entities"))
                    {
                        if ((new System.IO.DirectoryInfo(solutionDir + @"\Domain\-Logic\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).Count() > 0)
                        {
                            result = (new System.IO.DirectoryInfo(solutionDir + @"\Domain\-Logic\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).FirstOrDefault().FullName;
                        }
                        else if ((new System.IO.DirectoryInfo(solutionDir + @"\@M#\@MSharp.Domain\Entities\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).Count() > 0)
                        {
                            result = (new System.IO.DirectoryInfo(solutionDir + @"\@M#\@MSharp.Domain\Entities\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).FirstOrDefault().FullName;
                        }
                    }
                    else if (App.DTE.ActiveDocument.FullName.Contains("-Logic"))
                    {
                        if ((new System.IO.DirectoryInfo(solutionDir + @"\@M#\@MSharp.Domain\Entities\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).Count() > 0)
                        {
                            result = (new System.IO.DirectoryInfo(solutionDir + @"\@M#\@MSharp.Domain\Entities\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).FirstOrDefault().FullName;
                        }
                        else if ((new System.IO.DirectoryInfo(solutionDir + @"\Domain\Entities\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).Count() > 0)
                        {
                            result = (new System.IO.DirectoryInfo(solutionDir + @"\Domain\Entities\")).GetFiles(App.DTE.ActiveDocument.ProjectItem.Name, System.IO.SearchOption.AllDirectories).FirstOrDefault().FullName;
                        }
                    }
                    break;
            }

            return result;
        }
    }
}
