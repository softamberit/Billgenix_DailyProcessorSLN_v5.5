using BillingERPConn;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProccessorLib.Processors
{
    public class ReveProcessorService
    {
        DBUtility _idb;

        string _mobileNo;
        int PinNumber = 10000;

        public ReveProcessorService()
        {
            _idb = new DBUtility();
           
        }


        public void ReveIpIprocessorCall()
        {
            try
            {
                var strText = "**********Reve API Processor Scheduler has been started!*****" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt");
                WriteLogFile.WriteLog(strText);

                var baseUrl = _idb.GetDataBySQLString("Select ReveApiUrl From CompanyInfo Where Id=1");

                var reveApiUrl = baseUrl.Rows[0]["ReveApiUrl"].ToString();

                var dtCust = _idb.GetDataByProc("sp_getInfoReveApiCallHistory");

                var type = "&type=subscribePackage&packageID=";
                var crm = "&requestFromCRM=true";

                var processStartTime = DateTime.Now;
                var noOfCustomer = dtCust.Rows.Count;
                var mobile = "";
                var customerId = "";
                var successLogBeforeProcess = "";
                var SuccessLogAfterProcess = "";
                var processErrorlog = "";

                var url = "";
                var reveApIcAllHistoryId = "";

                var builder1 = new System.Text.StringBuilder();
                builder1.Append(successLogBeforeProcess);

                var builder2 = new System.Text.StringBuilder();
                builder2.Append(SuccessLogAfterProcess);

                foreach (DataRow dr in dtCust.Rows)
                {
                    try
                    {
                        reveApIcAllHistoryId = dr["ReveApICAllHistoryID"].ToString();
                        customerId = dr["CustomerID"].ToString();
                        mobile = dr["Mobile"].ToString();
                        var packageId = dr["RevePackageId"].ToString();

                        url = reveApiUrl + mobile + type + packageId + crm;

                        var client = new RestClient(url);
                        var request = new RestRequest(Method.GET);
                        IRestResponse response = client.Execute<ReveApiRequest>(request);

                        //var json = "[" + response.Content + "]";

                        dynamic data = JObject.Parse(response.Content);
                        string statusCode = data.status;

                        if (statusCode == "0")//
                        {
                            try
                            {


                                _idb.GetDataBySQLString(string.Format("Update REVEAPICALLHistory Set IsActive = 0, IsAPICallDone = 1, ApiCallDoneTime ='{0}',mobile='{1}'  Where ReveApICAllHistoryID = '{2}' select 'Success' Feedback ", DateTime.Now, mobile, reveApIcAllHistoryId));

                                //_idb.GetDataBySQLString("Update REVEAPICALLHistory Set IsActive = 0, IsAPICallDone = 1, ApiCallDoneTime = " +
                                //    "'" + DateTime.Now + "',mobile=" + mobile + "'  Where ReveApICAllHistoryID = '" + reveApIcAllHistoryId + "' select 'Success' Feedback ");

                                builder2.Append("RACHID: " + reveApIcAllHistoryId + " CID: " + customerId + " Mobile: " + mobile + " RPId: " + packageId + " StatusCode: " + statusCode + " # ");
                                WriteLogFile.WriteLog("RACHID: " + reveApIcAllHistoryId + " CID: " + customerId + " Mobile: " + mobile + " RPID: " + packageId + " StatusCode: " + statusCode + " # ");

                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }
                        }
                        else
                        {
                            //Added by Sobuj 2021-Apr-21
                            var ht1 = new Hashtable
                        {

                              { "ErrorMessage",statusCode}
                            , {"ReveApICAllHistoryID", reveApIcAllHistoryId }
                            , { "ReveApiUrl", url }
                            , { "CustomerID", customerId }
                            , { "CustMobile", mobile }
                            , { "LogEntryID", 50009 }

                        };

                            _idb.GetDataByProc(ht1, "sp_InsertReveProcessorErrorLog");

                            processErrorlog += response.ErrorException == null ? "" : response.ErrorException.Message + "# ";
                        }

                        builder1.Append("RACHID: " + reveApIcAllHistoryId + " CID: " + customerId + " Mobile: " + mobile + " RPId: " + packageId + " StatusCode: " + statusCode + " # ");
                    }
                    catch (Exception ex)
                    {
                        var ht1 = new Hashtable
                        {

                              { "ErrorMessage", ex.Message }
                            , {"ReveApICAllHistoryID", reveApIcAllHistoryId }
                            , { "ReveApiUrl", url }
                            , { "CustomerID", customerId }
                            , { "CustMobile", mobile }
                            , { "LogEntryID", 50000 }

                        };

                        _idb.GetDataByProc(ht1, "sp_InsertReveProcessorErrorLog");

                        processErrorlog += ex.Message + "# ";

                        // continue;
                    }
                }
                successLogBeforeProcess = builder1.ToString();
                SuccessLogAfterProcess = builder2.ToString();
                var ht = new Hashtable
                {
                    {"successLogBeforeProcess", successLogBeforeProcess},
                    {"SuccessLogAfterProcess", SuccessLogAfterProcess},
                    {"processErrorlog", processErrorlog},
                    {"processStartTime", processStartTime},
                    {"ProcessEndTime", DateTime.Now},
                    {"noOfCustomer", noOfCustomer},
                    {"ProcessorTypeName", "ReveAPIProcessor"},
                    {"ID", 3}
                };

                _idb.GetDataByProc(ht, "sp_InsertProcessLog");
            }
            catch (Exception xx)
            {
                WriteLogFile.WriteLog(xx.ToString());
            }
        }

        class ReveApiRequest
        {
            string status;
        }
    }
}
