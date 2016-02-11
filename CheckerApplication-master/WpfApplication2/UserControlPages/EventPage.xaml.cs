using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class EventPage : UserControl
    {

        AttendanceWriter attendanceWriter = MainWindow.AppWindow.getAttendanceWriter();
        List<Tuple<string, string, string, string>> eventList;
        Tuple<string, string, string, string> eventTuple;
        private string eventID;
        Boolean listBoxItemClicked;

        public EventPage()
        {
            InitializeComponent();

            eventList = new List<Tuple<string, string, string, string>>();
            
            
            for(int i = 0; i < eventList.Count; i++)
            {



            }
            
            

        }

        private void ListBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // ... Get RadioButton reference.
            string listBoxContent = listBox.SelectedItem.ToString();
            int listBoxItemLength = listBoxContent.Length;
            string id = listBoxContent.Substring(listBoxItemLength - 5, 5);

            // ... Display button content as title.
            this.eventID = id;
            attendanceWriter.setEventID(eventID);
            listBoxItemClicked = true;
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            if (!listBox.SelectedIndex.Equals(-1))
            {
                attendanceWriter.setEventID(eventID);
                MainWindow.AppWindow.GoToScanPage();
            }
            else
            {
                SystemSounds.Beep.Play();
                labelID.Text = "Please select an event.";
            }
            
        }

        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.AppWindow.GoToSignInPage();
            listBoxItemClicked = false;
        }
    }
}
