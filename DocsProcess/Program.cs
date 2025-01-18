using BillingERPConn;
using MkCommunication;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CustomerDocumentProcess
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {

                //DriveInfo cDrive = new DriveInfo("C");
                // DirectoryInfo dd = new DirectoryInfo("D:\\test");
                var folder = "E:\\CustomerImage";

                DBUtility dBUtility = new DBUtility();

                DataTable dt = new DataTable();
                dt.Columns.Add("CustomerId", typeof(string));
                dt.Columns.Add("DocsName", typeof(string));
                dt.Columns.Add("DocsType", typeof(int));



                var docs = dBUtility.GetDataBySQLString("Select CustomerID, DocName, DocTypeID from CustomerDocs Where DocName not like '%.%'  and COnvert(date,EntryDate )<'01/01/2021'");

                foreach (DataRow s in docs.Rows)
                {
                    try
                    {
                        var fileName = s["DocName"].ToString();
                        var CID = s["CustomerID"].ToString();
                        var doctype = Convert.ToInt32(s["DocTypeID"].ToString());

                        SearchOption searchOption = SearchOption.TopDirectoryOnly;
                        Regex reSearchPattern = new Regex(fileName + @"\a*", RegexOptions.IgnoreCase);
                        var files = Directory.EnumerateFiles(folder, "*", searchOption).Where(file => reSearchPattern.IsMatch(file));
                        foreach (var item in files)
                        {
                            var oFile = new FileInfo(item);
                            dt.Rows.Add(CID, oFile.Name, doctype);


                            WriteLogFile.WriteLog(CID + "  " + oFile.Name + " " + doctype);
                            Console.WriteLine(CID + "  " + oFile.Name + " " + doctype); ;

                        }
                    }
                    catch (Exception x)
                    {

                        continue;
                    }
                   
                }

                Hashtable ht = new Hashtable()
                {
                    { "TempTable", dt }
                };

                var Insert = dBUtility.GetDataByProc(ht, "InsertCustomerDocs_temp");
                WriteLogFile.WriteLog(Insert.Rows[0]["feedback"].ToString());
                Console.WriteLine(Insert.Rows[0]["feedback"].ToString()); ;
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                WriteLogFile.WriteLog(ex.Message);
                Console.WriteLine(ex.Message); ;
            }


            Console.ReadLine();
            
        }

        public void test()
        {
            DBUtility dBUtility = new DBUtility();

            // var docs = dBUtility.GetDataBySQLString("Select CustomerID from CustomerMaster Where CustomerID = 'CID-13389'");


            Hashtable ht = new Hashtable();

            var CustomerID = "51037";

            ht.Add("CustomerID", CustomerID);

            DataTable dtCustomerInfo = dBUtility.GetDataByProc(ht, "sp_getCustInfoforBillingProcessor");
            ht.Clear();
            foreach (DataRow datarow in dtCustomerInfo.Rows)
            {

                string Hostname = "", Username = "", Password = "", IPAddress = "", Mobile = "", mkVersion = "";
                string RouterID = ""; decimal NetMRC = 0; int ProtocolID = 0;

                Username = datarow["RouterUserName"].ToString();
                Password = datarow["Password"].ToString();
                IPAddress = datarow["IPAddress"].ToString();
                Hostname = datarow["Host"].ToString();
                NetMRC = decimal.Parse(datarow["NetMRC"].ToString());

                RouterID = datarow["RouterID"].ToString();
                ProtocolID = int.Parse(datarow["ProtocolID"].ToString());
                Mobile = datarow["Mobile"].ToString();

                DateTime CED = DateTime.Parse(datarow["EndDate"].ToString());
                var InsType = datarow["InsType"].ToString();
                var mkUser = datarow["MkUser"].ToString();
                mkVersion = datarow["mkVersion"].ToString();

                MkConnection objMKConnection = new MkConnection();

                MkConnStatus objMkConnStatusDisable = objMKConnection.DisableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, CustomerID, Convert.ToInt32(InsType), mkUser);
                Console.WriteLine(objMkConnStatusDisable.RetMessage);

                MkConnStatus objMkConnStatusEnable = objMKConnection.EnableMikrotik(Hostname, Username, Password, mkVersion, ProtocolID, CustomerID, Convert.ToInt32(InsType), mkUser);

                Console.WriteLine(objMkConnStatusEnable.RetMessage);

            }
        }
    }
}
