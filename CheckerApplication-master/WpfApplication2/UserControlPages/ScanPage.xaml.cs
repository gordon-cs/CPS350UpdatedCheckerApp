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
using System.Threading;
using System.Timers;
using System.Media;

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ScanPage : UserControl
    {
        //pulls attendanceWriter object from MainWindow
        AttendanceWriter attendanceWriter = MainWindow.AppWindow.getAttendanceWriter();
        
        
        //variables used throughout the class
        bool textFileCreated = false;
        bool deviceConnected = false;       
        int counter = 0;
        bool noCreditChecked = false;
        string lastIDforCredit = "";
        string lastNoCreditID = "";
        string lastNoCreditIDalready = "";
        string lastIDforCreditAlready = "";
        List <string> creditList;
        List <string> noCreditList;
        System.Timers.Timer aTimer;

        //noise makers for the scan
        private SoundPlayer happyPlayer = new SoundPlayer("../../Assets/blip.wav");
        private SoundPlayer failPlayer = new SoundPlayer("../../Assets/failure_beep.wav");

        //constants
        private const int SCANOFFSETTIME = 500;

        //creates the scan page
        public ScanPage()
        {
            //initializes the scan page, loading it into the main window
            InitializeComponent();

            //displays the event name at the top of the page, pulls it from main window
            labelEventTitle.Text = MainWindow.AppWindow.getEventName();

            //initialize lists
            noCreditList = new List<string>();
            creditList = new List<string>();

            //ensures buttons are in proper states
            buttonScan.IsEnabled = true;
            buttonStopScan.IsEnabled = false;
            Panel.SetZIndex(buttonScan, 1);
            Panel.SetZIndex(buttonStopScan, 0);


            //a text file is created for the event when the scan page is initialized
            if (textFileCreated == false)
            {
                attendanceWriter.CreateAttendanceTextFile();
                this.textFileCreated = true;
            }

            //All confirmation windows and buttons are disabled and set not to display
            confirmationTextBlock.Opacity = 0;
            confirmationBox.Opacity = 0;
            buttonDoneScanningNo.Opacity = 0;
            buttonDoneScanningYes.Opacity = 0;
            confirmationTextBlock2.Opacity = 0;
            confirmationBox2.Opacity = 0;
            buttonBlacklistNo.Opacity = 0;
            buttonBlacklistYes.Opacity = 0;
            buttonCancelNoCredit.Opacity = 0;

            buttonDoneScanningYes.IsEnabled = false;
            buttonDoneScanningNo.IsEnabled = false;
            buttonBlacklistYes.IsEnabled = false;
            buttonBlacklistNo.IsEnabled = false;
            buttonCancelNoCredit.IsEnabled = false;

            //checking if the device is connected with the actual device is expensive
            //so boolean from the MainWindow stores if the device is connected.
            deviceConnected = MainWindow.AppWindow.getDeviceConnected();

            //shows under scan button that the very next scan will recieve credit
            labelID.Foreground = new SolidColorBrush(Colors.DarkSlateBlue);
            labelID.Text = "Next Scan Will Receive Credit.";

            //if the device is not connected, runs through the device connection process
            if (!deviceConnected)
            {
                long DeviceID = 0;
                int rc = 0;
                rc = pcProxDLLAPI.usbConnect();
                if (rc == 1)
                {
                    DeviceID = pcProxDLLAPI.GetDID();
                    ushort proxDevice = pcProxDLLAPI.writeDevCfgToFile("prox_device_configuration");
                    MainWindow.AppWindow.textBox2.Text = "Connected to DeviceID: " + DeviceID;
                    this.deviceConnected = true;
                    MainWindow.AppWindow.setDeviceConnected(true);
                    labelID.Text = "USB Scanning Device Found.";
                }
                else
                {
                    MainWindow.AppWindow.textBox2.Text = "No devices found to connect with";
                    labelID.Text = "USB Scanning Device Not Found: \n\n Please Connect the USB Scanning Device.";
                }
            }

        }

        //button that initializes the scanning process
        private void buttonScan_Click(object sender, RoutedEventArgs e)
        {
            //disables the scan button because it is no longer needed
            buttonScan.IsEnabled = false;

            //if the device isnt connected, go through the device connection process
            if (!deviceConnected)
            {
                long DeviceID = 0;
                int rc = 0;
                rc = pcProxDLLAPI.usbConnect();
                if (rc == 1)
                {
                    DeviceID = pcProxDLLAPI.GetDID();
                    ushort proxDevice = pcProxDLLAPI.writeDevCfgToFile("prox_device_configuration");
                    MainWindow.AppWindow.textBox2.Text = "Connected to DeviceID: " + DeviceID;
                    this.deviceConnected = true;
                    MainWindow.AppWindow.setDeviceConnected(true);
                    labelID.Text = "USB Scanning Device Found.";
                }
                else
                {
                    MainWindow.AppWindow.textBox2.Text = "No devices found to connect with";
                }
            }


            //if the device is connected starts the scan process
            if (deviceConnected)
            {
                //enables the stop scan button and gets rid of the scan button
                buttonScan.IsEnabled = false;
                buttonStopScan.IsEnabled = true;
                Panel.SetZIndex(buttonScan, 0);
                Panel.SetZIndex(buttonStopScan, 1);

                //initializes the timer to run the scanCard function at an interval
                //given by constant above
                aTimer = new System.Timers.Timer();
                aTimer.Elapsed += new ElapsedEventHandler(scanCard);
                aTimer.Interval = SCANOFFSETTIME;
                aTimer.Enabled = true;


            }
        }

        // the scan function that runs on a timer
        private void scanCard(object source, ElapsedEventArgs e)
        {
            //initializes string variables
            string scannedID = "";
            string studentName = "";

            //gets the bits of the proxID from a scan
            Byte[] Id = new Byte[8];
            int nBits = pcProxDLLAPI.getActiveID(8);
            
            //if bits are received from the scan, sets the scannedID variable to the 
            //proxID bits that we want
            if (nBits > 0)
            {
                String s = nBits.ToString() + " Bit ID [0]..[7]: ";
                String proxID = "";
                for (short i = 1; i > -1; i--)
                {
                    Id[i] = pcProxDLLAPI.getActiveID_byte(i);
                    s = s + String.Format("{0:X2}.", Id[i]);
                    proxID += String.Format("{0:X2}", Id[i]);
                }

                //Console.Out.WriteLine("proxID is:" + proxID);
                scannedID = Int32.Parse(proxID, System.Globalization.NumberStyles.HexNumber).ToString();
                //Console.Out.WriteLine("scannedID is:" + scannedID);
                
                //performs a successful scan for credit if the following:
                //
                //the last id for credit does not equal the currently scannedID
                //
                //the noCredit checkbox is unchecked
                //
                //the noCreditList does not contain the currently scannedID
                //
                //the creditList does not contain the currently scannedID
                //
                if (!lastIDforCredit.Equals(scannedID) && !noCreditChecked && !noCreditList.Contains(scannedID) && !creditList.Contains(scannedID))
                {
                    lastIDforCredit = scannedID;
                    lastNoCreditIDalready = "";
                    lastNoCreditID = "";
                    lastIDforCreditAlready = "";
                    Dispatcher.Invoke(() =>
                    {
                        studentName = attendanceWriter.getStudentsName(scannedID);
                        labelID.Foreground = new SolidColorBrush(Colors.DarkSlateBlue);
                        labelID.Text = studentName + " will receive credit.";
                    });
                    creditList.Add(scannedID);
                    attendanceWriter.setNoCredit(0);
                    attendanceWriter.WriteAttendanceTextFile(scannedID);
                    happyPlayer.Play();
                }

                //performs a no credit scan for no credit if the following:
                //
                //the last no credit id does not equal the currently scannedID
                //
                //the noCredit checkbox is checked
                //
                //the noCreditList does not contain the currently scannedID
                //
                else if (!lastNoCreditID.Equals(scannedID) && noCreditChecked && !noCreditList.Contains(scannedID))
                {
                    lastNoCreditID = scannedID;
                    lastNoCreditIDalready = "";
                    lastIDforCreditAlready = "";
                    lastIDforCredit = "";
                    Dispatcher.Invoke(() =>
                    {
                        studentName = attendanceWriter.getStudentsName(scannedID);
                        labelID.Foreground = new SolidColorBrush(Colors.Red);
                        labelID.Text = studentName + " will not receive credit.";
                        labelID_Counter.Text = counter.ToString();
                        checkBoxNoCredit.IsChecked = false;
                        Panel.SetZIndex(buttonCancelNoCredit, -1);
                        buttonCancelNoCredit.Opacity = 0;
                        buttonCancelNoCredit.IsEnabled = false;
                    });
                    noCreditList.Add(scannedID);
                    attendanceWriter.setNoCredit(1);
                    attendanceWriter.WriteAttendanceTextFile(scannedID);
                    noCreditChecked = false;
                    failPlayer.Play();

                }

                //performs a no longer can receive credit scan if the following:
                //
                //the last id for no credit does not equal the currently scannedID
                //
                //the last id for no credit already does not equal the currently scannedID
                //
                //the no credit list does contain the currently scannedID
                //
                else if (!lastNoCreditIDalready.Equals(scannedID) && !lastNoCreditID.Equals(scannedID) && noCreditList.Contains(scannedID))
                {
                    lastNoCreditIDalready = scannedID;
                    lastIDforCreditAlready = "";
                    lastNoCreditID = "";
                    lastIDforCredit = "";
                    Dispatcher.Invoke(() =>
                    {
                        studentName = attendanceWriter.getStudentsName(scannedID);
                        labelID.Foreground = new SolidColorBrush(Colors.Red);
                        labelID.Text = studentName + " can no longer receive credit.";
                        checkBoxNoCredit.IsChecked = false;
                        Panel.SetZIndex(buttonCancelNoCredit, -1);
                        buttonCancelNoCredit.Opacity = 0;
                        buttonCancelNoCredit.IsEnabled = false;
                    });
                    noCreditChecked = false;
                    failPlayer.Play();
                }

                //performs an already received credit scan if the following:
                //
                //the last id for credit already does not equal the currently scannedID
                //
                //the last id for credit does not equal the currently scannedID
                //
                //the credit list does contain the scannedID
                //
                //the no credit list does not contain the currently scannedID
                //
                else if (!lastIDforCreditAlready.Equals(scannedID) && !lastIDforCredit.Equals(scannedID) && creditList.Contains(scannedID) && !noCreditList.Contains(scannedID))
                {
                    lastIDforCreditAlready = scannedID;
                    lastNoCreditIDalready = "";
                    lastNoCreditID = "";
                    lastIDforCredit = "";
                    Dispatcher.Invoke(() =>
                    {
                        studentName = attendanceWriter.getStudentsName(scannedID);
                        labelID.Foreground = new SolidColorBrush(Colors.DarkSlateBlue);
                        labelID.Text = studentName + " has already received credit.";
                    });
                    happyPlayer.Play();
                }


                //SystemSounds.Beep.Play();

            }
           
            //displays the counter to show the scanner is running
            Dispatcher.Invoke(() =>
            {
                labelID_Counter.Text = counter.ToString();
            });

            //if the counter is about to reach 3 digits, set back to 0
            //else raises the counter int
            if (counter > 98)
            {
                this.counter = 0;
            }
            else
            {            
                counter++;
            }
        }

        //if the stop scanning button is clicked, a confirmation window is displayed
        //that allows the user to cancel or continue with stopping the scan
        private void buttonStopScan_Click(object sender, RoutedEventArgs e)
        {
            buttonStopScan.IsEnabled = false;
            confirmationTextBlock.Opacity = 100;
            confirmationBox.Opacity = 100;
            buttonDoneScanningNo.Opacity = 100;
            buttonDoneScanningYes.Opacity = 100;
            Panel.SetZIndex(confirmationTextBlock, 2);
            Panel.SetZIndex(confirmationBox, 2);
            Panel.SetZIndex(buttonDoneScanningYes, 2);
            Panel.SetZIndex(buttonDoneScanningNo, 2);
            buttonDoneScanningYes.IsEnabled = true;
            buttonDoneScanningNo.IsEnabled = true;
        }
        // if the yes button is clicked, scanning is stopped, and moves to the results page
        private void buttonDoneScanningYes_Click(object sender, RoutedEventArgs e)
        {
            aTimer.Stop();
            MainWindow.AppWindow.GoToResultsPage();
        }

        // if the no button is clicked, scanning continues as usual
        private void buttonDoneScanningNo_Click(object sender, RoutedEventArgs e)
        {
            confirmationTextBlock.Opacity = 0;
            confirmationBox.Opacity = 0;
            buttonDoneScanningNo.Opacity = 0;
            buttonDoneScanningYes.Opacity = 0;
            Panel.SetZIndex(confirmationTextBlock, -1);
            Panel.SetZIndex(confirmationBox, -1);
            Panel.SetZIndex(buttonDoneScanningYes, -1);
            Panel.SetZIndex(buttonDoneScanningNo, -1);
            buttonDoneScanningYes.IsEnabled = false;
            buttonDoneScanningNo.IsEnabled = false;
            buttonStopScan.IsEnabled = true;
        }

        
        //if the no credit checkbox is checked, displays a confirmation window
        //that allows the user to cancel or continue with a no credit scan
        private void checkBoxNoCredit_Checked(object sender, RoutedEventArgs e)
        {
            confirmationTextBlock2.Opacity = 100;
            confirmationBox2.Opacity = 100;
            buttonBlacklistNo.Opacity = 100;
            buttonBlacklistYes.Opacity = 100;
            Panel.SetZIndex(confirmationTextBlock2, 2);
            Panel.SetZIndex(confirmationBox2, 2);
            Panel.SetZIndex(buttonBlacklistYes, 2);
            Panel.SetZIndex(buttonBlacklistNo, 2);

            buttonBlacklistYes.IsEnabled = true;
            buttonBlacklistNo.IsEnabled = true;
        }

        //if the yes button is clicked for no credit, then the very next scan
        //will receive no credit
        private void buttonBlacklistYes_Click(object sender, RoutedEventArgs e)
        {
            confirmationTextBlock2.Opacity = 0;
            confirmationBox2.Opacity = 0;
            buttonBlacklistNo.Opacity = 0;
            buttonBlacklistYes.Opacity = 0;
            Panel.SetZIndex(confirmationTextBlock2, -1);
            Panel.SetZIndex(confirmationBox2, -1);
            Panel.SetZIndex(buttonBlacklistYes, -1);
            Panel.SetZIndex(buttonBlacklistNo, -1);

            buttonBlacklistYes.IsEnabled = false;
            buttonBlacklistNo.IsEnabled = false;

            labelID.Foreground = new SolidColorBrush(Colors.Red);
            labelID.Text = "Next Scan Will Give No Credit.";
            lastNoCreditIDalready = "";
            lastIDforCreditAlready = "";
            lastNoCreditID = "";
            lastIDforCredit = "";

            Panel.SetZIndex(buttonCancelNoCredit, 2);
            buttonCancelNoCredit.Opacity = 100;
            buttonCancelNoCredit.IsEnabled = true;

            this.noCreditChecked = true;
        }

        //if the no button is clicked for no credit, scanning continues as usual
        private void buttonBlacklistNo_Click(object sender, RoutedEventArgs e)
        {
            confirmationTextBlock2.Opacity = 0;
            confirmationBox2.Opacity = 0;
            buttonBlacklistNo.Opacity = 0;
            buttonBlacklistYes.Opacity = 0;
            Panel.SetZIndex(confirmationTextBlock2, -1);
            Panel.SetZIndex(confirmationBox2, -1);
            Panel.SetZIndex(buttonBlacklistYes, -1);
            Panel.SetZIndex(buttonBlacklistNo, -1);

            buttonBlacklistYes.IsEnabled = false;
            buttonBlacklistNo.IsEnabled = false;

            checkBoxNoCredit.IsChecked = false;
        }
        // after making the next scan for no credit, a button appears that allows the user
        // to cancel the no credit scan
        private void buttonCancelNoCredit_Click(object sender, RoutedEventArgs e)
        {
            noCreditChecked = false;
            checkBoxNoCredit.IsChecked = false;
            Panel.SetZIndex(buttonCancelNoCredit, -1);
            buttonCancelNoCredit.Opacity = 0;
            buttonCancelNoCredit.IsEnabled = false;
            labelID.Text = "Next Scan Will Receive Credit.";
        }
    }
}
