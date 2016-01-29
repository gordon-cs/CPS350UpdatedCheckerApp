using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication2
{
    public class AttendanceWriter
    {

        private String path;
        private string line;
        private string chapelCheckerID;
        private string eventID;
        private int noCredit;
        private int hexStudentID;
        private int hexChapelCheckerID;


        public AttendanceWriter()
        {
        }


        public void CreateTextFile()
        {
            this.path = "Attendance_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            File.Create(path).Close();

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

        public void WriteTextFile(String s)
        {
            string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            StreamWriter file = new StreamWriter(path, true);

            file.WriteLine(s + "," + chapelCheckerID + "," + noCredit + "," + eventID + "," + dateTime);
            file.Close();
            

        }

        public string getFilePath()
        {
            return this.path;
        }

        public void omitMultipleEntries()
        {

            // Read the file and find all prox ids 
            StreamReader sr = new StreamReader(path);
            List<string> proxIdList = new List<string>();
            List<string> fullLineList = new List<string>();
            


            while ((line = sr.ReadLine()) != null)
            {

                string proxId = line.Substring(0, 4);
                proxIdList.Add(proxId);
                fullLineList.Add(line);
            }

            sr.Close();

            // find all the unique prox ids and put them in a list

            List<string> proxIdListUnique = proxIdList.Distinct().ToList();

            string tempPath = "temp.txt";

            
            StreamWriter sw = new StreamWriter(tempPath);

            Boolean doesContainId = false;

            foreach (string s in proxIdListUnique)
            {

                //sw.WriteLine(s);

                sr = new StreamReader(path);

                while ((line = sr.ReadLine()) != null && !doesContainId)
                {

                    if (line.Substring(0,4).Contains(s) && !doesContainId)
                    {
                        //sw.WriteLine(line + " contains " + s + "and is the first occurence of" + line);
                        sw.WriteLine(line);
                        doesContainId = true;
                    }
                    else
                    {
                        //sw.WriteLine("OMITTED LINE: " + line + "because it does not contain" + s + " or it is a duplicate");
                    }
                    
                }

                sr.Close();
                doesContainId = false;

            }

            sw.Close();

            File.Delete(path);
            File.Move(tempPath, path);


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
