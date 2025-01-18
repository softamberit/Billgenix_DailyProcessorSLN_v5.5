using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MkCommunication.Model
{
    public class ActiveUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Service { get; set; }
        public int Status { get; set; }
        public string IPAddress { get; set; }
        public int PopId { get; set; }
    }
}
