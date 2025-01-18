using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikrotikBulkProcessor.Models
{
    public class ActivePPPUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Service { get; set; }
        public int MkStatus { get; set; }
        public string IPAddress { get; set; }
        public string RemoteIP { get; set; }
        public int RouterId { get; set; }
        public int PopId { get; set; }
        public string CallerId { get; set; }
        public string Comment { get; set; }
        public string last_logged_out { get; set; }
        public string last_disconnect_reason { get; set; }
        public string last_caller_id { get; set; }
        public DateTime logDate { get; set; }

        public int StatusId { get; set; }
    }
}
