using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingProcessor.Models
{
    public class CustomerMaster
    {
        public string CustomerName { get; set; }
        public int StatusID { get; set; }
        public int RouterId { get; set; }
        public string IPAddress { get; set; }
        public decimal Discount { get; set; }
        public decimal CreditLimit { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal NetMRC { get; set; }
        public decimal TotalMRC { get; set; }
        public DateTime EndDate { get; set; }
        public string RouterName { get; set; }
        public decimal TotalOTC { get; set; }
        public string RouterUserName { get; set; }
        public string RouterPassword { get; set; }
        public string RouterHost { get; set; }
        public int RouterID { get; set; }
        public int ProtocolID { get; set; }
        public string Mobile { get; set; }
        public string SecondaryStatus { get; set; }
        public int InsType { get; set; }
        public string MkUser { get; set; }
        public string MkPass { get; set; }
        public string MkVersion { get; set; }
        public string CustomerID { get; set; }
        public int APIPort { get; set; }

        public CustomerStatus CustomerStatus { get; set; }

    }
}
