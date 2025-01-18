using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProccessorLib.Model
{
    public class DocTimeApiCALLHistory
    {
        public int DocApICAllHistoryID { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }

        public int CustomerStatus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int IsAPICallDone { get; set; }
        public int ProcessId { get; set; }
        public string Mobile { get; set; }
        public int EntryID { get; set; }
        public string subscription_plan_Name { get; set; }
        public DateTime ApiCallDoneTime { get; set; }
        public int RequestFor { get; set; }
        public int ResponeStatus { get; set; }



    }
}
