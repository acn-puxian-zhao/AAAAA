namespace Intelligent.OTC.Domain.Proxy.Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Intelligent.OTC.Common.Utils;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Intelligent.OTC.Common.Exceptions;

    public class WorkflowClient
    {
        public WorkflowClient(string wfEndPoint)
        {
            workflowEndPoint = wfEndPoint;
        }

        HttpClient client = new HttpClient();
        string workflowEndPoint { get; set; }

        public TaskResult CreateProcessInstance(string processDefinationId, string operId, string referenceNo)
        {
            AssertUtils.ArgumentHasText(processDefinationId, "ProcessDefinationId");
            AssertUtils.ArgumentHasText(operId, "OperatorId");
            AssertUtils.ArgumentHasText(referenceNo, "ReferenceNo");

            try
            {
                Task<HttpResponseMessage> task = client.PostAsJsonAsync<ProcessInstanceDTO>(workflowEndPoint + "CreateProcessInstance",
                    new ProcessInstanceDTO()
                    {
                        ProcessId = long.Parse(processDefinationId),
                        OperatorId = long.Parse(operId),
                        ReferenceNo = referenceNo
                    });

                task.Wait();

                task.Result.EnsureSuccessStatusCode();

                if (!task.Result.IsSuccessStatusCode)
                {
                    Helper.Log.Info(task.Result.Content.ReadAsStringAsync().Result);
                }                

                var res = task.Result.Content.ReadAsAsync<TaskResult>();
                res.Wait();

                return res.Result;
            }
            catch (Exception ex)
            {
                Helper.Log.Error("CreateProcessInstance method run failed", ex);
                throw;
            }
        }

        public void CreateBuckProcessInstance(string processDefinationId, string operId, List<string> referenceNoList, Action<string, TaskResult> instanceStartedCallBack)
        {
            AssertUtils.ArgumentHasText(processDefinationId, "ProcessDefinationId");
            AssertUtils.ArgumentHasText(operId, "OperatorId");
            AssertUtils.ArgumentHasElements(referenceNoList, "ReferenceNoList");

            HttpClient client = new HttpClient();

            TaskFactory tf = new TaskFactory();
            ProcessInstanceDTO process = new ProcessInstanceDTO();

            foreach (string referenceNo in referenceNoList)
            {
                process.ProcessId = long.Parse(processDefinationId);
                process.OperatorId = long.Parse(operId);
                process.ReferenceNo = referenceNo;

                tf.StartNew(() =>
                {
                    Task<HttpResponseMessage> task = client.PostAsJsonAsync<ProcessInstanceDTO>(workflowEndPoint + "CreateProcessInstance", process);
                    task.Wait();

                    var res = task.Result.Content.ReadAsAsync<TaskResult>();
                    res.Wait();

                    instanceStartedCallBack(process.ReferenceNo, res.Result);
                });
            }
        }

        public TaskResult AcceptTask(string operId, string referenceNo, long taskId)
        {
            AssertUtils.ArgumentHasText(operId, "OperatorId");
            AssertUtils.ArgumentHasText(referenceNo, "ReferenceNo");
            AssertUtils.ArgumentHasText(referenceNo, "ReferenceNo");

            Task<HttpResponseMessage> res = client.PostAsJsonAsync<TaskAcceptDTO>(
                workflowEndPoint + "AcceptTask",
                new TaskAcceptDTO() 
                { 
                    ReferenceNo = referenceNo, TaskId = taskId, OperatorId = long.Parse(operId)
                });

            res.Wait();

            var r = res.Result.Content.ReadAsAsync<TaskResult>();
            r.Wait();
            return r.Result;
        }

        public TaskResult FinishTask(string operId, string referenceNo, string taskId)
        {
            AssertUtils.ArgumentHasText(operId, "OperatorId");
            AssertUtils.ArgumentHasText(referenceNo, "ReferenceNo");
            AssertUtils.ArgumentHasText(referenceNo, "ReferenceNo");

            Task<HttpResponseMessage> res = client.PostAsJsonAsync<TaskAcceptDTO>(
                workflowEndPoint + "FinishTask",
                new TaskAcceptDTO()
                {
                    ReferenceNo = referenceNo,
                    TaskId = long.Parse(taskId),
                    OperatorId = long.Parse(operId)
                });

            res.Wait();

            var r = res.Result.Content.ReadAsAsync<TaskResult>();
            r.Wait();
            return r.Result;
        }

        public TaskResult ForeceCompleteProcess(string processDefinitionId, string processInstanceId, string referenceNo)
        {
            AssertUtils.ArgumentHasText(processDefinitionId, "ProcessDefinitionId");
            AssertUtils.ArgumentHasText(processInstanceId, "ProcessInstanceId");
            AssertUtils.ArgumentHasText(referenceNo, "ReferenceNo");

            Task<HttpResponseMessage> res = client.PostAsJsonAsync<TerminateProcessInstanceDTO>(workflowEndPoint + "ForceCompleteProcess", new TerminateProcessInstanceDTO()
            {
                ProcessId = long.Parse(processDefinitionId),
                ProcessInstanceId = processInstanceId,
                ReferenceNo = referenceNo
            });

            res.Wait();

            var r = res.Result.Content.ReadAsAsync<TaskResult>();
            r.Wait();
            return r.Result;
        }

        public TaskResult PauseTask(string operId, string referenceNo, string taskId)
        {
            Task<HttpResponseMessage> res = client.PostAsJsonAsync<TaskAcceptDTO>(workflowEndPoint + "PauseTask"
                , new TaskAcceptDTO()
                {
                    ReferenceNo = referenceNo,
                    TaskId = long.Parse(taskId),
                    OperatorId = long.Parse(operId)
                });
          
            res.Wait();

            var r = res.Result.Content.ReadAsAsync<TaskResult>();
            r.Wait();
            return r.Result;
        }

        public TaskResult RecallTask(string operId, string referenceNo, string taskId)
        {
            Exception ex = new NotImplementedException();
            Helper.Log.Error(ex.Message, ex);
            throw ex;
        }

        public TaskResult ResumePausedTask(string operId, string referenceNo, string taskId)
        {
            Task<HttpResponseMessage> res = client.PostAsJsonAsync<TaskAcceptDTO>(workflowEndPoint + "ResumePausedTask"
                , new TaskAcceptDTO()
                {
                    ReferenceNo = referenceNo,
                    TaskId = long.Parse(taskId),
                    OperatorId = long.Parse(operId)
                });

            res.Wait();

            var r = res.Result.Content.ReadAsAsync<TaskResult>();
            r.Wait();
            return r.Result;
        }

        public TaskResult TerminateProcessInstance(string processDefinationId, string operId, string referenceNo)
        {
            AssertUtils.ArgumentHasText(processDefinationId, "ProcessDefinationId");
            AssertUtils.ArgumentHasText(operId, "OperatorId");
            AssertUtils.ArgumentHasText(referenceNo, "ReferenceNo");

            Task<HttpResponseMessage> task = client.PostAsJsonAsync<ProcessInstanceDTO>(workflowEndPoint + "TerminateProcessInstance",
                new ProcessInstanceDTO()
                {
                    ProcessId = long.Parse(processDefinationId),
                    OperatorId = long.Parse(operId),
                    ReferenceNo = referenceNo
                });

            task.Wait();

            var res = task.Result.Content.ReadAsAsync<TaskResult>();
            res.Wait();

            return res.Result;
        }

        public TaskResult UnAcceptTask(string operId, string referenceNo, string taskId)
        {
            AssertUtils.ArgumentHasText(operId, "OperatorId");
            AssertUtils.ArgumentHasText(referenceNo, "ReferenceNo");
            AssertUtils.ArgumentHasText(referenceNo, "ReferenceNo");

            Task<HttpResponseMessage> res = client.PostAsJsonAsync<TaskAcceptDTO>(
                workflowEndPoint + "UnAcceptTask",
                new TaskAcceptDTO()
                {
                    ReferenceNo = referenceNo,
                    TaskId = long.Parse(taskId),
                    OperatorId = long.Parse(operId)
                });

            res.Wait();

            var r = res.Result.Content.ReadAsAsync<TaskResult>();
            r.Wait();
            return r.Result;
        }
    }
}

