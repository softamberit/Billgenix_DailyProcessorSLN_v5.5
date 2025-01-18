using BillingERPConn;
using Newtonsoft.Json;
using RestSharp;
using SWIFTDailyProcessor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web.UI;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;


namespace SMSProcessor
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
        // DBUtility idb = new DBUtility();
        System.Timers.Timer vsTimer;
        System.Timers.Timer vsTimer2;

        System.Timers.Timer tsTimer;
        private string SMS_UserName = "";
        string SMS_Password = "";
        string SMS_url = "";
        string ait_sms_url = "";
        string ait_voice_v2_url = "";
        string ait_voice_v2_token = "";

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            SmsConfig();
            // SendOTP_ViaSMS();
            schedule_Timer_Callback();
            // OnTimerEvent(this, new EventArgs());

            //dailyVoiceNotificationForDue();
            // dailySMSProcessor();
            //SendOTP();
        }

        int PinNumber = 10000;

        int ProcessInterval = 0;
        private void schedule_Timer_Callback()
        {

            tsTimer = new System.Timers.Timer();
            tsTimer.Interval = 1000;  // millisecond
            tsTimer.Enabled = true;
            tsTimer.Elapsed += TsTimer_Elapsed;
            // tsTimer. += OnTimerEvent;
            tsTimer.Start();


            vsTimer = new System.Timers.Timer();
            vsTimer.Interval = 500;
            vsTimer.Enabled = true;
            vsTimer.Elapsed += VsTimer_Tick;
            vsTimer.Start();




        }

        private void VsTimer_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                vsTimer.Stop();

                SendOTP_ViaVoice();


            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLogVoice("VoiceTik=>" + ex.Message);

            }
            finally
            {
                vsTimer.Start();
            }

        }

        // 

        private void TsTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                tsTimer.Stop();

                SendOTP_ViaSMS();

            }
            catch (Exception ex)
            {

                WriteLogFile.WriteLogSMS("SMS Tik" + ex.Message);

            }
            finally
            {
                tsTimer.Start();
            }
        }

        //private void VsTimer_Tick(object sender, EventArgs e)
        //{
        //    try
        //    {


        //        vsTimer.Stop();
        //        var today = DateTime.Now;
        //        if (today.TimeOfDay >= new TimeSpan(10, 00, 00))
        //        {
        //            WriteLogFile.WriteLogVoice("Start Voice SMS");
        //            dailyVoiceNotificationForDue();
        //        }
        //    }
        //    catch (Exception)
        //    {


        //    }
        //    finally
        //    {
        //        vsTimer.Start();

        //    }

        //}

        //private void OnTimerEvent(object sender, EventArgs e)
        //{
        //    //timer1.Interval = ProcessInterval;
        //    // string strText = "**********SMS Scheduler has been started!*****" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt");
        //    // WriteLogFile.WriteLog(strText);

        //    // listBox1.Items.Add(strText);
        //    try
        //    {
        //        tsTimer.Stop();
        //        Task.Run(() =>
        //        {
        //            WriteLogFile.WriteLogSMS("Start SMS");

        //            dailySMSProcessor();
        //        });

        //        var today = DateTime.Now;

        //        if (today.TimeOfDay >= new TimeSpan(10, 00, 00))
        //        {
        //            Task.Run(() =>
        //            {
        //                WriteLogFile.WriteLogVoice("Start Voice SMS");
        //                dailyVoiceNotificationForDue();
        //            });

        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //        WriteLogFile.WriteLogSMS(ex.Message);

        //    }
        //    finally
        //    {
        //        tsTimer.Start();
        //    }


        //}

        #region SMSProcessor 

        private void SmsConfig()
        {

            DBUtility db = new DBUtility();

            DataTable smsConfig = db.GetDataByProc("sp_getSMS_credential");
            if (smsConfig != null)
            {
                SMS_UserName = smsConfig.Rows[0]["SMS_UserName"].ToString();
                SMS_Password = smsConfig.Rows[0]["SMS_Password"].ToString();
                SMS_url = smsConfig.Rows[0]["SMS_url"].ToString();
                ait_sms_url = smsConfig.Rows[0]["AIT_SMS_URL"].ToString();
                ait_voice_v2_url = smsConfig.Rows[0]["AIT_Voice_URL"].ToString();
                ait_voice_v2_token = smsConfig.Rows[0]["AIT_Voice_Token"].ToString();


            }



        }

        private void SendOTP_ViaVoice()
        {
            DBUtility db = new DBUtility();
            Hashtable ht = new Hashtable();
            ht.Add("UseSMSPortal", 2);
            DataTable dt = db.GetDataByProc(ht, "sp_GetOtpSMS");
            OtpSMSProcessVoice(dt);
        }
        private void SendOTP_ViaSMS()
        {
            DBUtility db = new DBUtility();
            Hashtable ht = new Hashtable();
            ht.Add("UseSMSPortal", 1);
            DataTable dt = db.GetDataByProc(ht, "sp_GetOtpSMS");
            if (dt != null)
            {
                SMSProcess(dt);

            }
        }


        private void SMSProcess(DataTable dt)
        {


            string SchedulerID = "", CustomerID = "", Mobile = "", SMSText = "";
            int UseSMSPortal = 0;


            foreach (DataRow dr in dt.Rows)
            {
                try
                {
                    DateTime sDateTime = DateTime.Now;
                    SchedulerID = dr["ID"].ToString();
                    CustomerID = dr["CustomerID"].ToString();
                    Mobile = dr["Mobile"].ToString().Trim();
                    SMSText = dr["SMSText"].ToString().Trim();
                    UseSMSPortal = Convert.ToInt32(dr["UseSMSPortal"].ToString());



                    //SMS_UserName = dr["SMS_UserName"].ToString();
                    //SMS_Password = dr["SMS_Password"].ToString();
                    //SMS_url = dr["SMS_url"].ToString();


                    if (IsValidPhoneNumber(Mobile))
                    {
                        if (Mobile.Length == 11)
                        {
                            Mobile = "88" + Mobile;
                        }
                        if (Mobile.Length == 13)
                        {
                            var response = SentSMS(SchedulerID, CustomerID, Mobile, SMSText, SMS_UserName, SMS_Password, SMS_url, ait_sms_url, UseSMSPortal);

                            DateTime endDateTime = DateTime.Now;
                            double duration = (endDateTime - sDateTime).TotalSeconds;

                            var logMsg = string.Format("SMS=> ID:{4} {0}({2})==>Send==> {1}: Duration: {3} Seconds", CustomerID, response.StatusCode.ToString(), Mobile, duration, SchedulerID);

                            WriteLogFile.WriteLogSMS(logMsg);

                            // listBox1.Items.Add(logMsg);


                        }
                        else
                        {
                            //var logMsg = string.Format("{0}({2})==>Send==> {1}: Duration: {3} Seconds", CustomerID, response.StatusCode.ToString(), Mobile, duration);

                            UpdateSMSStatus(SchedulerID, 2);

                        }

                    }
                    else
                    {
                        UpdateSMSStatus(SchedulerID, 2); //Invalid Mobile No
                    }

                }
                catch (Exception ex)
                {
                    WriteLogFile.WriteLogSMS(CustomerID + ":: " + ex.Message.ToString());
                    continue;
                }
            }
            //}
        }

        private void OtpSMSProcessVoice(DataTable dt)
        {


            string SchedulerID = "", Mobile = "", otp = "";
            int billAmt = 0;
            DateTime dueDate;
            foreach (DataRow dr in dt.Rows)
            {
                try
                {
                    DateTime sDateTime = DateTime.Now;
                    SchedulerID = dr["ID"].ToString();
                    Mobile = GetValidMobileNo(dr["Mobile"].ToString().Trim());
                    otp = GetValidMobileNo(dr["VerificationCode"].ToString().Trim());



                    //SMSText = dr["SMSText"].ToString().Trim();
                    //SMS_UserName = dr["SMS_UserName"].ToString();
                    //SMS_Password = dr["SMS_Password"].ToString();
                    //SMS_url = dr["SMS_url"].ToString();

                    if (IsValidPhoneNumber(Mobile))
                    {

                        if (Mobile.Length == 11)
                        {

                            var response = SendVoiceSMS_V2(ait_voice_v2_url, ait_voice_v2_token, Mobile, otp);
                            if (response != null && response.request_status == "1")
                            {


                                DateTime endDateTime = DateTime.Now;
                                double duration = (endDateTime - sDateTime).TotalSeconds;

                                var logMsg = string.Format("Voice=> {0}({2})==>Send==> {1}: Duration: {3} Seconds", SchedulerID, response.message.ToString(), Mobile, duration);

                                WriteLogFile.WriteLogVoice2(logMsg);
                                UpdateVoiceStatus(SchedulerID, 1);
                            }



                        }
                        else
                        {
                            //var logMsg = string.Format("{0}({2})==>Send==> {1}: Duration: {3} Seconds", CustomerID, response.StatusCode.ToString(), Mobile, duration);

                            UpdateVoiceStatus(SchedulerID, 2);    ////Invalid Mobile No

                        }

                    }
                    else
                    {
                        UpdateVoiceStatus(SchedulerID, 2);

                    }

                }
                catch (Exception ex)
                {
                    WriteLogFile.WriteLogVoice2(Mobile + ":: " + ex.Message.ToString());
                    UpdateVoiceStatus(SchedulerID, 2);


                }
                System.Threading.Thread.Sleep(100);
            }
            //}
        }

        private string GetValidMobileNo(string mobileNo)
        {
            string mobile = mobileNo.Replace(" ", "").Replace("-", "");

            if (mobileNo.Length >= 13)
            {
                mobile = mobileNo.Substring(0, 13);
                mobile = mobile.Remove(0, 2);

            }
            return mobile;
        }

        private VoiceHttpResponse SendVoiceSMS_V1(string CustomerID, string mobile, int billAmt, DateTime dueDate)
        {
            try
            {

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                var url = "https://ca096.amberit.com.bd/api01/";
                var bearerTocken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJBbWJlcklUIiwiaWF0IjoxNzE1NDI1Nzc4LCJFeHBpcmVzIE9uIjoyMDMwOTU4NTc4LCJlbWFpbCI6Im1haGZ1enVyQGFtYmVyaXQuY29tLmJkIn0.Wlq9uKuUggmM7JjjBR-zYUNk8ra81VuAbiAAqFhe5jQ";
                var voiceSMS = new VoiceHttpRequest() { amount = billAmt.ToString(), mobileno = mobile, dateunix = ToUnixTime(dueDate).ToString() };

                var client = new RestClient(url);
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-type", "application/json");
                request.AddHeader("Authorization", "Bearer " + bearerTocken);
                request.AddJsonBody(voiceSMS);


                DateTime sDateTime = DateTime.Now;

                IRestResponse<VoiceHttpResponse> response = client.Post<VoiceHttpResponse>(request);
                DateTime endDateTime = DateTime.Now;
                double duration = (endDateTime - sDateTime).TotalSeconds;

                WriteLogFile.WriteLogVoice(string.Format("API Response Time: {0} seconds", duration));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    VoiceHttpResponse responseContent = JsonConvert.DeserializeObject<VoiceHttpResponse>(response.Content);

                    return responseContent;
                }
                else
                {
                    WriteLogFile.WriteLogVoice("API Not Found");
                }
                return null;
            }
            catch (Exception ex)
            {
                var logMsg = string.Format("{0}({2})==>Send==> {1}", CustomerID, "Failed", mobile);
                WriteLogFile.WriteLogVoice(logMsg);

                return null;
            }
        }
        private VoiceHttpResponse SendVoiceSMS_V2(string ait_voice_v2_url, string ait_voice_v2_token, string mobile, string otp)
        {
            try
            {

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                //var url = "https://ca096.amberit.com.bd/api01/";
                //var bearerTocken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJBbWJlcklUIiwiaWF0IjoxNzE1NDI1Nzc4LCJFeHBpcmVzIE9uIjoyMDMwOTU4NTc4LCJlbWFpbCI6Im1haGZ1enVyQGFtYmVyaXQuY29tLmJkIn0.Wlq9uKuUggmM7JjjBR-zYUNk8ra81VuAbiAAqFhe5jQ";
                var voiceSMS = new VoiceHttpRequestV2() { mobileno = mobile, otp = otp };

                var client = new RestClient(ait_voice_v2_url);
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-type", "application/json");
                request.AddHeader("Authorization", "Bearer " + ait_voice_v2_token);
                request.AddJsonBody(voiceSMS);


                DateTime sDateTime = DateTime.Now;

                IRestResponse<VoiceHttpResponse> response = client.Post<VoiceHttpResponse>(request);
                DateTime endDateTime = DateTime.Now;
                double duration = (endDateTime - sDateTime).TotalSeconds;

                WriteLogFile.WriteLogVoice(string.Format("API Response Time: {0} seconds", duration));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    VoiceHttpResponse responseContent = JsonConvert.DeserializeObject<VoiceHttpResponse>(response.Content);

                    return responseContent;
                }
                else
                {
                    WriteLogFile.WriteLogVoice("API Not Found");
                }
                return null;
            }
            catch (Exception ex)
            {
                var logMsg = string.Format("{0}({2})==>Send==> {1}", mobile, "Failed", mobile);
                WriteLogFile.WriteLogVoice(logMsg);

                return null;
            }
        }
        private string ToUnixTime(DateTime dateTime)
        {
            DateTimeOffset dto = new DateTimeOffset(dateTime.ToLocalTime());
            return dto.ToUnixTimeSeconds().ToString();
        }


        public bool ContainsUnicodeCharacter(string input)
        {
            const int MaxAnsiCode = 255;

            return input.Any(c => c > MaxAnsiCode);
        }
        private void UpdateSMSStatus(string schedulerID, int status)
        {
            Hashtable ht = new Hashtable();
            ht.Add("SchedulerID", schedulerID);
            ht.Add("status", status);


            DBUtility idb = new DBUtility();

            DataTable dt = idb.GetDataByProc(ht, "sp_UpdateSMS_Schedule");
        }

        private DataTable GetPendingVoiceMessage()
        {

            DBUtility idb = new DBUtility();

            DataTable dt = idb.GetDataByProc("Sp_GetPending_BillDuesVoiceNotification");

            return dt;
        }

        private void UpdateVoiceStatus(string schedulerID, int status)
        {
            Hashtable ht = new Hashtable();
            ht.Add("SMSID", schedulerID);
            ht.Add("status", status);


            DBUtility idb = new DBUtility();

            DataTable dt = idb.GetDataByProc(ht, "sp_Update_VoiceScheduleReminderDue");
        }

        private bool IsValidPhoneNumber(string mobile)
        {
            var rex = new Regex(@"^(?:(?:\+|00)88|88|01)?\d{11}\r?$");
            return rex.IsMatch(mobile);
        }

        public IRestResponse SentSMS(string SchedulerID, string CustomerID, string Mobile, string SMSText, string SMS_UserName, string SMS_Password, string SMS_url, string ait_url, int UseSMSPortal)
        {
            //string text = "";

            try
            {
                IRestResponse response = null;

                // InsertMessageHistory(SchedulerID, Mobile, CustomerID, SMSText, 0, "", "");

                //if(!ContainsUnicodeCharacter(SMSText))
                //{

                //}





                DateTime sDateTime = DateTime.Now;
                if (UseSMSPortal == 1)     //1=AIT SMS Portal, 0= SMS24  
                {
                    response = SendTextSmsViaAIT_v1(ait_url, Mobile, SMSText);
                }
                //else if (UseSMSPortal == 2)
                //{
                //    response = SendVoiceSMS_V2(ait_voice_v2_url, ait_voice_v2_token, Mobile, SMSText);

                //}
                else
                {
                    var listMsg = new List<Message>();
                    var msg = new Message()
                    {
                        from = "Amber IT",
                        to = Mobile,
                        // to = "8801787653277,8801787653283",
                        text = SMSText,
                        categoryName = "Promotion"
                    };
                    listMsg.Add(msg);
                    var smsHttpRequest = new SmsHttpRequest()
                    {
                        messages = listMsg,
                        campaignName = "Promotion",
                        routeId = "1",
                        scheduledDeliveryDateTime = DateTime.Now.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                        validityPeriodInHour = 0,
                        scheduledDeliveryDateTimeOffset = "+0600",
                        smsTypeId = 1,
                        responseType = 1

                    };

                    var url = SMS_url;
                    var username = SMS_UserName;
                    var password = SMS_Password;

                    response = SendSmsViaSMS24(smsHttpRequest, url, username, password);
                }


                DateTime endDateTime = DateTime.Now;
                double duration = (endDateTime - sDateTime).TotalSeconds;

                WriteLogFile.WriteLogSMS(string.Format("API Response Time: {0} seconds", duration));


                if (response.StatusCode == HttpStatusCode.OK)
                {
                    SmsHttpResponse responseMsg = null;
                    if (UseSMSPortal == 1)
                    {
                        AitSMSResponse result = SimpleJson.DeserializeObject<AitSMSResponse>(response.Content);
                        if (result != null)
                        {
                            responseMsg = new SmsHttpResponse();
                            responseMsg.bulkProcessId = result.Message_ID;
                            responseMsg.bulkProcessStatus = 1;

                        }
                    }
                    else
                    {
                        responseMsg = ParseSMSResponse(response.Content);
                    }

                    InsertMessageHistory(SchedulerID, Mobile, CustomerID, SMSText, responseMsg.bulkProcessStatus, response.StatusCode.ToString(), response.Content.ToString(), responseMsg);
                }
                else
                {
                    InsertMessageHistory(SchedulerID, Mobile, CustomerID, SMSText, 0, response.StatusCode.ToString(), response.Content.ToString());
                }



                return response;
            }
            catch (Exception ex)
            {
                var logMsg = string.Format("{0}({2})==>Send==> {1}", CustomerID, "Failed", Mobile);
                WriteLogFile.WriteLogSMS(logMsg);

                InsertMessageHistory(SchedulerID, Mobile, CustomerID, SMSText, 0, "", ex.Message.ToString());
                return null;
            }
        }

        private IRestResponse SendTextSmsViaAIT_v1(string ait_url, string mobile, string sMSText)
        {
            IRestResponse response;
            var client = new RestClient(string.Format(ait_url, mobile, sMSText));
            //var encoded = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            var request = new RestRequest(Method.GET);
            //request.AddHeader("Content-type", "application/json");
            //request.AddHeader("Authorization", "Basic " + encoded);
            //request.AddJsonBody(smsHttpRequest);
            response = client.Execute<AitSMSResponse>(request);

            return response;
        }
        //private IRestResponse SendVoiceSmsViaAIT_v2(string ait_voice_v2_url, string ait_voice_v2_token, string mobile, string sMSText)
        //{
        //    IRestResponse response;
        //    var client = new RestClient(ait_voice_v2_url);
        //    //var encoded = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
        //    var request = new RestRequest(Method.GET);
        //    //request.AddHeader("Content-type", "application/json");
        //    request.AddHeader("Authorization", "bearer " + ait_voice_v2_token);
        //    var smsBody = new
        //    {
        //        mobileno = mobile,
        //        otp = sMSText
        //    };
        //    request.AddJsonBody(smsBody);
        //    response = client.Execute<AitSMSResponse>(request);

        //    return response;
        //}

        private static IRestResponse SendSmsViaSMS24(SmsHttpRequest smsHttpRequest, string url, string username, string password)
        {
            IRestResponse response;
            var client = new RestClient(url);
            var encoded = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-type", "application/json");
            request.AddHeader("Authorization", "Basic " + encoded);
            request.AddJsonBody(smsHttpRequest);
            response = client.Execute<SmsHttpRequest>(request);
            return response;
        }

        private SmsHttpResponse ParseSMSResponse(string content)
        {
            return SimpleJson.DeserializeObject<SmsHttpResponse>(content);
        }

        private static void InsertMessageHistory(string SchedulerID, string Mobile, string CustomerID, string SMSText, int statudId, string stausCode, string responsMessage, SmsHttpResponse smsHttpResponse = null)
        {

            try
            {
                DBUtility idb = new DBUtility();

                Hashtable ht = new Hashtable();
                ht.Add("SchedulerID", SchedulerID);
                ht.Add("Mobile", Mobile);
                ht.Add("CustomerID", CustomerID);
                ht.Add("SMSText", SMSText);
                ht.Add("Status", statudId);
                ht.Add("StatusCode", stausCode);
                ht.Add("responseMessage", responsMessage);
                if (smsHttpResponse != null)
                {
                    ht.Add("bulkProcessId", smsHttpResponse.bulkProcessId);
                    ht.Add("bulkProcessStatus", smsHttpResponse.bulkProcessStatus);
                }
                else
                {
                    ht.Add("bulkProcessId", "");
                    ht.Add("bulkProcessStatus", "0");
                }


                DataTable dt = idb.GetDataByProc(ht, "spInsertMessageHistory");
            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLogSMS(ex.Message.ToString());
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
        public class VoiceHttpRequest
        {
            //{

            //"mobileno": "8801764010666",

            //"dateunix": "1713615296",

            //"amount": "1000"

            //}

            public string mobileno { get; set; }
            public string dateunix { get; set; }
            public string amount { get; set; }


        }
        public class VoiceHttpRequestV2
        {
            public string mobileno { get; set; }
            public string otp { get; set; }

        }

        public class VoiceHttpResponse
        {
            public string request_status { get; set; }
            public string message { get; set; }

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
        public class AitSMSResponse
        {
            public string Status { get; set; }
            public string Text { get; set; }
            public string Message_ID { get; set; }

        }


        /// <summary>
        /// based on the selected radion box returns the scheduler enum
        /// </summary>
        /// <returns></returns>

        private Scheduler getScheduler()
        {

            return Scheduler.EveryMinutes;
        }

        /// <summary>
        /// canceling the sheduler
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

