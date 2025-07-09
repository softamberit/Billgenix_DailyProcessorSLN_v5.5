using MkCommunication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tik4net;
using tik4net.Objects;
using tik4net.Objects.Ppp;

namespace MkCommunication
{
    public class MkConnection
    {
        int _defaultPort = 8928;
        //  readonly static int waiting_durationInSec = 30;
        public MkConnStatus DisableMikrotik(string Hostname, string Username, string Password, string mkVersion, int ProtocolId, int InsType, string mkUser, int port = 0)
        {
            MkConnStatus objConnStatus = new MkConnStatus();

            string message = "";

            try
            {
                //if (mkUser.Contains('-'))
                //{

                //    mkUser = mkUser.Replace("-", "");
                //}

                if (InsType == 1)
                {
                    if (Hostname != "" && Username != "" && Password != "" && ProtocolId != 0)
                    {

                        if (ProtocolId == 1)
                        {
                            objConnStatus = IPV4EnableDisable("yes", Hostname, Username, Password, mkVersion, ProtocolId, mkUser, port);
                            return objConnStatus;
                        }
                        else if (ProtocolId == 2)
                        {
                            objConnStatus = IPV6EnableDisable("yes", Hostname, Username, Password, mkVersion, ProtocolId, mkUser, port);
                            return objConnStatus;
                        }

                        else if (ProtocolId == 3)
                        {
                            MkConnStatus objConnStatus1 = IPV4EnableDisable("yes", Hostname, Username, Password, mkVersion, ProtocolId, mkUser, port);

                            MkConnStatus objConnStatus2 = IPV6EnableDisable("yes", Hostname, Username, Password, mkVersion, ProtocolId, mkUser, port);

                            if (objConnStatus1.StatusCode == "200" || objConnStatus2.StatusCode == "200")
                            {
                                objConnStatus.StatusCode = "200";
                                objConnStatus.Status = "Success";
                                objConnStatus.RetMessage = "IP Disabled Success";
                                objConnStatus.CodePortion = "Protocol 3 block";
                            }
                            else
                            {
                                objConnStatus.StatusCode = "220";
                                objConnStatus.Status = "Failed";
                                objConnStatus.RetMessage = objConnStatus1.RetMessage + ", " + objConnStatus2.RetMessage;
                                objConnStatus.CodePortion = "Protocol 3 block";
                            }
                        }
                    }
                }

                else if (InsType == 2) /// PPPoE
                {

                    objConnStatus = EnableDisableforPPPoE("true", Hostname, Username, Password, mkVersion, mkUser, port);
                    return objConnStatus;
                }

            }
            catch (Exception ex)
            {

                message = mkUser + " " + ex.Message.ToString();
                objConnStatus.StatusCode = "230";
                objConnStatus.Status = "Disabled Failed";
                objConnStatus.RetMessage = message;
                objConnStatus.CodePortion = "Main Catch Block";

            }

            return objConnStatus;
        }

        public MkConnStatus EnableMikrotik(string Hostname, string Username, string Password, string mkVersion, int ProtocolId, int InsType, string mkUser, int port = 0)
        {
            MkConnStatus objConnStatus = new MkConnStatus();

            string message = "";
            try
            {
                //if (mkUser.Contains('-'))
                //{

                //    mkUser = mkUser.Replace("-", "");
                //}

                if (InsType == 1)
                {
                    if (Hostname != "" && Username != "" && Password != "" && ProtocolId != 0)
                    {

                        if (ProtocolId == 1)
                        {
                            objConnStatus = IPV4EnableDisable("no", Hostname, Username, Password, mkVersion, ProtocolId, mkUser, port);
                            return objConnStatus;
                        }
                        else if (ProtocolId == 2)
                        {
                            objConnStatus = IPV6EnableDisable("no", Hostname, Username, Password, mkVersion, ProtocolId, mkUser, port);
                            return objConnStatus;
                        }

                        else if (ProtocolId == 3)
                        {
                            MkConnStatus objConnStatus1 = IPV4EnableDisable("no", Hostname, Username, Password, mkVersion, ProtocolId, mkUser, port);

                            MkConnStatus objConnStatus2 = IPV6EnableDisable("no", Hostname, Username, Password, mkVersion, ProtocolId, mkUser, port);

                            if (objConnStatus1.StatusCode == "200" || objConnStatus2.StatusCode == "200")
                            {
                                objConnStatus.StatusCode = "200";
                                objConnStatus.Status = "Success";
                                objConnStatus.RetMessage = "IP Enabled Success";
                                objConnStatus.CodePortion = "Protocol 3 block";
                            }
                            else
                            {
                                objConnStatus.StatusCode = "220";
                                objConnStatus.Status = "Failed";
                                objConnStatus.RetMessage = "IPV4: " + objConnStatus1.RetMessage + ", IPV6: " + objConnStatus2.RetMessage;
                                objConnStatus.CodePortion = "Protocol 3 block";
                            }
                        }
                    }
                }

                else if (InsType == 2)
                {
                    objConnStatus = EnableDisableforPPPoE("false", Hostname, Username, Password, mkVersion, mkUser, port);
                    return objConnStatus;
                }


            }
            catch (Exception ex)
            {

                message = mkUser + " " + ex.Message.ToString();
                objConnStatus.StatusCode = "230";
                objConnStatus.Status = "Enabled Failed";
                objConnStatus.RetMessage = message;
                objConnStatus.CodePortion = "Main Catch Block";
                WriteLogFile.WriteLog(message);
            }

            return objConnStatus;
        }

        public MkConnStatus MikrotikStatus(string Hostname, string Username, string Password, string mkVersion, int ProtocolId, int InsType, string mkUser, int port = 0)
        {
            MkConnStatus objConnStatus = new MkConnStatus();

            string message = "";
            try
            {


                if (InsType == 1)
                {
                    if (Hostname != "" && Username != "" && Password != "" && ProtocolId != 0)
                    {

                        if (ProtocolId == 1)
                        {
                            objConnStatus = FindIPV4Status(Hostname, Username, Password, mkVersion, ProtocolId, mkUser, port);

                        }
                        else if (ProtocolId == 2)
                        {
                            objConnStatus = FindIPV6Status(Hostname, Username, Password, mkVersion, ProtocolId, mkUser, port);

                        }

                        else if (ProtocolId == 3)
                        {
                            MkConnStatus objConnStatus1 = FindIPV4Status(Hostname, Username, Password, mkVersion, ProtocolId, mkUser, port);

                            MkConnStatus objConnStatus2 = FindIPV6Status(Hostname, Username, Password, mkVersion, ProtocolId, mkUser, port);

                            if (objConnStatus1.StatusCode == "200" || objConnStatus2.StatusCode == "200")
                            {

                                if (objConnStatus1.MikrotikStatus == 0 && objConnStatus2.MikrotikStatus == 0)
                                {
                                    objConnStatus.DLength = 0;
                                }

                                if (objConnStatus1.MikrotikStatus == 0 && objConnStatus2.MikrotikStatus == 0)
                                {
                                    objConnStatus.MikrotikStatus = 0;
                                }
                                else if (objConnStatus1.MikrotikStatus == 1 || objConnStatus2.MikrotikStatus == 1)
                                {
                                    objConnStatus.MikrotikStatus = 1;
                                }
                            }
                            else
                            {
                                objConnStatus.StatusCode = "220";
                                objConnStatus.Status = "Status not found";
                                objConnStatus.RetMessage = objConnStatus1.RetMessage + " " + objConnStatus2.RetMessage;
                                objConnStatus.CodePortion = "Inner Catch block";
                            }

                        }
                    }
                }

                else if (InsType == 2)
                {
                    objConnStatus = FindMKStatusforPPPoE(Hostname, Username, Password, mkVersion, mkUser, port);
                    return objConnStatus;
                }


            }
            catch (Exception ex)
            {

                message = mkUser + " " + ex.Message.ToString();
                objConnStatus.StatusCode = "230";
                objConnStatus.Status = "Enabled Failed";
                objConnStatus.RetMessage = message;
                objConnStatus.CodePortion = "MikrotikStatus Main Catch Block";
                WriteLogFile.WriteLog(message);




            }

            return objConnStatus;
        }

        public MkConnStatus IPV4EnableDisable(string Operation, string Hostname, string Username, string Password, string mkVersion, int ProtocolId, string mkUser, int port = 0)
        {
            MkConnStatus objConnStatus = new MkConnStatus();

            string message = "";
            string msg = "";

            if (Operation == "yes")
                msg = "Disabled";
            else msg = "Enabled";

            try
            {



                if (Hostname != "" && Username != "" && Password != "" && ProtocolId != 0)
                {
                    using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
                    {
                        int dlngth = 0;


                        IEnumerable<ITikReSentence> response = null;
                        connection.ReceiveTimeout = 60000;
                        connection.SendTimeout = 60000;
                        connection.Open(Hostname, _defaultPort, Username, Password);

                        var loadCmd = connection.CreateCommandAndParameters("/ip/arp/print", "comment", mkUser);
                        // response = loadCmd.ExecuteListWithDuration(waiting_durationInSec);
                        response = loadCmd.ExecuteList();

                        dlngth = response.Count();

                        var itemId = "";

                        for (int i = 0; i < dlngth; i++)
                        {
                            try
                            {
                                var itemId1 = response.ElementAt(i);

                                itemId = itemId1.GetId();
                                var updateCmd = connection.CreateCommandAndParameters("/ip/arp/set", "disabled", Operation, TikSpecialProperties.Id, itemId);
                                updateCmd.ExecuteNonQuery();


                                objConnStatus.StatusCode = "200";
                                objConnStatus.Status = "Success";
                                objConnStatus.RetMessage = "IP" + msg + " Success";
                                objConnStatus.CodePortion = "Inner Try block";

                                //break;
                            }
                            catch (Exception ex)
                            {

                                if (i == dlngth - 1)
                                {
                                    message = mkUser + " " + ex.Message.ToString();
                                    objConnStatus.StatusCode = "220";
                                    objConnStatus.Status = msg + " Failed";
                                    objConnStatus.RetMessage = message;
                                    objConnStatus.CodePortion = "Inner Catch block";

                                }
                                // continue;
                            }

                        }

                        if (dlngth == 0)
                        {
                            message = message + "Customer Comment Mismatch, " + mkUser + " so no operation";

                            objConnStatus.StatusCode = "240";
                            objConnStatus.Status = msg + " Failed";
                            objConnStatus.RetMessage = message;
                            objConnStatus.CodePortion = "lenth0 block";
                        }
                    }

                }
                else
                {
                    objConnStatus.StatusCode = "210";
                    objConnStatus.Status = msg + " Failed";
                    objConnStatus.RetMessage = "Host, Username, Password, IP did not provided.";
                    objConnStatus.CodePortion = "Host, Username, Password, IP check Block";
                }

            }
            catch (Exception ex)
            {

                message = mkUser + " " + ex.Message.ToString();
                objConnStatus.StatusCode = "230";
                objConnStatus.Status = msg + " Failed";
                objConnStatus.RetMessage = message;
                objConnStatus.CodePortion = "Main Catch Block";
                WriteLogFile.WriteLog(message);

            }

            return objConnStatus;
        }

        public MkConnStatus IPV6EnableDisable(string Operation, string Hostname, string Username, string Password, string mkVersion, int ProtocolId, string mkUser, int port = 0)
        {
            MkConnStatus objConnStatus = new MkConnStatus();

            string message = "";

            string msg = "";
            if (Operation == "yes")
                msg = "Disabled";
            else msg = "Enabled";

            try
            {

                //if (mkUser.Contains('-'))
                //{

                //    mkUser = mkUser.Replace("-", "");
                //}


                if (Hostname != "" && Username != "" && Password != "" && ProtocolId != 0)
                {
                    using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
                    {
                        int dlngth = 0;
                        IEnumerable<ITikReSentence> response = null;
                        connection.ReceiveTimeout = 60000;
                        connection.SendTimeout = 60000;
                        connection.Open(Hostname, _defaultPort, Username, Password);

                        var loadCmd = connection.CreateCommandAndParameters("/ipv6/address/print", "comment", mkUser);
                        response = loadCmd.ExecuteList();
                        //response = loadCmd.ExecuteListWithDuration(waiting_durationInSec);

                        dlngth = response.Count();

                        var itemId = "";

                        for (int i = 0; i < dlngth; i++)
                        {
                            try
                            {
                                var itemId1 = response.ElementAt(i);

                                itemId = itemId1.GetId();
                                var updateCmd = connection.CreateCommandAndParameters("/ipv6/address/set", "disabled", Operation, TikSpecialProperties.Id, itemId);
                                updateCmd.ExecuteNonQuery();

                                objConnStatus.StatusCode = "200";
                                objConnStatus.Status = "Success";
                                objConnStatus.RetMessage = "IP" + msg + " Sucess";
                                objConnStatus.CodePortion = "Inner Try block";

                                // break;
                            }
                            catch (Exception ex)
                            {

                                if (i == dlngth - 1)
                                {
                                    message = mkUser + " " + ex.Message.ToString();
                                    objConnStatus.StatusCode = "220";
                                    objConnStatus.Status = msg + " Failed";
                                    objConnStatus.RetMessage = message;
                                    objConnStatus.CodePortion = "Inner Catch block";

                                }
                                // continue;
                            }

                        }


                        if (dlngth == 0)
                        {
                            message = message + "Customer comment mismatch in router for " + mkUser + ", so no operation executed";

                            objConnStatus.StatusCode = "240";
                            objConnStatus.Status = msg + " Failed";
                            objConnStatus.RetMessage = message;
                            objConnStatus.CodePortion = "lenth0 block";
                        }
                    }

                }
                else
                {
                    objConnStatus.StatusCode = "210";
                    objConnStatus.Status = msg + " Failed";
                    objConnStatus.RetMessage = "Host, Username, Password, IP did not provided.";
                    objConnStatus.CodePortion = "Host, Username, Password, IP check Block";
                }

            }
            catch (Exception ex)
            {

                message = mkUser + " " + ex.Message.ToString();
                objConnStatus.StatusCode = "230";
                objConnStatus.Status = msg + " Failed";
                objConnStatus.RetMessage = message;
                objConnStatus.CodePortion = "Main Catch Block";
                WriteLogFile.WriteLog(message);

            }

            return objConnStatus;
        }

        public MkConnStatus FindIPV4Status(string Hostname, string Username, string Password, string mkVersion, int ProtocolId, string mkUser, int port = 0)
        {
            MkConnStatus objConnStatus = new MkConnStatus();

            string message = "";
            try
            {

                //if (mkUser.Contains('-'))
                //{

                //    mkUser = mkUser.Replace("-", "");
                //}


                if (Hostname != "" && Username != "" && Password != "" && ProtocolId != 0)
                {
                    using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
                    {
                        int dlngth = 0;
                        IEnumerable<ITikReSentence> response = null;
                        connection.ReceiveTimeout = 60000;
                        connection.SendTimeout = 60000;
                        connection.Open(Hostname, _defaultPort, Username, Password);

                        var loadCmd = connection.CreateCommandAndParameters("/ip/arp/print", "comment", mkUser);
                        response = loadCmd.ExecuteList();
                        // response = loadCmd.ExecuteListWithDuration(waiting_durationInSec);

                        dlngth = response.Count();
                        objConnStatus.DLength = dlngth;

                        var itemId = "";

                        for (int i = 0; i < dlngth; i++)
                        {
                            try
                            {
                                var itemId1 = response.ElementAt(i);

                                var disable_status = "";
                                var invalid_status = "";
                                var dynamic_status = "";


                                itemId = itemId1.GetId();
                                disable_status = itemId1.GetResponseField("disabled");
                                invalid_status = itemId1.GetResponseField("invalid");
                                dynamic_status = itemId1.GetResponseField("dynamic");

                                if (disable_status == "false" && invalid_status == "false" && dynamic_status == "false")
                                {
                                    objConnStatus.MikrotikStatus = 1;
                                    objConnStatus.StatusCode = "200";

                                }
                                else
                                {
                                    objConnStatus.MikrotikStatus = 0;
                                    objConnStatus.StatusCode = "200";

                                }


                                //objConnStatus.StatusCode = "200";
                                //objConnStatus.Status = "Success";
                                //objConnStatus.RetMessage = "IP Enabled Sucess";
                                //objConnStatus.CodePortion = "Inner Try block";

                                // break;
                            }
                            catch (Exception ex)
                            {

                                if (i == dlngth - 1)
                                {
                                    message = mkUser + " " + ex.Message.ToString();
                                    objConnStatus.StatusCode = "220";
                                    objConnStatus.Status = "Status not found";
                                    objConnStatus.RetMessage = message;
                                    objConnStatus.CodePortion = "Inner Catch block";

                                }

                                continue;
                            }

                        }

                        if (dlngth == 0)
                        {
                            message = message + "Customer comment mismatch in router for " + mkUser + ", so no operation executed";

                            objConnStatus.StatusCode = "240";
                            objConnStatus.Status = "Status not found";
                            objConnStatus.RetMessage = message;
                            objConnStatus.CodePortion = "lenth0 block";
                        }
                    }

                }
                else
                {
                    objConnStatus.StatusCode = "210";
                    objConnStatus.Status = "Status not found";
                    objConnStatus.RetMessage = "Host, Username, Password, IP did not provided.";
                    objConnStatus.CodePortion = "Host, Username, Password, IP check Block";
                }

            }
            catch (Exception ex)
            {

                message = mkUser + " " + ex.Message.ToString();
                objConnStatus.StatusCode = "230";
                objConnStatus.Status = "Status not found";
                objConnStatus.RetMessage = message;
                objConnStatus.CodePortion = "Main Catch Block";
                WriteLogFile.WriteLog(message);

            }

            return objConnStatus;
        }

        public MkConnStatus FindIPV6Status(string Hostname, string Username, string Password, string mkVersion, int ProtocolId, string mkUser, int port = 0)
        {
            MkConnStatus objConnStatus = new MkConnStatus();

            string message = "";
            try
            {

                //if (mkUser.Contains('-'))
                //{

                //    mkUser = mkUser.Replace("-", "");
                //}

                if (Hostname != "" && Username != "" && Password != "" && ProtocolId != 0)
                {
                    using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
                    {
                        int dlngth = 0;
                        IEnumerable<ITikReSentence> response = null;
                        connection.ReceiveTimeout = 60000;
                        connection.SendTimeout = 60000;
                        connection.Open(Hostname, _defaultPort, Username, Password);

                        var loadCmd = connection.CreateCommandAndParameters("/ipv6/address/print", "comment", mkUser);
                        response = loadCmd.ExecuteList();
                        //response = loadCmd.ExecuteListWithDuration(waiting_durationInSec);

                        dlngth = response.Count();
                        objConnStatus.DLength = dlngth;

                        var itemId = "";

                        for (int i = 0; i < dlngth; i++)
                        {
                            try
                            {
                                var itemId1 = response.ElementAt(i);

                                var disable_status = "";
                                var invalid_status = "";
                                var dynamic_status = "";


                                disable_status = itemId1.GetResponseField("disabled");
                                invalid_status = itemId1.GetResponseField("invalid");
                                dynamic_status = itemId1.GetResponseField("dynamic");

                                if (disable_status == "false" && invalid_status == "false")
                                {
                                    objConnStatus.MikrotikStatus = 1;
                                    objConnStatus.StatusCode = "200";
                                }
                                else
                                {
                                    objConnStatus.MikrotikStatus = 0;
                                    objConnStatus.StatusCode = "200";
                                }

                                //objConnStatus.StatusCode = "200";
                                //objConnStatus.Status = "Success";
                                //objConnStatus.RetMessage = "IP Enabled Sucess";
                                //objConnStatus.CodePortion = "Inner Try block";
                                //break;
                            }
                            catch (Exception ex)
                            {

                                if (i == dlngth - 1)
                                {
                                    message = mkUser + " " + ex.Message.ToString();
                                    objConnStatus.StatusCode = "220";
                                    objConnStatus.Status = "Status not found";
                                    objConnStatus.RetMessage = message;
                                    objConnStatus.CodePortion = "Inner Catch block";
                                }

                                continue;
                            }

                        }

                        if (dlngth == 0)
                        {
                            message = message + "Customer comment mismatch in router for " + mkUser + ", so no operation executed";

                            objConnStatus.StatusCode = "240";
                            objConnStatus.Status = "Status not found";
                            objConnStatus.RetMessage = message;
                            objConnStatus.CodePortion = "lenth0 block";
                        }
                    }
                }
                else
                {
                    objConnStatus.StatusCode = "210";
                    objConnStatus.Status = "Status not found";
                    objConnStatus.RetMessage = "Host, Username, Password, IP did not provided.";
                    objConnStatus.CodePortion = "Host, Username, Password, IP check Block";
                }

            }
            catch (Exception ex)
            {

                message = mkUser + " " + ex.Message.ToString();
                objConnStatus.StatusCode = "230";
                objConnStatus.Status = "Status not found";
                objConnStatus.RetMessage = message;
                objConnStatus.CodePortion = "Main Catch Block";
                WriteLogFile.WriteLog(message);

            }

            return objConnStatus;
        }


        public MkConnStatus EnableDisableforPPPoE(string Operation, string Hostname, string Username, string Password, string mkVersion, string mkUser, int port = 0)
        {
            MkConnStatus objConnStatus = new MkConnStatus();

            string message = "";
            string msg = "";

            if (Operation == "true")
                msg = "Disabled";
            else msg = "Enabled";

            try
            {


                using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
                {


                    try
                    {
                        if (port == 0)
                        {
                            port = _defaultPort;
                        }
                        IEnumerable<ITikReSentence> response = null;
                        connection.ReceiveTimeout = 60000;
                        connection.SendTimeout = 60000;

                        //connection.Open("118.179.187.152",_defaultPort, "iaminvincible", "^.(AbraKaDabra).$");
                        connection.Open(Hostname.Trim(), _defaultPort, Username.Trim(), Password.Trim());
                       
                        IEnumerable<ITikReSentence> sentences = connection.CreateCommandAndParameters("/ppp/secret/print", "name", mkUser).ExecuteList();
                        var mkResponse = MkResponse.Parse(sentences);
                        //var objMkResponse = new MkResponse();

                        // Mk real time status check
                        // string statusMk = secret.Words["disabled"].ToString();

                        if (mkResponse.Count == 0)
                        {
                            //throw new Exception("Not found");
                            objConnStatus.MikrotikStatus = 0;
                            objConnStatus.StatusCode = "201";
                            throw new Exception("Customer ID is not found in Mikrotik");

                        }
                        //if (mkResponse.Count > 1)
                        //{
                        //    throw new Exception("Multiple Customer found");

                        //}
                        //if (mkResponse.Count == 1)
                        //{
                        //    objMkResponse = mkResponse.SingleOrDefault();

                        //}

                        // Disable the secret.
                        foreach (var item in mkResponse)
                        {
                            connection.CreateCommandAndParameters("/ppp/secret/set", ".id", item.ID, "disabled", Operation).ExecuteNonQuery();

                        }
                        objConnStatus.StatusCode = "200";
                        objConnStatus.Status = "Success";
                        objConnStatus.RetMessage = "IP" + msg + " Success";
                        objConnStatus.CodePortion = "Inner Try block";


                        // Remove from Active directory
                        if (Operation.Equals("true"))
                        {
                            //foreach (var item in mkResponse)
                            //{
                            //    connection.CreateCommandAndParameters("/ppp/secret/remove", ".id", item.ID).ExecuteNonQuery();
                            //}

                            // IEnumerable<ITikReSentence> secrets = connection.CreateCommandAndParameters("/ppp/secret/print", "name", mkUser).ExecuteList();

                            //foreach (var item in mkResponse)
                            //{
                            //    connection.CreateCommandAndParameters("/ppp/active/remove", ".id", item.ID).ExecuteNonQuery();
                            //    connection.CreateCommandAndParameters("/ppp/secret/remove", ".id", item.ID).ExecuteNonQuery();

                            //}
                            var pppActiveList = connection.LoadAll<PppActive>();
                            // IEnumerable<ITikReSentence> sentences2 = connection.CreateCommandAndParameters("/ppp/active/print", "name", mkUser).ExecuteList();
                            //var mkResponse2 = MkResponse.Parse(sentences2);
                            //foreach (var item in mkResponse2)
                            //{
                            //    connection.CreateCommandAndParameters("/ppp/active/remove", ".id", item.ID).ExecuteNonQuery();
                            //}
                            if (pppActiveList != null)
                            {
                                var pppActive = pppActiveList.SingleOrDefault(s => s.Name == mkUser);
                                if (pppActive != null && pppActive.Id != null)
                                {
                                    connection.CreateCommandAndParameters("/ppp/active/remove", ".id", pppActive.Id).ExecuteNonQuery();

                                }
                            }

                            //ITikReSentence activeSentence = connection.CreateCommandAndParameters("/ppp/active/print", "name", mkUser).ExecuteSingleRow();
                            //connection.CreateCommandAndParameters("/ppp/active/remove", ".id", activeSentence.Words[".id"]).ExecuteNonQuery();
                        }


                    }
                    catch (Exception ex)
                    {
                        //if (ex.Message.Contains("A connection attempt failed because the connected party did not properly respond after a period of time"))
                        //{
                        //    message = Hostname + "==> Mikrotik is not reachable from the Billgenix server";
                        //}
                        //objConnStatus.StatusCode = "230";
                        //objConnStatus.Status = msg + " Failed";
                        //objConnStatus.RetMessage = message;
                        //objConnStatus.CodePortion = "Inner Catch block";
                        throw ex;
                    }
                    finally
                    {
                        if (connection.IsOpened)
                        {
                            connection.Close();
                            connection.Dispose();
                        }

                    }


                }



            }
            catch (Exception ex)
            {

                objConnStatus.StatusCode = "230";
                if (ex.Message.Contains("A connection attempt failed because the connected party did not properly respond after a period of time"))
                {
                    message = Hostname + " Mikrotik is not reachable from the Billgenix server";
                    objConnStatus.StatusCode = "401";

                }
                else
                {
                    message = ex.Message;
                }

                message = mkUser + "=> " + message;


                objConnStatus.Status = msg + " Failed";
                objConnStatus.RetMessage = message;
                objConnStatus.CodePortion = "Main Catch Block";

            }

            return objConnStatus;
        }


        public MkConnStatus FindMKStatusforPPPoE(string Hostname, string Username, string Password, string mkVersion, string mkUser, int port = 0)
        {
            MkConnStatus objConnStatus = new MkConnStatus();

            string message = "";
            try
            {

                //if (mkUser.Contains('-'))
                //{

                //    mkUser = mkUser.Replace("-", "");
                //}
                if (port == 0)
                {
                    port = _defaultPort;
                }

                using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
                {


                    IEnumerable<ITikReSentence> response = null;
                    connection.ReceiveTimeout = 60000;
                    connection.SendTimeout = 60000;
                    connection.Open(Hostname.Trim(), _defaultPort, Username, Password);

                    ITikReSentence secret = connection.CreateCommandAndParameters("/ppp/secret/print", "name", mkUser).ExecuteSingleRow();


                    // Mk real time status check
                    string statusMk = secret.Words["disabled"].ToString();


                    if (statusMk == "false")
                    {
                        objConnStatus.MikrotikStatus = 1;
                        objConnStatus.StatusCode = "200";

                    }
                    else
                    {
                        objConnStatus.MikrotikStatus = 0;
                        objConnStatus.StatusCode = "200";
                    }
                }
            }
            catch (Exception ex)
            {

                message = mkUser + " " + ex.Message.ToString();
                objConnStatus.StatusCode = "230";
                objConnStatus.Status = "Status not found";
                objConnStatus.RetMessage = message;
                objConnStatus.CodePortion = "FindMKStatusforPPPoE Main Catch Block";
                WriteLogFile.WriteLog(message);



            }

            return objConnStatus;
        }





        public MkConnStatus RouterConnectionStatus(string Hostname, string Username, string Password, string mkVersion)
        {
            MkConnStatus objConnStatus = new MkConnStatus();

            string message = "";

            string msg = "";


            try
            {
                if (Hostname != "" && Username != "" && Password != "")
                {
                    using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
                    {
                        int dlngth = 0;
                        IEnumerable<ITikReSentence> response = null;
                        //connection.ReceiveTimeout = 60000;
                        //connection.SendTimeout = 60000;
                        connection.Open(Hostname, _defaultPort, Username, Password);

                        var loadCmd = connection.CreateCommandAndParameters("/ip/arp/print");
                        response = loadCmd.ExecuteList();
                        //response = loadCmd.ExecuteListWithDuration(waiting_durationInSec);

                        dlngth = response.Count();

                        objConnStatus.StatusCode = "200";
                        objConnStatus.Status = Hostname + " Connection Success";


                    }

                }


            }
            catch (Exception ex)
            {
                objConnStatus.StatusCode = "400";
                objConnStatus.Status = Hostname + " Connection Failed";

            }

            return objConnStatus;
        }

        public MkConnStatus CreateUser(string Hostname, string Username, string Password, string mkVersion, string mkUser, string pwd, string ipAddress)
        {
            MkConnStatus objConnStatus = new MkConnStatus();

            string message = "";
            string msg = "";



            try
            {

                //if (mkUser.Contains('-'))
                //{

                //    mkUser = mkUser.Replace("-", "");
                //}

                using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
                {

                    IEnumerable<ITikReSentence> response = null;
                    connection.ReceiveTimeout = 60000;
                    connection.SendTimeout = 60000;
                    connection.Open(Hostname, _defaultPort, Username, Password);

                    // connection.Open(Hostname, Username, Password, mkVersion);

                    IEnumerable<ITikReSentence> secrets = connection.CreateCommandAndParameters("/ppp/secret/print", "name", mkUser).ExecuteList();
                    bool isExist = false;
                    foreach (var item in secrets)
                    {
                        if (item.Words["name"].ToString() == mkUser)
                        {
                            isExist = true;
                        }
                    }

                    //ppp secret remove [find name=test]

                    if (!isExist)
                    {
                        var command = connection.CreateCommand("/ppp/secret/add");
                        command.AddParameter("name", mkUser);
                        command.AddParameter("password", pwd);
                        command.AddParameter("service", "pppoe");
                        command.AddParameter("remote-address", ipAddress);
                        command.AddParameter("profile", "default");

                        command.ExecuteNonQuery();
                    }
                    // Mk real time status check
                    //  string statusMk = secret.Words["disabled"].ToString();

                    // Disable the secret.
                    // connection.CreateCommandAndParameters("/ppp/secret/set", ".id", secret.Words[".id"], "disabled", Operation).ExecuteNonQuery();


                    objConnStatus.StatusCode = "200";
                    objConnStatus.Status = "Success";
                    objConnStatus.RetMessage = "IP" + msg + " Success";
                    objConnStatus.CodePortion = "Inner Try block";





                }



            }
            catch (Exception ex)
            {

                if (ex.Message.Contains("failure: secret with the same name already exists"))
                {


                    message = mkUser + " " + ex.Message;
                    objConnStatus.StatusCode = "230";
                    objConnStatus.Status = msg + " Failed";
                    objConnStatus.RetMessage = message;
                    objConnStatus.CodePortion = "Main Catch Block";
                    WriteLogFile.WriteLog(message);

                }

            }

            return objConnStatus;
        }

        public string GetMkStatus(string statusId, string statusName, string secondaryStatus, string InsType, MkConnStatus objMkStatus)
        {
            string mkStatus = "";
            if (InsType == "1")
            {
                if (objMkStatus.DLength > 0)
                {
                    if (objMkStatus.MikrotikStatus == 0)
                    {
                        if (statusId == "1")
                        {

                            mkStatus = "SOFTWARE STATUS IS ACTIVE BUT MIKROTIK STATUS IS INACTIVE, NEED TO CHECK";
                        }
                        else if (statusId == "3")
                        {
                            mkStatus = statusName + ", " + secondaryStatus.ToString();
                        }

                        else if (statusId == "2")
                        {
                            mkStatus = statusName + ", " + secondaryStatus.ToString();
                        }

                        else if (statusId == "9")
                        {
                            mkStatus = statusName + ", " + secondaryStatus.ToString();
                        }

                    }
                    else if (objMkStatus.MikrotikStatus == 1)
                    {
                        if (statusId == "1")
                        {
                            //lbDiscontinueSource.ForeColor = Color.Red;
                            mkStatus = @"ACTIVE";

                        }
                        else if (statusId == "3")
                        {
                            mkStatus = @"SOFTWARE STATUS IS CANCEL BUT MIKROTIK STATUS IS ACTIVE";
                        }
                        else if (statusId == "9")
                        {
                            mkStatus = @"SOFTWARE STATUS IS INACTIVE BUT MIKROTIK STATUS IS ACTIVE";
                        }
                        else if (statusId == "0")
                        {
                            mkStatus = @"SOFTWARE STATUS IS HANDOVER PENDING BUT MIKROTIK STATUS IS ACTIVE";

                        }
                        else
                        {
                            mkStatus = @"SOFTWARE STATUS IS DISCONTINUE BUT MIKROTIK STATUS IS ACTIVE";
                        }
                    }
                }

                else if (objMkStatus.DLength == 0)
                {
                    if (statusId == "1")
                    {
                        //string ss = "Active";

                        mkStatus = "SOFTWARE STATUS IS ACTIVE BUT COMMENTS NOT FOUND IN MIKROTIK ROUTER, NEED TO CHECK";
                    }
                    else if (statusId == "3")
                    {
                        mkStatus = @"CANCEL";
                    }

                    else if (statusId == "2")
                    {
                        mkStatus = @"DISCONTINUE" + ", " + secondaryStatus.ToString(); ;
                    }

                    else if (statusId == "9")
                    {
                        mkStatus = "SOFTWARE STATUS IS INACTIVE BUT COMMENTS NOT FOUND IN MIKROTIK ROUTER, NEED TO CHECK";
                    }

                    else
                    {
                        mkStatus = "NO STATUS YET!";
                    }
                }
            }

            else if (InsType == "2") // PPPOE 
            {
                if (objMkStatus.MikrotikStatus == 1)
                {
                    //  lbDiscontinueSource.ForeColor = Color.Red;
                    //  mkStatus = @"ACTIVE";

                    if (statusId == "1")
                    {
                        //lbDiscontinueSource.ForeColor = Color.Red;
                        mkStatus = @"ACTIVE";

                    }
                    else if (statusId == "9")
                    {
                        mkStatus = @"SOFTWARE STATUS IS INACTIVE BUT MIKROTIK STATUS IS ACTIVE";
                    }
                    else if (statusId == "3")
                    {
                        mkStatus = @"SOFTWARE STATUS IS CANCEL BUT MIKROTIK STATUS IS ACTIVE";
                    }
                    else if (statusId == "0")
                    {
                        mkStatus = @"SOFTWARE STATUS IS HANDOVER PENDING BUT MIKROTIK STATUS IS ACTIVE";

                    }
                    else
                    {
                        mkStatus = @"SOFTWARE STATUS IS DISCONTINUE BUT MIKROTIK STATUS IS ACTIVE";
                    }

                }
                else if (objMkStatus.MikrotikStatus == 0)
                {
                    //lbDiscontinueSource.ForeColor = Color.Red;

                    if (statusId == "1")
                    {
                        //string ss = "Active";

                        mkStatus = "SOFTWARE STATUS IS ACTIVE BUT MIKROTIK STATUS IS INACTIVE, NEED TO CHECK";
                    }
                    else if (statusId == "3")
                    {
                        mkStatus = @"CANCEL";
                    }

                    else if (statusId == "2")
                    {
                        mkStatus = @"DISCONTINUE" + ", " + secondaryStatus.ToString(); ;
                    }

                    else if (statusId == "9")
                    {
                        mkStatus = statusName + ", " + secondaryStatus.ToString();
                    }

                    else
                    {
                        mkStatus = "NO STATUS YET!";
                    }

                }
            }
            else
            {
                if (statusId == "0")
                    mkStatus = "NO STATUS YET!";
                else
                    mkStatus = statusName;//

            }

            return mkStatus;
        }
    }
}
