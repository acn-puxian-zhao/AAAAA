using System;
using System.Collections.Generic;

namespace Intelligent.OTC.Domain.Dtos
{
   public class CaCustomerAttributeDto
    {
        public string ID { get; set; }
        public string LegalEntity { get; set; }
        public string Func_Currency { get; set; }
        public string Local_Currency { get; set; }
        public string CAOperator { get; set; }
        public string CUSTOMER_NUM { get; set; }
        public bool? IsFixedBankCharge { get; set; }
        public decimal? BankChargeFrom { get; set; }
        public decimal? BankChargeTo { get; set; }
        public string IsNeedRemittance { get; set; }
        public string IsMustPMTDetail { get; set; }
        public bool? IsJumpBankStatement { get; set; }
        public bool? IsJumpSiteUseId { get; set; }
        public bool? IsMustSiteUseIdApply { get; set; }
        public bool? IsNeedVat  { get; set; }
        public string IsEntryAndWiteOff { get; set; }
        public string CREATE_User { get; set; }
        public DateTime? CREATE_Date { get; set; }
        public string MODIFY_User { get; set; }
        public DateTime? MODIFY_Date { get; set; }
        public bool? IsFactoring { get; set; }

    }

    public class CaCustomerAttributeDtoPage
    {
        public List<CaCustomerAttributeDto> list;

        public int listCount;
    }

}
