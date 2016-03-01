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
using System.Media;
using System.Timers;

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class SignInPage : UserControl
    {

        AttendanceWriter attendanceWriter = MainWindow.AppWindow.getAttendanceWriter();

        List<string> authorizedCheckerIDs;
        List<string> temporaryCheckersList;
        System.Timers.Timer aTimer;
        int counter = 0;
        string chapelCheckerId;
        private SoundPlayer happyPlayer = new SoundPlayer(@"../../Assets/blip.wav");
        private SoundPlayer failPlayer = new SoundPlayer(@"../../Assets/failure_beep.wav");
        string lastID = "";
        bool deviceConnected = false;


        public SignInPage()
        {
            InitializeComponent();
            deviceConnected = MainWindow.AppWindow.getDeviceConnected();
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
                    MainWindow.AppWindow.textBox2.Text = "No device found";
                    labelID.Text = "RFID USB Device Not Found: \n\n Please Connect Device!";
                }              
            }
        }

        private void buttonScan_Click(object sender, RoutedEventArgs e)
        {
            buttonScan.IsEnabled = false;
            buttonUpdateStudentInfo.IsEnabled = false;
            deviceConnected = MainWindow.AppWindow.getDeviceConnected();

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
                    MainWindow.AppWindow.textBox2.Text = "No device found";
                    buttonScan.IsEnabled = true;
                    buttonUpdateStudentInfo.IsEnabled = true;
                }
            }

            if (deviceConnected)
            {

                getTemporaryCheckers();

                authorizedCheckerIDs = new List<string>();

                authorizedCheckerIDs = attendanceWriter.getAuthorizedCheckersFromTextFile();



                //Comment out this to skip authorization of sign in

                aTimer = new System.Timers.Timer();
                aTimer.Elapsed += new ElapsedEventHandler(signInScan);
                aTimer.Interval = 500;
                aTimer.Enabled = true;

                //Uncomment this to skip authorization to sign in
                //MainWindow.AppWindow.GoToEventsPage();

                buttonScan.IsEnabled = false;
            }
        }

        
        private void getTemporaryCheckers()
        {
            this.temporaryCheckersList = attendanceWriter.getTempCheckersFromTextFile();
        }
        

        private void successfulSignIn()
        {
            MainWindow.AppWindow.GoToEventsPage();
        }

        private void signInScan(object source, ElapsedEventArgs e)
        {
            
  
                counter++;
                Byte[] Id = new Byte[8];
                int nBits = pcProxDLLAPI.getActiveID(8);

                // MainWindow.AppWindow.textBox2.Text = nBits.ToString();

                if (nBits > 0)
                {

                    //SystemSounds.Beep.Play();

                    String s = nBits.ToString() + " Bit ID [0]..[7]: ";
                    String proxID = "";
                
                    string checkerID = "";
                    string checkersName = "";

                    for (short i = 1; i > -1; i--)
                    {
                        Id[i] = pcProxDLLAPI.getActiveID_byte(i);
                        s = s + String.Format("{0:X2}.", Id[i]);
                        proxID += String.Format("{0:X2}", Id[i]);
                        Console.Out.WriteLine("proxID bits: " + proxID);
                        
                    }
                Console.Out.WriteLine("ProxID: " + proxID);
                checkerID = Int32.Parse(proxID, System.Globalization.NumberStyles.HexNumber).ToString();
                Console.Out.WriteLine("Decimal ID: " + checkerID);


                if (!lastID.Equals(checkerID))
                    {

                        if (authorizedCheckerIDs.Contains(checkerID))
                        {
                            lastID = checkerID;

                            Dispatcher.Invoke(() =>
                            {
                                labelID.Foreground = new SolidColorBrush(Colors.ForestGreen);
                                checkersName = attendanceWriter.getAuthorizedCheckersName(checkerID);
                                labelID.Text = checkersName + "\nis an authorized Christian Life and Worship Credit Checker";
                                labelID_Counter.Text = "";
                                Panel.SetZIndex(buttonProceed, 1);
                                buttonProceed.IsEnabled = true;
                            });

                            this.chapelCheckerId = checkerID;

                            aTimer.Stop();
                            Dispatcher.Invoke(() =>
                            {
                                attendanceWriter.setChapelCheckerID(checkerID);
                                //successfulSignIn();

                            });
                            happyPlayer.Play();
                        }
                        else
                        {
                            lastID = checkerID;
                            Dispatcher.Invoke(() =>
                            {
                                checkersName = attendanceWriter.getStudentsName(checkerID);
                                labelID.Text = checkersName + "\nis not an authorized Christian Life and Worship Credit Checker";
                                labelID_Counter.Text = counter.ToString();
                            });
                            failPlayer.Play();
                        }
                    }

                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            labelID_Counter.Text = counter.ToString();
                        });
                    }

                }

                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        labelID_Counter.Text = counter.ToString();
                    });
                }


                if (counter > 98)
                    this.counter = 0;

            }
        

        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            buttonScan.IsEnabled = false;
            buttonUpdateStudentInfo.IsEnabled = false;
            SQLPuller sqlPuller = new SQLPuller();
            sqlPuller.pullEvents();
            sqlPuller.pullAuthorizedCheckers();
            sqlPuller.pullStudents();
            buttonUpdateStudentInfo.IsEnabled = true;
            buttonScan.IsEnabled = true;
            MainWindow.AppWindow.textBox1.Text = "Updated: " + attendanceWriter.getDate();
            Mouse.OverrideCursor = null;
        }

        private void buttonProceed_Click(object sender, RoutedEventArgs e)
        {
            successfulSignIn();
        }
    }
}
