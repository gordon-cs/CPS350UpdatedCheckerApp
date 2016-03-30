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

namespace CheckerApplication
{

    public partial class MainWindow : Window
    {
        //attendance writer variable
        AttendanceWriter attendanceWriter;

        //makes mainWindow accessible as AppWindow
        public static MainWindow AppWindow;

        //variables for the main window class to be called from other pages
        bool deviceConnected = false;
        string eventTitle = "";
        bool databaseUpdated = true;
        string scanTime = "";

        //constructor for the main window
        public MainWindow()
        {
            //Initializes classes
            AppWindow = this;
            attendanceWriter = new AttendanceWriter();
            SQLPuller sqlPuller = new SQLPuller();

            //the following lines update the database
            //comment them out if you dont want to update on startup
            //uncomment them if you want to update on startup

            //sqlPuller.pullAuthorizedCheckers();
            //sqlPuller.pullEvents();
            //sqlPuller.pullStudents();

            pcProxDLLAPI.USBDisconnect();
            
            //displays main window and goes to the sign in page
            InitializeComponent();

            //uncomment the page you want to go to on startup for testing?
            //sign in page is the normal process for the app
            //GoToSignInPage();
            //GoToEventsPage();     
            GoToScanPage();
            //GoToResultsPage();



            textBox1.Text = "Updated: " + attendanceWriter.getDate();

            //attempts connecting the usb reader device

            long DeviceID = 0;
         
            if (pcProxDLLAPI.usbConnect() == 1)
            {
                DeviceID = pcProxDLLAPI.GetDID();
                ushort proxDevice = pcProxDLLAPI.writeDevCfgToFile("prox_device_configuration");
                MainWindow.AppWindow.textBox2.Text = "Connected to DeviceID: " + DeviceID;
                this.deviceConnected = true;
            }
            else
            {
                MainWindow.AppWindow.textBox2.Text = "No device found";
            }
            
        }

        //functions to go to the various pages of the application
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

        //function to get or set the various variables of this window
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

        public string getScanTime()
        {
            return scanTime;
        }

        public void setScanTime(string value)
        {
            scanTime = value;
        }

        public bool getDatabaseUpdated()
        {
            return databaseUpdated;
        }

        public void setDatabaseUpdated(bool value)
        {
            databaseUpdated = value;
        }

    }
}
