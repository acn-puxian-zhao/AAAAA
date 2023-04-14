using Intelligent.OTC.Business;
using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "task")]

    public class taskController : ApiController
    {
        [HttpPost]
        //[PagingQueryable]
        [Route("api/task/query")]
        public TaskGridDto QueryTask(int pageindex, int pagesize, string filter, string legalEntity, string custNum, string custName, 
            string siteUseId, DateTime startDate, string status)
        {
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");
            List<TaskDto> list = service.GetTaskList(legalEntity, custNum, custName, siteUseId, startDate, status);
            TaskGridDto grid = new TaskGridDto();
            grid.taskRow = list;
            grid.count = list.Count();
            return grid;
        }
        [HttpPost]
        [Route("api/task/pmtquery")]
        public List<TaskPmtDto> QueryTask_PMT(string legalEntity, string custNum, string custName, string siteUseId, string status, string dateF, string dateT)
        {
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");
            List<TaskPmtDto> taskpmtList = new List<TaskPmtDto>();
            taskpmtList = service.GetTaskPmtList(legalEntity, custNum, custName, siteUseId, status, dateF, dateT);
            return taskpmtList;
        }

        [HttpGet]
        [Route("api/task/exportpmt")]
        public string exportpmt(string legalEntity, string custNum, string custName, string siteUseId, string status, string dateF, string dateT)
        {
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");
            return service.ExportTaskPmtList(legalEntity, custNum, custName, siteUseId, status, dateF, dateT);
        }

        [HttpPost]
        [Route("api/task/pmtdetailquery")]
        public List<TaskPmtDetailDto> QueryTask_PMTDetail(string siteUseId, decimal balanceAmt)
        {
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");
            List<TaskPmtDetailDto> taskpmtdetailList = new List<TaskPmtDetailDto>();
            taskpmtdetailList = service.GetTaskPmtDetailList(siteUseId, balanceAmt);
            return taskpmtdetailList;
        }

        [HttpPost]
        [Route("api/task/ptpquery")]
        public List<TaskPtpDto> QueryTask_PTP(string legalEntity, string custNum, string custName, string siteUseId, string status, string dateF, string dateT)
        {
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");
            List<TaskPtpDto> taskptpList = new List<TaskPtpDto>();
            taskptpList = service.GetTaskPtpList(legalEntity, custNum, custName, siteUseId, status, dateF, dateT);
            return taskptpList;
        }

        [HttpGet]
        [Route("api/task/exportptp")]
        public string exportptp(string legalEntity, string custNum, string custName, string siteUseId, string status, string dateF, string dateT)
        {
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");
            return service.ExportTaskPtpList(legalEntity, custNum, custName, siteUseId, status, dateF, dateT);
        }

        [HttpPost]
        [Route("api/task/queryPTPDetailTask")]
        public List<TaskPtpDetailDto> QueryTask_PTPDetail(long id)
        {
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");
            List<TaskPtpDetailDto> taskptpdetailList = new List<TaskPtpDetailDto>();
            taskptpdetailList = service.GetTaskPtpDetailList(id);
            return taskptpdetailList;
        }

        [HttpPost]
        [Route("api/task/disputequery")]
        public List<TaskDisputeDto> QueryTask_Dispute(string legalEntity, string custNum, string custName, string siteUseId, string status)
        {
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");
            List<TaskDisputeDto> taskDisputeList = new List<TaskDisputeDto>();
            taskDisputeList = service.GetTaskDisputeList(legalEntity, custNum, custName, siteUseId, status);
            return taskDisputeList;
        }

        [HttpPost]
        [Route("api/task/queryDisputeDetailTask")]
        public List<TaskDisputeDetailDto> QueryTask_DisputeDetail(long id)
        {
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");
            List<TaskDisputeDetailDto> taskdisputedetailList = new List<TaskDisputeDetailDto>();
            taskdisputedetailList = service.GetTaskDisputeDetailList(id);
            return taskdisputedetailList;
        }

        [HttpPost]
        [Route("api/task/remindingquery")]
        public List<TaskReminddingDto> QueryTask_Remindding(string legalEntity, string custNum, string custName, string siteUseId, string dateF, string dateT)
        {
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");
            List<TaskReminddingDto> taskReminddingList = new List<TaskReminddingDto>();
            taskReminddingList = service.GetTaskReminddingList(legalEntity, custNum, custName, siteUseId, Convert.ToDateTime(dateF + " 00:00:00"), Convert.ToDateTime(dateT + " 23:59:59"));
            return taskReminddingList;
        }

        [HttpGet]
        [Route("api/task/exporttask")]
        public HttpResponseMessage ExportTask(string cLegalEntity, string cCustNum, string cCustName, string cSiteUseId, DateTime cDate, string cStatus)
        {
            ITaskService service = SpringFactory.GetObjectImpl<TaskService>("TaskService");
            return service.ExportTask(cLegalEntity, cCustNum, cCustName, cSiteUseId, cDate, cStatus);
        }

        [HttpGet]
        [Route("api/task/exportsoadate")]
        public HttpResponseMessage ExportSoaDate(string cLegalEntity, string cCustNum, string cCustName, string cSiteUseId)
        {
            ITaskService service = SpringFactory.GetObjectImpl<TaskService>("TaskService");
            return service.ExportSoaDate(cLegalEntity, cCustNum, cCustName, cSiteUseId);
        }

        [HttpPost]
        [Route("api/task/newtask")]
        public bool NewTask(string deal, string legalEntity, string custNum, string siteUseId, string startDate, string taskType, string taskContent, string taskStatus, string isAuto) {
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");
            return service.NewTask(deal, legalEntity, custNum, siteUseId, startDate, taskType, taskContent, taskStatus, isAuto);
        }
        [HttpPost]
        [Route("api/task/updatetask")]
        public bool UpdateTask(string taskId, string startDate, string taskType, string taskContent, string taskStatus) {
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");
            return service.UpdateTask(taskId, startDate, taskType, taskContent, taskStatus);
        }
        
        [HttpPost]
        [Route("api/task/saveTaskPMT")]
        public bool SaveTaskPMT(string customerNum, string siteUseId, string invoiceNum, string status, string comments)
        {
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");
            if (comments == null) { comments = ""; }
            return service.SaveTaskPMT(customerNum, siteUseId, invoiceNum, status, comments);
        }

        [HttpPost]
        [Route("api/task/saveTaskPTP")]
        public bool SaveTaskPTP(string Id, string status, string comments)
        {
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");
            return service.SaveTaskPTP(Id, status, comments);
        }

        [HttpPost]
        [Route("api/task/saveTaskDispute")]
        public bool SaveTaskDispute(string Id, string status, string comments)
        {
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");
            return service.SaveTaskDispute(Id, status, comments);
        }

        [HttpGet]
        [Route("api/task/gettaskPMTSendList")]
        public PageResultDto<TaskSendSoaPmtList> gettaskPMTSendList(int page, int pageSize)
        {
            var resultDto = new PageResultDto<TaskSendSoaPmtList>();
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");

            int total = 0;
            resultDto.dataRows = service.gettaskPMTSendList(page, pageSize, out total);
            resultDto.count = total;

            return resultDto;
        }

        [HttpGet]
        [Route("api/task/gettaskSOASendList")]
        public PageResultDto<TaskSendSoaPmtList> gettaskSOASendList(int page, int pageSize)
        {
            var resultDto = new PageResultDto<TaskSendSoaPmtList>();
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");

            int total = 0;
            resultDto.dataRows = service.gettaskSOASendList(page, pageSize, out total);
            resultDto.count = total;

            return resultDto;
        }

        [HttpPost]
        [Route("api/task/sendTaskByUser")]
        public bool sendTaskByUser(string templeteLanguage, string deal, string region, string eid, int periodId, int alertType, string toTitle, string toName, string ccTitle, string customerNum, DateTime responseDate)
        {
            ITaskService service = SpringFactory.GetObjectImpl<ITaskService>("TaskService");
            return service.sendTaskByUser(templeteLanguage, deal, region, eid, periodId, alertType, toTitle, toName, ccTitle, customerNum, responseDate);
        }

    }
}