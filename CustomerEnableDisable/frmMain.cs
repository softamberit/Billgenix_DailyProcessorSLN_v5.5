using BillingERPConn;
using CustomerEnableDisable.Utilitys;
using MkCommunication;
using SWIFTDailyProcessor;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace CustomerEnableDisable
{

    public partial class FrmMain : Form
    {
        readonly DBUtility Idb = new DBUtility();

        public FrmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        int PinNumber = 10000;


        private void CustomerON_Click(object sender, EventArgs e)
        {

            var confirmResult = MessageBox.Show("Are you sure ??",
                                       "Confirm!",
                                       MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                Customer_ON();
                lblStat.Text = " ON Complete";
            }
            else
            {
                // If 'No', do something here.
            }

          
        }

        private void CustomerOFF_Click(object sender, EventArgs e)
        {


            var confirmResult = MessageBox.Show("Are you sure ??",
                                       "Confirm!",
                                       MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                Customer_OFF();
                lblStat.Text = "OFF Complete";
            }
            else
            {
                // If 'No', do something here.
            }

           
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

                            MkConnStatus objMkConnStatusEnable = objMkConnection.EnableMikrotik(hostname, username, password, mkVersion, protocolId , Conversion.TryCastInteger(InsType), mkUser);

                            if (objMkConnStatusEnable.StatusCode == "200")
                            {

                                WriteLogFile.WriteLog(customerId+"Customer ON");
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

                            MkConnStatus objMkConnStatusDisable = objMkConnection.DisableMikrotik(hostname, username, mkVersion, password, protocolId, Conversion.TryCastInteger(InsType), mkUser);

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


        private void exitBtn_Click(object sender, EventArgs e)
        {

            this.Close();
        }

        private void btnCheckRouter_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dtCustomerInfo = Idb.GetDataBySQLString("Select * From POPWiseRouter");
               
                foreach (DataRow datarow in dtCustomerInfo.Rows)
                {
                    var username = datarow["UserName"].ToString();
                    var password = datarow["Password"].ToString();                  
                    var hostname = datarow["Host"].ToString();
                    var mkVersion = datarow["mkVersion"].ToString();

                    MkConnection objMkConnection = new MkConnection();

                    MkConnStatus RouterConnectionStatus = objMkConnection.RouterConnectionStatus("118.179.187.152", "iaminvincible", "^.(AbraKaDabra).$", mkVersion);

                    //listBox1.Items.Add(RouterConnectionStatus.Status.ToString());


                    lblStatus.Text = lblStatus.Text + Environment.NewLine.ToString() + RouterConnectionStatus.Status.ToString();
                   


                }
            }
            catch (Exception)
            {
                
                throw;
            }

            
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                System.Text.StringBuilder copy_buffer = new System.Text.StringBuilder();
                foreach (object item in listBox1.SelectedItems)
                    copy_buffer.AppendLine(item.ToString());
                if (copy_buffer.Length > 0)
                    Clipboard.SetText(copy_buffer.ToString());
            }
        }

        private void btnPullComment_Click(object sender, EventArgs e)
        {
           
            //DataTable dtabpop = new DataTable();
            //dtabpop = Idb.GetDataBySQLString("Select * From PopMaster Where IsActive=1");

            //foreach (DataRow dr in dtabpop.Rows)
            //{

            //    GetCommentsDataFromMikroTik(int.Parse(dr["Id"].ToString()));
            //}

          //  GetCommentsDataFromMikroTik_New();
        }

        //public void GetCommentsDataFromMikroTik_New()
        //{

        //    var Hostname = "";
        //    var Username = "";
        //    var Password = "";
        //    var popid = "";
           
        //    try
        //    {

        //        using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
        //        {


        //            DataTable dtabpop = new DataTable();
        //            dtabpop = Idb.GetDataBySQLString("Select * From POPWiseRouter Where IsActive=1");



        //            string ipAddress = "";
        //            string CustomerID = "";
        //            foreach (DataRow dr in dtabpop.Rows)
        //            {

        //                Hostname = dr["Host"].ToString();
        //                Username = dr["Username"].ToString();
        //                Password = dr["Password"].ToString();
        //                popid = dr["ID"].ToString();
        //               string mkVersion = dr["mkVersion"].ToString();

        //                //DataTable dtabCust = new DataTable();
        //                //dtabCust = Idb.GetDataBySQLString("Select * From CustomerMaster Where RouterId=" + popid + " ");

        //
        //               (Hostname, Username, Password);

        //                var loadCmd = connection.CreateCommandAndParameters("/ip/arp/print");
        //                var response = loadCmd.ExecuteList();

        //                int dlngth = response.Count();

        //                for (int i = 0; i < dlngth; i++)
        //                {
        //                    try
        //                    {
        //                        var itemId1 = response.ElementAt(i);

        //                        var disable_status = itemId1.GetResponseField("disabled");
        //                        var invalid_status = itemId1.GetResponseField("invalid");
        //                        var dynamic_status = itemId1.GetResponseField("dynamic");
        //                        ipAddress = itemId1.GetResponseFieldOrDefault("address","");
        //                        var comment = itemId1.GetResponseFieldOrDefault("comment", "");
        //                        var macAddress = itemId1.GetResponseFieldOrDefault("mac-address", "");
        //                        var mkinterface = itemId1.GetResponseFieldOrDefault("interface", "");

        //                        //  mk_status = mk_status + "disable_status :" + disable_status + " | invalid_status :" + invalid_status + " | dynamic_status :" + dynamic_status;





        //                        Hashtable ht = new Hashtable();
        //                        ht.Add("CustomerId", CustomerID);
        //                        ht.Add("IPAddress", ipAddress);
        //                        ht.Add("disabled", disable_status);
        //                        ht.Add("invalid", invalid_status);
        //                        ht.Add("dynamic", dynamic_status);
        //                        ht.Add("PopId", popid);
        //                        ht.Add("PopIP", Hostname);
        //                        ht.Add("Comment", comment);
        //                        ht.Add("macaddress", macAddress);
        //                        ht.Add("interface", mkinterface);

        //                        var a = Idb.InsertData(ht, "sp_InsertSOFT_MIKROTIK_STATUS_CMNT");
        //                    }
        //                    catch (Exception ex)
        //                    {

        //                        string ss = ex.Message.ToString();
        //                        continue;
        //                    }
        //                }


        //                if (dlngth == 0)
        //                {

        //                    Hashtable ht = new Hashtable();
        //                    ht.Add("CustomerId", CustomerID);
        //                    ht.Add("IPAddress", ipAddress);
        //                    ht.Add("disabled", "");
        //                    ht.Add("invalid", "");
        //                    ht.Add("dynamic", "");
        //                    ht.Add("PopId", popid);
        //                    ht.Add("PopIP", Hostname);
        //                    ht.Add("Comment", "");
        //                    ht.Add("macaddress", "");
        //                    ht.Add("interface", "");

        //                    var a = Idb.InsertData(ht, "sp_InsertSOFT_MIKROTIK_STATUS_CMNT");

        //                }

        //                connection.Close();
                        
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        WriteLogFile.WriteLog(popid.ToString() + " :| " + ex.Message.ToString());
        //    }



        //    lblStatus.Text = "Comments Pull Done";

        //}


    }

}
