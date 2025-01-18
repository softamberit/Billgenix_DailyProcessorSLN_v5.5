using BBS.BusinessEntity;
using BBS.BusinessLogic;
using BBS.DataAccess;
using BBS.Utilitys;
using BillingERPConn;
using Newtonsoft.Json;

using RestSharp;
using System;
using System.Collections;
using System.Data;
using System.Windows.Forms;

namespace BkashDailyProcessor
{
    public partial class frmMain : Form
    {


        public frmMain()
        {
            InitializeComponent();
        }

        DBUtility idb = new DBUtility();
        private void frmMain_Load(object sender, EventArgs e)
        {
            // schedule_Timer_CollBkashSync();


            ////Timer Tick Call
            schedule_Timer_CollBkashSync_New();


        }

        int ProcessInterval = 0;
        private void schedule_Timer_CollBkashSync_New()
        {

            Hashtable ht = new Hashtable();
            ht.Add("ID", 3);
            DataTable dt = idb.GetDataByProc(ht, "sp_getProcessConfigInfo");
            int StartHour = int.Parse(dt.Rows[0]["StartHour"].ToString());
            int EndHour = int.Parse(dt.Rows[0]["EndHour"].ToString());
            int minutes = int.Parse(dt.Rows[0]["StartMin"].ToString());
            ProcessInterval = int.Parse(dt.Rows[0]["Interval"].ToString()); // Should be Milisecond

            txtStartHour.Text = EndHour.ToString();
            txtEndHour.Text = StartHour.ToString();
            lblText.Text = "This Process is run with " + (ProcessInterval / 60000).ToString() + " Minutes Interval.";



            timer1.Interval = ProcessInterval;
            timer1.Enabled = true;
            timer1.Tick += new System.EventHandler(OnTimerEvent);
            Bkash_Sync();

        }

        private void OnTimerEvent(object sender, EventArgs e)
        {
            //WriteLogFile.WriteLog("Starting ");
            timer1.Stop();
            try
            {
                Bkash_Sync();
            }
            catch (Exception)
            {

                
            }finally
            {
                timer1.Start();

            }

            //lblSync.Text = "Last Sync: " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt"); 
        }


        #region  Bkash Sync Process




        ///     Bkash Sync Process
        ///     
        private void Bkash_Sync()
        {
            WriteLogFile.WriteLog("bKash uncollecte data Sync");

            var dbUtil = new DBUtility();
            DataTable dtab = dbUtil.GetDataByProc("Sp_Bkash_UncollectedData_DailyProcessor");

            string tranRefNo = "";
            string MessageId = "";
            string transactionId = "";
            string amount = "";
            string dateTime = "";
            foreach (DataRow dr in dtab.Rows)
            {

                try
                {
                    transactionId = dr["TrxID"].ToString();
                    MessageId = dr["MessageId"].ToString();
                    amount = dr["amount"].ToString();
                    dateTime = dr["dateTime"].ToString();
                    WriteLogFile.WriteLog("Get Ref No for Transction number is " + transactionId);

                    tranRefNo = BkashReferenceNo(transactionId);
                    // MessageBox.Show(transactionId);
                    if (!string.IsNullOrEmpty(tranRefNo))
                    {
                        WriteLogFile.WriteLog("Update Transction number is " + transactionId);

                        // MessageBox.Show(tranRefNo);
                        BLSWIFT.Bkash_UpdateReferenceNoByTrnxID(transactionId, MessageId, tranRefNo);
                        Bkash_UpdateReferenceNoByTrnxID(transactionId, MessageId, tranRefNo, amount, dateTime);
                    }
                }
                catch (Exception ex)
                {

                    WriteLogFile.WriteLog("UpdateCustomerNextBillingDate_IsOldClient=false_InvoiceCount>0" + tranRefNo + "::" + ex.ToString());


                    
                }

            }

        }

        private long Bkash_UpdateReferenceNoByTrnxID(string TransactionNo, string messageId, string ReferenceNo, string amount, string transaction_time)
        {
            var res = 0;

            // ReferenceNo=string.

            try
            {

                if (Conversion.TryCastDecimal(amount) > 0)
                {

                    var result = BLSWIFT.BkashWebHookCollectionInsert(amount, TransactionNo, ReferenceNo, transaction_time);

                    if (result.Rows.Count > 0)
                    {

                        var statusCode = result.Rows[0][1].ToString();
                        var CustomerId = result.Rows[0][2].ToString();


                        if (statusCode == "200")
                        {


                            // Alert.Show(msg);

                            res = 1;
                        }

                        //do something
                        //res = 1;
                    }

                }


            }
            catch (Exception ex)
            {

                res = 0;
            }
            return res;

        }
        public string BkashReferenceNo(string TransactionNo = "")
        {



            var url = "";
            string app_key = "";
            string app_secret = "";
            string username = "";
            string password = "";
            string token_type = "";
            string id_token = "";
            string expires_in = "";
            string refresh_token = "";
            string CallStatus = "";
            string RefreshUrl = "";
            string SearchTxnAPI = "";


            try
            {



                BkashToken resToken = new BkashToken();




                //Checking For Token

                DataTable DtabTkn = BLSWIFT.ChecqToken();

                if (DtabTkn.Rows.Count > 0)
                {

                    app_key = DtabTkn.Rows[0]["app_key"].ToString();
                    app_secret = DtabTkn.Rows[0]["app_secret"].ToString();
                    username = DtabTkn.Rows[0]["username"].ToString();
                    password = DtabTkn.Rows[0]["password"].ToString();
                    token_type = DtabTkn.Rows[0]["token_type"].ToString();
                    id_token = DtabTkn.Rows[0]["id_token"].ToString();
                    expires_in = DtabTkn.Rows[0]["expires_in"].ToString();
                    refresh_token = DtabTkn.Rows[0]["refresh_token"].ToString();
                    CallStatus = DtabTkn.Rows[0]["CallStatus"].ToString();
                    url = DtabTkn.Rows[0]["Url"].ToString();
                    RefreshUrl = DtabTkn.Rows[0]["RefreshTokenUrl"].ToString();
                    SearchTxnAPI = DtabTkn.Rows[0]["SearchTxnAPI"].ToString();
                }





                if (CallStatus == "1stCall")
                {
                    var resTokenKey = new BkashTokenKey()
                    {
                        app_key = app_key,
                        app_secret = app_secret

                    };

                    var client = new RestClient(url);
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("Content-type", "application/json");
                    request.AddHeader("username", username);
                    request.AddHeader("password", password);
                    request.AddJsonBody(resTokenKey);

                    IRestResponse response = client.Execute<BkashToken>(request);
                    resToken = JsonConvert.DeserializeObject<BkashToken>(response.Content);
                    BLSWIFT.InsertBkashToken(resToken, TransactionNo, username, password, resTokenKey, url, RefreshUrl);

                    id_token = resToken.id_token;

                }

                else if (CallStatus == "RefreshCall")
                {
                    var resTokenKey = new BkashRefreshTokenKey()
                    {
                        app_key = app_key,
                        app_secret = app_secret,
                        refresh_token = refresh_token
                    };


                    var client = new RestClient(RefreshUrl);
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("Content-type", "application/json");
                    request.AddHeader("username", username);
                    request.AddHeader("password", password);
                    request.AddJsonBody(resTokenKey);

                    IRestResponse response = client.Execute<BkashToken>(request);
                    resToken = JsonConvert.DeserializeObject<BkashToken>(response.Content);
                    BLSWIFT.InsertBkashRefreshToken(resToken, TransactionNo, username, password, resTokenKey, url, RefreshUrl);

                    id_token = resToken.id_token;

                }




                //==================Call For Reference No=================


                var StxnAPI = new RestClient(SearchTxnAPI + TransactionNo);
                var StxnAPIrequest = new RestRequest(Method.GET);

                StxnAPIrequest.AddHeader("authorization", id_token);
                StxnAPIrequest.AddHeader("x-app-key", app_key);


                IRestResponse StxnAPIresponse = StxnAPI.Execute<BkashSearchTranDetails>(StxnAPIrequest);
                BLSWIFT.InsertBkashSearchTransactionHistory(TransactionNo, StxnAPIresponse.Content);
                BkashSearchTranDetails resStxnAPI = JsonConvert.DeserializeObject<BkashSearchTranDetails>(StxnAPIresponse.Content);

                return resStxnAPI.transactionReference;
            }
            catch (Exception ex)
            {
                BLSWIFT.InsertBkash_SearchTxnAPI_ERRORLog(id_token, TransactionNo, ex.Message.ToString());

                throw ex;
            }
            return "";
        }









        #endregion










    }
}
