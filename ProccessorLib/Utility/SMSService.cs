using BillingERPConn;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProccessorLib.Utility
{
    public class SMSService
    {
        public void SendSMS(string CustomerID, string SMSText, string Mobile)
        {
            DBUtility _idb = new DBUtility();

            Hashtable hashTable = new Hashtable();

            hashTable.Add("CustomerID", CustomerID);
            hashTable.Add("SMSText", SMSText);
            hashTable.Add("Mobile", Mobile);
            _idb.GetDataByProc(hashTable, "sp_insertSMS_Schedule");

        }
    }
}
