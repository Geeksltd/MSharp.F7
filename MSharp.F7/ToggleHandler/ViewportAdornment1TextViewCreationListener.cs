using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.Linq;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace MSharp.F7.ToggleHandler
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class ViewportAdornment1TextViewCreationListener : IWpfTextViewCreationListener
    {
#pragma warning disable 649, 169

        [Export(typeof(AdornmentLayerDefinition))]
        [Name("ViewportAdornment1")]
        [Order(After = PredefinedAdornmentLayers.Caret)]
        private AdornmentLayerDefinition editorAdornmentLayer;

#pragma warning restore 649, 169
        //PageOrModule State = PageOrModule.None;


        public void TextViewCreated(IWpfTextView textView)
        {
            var currentDocument = ActiveDoc(textView);

            var RelatedFilePath = "";
            var buttonVisible = false;
            var buttonText = "";
            try
            {
                if (currentDocument != null)
                    if (currentDocument.IsEntityFile())
                    {
                        if (Toolbox.NextEntityFilePath(currentDocument, ref RelatedFilePath))
                        {
                            buttonVisible = true;
                            buttonText = "Go To Related Entity File";
                            Toolbox.State = PageOrModule.None;
                        }
                    }
                    else if (currentDocument.IsComponentFile())
                    {
                        if (Toolbox.NextComponentFilePath(currentDocument, ref RelatedFilePath))
                        {
                            buttonVisible = true;
                            buttonText = "Go To Related Component File";
                            Toolbox.State = PageOrModule.None;
                        }
                    }
                    else if (Toolbox.State == PageOrModule.Page && currentDocument.IsModuleOfWebCtrlPage() && currentDocument.IsMvcWebController())
                    {
                        if (Toolbox.NextMvcFilePath(currentDocument, ref RelatedFilePath, ref Toolbox.State))
                        { buttonVisible = true; buttonText = "Go To Related MVC Page"; }
                    }
                    else if (Toolbox.State == PageOrModule.Page && currentDocument.IsModuleOfWebViewPage() && currentDocument.IsMvcWebView())
                    {
                        if (Toolbox.NextMvcFilePath(App.DTE.ActiveDocument, ref RelatedFilePath, ref Toolbox.State)) { buttonVisible = true; buttonText = "Go To Related MVC Page"; }
                    }
                    else if (currentDocument.IsModuleFile())
                    {
                        if (Toolbox.NextModuleFilePath(currentDocument, ref RelatedFilePath, ref Toolbox.State))
                        {
                            buttonVisible = true;
                            buttonText = "Go To Related Module File";
                            Toolbox.State = PageOrModule.None;
                        }
                    }
                    else if (currentDocument.IsMvcFile())
                        if (Toolbox.NextMvcFilePath(currentDocument, ref RelatedFilePath, ref Toolbox.State))
                        {
                            buttonVisible = true;
                            buttonText = "Go To Related MVC Page";
                        }

                if (buttonVisible)
                {
                    new ViewportAdornment1(textView, RelatedFilePath, buttonText);
                }
            }
            catch (Exception err)
            {
                StackTrace st = new StackTrace(err);
                Debug.WriteLine(st.ToString());
            }
        }

        private EnvDTE.Document ActiveDoc(IWpfTextView textView)
        {
            var filePath = GetPath(textView);
            EnvDTE.Document doc = null;
            if (filePath != null)
            {
                doc = App.DTE.Documents.OfType<EnvDTE.Document>().FirstOrDefault(d => d.FullName.ToUpper() == filePath.ToUpper());
            }
            return doc;
        }

        private string GetPath(IWpfTextView textView)
        {
            textView.TextBuffer.Properties.TryGetProperty(typeof(IVsTextBuffer), out IVsTextBuffer bufferAdapter);
            var persistFileFormat = bufferAdapter as IPersistFileFormat;

            if (persistFileFormat == null)
            {
                return null;
            }
            persistFileFormat.GetCurFile(out string filePath, out _);
            return filePath;
        }
    }
}
