using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProccessorLib.Model
{
    public class ConsultationReportDto
    {
        public List<Subscibers> data { get; set; }
        public Pagination meta { get; set; }

    }

    public class Pagination
    {
        public int per_page { get; set; }
        public int to { get; set; }
        public int total { get; set; }
    }

    public class Subscibers
    {
        public string name { get; set; }
        public string contact_no { get; set; }
        public int num_of_consultation { get; set; }
        public string consultation_duration { get; set; }
        public int web_visits { get; set; }
        public int app_visits { get; set; }
        public DateTime subscription_start_date { get; set; }
    }
}
