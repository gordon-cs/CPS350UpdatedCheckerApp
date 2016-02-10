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
        SQLPuller sqlPuller = MainWindow.AppWindow.getSQLPuller();
        List<Tuple<string, string, string>> authorizedCheckers;
        Tuple<string, string, string> checkerTuple;
        List<string> authorizedCheckerIDs;
        List<string> temporaryCheckersList;
        System.Timers.Timer aTimer;
        int counter = 0;
        string chapelCheckerId;
        private SoundPlayer happyPlayer = new SoundPlayer(@"../../Assets/blip.wav");
        private SoundPlayer failPlayer = new SoundPlayer(@"../../Assets/failure_beep.wav");


        public SignInPage()
        {
            InitializeComponent();
        }

        private void buttonScan_Click(object sender, RoutedEventArgs e)
        {
            authorizedCheckers = new List<Tuple<string, string, string>>();
            authorizedCheckers = sqlPuller.getAuthorizedCheckers();
            authorizedCheckerIDs = new List<string>();
            authorizedCheckerIDs = sqlPuller.getAuthorizedIDs();
            getTemporaryCheckers();

            //Comment out this to skip authorization of sign in
            
            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(signInScan);
            aTimer.Interval = 500;
            aTimer.Enabled = true;
            
            //Uncomment this to skip authorization to sign in
            //MainWindow.AppWindow.GoToEventsPage();

            buttonScan.IsEnabled = false;
        }

        
        private void getTemporaryCheckers()
        {
            this.temporaryCheckersList = attendanceWriter.getAuthorizedFromTextFile();
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
                for (short i = 1; i > -1; i--)
                {
                    Id[i] = pcProxDLLAPI.getActiveID_byte(i);
                    s = s + String.Format("{0:X2}.", Id[i]);
                    proxID += String.Format("{0:X2}", Id[i]);
                }


                if (temporaryCheckersList.Contains(proxID) || authorizedCheckerIDs.Contains(proxID))
                {
                    Dispatcher.Invoke(() => {
                        labelID.Text = counter + ": The prox ID: " + proxID + " is an authorized chapel checker";
                    });

                    this.chapelCheckerId = proxID;

                    aTimer.Stop();
                    Dispatcher.Invoke(() =>
                    {
                        attendanceWriter.setChapelCheckerID(proxID);
                        successfulSignIn();
                    });
                    happyPlayer.Play();
                }
                else
                {
                    Dispatcher.Invoke(() => {
                        labelID.Text = counter + ": The prox ID: " + proxID + " is not authorized";
                    });
                    failPlayer.Play();
                }

            }
            else
            {
                    Dispatcher.Invoke(() => {
                        labelID.Text = counter + ": No Card Found";
                    });
            }

            if (counter > 98)
                this.counter = 0;

        }

        

    }
}
