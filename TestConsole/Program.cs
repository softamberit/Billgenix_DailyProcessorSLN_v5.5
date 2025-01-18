using BillingERPConn;
using HtmlAgilityPack;
using MkCommunication;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            // ProccessorLib.BackgroundServiceManager _manager = new ProccessorLib.BackgroundServiceManager();
            //_manager.ProcessorHandler();


            //DBUtility idb = new DBUtility();

            // Hashtable ht1 = new Hashtable();


            // ht1.Add("Name", @"India'; EXEC sp_MSforeachtable @command1 = 'DROP TABLE ApprovalHistory ' --");
            // var data= idb.GetDataByProc(ht1, "Usp_GetCountry");


            //CreateUser();

            //var sb = new StringBuilder();

            //sb.Append("User Created Successfully");

            //Console.WriteLine(sb.ToString());
            //Console.ReadLine();

            WebScraping();

        }

        private static void WebScraping()
        {

            //var baseAddress = new Uri("https://www.bkashcluster.com:9081/mr_portal/");
            //var cookieContainer = new CookieContainer();

            //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://wttv.click-tt.de/");
            //request.CookieContainer = cookieContainer;
            ////set the user agent and accept header values, to simulate a real web browser
            //request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36";
            //request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            //request.ServicePoint.Expect100Continue = true;
            

            //Console.WriteLine("FIRST RESPONSE");
            //Console.WriteLine();
            //using (WebResponse response = request.GetResponse())
            //{
            //    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            //    {
            //        Console.WriteLine(sr.ReadToEnd());
            //    }
            //}

            //request = (HttpWebRequest)HttpWebRequest.Create("https://wttv.click-tt.de/cgi-bin/WebObjects/nuLigaTTDE.woa/wa/teamPortrait?teamtable=1673669&pageState=rueckrunde&championship=SK+Bez.+BB+13%2F14&group=204559");
            ////set the cookie container object
            //request.CookieContainer = cookieContainer;
            //request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36";
            //request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";

            ////set method POST and content type application/x-www-form-urlencoded
            //request.Method = "POST";
            //request.ContentType = "application/x-www-form-urlencoded";

            ////insert your username and password
            //string data = string.Format("username={0}&password={1}", "username", "password");
            //byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);

            //request.ContentLength = bytes.Length;

            //using (Stream dataStream = request.GetRequestStream())
            //{
            //    dataStream.Write(bytes, 0, bytes.Length);
            //    dataStream.Close();
            //}

            //Console.WriteLine("LOGIN RESPONSE");
            //Console.WriteLine();
            //using (WebResponse response = request.GetResponse())
            //{
            //    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            //    {
            //        Console.WriteLine(sr.ReadToEnd());
            //    }
            //}



        }

        private static void CreateUser()
        {
            MkConnection connection = new MkConnection();
            connection.CreateUser("192.168.10.1", "sobuj","12345", "Post_6.44","CID2312","123451","192.168.10.5");
        }

        public void test()
        {
            DBUtility idb = new DBUtility();

            var popUser = idb.GetDataBySQLString("Select * from POPWiseRouter where IsActive=1");
            Console.WriteLine("Starting...");

            using (tik4net.ITikConnection connection = tik4net.ConnectionFactory.CreateConnection(tik4net.TikConnectionType.Api))
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
                        Console.WriteLine(routerName + " Connecting...");

                        connection.Open(Hostname, Username, Password, mkVersion);
                        int installType = 0;
                        Console.WriteLine(routerName + " Connected");


                        try
                        {
                            installType = 2;
                            Console.WriteLine(routerName + " PPPoe data collecting...");
                            var activeCustArray = connection.CreateCommandAndParameters("/ppp/secret/print", "disabled", "no").ExecuteList();
                            var activeUserList = BuildActiveUserList(activeCustArray, installType);
                            //  DBUtility idb = new DBUtility();

                            ActiveStatusSaveToDB(activeUserList, RouterID, installType);
                            Console.WriteLine(routerName + " Inserted=>" + activeUserList.Count + "\n");
                            WriteLogFile.WriteLog(routerName + " Inserted=>" + activeUserList.Count + "\n");

                        }
                        catch (Exception ex)
                        {

                        }

                        try
                        {
                            installType = 1;

                            Console.WriteLine(routerName + " ARP data collecting...");
                            var activeCustArray = connection.CreateCommandAndParameters("ip/arp/print", "disabled", "no").ExecuteList();
                            var activeUserList = BuildActiveUserList(activeCustArray, installType);
                            //  DBUtility idb = new DBUtility();
                            // idb.GetDataBySQLString(string.Format("Delete ActiveCustomer where RouterId={0} and InstallType=1; Select 'mm' ", RouterID));
                            ActiveStatusSaveToDB(activeUserList, RouterID, installType);
                            Console.WriteLine(routerName + " Inserted=>" + activeUserList.Count + "\n");
                            WriteLogFile.WriteLog(routerName + " Inserted=>" + activeUserList.Count + "\n");

                        }
                        catch (Exception ex)
                        {

                        }



                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(routerName + " Error: " + ex.Message);
                        WriteLogFile.WriteLog(routerName + " Error: " + ex.Message);

                    }
                }
                WriteLogFile.WriteLog("End.......");



                // Mk real time status check
                //  string statusMk = secret.Words["disabled"].ToString();

            }

            Console.ReadLine();
        }

        private static void ActiveStatusSaveToDB(List<ActiveUser> activeUserList, string RouterID, int installType)
        {
            DBUtility idb = new DBUtility();
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

        private static List<ActiveUser> BuildActiveUserList(IEnumerable<tik4net.ITikReSentence> activeCustArray, int installType)
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

                    Console.WriteLine(ex.Message);
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
