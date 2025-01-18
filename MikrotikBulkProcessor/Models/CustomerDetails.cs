using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikrotikBulkProcessor.Models
{
    public class CustomerDetails
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }

        public string Username { get; set; }//Router UserName
        public string Password { get; set; }
        public string IPAddress { get; set; }
        public string Host { get; set; }
        public string NetMRC { get; set; }
        public string TotalOTC { get; set; }
        public string RouterID { get; set; }
        public int ProtocolID { get; set; }
        public int InsType { get; set; }
        public string MkUser { get; set; }
        public string mkVersion { get; set; }

        public string SecondaryStatus { get; set; }
        public string StatusId { get; set; }
        public string StatusName { get; internal set; }
    }
}
