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
            try
            {
                if (null != cmd && App.DTE.ActiveDocument.FullName.IsEntityFile())
                {
                    if (NextEntityFilePath())
                    {
                        cmd.Visible = true;
                        cmd.Text = "Go To Related File";
                    }
                }
                else if (null != cmd && App.DTE.ActiveDocument.FullName.IsUIPageFile())
                {
                    if (NextMvcFilePath())
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

        private Boolean NextEntityFilePath()
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

        private Boolean NextMvcFilePath()
        {
            try
            {
                var nameSpace = App.DTE.ActiveDocument.ProjectItem.FileCodeModel.CodeElements.OfType<CodeElement>().SingleOrDefault(d => d.Kind == vsCMElement.vsCMElementNamespace).Name;
                var fileName = nameSpace.Remove(".") + App.DTE.ActiveDocument.ProjectItem.Name + "html";
                var prjItem = App.DTE.Solution.Projects.OfType<Project>().FirstOrDefault(p => p.Name == "Website").ProjectItems.Item("Views").ProjectItems.Item("Pages").ProjectItems.Item(fileName);
                RelatedFilePath = prjItem.FileNames[0];
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

    }
}
