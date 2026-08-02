using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Internship_WPF_Project.View.IP
{
    /// <summary>
    /// Interaction logic for ip.xaml
    /// </summary>
    public partial class ip : Window
    {
        public string Input_UserName { get; set; }
        public ip(Window parentWindow)
        {
            Owner = parentWindow;
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
