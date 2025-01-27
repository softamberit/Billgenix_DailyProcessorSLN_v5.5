using BBS.Utilitys;
using BillingERPConn;
using BillingProcessor.Models;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using MkCommunication;
using ReportBilling;
using SWIFTDailyProcessor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Windows.Forms;
using System.Xml;
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using Telerik.ReportViewer.WinForms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace BillingProcessor
{
    public partial class FrmMain : Form
    {
        readonly DBUtility _idb = new DBUtility();
        private List<string> ErrorHost = new List<string>();
        public FrmMain()
        {
            InitializeComponent();

            // numHours.Value = DateTime.Now.Hour;
            //  numMins.Value = DateTime.Now.Minute;

        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //  DailyBillingProcessor();
            schedule_Timer_Callback();
            // ReminderProcessorDue();
            //LockStatementMailProcessor();
            //DailyBillingProcessor();
            // MethodToCall();

            // InactiveToDeviceColl();

            // ReminderProcessorDue();

            //  PackageChangeRequest();

        }

        int PinNumber = 10000;

        public int ProcessInterval;
        string startHour = "";
        int endHour = 0;
        List<TimeSpan> dwnPrpcessTimes = new List<TimeSpan>();
        private void schedule_Timer_Callback()
        {

            string strText = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss:ffff tt");
            WriteLogFile.WriteLog(strText);

            //listBox1.Items.Add(strText);

            LoadProcessConfig();

            //timer1.Interval = startMiliSeconds;  // millisecond

            //System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer
            //{
            //    Interval = ProcessInterval  ///millisecond
            //};



            int currHour = DateTime.Now.Hour;

            //if (currHour >= startHour && currHour <= endHour)
            //{
            timer1.Interval = 1000 * 60;
            timer1.Enabled = true;
            timer1.Tick += OnTimerEvent;

            TimeSpan dwnPrpcessTime = new TimeSpan(10, 0, 0);
            var dwPrtimeStr = ConfigurationManager.AppSettings["DowngredProcessTime"];
            if (!string.IsNullOrEmpty(dwPrtimeStr))
            {
                if (dwPrtimeStr.Contains(","))
                {
                    var dwPrtimeStrArray = dwPrtimeStr.Split(',');
                    foreach (var processTime in dwPrtimeStrArray)
                    {
                        TimeSpan.TryParse(processTime, out TimeSpan ptime);
                        dwnPrpcessTimes.Add(ptime);
                    }
                }
                else
                {
                    TimeSpan.TryParse(dwPrtimeStr, out TimeSpan ptime);
                    dwnPrpcessTimes.Add(ptime);
                }
            }
            else
            {
                dwnPrpcessTimes.Add(dwnPrpcessTime);

            }

        }

        private void LoadProcessConfig()
        {
            Hashtable ht = new Hashtable { { "ID", 1 } };
            DataTable dt = _idb.GetDataByProc(ht, "sp_getProcessConfigInfo");
            startHour = dt.Rows[0]["StartHour"].ToString();
            endHour = int.Parse(dt.Rows[0]["EndHour"].ToString());
            int startMiliSeconds = int.Parse(dt.Rows[0]["StartMin"].ToString());
            ProcessInterval = int.Parse(dt.Rows[0]["Interval"].ToString()); // Should be Millisecond

            txtStartHour.Text = startHour.ToString();
            txtEndHour.Text = endHour.ToString();
            lblText.Text = @"This Process is run with " + ProcessInterval / 60000 + @" Minutes Interval.";


        }

        private void OnTimerEvent(object sender, EventArgs e)
        {
            timer1.Stop();
            try
            {
                //LoadProcessConfig();
                // timer1.Interval = ProcessInterval;
                var today = DateTime.Now;
                var now = today.ToString("HH:mm");
                var startTime = TimeSpan.Parse(startHour);
                var endInterval = new TimeSpan(0, endHour, 0);
                var endTime = startTime.Add(endInterval);
                var currentTime = TimeSpan.Parse(now);
                // WriteLogFile.WriteLog(startTime.ToString());

                foreach (var item in dwnPrpcessTimes)
                {


                    if (currentTime == item)
                    {
                        WriteLogFile.WriteLog("PackageChangeRequest process is starting..");

                        PackageChangeRequest();

                        WriteLogFile.WriteLog("PackageChangeRequest process has completed");


                        WriteLogFile.WriteLog("PackageChangeRequest_Upgrade process is starting..");

                        PackageChangeRequest_Upgrade();
                        WriteLogFile.WriteLog("PackageChangeRequest_Upgrade process has completed");


                    }
                }

                // Regular Process 
                if (startTime == currentTime)
                {

                    //  listBox1.Items.Clear();
                    string strText = "Billing Processor Starting on " + now;
                    WriteLogFile.WriteLog(strText);

                    // listBox1.Items.Add(strText);

                    Mobile();

                    SendSMS("", "Billing Processor is starting", _mobileNo);

                    MethodToCall();


                    SendSMS("", "Billing Processor Stop", _mobileNo);



                }
            }
            catch (Exception ex)
            {

                WriteLogFile.WriteLog(ex.Message);

            }
            finally
            {
                timer1.Start();
            }

            // lblSync.Text = "Last Sync: " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt");
        }

        string _mobileNo;
        void Mobile()
        {
            DataTable mob = _idb.GetDataBySQLString("SELECT MobileNo FROM CompanyInfo WHERE id=1");
            _mobileNo = mob.Rows[0]["MobileNo"].ToString();
        }

        private void MethodToCall()
        {
            string strText = "";

            //------------------------------------------------------
            strText = "PackageChangeRequest process is starting..";
            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);

            PackageChangeRequest();

            strText = "PackageChangeRequest process has completed";



            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);

            //------------------------------------------------------

            strText = "PackageChangeRequest_Upgrade process is starting..";
            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);

            PackageChangeRequest_Upgrade();

            strText = "PackageChangeRequest_Upgrade process has completed";
            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);

            //-------------------------------------------------------

            strText = "ShiftingRequest process is starting..";
            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);

            ShiftingRequest();

            strText = "ShiftingRequest process has completed";
            WriteLogFile.WriteLog(strText);
            listBox1.Items.Add(strText);
            //-------------------------------------------------------

            strText = "OtherChargeRequest process is starting..";
            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);

            OtherChargeRequest();

            strText = "OtherChargeRequest process has completed";
            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);

            //-------------------------------------------------------

            strText = "RequestToTemporaryStop process is starting..";
            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);

            RequestToTemporaryStop();

            strText = "RequestToTemporaryStop process has completed";
            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);

            //-------------------------------------------------------

            strText = "DiscontinueRequestToDevColl process is starting..";
            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);

            DiscontinueRequestToDevColl();

            strText = "DiscontinueRequestToDevColl process has completed";
            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);

            //-------------------------------------------------------

            strText = "HandoverPendingProcessDaily process is starting..";
            WriteLogFile.WriteLog(strText);
            listBox1.Items.Add(strText);


            HandoverPendingProcessDaily();

            strText = "HandoverPendingProcessDaily process has completed";
            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);

            //-------------------------------------------------------

            strText = "TempStopToDeviceCollection process is starting..";
            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);
            TempStopToDeviceCollection();
            strText = "TempStopToDeviceCollection process has completed";
            WriteLogFile.WriteLog(strText);
            // listBox1.Items.Add(strText);
            //-------------------------------------------------------


            strText = "InactiveToDeviceColl process is starting..";
            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);

            InactiveToDeviceColl();

            strText = "InactiveToDeviceColl process has completed";
            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);
            //-------------------------------------------------------
            //strText = "ReminderProcessorBill process is starting..";
            //WriteLogFile.WriteLog(strText);


            ////1st Reminder is stopped 2024-04-26

            //ReminderProcessorBill();

            //strText = "ReminderProcessorBill process has completed";
            //WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);
            //-------------------------------------------------------

            strText = "ReminderProcessorDue process is starting..";
            WriteLogFile.WriteLog(strText);
            // listBox1.Items.Add(strText);

            ReminderProcessorDue();

            strText = "ReminderProcessorDue process has completed";
            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);
            //-------------------------------------------------------
            strText = "DailyBillingProcessor process is starting..";

            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);

            DailyBillingProcessor();

            strText = "DailyBillingProcessor process has completed";
            WriteLogFile.WriteLog(strText);
            //listBox1.Items.Add(strText);
            //-------------------------------------------------------

            //Reve pakacge for Corporate Customer in Billgenix

            if (DateTime.Today.Day == 1)
            {
                strText = "MonthlyRevePackageForCorporate process is starting..";
                WriteLogFile.WriteLog(strText);
                //listBox1.Items.Add(strText);

                MonthlyRevePackageForCorporate();

                strText = "MonthlyRevePackageForCorporate process has completed";
                WriteLogFile.WriteLog(strText);
                // listBox1.Items.Add(strText);
            }

            strText = " Daily Lock Statement Mail Sending process is starting..";

            WriteLogFile.WriteLog(strText);
            LockStatementMailProcessor();

            strText = " Daily Lock Statement Mail Sending process has completed";
            WriteLogFile.WriteLog(strText);


        }




        private void MonthlyRevePackageForCorporate()
        {
            try
            {
                DataTable dtCust = _idb.GetDataBySQLString(@"Select CustomerID from CorpCust_Ignore");
                foreach (DataRow dr in dtCust.Rows)
                {
                    try
                    {
                        var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                        var endDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
                        Hashtable ht = new Hashtable();
                        var customerId = dr["CustomerID"].ToString();
                        ht.Add("CustomerID", customerId);
                        ht.Add("EntryID", PinNumber);
                        ht.Add("UpdateID", 1);
                        ht.Add("ProcessId", 901);
                        ht.Add("StartDate", startDate);
                        ht.Add("EndDate", endDate);

                        DataTable dtCustomerInfo = _idb.GetDataByProc(ht, "sp_InsertReveAPICallHistory_Corporate");
                        ht.Clear();

                    }
                    catch (Exception ex)
                    {

                        WriteLogFile.WriteLog("RevePackageForCorporate: " + ex.Message);

                    }
                }
            }
            catch (Exception ex)
            {

                WriteLogFile.WriteLog("RevePackageForCorporate: " + ex.Message);

            }
        }

        string GetCompanyInfo()
        {
            var dtbiturl = _idb.GetDataByProc("sp_getCompanyInformation");
            var biturl = dtbiturl.Rows[0]["bitUrl"].ToString();

            return biturl;
        }

        #region DailyBillingProcessor

        public void DailyBillingProcessor()
        {
            try
            {
                ErrorHost.Clear();
                MkConnection objMkConnection = new MkConnection();

                //DataTable dtCust = _idb.GetDataBySQLString(@"SELECT CustomerMaster.[CustomerID] FROM [dbo].[CustomerMaster] Where StatusID=1 
                //                    and  CustomerMaster.[CustomerID]   not in (Select CustomerID from CorpCust_Ignore)
                //                    order By CustomerID asc");

                DataTable dtCust = _idb.GetDataByProc(@"sp_GetPendingLockForBillingProcessor");

                //DataTable dtCust = _idb.GetDataBySQLString(@"SELECT CustomerMaster.[CustomerID] FROM [dbo].[CustomerMaster] Where  CustomerMaster.[CustomerID] ='720625'
                //            order By CustomerID asc");

                DateTime cd = DateTime.Today;
                DateTime processStartTime = DateTime.Now;
                int noOfCustomer = 0;

                string MkStatus = "true";

                string customerId = "", smsText;
                string successLogBeforeProcess = "", successLogAfterProcess = "", processErrorlog = "", mkLogError = "";
                int noOfActiveCustomer = 0;
                foreach (DataRow dr in dtCust.Rows)
                {
                    try
                    {
                        noOfActiveCustomer++;

                        Hashtable ht = new Hashtable();
                        customerId = dr["CustomerID"].ToString();
                        ht.Add("CustomerID", customerId);
                        DataTable dtCustomerInfo = _idb.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();

                        foreach (DataRow datarow in dtCustomerInfo.Rows)
                        {

                            if (!String.IsNullOrEmpty(datarow["EndDate"].ToString()))
                            {
                                decimal debit = Conversion.TryCastDecimal(datarow["Debit"].ToString());
                                decimal credit = Conversion.TryCastDecimal(datarow["Credit"].ToString());
                                decimal cl = Conversion.TryCastDecimal(datarow["CreditLimit"].ToString());
                                decimal pv = Conversion.TryCastDecimal(datarow["TotalMRC"].ToString());
                                decimal dsc = Conversion.TryCastDecimal(datarow["Discount"].ToString());
                                DateTime ed = Conversion.TryCastDate(datarow["EndDate"].ToString());

                                var username = datarow["RouterUserName"].ToString();
                                var password = datarow["Password"].ToString();
                                var ipAddress = datarow["IPAddress"].ToString();
                                var hostname = datarow["Host"].ToString();
                                var netMrc = decimal.Parse(datarow["NetMRC"].ToString());
                                var routerId = int.Parse(datarow["RouterID"].ToString());
                                var protocolId = int.Parse(datarow["ProtocolID"].ToString());
                                var mobile = datarow["Mobile"].ToString();
                                decimal balance = credit - debit;
                                DateTime ced = DateTime.Parse(datarow["EndDate"].ToString());
                                var insType = datarow["InsType"].ToString();
                                var mkUser = datarow["MkUser"].ToString();
                                var mkVersion = datarow["mkVersion"].ToString();
                                int port = Conversion.TryCastInteger(datarow["APIPort"].ToString());

                                //successLogBeforeProcess += "ID:" + customerId + ", M:" + netMrc + ", B:" + balance + "CED:" + ced + ", MK:" + MkStatus + " #";
                                successLogBeforeProcess += customerId + ",";
                                WriteLogFile.WriteLog(string.Concat(customerId, " is active found"));

                                if (ErrorHost.Exists(s => s.Equals(hostname)))
                                {
                                    continue;
                                }

                                if (cd > ed)
                                {
                                    decimal inv = debit + pv - dsc;
                                    decimal ca = credit + cl;

                                    if (inv > ca)
                                    {
                                        // MK OFF, DIscontinue 

                                        MkConnStatus objMkConnStatusDisable = objMkConnection.DisableMikrotik(hostname, username, password, mkVersion, protocolId, Conversion.TryCastInteger(insType), mkUser, port);

                                        if (objMkConnStatusDisable.StatusCode == "200")
                                        {
                                            WriteLogFile.WriteLog(string.Concat(customerId, " is inactivated"));

                                            ht = new Hashtable();
                                            ht.Add("CustomerID", customerId);
                                            ht.Add("POPId", routerId);
                                            ht.Add("CustomerIP", ipAddress);
                                            ht.Add("Status", 0);
                                            ht.Add("StatusID", 9);
                                            ht.Add("ProcessID", 100);
                                            ht.Add("EntryID", PinNumber);
                                            ht.Add("ActivityDetail", "LOCK_FROM_BILLING");
                                            ht.Add("SeconderyStatus", "LOCK_FROM_BILLING");

                                            _idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                            ht.Clear();

                                            //successLogAfterProcess += "ID:" + customerId + ", M:" + netMrc + ", B:" + balance + "CED:" + ced + ", MK:" + "false #";
                                            successLogAfterProcess += customerId + ",";

                                            // SMS 

                                            //smsText = "Your Internet has been locked on " + dateStr + ".Pls pay your bill to avoid disconnection. For details visit " + GetCompanyInfo() + ". Pls ignore if already paid";
                                            //smsText = "বিল বকেয়ার জন্য আপনার ইন্টারনেট " + dateStr + " তারিখে সাময়িক ভাবে বন্ধ করা হয়েছে। পুনরায় সংযোগ পেতে আপনার বকেয়া বিলটি পরিশোধ করতে লগইন করুন " + GetCompanyInfo();


                                            // SendSMS(customerId, smsText, mobile);

                                            SendSMSAfterLock(customerId, mobile, netMrc);

                                            noOfCustomer++;
                                        }

                                        // Insert MK Error log
                                        else
                                        {
                                            if (objMkConnStatusDisable.StatusCode == "401")
                                            {
                                                ErrorHost.Add(hostname);
                                            }

                                            // mkLogError += "I:" + customerId + ", Er. " + objMkConnStatusDisable.RetMessage + " #";
                                            mkLogError += customerId + ",";
                                            InsertMikrotikErrorLog(customerId, routerId, hostname, ipAddress, objMkConnStatusDisable.RetMessage, objMkConnStatusDisable.StatusCode);
                                            WriteLogFile.WriteLog(string.Concat(customerId, " Error:", objMkConnStatusDisable.RetMessage));

                                        }

                                    }
                                    else if (inv <= ca)
                                    {
                                        // MK ON, INV GEN

                                        // -----  Bill generation ----------//
                                        ht = new Hashtable();
                                        ht.Add("CustomerID", customerId);
                                        ht.Add("EntryID", PinNumber);
                                        ht.Add("ProcessID", 100);
                                        ht.Add("MRCAmount", netMrc);
                                        ht.Add("ActivityDetail", "ACTIVE_FROM_BILLING");

                                        _idb.GetDataByProc(ht, "sp_BillGeneDuringDailyProcess");

                                        ht.Clear();
                                        // successLogBeforeProcess += "ID:" + customerId + ", M:" + netMrc + ", B:" + balance + "CED:" + ced + ", MK:" + MkStatus + " #";
                                        successLogBeforeProcess += customerId + ",";
                                        // SMS 
                                        string dateStr = DateTime.Today.ToString("MMM dd, yyyy");
                                        //smsText = "Your current billing cycle has been started from " + dateStr + " for MRC " + netMrc + " .You can access our online billing on " + GetCompanyInfo() + ".";
                                        smsText = "ধন্যবাদ, আপনার নতুন বিলিং সাইকেল শুরুর তারিখ " + dateStr + " এবং প্যাকেজ মূল্য টাকা " + netMrc + "। আমাদের অনলাইন পোর্টাল ব্যবহার করতে লগইন করুন " + GetCompanyInfo() + "";

                                        SendSMS(customerId, smsText, mobile);
                                        noOfCustomer++;


                                        //  Mikrotik Enable Check 
                                        MkConnStatus objMkConnStatusEnable = objMkConnection.EnableMikrotik(hostname, username, password, mkVersion, protocolId, Conversion.TryCastInteger(insType), mkUser, port);


                                        if (objMkConnStatusEnable.StatusCode == "200")
                                        {
                                            WriteLogFile.WriteLog(string.Concat(customerId, " is activated:"));

                                            ht = new Hashtable();
                                            ht.Add("CustomerID", customerId);
                                            ht.Add("POPId", routerId);
                                            ht.Add("CustomerIP", ipAddress);
                                            ht.Add("Status", 1);
                                            ht.Add("ProcessID", 100);
                                            ht.Add("EntryID", PinNumber);

                                            _idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                            ht.Clear();

                                            //successLogAfterProcess += "ID:" + customerId + ", M:" + netMrc + ", B:" + balance + "CED:" + DateTime.Today.AddDays(31) + ", MK:" + "true #";
                                            successLogAfterProcess += customerId + ",";

                                        }

                                        // --- Insert MK Error log for Enabling IP-----//

                                        else
                                        {
                                            if (objMkConnStatusEnable.StatusCode == "401")
                                            {
                                                ErrorHost.Add(hostname);
                                            }

                                            mkLogError += customerId + ",";

                                            // mkLogError += "I:" + customerId + ", Er. " + objMkConnStatusEnable.RetMessage + " #";
                                            InsertMikrotikErrorLog(customerId, routerId, hostname, ipAddress, objMkConnStatusEnable.RetMessage, objMkConnStatusEnable.StatusCode);
                                            WriteLogFile.WriteLog(string.Concat(customerId, " Error:", objMkConnStatusEnable.RetMessage));

                                        }
                                    }

                                }

                                else if (cd <= ed)
                                {
                                    decimal inv = debit;
                                    decimal mr = credit + cl;

                                    if (inv <= mr)
                                    {
                                        // MK ON   NO NEED TO CODE??

                                    }
                                    else if (inv > mr)
                                    {
                                        //MK OFF Discontinue

                                        MkConnStatus objMkConnStatusDisable = objMkConnection.DisableMikrotik(hostname, username, password, mkVersion, protocolId, Conversion.TryCastInteger(insType), mkUser, port);

                                        if (objMkConnStatusDisable.StatusCode == "200")
                                        {
                                            WriteLogFile.WriteLog(string.Concat(customerId, " is inactivated"));


                                            ht = new Hashtable();
                                            ht.Add("CustomerID", customerId);
                                            ht.Add("POPId", routerId);
                                            ht.Add("CustomerIP", ipAddress);
                                            ht.Add("Status", 0);
                                            ht.Add("StatusID", 9);
                                            ht.Add("ProcessID", 100);
                                            ht.Add("EntryID", PinNumber);
                                            ht.Add("ActivityDetail", "LOCK_FROM_BILLING");
                                            ht.Add("SeconderyStatus", "LOCK_FROM_BILLING");

                                            _idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                            ht.Clear();
                                            //successLogAfterProcess += "ID:" + customerId + ", M:" + netMrc + ", B:" + balance + "CED:" + ced + ", MK:" + "false #";
                                            successLogAfterProcess += customerId + ",";

                                            // string dateStr = DateTime.Today.ToString("dd-MMM-yy");
                                            // smsText = "Your Internet has been locked on " + dateStr + ". Pls pay your bill to avoid disconnection. For details visit " + GetCompanyInfo() + ". Pls ignore if already paid";
                                            //smsText = "বিল বকেয়ার জন্য আপনার ইন্টারনেট " + dateStr + " তারিখে সাময়িক ভাবে বন্ধ করা হয়েছে। পুনরায় সংযোগ পেতে আপনার বকেয়া বিলটি পরিশোধ করতে লগইন করুন " + GetCompanyInfo();


                                            //SendSMS(customerId, smsText, mobile);

                                            SendSMSAfterLock(customerId, mobile, netMrc);
                                            noOfCustomer++;
                                        }

                                        // --- Insert MK Error log-----//
                                        else
                                        {
                                            if (objMkConnStatusDisable.StatusCode == "401")
                                            {
                                                ErrorHost.Add(hostname);
                                            }
                                            // mkLogError += "I:" + customerId + ", Er. " + objMkConnStatusDisable.RetMessage + " #";
                                            mkLogError += customerId + ",";
                                            WriteLogFile.WriteLog(string.Concat(customerId, " Error:", objMkConnStatusDisable.RetMessage));
                                            InsertMikrotikErrorLog(customerId, routerId, hostname, ipAddress, objMkConnStatusDisable.RetMessage, objMkConnStatusDisable.StatusCode);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        processErrorlog += "ID:" + customerId + ", Er. " + ex.Message + " #";
                        continue;
                    }
                }
                WriteLogFile.WriteLog("BillingProcessorLog: Entry ");
                Hashtable ht1 = new Hashtable
                {
                    {"SuccessLogBeforeProcess", successLogBeforeProcess},
                    {"SuccessLogAfterProcess", successLogAfterProcess},
                    {"ProcessErrorlog", processErrorlog},
                    {"MKLogError", mkLogError},
                    {"ProcessStartTime", processStartTime},
                    {"ProcessEndTime", DateTime.Now},
                    {"NoOfCustomer", noOfCustomer},
                    {"ID", 1}        ,
                    {"NoOfActiveCustomer", noOfActiveCustomer}
                };

                DataTable processlog = _idb.GetDataByProc(ht1, "sp_InsertProcessLog");
                var processorId = int.Parse(processlog.Rows[0]["PROCSSORLOGID"].ToString());

                // SMS 

                //if (processErrorlog != "" || mkLogError != "")
                //{
                //    smsText = "Error!! " + " for Billing Processor id " + processorId + ", please take the neccessery steps to solve the problem.";
                //    if (_mobileNo != "")
                //    {
                //        SendSMS(customerId, smsText, _mobileNo);
                //    }
                //}
            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLog("BillingProcessor: " + ex.Message);
            }
        }

        private void SendSMSAfterLock(string customerId, string mobile, decimal dues)
        {
            string dateStr = DateTime.Now.ToString("dd-MMM-yy hh:mm tt");
            //string smsText = string.Format("কাস্টমার আইডি {0}, সর্বমোট বিল {1} টাকা পরিশোধ না করায় আপনার ইন্টারনেট {2} তারিখ হতে সাময়িক ভাবে বন্ধ রয়েছে। সংযোগটি সচল করতে বিল পরিশোধ করুনঃ {3}", customerId, Convert.ToInt32(dues), dateStr, GetCompanyInfo());
            Hashtable hashTable = new Hashtable();
            //            @CustomerID varchar(20),
            //@Mobile varchar(15),
            //@Dues   int,
            //@DuesDate varchar(25)
            hashTable.Add("CustomerID", customerId);
            hashTable.Add("Dues", Convert.ToInt32(dues).ToString());
            hashTable.Add("Mobile", mobile);
            hashTable.Add("DuesDate", dateStr);
            _idb.GetDataByProc(hashTable, "sp_insertSMS_AfterLock");
            hashTable.Clear();
        }

        #endregion

        #region RequestToTemporaryStop

        public void RequestToTemporaryStop()
        {
            try
            {
                MkConnection objMKConnection = new MkConnection();

                DataTable dtCust = _idb.GetDataByProc("sp_CustomerListForTemporaryStopRequest");
                DateTime ProcessStartTime = DateTime.Now;
                int NoOfCustomer = dtCust.Rows.Count;
                string MkStatus = "";
                int ProcessorID = 0;
                string CustomerID = "", SMSText = "";
                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", ProcessErrorlog = "", MKLogError = "";
                string InsType = "", mkUser = "";

                foreach (DataRow dr in dtCust.Rows)
                {

                    CustomerID = dr["CustomerID"].ToString();
                    string RequestRefNo = dr["RequestRefNo"].ToString();

                    string SeconderyStatus = "TEMPORARYSTOP_FROM_REQUEST";

                    try
                    {

                        CustomerID = dr["CustomerID"].ToString();
                        Hashtable ht = new Hashtable();

                        ht.Add("CustomerID", CustomerID);
                        DataTable dtCustomerInfo = _idb.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();

                        string Hostname = "", Username = "", Password = "", IPAddress = "", Mobile = "", mkVersion = "";
                        int RouterID = 0, ProtocolID = 0;


                        foreach (DataRow datarow in dtCustomerInfo.Rows)
                        {
                            Username = datarow["RouterUserName"].ToString();
                            Password = datarow["Password"].ToString();
                            IPAddress = datarow["IPAddress"].ToString();
                            Hostname = datarow["Host"].ToString();

                            RouterID = int.Parse(datarow["RouterID"].ToString());
                            ProtocolID = int.Parse(datarow["ProtocolID"].ToString());
                            Mobile = datarow["Mobile"].ToString();

                            decimal debit = Conversion.TryCastDecimal(datarow["Debit"].ToString());
                            decimal credit = Conversion.TryCastDecimal(datarow["Credit"].ToString());
                            decimal Balance = credit - debit;
                            decimal NetMRC = decimal.Parse(datarow["NetMRC"].ToString());

                            DateTime CED = DateTime.Parse(datarow["EndDate"].ToString());
                            InsType = datarow["InsType"].ToString();
                            mkUser = datarow["MkUser"].ToString();
                            mkVersion = datarow["mkVersion"].ToString();

                            MkConnStatus OBJMkStatus = objMKConnection.MikrotikStatus(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser);

                            if (OBJMkStatus.MikrotikStatus == 1)
                            {
                                MkStatus = "true";
                            }
                            else
                            {
                                MkStatus = "false";
                            }
                            // MK OFF, DIscontinue 
                            //SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";
                            SuccessLogBeforeProcess += CustomerID + ",";

                            MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser);

                            if (objMkConnStatusDisable.StatusCode == "200")
                            {
                                ht = new Hashtable();
                                ht.Add("CustomerID", CustomerID);
                                ht.Add("POPId", RouterID);
                                ht.Add("CustomerIP", IPAddress);
                                ht.Add("Status", 0);
                                ht.Add("StatusID", 9);
                                ht.Add("ProcessID", 301);
                                ht.Add("EntryID", PinNumber);
                                ht.Add("ActivityDetail", "TemporaryStop_from_RequestProcess");
                                ht.Add("SeconderyStatus", SeconderyStatus);
                                ht.Add("DeviceCollStatus", 0);

                                _idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                ht.Clear();

                                DataTable reqCompleted = _idb.GetDataBySQLString("UPDATE RequestMaster SET IsCompleted =1, CompleteBy = " + PinNumber + ", CompleteDate = '" + DateTime.Today + "'  WHERE RequestRefNo = '" + RequestRefNo + "'; SELECT 'SUCCESS' AS SUCCESS");

                                SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "false #";

                                //  SMSText = "Your Internet connection has been Temporarily stopped because of your Temporary stop request.";
                                SMSText = "আপনার অনুরোধের প্রেক্ষিতে আপনার সংযোগ সাময়িক ভাবে বন্ধ করা হয়েছে।";

                                SendSMS(CustomerID, SMSText, Mobile);

                            }

                            // Insert MK Error log
                            else
                            {
                                // MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusDisable.RetMessage.ToString() + " #";
                                MKLogError += CustomerID + ",";
                                InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusDisable.RetMessage.ToString(), objMkConnStatusDisable.StatusCode.ToString());
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        // SMS 
                        ProcessErrorlog += "ID:" + CustomerID + ", Er. " + ex.ToString() + " #";
                        continue;
                    }

                }

                Hashtable HT = new Hashtable();
                HT.Add("SuccessLogBeforeProcess", SuccessLogBeforeProcess);
                HT.Add("SuccessLogAfterProcess", SuccessLogAfterProcess);
                HT.Add("ProcessErrorlog", ProcessErrorlog);
                HT.Add("MKLogError", MKLogError);
                HT.Add("ProcessStartTime", ProcessStartTime);
                HT.Add("ProcessEndTime", DateTime.Now);
                HT.Add("NoOfCustomer", NoOfCustomer);
                HT.Add("ProcessorTypeName", "RequestToTemporaryStop");
                HT.Add("ID", 3);

                DataTable PROCESSLOG = _idb.GetDataByProc(HT, "sp_InsertProcessLog");
                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                if (ProcessErrorlog != "" || MKLogError != "")
                {
                    SMSText = "Error!! " + " for Request Processor id " + ProcessorID + ", please take the necessary steps to solve the problem.";
                    if (_mobileNo != "")
                    {
                        SendSMS(CustomerID, SMSText, _mobileNo);
                    }
                }

            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ex.Message.ToString());
            }
        }

        #endregion

        #region DiscontinueRequestToDevColl

        public void DiscontinueRequestToDevColl()
        {
            try
            {
                MkConnection objMKConnection = new MkConnection();

                DataTable dtCust = _idb.GetDataByProc("sp_CustomerListForDiscontinueRequest");
                DateTime ProcessStartTime = DateTime.Now;
                int NoOfCustomer = dtCust.Rows.Count;
                string MkStatus = "";
                int ProcessorID = 0;
                string CustomerID = "", SMSText = "";
                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", ProcessErrorlog = "", MKLogError = "";
                string InsType = "", mkUser = "", mkVersion = "";

                foreach (DataRow dr in dtCust.Rows)
                {

                    CustomerID = dr["CustomerID"].ToString();
                    string RequestRefNo = dr["RequestRefNo"].ToString();

                    string SeconderyStatus = "DISC._FROM_REQUST";

                    try
                    {

                        CustomerID = dr["CustomerID"].ToString();
                        Hashtable ht = new Hashtable();

                        ht.Add("CustomerID", CustomerID);
                        DataTable dtCustomerInfo = _idb.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();

                        string Hostname = "", Username = "", Password = "", IPAddress = "", Mobile = "";
                        int RouterID = 0, ProtocolID = 0;


                        foreach (DataRow datarow in dtCustomerInfo.Rows)
                        {
                            Username = datarow["RouterUserName"].ToString();
                            Password = datarow["Password"].ToString();
                            IPAddress = datarow["IPAddress"].ToString();
                            Hostname = datarow["Host"].ToString();

                            RouterID = int.Parse(datarow["RouterID"].ToString());
                            ProtocolID = int.Parse(datarow["ProtocolID"].ToString());
                            Mobile = datarow["Mobile"].ToString();

                            decimal debit = Conversion.TryCastDecimal(datarow["Debit"].ToString());
                            decimal credit = Conversion.TryCastDecimal(datarow["Credit"].ToString());
                            decimal Balance = credit - debit;
                            decimal NetMRC = decimal.Parse(datarow["NetMRC"].ToString());

                            string CED = datarow["EndDate"].ToString();
                            InsType = datarow["InsType"].ToString();
                            mkUser = datarow["MkUser"].ToString();
                            mkVersion = datarow["mkVersion"].ToString();
                            int port = Conversion.TryCastInteger(datarow["APIPort"].ToString());

                            MkConnStatus OBJMkStatus = objMKConnection.MikrotikStatus(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser, port);

                            if (OBJMkStatus.MikrotikStatus == 1)
                            {
                                MkStatus = "true";
                            }
                            else
                            {
                                MkStatus = "false";
                            }

                            // MK OFF, DIscontinue 

                            MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser, port);
                            ht = new Hashtable();
                            ht.Add("CustomerID", CustomerID);
                            ht.Add("POPId", RouterID);
                            ht.Add("CustomerIP", IPAddress);
                            ht.Add("Status", 0);
                            ht.Add("StatusID", 2);
                            ht.Add("ProcessID", 302);
                            ht.Add("EntryID", PinNumber);
                            ht.Add("ActivityDetail", "DISC._FROM_REQUST");
                            ht.Add("SeconderyStatus", SeconderyStatus);
                            ht.Add("DeviceCollStatus", 1);
                            ht.Add("LockDate", DateTime.Now);

                            _idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                            ht.Clear();

                            DataTable reqCompleted = _idb.GetDataBySQLString("UPDATE RequestMaster SET IsCompleted =1, CompleteBy = " + PinNumber + ", CompleteDate = '" + DateTime.Today + "'  WHERE RequestRefNo = '" + RequestRefNo + "'; SELECT 'SUCCESS' AS SUCCESS");

                            SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";


                            if (objMkConnStatusDisable.StatusCode == "200")
                            {

                                SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "false #";
                                // SuccessLogAfterProcess += CustomerID + ",";

                            }

                            // Insert MK Error log
                            else
                            {
                                MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusDisable.RetMessage.ToString() + " #";
                                InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusDisable.RetMessage.ToString(), objMkConnStatusDisable.StatusCode.ToString());
                            }
                        }
                    }

                    catch (Exception ex)
                    {

                        // Error Log 
                        ProcessErrorlog += "ID:" + CustomerID + ", Er. " + ex.ToString() + " #";
                        continue;
                    }

                }

                Hashtable HT = new Hashtable();
                HT.Add("SuccessLogBeforeProcess", SuccessLogBeforeProcess);
                HT.Add("SuccessLogAfterProcess", SuccessLogAfterProcess);
                HT.Add("ProcessErrorlog", ProcessErrorlog);
                HT.Add("MKLogError", MKLogError);
                HT.Add("ProcessStartTime", ProcessStartTime);
                HT.Add("ProcessEndTime", DateTime.Now);
                HT.Add("NoOfCustomer", NoOfCustomer);
                HT.Add("ProcessorTypeName", "DiscontinueRequestToDevColl");
                HT.Add("ID", 3);


                DataTable PROCESSLOG = _idb.GetDataByProc(HT, "sp_InsertProcessLog");
                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                if (ProcessErrorlog != "" || MKLogError != "")
                {
                    SMSText = "Error!! " + " for Request Processor id " + ProcessorID + ", please take the necessary steps to solve the problem.";
                    if (_mobileNo != "")
                    {
                        SendSMS(CustomerID, SMSText, _mobileNo);
                    }
                }

            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ex.Message.ToString());
            }
        }

        #endregion

        #region InactiveToDeviceColl

        public void InactiveToDeviceColl()
        {
            try
            {
                MkConnection objMKConnection = new MkConnection();

                DataTable dtCust = _idb.GetDataByProc("sp_CustomerListForInactiveToDeviceColl");
                DateTime ProcessStartTime = DateTime.Now;
                int NoOfCustomer = dtCust.Rows.Count;
                string MkStatus = "";
                int ProcessorID = 0;
                string CustomerID = "", SMSText = "";
                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", ProcessErrorlog = "", MKLogError = "";

                string InsType = "", mkUser = "";



                foreach (DataRow dr in dtCust.Rows)
                {

                    CustomerID = dr["CustomerID"].ToString();
                    DateTime LockDate = Conversion.TryCastDate(dr["StatusEntryTime"].ToString());

                    try
                    {

                        Hashtable ht = new Hashtable();
                        CustomerID = dr["CustomerID"].ToString();

                        ht.Add("CustomerID", CustomerID);
                        DataTable dtCustomerInfo = _idb.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();

                        string Hostname = "", Username = "", Password = "", IPAddress = "", Mobile = "", SecondaryStatus = "", mkVersion = "";
                        int RouterID = 0, ProtocolID = 0;

                        foreach (DataRow datarow in dtCustomerInfo.Rows)
                        {
                            Username = datarow["RouterUserName"].ToString();
                            Password = datarow["Password"].ToString();
                            IPAddress = datarow["IPAddress"].ToString();
                            Hostname = datarow["Host"].ToString();

                            RouterID = int.Parse(datarow["RouterID"].ToString());
                            ProtocolID = int.Parse(datarow["ProtocolID"].ToString());
                            Mobile = datarow["Mobile"].ToString();
                            SecondaryStatus = datarow["SecondaryStatus"].ToString();
                            decimal debit = Conversion.TryCastDecimal(datarow["Debit"].ToString());
                            decimal credit = Conversion.TryCastDecimal(datarow["Credit"].ToString());
                            decimal Balance = credit - debit;
                            decimal NetMRC = decimal.Parse(datarow["NetMRC"].ToString());

                            string CED = datarow["EndDate"].ToString();
                            InsType = datarow["InsType"].ToString();
                            mkUser = datarow["MkUser"].ToString();
                            mkVersion = datarow["mkVersion"].ToString();

                            MkConnStatus OBJMkStatus = objMKConnection.MikrotikStatus(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser);

                            if (OBJMkStatus.MikrotikStatus == 1)
                            {
                                MkStatus = "true";
                            }
                            else
                            {
                                MkStatus = "false";
                            }

                            // MK OFF, DIscontinue 

                            MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser);
                            ht = new Hashtable();
                            ht.Add("CustomerID", CustomerID);
                            ht.Add("POPId", RouterID);
                            ht.Add("CustomerIP", IPAddress);
                            ht.Add("Status", 0);
                            ht.Add("StatusID", 9);
                            ht.Add("ProcessID", 303);
                            ht.Add("EntryID", PinNumber);
                            ht.Add("ActivityDetail", "INACTIVE_FROM_PROCESS");
                            ht.Add("DeviceCollStatus", 1);
                            ht.Add("SeconderyStatus", SecondaryStatus);
                            ht.Add("LockDate", LockDate);

                            _idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                            ht.Clear();

                            SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";



                            if (objMkConnStatusDisable.StatusCode == "200")
                            {
                                SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "false #";

                            }

                            // Insert MK Error log
                            else
                            {
                                MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusDisable.RetMessage.ToString() + " #";
                                InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusDisable.RetMessage.ToString(), objMkConnStatusDisable.StatusCode.ToString());
                            }
                        }
                    }

                    catch (Exception ex)
                    {

                        // SMS 
                        ProcessErrorlog += "ID:" + CustomerID + ", Er. " + ex.ToString() + " #";
                        continue;
                    }

                }

                Hashtable HT = new Hashtable();
                HT.Add("SuccessLogBeforeProcess", SuccessLogBeforeProcess);
                HT.Add("SuccessLogAfterProcess", SuccessLogAfterProcess);
                HT.Add("ProcessErrorlog", ProcessErrorlog);
                HT.Add("MKLogError", MKLogError);
                HT.Add("ProcessStartTime", ProcessStartTime);
                HT.Add("ProcessEndTime", DateTime.Now);
                HT.Add("NoOfCustomer", NoOfCustomer);
                HT.Add("ProcessorTypeName", "HandoverLockToInactive");
                HT.Add("ID", 3);

                DataTable PROCESSLOG = _idb.GetDataByProc(HT, "sp_InsertProcessLog");
                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                if (ProcessErrorlog != "" || MKLogError != "")
                {
                    SMSText = "Error!! " + " for Request Processor id " + ProcessorID + ", please teke the neccessery steps to solve the problem.";
                    if (_mobileNo != "")
                    {
                        SendSMS(CustomerID, SMSText, _mobileNo);
                    }
                }

            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ex.Message.ToString());
            }
        }

        #endregion

        #region TempStopToDeviceCollection

        public void TempStopToDeviceCollection()

        {
            try
            {
                MkConnection objMKConnection = new MkConnection();

                DataTable dtCust = _idb.GetDataByProc("sp_CustomerListForForInactiveFromBillingLock"); //sp_CustomerListForFORInactive
                DateTime ProcessStartTime = DateTime.Now;
                int NoOfCustomer = dtCust.Rows.Count;
                string MkStatus = "";
                int ProcessorID = 0;
                string CustomerID = "", SMSText = "";
                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", ProcessErrorlog = "", MKLogError = "";
                string InsType = "", mkUser = "";

                foreach (DataRow dr in dtCust.Rows)
                {

                    CustomerID = dr["CustomerID"].ToString();

                    try
                    {
                        CustomerID = dr["CustomerID"].ToString();

                        DateTime LockDate = Conversion.TryCastDate(dr["StatusEntryTime"].ToString());

                        Hashtable ht = new Hashtable();

                        ht.Add("CustomerID", CustomerID);
                        DataTable dtCustomerInfo = _idb.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();

                        string Hostname = "", Username = "", Password = "", IPAddress = "", Mobile = "", SecondaryStatus = "", mkVersion = "";
                        int RouterID = 0, ProtocolID = 0;

                        foreach (DataRow datarow in dtCustomerInfo.Rows)
                        {
                            Username = datarow["RouterUserName"].ToString();
                            Password = datarow["Password"].ToString();
                            IPAddress = datarow["IPAddress"].ToString();
                            Hostname = datarow["Host"].ToString();

                            RouterID = int.Parse(datarow["RouterID"].ToString());
                            ProtocolID = int.Parse(datarow["ProtocolID"].ToString());
                            Mobile = datarow["Mobile"].ToString();
                            //SecondaryStatus = datarow["SecondaryStatus"].ToString();

                            decimal debit = Conversion.TryCastDecimal(datarow["Debit"].ToString());
                            decimal credit = Conversion.TryCastDecimal(datarow["Credit"].ToString());
                            decimal Balance = credit - debit;
                            decimal NetMRC = decimal.Parse(datarow["NetMRC"].ToString());

                            DateTime CED = DateTime.Parse(datarow["EndDate"].ToString());

                            InsType = datarow["InsType"].ToString();
                            mkUser = datarow["MkUser"].ToString();
                            mkVersion = datarow["mkVersion"].ToString();
                            MkConnStatus OBJMkStatus = objMKConnection.MikrotikStatus(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser);

                            if (OBJMkStatus.MikrotikStatus == 1)
                            {
                                MkStatus = "true";
                            }
                            else
                            {
                                MkStatus = "false";
                            }
                            // MK OFF, DIscontinue 

                            MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser);
                            ht = new Hashtable();
                            ht.Add("CustomerID", CustomerID);
                            ht.Add("POPId", RouterID);
                            ht.Add("CustomerIP", IPAddress);
                            ht.Add("Status", 0);
                            ht.Add("StatusID", 9);
                            ht.Add("ProcessID", 304);
                            ht.Add("EntryID", PinNumber);
                            ht.Add("ActivityDetail", "DISCONTINUE_FROM_TEMP.STOP");
                            ht.Add("DeviceCollStatus", 1);
                            ht.Add("SeconderyStatus", "DISCONTINUE_FROM_TEMP.STOP");
                            ht.Add("LockDate", LockDate);

                            _idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                            ht.Clear();

                            SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";


                            if (objMkConnStatusDisable.StatusCode == "200")
                            {
                                SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "false #";

                            }

                            // Insert MK Error log
                            else
                            {
                                MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusDisable.RetMessage.ToString() + " #";
                                InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusDisable.RetMessage.ToString(), objMkConnStatusDisable.StatusCode.ToString());
                            }
                        }
                    }

                    catch (Exception ex)
                    {

                        // SMS 
                        ProcessErrorlog += "ID:" + CustomerID + ", Er. " + ex.ToString() + " #";
                        continue;
                    }

                }

                Hashtable HT = new Hashtable();
                HT.Add("SuccessLogBeforeProcess", SuccessLogBeforeProcess);
                HT.Add("SuccessLogAfterProcess", SuccessLogAfterProcess);
                HT.Add("ProcessErrorlog", ProcessErrorlog);
                HT.Add("MKLogError", MKLogError);
                HT.Add("ProcessStartTime", ProcessStartTime);
                HT.Add("ProcessEndTime", DateTime.Now);
                HT.Add("NoOfCustomer", NoOfCustomer);
                HT.Add("ProcessorTypeName", "TempStopToDeviceCollection");
                HT.Add("ID", 3);

                DataTable PROCESSLOG = _idb.GetDataByProc(HT, "sp_InsertProcessLog");
                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                if (ProcessErrorlog != "" || MKLogError != "")
                {
                    SMSText = "Error!! " + " for Request Processor id " + ProcessorID + ", please take the neccessery steps to solve the problem.";
                    if (_mobileNo != "")
                    {
                        SendSMS(CustomerID, SMSText, _mobileNo);
                    }
                }

            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ex.Message.ToString());
            }
        }

        #endregion

        #region PackageChangeRequest Downgrade

        public void PackageChangeRequest()
        {
            try
            {
                MkConnection objMKConnection = new MkConnection();

                DataTable dtCust = _idb.GetDataByProc("sp_CustomerListForPackageChanged");
                DateTime ProcessStartTime = DateTime.Now;
                int NoOfCustomer = dtCust.Rows.Count;
                int ProcessorID = 0;

                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", ProcessErrorlog = "", MKLogError = "";


                foreach (DataRow dr in dtCust.Rows)
                {
                    string CustomerID = dr["CustomerID"].ToString();
                    int balance = Conversion.TryCastInteger(dr["Balance"].ToString());
                    int mrc = Conversion.TryCastInteger(dr["InternetMRC"].ToString());
                    int downChg = Conversion.TryCastInteger(dr["downChg"].ToString());
                    int fromStatus = Conversion.TryCastInteger(dr["FromStatusId"].ToString());


                    int totalDues = downChg + mrc;
                    (BillingStatus billingStatus, CustomerMaster customer) = BillingService.GetBillingStatus(CustomerID, totalDues);
                    if (billingStatus == BillingStatus.ExpiredButBalanceAvailable || billingStatus == BillingStatus.NotExpiredButBalanceAvailable)
                    {
                        try
                        {
                            string RequestRefNo = dr["RequestRefNo"].ToString();
                            CustomerID = dr["CustomerID"].ToString();

                            Hashtable ht = new Hashtable();
                            ht.Add("CustomerID", CustomerID);
                            ht.Add("ProcessID", 400);
                            ht.Add("EntryID", PinNumber);
                            ht.Add("RequestRefNo", RequestRefNo);

                            DataTable DATA = _idb.GetDataByProc(ht, "sp_CustomerPackageChangedfromProcessor");

                            SuccessLogAfterProcess += CustomerID + ":" + DATA.Rows[0]["SUCCESS"].ToString() + ", ";
                        }

                        catch (Exception ex)
                        {

                            continue;
                        }

                        if (fromStatus == (int)CustomerStatus.INACTIVE)
                        {

                            // -----  Bill generation ----------//
                            var ht = new Hashtable();
                            ht.Add("CustomerID", CustomerID);
                            ht.Add("EntryID", PinNumber);
                            ht.Add("ProcessID", 400);
                            ht.Add("MRCAmount", mrc);
                            ht.Add("ActivityDetail", "ACTIVE_FROM_DOWNGRADATION");

                            _idb.GetDataByProc(ht, "sp_BillGeneDuringDailyProcess");

                            if (customer.CustomerStatus == CustomerStatus.INACTIVE)
                            {
                                //  Mikrotik Enable Check 
                                MkConnStatus objMkConnStatusEnable = objMKConnection.EnableMikrotik(customer.Host, customer.RouterUserName, customer.RouterPassword, customer.MkVersion, customer.ProtocolID, Conversion.TryCastInteger(customer.InsType), customer.MkUser, customer.APIPort);
                                if (objMkConnStatusEnable.StatusCode == "200")
                                {
                                    WriteLogFile.WriteLog(string.Concat(CustomerID, " is activated:"));

                                    ht = new Hashtable();
                                    ht.Add("CustomerID", CustomerID);
                                    ht.Add("POPId", customer.RouterId);
                                    ht.Add("CustomerIP", customer.IPAddress);
                                    ht.Add("Status", 1);
                                    ht.Add("ProcessID", 400);
                                    ht.Add("EntryID", PinNumber);

                                    _idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                    ht.Clear();

                                }
                            }
                        }

                    }
                    else
                    {
                        if (customer.CustomerStatus == CustomerStatus.ACTIVE)
                        {

                            MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(customer.Host, customer.UserName, customer.Password, customer.MkVersion, customer.ProtocolID, Conversion.TryCastInteger(customer.InsType), customer.MkUser, customer.APIPort);

                            if (objMkConnStatusDisable.StatusCode == "200")
                            {
                                WriteLogFile.WriteLog(string.Concat(CustomerID, " is inactivated"));
                                Hashtable ht = new Hashtable();

                                ht = new Hashtable();
                                ht.Add("CustomerID", customer.CustomerID);
                                ht.Add("POPId", customer.RouterId);
                                ht.Add("CustomerIP", customer.IPAddress);
                                ht.Add("Status", 0);
                                ht.Add("StatusID", 9);
                                ht.Add("ProcessID", 400);
                                ht.Add("EntryID", PinNumber);
                                ht.Add("ActivityDetail", "LOCK_FROM_BILLING");
                                ht.Add("SeconderyStatus", "LOCK_FROM_BILLING");

                                _idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                ht.Clear();


                            }

                        }
                    }
                }


                //Hashtable HT = new Hashtable();
                //HT.Add("SuccessLogBeforeProcess", SuccessLogBeforeProcess);
                //HT.Add("SuccessLogAfterProcess", SuccessLogAfterProcess);
                //HT.Add("ProcessErrorlog", ProcessErrorlog);
                //HT.Add("MKLogError", MKLogError);
                //HT.Add("ProcessStartTime", ProcessStartTime);
                //HT.Add("ProcessEndTime", DateTime.Now);
                //HT.Add("NoOfCustomer", NoOfCustomer);
                //HT.Add("ProcessorTypeName", "PackageChangeRequest");
                //HT.Add("ID", 3);

                //DataTable PROCESSLOG = _idb.GetDataByProc(HT, "sp_InsertProcessLog");
                //ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                //if (ProcessErrorlog != "" || MKLogError != "")
                //{
                //    SMSText = "Error!! " + " for Request Processor id " + ProcessorID + ", please take the neccessery steps to solve the problem.";
                //    if (_mobileNo != "")
                //    {
                //        SendSMS(CustomerID, SMSText, _mobileNo);
                //    }
                //}
            }

            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ex.Message.ToString());
            }
        }

        #endregion

        #region PackageChangeRequest Upgrade

        public void PackageChangeRequest_Upgrade()
        {
            string ProcessErrorlog = "";
            try
            {
                MkConnection objMKConnection = new MkConnection();

                Hashtable htFasuki = new Hashtable();
                htFasuki.Add("Fasuki", "");

                var upgradRequests = _idb.GetDataByProc<CustomerUpgradeRequest>(htFasuki, "sp_CustomerListForPackageChanged_Upgradation");

                DateTime ProcessStartTime = DateTime.Now;
                int NoOfCustomer = upgradRequests.Count;
                int ProcessorID = 0;
                string SMSText = "";
                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", MKLogError = "";


                foreach (CustomerUpgradeRequest request in upgradRequests)
                {
                    decimal mrc = request.Amount;
                    decimal adjAmt = request.BalAfterAdj;
                    decimal totalDues = mrc + adjAmt;
                    string CustomerID = request.CustomerID;
                    string RequestRefNo = request.RequestRefNo;
                    (BillingStatus billingStatus, CustomerMaster customer) = BillingService.GetBillingStatus(CustomerID, totalDues);

                    if (billingStatus == BillingStatus.ExpiredButBalanceAvailable || billingStatus == BillingStatus.NotExpiredButBalanceAvailable)
                    {
                        try
                        {
                            Hashtable ht = new Hashtable();
                            ht.Add("CustomerID", CustomerID);
                            ht.Add("ProcessID", 401);
                            ht.Add("EntryID", PinNumber);
                            ht.Add("RequestRefNo", RequestRefNo);

                            DataTable DATA = _idb.GetDataByProc(ht, "sp_CustomerPackageChanged_Upgradation_fromProcessor");
                            //sp_CustomerPackageChanged_Upgradation_fromProcessor
                            // SuccessLogAfterProcess += CustomerID + ":" + DATA.Rows[0]["Feedback"].ToString() + ", ";
                        }

                        catch (Exception ex)
                        {
                            // ProcessErrorlog += "ID:" + CustomerID + ", Er. " + ex.ToString() + " #";
                            continue;
                        }
                        //int balance = Conversion.TryCastInteger(dr["Balance"].ToString());
                        //
                        if (customer.CustomerStatus == CustomerStatus.INACTIVE)
                        {
                            // -----  Bill generation ----------//
                            var ht = new Hashtable();
                            ht.Add("CustomerID", CustomerID);
                            ht.Add("EntryID", PinNumber);
                            ht.Add("ProcessID", 401);
                            ht.Add("MRCAmount", mrc);
                            ht.Add("ActivityDetail", "ACTIVE_FROM_UPGRADATION");

                            _idb.GetDataByProc(ht, "sp_BillGeneDuringDailyProcess");

                            if (customer.CustomerStatus == CustomerStatus.INACTIVE)
                            {
                                //  Mikrotik Enable Check 
                                MkConnStatus objMkConnStatusEnable = objMKConnection.EnableMikrotik(customer.Host, customer.RouterUserName, customer.RouterPassword, customer.MkVersion, customer.ProtocolID, Conversion.TryCastInteger(customer.InsType), customer.MkUser, customer.APIPort);
                                if (objMkConnStatusEnable.StatusCode == "200")
                                {
                                    WriteLogFile.WriteLog(string.Concat(CustomerID, " is activated:"));

                                    ht = new Hashtable();
                                    ht.Add("CustomerID", CustomerID);
                                    ht.Add("POPId", customer.RouterId);
                                    ht.Add("CustomerIP", customer.IPAddress);
                                    ht.Add("Status", 1);
                                    ht.Add("ProcessID", 401);
                                    ht.Add("EntryID", PinNumber);

                                    _idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                    ht.Clear();

                                }
                            }
                        }

                        // SuccessLogBeforeProcess += CustomerID + ", ";

                    }
                    else
                    {
                        if (customer.CustomerStatus == CustomerStatus.ACTIVE)
                        {

                            MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(customer.Host, customer.UserName, customer.Password, customer.MkVersion, customer.ProtocolID, Conversion.TryCastInteger(customer.InsType), customer.MkUser, customer.APIPort);

                            if (objMkConnStatusDisable.StatusCode == "200")
                            {
                                Hashtable ht = new Hashtable();

                                ht = new Hashtable();
                                ht.Add("CustomerID", customer.CustomerID);
                                ht.Add("POPId", customer.RouterId);
                                ht.Add("CustomerIP", customer.IPAddress);
                                ht.Add("Status", 0);
                                ht.Add("StatusID", 9);
                                ht.Add("ProcessID", 401);
                                ht.Add("EntryID", PinNumber);
                                ht.Add("ActivityDetail", "LOCK_FROM_BILLING");
                                ht.Add("SeconderyStatus", "LOCK_FROM_BILLING");

                                _idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                ht.Clear();
                                //successLogAfterProcess += "ID:" + customerId + ", M:" + netMrc + ", B:" + balance + "CED:" + ced + ", MK:" + "false #";


                                // string dateStr = DateTime.Today.ToString("dd-MMM-yy");
                                // smsText = "Your Internet has been locked on " + dateStr + ". Pls pay your bill to avoid disconnection. For details visit " + GetCompanyInfo() + ". Pls ignore if already paid";
                                //smsText = "বিল বকেয়ার জন্য আপনার ইন্টারনেট " + dateStr + " তারিখে সাময়িক ভাবে বন্ধ করা হয়েছে। পুনরায় সংযোগ পেতে আপনার বকেয়া বিলটি পরিশোধ করতে লগইন করুন " + GetCompanyInfo();


                                //SendSMS(customerId, smsText, mobile);

                                //  SendSMSAfterLock(customer.CustomerID, customer.Mobile, customer.NetMRC);

                            }
                        }

                    }

                }

                //Hashtable HT = new Hashtable();
                //HT.Add("SuccessLogBeforeProcess", SuccessLogBeforeProcess);
                //HT.Add("SuccessLogAfterProcess", SuccessLogAfterProcess);
                //HT.Add("ProcessErrorlog", ProcessErrorlog);
                //HT.Add("MKLogError", MKLogError);
                //HT.Add("ProcessStartTime", ProcessStartTime);
                //HT.Add("ProcessEndTime", DateTime.Now);
                //HT.Add("NoOfCustomer", NoOfCustomer);
                //HT.Add("ProcessorTypeName", "PackageChangeRequest_Upgrade");
                //HT.Add("ID", 3);

                //DataTable PROCESSLOG = _idb.GetDataByProc(HT, "sp_InsertProcessLog");
                //ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                //if (ProcessErrorlog != "" || MKLogError != "")
                //{
                //    SMSText = "Error!! " + " for Request Processor id " + ProcessorID + ", please take the neccessery steps to solve the problem.";
                //    if (_mobileNo != "")
                //    {
                //        SendSMS("", SMSText, _mobileNo);
                //    }
                //}
            }

            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ProcessErrorlog);

                WriteLogFile.WriteLog(ex.Message.ToString());
            }
        }

        #endregion

        #region ShiftingRequest 

        public void ShiftingRequest()
        {
            try
            {
                MkConnection objMKConnection = new MkConnection();

                DataTable dtCust = _idb.GetDataByProc("sp_CustomerListForShifting");
                DateTime ProcessStartTime = DateTime.Now;
                int NoOfCustomer = dtCust.Rows.Count;
                int ProcessorID = 0;
                string CustomerID = "", SMSText = "";
                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", ProcessErrorlog = "", MKLogError = "";

                foreach (DataRow dr in dtCust.Rows)
                {
                    CustomerID = dr["CustomerID"].ToString();
                    SuccessLogBeforeProcess += CustomerID + ", ";
                    try
                    {
                        string RequestRefNo = dr["RequestRefNo"].ToString();
                        CustomerID = dr["CustomerID"].ToString();
                        Hashtable ht = new Hashtable();

                        ht.Add("CustomerID", CustomerID);
                        ht.Add("ProcessID", 305);
                        ht.Add("EntryID", PinNumber);
                        ht.Add("RequestRefNo", RequestRefNo);

                        DataTable DATA = _idb.GetDataByProc(ht, "sp_CustomerShiftingfromProcessor");
                        SuccessLogAfterProcess += CustomerID + ", ";
                    }

                    catch (Exception ex)
                    {
                        ProcessErrorlog += "ID:" + CustomerID + ", Er. " + ex.ToString() + " #";
                        continue;
                    }
                }

                Hashtable HT = new Hashtable();
                HT.Add("SuccessLogBeforeProcess", SuccessLogBeforeProcess);
                HT.Add("SuccessLogAfterProcess", SuccessLogAfterProcess);
                HT.Add("ProcessErrorlog", ProcessErrorlog);
                HT.Add("MKLogError", MKLogError);
                HT.Add("ProcessStartTime", ProcessStartTime);
                HT.Add("ProcessEndTime", DateTime.Now);
                HT.Add("NoOfCustomer", NoOfCustomer);
                HT.Add("ProcessorTypeName", "ShiftingRequest");
                HT.Add("ID", 3);

                DataTable PROCESSLOG = _idb.GetDataByProc(HT, "sp_InsertProcessLog");
                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                if (ProcessErrorlog != "" || MKLogError != "")
                {
                    SMSText = "Error!! " + " for Request Processor id " + ProcessorID + ", please take the neccessery steps to solve the problem.";
                    if (_mobileNo != "")
                    {
                        SendSMS(CustomerID, SMSText, _mobileNo);
                    }
                }
            }

            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ex.Message.ToString());
            }
        }

        #endregion

        #region HandoverPendingProcessDaily

        public void HandoverPendingProcessDaily()
        {
            try
            {

                MkConnection objMKConnection = new MkConnection();
                DataTable dtCust = _idb.GetDataByProc("sp_CustomerListForHandoverpending");

                DateTime CD = DateTime.Today;
                DateTime ProcessStartTime = DateTime.Now;
                int NoOfCustomer = 0;

                int ProcessorID = 0;
                string CustomerID = "", SMSText = "", MkStatus = "";
                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", ProcessErrorlog = "", MKLogError = "";
                string InsType = "", mkUser = "";

                foreach (DataRow dr in dtCust.Rows)
                {
                    try
                    {
                        CustomerID = dr["CustomerID"].ToString();
                        Hashtable ht = new Hashtable();

                        ht.Add("CustomerID", CustomerID);
                        DataTable dtCustomerInfo = _idb.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();
                        foreach (DataRow datarow in dtCustomerInfo.Rows)
                        {

                            string Hostname = "", Username = "", Password = "", IPAddress = "", Mobile = "", mkVersion = "";
                            int RouterID = 0, ProtocolID = 0; decimal NetMRC = 0;

                            Username = datarow["RouterUserName"].ToString();
                            Password = datarow["Password"].ToString();
                            IPAddress = datarow["IPAddress"].ToString();
                            Hostname = datarow["Host"].ToString();
                            NetMRC = decimal.Parse(datarow["NetMRC"].ToString());

                            RouterID = int.Parse(datarow["RouterID"].ToString());
                            ProtocolID = int.Parse(datarow["ProtocolID"].ToString());
                            Mobile = datarow["Mobile"].ToString();

                            decimal debit = Conversion.TryCastDecimal(datarow["Debit"].ToString());
                            decimal credit = Conversion.TryCastDecimal(datarow["Credit"].ToString());
                            decimal Balance = credit - debit;
                            decimal PV = Conversion.TryCastDecimal(datarow["TotalMRC"].ToString());
                            decimal DSC = Conversion.TryCastDecimal(datarow["Discount"].ToString());
                            decimal CL = Conversion.TryCastDecimal(datarow["CreditLimit"].ToString());
                            decimal OTC = Conversion.TryCastDecimal(datarow["TotalOTC"].ToString());
                            InsType = datarow["InsType"].ToString();
                            mkUser = datarow["MkUser"].ToString();
                            mkVersion = datarow["mkVersion"].ToString();
                            MkConnStatus OBJMkStatus = objMKConnection.MikrotikStatus(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser);

                            if (OBJMkStatus.MikrotikStatus == 1)
                            {
                                MkStatus = "true";
                            }
                            else
                            {
                                MkStatus = "false";
                            }

                            if ((NetMRC + OTC - DSC) > (credit + CL) && OBJMkStatus.MikrotikStatus == 1)
                            {
                                // MK OFF, DIscontinue 

                                MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser);

                                if (objMkConnStatusDisable.StatusCode == "200")
                                {
                                    ht = new Hashtable();
                                    ht.Add("CustomerID", CustomerID);
                                    ht.Add("POPId", RouterID);
                                    ht.Add("CustomerIP", IPAddress);
                                    ht.Add("Status", 0);
                                    ht.Add("StatusID", 0); //Status: Not Set Yet
                                    ht.Add("ProcessID", 306);
                                    ht.Add("EntryID", PinNumber);
                                    ht.Add("ActivityDetail", "LOCK_BEFORE_HANDOVER");
                                    ht.Add("DeviceCollStatus", 0);
                                    ht.Add("SeconderyStatus", "LOCK_BEFORE_HANDOVER");

                                    _idb.InsertData(ht, "sp_insertMKlogNCustStatus");

                                    SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + ", MK:" + MkStatus + " #";
                                    SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + ", MK:" + "false #";

                                    //SMSText = "Your internet connection has been discontinued because of your discontinue request.";
                                    //SendSMS(CustomerID, SMSText, Mobile);

                                }

                                // Insert MK Error log
                                else
                                {
                                    MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusDisable.RetMessage.ToString() + " #";
                                    InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusDisable.RetMessage.ToString(), objMkConnStatusDisable.StatusCode.ToString());
                                }
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        ProcessErrorlog += "ID:" + CustomerID + ", Er. " + ex.ToString() + " #";
                        continue;
                    }
                }

                Hashtable HT = new Hashtable();

                HT.Add("SuccessLogBeforeProcess", SuccessLogBeforeProcess);
                HT.Add("SuccessLogAfterProcess", SuccessLogAfterProcess);
                HT.Add("ProcessErrorlog", ProcessErrorlog);
                HT.Add("MKLogError", MKLogError);
                HT.Add("ProcessStartTime", ProcessStartTime);
                HT.Add("ProcessEndTime", DateTime.Now);
                HT.Add("NoOfCustomer", NoOfCustomer);
                HT.Add("ProcessorTypeName", "HandoverPendingProcessDaily");
                HT.Add("ID", 3);
                DataTable PROCESSLOG = _idb.GetDataByProc(HT, "sp_InsertProcessLog");

                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                // SMS 

                if (ProcessErrorlog != "" || MKLogError != "")
                {
                    //SMSText = "Error!! " + " for Request Processor id " + ProcessorID + ", please take the neccessery steps to solve the problem.";
                    SMSText = string.Format("Found Err:RP ID:{0} CID:{1}", ProcessorID, CustomerID);
                    if (_mobileNo != "")
                    {
                        SendSMS(CustomerID, SMSText, _mobileNo);
                        // SendEmail()
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ex.Message.ToString());
            }
        }

        #endregion

        #region ReminderProcessor

        private void ReminderProcessorBill()
        {
            Hashtable ht = new Hashtable();

            DataTable dt = _idb.GetDataByProc(ht, "sp_getRemindersmsinfoBill");  //
            ht.Clear();

            if (IsSMSSendOptionEnabled())
            {

                string CustomerID = "";
                foreach (DataRow dr in dt.Rows)
                {
                    CustomerID = dr["CustomerID"].ToString();
                    try
                    {
                        insertSMSScheduleBill(CustomerID);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            if (IsEmailSendOptionEnabled())
            {

                string CustomerID = "";
                foreach (DataRow dr in dt.Rows)
                {
                    CustomerID = dr["CustomerID"].ToString();
                    try
                    {
                        insertEmailScheduleBill(CustomerID);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

        }

        private static void insertSMSScheduleBill(string CustomerID)
        {

            try
            {
                DBUtility idb = new DBUtility();

                Hashtable ht = new Hashtable();
                ht.Add("CustomerID", CustomerID);
                DataTable SMS = idb.GetDataByProc(ht, "spInsertSMS_ScheduleReminderBill");
                //DataTable Email = idb.GetDataByProc(ht, "spInsertEmail_ScheduleReminderBill");
            }
            catch (Exception ex)
            {

            }
        }

        private static void insertEmailScheduleBill(string CustomerID)
        {

            try
            {
                DBUtility idb = new DBUtility();

                Hashtable ht = new Hashtable();
                ht.Add("CustomerID", CustomerID);
                DataTable Email = idb.GetDataByProc(ht, "spInsertEmail_ScheduleReminderBill");
            }
            catch (Exception ex)
            {

            }
        }


        // Reminder before 1 day
        private void ReminderProcessorDue()
        {

            if (IsSMSSendOptionEnabled())
            {
                Hashtable ht = new Hashtable();

                DataTable dt = _idb.GetDataByProc(ht, "sp_getRemindersmsinfoDue");
                ht.Clear();
                string CustomerID = "";
                foreach (DataRow dr in dt.Rows)
                {
                    CustomerID = dr["CustomerID"].ToString();
                    try
                    {
                        insertSMSScheduleDue(CustomerID);
                    }
                    catch (Exception ex)
                    {
                        WriteLogFile.WriteLog("ReminderProcessorDue=>" + ex.Message);
                        continue;
                    }
                }
            }
        }

        private static void insertSMSScheduleDue(string CustomerID)
        {

            try
            {
                DBUtility idb = new DBUtility();

                Hashtable ht = new Hashtable();
                ht.Add("CustomerID", CustomerID);
                DataTable dt = idb.GetDataByProc(ht, "spInsertSMS_ScheduleReminderDue");
            }
            catch (Exception ex)
            {

            }

        }

        private static bool IsSMSSendOptionEnabled()
        {
            DBUtility idb = new DBUtility();
            try
            {
                DataTable dt = idb.GetDataBySQLString("SELECT * from CompanyInfo where id=1 AND IsSMSEnabled=1 ");

                if (dt.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool IsEmailSendOptionEnabled()
        {
            DBUtility idb = new DBUtility();
            try
            {
                DataTable dt = idb.GetDataBySQLString("SELECT * from CompanyInfo where id=1 AND IsEmailEnabled=1 ");

                if (dt.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public class SmsHttpRequest
        {

            public string campaignName { get; set; }
            public string routeId { get; set; }
            public List<Message> messages { get; set; }
            public string scheduledDeliveryDateTime { get; set; }
            public string scheduledDeliveryDateTimeOffset { get; set; }
            public int smsTypeId { get; set; }
            public int validityPeriodInHour { get; set; }

            public int responseType { get; set; }
        }

        public class Message
        {
            public string from { get; set; }
            public string to { get; set; }
            public string text { get; set; }
            public string categoryName { get; set; }
        }

        public class SmsHttpResponse
        {
            public string bulkProcessId { get; set; }
            public int bulkProcessStatus { get; set; }
            public string messages { get; set; }
        }

        #endregion

        #region OtherChargeRequest

        public void OtherChargeRequest()
        {
            try
            {
                MkConnection objMKConnection = new MkConnection();

                DataTable dtCust = _idb.GetDataByProc("sp_CustomerListForOtherCharge");
                DateTime ProcessStartTime = DateTime.Now;
                int NoOfCustomer = dtCust.Rows.Count;
                int ProcessorID = 0;
                string CustomerID = "", SMSText = "";
                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", ProcessErrorlog = "", MKLogError = "";

                foreach (DataRow dr in dtCust.Rows)
                {
                    CustomerID = dr["CustomerID"].ToString();
                    SuccessLogBeforeProcess += CustomerID + ", ";
                    try
                    {
                        string RequestRefNo = dr["RequestRefNo"].ToString();
                        CustomerID = dr["CustomerID"].ToString();
                        Hashtable ht = new Hashtable();

                        ht.Add("CustomerID", CustomerID);
                        ht.Add("ProcessID", 402);
                        ht.Add("EntryID", PinNumber);
                        ht.Add("RequestRefNo", RequestRefNo);

                        DataTable DATA = _idb.GetDataByProc(ht, "sp_CustomerShiftingfromProcessor");
                        SuccessLogAfterProcess += CustomerID + ", ";
                    }

                    catch (Exception ex)
                    {
                        ProcessErrorlog += "ID:" + CustomerID + ", Er. " + ex.ToString() + " #";
                        continue;
                    }
                }

                Hashtable HT = new Hashtable();
                HT.Add("SuccessLogBeforeProcess", SuccessLogBeforeProcess);
                HT.Add("SuccessLogAfterProcess", SuccessLogAfterProcess);
                HT.Add("ProcessErrorlog", ProcessErrorlog);
                HT.Add("MKLogError", MKLogError);
                HT.Add("ProcessStartTime", ProcessStartTime);
                HT.Add("ProcessEndTime", DateTime.Now);
                HT.Add("NoOfCustomer", NoOfCustomer);
                HT.Add("ProcessorTypeName", "OtherChargeRequest");
                HT.Add("ID", 3);

                DataTable PROCESSLOG = _idb.GetDataByProc(HT, "sp_InsertProcessLog");
                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                if (ProcessErrorlog != "" || MKLogError != "")
                {
                    SMSText = "Error!! " + " for Request Processor id " + ProcessorID + ", please take the neccessery steps to solve the problem.";
                    if (_mobileNo != "")
                    {
                        SendSMS(CustomerID, SMSText, _mobileNo);
                    }
                }
            }

            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ex.Message.ToString());
            }
        }

        #endregion

        #region SMSCreation

        public void SendSMS(string CustomerID, string SMSText, string Mobile)
        {
            Hashtable hashTable = new Hashtable();

            hashTable.Add("CustomerID", CustomerID);
            hashTable.Add("SMSText", SMSText);
            hashTable.Add("Mobile", Mobile);
            _idb.GetDataByProc(hashTable, "sp_insertSMS_Schedule");

        }

        #endregion

        #region InsertMikrotikErrorLog
        public void InsertMikrotikErrorLog(string CustomerID, int RouterID, string Hostname, string IPAddress, string RetMessage, string Statuscode)
        {

            Hashtable ht = new Hashtable();

            ht.Add("CustomerID", CustomerID);
            ht.Add("POPId", RouterID);
            ht.Add("CustomerIP", IPAddress);
            ht.Add("Error_description", RetMessage);
            ht.Add("ProcessID", 100);

            ht.Add("EntryID", PinNumber);
            //ht.Add("IPAddress", IPAddress);
            ht.Add("StatusCode", Statuscode);

            _idb.InsertData(ht, "sp_insertMKCommunication_Errorlog");
        }

        #endregion

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            //DailyBillingProcessor();
            LockStatementMailProcessor();

        }


        #region Lockstatement to mail   

        private void LockStatementMailProcessor()
        {
            // List Of Pop 
            DateTime toDay = DateTime.Now;

            Hashtable ht = new Hashtable
                {
                    {"StartDate",toDay.Date},
                    {"EndDate", toDay.Date},

                };

            var popList = _idb.GetDataByProc(ht, "rpt_LockSummary");
            if (popList == null)
            {
                return;
            }
            StillLockHistoryProcessor(toDay);

            foreach (DataRow item in popList.Rows)
            {
                int popId = Conversion.TryCastInteger(item["POPId"]);
                int totalLock = Conversion.TryCastInteger(item["NoOfLock"]);
                int totalStillLock = Conversion.TryCastInteger(item["StillLock"]);
                string popName = item["POPName"].ToString();

                var pop = _idb.GetDataBySQLString(@"Select POPID,POPName, ToMailAddress,isnull(CCMailAddress,'')CCMailAddress from 
                                                    POPMaster where IsActive=1 and IsEnableEmail=1 and PopId=" + popId);

                if (pop.Rows.Count == 0)
                {
                    WriteLogFile.WriteLog("Not Allowed to Email for  " + popName);
                    continue;
                }
                if (totalStillLock == 0)
                {
                    WriteLogFile.WriteLog("Not Still Lock Found for " + popName);
                    continue;
                }

                try
                {

                    var toEmails = Conversion.TryCastString(pop.Rows[0]["ToMailAddress"]);
                    var mailCc = Conversion.TryCastString(pop.Rows[0]["CCMailAddress"]);


                    List<string> ccList = new List<string>();
                    List<string> toList = new List<string>();

                    if (!string.IsNullOrEmpty(toEmails))
                    {
                        var toEmailsArray = toEmails.Split(',');
                        foreach (var to in toEmailsArray)
                        {
                            toList.Add(to.Trim());
                        }
                    }

                    if (!string.IsNullOrEmpty(mailCc))
                    {
                        var ccEmailsArray = mailCc.Split(',');
                        foreach (var cc in ccEmailsArray)
                        {
                            ccList.Add(cc.Trim());
                        }
                    }


                    // RichTextBox richTexbox = 

                    string subject = string.Format("Lock statement of {0} on {1} ", popName, toDay.ToString("dd MMM, yyyy hh:mm tt"));
                    string mailBody = "Dear Concern,\n";
                    mailBody += "Find Customer Lock Statement in attachment. ";
                    mailBody += "\n";
                    mailBody += string.Format("POP Name: {0}", popName);
                    mailBody += "\n";

                    mailBody += string.Format("Total Lock in Today: {0}", totalLock);
                    mailBody += "\n";

                    mailBody += string.Format("Total Still Lock: {0}", totalStillLock);


                    mailBody += "\n";
                    mailBody += "\n";

                    mailBody += "Thanking You\n";
                    mailBody += "\n";
                    mailBody += "\n";
                    mailBody += "Amber IT Limited\n";
                    mailBody += "\n";
                    mailBody += "\n";
                    mailBody += "Note: This mail is generated by Billgenix";
                    var mailResult = MailReport(toList, ccList, subject, mailBody, popId);

                    //  Insert History  ==1

                    if (mailResult == true)
                        InsertMailHistory(toEmails, mailBody, 1);

                }

                catch (Exception ex)
                {
                    WriteLogFile.WriteLog("Email Catch " + ex.Message);
                }

                // SendEmail(Conversion.TryCastString(item["ToMailAddress"]), Conversion.TryCastString(item["CCMailAddress"]), Conversion.TryCastInteger(item["POPID"]));

            }



        }

        private void StillLockHistoryProcessor(DateTime toDay)
        {
            Hashtable ht = new Hashtable();
            ht.Add("Date", toDay.Date.ToString("yyyy-MM-dd"));

            DataTable DATA = _idb.GetDataByProc(ht, "sp_InsertStillLockHistory");
            ht.Clear();
        }

        public bool MailReport(List<string> toAddress, List<string> ccAddress, string subject, string mailBody, int PopId)
        {

            bool result;

            try
            {
                var report = new REPORT_LOCK_SUMMARY_DETAILS();
                Telerik.Reporting.InstanceReportSource instanceReportSource = new Telerik.Reporting.InstanceReportSource();
                instanceReportSource.ReportDocument = report;
                instanceReportSource.Parameters.Add("StartDate", DateTime.Now);
                instanceReportSource.Parameters.Add("EndDate", DateTime.Now);
                instanceReportSource.Parameters.Add("Option", "LockWithoutUnlock");
                instanceReportSource.Parameters.Add("PopId", PopId);

                string from = "swift@amberit.com.bd";

                ReportProcessor reportProcessor = new ReportProcessor();
                RenderingResult rresult = reportProcessor.RenderReport("XLSX", instanceReportSource, null);
                MemoryStream ms = new MemoryStream(rresult.DocumentBytes);
                ms.Position = 0;

                Attachment attachment = new Attachment(ms, subject + ".xlsx");

                MailMessage msg = new MailMessage();
                foreach (var to in toAddress)
                {
                    msg.To.Add(to);

                }
                foreach (var cc in ccAddress)
                {
                    msg.CC.Add(cc);

                }
                msg.From = new MailAddress(from);
                msg.Subject = subject;
                msg.Body = mailBody;
                msg.Attachments.Add(attachment);


                string host = "202.4.96.7";
                int port = 25;

                var smtp_h = ConfigurationManager.AppSettings["smtp_host"];
                if (!string.IsNullOrEmpty(smtp_h))
                {
                    host = smtp_h;
                }
                var port_h = ConfigurationManager.AppSettings["Port"];
                if (!string.IsNullOrEmpty(port_h))
                    int.TryParse(port_h, out port);


                var smtp = new SmtpClient();
                {
                    smtp.Host = host;
                    smtp.Port = port;
                    smtp.EnableSsl = false;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Credentials = new NetworkCredential("swift@amberit.com.bd", "");
                    smtp.Timeout = 20000;
                }
                smtp.Send(msg);
                result = true;
            }

            catch (Exception ix1)
            {
                WriteLogFile.WriteLog("Sub: " + subject + " error: " + ix1.Message);
                result = false;

            }

            return result;
        }

        private static void InsertMailHistory(string emailNo, string emailBody, int statudId)
        {
            try
            {
                DBUtility idb = new DBUtility();

                Hashtable ht = new Hashtable
                {
                    {"CustomerID", ""},
                    {"EmailNo", emailNo},
                    {"EmialBody", emailBody},
                    {"StatusId", statudId},
                    {"CreatedBy", 10000},
                    {"Remarks", "Daily Lock Statement" }
                };

                idb.GetDataByProc(ht, "sp_InsertEmailHistory");
            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ex.Message);
            }
        }
        #endregion
    }
}
