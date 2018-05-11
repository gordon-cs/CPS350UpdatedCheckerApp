/*
* AttendanceWriter.cs - class responsible for reading from and writing to 
* text files
*
* Responsible for creating directories and text files
*
* MainWindow holds the attendanceWriter class
* so to access attendanceWriter one must use:
* MainWindow.AppWindow.attendanceWriter
* Otherwise you would be using a new instance of the attendanceWriter
*
* Constants for directory names and file paths can be set, as well
* as the special character used for dividing the sql data text files
*
* Authors: Jonathan Manos, Travis Pullen
* Last Modified: 4/25/16
*
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CheckerApplication
{
    public class AttendanceWriter
    {
        //variables for the attendanceWriter
        private string attendancePath;
        private string line;
        private string chapelCheckerID;
        private string eventID;
        private int noCredit;
        private int numberScanned;

        //constant file paths
        private const string ATTENDANCEDIR = "C:/_system/attendance/";
        private const string DATABASEDIR = "C:/_system/db/";
        private const string EVENTSPATH = "C:/_system/db/Events.claw";
        private const string CHECKERSPATH = "C:/_system/db/Checkers.claw";
        private const string STUDENTSPATH = "C:/_system/db/Students.claw";
        private const string DATEPATH = "C:/_system/db/Date.claw";
        private const string TESTPATH = "C:/_system/db/Testing.claw";

        //unique character used to separate values in the sql data tables,
        //otherwise separating the values can be erroneous
        private const char SC = '†';

        //constructor for the attendanceWriter object
        public AttendanceWriter()
        {

        }

        //function that gets a unique ID for the attendance text file from the usb device
        private string getUniqueID()
        {
            string uniqueID = System.Net.Dns.GetHostName();

            return uniqueID;
        }

        //function that gets a unique ID for the attendance text file from the usb device
        public int getNumberScanned()
        {
            return numberScanned;
        }

        //functions to create text files for the attendance file and the various SQL data text files
        public void CreateAttendanceTextFile()
        {
            if (!Directory.Exists(ATTENDANCEDIR))
            {
                Directory.CreateDirectory(ATTENDANCEDIR);
            }
            this.attendancePath = ATTENDANCEDIR + "Attendance_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + getUniqueID() + ".claw";
            File.Create(attendancePath).Close();

        }

        public void CreateEventsTextFile()
        {
            if (!Directory.Exists(DATABASEDIR))
            {
                Directory.CreateDirectory(DATABASEDIR);
            }
            File.Create(EVENTSPATH).Close();
        }

        public void CreateCheckersTextFile()
        {
            if (!Directory.Exists(DATABASEDIR))
            {
                Directory.CreateDirectory(DATABASEDIR);
            }
            File.Create(CHECKERSPATH).Close();
        }

        public void CreateStudentsTextFile()
        {
            if (!Directory.Exists(DATABASEDIR))
            {
                Directory.CreateDirectory(DATABASEDIR);
            }
            File.Create(STUDENTSPATH).Close();
        }

        public void CreateDateTextFile()
        {
            if (!Directory.Exists(DATABASEDIR))
            {
                Directory.CreateDirectory(DATABASEDIR);
            }
            if (File.Exists(DATEPATH))
            {
                File.Delete(DATEPATH);
            }

            File.Create(DATEPATH).Close();
            StreamWriter file = new StreamWriter(DATEPATH, true);

            string dateTime = DateTime.Now.ToString();
            file.WriteLine(dateTime);
            file.Close();

            MainWindow.AppWindow.textBox1.Text = "Updated: " + dateTime;
        }

        //functions to set variables for the attendance text file
        public void setChapelCheckerID(string id)
        {
            id = getStudentsBarcode(id);
            Console.Out.WriteLine("Barcode ID:" + id);
            this.chapelCheckerID = id;
        }

        public void setEventID(string id)
        {
            this.eventID = id;
        }

        public void setNoCredit(int a)
        {
            this.noCredit = a;
        }

        //writes attendance text file by taking current scannedID from scanPage,
        //the current dateTime, noCredit boolean, the event id, and the claw checkers id
        //and writes a line to the attendance text file
        public void WriteAttendanceTextFile(string studentID)
        {
            studentID = getStudentsBarcode(studentID);
            string dateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss");
            StreamWriter file = new StreamWriter(attendancePath, true);

            file.WriteLine(studentID + "," + chapelCheckerID + "," + noCredit + "," + eventID + "," + dateTime);
            file.Close();
        }

        //functions that write the various text files from SQL pulls containing data for the checker system
        public void WriteEventsTextFile(string eventID, string eventTitle, string shortTitle, string eventStart, string eventEnd)
        {
            StreamWriter file = new StreamWriter(EVENTSPATH, true);

            file.WriteLine(eventID + SC + eventTitle + SC + shortTitle + SC + eventStart + SC + eventEnd);
            file.Close();
        }

        public void WriteCheckersTextFile(string checkerID, string checkerBarcode, string lastName, string firstName)
        {
            StreamWriter file = new StreamWriter(CHECKERSPATH, true);

            file.WriteLine(checkerID + SC + checkerBarcode + SC + lastName + SC + firstName);
            file.Close();
        }

        public void WriteStudentsTextFile(string studentID, string studentBarcode, string lastName, string firstName)
        {
            StreamWriter file = new StreamWriter(STUDENTSPATH, true);

            file.WriteLine(studentID + SC + studentBarcode + SC + lastName + SC + firstName);
            file.Close();
        }


        //gets various paths for different text files containing information
        public string getAttendanceFilePath()
        {
            return this.attendancePath;
        }

        public string getEventsFilePath()
        {
            return EVENTSPATH;
        }

        public string getCheckersFilePath()
        {
            return CHECKERSPATH;
        }

        public string getStudentsFilePath()
        {
            return STUDENTSPATH;
        }

        public string getDateFilePath()
        {
            return DATEPATH;
        }
        public string getDatabaseFilePath()
        {
            return DATABASEDIR;
        }

        //this function removes any duplicate entries, and if there is an entry for someone
        //for credit and no credit, only the entry for no credit will be kept
        public void omitMultipleEntries()
        {

            numberScanned = 0;

            //initialize variables for the omit entries function
            StreamReader sr = new StreamReader(attendancePath);
            List<string> listBarcode = new List<string>();
            string[] values = new string[5];

            // Read through the file and find all the barcodeIDs
            while ((line = sr.ReadLine()) != null)
            {
                values = line.Split(',');
                string barcode = values[0];
                listBarcode.Add(barcode);
            }
            sr.Close();

            // find all the unique barcode ids and put them in a list
            List<string> listBarcodeUnique = listBarcode.Distinct().ToList();

            // a temporary path for the deDuplicated text file
            string tempPath = "temp.txt";

            //initializes streamWriter
            StreamWriter sw = new StreamWriter(tempPath);

            //initializes variables used for deduplicating
            string superLine = "";
            Boolean doesContainId = false;
            Boolean doesContainNoCredit = false;          

            //for each barcode in the unique barcode list
            foreach (string s in listBarcodeUnique)
            {
                //initializes new stream reader in the attendance text file
                sr = new StreamReader(attendancePath);

                //reads through each line of attendance text file and stops reading
                //if it finds an entry for the current barcode that has no credit
                while ((line = sr.ReadLine()) != null && !doesContainNoCredit)
                {
                    //splits the attendance text file by a ',' to get the individual values
                    values = line.Split(',');

                    //if statement that stores the current line into the superLine string if
                    //
                    //the current line Equals the current barcode from the unique list
                    //
                    //a line for credit has not already been found in the text file
                    //
                    //the no credit value is 0, so the entry is for credit
                    //
                    //an entry for no credit has not been found yet
                    //
                    if (values[0].Equals(s) && !doesContainId && values[2].Equals("0") && !doesContainNoCredit)
                    {
                        superLine = line;
                        doesContainId = true;
                    }

                    //if statement that stores the current line into the superLine string if
                    //
                    //the current line Equals the current barcode from the unique list
                    //
                    //the no credit value is 1, so the entry is for credit
                    //
                    //an entry for no credit has not been found yet
                    //
                    else if (values[0].Equals(s) && values[2].Equals("1") && !doesContainNoCredit)
                    {
                        superLine = line;
                        doesContainNoCredit = true;
                    }
                }

                //writes the super line to the temporary text file and closes the reader
                numberScanned++;
                sw.WriteLine(superLine);
                sr.Close();

                //sets up the variables for the next unique ID
                superLine = "";
                doesContainId = false;
                doesContainNoCredit = false;

            }

            //closes the stream writer
            sw.Close();

            //deletes the previous attendance text file
            File.Delete(attendancePath);
            //copies over the new attendance text file into the 
            //the previous text files path
            File.Move(tempPath, attendancePath);

            if (numberScanned == 0)
                File.Delete(attendancePath);
        }
        
        //function that returns a list of a select few temporary checkers
        public List<string> getTempCheckersFromTextFile()
        {

            string tempCheckerPath = "authorized_checkers_list.txt";

            if (!File.Exists(tempCheckerPath))
            {
                File.Create(tempCheckerPath).Close();
            }

            StreamReader sr = new StreamReader(tempCheckerPath);
            List<string> authorizedList = new List<string>();

            while ((line = sr.ReadLine()) != null)
            {
                authorizedList.Add(line);
            }
            sr.Close();
            return authorizedList;
        }

        //function that returns a list of the actual authorized checkers
        public List<string> getAuthorizedCheckersFromTextFile()
        {

            if (!File.Exists(CHECKERSPATH))
            {
                File.Create(CHECKERSPATH).Close();
            }

            StreamReader sr = new StreamReader(CHECKERSPATH);
            List<string> authorizedList = new List<string>();

            while ((line = sr.ReadLine()) != null)
            {
                authorizedList.Add(line.Substring(0, 5));
            }
            sr.Close();
            return authorizedList;
        }

        //function that returns the name string of authorized checkers
        //given their scannedID
        public string getAuthorizedCheckersName(string checkerID)
        {

            if (!File.Exists(CHECKERSPATH))
            {
                File.Create(CHECKERSPATH).Close();
            }

            StreamReader sr = new StreamReader(CHECKERSPATH);
            string checkersName = "";
            string[] values = new string[5];

            while ((line = sr.ReadLine()) != null)
            {
                values = line.Split(SC);
                if (values[0].Equals(checkerID))
                    checkersName = (values[2] + " " + values[3]);
            }
            sr.Close();
            if (checkersName.Length == 0)
                checkersName = "Student";
            return checkersName;
        }

        //function that returns the name string of any scanned student
        //given their scannedID
        public string getStudentsName(string studentID)
        {

            if (!File.Exists(STUDENTSPATH))
            {
                File.Create(STUDENTSPATH).Close();
            }

            StreamReader sr = new StreamReader(STUDENTSPATH);
            string studentsName = "";
            string[] values = new string[5];

            while ((line = sr.ReadLine()) != null)
            {
                values = line.Split(SC);
                if (values[0].Equals(studentID))
                    studentsName = (values[2] + " " + values[3]);
            }
            sr.Close();
            if (studentsName.Length == 0)
                studentsName = "Student";
            return studentsName;
        }
        
        //function that gets the barcode string of a student,
        //given the scannedID
        public string getStudentsBarcode(string scannedID)
        {

            if (!File.Exists(STUDENTSPATH))
            {
                File.Create(STUDENTSPATH).Close();
            }

            StreamReader sr = new StreamReader(STUDENTSPATH);
            string studentsBarcode = "";
            string[] values = new string[5];

            while ((line = sr.ReadLine()) != null)
            {
                values = line.Split(SC);
                if (values[0].Equals(scannedID))
                    studentsBarcode = (values[1]);
            }
            sr.Close();
            return studentsBarcode;
        }

        //function that gets a list of event descriptions for the events page
        public List<string> getEventInformationList()
        {

            if (!File.Exists(EVENTSPATH))
            {
                File.Create(EVENTSPATH).Close();
            }

            StreamReader sr = new StreamReader(EVENTSPATH);
            List<string> events = new List<string>();
            string[] values = new string[5];

            while ((line = sr.ReadLine()) != null)
            {
                values = line.Split(SC);
                int length = values[3].Count();
                if (length == 19)
                    events.Add(values[3] + "       " + values[2]);
                else if (length == 20)
                    events.Add(values[3] + "     " + values[2]);
                else if (length == 21)
                    events.Add(values[3] + "   " + values[2]);
                else if (length == 22)
                    events.Add(values[3] + "  " + values[2]);
                else if (length == 23)
                    events.Add(values[3] + " " + values[2]);
                else
                    events.Add(values[3] + " " + values[2]);
            }
            sr.Close();
            return events;
        }

        //function that gets a list of all the eventIDs 
        public List<string> getEventIDList()
        {

            if (!File.Exists(EVENTSPATH))
            {
                File.Create(EVENTSPATH).Close();
            }

            StreamReader sr = new StreamReader(EVENTSPATH);
            List<string> eventIDList = new List<string>();
            string[] values = new string[5];

            while ((line = sr.ReadLine()) != null)
            {
                values = line.Split(SC);
                eventIDList.Add(values[0]);
            }
            sr.Close();
            return eventIDList;
        }

        //function that gets the barcode string of a student,
        //given the scannedID
        public string getDate()
        {

            if (!File.Exists(DATEPATH))
            {
                return "Unknown";
            }

            StreamReader sr = new StreamReader(DATEPATH);
            string date = "";

            while ((line = sr.ReadLine()) != null)
            {
                date = line;
            }
            sr.Close();
            return date;
        }

        //function called on shutdown that deletes the current attendance file
        // if it is empty otherwise omitting multiple entries
        public void cleanUpAttendanceFile()
        {
            if (File.Exists(attendancePath))
            {
                if (new FileInfo(attendancePath).Length == 0)
                {
                    File.Delete(attendancePath);
                }
                else
                {
                    omitMultipleEntries();
                }
            }
        }

    }
}
