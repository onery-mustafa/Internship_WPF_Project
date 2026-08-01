using System.Windows;
using System.ComponentModel;
using Internship_WPF_Project.View.SignIn;

namespace Internship_WPF_Project
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        private GridLength speedValue;
        private double speedValue_percent;

        public event PropertyChangedEventHandler? PropertyChanged;

        public GridLength SpeedValue
        {
            get { return speedValue; }

            set
            {
                speedValue = value;
                OnPropertyChanged("SpeedValue");
                SpeedValue_percent = SpeedValue.Value;
            }

        }

        public double SpeedValue_percent
        {
            get { return speedValue_percent; }

            set
            {
                speedValue_percent = value;
                speedValue_percent = speedValue_percent / 140 * 100;  
                OnPropertyChanged("SpeedValue_percent");
            }

        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButton(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //SpeedValue.Value = SpeedValue.Value / 140 * 100;  
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            signin signin = new signin();
            signin.ShowDialog();
        }

        private void btnIP_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}