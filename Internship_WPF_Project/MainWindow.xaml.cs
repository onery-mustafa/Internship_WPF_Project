using System.Windows;
using System.ComponentModel;
using Internship_WPF_Project.View.SignIn;
using Internship_WPF_Project.View.IP;

using System.Windows.Threading; // Timer için gerekli
using System; // Array işlemleri için gerekli

using FRRJIf;

namespace Internship_WPF_Project
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // --- FANUC NESNELERİ ---
        private FRRJIf.Core mobjCore;
        private FRRJIf.DataTable mobjDataTable;
        private FRRJIf.DataCurPos mobjCurPos;
        private FRRJIf.DataSysVar mobjSpeedVar; // Hız barı için sistem değişkeni okuyucu

        // --- WPF TIMER ---
        private DispatcherTimer refreshTimer;

        private double x_pos;
        public double X_Pos { get { return x_pos; } set { x_pos = value; OnPropertyChanged("X_Pos"); } }
        private double y_pos;
        public double Y_Pos { get { return y_pos; } set { y_pos = value; OnPropertyChanged("Y_Pos"); } }
        // Not: Z, W, P, R ve J1...J6 için de aynı property'leri yukarıdaki gibi tanımlamalısın.

        private double z_pos;
        public double Z_Pos { get { return z_pos; } set { z_pos = value; OnPropertyChanged("Z_Pos"); } }

        private double w_pos;
        public double W_Pos { get { return w_pos; } set { w_pos = value; OnPropertyChanged("W_Pos"); } }

        private double p_pos;
        public double P_Pos { get { return p_pos; } set { p_pos = value; OnPropertyChanged("P_Pos"); } }

        private double r_pos;
        public double R_Pos { get { return r_pos; } set { r_pos = value; OnPropertyChanged("R_Pos"); } }


        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            SetupTimer();
        }

        private void SetupTimer()
        {
            // Robottan verileri saniyede 10 kez (100ms) okuyacak WPF zamanlayıcısı
            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromMilliseconds(100);
            refreshTimer.Tick += RefreshTimer_Tick;
        }

        public void ConnectToRobot(string ipAddress)
        {
            try
            {
                // 1. Nesne Oluşturma
                mobjCore = new FRRJIf.Core();
                mobjDataTable = mobjCore.DataTable;

                // 2. Data Table'a Okunacak Verileri Kaydetme (Bağlanmadan önce yapılmalı!)
                mobjCurPos = mobjDataTable.AddCurPos(FRRJIf.FRIF_DATA_TYPE.CURPOS, 1); // Group 1 pozisyonları
                mobjSpeedVar = mobjDataTable.AddSysVar(FRRJIf.FRIF_DATA_TYPE.SYSVAR_INT, "$MCR.$GENOVERRIDE"); // Genel Hız

                // 3. Bağlantıyı Kurma
                if (mobjCore.Connect(ipAddress))
                {
                    MessageBox.Show(ipAddress + " adresine başarıyla bağlanıldı!", "Bağlantı Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    refreshTimer.Start(); // Bağlantı başarılıysa veri okumayı başlat
                }
                else
                {
                    MessageBox.Show("Bağlantı kurulamadı. Roboguide IP'sini kontrol edin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Haberleşme Hatası: " + ex.Message);
            }
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (mobjCore == null || mobjDataTable == null) return;

            // 1. Robottan paket halinde son verileri iste
            if (!mobjDataTable.Refresh())
            {
                refreshTimer.Stop();
                MessageBox.Show("Bağlantı koptu!", "Uyarı");
                return;
            }

            // 2. Gelen verileri ayıkla
            Array xyzwpr = new float[9];
            Array config = new short[7];
            Array joint = new float[9];
            short intUF = 0, intUT = 0, intValidC = 0, intValidJ = 0;

            if (mobjCurPos.GetValue(ref xyzwpr, ref config, ref joint, ref intUF, ref intUT, ref intValidC, ref intValidJ))
            {
                // Arayüzdeki (UI) Binding özelliklerini güncelle
                if (intValidC != 0) // Kartezyen pozisyonlar geçerliyse
                {
                    X_Pos = (float)xyzwpr.GetValue(0);
                    Y_Pos = (float)xyzwpr.GetValue(1);
                    Z_Pos = (float)xyzwpr.GetValue(2);
                    W_Pos = (float)xyzwpr.GetValue(3);
                    P_Pos = (float)xyzwpr.GetValue(4);
                    R_Pos = (float)xyzwpr.GetValue(5);
                    // Z_Pos = (float)xyzwpr.GetValue(2); vs...
                }

                if (intValidJ != 0) // Eklem (Joint) pozisyonları geçerliyse
                {
                    // J1_Pos = (float)joint.GetValue(0); vs...
                }
            }
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
            signin signin = new signin(this);
            Opacity = 0.4;
            signin.ShowDialog();
            Opacity = 1;
        }

        private void btnIP_Click(object sender, RoutedEventArgs e)
        {
            ip ip = new ip(this);
            Opacity = 0.4;
            ip.ShowDialog();
            Opacity = 1;

            btnIP.Content = ip.InputIP;
            ConnectToRobot(ip.InputIP);
        }
    }
}