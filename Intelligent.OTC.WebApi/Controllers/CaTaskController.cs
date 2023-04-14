using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{

    public class CaTaskController : ApiController
    {
        [HttpGet]
        [Route("api/caTaskController/getCaTaskList")]
        public CaTaskDtoPage getCaTaskList(string taskType,string status,string taskName,string dateF,string dateT, int page, int pageSize)
        {
            ICaTaskService service = SpringFactory.GetObjectImpl<ICaTaskService>("CaTaskService");
            var res = service.getCaTaskList(taskType, status, taskName, dateF, dateT, page, pageSize);
            return res;
        }

        [HttpGet]
        [Route("api/caTaskController/getCaTaskListByType")]
        public CaTaskDtoPage getCaTaskListByType(int page, int pageSize, string taskType)
        {
            ICaTaskService service = SpringFactory.GetObjectImpl<ICaTaskService>("CaTaskService");
            var res = service.getCaTaskListByType(page, pageSize, taskType);
            return res;
        }

        [HttpPost]
        [Route("api/caTaskController/cacallback")]
        public Boolean caCallBack(CaCallBackResult callBackResult)
        {

            Helper.Log.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + ":caCallBack start");
            ICaTaskService service = SpringFactory.GetObjectImpl<ICaTaskService>("CaTaskService");

            if (callBackResult == null)
            {
                Helper.Log.Info("callBackResult is null");
            }
            Helper.Log.Info(callBackResult.type);

            if ("1".Equals(callBackResult.type))
            {
                Helper.Log.Info("callBackResult is 1");
                Helper.Log.Info("callBackResult taskid:" + callBackResult.taskId);
                // Identify回调
                service.indentifyCallBack(callBackResult);
            }
            else if ("2".Equals(callBackResult.type))
            {
                Helper.Log.Info("callBackResult is 2");
                Helper.Log.Info("callBackResult taskid:" + callBackResult.taskId);
                // Unknown回调
                service.unknownCallBack(callBackResult);
            }
            else if ("3".Equals(callBackResult.type))
            {
                Helper.Log.Info("callBackResult is 3");
                Helper.Log.Info("callBackResult taskid:" + callBackResult.taskId);
                // Recon单条回调
                service.reconCallBack(callBackResult);
            }
            else if ("4".Equals(callBackResult.type))
            {
                Helper.Log.Info("callBackResult is 4");
                Helper.Log.Info("callBackResult taskid:" + callBackResult.taskId);
                // Recon task回调
                service.reconTaskCallBack(callBackResult);
            }
            else if ("5".Equals(callBackResult.type))
            {
                Helper.Log.Info("callBackResult is 5");
                Helper.Log.Info("callBackResult taskid:" + callBackResult.taskId);
                // Unknown单条回调
                service.unknownBSCallBack(callBackResult);
            }
            else if ("6".Equals(callBackResult.type))
            {
                Helper.Log.Info("callBackResult is 6");
                Helper.Log.Info("callBackResult taskid:" + callBackResult.taskId);
                // Auto Identify回调
                service.autoIdentifyCallBack(callBackResult);
            }
            else if ("7".Equals(callBackResult.type))
            {
                Helper.Log.Info("callBackResult is 7");
                Helper.Log.Info("callBackResult taskid:" + callBackResult.taskId);
                // Auto Recon单条回调
                service.autoReconCallBack(callBackResult);
            }
            else if ("8".Equals(callBackResult.type))
            {
                Helper.Log.Info("callBackResult is 8");
                Helper.Log.Info("callBackResult taskid:" + callBackResult.taskId);
                // Auto Recon Task回调
                service.autoReconTaskCallBack(callBackResult);
            }
            Helper.Log.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + ":caCallBack end");
            return true;
        }
    }
}
