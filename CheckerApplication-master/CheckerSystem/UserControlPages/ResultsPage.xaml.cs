using System;
using System.Collections.Generic;
using System.IO;
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
using System.Diagnostics;

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
