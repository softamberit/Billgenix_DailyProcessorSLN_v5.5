using BillingERPConn;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Data;
using System.Windows.Forms;

namespace ReveProcessor
{
    enum Scheduler
    {
        EveryMinutes,
        EveryHour,
        EveryHalfDay,
        EveryDay,
        EveryWeek,
        EveryMonth,
        EveryYear,
        EveryTenMinutes,
        EveryEightHours
    }

    public partial class FrmMain : Form
    {
        readonly DBUtility _idb = new DBUtility();

        public FrmMain()
        {
            InitializeComponent();

            //  numHours.Value = DateTime.Now.Hour;
            //  numMins.Value = DateTime.Now.Minute;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
           
            schedule_Timer_Callback();
            ReveIpIprocessorCall();

        }

        #region ProcessorSchedular

        int _processInterval;
        private void schedule_Timer_Callback()
        {

            Hashtable ht = new Hashtable { { "ID", 5 } };
            DataTable dt = _idb.GetDataByProc(ht, "sp_getProcessConfigInfo");
            int startHour = int.Parse(dt.Rows[0]["StartHour"].ToString());
            int endHour = int.Parse(dt.Rows[0]["EndHour"].ToString());
            _processInterval = int.Parse(dt.Rows[0]["Interval"].ToString()); // Should be Milisecond

            int minutes = int.Parse(dt.Rows[0]["StartMin"].ToString());

            txtStartHour.Text = endHour.ToString();
            txtEndHour.Text = startHour.ToString();
            lblText.Text = @"This Process is run with " + _processInterval / 60000 + @" Minutes Interval.";

            timer1.Interval = minutes;  // milisecond

            //System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer
            //{
            //    Interval = ProcessInterval  ///milisecond
            //};

            var currHour = DateTime.Now.Hour;

            //if (currHour < startHour && currHour < endHour || currHour > startHour && currHour > endHour)
            //{
            timer1.Enabled = true;
            timer1.Tick += OnTimerEvent;
            //}
        }

        private void OnTimerEvent(object sender, EventArgs e)
        {
            timer1.Interval = _processInterval;
           

            //listBox1.Items.Add(strText);
            ReveIpIprocessorCall();

        }

        #endregion

        public void ReveIpIprocessorCall()
        {
            try
            {
                var strText = "**********Reve API Processor Scheduler has been started!*****" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt");
                WriteLogFile.WriteLog(strText);

                var baseUrl = _idb.GetDataBySQLString("Select ReveApiUrl From CompanyInfo Where Id=1");

                var reveApiUrl = baseUrl.Rows[0]["ReveApiUrl"].ToString();

                var dtCust = _idb.GetDataByProc("sp_getInfoReveApiCallHistory");

                var type = "&type=subscribePackage&packageID=";
                var crm = "&requestFromCRM=true";

                var processStartTime = DateTime.Now;
                var noOfCustomer = dtCust.Rows.Count;
                var mobile = "";
                var customerId = "";
                var successLogBeforeProcess = "";
                var SuccessLogAfterProcess = "";
                var processErrorlog = "";

                var url = "";
                var reveApIcAllHistoryId = "";

                var builder1 = new System.Text.StringBuilder();
                builder1.Append(successLogBeforeProcess);

                var builder2 = new System.Text.StringBuilder();
                builder2.Append(SuccessLogAfterProcess);

                foreach (DataRow dr in dtCust.Rows)
                {
                    try
                    {
                        reveApIcAllHistoryId = dr["ReveApICAllHistoryID"].ToString();
                        customerId = dr["CustomerID"].ToString();
                        mobile = dr["Mobile"].ToString();
                        var packageId = dr["RevePackageId"].ToString();

                        url = reveApiUrl + mobile + type + packageId + crm;

                        var client = new RestClient(url);
                        var request = new RestRequest(Method.GET);
                        IRestResponse response = client.Execute<ReveApiRequest>(request);

                        //var json = "[" + response.Content + "]";

                        dynamic data = JObject.Parse(response.Content);
                        string statusCode = data.status;

                        if (statusCode == "0")//
                        {
                            try
                            {


                                _idb.GetDataBySQLString(string.Format("Update REVEAPICALLHistory Set IsActive = 0, IsAPICallDone = 1, ApiCallDoneTime ='{0}',mobile='{1}'  Where ReveApICAllHistoryID = '{2}' select 'Success' Feedback ", DateTime.Now, mobile, reveApIcAllHistoryId));

                                //_idb.GetDataBySQLString("Update REVEAPICALLHistory Set IsActive = 0, IsAPICallDone = 1, ApiCallDoneTime = " +
                                //    "'" + DateTime.Now + "',mobile=" + mobile + "'  Where ReveApICAllHistoryID = '" + reveApIcAllHistoryId + "' select 'Success' Feedback ");

                                builder2.Append("RACHID: " + reveApIcAllHistoryId + " CID: " + customerId + " Mobile: " + mobile + " RPId: " + packageId + " StatusCode: " + statusCode + " # ");
                                WriteLogFile.WriteLog("RACHID: " + reveApIcAllHistoryId + " CID: " + customerId + " Mobile: " + mobile + " RPID: " + packageId + " StatusCode: " + statusCode + " # ");

                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }
                        }
                        else
                        {
                            //Added by Sobuj 2021-Apr-21
                            var ht1 = new Hashtable
                        {

                              { "ErrorMessage",statusCode}
                            , {"ReveApICAllHistoryID", reveApIcAllHistoryId }
                            , { "ReveApiUrl", url }
                            , { "CustomerID", customerId }
                            , { "CustMobile", mobile }
                            , { "LogEntryID", 50009 }

                        };

                            _idb.GetDataByProc(ht1, "sp_InsertReveProcessorErrorLog");

                            processErrorlog += response.ErrorException == null ? "" : response.ErrorException.Message + "# ";
                        }

                        builder1.Append("RACHID: " + reveApIcAllHistoryId + " CID: " + customerId + " Mobile: " + mobile + " RPId: " + packageId + " StatusCode: " + statusCode + " # ");
                    }
                    catch (Exception ex)
                    {
                        var ht1 = new Hashtable
                        {

                              { "ErrorMessage", ex.Message }
                            , {"ReveApICAllHistoryID", reveApIcAllHistoryId }
                            , { "ReveApiUrl", url }
                            , { "CustomerID", customerId }
                            , { "CustMobile", mobile }
                            , { "LogEntryID", 50000 }

                        };

                        _idb.GetDataByProc(ht1, "sp_InsertReveProcessorErrorLog");

                        processErrorlog += ex.Message + "# ";

                        // continue;
                    }
                }
                successLogBeforeProcess = builder1.ToString();
                SuccessLogAfterProcess = builder2.ToString();
                var ht = new Hashtable
                {
                    {"successLogBeforeProcess", successLogBeforeProcess},
                    {"SuccessLogAfterProcess", SuccessLogAfterProcess},
                    {"processErrorlog", processErrorlog},
                    {"processStartTime", processStartTime},
                    {"ProcessEndTime", DateTime.Now},
                    {"noOfCustomer", noOfCustomer},
                    {"ProcessorTypeName", "ReveAPIProcessor"},
                    {"ID", 3}
                };

                _idb.GetDataByProc(ht, "sp_InsertProcessLog");
            }
            catch (Exception xx)
            {
                WriteLogFile.WriteLog(xx.ToString());
            }

        }

        class ReveApiRequest
        {
            string status;
        }

        //private Scheduler getScheduler()
        //{
        //    if (rdbMinute.Checked)
        //        return Scheduler.EveryMinutes;
        //    if (rdbTenMinute.Checked)
        //        return Scheduler.EveryTenMinutes;
        //    if (rdbHour.Checked)
        //        return Scheduler.EveryHour;

        //    if (rdbEightHour.Checked)
        //        return Scheduler.EveryEightHours;

        //    if (rdbHalfDay.Checked)
        //        return Scheduler.EveryHalfDay;
        //    if (rdbDay.Checked)
        //        return Scheduler.EveryDay;
        //    if (rdbWeek.Checked)
        //        return Scheduler.EveryWeek;
        //    if (rdbMonth.Checked)
        //        return Scheduler.EveryMonth;
        //    if (rdbYear.Checked)
        //        return Scheduler.EveryYear;

        //    default
        //    return Scheduler.EveryMinutes;
        //}

        /// <summary>
        /// canceling the scheduler
        /// </summary>
        /// <summary>
        /// Exits the app
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>

        private void exitBtn_Click(object sender, EventArgs e)
        {

            Close();
        }


    }

}
