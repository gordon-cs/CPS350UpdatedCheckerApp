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

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for UserControl2.xaml
    /// </summary>
    public partial class ResultsPage : UserControl
    {

        AttendanceWriter attendanceWriter = MainWindow.AppWindow.getAttendanceWriter();

        public ResultsPage()
        {
            InitializeComponent();
            attendanceWriter.omitMultipleEntries();
            textBlockSaveFile.Text += attendanceWriter.getAttendanceFilePath();
            labelEventTitle.Text = MainWindow.AppWindow.getEventName();
        }

        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.AppWindow.GoToSignInPage();
        }

        private void buttonOpenText_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(attendanceWriter.getAttendanceFilePath());
        }
    }
}
