using ProccessorLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessorServiceManager
{
    public partial class ProcessorSvc : ServiceBase
    {
        BackgroundServiceManager _manager;
        
        public ProcessorSvc()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _manager = new BackgroundServiceManager();
            WriteLogFile.WriteLog("Monitoring process is running...");
            _manager.InitTimer();

            base.OnStart(args);


        }

        

        protected override void OnStop()
        {
            _manager.StopTimer();
            base.OnStop();

        }
    }
}
