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
        private string eventsPath;
        private string checkersPath;
        private string studentsPath;
        private string line;
        private string chapelCheckerID;
        private string eventID;
        private int noCredit;
        private string hexStudentID;
        private string hexChapelCheckerID;
        private string uniqueDeviceID;


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
            this.attendancePath = "Attendance_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + getUniqueID() + ".txt";
            File.Create(attendancePath).Close();

        }

        public void CreateEventsTextFile()
        {
            this.attendancePath = "Attendance_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + getUniqueID() + ".txt";
            File.Create(attendancePath).Close();

        }

        public void CreateCheckersTextFile()
        {
            this.attendancePath = "Attendance_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + getUniqueID() + ".txt";
            File.Create(attendancePath).Close();

        }

        public void CreateAllStudentsTextFile()
        {
            this.attendancePath = "Attendance_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + getUniqueID() + ".txt";
            File.Create(attendancePath).Close();

        }

        public void setChapelCheckerID(string id)
        {
            this.hexChapelCheckerID = Int32.Parse(id, System.Globalization.NumberStyles.HexNumber).ToString();
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

        public void WriteAttendanceTextFile(String s)
        {
            this.hexStudentID = Int32.Parse(s, System.Globalization.NumberStyles.HexNumber).ToString();
            string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            StreamWriter file = new StreamWriter(attendancePath, true);

            file.WriteLine(hexStudentID + "," + hexChapelCheckerID + "," + noCredit + "," + eventID + "," + dateTime);
            file.Close();
            

        }

        public string getFilePath()
        {
            return this.attendancePath;
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

        public List<string> getAuthorizedFromTextFile()
        {

            string checkerPath = "authorized_checkers_list.txt";

            if (!File.Exists(checkerPath))
            {
                File.Create(checkerPath).Close();
            }
            
            StreamReader sr = new StreamReader(checkerPath);
            List<string> authorizedList = new List<string>();

            while ((line = sr.ReadLine()) != null)
            {
                authorizedList.Add(line);
            }

            return authorizedList;
        }

    }
}
