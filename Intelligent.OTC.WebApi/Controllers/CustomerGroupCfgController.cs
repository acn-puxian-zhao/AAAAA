using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.WebApi.Core;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "master")]
    public class CustomerGroupCfgController : ApiController
    {

        [HttpGet]
        [PagingQueryable]
        public IQueryable<CustomerGroupCfg> Get()
        {
            CustomerGroupCfgService service = SpringFactory.GetObjectImpl<CustomerGroupCfgService>("CustomerGroupCfgService");
            IQueryable<CustomerGroupCfg> grp = service.GetAllGroups();
            return grp;
        }
    }
}