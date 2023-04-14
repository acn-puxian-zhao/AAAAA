
namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class SoaDto
    {
        public int Id { get; set; }
        public System.DateTime ActionDate { get; set; }
        public string Deal { get; set; }
        public string TaskId { get; set; }
        public string ReferenceNo { get; set; }
        public string ProcessId { get; set; }
        public string SoaStatus { get; set; }
        public string CauseObjectNumber { get; set; }
        public Nullable<int> BatchType { get; set; }
        public string FailedReason { get; set; }
        public Nullable<int> PeriodId { get; set; }
        public int AlertType { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string BillGroupCode { get; set; }
        public string BillGroupName { get; set; }
        //public string ExistCont { get; set; }
        public string CusStatus { get; set; }
        public string IsHoldFlg { get; set; }
        public string Operator { get; set; }
        //public IEnumerable<string> ContactList { get; set; }
        public IEnumerable<string> LegalEntityList { get; set; }
        //public string LegalEntity { get; set; }
        public Nullable<decimal> CreditLimit { get; set; }
        public Nullable<decimal> TotalAmt { get; set; }
        public Nullable<decimal> CurrentAmt { get; set; }
        public Nullable<decimal> FDueOver90Amt { get; set; }
        public Nullable<decimal> PastDueAmt { get; set; }
        public Nullable<decimal> Risk { get; set; }
        public Nullable<decimal> Value { get; set; }
        public string Class { get; set; }
        //Start add by xuan.wu for Arrow adding
        public string SiteUseId { get;set;}
        public string CreditTrem { get; set; }
        public string CollectorName { get; set; }
        public string Sales { get; set; }
        public Nullable<decimal> Due15Amt { get; set; }
        public Nullable<decimal> Due30Amt { get; set; }
        public Nullable<decimal> Due45Amt { get; set; }
        public Nullable<decimal> Due60Amt { get; set; }
        public Nullable<decimal> Due90Amt { get; set; }
        public Nullable<decimal> Due120Amt { get; set; }
        public Nullable<decimal> TotalFutureDue { get; set; }
        public string CS { get; set; }
        public int? PTP_1 { get; set; }
        public int? PTP_2 { get; set; }
        public int? Remindering { get; set; }
        public int? Dunning_1 { get; set; }
        public int? Dunning_2 { get; set; }
        public Nullable<decimal> overDueAMT { get; set; }
        public Nullable<decimal> arBalance { get; set; }
        public string EB { get; set; }
        //End add by xuan.wu for Arrow adding
        public DateTime? CommentExpirationDate { get; set; }
        //public string Contact
        //{
        //    get
        //    {
        //        StringBuilder res = new StringBuilder();
        //        ContactList.ToList().ForEach(c => res.Append(c).Append(","));
        //        return res.ToString().TrimEnd(',');
        //    }
        //}
        public string LegalEntity
        {
            get
            {
                StringBuilder res = new StringBuilder();
                LegalEntityList.ToList().ForEach(c => res.Append(c).Append(","));
                return res.ToString().TrimEnd(',');
            }
        }
    }
}
