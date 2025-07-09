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
        public static void WriteLogUD(string log)
        {
            string strFileName = Application.StartupPath + "\\Log";
            //if (!Directory.Exists(strFileName))
            //{
            //    Directory.CreateDirectory(strFileName);
            //}

            var filename = "UpDownProcessor_" + DateTime.Now.ToString("yyyy_MM_dd") + "_Log.txt";
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
        public static void WriteLog(string log)
        {
            string strFileName = Application.StartupPath + "\\Log";
            //if (!Directory.Exists(strFileName))
            //{
            //    Directory.CreateDirectory(strFileName);
            //}

            var filename = "DailyBilling_" + DateTime.Now.ToString("yyyy_MM_dd") + "_Log.txt";
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