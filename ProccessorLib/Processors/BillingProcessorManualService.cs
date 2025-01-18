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
    public class BillingProcessorManualService
    {
        DBUtility idb;
        SMSService smsService;
        MikrotikErrorLogService mkErrorLogService;
        string _mobileNo;
        int PinNumber = 10000;

        public BillingProcessorManualService()
        {
            idb = new DBUtility();
            smsService = new SMSService();
            mkErrorLogService = new MikrotikErrorLogService(PinNumber);
        }
        public void DailyBillingProcessor()
        {
            try
            {

                MkConnection objMKConnection = new MkConnection();

                DataTable dtCust = idb.GetDataBySQLString("SELECT [CustomerID] FROM test");

                DateTime CD = DateTime.Today;
                DateTime ProcessStartTime = DateTime.Now;
                int NoOfCustomer = 0;

                string MkStatus = "";

                int ProcessorID = 0;
                string CustomerID = "", SMSText = "";
                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", ProcessErrorlog = "", MKLogError = "";
                string InsType = "", mkUser = "";
                foreach (DataRow dr in dtCust.Rows)
                {
                    try
                    {
                        Hashtable ht = new Hashtable();
                        CustomerID = dr["CustomerID"].ToString();

                        ht.Add("CustomerID", CustomerID);
                        DataTable dtCustomerInfo = idb.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();
                        foreach (DataRow datarow in dtCustomerInfo.Rows)
                        {
                            if (!String.IsNullOrEmpty(datarow["EndDate"].ToString()))
                            {
                                decimal debit = Conversion.TryCastDecimal(datarow["Debit"].ToString());
                                decimal credit = Conversion.TryCastDecimal(datarow["Credit"].ToString());
                                decimal CL = Conversion.TryCastDecimal(datarow["CreditLimit"].ToString());
                                decimal PV = Conversion.TryCastDecimal(datarow["TotalMRC"].ToString());
                                decimal DSC = Conversion.TryCastDecimal(datarow["Discount"].ToString());
                                DateTime ED = Conversion.TryCastDate(datarow["EndDate"].ToString());

                                string Hostname = "", Username = "", Password = "", IPAddress = "", Mobile = "";
                                decimal NetMRC = 0; string RouterID = ""; int ProtocolID = 0; string mkVersion = "";

                                Username = datarow["RouterUserName"].ToString();
                                Password = datarow["Password"].ToString();
                                IPAddress = datarow["IPAddress"].ToString();
                                Hostname = datarow["Host"].ToString();
                                NetMRC = decimal.Parse(datarow["NetMRC"].ToString());
                                RouterID = (datarow["RouterID"].ToString());
                                ProtocolID = int.Parse(datarow["ProtocolID"].ToString());
                                Mobile = datarow["Mobile"].ToString();
                                decimal Balance = credit - debit;
                                DateTime CED = DateTime.Parse(datarow["EndDate"].ToString());
                                InsType = datarow["InsType"].ToString();
                                mkUser = datarow["MkUser"].ToString();
                                mkVersion = datarow["mkVersion"].ToString();

                                if (CD > ED)
                                {
                                    decimal INV = debit + PV - DSC;
                                    decimal CA = credit + CL;

                                    if (INV > CA)
                                    {
                                        // MK OFF, DIscontinue 

                                        MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, CustomerID, Conversion.TryCastInteger(InsType), mkUser);

                                        if (objMkConnStatusDisable.StatusCode == "200")
                                        {
                                            ht.Add("CustomerID", CustomerID);
                                            ht.Add("POPId", RouterID);
                                            ht.Add("CustomerIP", IPAddress);
                                            ht.Add("Status", 0);
                                            ht.Add("StatusID", 9);
                                            ht.Add("ProcessID", 100);
                                            ht.Add("EntryID", PinNumber);
                                            ht.Add("ActivityDetail", "LOCK_FROM_BILLING");
                                            ht.Add("SeconderyStatus", "LOCK_FROM_BILLING");

                                            idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                            ht.Clear();

                                            SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";
                                            SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "false #";

                                            // SMS 
                                            string dateStr = DateTime.Today.ToString("MMM dd, yyyy");
                                            SMSText = "Your Internet has been locked on " + dateStr + ".Pls pay your bill to avoid disconnection. For details visit " + getCompanyInfo() + ". Pls ignore if already paid";
                                            smsService.SendSMS(CustomerID, SMSText, Mobile);


                                            NoOfCustomer++;
                                        }

                                        // Insert MK Error log
                                        else
                                        {
                                            MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusDisable.RetMessage.ToString() + " #";
                                            mkErrorLogService.InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusDisable.RetMessage.ToString(), objMkConnStatusDisable.StatusCode.ToString());
                                        }

                                    }
                                    else if (INV <= CA)
                                    {
                                        // MK ON, INV GEN

                                        // -----  Bill generation ----------//

                                        ht.Add("CustomerID", CustomerID);
                                        ht.Add("EntryID", PinNumber);
                                        ht.Add("ProcessID", 100);
                                        ht.Add("MRCAmount", NetMRC);
                                        ht.Add("ActivityDetail", "ACTIVE_FROM_BILLING");

                                        DataTable Insert = idb.GetDataByProc(ht, "sp_BillGeneDuringDailyProcess");
                                        ht.Clear();
                                        SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";

                                        // SMS 
                                        string dateStr = DateTime.Today.ToString("MMM dd, yyyy");
                                        SMSText = "Your current billing cycle has been started from " + dateStr + " for MRC " + NetMRC + " .You can access our online billing on " + getCompanyInfo() + ".";
                                        smsService.SendSMS(CustomerID, SMSText, Mobile);
                                        NoOfCustomer++;


                                        //  Mikrotik Enable Check 
                                        MkConnStatus objMkConnStatusEnable = objMKConnection.EnableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, CustomerID, Conversion.TryCastInteger(InsType), mkUser);


                                        if (objMkConnStatusEnable.StatusCode == "200")
                                        {

                                            ht.Add("CustomerID", CustomerID);
                                            ht.Add("POPId", RouterID);
                                            ht.Add("CustomerIP", IPAddress);
                                            ht.Add("Status", 1);
                                            ht.Add("StatusId", 1);

                                            ht.Add("ProcessID", 100);
                                            ht.Add("EntryID", PinNumber);

                                            idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                            ht.Clear();

                                            SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + DateTime.Today.AddDays(31) + ", MK:" + "true #";

                                        }

                                        // --- Insert MK Error log for Enabling IP-----//

                                        else
                                        {
                                            MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusEnable.RetMessage.ToString() + " #";
                                            mkErrorLogService.InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusEnable.RetMessage.ToString(), objMkConnStatusEnable.StatusCode.ToString());
                                        }
                                    }

                                }

                                else if (CD <= ED)
                                {
                                    decimal INV = debit;
                                    decimal MR = credit + CL;

                                    if (INV <= MR)
                                    {
                                        // MK ON   NO NEED TO CODE??

                                    }
                                    else if (INV > MR)
                                    {
                                        //MK OFF Discontinue

                                        MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, CustomerID, Conversion.TryCastInteger(InsType), mkUser);

                                        if (objMkConnStatusDisable.StatusCode == "200")
                                        {
                                            ht.Add("CustomerID", CustomerID);
                                            ht.Add("POPId", RouterID);
                                            ht.Add("CustomerIP", IPAddress);
                                            ht.Add("Status", 0);
                                            ht.Add("StatusID", 9);
                                            ht.Add("ProcessID", 100);
                                            ht.Add("EntryID", PinNumber);
                                            ht.Add("ActivityDetail", "LOCK_FROM_BILLING");
                                            ht.Add("SeconderyStatus", "LOCK_FROM_BILLING");

                                            idb.InsertData(ht, "sp_insertMKlogNCustStatus");
                                            ht.Clear();
                                            SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";
                                            SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "false #";

                                            string dateStr = DateTime.Today.ToString("MMM dd, yyyy");
                                            SMSText = "Your Internet has been locked on " + dateStr + ". Pls pay your bill to avoid disconnection. For details visit " + getCompanyInfo() + ". Pls ignore if already paid";
                                            smsService.SendSMS(CustomerID, SMSText, Mobile);

                                            NoOfCustomer++;
                                        }

                                        // --- Insert MK Error log-----//
                                        else
                                        {
                                            MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusDisable.RetMessage.ToString() + " #";
                                            mkErrorLogService.InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusDisable.RetMessage.ToString(), objMkConnStatusDisable.StatusCode.ToString());
                                        }
                                    }
                                }
                            }

                            else
                            {
                                // DO something
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ProcessErrorlog += "ID:" + CustomerID + ", Er. " + ex.ToString() + " #";
                        continue;
                    }
                }

                Hashtable HT = new Hashtable();

                HT.Add("SuccessLogBeforeProcess", SuccessLogBeforeProcess);
                HT.Add("SuccessLogAfterProcess", SuccessLogAfterProcess);
                HT.Add("ProcessErrorlog", ProcessErrorlog);
                HT.Add("MKLogError", MKLogError);
                HT.Add("ProcessStartTime", ProcessStartTime);
                HT.Add("ProcessEndTime", DateTime.Now);
                HT.Add("NoOfCustomer", NoOfCustomer);
                HT.Add("ID", 1);


                DataTable PROCESSLOG = idb.GetDataByProc(HT, "sp_InsertProcessLog");
                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

                // SMS 

                if (ProcessErrorlog != "" || MKLogError != "")
                {
                    SMSText = "Error!! " + " for Billing Processor id " + ProcessorID + ", please take the neccessery steps to solve the problem.";
                    if (_mobileNo != "")
                    {
                        smsService.SendSMS(CustomerID, SMSText, _mobileNo);
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
            DataTable mob = idb.GetDataBySQLString("SELECT MobileNo FROM CompanyInfo WHERE id=1");
            _mobileNo = mob.Rows[0]["MobileNo"].ToString();
        }
        string getCompanyInfo()
        {
            var dtbiturl = idb.GetDataByProc("sp_getCompanyInformation");
            var biturl = dtbiturl.Rows[0]["bitUrl"].ToString();

            return biturl;
        }
    }
}
