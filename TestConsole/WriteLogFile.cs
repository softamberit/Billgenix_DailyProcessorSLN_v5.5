using System;
using System.IO;

namespace TestConsole
{
    class WriteLogFile
    {
        private static string fileName = "";


        public static void WriteLog(string log)
        {


            //string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //int index = appPath.LastIndexOf("\\");
            //appPath = appPath.Remove(index);

            string path = GetErrorLogPath();
            fileName = DateTime.Now.ToString("yyyy_MM_dd") + "_Log.log";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            try
            {
                File.AppendAllText(Path.Combine(path, fileName), "\r\n" + DateTime.Now.ToString("dd HH:mm:ss.f") + "\t" + log);
            }
            catch { }
        }

        public static string GetErrorLogPath()
        {

            string appPath = System.AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.Combine(appPath, "Service" + DateTime.Now.Year.ToString());

            return path;

        }

        public static string GetErrorLogFullPath()
        {
            fileName = DateTime.Now.ToString("yyyy_MM_dd") + "_error.log";


            string appPath = System.AppDomain.CurrentDomain.BaseDirectory; ;
            string path = Path.Combine(appPath, "Service" + DateTime.Now.Year.ToString());

            return Path.Combine(path, fileName);

        }
    }
}