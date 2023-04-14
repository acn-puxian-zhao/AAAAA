
namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ContactCustomerDto
    {
        public int Id { get; set; }
        public string Deal { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string BillGroupCode { get; set; }
        public string BillGroupName { get; set; }
        public string Class { get; set; }
        public Nullable<decimal> Risk { get; set; }
        public Nullable<decimal> TotalAmt { get; set; }
        public Nullable<decimal> PastDueAmt { get; set; }
        public Nullable<decimal> FDueOver90Amt { get; set; }
        public Nullable<decimal> CreditLimit { get; set; }
        public string Operator { get; set; }
        public string CusStatus { get; set; }
        public string IsHoldFlg { get; set; }
        public string MailFlag { get; set; }
        public IEnumerable<string> LegalEntityList { get; set; }
        //public string LegalEntity { get; set; }
        public string LegalEntity
        {
            get
            {
                StringBuilder res = new StringBuilder();
                LegalEntityList.ToList().ForEach(c => res.Append(c).Append(","));
                return res.ToString().TrimEnd(',');
            }
            set
            { 
                
            }
        }
    }
}
