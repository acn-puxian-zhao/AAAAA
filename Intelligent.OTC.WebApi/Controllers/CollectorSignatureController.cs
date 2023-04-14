using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.DataModel;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;


namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "common")]
    public class CollectorSignatureController : ApiController
    {
        [HttpPost]
        [Route("api/getCustomerByCustomerNum")]
        public CollectorSignature GetCustomerByCustomerNum()
        {
            HttpRequest request = HttpContext.Current.Request;
            string lId = request["languageId"];
            CollectorSignatureService service = SpringFactory.GetObjectImpl<CollectorSignatureService>("CollectorSignatureService");
            return service.GetCollectSignture(lId);
        }

        [HttpPost]
        public string Post([FromBody] List<string> signature)
        {
            CollectorSignatureService service = SpringFactory.GetObjectImpl<CollectorSignatureService>("CollectorSignatureService");
            return service.SaveOrUpdateSign(signature);
        }

    }
}