using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace WpfApplication2
{
    public class SQLPuller
    {
        string connectionString4NoUserIDNoPassword;
        string password = "Gordon2016";
        string userName = "chapelchecker";
        string SQLConnectionString;
        List<Tuple<string, string, string, string>> eventList;
        Tuple<string, string, string, string> eventTuple;
        List<Tuple<string, string, string>> authorizedCheckers;
        Tuple<string, string, string> checkerTuple;
        List<Tuple<string, string, string>> allStudents;
        Tuple<string, string, string> studentTuple;
        List<string> authorizedIDs;

        public SQLPuller()
        {
        }

        public List<string> getAuthorizedIDs()
        {
            return authorizedIDs;
        }

        public List<Tuple<string,string,string>> getAuthorizedCheckers()
        {
            return authorizedCheckers;
        }

        public List<Tuple<string,string,string,string>> getEvents()
        {
            return eventList;
        }

        public void pullEvents()
        {
            string connectionString = null;
            SqlConnection connection;
            SqlCommand command;
            string sql = null;
            SqlDataReader dataReader;
            
            eventList = new List<Tuple<string, string, string, string>>();
            eventTuple = new Tuple<string, string, string, string>("","","","");

            //connectionString = "Server=adminprodsql; Database=Attendum; Integrated Security=SSPI;";
            //connectionString = "Server=tcp:adminprodsql.database.windows.net,1433; Database=attendum; Connection Timeout=10; Encrypt=True; TrustServerCertificate=False;";
            connectionString = "Data Source=adminprodsql; Initial Catalog=Attendum; Integrated Security=SSPI;";
            //SQLConnectionString = "Password=" + password + ';' + "User ID=" + userName + ';' + connectionString;
            //SQLConnectionString = "Data Source=adminprodsql;Initial Catalog=Attendum;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;";


            sql = @"select sEventID, sTitle, dtStart, dtEnd
                    from Attendum.dbo.Events
                    where abs(datediff(day, dtStart, dtEnd)) <= (31 * 1)
                    and dtStart >= getdate() - 7
                    order by dtStart";
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                command = new SqlCommand(sql, connection);
                dataReader = command.ExecuteReader();
                Console.WriteLine("TRYING EVENT SQL");
                while (dataReader.Read())
                {
                    //Console.WriteLine(dataReader.GetValue(0).ToString() + dataReader.GetValue(1).ToString() +
                    //  dataReader.GetValue(2).ToString() + dataReader.GetValue(3).ToString());
                    eventTuple = Tuple.Create
                         (dataReader.GetValue(0).ToString(), dataReader.GetValue(1).ToString(),
                         dataReader.GetValue(2).ToString(), dataReader.GetValue(3).ToString());
                    eventList.Add(eventTuple);
                    //Console.WriteLine("DID IT");
                }
                dataReader.Close();
                command.Dispose();
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.Out.Write("Can not open connection ! ");
            }
        }

        public void pullAuthorizedCheckers()
        {
            string connectionString = null;
            SqlConnection connection;
            SqlCommand command;
            string sql = null;
            SqlDataReader dataReader;

            authorizedCheckers = new List<Tuple<string, string, string>>();
            checkerTuple = new Tuple<string, string, string>("", "", "");
            authorizedIDs = new List<string>();

            string decID;
            string hexValue;

            //connectionString = "Server=adminprodsql; Database=Attendum; Integrated Security=SSPI;";
            //connectionString = "Server=tcp:adminprodsql.database.windows.net,1433; Database=attendum; Connection Timeout=10; Encrypt=True; TrustServerCertificate=False;";
            connectionString = "Data Source=adminprodsql; Initial Catalog=Attendum;  Integrated Security=SSPI;";
            //SQLConnectionString = "Password=" + password + ';' + "User ID=" + userName + ';' + connectionString;
            //SQLConnectionString = "Data Source=adminprodsql;Initial Catalog=Attendum;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;";



            sql = @"select a.validine_id, a.firstname, a.lastname
                    from Attendum.dbo.collectors as c
                    inner join
                    Attendum.dbo.account as a
                    on c.sCollectorAltPin = a.gordon_id";
            connection = new SqlConnection(connectionString);
            Console.Out.WriteLine("TRYING CHECKERS SQL");
            try
            {
                connection.Open();
                command = new SqlCommand(sql, connection);
                dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    //Console.WriteLine(dataReader.GetValue(0).ToString() + dataReader.GetValue(1).ToString() +
                    //     dataReader.GetValue(2).ToString());

                    decID = dataReader.GetString(0);
                    hexValue = Int32.Parse(decID).ToString("X");
                    authorizedIDs.Add(hexValue);
                    
                    
                    checkerTuple = Tuple.Create
                         (dataReader.GetValue(0).ToString(), dataReader.GetValue(1).ToString(),
                          dataReader.GetValue(2).ToString());
                    authorizedCheckers.Add(checkerTuple);

                }
                dataReader.Close();
                command.Dispose();
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.Out.Write("Can not open connection ! ");
            }
        }

        public void pullStudents()
        {
            string connectionString = null;
            SqlConnection connection;
            SqlCommand command;
            string sql = null;
            SqlDataReader dataReader;

            allStudents = new List<Tuple<string, string, string>>();
            studentTuple = new Tuple<string, string, string>("", "", "");

            string decID;
            string hexValue;

            //connectionString = "Server=adminprodsql; Database=Attendum; Integrated Security=SSPI;";
            //connectionString = "Server=tcp:adminprodsql.database.windows.net,1433; Database=attendum; Connection Timeout=10; Encrypt=True; TrustServerCertificate=False;";
            connectionString = "Data Source=adminprodsql; Initial Catalog=Attendum;  Integrated Security=SSPI;";
            //SQLConnectionString = "Password=" + password + ';' + "User ID=" + userName + ';' + connectionString;
            //SQLConnectionString = "Data Source=adminprodsql;Initial Catalog=Attendum;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;";



            sql = @"select a.validine_id, a.firstname, a.lastname
                    from Attendum.dbo.collectors as c
                    inner join
                    Attendum.dbo.account as a
                    on c.sCollectorAltPin = a.gordon_id";
            connection = new SqlConnection(connectionString);
            Console.Out.WriteLine("TRYING CHECKERS SQL");
            try
            {
                connection.Open();
                command = new SqlCommand(sql, connection);
                dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    //Console.WriteLine(dataReader.GetValue(0).ToString() + dataReader.GetValue(1).ToString() +
                    //     dataReader.GetValue(2).ToString());

                    decID = dataReader.GetString(0);
                    hexValue = Int32.Parse(decID).ToString("X");

                    studentTuple = Tuple.Create
                         (hexValue, dataReader.GetValue(1).ToString(),
                          dataReader.GetValue(2).ToString());
                    allStudents.Add(checkerTuple);

                }
                dataReader.Close();
                command.Dispose();
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.Out.Write("Can not open connection ! ");
            }
        }

    }

}
