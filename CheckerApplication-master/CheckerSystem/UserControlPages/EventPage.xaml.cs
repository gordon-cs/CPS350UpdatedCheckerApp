/*
* EventPage.xaml.cs - class responsible for displaying all the events
* found in the events data file
*
* Responsible for displaying all of the gordon events last pulled from
* the database
*
* Allows the user to go back to the SignInPage or progress to the ScanPage
* if an event has been selected
*
* Authors: Jonathan Manos, Travis Pullen
* 
* Modified By: Jacob Bradley & Benjamin Hills
* Last Modified: 5/11/18
*/

using System.Collections.Generic;
using System.Media;
using System.Windows;
using System.Windows.Controls;

namespace CheckerApplication
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class EventPage : UserControl
    {
        AttendanceWriter attendanceWriter = MainWindow.AppWindow.getAttendanceWriter();
        private string eventID;
        private List<string> listEvents;
        private List<string> listEventIDs;

        //Constructor for the Select Event Page
        public EventPage()
        {
            InitializeComponent();
            
            // initializes the lists with event information and event ids
            listEvents = attendanceWriter.getEventInformationList();
            listEventIDs = attendanceWriter.getEventIDList();
            
            // populates the listBox with the event information
            for(int i = 0; i < listEvents.Count; i++)
            {
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Content = listEvents[i];
                eventListBox.Items.Add(listBoxItem);
            }
        }

        // function that is called when a selection is made in the list box .. of an event
        private void ListBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // get the id of the event
            int listBoxItemLocation = eventListBox.SelectedIndex;
            string id = listEventIDs[listBoxItemLocation];
            
            // give main window the event title so it can be used elswhere
            string eventTitle = listEvents[listBoxItemLocation];
            MainWindow.AppWindow.setEventName(eventTitle);

            // set the id of the event selected in the attendance text file
            this.eventID = id;
            attendanceWriter.setEventID(eventID);
        }

        // function that is called when OK button is clicked
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            // If an event is selected, continue onward to the scanning page
            if (!eventListBox.SelectedIndex.Equals(-1))
            {
                attendanceWriter.setEventID(eventID);
                MainWindow.AppWindow.GoToScanPage();
            }
            // If an event is not selected, play noise and display please select
            else
            {
                SystemSounds.Beep.Play();
                labelID.Text = "Please select an event.";
            }
        }

        //if the back button is clicked, return to the sign in page
        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.AppWindow.GoToSignInPage();
        }
    }
}
