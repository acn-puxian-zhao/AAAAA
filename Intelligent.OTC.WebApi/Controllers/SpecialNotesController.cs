using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Common;
using Intelligent.OTC.Business;
using System.Configuration;
using System.IO;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.WebApi.Core;
using Intelligent.OTC.Domain;
using Intelligent.OTC.Common.Exceptions;
using System.Web.OData;
using Intelligent.OTC.Common.Attr;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "master")]
    public class SpecialNotesController : ApiController
    {
        [HttpGet]
        [PagingQueryable]
        public SpecialNote GetNotes(string customerCode)
        {
            string custNum = "";
            string custSiteUseId = "";
            if (customerCode.Equals("newCust"))
            {
                custNum = "";
                custSiteUseId = "";
            }
            else
            {
                var list = customerCode.Split(',');
                custNum = list[0];
                custSiteUseId = list[1];
            }
            
            SpecialNotesService service = SpringFactory.GetObjectImpl<SpecialNotesService>("SpecialNotesService");
            return service.GetSpecialNotes(custNum, custSiteUseId);
        }

        [HttpPost]
        public int Post([FromBody] List<string> list)
        {
            var custNum=list[0];
            var legal=list[1];
            var note=list[2];
            var siteUseId = list[3];
            SpecialNotesService service = SpringFactory.GetObjectImpl<SpecialNotesService>("SpecialNotesService");
            return service.AddOrUpdateByPara(custNum, siteUseId, legal, note);       
        }

    }
}