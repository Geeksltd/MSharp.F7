using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;

namespace Geeks.GeeksProductivityTools.Menus.OpenInMSharp
{
    public class OpenInMSharpCodeWindow : OpenInMSharpHandler
    {
        OleMenuCommandService Menu;

        public OpenInMSharpCodeWindow(OleMenuCommandService menu)
        {
            Menu = menu;
        }

        public void SetupCommands()
        {
            var menuCommandID = new CommandID(GuidList.GuidMSharpEditorCmdSet, (int)PkgCmdIDList.CmdOpenInMSharp);
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
                System.Windows.Forms.MessageBox.Show(err.Message);
            }
        }

        void OpenInMSharpMenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            var cmd = sender as OleMenuCommand;
            var activeDoc = App.DTE.ActiveDocument;

            if (null != cmd && activeDoc != null)
            {
                cmd.Visible = IsItemVisible(activeDoc.FullName.ToUpper());
            }
        }
    }
}
