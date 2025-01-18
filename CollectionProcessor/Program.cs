using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CollectionProcessor;

namespace CollectionProcessor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        public readonly static string ProgramName;

        public static Mutex MutexExe;

        static Program()
        {
            Program.ProgramName = "CollectionProcessor.exe";
        }

        [STAThread]
        static void Main()
        {
            //bool flag = false;
            //int num = 5;
            //do
            //{
            //    Program.MutexExe = new Mutex(true, Program.ProgramName, out flag);
            //    num--;
            //    Thread.Sleep(1000);
            //}
            //while (!flag && num > 0);
            //if (!flag)
            //{
            //    MessageBox.Show(string.Format("{0} is already open. Only one instance of this application is allowed at a time.", ProgramName));
            //    return;
            //}
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
