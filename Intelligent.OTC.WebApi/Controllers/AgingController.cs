using Intelligent.OTC.Business.Collection;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "agingreport")]
    public class AgingController : ApiController
    {
        [HttpGet]
        [Route("api/aging/query")]
        public AgingReportDtoPage QueryReport(string region, string legalentity, string custName, string siteUseId, string invoicecode, string status, string docType, string poNum, string soNum, string creditTerm, string invoiceMemo, string eb, string invoiceDateFrom, string invoiceDateTo, string DuedateFrom, string DuedateTo, int pageindex, int pagesize)
        {
            CollectionService service = SpringFactory.GetObjectImpl<CollectionService>("CollectionService");
            return service.QueryAgingReport(region, legalentity, custName, siteUseId, invoicecode, status, docType, poNum, soNum, creditTerm, invoiceMemo, eb, invoiceDateFrom, invoiceDateTo, DuedateFrom, DuedateTo, pageindex, pagesize);
        }

        [HttpGet]
        [Route("api/aging/querysummary")]
        public AgingReportDtoPage QuerySummaryReport(string region, string legalentity, string custName, string siteUseId, string invoicecode, string status, string docType, string poNum, string soNum, string creditTerm, string invoiceMemo, string eb, string invoiceDateFrom, string invoiceDateTo, string DuedateFrom, string DuedateTo, int pageindex, int pagesize)
        {
            CollectionService service = SpringFactory.GetObjectImpl<CollectionService>("CollectionService");
            return service.QueryAgingSummaryReport(region, legalentity, custName, siteUseId, invoicecode, status, docType, poNum, soNum, creditTerm, invoiceMemo, eb, invoiceDateFrom, invoiceDateTo, DuedateFrom, DuedateTo, pageindex, pagesize);
        }

        [HttpGet]
        [Route("api/aging/download")]
        public string DownloadReport(string region, string legalentity, string custName, string siteUseId, string invoicecode, string status, string docType, string poNum, string soNum, string creditTerm, string invoiceMemo, string eb, string invoiceDateFrom, string invoiceDateTo, string DuedateFrom, string DuedateTo)
        {
            CollectionService service = SpringFactory.GetObjectImpl<CollectionService>("CollectionService");
            return service.ExportAgingReportNew(region, legalentity, custName, siteUseId, invoicecode, status, docType, poNum, soNum, creditTerm, invoiceMemo, eb, invoiceDateFrom, invoiceDateTo, DuedateFrom, DuedateTo);
        }
    }
}