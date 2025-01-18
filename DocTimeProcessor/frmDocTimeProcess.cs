using ProccessorLib.Model;
using ProccessorLib.Processors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DocTimeProcessor
{
    public partial class frmDocTimeProcess : Form
    {
        DocTimeProcessorService _service;
        ProcessorConfig _config;
        const int processorId = 8;
        int _processInterval;
        Timer timer1 = new Timer();
        DateTime _curretnTime;
        public frmDocTimeProcess()
        {
            InitializeComponent();
            _service = new DocTimeProcessorService();
        }


        private void frmDocTimeProcess_Load(object sender, EventArgs e)
        {
            _config = ProcessorConfigService.GetConfig(processorId);
            if (_config != null)
            {
                _processInterval = _config.Interval;
                var currHour = DateTime.Now.Hour;
                timer1.Enabled = true;
                timer1.Tick += OnTimerEvent;
                _curretnTime = DateTime.Today.AddDays(-1);

               

            }

        }

        private void OnTimerEvent(object sender, EventArgs e)
        {
            timer1.Interval = _processInterval;
            _service.DocTimeAPICall();

            if (_curretnTime != DateTime.Today)
            {
            

                _service.ProcessConsultationReport();
                _curretnTime = DateTime.Today;

            }


        }
    }
}
