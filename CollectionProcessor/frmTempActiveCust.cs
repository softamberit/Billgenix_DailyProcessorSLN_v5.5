
using BBS.Utilitys;
using BillingERPConn;
using MkCommunication;
using System;
using System.Collections;
using System.Data;
using System.Windows.Forms;


namespace CollectionProcessor
{
   

    public partial class frmTempActiveCust : Form
    {

        MkConnection objMKConnection = null;
        public frmTempActiveCust()
        {
            InitializeComponent();
            //  numHours.Value = DateTime.Now.Hour;
            //  numMins.Value = DateTime.Now.Minute;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            objMKConnection = new MkConnection();

            schedule_Timer_Callback();
        }

        int PinNumber = 10000;

        int ProcessInterval = 0;
        private void schedule_Timer_Callback()
        {
            string strText = "**********Collection Processor Scheduler has been started!*****" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt");
            WriteLogFile.WriteLog(strText);
            //new DBUtility() = new DBUtility();
            Hashtable ht = new Hashtable();
            ht.Add("ID", 2);
            DataTable dt = new DBUtility().GetDataByProc(ht, "sp_getProcessConfigInfo");
            int StartHour = int.Parse(dt.Rows[0]["StartHour"].ToString());
            int EndHour = int.Parse(dt.Rows[0]["EndHour"].ToString());
            int minutes = int.Parse(dt.Rows[0]["StartMin"].ToString());
            ProcessInterval = 5000; // Should be Millisecond

            txtStartHour.Text = EndHour.ToString();
            txtEndHour.Text = StartHour.ToString();
            lblText.Text = "This Process is run with " + (ProcessInterval / 60000).ToString() + " Minutes Interval.";

            timer1.Interval = ProcessInterval;  // millisecond

            //System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer
            //{
            //    Interval = ProcessInterval  ///millisecond
            //};

            int curr_hour = DateTime.Now.Hour;

            //if ((curr_hour < StartHour && curr_hour < EndHour) || (curr_hour > StartHour && curr_hour > EndHour))
            //{

            timer1.Enabled = true;
            timer1.Tick += new System.EventHandler(OnTimerEvent);
            //}

            //DailyCollectionProcessor();

        }

        private void OnTimerEvent(object sender, EventArgs e)
        {
           // timer1.Interval = ProcessInterval;

            // Mobile();
            try
            {
                timer1.Stop();
                timer1.Enabled=false;

                WriteLogFile.WriteLog("------------- Start Daily Collection ------------");

                DailyCollectionProcessor();

               

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


        string getCompanyInfo()

        {
            var dtbiturl = new DBUtility().GetDataByProc("sp_getCompanyInformation");
            var biturl = dtbiturl.Rows[0]["bitUrl"].ToString();

            return biturl;
        }

       
        public void DailyCollectionProcessor()
        {
            try
            {


                DataTable dtCust = new DBUtility().GetDataByProc("sp_CustomerListForCollectionProcessor2");
                //DataTable dtCust = new DBUtility().GetDataByProc("Temporary_Customers_Pending");
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
                        var msg = " Collection Found CID: " + CustomerID;
                        // listBox1.Items.Add(DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + msg);
                        WriteLogFile.WriteLog(msg);
                        Hashtable ht = new Hashtable();
                        ht.Add("CustomerID", CustomerID);

                        DataTable dtCustomerInfo = new DBUtility().GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();
                        foreach (DataRow dr in dtCustomerInfo.Rows)
                        {

                            //if (!String.IsNullOrEmpty(dr["EndDate"].ToString()) && !String.IsNullOrEmpty(Custdr["RefDate"].ToString()))
                            //{

                                //decimal debit = Conversion.TryCastDecimal(dr["Debit"].ToString());
                                //decimal credit = Conversion.TryCastDecimal(dr["Credit"].ToString());
                                //decimal CL = Conversion.TryCastDecimal(dr["CreditLimit"].ToString());
                                //decimal PV = Conversion.TryCastDecimal(dr["TotalMRC"].ToString());
                                //decimal DSC = Conversion.TryCastDecimal(dr["Discount"].ToString());

                                //string Refno = Custdr["RefNo"].ToString();
                                string SNID = Custdr["SNID"].ToString();


                                //DateTime CLD = Conversion.TryCastDate(Custdr["RefDate"].ToString());
                                //DateTime ED = Conversion.TryCastDate(dr["EndDate"].ToString());


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
                             //   decimal Balance = credit - debit;

                                // MK ON 
                                try
                                {
                                    MkConnStatus objMkConnStatusEnable = objMKConnection.EnableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser);

                                    if (objMkConnStatusEnable.StatusCode == "200")
                                    {
                                        ht.Add("CustomerID", CustomerID);
                                        ht.Add("POPId", RouterID);
                                        ht.Add("CustomerIP", IPAddress);
                                        ht.Add("Status", 1);
                                        ht.Add("ProcessID", 201);
                                        ht.Add("EntryID", PinNumber);

                                        var dResponse = new DBUtility().GetDataByProc(ht, "sp_insertMKlogNCustStatus");
                                        ht.Clear();
                                        var custStatus = dResponse.Rows[0]["Feedback"].ToString();
                                        if (dResponse != null && custStatus == "Customer status changed successfully!")
                                        {
                                            //SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";
                                            //SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "true #";


                                            // UPDATE BILLING MASTER IsProcessed=1
                                            DataTable ProcessUpdate = new DBUtility().GetDataBySQLString("UPDATE Temp_Customer_Activation SET StatusId = 1 WHERE SNID = '" + SNID + "' SELECT 'SUCCESS' AS SUCCESS");
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
                                       // InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusEnable.RetMessage.ToString(), objMkConnStatusEnable.StatusCode.ToString());

                                        WriteLogFile.WriteLog(CustomerID + "==>" + MKLogError);
                                    }
                                }
                                catch (Exception rx)
                                {

                                    WriteLogFile.WriteLog(CustomerID + "==>" + rx.Message);
                                }
                            //}
                            //else
                            //{
                            //    // DO something
                            //}
                        }
                    }
                    catch (Exception ex)
                    {
                        ProcessErrorlog += "ID:" + CustomerID + ", Er. " + ex.ToString() + " #";
                        WriteLogFile.WriteLog(ProcessErrorlog);
                    }
                }

                //Hashtable ht1 = new Hashtable();
                //ht1.Add("SuccessLogBeforeProcess", SuccessLogBeforeProcess);
                //ht1.Add("SuccessLogAfterProcess", SuccessLogAfterProcess);
                //ht1.Add("ProcessErrorlog", ProcessErrorlog);
                //ht1.Add("MKLogError", MKLogError);
                //ht1.Add("ProcessStartTime", ProcessStartTime);
                //ht1.Add("ProcessEndTime", DateTime.Now);
                //ht1.Add("NoOfCustomer", NoOfCustomer);

                //ht1.Add("ID", 2);

                //DataTable PROCESSLOG = new DBUtility().GetDataByProc(ht1, "sp_InsertProcessLog");
                //ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

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
