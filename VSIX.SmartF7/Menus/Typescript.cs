using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Geeks.GeeksProductivityTools.TypeScript;
using Microsoft.VisualStudio.Shell;

namespace Geeks.GeeksProductivityTools.Menus
{
    public class Typescript
    {
        OleMenuCommandService Menu;

        public Typescript(OleMenuCommandService menu)
        {
            Menu = menu;
        }

        public void SetupCommands()
        {
            var menuCommandID = new CommandID(GuidList.GuidCompileTsCmdSet, (int)PkgCmdIDList.CmdCompileTsFiles);
            var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
            menuItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;
            Menu.AddCommand(menuItem);
        }

        void MenuItemCallback(object sender, EventArgs e)
        {
            App.Settings.Load();

            IEnumerable<string> tsFiles = TypeScriptHelper.GetSelectedTypescriptFiles();

            // If contains, remove from settings...
            if (App.Settings.Typescript.RootFiles.Any(f => tsFiles.Contains(f)))
            {
                App.Settings.Typescript.RootFiles = App.Settings.Typescript.RootFiles.Except(tsFiles).ToArray();
                App.Settings.Save();
            }
            else // else add to settings
            {
                App.Settings.Typescript.RootFiles = App.Settings.Typescript.RootFiles.Concat(tsFiles).Distinct().ToArray();
                App.Settings.Save();

                tsFiles = tsFiles.Select(f => TypeScriptHelper.GetFullPathFromRelativePath(f));
                System.Threading.Tasks.Task.Run(() =>
                {
                    Geeks.GeeksProductivityTools.TypeScript.TypescriptCompiler.Compile(tsFiles);
                });
            }
        }

        void menuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            // get the menu that fired the event
            var menuCommand = sender as OleMenuCommand;

            // disable - not needed anymore
            menuCommand.Visible = false;
            menuCommand.Enabled = false;
            return;

            if (menuCommand != null)
            {
                // ############# Visibility #############
                // start by assuming that the menu will not be shown
                menuCommand.Visible = false;
                menuCommand.Enabled = false;

                IEnumerable<string> tsFiles = TypeScriptHelper.GetSelectedTypescriptFiles();
                if (!tsFiles.Any()) return;

                menuCommand.Visible = true;
                menuCommand.Enabled = true;

                // ############# Text #############

                if (App.Settings.Typescript.RootFiles.Any(f => tsFiles.Contains(f)))
                    menuCommand.Text = "Stop combining JS";
                else
                    menuCommand.Text = "Combine JS files";
            }
        }

        public static void OnDocumentSaved(EnvDTE.Document Document)
        {
            // handle typescript saved files
            if (!TypeScriptHelper.IsValidTsFile(Document.Name))
                return;

            var filesToCompile = TypeScriptHelper.RootFilesToCompileFromFile(Document.FullName);
            System.Threading.Tasks.Task.Run(() =>
            {
                Geeks.GeeksProductivityTools.TypeScript.TypescriptCompiler.Compile(filesToCompile);
            });
        }
    }
}
