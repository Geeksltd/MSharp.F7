using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Geeks.GeeksProductivityTools.Menus
{
    public class RunBatchFiles
    {
        OleMenuCommandService Menu;

        public RunBatchFiles(OleMenuCommandService menu)
        {
            Menu = menu;
        }

        public void SetupCommands()
        {
            var menuCommandID = new CommandID(GuidList.GuidRunBatchFileCmdSet, (int)PkgCmdIDList.CmdRunBatchFile);
            var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
            menuItem.BeforeQueryStatus += BeforeQueryStatus;
            Menu.AddCommand(menuItem);
        }

        async void MenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                var fileName = GetSelectedItem().Get(f => f.ToLower());
                if (fileName == null) return;

                var workingDirectory = Path.GetDirectoryName(fileName);
                var projectFolder = FindProjectFolder(workingDirectory);
                if (projectFolder != null)
                    workingDirectory = projectFolder;

                await System.Threading.Tasks.Task.Run(() => Process.Start(new ProcessStartInfo
                {
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    FileName = "cmd.exe",
                    Arguments = "/c \"\"" + fileName + "\"\""
                }));
            }
            catch (Exception err)
            {
                ErrorNotification.EmailError(err);
            }
        }

        string FindProjectFolder(string directory)
        {
            while (directory != null && !ContainsProjectFile(directory))
                directory = Directory.GetParent(directory).Get(d => d.FullName);


            return directory;
        }

        bool ContainsProjectFile(string directory) => Directory.GetFiles(directory, "*.csproj").Any();

        void BeforeQueryStatus(object sender, EventArgs e)
        {
            var command = sender as OleMenuCommand;
            if (command != null)
            {
                command.Visible = false;

                var fileName = GetSelectedItem();
                if (fileName == null) return;

                command.Visible = fileName.ToLower().EndsWithAny(".bat", ".cmd", ".ps");
            }
        }

        static string GetSelectedItem()
        {
            IntPtr hierarchyPtr, selectionContainerPtr;
            uint projectItemId;
            IVsMultiItemSelect multiItemSelect;

            var monitorSelection = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
            monitorSelection.GetCurrentSelection(out hierarchyPtr, out projectItemId, out multiItemSelect, out selectionContainerPtr);

            var hierarchy = Marshal.GetTypedObjectForIUnknown(hierarchyPtr, typeof(IVsHierarchy)) as IVsHierarchy;
            if (hierarchy == null) return null;

            string canonicalName;
            hierarchy.GetCanonicalName(projectItemId, out canonicalName);
            if (canonicalName == null) return null;

            return canonicalName.ToString();
        }
    }
}