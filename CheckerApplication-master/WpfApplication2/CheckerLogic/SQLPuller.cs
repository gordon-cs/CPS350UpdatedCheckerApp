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
        AttendanceWriter attendanceWriter = MainWindow.AppWindow.getAttendanceWriter();


        public SQLPuller()
        {
        }

 

        public void pullEvents()
        {
            string connectionString = null;
            SqlConnection connection;
            SqlCommand command;
            string sql = null;
            SqlDataReader dataReader;


            connectionString = "Data Source=adminprodsql; Initial Catalog=Attendum; Integrated Security=SSPI;";


            sql = @"select sEventID, sTitle, dtStart, dtEnd
                    from Attendum.dbo.Events
                    where abs(datediff(day, dtStart, dtEnd)) <= (31 * 1)
                    and dtStart >= getdate() - 8
                    order by dtStart";
            connection = new SqlConnection(connectionString);
            try
            {
                Console.WriteLine("TRYING EVENT SQL");
                connection.Open();
                command = new SqlCommand(sql, connection);
                dataReader = command.ExecuteReader();

                attendanceWriter.CreateEventsTextFile();

                while (dataReader.Read())
                {

                    attendanceWriter.WriteEventsTextFile(dataReader.GetValue(0).ToString(), dataReader.GetValue(1).ToString(),
                                                        dataReader.GetValue(2).ToString(), dataReader.GetValue(3).ToString());

                }
                dataReader.Close();
                command.Dispose();
                connection.Close();
                Console.Out.WriteLine("EVENT SQL SUCCESS ");
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("EVENT SQL FAILED ");
            }
            
        }

        public void pullAuthorizedCheckers()
        {
            string connectionString = null;
            SqlConnection connection;
            SqlCommand command;
            string sql = null;
            SqlDataReader dataReader;

            string decID;
            string hexValue;

            connectionString = "Data Source=adminprodsql; Initial Catalog=Attendum;  Integrated Security=SSPI;";



            sql = @"select a.validine_id, a.barcode, a.firstname, a.lastname
                    from Attendum.dbo.collectors as c
                    inner join
                    Attendum.dbo.account as a
                    on c.sCollectorAltPin = a.gordon_id";
            connection = new SqlConnection(connectionString);          
            try
            {
                Console.Out.WriteLine("TRYING CHECKERS SQL");
                connection.Open();
                command = new SqlCommand(sql, connection);
                dataReader = command.ExecuteReader();

                attendanceWriter.CreateCheckersTextFile();

                while (dataReader.Read())
                {
                    
                    attendanceWriter.WriteCheckersTextFile(dataReader.GetValue(0).ToString(), dataReader.GetValue(1).ToString(),
                                                        dataReader.GetValue(2).ToString(), dataReader.GetValue(3).ToString());

                }
                dataReader.Close();
                command.Dispose();
                connection.Close();
                Console.Out.WriteLine("CHECKERS SQL SUCCESS ");
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("CHECKERS SQL FAILED ");
            }
        }

        public void pullStudents()
        {
            string connectionString = null;
            SqlConnection connection;
            SqlCommand command;
            string sql = null;
            SqlDataReader dataReader;


            string decID;
            string hexValue;

            connectionString = "Data Source=adminprodsql; Initial Catalog=Attendum;  Integrated Security=SSPI;";



            sql = @"select validine_id, barcode, firstname, lastname
                    from Attendum.dbo.account";
            connection = new SqlConnection(connectionString);
            
            try
            {
                Console.Out.WriteLine("TRYING STUDENTS SQL");
                connection.Open();
                command = new SqlCommand(sql, connection);
                dataReader = command.ExecuteReader();

                attendanceWriter.CreateStudentsTextFile();
                while (dataReader.Read())
                {

                    attendanceWriter.WriteStudentsTextFile(dataReader.GetValue(0).ToString(), dataReader.GetValue(1).ToString(),
                                                        dataReader.GetValue(2).ToString(), dataReader.GetValue(3).ToString());

                }
                dataReader.Close();
                command.Dispose();
                connection.Close();
                Console.Out.WriteLine("STUDENTS SQL SUCCESS ");

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("STUDENTS SQL FAILED ");
            }
        }

    }

}
