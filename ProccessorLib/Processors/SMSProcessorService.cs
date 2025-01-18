using BillingERPConn;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProccessorLib.Processors
{
   public class SMSProcessorService
    {
        DBUtility _idb;
        string _mobileNo;
        int PinNumber = 10000;


        public SMSProcessorService()
        {
            _idb = new DBUtility();

        }


        private void dailySMSProcessor()
        {

            if (IsSMSSendOptionEnabled())
            {
                Hashtable ht = new Hashtable();

                DataTable dt = new DBUtility().GetDataByProc(ht, "sp_getsmsinfo");
                ht.Clear();
                string SchedulerID = "", CustomerID = "", Mobile = "", SMSText = "", SMS_UserName = "", SMS_Password = "", SMS_url = "";
                foreach (DataRow dr in dt.Rows)
                {
                    try
                    {
                        SchedulerID = dr["ID"].ToString();
                        CustomerID = dr["CustomerID"].ToString();
                        Mobile = dr["Mobile"].ToString();
                        SMSText = dr["SMSText"].ToString();
                        SMS_UserName = dr["SMS_UserName"].ToString();
                        SMS_Password = dr["SMS_Password"].ToString();
                        SMS_url = dr["SMS_url"].ToString();

                        SentSMS(SchedulerID, CustomerID, Mobile, SMSText, SMS_UserName, SMS_Password, SMS_url);
                    }
                    catch (Exception ex)
                    {
                        WriteLogFile.WriteLog(CustomerID + ":: " + ex.Message.ToString());
                        continue;
                    }
                }
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

        public static IRestResponse SentSMS(string SchedulerID, string CustomerID, string Mobile, string SMSText, string SMS_UserName, string SMS_Password, string SMS_url)
        {
            //string text = "";

            try
            {
                IRestResponse response = null;

                InsertMessageHistory(SchedulerID, Mobile, CustomerID, SMSText, 0, "", "");


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


                var encoded = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));

                var client = new RestClient(url);
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-type", "application/json");
                request.AddHeader("Authorization", "Basic " + encoded);
                request.AddJsonBody(smsHttpRequest);

                response = client.Execute<SmsHttpRequest>(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    InsertMessageHistory(SchedulerID, Mobile, CustomerID, SMSText, 1, response.StatusCode.ToString(), response.Content.ToString());
                }
                else
                {
                    InsertMessageHistory(SchedulerID, Mobile, CustomerID, SMSText, 0, response.StatusCode.ToString(), response.Content.ToString());
                }




                return response;
            }
            catch (Exception ex)
            {
                InsertMessageHistory(SchedulerID, Mobile, CustomerID, SMSText, 0, "", ex.Message.ToString());
                return null;
            }
        }

        private static void InsertMessageHistory(string SchedulerID, string Mobile, string CustomerID, string SMSText, int statudId, string stausCode, string responsMessage)
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
                DataTable dt = idb.GetDataByProc(ht, "spInsertMessageHistory");
            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ex.Message.ToString());
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

    }
}
