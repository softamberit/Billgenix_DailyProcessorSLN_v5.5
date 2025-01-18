using System.Collections;
using BillingERPConn;
using System.Data;
using System;

namespace ProccessorLib.DataAccess
{
    public class DASWIFT
    {

        public DASWIFT()
        {
        }
       DBUtility iDBUtility = new DBUtility();

      

        internal string Payment_validation_asp(string userid, string pwd, string refNo, string amount)
        {
            try
            {
                var amnt = Conversion.TryCastDecimal(amount);
               var result = "";

               if (amnt > 0)
               {

                   Hashtable ht = new Hashtable();
                   ht.Add("userid", userid);
                   ht.Add("user_password", pwd);
                   ht.Add("refNo", refNo);
                   ht.Add("amount", amnt);

                   result= iDBUtility.GetDataByProc(ht, "sp_dbbl_validation").Rows[0][1].ToString();
                  
                   
               }
               return result;
            }
            catch
            {
                return "11";
            }
        }


        internal string Payment_Confirmation_asp(string userid, string pwd, string refNo, string amount, string txnid, string txndate)
        {
            try
            {
                var amnt = Conversion.TryCastDecimal(amount);
                var result = "";

                if (amnt > 0)
                {

 

                    Hashtable ht = new Hashtable();
                    ht.Add("userid", userid);
                    ht.Add("user_password", pwd);
                    ht.Add("refNo", refNo);
                    ht.Add("amount", amnt);

                     ht.Add("transaction_id", txnid);
                    ht.Add("transaction_time", txndate);

                    result= iDBUtility.GetDataByProc(ht, "sp_dbbl_confirmation").Rows[0][1].ToString();

                }
                return result;
            }
            catch
            {
                return "NOT OK";
            }
        }
        internal long Bkash_UpdateReferenceNoByTrnxID(string TransactionNo, string messageId, string ReferenceNo)
        {
            try
            {


                Hashtable ht = new Hashtable();
                ht.Add("TransactionNo", TransactionNo);
                ht.Add("MessageId", messageId);
                ht.Add("ReferenceNo", ReferenceNo);



                return Convert.ToInt64(iDBUtility.InsertData(ht, "spUpdate_BkashWebHookReferenceNo"));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal DataTable BkashWebHookCollectionInsert(string amount, string TransactionNo, string ReferenceNo, string transaction_time)
        {
            try
            {
                var amnt = Conversion.TryCastDecimal(amount);
                DataTable result = null;

                Hashtable ht = new Hashtable();
                ht.Add("ReferenceNo", ReferenceNo);
                ht.Add("amount", amount);
                ht.Add("TransactionNo", TransactionNo);
                ht.Add("transaction_time", transaction_time);


                result = iDBUtility.GetDataByProc(ht, "sp_BkashWebHook_CollectionInsert");

                return result;
            }
            catch
            {
                return null;
            }
        }



    }
}