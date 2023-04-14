using Intelligent.OTC.Business;
using Intelligent.OTC.Business.Collection;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DomainModel;
using System;
using System.Web.Http;
namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "dashboard")]
    public class DashBoardController : ApiController
    {
        [HttpGet]
        public DashBoardModel Get()
        {
            try
            {
                XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
                var collecotrList = ",";
                var userId = AppContext.Current.User.EID;
                collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");
                IMailService ms = SpringFactory.GetObjectImpl<IMailService>("MailService");
                string nMailAdderss = ms.GetSenderMailAddress();
                CollectionService service = SpringFactory.GetObjectImpl<CollectionService>("CollectionService");
                return service.GetDashboardReport(collecotrList, nMailAdderss);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                return new DashBoardModel();
            }
        }
    }
}