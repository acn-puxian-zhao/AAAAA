using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface ICustAndBankCustService
    {
        CACustomerMappingDtoPage getCustomerMapping(int page,int pageSize, string legalEntity, string customerNum, string bankCustomerName);
        CACustomerMappingDto getCustomerName(string customerNum, string legalEntity);
        int AddOrUpdate(CACustomerMappingDto model);
        void Remove(string id);
        HttpResponseMessage exporAll();
    }

}
