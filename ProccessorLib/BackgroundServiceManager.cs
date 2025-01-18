using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Timers;
//using System.Threading;
//using System.Threading.Tasks;

namespace ProccessorLib
{
    public class BackgroundServiceManager
    {
        Timer timer;
        bool isCheckBillingProcessor = false;
        DateTime runDate;
        public BackgroundServiceManager()
        {
            int interval = 1;
            int.TryParse(ConfigurationManager.AppSettings["Interval"], out interval);
            timer = new Timer();
            timer.Interval = 1000 * 60 * interval;
            timer.Enabled = false;
            timer.Elapsed += Timer_Elapsed;
            runDate = new DateTime();
        }
        public void ProcessorHandler()
        {
            var processors = ProcessorList.GetAll();
            foreach (var proc in processors)
            {
                try
                {
                    WriteLogFile.WriteLog(string.Format("{0} is checking...", proc.ProcessorExeName));
                    if (!IsOpenUI(proc))
                    {
                        if (IsValidProcessor(proc))
                        {
                            if (StartWinApp(proc))
                            {

                                var process = Process.GetProcessesByName(proc.ProcessorExeName).FirstOrDefault();
                                proc.IsResponding = process.Responding;
                                proc.ProcessResponse = "Newly Started";
                                proc.LastRunTime = DateTime.Now;

                                WriteLogFile.WriteLog(string.Format("{0} is started.", proc.ProcessorExeName));
                            }

                        }
                        else
                        {
                            proc.ProcessResponse = "Processor path not valid";
                        }
                    }
                    else
                    {

                        if (proc.IsRestart)
                        {
                            ProcessorList.SaveProcesorResponsLog(proc);
                            StopWinApp(proc.ProcessorExeName);
                            if (StartWinApp(proc))
                            {
                                proc.ProcessResponse = "Restarted";
                                proc.IsRestart = false;
                            }

                        }
                        //else
                        //{

                        //    var process = Process.GetProcessesByName(proc.ProcessorExeName).FirstOrDefault();
                        //    if (process.Responding != proc.IsResponding)
                        //    {
                        //        if ((proc.LastRunTime - DateTime.Now).Minutes > 15)
                        //        {
                        //            ProcessorList.SaveProcesorResponsLog(proc);
                        //            proc.ProcessResponse = "Long Time Not Responding";

                        //            proc.IsRestart = true;
                        //        }

                        //    }

                        //    proc.IsResponding = process.Responding;
                        //    proc.ProcessResponse = "Started";
                        //    proc.LastRunTime = DateTime.Now;

                        //    WriteLogFile.WriteLog(string.Format("{0} is started.", proc.ProcessorExeName));
                        //}
                    }
                    ProcessorList.SaveProcesorRespons(proc);

                }
                catch (Exception ex)
                {
                    proc.IsResponding = false;
                    proc.ProcessResponse = ex.Message;
                    WriteLogFile.WriteLog(string.Format("{0}- Err-->{1}", proc.ProcessorExeName, ex.Message));
                }

            }
        }

        public void StopTimer()
        {
            timer?.Stop();
        }

        public void InitTimer()
        {
            timer.Enabled = true;
            timer?.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {

                timer.Stop();
                ProcessorHandler();

                if (DateTime.Now.TimeOfDay >= new TimeSpan(10, 00, 00) && DateTime.Now.TimeOfDay >= new TimeSpan(10, 10, 00))
                {
                    if (!isCheckBillingProcessor)
                    {
                        isCheckBillingProcessor = true;
                        runDate = DateTime.Now;

                        BillingProcessorRunStatus();

                    }
                }

                if (runDate.Date != DateTime.Now.Date)
                {
                    isCheckBillingProcessor = false;
                }


            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ex.Message);

            }
            finally
            {
                timer.Start();
            }
        }

        private void BillingProcessorRunStatus()
        {
            var log = ProcessorList.GetProcessorLog();
            if (log == null)
            {


                string fromAddress = "swift@amberit.com.bd";
                string toAddress = "software@amberit.com.bd";
                string subject = "Billing processor is not run at today: " + DateTime.Now.ToString("dd-MMM-yyyy");
                string body = "Dear Concern,";
                body += "<br>" +
                    "Billing process is not run at today, please take necessary step. ";
                body += "<br>" + "Processor Checking time: " + DateTime.Now.ToString();
                WriteLogFile.WriteLog(subject);
                MailReport(fromAddress, "", toAddress, "", subject, body, "");
                WriteLogFile.WriteLog("Mail Sent to: " + toAddress);

            }
            else
            {
                string fromAddress = "swift@amberit.com.bd";
                string toAddress = "software@amberit.com.bd";
                string subject = "Billing processor is run successfully at today: " + DateTime.Now.ToString("dd-MMM-yyyy");
                string body = "Dear Concern,";
                body += string.Format("</br>" +
                    "Billing process is run successfully at today.</br> ");
                body += string.Format("Process Start Time: " + log.ProcessStartTime);
                body += string.Format("Process End Time: " + log.ProcessEndTime);
                body += string.Format("Process Duration: " + (log.ProcessEndTime- log.ProcessStartTime));
                body += string.Format("Total Customers: " + log.NumberOfCustomer);
                 

                WriteLogFile.WriteLog(subject);

                MailReport(fromAddress, "", toAddress, "", subject, body, "");
            }
        }

        private bool IsValidProcessor(ProcessorModel proc)
        {
            try
            {
                string fullPath = Path.Combine(proc.PhysicalExePath, proc.ProcessorExeName + ".exe");
                return File.Exists(fullPath);
            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLog(string.Format("{0} not valid path: {1}. Error: {2}", proc.ProcessorExeName, proc.PhysicalExePath, ex.Message));

                return false;
            }

        }

        public bool StartWinApp(ProcessorModel processor)
        {
            var process = Process.GetProcessesByName(processor.ProcessorExeName);

            try
            {


                if (process.Length == 0)
                {
                    // var appPath = AppDomain.CurrentDomain.BaseDirectory;
                    var frmAppPath = Path.Combine(processor.PhysicalExePath, processor.ProcessorExeName + ".exe");

                    var proc = new Process
                    {
                        StartInfo =
                    {
                        FileName = frmAppPath,
                        UseShellExecute = true,


                    }
                    };


                    proc.Start();
                    proc.Close();
                    WriteLogFile.WriteLog(string.Format("{0} is starting", processor.ProcessorExeName));
                    return true;

                }
                else
                {
                    //if (process.Length > 1)
                    //{
                    //    StopWinApp(processor.ProcessorExeName);
                    //}
                }

            }
            catch (Exception dx)
            {

                WriteLogFile.WriteLog(string.Format("{0} ", dx.Message));

            }
            return false;

        }

        public void StopWinApp(string serviceName)
        {


            var process = Process.GetProcessesByName(serviceName);

            foreach (var pr in process)
            {
                try
                {
                    pr.Kill();
                    WriteLogFile.WriteLog(string.Format("{0} is stopped", serviceName));

                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Access is denied"))
                    {
                        string msg = string.Format("Please set {0}-->Right Click-->Properties-->Compatibility-->Run this program as an administrator", serviceName);

                        WriteLogFile.WriteLog(msg);


                    }

                }
            }


        }

        public bool IsOpenUI(ProcessorModel model)
        {
            var process = Process.GetProcessesByName(model.ProcessorExeName);

            if (process.Length > 0)
            {


                return true;
            }
            return false;
        }

        public bool MailReport(string from, string fromPassword, string to, string cc, string subject, string mailBody, string softid)
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
                if (string.IsNullOrEmpty(smtp_h))
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
    }
}
