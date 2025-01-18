
using BBS.Utilitys;
using BillingERPConn;
using MkCommunication;
using System;
using System.Collections;
using System.Data;
using System.Net;
using System.Windows.Forms;


namespace CollectionProcessor
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

    public partial class frmMain : Form
    {

        //MkConnection objMKConnection = null;

        public frmMain()
        {
            InitializeComponent();
            //  numHours.Value = DateTime.Now.Hour;
            //  numMins.Value = DateTime.Now.Minute;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

            schedule_Timer_Callback();
            // DataTable dtCust = db.GetDataByProc("sp_CustomerListForCollectionProcessor");
            //DailyCollectionProcessor(dtCust);

        }

        int PinNumber = 10000;

        int ProcessInterval = 0;
        DBUtility db = new DBUtility();
        private void schedule_Timer_Callback()
        {
            string strText = "**********Collection Processor Scheduler has been started!*****" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt");
            WriteLogFile.WriteLog(strText);

            //Hashtable ht = new Hashtable();
            //ht.Add("ID", 2);
            //DataTable dt = db.GetDataByProc(ht, "sp_getProcessConfigInfo");
            //int StartHour = int.Parse(dt.Rows[0]["StartHour"].ToString());
            //int EndHour = int.Parse(dt.Rows[0]["EndHour"].ToString());
            //int minutes = int.Parse(dt.Rows[0]["StartMin"].ToString());

            //txtStartHour.Text = EndHour.ToString();
            //txtEndHour.Text = StartHour.ToString();
            //lblText.Text = "This Process is run with " + (ProcessInterval / 60000).ToString() + " Minutes Interval.";

            timer1.Interval = 5000;  // millisecond
            timer1.Enabled = true;
            timer1.Tick += new System.EventHandler(OnTimerEvent);

            timer2.Interval = 1000 * 60 * 15;  // millisecond
            timer2.Enabled = true;
            timer2.Tick += new System.EventHandler(OnTimerEvent2);


        }



        private void OnTimerEvent(object sender, EventArgs e)
        {
            // timer1.Interval = ProcessInterval;

            // Mobile();
            try
            {
                timer1.Stop();
                timer1.Enabled = false;

                WriteLogFile.WriteLog("------------- Start Daily Collection ------------");
                DataTable dtCust = db.GetDataByProc("sp_CustomerListForCollectionProcessor");

                DailyCollectionProcessor(dtCust);

                //WriteLogFile.WriteLog("------------- Start Customer Query Payment Collection ------------");
                //CustomerQueryPaymentProcessor();
                //WriteLogFile.WriteLog("------------- Start Customer Query Payment Collection Manual ------------");

            }
            catch (Exception ex)
            {

                WriteLogFile.WriteLog(ex.Message);

            }
            finally
            {
                timer1.Enabled = true;

                timer1.Start();
            }


        }
        private void OnTimerEvent2(object sender, EventArgs e)
        {
            try
            {
                timer2.Stop();
                timer2.Enabled = false;

                WriteLogFile.WriteLog("------------- Start MK Error Again Process ------------");
                DataTable dtCust = db.GetDataByProc("sp_PendingMkErrorForCollectionProcessor");
                DailyCollectionProcessor(dtCust);

            }
            catch (Exception ex)
            {

                WriteLogFile.WriteLog(ex.Message);

            }
            finally
            {
                timer2.Enabled = true;

                timer2.Start();
            }
        }

        string getCompanyInfo()

        {
            var dtbiturl = new DBUtility().GetDataByProc("sp_getCompanyInformation");
            var biturl = dtbiturl.Rows[0]["bitUrl"].ToString();

            return biturl;
        }

        string MobileNo;
        void Mobile()
        {
            DataTable mob = new DBUtility().GetDataBySQLString("SELECT MobileNo FROM CompanyInfo WHERE id=1");
            MobileNo = mob.Rows[0]["MobileNo"].ToString();
        }
        public void DailyCollectionProcessor(DataTable dtCust)
        {
            try
            {
                MkConnection objMKConnection = new MkConnection();
                DBUtility dBUtility = new DBUtility();

                // DataTable dtCust = new DBUtility().GetDataByProc("sp_CustomerListForCollectionProcessorV2");

                DateTime ProcessStartTime = DateTime.Now;
                int NoOfCustomer = dtCust.Rows.Count;
                WriteLogFile.WriteLog("Total Collection Found: " + NoOfCustomer);
                string MkStatus = "";

                int ProcessorID = 0;
                string CustomerID = "", SMSText = "";
                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", ProcessErrorlog = "", MKLogError = "";
                string InsType = "", mkUser = "";
                listBox1.Items.Clear();
                foreach (DataRow Custdr in dtCust.Rows)
                {

                    try
                    {
                        CustomerID = Custdr["CustomerID"].ToString();
                        string Refno = Custdr["RefNo"].ToString();
                        string SNID = Custdr["SNID"].ToString();
                        string RefDate = Custdr["RefDate"].ToString();
                        string TranID = Custdr["TranID"].ToString();
                        string EntryDate = Custdr["EntryDate"].ToString();


                        var msg = string.Concat("Collection is found of CID: ", CustomerID, ", Tran ID:", TranID, ", Tran Date:", EntryDate);
                        // listBox1.Items.Add(DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + msg);
                        WriteLogFile.WriteLog(msg);
                        Hashtable ht = new Hashtable();
                        ht.Add("CustomerID", CustomerID);

                        DataTable dtCustomerInfo = dBUtility.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();

                        foreach (DataRow dr in dtCustomerInfo.Rows)
                        {

                            if (!String.IsNullOrEmpty(dr["EndDate"].ToString()) && !String.IsNullOrEmpty(RefDate))
                            {

                                decimal debit = Conversion.TryCastDecimal(dr["Debit"].ToString());
                                decimal credit = Conversion.TryCastDecimal(dr["Credit"].ToString());
                                decimal CL = Conversion.TryCastDecimal(dr["CreditLimit"].ToString());
                                decimal PV = Conversion.TryCastDecimal(dr["TotalMRC"].ToString());
                                decimal DSC = Conversion.TryCastDecimal(dr["Discount"].ToString());




                                DateTime CLD = Conversion.TryCastDate(RefDate);
                                DateTime ED = Conversion.TryCastDate(dr["EndDate"].ToString());


                                string Hostname = "", Username = "", Password = "", IPAddress = "", Mobile = "", mkVersion = "";
                                string RouterID = ""; decimal NetMRC = 0; int ProtocolID = 0;

                                Username = dr["RouterUserName"].ToString();
                                Password = dr["Password"].ToString();
                                IPAddress = dr["IPAddress"].ToString();
                                Hostname = dr["Host"].ToString();
                                NetMRC = decimal.Parse(dr["NetMRC"].ToString());

                                RouterID = dr["RouterID"].ToString();
                                ProtocolID = int.Parse(dr["ProtocolID"].ToString());
                                Mobile = dr["Mobile"].ToString();

                                DateTime CED = DateTime.Parse(dr["EndDate"].ToString());
                                InsType = dr["InsType"].ToString();
                                mkUser = dr["MkUser"].ToString();
                                mkVersion = dr["mkVersion"].ToString();
                                int port = Conversion.TryCastInteger(dr["APIPort"].ToString());
                                //MkConnStatus OBJMkStatus = objMKConnection.MikrotikStatus(Hostname, Username, Password, mkVersion, ProtocolID, CustomerID, Conversion.TryCastInteger(InsType), mkUser);

                                //if (OBJMkStatus.MikrotikStatus == 1)
                                //{
                                //    MkStatus = "true";
                                //}
                                //else
                                //{
                                //    MkStatus = "false";
                                //}
                                MkStatus = "true";// close status checking
                                decimal Balance = credit - debit;

                                if (CLD <= ED)     //
                                {

                                    decimal INV = debit;
                                    decimal MR = credit + CL;

                                    if (INV <= MR)
                                    {
                                        // MK ON 
                                        try
                                        {
                                            MkConnStatus objMkConnStatusEnable = objMKConnection.EnableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID,  Conversion.TryCastInteger(InsType), mkUser, port);

                                            if (objMkConnStatusEnable.StatusCode == "200")
                                            {
                                                ht.Add("CustomerID", CustomerID);
                                                ht.Add("POPId", RouterID);
                                                ht.Add("CustomerIP", IPAddress);
                                                ht.Add("Status", 1);
                                                ht.Add("ProcessID", 200);
                                                ht.Add("EntryID", PinNumber);

                                                var dResponse = dBUtility.GetDataByProc(ht, "sp_insertMKlogNCustStatus");
                                                ht.Clear();
                                                var custStatus = dResponse.Rows[0]["Feedback"].ToString();
                                                if (dResponse != null && custStatus == "Customer status changed successfully!")
                                                {
                                                    SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";
                                                    SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "true #";

                                                    // ProcessedStatus=1  Success full 

                                                    // UPDATE BILLING MASTER IsProcessed=1
                                                    DataTable ProcessUpdate = dBUtility.GetDataBySQLString("UPDATE BillingMaster SET IsProcessed = 1,ProcessedStatus=1 WHERE SNID = '" + SNID + "' SELECT 'SUCCESS' AS SUCCESS");
                                                    var msg1 = " Connected CID: " + CustomerID;
                                                    // listBox1.Items.Add(DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + msg1);
                                                    WriteLogFile.WriteLog(msg1);
                                                }
                                                else
                                                {
                                                    WriteLogFile.WriteLog(CustomerID + "==>" + custStatus);

                                                }
                                            }

                                            // Insert Error log for Mk enable

                                            else
                                            {


                                                MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusEnable.RetMessage.ToString() + " #";
                                                InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusEnable.RetMessage.ToString(), objMkConnStatusEnable.StatusCode.ToString());
                                                DataTable ProcessUpdate = dBUtility.GetDataBySQLString("UPDATE BillingMaster SET ProcessedStatus = 2 WHERE SNID = '" + SNID + "' SELECT 'SUCCESS' AS SUCCESS");
                                                WriteLogFile.WriteLog(CustomerID + "==>" + objMkConnStatusEnable.RetMessage.ToString());

                                                //WriteLogFile.WriteLog(CustomerID + "==>" + MKLogError);
                                            }
                                        }
                                        catch (Exception rx)
                                        {

                                            WriteLogFile.WriteLog(CustomerID + "==>" + rx.Message);
                                        }

                                    }

                                    else if (INV > MR)
                                    {
                                        //MK OFF,  DISCONTINUE

                                        try
                                        {


                                            MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID,  Conversion.TryCastInteger(InsType), mkUser, port);

                                            if (objMkConnStatusDisable.StatusCode == "200")
                                            {
                                                ht.Add("CustomerID", CustomerID);
                                                ht.Add("POPId", int.Parse(RouterID));
                                                ht.Add("CustomerIP", IPAddress);
                                                ht.Add("Status", 0);
                                                ht.Add("StatusID", 9);
                                                ht.Add("ProcessID", 200);
                                                ht.Add("EntryID", PinNumber);
                                                ht.Add("ActivityDetail", "Discontinue_from_DailyProcess");
                                                ht.Add("SeconderyStatus", "LOCK_FROM_BILLING");

                                                var dResponse = dBUtility.GetDataByProc(ht, "sp_insertMKlogNCustStatus");
                                                ht.Clear();
                                                var custStatus = dResponse.Rows[0]["Feedback"].ToString();
                                                if (dResponse != null && custStatus == "Customer status changed successfully!")
                                                {
                                                    SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";
                                                    SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "false #";

                                                    // SMS 
                                                    // SMSText = @"Sorry! Your Internet connection has been discontinued due to insufficient balance.";

                                                    // ProcessedStatus=3  insufficient balance

                                                    // UPDATE BILLING MASTER IsProcessed=1
                                                    DataTable ProcessUpdate = dBUtility.GetDataBySQLString("UPDATE BillingMaster SET IsProcessed = 1,ProcessedStatus = 3 WHERE SNID = '" + SNID + "' SELECT 'SUCCESS' AS SUCCESS");
                                                    var msg1 = string.Concat(CustomerID, " is not connected for insufficient balance");
                                                    //listBox1.Items.Add(DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + msg1);
                                                    WriteLogFile.WriteLog(msg1);

                                                    SMSText = @"অপর্যাপ্ত ব্যালেন্সের কারণে আপনার ইন্টারনেট সংযোগ দিতে ব্যর্থ হয়েছে।";

                                                    SendSMS(CustomerID, SMSText, Mobile);
                                                }
                                                else
                                                {
                                                    WriteLogFile.WriteLog(CustomerID + "==>" + custStatus);

                                                }
                                            }

                                            // Insert MK Error log
                                            else
                                            {


                                                MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusDisable.RetMessage.ToString() + " #";
                                                InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusDisable.RetMessage.ToString(), objMkConnStatusDisable.StatusCode.ToString());
                                                DataTable ProcessUpdate = dBUtility.GetDataBySQLString("UPDATE BillingMaster SET ProcessedStatus = 2 WHERE SNID = '" + SNID + "' SELECT 'SUCCESS' AS SUCCESS");

                                                WriteLogFile.WriteLog(CustomerID + "==>" + objMkConnStatusDisable.RetMessage.ToString());

                                            }
                                        }
                                        catch (Exception rx)
                                        {

                                            WriteLogFile.WriteLog(CustomerID + "==>" + rx.Message);
                                        }
                                    }
                                }
                                else if (CLD > ED)
                                {
                                    decimal INV = debit + PV - DSC;
                                    decimal MR = credit + CL;
                                    if (INV <= MR)
                                    {
                                        // INV GEN, BILL CALENDER
                                        ht.Add("CustomerID", CustomerID);
                                        ht.Add("EntryID", PinNumber);
                                        ht.Add("ProcessID", 200);
                                        ht.Add("MRCAmount", NetMRC);
                                        ht.Add("Narration", "MRC Invoice");
                                        DataTable Insert = dBUtility.GetDataByProc(ht, "sp_MRCInvoiceGeneration");
                                        ht.Clear();

                                        SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";
                                        // SMS 
                                        //SMSText = @"Greetings! Your current billing cycle has been started from " + DateTime.Today.ToString("dd/MM/yyyy") + " and package price " + NetMRC + ", you can access our online billing on " + getCompanyInfo() + ".";


                                        try
                                        {


                                            // UPDATE BILLING MASTER IsProcessed=1

                                            //MK Enable 
                                            MkConnStatus objMkConnStatusEnable = objMKConnection.EnableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser, port);


                                            if (objMkConnStatusEnable.StatusCode == "200")
                                            {
                                                ht.Add("CustomerID", CustomerID);
                                                ht.Add("POPId", int.Parse(RouterID));
                                                ht.Add("CustomerIP", IPAddress);
                                                ht.Add("Status", 1);
                                                ht.Add("StatusID", 1);
                                                ht.Add("ProcessID", 200);
                                                ht.Add("EntryID", PinNumber);
                                                ht.Add("ActivityDetail", "Active_from_CollectionProcess");

                                                var dResponse = dBUtility.GetDataByProc(ht, "sp_insertMKlogNCustStatus");
                                                ht.Clear();
                                                var custStatus = dResponse.Rows[0]["Feedback"].ToString();
                                                if (dResponse != null && custStatus == "Customer status changed successfully!")
                                                {
                                                    SMSText = @"ধন্যবাদ, আপনার নতুন বিলিং সাইকেল শুরুর তারিখ " + DateTime.Today.ToString("dd/MM/yyyy") + " এবং প্যাকেজ মূল্য টাকা " + NetMRC + ", আমাদের অনলাইন পোর্টাল ব্যবহার করতে লগইন করুন " + getCompanyInfo() + ".";
                                                    SendSMS(CustomerID, SMSText, Mobile);
                                                    //ProcessedStatus = 1 Success full Bill Generation
                                                    DataTable ProcessUpdate = dBUtility.GetDataBySQLString("UPDATE BillingMaster SET IsProcessed = 1,ProcessedStatus = 1 WHERE SNID = '" + SNID + "' SELECT 'SUCCESS' AS SUCCESS");

                                                    SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + DateTime.Today.AddDays(31) + ", MK:" + MkStatus + " #";

                                                    var msg1 = " Connected CID: " + CustomerID;
                                                    //listBox1.Items.Add(DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + msg1);
                                                    WriteLogFile.WriteLog(msg1);

                                                }
                                                else
                                                {
                                                    WriteLogFile.WriteLog(CustomerID + "==>" + custStatus);

                                                }


                                            }

                                            // Insert Error log for Mk enable

                                            else
                                            {
                                                // ProcessedStatus=2 MK Error
                                                MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusEnable.RetMessage.ToString() + " #";
                                                InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusEnable.RetMessage.ToString(), objMkConnStatusEnable.StatusCode.ToString());
                                                DataTable ProcessUpdate = dBUtility.GetDataBySQLString("UPDATE BillingMaster SET ProcessedStatus = 2 WHERE SNID = '" + SNID + "' SELECT 'SUCCESS' AS SUCCESS");

                                                WriteLogFile.WriteLog(CustomerID + "==>" + objMkConnStatusEnable.RetMessage.ToString());

                                            }
                                        }
                                        catch (Exception rx)
                                        {

                                            WriteLogFile.WriteLog(CustomerID + "==>" + rx.Message);
                                        }
                                    }
                                    else
                                    {
                                        // ProcessedStatus=3  insufficient balance
                                        DataTable ProcessUpdate = dBUtility.GetDataBySQLString("UPDATE BillingMaster SET IsProcessed = 1,ProcessedStatus=3 WHERE SNID = '" + SNID + "' SELECT 'SUCCESS' AS SUCCESS");
                                        var msg1 = string.Concat(CustomerID, " is not connected for insufficient balance");


                                        //var msg1 = CustomerID + " is not connected for insufficient balance";

                                        WriteLogFile.WriteLog(msg1);
                                    }
                                }
                            }
                            else
                            {
                                // DO something
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ProcessErrorlog += "ID:" + CustomerID + ", Er. " + ex.ToString() + " #";
                        WriteLogFile.WriteLog(ProcessErrorlog);
                    }
                }
                if (dtCust != null && dtCust.Rows.Count > 0)
                {
                    Hashtable ht1 = new Hashtable();
                    ht1.Add("SuccessLogBeforeProcess", SuccessLogBeforeProcess);
                    ht1.Add("SuccessLogAfterProcess", SuccessLogAfterProcess);
                    ht1.Add("ProcessErrorlog", ProcessErrorlog);
                    ht1.Add("MKLogError", MKLogError);
                    ht1.Add("ProcessStartTime", ProcessStartTime);
                    ht1.Add("ProcessEndTime", DateTime.Now);
                    ht1.Add("NoOfCustomer", NoOfCustomer);

                    ht1.Add("ID", 2);

                    DataTable PROCESSLOG = dBUtility.GetDataByProc(ht1, "sp_InsertProcessLog");
                    ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());
                }

            }
            catch (Exception xx)
            {
                WriteLogFile.WriteLog(xx.ToString());
            }

        }

        public void CustomerQueryPaymentProcessor()
        {
            try
            {
                var dtCust = new DBUtility().GetDataByProc("sp_getCustomerQueryPayment");//Cursor in SP

                DateTime ProcessStartTime = DateTime.Now;
                WriteLogFile.WriteLog("Running Successfully for Customer Query Processor");


            }
            catch (Exception ex)
            {

                WriteLogFile.WriteLog("Customer Query: " + ex.Message);
            }
        }



        public void SendSMS(string CustomerID, string SMSText, string Mobile)
        {
            Hashtable hashTable = new Hashtable();

            hashTable.Add("CustomerID", CustomerID);
            hashTable.Add("SMSText", SMSText);
            hashTable.Add("Mobile", Mobile);
            new DBUtility().GetDataByProc(hashTable, "sp_insertSMS_Schedule");

        }


        #region InsertMikrotikErrorLog
        public void InsertMikrotikErrorLog(string CustomerID, string RouterID, string Hostname, string IPAddress, string RetMessage, string Statuscode)
        {

            Hashtable ht = new Hashtable();

            ht.Add("CustomerID", CustomerID);
            ht.Add("POPId", RouterID);
            ht.Add("CustomerIP", IPAddress);
            ht.Add("Error_description", RetMessage);
            ht.Add("ProcessID", 200);

            ht.Add("EntryID", PinNumber);
            //ht.Add("IPAddress", IPAddress);
            ht.Add("StatusCode", Statuscode);

            new DBUtility().InsertData(ht, "sp_insertMKCommunication_Errorlog");
        }


        #endregion

        /// <summary>
        /// based on the selected radio box returns the scheduler enum
        /// </summary>
        /// <returns></returns>


        private Scheduler getScheduler()
        {
            //if(rdbMinute.Checked)
            //    return Scheduler.EveryMinutes;
            //if (rdbTenMinute.Checked)
            //    return Scheduler.EveryTenMinutes;
            //if (rdbHour.Checked)
            //    return Scheduler.EveryHour;

            //if (rdbEightHour.Checked)
            //    return Scheduler.EveryEightHours;

            //if (rdbHalfDay.Checked)
            //    return Scheduler.EveryHalfDay;
            //if (rdbDay.Checked)
            //    return Scheduler.EveryDay;
            //if (rdbWeek.Checked)
            //    return Scheduler.EveryWeek;
            //if (rdbMonth.Checked)
            //    return Scheduler.EveryMonth;
            //if (rdbYear.Checked)
            //    return Scheduler.EveryYear;

            //default
            return Scheduler.EveryMinutes;
        }

        /// <summary>
        /// canceling the scheduler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>



        /// <summary>
        /// Exits the app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitBtn_Click(object sender, EventArgs e)
        {

            this.Close();
        }


    }

}
