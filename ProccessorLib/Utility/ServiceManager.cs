using BillingERPConn;
using MkCommunication;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using tik4net;

namespace ProccessorLib
{
    public class ServiceManager
    {
        DBUtility idb = new DBUtility();

        public void GetActiveCustomerFromMikrotik()
        {

            var popUser = idb.GetDataBySQLString("Select * from POPWiseRouter where IsActive=1");
            // Console.WriteLine("Starting...");

            using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
            {
                int dlngth = 0;
                //IEnumerable<ITikReSentence> response = null;
                WriteLogFile.WriteLog("Start.......");
                idb.GetDataBySQLString(string.Format("Truncate table ActiveCustomer; Select 'mm' "));

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

                        connection.Open(Hostname, Username, Password, "");
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
                }
                WriteLogFile.WriteLog("End.......");



                // Mk real time status check
                //  string statusMk = secret.Words["disabled"].ToString();

            }

            // Console.ReadLine();

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
                        var port = Conversion.TryCastInteger(datarow["APIPort"].ToString());


                        //MK OFF,  DISCONTINUE

                        //MkConnStatus objMkStatuscheck = objMKConnection.MikrotikStatus(Hostname, Username, Password, mkVersion, ProtocolID, CustomerID, Conversion.TryCastInteger(InsType), mkUser);

                        //if (objMkStatuscheck.MikrotikStatus == 1)
                        //{
                        MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, Conversion.TryCastInteger(InsType), mkUser, port);

                        if (objMkConnStatusDisable.StatusCode == "200")
                        {
                            SuccessLogProcess += CustomerID + ", ";
                            NoofSuccessOperations++;
                        }

                        // Insert MK Error log
                        else
                        {
                            MKLogError += CustomerID + ", Er. " + objMkConnStatusDisable.RetMessage.ToString() + " #";
                        }
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
}
