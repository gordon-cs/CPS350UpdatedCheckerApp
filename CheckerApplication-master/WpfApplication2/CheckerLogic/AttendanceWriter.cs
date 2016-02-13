using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using RFIDeas_pcProxAPI;

namespace WpfApplication2
{
    public class AttendanceWriter
    {

        private string attendancePath;
        private const string EVENTSPATH = "Events.claw";
        private const string CHECKERSPATH = "Checkers.claw";
        private const string STUDENTSPATH = "Students.claw";
        private string line;
        private string chapelCheckerID;
        private string eventID;
        private int noCredit;
        private string hexStudentID;
        private string hexChapelCheckerID;
        private const char SC = '†';


        public AttendanceWriter()
        {
        }

        private string getUniqueID()
        {
            string uniqueID = pcProxDLLAPI.GetSN().ToString();

            return uniqueID;
        }

        public void CreateAttendanceTextFile()
        {
            this.attendancePath = "Attendance_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + getUniqueID() + ".claw";
            File.Create(attendancePath).Close();

        }

        public void CreateEventsTextFile()
        {
            File.Create(EVENTSPATH).Close();

        }

        public void CreateCheckersTextFile()
        {
            File.Create(CHECKERSPATH).Close();

        }

        public void CreateStudentsTextFile()
        {
            File.Create(STUDENTSPATH).Close();

        }

        public void setChapelCheckerID(string id)
        {
            id = getStudentsBarcode(id);
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

        public void WriteAttendanceTextFile(string studentID)
        {
            studentID = getStudentsBarcode(studentID);
            string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            StreamWriter file = new StreamWriter(attendancePath, true);

            file.WriteLine(chapelCheckerID + "," + studentID + "," + noCredit + "," + eventID + "," + dateTime);
            file.Close();
        }

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

        public void omitMultipleEntries()
        {

            // Read the file and find all prox ids 
            StreamReader sr = new StreamReader(attendancePath);
            List<string> listBarcode = new List<string>();
            string[] values = new string[5];


            while ((line = sr.ReadLine()) != null)
            {
                values = line.Split(',');
                string barcode = values[1];
                listBarcode.Add(barcode);
            }

            sr.Close();

            // find all the unique prox ids and put them in a list

            List<string> listBarcodeUnique = listBarcode.Distinct().ToList();

            string tempPath = "temp.txt";


            StreamWriter sw = new StreamWriter(tempPath);

            string superLine = "";
            Boolean doesContainId = false;
            Boolean doesContainNoCredit = false;
            int noCredit = 0;
            


            foreach (string s in listBarcodeUnique)
            {

                //sw.WriteLine(s);

                sr = new StreamReader(attendancePath);





                while ((line = sr.ReadLine()) != null && !doesContainNoCredit)
                {

                    values = line.Split(',');
                    //sw.WriteLine(line + "HERE: " + noCredit);



                    if (values[1].Contains(s) && !doesContainId && values[2].Equals("0") && !doesContainNoCredit)
                    {
                        //sw.WriteLine(line + " contains " + s + "and is the first occurence of" + line + "and is for credit" + noCredit + "and noCredit hasnt arrived" + doesContainNoCredit);
                        superLine = line;
                        doesContainId = true;
                    }
                    else if (values[1].Contains(s) && values[2].Equals("1") && !doesContainNoCredit)
                    {
                        //sw.WriteLine("OMITTED LINE: " + line + "because it does not contain" + s + " or it is a duplicate" );
                        superLine = line;
                        doesContainNoCredit = true;
                    }
                    else
                    {

                    }

                }


                sw.WriteLine(superLine);
                sr.Close();
                superLine = "";
                doesContainId = false;
                doesContainNoCredit = false;
                noCredit = 0;

            }

            sw.Close();

            File.Delete(attendancePath);
            File.Move(tempPath, attendancePath);


            sw.Close();

        }

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

            return authorizedList;
        }

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

            return authorizedList;
        }

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
                if (line.Contains(checkerID))
                    values = line.Split(SC);
                checkersName = (values[2] + " " + values[3]);
            }

            return checkersName;
        }

        public string getStudentsName(string checkerID)
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
                if (line.Contains(checkerID))
                    values = line.Split(SC);
                studentsName = (values[2] + " " + values[3]);
            }

            return studentsName;
        }

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
                if (line.Contains(scannedID))
                    values = line.Split(SC);
                studentsBarcode = (values[1]);
            }

            return studentsBarcode;
        }

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
                if(length == 20)
                    events.Add(values[3] + "    " + values[2]);
                if (length == 21)
                    events.Add(values[3] + "  " + values[2]);
                if (length == 22)
                    events.Add(values[3] + " " + values[2]);
            }

            return events;
        }

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

            return eventIDList;
        }

    }
}
