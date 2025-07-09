using BBS.Utilitys;
using BillingERPConn;
using BillingProcessor.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using MkCommunication;
using ReportBilling;
using SWIFTDailyProcessor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;
using System.Xml;
using Telerik.Reporting.Processing;

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

           schedule_Timer_Callback();


            //PackageDowngradProcess();
            PackageUpgradationProcess();

        // PackageUpgradeDowngradeProcess();

        }

        int PinNumber = 10000;

        public int ProcessInterval;
        string startHour = "";
        int endHour = 0;
        //List<TimeSpan> dwnPrpcessTimes = new List<TimeSpan>();
        private void schedule_Timer_Callback()
        {



            //listBox1.Items.Add(strText);

            LoadProcessConfig();


            WriteLogFile.WriteLog("Billing Processor Starting Time Is : " + startHour);


            //timer1.Interval = startMiliSeconds;  // millisecond

            //System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer
            //{
            //    Interval = ProcessInterval  ///millisecond
            //};



            int currHour = DateTime.Now.Hour;

            //if (currHour >= startHour && currHour <= endHour)
            //{
            timer1.Interval = 1000 * 60 * 6;
            timer1.Enabled = true;
            timer1.Tick += OnTimerEvent;


            timer2.Interval = 1000 * 300;
            timer2.Enabled = true;
            timer2.Tick += OnTimerEventUpDownProc;


        }

        private void OnTimerEventUpDownProc(object sender, EventArgs e)
        {
            timer2.Stop();
            try
            {
                var today = DateTime.Now;
                var now = today.ToString("HH:mm");
                var startTime = TimeSpan.Parse(startHour);
                var endInterval = new TimeSpan(0, endHour, 0);
                var endTime = startTime.Add(endInterval);
                var currentTime = TimeSpan.Parse(now);
                // WriteLogFile.WriteLog(startTime.ToString());

                //foreach (var item in dwnPrpcessTimes)
                //{


                if (today.TimeOfDay > new TimeSpan(6, 0, 0))
                {

                    WriteLogFile.WriteLogUD("PackageChangeRequest process is starting..");

                    PackageDowngradProcess();

                    WriteLogFile.WriteLogUD("PackageChangeRequest process has completed");


                    WriteLogFile.WriteLogUD("PackageChangeRequest_Upgrade process is starting..");

                    PackageUpgradationProcess();

                    WriteLogFile.WriteLogUD("PackageChangeRequest_Upgrade process has completed");


                }
            }
            catch (Exception ex)
            {

                WriteLogFile.WriteLogUD(ex.Message);

            }
            finally
            {
                timer2.Start();
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

                var today = DateTime.Now;
                var now = today.ToString("HH:mm");
                var startTime = TimeSpan.Parse(startHour);
                var endInterval = new TimeSpan(0, endHour, 0);
                var endTime = startTime.Add(endInterval);
                var currentTime = TimeSpan.Parse(now);


                // Regular Process 
                if (today.TimeOfDay > new TimeSpan(0, 5, 0))
                {

                    //  listBox1.Items.Clear();
                    //string strText = "Package Changes Processor Starting on " + now;
                    //WriteLogFile.WriteLog(strText);

                    // listBox1.Items.Add(strText);

                    Mobile();

                    // SendSMS("", "Package Changes Processor is starting", _mobileNo);
                    //SendSMS("", "Billing Processor is starting", "8801713396444");
                    //SendSMS("", "Billing Processor is starting", "8801713396568");


                    PackageUpgradeDowngradeProcess();


                    // SendSMS("", "Billing Processor has been completed", _mobileNo);



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



        private void PackageUpgradeDowngradeProcess()
        {

            // Downgrade package update to Customer Master
            var Requests = _idb.GetDataByProc<CustomerRequest>("Sp_GetPackageChangeListForProcessor");
            if (Requests != null && Requests.Count > 0)
            {
                foreach (var request in Requests)
                {
                    int rt = request.RequestType;
                    string customerId = request.CustomerID;

                    if (rt == 4)
                    {
                        Hashtable ht = new Hashtable();
                        ht.Add("CustomerID", customerId);
                        _idb.InsertData(ht, "sp_PackageUpgrade_UpdateProfile");
                        ht.Clear();

                        // sp_getRequestForPackageDowngrade
                        Hashtable ht2 = new Hashtable();
                        ht2.Add("CustomerID", customerId);
                        var dr = _idb.GetDataByProc<CustomerRequest>(ht2, "sp_getRequestForPackageUpgradation");
                        ht2.Clear();
                        if (dr != null && dr.Count > 0)
                            BalanceCheckForUpgradation(dr.SingleOrDefault());


                    }
                    if (rt == 5)
                    {
                        Hashtable ht = new Hashtable();
                        ht.Add("CustomerID", customerId);
                        _idb.InsertData(ht, "sp_PackageDowngrad_UpdateProfile");
                        ht.Clear();

                        // sp_getRequestForPackageDowngrade
                        Hashtable ht2 = new Hashtable();
                        ht2.Add("CustomerID", customerId);
                        var dr = _idb.GetDataByProc<CustomerRequest>(ht2, "sp_getRequestForPackageDowngrade");
                        ht2.Clear();
                        if (dr != null && dr.Count > 0)
                            BalanceCheckForDowngrade(dr.SingleOrDefault());
                    }
                }



            }


        }



        string GetCompanyInfo()
        {
            var dtbiturl = _idb.GetDataByProc("sp_getCompanyInformation");
            var biturl = dtbiturl.Rows[0]["bitUrl"].ToString();

            return biturl;
        }


        #region PackageChangeRequest Downgrade

        public void PackageDowngradProcess()
        {
            try
            {
                var downgradeRequests = _idb.GetDataByProc<CustomerRequest>("sp_CustomerListForPackageDowngrade");

                // DataTable dtCust = _idb.GetDataByProc("sp_CustomerListForPackageDowngrade");
                DateTime ProcessStartTime = DateTime.Now;
                int NoOfCustomer = downgradeRequests.Count;
                int ProcessorID = 0;
                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", ProcessErrorlog = "", MKLogError = "";

                foreach (var request in downgradeRequests)
                {
                    try
                    {
                        BalanceCheckForDowngrade(request);
                    }

                    catch (Exception ex)
                    {

                        continue;
                    }
                }



            }

            catch (Exception ex)
            {
                WriteLogFile.WriteLogUD(ex.Message.ToString());
            }
        }

        private void BalanceCheckForDowngrade(CustomerRequest request)
        {
            MkConnection objMKConnection = new MkConnection();

            string CustomerID = request.CustomerID;
            int balance = request.Balance;
            decimal mrc = request.InternetMRC;
            decimal downChg = request.DownChg;
            //int fromStatus = request.FromStatus;
            int billingId = request.BillingId;
            string RequestRefNo = request.RequestRefNo;


            decimal totalDues = downChg + mrc;
            (BillingStatus billingStatus, CustomerMaster customer) = BillingService.GetBillingStatus(CustomerID, totalDues);
            if (billingStatus == BillingStatus.ExpiredButBalanceAvailable || billingStatus == BillingStatus.NotExpiredButBalanceAvailable)
            {

                //CustomerID = dr["CustomerID"].ToString();

                //if (customer.CustomerStatus == CustomerStatus.INACTIVE || customer.CustomerStatus == CustomerStatus.DISCONTINUE)
                //{
                //  Mikrotik Enable Check 
                MkConnStatus objMkConnStatusEnable = objMKConnection.EnableMikrotik(customer.Host, customer.RouterUserName, customer.RouterPassword, customer.MkVersion, customer.ProtocolID, Conversion.TryCastInteger(customer.InsType), customer.MkUser, customer.APIPort);
                if (objMkConnStatusEnable.StatusCode == "200")
                {
                    Hashtable hts = new Hashtable();
                    hts.Add("CustomerID", CustomerID);
                    hts.Add("ProcessID", 400);
                    hts.Add("EntryID", PinNumber);
                    hts.Add("RequestRefNo", RequestRefNo);

                    DataTable DATA = _idb.GetDataByProc(hts, "sp_PackageDowngradInvoice");

                    WriteLogFile.WriteLogUD(string.Concat(CustomerID, " is activated:"));

                    Hashtable ht = new Hashtable();
                    ht.Add("CustomerID", CustomerID);
                    ht.Add("POPId", customer.RouterId);
                    ht.Add("CustomerIP", customer.IPAddress);
                    ht.Add("Status", 1);
                    ht.Add("ProcessID", 400);
                    ht.Add("EntryID", PinNumber);

                    _idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                    ht.Clear();
                    if (billingId > 0)
                    {
                        DataTable ProcessUpdate = _idb.GetDataBySQLString("UPDATE BillingMaster SET IsProcessed = 1,ProcessedStatus=1 WHERE SNID = '" + billingId + "' SELECT 'SUCCESS' AS SUCCESS");
                    }

                }
                else
                {
                    InsertMikrotikErrorLog(CustomerID, customer.RouterId, customer.Host, customer.IPAddress, objMkConnStatusEnable.RetMessage, objMkConnStatusEnable.StatusCode, 400);

                }



                //}

            }
            else
            {
                //if (customer.CustomerStatus == CustomerStatus.ACTIVE)
                //{

                MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(customer.Host, customer.UserName, customer.Password, customer.MkVersion, customer.ProtocolID, Conversion.TryCastInteger(customer.InsType), customer.MkUser, customer.APIPort);

                if (objMkConnStatusDisable.StatusCode == "200")
                {
                    WriteLogFile.WriteLogUD(string.Concat(CustomerID, " is inactivated"));
                    Hashtable ht2 = new Hashtable();
                    ht2.Add("CustomerID", customer.CustomerID);
                    ht2.Add("POPId", customer.RouterId);
                    ht2.Add("CustomerIP", customer.IPAddress);
                    ht2.Add("Status", 0);
                    ht2.Add("StatusID", 9);
                    ht2.Add("ProcessID", 400);
                    ht2.Add("EntryID", PinNumber);
                    ht2.Add("ActivityDetail", "LOCK_FROM_BILLING");
                    ht2.Add("SeconderyStatus", "LOCK_FROM_BILLING");

                    _idb.InsertData(ht2, "sp_insertMKlogNCustStatus");
                    ht2.Clear();
                    if (billingId > 0)
                    {
                        DataTable ProcessUpdate = _idb.GetDataBySQLString("UPDATE BillingMaster SET IsProcessed = 1,ProcessedStatus = 3 WHERE SNID = '" + billingId + "' SELECT 'SUCCESS' AS SUCCESS");
                    }
                    var msg1 = string.Concat(CustomerID, " is not connected for insufficient balance");
                    //listBox1.Items.Add(DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + msg1);
                    WriteLogFile.WriteLog(msg1);

                    //var  SMSText = @"অপর্যাপ্ত ব্যালেন্সের কারণে আপনার ইন্টারনেট সংযোগ দিতে ব্যর্থ হয়েছে।";

                    //  SendSMS(CustomerID, SMSText, Mobile);


                }
                else
                {

                    InsertMikrotikErrorLog(CustomerID, customer.RouterId, customer.Host, customer.IPAddress, objMkConnStatusDisable.RetMessage, objMkConnStatusDisable.StatusCode, 400);


                }

                //}
            }
        }

        #endregion

        #region PackageChangeRequest Upgrade

        public void PackageUpgradationProcess()
        {
            string ProcessErrorlog = "";
            try
            {
                MkConnection objMKConnection = new MkConnection();

                //Hashtable htFasuki = new Hashtable();
                //htFasuki.Add("Fasuki", "");

                var upgradRequests = _idb.GetDataByProc<CustomerRequest>("sp_CustomerListForPackage_Upgradation");

                DateTime ProcessStartTime = DateTime.Now;
                int NoOfCustomer = upgradRequests.Count;
                int ProcessorID = 0;
                string SMSText = "";
                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", MKLogError = "";


                foreach (CustomerRequest request in upgradRequests)
                {
                    try
                    {
                        BalanceCheckForUpgradation(request);
                    }

                    catch (Exception ex)
                    {
                        // ProcessErrorlog += "ID:" + CustomerID + ", Er. " + ex.ToString() + " #";
                        continue;
                    }
                }

            }

            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ProcessErrorlog);

                WriteLogFile.WriteLog(ex.Message.ToString());
            }


        }
        private void BalanceCheckForUpgradation(CustomerRequest request)
        {
            decimal mrc = request.Amount;
            decimal adjAmt = request.BalAfterAdj;
            decimal totalDues = mrc;
            string CustomerID = request.CustomerID;
            string RequestRefNo = request.RequestRefNo;
            (BillingStatus billingStatus, CustomerMaster customer) = BillingService.GetBillingStatus(CustomerID, totalDues);
            MkConnection objMKConnection = new MkConnection();


            if (billingStatus == BillingStatus.ExpiredButBalanceAvailable || billingStatus == BillingStatus.NotExpiredButBalanceAvailable)
            {



                //  Mikrotik Enable Check 
                MkConnStatus objMkConnStatusEnable = objMKConnection.EnableMikrotik(customer.Host, customer.RouterUserName, customer.RouterPassword, customer.MkVersion, customer.ProtocolID, Conversion.TryCastInteger(customer.InsType), customer.MkUser, customer.APIPort);
                if (objMkConnStatusEnable.StatusCode == "200")
                {

                    Hashtable ht = new Hashtable();
                    ht.Add("CustomerID", CustomerID);
                    ht.Add("ProcessID", 401);
                    ht.Add("EntryID", PinNumber);
                    ht.Add("RequestRefNo", RequestRefNo);

                    DataTable DATA = _idb.GetDataByProc(ht, "sp_PackageUpgradeInvoice");
                    ht.Clear();

                    WriteLogFile.WriteLogUD(string.Concat(CustomerID, " is activated:"));

                    Hashtable ht2 = new Hashtable();
                    ht2.Add("CustomerID", CustomerID);
                    ht2.Add("POPId", customer.RouterId);
                    ht2.Add("CustomerIP", customer.IPAddress);
                    ht2.Add("Status", 1);
                    ht2.Add("ProcessID", 401);
                    ht2.Add("EntryID", PinNumber);

                    _idb.InsertData(ht2, "sp_insertMKlogNCustStatus");
                    ht2.Clear();
                    if (request.BillingId > 0)
                    {
                        DataTable ProcessUpdate = _idb.GetDataBySQLString("UPDATE BillingMaster SET IsProcessed = 1,ProcessedStatus=1 WHERE SNID = '" + request.BillingId + "' SELECT 'SUCCESS' AS SUCCESS");
                    }
                    DataTable requestComp = _idb.GetDataBySQLString("Update RequestMaster set IsCompleted=1,CompleteBy=10000,CompleteDate=GETDATE() where RequestRefNo = '" + request.RequestRefNo + "' SELECT 'SUCCESS' AS SUCCESS");


                }
                else
                {
                    if (request.BillingId > 0)
                    {
                        DataTable ProcessUpdate = _idb.GetDataBySQLString("UPDATE BillingMaster SET IsProcessed = 0,ProcessedStatus = 3 WHERE SNID = '" + request.BillingId + "' SELECT 'SUCCESS' AS SUCCESS");
                    }

                    InsertMikrotikErrorLog(CustomerID, customer.RouterId, customer.Host, customer.IPAddress, objMkConnStatusEnable.RetMessage, objMkConnStatusEnable.StatusCode, 401);
                    WriteLogFile.WriteLogUD(string.Concat(CustomerID, " Error:", objMkConnStatusEnable.RetMessage));
                }
            }
            else
            {
                //if (customer.CustomerStatus == CustomerStatus.ACTIVE)
                //{

                var mkStatus = objMKConnection.MikrotikStatus(customer.Host, customer.UserName, customer.Password, customer.MkVersion, customer.ProtocolID, Conversion.TryCastInteger(customer.InsType), customer.MkUser, customer.APIPort);
                if (mkStatus.StatusCode == "200")
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
                        if (request.BillingId > 0)
                        {
                            DataTable ProcessUpdate = _idb.GetDataBySQLString("UPDATE BillingMaster SET IsProcessed = 1,ProcessedStatus = 3 WHERE SNID = '" + request.BillingId + "' SELECT 'SUCCESS' AS SUCCESS");
                        }
                        var msg1 = string.Concat(CustomerID, " is not connected for insufficient balance");
                        //listBox1.Items.Add(DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + msg1);
                        WriteLogFile.WriteLog(msg1);

                        //var  SMSText = @"অপর্যাপ্ত ব্যালেন্সের কারণে আপনার ইন্টারনেট সংযোগ দিতে ব্যর্থ হয়েছে।";

                        //  SendSMS(CustomerID, SMSText, Mobile);

                    }
                    else
                    {
                        if (request.BillingId > 0)
                        {
                            DataTable ProcessUpdate = _idb.GetDataBySQLString("UPDATE BillingMaster SET IsProcessed = 0,ProcessedStatus = 3 WHERE SNID = '" + request.BillingId + "' SELECT 'SUCCESS' AS SUCCESS");
                        }
                        InsertMikrotikErrorLog(CustomerID, customer.RouterId, customer.Host, customer.IPAddress, objMkConnStatusDisable.RetMessage, objMkConnStatusDisable.StatusCode, 401);
                        WriteLogFile.WriteLogUD(string.Concat(CustomerID, " Error:", objMkConnStatusDisable.RetMessage));
                    }
                }

            }
        }

        #endregion



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
        public void InsertMikrotikErrorLog(string CustomerID, int RouterID, string Hostname, string IPAddress, string RetMessage, string Statuscode, int proccesId)
        {

            Hashtable ht = new Hashtable();

            ht.Add("CustomerID", CustomerID);
            ht.Add("POPId", RouterID);
            ht.Add("CustomerIP", IPAddress);
            ht.Add("Error_description", RetMessage);
            ht.Add("ProcessID", proccesId);

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


        }



    }
}
