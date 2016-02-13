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
        private string eventID;
        List<string> listEvents;
        List<string> listEventIDs;
        bool listBoxItemClicked = false;

        public EventPage()
        {
            
            InitializeComponent();

            
            
            listEvents = attendanceWriter.getEventInformationList();
            listEventIDs = attendanceWriter.getEventIDList();
            
            for(int i = 0; i < listEvents.Count; i++)
            {
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.FontSize = 16;
                listBoxItem.Content = listEvents[i];
                listBox.Items.Add(listBoxItem);
            }
            
            

        }

        private void ListBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // ... Get RadioButton reference.
            int listBoxItemLocation = listBox.SelectedIndex;
            string id = listEventIDs[listBoxItemLocation];

            string eventTitle = listEvents[listBoxItemLocation];
            MainWindow.AppWindow.setEventName(eventTitle);

            

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
