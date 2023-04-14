using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface ITaskService
    {
        List<TaskDto> GetTaskList(string legalEntity, string customerNum, string customerName, string siteUseId, DateTime startDate, string status);
        List<TaskPmtDto> GetTaskPmtList(string legalEntity, string customerNum, string customerName, string siteUseId, string status, string dateF, string dateT);
        string ExportTaskPmtList(string legalEntity, string customerNum, string customerName, string siteUseId, string status, string dateF, string dateT);
        List<TaskPmtDetailDto> GetTaskPmtDetailList(string siteUseId, decimal balanceAmt);
        List<TaskPtpDto> GetTaskPtpList(string legalEntity, string customerNum, string customerName, string siteUseId, string status, string dateF, string dateT);
        string ExportTaskPtpList(string legalEntity, string customerNum, string customerName, string siteUseId, string status, string dateF, string dateT);
        List<TaskPtpDetailDto> GetTaskPtpDetailList(long id);
        List<TaskDisputeDto> GetTaskDisputeList(string legalEntity, string customerNum, string customerName, string siteUseId, string status);
        List<TaskDisputeDetailDto> GetTaskDisputeDetailList(long id);
        List<TaskReminddingDto> GetTaskReminddingList(string legalEntity, string customerNum, string customerName, string siteUseId, DateTime dateF, DateTime dateTS);
        HttpResponseMessage ExportTask(string cLegalEntity, string cCustNum, string cCustName, string cSiteUseId, DateTime cDate, string cStatus);
        HttpResponseMessage ExportSoaDate(string cLegalEntity, string cCustNum, string cCustName, string cSiteUseId);
        //void getTaskDetail(string legalEntity, string custNum, string custName, string siteUseId, DateTime startDate, string status, ref List<TaskDto> list);
        //void getTaskSoa(ref List<TaskDto> list);
        bool NewTask(string deal, string legalEntity, string custNum, string siteUseId, string startDate, string taskType, string taskContent, string taskStatus, string isAuto);
        bool UpdateTask(string taskId, string startDate, string taskType, string taskContent, string taskStatus);
        bool SaveTaskPMT(string customerNum, string siteUseId,string invoiceNum, string status, string comments);
        bool SaveTaskPTP(string Id, string status, string comments);
        bool SaveTaskDispute(string Id, string status, string comments);

        List<TaskSendSoaPmtList> gettaskPMTSendList(int page, int pageSize, out int total);
        List<TaskSendSoaPmtList> gettaskSOASendList(int page, int pageSize, out int total);

        bool sendTaskByUser(string templeteLanguage, string deal, string region, string eid, int periodId, int alertType, string toTitle, string toName, string ccTitle, string customerNum, DateTime responseDate);
    }
}
