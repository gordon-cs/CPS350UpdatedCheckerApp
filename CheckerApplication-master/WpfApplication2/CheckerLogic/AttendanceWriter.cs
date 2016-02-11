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

            string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            StreamWriter file = new StreamWriter(attendancePath, true);

            file.WriteLine(chapelCheckerID + "," + studentID + "," + noCredit + "," + eventID + "," + dateTime);
            file.Close();
        }

        public void WriteEventsTextFile(string eventID, string eventTitle, string eventStart, string eventEnd)
        {
            StreamWriter file = new StreamWriter(EVENTSPATH, true);

            file.WriteLine(eventID + "," + eventTitle + "," + eventStart + "," + eventEnd);
            file.Close();
        }

        public void WriteCheckersTextFile(string checkerID, string checkerBarcode, string lastName, string firstName)
        {
            StreamWriter file = new StreamWriter(CHECKERSPATH, true);

            file.WriteLine(checkerID + "," + checkerBarcode + "," + lastName + "," + firstName);
            file.Close();
        }

        public void WriteStudentsTextFile(string studentID, string studentBarcode, string lastName, string firstName)
        {
            StreamWriter file = new StreamWriter(STUDENTSPATH, true);

            file.WriteLine(studentID + "," + studentBarcode + "," + lastName + "," + firstName);
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
            List<string> proxIdList = new List<string>();
            List<string> fullLineList = new List<string>();
            


            while ((line = sr.ReadLine()) != null)
            {

                string proxId = line.Substring(0, 5);
                proxIdList.Add(proxId);
                fullLineList.Add(line);
            }

            sr.Close();

            // find all the unique prox ids and put them in a list

            List<string> proxIdListUnique = proxIdList.Distinct().ToList();

            string tempPath = "temp.txt";

            
            StreamWriter sw = new StreamWriter(tempPath);

            string superLine = "";
            Boolean doesContainId = false;
            Boolean doesContainNoCredit = false;
            int noCredit = 0;


            foreach (string s in proxIdListUnique)
            {

                //sw.WriteLine(s);

                sr = new StreamReader(attendancePath);
                




                while ((line = sr.ReadLine()) != null && !doesContainNoCredit)
                {

                        noCredit = Int32.Parse(line.Substring(12, 1));
                        //sw.WriteLine(line + "HERE: " + noCredit);
                    
                    

                    if (line.Substring(0,5).Contains(s) && !doesContainId && noCredit == 0 && !doesContainNoCredit)
                    {
                        //sw.WriteLine(line + " contains " + s + "and is the first occurence of" + line + "and is for credit" + noCredit + "and noCredit hasnt arrived" + doesContainNoCredit);
                        superLine = line;
                        doesContainId = true;                        
                    }
                    else if(line.Substring(0, 5).Contains(s) && noCredit == 1 && !doesContainNoCredit)
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
                authorizedList.Add(line.Substring(0,5));
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
                if(line.Contains(checkerID))
                     values = line.Split(',');
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
                    values = line.Split(',');
                studentsName = (values[2] + " " + values[3]);
            }

            return studentsName;
        }

    }
}
