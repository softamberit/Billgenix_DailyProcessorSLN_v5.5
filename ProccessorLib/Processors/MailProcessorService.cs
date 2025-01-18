using BillingERPConn;
using ProccessorLib.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ProccessorLib.Processors
{
   public class MailProcessorService
    {

        DBUtility _idb;
        string _mobileNo;
        int PinNumber = 10000;
       

        public MailProcessorService()
        {
            _idb = new DBUtility();
        
        }
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

        public string InvoiceNumberGeneration(string customerId, string year)
        {

            DataTable dt = _idb.GetDataBySQLString("SELECT ISNULL(MAX(RefnoSerial), 0) + 1 AS RefNo FROM BillingMaster WHERE CustomerID = '" + customerId + "' AND (RefTypeID = 1 OR RefTypeID = 2 OR RefTypeID = 3) ");

            string refno = dt.Rows[0]["RefNo"].ToString();
            string invNumber = year + customerId + refno;

            return invNumber;
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
        public bool SendEmail(string schedulerId, string customerId, string billingEmail, string emailText, string mailCc, string addressFrom)
        {
            bool result;

            try
            {
                DataTable dt = _idb.GetDataBySQLString("SELECT EndDate FROM EMAIL_Schedule WHERE CustomerID = '" + customerId + "' AND ID ='" + schedulerId + "'");
                DateTime invoiceDate = DateTime.Parse(dt.Rows[0]["EndDate"].ToString());

                DataTable dt1 = _idb.GetDataBySQLString("SELECT NetMRC FROM CustomerMaster WHERE CustomerID = '" + customerId + "' ");
                string mrc = dt1.Rows[0]["NetMRC"].ToString();

                string email = billingEmail;
                string invYear = invoiceDate.Year.ToString();
                string monthName = invoiceDate.ToString("MMMM");
                string invNo = InvoiceNumberGeneration(customerId, invYear); //Gen

                string ccList = mailCc;

                // RichTextBox richTexbox = 

                string subject = "Monthly Bill on  " + monthName + ", " + invYear;
                string mailBody = "Dear Valued Customer,\n";

                mailBody += "Thank you very much for choosing Swift Internet Service provided by Amber IT Limited." +
                "Your monthly bill " + monthName + "- " + invYear + " is " + mrc + ". Swift Internet service bills send only through email, no bills will be send as hard copy." +
                "It is recommended paying your bills using bKash. Please pay your bills in due time and enjoy uninterrupted service." +
                "For service complain, please contact at 09611123123 (24X7). If you have any billing or marketing issues please mail to swift@amberit.com.bd  or contact your respective sales person." +
                "During any inquiry don’t forget to mention your Customer-ID. Our effort becomes meaningful if we achieve your satisfaction.\n";

                string fromAddress = "swift@amberit.com.bd";
                string toAddress = email;
                string ccAddress = ccList;
                string fromPassword = "";

                mailBody += "You can find your invoice in our self-care panel https://myswift.amberit.com.bd/ \n";
                mailBody += "\n";
                mailBody += "Thanking You\n";
                mailBody += "\n";
                mailBody += "\n";
                mailBody += "Amber IT Limited\n";

                mailBody += "Swift Team\n";
                mailBody += "swift@amberit.com.bd\n";
                mailBody += "www.amberit.com.bd\n";
                mailBody += "HO Navana Tower Level 7\n";
                mailBody += "Gulshan 1, Dhaka 1212\n";

                mailBody += "\n";
                mailBody += "\n";
                mailBody += "Note: This mail is generated by Amber Business Suite(ABS).";

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
                    InsertMailHistory(schedulerId, email, customerId, mailBody, 1);



            }

            catch (Exception ex)
            {
                WriteLogFile.WriteLog(customerId + ":: Email Catch " + ex.Message);
                result = false;
            }

            return result;
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


                //   msg.Attachments.Add(attachment);



                var smtp = new SmtpClient();
                {
                    smtp.Host = "202.4.96.7";
                    smtp.Port = 25;
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
    }
}
