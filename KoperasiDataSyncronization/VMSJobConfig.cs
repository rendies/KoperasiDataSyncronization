using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace KoperasiDataSyncronization
{
    public class VMSJobConfig
    {
        string conString;

        public VMSJobConfig()
        {
            conString = ConfigurationManager.AppSettings["ConfigDBLocal"];
        }

        public bool CheckIsScheduleJobMustRunning()
        {
            bool result = false;
            SqlConnection con = new SqlConnection(conString);
            try
            {
                SqlCommand com = new SqlCommand("SELECT count(*) as counter FROM [dbo].[vms_sync_config] WHERE is_active = 1 AND schedule_time BETWEEN '" + DateTime.Now.ToString("HH:mm:ss") + "' AND '" + DateTime.Now.AddHours(1).ToString("HH:mm:ss") + "'", con);
                com.CommandTimeout = 0;
                com.CommandType = CommandType.Text;

                con.Open();
                SqlDataReader reader = com.ExecuteReader();
                while (reader.Read())
                {
                    if (int.Parse(reader["counter"].ToString()) > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception err)
            {
                con.Close();
            }
            finally
            {
                con.Close();
            }

            return result;
        }

        public bool InsertLog(DateTime time, string type, string logs)
        {
            bool result = false;
            SqlConnection con = new SqlConnection(conString);
            try
            {
                SqlCommand com = new SqlCommand("INSERT INTO [dbo].[vms_sync_job_logs] ([timestamp],[type],[log_message]) VALUES ('" + time.ToString("yyyy-MM-dd hh:mm:ss") + "', '" + type + "', '" + logs + "'); ", con);
                com.CommandTimeout = 0;
                com.CommandType = CommandType.Text;

                con.Open();
                
                if (com.ExecuteNonQuery() > 0)
                {
                    result = true;
                }
                
            }
            catch (Exception err)
            {
                con.Close();
            }
            finally
            {
                con.Close();
            }

            return result;
        }
    }
}
