using BillingERPConn;
using Newtonsoft.Json.Linq;
using ProccessorLib.Model;
using ProccessorLib.Processors;
using RestSharp;
using System;
using System.Collections;
using System.Data;
using System.Windows.Forms;

namespace ReveProcessor
{
    public partial class FrmMain : Form
    {
        ReveProcessorService _reveProcessor;
        ProcessorConfig _config;
        const int processorId = 5;
        int _processInterval;
        public FrmMain()
        {
            InitializeComponent();

            _reveProcessor = new ReveProcessorService();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            _config = ProcessorConfigService.GetConfig(processorId);
            if (_config != null)
            {
                _processInterval = _config.Interval;
                var currHour = DateTime.Now.Hour;
                timer1.Enabled = true;
                timer1.Tick += OnTimerEvent;
                _reveProcessor.ReveIpIprocessorCall();
            }
          

        }

        #region ProcessorSchedular




        private void OnTimerEvent(object sender, EventArgs e)
        {
            timer1.Interval = _processInterval;
            //listBox1.Items.Add(strText);
            _reveProcessor.ReveIpIprocessorCall();

        }

        #endregion
        
          
        private void exitBtn_Click(object sender, EventArgs e)
        {

            Close();
        }


    }

}
