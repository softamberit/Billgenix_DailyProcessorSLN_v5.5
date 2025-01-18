using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Windows.Forms;

namespace SWIFTDailyProcessor
{
    class WriteLogFile
    {
        //public static bool WriteLog(string strMessage)
        //{
        //    string strFileName = "C:\\DailyProcessorLog\\SMS_EMailProcesslog.txt";




        //    strMessage = DateTime.Now.ToString() + " " + strMessage;


        //    try
        //    {
        //        FileStream objFilestream = new FileStream(string.Format("{0}", strFileName), FileMode.Append, FileAccess.Write);
        //        StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);
        //        objStreamWriter.WriteLine(strMessage);
        //        objStreamWriter.Close();
        //        objFilestream.Close();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}
        public static void WriteLogSMS(string log)
        {
            WriteLog(log, "SMSLog");
        }
        public static void WriteLogVoice(string log)
        {
            WriteLog(log, "VoiceLog");
        }
        public static void WriteLogVoice2(string log)
        {
            WriteLog(log, "VoiceLog2");
        }
        public static void WriteLog(string log, string logName)
        {
            string strFileName = Application.StartupPath + "\\Log";
            //if (!Directory.Exists(strFileName))
            //{
            //    Directory.CreateDirectory(strFileName);
            //}

            var filename = logName + "_" + DateTime.Now.ToString("yyyy_MM_dd") + "_Log.txt";
            // var path = Path.Combine(strFileName, filename);

            if (!Directory.Exists(strFileName))
            {
                Directory.CreateDirectory(strFileName);
            }
            try
            {
                File.AppendAllText(strFileName + "\\" + filename, "\r\n" + DateTime.Now.ToString("dd HH:mm:ss.f") + "\t" + log);

            }
            catch (Exception e)
            {

            }
        }
    }
}