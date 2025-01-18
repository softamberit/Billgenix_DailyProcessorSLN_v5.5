using BillingERPConn;
using MkCommunication;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProccessorLib.Processors
{
    public class CustomerEnableDisableService
    {
        DBUtility Idb;
        
        string _mobileNo;
        int PinNumber = 10000;

        public CustomerEnableDisableService()
        {
            Idb = new DBUtility();
            
        }

        private void Customer_ON()
        {
            try
            {
                MkConnection objMkConnection = new MkConnection();

                DataTable dtCust = Idb.GetDataByProc("sp_getCustomerBulkON");

                DateTime processStartTime = DateTime.Now;
                int noOfCustomer = dtCust.Rows.Count;

                string customerId = "";
                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", processErrorlog = "", mkLogError = "";
                string InsType = "", mkUser = "", mkVersion = "";

                foreach (DataRow dr in dtCust.Rows)
                {

                    try
                    {
                        customerId = dr["CustomerID"].ToString();
                        Hashtable ht = new Hashtable { { "CustomerID", customerId } };



                        DataTable dtCustomerInfo = Idb.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();
                        foreach (DataRow datarow in dtCustomerInfo.Rows)
                        {
                            var username = datarow["RouterUserName"].ToString();
                            var password = datarow["Password"].ToString();
                            var ipAddress = datarow["IPAddress"].ToString();
                            var hostname = datarow["Host"].ToString();

                            var routerId = datarow["RouterID"].ToString();
                            var protocolId = int.Parse(datarow["ProtocolID"].ToString());
                            InsType = datarow["InsType"].ToString();
                            mkUser = datarow["MkUser"].ToString();

                            mkVersion = datarow["mkVersion"].ToString();

                            // MK ON 

                            MkConnStatus objMkConnStatusEnable = objMkConnection.EnableMikrotik(hostname, username, password, mkVersion, protocolId, customerId, Conversion.TryCastInteger(InsType), mkUser);

                            if (objMkConnStatusEnable.StatusCode == "200")
                            {

                                WriteLogFile.WriteLog(customerId + "Customer ON");
                            }

                            // Insert Error log for Mk Enable

                            else
                            {
                                mkLogError += "I:" + customerId + ", Er. " + objMkConnStatusEnable.RetMessage + " #";
                                InsertMikrotikErrorLog(customerId, routerId, ipAddress, objMkConnStatusEnable.RetMessage, objMkConnStatusEnable.StatusCode);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        processErrorlog += "ID:" + customerId + ", Er. " + ex + " #";
                        continue;
                    }
                }

                Hashtable ht1 = new Hashtable
                {
                    {"SuccessLogBeforeProcess", SuccessLogBeforeProcess},
                    {"SuccessLogAfterProcess", SuccessLogAfterProcess},
                    {"ProcessErrorlog", processErrorlog},
                    {"MKLogError", mkLogError},
                    {"ProcessStartTime", processStartTime},
                    {"ProcessEndTime", DateTime.Now},
                    {"NoOfCustomer", noOfCustomer},
                    {"ID", 2}
                };

                Idb.GetDataByProc(ht1, "sp_InsertProcessLog");

            }
            catch (Exception xx)
            {
                WriteLogFile.WriteLog(xx.ToString());
            }

        }
        private void Customer_OFF()
        {
            try
            {
                MkConnection objMkConnection = new MkConnection();

                DataTable dtCust = Idb.GetDataByProc("sp_getCustomerBulkOFF");

                DateTime processStartTime = DateTime.Now;
                int noOfCustomer = dtCust.Rows.Count;

                string customerId = "";
                string SuccessLogBeforeProcess = "", SuccessLogAfterProcess = "", processErrorlog = "", mkLogError = "";
                string InsType = "", mkUser = "", mkVersion = "";
                foreach (DataRow dr in dtCust.Rows)
                {

                    try
                    {
                        customerId = dr["CustomerID"].ToString();
                        Hashtable ht = new Hashtable { { "CustomerID", customerId } };



                        DataTable dtCustomerInfo = Idb.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
                        ht.Clear();
                        foreach (DataRow datarow in dtCustomerInfo.Rows)
                        {
                            var username = datarow["RouterUserName"].ToString();
                            var password = datarow["Password"].ToString();
                            var ipAddress = datarow["IPAddress"].ToString();
                            var hostname = datarow["Host"].ToString();

                            var routerId = datarow["RouterID"].ToString();
                            var protocolId = int.Parse(datarow["ProtocolID"].ToString());

                            InsType = datarow["InsType"].ToString();
                            mkUser = datarow["MkUser"].ToString();

                            mkVersion = datarow["mkVersion"].ToString();
                            //MK OFF,  DISCONTINUE

                            MkConnStatus objMkConnStatusDisable = objMkConnection.DisableMikrotik(hostname, username, mkVersion, password, protocolId, customerId, Conversion.TryCastInteger(InsType), mkUser);

                            if (objMkConnStatusDisable.StatusCode == "200")
                            {

                                WriteLogFile.WriteLog(customerId + "Customer OFF");
                            }

                            // Insert MK Error log
                            else
                            {
                                mkLogError += "I:" + customerId + ", Er. " + objMkConnStatusDisable.RetMessage + " #";
                                InsertMikrotikErrorLog(customerId, routerId, ipAddress, objMkConnStatusDisable.RetMessage, objMkConnStatusDisable.StatusCode);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        processErrorlog += "ID:" + customerId + ", Er. " + ex + " #";
                        continue;
                    }
                }

                Hashtable ht1 = new Hashtable
                {
                    {"SuccessLogBeforeProcess", SuccessLogBeforeProcess},
                    {"SuccessLogAfterProcess", SuccessLogAfterProcess},
                    {"ProcessErrorlog", processErrorlog},
                    {"MKLogError", mkLogError},
                    {"ProcessStartTime", processStartTime},
                    {"ProcessEndTime", DateTime.Now},
                    {"NoOfCustomer", noOfCustomer},
                    {"ID", 2}
                };


                Idb.GetDataByProc(ht1, "sp_InsertProcessLog");

            }
            catch (Exception xx)
            {
                WriteLogFile.WriteLog(xx.ToString());
            }

        }


        #region InsertMikrotikErrorLog

        private void InsertMikrotikErrorLog(string customerId, string routerId, string ipAddress, string retMessage, string statuscode)
        {

            Hashtable ht = new Hashtable
            {
                {"CustomerID", customerId},
                {"POPId", routerId},
                {"CustomerIP", ipAddress},
                {"Error_description", retMessage},
                {"ProcessID", 200},
                {"EntryID", PinNumber},
                {"StatusCode", statuscode}
            };


            //ht.Add("IPAddress", IPAddress);

            Idb.InsertData(ht, "sp_insertMKCommunication_Errorlog");
        }


        #endregion
    }
}
