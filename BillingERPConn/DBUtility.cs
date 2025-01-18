using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Configuration;

namespace BillingERPConn
{
    public class DBUtility
    {

        public static string ConnectionString="";
        private SqlTransaction tran;
        private SqlConnection con = null;
        private SqlCommand cmd = null;
        public DBUtility()
        {
            ConnectionString = GetConnectionString("DbBillingERPConnectionString");
            con = new SqlConnection(ConnectionString);
            cmd = new SqlCommand();
        }
        public DBUtility(string conStrName)
        {
            ConnectionString = GetConnectionString(conStrName);
            con = new SqlConnection(ConnectionString);
            cmd = new SqlCommand();
        }

        public static string GetConnectionString()
        {
            return GetConnectionString("DbBillingERPConnectionString");
        }
        public static string GetConnectionStringForReport()
        {
            return GetConnectionString("ReportBilling.Properties.Settings.DbBillingERP");
        }
        private static string GetConnectionString(string conStrName)
        {
            bool.TryParse(ConfigurationManager.AppSettings["PwdEncryptable"], out bool isPwdEncryptable);
            var conStr = ConfigurationManager.ConnectionStrings[conStrName].ConnectionString;

            if (isPwdEncryptable)
            {

                try
                {
                    var builder = new SqlConnectionStringBuilder(conStr);

                    var pwd = EncryptDecryptHelper.Decrypt(builder.Password);
                    builder.Password = pwd;
                    return builder.ConnectionString;

                }
                catch (Exception ex)
                {
                    throw ex;


                }
            }

            return conStr;
        }

      

        public long InsertData(Hashtable htable, string procedureName)
        {
           
            long retID = 0;
            try
            {
                if (con.State != ConnectionState.Open)
                {
                   // con = new SqlConnection(ConnectionString);
                    con.Open();
                }
                cmd.Connection = con;
                cmd.CommandText = procedureName;
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (object OBJ in htable.Keys)
                {
                    string COLUMN_NAME = Convert.ToString(OBJ);
                    SqlParameter param = new SqlParameter("@" + COLUMN_NAME, htable[OBJ]);
                    cmd.Parameters.Add(param);
                }

                cmd.ExecuteNonQuery();
                retID = 1;
                cmd.Parameters.Clear();
            }
            catch (SqlException ex)
            {
                if (tran != null)
                {
                    tran.Rollback();
                    tran = null;
                }
                con.Close();
                throw ex;

            }
            catch (Exception ex)
            {
                if (tran != null)
                {
                    tran.Rollback();
                    tran = null;
                    con.Close();
                    throw ex;
                }
            }
            finally
            {
                con.Close();
            }
            return retID;
        }

        public DataTable GetDataByProc(Hashtable ht, string SProc)
        {
            
            DataSet ds = new DataSet();
            SqlDataAdapter adp = new SqlDataAdapter();
            try
            {
                if (con.State != ConnectionState.Open)
                {
                    //con = new SqlConnection(ConnectionString);
                    con.Open();
                }
                cmd.Connection = con;
                cmd.CommandText = SProc;
                cmd.CommandType = CommandType.StoredProcedure;
                foreach (object obj in ht.Keys)
                {
                    string ColumnName = Convert.ToString(obj);
                    SqlParameter param = new SqlParameter(ColumnName, ht[obj]);
                    cmd.Parameters.Add(param);
                }
                adp.SelectCommand = cmd;
                adp.Fill(ds, "Table1");
              
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Parameters.Clear();
                con.Close();
            }
            return ds.Tables[0];
        }
        public DataTable GetDataByProc(string SProc)
        {
           
            DataSet ds = new DataSet();
            SqlDataAdapter adp = new SqlDataAdapter();
            try
            {
                if (con.State != ConnectionState.Open)
                {
                    //con = new SqlConnection(ConnectionString);
                    con.Open();
                }
                cmd.Connection = con;
                cmd.CommandText = SProc;
                cmd.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand = cmd;
                adp.Fill(ds, "Table1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                con.Close();
            }
            return ds.Tables[0];
        }



        public long ExecuteSP(string SProc)
        {
           
            long retID = 0;
            try
            {
                if (con.State != ConnectionState.Open)
                {
                    //con = new SqlConnection(ConnectionString);
                    con.Open();
                }
                cmd.Connection = con;
                cmd.CommandText = SProc;
                // cmd.Transaction = tran;   
                cmd.CommandType = CommandType.StoredProcedure;


                cmd.ExecuteNonQuery();
                retID = 1;
                cmd.Parameters.Clear();
                if (tran == null)
                {
                    con.Close();
                }
            }
            catch (SqlException ex)
            {
                if (tran != null)
                {
                    tran.Rollback();
                    tran = null;
                }
                con.Close();
                throw ex;

            }
            catch (Exception ex)
            {
                if (tran != null)
                {
                    tran.Rollback();
                    tran = null;
                    con.Close();
                    throw ex;
                }

            }
            finally
            {
                con.Close();
            }

            return retID;

        }

        public object AggRetrive(string Command)
        {

            object obj = new object();
            try
            {
                con.Open();
                cmd = new SqlCommand(Command.Trim(), con);
                obj = cmd.ExecuteScalar();

                cmd.Dispose();
            }
            catch (SqlException sqlExcp)
            {
                throw sqlExcp;
            }
            catch (Exception excp)
            {
                throw excp;
            }
            finally
            {
                con.Close();
            }
            return obj;
        }
        public DataTable GetDataBySQLString(string SQLString)
        {
            
            DataSet ds = new DataSet();
            SqlDataAdapter adp = new SqlDataAdapter();
            try
            {
                if (con.State != ConnectionState.Open)
                {
                    //con = new SqlConnection(ConnectionString);
                    con.Open();
                }
                cmd.Connection = con;
                cmd.CommandText = SQLString;
                cmd.CommandType = CommandType.Text;
                adp.SelectCommand = cmd;
                adp.Fill(ds, "Table1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                con.Close();
            }
            return ds.Tables[0];
        }
        public long ExecuteCommand(Hashtable htable, string procedureName)
        {
           
            long retID = 0;
            try
            {
                if (con.State != ConnectionState.Open)
                {
                    con = new SqlConnection(ConnectionString);
                    con.Open();
                }
                cmd.Connection = con;
                cmd.CommandText = procedureName;
                // cmd.Transaction = tran;   
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (object OBJ in htable.Keys)
                {
                    string COLUMN_NAME = Convert.ToString(OBJ);
                    SqlParameter param = new SqlParameter("@" + COLUMN_NAME, htable[OBJ]);
                    cmd.Parameters.Add(param);
                }
                cmd.ExecuteNonQuery();
                retID = 1;
                cmd.Parameters.Clear();
                if (tran == null)
                {
                    con.Close();
                }
            }
            catch (SqlException ex)
            {
                if (tran != null)
                {
                    tran.Rollback();
                    tran = null;
                }
                //con.Close();
                throw ex;

            }
            catch (Exception ex)
            {
                if (tran != null)
                {
                    tran.Rollback();
                    tran = null;
                    //con.Close();
                    throw ex;
                }

            }
            finally
            {
                con.Close();
            }

            return retID;
        }
        public void beginTransaction()
        {
            

            if (con.State != ConnectionState.Open)
            {
                con = new SqlConnection(ConnectionString);
                con.Open();
            }
            if (tran == null)
            {
                cmd = con.CreateCommand();
                tran = con.BeginTransaction();
                cmd.Transaction = tran;
            }
        }
        public void commitTransaction()
        {
            if (tran != null)
            {
                tran.Commit();
                tran = null;
            }
            else
            {
                tran.Rollback();


            }
        }

        public long ServerCheck()
        {
            string startdate = "07/24/2012";
            string enddate = DateTime.Today.ToShortDateString();

            DateTime sd = Convert.ToDateTime(startdate);
            DateTime ed = Convert.ToDateTime(enddate);

            int mdays;

            TimeSpan ts = ed.Subtract(sd);
            mdays = mdays = int.Parse(ts.Days.ToString());

            return mdays;
        }
    }
}
