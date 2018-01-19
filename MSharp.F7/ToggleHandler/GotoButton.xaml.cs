using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MSharp.F7.ToggleHandler
{
    /// <summary>
    /// Interaction logic for GotoButton.xaml
    /// </summary>
    public partial class GotoButton : UserControl
    {
        private string RelatedFilePath;
        public GotoButton(string relatedFilePath,string btnCaption)
        {
            InitializeComponent();
            this.RelatedFilePath = relatedFilePath;
            this.GoButton.Content = btnCaption;
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.DTE.ItemOperations.OpenFile(RelatedFilePath);
            }
            catch (Exception err)
            {
                StackTrace st = new StackTrace(err);
                Debug.WriteLine(st.ToString());
            }
        }
    }
}
