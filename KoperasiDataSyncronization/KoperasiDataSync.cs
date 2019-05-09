using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace KoperasiDataSyncronization
{
    public partial class KoperasiDataSync : ServiceBase
    {
        Timer timer = new Timer();
        VMSJobConfig config = new VMSJobConfig();

        public KoperasiDataSync()
        {
            InitializeComponent();

        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Service is started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = int.Parse(ConfigurationManager.AppSettings["Timer_Elapse"]); //number in milisecinds  
            timer.Enabled = true;            
        }
        public void TestOnElapsedTime()
        {


            WriteToFile("Service is recall =============== at " + DateTime.Now);

            if (config.CheckIsScheduleJobMustRunning())
            {
                WriteToFile("Service Started " + DateTime.Now);
                VMSAPIService result = new VMSAPIService();
                WriteToFile("Terminate User Job Sync: " + result.terminanteUserResult.ToString() + " at: " + DateTime.Now);
                WriteToFile("Update User Job Sync: " + result.updateUserResult.ToString() + " at: " + DateTime.Now);
                config.InsertLog(DateTime.Now, "API Terminate", "Terminate User Job Sync: " + result.terminanteUserResult.ToString() + " at: " + DateTime.Now);
                config.InsertLog(DateTime.Now, "API Update", "Update User Job Sync: " + result.updateUserResult.ToString() + " at: " + DateTime.Now);
            }
            WriteToFile("Service Recall end =============== " + DateTime.Now);
        }

        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            

            WriteToFile("Service is recall =============== at " + DateTime.Now);

            if(config.CheckIsScheduleJobMustRunning())
            {
                WriteToFile("Service Started " + DateTime.Now);
                VMSAPIService result = new VMSAPIService();
                WriteToFile("Terminate User Job Sync: " + result.terminanteUserResult.ToString() + " at: " + DateTime.Now);
                WriteToFile("Update User Job Sync: " + result.updateUserResult.ToString() + " at: " + DateTime.Now);
                config.InsertLog(DateTime.Now, "API Terminate", "Terminate User Job Sync: " + result.terminanteUserResult.ToString() + " at: " + DateTime.Now);
                config.InsertLog(DateTime.Now, "API Update", "Update User Job Sync: " + result.updateUserResult.ToString() + " at: " + DateTime.Now);
            }
            WriteToFile("Service Recall end =============== " + DateTime.Now);
        }
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
