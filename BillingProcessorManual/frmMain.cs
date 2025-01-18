
using BBS.Utilitys;
using BillingERPConn;
using MkCommunication;
using SWIFTDailyProcessor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace BillingProcessorManual
{
    public partial class frmMain : Form
    {
        DBUtility idb = new DBUtility();

        public frmMain()
        {
            InitializeComponent();

            // numHours.Value = DateTime.Now.Hour;
            //  numMins.Value = DateTime.Now.Minute;

        }


        private void frmMain_Load(object sender, EventArgs e)
        {


            // schedule_Timer_Callback();
        }

        int PinNumber = 10000;

        int ProcessInterval = 0;


        private void schedule_Timer_Callback()
        {

            Hashtable ht = new Hashtable();
            ht.Add("ID", 1);
            DataTable dt = idb.GetDataByProc(ht, "sp_getProcessConfigInfo");
            int StartHour = int.Parse(dt.Rows[0]["StartHour"].ToString());
            int EndHour = int.Parse(dt.Rows[0]["EndHour"].ToString());
            int minutes = int.Parse(dt.Rows[0]["StartMin"].ToString());
            ProcessInterval = int.Parse(dt.Rows[0]["Interval"].ToString()); // Should be Milisecond

            //   txtStartHour.Text = StartHour.ToString();
            //     txtEndHour.Text = EndHour.ToString();
            //    lblText.Text = "This Process is run with "+(ProcessInterval/60000).ToString() + " Minutes Interval." ;

            timer1.Interval = 10000;  // milisecond

            int curr_hour = DateTime.Now.Hour;

            if (curr_hour >= StartHour && curr_hour <= EndHour)
            {
                timer1.Enabled = true;
                timer1.Tick += new System.EventHandler(OnTimerEvent);

            }
        }

        private void OnTimerEvent(object sender, EventArgs e)
        {
            timer1.Interval = ProcessInterval;
            string strText = "**********Scheduler has been started!*****" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt");
            WriteLogFile.WriteLog(strText);

            listBox1.Items.Add(strText);

            //  methodToCall();


            // lblSync.Text = "Last Sync: " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt");
        }

        string MobileNo;
        void Mobile()
        {
            DataTable mob = idb.GetDataBySQLString("SELECT MobileNo FROM CompanyInfo WHERE id=1");
            MobileNo = mob.Rows[0]["MobileNo"].ToString();
        }

        private void methodToCall()
        {
            //Mobile(); 
            //PackageChangeRequest();
            //ShiftingRequest();

            //RequestToTemporaryStop();
            //DiscontinueRequestToDevColl();
            //HandoverPendingProcessDaily();
            //TempStopToDeviceCollection();
            //InactiveToDeviceColl();
            //ReminderProcessorBill();
            //ReminderProcessorDue();

            //DailyBillingProcessor();

        }

        string getCompanyInfo()

        {
            var dtbiturl = idb.GetDataByProc("sp_getCompanyInformation");
            var biturl = dtbiturl.Rows[0]["bitUrl"].ToString();

            return biturl;
        }


        #region DailyBillingProcessor

        public void DailyBillingProcessor()
        {
            try
            {

                MkConnection objMKConnection = new MkConnection();

                DataTable dtCust = idb.GetDataBySQLString("SELECT [CustomerID] FROM test");

                DateTime CD = DateTime.Today;
                DateTime ProcessStartTime = DateTime.Now;
                int NoOfCustomer = 0;

                string MkStatus = "";

                int ProcessorID = 0;
                string CustomerID = "", SMSText = "";
                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", ProcessErrorlog = "", MKLogError = "";
                string InsType = "", mkUser = "";
                foreach (DataRow dr in dtCust.Rows)
                {
                    try
                    {
                        Hashtable ht = new Hashtable();
                        CustomerID = dr["CustomerID"].ToString();

                        ht.Add("CustomerID", CustomerID);
                        DataTable dtCustomerInfo = idb.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();
                        foreach (DataRow datarow in dtCustomerInfo.Rows)
                        {
                            if (!String.IsNullOrEmpty(datarow["EndDate"].ToString()))
                            {
                                decimal debit = Conversion.TryCastDecimal(datarow["Debit"].ToString());
                                decimal credit = Conversion.TryCastDecimal(datarow["Credit"].ToString());
                                decimal CL = Conversion.TryCastDecimal(datarow["CreditLimit"].ToString());
                                decimal PV = Conversion.TryCastDecimal(datarow["TotalMRC"].ToString());
                                decimal DSC = Conversion.TryCastDecimal(datarow["Discount"].ToString());
                                DateTime ED = Conversion.TryCastDate(datarow["EndDate"].ToString());

                                string Hostname = "", Username = "", Password = "", IPAddress = "", Mobile = "";
                                decimal NetMRC = 0; int RouterID = 0, ProtocolID = 0; string mkVersion = "";

                                Username = datarow["RouterUserName"].ToString();
                                Password = datarow["Password"].ToString();
                                IPAddress = datarow["IPAddress"].ToString();
                                Hostname = datarow["Host"].ToString();
                                NetMRC = decimal.Parse(datarow["NetMRC"].ToString());
                                RouterID = int.Parse(datarow["RouterID"].ToString());
                                ProtocolID = int.Parse(datarow["ProtocolID"].ToString());
                                Mobile = datarow["Mobile"].ToString();
                                decimal Balance = credit - debit;
                                DateTime CED = DateTime.Parse(datarow["EndDate"].ToString());
                                InsType = datarow["InsType"].ToString();
                                mkUser = datarow["MkUser"].ToString();
                                mkVersion = datarow["mkVersion"].ToString();

                                if (CD > ED)
                                {
                                    decimal INV = debit + PV - DSC;
                                    decimal CA = credit + CL;

                                    if (INV > CA)
                                    {
                                        // MK OFF, DIscontinue 

                                        MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser);

                                        if (objMkConnStatusDisable.StatusCode == "200")
                                        {
                                            ht.Add("CustomerID", CustomerID);
                                            ht.Add("POPId", RouterID);
                                            ht.Add("CustomerIP", IPAddress);
                                            ht.Add("Status", 0);
                                            ht.Add("StatusID", 9);
                                            ht.Add("ProcessID", 100);
                                            ht.Add("EntryID", PinNumber);
                                            ht.Add("ActivityDetail", "LOCK_FROM_BILLING");
                                            ht.Add("SeconderyStatus", "LOCK_FROM_BILLING");

                                            idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                            ht.Clear();

                                            SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";
                                            SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "false #";

                                            // SMS 
                                            string dateStr = DateTime.Today.ToString("MMM dd, yyyy");
                                            SMSText = "Your Internet has been locked on " + dateStr + ".Pls pay your bill to avoid disconnection. For details visit " + getCompanyInfo() + ". Pls ignore if already paid";
                                            SendSMS(CustomerID, SMSText, Mobile);


                                            NoOfCustomer++;
                                        }

                                        // Insert MK Error log
                                        else
                                        {
                                            MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusDisable.RetMessage.ToString() + " #";
                                            InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusDisable.RetMessage.ToString(), objMkConnStatusDisable.StatusCode.ToString());
                                        }

                                    }
                                    else if (INV <= CA)
                                    {
                                        // MK ON, INV GEN

                                        // -----  Bill generation ----------//

                                        ht.Add("CustomerID", CustomerID);
                                        ht.Add("EntryID", PinNumber);
                                        ht.Add("ProcessID", 100);
                                        ht.Add("MRCAmount", NetMRC);
                                        ht.Add("ActivityDetail", "ACTIVE_FROM_BILLING");

                                        DataTable Insert = idb.GetDataByProc(ht, "sp_BillGeneDuringDailyProcess");
                                        ht.Clear();
                                        SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";

                                        // SMS 
                                        string dateStr = DateTime.Today.ToString("MMM dd, yyyy");
                                        SMSText = "Your current billing cycle has been started from " + dateStr + " for MRC " + NetMRC + " .You can access our online billing on " + getCompanyInfo() + ".";
                                        SendSMS(CustomerID, SMSText, Mobile);
                                        NoOfCustomer++;


                                        //  Mikrotik Enable Check 
                                        MkConnStatus objMkConnStatusEnable = objMKConnection.EnableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser);


                                        if (objMkConnStatusEnable.StatusCode == "200")
                                        {

                                            ht.Add("CustomerID", CustomerID);
                                            ht.Add("POPId", RouterID);
                                            ht.Add("CustomerIP", IPAddress);
                                            ht.Add("Status", 1);
                                            ht.Add("ProcessID", 100);
                                            ht.Add("EntryID", PinNumber);

                                            idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                            ht.Clear();

                                            SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + DateTime.Today.AddDays(31) + ", MK:" + "true #";

                                        }

                                        // --- Insert MK Error log for Enabling IP-----//

                                        else
                                        {
                                            MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusEnable.RetMessage.ToString() + " #";
                                            InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusEnable.RetMessage.ToString(), objMkConnStatusEnable.StatusCode.ToString());
                                        }
                                    }

                                }

                                else if (CD <= ED)
                                {
                                    decimal INV = debit;
                                    decimal MR = credit + CL;

                                    if (INV <= MR)
                                    {
                                        // MK ON   NO NEED TO CODE??

                                    }
                                    else if (INV > MR)
                                    {
                                        //MK OFF Discontinue

                                        MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser);

                                        if (objMkConnStatusDisable.StatusCode == "200")
                                        {
                                            ht.Add("CustomerID", CustomerID);
                                            ht.Add("POPId", RouterID);
                                            ht.Add("CustomerIP", IPAddress);
                                            ht.Add("Status", 0);
                                            ht.Add("StatusID", 9);
                                            ht.Add("ProcessID", 100);
                                            ht.Add("EntryID", PinNumber);
                                            ht.Add("ActivityDetail", "LOCK_FROM_BILLING");
                                            ht.Add("SeconderyStatus", "LOCK_FROM_BILLING");

                                            idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                            ht.Clear();
                                            SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";
                                            SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "false #";

                                            string dateStr = DateTime.Today.ToString("MMM dd, yyyy");
                                            SMSText = "Your Internet has been locked on " + dateStr + ". Pls pay your bill to avoid disconnection. For details visit " + getCompanyInfo() + ". Pls ignore if already paid";
                                            SendSMS(CustomerID, SMSText, Mobile);

                                            NoOfCustomer++;
                                        }

                                        // --- Insert MK Error log-----//
                                        else
                                        {
                                            MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusDisable.RetMessage.ToString() + " #";
                                            InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusDisable.RetMessage.ToString(), objMkConnStatusDisable.StatusCode.ToString());
                                        }
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
                HT.Add("ID", 1);


                DataTable PROCESSLOG = idb.GetDataByProc(HT, "sp_InsertProcessLog");
                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                // SMS 

                if (ProcessErrorlog != "" || MKLogError != "")
                {
                    SMSText = "Error!! " + " for Billing Processor id " + ProcessorID + ", please take the neccessery steps to solve the problem.";
                    if (MobileNo != "")
                    {
                        SendSMS(CustomerID, SMSText, MobileNo);
                    }

                }

            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLog("BillingProcessor: " + ex.Message);
            }
        }

        #endregion

        #region RequestToTemporaryStop

        public void RequestToTemporaryStop()
        {
            try
            {
                MkConnection objMKConnection = new MkConnection();

                DataTable dtCust = idb.GetDataByProc("sp_CustomerListForTemporaryStopRequest");
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
                        DataTable dtCustomerInfo = idb.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();

                        string Hostname = "", Username = "", Password = "", IPAddress = "", Mobile = "";
                        int RouterID = 0, ProtocolID = 0; string mkVersion = "";

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

                            MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser);

                            if (objMkConnStatusDisable.StatusCode == "200")
                            {
                                ht.Add("CustomerID", CustomerID);
                                ht.Add("POPId", RouterID);
                                ht.Add("CustomerIP", IPAddress);
                                ht.Add("Status", 0);
                                ht.Add("StatusID", 9);
                                ht.Add("ProcessID", 300);
                                ht.Add("EntryID", PinNumber);
                                ht.Add("ActivityDetail", "TemporaryStop_from_RequestProcess");
                                ht.Add("SeconderyStatus", SeconderyStatus);
                                ht.Add("DeviceCollStatus", 0);

                                idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                ht.Clear();

                                DataTable reqCompleted = idb.GetDataBySQLString("UPDATE RequestMaster SET IsCompleted =1, CompleteBy = " + PinNumber + ", CompleteDate = '" + DateTime.Today + "'  WHERE RequestRefNo = '" + RequestRefNo + "'; SELECT 'SUCCESS' AS SUCCESS");

                                SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";
                                SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "false #";

                                SMSText = "Your internet connection has been Temporarily stoped because of your Temporary stop request.";
                                SendSMS(CustomerID, SMSText, Mobile);

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
                HT.Add("ProcessorTypeName", "RequestToTemporaryStop");
                HT.Add("ID", 3);

                DataTable PROCESSLOG = idb.GetDataByProc(HT, "sp_InsertProcessLog");
                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                if (ProcessErrorlog != "" || MKLogError != "")
                {
                    SMSText = "Error!! " + " for Processor id " + ProcessorID + ", please teke the neccessery steps to solve the problem.";
                    if (MobileNo != "")
                    {
                        SendSMS(CustomerID, SMSText, MobileNo);
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

                DataTable dtCust = idb.GetDataByProc("sp_CustomerListForInactiveToDeviceColl");
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

                        Hashtable ht = new Hashtable();
                        CustomerID = dr["CustomerID"].ToString();

                        ht.Add("CustomerID", CustomerID);
                        DataTable dtCustomerInfo = idb.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();

                        string Hostname = "", Username = "", Password = "", IPAddress = "", Mobile = "", SecondaryStatus = "";
                        int RouterID = 0, ProtocolID = 0; string mkVersion = "";

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

                            if (objMkConnStatusDisable.StatusCode == "200")
                            {
                                ht.Add("CustomerID", CustomerID);
                                ht.Add("POPId", RouterID);
                                ht.Add("CustomerIP", IPAddress);
                                ht.Add("Status", 0);
                                ht.Add("StatusID", 9);
                                ht.Add("ProcessID", 300);
                                ht.Add("EntryID", PinNumber);
                                ht.Add("ActivityDetail", "INACTIVE_FROM_PROCESS");
                                ht.Add("DeviceCollStatus", 1);
                                ht.Add("SeconderyStatus", SecondaryStatus);

                                idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                ht.Clear();

                                SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";
                                SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "false #";

                                // SMSText = "Your internet connection has been Temporarily stoped because of your Temporary stop request.";
                                // SendSMS(CustomerID, SMSText, Mobile);

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

                DataTable PROCESSLOG = idb.GetDataByProc(HT, "sp_InsertProcessLog");
                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                if (ProcessErrorlog != "" || MKLogError != "")
                {
                    SMSText = "Error!! " + " for Processor id " + ProcessorID + ", please teke the neccessery steps to solve the problem.";
                    if (MobileNo != "")
                    {
                        SendSMS(CustomerID, SMSText, MobileNo);
                    }
                }

            }
            catch (Exception ex)
            {
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

                DataTable dtCust = idb.GetDataByProc("sp_CustomerListForShifting");
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
                        ht.Add("ProcessID", 400);
                        ht.Add("EntryID", PinNumber);
                        ht.Add("RequestRefNo", RequestRefNo);

                        DataTable DATA = idb.GetDataByProc(ht, "sp_CustomerShiftingfromProcessor");
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

                DataTable PROCESSLOG = idb.GetDataByProc(HT, "sp_InsertProcessLog");
                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                if (ProcessErrorlog != "" || MKLogError != "")
                {
                    SMSText = "Error!! " + " for Processor id " + ProcessorID + ", please teke the neccessery steps to solve the problem.";
                    if (MobileNo != "")
                    {
                        SendSMS(CustomerID, SMSText, MobileNo);
                    }
                }
            }

            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ex.Message.ToString());
            }
        }

        #endregion


        #region PackageChangeRequest 

        public void PackageChangeRequest()
        {
            try
            {
                MkConnection objMKConnection = new MkConnection();

                DataTable dtCust = idb.GetDataByProc("sp_CustomerListForPackageChanged");
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
                        ht.Add("ProcessID", 400);
                        ht.Add("EntryID", PinNumber);
                        ht.Add("RequestRefNo", RequestRefNo);

                        DataTable DATA = idb.GetDataByProc(ht, "sp_CustomerPackageChangedfromProcessor");

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
                HT.Add("ProcessorTypeName", "PackageChangeRequest");
                HT.Add("ID", 3);

                DataTable PROCESSLOG = idb.GetDataByProc(HT, "sp_InsertProcessLog");
                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                if (ProcessErrorlog != "" || MKLogError != "")
                {
                    SMSText = "Error!! " + " for Processor id " + ProcessorID + ", please teke the neccessery steps to solve the problem.";
                    if (MobileNo != "")
                    {
                        SendSMS(CustomerID, SMSText, MobileNo);
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

                DataTable dtCust = idb.GetDataByProc("sp_CustomerListForDiscontinueRequest");
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

                    string SeconderyStatus = "DISC._FROM_REQUST";

                    try
                    {

                        CustomerID = dr["CustomerID"].ToString();
                        Hashtable ht = new Hashtable();

                        ht.Add("CustomerID", CustomerID);
                        DataTable dtCustomerInfo = idb.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();

                        string Hostname = "", Username = "", Password = "", IPAddress = "", Mobile = "";
                        int RouterID = 0, ProtocolID = 0; string mkVersion = "";

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

                            if (objMkConnStatusDisable.StatusCode == "200")
                            {
                                ht.Add("CustomerID", CustomerID);
                                ht.Add("POPId", RouterID);
                                ht.Add("CustomerIP", IPAddress);
                                ht.Add("Status", 0);
                                ht.Add("StatusID", 2);
                                ht.Add("ProcessID", 300);
                                ht.Add("EntryID", PinNumber);
                                ht.Add("ActivityDetail", "DISC._FROM_REQUST");
                                ht.Add("SeconderyStatus", SeconderyStatus);
                                ht.Add("DeviceCollStatus", 1);

                                idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                ht.Clear();

                                DataTable reqCompleted = idb.GetDataBySQLString("UPDATE RequestMaster SET IsCompleted =1, CompleteBy = " + PinNumber + ", CompleteDate = '" + DateTime.Today + "'  WHERE RequestRefNo = '" + RequestRefNo + "'; SELECT 'SUCCESS' AS SUCCESS");

                                SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";
                                SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "true #";

                                //SMSText = "Your internet connection has been Temporarily stoped because of your Temporary stop request.";
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
                HT.Add("ProcessorTypeName", "DiscontinueRequestToDevColl");
                HT.Add("ID", 3);


                DataTable PROCESSLOG = idb.GetDataByProc(HT, "sp_InsertProcessLog");
                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                if (ProcessErrorlog != "" || MKLogError != "")
                {
                    SMSText = "Error!! " + " for Processor id " + ProcessorID + ", please teke the neccessery steps to solve the problem.";
                    if (MobileNo != "")
                    {
                        SendSMS(CustomerID, SMSText, MobileNo);
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

                DataTable dtCust = idb.GetDataByProc("sp_CustomerListForForInactiveFromBillingLock"); //sp_CustomerListForFORInactive
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
                        Hashtable ht = new Hashtable();

                        ht.Add("CustomerID", CustomerID);
                        DataTable dtCustomerInfo = idb.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();

                        string Hostname = "", Username = "", Password = "", IPAddress = "", Mobile = "", SecondaryStatus = "";
                        int RouterID = 0, ProtocolID = 0; string mkVersion = "";

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

                            if (objMkConnStatusDisable.StatusCode == "200")
                            {
                                ht.Add("CustomerID", CustomerID);
                                ht.Add("POPId", RouterID);
                                ht.Add("CustomerIP", IPAddress);
                                ht.Add("Status", 0);
                                ht.Add("StatusID", 9);
                                ht.Add("ProcessID", 300);
                                ht.Add("EntryID", PinNumber);
                                ht.Add("ActivityDetail", "DISCONTINUE_FROM_TEMP.STOP");
                                ht.Add("DeviceCollStatus", 1);
                                ht.Add("SeconderyStatus", "DISCONTINUE_FROM_TEMP.STOP");

                                idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                ht.Clear();

                                SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";
                                SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "false #";

                                //SMSText = "Your internet connection has been Temporarily stoped because of your Temporary stop request.";
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

                DataTable PROCESSLOG = idb.GetDataByProc(HT, "sp_InsertProcessLog");
                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                if (ProcessErrorlog != "" || MKLogError != "")
                {
                    SMSText = "Error!! " + " for Processor id " + ProcessorID + ", please teke the neccessery steps to solve the problem.";
                    if (MobileNo != "")
                    {
                        SendSMS(CustomerID, SMSText, MobileNo);
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
                DataTable dtCust = idb.GetDataByProc("sp_CustomerListForHandoverpending");

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
                        DataTable dtCustomerInfo = idb.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();
                        foreach (DataRow datarow in dtCustomerInfo.Rows)
                        {

                            string Hostname = "", Username = "", Password = "", IPAddress = "", Mobile = "";
                            int RouterID = 0, ProtocolID = 0; decimal NetMRC = 0; string mkVersion = "";

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

                            if ((NetMRC + OTC - DSC) > (credit + CL))
                            {
                                // MK OFF, DIscontinue 

                                MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser);

                                if (objMkConnStatusDisable.StatusCode == "200")
                                {
                                    ht.Add("CustomerID", CustomerID);
                                    ht.Add("POPId", RouterID);
                                    ht.Add("CustomerIP", IPAddress);
                                    ht.Add("Status", 0);
                                    ht.Add("StatusID", 9);
                                    ht.Add("ProcessID", 300);
                                    ht.Add("EntryID", PinNumber);
                                    ht.Add("ActivityDetail", "LOCK_BEFORE_HANDOVER");
                                    ht.Add("DeviceCollStatus", 0);
                                    ht.Add("SeconderyStatus", "LOCK_BEFORE_HANDOVER");

                                    idb.InsertData(ht, "sp_insertMKlogNCustStatus");

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
                DataTable PROCESSLOG = idb.GetDataByProc(HT, "sp_InsertProcessLog");

                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                // SMS 

                if (ProcessErrorlog != "" || MKLogError != "")
                {
                    SMSText = "Error!! " + " for Processor id " + ProcessorID + ", please teke the neccessery steps to solve the problem.";
                    if (MobileNo != "")
                    {
                        SendSMS(CustomerID, SMSText, MobileNo);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ex.Message.ToString());
            }
        }

        #endregion


        #region SMSReminderProcessor

        private void ReminderProcessorBill()
        {
            Hashtable ht = new Hashtable();

            DataTable dt = idb.GetDataByProc(ht, "sp_getRemindersmsinfoBill");  //
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

        private void ReminderProcessorDue()
        {

            if (IsSMSSendOptionEnabled())
            {
                Hashtable ht = new Hashtable();

                DataTable dt = idb.GetDataByProc(ht, "sp_getRemindersmsinfoDue");
                ht.Clear();
                string CustomerID = "";
                foreach (DataRow dr in dt.Rows)
                {
                    CustomerID = dr["CustomerID"].ToString();
                    try
                    {
                        insertSMSScheduleDue(CustomerID);
                    }
                    catch (Exception)
                    {

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


        #region SMSCreation

        public void SendSMS(string CustomerID, string SMSText, string Mobile)
        {
            Hashtable hashTable = new Hashtable();

            hashTable.Add("CustomerID", CustomerID);
            hashTable.Add("SMSText", SMSText);
            hashTable.Add("Mobile", Mobile);
            idb.GetDataByProc(hashTable, "sp_insertSMS_Schedule");

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

            idb.InsertData(ht, "sp_insertMKCommunication_Errorlog");
        }


        #endregion



        #region ButtonClickMethod

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBillingProcessor_Click(object sender, EventArgs e)
        {

            var confirmResult = MessageBox.Show("Are you sure ??",
                                       "Confirm!",
                                       MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                DailyBillingProcessor();
                lblStat.Text = btnBillingProcessor.Text + " Complete";
            }
            else
            {
                // If 'No', do something here.
            }


        }

        private void btnRequesttoTempStop_Click(object sender, EventArgs e)
        {
            RequestToTemporaryStop();
        }

        private void btnDiscontinueToDeviceColl_Click(object sender, EventArgs e)
        {
            DiscontinueRequestToDevColl();
        }

        private void btnInactiveToDeviceColl_Click(object sender, EventArgs e)
        {
            InactiveToDeviceColl();
        }

        private void btnPackageChange_Click(object sender, EventArgs e)
        {
            PackageChangeRequest();
        }

        private void btnTempStopToDeviceColl_Click(object sender, EventArgs e)
        {
            TempStopToDeviceCollection();
        }

        private void btnShifting_Click(object sender, EventArgs e)
        {
            ShiftingRequest();
        }

        private void btnHandOverPending_Click(object sender, EventArgs e)
        {
            HandoverPendingProcessDaily();
        }

        private void btnSMSRemainder_Click(object sender, EventArgs e)
        {



            var confirmResult = MessageBox.Show("Are you sure ??",
                                       "Confirm!",
                                       MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                ReminderProcessorBill();
                lblStat.Text = btnBillingProcessor.Text + " Complete";
            }
            else
            {
                // If 'No', do something here.
            }
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {



            var confirmResult = MessageBox.Show("Are you sure ??",
                                       "Confirm!",
                                       MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {

                ReminderProcessorDue();
                lblStat.Text = btnBillingProcessor.Text + " Complete";
            }
            else
            {
                // If 'No', do something here.
            }
        }

        private void lblStat_Click(object sender, EventArgs e)
        {

        }
    }
}
