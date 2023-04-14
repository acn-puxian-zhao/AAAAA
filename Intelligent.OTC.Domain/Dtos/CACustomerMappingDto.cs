using System;
using System.Collections.Generic;

namespace Intelligent.OTC.Domain.Dtos
{
   public class CACustomerMappingDto
    {
        public string  Id { get; set; }
        public string  LegalEntity { get; set; }
        public string  CustomerNum { get; set; }
        public string  BankCustomerName { get; set; }
        public string CustomerName { get; set; }
        public string LocalizeCustomerName { get; set; }
        public string  CreateUser { get; set; }
        public DateTime  CreateDate { get; set; }
        public string  ModifyUser { get; set; }
        public DateTime  ModifyDate { get; set; }
    }

    public class CACustomerMappingDtoPage
    {
        public List<CACustomerMappingDto> list;

        public int listCount;
    }
}
