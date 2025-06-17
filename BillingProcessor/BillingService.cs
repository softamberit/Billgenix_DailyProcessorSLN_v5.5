using BillingERPConn;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BBS.Utilitys;
using System.Diagnostics;
using System.Security.Cryptography;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using BillingProcessor.Models;
using Newtonsoft.Json;
using MkCommunication;
namespace BillingProcessor
{
    public enum BillingStatus
    {
        Nothing,
        ExpiredButBalanceAvailable,
        ExpiredAndNoBalance,// Need Bill Generation
        NotExpiredButBalanceAvailable,
        NotExpiredAndNoBalance,

    }

    public enum CustomerStatus
    {
        NoStatusYet = 0,
        ACTIVE = 1,
        DISCONTINUE = 2,
        CANCEL = 3,
        INACTIVE = 9

    }
    public class BillingService
    {


        public static (BillingStatus, CustomerMaster) GetBillingStatus(string customerId, decimal totalDues = 0)
        {
            CustomerStatus customerStatus = new CustomerStatus();
            DBUtility _idb = new DBUtility();
            Hashtable ht = new Hashtable();
            ht.Add("CustomerID", customerId);
          //  var data = _idb.GetO<CustomerMaster>(ht, "sp_getCustInfoforBillingProcessor");
          
            var customer = _idb.GetObjectByProc<CustomerMaster>(ht, "sp_getCustInfoforBillingProcessor");
            ht.Clear();
            decimal debit = 0, credit = 0, cl = 0, requirdAmt = 0;
            DateTime ed = DateTime.MinValue;
            int statusID = 0;
            //foreach (DataRow datarow in dtCustomerInfo.Rows)
            //{
            debit = customer.Debit;
            credit = customer.Credit;
            cl = customer.CreditLimit;
            if (totalDues == 0)
            {
                requirdAmt = customer.NetMRC;
            }
            else
            {
                requirdAmt = totalDues;
            }
            //dsc = customer.Discount;
            ed = customer.EndDate;
            statusID = customer.StatusID;
            switch (statusID)
            {

                case 1:
                    customerStatus = CustomerStatus.ACTIVE;
                    break;
                case 2:
                    customerStatus = CustomerStatus.DISCONTINUE;
                    break;
                case 9:
                    customerStatus = CustomerStatus.INACTIVE;
                    break;
                default:
                    customerStatus = CustomerStatus.NoStatusYet;
                    break;
            }
            customer.CustomerStatus = customerStatus;
            return (BalanceChecker(debit, credit, cl, requirdAmt, ed), customer);
        }

      

        private static BillingStatus BalanceChecker(decimal debit, decimal credit, decimal cl, decimal requirdAmt, DateTime ed)
        {

            DateTime cd = DateTime.Today;
            if (cd > ed)
            {
                decimal inv = debit + requirdAmt;
                decimal ca = credit + cl;

                if (inv > ca)
                {
                    // MK OFF, DIscontinue 
                    return BillingStatus.ExpiredAndNoBalance;
                }
                else if (inv <= ca)
                {
                    // MK ON, INV GEN

                    return BillingStatus.ExpiredButBalanceAvailable;
                }
            }
            else if (cd <= ed)
            {
                decimal inv = debit+ requirdAmt;
                decimal mr = credit + cl;
                if (inv <= mr)
                {
                    // MK ON   NO NEED TO CODE??
                    return BillingStatus.NotExpiredButBalanceAvailable;

                }
                else if (inv > mr)
                {
                    //MK OFF Discontinue

                    return BillingStatus.NotExpiredAndNoBalance;
                }
            }
            return 0;
        }
    }
}
