using BillingERPConn;
using ReveProcessor;
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;
using System.Xml.Linq;

namespace MailProcessor
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
        private readonly DBUtility _idb = new DBUtility();

        public FrmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //DailyEmailProcessor();
            schedule_Timer_Callback();
            OnTimerEvent(this, new EventArgs());
        }

        int _processInterval;

        private void schedule_Timer_Callback()
        {


            timer1.Interval = 1000 * 60 * 5;  // millisecond
            timer1.Enabled = true;
            timer1.Tick += OnTimerEvent;

        }

        private void OnTimerEvent(object sender, EventArgs e)
        {
           
            DailyEmailProcessor();
        }


        #region EmailProcessor

        private void DailyEmailProcessor()
        {

            Hashtable ht = new Hashtable();

            DataTable dt = _idb.GetDataByProc(ht, "sp_Get_Followup_Mail_Pending");
            ht.Clear();
            
            foreach (DataRow dr in dt.Rows)
            {
                try
                {
                    string customerId = "";

                    var schedulerId = dr["ID"].ToString();
                    customerId = dr["CustomerID"].ToString();
                    var mailAddress = dr["Email"].ToString();
                    var emailText = dr["EmailText"].ToString();
                    var emailCc = dr["MailCC"].ToString();
                    var subject = dr["MailSubject"].ToString();

                    var addressFrom = dr["AddressFrom"].ToString();

                    SendEmail(schedulerId, customerId, mailAddress, emailText, subject,addressFrom);

                }
                catch (Exception ex)
                {
                  //  WriteLogFile.WriteLog(customerId + ":: " + ex.Message);
                    continue;
                }
            }
        }

      

        public bool SendEmail(string schedulerId, string customerId, string email, string mailBody,string subject, string addressFrom)
        {
            bool result;

            try
            {
              
                string toAddress = email;
              
                
                InsertMailHistory(schedulerId, email, customerId, mailBody, 0);

                result = MailReport(addressFrom, "", toAddress, "", subject, mailBody);

                //  Insert History  ==1

                if (result == true)
                {
                    InsertMailHistory(schedulerId, email, customerId, mailBody, 1);

                    WriteLogFile.WriteLog(string.Format("Customer ID : {0} and Email send to {1}", customerId, toAddress));

                }



            }

            catch (Exception ex)
            {
                WriteLogFile.WriteLog(customerId + ":: Email Catch " + ex.Message);
                result = false;
            }

            return result;
        }
         

        public bool MailReport(string from, string fromPassword, string to, string cc, string subject, string mailBody)
        {

            bool result;

            try
            {

              

                MailMessage msg = new MailMessage();

                
                msg.To.Add(to);
                // msg.CC.Add(cc);
                msg.From = new MailAddress(from);

                msg.Subject = subject;
                msg.Body = mailBody;
                msg.IsBodyHtml = true;

                //   msg.Attachments.Add(attachment);

                string host = "202.4.96.7";
                int port = 25;

                var smtp_h = ConfigurationManager.AppSettings["smtp_host"];
                if (!string.IsNullOrEmpty(smtp_h))
                {
                    host = smtp_h;
                }
                int.TryParse(ConfigurationManager.AppSettings["Port"], out port);


                var smtp = new SmtpClient();
                {
                    smtp.Host = host;
                    smtp.Port = port;
                    smtp.EnableSsl = false;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Credentials = new NetworkCredential(from, fromPassword);
                    smtp.Timeout = 20000;

                }
                smtp.Send(msg);

                result = true;
            }

            catch (Exception ix1)
            {
                WriteLogFile.WriteLog("TO " + to + " From " + from + " Sub: " + subject + " error: " + ix1.Message);
                result = false;

            }

            return result;
        }

        private static void InsertMailHistory(string schedulerId, string emailNo, string customerId, string emailBody, int statudId)
        {
            try
            {
                DBUtility idb = new DBUtility();

                Hashtable ht = new Hashtable
                {
                    {"CustomerID", customerId},
                    {"SchedulerID", schedulerId},
                    {"EmailNo", emailNo},
                    {"EmialBody", emailBody},
                    {"StatusId", statudId},
                    {"CreatedBy", 10000}
                };

                idb.GetDataByProc(ht, "sp_InsertEmailHistory");
            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ex.Message);
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


        #endregion



        private Scheduler GetScheduler()
        {

            return Scheduler.EveryMinutes;
        }

        private void exitBtn_Click(object sender, EventArgs e)
        {

            Close();
        }



    }

    // public class EmailHttpRequest
    // {

    // public string campaignName { get; set; }
    // public string routeId { get; set; }
    // public List<Message> Messages { get; set; }
    // public string scheduledDeliveryDateTime { get; set; }
    //  public string scheduledDeliveryDateTimeOffset { get; set; }
    // public int EmailTypeId { get; set; }
    // public int validityPeriodInHour { get; set; }

    //  public int responseType { get; set; }
    // }

    public class Message
    {
        // public string from { get; set; }
        public string To { get; set; }
        public string Text { get; set; }
        //public string categoryName { get; set; }
    }

    //public class EmailHttpResponse
    //{
    //    public string bulkProcessId { get; set; }
    //    public int bulkProcessStatus { get; set; }
    //    public string messages { get; set; }
    //}
}

