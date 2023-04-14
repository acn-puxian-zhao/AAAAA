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
    public interface ICaStatusReportService
    {
        List<CaStatusReportDto> getStatusReport(string valueDateF, string valueDateT);
        HttpResponseMessage exporAll(string valueDateF, string valueDateT);
    }

}
