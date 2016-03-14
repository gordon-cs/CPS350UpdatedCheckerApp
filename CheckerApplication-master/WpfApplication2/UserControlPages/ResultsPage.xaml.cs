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
            textBlockSaveFile.Text += attendanceWriter.getAttendanceFilePath();
            labelEventTitle.Text = MainWindow.AppWindow.getEventName();
        }
        
        //function for when back to sign-in is clicked -- returns to sign in page
        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.AppWindow.GoToSignInPage();
        }

        //function for when open text file is clicked -- opens files
        private void buttonOpenText_Click(object sender, RoutedEventArgs e)
        {
            //starts process of opening attendance text file
            Process.Start(attendanceWriter.getAttendanceFilePath());
        }
    }
}
