﻿using System;
using System.Collections;
using System.Data;
using BillingERPConn;

namespace BBSReport
{
    public class BBSClass
    {


        public object NoOfClientDueByMonth(string invoiceMonth, DateTime Sdate, DateTime edate, int popId, string CustomerId)
        {

            DBUtility objDBUitility = new DBUtility();
            var ht = new Hashtable();
            ht.Add("StartDate", Sdate);
            ht.Add("EndDate", edate);
            ht.Add("PopId", popId);
            ht.Add("CustomerId", CustomerId);

            var dtab = objDBUitility.GetDataByProc(ht,"sp_BillSummeryDueCount");

            DataRow[] dr = new DataRow[1];
            dr = dtab.Select("InvoiceMonth='" + invoiceMonth + "'");

            if(dr.Length>0)
             return int.Parse(dr[0][1].ToString());
            else return 0;


        }


        public object NoOfClientDueByPop(string pop, DateTime Sdate, DateTime edate, int popId, string CustomerId)
        {

            var objDBUitility = new DBUtility();
            var ht = new Hashtable();
            ht.Add("StartDate", Sdate);
            ht.Add("EndDate", edate);
            ht.Add("PopId", popId);
            ht.Add("CustomerId", CustomerId);

            var dtab = objDBUitility.GetDataByProc(ht, "sp_BillSummeryDueCountPopwise");

            DataRow[] dr = new DataRow[1];
            dr = dtab.Select("POPName='" + pop + "'");

            if (dr.Length > 0)
                return int.Parse(dr[0][1].ToString());
            else return 0;


        }




        public string MName(int m)
        { 
            string mname;

            if (m == 1) mname = "January";
            else if (m == 2) mname = "February ";
            else if (m == 3) mname = "March ";
            else if (m == 4) mname = "April ";
            else if (m == 5) mname = "May ";
            else if (m == 6) mname = "June ";
            else if (m == 7) mname = "July ";
            else if (m == 8) mname = "August ";
            else if (m == 9) mname = "September ";
            else if (m == 10) mname = "October ";
            else if (m == 11) mname = "November ";
            else  mname = "December ";

            return mname;


             

        }

        public int DayCalCulate(DateTime Sdate, DateTime edate)
        {

            int noofdays;            
            noofdays =(Sdate.Date  - edate.Date ).Days  + 1;
            return noofdays;

        
        
        }




        public String changeNumericToWords(double numb)
        {
            if(numb<0)
            {
                return "Advance(" + Math.Abs(numb).ToString()+")";
            }
            else
            {
                String num = numb.ToString();
                return changeToWords(num, false);
            }

           
        }
        public String changeCurrencyToWords(String numb)
        {
            return changeToWords(numb, true);
        }
        public String changeNumericToWords(String numb)
        {
            return changeToWords(numb, false);
        }
        public String changeCurrencyToWords(double numb)
        {
            return changeToWords(numb.ToString(), true);
        }
        private String changeToWords(String numb, bool isCurrency)
        {
            String val = "", wholeNo = numb, points = "", andStr = "", pointStr = "";
            String endStr = (isCurrency) ? ("Only") : ("");
            try
            {
                int decimalPlace = numb.IndexOf(".");
                if (decimalPlace > 0)
                {
                    wholeNo = numb.Substring(0, decimalPlace);
                    points = numb.Substring(decimalPlace + 1);
                    if (Convert.ToInt32(points) > 0)
                    {
                        andStr = (isCurrency) ? ("and") : ("point");// just to separate whole numbers from points/cents
                        endStr = (isCurrency) ? ("Cents " + endStr) : ("");
                        pointStr = translateCents(points);
                    }
                }
                val = String.Format("{0} {1}{2} {3}", translateWholeNumber(wholeNo).Trim(), andStr, pointStr, endStr);
            }
            catch { ;}
            return val;
        }
        private String translateWholeNumber(String number)
        {
            string word = "";

            try
            {

                bool beginsZero = false;//tests for 0XX

                bool isDone = false;//test if already translated
                double dblAmt = (Convert.ToDouble(number));

                //if ((dblAmt > 0) && number.StartsWith("0"))
                if (dblAmt > 0)
                {//test for zero or digit zero in a nuemric
                    beginsZero = number.StartsWith("0");

                    int numDigits = number.Length;

                    int pos = 0;//store digit grouping

                    String place = "";//digit grouping name:hundres,thousand,etc...
                    switch (numDigits)
                    {

                        case 1://ones' range

                            word = ones(number);
                            isDone = true;

                            break;

                        case 2://tens' range

                            word = tens(number);
                            isDone = true;

                            break;

                        case 3://hundreds' range

                            pos = (numDigits % 3) + 1;
                            place = " Hundred ";

                            break;

                        case 4://thousands' range
                        case 5:

                            pos = (numDigits % 4) + 1;

                            place = " Thousand ";
                            break;

                        case 6://Lakhs' range
                        case 7:

                            pos = (numDigits % 6) + 1;

                            place = " Lac ";
                            break;

                        case 8://Crores' range
                        case 9:

                            pos = (numDigits % 8) + 1;

                            place = " Crore ";
                            break;

                        case 10://Arabs range
                        case 11:

                            pos = (numDigits % 10) + 1;
                            place = " Arab ";

                            break;


                        //add extra case options for anything above Billion...
                        default:

                            isDone = true;
                            break;

                    }
                    if (!isDone)
                    {//if transalation is not done, continue...(Recursion comes in now!!)

                        word = translateWholeNumber(number.Substring(0, pos)) + place + translateWholeNumber(number.Substring(pos));

                        //check for trailing zeros
                        if (beginsZero) word = " and " + word.Trim();

                    }

                    //ignore digit grouping names
                    if (word.Trim().Equals(place.Trim())) word = "";

                }

                String Result = word.Trim();
                Result = Result.Replace("and Hundred", "");
                Result = Result.Replace("and Thousand", "");
                Result = Result.Replace("and Lac", "");
                Result = Result.Replace("and Crore", "");
                Result = Result.Replace(" and ", " ");

                word = Result;
            }
            catch { ;} return word.Trim();

        }
        private String tens(String digit)
        {
            int digt = Convert.ToInt32(digit);
            String name = null;
            switch (digt)
            {
                case 10:
                    name = "Ten";
                    break;
                case 11:
                    name = "Eleven";
                    break;
                case 12:
                    name = "Twelve";
                    break;
                case 13:
                    name = "Thirteen";
                    break;
                case 14:
                    name = "Fourteen";
                    break;
                case 15:
                    name = "Fifteen";
                    break;
                case 16:
                    name = "Sixteen";
                    break;
                case 17:
                    name = "Seventeen";
                    break;
                case 18:
                    name = "Eighteen";
                    break;
                case 19:
                    name = "Nineteen";
                    break;
                case 20:
                    name = "Twenty";
                    break;
                case 30:
                    name = "Thirty";
                    break;
                case 40:
                    name = "Forty";
                    break;
                case 50:
                    name = "Fifty";
                    break;
                case 60:
                    name = "Sixty";
                    break;
                case 70:
                    name = "Seventy";
                    break;
                case 80:
                    name = "Eighty";
                    break;
                case 90:
                    name = "Ninety";
                    break;
                default:
                    if (digt > 0)
                    {
                        name = tens(digit.Substring(0, 1) + "0") + " " + ones(digit.Substring(1));
                    }
                    break;
            }
            return name;
        }
        private String ones(String digit)
        {
            int digt = Convert.ToInt32(digit);
            String name = "";
            switch (digt)
            {
                case 1:
                    name = "One";
                    break;
                case 2:
                    name = "Two";
                    break;
                case 3:
                    name = "Three";
                    break;
                case 4:
                    name = "Four";
                    break;
                case 5:
                    name = "Five";
                    break;
                case 6:
                    name = "Six";
                    break;
                case 7:
                    name = "Seven";
                    break;
                case 8:
                    name = "Eight";
                    break;
                case 9:
                    name = "Nine";
                    break;
            }
            return name;
        }




        private String translateCents(String cents)
        {
            String cts = "", digit = "", engOne = "";
            for (int i = 0; i < cents.Length; i++)
            {
                digit = cents[i].ToString();
                if (digit.Equals("0"))
                {
                    engOne = "Zero";
                }
                else
                {
                    engOne = ones(digit);
                }
                cts += " " + engOne;
            }
            return cts;
        }



    }
}

