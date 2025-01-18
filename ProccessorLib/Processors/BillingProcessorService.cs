using BillingERPConn;
using MkCommunication;
using ProccessorLib.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProccessorLib.Processors
{
    public class BillingProcessorService
    {
        DBUtility _idb;
        SMSService smsService;
        MikrotikErrorLogService mkErrorLogService;
        string _mobileNo;
        int PinNumber = 10000;

        public BillingProcessorService()
        {
            _idb = new DBUtility();
            smsService = new SMSService();
            mkErrorLogService = new MikrotikErrorLogService(PinNumber);
        }
      
        public void DailyBillingProcessor()
        {
            try
            {

                MkConnection objMkConnection = new MkConnection();

                DataTable dtCust = _idb.GetDataBySQLString("SELECT CustomerMaster.[CustomerID] FROM [dbo].[CustomerMaster] Where StatusID=1 order By CustomerID asc");

                DateTime cd = DateTime.Today;
                DateTime processStartTime = DateTime.Now;
                int noOfCustomer = 0;

                string MkStatus = "";

                string customerId = "", smsText;
                string successLogBeforeProcess = "", successLogAfterProcess = "", processErrorlog = "", mkLogError = "";
                foreach (DataRow dr in dtCust.Rows)
                {
                    try
                    {
                        Hashtable ht = new Hashtable();
                        customerId = dr["CustomerID"].ToString();

                        ht.Add("CustomerID", customerId);
                        DataTable dtCustomerInfo = _idb.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();
                        foreach (DataRow datarow in dtCustomerInfo.Rows)
                        {
                            if (!String.IsNullOrEmpty(datarow["EndDate"].ToString()))
                            {
                                decimal debit = Conversion.TryCastDecimal(datarow["Debit"].ToString());
                                decimal credit = Conversion.TryCastDecimal(datarow["Credit"].ToString());
                                decimal cl = Conversion.TryCastDecimal(datarow["CreditLimit"].ToString());
                                decimal pv = Conversion.TryCastDecimal(datarow["TotalMRC"].ToString());
                                decimal dsc = Conversion.TryCastDecimal(datarow["Discount"].ToString());
                                DateTime ed = Conversion.TryCastDate(datarow["EndDate"].ToString());

                                var username = datarow["RouterUserName"].ToString();
                                var password = datarow["Password"].ToString();
                                var ipAddress = datarow["IPAddress"].ToString();
                                var hostname = datarow["Host"].ToString();
                                var netMrc = decimal.Parse(datarow["NetMRC"].ToString());
                                var routerId = (datarow["RouterID"].ToString());
                                var protocolId = int.Parse(datarow["ProtocolID"].ToString());
                                var mobile = datarow["Mobile"].ToString();
                                decimal balance = credit - debit;
                                DateTime ced = DateTime.Parse(datarow["EndDate"].ToString());
                                var insType = datarow["InsType"].ToString();
                                var mkUser = datarow["MkUser"].ToString();
                                var mkVersion = datarow["mkVersion"].ToString();
                                if (cd > ed)
                                {
                                    decimal inv = debit + pv - dsc;
                                    decimal ca = credit + cl;

                                    if (inv > ca)
                                    {
                                        // MK OFF, DIscontinue 

                                        MkConnStatus objMkConnStatusDisable = objMkConnection.DisableMikrotik(hostname, username, password, mkVersion, protocolId, customerId, Conversion.TryCastInteger(insType), mkUser);

                                        if (objMkConnStatusDisable.StatusCode == "200")
                                        {
                                            ht.Add("CustomerID", customerId);
                                            ht.Add("POPId", routerId);
                                            ht.Add("CustomerIP", ipAddress);
                                            ht.Add("Status", 0);
                                            ht.Add("StatusID", 9);
                                            ht.Add("ProcessID", 100);
                                            ht.Add("EntryID", PinNumber);
                                            ht.Add("ActivityDetail", "LOCK_FROM_BILLING");
                                            ht.Add("SeconderyStatus", "LOCK_FROM_BILLING");

                                            _idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                            ht.Clear();

                                            successLogBeforeProcess += "ID:" + customerId + ", M:" + netMrc + ", B:" + balance + "CED:" + ced + ", MK:" + MkStatus + " #";
                                            successLogAfterProcess += "ID:" + customerId + ", M:" + netMrc + ", B:" + balance + "CED:" + ced + ", MK:" + "false #";

                                            // SMS 
                                            string dateStr = DateTime.Today.ToString("MMM dd, yyyy");
                                            smsText = "Your Internet has been locked on " + dateStr + ".Pls pay your bill to avoid disconnection. For details visit " + GetCompanyInfo() + ". Pls ignore if already paid";
                                            smsService.SendSMS(customerId, smsText, mobile);


                                            noOfCustomer++;
                                        }

                                        // Insert MK Error log
                                        else
                                        {
                                            mkLogError += "I:" + customerId + ", Er. " + objMkConnStatusDisable.RetMessage + " #";
                                            mkErrorLogService.InsertMikrotikErrorLog(customerId, routerId, hostname, ipAddress, objMkConnStatusDisable.RetMessage, objMkConnStatusDisable.StatusCode);
                                        }

                                    }
                                    else if (inv <= ca)
                                    {
                                        // MK ON, INV GEN

                                        // -----  Bill generation ----------//

                                        ht.Add("CustomerID", customerId);
                                        ht.Add("EntryID", PinNumber);
                                        ht.Add("ProcessID", 100);
                                        ht.Add("MRCAmount", netMrc);
                                        ht.Add("ActivityDetail", "ACTIVE_FROM_BILLING");

                                        _idb.GetDataByProc(ht, "sp_BillGeneDuringDailyProcess");

                                        ht.Clear();
                                        successLogBeforeProcess += "ID:" + customerId + ", M:" + netMrc + ", B:" + balance + "CED:" + ced + ", MK:" + MkStatus + " #";

                                        // SMS 
                                        string dateStr = DateTime.Today.ToString("MMM dd, yyyy");
                                        smsText = "Your current billing cycle has been started from " + dateStr + " for MRC " + netMrc + " .You can access our online billing on " + GetCompanyInfo() + ".";
                                        smsService.SendSMS(customerId, smsText, mobile);
                                        noOfCustomer++;


                                        //  Mikrotik Enable Check 
                                        MkConnStatus objMkConnStatusEnable = objMkConnection.EnableMikrotik(hostname, username, password, mkVersion, protocolId, customerId, Conversion.TryCastInteger(insType), mkUser);


                                        if (objMkConnStatusEnable.StatusCode == "200")
                                        {

                                            ht.Add("CustomerID", customerId);
                                            ht.Add("POPId", routerId);
                                            ht.Add("CustomerIP", ipAddress);
                                            ht.Add("Status", 1);

                                            ht.Add("ProcessID", 100);
                                            ht.Add("EntryID", PinNumber);

                                            _idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                            ht.Clear();

                                            successLogAfterProcess += "ID:" + customerId + ", M:" + netMrc + ", B:" + balance + "CED:" + DateTime.Today.AddDays(31) + ", MK:" + "true #";

                                        }

                                        // --- Insert MK Error log for Enabling IP-----//

                                        else
                                        {
                                            mkLogError += "I:" + customerId + ", Er. " + objMkConnStatusEnable.RetMessage + " #";
                                            mkErrorLogService.InsertMikrotikErrorLog(customerId, routerId, hostname, ipAddress, objMkConnStatusEnable.RetMessage, objMkConnStatusEnable.StatusCode);
                                        }
                                    }

                                }

                                else if (cd <= ed)
                                {
                                    decimal inv = debit;
                                    decimal mr = credit + cl;

                                    if (inv <= mr)
                                    {
                                        // MK ON   NO NEED TO CODE??

                                    }
                                    else if (inv > mr)
                                    {
                                        //MK OFF Discontinue

                                        MkConnStatus objMkConnStatusDisable = objMkConnection.DisableMikrotik(hostname, username, password, mkVersion, protocolId, customerId, Conversion.TryCastInteger(insType), mkUser);

                                        if (objMkConnStatusDisable.StatusCode == "200")
                                        {
                                            ht.Add("CustomerID", customerId);
                                            ht.Add("POPId", routerId);
                                            ht.Add("CustomerIP", ipAddress);
                                            ht.Add("Status", 0);
                                            ht.Add("StatusID", 9);
                                            ht.Add("ProcessID", 100);
                                            ht.Add("EntryID", PinNumber);
                                            ht.Add("ActivityDetail", "LOCK_FROM_BILLING");
                                            ht.Add("SeconderyStatus", "LOCK_FROM_BILLING");

                                            _idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                            ht.Clear();
                                            successLogBeforeProcess += "ID:" + customerId + ", M:" + netMrc + ", B:" + balance + "CED:" + ced + ", MK:" + MkStatus + " #";
                                            successLogAfterProcess += "ID:" + customerId + ", M:" + netMrc + ", B:" + balance + "CED:" + ced + ", MK:" + "false #";

                                            string dateStr = DateTime.Today.ToString("MMM dd, yyyy");
                                            smsText = "Your Internet has been locked on " + dateStr + ". Pls pay your bill to avoid disconnection. For details visit " + GetCompanyInfo() + ". Pls ignore if already paid";
                                            smsService.SendSMS(customerId, smsText, mobile);

                                            noOfCustomer++;
                                        }

                                        // --- Insert MK Error log-----//
                                        else
                                        {
                                            mkLogError += "I:" + customerId + ", Er. " + objMkConnStatusDisable.RetMessage + " #";
                                            mkErrorLogService.InsertMikrotikErrorLog(customerId, routerId, hostname, ipAddress, objMkConnStatusDisable.RetMessage, objMkConnStatusDisable.StatusCode);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        processErrorlog += "ID:" + customerId + ", Er. " + ex.Message + " #";
                        continue;
                    }
                }

                Hashtable ht1 = new Hashtable
                {
                    {"SuccessLogBeforeProcess", successLogBeforeProcess},
                    {"SuccessLogAfterProcess", successLogAfterProcess},
                    {"ProcessErrorlog", processErrorlog},
                    {"MKLogError", mkLogError},
                    {"ProcessStartTime", processStartTime},
                    {"ProcessEndTime", DateTime.Now},
                    {"NoOfCustomer", noOfCustomer},
                    {"ID", 1}
                };

                DataTable processlog = _idb.GetDataByProc(ht1, "sp_InsertProcessLog");
                var processorId = int.Parse(processlog.Rows[0]["PROCSSORLOGID"].ToString());

                // SMS 

                if (processErrorlog != "" || mkLogError != "")
                {
                    smsText = "Error!! " + " for Billing Processor id " + processorId + ", please take the neccessery steps to solve the problem.";
                    if (_mobileNo != "")
                    {
                        smsService.SendSMS(customerId, smsText, _mobileNo);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLog("BillingProcessor: " + ex.Message);
            }
        }

        void Mobile()
        {
            DataTable mob = _idb.GetDataBySQLString("SELECT MobileNo FROM CompanyInfo WHERE id=1");
            _mobileNo = mob.Rows[0]["MobileNo"].ToString();
        }
        string GetCompanyInfo()
        {
            var dtbiturl = _idb.GetDataByProc("sp_getCompanyInformation");
            var biturl = dtbiturl.Rows[0]["bitUrl"].ToString();

            return biturl;
        }
    }
}
