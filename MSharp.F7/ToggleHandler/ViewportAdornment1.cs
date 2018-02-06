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

        private const double TopMargin = 0;

        private const double RightMargin = 230;

        private readonly IWpfTextView view;

        private GotoButton goButton;

        private readonly IAdornmentLayer adornmentLayer;

        public ViewportAdornment1(IWpfTextView view,string docType, string relatedFilePath1,bool ShowRelatedFilePath1, string relatedFilePath2, bool ShowRelatedFilePath2, string relatedFilePath3, bool ShowRelatedFilePath3)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            this.view = view;

            this.adornmentLayer = view.GetAdornmentLayer("ViewportAdornment1");
            this.view.GotAggregateFocus += View_GotAggregateFocus;
            this.view.LayoutChanged += this.OnSizeChanged;

            goButton = new GotoButton(relatedFilePath1, relatedFilePath2, relatedFilePath3);
            goButton.DocType = docType;
            goButton.ShowButton1 = ShowRelatedFilePath1;
            goButton.ShowButton2 = ShowRelatedFilePath2;
            goButton.ShowButton3 = ShowRelatedFilePath3;
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            this.adornmentLayer.RemoveAllAdornments();

            Canvas.SetLeft(this.goButton, this.view.ViewportRight - RightMargin - 100);
            Canvas.SetTop(this.goButton, this.view.ViewportTop + TopMargin);

            this.adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, this.goButton, null);
        }

        private void View_GotAggregateFocus(object sender, EventArgs e)
        {
            var currentDocument = Toolbox.ActiveDoc(this.view).ProjectItem;

            string RelatedFilePath1 = "", RelatedFilePath2 = "", RelatedFilePath3 = "";
            bool ShowRelatedFilePath1 = true, ShowRelatedFilePath2 = true, ShowRelatedFilePath3 = true;
            var buttonVisible = false;
            try
            {
                if (currentDocument != null)
                    if (Toolbox.State == PageOrModule.Page && currentDocument.IsModuleOfWebCtrlPage() && currentDocument.IsMvcWebController())
                    {
                        buttonVisible = true;
                        currentDocument.GetSiblingMvcOfWebController(ref RelatedFilePath1, ref RelatedFilePath3);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath3 = RelatedFilePath3.Length != 0;
                    }
                    else if (Toolbox.State == PageOrModule.Page && currentDocument.IsModuleOfWebViewPage() && currentDocument.IsMvcWebView())
                    {
                        buttonVisible = true;
                        currentDocument.GetSiblingMvcOfWebView(ref RelatedFilePath1, ref RelatedFilePath2);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath2 = RelatedFilePath2.Length != 0;
                    }
                    else if (currentDocument.IsModuleOfUI())
                    {
                        buttonVisible = true;
                        currentDocument.GetSiblingModuleOfUI(ref RelatedFilePath2, ref RelatedFilePath3);
                        ShowRelatedFilePath2 = RelatedFilePath2.Length != 0;
                        ShowRelatedFilePath3 = RelatedFilePath3.Length != 0;
                        Toolbox.State = PageOrModule.None;
                    }
                    else if (currentDocument.IsModuleOfWebCtrlModule())
                    {
                        buttonVisible = true;
                        currentDocument.GetSiblingModuleOfWebCtrlModule(ref RelatedFilePath1, ref RelatedFilePath3);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath3 = RelatedFilePath3.Length != 0;
                        Toolbox.State = PageOrModule.None;
                    }
                    else if (currentDocument.IsModuleOfWebCtrlPage())
                    {
                        buttonVisible = true;
                        currentDocument.GetSiblingModuleOfWebCtrlPage(ref RelatedFilePath1, ref RelatedFilePath3);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath3 = RelatedFilePath3.Length != 0;
                        Toolbox.State = PageOrModule.None;
                    }
                    else if (currentDocument.IsModuleOfWebVeiwModule())
                    {
                        buttonVisible = true;
                        currentDocument.GetSiblingModuleOfWebViewModule(ref RelatedFilePath1, ref RelatedFilePath2);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath2 = RelatedFilePath2.Length != 0;
                        Toolbox.State = PageOrModule.None;
                    }
                    else if (currentDocument.IsModuleOfWebViewPage())
                    {
                        buttonVisible = true;
                        currentDocument.GetSiblingModuleOfWebViewPage(ref RelatedFilePath1, ref RelatedFilePath2);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath2 = RelatedFilePath2.Length != 0;
                        Toolbox.State = PageOrModule.None;
                    }
                    else if (currentDocument.IsMvcUIPage())
                    {
                        buttonVisible = true;
                        currentDocument.GetSiblingMvcOfUIPage(ref RelatedFilePath2, ref RelatedFilePath3);
                        ShowRelatedFilePath2 = RelatedFilePath2.Length != 0;
                        ShowRelatedFilePath3 = RelatedFilePath3.Length != 0;
                        Toolbox.State = PageOrModule.Page;
                    }
                    else if (currentDocument.IsMvcWebController())
                    {
                        buttonVisible = true;
                        currentDocument.GetSiblingMvcOfWebController(ref RelatedFilePath1, ref RelatedFilePath3);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath3 = RelatedFilePath3.Length != 0;
                        Toolbox.State = PageOrModule.None;
                    }
                    else if (currentDocument.IsMvcWebView())
                    {
                        buttonVisible = true;
                        currentDocument.GetSiblingMvcOfWebView(ref RelatedFilePath1, ref RelatedFilePath2);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath2 = RelatedFilePath2.Length != 0;
                        Toolbox.State = PageOrModule.None;
                    }
                if (buttonVisible)
                {
                    goButton.RelatedFilePath1 = RelatedFilePath1;
                    goButton.RelatedFilePath2 = RelatedFilePath2;
                    goButton.RelatedFilePath3 = RelatedFilePath3;
                    goButton.ShowButton1 = ShowRelatedFilePath1;
                    goButton.ShowButton2 = ShowRelatedFilePath2;
                    goButton.ShowButton3 = ShowRelatedFilePath3;
                }
            }
            catch (Exception err)
            {
                StackTrace st = new StackTrace(err);
                Debug.WriteLine(st.ToString());
            }
        }
    }
}
