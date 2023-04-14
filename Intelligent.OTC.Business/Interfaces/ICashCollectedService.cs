using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface ICashCollectedService
    {
        OTCRepository CommonRep { get; set; }

        IQueryable<Object> GetCashCollected(int year, int month);
    }
}
