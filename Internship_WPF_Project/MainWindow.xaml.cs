using Internship_WPF_Project.View.IP;
using Internship_WPF_Project.View.SignIn;
using System.Collections; // asenkron timer için. işlemler için. 
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading; // Timer için
using System.Collections; // array için


namespace Internship_WPF_Project
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Fanuc nesneleri
        private FRRJIf.Core mobjCore;
        private FRRJIf.DataTable mobjDataTable;
        private FRRJIf.DataCurPos mobjCurPos;
        private FRRJIf.DataSysVar mobjSpeedVar; // hız
        private FRRJIf.DataTask mobjTask; //program adını alma
        private FRRJIf.DataNumReg mobjNumReg; // numerik regiterlerı get ve set edebilmek için. 
        private FRRJIf.DataPosReg mobjPosReg; // PR' ları get ve set edebilmek için

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

        private short ufNum;
        public short UFNum { get { return ufNum; } set { ufNum = value; OnPropertyChanged("UFNum"); } }


        object valueNumReg = null;


        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            SetupTimer();

            
           // int[] numReg = new int[200];
            for (int i = 1; i <= 200; i++) numRegValues.Items.Add($"R[{i}] = ");

            for (int i = 1; i <= 200; i++) posRegValues.Items.Add($"PR[{i}]:    J1=                      J2=                      J3=                      J4=                      J5=                      J6=                      ");




            /*
            int[] numReg = new int[200];
            numReg[1] = 0; // drawer1 için 1. registerı 0 yap
            for (int i = 0; i < 200; i++)
            {
                
            }
            */

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
                mobjTask = mobjDataTable.AddTask(FRRJIf.FRIF_DATA_TYPE.TASK, 1); // program adı
                mobjNumReg = mobjDataTable.AddNumReg(FRRJIf.FRIF_DATA_TYPE.NUMREG_INT, 1, 200); // numerik registerlar
                mobjPosReg = mobjDataTable.AddPosReg(FRRJIf.FRIF_DATA_TYPE.POSREG, 1, 1, 10); // Position registerlar



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
                    UFNum = intUF;
                    X_Pos = (float)xyzwpr.GetValue(0);
                    Y_Pos = (float)xyzwpr.GetValue(1);
                    Z_Pos = (float)xyzwpr.GetValue(2);
                    W_Pos = (float)xyzwpr.GetValue(3);
                    P_Pos = (float)xyzwpr.GetValue(4);
                    R_Pos = (float)xyzwpr.GetValue(5);           
                }

                if (intValidJ != 0) // Eklem (Joint) pozisyonları geçerliyse
                {
                    
                    J1_Pos = (float)joint.GetValue(0);
                    J2_Pos = (float)joint.GetValue(1);
                    J3_Pos = (float)joint.GetValue(2);
                    J4_Pos = (float)joint.GetValue(3);
                    J5_Pos = (float)joint.GetValue(4);
                    J6_Pos = (float)joint.GetValue(5);

                }
            }
            string progName = "";
            short lineNumber = 0;
            short state = 0;
            string parentProgName = "";
            if (mobjTask.GetValue(ref progName, ref lineNumber, ref state, ref parentProgName))
            {
                txtProgramName.Text = progName;
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

            mobjCore.DataTable.Refresh();
            numRegValues.Items.Clear();

            if (communicationState)
            {
                for (int i = 1; i <= 200; i++)
                {
                    mobjNumReg.GetValue(i, ref valueNumReg);
                    numRegValues.Items.Add($"R[{i}] = {valueNumReg}");
                }
            }





            // btnIP.Content = ip.InputIP;
        }

        

       

        private async void btnRun_Click(object sender, RoutedEventArgs e)  //Çalışmıyor!!
        {
            // mobjCore.Cgtp.SelectProgram("MAIN", 1);  // https://github.com/underautomation/Fanuc.NET/blob/main/README.md
            if (communicationState)
            {
                Array signalOn = new short[] { 1 };
                Array signalOff = new short[] { 0 };
    
                mobjCore.WriteUI(6, ref signalOn, 1);              
                await Task.Delay(300);
                mobjCore.WriteUI(6, ref signalOff, 1);
            }
            else
            {
                MessageBox.Show("Robota bağlı değilsiniz.");
            }
        }

        private void btnAlarmsClear_Click(object sender, RoutedEventArgs e)
        {
            if(communicationState) mobjCore.ClearAlarm(0);
        }

        private void btnNumRegSet_Click(object sender, RoutedEventArgs e)
        {
            if (!communicationState)
            {
                MessageBox.Show("No connection!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int listViewIndex = numRegValues.SelectedIndex;
            int numRegIndex = 0;
            int[] NumRegIndex = new int[200]; // kullanılmadı. 
            numRegIndex = listViewIndex + 1;

            if (!int.TryParse(txtNumRegSet.Text, out int numRegValue) )
            {
                MessageBox.Show("Invalid value!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if(numRegValues.SelectedIndex == -1) MessageBox.Show("Please selecet a register!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);



            mobjNumReg.SetValue(numRegIndex, numRegValue);

            
           // object[] ValueNumReg = new object[200];

            mobjCore.DataTable.Refresh();
            numRegValues.Items.Clear();

            if (communicationState)
            {
                for (int i = 1; i <= 200; i++)
                {
                    mobjNumReg.GetValue(i, ref valueNumReg);
                    numRegValues.Items.Add($"R[{i}] = {valueNumReg}");
                }
            }

            
           // for (i = 1; i <= 200; i++) numRegValues.Items.Add($"R[{i}] = {valueNumReg}");

            //  numRegValues.Items.Insert(listViewIndex, $"R[{numRegIndex}] = {valueNumReg}");

        }

        Array sngJoint = new float[6];
        private void btnPosRegSet_J1_Click(object sender, RoutedEventArgs e)
        {
            if (!communicationState)
            {
                MessageBox.Show("No connection!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int listViewIndex = posRegValues.SelectedIndex;
            int posRegIndex = 0;
            posRegIndex = listViewIndex + 1;
            

            if (!float.TryParse(txtPosRegSet_J1.Text, out float posRegValue))
            {
                MessageBox.Show("Invalid value!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (posRegValues.SelectedIndex == -1) MessageBox.Show("Please selecet a register!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

            sngJoint.SetValue(posRegValue, 0);
            mobjPosReg.SetValueJoint(posRegIndex, ref sngJoint, 15, 15);
        }

        private void btnPosRegSet_J2_Click(object sender, RoutedEventArgs e)
        {
            if (!communicationState)
            {
                MessageBox.Show("No connection!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int listViewIndex = posRegValues.SelectedIndex;
            int posRegIndex = 0;
            posRegIndex = listViewIndex + 1;
            

            if (!float.TryParse(txtPosRegSet_J2.Text, out float posRegValue))
            {
                MessageBox.Show("Invalid value!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (posRegValues.SelectedIndex == -1) MessageBox.Show("Please selecet a register!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

            sngJoint.SetValue(posRegValue, 1);
            mobjPosReg.SetValueJoint(posRegIndex, ref sngJoint, 15, 15);
        }

        private void btnPosRegSet_J3_Click(object sender, RoutedEventArgs e)
        {
            if (!communicationState)
            {
                MessageBox.Show("No connection!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int listViewIndex = posRegValues.SelectedIndex;
            int posRegIndex = 0;
            posRegIndex = listViewIndex + 1;
            

            if (!float.TryParse(txtPosRegSet_J3.Text, out float posRegValue))
            {
                MessageBox.Show("Invalid value!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (posRegValues.SelectedIndex == -1) MessageBox.Show("Please selecet a register!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

            sngJoint.SetValue(posRegValue, 2);
            mobjPosReg.SetValueJoint(posRegIndex, ref sngJoint, 15, 15);
        }

        private void btnPosRegSet_J4_Click(object sender, RoutedEventArgs e)
        {
            if (!communicationState)
            {
                MessageBox.Show("No connection!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int listViewIndex = posRegValues.SelectedIndex;
            int posRegIndex = 0;
            posRegIndex = listViewIndex + 1;
            

            if (!float.TryParse(txtPosRegSet_J4.Text, out float posRegValue))
            {
                MessageBox.Show("Invalid value!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (posRegValues.SelectedIndex == -1) MessageBox.Show("Please selecet a register!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

            sngJoint.SetValue(posRegValue, 3);
            mobjPosReg.SetValueJoint(posRegIndex, ref sngJoint, 15, 15);
        }

        private void btnPosRegSet_J5_Click(object sender, RoutedEventArgs e)
        {
            if (!communicationState)
            {
                MessageBox.Show("No connection!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int listViewIndex = posRegValues.SelectedIndex;
            int posRegIndex = 0;
            posRegIndex = listViewIndex + 1;
            

            if (!float.TryParse(txtPosRegSet_J5.Text, out float posRegValue))
            {
                MessageBox.Show("Invalid value!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (posRegValues.SelectedIndex == -1) MessageBox.Show("Please selecet a register!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

            sngJoint.SetValue(posRegValue, 4);
            mobjPosReg.SetValueJoint(posRegIndex, ref sngJoint, 15, 15);
        }

        private void btnPosRegSet_J6_Click(object sender, RoutedEventArgs e)
        {
            if (!communicationState)
            {
                MessageBox.Show("No connection!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int listViewIndex = posRegValues.SelectedIndex;
            int posRegIndex = 0;
            posRegIndex = listViewIndex + 1;
            

            if (!float.TryParse(txtPosRegSet_J6.Text, out float posRegValue))
            {
                MessageBox.Show("Invalid value!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (posRegValues.SelectedIndex == -1) MessageBox.Show("Please selecet a register!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

            sngJoint.SetValue(posRegValue, 5);
            mobjPosReg.SetValueJoint(posRegIndex, ref sngJoint, 15, 15);
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