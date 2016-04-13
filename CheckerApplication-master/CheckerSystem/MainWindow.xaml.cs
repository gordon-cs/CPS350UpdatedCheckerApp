




using System.Windows;
using RFIDeas_pcProxAPI;
using System.ComponentModel;

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
            GoToSignInPage();
            //GoToEventsPage();     
            //GoToScanPage();
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
                pcProxDLLAPI.setTimeParms_iIDHoldTO(500);
                pcProxDLLAPI.setTimeParms_iIDLockOutTm(500);
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
