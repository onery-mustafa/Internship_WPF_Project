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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Internship_WPF_Project.View.SignIn
{
    /// <summary>
    /// Interaction logic for textbox.xaml
    /// </summary>
    public partial class textbox : UserControl
    {
        public textbox()
        {
            InitializeComponent();
        }

        private string placeholder;

        // İçerideki txtInput'un metnine dışarıdan ulaşmamızı sağlar
        public string Text
        {
            get { return txtInput.Text; }
        }

        public string Placeholder
        {
            get { return placeholder; }
            set
            {
                placeholder = value;
                tbPlaceHolder.Text = placeholder; // Yer tutucu metnini güncelle

            }
        }

        private void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtInput.Text))
            {
                tbPlaceHolder.Visibility = Visibility.Visible; // TextBox boşsa yer tutucu metnini göster
            }
            else
            {
                tbPlaceHolder.Visibility = Visibility.Hidden; // TextBox doluysa yer tutucu metnini gizle
            }
        }

        private void btnClearClick(object sender, RoutedEventArgs e)
        {
            txtInput.Clear();
            txtInput.Focus(); // kutu temizlendikten sonra imleci tekrar kutuya odaklamak için
        }
    }
}
