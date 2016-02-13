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

        AttendanceWriter attendanceWriter = MainWindow.AppWindow.getAttendanceWriter();
        bool textFileCreated = false;
        bool deviceConnected = false;
        System.Timers.Timer aTimer;
        int counter = 0;
        bool noCreditChecked = false;
        string lastID = "";
        string lastNoCreditID = "";
        string lastNoLongerID = "";
        string lastAlreadyID = "";
        List <string> creditList;
        List <string> noCreditList;
        private SoundPlayer happyPlayer = new SoundPlayer("../../Assets/blip.wav");
        private SoundPlayer failPlayer = new SoundPlayer("../../Assets/failure_beep.wav");

        public ScanPage()
        {
            InitializeComponent();

            labelEventTitle.Text = MainWindow.AppWindow.getEventName();

            noCreditList = new List<string>();
            creditList = new List<string>();
            buttonScan.IsEnabled = true;
            buttonStopScan.IsEnabled = false;
            Panel.SetZIndex(buttonScan, 1);
            Panel.SetZIndex(buttonStopScan, 0);



            if (textFileCreated == false)
            {
                attendanceWriter.CreateAttendanceTextFile();
                this.textFileCreated = true;
            }

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

            deviceConnected = MainWindow.AppWindow.getDeviceConnected();

            labelID.Foreground = new SolidColorBrush(Colors.DarkSlateBlue);
            labelID.Text = "Next Scan Will Receive Credit.";

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

        private void buttonScan_Click(object sender, RoutedEventArgs e)
        {

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



            if (deviceConnected)
            {
                buttonScan.IsEnabled = false;
                buttonStopScan.IsEnabled = true;
                Panel.SetZIndex(buttonScan, 0);
                Panel.SetZIndex(buttonStopScan, 1);


                
                aTimer = new System.Timers.Timer();
                aTimer.Elapsed += new ElapsedEventHandler(scanCard);
                aTimer.Interval = 500;
                aTimer.Enabled = true;


            }
        }

        private void scanCard(object source, ElapsedEventArgs e)
        {
            counter++;
            Byte[] Id = new Byte[8];
            int nBits = pcProxDLLAPI.getActiveID(8);
            string scannedID = "";
            string studentName = "";

            // MainWindow.AppWindow.textBox2.Text = nBits.ToString();

            if (nBits > 0)
            {
                String s = nBits.ToString() + " Bit ID [0]..[7]: ";
                String proxID = "";
                for (short i = 1; i > -1; i--)
                {
                    Id[i] = pcProxDLLAPI.getActiveID_byte(i);
                    s = s + String.Format("{0:X2}.", Id[i]);
                    proxID += String.Format("{0:X2}", Id[i]);
                    scannedID = Int32.Parse(proxID, System.Globalization.NumberStyles.HexNumber).ToString();
                }

                
                if (!lastID.Equals(scannedID) && !noCreditChecked && !noCreditList.Contains(scannedID) && !creditList.Contains(scannedID))
                {
                    lastID = scannedID;
                    lastNoLongerID = "";
                    lastNoCreditID = "";
                    lastAlreadyID = "";
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
                else if (!lastNoCreditID.Equals(scannedID) && noCreditChecked && !noCreditList.Contains(scannedID))
                {
                    lastNoCreditID = scannedID;
                    lastNoLongerID = "";
                    lastAlreadyID = "";
                    lastID = "";
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
                else if (!lastNoLongerID.Equals(scannedID) && noCreditList.Contains(scannedID))
                {
                    lastNoLongerID = scannedID;
                    lastAlreadyID = "";
                    lastNoCreditID = "";
                    lastID = "";
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
                else if (!lastAlreadyID.Equals(scannedID) && creditList.Contains(scannedID) && !noCreditList.Contains(scannedID))
                {
                    lastAlreadyID = scannedID;
                    lastNoLongerID = "";
                    lastNoCreditID = "";
                    lastID = "";
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
           

            Dispatcher.Invoke(() =>
            {
                labelID_Counter.Text = counter.ToString();
            });
            
            
            if (counter > 98)
                this.counter = 0;

        }


        private void buttonStopScan_Click(object sender, RoutedEventArgs e)
        {
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

        private void buttonDoneScanningYes_Click(object sender, RoutedEventArgs e)
        {
            aTimer.Stop();
            MainWindow.AppWindow.GoToResultsPage();
        }

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
        }

        public void setDeviceConnected(Boolean b)
        {
            this.deviceConnected = b;
        }

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
            lastNoLongerID = "";
            lastAlreadyID = "";
            lastNoCreditID = "";
            lastID = "";

            Panel.SetZIndex(buttonCancelNoCredit, 2);
            buttonCancelNoCredit.Opacity = 100;
            buttonCancelNoCredit.IsEnabled = true;

            this.noCreditChecked = true;
        }

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
