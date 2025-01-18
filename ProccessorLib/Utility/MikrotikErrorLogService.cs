using BillingERPConn;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProccessorLib.Utility
{
    public class MikrotikErrorLogService
    {
        private int PinNumber;
        readonly DBUtility _idb;

        public MikrotikErrorLogService(int pin)
        {
            PinNumber = pin;
            _idb = new DBUtility();
        }
        public void InsertMikrotikErrorLog(string CustomerID, string RouterID, string Hostname, string IPAddress, string RetMessage, string Statuscode)
        {

            Hashtable ht = new Hashtable();

            ht.Add("CustomerID", CustomerID);
            ht.Add("POPId", RouterID);
            ht.Add("CustomerIP", IPAddress);
            ht.Add("Error_description", RetMessage);
            ht.Add("ProcessID", 100);

            ht.Add("EntryID", PinNumber);
            //ht.Add("IPAddress", IPAddress);
            ht.Add("StatusCode", Statuscode);

            _idb.InsertData(ht, "sp_insertMKCommunication_Errorlog");
        }
    }
}
