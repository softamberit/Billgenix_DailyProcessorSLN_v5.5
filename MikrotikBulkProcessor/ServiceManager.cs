using BBS.Utilitys;
using BillingERPConn;
using MkCommunication;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using tik4net;

namespace MikrotikBulkProcessor
{
    public class ServiceManager
    {
        DBUtility idb;
        public ServiceManager()
        {
            idb = new DBUtility();
        }
        public void GetActiveCustomerFromMikrotik()
        {

            var popUser = idb.GetDataBySQLString("Select * from POPWiseRouter where IsActive=1");
            // Console.WriteLine("Starting...");

            using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
            {
                int dlngth = 0;
                //IEnumerable<ITikReSentence> response = null;
                WriteLogFile.WriteLog("Start.......");
                idb.GetDataBySQLString(string.Format("delete from ActiveCustomer; Select 'mm' "));

                foreach (DataRow datarow in popUser.Rows)
                {
                    var routerName = datarow["RouterName"].ToString();
                    try
                    {

                        var Username = datarow["UserName"].ToString();
                        var Password = datarow["Password"].ToString();

                        var Hostname = datarow["Host"].ToString();

                        var RouterID = datarow["Id"].ToString();
                        var mkVersion = datarow["mkVersion"].ToString();
                        // Console.WriteLine(routerName + " Connecting...");

                        connection.Open(Hostname, Username, Password, mkVersion);

                        int installType = 0;
                        // Console.WriteLine(routerName + " Connected");

                        try
                        {
                            installType = 2;
                            // Console.WriteLine(routerName + " PPPoe data collecting...");
                            var activeCustArray = connection.CreateCommandAndParameters("/ppp/secret/print", "disabled", "no").ExecuteList();
                            var activeUserList = BuildActiveUserList(activeCustArray, installType);
                            //  DBUtility idb = new DBUtility();

                            ActiveStatusSaveToDB(activeUserList, RouterID, installType);
                            // Console.WriteLine(routerName + " Inserted=>" + activeUserList.Count + "\n");
                            WriteLogFile.WriteLog(routerName + " Inserted=>" + activeUserList.Count + "\n");

                        }
                        catch (Exception ex)
                        {
                            WriteLogFile.WriteLog(ex.Message);
                        }

                        try
                        {
                            installType = 1;

                            // Console.WriteLine(routerName + " ARP data collecting...");
                            var activeCustArray = connection.CreateCommandAndParameters("ip/arp/print", "disabled", "no").ExecuteList();
                            var activeUserList = BuildActiveUserList(activeCustArray, installType);
                            //  DBUtility idb = new DBUtility();
                            // idb.GetDataBySQLString(string.Format("Delete ActiveCustomer where RouterId={0} and InstallType=1; Select 'mm' ", RouterID));
                            ActiveStatusSaveToDB(activeUserList, RouterID, installType);
                            // Console.WriteLine(routerName + " Inserted=>" + activeUserList.Count + "\n");
                            WriteLogFile.WriteLog(routerName + " Inserted=>" + activeUserList.Count + "\n");

                        }
                        catch (Exception ex)
                        {
                            WriteLogFile.WriteLog(ex.Message);
                        }
                    }
                    catch (Exception ex)
                    {

                        // Console.WriteLine(routerName + " Error: " + ex.Message);
                        WriteLogFile.WriteLog(routerName + " Error: " + ex.Message);

                    }
                    finally
                    {
                        //connection.Close();
                    }
                }
                WriteLogFile.WriteLog("End.......");



                // Mk real time status check
                //  string statusMk = secret.Words["disabled"].ToString();

            }

            // Console.ReadLine();

        }
         
        internal void MkStatusCheckingAllCustomerProcess()
        {
            string Hostname = "", Username = "", Password = "", IPAddress = "", ProtocolID = "";
            string RouterID = ""; string InsType = ""; string mkUser = ""; string mkVersion = "";
            string statusId = "";
            string statusName = "";
            string secondaryStatus = "";
            string customerId = "";

            MkConnection objMKConnection = new MkConnection();
            DataTable dtCustomerInfo = idb.GetDataByProc("GetCustomerListForMkStatusCheck");
            WriteLogFile.WriteLog("Total No. of Customer for status checking: " + dtCustomerInfo.Rows.Count);

            foreach (DataRow dr in dtCustomerInfo.Rows)
            {
                var custStatus = "";
                var disableStatus = "";
                try
                {

                    customerId = dr["CustomerId"].ToString();
                    Username = dr["RouterUserName"].ToString();
                    Password = dr["Password"].ToString();
                    Hostname = dr["Host"].ToString();
                    ProtocolID = dr["ProtocolID"].ToString();
                    InsType = dr["InsType"].ToString();
                    mkUser = dr["MkUser"].ToString();
                    mkVersion = dr["mkVersion"].ToString();
                    statusId = dr["StatusID"].ToString();
                    statusName = dr["StatusName"].ToString();
                    secondaryStatus = dr["SecondaryStatus"].ToString();
                    MkConnStatus objMkStatus = objMKConnection.MikrotikStatus(Hostname, Username, Password, mkVersion, Conversion.TryCastInteger(ProtocolID), Conversion.TryCastInteger(InsType), mkUser);
                    custStatus = objMKConnection.GetMkStatus(statusId, statusName, secondaryStatus, InsType, objMkStatus);


                    Hashtable ht = new Hashtable();
                    ht.Add("CustomerID", customerId);
                    ht.Add("Message", custStatus);
                    ht.Add("StatusId", statusId);
                    ht.Add("LogDate", DateTime.Now);
                    ht.Add("MkDisableStatus", disableStatus);
                    ht.Add("MkStatus", objMkStatus.MikrotikStatus);

                    idb.InsertData(ht, "sp_InsertCustomerMKStatus");
                    ht.Clear();
                }
                catch (Exception ex)
                {

                    // custStatus = ex.Message;
                    WriteLogFile.WriteLog(ex.Message);

                }


            }



        }

        internal void MkDisableForInactiveCustomerProcess()
        {
            string Hostname = "", Username = "", Password = "", IPAddress = "", ProtocolID = "";
            string RouterID = ""; string InsType = ""; string mkUser = ""; string mkVersion = "";
         
            string statusName = "";
            string secondaryStatus = "";
            string customerId = "";

            MkConnection objMKConnection = new MkConnection();
            DataTable dtCustomerInfo = idb.GetDataByProc("spGetUnExpectedMikrotikStatusActive");
            WriteLogFile.WriteLog("Total No. of Customer(Mikrotik Status is active): " + dtCustomerInfo.Rows.Count);

            int inactiveCount = 0;

            foreach (DataRow dr in dtCustomerInfo.Rows)
            {
                string statusId = "";
                var custStatus = "";
                var disableStatus = "";
                try
                {

                    customerId = dr["CustomerId"].ToString();
                    Username = dr["RouterUserName"].ToString();
                    Password = dr["Password"].ToString();
                    Hostname = dr["Host"].ToString();
                    ProtocolID = dr["ProtocolID"].ToString();
                    InsType = dr["InsType"].ToString();
                    mkUser = dr["MkUser"].ToString();
                    mkVersion = dr["mkVersion"].ToString();
                    //statusId = dr["StatusID"].ToString();
                    RouterID = dr["RouterID"].ToString();
                    statusName = dr["StatusName"].ToString();
                    //secondaryStatus = dr["SecondaryStatus"].ToString();


                    var custDt = idb.GetDataBySQLString(string.Format("Select CustomerId, StatusID,SecondaryStatus from CustomerMaster where CustomerId='{0}'", customerId));
                    foreach (DataRow dr1 in custDt.Rows)
                    {
                        statusId = dr1["StatusID"].ToString();
                        secondaryStatus = dr1["SecondaryStatus"].ToString();

                    }

                    MkConnStatus objMkStatus = objMKConnection.MikrotikStatus(Hostname, Username, Password, mkVersion, Conversion.TryCastInteger(ProtocolID), Conversion.TryCastInteger(InsType), mkUser);
                    custStatus = objMKConnection.GetMkStatus(statusId, statusName, secondaryStatus, InsType, objMkStatus);
                   
                    //if (statusId == "1" && secondaryStatus != "INACTIVE, LOCK_FROM_BILLING," && objMkStatus.MikrotikStatus == 0)
                    //{

                    //    try
                    //    {
                    //        MkConnStatus objMkConnStatusEnable = null;
                    //        objMkConnStatusEnable = objMKConnection.EnableMikrotik(Hostname, Username, Password, mkVersion, Conversion.TryCastInteger(ProtocolID), customerId, Conversion.TryCastInteger(InsType), mkUser);

                    //        if (objMkConnStatusEnable.StatusCode == "200")
                    //        {
                    //            disableStatus = "False";
                    //            WriteLogFile.WriteLog(string.Format("CustomerID:{0},BillgenixStatus:{1}, MikrotikStatus:{2}", customerId, statusName, "Active"));

                    //            Hashtable ht1 = new Hashtable();
                    //            ht1.Add("CustomerID", customerId);
                    //            ht1.Add("POPId", RouterID);
                    //            ht1.Add("CustomerIP", IPAddress);
                    //            ht1.Add("Status", 1);
                    //            ht1.Add("StatusID", 1);

                    //            ht1.Add("ProcessID", 209);
                    //            ht1.Add("EntryID", 10000);

                    //            new DBUtility().InsertData(ht1, "sp_insertMKlogNCustStatus");
                    //            ht1.Clear();
                    //        }

                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        disableStatus = ex.Message;
                    //        WriteLogFile.WriteLog(customerId + "==>" + ex.Message);

                    //    }


                    //}

                    if (statusId != "1" && objMkStatus.MikrotikStatus == 1)
                    {
                        try
                        {

                            MkConnStatus objMkConnStatusDisable = null;


                            objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, Conversion.TryCastInteger(ProtocolID), Conversion.TryCastInteger(InsType), mkUser);
                            if (objMkConnStatusDisable.StatusCode == "200")
                            {
                                WriteLogFile.WriteLog(string.Format("CustomerID:{0},BillgenixStatus:{1}, MikrotikStatus:{2}, MK=", customerId, statusName, "Inactive"));

                                inactiveCount++;
                                disableStatus = objMkConnStatusDisable.Status;

                                Hashtable ht1 = new Hashtable();
                                ht1.Add("CustomerID", customerId);
                                ht1.Add("POPId", RouterID);  // pop id== router Id for "Mikrotiklog" table 
                                ht1.Add("CustomerIP", IPAddress);
                                ht1.Add("Status", 0);     //  Mikrotik Inactive
                                ht1.Add("StatusID", 9);
                                ht1.Add("ProcessID", 209);
                                ht1.Add("EntryID", 10000);
                                ht1.Add("ActivityDetail", custStatus);
                                new DBUtility().InsertData(ht1, "sp_insertMKlogNCustStatus");
                                ht1.Clear();
                            }
                        }
                        catch (Exception ex)
                        {
                            disableStatus = ex.Message;
                            WriteLogFile.WriteLog(customerId + "==>" + ex.Message);

                        }


                    }
                           


                    //Hashtable ht = new Hashtable();
                    //ht.Add("CustomerID", customerId);
                    //ht.Add("Message", custStatus);
                    //ht.Add("StatusId", statusId);
                    //ht.Add("LogDate", DateTime.Now);
                    //ht.Add("MkDisableStatus", disableStatus);
                    //ht.Add("MkStatus", objMkStatus.MikrotikStatus);

                    //idb.InsertData(ht, "sp_InsertCustomerMKStatus");
                    //ht.Clear();


                }
                catch (Exception ex)
                {

                    // custStatus = ex.Message;
                    WriteLogFile.WriteLog(ex.Message);

                }


            }
            WriteLogFile.WriteLog("Total No. of Inactive customer after processed: " + inactiveCount);



        }

        public void MikrotikBulkProcessor()
        {
            try
            {
                MkConnection objMKConnection = new MkConnection();

                DataTable dtCust = idb.GetDataByProc("sp_CustomerListForMikrotikBulkProcessor");

                DateTime ProcessStartTime = DateTime.Now;
                int NoOfCustomer = dtCust.Rows.Count;
                int NoofSuccessOperations = 0;
                string CustomerID = "";
                string SuccessLogProcess = "", MKLogError = "", processError = "";
                var allCustomers = "";

                foreach (DataRow datarow in dtCust.Rows)
                {

                    try
                    {
                        CustomerID = datarow["CustomerID"].ToString();

                        allCustomers += CustomerID + ",";


                        var Username = datarow["RouterUserName"].ToString();
                        var Password = datarow["Password"].ToString();
                        var IPAddress = datarow["IPAddress"].ToString();
                        var Hostname = datarow["Host"].ToString();

                        var RouterID = datarow["RouterID"].ToString();
                        var ProtocolID = Conversion.TryCastInteger(datarow["ProtocolID"].ToString());

                        var InsType = datarow["InsType"].ToString();
                        var mkUser = datarow["MkUser"].ToString();
                        var mkVersion = datarow["mkVersion"].ToString();

                        //MK OFF,  DISCONTINUE

                        MkConnStatus objMkStatuscheck = objMKConnection.MikrotikStatus(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser);

                        if (objMkStatuscheck.MikrotikStatus == 1)
                        {
                            MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser);

                            //if (objMkConnStatusDisable.StatusCode == "200")
                            //{

                            //    idb.GetDataBySQLString(string.Format("Update CustomerMaster set statusId=1 where customerId='{0}'; Select 'ok' as feedback", CustomerID));

                            SuccessLogProcess += CustomerID + ", ";
                            NoofSuccessOperations++;
                        }

                        // Insert MK Error log
                        //else
                        //{
                        //    MKLogError += CustomerID + ", Er. " + objMkConnStatusDisable.RetMessage.ToString() + " #";
                        //}


                    }
                    catch (Exception ex)
                    {
                        processError += "ID:" + CustomerID + ", Er. " + ex.ToString() + " #";
                        //WriteLogFile.WriteLog(processError);

                    }
                }

                Hashtable ht1 = new Hashtable();

                ht1.Add("SuccessOperation", SuccessLogProcess);
                ht1.Add("TotalCustomers", NoOfCustomer);
                ht1.Add("ErrorDescription", processError);
                ht1.Add("MkError", MKLogError);
                ht1.Add("ActiveCustomers", NoofSuccessOperations);
                ht1.Add("EntryDate", DateTime.Now);
                ht1.Add("AllCustomers", allCustomers);

                idb.GetDataByProc(ht1, "sp_InsertBulkMkOperations");
            }
            catch (Exception xx)
            {
                WriteLogFile.WriteLog(xx.ToString());
            }

        }

        public void RouterAliveStatusProcessor()
        {
            var popUser = idb.GetDataBySQLString("Select * from POPWiseRouter where IsActive=1");
            // Console.WriteLine("Starting...");


            int dlngth = 0;
            //IEnumerable<ITikReSentence> response = null;
            //WriteLogFile.WriteLog("Start.......");

            List<ActiveRouter> activeRouters = new List<ActiveRouter>();
            using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
            {
                foreach (DataRow datarow in popUser.Rows)
                {
                    ActiveRouter router = new ActiveRouter();

                    try
                    {
                        var routerName = datarow["RouterName"].ToString();
                        var Username = datarow["UserName"].ToString();
                        var Password = datarow["Password"].ToString();
                        var Hostname = datarow["Host"].ToString();
                        var RouterID = datarow["Id"].ToString();
                        var mkVersion = datarow["mkVersion"].ToString();
                        router.RouterId = Convert.ToInt32(RouterID);
                        router.UserName = Username;
                        router.Password = Password;
                        router.Host = Hostname;
                        router.mkVersion = mkVersion;

                        connection.Open(Hostname, Username, Password, mkVersion);
                        router.Message = "Login/Ok";
                        router.LogTime = DateTime.Now;


                    }
                    catch (Exception ex)
                    {

                        router.Message = ex.Message;
                        router.LogTime = DateTime.Now;

                    }
                    finally
                    {
                        //connection.Close();
                    }
                    activeRouters.Add(router);
                }

                SaveRouterStatus(activeRouters);

                //WriteLogFile.WriteLog("End.......");



                // Mk real time status check
                //  string statusMk = secret.Words["disabled"].ToString();

            }

        }

        private void SaveRouterStatus(List<ActiveRouter> activeRouters)
        {
            if (activeRouters != null)
            {
                foreach (var item in activeRouters)
                {
                    Hashtable ht1 = new Hashtable();
                    ht1.Add("RouterId", item.RouterId);
                    ht1.Add("Message", item.Message);
                    ht1.Add("LogTime", item.LogTime);
                    ht1.Add("UserName", item.UserName);
                    ht1.Add("Password", item.Password);
                    ht1.Add("EntryDate", DateTime.Now);
                    ht1.Add("Host", item.Host);
                    ht1.Add("mkVersion", item.mkVersion);
                    idb.InsertData(ht1, "spActiveRouterLog");
                    ht1.Clear();
                }
            }
        }

        private void ActiveStatusSaveToDB(List<ActiveUser> activeUserList, string RouterID, int installType)
        {

            if (activeUserList != null)
            {
                foreach (var item in activeUserList)
                {
                    Hashtable ht1 = new Hashtable();

                    ht1.Add("CustomerId", item.Name);
                    ht1.Add("Service", item.Service);
                    ht1.Add("Status", item.Status);
                    ht1.Add("ID", item.Id);
                    ht1.Add("EntryDate", DateTime.Now);
                    ht1.Add("RouterID", RouterID);
                    ht1.Add("installType", installType);
                    idb.InsertData(ht1, "sp_InsertActiveCustomer");
                }
            }


        }

        private List<ActiveUser> BuildActiveUserList(IEnumerable<ITikReSentence> activeCustArray, int installType)
        {
            var list = new List<ActiveUser>();
            foreach (var item in activeCustArray)
            {
                try
                {


                    var obj = new ActiveUser();
                    obj.Id = item.Words.Where(s => s.Key == ".id").SingleOrDefault().Value;
                    if (installType == 1)
                    {
                        obj.Name = item.Words.Where(s => s.Key == "comment").SingleOrDefault().Value;

                    }
                    else
                    {
                        obj.Name = item.Words.Where(s => s.Key == "name").SingleOrDefault().Value;

                    }
                    obj.Service = item.Words.Where(s => s.Key == "service").SingleOrDefault().Value;
                    obj.Status = item.Words.Where(s => s.Key == "disabled").SingleOrDefault().Value == "false" ? 1 : 0;
                    list.Add(obj);
                }
                catch (Exception ex)
                {

                    // Console.WriteLine(ex.Message);
                }

            }


            return list;

        }
    }





    public class ActiveUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Service { get; set; }
        public int Status { get; set; }



    }

    public class ActiveRouter
    {
        public int RouterId { get; set; }
        public string RouterName { get; set; }
        public string Message { get; set; }
        public DateTime LogTime { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public string mkVersion { get; set; }




    }
}
