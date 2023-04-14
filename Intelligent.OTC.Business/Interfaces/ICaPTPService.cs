using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface ICaPTPService
    {
        CaPTPDtoPage getCaPTPList(string customerNum,string legalEntity, string customerCurrency, string invCurrency, string amt, string localAmt, string ptpDateF, string ptpDateT);
    }

}
