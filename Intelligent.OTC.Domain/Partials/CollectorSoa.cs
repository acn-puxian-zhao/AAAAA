using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class CollectorSoa
    {
        public int Id { get; set; }
        public string Deal { get; set; }
        public string LegalEntity { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string BillGroupCode { get; set; }
        public string BillGroupName { get; set; }
        public Nullable<decimal> Risk { get; set; }
        public Nullable<decimal> Value { get; set; }
        public string Class { get; set; }
        public string TaskId { get; set; }
        public string ReferenceNo { get; set; }
        public string ProcessId { get; set; }
        public string SoaStatus { get; set; }
        public string CauseObjectNumber { get; set; }
        public string Contact { get; set; }
        public Nullable<decimal> CreditLimit { get; set; }
        public Nullable<decimal> TotalAmt { get; set; }
        public Nullable<decimal> CurrentAmt { get; set; }
        public Nullable<decimal> FDueOver90Amt { get; set; }
        public string ExistCont { get; set; }
        public string CusStatus { get; set; }
        public string IsHoldFlg { get; set; }
        public string Operator { get; set; }
        public Nullable<int> BatchType { get; set; }
        public Nullable<decimal> PastDueAmount { get; set; }
    }
}
