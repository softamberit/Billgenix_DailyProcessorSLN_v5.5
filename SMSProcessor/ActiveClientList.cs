namespace EMAILProcessor
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using Telerik.Reporting;
    using Telerik.Reporting.Drawing;

    /// <summary>
    /// Summary description for CustomerInfoRpt.
    /// </summary>
    public partial class ActiveClientList : Telerik.Reporting.Report
    {
        public ActiveClientList()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        public int CustomerType
        {
            get
            {
                return 1;//(int)sqlDsActiveList.Parameters[0].Value;
            }
            set
            {
             //   sqlDsActiveList.Parameters[0].Value = value;
               

            }

        }
        public int Status
        {
            get
            {
                return 1;// (int)sqlDsActiveList.Parameters[1].Value;
            }
            set
            {
               // sqlDsActiveList.Parameters[1].Value = value;
               

            }

        }
    }
}