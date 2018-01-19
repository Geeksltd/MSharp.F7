using System;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Editor;
using System.Linq;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;

namespace MSharp.F7.ToggleHandler
{
    internal sealed class ViewportAdornment1
    {
        private const double AdornmentWidth = 120;

        private const double AdornmentHeight = 30;

        private const double TopMargin = 30;

        private const double RightMargin = 30;

        private readonly IWpfTextView view;

        private GotoButton goButton;

        private readonly IAdornmentLayer adornmentLayer;

        public ViewportAdornment1(IWpfTextView view,string relatedFilePath,string buttonText)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            this.view = view;

            this.adornmentLayer = view.GetAdornmentLayer("ViewportAdornment1");
            this.view.GotAggregateFocus += View_GotAggregateFocus;
            this.view.LayoutChanged += this.OnSizeChanged;

            goButton = new GotoButton(relatedFilePath,buttonText);
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            this.adornmentLayer.RemoveAllAdornments();

            Canvas.SetLeft(this.goButton, this.view.ViewportRight - RightMargin - 200);
            Canvas.SetTop(this.goButton, this.view.ViewportTop + TopMargin);

            this.adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, this.goButton, null);
        }

        private void View_GotAggregateFocus(object sender, EventArgs e)
        {
            //System.Windows.MessageBox.Show("focused");
            var currentDocument = ActiveDoc(this.view);

            var RelatedFilePath = "";
            var buttonVisible = false;
            var buttonText = "";
            try
            {
                if (currentDocument != null)
                    if (Toolbox.State == PageOrModule.Page && currentDocument.IsModuleOfWebCtrlPage() && currentDocument.IsMvcWebController())
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
                    goButton.RelatedFilePath = RelatedFilePath;
                    goButton.BtnCaption = buttonText;
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
