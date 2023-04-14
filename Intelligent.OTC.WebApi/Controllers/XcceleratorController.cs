using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.OData;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "common")]
    public class XcceleratorController : ApiController
    {
        [HttpGet]
        [EnableQuery]
        public IQueryable<SysUser> Get()
        {
            XcceleratorService collector = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
            int regionId = Convert.ToInt32(AppContext.Current.User.RegionId);
            int centerId= Convert.ToInt32(AppContext.Current.User.CenterId);
            int groupId= Convert.ToInt32(AppContext.Current.User.GroupId);
            int dealId= Convert.ToInt32(AppContext.Current.User.DealId);
            int teamId = Convert.ToInt32(AppContext.Current.User.TeamId);
            var collectorList = collector.GetUsers(regionId, centerId, groupId, dealId, teamId, AppContext.Current.User.Deal);
            return collectorList.AsQueryable();
        }
    }
}