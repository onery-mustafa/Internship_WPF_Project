using System.Windows;
using Internship_WPF_Project.View.StartingWindow;

namespace Internship_WPF_Project
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Starting starting = new Starting();
            starting.Show();
            Close();
        }
    }
}