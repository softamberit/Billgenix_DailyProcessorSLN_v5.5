using BillingERPConn;
using Newtonsoft.Json.Linq;
using ProccessorLib.HttpHelper;
using ProccessorLib.Model;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProccessorLib.Processors
{
    public class DocTimeProcessorService
    {
        DBUtility _idb;

        string _mobileNo;
        int PinNumber = 10000;
        DoctimeHttpHelper _docHttp;

        public DocTimeProcessorService()
        {
            _idb = new DBUtility();
            _docHttp = new DoctimeHttpHelper();
        }
        public void DocTimeAPICall()
        {
            try
            {
                WriteLogFile.WriteLog("<=== Start Doctime API ===>");

                var dtCust = _idb.GetDataByProc<DocTimeApiCALLHistory>("sp_getPendingDoctimeSubscription");
                var processStartTime = DateTime.Now;
                var noOfCustomer = dtCust.Count;
                var successLogBeforeProcess = "";
                var SuccessLogAfterProcess = "";

                var builder1 = new System.Text.StringBuilder();
                var builder2 = new System.Text.StringBuilder();
                var builder3 = new System.Text.StringBuilder();
                foreach (var cust in dtCust)
                {
                    try
                    {

                        if (cust.Mobile.Length == 13 && cust.Mobile.StartsWith("88"))
                        {
                            cust.Mobile = cust.Mobile.Remove(0, 2);
                        }

                        builder1.Append(string.Format("{0},", cust.CustomerID));

                        DocApiResponse docApiResponse = null;
                        if (cust.RequestFor == 1) //Subscription
                        {
                            WriteLogFile.WriteLog(cust.CustomerID + "==>" + "Subscription");

                            docApiResponse = _docHttp.Subscription(cust);
                            WriteLogFile.WriteLog(cust.CustomerID + "==>" + docApiResponse.message);

                        }
                        else if (cust.RequestFor == 3)  // Extend
                        {
                            WriteLogFile.WriteLog("Extend");
                            docApiResponse = _docHttp.ExtendSubscription(cust);
                            WriteLogFile.WriteLog(cust.CustomerID + "==>" + docApiResponse.message);

                            if (docApiResponse.StatusCode == 406)
                            {
                                docApiResponse = _docHttp.Subscription(cust);
                            }
                            else if (docApiResponse.StatusCode == 404)
                            {
                                var dbh = new DBUtility();
                                Hashtable hts = new Hashtable
                                {
                                    { "CustomerId", cust.CustomerID }
                                };
                                var lastSubs = dbh.GetDataByProc<DocTimeApiCALLHistory>(hts,"sp_GetLastSubscribedDoctime");
                                hts.Remove(cust.CustomerID);
                                hts.Clear();
                               


                                if (lastSubs != null && !string.IsNullOrEmpty(lastSubs.SingleOrDefault().Mobile) && lastSubs.SingleOrDefault().Mobile != cust.Mobile)
                                {
                                    var unsubs = new DocTimeApiCALLHistory();
                                    unsubs.CustomerID = cust.CustomerID;
                                    unsubs.Mobile = lastSubs.SingleOrDefault().Mobile;
                                    docApiResponse = _docHttp.Unsubscription(unsubs);
                                    if (docApiResponse.StatusCode == 200 || docApiResponse.StatusCode==406)
                                    {
                                        docApiResponse = _docHttp.Subscription(cust);
                                    }

                                }


                            }

                        }
                        else  //Unsubscribe
                        {
                            WriteLogFile.WriteLog(cust.CustomerID + "==>" + "Unsubscribe");

                            docApiResponse = _docHttp.Unsubscription(cust);
                            WriteLogFile.WriteLog(cust.CustomerID + "==>" + docApiResponse.message);

                        }

                        if (docApiResponse.ResponseStatus == 1)//
                        {
                            try
                            {
                                _idb.GetDataBySQLString(string.Format(@"Update DocTimeApiCALLHistory Set IsAPICallDone = 1, ApiCallDoneTime =getdate(),Response='{1}',
                                    subscription_plan_Name='{2}',subscription_ref='{3}',HttpStatusCode={4} Where DocApICAllHistoryID = '{0}' select 'Success' Feedback ",
                                    cust.DocApICAllHistoryID, docApiResponse.responseJson, docApiResponse.data.subscription.subscription_plan.Name, docApiResponse.data.subscription.@ref, docApiResponse.StatusCode));
                                builder2.Append(string.Format("{0},", cust.CustomerID));
                            }
                            catch (Exception ex)
                            {
                                builder3.Append(string.Format("{0}:{1},", cust.CustomerID, ex.Message));

                            }
                        }
                        else
                        {
                            _idb.GetDataBySQLString(string.Format(@"Update DocTimeApiCALLHistory Set IsAPICallDone =-1, ApiCallDoneTime =getdate(),Response='{1}',HttpStatusCode={2}  Where DocApICAllHistoryID = '{0}'  select 'Failed' as Feedback ",
                                   cust.DocApICAllHistoryID, docApiResponse.responseJson.Replace("'", "''"), docApiResponse.StatusCode));

                            builder3.Append(string.Format("{0}:{1},", cust.CustomerID, docApiResponse.message));
                        }


                    }
                    catch (Exception ex)
                    {
                        WriteLogFile.WriteLog(cust.CustomerID + "==>" + ex.ToString());
                    }
                    Thread.Sleep(1000);
                }
                if (dtCust.Count > 0)
                {


                    successLogBeforeProcess = builder1.ToString();
                    SuccessLogAfterProcess = builder2.ToString();
                    WriteLogFile.WriteLog(SuccessLogAfterProcess);
                    var ht = new Hashtable
                {
                    {"successLogBeforeProcess", successLogBeforeProcess},
                    {"SuccessLogAfterProcess", SuccessLogAfterProcess},
                    {"processErrorlog", builder3.ToString()},
                    {"processStartTime", processStartTime},
                    {"ProcessEndTime", DateTime.Now},
                    {"noOfCustomer", noOfCustomer},
                    {"ProcessorTypeName", "DocTimeAPIProcessor"},
                    {"ID", 3}
                };

                    _idb.GetDataByProc(ht, "sp_InsertProcessLog");
                }
            }
            catch (Exception xx)
            {
                WriteLogFile.WriteLog(xx.ToString());
            }
            finally
            {
                WriteLogFile.WriteLog("<=== End Doctime API ===>");

            }
        }

        public void ProcessConsultationReport()
        {
            WriteLogFile.WriteLog("<=== Start Doctime Consultation Log ===>");
            var logs = _docHttp.GetConsultationReport(DateTime.Today.AddDays(-1), DateTime.Now);
            foreach (var item in logs.data)
            {
                var ht = new Hashtable
                {
                    {"name", item.name},
                    {"contact_no", item.contact_no},
                    {"num_of_consultation", item.num_of_consultation},
                    {"consultation_duration", item.consultation_duration??""},
                    {"web_visits", item.web_visits},
                    {"app_visits", item.app_visits},
                    {"subscription_start_date", item.subscription_start_date},

                };


                _idb.GetDataByProc(ht, "spInsertDoctimeConsultationLog");
                ht.Clear();
            }
            WriteLogFile.WriteLog("<=== End Doctime Consultation Log ===>");


        }

        class ReveApiRequest
        {
            string status;
        }
    }
}
