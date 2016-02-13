using System;
using System.Collections.Generic;
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
using RFIDeas_pcProxAPI;

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //int to determine which page is currently active
        AttendanceWriter attendanceWriter;
        public static MainWindow AppWindow;
        bool deviceConnected = false;
        string eventTitle = "";

        public MainWindow()
        {
            AppWindow = this;
            attendanceWriter = new AttendanceWriter();

            SQLPuller sqlPuller = new SQLPuller();

            //sqlPuller.pullAuthorizedCheckers();
            //sqlPuller.pullEvents();
            //sqlPuller.pullStudents();


            pcProxDLLAPI.USBDisconnect();
            

            InitializeComponent();
            GoToSignInPage();
            //GoToEventsPage();     
            //GoToScanPage();
            //GoToResultsPage();

            
            long DeviceID = 0;
            int rc = 0;
            rc = pcProxDLLAPI.usbConnect();
            if (rc == 1)
            {
                DeviceID = pcProxDLLAPI.GetDID();
                ushort proxDevice = pcProxDLLAPI.writeDevCfgToFile("prox_device_configuration");
                MainWindow.AppWindow.textBox2.Text = "Connected to DeviceID: " + DeviceID;
                this.deviceConnected = true;
            }
            else
            {
                MainWindow.AppWindow.textBox2.Text = "No devices found to connect with";
            }
            

        }

        public void GoToResultsPage()
        {
            this.contentControl.Content = new ResultsPage();
        }

        public void GoToScanPage()
        {
            this.contentControl.Content = new ScanPage();
        }

        public void GoToSignInPage()
        {
            this.contentControl.Content = new SignInPage();
        }

        public void GoToEventsPage()
        {
            this.contentControl.Content = new EventPage();
        }

        public AttendanceWriter getAttendanceWriter()
        {
            return attendanceWriter;
        }

        public bool getDeviceConnected()
        {
            return deviceConnected;
        }

        public void setDeviceConnected(bool value)
        {
            deviceConnected = value;
        }

        public string getEventName()
        {
            return eventTitle;
        }

        public void setEventName(string value)
        {
            eventTitle = value;
        }

    }
}
