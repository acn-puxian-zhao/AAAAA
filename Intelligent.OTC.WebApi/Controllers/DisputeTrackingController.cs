using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.WebApi.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "dispute")]
    public class DisputeTrackingController : ApiController
    {
        [HttpGet]
        [PagingQueryable]
        public IEnumerable<DisputeTrackingView> Get(string InvoiceNum)
        {
            DisputeTrackingService service = SpringFactory.GetObjectImpl<DisputeTrackingService>("DisputeTrackingService");
            var result = service.GetDisputeDatas(InvoiceNum).AsQueryable();
            return result;
        }

        [HttpGet]
        public List<string> GetDisputeById(int id)
        {
            DisputeTrackingService service = SpringFactory.GetObjectImpl<DisputeTrackingService>("DisputeTrackingService");
            return service.GetDisputeById(id);
        }

        [HttpGet]
        public IEnumerable<DisputeTracking> GetDisputeInvoiceDatas(int disId)
        {
            DisputeTrackingService service = SpringFactory.GetObjectImpl<DisputeTrackingService>("DisputeTrackingService");
            return service.GetDisputeInvoiceDatas(disId).AsQueryable();
        }

        [HttpGet]
        public IEnumerable<DisputeHis> GetDisputeStatusChange(int disputeid)
        {
            DisputeTrackingService service = SpringFactory.GetObjectImpl<DisputeTrackingService>("DisputeTrackingService");
            return service.GetDisputeStatusChange(disputeid).AsQueryable();
        }

        //save Notes
        [HttpPost]
        public void SaveNotes([FromBody]List<string> list)
        {
            DisputeTrackingService service = SpringFactory.GetObjectImpl<DisputeTrackingService>("DisputeTrackingService");
            service.SaveNotes(int.Parse(list[0]), list[1]);
        }

        [HttpPost]
        [Route("api/disputeTracking/sendMail")]
        public void SendDisputeMail([FromBody]SendMailDto mailDto)
        {
            DisputeTrackingService service = SpringFactory.GetObjectImpl<DisputeTrackingService>("DisputeTrackingService");
            service.SendDisputeMail(mailDto);
        }
        [HttpPost]
        [Route("api/disputeTracking/generate")]
        public MailTmp GetSOAMailInstance(string customerNums, string siteUseId, string temptype, [FromBody]List<int> intIds, string fileType)
        {
            IMailService Mailservice = SpringFactory.GetObjectImpl<IMailService>("MailService");
            string language = Mailservice.getCustomerLanguageByCusnum(customerNums, siteUseId);

            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.GetNewMailInstance(customerNums, siteUseId, temptype, language, intIds, fileType);
        }
    }
}