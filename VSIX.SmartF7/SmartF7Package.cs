using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;

namespace Geeks.GeeksProductivityTools
{
    [ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")]    // Microsoft.VisualStudio.VSConstants.UICONTEXT_NoSolution
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionsPage), "Geeks productivity tools", "General", 0, 0, true)]
    [Guid(GuidList.GuidGeeksProductivityToolsPkgString)]
    public sealed class GeeksProductivityToolsPackage : Package
    {
        public GeeksProductivityToolsPackage() { }

        // Strongly reference events so that it's not GC'd
        EnvDTE.DocumentEvents docEvents;
        EnvDTE.SolutionEvents solEvents;
        EnvDTE.Events events;

        public static GeeksProductivityToolsPackage Instance { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();
            App.Initialize(GetDialogPage(typeof(OptionsPage)) as OptionsPage);

            Instance = this;

            var componentModel = (IComponentModel)GetService(typeof(SComponentModel));

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var menuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (null != menuCommandService)
            {
                // var mainMenu = new CommandID(GuidList.GuidGeeksProductivityToolsCmdSet, (int)PkgCmdIDList.CmdidMainMenu);
                // var founded = menuCommandService.FindCommand(mainMenu);
                // if (founded == null)
                // {
                //    var menuCommand2 = new OleMenuCommand(null, mainMenu);
                //    menuCommandService.AddCommand(menuCommand2);
                //    menuCommand2.BeforeQueryStatus += MenuCommand2_BeforeQueryStatus;
                // }
                // Set up menu items

                new Menus.Typescript(menuCommandService).SetupCommands();
                new Menus.RunBatchFiles(menuCommandService).SetupCommands();

                new Menus.OpenRelatedFileF7(menuCommandService).SetupCommands();
            }

            // Hook up event handlers
            events = App.DTE.Events;
            docEvents = events.DocumentEvents;
            solEvents = events.SolutionEvents;
            docEvents.DocumentSaved += DocumentEvents_DocumentSaved;
            solEvents.Opened += delegate { App.Initialize(GetDialogPage(typeof(OptionsPage)) as OptionsPage); };
        }
        // private void MenuCommand2_BeforeQueryStatus(object sender, EventArgs e)
        // {

        //    var cmd = sender as OleMenuCommand;
        // }

        void DocumentEvents_DocumentSaved(EnvDTE.Document document)
        {
            try
            {
                if (document.Name.EndsWith(".cs") ||
                    document.Name.EndsWith(".css") ||
                    document.Name.EndsWith(".js") ||
                    document.Name.EndsWith(".ts"))
                {
                    document.DTE.ExecuteCommand("Edit.FormatDocument");
                }

                if (!document.Saved) document.Save();

                Menus.Typescript.OnDocumentSaved(document);
            }
            catch
            {
            }
        }
    }
}