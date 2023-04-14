using System;
using System.Collections.Generic;

namespace Intelligent.OTC.Domain.Dtos
{
   public class CAForwarderListDto 
    {
        public string  Id { get; set; }
        public string LegalEntity { get; set; }
        public string  CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string LocalizeCustomerName { get; set; }
        public string ForwardNum { get; set; }
        public string ForwardName { get; set; }
        public string ForwardGroup { get; set; }
        public string  CreateUser { get; set; }
        public DateTime  CreateDate { get; set; }
        public string  ModifyUser { get; set; }
        public DateTime  ModifyDate { get; set; }
    }

    public class CAForwarderListDtoPage
    {
        public List<CAForwarderListDto> list;

        public int listCount;
    }
}
