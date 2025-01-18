using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tik4net;

namespace MkCommunication.Model
{
    public class MkResponse
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Service { get; set; }
        public string Profile { get; set; }
        public string LocalAddress { get; set; }
        public string RemoteAddress { get; set; }
        public string LastLoggedOut { get; set; }
        public string Disabled { get; set; }

        internal static List<MkResponse> Parse(IEnumerable<ITikReSentence> sentences)
        {
            var objMkResList = new List<MkResponse>();

            foreach (ITikReSentence sentence in sentences)
            {
                objMkResList.Add(Parse(sentence));
            }
            return objMkResList;
        }

        internal static MkResponse Parse(ITikReSentence sentence)
        {
            var objMkRes = new MkResponse();
            foreach (var item in sentence.Words)
            {
                if (item.Key == ".id")
                {
                    objMkRes.ID = item.Value;
                }
                if (item.Key == "service")
                {
                    objMkRes.Service = item.Value;
                }
                if (item.Key == "name")
                {
                    objMkRes.Name = item.Value;
                }
                if (item.Key == "profile")
                {
                    objMkRes.Profile = item.Value;
                }
                if (item.Key == "local-address")
                {
                    objMkRes.LocalAddress = item.Value;
                }
                if (item.Key == "remote-address")
                {
                    objMkRes.RemoteAddress = item.Value;
                }
                if (item.Key == "last-logged-out")
                {
                    objMkRes.LastLoggedOut = item.Value;
                }
                if (item.Key == "disabled")
                {
                    objMkRes.Disabled = item.Value;
                }
            }
            return objMkRes;
        }
    }
}
