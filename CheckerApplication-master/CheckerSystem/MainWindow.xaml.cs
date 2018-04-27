/*
* MainWindow.xaml.cs - blank window that holds the various user control pages
*
* Responsible for navigating between the different user control pages: 
* SignInPage, EventPage, ScanPage, ResultsPage
* 
* MainWindow is accessible from other classes by using: 
* MainWindow.AppWindow
*
* MainWindow holds the attendanceWriter class
* so to access attendanceWriter one must use:
* MainWindow.AppWindow.attendanceWriter
* Otherwise you would be using a new instance of the attendanceWriter
*
* MainWindow is also the first location in the program that checks for the 
* RFID USB device and sets the program up as device connected if it is 
* found
*
* Authors: Jonathan Manos, Travis Pullen
* Last Modified: 4/25/16
*
*/

using System.Windows;
using RFIDeas_pcProxAPI;
using System.ComponentModel;
using System;

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
        bool databaseUpdated = false;
        string scanTime = "";

        //Time per scan constant set for the device
        private const int SCANOFFSETTIME = 500;

        //constructor for the main window
        public MainWindow()
        {
            //Initializes classes
            AppWindow = this;
            attendanceWriter = new AttendanceWriter();

            //the following lines update the database
            //comment them out if you dont want to update on startup
            //uncomment them if you want to update on startup

            //SQLPuller sqlPuller = new SQLPuller();
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
                //if the device's scan time does not match the constant set, write new
                //settings to device with the constant's scan time
                if (pcProxDLLAPI.getTimeParms_iIDHoldTO() != SCANOFFSETTIME || pcProxDLLAPI.getTimeParms_iIDLockOutTm() != SCANOFFSETTIME)
                {
                    pcProxDLLAPI.setTimeParms_iIDHoldTO(SCANOFFSETTIME);
                    pcProxDLLAPI.setTimeParms_iIDLockOutTm(SCANOFFSETTIME);
                    Console.Out.WriteLine(pcProxDLLAPI.getTimeParms_iIDHoldTO());
                    Console.Out.WriteLine(pcProxDLLAPI.getTimeParms_iIDLockOutTm());
                    Console.Out.WriteLine("WRITING CONFIG TO DEVICE");
                    Console.Out.WriteLine(pcProxDLLAPI.WriteCfg());
                    pcProxDLLAPI.ReadCfg();
                    Console.Out.WriteLine(pcProxDLLAPI.getTimeParms_iIDHoldTO());
                    Console.Out.WriteLine(pcProxDLLAPI.getTimeParms_iIDLockOutTm());
                }
            }
            else
            {
                MainWindow.AppWindow.textBox2.Text = "No device found";
            }     
        }

        //Asks user to confirm whether they really want to close out of the application
        public void MainWindow_Closing(object sender, CancelEventArgs e)
        {

            string message = "Are you sure that you want to exit the application?";
            MessageBoxResult result =
              MessageBox.Show(
                message,
                "CLAW Credit Checker Application",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
            {
                // If user doesn't want to close, cancel closure
                e.Cancel = true;
            }
            else
            {
                attendanceWriter.cleanUpAttendanceFile();
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
