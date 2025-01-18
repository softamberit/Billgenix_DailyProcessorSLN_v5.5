using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace ProccessorLib
{
    public class WriteLogFile
    {
        public static void WriteLog(string log)
        {
            var appPath = Assembly.GetExecutingAssembly().Location;
            var index = appPath.LastIndexOf("\\");
            appPath = appPath.Remove(index);
            // Path.Combine(appPath, "ServiceLog" + DateTime.Now.Year);

            string strFileName = appPath + "\\Log";
            //if (!Directory.Exists(strFileName))
            //{
            //    Directory.CreateDirectory(strFileName);
            //}

            var filename = "ServiceLog_" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt";
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