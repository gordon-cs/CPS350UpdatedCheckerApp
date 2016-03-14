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
using System.Windows.Media.Animation;

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
        System.Timers.Timer scanTimer;
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
                    if (MainWindow.AppWindow.getDatabaseUpdated())
                    {
                        labelID.Text = "Database Updated. \n\n Sign in with Gordon ID";
                    }
                    else
                    {
                        labelID.Text = "Database Outdated. \n\n Update Database\nor Sign in";
                    }
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

                scanTimer = new System.Timers.Timer();
                scanTimer.Elapsed += new ElapsedEventHandler(signInScan);
                scanTimer.Interval = 500;
                scanTimer.Enabled = true;

                //Uncomment this to skip authorization to sign in
                //MainWindow.AppWindow.GoToEventsPage();

                buttonScan.IsEnabled = false;
                buttonCancelScan.IsEnabled = true;
                Panel.SetZIndex(buttonCancelScan, 2);
            }
        }

        //plays the happy sound
        private void playHappySound()
        {
            happyPlayer.Play();
        }

        //plays the failure sound
        private void playFailSound()
        {
            failPlayer.Play();
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

                }
                checkerID = Int32.Parse(proxID, System.Globalization.NumberStyles.HexNumber).ToString();
                Console.Out.WriteLine("checkers hex id:" + proxID);
                Console.Out.WriteLine("checkers decimal id:" + checkerID);
                Console.Out.WriteLine("checkers barcode: " + MainWindow.AppWindow.getAttendanceWriter().getStudentsBarcode(checkerID));


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
                            buttonCancelScan.IsEnabled = false;
                            Panel.SetZIndex(buttonCancelScan, -1);
                            Panel.SetZIndex(buttonProceed, 1);
                            buttonProceed.IsEnabled = true;
                            circleAnimation.Opacity = 0;
                        });

                        this.chapelCheckerId = checkerID;

                        scanTimer.Stop();
                        Dispatcher.Invoke(() =>
                        {
                            attendanceWriter.setChapelCheckerID(checkerID);
                                //successfulSignIn();

                            });
                        playHappySound();
                    }
                    else
                    {
                        lastID = checkerID;
                        Dispatcher.Invoke(() =>
                        {
                            checkersName = attendanceWriter.getStudentsName(checkerID);
                            if (checkersName.Contains("Student"))
                                labelID.Text = checkersName + " is not in the Database \n\nTry Updating Database";
                            else
                                labelID.Text = checkersName + "\nis not an Authorized Christian Life and Worship Credit Checker";
                            labelID_Counter.Text = counter.ToString();
                        });
                        playFailSound();
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
            labelID.Text = "Updating Database...";
            buttonScan.IsEnabled = false;
            buttonUpdateStudentInfo.IsEnabled = false;
            SQLPuller sqlPuller = new SQLPuller();
            sqlPuller.pullEvents();
            sqlPuller.pullAuthorizedCheckers();
            sqlPuller.pullStudents();
            buttonUpdateStudentInfo.IsEnabled = true;
            buttonScan.IsEnabled = true;
            MainWindow.AppWindow.textBox1.Text = "Updated: " + attendanceWriter.getDate();

            if (MainWindow.AppWindow.getDatabaseUpdated() == true)
            {
                attendanceWriter.CreateDateTextFile();
                labelID.Text = "Database Update Successful \n\n Sign in with Gordon ID";
            }
            else
            {
                labelID.Text = "Database Update Failed \n \n Check Internet Connection";
            }
            Mouse.OverrideCursor = null;
        }

        private void buttonProceed_Click(object sender, RoutedEventArgs e)
        {
            successfulSignIn();
        }

        private void buttonCancelScan_Click(object sender, RoutedEventArgs e)
        {
            scanTimer.Stop();
            counter = 0;
            labelID_Counter.Text = "";
            labelID.Text = "Sign In\nor Update Database";
            lastID = "";
            buttonScan.IsEnabled = true;
            buttonUpdateStudentInfo.IsEnabled = true;
            buttonCancelScan.IsEnabled = false;
            Panel.SetZIndex(buttonCancelScan, -1);
        }
    }
}
