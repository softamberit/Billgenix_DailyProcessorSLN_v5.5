using BillingERPConn;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProccessorLib
{
    public class ProcessorList
    {

        public static List<ProcessorModel> GetAll()
        {
            var conUt = new DBUtility();

            var list = new List<ProcessorModel>();
            //var appSettings = ConfigurationManager.AppSettings;
            // var processors = appSettings.AllKeys.Where(s => s.Contains("Processor"));
            var dataTable = conUt.GetDataBySQLString("Select * from ProcessorConfiguration where isMonitoring=1");

            foreach (DataRow item in dataTable.Rows)
            {
                var model = new ProcessorModel();
                model.ProcessorExeName = item["ProcessorExeName"].ToString();
                model.PhysicalExePath = item["PhysicalExePath"].ToString();
                model.ID = Convert.ToInt32(item["ID"]);
                model.Interval = Convert.ToInt32(item["Interval"]);
                model.StartHour = item["StartHour"].ToString();
                model.IsResponding = Convert.ToBoolean(item["IsResponding"]);
                model.IsRestart = Convert.ToBoolean(item["IsRestart"]);
                model.ProcessResponse = item["ProcessResponse"].ToString();
                model.LastRunTime = Convert.ToDateTime(item["LastRunTime"].ToString());
                list.Add(model);
            }
          
            return list;
        }

        public static ProcessorLog GetProcessorLog()
        {
            var conUt = new DBUtility();

            var list = new List<ProcessorLog>();
            //var appSettings = ConfigurationManager.AppSettings;
            // var processors = appSettings.AllKeys.Where(s => s.Contains("Processor"));
            var dataTable = conUt.GetDataBySQLString(string.Format(@"Select * from BillingProcessor_Log
               where  cast(ProcessStartTime as date)= '{0}'", DateTime.Today.ToString("yyyy-MM-dd")));
            if (dataTable != null && dataTable.Rows.Count > 0)
            {

                foreach (DataRow item in dataTable.Rows)
                {
                    var model = new ProcessorLog();
                    model.LogDescriptionAfterProcess = item["LogDescriptionAfterProcess"].ToString();
                    model.LogDescriptionBeforeProcess = item["LogDescriptionBeforeProcess"].ToString();
                    model.MKErrorDescription = item["MKErrorDescription"].ToString();
                    model.PorcessErrorDescription = item["PorcessErrorDescription"].ToString();
                    model.ProcessStartTime = Convert.ToDateTime(item["ProcessStartTime"].ToString());
                    model.ProcessEndTime = Convert.ToDateTime(item["ProcessEndTime"]);
                    model.ProcessorID = Convert.ToInt32(item["ProcessorID"]);
                    model.NumberOfCustomer = Convert.ToInt32(item["NumberOfCustomer"].ToString());
                    list.Add(model);
                }
                return list.SingleOrDefault();
            }
            return null;
        }

        public static string SaveProcesorRespons(ProcessorModel model)
        {
            Hashtable ht = new Hashtable();

            ht.Add("PID", 1);
            ht.Add("ID", model.ID);
            ht.Add("IsResponding", model.IsResponding);
            ht.Add("ProcessResponse", model.ProcessResponse);
            ht.Add("LastRunTime", model.LastRunTime);
            ht.Add("IsRestart", model.IsRestart);
            var res = new DBUtility().ExecuteCommand(ht, "sp_UpdateProcessorResponse");

            return "OK";
        }
        public static string SaveProcesorResponsLog(ProcessorModel model)
        {
            Hashtable ht = new Hashtable();

            ht.Add("PID", 2);
            ht.Add("ID", model.ID);
            ht.Add("IsResponding", model.IsResponding);
            ht.Add("ProcessResponse", model.ProcessResponse);
            ht.Add("LastRunTime", model.LastRunTime);
            ht.Add("IsRestart", model.IsRestart);
            ht.Add("LogEntryDate", DateTime.Now);


            var res = new DBUtility().ExecuteCommand(ht, "sp_UpdateProcessorResponse");

            return "OK";
        }

    }
    public class ProcessorModel
    {
        public int ID { get; set; }
        public string ProcessName { get; set; }
        public string ProcessorExeName { get; set; }
        public string PhysicalExePath { get; set; }
        public string ProcessResponse { get; set; }
        public DateTime LastRunTime { get; set; }
        public bool IsMonitoring { get; set; }
        public int Interval { get; set; }
        public string StartHour { get; set; }
        public bool IsResponding { get; set; }
        public bool IsRestart { get; set; }
    }
    public class ProcessorLog
    {
        public int ProcessorID { get; set; }
        public string LogDescriptionBeforeProcess { get; set; }
        public string PorcessErrorDescription { get; set; }
        public string MKErrorDescription { get; set; }
        public string LogDescriptionAfterProcess { get; set; }
        public DateTime ProcessStartTime { get; set; }
        public DateTime ProcessEndTime { get; set; }

        public int NumberOfCustomer { get; set; }
    }
}
