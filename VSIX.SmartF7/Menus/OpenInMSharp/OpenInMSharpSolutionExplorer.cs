using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;

namespace Geeks.GeeksProductivityTools.Menus.OpenInMSharp
{
    public class OpenInMSharpSolutionExplorer : OpenInMSharpHandler
    {
        OleMenuCommandService Menu;

        public OpenInMSharpSolutionExplorer(OleMenuCommandService menu)
        {
            Menu = menu;
        }

        public void SetupCommands()
        {
            var menuCommandID = new CommandID(GuidList.GuidOpenInMSharpSlnCmdSet, (int)PkgCmdIDList.CmdOpenInMSharpSln);
            var menuItem = new OleMenuCommand(OpenInMSharpMenuItemCallback, menuCommandID);
            menuItem.BeforeQueryStatus += OpenInMSharpMenuItem_BeforeQueryStatus;
            Menu.AddCommand(menuItem);
        }

        async void OpenInMSharpMenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                var url = await GeeksAddin.Utils.FindMSharpProjectUrl(App.DTE);
                var properPath = GetProperFilePathCapitalization(App.DTE.ActiveDocument.FullName);
                url = BuildCompleteUrl(url, properPath);
                Process.Start(url);
            }
            catch (Exception err)
            {
                ErrorNotification.EmailError(err);
            }
        }

        void OpenInMSharpMenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            var cmd = sender as OleMenuCommand;
            var activeDoc = App.DTE.ActiveDocument;

            if (null != cmd && activeDoc != null)
            {
                cmd.Visible = IsItemVisible(App.DTE.ActiveDocument.FullName.ToUpper());
            }
        }
    }
}
