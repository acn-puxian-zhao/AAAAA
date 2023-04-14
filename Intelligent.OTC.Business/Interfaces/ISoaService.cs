using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using Intelligent.OTC.Common.UnitOfWork;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.Dtos;

namespace Intelligent.OTC.Business
{
    public interface ISoaService
    {
        IEnumerable<SoaDto> GetSoaList(string invoiceState = "", string invoiceTrackState = "", string legalEntity = "", string invoiceNum = "", string soNum = "", string poNum = "", string invoiceMemo = "", string customerNum = "", string customerName = "", string customerClass = "", string siteUseId = "",string EB = "");
        IEnumerable<SoaDto> GetNoPaging(string ListType);
        void SaveComm(int invid, string comm,string commDate);
        void BatchSaveComm(string invids, string comm,string commDate);
        void BatchSoa(string Cusnums,string siteUseId);
        IEnumerable<SendSoaHead> CreateSoa(string ColSoa, string Type);
        CollectorAlert GetSoa(string TaskNo);
        CollectorAlert GetStatus(string ReferenceNo);
        IEnumerable<InvoiceLog> GetInvLog(string InvNum);
        IEnumerable<T_Invoice_Detail> GetInvoiceDetail(string InvNum);
        int GetPStatus(string referenceNo, string status);
        void Wfchange(string processDefinationId, string referenceNo, string type);
        void UpdateAlert(string[] cusnums, string TaskId, string ProcessId,string causeObjectNum, string status, int type);
        IEnumerable<CustomerPaymentBank> GetSoaPayment(string CustNumFPb);
        IEnumerable<CustomerPaymentCircle> GetSoaPaymentCircle(string CustNumFPc, string SiteUseIdFPc);
        IEnumerable<ContactorDomain> GetSoaContactDomain(string CustNumFPd);
        IEnumerable<ContactHistory> GetContactHistory(string CustNumsFCH);
        int sendSoaSaveInfoToDB(MailTmp mailInstance, List<int> invs, int inputAlertType = -1, string Collector = "", string legalEntity="", string CustomerNum = "", string SiteUseId = "", string toTitle = "", string toName = "", string ccTitle = "", int periodId = 0, string TempleteLanguage = "");
        MailTmp GetNewMailInstance(string customerNums,string siteUseId, string templateType, string templatelang, List<int> intIds, string Collector = "", string ToTitle = "", string ToName = "", string CCTitle = "", string ResponseDate="", string Region="", string indexFile = "", string fileType = "XLS");
        MailTmp GetPmtMailInstance(string customerNums, string siteUseId, string templateType, string templatelang);
        MailTmp GetCaPmtMailInstance(string EID, string strId, string strBsId, string strLegalEntity, string strCustomerNum, string strSiteUseId, string templateType, string templatelang, string indexFile = "",  string fileType = "XLS");
        MailTmp GetCaClearMailInstance(string EID, string strId,string strbsid, string strLegalEntity, string strCustomerNum, string strSiteUseId, string templateType, string templatelang, string indexFile = "",  string fileType = "XLS");
        void SaveNotes(string Cus, string SpNotes);
        IEnumerable<PeriodControl> GetAllPeriod();
        IEnumerable<SoaDto> SelectChangePeriod(int PeriodId);
        int CheckPermission(string ColSoa);
        IEnumerable<SendSoaHead> CreateSoaForArrow(string ColSoa, string Type, string SiteUsrId);
        IEnumerable<InvoicesStatusDto> GetInvoicesStatusList();
        List<CustomerCommentStatusDto> GetCustomerCommentStatusData();
        string SetInvoicesStatusList();
        string DelInvoicesStatusData();
        string saveCustomerAgingComments(string LegalEntity, string CustomerNo, string SiteUseId, string Comments);
        List<T_LSRFSR_CHANGE> getLSRFSRList();
        List<int> GetAlertAutoSendInvoice(string strCollector, string strdeal, string strLegalEntity, string strCustomerNum, string strSiteUseId, string alertType, string strToTitle, string strToName, string strTempleteLanguage);
        List<SendPMTSummary> GetPmtSendSummaryList();
        List<TaskPmtDto> GetPmtSendDetailList();
        List<MyinvoicesDto> GetInvoiceByIds(List<int> ids);
        List<SendSoaRemindingDetail> GetSoaSendWarningDetail();
        List<SendSoaRemindingSum> GetSoaSendWarningSum();
        List<SendSoaRemindingDetail> GetSoaSendWarningDetailASEAN();
        List<SendSoaRemindingSum> GetSoaSendWarningSumASEAN();
        List<SendSoaRemindingDetail> GetSoaSendWarningDetailANZ();
        List<SendSoaRemindingSum> GetSoaSendWarningSumANZ();
        List<NoContactor> getNoContactorDetail();
        List<NoContactorSummary> getNoContactorSummary();
        List<NoContactorSiteUseId> getNoContactorSiteUseId();
        List<NewCustomerRemdindingSum> getNewCustomerRemindingSum();
        List<NewCustomerRemdindingDetail> getNewCustomerRemindingDetail();
        string CreateSendPMTAttachment(List<TaskPmtDto> sendDetail);
        string CreateSendSoaRemindingAttachment(List<SendSoaRemindingDetail> soaAlertDetail);
        string CreateNoCsSalesRemindingAttachment(List<NoContactorSiteUseId> NoContactorSiteUseIdList, List<NoContactor> NoContactorDetailList);
        string CreateNewCustomerRemindingAttachment(List<NewCustomerRemdindingDetail> newCustomerDetail);
        int BuildContactorByAlert(string deal, string region, string eid, string templeteLanguage, int periodId, int alertType, string customerNum, string toTitle, string toName, string ccTitle);
        string getCustomerName(string strCustomerNum, string strSiteUseId);
        List<int> GetCaPmtMailSendInvoice(string strLegalEntity, string strCustomerNum);
        int sendCaPmtMailSaveInfoToDB(MailTmp mailInstance, string strID);
        IEnumerable<CusExpDateHisDto> getCommDateHistory(string CustomerCode, string SiteUseId);
        IEnumerable<AgingExpDateHisDto> getAgingDateHistory(int InvId);
        
    }
}
