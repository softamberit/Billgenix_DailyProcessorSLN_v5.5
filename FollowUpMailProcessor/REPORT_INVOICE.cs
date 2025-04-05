using BBSReport;
using System;

namespace ReportBilling
{
    /// <summary>
    /// Summary description for InvoiceReport.
    /// </summary>
    public partial class REPORT_INVOICE : Telerik.Reporting.Report
    {
        public REPORT_INVOICE()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        public string CustomerID
        {
            get
            {
                return (string)sdc_InvoiceInfo.Parameters[0].Value;
            }
            set
            {
                sdc_InvoiceInfo.Parameters[0].Value = value;
                sdc_BillingInfo.Parameters[0].Value = value;
            }

        }

        public static object EtoWDO(object value1)
        {
            double d1 = Convert.ToDouble(value1);

            BBSClass ns1 = new BBSClass();
            return ns1.changeNumericToWords(d1);
        }
    }
}