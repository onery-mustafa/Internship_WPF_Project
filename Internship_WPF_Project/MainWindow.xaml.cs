using System.Windows;
using System.ComponentModel;


namespace Internship_WPF_Project
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        private int speedValue;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int SpeedValue
        {
            get { return speedValue; }

            set
            {
                speedValue = value;
                OnPropertyChanged("SpeedValue");
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
        }


    }
}