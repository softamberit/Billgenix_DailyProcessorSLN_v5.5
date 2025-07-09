namespace ReportBilling
{
    using BillingERPConn;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using Telerik.Reporting;
    using Telerik.Reporting.Drawing;

    /// <summary>
    /// Summary description for REPORT_INV_STATEMENT.
    /// </summary>
    public partial class REPORT_LOCK_SUMMARY_DETAILS : Telerik.Reporting.Report
    {
        public REPORT_LOCK_SUMMARY_DETAILS()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();
            sdc_LockSummaryDetails.ConnectionString = DBUtility.GetConnectionString();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }


        public DateTime StartDate
        {
            get { return (DateTime)sdc_LockSummaryDetails.Parameters[0].Value; }
            set
            {
                sdc_LockSummaryDetails.Parameters[0].Value = value;
            }
        }

        public DateTime EndDate
        {
            get { return (DateTime)sdc_LockSummaryDetails.Parameters[1].Value; }
            set
            {
                sdc_LockSummaryDetails.Parameters[1].Value = value;
            }
        }


        public string Option
        {
            get { return (string)sdc_LockSummaryDetails.Parameters[2].Value; }
            set
            {
                sdc_LockSummaryDetails.Parameters[2].Value = value;
            }
        }
        public int PopId
        {
            get { return (int)sdc_LockSummaryDetails.Parameters[3].Value; }
            set
            {
                sdc_LockSummaryDetails.Parameters[3].Value = value;
            }
        }
        
    }
}