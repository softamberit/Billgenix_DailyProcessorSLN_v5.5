using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingProcessor
{
    class BOInvoice
    {
        public bool IsSendSMS { get; set; }

        public string InvoiceID { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerID { get; set; }
        public decimal CurrentInvoiceAmount { get; set; }
        public decimal DueAmount { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal NetPay { get; set; }
        public DateTime CreationDate { get; set; }
        public int BillMonth { get; set; }
        public int BillYear { get; set; }
        public bool IsActive { get; set; }
        public bool IsDuplicate { get; set; }
        public string AddBy { get; set; }
        public DateTime AddDate { get; set; }
        public string ModiBy { get; set; }
        public DateTime ModiDate { get; set; }
        public int InvoiceIDSerial { get; set; }
        public int CollectionStatus { get; set; }
        public bool IsVat { get; set; }
        public bool IsCommission { get; set; }
        public DateTime BillingStartDate { get; set; }
        public DateTime BillingEndDate { get; set; }
        public DateTime DueDate { get; set; }
        public string InvCountCWise { get; set; }
        public bool IsSendMail { get; set; }

        public string CustomerName { get; set; }

        public string Address { get; set; }

        public string BillingTel { get; set; }

        public string BillingEmail { get; set; }

        public string DueDateStr { get; set; }

        public string InvoiceDateStr { get; set; }

        public string ChkBox { get; set; }

        public int pin_number { get; set; }

        public string MailFrom1 { get; set; }

        public string MailTo1 { get; set; }

        public string MailCC { get; set; }

        public DateTime SendTime1 { get; set; }

        public string MailMessage1 { get; set; }

        public string Remarks1 { get; set; }

        public string MonthName { get; set; }
        public string ServiceName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public string BillingStartDateStr { get; set; }
        public string BillingEndDateStr { get; set; }
        public string ServiceType { get; set; }
        public string POPName { get; set; }

        public string Download { get; set; }

        public string NetPayStr { get; set; }

        public string Status { get; set; }

        public string TotalAmount { get; set; }

        public string JsonString { get; set; }

        public string Others { get; set; }
        public string NetworkID { get; set; }

        public int MkProcessDays { get; set; }

        public int LockProcessAfterMk { get; set; }

        public int InvoiceGenDayAfter { get; set; }

        public int DuesSMSDaysBefore { get; set; }
    }
}
