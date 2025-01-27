using BillingERPConn;
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

       
    }
}
