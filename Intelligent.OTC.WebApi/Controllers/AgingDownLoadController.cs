using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using System;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "dataprepare")]
    public class AgingDownloadController : ApiController
    {
        [HttpPost]
        public void Post()
        {
            try
            {
                ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
                service.createAgingReport();
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
    }
}