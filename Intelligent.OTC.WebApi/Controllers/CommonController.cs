using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "common")]
    public class CommonController : ApiController
    {
        [HttpGet]
        public List<CommonService.MyClass> Get()
        {
            CommonService service = SpringFactory.GetObjectImpl<CommonService>("CommonService");
            return service.WorkFlowPendingNum();
        }

        [HttpPost]
        public void KeepAlive()
        {
            if (AppContext.Current.User != null)
            {
                // session is alive
                Helper.Log.Info(string.Format("Keep session: [{0}] alive for user: [{1}], Current timeout: [{2}]", HttpContext.Current.Session.SessionID, AppContext.Current.User.EID, HttpContext.Current.Session.Timeout));
            }
        }

        [HttpGet]
        public void GetServerTime(string dummy)
        { 
            //CultureInfo ci = Environment.
        }

        [HttpGet]
        public IEnumerable<ContactHistory> GetCallList(string customerNum)
        {
            CommonService service = SpringFactory.GetObjectImpl<CommonService>("CommonService");
            return service.GetCallList(customerNum).AsQueryable();
        }
        
        [HttpPost]
        [Route("api/common/UpdateInvoiceStatus")]
        public void UpdateInvoiceStatus(UpdateInvoceStatusDto updateDto)
        {
            DisputeTrackingService service = SpringFactory.GetObjectImpl<DisputeTrackingService>("DisputeTrackingService");
            service.UpdateInvoicesStatus(updateDto.disputeId,updateDto.status, updateDto.invIds);
        }

        [HttpPost]
        public string UpdateStatus(string id, string status, string statusFlg,string mailId,string actionownerdept,string disputereason)
        {
            //statusFlg(1:dispute, 2:break ptp)
            if (statusFlg == "1") {
                int disputeid = int.Parse(id);
                DisputeTrackingService service = SpringFactory.GetObjectImpl<DisputeTrackingService>("DisputeTrackingService");
                return service.UpdateStatus(disputeid, status, actionownerdept, disputereason);
            }
            //Break PTP
            if (statusFlg == "2")
            {
                string[] invoiceId= id.Split(',');
                int[] result = invoiceId.Select(i => int.Parse(i)).ToArray();
                BreakPtpService service = SpringFactory.GetObjectImpl<BreakPtpService>("BreakPtpService");
                service.changeStatus(result, status,mailId);
            }

            //Hold Customer
            if (statusFlg == "3")
            {
                string[] invoiceId = id.Split(',');
                int[] result = invoiceId.Select(i => int.Parse(i)).ToArray();
                HoldCustomerService service = SpringFactory.GetObjectImpl<HoldCustomerService>("HoldCustomerService");
                service.changeStatus(result, status, mailId);
            }

            return "success";
        }

        [HttpGet]
        public CurrentTracking GetTracking(string customerNum, string legalEntity)
        {
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            List<CollectorAlert> reminders = service.GetEstimatedReminders(new List<string>() { customerNum }, legalEntity: legalEntity, dtBase: null);

            // logic to build reminder calendars
            ReminderCalendar calendar = new ReminderCalendar();
            // 1. SOA
            var tracking = calendar.GetTracking(reminders.FindAll(a => a.CustomerNum == customerNum && string.IsNullOrEmpty(a.LegalEntity)));
            // 2. Other reminders
            tracking = calendar.GetTracking(reminders.FindAll(a => a.CustomerNum == customerNum && a.LegalEntity == legalEntity), tracking);
            // 3. Append other information shown in UI;

            IBaseDataService bdSer = SpringFactory.GetObjectImpl<IBaseDataService>("BaseDataService");

            return tracking;
        }
    }
}