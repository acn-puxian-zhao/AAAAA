using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.WebApi.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "common")]
    public class SitesController : ApiController
    {
        [HttpGet]
        [Queryable]
        public IQueryable<Sites> Get(string siteCode)
        {
            SiteService basedata = SpringFactory.GetObjectImpl<SiteService>("SiteService");
            var dataList = basedata.GetSites(siteCode);
            return dataList.AsQueryable();
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Sites> Get()
        {
            SiteService basedata = SpringFactory.GetObjectImpl<SiteService>("SiteService");
            var dataList = basedata.GetSites().ToList();

            List<Sites> newList = new List<Sites>();
            foreach (var item1 in dataList)
            {
                if (newList.Find(m => m.LegalEntity == item1.LegalEntity) == null)
                {
                    newList.Add(item1);
                }
            }
            
            return newList.AsQueryable();
        }

        public IQueryable<Sites> GetLegalEntity(string type) {
            SiteService basedata = SpringFactory.GetObjectImpl<SiteService>("SiteService");
            var dataList = basedata.GetAllSites().AsQueryable();
            return dataList;
        }
    }
}