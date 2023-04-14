using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Domain.DataModel;

namespace Intelligent.OTC.Business
{
    public interface IContactCustomerService
    {
        IEnumerable<ContactCustomerDto> GetContactCustomer(string invoiceState = "", string invoiceTrackState = "", string legalEntity = "", string invoiceNum = "", string soNum = "", string poNum = "", string invoiceMemo = "");
        
    }
}
