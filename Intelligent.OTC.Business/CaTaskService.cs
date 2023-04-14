using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using System.Collections.Generic;
using System;
using Intelligent.OTC.Domain.DomainModel;
using System.Linq;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Common;
using System.Transactions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;

namespace Intelligent.OTC.Business
{
    public class CaTaskService : ICaTaskService
    {

        public OTCRepository CommonRep { get; set; }

        public CaTaskDtoPage getCaTaskList(string taskType, string status, string taskName, string dateF, string dateT, int page, int pageSize)
        {
            CaTaskDtoPage result = new CaTaskDtoPage();


            if (string.IsNullOrEmpty(taskType) || taskType == "undefined")
            {
                taskType = "";
            }
            if (string.IsNullOrEmpty(status) || status == "undefined")
            {
                status = "";
            }
            if (string.IsNullOrEmpty(taskName) || taskName == "undefined")
            {
                taskName = "";
            }
            if (string.IsNullOrEmpty(dateF) || dateF == "undefined")
            {
                dateF = "";
            }
            if (string.IsNullOrEmpty(dateT) || dateT == "undefined")
            {
                dateT = "";
            }

            string sql = string.Format(@"SELECT
	                                *
                                FROM
	                                (
		                                SELECT
			                                ROW_NUMBER () OVER (ORDER BY t0.CREATE_TIME DESC) AS RowNumber,
			                                t0.ID AS Id,
			                                t0.TASK_TYPE AS TaskType,
			                                t0.CREATE_TIME AS CreateTime,
			                                t0.UPDATE_TIME AS UpdateTime,
			                                t0.TASK_NAME AS TaskName,
			                                t0.DEL_FLAG AS DelFlag,
			                                t0.STATUS AS Status,
			                                t0.CREATE_USER AS CreateUser,
			                                t0.FILE_ID AS FileId,
			                                t1.DETAIL_NAME AS TaskTypeName,
                                            f.PHYSICAL_PATH AS FilePath,
                                            f.FILE_NAME as FileName,
                                            t3.DETAIL_NAME as StatusName
		                                FROM
			                                T_CA_Task t0 
                                            LEFT JOIN T_FILE f on f.FILE_ID = t0.FILE_ID
                                            LEFT JOIN T_SYS_TYPE_DETAIL t1 ON t1.DETAIL_VALUE = t0.TASK_TYPE AND t1.TYPE_CODE = '082'
                                            LEFT JOIN T_SYS_TYPE_DETAIL t3 ON t3.DETAIL_VALUE = t0.STATUS AND t3.TYPE_CODE = '083'
		                                WHERE
			                                DEL_FLAG = 0
		                                AND TASK_TYPE not in ('1', '2', '8')
		                                AND CREATE_USER = '{0}'
                                        AND ((t0.TASK_TYPE = '{3}') OR '' = '{3}')
                                        AND ((t0.STATUS = '{4}') OR '' = '{4}')
                                        AND ((t0.TASK_NAME = '%{5}%') OR '' = '{5}')
                                        AND (t0.CREATE_TIME >= '{6} 00:00:00' OR t0.UPDATE_TIME >= '{6} 00:00:00' OR '' = '{6}')
                                        AND (t0.CREATE_TIME <= '{7} 23:59:59' OR t0.UPDATE_TIME >= '{7} 00:00:00' OR '' = '{7}')
	                                ) AS t
                                WHERE
	                                RowNumber BETWEEN {1} AND {2}", AppContext.Current.User.EID, page == 1 ? 0 : pageSize * (page - 1) + 1, pageSize * page, taskType, status, taskName, dateF, dateT);


            List<CaTaskDto> dto = CommonRep.ExecuteSqlQuery<CaTaskDto>(sql).ToList();

            string sql1 = string.Format(@"select count(1) as count from T_CA_Task with(nolock) WHERE
			                                DEL_FLAG = 0
		                                AND TASK_TYPE not in ('1', '2')
		                                AND CREATE_USER = '{0}' 
                                        AND ((TASK_TYPE = '{1}') OR '' = '{1}')
                                        AND ((STATUS = '{2}') OR '' = '{2}')
                                        AND ((TASK_NAME = '%{3}%') OR '' = '{3}')
                                        AND (CREATE_TIME >= '{4} 00:00:00' OR UPDATE_TIME >= '{4} 00:00:00' OR '' = '{4}')
                                        AND (CREATE_TIME <= '{5} 23:59:59' OR UPDATE_TIME >= '{5} 00:00:00' OR '' = '{5}')", AppContext.Current.User.EID, taskType, status, taskName, dateF, dateT);

            result.dataRows = dto;
            result.count = SqlHelper.ExcuteScalar<int>(sql1);

            return result;
        }

        public CaTaskDtoPage getCaTaskListByType(int page, int pageSize, string taskType)
        {
            CaTaskDtoPage result = new CaTaskDtoPage();

            taskType = taskType.Replace(",", "','");

            string sql = string.Format(@"SELECT
	                                *
                                FROM
	                                (
		                                SELECT
			                                ROW_NUMBER () OVER (ORDER BY t0.CREATE_TIME DESC) AS RowNumber,
			                                t0.ID AS Id,
			                                t0.TASK_TYPE AS TaskType,
			                                t0.CREATE_TIME AS CreateTime,
			                                t0.UPDATE_TIME AS UpdateTime,
			                                t0.TASK_NAME AS TaskName,
			                                t0.DEL_FLAG AS DelFlag,
			                                t0.STATUS AS Status,
			                                t0.CREATE_USER AS CreateUser,
			                                t0.FILE_ID AS FileId,
			                                t1.DETAIL_NAME AS TaskTypeName,
                                            f.PHYSICAL_PATH AS FilePath,
                                            f.FILE_NAME as FileName,
                                            t3.DETAIL_NAME as StatusName,
                                            CHARINDEX(';', t0.FILE_ID,0) as resultFileFlag
		                                FROM
			                                T_CA_Task t0 
                                            LEFT JOIN T_FILE f on f.FILE_ID = t0.FILE_ID
                                            LEFT JOIN T_SYS_TYPE_DETAIL t1 ON t1.DETAIL_VALUE = t0.TASK_TYPE AND t1.TYPE_CODE = '082'
                                            LEFT JOIN T_SYS_TYPE_DETAIL t3 ON t3.DETAIL_VALUE = t0.STATUS AND t3.TYPE_CODE = '083'
		                                WHERE
			                                DEL_FLAG = 0
		                                AND TASK_TYPE in ('{3}')
		                                AND CREATE_USER = '{0}' 
	                                ) AS t
                                WHERE
	                                RowNumber BETWEEN {1} AND {2}", AppContext.Current.User.EID, page == 1 ? 0 : pageSize * (page - 1) + 1, pageSize * page, taskType);


            List<CaTaskDto> dto = CommonRep.ExecuteSqlQuery<CaTaskDto>(sql).ToList();

            string sql1 = string.Format(@"select count(1) as count from T_CA_Task with(nolock) WHERE
			                                DEL_FLAG = 0
		                                AND TASK_TYPE in ('{1}')
		                                AND CREATE_USER = '{0}' ", AppContext.Current.User.EID, taskType);

            result.dataRows = dto;
            result.count = SqlHelper.ExcuteScalar<int>(sql1);

            return result;
        }

        public string createTask(int type, string[] bankIds, string fileName, string fileId, DateTime now, int taskStatus = 1)
        {
            string taskId = Guid.NewGuid().ToString();
            var insertSql = string.Format(@"
                    INSERT INTO T_CA_Task (
	                    [ID],
	                    [TASK_TYPE],
	                    [TASK_NAME],
	                    [STATUS],
	                    [DEL_FLAG],
	                    [CREATE_TIME],
	                    [CREATE_USER],
                        [FILE_ID]
                    )
                    VALUES
	                    (
		                    N'{0}',
		                    N'{1}',
		                    N'{2}',
		                    N'{3}',
		                    N'{4}',
		                    '{5}',
		                    N'{6}',
		                    N'{7}'
	                    )
                ", taskId,
                type,
                DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + AppContext.Current.User.EID + (string.IsNullOrEmpty(fileName) ? "" : "_" + fileName),
                taskStatus,
                0,
                now,
                AppContext.Current.User.EID,
                fileId);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(insertSql);

            createTaskBS(taskId, bankIds);

            return taskId;
        }

        public void createTaskBS(string taskId, string[] bankIds)
        {
            foreach (var id in bankIds)
            {
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }
                var refInsertSql = string.Format(@"
                    INSERT INTO T_CA_TaskBS (ID, BSID, TASKID)
                    VALUES
	                    ('{0}', '{1}', '{2}')
                ", Guid.NewGuid(), id, taskId);
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(refInsertSql);
            }
        }

        public void updateTaskStatusById(string id, string status)
        {
            string sql = string.Format(@"UPDATE T_CA_Task
                                SET STATUS = '{0}'
                                WHERE
	                                ID = '{1}'", status, id);

            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);
        }

        public void updateTaskStatusCommentsById(string id, string status, string comments, string fileId)
        {
            string sql = string.Format(@"UPDATE T_CA_Task
                                SET STATUS = N'{0}',
                                Comments = N'{1}',
                                FILE_ID =  (CASE WHEN ISNULL(FILE_ID,'') = '' THEN N'{2}' ELSE FILE_ID + ';' + N'{2}' END) 
                                WHERE
	                                ID = '{3}'", status, comments, fileId, id);

            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);
        }

        public CaTaskDto getCaTaskById(string id)
        {
            string sql = string.Format(@"SELECT
	                        ID AS Id,
	                        FILE_ID AS FileId,
	                        Comments AS Comments,
	                        TASK_NAME AS TaskName,
	                        STATUS AS Status,
	                        CREATE_USER AS CreateUser,
	                        TASK_TYPE AS TaskType,
	                        CREATE_TIME AS CreateTime,
	                        UPDATE_TIME AS UpdateTime,
	                        DEL_FLAG AS DelFlag
                        FROM
	                        T_CA_Task
                        WHERE
	                        ID = '{0}'", id);

            List<CaTaskDto> list = CommonRep.ExecuteSqlQuery<CaTaskDto>(sql).ToList();
            if (null != list && list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return new CaTaskDto();
            }
        }

        public void indentifyCallBack(CaCallBackResult callBackResult)
        {
            // 修改task状态
            string status = "2";
            if (!"success".Equals(callBackResult.result))
            {
                status = "3";
            }

            updateTaskStatusCommentsById(callBackResult.taskId, status, callBackResult.comments,"");

            // 解除bank锁定状态
            // lock bank
            string sql = string.Format(@"UPDATE T_CA_BankStatement
                                SET ISLOCKED = 0
                                WHERE
	                                ID IN
                                (SELECT BSID FROM T_CA_TaskBS with(nolock) WHERE TASKID = '{0}')", callBackResult.taskId);

            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);
        }

        public void unknownCallBack(CaCallBackResult callBackResult)
        {
            // 修改task状态
            string status = "2";
            if (!"success".Equals(callBackResult.result))
            {
                status = "3";
            }

            updateTaskStatusCommentsById(callBackResult.taskId, status, callBackResult.comments,"");

            /*这段每结束一条BS就调用了一次*/
            // 根据taskId获取实体
            CaTaskService taskService = SpringFactory.GetObjectImpl<CaTaskService>("CaTaskService");
            CaTaskDto task = taskService.getCaTaskById(callBackResult.taskId);

            //发送任务结束提醒邮件
            IMailSendService service = SpringFactory.GetObjectImpl<IMailSendService>("MailSendService");
            StringBuilder strBody = new StringBuilder();
            DateTime dtStart = task.CreateTime == null ? DateTime.Now : Convert.ToDateTime(task.CreateTime);
            strBody.Append("Task Finished!<br>");
            strBody.Append("TaskType: Unknown<br>");
            strBody.Append("StartTime: " + dtStart.ToString("yyyy-MM-dd HH:mm:ss") + "<br>");
            strBody.Append("EndTime: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "<br>");
            service.sendTaskFinishedMail(strBody.ToString(), task.CreateUser);

        }

        public void reconCallBack(CaCallBackResult callBackResult)
        {
            ICaReconService reconService = SpringFactory.GetObjectImpl<ICaReconService>("CaReconService");
            // 根据taskId获取实体
            CaReconTaskDto taskDto = reconService.getCaReconTaskById(callBackResult.taskId);
            
            CaReconCallBackMsgDto inputCallBackMsgDto = JsonConvert.DeserializeObject<CaReconCallBackMsgDto>(taskDto.INPUT);

            // 获取bank信息
            List<CaBankStatementDto> bankList = inputCallBackMsgDto.bankList;
            List<string> bsIds = new List<string>();
            foreach (var bs in bankList)
            {
                bsIds.Add(bs.ID);
                unlockReconBSById(bs.ID);
            }

            // 若当前output为空则不进行其他操作
            if (string.IsNullOrEmpty(taskDto.OUTPUT))
            {
                return;
            }
            CaReconCallBackMsgDto callBackMsgDto = JsonConvert.DeserializeObject<CaReconCallBackMsgDto>(taskDto.OUTPUT);
            // 获取ar信息
            List<string> arIds = callBackMsgDto.reconResult.ToList<string>();
            reconService.createReconGroupByRecon(bsIds, arIds, callBackMsgDto.comments, callBackMsgDto.MENU_REGION, taskDto.TASK_ID, taskDto.CREATE_USER);
        }

        public void reconTaskCallBack(CaCallBackResult callBackResult)
        {
            // 修改task状态
            string status = "2";
            if (!"success".Equals(callBackResult.result))
            {
                status = "3";
            }

            updateTaskStatusCommentsById(callBackResult.taskId, status, callBackResult.comments,"");

            /*这段每结束一条BS就调用了一次*/
            // 根据taskId获取实体
            CaTaskService taskService = SpringFactory.GetObjectImpl<CaTaskService>("CaTaskService");
            CaTaskDto task = taskService.getCaTaskById(callBackResult.taskId);

            //发送任务结束提醒邮件
            IMailSendService service = SpringFactory.GetObjectImpl<IMailSendService>("MailSendService");
            StringBuilder strBody = new StringBuilder();
            DateTime dtStart = task.CreateTime == null ? DateTime.Now : Convert.ToDateTime(task.CreateTime);
            strBody.Append("Task Finished!<br>");
            strBody.Append("TaskType: Recon<br>");
            strBody.Append("StartTime: " + dtStart.ToString("yyyy-MM-dd HH:mm:ss") + "<br>");
            strBody.Append("EndTime: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "<br>");
            service.sendTaskFinishedMail(strBody.ToString(), task.CreateUser);
        }

        public void unlockReconBSById(string bsId)
        {

            // 解除bank锁定状态
            // lock bank
            string sql = string.Format(@"UPDATE T_CA_BankStatement
                                SET ISLOCKED = 0
                                WHERE
	                                ID = '{1}'", DateTime.Now.ToString(), bsId);

            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);
        }

        public void unknownBSCallBack(CaCallBackResult callBackResult)
        {
            // 解除bank锁定状态
            // lock bank
            string sql = string.Format(@"UPDATE T_CA_BankStatement
                                SET ISLOCKED = 0
                                WHERE
	                                ID = '{0}'", callBackResult.taskId);

            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);
        }

        public void autoIdentifyCallBack(CaCallBackResult callBackResult)
        {
            CaBankStatementService bankService = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
            CaTaskService taskService = SpringFactory.GetObjectImpl<CaTaskService>("CaTaskService");

            // 解除bank锁定状态
            // lock bank
            string sql = string.Format(@"UPDATE T_CA_BankStatement
                                SET ISLOCKED = 0
                                WHERE
	                                ID IN
                                (SELECT BSID FROM T_CA_TaskBS with(nolock) WHERE TASKID = '{0}')", callBackResult.taskId);

            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);

            CaTaskDto task = taskService.getCaTaskById(callBackResult.taskId);

            // 执行PMT unknown
            List<string> unknownBsIds = bankService.getAllAvailableBSIds(task.CreateUser, callBackResult.taskId);

            if (null != unknownBsIds && unknownBsIds.Count > 0)
            {
                // auto identify之后执行PMT unknown操作
                bankService.pmtUnknownCashAdvisor(unknownBsIds, task.Id);
            }
            
            #region AutoRecon 执行完Identify，即先生成Post文件
            CaCommonService commonService = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            string status = task.Status;
            //// auto recon之后执行post & clear操作
            string fileId = commonService.postAndClear("1", getBSIdsByTaskId(callBackResult.taskId), callBackResult.taskId, task.CreateUser, 1);
            updateTaskStatusCommentsById(callBackResult.taskId, status, callBackResult.comments, fileId);

            //发送任务结束提醒邮件
            IMailSendService service = SpringFactory.GetObjectImpl<IMailSendService>("MailSendService");
            StringBuilder strBody = new StringBuilder();
            DateTime dtStart = task.CreateTime == null ? DateTime.Now : Convert.ToDateTime(task.CreateTime);
            strBody.Append("Post Finished!<br>");
            strBody.Append("TaskType: AutoRecon-Post<br>");
            strBody.Append("StartTime: " + dtStart.ToString("yyyy-MM-dd HH:mm:ss") + "<br>");
            strBody.Append("EndTime: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "<br>");
            service.sendTaskFinishedMail(strBody.ToString(), task.CreateUser, fileId.Replace(";", ","));
            #endregion


            List<string> bsIds = bankService.getAllUnmatchAvailableBSIds(task.CreateUser, task.Id);

            // auto identify之后执行recon操作
            bankService.recon(bsIds, callBackResult.taskId, task.CreateUser);

            
        }

        public void autoReconCallBack(CaCallBackResult callBackResult)
        {
            reconCallBack(callBackResult);
        }

        public void autoReconTaskCallBack(CaCallBackResult callBackResult)
        {
            CaCommonService commonService = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            CaTaskService taskService = SpringFactory.GetObjectImpl<CaTaskService>("CaTaskService");
            CaBankStatementService bankService = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");

            // 修改task状态
            string status = "2";
            if (!"success".Equals(callBackResult.result))
            {
                status = "3";
            }
            
            CaTaskDto task = taskService.getCaTaskById(callBackResult.taskId);
            // auto recon之后执行post & clear操作
            string fileId = commonService.postAndClear("1", getBSIdsByTaskId(callBackResult.taskId), callBackResult.taskId, task.CreateUser,1);

            updateTaskStatusCommentsById(callBackResult.taskId, status, callBackResult.comments, fileId);

            // 发送邮件
            /*这段每结束一条BS就调用了一次*/

            //发送任务结束提醒邮件
            IMailSendService service = SpringFactory.GetObjectImpl<IMailSendService>("MailSendService");
            StringBuilder strBody = new StringBuilder();
            DateTime dtStart = task.CreateTime == null ? DateTime.Now : Convert.ToDateTime(task.CreateTime);
            strBody.Append("Task Finished!<br>");
            strBody.Append("TaskType: Recon<br>");
            strBody.Append("StartTime: " + dtStart.ToString("yyyy-MM-dd HH:mm:ss") + "<br>");
            strBody.Append("EndTime: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "<br>");
            service.sendTaskFinishedMail(strBody.ToString(), task.CreateUser, fileId.Replace(";",","));
        }

        public string[] getBSIdsByTaskId(string taskId)
        {
            string sql = string.Format(@"SELECT
	                            BSID
                            FROM
	                            T_CA_TaskBS
                            WHERE
	                            TASKID = '{0}'", taskId);

            return CommonRep.ExecuteSqlQuery<string>(sql).ToArray();
        }
    }
}
