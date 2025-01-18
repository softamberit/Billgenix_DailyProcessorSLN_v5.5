using System;

namespace BBS.BusinessEntity
{
    public class BOClientInfo
    {
        public BOClientInfo()
        {

        }

        private Boolean IsBillingActive;

        public Boolean IsBillingActive1
        {
            get { return IsBillingActive; }
            set { IsBillingActive = value; }
        }

      

        private string ClientID;

        public string ClientID1
        {
            get { return ClientID; }
            set { ClientID = value; }
        }
        private string ClientName;

        public string ClientName1
        {
            get { return ClientName; }
            set { ClientName = value; }
        }
        private string Address;

        public string Address1
        {
            get { return Address; }
            set { Address = value; }
        }
        private string ContactPerson;

        public string ContactPerson1
        {
            get { return ContactPerson; }
            set { ContactPerson = value; }
        }
        private string ContactNo;

        public string ContactNo1
        {
            get { return ContactNo; }
            set { ContactNo = value; }
        }
        private string PhoneNo;

        public string PhoneNo1
        {
            get { return PhoneNo; }
            set { PhoneNo = value; }
        }
        private string FaxNo;

        public string FaxNo1
        {
            get { return FaxNo; }
            set { FaxNo = value; }
        }
        private string EmailID;

        public string EmailID1
        {
            get { return EmailID; }
            set { EmailID = value; }
        }
      
        private string AddBy;

        public string AddBy1
        {
            get { return AddBy; }
            set { AddBy = value; }
        }
        private DateTime AddDate;

        public DateTime AddDate1
        {
            get { return AddDate; }
            set { AddDate = value; }
        }
        private string ModiBy;

        public string ModiBy1
        {
            get { return ModiBy; }
            set { ModiBy = value; }
        }
        private DateTime ModiDate;

        public DateTime ModiDate1
        {
            get { return ModiDate; }
            set { ModiDate = value; }
        }
        private int ClientIDSerial;

        public int ClientIDSerial1
        {
            get { return ClientIDSerial; }
            set { ClientIDSerial = value; }
        }

        public string ChkBox { get; set; }

        public string JsonString { get; set; }


        private string CustomerID;

        public string CustomerID1
        {
            get { return CustomerID; }
            set { CustomerID = value; }
        }
        private string NetworkID;

        public string NetworkID1
        {
            get { return NetworkID; }
            set { NetworkID = value; }
        }

        private string CustomerName;

        public string CustomerName1
        {
            get { return CustomerName; }
            set { CustomerName = value; }
        }

        private string Attention;

        public string Attention1
        {
            get { return Attention; }
            set { Attention = value; }
        }



        private string BillingTel;

        public string BillingTel1
        {
            get { return BillingTel; }
            set { BillingTel = value; }
        }

        private string BillingEmail;

        public string BillingEmail1
        {
            get { return BillingEmail; }
            set { BillingEmail = value; }
        }

        private string NocTel;

        public string NocTel1
        {
            get { return NocTel; }
            set { NocTel = value; }
        }

        private string NocEmail;

        public string NocEmail1
        {
            get { return NocEmail; }
            set { NocEmail = value; }
        }



        private string AgreeRefNo;

        public string AgreeRefNo1
        {
            get { return AgreeRefNo; }
            set { AgreeRefNo = value; }
        }


        private string RepresentativeName;

        public string RepresentativeName1
        {
            get { return RepresentativeName; }
            set { RepresentativeName = value; }
        }

        private string ContactNumber;

        public string ContactNumber1
        {
            get { return ContactNumber; }
            set { ContactNumber = value; }
        }



        private string EntryID;

        public string EntryID1
        {
            get { return EntryID; }
            set { EntryID = value; }
        }

        private DateTime EntryDate;

        public DateTime EntryDate1
        {
            get { return EntryDate; }
            set { EntryDate = value; }
        }
        private string UpdateID;

        public string UpdateID1
        {
            get { return UpdateID; }
            set { UpdateID = value; }
        }

        private DateTime UpdateDate;

        public DateTime UpdateDate1
        {
            get { return UpdateDate; }
            set { UpdateDate = value; }
        }

        private int CustomerSerial;

        public int CustomerSerial1
        {
            get { return CustomerSerial; }
            set { CustomerSerial = value; }
        }

        private DateTime CustomerEntryDate;





        public DateTime CustomerEntryDate1
        {
            get { return CustomerEntryDate; }
            set { CustomerEntryDate = value; }
        }

        private int ServiceID;

        public int ServiceID1
        {
            get { return ServiceID; }
            set { ServiceID = value; }
        }

        private DateTime StartDate;

        public DateTime StartDate1
        {
            get { return StartDate; }
            set { StartDate = value; }
        }

        private DateTime EndDate;

        public DateTime EndDate1
        {
            get { return EndDate; }
            set { EndDate = value; }
        }



        private int SerialNumber;

        public int SerialNumber1
        {
            get { return SerialNumber; }
            set { SerialNumber = value; }
        }



        private float Quantity;

        public float Quantity1
        {
            get { return Quantity; }
            set { Quantity = value; }
        }



        private Boolean IsVat;

        public Boolean IsVat1
        {
            get { return IsVat; }
            set { IsVat = value; }
        }



        private string ServiceType;

        public string ServiceType1
        {
            get { return ServiceType; }
            set { ServiceType = value; }
        }


        // for collection master table

        private int CollectionID;

        public int CollectionID1
        {
            get { return CollectionID; }
            set { CollectionID = value; }
        }

        private DateTime CollectionDate;

        public DateTime CollectionDate1
        {
            get { return CollectionDate; }
            set { CollectionDate = value; }
        }

        private string InvoiceID;

        public string InvoiceID1
        {
            get { return InvoiceID; }
            set { InvoiceID = value; }
        }

        private DateTime InvoiceDate;

        public DateTime InvoiceDate1
        {
            get { return InvoiceDate; }
            set { InvoiceDate = value; }
        }

        private float CollectionAmount;

        public float CollectionAmount1
        {
            get { return CollectionAmount; }
            set { CollectionAmount = value; }
        }

        private float DueAmount;

        public float DueAmount1
        {
            get { return DueAmount; }
            set { DueAmount = value; }
        }

        private float AdjustmentAmount;

        public float AdjustmentAmount1
        {
            get { return AdjustmentAmount; }
            set { AdjustmentAmount = value; }
        }

        private string ModeOfPayment;

        public string ModeOfPayment1
        {
            get { return ModeOfPayment; }
            set { ModeOfPayment = value; }
        }

        private int BillMonth;

        public int BillMonth1
        {
            get { return BillMonth; }
            set { BillMonth = value; }
        }

        private int BillYear;

        public int BillYear1
        {
            get { return BillYear; }
            set { BillYear = value; }
        }

   



        // for 

        private float CurrentInvoiceAmount;

        public float CurrentInvoiceAmount1
        {
            get { return CurrentInvoiceAmount; }
            set { CurrentInvoiceAmount = value; }
        }


        private float AdvanceAmount;

        public float AdvanceAmount1
        {
            get { return AdvanceAmount; }
            set { AdvanceAmount = value; }
        }
        private float NetPay;

        public float NetPay1
        {
            get { return NetPay; }
            set { NetPay = value; }
        }
        private DateTime CreationDate;

        public DateTime CreationDate1
        {
            get { return CreationDate; }
            set { CreationDate = value; }
        }

        private bool IsDuplicate;

        public bool IsDuplicate1
        {
            get { return IsDuplicate; }
            set { IsDuplicate = value; }
        }

        private int InvoiceIDSerial;

        public int InvoiceIDSerial1
        {
            get { return InvoiceIDSerial; }
            set { InvoiceIDSerial = value; }
        }
        private int CollectionStatus;

        public int CollectionStatus1
        {
            get { return CollectionStatus; }
            set { CollectionStatus = value; }
        }

        private bool IsCommission;

        public bool IsCommission1
        {
            get { return IsCommission; }
            set { IsCommission = value; }
        }
        private int InvoiceSerial;

        public int InvoiceSerial1
        {
            get { return InvoiceSerial; }
            set { InvoiceSerial = value; }
        }


        private DateTime BillingStartDate;

        public DateTime BillingStartDate1
        {
            get { return BillingStartDate; }
            set { BillingStartDate = value; }
        }
        private DateTime BillingEndDate;

        public DateTime BillingEndDate1
        {
            get { return BillingEndDate; }
            set { BillingEndDate = value; }
        }

        private int BID;

        public int BID1
        {
            get { return BID; }
            set { BID = value; }

        }

        private string Bandwidth;

        public string Bandwidth1
        {
            get { return Bandwidth; }
            set { Bandwidth = value; }
        }

        public int PopId
        {
            get { return POPId; }
            set { POPId = value; }
        }

        public int RouterId
        {
            get { return _routerId; }
            set { _routerId = value; }
        }

        public int OltId
        {
            get { return OLTId; }
            set { OLTId = value; }
        }

        public int SplitterId
        {
            get { return _splitterId; }
            set { _splitterId = value; }
        }

        public int OntId
        {
            get { return ONTId; }
            set { ONTId = value; }
        }

        public string Add { get; set; }

        private int POPId;
        private int _routerId;

        private int OLTId;
        private int _splitterId;
        private int ONTId;
        public float MRCAmount { get; set; }


        public int pin_number { get; set; }

        public string ClientId { get; set; }

        public DateTime InstallDate { get; set; }

        public int InstallBy { get; set; }

        public string IPAddress { get; set; }

        public string VLAN { get; set; }

        public string InstallDateSrt { get; set; }

        public int Splitter1Id { get; set; }

        public int Splitter2Id { get; set; }





  
        public string PackageName { get; set; }

  

        public string Status { get; set; }

        public string Password { get; set; }
        public decimal UnitPrice { get; set; }


        public int PopId_p { get; set; }

        public int RouterId_p { get; set; }

        public int OltId_p { get; set; }

        public int Splitter1Id_p { get; set; }

        public int Splitter2Id_p { get; set; }

        public int OntId_p { get; set; }

        public string OnuMac { get; set; }

        public string OnuSerial { get; set; }

        public string PopName { get; set; }

        public string PopAddress { get; set; }

        public string OLTName { get; set; }

        public string OLTAddress { get; set; }

        public string SplitterNameL1 { get; set; }

        public string SplitterAddressL1 { get; set; }

        public string SplitterAddressL2 { get; set; }

        public string OntName { get; set; }

        public string OntAddress { get; set; }

        public string OntMac { get; set; }

        public string SplitterNameL2 { get; set; }

        public string ActivationDateStr { get; set; }
        public DateTime ActivationDate { get; set; }

        public string viewDetails { get; set; }

        public string ChangeTypeId { get; set; }

        public string ChangeTypeName { get; set; }

        public string Description { get; set; }

        public string Option { get; set; }

        public string hiddenFld { get; set; }

        public string hiddenFldChangeTypeId { get; set; }

        public string hiddenFldPopId { get; set; }

        public string hiddenFldRouterId { get; set; }

        public string hiddenFldOltId { get; set; }

        public string hiddenFldSp1Id { get; set; }

        public string hiddenFldSp2Id { get; set; }

        public string hiddenFldOntId { get; set; }

        public string hiddenFldIsttId { get; set; }

        public string CreationDateStr { get; set; }

        public string Remarks { get; set; }

        public bool IsActive { get; set; }

        public bool IsActive1 { get; set; }

        public string Gateway { get; set; }
        public bool IsDeviceCollected { get; set; }
        public float CreditLimit { get; set; }

        public string Username { get; set; }

        public string Hostname { get; set; }
        public string ActivityStatus { get; set; }
        public string ClientMac { get; set; }
        public string SubNetMask { get; set; }
        public string Screen { get; set; }
        public string MRNo{ get; set; }
        public string CustomerEntryDateStr { get; set; }
        public bool IsSendDiscontinue { get; set; }
        public string AddComments { get; set; }
        public string RejectedStatus { get; set; }
        public int ProcessStartDay { get; set; }

        public DateTime DueDate { get; set; }

        public bool IsOldClient { get; set; }

        public int InvoiceCount { get; set; }
    }
}



public class BkashSearchTranDetails
{

    public string amount { get; set; }
    public string completedTime { get; set; }
    public string currency { get; set; }
    public string customerMsisdn { get; set; }
    public string initiationTime { get; set; }
    public string organizationShortCode { get; set; }
    public string transactionReference { get; set; }
    public string transactionStatus { get; set; }
    public string transactionType { get; set; }
    public string trxID { get; set; }












}

public class BkashToken
{
    public string token_type { get; set; }
    public string id_token { get; set; }
    public string expires_in { get; set; }
    public string refresh_token { get; set; }



}



public class BkashTokenKey
{
    public string app_key { get; set; }
    public string app_secret { get; set; }

}

public class BkashRefreshTokenKey
{
    public string app_key { get; set; }
    public string app_secret { get; set; }
    public string refresh_token { get; set; }

}



