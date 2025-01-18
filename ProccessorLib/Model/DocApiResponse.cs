using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProccessorLib.Model
{
    public class DocApiResponse
    {
        public DocApiResponse()
        {
            data = new Data();
        }
        public string message { get; set; }
        public Data data { get; set; }
        public string responseJson { get; set; }
        public int ResponseStatus { get; set; }
        public int StatusCode { get; set; }


        public class Data
        {
            public Data()
            {
                user = new User();
                subscription = new Subscription();

            }
            public User user { get; set; }
            public Subscription subscription { get; set; }

        }
        public class User
        {
            public string partner_user_ref { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string contact_no { get; set; }
            public string gender { get; set; }
            public string dob { get; set; }


        }
        public class Subscription
        {
            public Subscription()
            {
                subscription_plan = new Subscription_Plan();
            }
            public string @ref { get; set; }
            public DateTime started_at { get; set; }
            public DateTime ends_at { get; set; }
            public Subscription_Plan subscription_plan { get; set; }


        }
        public class Subscription_Plan
        {
            public string Name { get; set; }
        }
    }
}
