using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface ICaTaskService
    {
        CaTaskDtoPage getCaTaskList(string taskType, string status, string taskName, string dateF, string dateT, int page, int pageSize);

        CaTaskDtoPage getCaTaskListByType(int page, int pageSize, string taskType);

        string createTask(int type, string[] bankIds, string fileName, string fileId, DateTime now, int taskStatus = 1);

        void updateTaskStatusById(string id, string status);

        CaTaskDto getCaTaskById(string id);
        void indentifyCallBack(CaCallBackResult callBackResult);
        void unknownCallBack(CaCallBackResult callBackResult);
        void reconCallBack(CaCallBackResult callBackResult);
        void reconTaskCallBack(CaCallBackResult callBackResult);
        void unknownBSCallBack(CaCallBackResult callBackResult);
        void autoIdentifyCallBack(CaCallBackResult callBackResult);
        void autoReconCallBack(CaCallBackResult callBackResult);
        void autoReconTaskCallBack(CaCallBackResult callBackResult);
    }

}
