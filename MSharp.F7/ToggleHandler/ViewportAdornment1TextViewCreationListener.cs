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


        public void TextViewCreated(IWpfTextView textView)
        {
            if (textView == null)
                return;
            string RelatedFilePath1 = "", RelatedFilePath2 = "", RelatedFilePath3 = "";
            bool ShowRelatedFilePath1 = true, ShowRelatedFilePath2 = true, ShowRelatedFilePath3 = true;
            var buttonVisible = false;
            var DocType = "";
            try
            {
                var currentDocument = Toolbox.ActiveDoc(textView).ProjectItem;
                if (currentDocument != null)
                { 
                    if (currentDocument.IsEntityOfModel())
                    {
                        DocType = "entity";
                        buttonVisible = true;
                        Toolbox.State = PageOrModule.None;
                        currentDocument.GetSiblingEntityOfModel(ref RelatedFilePath2,ref RelatedFilePath3);
                        ShowRelatedFilePath2 = RelatedFilePath2.Length != 0;
                        ShowRelatedFilePath3 = RelatedFilePath3.Length != 0;
                    }
                    else if (currentDocument.IsEntityOfDomainEntity())
                    {
                        DocType = "entity";
                        buttonVisible = true;
                        Toolbox.State = PageOrModule.None;
                        currentDocument.GetSiblingEntityOfDomainEntity(ref RelatedFilePath1,ref RelatedFilePath3);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath3 = RelatedFilePath3.Length != 0;
                    }
                    else if (currentDocument.IsEntityOfDomainLogic())
                    {
                        DocType = "entity";
                        buttonVisible = true;
                        Toolbox.State = PageOrModule.None;
                        currentDocument.GetSiblingEntityOfDomainLogic(ref RelatedFilePath1,ref RelatedFilePath2);
                        ShowRelatedFilePath2 = RelatedFilePath2.Length != 0;
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                    }
                    else if (currentDocument.IsComponentOfUI())
                    {
                        DocType = "component";
                        buttonVisible = true;
                        Toolbox.State = PageOrModule.None;
                        currentDocument.GetSiblingComponentFromUI(ref RelatedFilePath2, ref RelatedFilePath3);
                        ShowRelatedFilePath2 = RelatedFilePath2.Length != 0;
                        ShowRelatedFilePath3 = RelatedFilePath3.Length != 0;
                    }
                    else if (currentDocument.IsComponentOfWebCtrl())
                    {
                        DocType = "component";
                        buttonVisible = true;
                        Toolbox.State = PageOrModule.None;
                        currentDocument.GetSiblingComponentFromCtrl(ref RelatedFilePath1, ref RelatedFilePath3);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath3 = RelatedFilePath3.Length != 0;
                    }
                    else if (currentDocument.IsComponentOfWebView())
                    {
                        DocType = "component";
                        buttonVisible = true;
                        Toolbox.State = PageOrModule.None;
                        currentDocument.GetSiblingComponentFromView(ref RelatedFilePath1, ref RelatedFilePath2);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath2 = RelatedFilePath2.Length != 0;
                    }
                    else if (Toolbox.State == PageOrModule.Page && currentDocument.IsModuleOfWebCtrlPage() && currentDocument.IsMvcWebController())
                    {
                        DocType = "mvc";
                        buttonVisible = true;
                        currentDocument.GetSiblingMvcOfWebController(ref RelatedFilePath1, ref RelatedFilePath3);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath3 = RelatedFilePath3.Length != 0;
                    }
                    else if (Toolbox.State == PageOrModule.Page && currentDocument.IsModuleOfWebViewPage() && currentDocument.IsMvcWebView())
                    {
                        DocType = "mvc";
                        buttonVisible = true;
                        currentDocument.GetSiblingMvcOfWebView(ref RelatedFilePath1, ref RelatedFilePath2);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath2 = RelatedFilePath2.Length != 0;
                    }
                    else if (currentDocument.IsModuleOfUI())
                    {
                        DocType = "module";
                        buttonVisible = true;
                        currentDocument.GetSiblingModuleOfUI(ref RelatedFilePath2, ref RelatedFilePath3);
                        ShowRelatedFilePath2 = RelatedFilePath2.Length != 0;
                        ShowRelatedFilePath3 = RelatedFilePath3.Length != 0;
                        Toolbox.State = PageOrModule.None;
                    }
                    else if (currentDocument.IsModuleOfWebCtrlModule())
                    {
                        DocType = "module";
                        buttonVisible = true;
                        currentDocument.GetSiblingModuleOfWebCtrlModule(ref RelatedFilePath1, ref RelatedFilePath3);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath3 = RelatedFilePath3.Length != 0;
                        Toolbox.State = PageOrModule.None;
                    }
                    else if (currentDocument.IsModuleOfWebCtrlPage())
                    {
                        DocType = "module";
                        buttonVisible = true;
                        currentDocument.GetSiblingModuleOfWebCtrlPage(ref RelatedFilePath1, ref RelatedFilePath3);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath3 = RelatedFilePath3.Length != 0;
                        Toolbox.State = PageOrModule.None;
                    }
                    else if (currentDocument.IsModuleOfWebVeiwModule())
                    {
                        DocType = "module";
                        buttonVisible = true;
                        currentDocument.GetSiblingModuleOfWebViewModule(ref RelatedFilePath1, ref RelatedFilePath2);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath2 = RelatedFilePath2.Length != 0;
                        Toolbox.State = PageOrModule.None;
                    }
                    else if (currentDocument.IsModuleOfWebViewPage())
                    {
                        DocType = "module";
                        buttonVisible = true;
                        currentDocument.GetSiblingModuleOfWebViewPage(ref RelatedFilePath1, ref RelatedFilePath2);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath2 = RelatedFilePath2.Length != 0;
                        Toolbox.State = PageOrModule.None;
                    }
                    else if (currentDocument.IsMvcUIPage())
                    {
                        DocType = "mvc";
                        buttonVisible = true;
                        currentDocument.GetSiblingMvcOfUIPage(ref RelatedFilePath2, ref RelatedFilePath3);
                        ShowRelatedFilePath2 = RelatedFilePath2.Length != 0;
                        ShowRelatedFilePath3 = RelatedFilePath3.Length != 0;
                        Toolbox.State = PageOrModule.Page;
                    }
                    else if (currentDocument.IsMvcWebController())
                    {
                        DocType = "mvc";
                        buttonVisible = true;
                        currentDocument.GetSiblingMvcOfWebController(ref RelatedFilePath1, ref RelatedFilePath3);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath3 = RelatedFilePath3.Length != 0;
                        Toolbox.State = PageOrModule.None;
                    }
                    else if (currentDocument.IsMvcWebView())
                    {
                        DocType = "mvc";
                        buttonVisible = true;
                        currentDocument.GetSiblingMvcOfWebView(ref RelatedFilePath1, ref RelatedFilePath2);
                        ShowRelatedFilePath1 = RelatedFilePath1.Length != 0;
                        ShowRelatedFilePath2 = RelatedFilePath2.Length != 0;
                        Toolbox.State = PageOrModule.None;
                    }

                    if (buttonVisible)
                    {
                        new ViewportAdornment1(textView,DocType, RelatedFilePath1, ShowRelatedFilePath1, RelatedFilePath2, ShowRelatedFilePath2, RelatedFilePath3, ShowRelatedFilePath3);
                    }
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
