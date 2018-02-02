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
        public string DocType
        {
            set
            {
                switch (value)
                {
                    case "entity":
                        GoButton1.Content = "M#";
                        GoButton2.Content = "Entity";
                        GoButton3.Content = "Logic";
                        break;
                    default:
                        GoButton1.Content = "M#";
                        GoButton2.Content = "Controller";
                        GoButton3.Content = "View";
                        break;
                }
            }
        }

        private string relatedFilePath1;
        public string RelatedFilePath1
        {
            set
            {
                relatedFilePath1 = value;
                if (relatedFilePath1.Length == 0)
                {
                    GoButton1.IsEnabled = false;
                }
                else
                    GoButton1.IsEnabled = true;
            }
        }

        private string relatedFilePath2;
        public string RelatedFilePath2
        {
            set
            {
                relatedFilePath2 = value;
                if (relatedFilePath2.Length == 0)
                {
                    GoButton2.IsEnabled = false;
                }
                else
                    GoButton2.IsEnabled = true;
            }
        }

        private string relatedFilePath3;
        public string RelatedFilePath3
        {
            set
            {
                relatedFilePath3 = value;
                if (relatedFilePath3.Length == 0)
                {
                    GoButton3.IsEnabled = false;
                }
                else
                    GoButton3.IsEnabled = true;
            }
        }

        public bool ShowButton1
        {
            set
            {
                if (value)
                {
                    GoButton1.Visibility = Visibility.Visible;
                }
                else
                {
                    GoButton1.Visibility = Visibility.Collapsed;
                }
            }
        }
        public bool ShowButton2
        {
            set
            {
                if (value)
                {
                    GoButton2.Visibility = Visibility.Visible;
                }
                else
                {
                    GoButton2.Visibility = Visibility.Collapsed;
                }
            }
        }
        public bool ShowButton3
        {
            set
            {
                if (value)
                {
                    GoButton3.Visibility = Visibility.Visible;
                }
                else
                {
                    GoButton3.Visibility = Visibility.Collapsed;
                }
            }
        }

        public GotoButton(string relatedFilePath1, string relatedFilePath2, string relatedFilePath3)
        {
            InitializeComponent();
            RelatedFilePath1 = relatedFilePath1;
            
            RelatedFilePath2 = relatedFilePath2;
           
            RelatedFilePath3 = relatedFilePath3;
        }

        private void GoButton1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (relatedFilePath1.Length > 0)
                    App.DTE.ItemOperations.OpenFile(relatedFilePath1);
            }
            catch (Exception err)
            {
                StackTrace st = new StackTrace(err);
                Debug.WriteLine(st.ToString());
            }
        }

        private void GoButton2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (relatedFilePath2.Length > 0)
                    App.DTE.ItemOperations.OpenFile(relatedFilePath2);
            }
            catch (Exception err)
            {
                StackTrace st = new StackTrace(err);
                Debug.WriteLine(st.ToString());
            }
        }

        private void GoButton3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (relatedFilePath3.Length > 0)
                    App.DTE.ItemOperations.OpenFile(relatedFilePath3);
            }
            catch (Exception err)
            {
                StackTrace st = new StackTrace(err);
                Debug.WriteLine(st.ToString());
            }
        }
    }
}
