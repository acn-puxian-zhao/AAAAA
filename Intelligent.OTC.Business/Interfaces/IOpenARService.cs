using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface IOpenARService
    {
        OTCRepository CommonRep { get; set; }

        IQueryable<OpenARDto> GetOpenAR(string region);
    }
}
