using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.WebApi.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class MailServerController : ApiController
    {
        [HttpGet]
        [PagingQueryable]
        public IEnumerable<T_MailServer> GetAssign()
        {
            IMailServerService service = SpringFactory.GetObjectImpl<IMailServerService>("MailServerService");
            var res = service.GetMailServer("asd");

            return res.AsQueryable();
        }
    }
}
