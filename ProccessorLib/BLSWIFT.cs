using BillingERPConn;
using ProccessorLib.DataAccess;
using System.Collections;
using System.Data;

namespace ProccessorLib.BusinessLogic
{
    public class BLSWIFT
    {
        public BLSWIFT()
        {
        }
     

        public static string Payment_validation_asp(string userid, string pwd, string refNo, string amount)
        {
            DASWIFT objStudnet = new DASWIFT();
            return objStudnet.Payment_validation_asp(userid, pwd, refNo, amount);
        }
        public static string Payment_Confirmation_asp(string userid, string pwd, string refNo, string amount, string txnid, string txndate)
        {
            DASWIFT objStudnet = new DASWIFT();
            return objStudnet.Payment_Confirmation_asp(userid, pwd, refNo, amount, txnid, txndate);
        }



        internal static DataTable ChecqToken()
        {
            var dbUtil = new DBUtility();
            return dbUtil.GetDataByProc("Sp_Bkash_GetLastToken");
        }


        internal static long InsertBkashToken(BkashToken resToken, string TransactionNo, string username, string password, BkashTokenKey resTokenKey, string url, string RefreshUrl)
        {
            var dbUtil = new DBUtility();
            var ht = new Hashtable
                {
                    {"TransactionNo", TransactionNo},
                    {"app_key", resTokenKey.app_key},
                    {"app_secret", resTokenKey.app_secret},
                    {"username", username},
                    {"password", password},
                    {"token_type", resToken.token_type},
                    {"id_token", resToken.id_token},
                    {"expires_in", resToken.expires_in},
                    {"refresh_token", resToken.refresh_token},
                    {"Url", url},
                     {"RefreshTokenUrl", RefreshUrl}
                };



            return dbUtil.InsertData(ht, "sp_InsertBkashToken");
        }




        internal static long InsertBkash_SearchTxnAPI_ERRORLog(string id_token, string TransactionNo, string ErrorMessage)
        {
            var dbUtil = new DBUtility();
            var ht = new Hashtable
                {
                    {"TransactionNo", TransactionNo},                   
                    {"id_token", id_token},
                    {"ErrorMessage", ErrorMessage}
                                     
                };


            //return 0;
            return dbUtil.InsertData(ht, "sp_InsertBkash_SearchTxnAPI_ERRORLog");
        }



        internal static long InsertBkashRefreshToken(BkashToken resToken, string TransactionNo, string username, string password, BkashRefreshTokenKey resTokenKey, string url, string RefreshUrl)
        {
            var dbUtil = new DBUtility();
            var ht = new Hashtable
                {
                    {"TransactionNo", TransactionNo},
                    {"app_key", resTokenKey.app_key},
                    {"app_secret", resTokenKey.app_secret},
                    {"username", username},
                    {"password", password},
                    {"token_type", resToken.token_type},
                    {"id_token", resToken.id_token},
                    {"expires_in", resToken.expires_in},
                    {"refresh_token", resToken.refresh_token},
                    {"Url", url},
                     {"RefreshTokenUrl", RefreshUrl}
                };



            return dbUtil.InsertData(ht, "sp_InsertBkashToken");
        }




        internal static long InsertBkashSearchTransactionHistory(string TransactionNo, string Content)
        {
            var dbUtil = new DBUtility();
            var ht = new Hashtable
                {
                    {"TransactionNo", TransactionNo},
                    {"ResponseContent", Content}
                   
                };



            return dbUtil.InsertData(ht, "sp_InsertBkashSerchTxnHistory");
        }

        public static long Bkash_UpdateReferenceNoByTrnxID(string TransactionNo, string messageId, string ReferenceNo)
        {
            DASWIFT objStudnet = new DASWIFT();
            return objStudnet.Bkash_UpdateReferenceNoByTrnxID(TransactionNo, messageId, ReferenceNo);
        }




        public static DataTable BkashWebHookCollectionInsert(string amount, string TransactionNo, string ReferenceNo, string transaction_time)
        {
            DASWIFT objStudnet = new DASWIFT();
            return objStudnet.BkashWebHookCollectionInsert(amount, TransactionNo, ReferenceNo, transaction_time);
        }




    }
}