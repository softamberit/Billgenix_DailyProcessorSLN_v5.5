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
   public class CollectionProcessorService
    {
        DBUtility _idb;
        SMSService smsService;
        MikrotikErrorLogService mkErrorLogService;
        int PinNumber = 10000;
        public CollectionProcessorService()
        {
            _idb = new DBUtility();
            mkErrorLogService = new MikrotikErrorLogService(PinNumber);
            smsService = new SMSService();
        }
        public void DailyCollectionProcessor()
        {
            try
            {
                MkConnection objMKConnection = new MkConnection();

                // DataTable dtCust = new DBUtility().GetDataByProc("sp_CustomerListForCollectionProcessorV2");
                DataTable dtCust = new DBUtility().GetDataByProc("sp_CustomerListForCollectionProcessor");
                DateTime ProcessStartTime = DateTime.Now;
                int NoOfCustomer = dtCust.Rows.Count;
                WriteLogFile.WriteLog("Total Collection Found: " + NoOfCustomer);
                string MkStatus = "";

                int ProcessorID = 0;
                string CustomerID = "", SMSText = "";
                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", ProcessErrorlog = "", MKLogError = "";
                string InsType = "", mkUser = "";
                //listBox1.Items.Clear();
                foreach (DataRow Custdr in dtCust.Rows)
                {

                    try
                    {
                        CustomerID = Custdr["CustomerID"].ToString();
                        var msg = " Collection Found CID: " + CustomerID;
                        //listBox1.Items.Add(DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + msg);
                        WriteLogFile.WriteLog(msg);
                        Hashtable ht = new Hashtable();


                        ht.Add("CustomerID", CustomerID);

                        DataTable dtCustomerInfo = new DBUtility().GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();
                        foreach (DataRow dr in dtCustomerInfo.Rows)
                        {

                            if (!String.IsNullOrEmpty(dr["EndDate"].ToString()) && !String.IsNullOrEmpty(Custdr["RefDate"].ToString()))
                            {

                                decimal debit = Conversion.TryCastDecimal(dr["Debit"].ToString());
                                decimal credit = Conversion.TryCastDecimal(dr["Credit"].ToString());
                                decimal CL = Conversion.TryCastDecimal(dr["CreditLimit"].ToString());
                                decimal PV = Conversion.TryCastDecimal(dr["TotalMRC"].ToString());
                                decimal DSC = Conversion.TryCastDecimal(dr["Discount"].ToString());

                                string Refno = Custdr["RefNo"].ToString();

                                DateTime CLD = Conversion.TryCastDate(Custdr["RefDate"].ToString());
                                DateTime ED = Conversion.TryCastDate(dr["EndDate"].ToString());


                                string Hostname = "", Username = "", Password = "", IPAddress = "", Mobile = "", mkVersion = "";
                                string RouterID = ""; decimal NetMRC = 0; int ProtocolID = 0;

                                Username = dr["RouterUserName"].ToString();
                                Password = dr["Password"].ToString();
                                IPAddress = dr["IPAddress"].ToString();
                                Hostname = dr["Host"].ToString();
                                NetMRC = decimal.Parse(dr["NetMRC"].ToString());

                                RouterID = dr["RouterID"].ToString();
                                ProtocolID = int.Parse(dr["ProtocolID"].ToString());
                                Mobile = dr["Mobile"].ToString();

                                DateTime CED = DateTime.Parse(dr["EndDate"].ToString());
                                InsType = dr["InsType"].ToString();
                                mkUser = dr["MkUser"].ToString();
                                mkVersion = dr["mkVersion"].ToString();
                                //MkConnStatus OBJMkStatus = objMKConnection.MikrotikStatus(Hostname, Username, Password, mkVersion, ProtocolID, CustomerID, Conversion.TryCastInteger(InsType), mkUser);

                                //if (OBJMkStatus.MikrotikStatus == 1)
                                //{
                                //    MkStatus = "true";
                                //}
                                //else
                                //{
                                //    MkStatus = "false";
                                //}
                                MkStatus = "true";// close status checking by sobuj

                                decimal Balance = credit - debit;

                                if (CLD <= ED)
                                {

                                    decimal INV = debit;
                                    decimal MR = credit + CL;

                                    if (INV <= MR)
                                    {
                                        // MK ON 
                                        try
                                        {


                                            MkConnStatus objMkConnStatusEnable = objMKConnection.EnableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, CustomerID, Conversion.TryCastInteger(InsType), mkUser);

                                            if (objMkConnStatusEnable.StatusCode == "200")
                                            {
                                                ht.Add("CustomerID", CustomerID);
                                                ht.Add("POPId", RouterID);
                                                ht.Add("CustomerIP", IPAddress);
                                                ht.Add("Status", 1);
                                                ht.Add("ProcessID", 200);
                                                ht.Add("EntryID", PinNumber);

                                                new DBUtility().InsertData(ht, "sp_insertMKlogNCustStatus");
                                                ht.Clear();
                                                SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";
                                                SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "true #";


                                                // UPDATE BILLING MASTER IsProcessed=1
                                                DataTable ProcessUpdate = new DBUtility().GetDataBySQLString("UPDATE BillingMaster SET IsProcessed = 1 WHERE REFNO = '" + Refno + "' SELECT 'SUCCESS' AS SUCCESS");
                                                var msg1 = " Connected CID: " + CustomerID;
                                               // listBox1.Items.Add(DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + msg1);
                                                WriteLogFile.WriteLog(msg1);
                                            }

                                            // Insert Error log for Mk enable

                                            else
                                            {


                                                MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusEnable.RetMessage.ToString() + " #";
                                                mkErrorLogService.InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusEnable.RetMessage.ToString(), objMkConnStatusEnable.StatusCode.ToString());

                                                WriteLogFile.WriteLog(MKLogError);
                                            }
                                        }
                                        catch (Exception rx)
                                        {

                                            WriteLogFile.WriteLog(rx.Message);
                                        }

                                    }

                                    else if (INV > MR)
                                    {
                                        //MK OFF,  DISCONTINUE

                                        try
                                        {


                                            MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, CustomerID, Conversion.TryCastInteger(InsType), mkUser);

                                            if (objMkConnStatusDisable.StatusCode == "200")
                                            {
                                                ht.Add("CustomerID", CustomerID);
                                                ht.Add("POPId", int.Parse(RouterID));
                                                ht.Add("CustomerIP", IPAddress);
                                                ht.Add("Status", 0);
                                                ht.Add("StatusID", 9);
                                                ht.Add("ProcessID", 200);
                                                ht.Add("EntryID", PinNumber);
                                                ht.Add("ActivityDetail", "Discontinue_from_DailyProcess");
                                                ht.Add("SeconderyStatus", "LOCK_FROM_BILLING");

                                                new DBUtility().InsertData(ht, "sp_insertMKlogNCustStatus");
                                                ht.Clear();
                                                SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";
                                                SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + "false #";

                                                // SMS 
                                                SMSText = @"Sorry! Your Internet connection has been discontinued due to insufficient balance.";
                                                smsService.SendSMS(CustomerID, SMSText, Mobile);

                                                // UPDATE BILLING MASTER IsProcessed=1
                                                DataTable ProcessUpdate = new DBUtility().GetDataBySQLString("UPDATE BillingMaster SET IsProcessed = 1 WHERE REFNO = '" + Refno + "' SELECT 'SUCCESS' AS SUCCESS");
                                                var msg1 = " Disconnected CID: " + CustomerID;
                                               // listBox1.Items.Add(DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + msg1);
                                                WriteLogFile.WriteLog(msg1);
                                            }

                                            // Insert MK Error log
                                            else
                                            {


                                                MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusDisable.RetMessage.ToString() + " #";
                                                mkErrorLogService.InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusDisable.RetMessage.ToString(), objMkConnStatusDisable.StatusCode.ToString());
                                                WriteLogFile.WriteLog(MKLogError);
                                            }
                                        }
                                        catch (Exception rx)
                                        {

                                            WriteLogFile.WriteLog(rx.Message);
                                        }
                                    }
                                }

                                else if (CLD > ED)
                                {

                                    decimal INV = debit + PV - DSC;
                                    decimal MR = credit + CL;

                                    if (INV <= MR)
                                    {
                                        // INV GEN, BILL CALENDER

                                        ht.Add("CustomerID", CustomerID);
                                        ht.Add("EntryID", PinNumber);
                                        ht.Add("ProcessID", 200);
                                        ht.Add("MRCAmount", NetMRC);
                                        ht.Add("Narration", "Active_from_CollectionProcess");
                                        DataTable Insert = new DBUtility().GetDataByProc(ht, "sp_MRCInvoiceGeneration");
                                        ht.Clear();

                                        SuccessLogBeforeProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + CED + ", MK:" + MkStatus + " #";

                                        // SMS 
                                        SMSText = @"Greetings! Your current billing cycle has been started from " + DateTime.Today.ToString("dd/MM/yyyy") + " and package price " + NetMRC + ", you can access our online billing on " + getCompanyInfo() + ".";
                                        smsService.SendSMS(CustomerID, SMSText, Mobile);

                                        try
                                        {


                                            // UPDATE BILLING MASTER IsProcessed=1
                                            DataTable ProcessUpdate = new DBUtility().GetDataBySQLString("UPDATE BillingMaster SET IsProcessed = 1 WHERE REFNO = '" + Refno + "' SELECT 'SUCCESS' AS SUCCESS");

                                            //MK Enable 
                                            MkConnStatus objMkConnStatusEnable = objMKConnection.EnableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, CustomerID, Conversion.TryCastInteger(InsType), mkUser);


                                            if (objMkConnStatusEnable.StatusCode == "200")
                                            {
                                                ht.Add("CustomerID", CustomerID);
                                                ht.Add("POPId", int.Parse(RouterID));
                                                ht.Add("CustomerIP", IPAddress);
                                                ht.Add("Status", 1);
                                                ht.Add("StatusID", 1);
                                                ht.Add("ProcessID", 200);
                                                ht.Add("EntryID", PinNumber);
                                                ht.Add("ActivityDetail", "Active_from_CollectionProcess");

                                                new DBUtility().InsertData(ht, "sp_insertMKlogNCustStatus");
                                                ht.Clear();


                                                SuccessLogAfterProcess += "ID:" + CustomerID + ", M:" + NetMRC + ", B:" + Balance + "CED:" + DateTime.Today.AddDays(31) + ", MK:" + MkStatus + " #";

                                                var msg1 = " Connected CID: " + CustomerID;
                                               // listBox1.Items.Add(DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + msg1);
                                                WriteLogFile.WriteLog(msg1);

                                            }

                                            // Insert Error log for Mk enable

                                            else
                                            {
                                                MKLogError += "I:" + CustomerID + ", Er. " + objMkConnStatusEnable.RetMessage.ToString() + " #";
                                                mkErrorLogService.InsertMikrotikErrorLog(CustomerID, RouterID, Hostname, IPAddress, objMkConnStatusEnable.RetMessage.ToString(), objMkConnStatusEnable.StatusCode.ToString());
                                                WriteLogFile.WriteLog(MKLogError);

                                            }
                                        }
                                        catch (Exception rx)
                                        {

                                            WriteLogFile.WriteLog(rx.Message);
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
                        WriteLogFile.WriteLog(ProcessErrorlog);
                    }
                }

                Hashtable ht1 = new Hashtable();
                ht1.Add("SuccessLogBeforeProcess", SuccessLogBeforeProcess);
                ht1.Add("SuccessLogAfterProcess", SuccessLogAfterProcess);
                ht1.Add("ProcessErrorlog", ProcessErrorlog);
                ht1.Add("MKLogError", MKLogError);
                ht1.Add("ProcessStartTime", ProcessStartTime);
                ht1.Add("ProcessEndTime", DateTime.Now);
                ht1.Add("NoOfCustomer", NoOfCustomer);

                ht1.Add("ID", 2);

                DataTable PROCESSLOG = new DBUtility().GetDataByProc(ht1, "sp_InsertProcessLog");
                ProcessorID = int.Parse(PROCESSLOG.Rows[0]["PROCSSORLOGID"].ToString());

            }
            catch (Exception xx)
            {
                WriteLogFile.WriteLog(xx.ToString());
            }

        }
        string getCompanyInfo()
        {
            var dtbiturl = _idb.GetDataByProc("sp_getCompanyInformation");
            var biturl = dtbiturl.Rows[0]["bitUrl"].ToString();

            return biturl;
        }
    }
}
