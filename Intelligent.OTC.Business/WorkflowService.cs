using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Proxy.Workflow;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Intelligent.OTC.Business
{
    public class WorkflowService : IWorkflowService 
    {
        public OTCRepository CommonRep { get; set; }

        public WorkflowRepository WFRep { private get; set; }

        public List<T_WF_CurrentTask> GetMyTaskList(string operId,List<string> status)
        {
            var res = (from task in WFRep.GetDbSet<T_WF_CurrentTask>()
                       where task.AcceptPeople == operId && status.ToList().Contains(task.Status)
                      select task).ToList();

            if (res == null)
            {
                return new List<T_WF_CurrentTask>();
            }
            else
            {
                return res;
            }
        }

        public List<T_WF_ProcessInstance> GetProcessStatus(string processDefinationId, string referenceNo, string oper, string statues)
        {
            Int64 ProcessDefine_Id = long.Parse(processDefinationId);
            List<T_WF_ProcessInstance> res = WFRep.GetDbSet<T_WF_ProcessInstance>()
                .Where(
                    proc => proc.ProcessDefine_Id == ProcessDefine_Id
                            && proc.CauseObjectNumber == referenceNo
                            && proc.CreatePerson == oper
                            && proc.Status == statues).ToList();

            if (res != null && res.Count > 0)
            {
                return res;
            }
            else
            {
                return new List<T_WF_ProcessInstance>();
            }
        }

        public TaskResult StartProcess(string processDefId, string referenceNo, string oper)
        {
            var runningProcs = this.GetProcessStatus(processDefId, referenceNo, oper, "Processing");

            if (runningProcs.Count > 0)
            {
                Exception ex = new OTCServiceException("There are running flow for reference number:" + referenceNo + ". Process cannot start again.");
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

            // start flow only if no running process
            WorkflowClient wfApi = new WorkflowClient(WorkflowEndPoint);
            TaskResult tr = null;
            try
            {
                tr = wfApi.CreateProcessInstance(processDefId, oper, referenceNo);
                Helper.Log.Info(string.Format("Workflow instance created for reference No:[{0}] by user Id:[{1}]", referenceNo, oper));
                return tr;
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Error happened while creating workflow instance.", ex);
                throw ex;
            }
        }

        public void AcceptTask(long taskId, string referenceNo, string oper)
        {
            try
            {
                WorkflowClient wfApi = new WorkflowClient(WorkflowEndPoint);

                // 2, Accept SOA task
                wfApi.AcceptTask(oper, referenceNo, taskId);
                Helper.Log.Info(string.Format("Task Id:[{0}] accepted by user Id:[{1}]", taskId, oper));
            }
            catch (Exception ex)
            {
                Helper.Log.Error(string.Format("Error happened while accept the task Id:[0] of the workflow instance.", taskId), ex);
                throw ex;
            }

        }

        public TaskResult CancelTask(string processDefId, string referenceNo, string oper, string taskid)
        {
            var runningProcs = this.GetProcessStatus(processDefId, referenceNo, oper, "Processing");

            if (runningProcs.Count == 0)
            {
                runningProcs = this.GetProcessStatus(processDefId, referenceNo, oper, "Pausing");
            }
            if (runningProcs.Count == 0)
            {
                return null;
            }
            else
            {
                T_WF_ProcessInstance pauseProcess = runningProcs[0];

                // start flow only if there are running process
                WorkflowClient wfApi = new WorkflowClient(WorkflowEndPoint);
                TaskResult tr = null;
                try
                {
                    tr = wfApi.TerminateProcessInstance(processDefId, oper, referenceNo);
                    Helper.Log.Info(string.Format("Workflow instance [{0}] force completed by user [{1}]", pauseProcess.Id.ToString(), oper));
                    return tr;
                }
                catch (Exception ex)
                {
                    Helper.Log.Error("Error happened while creating workflow instance.", ex);
                    throw ex;
                }
            }
        }

        public TaskResult PauseProcess(string processDefId, string referenceNo, string oper, string taskid)
        {
            var runningProcs = this.GetProcessStatus(processDefId, referenceNo, oper, "Processing");

            if (runningProcs.Count == 0)
            {
                throw new OTCServiceException("There are no running flow for reference number:" + referenceNo + ". Process cannot be paused.");
            }

            T_WF_ProcessInstance pauseProcess = runningProcs[0];

            // start flow only if there are running process
            WorkflowClient wfApi = new WorkflowClient(WorkflowEndPoint);
            TaskResult tr = null;
            try
            {
                tr = wfApi.PauseTask(oper, referenceNo, taskid);
                Helper.Log.Info(string.Format("Workflow instance [{0}] force completed by user [{1}]", pauseProcess.Id.ToString(), oper));
                return tr;
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Error happened while creating workflow instance.", ex);
                throw ex;
            }
        }

        public string WorkflowEndPoint
        {
            get
            {
                return ConfigurationManager.AppSettings["WorkflowEndPoint"] as string;
            }
        }

        public TaskResult ResumeProcess(string processDefId, string referenceNo, string oper, string taskid)
        {
            var runningProcs = this.GetProcessStatus(processDefId, referenceNo, oper, "Processing");

            if (runningProcs.Count == 0)
            {
                throw new OTCServiceException("There are no running flow for reference number:" + referenceNo + ". Process cannot be paused.");
            }

            T_WF_ProcessInstance resumeProcess = runningProcs[0];

            // start flow only if there are running process
            WorkflowClient wfApi = new WorkflowClient(WorkflowEndPoint);
            TaskResult tr = null;
            try
            {
                tr = wfApi.ResumePausedTask(oper, referenceNo, taskid);
                Helper.Log.Info(string.Format("Workflow instance [{0}] force completed by user [{1}]", resumeProcess.Id.ToString(), oper));
                return tr;
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Error happened while creating workflow instance.", ex);
                throw ex;
            }
        }

        public TaskResult FinishProcess(string processDefId, string referenceNo, string oper, string taskid) 
        {
            var runningProcs = this.GetProcessStatus(processDefId, referenceNo, oper, "Processing");

            if (runningProcs.Count == 0)
            {
                throw new OTCServiceException("There are no running flow for reference number:" + referenceNo + ". Process cannot be paused.");
            }

            T_WF_ProcessInstance finishProcess = runningProcs[0];

            // start flow only if there are running process
            WorkflowClient wfApi = new WorkflowClient(WorkflowEndPoint);
            TaskResult tr = null;
            try
            {
                tr = wfApi.FinishTask(oper, referenceNo, taskid);
                Helper.Log.Info(string.Format("Workflow instance [{0}] force completed by user [{1}]", finishProcess.Id.ToString(), oper));
                return tr;
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Error happened while creating workflow instance.", ex);
                throw ex;
            }
        }


        //WFchange
        public void Wfchange(string processDefinationId, string referenceNo,string type) 
        {
            string oper = AppContext.Current.User.Id.ToString();
            string deal = AppContext.Current.User.Deal.ToString();
            string eid = AppContext.Current.User.EID.ToString();
            string[] cus = referenceNo.Split(',');
            var alert = CommonRep.GetQueryable<CollectorAlert>().ToList()
                    .FindAll(o => o.Deal == deal && o.CustomerNum == cus[0] && o.AlertType == 1 && o.Eid == eid)
                    .OrderByDescending(o => o.PeriodId).FirstOrDefault();

            if (type == "start")
            {
                var task = StartProcess(processDefinationId, referenceNo, oper);
                AcceptTask(task.TaskId, referenceNo, oper);
                string processid = GetProcessId(task.TaskId);
            }
            else if (type == "cancel")
            {
                CancelTask(processDefinationId, referenceNo, oper, alert.TaskId);
            }
            else if (type == "pause")
            {
                PauseProcess(processDefinationId, referenceNo, oper, alert.TaskId);
            }
            else if (type == "resume")
            {
                ResumeProcess(processDefinationId, referenceNo, oper, alert.TaskId);
            }
            else if (type == "finish")
            {
                FinishProcess(processDefinationId, referenceNo, oper, alert.TaskId);
            }
        }

        //Get ProcessId
        public string GetProcessId(long taskid)
        {
            string oper = AppContext.Current.User.Id.ToString();
            //get processinstanceid
            List<string> status = new List<string>();
            status.Add("Processing");
            var task = GetMyTaskList(oper, status).Find(m => m.Id == taskid);
            string processid = task.ProcessInstance_Id.ToString();

            return processid;
        }
    }
}
