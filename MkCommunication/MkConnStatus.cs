using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MkCommunication
{
   public class MkConnStatus
    {

        public string StatusCode { get; set; }
        public string Status { get; set; }
        public string RetMessage { get; set; }
        public string CodePortion { get; set; }
        public int MikrotikStatus { get; set; }
        public int DLength { get; set; }

    }
}
