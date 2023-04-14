using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Common.UnitOfWork;
using Intelligent.OTC.Business;
using Intelligent.OTC.Domain.Repositories;
using Intelligent.OTC.Domain;
using Intelligent.OTC.Common;
using Intelligent.OTC.WebApi.Core;
using System.Web.OData;
using Intelligent.OTC.Common.Attr;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "common")]
    public class EbsController : ApiController
    {
        [HttpGet]
        [EnableQuery]
        public IEnumerable<T_LeglalEB> Get()
        {
            EbService basedata = SpringFactory.GetObjectImpl<EbService>("EbService");
            var dataList = basedata.GetAllEbs().ToList();
                        
            return dataList.AsQueryable();
        }

    }
}