using BBS.Utilitys;
using BillingERPConn;
using MkCommunication;
using System;
using System.Collections;
using System.Data;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace MikrotikBulkProcessor
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
        DBUtility idb = new DBUtility();
        int PinNumber = 10000;
        ServiceManager svrManager = new ServiceManager();
        Timer timer2;

        public frmMain()
        {
            InitializeComponent();
            //  numHours.Value = DateTime.Now.Hour;
            //  numMins.Value = DateTime.Now.Minute;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //schedule_Timer_Callback();
            //timer2 = new Timer();
            //timer2.Enabled = true;
            //timer2.Interval = 1000;
            //timer2.Tick += Timer2_Tick;
            //svrManager.GetActiveCustomerFromMikrotik();
            //svrManager.MikrotikBulkProcessor();
            //svrManager.GetMkStatusProcess();
            //svrManager.RouterAliveStatusProcessor();
            string strText = "********** Starting *****";
            WriteLogFile.WriteLog(strText);
            //svrManager.MkStatusCheckingAllCustomerProcess();

            svrManager.MkDisableForInactiveCustomerProcess();
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                timer2.Stop();

                var time = new TimeSpan(7, 0, 0);
                var currTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);

                if (currTime == time)
                {
                    WriteLogFile.WriteLog("Process starting...");

                    //svrManager.MkStatusCheckingAllCustomerProcess();
                    svrManager.MkDisableForInactiveCustomerProcess();
                }
            }
            catch (Exception ex)
            {

                WriteLogFile.WriteLog(ex.Message);

            }
            finally
            {
                // Thread.Sleep(60000);
                timer2.Start();
            }
        }

        int ProcessInterval = 0;
        private void schedule_Timer_Callback()
        {
            try
            {
                Hashtable ht = new Hashtable();
                ht.Add("ID", 7);
                DataTable dt = idb.GetDataByProc(ht, "sp_getProcessConfigInfo");
                int StartHour = Conversion.TryCastInteger(dt.Rows[0]["StartHour"].ToString());
                int EndHour = Conversion.TryCastInteger(dt.Rows[0]["EndHour"].ToString());
                int minutes = Conversion.TryCastInteger(dt.Rows[0]["StartMin"].ToString());
                ProcessInterval = Conversion.TryCastInteger(dt.Rows[0]["Interval"].ToString()); // Should be Millisecond

                txtStartHour.Text = StartHour.ToString();
                txtEndHour.Text = EndHour.ToString();
                // lblText.Text = "This Process is run with " + (ProcessInterval / 60000).ToString() + " Minutes Interval.";

                timer1.Interval = 1000 * 60 * 60 * 24;  // millisecond

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

                //svrManager.GetActiveCustomerFromMikrotik();
                //svrManager.MikrotikBulkProcessor();
            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ex.Message);
            }

        }

        private void OnTimerEvent(object sender, EventArgs e)
        {
            timer1.Interval = ProcessInterval;
            string strText = "**********All Active Customer Mikrotik Status Processor Scheduler has been started!*****" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt");
            WriteLogFile.WriteLog(strText);

            listBox1.Items.Add(strText);
            //All Customer Mikrotik Status


            //svrManager.MikrotikBulkProcessor();
            // svrManager.GetMkStatusProcess();
            //svrManager.MikrotikBulkProcessor();

        }



        /// <summary>
        /// Exits the app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitBtn_Click(object sender, EventArgs e)
        {

            this.Close();
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }



        private void inActiveToActive_Click(object sender, EventArgs e)
        {
            svrManager.MkDisableForInactiveCustomerProcess();

        }

        private void btnMkStatus_Click(object sender, EventArgs e)
        {
            svrManager.GetActiveCustomerFromMikrotik();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            svrManager.MkStatusCheckingAllCustomerProcess();
        }
    }

}
