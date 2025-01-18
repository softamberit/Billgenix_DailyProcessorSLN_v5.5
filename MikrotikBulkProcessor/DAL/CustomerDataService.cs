using BBS.Utilitys;
using BillingERPConn;
using DocumentFormat.OpenXml.Office2010.Excel;
using MikrotikBulkProcessor.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MikrotikBulkProcessor.DAL
{
    public class CustomerDataService
    {

        public CustomerDetails GetCustomerDetailsByCID(string cid)
        {

            var custId = Conversion.CustIdMaking(cid);

            DBUtility objDBUitility = new DBUtility();

            var ht = new Hashtable();

            ht.Add("CustomerID", custId);
            var dtCustomerInfo = objDBUitility.GetDataByProc(ht, "sp_getAllDetailForCustomer");
            ht.Clear();

            // string Hostname = "", Username = "", Password = "", IPAddress = "", ProtocolID = "";
            //string NetMRC = "", TotalOTC = "", RouterID = ""; string InsType = ""; string mkUser = ""; string mkVersion = "";
            if (dtCustomerInfo != null)
            {
                if (dtCustomerInfo.Rows[0]["Feedback"].ToString() != "Found")
                {
                    return null;
                }

            }
            else
                return null;

            CustomerDetails customer = new CustomerDetails();
            foreach (DataRow dr in dtCustomerInfo.Rows)
            {
                customer.CustomerId = custId;
                customer.Username = dr["RouterUserName"].ToString();
                customer.Password = dr["Password"].ToString();
                customer.IPAddress = dr["IPAddress"].ToString();
                customer.Host = dr["Host"].ToString();
                customer.NetMRC = dr["NetMRC"].ToString();
                customer.TotalOTC = dr["TotalOTC"].ToString();
                customer.RouterID = dr["RouterID"].ToString();
                customer.ProtocolID = Convert.ToInt32(string.IsNullOrEmpty(dr["ProtocolID"].ToString()) ? "0" : dr["ProtocolID"]);
                customer.InsType = Convert.ToInt32(string.IsNullOrEmpty(dr["InsType"].ToString()) ? "0" : dr["InsType"]);
                customer.MkUser = dr["MkUser"].ToString();
                customer.mkVersion = dr["mkVersion"].ToString();
                customer.StatusId = dr["Status"].ToString();
                customer.SecondaryStatus = dr["SecondaryStatus2"].ToString();
                customer.StatusName = dr["StatusName"].ToString();


            }
            return customer;
        }

        internal void SaveCustomerMkStatus(List<ActivePPPUser> customers)
        {
            try
            {


                string connectionString = DBUtility.GetConnectionString();
                string tableName = "PopwiseMikrotikStatus";

                // Create a DataTable with the same structure as your destination table
                DataTable dataTable = CreateDataTable(customers);

                // Add rows to the DataTable (populate your data)

                // Perform bulk insert using SqlBulkCopy
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.DestinationTableName = tableName;
                        //               ,[CustomerId]

                        //,[InstallType]
                        //,[EntryDate]
                        //,[MkCID]
                        //,[CallerId]
                        //,[last_caller_id]
                        //,[MKUser]
                        //,[IPAddress]
                        //,[RemoteIP]
                        //,[last_logged_out]
                        //,[Comment]
                        //,[MkID]
                        //,[StatusId]
                        // Map columns in the source DataTable to columns in the destination table
                        bulkCopy.ColumnMappings.Add("Name", "CustomerId");
                        bulkCopy.ColumnMappings.Add("MkStatus", "MkStatus");
                        bulkCopy.ColumnMappings.Add("Service", "InstallType");
                        bulkCopy.ColumnMappings.Add("Id", "MkID");
                        bulkCopy.ColumnMappings.Add("PopId", "PopId");
                        bulkCopy.ColumnMappings.Add("RouterId", "RouterId");
                        bulkCopy.ColumnMappings.Add("CallerId", "CallerId");
                        bulkCopy.ColumnMappings.Add("RemoteIP", "RemoteIP");
                        bulkCopy.ColumnMappings.Add("IPAddress", "IPAddress");
                        bulkCopy.ColumnMappings.Add("Comment", "Comment");
                        bulkCopy.ColumnMappings.Add("last_logged_out", "last_logged_out");
                        bulkCopy.ColumnMappings.Add("last_caller_id", "last_caller_id");
                        bulkCopy.ColumnMappings.Add("logDate", "EntryDate");
                        bulkCopy.ColumnMappings.Add("StatusId", "StatusId");




                        // Set other options if needed, such as BatchSize, NotifyAfter, etc.
                        bulkCopy.BatchSize = 5000;

                        // Perform the bulk copy
                        bulkCopy.WriteToServer(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {


            }
            // Console.WriteLine("Bulk insert completed.");
        }

        // Helper method to create a DataTable with the same structure as your destination table
        static DataTable CreateDataTable(List<ActivePPPUser> list)
        {
            //DataTable dataTable = new DataTable();

            //// Add columns to match the structure of your destination table
            //dataTable.Columns.Add("Column1", typeof(int));
            //dataTable.Columns.Add("Column2", typeof(string));
            // Add more columns as needed
            var json = JsonConvert.SerializeObject(list);
            var dataTable = JsonConvert.DeserializeObject<DataTable>(json);

            return dataTable;
        }

        internal void SoftwareStatusProcess(int routerId)
        {
            DBUtility objDBUitility = new DBUtility();
            var ht = new Hashtable();
            ht.Add("routerId", routerId);
            var dtCustomerInfo = objDBUitility.GetDataByProc(ht, "sp_ProcessPopwiseMikrotikStatus");
            ht.Clear();
        }

        internal DataTable GetPopWiseRouter()
        {
            DBUtility objDBUitility = new DBUtility();

            Hashtable ht = new Hashtable { { "POPID", 0 } };
            DataTable dt = objDBUitility.GetDataByProc(ht, "GetPopWiseRouterByPopID");
            return dt;
        }

        internal void deleteTodayData()
        {
            DBUtility objDBUitility = new DBUtility();
            Hashtable ht1 = new Hashtable()
                { { "POPId", 0} };
            objDBUitility.GetDataByProc(ht1, "sp_DeletePopwiseMikrotikStatus");
            ht1.Clear();
        }

        internal DataTable GetTodatMismatchCustomer()
        {
            DBUtility objDBUitility = new DBUtility();

            Hashtable ht = new Hashtable { { "POPId", 0 } };
            DataTable dt = objDBUitility.GetDataByProc(ht, "sp_GetPopWiseMikrotikStatus");
            return dt;
        }

        internal void MikrotikStatus(CustomerDetails custDtl)
        {
            Hashtable ht1 = new Hashtable();
            ht1.Add("CustomerID", custDtl.CustomerId);
            ht1.Add("POPId", custDtl.RouterID);  // pop id== router Id for "Mikrotiklog" table 
            ht1.Add("CustomerIP", custDtl.IPAddress);
            ht1.Add("Status", 0);     //  Mikrotik Inactive
            ht1.Add("StatusID", 9);
            ht1.Add("ProcessID", 209);
            ht1.Add("EntryID", 10000);
            ht1.Add("ActivityDetail", "Active to Inactive");
            new DBUtility().InsertData(ht1, "sp_insertMKlogNCustStatus");
            ht1.Clear();
        }
    }
}
