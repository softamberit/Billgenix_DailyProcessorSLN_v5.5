using System;
using System.IO;

namespace MikrotikBulkProcessor
{
   public class WriteLogFile
    {
        public static bool WriteLog(string strMessage)
        {
            string strFileName = "C:\\DailyProcessorLog\\MKMoitoringProcesslog.txt";




            strMessage = DateTime.Now.ToString() + " " + strMessage;
            if (!File.Exists(strFileName))
            {
                File.Create(strFileName);
            }

            try
            {
                FileStream objFilestream = new FileStream(string.Format("{0}", strFileName), FileMode.Append, FileAccess.Write);
                StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);
                objStreamWriter.WriteLine(strMessage);
                objStreamWriter.Close();
                objFilestream.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }  
}