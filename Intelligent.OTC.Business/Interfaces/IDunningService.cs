using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using Intelligent.OTC.Common.UnitOfWork;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Business
{
    public interface IDunningService
    {
        IEnumerable<DunningReminderDto> GetDunningList(string invoiceState = "", string invoiceTrackState = "", string invoiceNum = "", string soNum = "", string poNum = "", string invoiceMemo = "");
        void insertDunningReminder(List<string> strCondition, string strType, string strReminderType, DateTime? dtBase = null);

        IEnumerable<SendSoaHead> CreateDun(string ColDun, int AlertType, int AlertId);

        CollectorAlert GetStatus(int AlertId);

        CurrentTracking GetCT(int AlertIdForCT);

        void Wfchange(string processDefinationId, int AlertId, string type, int AlertType);

        IEnumerable<DunningReminderDto> GetNoPaging(string ListType);

        IEnumerable<DunningReminderDto> SelectChangePeriod(int PeriodId);

        IQueryable<DunningReminderConfig> GetDunningConfig(string num,string SiteUseId);

        //DunningReminderConfig GetDefaultConfig();
        void SaveCustConfig(DunningReminderConfig config);

        void SaveConfigBySingle(int AlertId, List<string> list);

        void SaveActionDate(int AlertId, string Date);

        CurrentTracking Calcu(int AlertId);

        MailTmp GetSecondReminderMailInstance(string customerNums,string siteUseIds, string totalInvoiceAmount, string finalReminderDay, List<int> intIds, int templateId = 0);

        MailTmp GetFinalReminderMailInstance(string customerNums, string siteUseIds, string totalInvoiceAmount, string holdDay, List<int> intIds, int templateId = 0);

        List<CollectorAlert> GetEstimatedReminders(List<string> customerNums, string legalEntity = null, DateTime? dtBase = null);
        int CheckPermission(string ColDun);
        List<CollectorAlert> GetEstimatedRemindersForArrow(List<string> customerNums, string siteUseId, string legalEntity = null, DateTime? dtBase = null);
    }
}
