using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.WebApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "dunning")]
    public class DunningController : ApiController
    {
        [HttpGet]
        [PagingQueryable]
        public IEnumerable<DunningReminderDto> Get(string invoiceState, string invoiceTrackState, string invoiceNum, string soNum, string poNum, string invoiceMemo)
        {
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");

            return service.GetDunningList(invoiceState, invoiceTrackState, invoiceNum, soNum, poNum, invoiceMemo);
        }

        [HttpGet]
        public IEnumerable<SendSoaHead> CreateDun(string ColDun, int AlertType, int AlertId)
        {
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            return service.CreateDun(ColDun, AlertType, AlertId).AsQueryable();
        }

        //get status of a dunning
        [HttpGet]
        public CollectorAlert GetStatus(int AlertId)
        {
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            return service.GetStatus(AlertId);
        }

        [HttpGet]
        public CurrentTracking GetCT(int AlertIdForCT)
        {
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            return service.GetCT(AlertIdForCT);
        }

        [HttpGet]
        public IEnumerable<DunningReminderDto> SelectChangePeriod(int PeriodId)
        {
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            return service.SelectChangePeriod(PeriodId);
        }

        [HttpGet]
        public IEnumerable<DunningReminderDto> GetNoPaging(string ListType)
        {
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            return service.GetNoPaging(ListType).AsQueryable();
        }

        [HttpPost]
        public void Wfchange(int AlertId, string type, int AlertType)
        {
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            service.Wfchange("4", AlertId, type, AlertType);
        }

        [HttpGet]
        [Queryable]
        public IQueryable<DunningReminderConfig> Get(string customerCode)
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
                var paramsList = customerCode.Split(',');
                custNum = paramsList[0];
                custSiteUseId = paramsList[1];
            }
            
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            var config = service.GetDunningConfig(custNum, custSiteUseId);
            return config.AsQueryable();
        }

        [HttpPost]
        public void SaveActionDate(int AlertId, string Date) {
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            service.SaveActionDate(AlertId, Date);
        }

        [HttpPost]
        public void SaveConfig([FromBody] DunningReminderConfig config)
        {
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            service.SaveCustConfig(config);
        }


        [HttpPost]
        public void SaveConfigBySingle(int AlertId,[FromBody] List<string> list)
        {
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            service.SaveConfigBySingle(AlertId, list);
        }

        [HttpPost]
        public CurrentTracking Calcu(int AlertIdFCal)
        {
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            return service.Calcu(AlertIdFCal);
        }

        [HttpGet]
        public List<CollectorAlert> GetEstimatedReminders(string customerNums, DateTime? dtBase = null)
        {
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            return service.GetEstimatedReminders(customerNums.Split(',').ToList(), dtBase: dtBase);
        }

        [HttpPost]
        [Route("api/dunning/dun")]
        public MailTmp GetDunningMailInstance(string customerNums,string siteUseIds, string totalInvoiceAmount, string reminderOrHoldDay, string alertType, [FromBody]List<int> intIds, int templateId = 0)
        {
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            switch (alertType)
            {
                case "2":
                    return service.GetSecondReminderMailInstance(customerNums, siteUseIds, totalInvoiceAmount, reminderOrHoldDay, intIds, templateId);
                case "3":
                    return service.GetFinalReminderMailInstance(customerNums, siteUseIds, totalInvoiceAmount, reminderOrHoldDay, intIds, templateId);
                default:
                    throw new OTCServiceException("Not a recognized alert type: "+ alertType);
            }
        }

        [HttpGet]
        public int CheckPermission(string ColDun)
        {
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            return service.CheckPermission(ColDun);
        }

    }
}
