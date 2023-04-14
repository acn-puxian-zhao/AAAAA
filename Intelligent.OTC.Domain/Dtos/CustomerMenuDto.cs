using System;
using System.Collections.Generic;

namespace Intelligent.OTC.Domain.Dtos
{
   public class CustomerMenuDto 
    {
        public string Id { get; set; } 
        public string Menuregion { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string LocalizeCustomerName { get; set; }
        public decimal? Amt { get; set; }
        public bool? NeedSendMail { get; set; }
        public string MailId { get; set; }
        public DateTime? MailDate { get; set; }
        public string BankStatementId { get; set; }
        public string ReconId { get; set; }
        public bool? OnlyReconResult { get; set; }
    }

    public class CustomerMenuDtoPage
    {
        public List<CustomerMenuDto> list;

        public int listCount;
    }
}
