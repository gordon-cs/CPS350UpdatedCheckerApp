using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace CheckerApplication
{
    public class SQLPuller
    {
        // grabs the attendanceWriter to class to write SQL to text files
        AttendanceWriter attendanceWriter = MainWindow.AppWindow.getAttendanceWriter();

        //constructor of the SQLPuller class
        public SQLPuller()
        {
        }
        
        //Function to pull the event information and write it to the event text file
        public void pullEvents()
        {
            string connectionString = null;
            SqlConnection connection;
            SqlCommand command;
            string sql = null;
            SqlDataReader dataReader;


            connectionString = "Data Source=adminprodsql; Initial Catalog=Attendum; Integrated Security=SSPI;";


            sql = @"select sEventID, sTitle, case when len(sTitle) > 40 then left(sTitle,40)+'...' else sTitle end as Truncated_sTitle, dtStart, dtEnd
                    from Attendum.dbo.Events
                    where abs( datediff(day, dtStart, dtEnd) ) <= (31 * 1)
                    and dtStart >= convert(varchar, getdate() -7, 101)
                    and dtStart <= case when month(getdate()) in ('1','2','3','4','5') then concat('06/30/', year(getdate())) else concat('12/31/', year(getdate())) end
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
                                                        dataReader.GetValue(2).ToString(), dataReader.GetValue(3).ToString(),
                                                        dataReader.GetValue(4).ToString());

                }
                dataReader.Close();
                command.Dispose();
                connection.Close();
                Console.Out.WriteLine("EVENT SQL SUCCESS ");
                MainWindow.AppWindow.setDatabaseUpdated(true);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("EVENT SQL FAILED ");
                MainWindow.AppWindow.setDatabaseUpdated(false);
            }
            
        }

        //Function to pull the authorized chapel checker information and write it to the chapel checker text file
        public void pullAuthorizedCheckers()
        {
            string connectionString = null;
            SqlConnection connection;
            SqlCommand command;
            string sql = null;
            SqlDataReader dataReader;

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
                MainWindow.AppWindow.setDatabaseUpdated(true);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("CHECKERS SQL FAILED ");
                MainWindow.AppWindow.setDatabaseUpdated(false);
            }
        }

        //Function to pull all current student information and write it to the students text file
        public void pullStudents()
        {
            string connectionString = null;
            SqlConnection connection;
            SqlCommand command;
            string sql = null;
            SqlDataReader dataReader;

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
                MainWindow.AppWindow.setDatabaseUpdated(true);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("STUDENTS SQL FAILED ");
                MainWindow.AppWindow.setDatabaseUpdated(false);
            }
        }

    }

}
