using Newtonsoft.Json;
using ProccessorLib.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProccessorLib.HttpHelper
{
    public class DoctimeHttpHelper
    {



        private string base_url { get; set; }
        private string subscription_ref { get; set; }
        private string xapikey { get; set; }

        public DoctimeHttpHelper()
        {
            base_url = ConfigurationManager.AppSettings["base_url"];
            subscription_ref = ConfigurationManager.AppSettings["subscription_ref"];
            xapikey = ConfigurationManager.AppSettings["x-api-key"];
        }

        public DocApiResponse Subscription(DocTimeApiCALLHistory customer)
        {
            var custName = Regex.Replace(customer.CustomerName, @"[\d-]", string.Empty);
            string url = string.Format("{0}/partners/AMBERIT/subscriptions/{1}/subscribe?contact_no={2}&first_name={3}&partner_user_ref={4}&subscription_end_date={5}", base_url, subscription_ref, customer.Mobile, custName, customer.CustomerID, customer.EndDate.AddDays(1).ToString("yyyy-MM-dd"));
            DocApiResponse response = PostRequest(url);
            if (response.StatusCode == 200)
            {
                //if (response.message == "Member subscribed successfully!")
                //{
                response.ResponseStatus = 1;
                //}
            }

            return response;
        }
        public DocApiResponse Unsubscription(DocTimeApiCALLHistory customer)
        {
            string url = string.Format("{0}/partners/AMBERIT/subscriptions/{1}/unsubscribe?contact_no={2}", base_url, subscription_ref, customer.Mobile);
            DocApiResponse response = PostRequest(url);
            if (response.StatusCode == 200)
            {
                if (response.message == "Unsubscribed successfully!")
                {
                    response.ResponseStatus = 1;
                }
            }

            return response;
        }
        public DocApiResponse ExtendSubscription(DocTimeApiCALLHistory customer)
        {
            string url = string.Format("{0}/partners/AMBERIT/subscriptions/{1}/extend?contact_no={2}&subscription_end_date={3}", base_url, subscription_ref, customer.Mobile, customer.EndDate.ToString("yyyy-MM-dd"));
            DocApiResponse response = PostRequest(url);

            if (response.StatusCode == 200)
            {
                //if (response.message == "Extended successfully!")
                //{
                response.ResponseStatus = 1;
                //}

            }

            return response;
        }
        public ConsultationReportDto GetConsultationReport(DateTime startDate, DateTime endDate)
        {
            string url = string.Format("{0}/partners/AMBERIT/consultation-reports?subscription_ref={1}&start_date={2}&end_date={3}&per_page=1000&page=1", base_url, subscription_ref, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
            ConsultationReportDto response = GetRequest(url);

            return response;
        }
        private DocApiResponse PostRequest(string url)
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", xapikey);
            request.AddHeader("Accept", "application/json");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new
                                RemoteCertificateValidationCallback
                                (
                                   delegate { return true; }
                                );


            DocApiResponse response = new DocApiResponse();

            var res = client.Execute<DocApiResponse>(request);


            if (!string.IsNullOrEmpty(res.Content))
            {

                response = JsonConvert.DeserializeObject<DocApiResponse>(res.Content);
                response.responseJson = res.Content;
            }

            response.StatusCode = ((int)res.StatusCode);

            return response;
        }

        private ConsultationReportDto GetRequest(string url)
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-api-key", xapikey);
            request.AddHeader("Accept", "application/json");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new
                                RemoteCertificateValidationCallback
                                (
                                   delegate { return true; }
                                );


            ConsultationReportDto response = new ConsultationReportDto();

            var res = client.Get<ConsultationReportDto>(request);


            if (!string.IsNullOrEmpty(res.Content))
            {

                response = JsonConvert.DeserializeObject<ConsultationReportDto>(res.Content);

            }

            return response;
        }
    }
}
