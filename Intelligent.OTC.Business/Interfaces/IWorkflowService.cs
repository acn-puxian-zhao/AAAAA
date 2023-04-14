using System;
using System.Collections.Generic;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Proxy.Workflow;
namespace Intelligent.OTC.Business
{
    public interface IWorkflowService
    {
        void AcceptTask(long taskId, string referenceNo, string oper);
        TaskResult CancelTask(string processDefId, string referenceNo, string oper, string taskid);
        System.Collections.Generic.List<T_WF_CurrentTask> GetMyTaskList(string operId,List<string> status);
        System.Collections.Generic.List<T_WF_ProcessInstance> GetProcessStatus(string processDefinationId, string referenceNo, string oper, string statues);
        TaskResult PauseProcess(string processDefId, string referenceNo, string oper,string taskid);
        TaskResult StartProcess(string processDefId, string referenceNo, string oper);
        TaskResult ResumeProcess(string processDefId, string referenceNo, string oper, string taskid);
        TaskResult FinishProcess(string processDefId, string referenceNo, string oper, string taskid);

        void Wfchange(string processDefinationId, string referenceNo, string type);
        string GetProcessId(long taskid);
    }
}
