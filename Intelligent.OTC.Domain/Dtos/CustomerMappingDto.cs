using System;
using System.Collections.Generic;

namespace Intelligent.OTC.Domain.Dtos
{
   public class CustomerMappingDto
    {
        public string  Id { get; set; }
        public string  Menuregion { get; set; }
        public string  CustomerNum { get; set; }
        public string  BankCustomerName { get; set; }
        public string  CreateUser { get; set; }
        public DateTime  CreateDate { get; set; }
        public string  ModifyUser { get; set; }
        public DateTime  ModifyDate { get; set; }
    }

    public class CustomerMappingDtoPage
    {
        public List<CustomerMappingDto> list;

        public int listCount;
    }
}
