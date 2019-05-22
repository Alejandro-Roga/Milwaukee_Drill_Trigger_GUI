using LiveCharts;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Milwaukee_Drill_Trigger_GUI
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Constants
        private const string MPS_BLUE = "#FF006AC6";
        private const string MPS_GREY = "#FFC1C8D0";
        private const double MAX_ANGLE = 300;
        private const double MAX_TRAVEL = 8.80;
        #endregion

        #region Variables
        private bool IsDeviceConnected = false;
        private byte MG_value = 0;
        private double bct_k = 1;
        private int mght_fall;
        private int mght_rise;
        private int mglt_fall;
        private int mglt_rise;
        private double _angleValue;
        private double _travelValue;
        private double angToTrv;
        private double ang_tsh;
        private double trv_tsh = 3;
        private readonly Regex _regex = new Regex("[0-9]");
        Connect device;
        #endregion

        #region Properties
        public double Angle_Value
        {
            get { return _angleValue; }
            set
            {
                _angleValue = value;
                Angle_Value_Text.Text = Convert.ToInt32(value) + "º";
                OnPropertyChanged("Angle_Value");
            }
        }
        public double Travel_Value
        {
            get { return _travelValue; }
            set
            {
                _travelValue = value;
                Position_Value_Text.Text = value.ToString("#.##") + " mm";
                OnPropertyChanged("Travel_Value");
            }
        }
        public UInt16 Zero_Value { get; set; } = 0;
        public UInt16 Zero_Prev { get; set; } = 0;
        public bool MGL { get; set; } = false; //Register 0x27 bit 6
        public bool MGH { get; set; } = false; //Register 0x27 bit 7

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }
        public int Sample_period { get; set; }
        public bool IsReading { get; set; }
        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            Sample_period = 20;
            angToTrv = MAX_TRAVEL / MAX_ANGLE;
            ang_tsh = trv_tsh / angToTrv;

            Angle_Graph.ToValue = MAX_ANGLE;
            Angle_Graph.Wedge = MAX_ANGLE;

            Angle_Graph_OFF.FromValue = 0;
            Angle_Graph_OFF.ToValue = ang_tsh;
            Angle_Graph_ON.FromValue = ang_tsh;
            Angle_Graph_ON.ToValue = MAX_ANGLE;
            Position_Graph.Maximum = MAX_TRAVEL;

            Formatter = value => value.ToString("N");
            device = new Connect();

            DataContext = this;
        }
        #endregion

        private async void Read()
        {
            while (IsReading)
            {
                Angle_Value = device.AngularPosition();
                if (Angle_Value == 12000)
                    ResetUI(true);

                device.ReadMagnetFlag();
                
                //Console.WriteLine("angle: " + Angle_Value);
                if (Angle_Value > MAX_ANGLE) Angle_Value = MAX_ANGLE;

                if (device.MGH) MGH_Led.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom(MPS_BLUE));
                else MGH_Led.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

                if (device.MGL) MGL_Led.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom(MPS_BLUE));
                else MGL_Led.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

                Travel_Value = Angle_Value * angToTrv;
                if (Travel_Value > trv_tsh)
                {
                    if(!(device.MGL||device.MGH))
                    {
                        LED_Drill.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom(MPS_BLUE));
                        LED_Drill_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
                        LED_Drill_Text.Text = "Drill ON";
                    }
                    else
                    {
                        LED_Drill.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
                        LED_Drill_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(MPS_BLUE));
                        LED_Drill_Text.Text = "Drill OFF";
                    }

                    Position_Graph.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(MPS_BLUE));
                    Position_Graph.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom(MPS_BLUE));
                    Angle_Value_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(MPS_BLUE));
                }
                else
                {
                    LED_Drill.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
                    LED_Drill_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(MPS_BLUE));
                    LED_Drill_Text.Text = "Drill OFF";
                    Position_Graph.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(MPS_GREY));
                    Position_Graph.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom(MPS_GREY));
                    Angle_Value_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(MPS_GREY));
                }

                await AsyncDelay(Sample_period);
            }
        }
        private void ResetUI(bool error)
        {
            Travel_Value = 0;
            Angle_Value = 0;
            IsReading = false;
            if(error) MessageBox.Show("The device has been disconncted.", "Connection Error");
            Connect_Button.Content = "CONNECT";
            Connect_Button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF006AC6"));
            Start_Button.Content = "START";
            Start_Button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF34D08C"));
            Config_Container.IsEnabled = false;
            LED_Drill.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            LED_Drill_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(MPS_BLUE));
            LED_Drill_Text.Text = "Drill OFF";
           
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Buton Click Events
        private void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!IsDeviceConnected)
            {
                device.InitConnect();
                if (device.IsConnected)
                {
                    IsDeviceConnected = true;

                    Connect_Button.Content = "CONNECTED";
                    Connect_Button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF34D08C"));
                    Config_Container.IsEnabled = true;

                    MG_value = device.ReadMagnetValue();
                    if (MG_value > 223) MGL_Slider.Value = (MG_value >> 5) - 1;
                    else MGL_Slider.Value = (MG_value >> 5) + 1;
                    if ((MG_value & 0x1C) > 27) MGH_Slider.Value = (MG_value >> 2) - 1;
                    else MGH_Slider.Value = (MG_value >> 2) + 1;

                    MGH_Slider.Value = MG_value >> 2;
                    MGL_Slider.Value = MG_value >> 5;

                    device.WriteAtRegister(0, 0);
                    device.WriteAtRegister(1, 0);

                    if (!IsReading)
                    {
                        IsReading = true;
                        if (IsReading) Read();
                       
                    }
                   
                }
            }
            else
            {
                ResetUI(false);
                device.CloseConnection();
                IsDeviceConnected = false;
                Connect_Button.Content = "CONNECT";
                Connect_Button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(MPS_BLUE));
                Config_Container.IsEnabled = false;
            }
        }
        
        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            if (device.IsConnected)
            {
                if (!IsReading)
                {
                    IsReading = true;
                    if (IsReading) Read();
                    Start_Button.Content = "STOP";
                    Start_Button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFE14938"));
                }
                else
                {
                    IsReading = false;
                    Start_Button.Content = "START";
                    Start_Button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF34D08C"));
                }
            }
        }
        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not implemented yet.");
        }
        #endregion

        #region Inputs Events
        private bool IsTextAllowed(string text)
        {
            return _regex.IsMatch(text);
        }
        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void BCT_Text_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BCT_Text.Text == "") BCT_Text.Text = "0";
            int bct_value = Convert.ToByte(BCT_Text.Text);
            bct_k = (float) (258 - bct_value) / 258;

            MGH_RiseFall(int.Parse(MGH_Text.Text));
            MGL_RiseFall(int.Parse(MGL_Text.Text));

            device.WriteAtRegister(2, Convert.ToByte(bct_value));
        }
        private void MGH_Text_LostFocus(object sender, RoutedEventArgs e)
        {
            if (MGH_Text.Text == "") MGH_Text.Text = "0";

            int temp = int.Parse(MGH_Text.Text);
            MGH_RiseFall(temp);
            temp = (int.Parse(MGL_Text.Text) << 5) + (int.Parse(MGH_Text.Text) << 2);

            //device.WriteAtRegister(6, Convert.ToByte(temp));

            MG_value = device.ReadMagnetValue();
            Console.WriteLine(Convert.ToString(MG_value, 2));
        }
        private void MGL_Text_LostFocus(object sender, RoutedEventArgs e)
        {
            if (MGL_Text.Text == "") MGL_Text.Text = "0";
            if (int.Parse(MGL_Text.Text) > 7) MGL_Text.Text = "7";
            int temp = int.Parse(MGL_Text.Text);
            MGL_RiseFall(temp);

            temp = (int.Parse(MGL_Text.Text) << 5) + (int.Parse(MGH_Text.Text) << 2);
            //device.WriteAtRegister(6, Convert.ToByte(temp));
            MG_value = device.ReadMagnetValue();
            Console.WriteLine(Convert.ToString(MG_value, 2));
        }
        private void Zero_Text_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Zero_Text.Text == "") Zero_Text.Text = "0";

        }

        private void BCT_Text_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if(BCT_Text.Text != "")
                    BMT_Slider.Value = int.Parse(BCT_Text.Text);
        }
        private void MGH_Text_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if (MGH_Text.Text != "")
                    MGH_Slider.Value = int.Parse(MGH_Text.Text);
        }
        private void MGL_Text_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if (MGL_Text.Text != "")
                    MGL_Slider.Value = int.Parse(MGL_Text.Text);
        }

        private void MGH_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MGH_RiseFall((int)MGH_Slider.Value);

            //device.WriteAtRegister(6, Convert.ToByte(temp >> 2));

        }
        private void MGL_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MGL_RiseFall((int)MGL_Slider.Value);

        }
        #endregion

        private async static Task AsyncDelay(int interval) => await Task.Delay(interval);

        private void BMT_Slider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int bct_value = (int)BMT_Slider.Value;
            bct_k = (float)(258 - bct_value) / 258;

            MGH_RiseFall(int.Parse(MGH_Text.Text));
            MGL_RiseFall(int.Parse(MGL_Text.Text));

            //device.WriteAtRegister(2, Convert.ToByte(bct_value));



        }

        private void MGH_RiseFall(int temp)
        {
            switch (temp)
            {
                case 0:
                    mght_fall = 26;
                    mght_rise = 20;
                    break;
                case 1:
                    mght_fall = 41;
                    mght_rise = 35;
                    break;
                case 2:
                    mght_fall = 56;
                    mght_rise = 50;
                    break;
                case 3:
                    mght_fall = 70;
                    mght_rise = 64;
                    break;
                case 4:
                    mght_fall = 84;
                    mght_rise = 78;
                    break;
                case 5:
                    mght_fall = 98;
                    mght_rise = 92;
                    break;
                case 6:
                    mght_fall = 112;
                    mght_rise = 106;
                    break;
                case 7:
                    mght_fall = 126;
                    mght_rise = 120;
                    break;
            }

            MGH_Fall.Text = (mght_fall / bct_k).ToString();
            MGH_Rise.Text = (mght_rise / bct_k).ToString();
        }
        private void MGL_RiseFall(int temp)
        {
            switch (temp)
            {
                case 0:
                    mglt_fall = 26;
                    mglt_rise = 20;
                    break;
                case 1:
                    mglt_fall = 41;
                    mglt_rise = 35;
                    break;
                case 2:
                    mglt_fall = 56;
                    mglt_rise = 50;
                    break;
                case 3:
                    mglt_fall = 70;
                    mglt_rise = 64;
                    break;
                case 4:
                    mglt_fall = 84;
                    mglt_rise = 78;
                    break;
                case 5:
                    mglt_fall = 98;
                    mglt_rise = 92;
                    break;
                case 6:
                    mglt_fall = 112;
                    mglt_rise = 106;
                    break;
                case 7:
                    mglt_fall = 126;
                    mglt_rise = 120;
                    break;
            }

            MGL_Fall.Text = (mglt_fall / bct_k).ToString();
            MGL_Rise.Text = (mglt_rise / bct_k).ToString();
        }

        private void Zero_Button_Click(object sender, RoutedEventArgs e)
        {
            device.WriteAtRegister(0, 0);
            device.WriteAtRegister(1, 0);

            Angle_Value = device.AngularPosition();
            Console.WriteLine("a: " + Angle_Value);

            UInt16 temp = Convert.ToUInt16(Angle_Value * 65536 / 360);

            Zero_Value =  Convert.ToUInt16(temp - 10);
            Zero_Text.Text = (Zero_Value * 0.0055).ToString("#.##") + "º";
            device.WriteAtRegister(0, Convert.ToByte(Zero_Value & 0x00FF));
            device.WriteAtRegister(1, Convert.ToByte((Zero_Value & 0xFF00) >> 8));
            Zero_Prev += temp;

        }
    }
}

