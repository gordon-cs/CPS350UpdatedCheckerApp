/*
* ResultsPage.xaml.cs - class responsible for commanding final cleanup
* of the attendance file as well as displaying the number of students 
* scanned and the time spent scanning
*
* Class is not essential in the checking process but allows the user
* to know scanning is complete as well as return to the SignInPage to
* scan further
*
* Authors: Jonathan Manos, Travis Pullen
* Last Modified: 4/25/16
*
*/

using System.Windows;
using System.Windows.Controls;

namespace CheckerApplication
{
    /// <summary>
    /// Interaction logic for UserControl2.xaml
    /// </summary>
    public partial class ResultsPage : UserControl
    {

        AttendanceWriter attendanceWriter = MainWindow.AppWindow.getAttendanceWriter();

        //constructor for the results page
        public ResultsPage()
        {
            //displays page
            InitializeComponent();

            //function called to omit multiple entries in the attendance text file
            attendanceWriter.omitMultipleEntries();

            //gets and sets event name and attendance file path
            labelEventTitle.Text = MainWindow.AppWindow.getEventName();
            if(attendanceWriter.getNumberScanned() > 1)
                textBlockNumberScanned.Text = attendanceWriter.getNumberScanned() + " Students Scanned";
            if (attendanceWriter.getNumberScanned() == 1)
                textBlockNumberScanned.Text = attendanceWriter.getNumberScanned() + " Student Scanned";
            if (attendanceWriter.getNumberScanned() == 0)
                textBlockNumberScanned.Text = attendanceWriter.getNumberScanned() + " Students Scanned";

            textBlockScanTime.Text = "Scanning Time: " + MainWindow.AppWindow.getScanTime();
        }
        
        //function for when back to sign-in is clicked -- returns to sign in page
        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.AppWindow.GoToSignInPage();
        }
    }
}
