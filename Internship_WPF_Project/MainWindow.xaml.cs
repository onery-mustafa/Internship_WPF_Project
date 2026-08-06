using System.Windows;
using System.ComponentModel;
using Internship_WPF_Project.View.SignIn;
using Internship_WPF_Project.View.IP;

using System.Windows.Threading; // Timer için


namespace Internship_WPF_Project
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Fanuc nesneleri
        private FRRJIf.Core mobjCore;
        private FRRJIf.DataTable mobjDataTable;
        private FRRJIf.DataCurPos mobjCurPos;
        private FRRJIf.DataSysVar mobjSpeedVar; // hız

        private bool communicationState;
        private bool signinState;



        private DispatcherTimer refreshTimer; // timer

        private double x_pos;
        public double X_Pos { get { return x_pos; } set { x_pos = value; OnPropertyChanged("X_Pos"); } }
        private double y_pos;
        public double Y_Pos { get { return y_pos; } set { y_pos = value; OnPropertyChanged("Y_Pos"); } }

        private double z_pos;
        public double Z_Pos { get { return z_pos; } set { z_pos = value; OnPropertyChanged("Z_Pos"); } }

        private double w_pos;
        public double W_Pos { get { return w_pos; } set { w_pos = value; OnPropertyChanged("W_Pos"); } }

        private double p_pos;
        public double P_Pos { get { return p_pos; } set { p_pos = value; OnPropertyChanged("P_Pos"); } }

        private double r_pos;
        public double R_Pos { get { return r_pos; } set { r_pos = value; OnPropertyChanged("R_Pos"); } }

        private double j1_pos;
        public double J1_Pos { get { return j1_pos; } set { j1_pos = value; OnPropertyChanged("J1_Pos"); } }

        private double j2_pos;
        public double J2_Pos { get { return j2_pos; } set { j2_pos = value; OnPropertyChanged("J2_Pos"); } }

        private double j3_pos;
        public double J3_Pos { get { return j3_pos; } set { j3_pos = value; OnPropertyChanged("J3_Pos"); } }

        private double j4_pos;
        public double J4_Pos { get { return j4_pos; } set { j4_pos = value; OnPropertyChanged("J4_Pos"); } }

        private double j5_pos;
        public double J5_Pos { get { return j5_pos; } set { j5_pos = value; OnPropertyChanged("J5_Pos"); } }

        private double j6_pos;
        public double J6_Pos { get { return j6_pos; } set { j6_pos = value; OnPropertyChanged("J6_Pos"); } }



        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            SetupTimer();
        }

        private void SetupTimer()
        {
            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromMilliseconds(10);
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
                mobjCurPos = mobjDataTable.AddCurPos(FRRJIf.FRIF_DATA_TYPE.CURPOS, 1);
                mobjSpeedVar = mobjDataTable.AddSysVar(FRRJIf.FRIF_DATA_TYPE.SYSVAR_INT, "$MCR.$GENOVERRIDE"); // hız



                // 3. Bağlantıyı Kurma
                if (mobjCore.Connect(ipAddress))
                {
                    communicationState = true;
                    btnIP.Background = System.Windows.Media.Brushes.Green;
                    btnIP.Content = "Connected";
                    txtIP.Text = ipAddress;
                    MessageBox.Show(ipAddress + " Successfully connected to the address!", "Connection Successful", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    refreshTimer.Start(); // Bağlantı başarılıysa veri okumayı başlat
                }
                else
                {
                    X_Pos = 0; Y_Pos = 0; Z_Pos = 0; W_Pos = 0; P_Pos = 0; R_Pos = 0;
                    J1_Pos = 0; J2_Pos = 0; J3_Pos = 0; J4_Pos = 0; J5_Pos = 0; J6_Pos = 0;
                    communicationState = false;
                    btnIP.Content = "Disconnected";
                    btnIP.Background = System.Windows.Media.Brushes.Red;
                    txtIP.Text = "No Found";
                    MessageBox.Show("Connection failed. Check Roboguide's IP address.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Communication Error: " + ex.Message);
            }
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)  // pozisyonları güncellemek için çağrılan fonksiyon
        {
            if (mobjCore == null || mobjDataTable == null) return;

            // 1. Robottan paket halinde son verileri iste
            if (!mobjDataTable.Refresh())
            {
                communicationState = false;
                btnIP.Content = "Disconnected";
                btnIP.Background = System.Windows.Media.Brushes.Red;
                txtIP.Text = "No Found";
                X_Pos = 0; Y_Pos = 0; Z_Pos = 0; W_Pos = 0; P_Pos = 0; R_Pos = 0;
                J1_Pos = 0; J2_Pos = 0; J3_Pos = 0; J4_Pos = 0; J5_Pos = 0; J6_Pos = 0;
                refreshTimer.Stop();
                MessageBox.Show("Connection lost!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    J1_Pos = (float)joint.GetValue(0);
                    J2_Pos = (float)joint.GetValue(1);
                    J3_Pos = (float)joint.GetValue(2);
                    J4_Pos = (float)joint.GetValue(3);
                    J5_Pos = (float)joint.GetValue(4);
                    J6_Pos = (float)joint.GetValue(5);

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

                if (communicationState) mobjSpeedVar.SetValue((int)SpeedValue_percent);
                else MessageBox.Show("No connection! Speed setting is not possible.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            if (signinState)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to sign out?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
                else
                {
                    signinState = false;
                    btnSignIn.Content = "Sign In";
                    return;
                }
            }

            Opacity = 0.4;
            signin.ShowDialog();
            Opacity = 1;

            if (signin.txtUserName.Text == "Admin" && signin.txtPassword.Text == "1234")
            {
                signinState = true;
                btnSignIn.Content = signin.txtUserName.Text;
                MessageBox.Show("Login successful!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                signinState = false;
                btnSignIn.Content = "Sign In";
                MessageBox.Show("Invalid username or password!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
        }

        private void btnIP_Click(object sender, RoutedEventArgs e)
        {
            ip ip = new ip(this);
            if (communicationState)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to terminate the connection?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    mobjCore.Disconnect();
                    communicationState = false;
                    btnIP.Content = "Connect";
                    btnIP.Background = System.Windows.Media.Brushes.Transparent;
                    txtIP.Text = "No Found";
                    X_Pos = 0; Y_Pos = 0; Z_Pos = 0; W_Pos = 0; P_Pos = 0; R_Pos = 0;
                    J1_Pos = 0; J2_Pos = 0; J3_Pos = 0; J4_Pos = 0; J5_Pos = 0; J6_Pos = 0;
                    refreshTimer.Stop();

                    return;
                }
                else return;
            }

            
            if (!signinState)
            {
                MessageBox.Show("Please sign in first!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Opacity = 0.4;
            ip.ShowDialog();
            Opacity = 1;
            if (signinState)
            {
                if (!string.IsNullOrEmpty(ip.InputIP)) ConnectToRobot(ip.InputIP);
            }
            

            
            

            // btnIP.Content = ip.InputIP;
        }
    }
}





/*
 
 // hız set ve get:

private FRRJIf.DataSysVar mobjSysVarInt_GENERAL_OVERRIDE;

mobjSysVarInt_GENERAL_OVERRIDE = mobjDataTable.AddSysVar(FRRJIf.FRIF_DATA_TYPE.SYSVAR_INT, "$MCR.$GENOVERRIDE");

public bool SetOverride(int speed)
{
    var result = mobjSysVarInt_GENERAL_OVERRIDE.SetValue(speed);
    return result;
}

public int? GetOverride()
{
    object vntValue2 = null;
    if (mobjSysVarInt_GENERAL_OVERRIDE.GetValue(ref vntValue2) == true)
    {
        return Convert.ToInt32((int)vntValue2);
    }
    return null;
}

*/