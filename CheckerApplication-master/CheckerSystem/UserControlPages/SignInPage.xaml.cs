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
using System.ComponentModel;
using System.IO;

namespace CheckerApplication
{

    public partial class SignInPage : UserControl
    {
        //pulls attendanceWriter object from MainWindow
        AttendanceWriter attendanceWriter = MainWindow.AppWindow.getAttendanceWriter();
        BackgroundWorker backgroundWorker = new System.ComponentModel.BackgroundWorker();

        //variables used throughout the class
        List<string> authorizedCheckerIDs;
        List<string> temporaryCheckersList;
        System.Timers.Timer scanTimer;
        string chapelCheckerId;    
        string lastID = "";
        bool deviceConnected = false;

        //noise makers for the scan
        private SoundPlayer happyPlayer = new SoundPlayer(@"../../Assets/blip.wav");
        private SoundPlayer failPlayer = new SoundPlayer(@"../../Assets/failure_beep.wav");

        //constructor for the sign in page
        public SignInPage()
        {
            //display page
            InitializeComponent();
            //get and set if the device is connected to deviceConnected
            deviceConnected = MainWindow.AppWindow.getDeviceConnected();

            circleAnimationScan.Opacity = 0;
            circleAnimationUpdate.Opacity = 0;

            //string substringChecker = "108745";
            //substringChecker.Substring(1);
            //Console.Out.WriteLine("substringChecker value is" + substringChecker);

            //if the device is not connected, attempt connecting it
            if (!deviceConnected)
            {
                long DeviceID = 0;
                if (pcProxDLLAPI.usbConnect() == 1)
                {
                    DeviceID = pcProxDLLAPI.GetDID();
                    ushort proxDevice = pcProxDLLAPI.writeDevCfgToFile("prox_device_configuration");
                    MainWindow.AppWindow.textBox2.Text = "Connected to DeviceID: " + DeviceID;
                    this.deviceConnected = true;
                    MainWindow.AppWindow.setDeviceConnected(true);
                    if (MainWindow.AppWindow.getDatabaseUpdated())
                    {
                        labelID.Text = "Database Updated.\n\nSign in with Gordon ID";
                    }
                    else
                    {
                        labelID.Text = "Database Outdated. \n\n Update Database\nor\nSign in";
                    }
                }
                else
                {
                    MainWindow.AppWindow.textBox2.Text = "No device found";
                    labelID.Text = "RFID USB Device Not Found:\n\nPlease Connect Device!";
                }
            }

            if (!Directory.Exists(attendanceWriter.getDatabaseFilePath()))
            {
                buttonScan.IsEnabled = false;
                labelID.Text = "Database Not Initialized. \n\n Update Database";
            }

        }

        //function runs f the scan button is clicked 
        private void buttonScan_Click(object sender, RoutedEventArgs e)
        {
            //disables buttons and gets whether the device is already connected
            buttonScan.IsEnabled = false;
            buttonUpdate.IsEnabled = false;
            deviceConnected = MainWindow.AppWindow.getDeviceConnected();

            //if the device is not connected, try connecting the device
            //else display device not found and enable buttons
            if (!deviceConnected)
            {
                long DeviceID = 0;
                if (pcProxDLLAPI.usbConnect() == 1)
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
                    labelID.Text = "RFID USB Device Not Found:\n\nPlease Connect Device!";
                    buttonScan.IsEnabled = true;
                    buttonUpdate.IsEnabled = true;
                }
            }

            //if the device is connected
            if (deviceConnected)
            {
                //function to set authorized checker ids into a list
                authorizedCheckerIDs = new List<string>();
                authorizedCheckerIDs = attendanceWriter.getAuthorizedCheckersFromTextFile();



                //Comment out the following to skip authorization of sign in

                //starts the sign in timer to run function every 500ms
                scanTimer = new System.Timers.Timer();
                scanTimer.Elapsed += new ElapsedEventHandler(signInScan);
                scanTimer.Interval = 500;
                scanTimer.Enabled = true;

                circleAnimationScan.Opacity = 100;

                labelID.Text = "Scanning...";
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

        //function to move on to the event selection page
        private void successfulSignIn()
        {
            MainWindow.AppWindow.GoToEventsPage();
        }

        //function that runs a scan on a timer
        private void signInScan(object source, ElapsedEventArgs e)
        {

            //gets the bits of the proxID from a scan
            Byte[] Id = new Byte[8];
            int nBits = pcProxDLLAPI.getActiveID(8);

            //if bits are received from the scan, sets the scannedID variable to the 
            //proxID bits that we want
            if (nBits > 0)
            {
                String s = nBits.ToString() + " Bit ID [0]..[7]: ";
                String proxID = "";

                string checkerID = "";
                string checkersName = "";

                for (short i = 2; i > -1; i--)
                {
                    Id[i] = pcProxDLLAPI.getActiveID_byte(i);
                    s = s + String.Format("{0:X2}.", Id[i]);
                    proxID += String.Format("{0:X2}", Id[i]);

                }
                checkerID = Int32.Parse(proxID.Substring(1), System.Globalization.NumberStyles.HexNumber).ToString();
                Console.Out.WriteLine("checkers hex id:" + proxID);
                Console.Out.WriteLine("checkers decimal id:" + checkerID);
                Console.Out.WriteLine("checkers barcode: " + MainWindow.AppWindow.getAttendanceWriter().getStudentsBarcode(checkerID));

                //if the last ID scanned is the same ID, does not check it for authorization
                if (!lastID.Equals(checkerID))
                {
                    //perform successful scan if the authorized list contains the id scanned
                    //else says they are either not in the database or aren't  authorized
                    if (authorizedCheckerIDs.Contains(checkerID))
                    {
                        lastID = checkerID;

                        Dispatcher.Invoke(() =>
                        {
                            labelID.Foreground = new SolidColorBrush(Colors.ForestGreen);
                            checkersName = attendanceWriter.getAuthorizedCheckersName(checkerID);
                            labelID.Text = checkersName + "\nis an authorized Christian Life and Worship Credit Checker";
                            buttonCancelScan.IsEnabled = false;
                            Panel.SetZIndex(buttonCancelScan, -1);
                            Panel.SetZIndex(buttonProceed, 1);
                            buttonProceed.IsEnabled = true;
                            circleAnimationScan.Opacity = 0;
                        });

                        this.chapelCheckerId = checkerID;

                        scanTimer.Stop();
                        Dispatcher.Invoke(() =>
                        {
                            attendanceWriter.setChapelCheckerID(checkerID);
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
                                labelID.Text = checkersName + " is not in the Database\n\nTry Updating Database";
                            else
                                labelID.Text = checkersName + "\nis not an Authorized Christian Life and Worship Credit Checker";
                        });
                        playFailSound();
                    }
                }
            }
        }

        //function that runs if the update database button is clicked
        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            //disables buttons and gives mouse waiting symbol
            Mouse.OverrideCursor = Cursors.Wait;
            circleAnimationUpdate.Opacity = 100;
            labelID.Text = "Updating Database...";
            buttonScan.IsEnabled = false;
            buttonUpdate.IsEnabled = false;
            
            //pulls new sql data from databases
            backgroundWorker.DoWork += delegate (object s, DoWorkEventArgs args)
            {
                // Will be run on background thread
                SQLPuller sqlPuller = new SQLPuller();
                sqlPuller.pullAuthorizedCheckers();
                sqlPuller.pullEvents();
                sqlPuller.pullStudents();
            };

            backgroundWorker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                //enables buttons
                buttonUpdate.IsEnabled = true;
                buttonScan.IsEnabled = true;
                circleAnimationUpdate.Opacity = 0;

                //statements to display whether updates failed or succeeded
                if (MainWindow.AppWindow.getDatabaseUpdated() == true)
                {
                    attendanceWriter.CreateDateTextFile();
                    labelID.Text = "Database Update Successful\n\nSign in with Gordon ID";
                }
                else
                {
                    labelID.Text = "Database Update Failed\n\nCheck Internet Connection";
                }
                //give mouse normal cursor
                Mouse.OverrideCursor = null;
            };

            backgroundWorker.RunWorkerAsync();
           
        }

        //function if the proceed button is clicked
        private void buttonProceed_Click(object sender, RoutedEventArgs e)
        {
            successfulSignIn();
        }

        //function if the cancel scan button is clicked
        private void buttonCancelScan_Click(object sender, RoutedEventArgs e)
        {
            //stops timer for scanning and shows default page
            scanTimer.Stop();
            circleAnimationScan.Opacity = 0;
            labelID.Text = "Sign In\nor\nUpdate Database";
            lastID = "";
            buttonScan.IsEnabled = true;
            buttonUpdate.IsEnabled = true;
            buttonCancelScan.IsEnabled = false;
            Panel.SetZIndex(buttonCancelScan, -1);
        }
    }
}
