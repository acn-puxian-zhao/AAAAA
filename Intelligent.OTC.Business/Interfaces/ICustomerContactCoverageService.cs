using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface ICustomerContactCoverageService
    {
        OTCRepository CommonRep { get; set; }

        IQueryable<Object> GetCustomerContactCount(int year,int month);
    }
}
