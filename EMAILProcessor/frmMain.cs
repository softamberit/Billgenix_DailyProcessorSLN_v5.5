using BillingERPConn;
using ReveProcessor;
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;

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

            Hashtable ht = new Hashtable { { "ID", 4 } };
            DataTable dt = _idb.GetDataByProc(ht, "sp_getProcessConfigInfo");
            int startHour = int.Parse(dt.Rows[0]["StartHour"].ToString());
            int endHour = int.Parse(dt.Rows[0]["EndHour"].ToString());
            // int minutes = int.Parse(dt.Rows[0]["StartMin"].ToString());
            _processInterval = int.Parse(dt.Rows[0]["Interval"].ToString()); // Should be Millisecond

            txtStartHour.Text = endHour.ToString();
            txtEndHour.Text = startHour.ToString();
            lblText.Text = @"This Process is run with " + (_processInterval / 60000) + @" Minutes Interval.";

            timer1.Interval = _processInterval;  // millisecond

            //System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer
            //{
            //    Interval = ProcessInterval  /// millisecond
            //};

            int currHour = DateTime.Now.Hour;

            if ((currHour < startHour && currHour < endHour) || (currHour > startHour && currHour > endHour))
            {
                timer1.Enabled = true;
                timer1.Tick += OnTimerEvent;
            }

        }

        private void OnTimerEvent(object sender, EventArgs e)
        {
            timer1.Interval = _processInterval;
            // string strText = "********** EMAIL Scheduler has been started!*****" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt");

            // WriteLogFile.WriteLog(strText);

            //listBox1.Items.Add(strText);

            DailyEmailProcessor();
        }


        #region EmailProcessor

        private void DailyEmailProcessor()
        {

            if (IsEmailSendOptionEnabled())
            {
                Hashtable ht = new Hashtable();

                DataTable dt = _idb.GetDataByProc(ht, "sp_getEmailinfo");
                ht.Clear();
                string customerId = "";
                foreach (DataRow dr in dt.Rows)
                {
                    try
                    {
                        var schedulerId = dr["ID"].ToString();
                        customerId = dr["CustomerID"].ToString();
                        var mobile = dr["Email"].ToString();
                        var emailText = dr["EmailText"].ToString();
                        var emailCc = dr["MailCC"].ToString();
                        var addressFrom = dr["AddressFrom"].ToString();

                        SendEmail(schedulerId, customerId, mobile, emailText, emailCc, addressFrom);

                    }
                    catch (Exception ex)
                    {
                        WriteLogFile.WriteLog(customerId + ":: " + ex.Message);
                        continue;
                    }
                }
            }
        }

        public string InvoiceNumberGeneration(string customerId, string year)
        {

            DataTable dt = _idb.GetDataBySQLString("SELECT ISNULL(MAX(RefnoSerial), 0) + 1 AS RefNo FROM BillingMaster WHERE CustomerID = '" + customerId + "' AND (RefTypeID = 1 OR RefTypeID = 2 OR RefTypeID = 3) ");

            string refno = dt.Rows[0]["RefNo"].ToString();
            string invNumber = year + customerId + refno;

            return invNumber;
        }

        public bool SendEmail(string schedulerId, string customerId, string billingEmail, string emailText, string mailCc, string addressFrom)
        {
            bool result;

            try
            {
                DataTable dt = _idb.GetDataBySQLString("SELECT EndDate FROM EMAIL_Schedule WHERE CustomerID = '" + customerId + "' AND ID ='" + schedulerId + "'");
                DateTime invoiceDate = DateTime.Parse(dt.Rows[0]["EndDate"].ToString());

                DataTable dt1 = _idb.GetDataBySQLString("SELECT NetMRC FROM CustomerMaster WHERE CustomerID = '" + customerId + "' ");
                DataTable dt2 = _idb.GetDataBySQLString("Select Format(EndDate ,'d-MMMM-yyyy') as DueDate from BillingCalender where CustomerID='" + customerId + "' ");

                string mrc = dt1.Rows[0]["NetMRC"].ToString();
                string dueDate = dt2.Rows[0]["DueDate"].ToString(); ;


                string email = billingEmail;
                string invYear = invoiceDate.Year.ToString();
                string monthName = invoiceDate.ToString("MMMM");
                string invNo = InvoiceNumberGeneration(customerId, invYear); //Gen

                string ccList = mailCc;

                // RichTextBox richTexbox = 

                string fromAddress = "swift@amberit.com.bd";
                string toAddress = email;
                string ccAddress = ccList;
                string fromPassword = "";
                string subject = "Monthly Bill on  " + monthName + ", " + invYear;
                string mailBody = getMailBody(mrc, dueDate, monthName + "- " + invYear, customerId);
                //string mailBody = "Dear Valued Customer,\n";

                // mailBody += "Thank you very much for choosing Swift Internet Service provided by Amber IT Limited." +
                // "Your monthly bill " + monthName + "- " + invYear + " is " + mrc + ". Swift Internet service bills send only through email, no bills will be send as hard copy." +
                // "It is recommended paying your bills using bKash. Please pay your bills in due time and enjoy uninterrupted service." +
                // "For service complain, please contact at 09611123123 (24X7). If you have any billing or marketing issues please mail to swift@amberit.com.bd  or contact your respective sales person." +
                // "During any inquiry don’t forget to mention your Customer-ID. Our effort becomes meaningful if we achieve your satisfaction.\n";

                // string fromAddress = "swift@amberit.com.bd";
                // string toAddress = email;
                // string ccAddress = ccList;
                // string fromPassword = "";

                // mailBody += "You can find your invoice in our self-care panel https://myswift.amberit.com.bd/ \n";
                // mailBody += "\n";
                // mailBody += "Thanking You\n";
                // mailBody += "\n";
                // mailBody += "\n";
                // mailBody += "Amber IT Limited\n";

                // mailBody += "Swift Team\n";
                // mailBody += "swift@amberit.com.bd\n";
                // mailBody += "www.amberit.com.bd\n";
                // mailBody += "HO Navana Tower Level 7\n";
                // mailBody += "Gulshan 1, Dhaka 1212\n";

                // mailBody += "\n";
                // mailBody += "\n";
                // mailBody += "Note: This mail is generated by Amber Business Suite(ABS).";

                // ReportViewer ReportViewer1 = new ReportViewer();

                // ReportViewer1.Report = new REPORT_INVOICE();

                //( ReportViewer1.Report as REPORT_INVOICE ).CustomerID = CustomerID;

                // ReportViewer1.RefreshReport();

                //Telerik.Reporting.Report report = ( Telerik.Reporting.Report )ReportViewer1.Report;

                // Insert History ==0 

                InsertMailHistory(schedulerId, email, customerId, mailBody, 0);

                result = MailReport(fromAddress, fromPassword, toAddress, ccAddress, subject, mailBody, invNo);

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

        private string getMailBody(string mrc, string dueDate, string month, string cid)
        {
            string body = string.Format(@"
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:107%;font-size:15px;font-family:'Calibri',sans-serif;'><b>Dear Customer,</b></p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:normal;font-size:15px;font-family:'Calibri',sans-serif;'>&nbsp;</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:107%;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>As-salamu Alaykum and Warm Greetings from Amber IT Ltd.</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:normal;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>&nbsp;</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:107%;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>Thank You for Choosing Amber IT as your Trusted Internet Service Provider.</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:normal;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>&nbsp;</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:107%;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>Your monthly e-Statement for {2} is ready for viewing. You can login to Amber IT self-care portal <a href='https://myswift.amberit.com.bd/'>https://myswift.amberit.com.bd/</a> for viewing the Invoice and Payment.</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:normal;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>&nbsp;</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:107%;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;background:#D0CECE;'><span style='color:black;'>Statement Balance in BDT: <strong>{0}</strong></span></p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:107%;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;background:#D0CECE;'><span style='color:black;'>Payment Due Date: <strong>{1}&nbsp;</strong></span></p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:normal;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>&nbsp;</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:8.0pt;margin-left:0cm;line-height:107%;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>Please pay your bill before Due Date and enjoy Uninterrupted Services.</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:8.0pt;margin-left:0cm;line-height:normal;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>For payment mode details please visit <a href='https://www.amberit.com.bd/bill-pay'>https://www.amberit.com.bd/bill-pay</a>.</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:normal;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>For further queries, please call at our 24 hours Contact Center +88 09611-123123 or email us at <a href='mailto:swift@amberit.com.bd'>swift@amberit.com.bd</a>.</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:normal;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>&nbsp;</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:normal;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>During correspondence, always mention your Amber IT Customer-ID ({3}) for quick response. &nbsp; &nbsp;</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:normal;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>&nbsp;</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:normal;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>For more information about our products and services, please visit <a href='http://www.amberit.com.bd'>www.amberit.com.bd</a>.</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:107%;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>You can also find us on Facebook at <a href='https://www.facebook.com/amberit247'>https://www.facebook.com/amberit247</a> where you will get latest information.</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:normal;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>&nbsp;</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:normal;font-size:15px;font-family:'Calibri',sans-serif;text-align:justify;'>We Look Forward to Providing the Best Service Possible with Your Utmost Satisfaction.</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:107%;font-size:15px;font-family:'Calibri',sans-serif;'>&nbsp;</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:107%;font-size:15px;font-family:'Calibri',sans-serif;'>Best Regards,</p>
                    <p style='margin-top:0cm;margin-right:0cm;margin-bottom:.0001pt;margin-left:0cm;line-height:107%;font-size:15px;font-family:'Calibri',sans-serif;'>Amber IT Billing Team</p>", mrc, dueDate, month, cid); //2 - April - 2023

            return body;
        }

        public bool MailReport(string from, string fromPassword, string to, string cc, string subject, string mailBody, string softid)
        {

            bool result;

            try
            {

                // ReportProcessor reportProcessor = new ReportProcessor();
                // RenderingResult result = reportProcessor.RenderReport("PDF", report, null);

                //Attachment attachment = null;
                //using (MemoryStream ms = new MemoryStream(result.DocumentBytes))
                //{
                //     Do something with ms..

                //  attachment= new Attachment(ms, softid + ".pdf");
                //}

                //MemoryStream ms = new MemoryStream(result.DocumentBytes);
                //ms.Position = 0;

                //Attachment attachment = new Attachment(ms, softid + ".pdf");
                //ms.SetLength(0);

                //Mail Message option

                MailMessage msg = new MailMessage();

                // MailAddress fromm = new MailAddress(from);  //"ben@contoso.com", "Ben Miller"
                // MailAddress tom = new MailAddress(to);


                // MailMessage msg = new MailMessage(fromm, tom);

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



        /// <summary>
        /// based on the selected radion box returns the scheduler enum
        /// </summary>
        /// <returns></returns>

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

