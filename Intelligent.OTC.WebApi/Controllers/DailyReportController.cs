using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.WebApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "dailyReport")]
    public class DailyReportController : ApiController
    {
        [HttpPost]
        public string Post()
        {
            try
            {
                IBaseDataService service = SpringFactory.GetObjectImpl<IBaseDataService>("BaseDataService");
                string rtn = service.CreateDailyReport();
                return rtn;
            }
            catch (OTCServiceException ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException(ex.Message);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Report creating error!" + ex.Message);
            }
        }

        [HttpGet]
        [PagingQueryable]
        public IQueryable<FileDownloadHistory> Get()
        {
            FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
            return service.getAllReportInfo().ToList().AsQueryable();
        }

        [HttpGet]
        public IEnumerable<CollectorReport> Get(string report)
        {
            IBaseDataService service = SpringFactory.GetObjectImpl<IBaseDataService>("BaseDataService");
            return service.GetCollectorReport();
        }

    }

}