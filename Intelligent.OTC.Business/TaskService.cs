using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace Intelligent.OTC.Business
{
    public class TaskService : ITaskService
    {
        public OTCRepository CommonRep { get; set; }
        public XcceleratorService XccService { get; set; }

        public SoaService soaService { get; set; }

        public int SupervisorPermission
        {
            get
            {
                if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }

            }
        }

        public List<TaskDto> GetTaskList(string legalEntity, string customerNum, string customerName, string siteUseId, DateTime startDate, string status)
        {
            try
            {
                List<SysUser> listUser = new List<SysUser>();
                listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
                string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
                string collecotrList = "'" + string.Join("','", userGroup.ToArray()) + "'";

                StringBuilder sbselect = new StringBuilder();
                StringBuilder sbselectWhere = new StringBuilder();
                StringBuilder sbOderby = new StringBuilder();
                sbselect.Append(@" SELECT DISTINCT C.Deal as Deal, 
                                          C.Organization AS LegalEntity, 
                                          C.CUSTOMER_NUM AS CustomerNo, 
                                          C.CUSTOMER_NAME AS CustomerName, 
                                          C.SiteUseId AS SiteUseId,
                                          C.COLLECTOR AS Collector, 
                                          C.Ebname as EbName,
                                          C.CREDIT_TREM as CreditTerm,
                                          ca.SALES AS Sales,
										  STUFF((SELECT ';'+ A.LsrNameHist FROM  (SELECT DISTINCT SiteUseId, LsrNameHist FROM dbo.T_INVOICE_AGING (NOLOCK)) AS A  WHERE A.SiteUseId = CA.SiteUseId for xml path('')),1,1,'') AS CS,
                                          C.LastSendDate AS lastsenddate,
                                          VC.CONTACT AS Contactor,
                                          CA.CURRENCY AS Currency, 
                                          CA.TOTAL_AMT AS TotalAr, 
                                          CA.CURRENT_AMT AS NotOverdue, 
                                          CA.DUEOVER_TOTAL_AMT AS Overdue, 
                                          (CA.DUEOVER_TOTAL_AMT - (ISNULL(CA.DUE15_AMT,0) + ISNULL(CA.DUE30_AMT,0) + ISNULL(CA.DUE45_AMT,0))) AS Overdue60,
                                          (ISNULL(CA.DUE180_AMT,0) + ISNULL(CA.DUE270_AMT,0) + ISNULL(CA.DUE360_AMT,0) + ISNULL(CA.DUEOVER360_AMT,0)) AS Overdue120,
                                          (ISNULL(CA.DUE360_AMT,0) + ISNULL(CA.DUEOVER360_AMT,0)) AS Overdue270,
                                          (ISNULL(CA.DUEOVER360_AMT,0)) AS Overdue360,
                                          C.CommentExpirationDate AS CommentExpirationDate,
                                          ((CASE WHEN C.CommentExpirationDate IS NOT NULL AND Convert(varchar(10),C.CommentExpirationDate,120) <= Convert(varchar(10), getdate(),120) THEN  1  ELSE  
										  (
										  case when exists(select 1 from T_INVOICE_AGING as a where a.CUSTOMER_NUM = c.CUSTOMER_NUM and a.SiteUseId = c.siteuseid and a.TRACK_STATES not in ('014','016') and Convert(varchar(10),a.MemoExpirationDate,120) <= Convert(varchar(10), getdate(),120)) then 1 else 0
										  end
										  ) END))  as isExp,
                                          C.PTPDATE AS PtpDate,
                                          C.PTPAMOUNT AS PtpAmount
                                     FROM dbo.T_CUSTOMER AS C WITH (NOLOCK) 
                                LEFT JOIN dbo.T_CUSTOMER_AGING AS CA WITH (NOLOCK) ON C.SiteUseId = CA.SiteUseId
                                LEFT JOIN dbo.V_CUSTOMER_CONTACTOR AS VC WITH (NOLOCK) ON C.SiteUseId = VC.SiteUseId
								LEFT JOIN dbo.T_INVOICE_AGING AS aging WITH (NOLOCK) ON c.SiteUseId = aging.SiteUseId
                                WHERE C.IsActive = '1' ");
                ArrayList list = new ArrayList();
                if (SupervisorPermission != 1)
                {
                    sbselectWhere.Append(@" AND (C.COLLECTOR IN (" + collecotrList + "))");
                }
                //Legal Entity查询条件
                if (!string.IsNullOrEmpty(legalEntity) && legalEntity != "null" && legalEntity != "undefined")
                {
                    sbselectWhere.Append(@" and (C.Organization = @legalEntity ) ");
                    SqlParameter param = new SqlParameter("@legalEntity", legalEntity);
                    list.Add(param);

                }
                //CustomerNum查询条件
                if (!string.IsNullOrEmpty(customerNum) && customerNum != "null" && customerNum != "undefined")
                {
                    sbselectWhere.Append(@" and (C.CUSTOMER_NUM like @customerNum )");
                    SqlParameter param = new SqlParameter("@customerNum", "%" + customerNum + "%");
                    list.Add(param);
                }
                //CustomerName查询条件
                if (!string.IsNullOrEmpty(customerName) && customerName != "null" && customerName != "undefined")
                {
                    sbselectWhere.Append(@" and (C.CUSTOMER_NAME like @customerName )");
                    SqlParameter param = new SqlParameter("@customerName", "%" + customerName + "%");
                    list.Add(param);
                }
                //SiteUseId查询条件
                if (!string.IsNullOrEmpty(siteUseId) && siteUseId != "null" && siteUseId != "undefined")
                {
                    sbselectWhere.Append(@" and (C.SiteUseId like @siteUseId )");
                    SqlParameter param = new SqlParameter("@siteUseId", "%" + siteUseId + "%");
                    list.Add(param);
                }

                SqlParameter[] paramForSQL = new SqlParameter[list.Count];
                int intParam = 0;
                foreach (var item in list)
                {
                    paramForSQL[intParam] = (SqlParameter)item;
                    intParam++;
                }

                sbOderby.Append(@" ORDER BY isExp desc,(CA.DUEOVER_TOTAL_AMT - (ISNULL(CA.DUE15_AMT,0) + ISNULL(CA.DUE30_AMT,0) + ISNULL(CA.DUE45_AMT,0))) DESC ");

                DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString() + sbselectWhere.ToString() + sbOderby.ToString(), System.Data.CommandType.Text, paramForSQL);
                List<TaskDto> taskList = SqlHelper.GetList<TaskDto>(dt);

                //获得Task明细
                DateTime ldt_Start = Convert.ToDateTime(startDate.ToString("yyyy-MM-") + "01 00:00:00");
                DateTime ldt_End = Convert.ToDateTime(startDate.AddMonths(1).ToString("yyyy-MM-") + "01 23:59:59").AddDays(-1);
                SqlParameter[] paramForSQL_Detail = new SqlParameter[list.Count + 2];
                SqlParameter param0 = new SqlParameter("@Start_Date", ldt_Start);
                SqlParameter param1 = new SqlParameter("@End_Date", ldt_End);
                paramForSQL_Detail[0] = param0;
                paramForSQL_Detail[1] = param1;
                int intParam1 = 2;
                foreach (var item in list)
                {
                    paramForSQL_Detail[intParam1] = (SqlParameter)item;
                    intParam1++;
                }

                StringBuilder sbselect_Detail = new StringBuilder();
                StringBuilder sbOderby_Detail = new StringBuilder();
                sbselect_Detail.Append(@" SELECT   T.ID AS TASKID,
                                                       T.DEAL AS DEAL,
                                                       T.LEGAL_ENTITY AS LEGAL_ENTITY,
                                                       T.CUSTOMER_NUM AS CUSTOMER_NUM,
                                                       T.SITEUSEID AS SITEUSEID,
                                                       T.TASK_DATE AS TASK_DATE,
                                                       T.TASK_TYPE AS TASK_TYPE,
                                                       T.TASK_CONTENT AS TASK_CONTENT,
                                                       T.TASK_STATUS AS TASK_STATUS,
                                                       T.ISAUTO AS ISAUTO,
                                                       T.CREATE_USER AS CREATE_USER,
                                                       T.CREATE_DATE AS CREATE_DATE,
                                                       T.UPDATE_USER AS UPDATE_USER,
                                                       T.UPDATE_DATE AS UPDATE_DATE 
                                                  FROM T_TASK as T WITH (NOLOCK) 
                                                  JOIN T_CUSTOMER AS C WITH (NOLOCK) ON T.SITEUSEID = C.SITEUSEID
                                                 WHERE T.isAuto = '0' And (T.TASK_DATE BETWEEN @Start_Date And @End_Date) ");
                
                sbOderby_Detail.Append(@" ORDER BY T.TASK_DATE ASC");
                DataTable dt_Detail = SqlHelper.ExcuteTable(sbselect_Detail.ToString() + sbselectWhere.ToString() + sbOderby_Detail.ToString(), System.Data.CommandType.Text, paramForSQL_Detail);
                List<TaskDetailDto> taskDetailList = SqlHelper.GetList<TaskDetailDto>(dt_Detail);
                
                StringBuilder sbselect_response = new StringBuilder();
                sbselect_response.Append(@" SELECT  T.SiteUseId, Sum(1) as ResponseCount
                                                  FROM T_CONTACT_HISTORY as T WITH (NOLOCK) 
                                                  JOIN T_CUSTOMER AS C WITH (NOLOCK) ON T.SITEUSEID = C.SITEUSEID
                                                 WHERE T.CONTACT_TYPE = 'Response' And (T.CONTACT_DATE BETWEEN @Start_Date And @End_Date) ");
                DataTable dt_response = SqlHelper.ExcuteTable(sbselect_response.ToString() + sbselectWhere.ToString() + " Group by T.SiteUseId ", CommandType.Text, paramForSQL_Detail);
                List<CustomerResponseCount> responseList = SqlHelper.GetList<CustomerResponseCount>(dt_response);


                foreach (TaskDto task in taskList)
                {
                    var curTaskDetails = taskDetailList.Where(o => o.SITEUSEID == task.SiteUseId).Select(x=>x).ToList();
                    task.taskDetail = curTaskDetails;

                    int responseCount = responseList.Where(o => o.SiteUseId == task.SiteUseId).Select(x => x.ResponseCount).FirstOrDefault();
                    task.ResponseTimes = responseCount;

                }

                return taskList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

        }

        public List<TaskPmtDto> GetTaskPmtList(string legalEntity, string customerNum, string customerName, string siteUseId, string status, string dateF, string dateT)
        {

            try
            {
                List<SysUser> listUser = new List<SysUser>();
                listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
                string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
                string collecotrList = "'" + string.Join("','", userGroup.ToArray()) + "'";

                StringBuilder sbselect = new StringBuilder();
                StringBuilder sbOderby = new StringBuilder();
                sbselect.Append(@" SELECT INV.ID        AS ID,
                                      INV.LEGAL_ENTITY  AS LegalEntity, 
                                      INV.CUSTOMER_NUM  AS CustomerNum, 
                                      C.CUSTOMER_NAME   AS CustomerName,
                                      INV.SiteUseId     AS SiteUseId, 
                                      INV.CURRENCY      AS Currency, 
                                      INV.CLASS         AS Class, 
                                      INV.INVOICE_NUM   AS InvoiceNum, 
                                      INV.INVOICE_DATE  AS InvoiceDate, 
                                      INV.BALANCE_AMT   AS BalanceAmt, 
                                      INV.TRACK_STATES  AS TrackStatus,
									  D.DETAIL_NAME     AS TrackStatusName, 
                                      INV.TRACK_DATE    AS TrackDate,
                                      INV.COMMENTS  AS Comments,
                                      INV.hasPmt   as  haspmt
                                FROM T_INVOICE_AGING AS INV WITH (NOLOCK) 
                           LEFT JOIN T_CUSTOMER AS C WITH (NOLOCK) ON C.SiteUseId = INV.SiteUseId
                           LEFT JOIN dbo.T_SYS_TYPE_DETAIL AS D ON D.TYPE_CODE = '029' AND INV.TRACK_STATES = D.DETAIL_VALUE
                                WHERE C.CREDIT_TREM <> 'PREPAYMENT' AND INV.CLASS = 'PMT' AND INV.TRACK_STATES = '000'  ");
                ArrayList list = new ArrayList();
                if (!string.IsNullOrEmpty(status) && status != "undefined")
                {
                    sbselect.Append(@" AND (isnull(INV.hasPMT,'0') = @status)");
                    SqlParameter param = new SqlParameter("@status", status);
                    list.Add(param);
                }
                if (status != "0")
                {
                }
                if (SupervisorPermission != 1)
                {
                    sbselect.Append(@" AND (C.COLLECTOR IN (" + collecotrList + "))");
                }
                //Legal Entity查询条件
                if (!string.IsNullOrEmpty(legalEntity) && legalEntity != "null" && legalEntity != "undefined")
                {
                    sbselect.Append(@" and (INV.LEGAL_ENTITY = @legalEntity ) ");
                    SqlParameter param = new SqlParameter("@legalEntity", legalEntity);
                    list.Add(param);

                }
                //CustomerNum查询条件
                if (!string.IsNullOrEmpty(customerNum) && customerNum != "null" && customerNum != "undefined")
                {
                    sbselect.Append(@" and (INV.CUSTOMER_NUM like @customerNum )");
                    SqlParameter param = new SqlParameter("@customerNum", "%" + customerNum + "%");
                    list.Add(param);
                }
                //CustomerName查询条件
                if (!string.IsNullOrEmpty(customerName) && customerName != "null" && customerName != "undefined")
                {
                    sbselect.Append(@" and (C.CUSTOMER_NAME like @customerName )");
                    SqlParameter param = new SqlParameter("@customerName", "%" + customerName + "%");
                    list.Add(param);
                }
                //SiteUseId查询条件
                if (!string.IsNullOrEmpty(siteUseId) && siteUseId != "null" && siteUseId != "undefined")
                {
                    sbselect.Append(@" and (INV.SiteUseId like @siteUseId )");
                    SqlParameter param = new SqlParameter("@siteUseId", "%" + siteUseId + "%");
                    list.Add(param);
                }

                SqlParameter[] paramForSQL = new SqlParameter[list.Count];
                int intParam = 0;
                foreach (var item in list)
                {
                    paramForSQL[intParam] = (SqlParameter)item;
                    intParam++;
                }

                sbOderby.Append(@" ORDER BY INV.INVOICE_DATE DESC ");

                DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString() + sbOderby.ToString(), System.Data.CommandType.Text, paramForSQL);
                List<TaskPmtDto> taskPmtList = SqlHelper.GetList<TaskPmtDto>(dt);

                return taskPmtList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public string ExportTaskPmtList(string legalEntity, string customerNum, string customerName, string siteUseId, string status, string dateF, string dateT) {

            string templateFile = "";
            string fileName = "";
            string tmpFile = "";

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportTaskPMTTemplate"].ToString());
                fileName = "PMT_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["DailyReportPath"].ToString() + fileName);
                
                List<TaskPmtDto> listData = GetTaskPmtList(legalEntity, customerNum, customerName, siteUseId, status, dateF, dateT);

                this.setPmtData(templateFile, tmpFile, listData);

                HttpRequest request = HttpContext.Current.Request;
                StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
                appUriBuilder.Append(Uri.SchemeDelimiter);
                appUriBuilder.Append(request.Url.Authority);
                if (String.Compare(request.ApplicationPath, @"/") != 0)
                {
                    appUriBuilder.Append(request.ApplicationPath);
                }
                var virPatnName = appUriBuilder.ToString() + ConfigurationManager.AppSettings["DailyReportPath"].ToString().Trim('~') + fileName;
                return virPatnName;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }

        public List<TaskPmtDetailDto> GetTaskPmtDetailList(string siteUseId, decimal balanceAmt)
        {
            try
            {
                StringBuilder sbselect = new StringBuilder();
                sbselect.Append(@" SELECT INV.Id             AS Id, 
                                          INV.INVOICE_NUM       AS InvoiceNum, 
                                          INV.BALANCE_AMT       AS BalanceAmt, 
                                          INV.COMMENTS          AS Comments,
                                          (case when INV.BALANCE_AMT = -@balanceAmt then 1 else 0 end) AS IsFullMatch,
                                          (case when INV.BALANCE_AMT = -@balanceAmt then 'red' else 'black' end) AS Color
                                     FROM T_INVOICE_AGING AS INV WITH (NOLOCK) 
                                    WHERE INV.SiteUseId = @SiteUseId
                                      AND INV.CLASS = 'INV'
                                      AND INV.TRACK_STATES NOT IN ('014','016')
                                 ORDER BY (case when INV.BALANCE_AMT = -@balanceAmt then 1 else 0 end) desc, INV.INVOICE_NUM ASC ");
                SqlParameter[] paramForSQL = new SqlParameter[2];
                paramForSQL[0] = new SqlParameter("@SiteUseId", siteUseId);
                paramForSQL[1] = new SqlParameter("@balanceAmt", balanceAmt);

                DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString(), System.Data.CommandType.Text, paramForSQL);
                List<TaskPmtDetailDto> taskPmtDetailList = SqlHelper.GetList<TaskPmtDetailDto>(dt);

                return taskPmtDetailList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public List<TaskPtpDto> GetTaskPtpList(string legalEntity, string customerNum, string customerName, string siteUseId, string status, string dateF, string dateT)
        {

            try
            {
                List<SysUser> listUser = new List<SysUser>();
                listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
                string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
                string collecotrList = "'" + string.Join("','", userGroup.ToArray()) + "'";

                StringBuilder sbselect = new StringBuilder();
                StringBuilder sbOderby = new StringBuilder();
                sbselect.Append(@" SELECT C.Organization        AS LegalEntity, 
                                          PTP.ID                AS Id,
                                          PTP.CustomerNum      AS CustomerNum, 
                                          C.CUSTOMER_NAME   AS CustomerName,
                                          PTP.SiteUseId         AS SiteUseId, 
                                          PTP.PromiseDate       AS PromiseDate, 
                                          PTP.IsPartialPay      AS IsPartialPay, 
                                          PTP.Payer             AS Payer, 
                                          PTP.PromissAmount     AS PromissAmount, 
                                          PTP.Comments          AS Comments, 
                                          PTP.CreateTime        AS CreateTime, 
                                          PTP.PTPStatus         AS PTPStatus,
										  D.DETAIL_NAME         AS PTPStatusName,
                                          PTP.Status_Date       AS Status_Date
                                FROM T_PTPPayment AS PTP WITH (NOLOCK) 
							JOIN dbo.T_PTPPayment_Invoice AS PTPD WITH (NOLOCK) ON PTP.ID = PTPD.PTPPaymentId
							JOIN dbo.T_INVOICE_AGING AS AGING WITH (NOLOCK) ON AGING.ID = PTPD.InvoiceId 
                           LEFT JOIN T_CUSTOMER AS C WITH (NOLOCK) ON C.SiteUseId = PTP.SiteUseId
                           LEFT JOIN dbo.T_SYS_TYPE_DETAIL AS D WITH (NOLOCK) ON D.TYPE_CODE = '039' AND PTP.PTPStatus = D.DETAIL_VALUE
                               WHERE AGING.TRACK_STATES NOT IN ('014','016') ");
                ArrayList list = new ArrayList();

                switch (status) {
                    case "001":
                        sbselect.Append(@" AND (CONVERT(VARCHAR(10),PTP.PromiseDate,120) >= CONVERT(VARCHAR(10),getdate(),120))");
                        break;
                    case "002":
                        sbselect.Append(@" AND (CONVERT(VARCHAR(10),PTP.PromiseDate,120) < CONVERT(VARCHAR(10),getdate(),120))");
                        break;
                    default:
                        break;
                }
                if (SupervisorPermission != 1)
                {
                    sbselect.Append(@" AND (C.COLLECTOR IN (" + collecotrList + "))");
                }
                //Legal Entity查询条件
                if (!string.IsNullOrEmpty(legalEntity) && legalEntity != "null" && legalEntity != "undefined")
                {
                    sbselect.Append(@" and (C.LEGAL_ENTITY = @legalEntity ) ");
                    SqlParameter param = new SqlParameter("@legalEntity", legalEntity);
                    list.Add(param);

                }
                //CustomerNum查询条件
                if (!string.IsNullOrEmpty(customerNum) && customerNum != "null" && customerNum != "undefined")
                {
                    sbselect.Append(@" and (PTP.CUSTOMER_NUM like @customerNum )");
                    SqlParameter param = new SqlParameter("@customerNum", "%" + customerNum + "%");
                    list.Add(param);
                }
                //CustomerName查询条件
                if (!string.IsNullOrEmpty(customerName) && customerName != "null" && customerName != "undefined")
                {
                    sbselect.Append(@" and (C.CUSTOMER_NAME like @customerName )");
                    SqlParameter param = new SqlParameter("@customerName", "%" + customerName + "%");
                    list.Add(param);
                }
                //SiteUseId查询条件
                if (!string.IsNullOrEmpty(siteUseId) && siteUseId != "null" && siteUseId != "undefined")
                {
                    sbselect.Append(@" and (PTP.SiteUseId like @siteUseId )");
                    SqlParameter param = new SqlParameter("@siteUseId", "%" + siteUseId + "%");
                    list.Add(param);
                }

                SqlParameter[] paramForSQL = new SqlParameter[list.Count];
                int intParam = 0;
                foreach (var item in list)
                {
                    paramForSQL[intParam] = (SqlParameter)item;
                    intParam++;
                }

                sbOderby.Append(@" ORDER BY PTP.PromiseDate ASC ");

                DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString() + sbOderby.ToString(), System.Data.CommandType.Text, paramForSQL);
                List<TaskPtpDto> taskPmtList = SqlHelper.GetList<TaskPtpDto>(dt);

                return taskPmtList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public string ExportTaskPtpList(string legalEntity, string customerNum, string customerName, string siteUseId, string status, string dateF, string dateT)
        {

            string templateFile = "";
            string fileName = "";
            string tmpFile = "";

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportTaskPTPTemplate"].ToString());
                fileName = "PTP_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["DailyReportPath"].ToString() + fileName);
                
                List<TaskPtpDto> listData = GetTaskPtpList(legalEntity, customerNum, customerName, siteUseId, status, dateF, dateT);

                this.setPtpData(templateFile, tmpFile, listData);

                HttpRequest request = HttpContext.Current.Request;
                StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
                appUriBuilder.Append(Uri.SchemeDelimiter);
                appUriBuilder.Append(request.Url.Authority);
                if (String.Compare(request.ApplicationPath, @"/") != 0)
                {
                    appUriBuilder.Append(request.ApplicationPath);
                }
                var virPatnName = appUriBuilder.ToString() + ConfigurationManager.AppSettings["DailyReportPath"].ToString().Trim('~') + fileName;
                return virPatnName;


            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }

        public List<TaskPtpDetailDto> GetTaskPtpDetailList(long id)
        {
            try
            {
                StringBuilder sbselect = new StringBuilder();
                sbselect.Append(@" SELECT PTPINV.Id             AS Id, 
                                          PTPINV.InvoiceId      AS InvoiceId, 
                                          INV.INVOICE_NUM       AS InvoiceNum, 
                                          INV.BALANCE_AMT       AS BalanceAmt, 
                                          INV.PTP_DATE          AS PtpDate, 
                                          INV.COMMENTS          AS Comments
                                     FROM T_PTPPayment_Invoice AS PTPINV WITH (NOLOCK) 
                                LEFT JOIN T_INVOICE_AGING AS INV WITH (NOLOCK) ON INV.ID = PTPINV.InvoiceId
                                    WHERE INV.TRACK_STATES = '003' and PTPINV.PTPPaymentId = @Id
                                 ORDER BY INV.INVOICE_NUM ASC ");

                SqlParameter[] paramForSQL = new SqlParameter[1];
                paramForSQL[0] = new SqlParameter("@Id", id);

                DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString(), System.Data.CommandType.Text, paramForSQL);
                List<TaskPtpDetailDto> taskPmtDetailList = SqlHelper.GetList<TaskPtpDetailDto>(dt);

                return taskPmtDetailList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public List<TaskDisputeDto> GetTaskDisputeList(string legalEntity, string customerNum, string customerName, string siteUseId, string status)
        {

            try
            {
                List<SysUser> listUser = new List<SysUser>();
                listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
                string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
                string collecotrList = "'" + string.Join("','", userGroup.ToArray()) + "'";

                StringBuilder sbselect = new StringBuilder();
                StringBuilder sbOderby = new StringBuilder();
                sbselect.Append(@" SELECT 
                                          Dispute.ID                AS Id,
										  C.Organization            AS LegalEntity, 
                                          Dispute.CUSTOMER_NUM      AS CustomerNum, 
                                          Dispute.SiteUseId         AS SiteUseId, 
                                          Dispute.ISSUE_REASON      AS IssueReason, 
                                          D1.DETAIL_NAME            AS IssueReasonName, 
                                          Dispute.STATUS            AS DisputeStatus,
										  D.DETAIL_NAME             AS DisputeStatusName,
                                          Dispute.Status_Date       AS Status_Date, 
                                          Dispute.Comments          AS Comments
                                FROM T_DISPUTE AS Dispute WITH (NOLOCK) 
                           LEFT JOIN T_CUSTOMER AS C WITH (NOLOCK) ON C.SiteUseId = Dispute.SiteUseId
                           LEFT JOIN dbo.T_SYS_TYPE_DETAIL AS D WITH (NOLOCK) ON D.TYPE_CODE = '026' AND Dispute.STATUS = D.DETAIL_VALUE
                           LEFT JOIN dbo.T_SYS_TYPE_DETAIL AS D1 WITH (NOLOCK) ON D1.TYPE_CODE = '025' AND Dispute.ISSUE_REASON = D1.DETAIL_VALUE
                               WHERE 1 = 1 ");
                ArrayList list = new ArrayList();
                if (!string.IsNullOrEmpty(status) && status != "undefined")
                {
                    sbselect.Append(@" AND (Dispute.STATUS = @status)");
                    SqlParameter param = new SqlParameter("@status", status);
                    list.Add(param);
                }
                if (SupervisorPermission != 1)
                {
                    sbselect.Append(@" AND (C.COLLECTOR IN (" + collecotrList + "))");
                }
                //Legal Entity查询条件
                if (!string.IsNullOrEmpty(legalEntity) && legalEntity != "null" && legalEntity != "undefined")
                {
                    sbselect.Append(@" and (C.LEGAL_ENTITY = @legalEntity ) ");
                    SqlParameter param = new SqlParameter("@legalEntity", legalEntity);
                    list.Add(param);

                }
                //CustomerNum查询条件
                if (!string.IsNullOrEmpty(customerNum) && customerNum != "null" && customerNum != "undefined")
                {
                    sbselect.Append(@" and (Dispute.CUSTOMER_NUM like @customerNum )");
                    SqlParameter param = new SqlParameter("@customerNum", "%" + customerNum + "%");
                    list.Add(param);
                }
                //CustomerName查询条件
                if (!string.IsNullOrEmpty(customerName) && customerName != "null" && customerName != "undefined")
                {
                    sbselect.Append(@" and (C.CUSTOMER_NAME like @customerName )");
                    SqlParameter param = new SqlParameter("@customerName", "%" + customerName + "%");
                    list.Add(param);
                }
                //SiteUseId查询条件
                if (!string.IsNullOrEmpty(siteUseId) && siteUseId != "null" && siteUseId != "undefined")
                {
                    sbselect.Append(@" and (Dispute.SiteUseId like @siteUseId )");
                    SqlParameter param = new SqlParameter("@siteUseId", "%" + siteUseId + "%");
                    list.Add(param);
                }

                SqlParameter[] paramForSQL = new SqlParameter[list.Count];
                int intParam = 0;
                foreach (var item in list)
                {
                    paramForSQL[intParam] = (SqlParameter)item;
                    intParam++;
                }

                sbOderby.Append(@" ORDER BY Dispute.Status_Date DESC ");

                DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString() + sbOderby.ToString(), System.Data.CommandType.Text, paramForSQL);
                List<TaskDisputeDto> taskDisputeList = SqlHelper.GetList<TaskDisputeDto>(dt);

                return taskDisputeList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public List<TaskDisputeDetailDto> GetTaskDisputeDetailList(long id)
        {
            try
            {
                StringBuilder sbselect = new StringBuilder();
                sbselect.Append(@" SELECT DisputeINV.Id         AS Id, 
                                          DisputeINV.INVOICE_ID AS InvoiceId, 
                                          INV.INVOICE_NUM       AS InvoiceNum, 
                                          INV.BALANCE_AMT       AS BalanceAmt, 
                                          INV.COMMENTS          AS Comments
                                     FROM T_DISPUTE_INVOICE AS DisputeINV WITH (NOLOCK) 
                                LEFT JOIN T_INVOICE_AGING AS INV WITH (NOLOCK) ON INV.INVOICE_NUM = DisputeINV.INVOICE_ID
                                    WHERE INV.TRACK_STATES = '007' and DisputeINV.DISPUTE_ID = @Id
                                 ORDER BY INV.INVOICE_NUM ASC ");

                SqlParameter[] paramForSQL = new SqlParameter[1];
                paramForSQL[0] = new SqlParameter("@Id", id);

                DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString(), System.Data.CommandType.Text, paramForSQL);
                List<TaskDisputeDetailDto> taskDisputeDetailList = SqlHelper.GetList<TaskDisputeDetailDto>(dt);

                return taskDisputeDetailList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public List<TaskReminddingDto> GetTaskReminddingList(string legalEntity, string customerNum, string customerName, string siteUseId, DateTime dateF, DateTime dateT)
        {
            try
            {
                List<SysUser> listUser = new List<SysUser>();
                listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
                string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
                string collecotrList = "'" + string.Join("','", userGroup.ToArray()) + "'";

                StringBuilder sbselect = new StringBuilder();
                StringBuilder sbOderby = new StringBuilder();
                sbselect.Append(@" SELECT task.DEAL                 AS deal,
									      task.LEGAL_ENTITY         AS legalEntity, 
                                          task.CUSTOMER_NUM         AS customerNum, 
                                          C.CUSTOMER_NAME           AS CustomerName,
                                          task.SITEUSEID            AS siteUseId, 
                                          task.TASK_DATE            AS task_date, 
                                          task.TASK_CONTENT         AS task_content
                                FROM T_TASK AS task WITH (NOLOCK) 
                           LEFT JOIN T_CUSTOMER AS C WITH (NOLOCK) ON C.SiteUseId = task.SiteUseId
                               WHERE (CONVERT(VARCHAR(10),task.TASK_DATE,120) >= CONVERT(VARCHAR(10),getdate(),120))");
                ArrayList list = new ArrayList();

                if (SupervisorPermission != 1)
                {
                    sbselect.Append(@" AND (C.COLLECTOR IN (" + collecotrList + "))");
                }
                //Legal Entity查询条件
                if (!string.IsNullOrEmpty(legalEntity) && legalEntity != "null" && legalEntity != "undefined")
                {
                    sbselect.Append(@" and (C.LEGAL_ENTITY = @legalEntity ) ");
                    SqlParameter param = new SqlParameter("@legalEntity", legalEntity);
                    list.Add(param);

                }
                //CustomerNum查询条件
                if (!string.IsNullOrEmpty(customerNum) && customerNum != "null" && customerNum != "undefined")
                {
                    sbselect.Append(@" and (task.CUSTOMER_NUM like @customerNum )");
                    SqlParameter param = new SqlParameter("@customerNum", "%" + customerNum + "%");
                    list.Add(param);
                }
                //CustomerName查询条件
                if (!string.IsNullOrEmpty(customerName) && customerName != "null" && customerName != "undefined")
                {
                    sbselect.Append(@" and (C.CUSTOMER_NAME like @customerName )");
                    SqlParameter param = new SqlParameter("@customerName", "%" + customerName + "%");
                    list.Add(param);
                }
                //SiteUseId查询条件
                if (!string.IsNullOrEmpty(siteUseId) && siteUseId != "null" && siteUseId != "undefined")
                {
                    sbselect.Append(@" and (task.SiteUseId like @siteUseId )");
                    SqlParameter param = new SqlParameter("@siteUseId", "%" + siteUseId + "%");
                    list.Add(param);
                }

                SqlParameter[] paramForSQL = new SqlParameter[list.Count];
                int intParam = 0;
                foreach (var item in list)
                {
                    paramForSQL[intParam] = (SqlParameter)item;
                    intParam++;
                }

                sbOderby.Append(@" ORDER BY task.TASK_DATE DESC ");

                DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString() + sbOderby.ToString(), System.Data.CommandType.Text, paramForSQL);
                List<TaskReminddingDto> taskReminddingList = SqlHelper.GetList<TaskReminddingDto>(dt);

                return taskReminddingList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public HttpResponseMessage ExportTask(string cLegalEntity, string cCustNum, string cCustName, string cSiteUseId, DateTime cDate, string cStatus)
        {
            if (cLegalEntity == "null") { cLegalEntity = ""; }
            if (cCustNum == "null") { cCustNum = ""; }
            if (cCustName == "null") { cCustName = ""; }
            if (cSiteUseId == "null") { cSiteUseId = ""; }
            if (cStatus == "null") { cStatus = ""; }

            string templateFile = "";
            string fileName = "";
            string tmpFile = "";

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportTaskTemplate"].ToString());
                fileName = "FollowUp_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                List<SysUser> listUser = new List<SysUser>();
                listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
                string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
                string collecotrList = "," + string.Join(",", userGroup.ToArray()) + ",";

                DateTime dt_StartDate = DateTime.Now;
                if (cDate != null) { dt_StartDate = cDate; }
                dt_StartDate = Convert.ToDateTime(dt_StartDate.ToString("yyyy-MM-dd") + " 00:00:00");
                DateTime dt_EndDate = dt_StartDate.AddDays(10);
                List<TaskDto> lstTask = GetTaskList(cLegalEntity, cCustNum, cCustName, cSiteUseId, cDate, cStatus);
                this.setData(templateFile, tmpFile, lstTask);

                HttpResponseMessage response = new HttpResponseMessage();
                response.StatusCode = HttpStatusCode.OK;
                MemoryStream fileStream = new MemoryStream();
                if (File.Exists(tmpFile))
                {
                    using (FileStream fs = File.OpenRead(tmpFile))
                    {
                        fs.CopyTo(fileStream);
                    }
                }
                else
                {
                    throw new OTCServiceException("Get file failed because file not exist with physical path: " + tmpFile);
                }
                Stream ms = fileStream;
                ms.Position = 0;
                response.Content = new StreamContent(ms);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentLength = ms.Length;

                return response;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
            finally
            {
                //File.Delete(tmpFile);
            }
        }

        public HttpResponseMessage ExportSoaDate(string cLegalEntity, string cCustNum, string cCustName, string cSiteUseId)
        {
            if (cLegalEntity == "null") { cLegalEntity = ""; }
            if (cCustNum == "null") { cCustNum = ""; }
            if (cCustName == "null") { cCustName = ""; }
            if (cSiteUseId == "null") { cSiteUseId = ""; }

            List<TaskReportDto> lstTask = new List<TaskReportDto>();
            string templateFile = "";
            string fileName = "";
            string tmpFile = "";

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportSoaDateTemplate"].ToString());
                fileName = "SoaDate_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                List<SysUser> listUser = new List<SysUser>();
                listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
                string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
                string collecotrList = "," + string.Join(",", userGroup.ToArray()) + ",";

                var taskSql = string.Format(@"
                        SELECT DISTINCT 
                        C.DEAL as Deal, C.Organization as LegalEntity, C.CUSTOMER_NUM as CustomerNo,C.CUSTOMER_NAME as CustomerName,C.SiteUseId as SiteUseId,C.COLLECTOR as Collector, t.CURRENT_AMT as TotalAr, c.Star as Star
                        FROM dbo.T_CUSTOMER c WITH (NOLOCK) 
                        JOIN dbo.T_CUSTOMER_AGING t WITH (NOLOCK) ON c.DEAL = t.DEAL AND c.Organization = t.LEGAL_ENTITY AND c.CUSTOMER_NUM = t.CUSTOMER_NUM AND c.SiteUseId = t.SiteUseId
                        WHERE '{0}' like '%,' + c.COLLECTOR + ',%' AND
                              (C.Organization = '{1}' OR '{1}' = '') AND
                              ((C.CUSTOMER_NUM like '%' + '{2}' + '%') OR '{2}' = '') AND
                              ((C.CUSTOMER_NAME like '%' + '{3}' + '%') OR '{3}' = '') AND
                              ((C.SiteUseId like '%' + '{4}' + '%') OR '{4}' = '')
                        ORDER BY C.DEAL, C.Organization, C.CUSTOMER_NUM,C.SiteUseId,C.COLLECTOR
                    ", collecotrList, cLegalEntity, cCustNum, cCustName, cSiteUseId);
                lstTask = CommonRep.GetDBContext().Database.SqlQuery<TaskReportDto>(taskSql).ToList();

                foreach (TaskReportDto item in lstTask)
                {
                    var lastSoaDate = (from x in CommonRep.GetDbSet<ContactHistory>()
                                       where item.Deal == x.Deal &&
                                             item.CustomerNo == x.CustomerNum &&
                                             item.SiteUseId == x.SiteUseId &&
                                             x.ContactType == "Mail" &&
                                             x.CollectorId == "BATCH_USER"
                                       select x.ContactDate).DefaultIfEmpty().Max();
                    item.LastSoaDate = lastSoaDate.ToString("yyyy-MM-dd");
                    if (item.LastSoaDate == "0001-01-01")
                    {
                        item.LastSoaDate = "";
                    }
                }

                this.setSoaData(templateFile, tmpFile, lstTask);

                HttpResponseMessage response = new HttpResponseMessage();
                response.StatusCode = HttpStatusCode.OK;
                MemoryStream fileStream = new MemoryStream();
                if (File.Exists(tmpFile))
                {
                    using (FileStream fs = File.OpenRead(tmpFile))
                    {
                        fs.CopyTo(fileStream);
                    }
                }
                else
                {
                    throw new OTCServiceException("Get file failed because file not exist with physical path: " + tmpFile);
                }
                Stream ms = fileStream;
                ms.Position = 0;
                response.Content = new StreamContent(ms);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentLength = ms.Length;

                return response;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
            finally
            {
            }
        }

        public bool NewTask(string deal, string legalEntity, string custNum, string siteUseId, string startDate, string taskType, string taskContent, string taskStatus, string isAuto)
        {
            bool lb_Success = false;

            try
            {
                T_TASK task = new T_TASK();
                task.DEAL = deal;
                task.LEGAL_ENTITY = legalEntity;
                task.CUSTOMER_NUM = custNum;
                task.SITEUSEID = siteUseId;
                task.TASK_DATE = Convert.ToDateTime(startDate);
                task.TASK_TYPE = taskType;
                task.TASK_CONTENT = taskContent;
                task.TASK_STATUS = taskStatus;
                task.ISAUTO = isAuto;
                task.CREATE_DATE = AppContext.Current.User.Now;
                task.CREATE_USER = AppContext.Current.User.Id;
                List<T_TASK> taskList = new List<T_TASK>();
                taskList.Add(task);
                CommonRep.BulkInsert<T_TASK>(taskList);
                lb_Success = true;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
            }

            return lb_Success;
        }

        public bool UpdateTask(string taskId, string startDate, string taskType, string taskContent, string taskStatus)
        {
            bool lb_Success = false;

            try
            {
                var task = from x in CommonRep.GetDbSet<T_TASK>()
                           where (x.ID.ToString() == taskId)
                           select x;
                if (task == null || task.Count() <= 0)
                {
                    return lb_Success;
                }
                List<T_TASK> taskList = task.ToList();
                taskList[0].TASK_DATE = Convert.ToDateTime(startDate);
                taskList[0].TASK_TYPE = taskType;
                taskList[0].TASK_CONTENT = taskContent;
                taskList[0].TASK_STATUS = taskStatus;
                taskList[0].UPDATE_DATE = AppContext.Current.User.Now;
                taskList[0].UPDATE_USER = AppContext.Current.User.Id;
                CommonRep.BulkUpdate<T_TASK>(taskList);
                lb_Success = true;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
            }

            return lb_Success;
        }

        public bool SaveTaskPMT(string customerNum, string siteUseId, string invoiceNum, string status, string comments)
        {
            bool lb_Success = false;

            try
            {
                string strStatusCode = "";
                switch (status)
                {
                    case "Ignore":
                        strStatusCode = "-";
                        break;
                    case "Offset":
                        strStatusCode = "2";
                        break;
                    case "NotOffset":
                        strStatusCode = "3";
                        break;
                }

                comments = comments.Replace("undefined", "");

                SqlParameter[] paramForSQL = new SqlParameter[5];
                paramForSQL[0] = new SqlParameter("@customerNum", customerNum);
                paramForSQL[1] = new SqlParameter("@siteUseId", siteUseId);
                paramForSQL[2] = new SqlParameter("@invoiceNum", invoiceNum);
                paramForSQL[3] = new SqlParameter("@hasPMT", strStatusCode);
                paramForSQL[4] = new SqlParameter("@Comments", comments);

                StringBuilder sbUpdateInv = new StringBuilder();
                sbUpdateInv.Append(@" UPDATE dbo.T_INVOICE_AGING
                                      SET hasPmt = @hasPMT,
                                          COMMENTS = ISNULL(COMMENTS,'') + ';' + @Comments
                                    WHERE CUSTOMER_NUM = @customerNum
                                      AND SiteUseId = @siteUseId
                                      AND INVOICE_NUM = @invoiceNum
                                      AND CLASS = 'PMT'
                                      AND TRACK_STATES = '000' ");
                SqlHelper.ExcuteSql(sbUpdateInv.ToString(), paramForSQL);

                lb_Success = true;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
            }

            return lb_Success;
        }

        public bool SaveTaskPTP(string Id, string status, string comments)
        {
            bool lb_Success = false;

            try
            {
                string strStatusCode = "";
                switch (status)
                {
                    case "Executed":
                        strStatusCode = "002";
                        break;
                    case "Broken":
                        strStatusCode = "003";
                        break;
                    case "Cancel":
                        strStatusCode = "004";
                        break;
                }

                SqlParameter[] paramForSQL = new SqlParameter[1];
                paramForSQL[0] = new SqlParameter("@Id", Id);

                StringBuilder sbUpdateInv = new StringBuilder();
                sbUpdateInv.Append(@" UPDATE dbo.T_INVOICE_AGING
                                      SET PTP_DATE = NULL,
                                          TRACK_STATES = '000',
                                          TRACK_DATE = getdate()
                                    WHERE id IN (SELECT InvoiceId FROM dbo.T_PTPPayment_Invoice WITH (NOLOCK) WHERE PTPPaymentId = @Id) ");
                SqlHelper.ExcuteSql(sbUpdateInv.ToString(), paramForSQL);


                StringBuilder sbUpdatePTP = new StringBuilder();
                SqlParameter[] paramForSQL1 = new SqlParameter[3];
                paramForSQL1[0] = new SqlParameter("@Id", Id);
                paramForSQL1[1] = new SqlParameter("@status", strStatusCode);
                paramForSQL1[2] = new SqlParameter("@comments", comments);
                sbUpdatePTP.Append(@" UPDATE dbo.T_PTPPayment
                                         SET PTPStatus = @status,
                                             Status_Date = GETDATE(),
	                                         Comments = isnull(Comments,'') + @comments
                                       WHERE id = @Id");
                SqlHelper.ExcuteSql(sbUpdatePTP.ToString(), paramForSQL1);

                lb_Success = true;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
            }

            return lb_Success;
        }

        public bool SaveTaskDispute(string Id, string status, string comments)
        {
            bool lb_Success = false;

            try
            {
                string strStatusCode = "";
                switch (status)
                {
                    case "Resolved":
                        strStatusCode = "026012";
                        break;
                    case "Cancel":
                        strStatusCode = "026011";
                        break;
                }

                SqlParameter[] paramForSQL = new SqlParameter[1];
                paramForSQL[0] = new SqlParameter("@Id", Id);

                StringBuilder sbUpdateInv = new StringBuilder();
                sbUpdateInv.Append(@" UPDATE dbo.T_INVOICE_AGING
                                      SET TRACK_STATES = '000',
                                          TRACK_DATE = getdate()
                                    WHERE INVOICE_NUM IN (SELECT INVOICE_ID FROM dbo.T_DISPUTE_INVOICE WITH (NOLOCK) WHERE DISPUTE_ID = @Id) ");
                SqlHelper.ExcuteSql(sbUpdateInv.ToString(), paramForSQL);


                StringBuilder sbUpdatePTP = new StringBuilder();
                SqlParameter[] paramForSQL1 = new SqlParameter[3];
                paramForSQL1[0] = new SqlParameter("@Id", Id);
                paramForSQL1[1] = new SqlParameter("@status", strStatusCode);
                paramForSQL1[2] = new SqlParameter("@comments", comments);
                sbUpdatePTP.Append(@" UPDATE dbo.T_DISPUTE
                                         SET STATUS = @status,
                                             Status_Date = GETDATE(),
	                                         Comments = isnull(Comments,'') + @comments
                                       WHERE id = @Id");
                SqlHelper.ExcuteSql(sbUpdatePTP.ToString(), paramForSQL1);

                lb_Success = true;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
            }

            return lb_Success;
        }

        private void setSoaData(string templateFileName, string tmpFile, List<TaskReportDto> lstDatas)
        {
            int rowNo = 1;

            try
            {
                NpoiHelper helper = new NpoiHelper(templateFileName);
                helper.Save(tmpFile, true);
                helper = new NpoiHelper(tmpFile);
                string sheetName = "";

                foreach (string sheet in helper.Sheets)
                {
                    sheetName = sheet;
                    break;
                }

                //设置sheet
                helper.ActiveSheetName = sheetName;

                //设置Excel的内容信息
                foreach (var lst in lstDatas)
                {
                    helper.SetData(rowNo, 0, lst.LegalEntity);
                    helper.SetData(rowNo, 1, lst.CustomerNo);
                    helper.SetData(rowNo, 2, lst.CustomerName);
                    helper.SetData(rowNo, 3, lst.SiteUseId);
                    helper.SetData(rowNo, 4, new string('★', Convert.ToInt32(lst.Star == null ? 0 : lst.Star)));
                    helper.SetData(rowNo, 5, lst.Collector);
                    helper.SetData(rowNo, 6, lst.LastSoaDate);
                    helper.SetData(rowNo, 7, lst.TotalAr);
                    rowNo++;
                }

                helper.Save(tmpFile, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        private void setData(string templateFileName, string tmpFile, List<TaskDto> lstDatas)
        {
            int rowNo = 1;

            try
            {
                NpoiHelper helper = new NpoiHelper(templateFileName);
                helper.Save(tmpFile, true);
                helper = new NpoiHelper(tmpFile);
                string sheetName = "";

                foreach (string sheet in helper.Sheets)
                {
                    sheetName = sheet;
                    break;
                }

                //设置sheet
                helper.ActiveSheetName = sheetName;

                //设置Excel的内容信息
                foreach (var lst in lstDatas)
                {
                    helper.SetData(rowNo, 0, lst.LegalEntity);
                    helper.SetData(rowNo, 1, lst.CustomerNo);
                    helper.SetData(rowNo, 2, lst.CustomerName);
                    helper.SetData(rowNo, 3, lst.SiteUseId);
                    helper.SetData(rowNo, 4, lst.Collector);
                    helper.SetData(rowNo, 5, lst.Currency);
                    helper.SetData(rowNo, 6, lst.EbName);
                    helper.SetData(rowNo, 7, lst.CreditTerm);
                    helper.SetData(rowNo, 8, lst.Sales);
                    helper.SetData(rowNo, 9, lst.CS);
                    helper.SetData(rowNo, 10, lst.LastSoaDate);
                    helper.SetData(rowNo, 11, lst.TotalAr);
                    helper.SetData(rowNo, 12, lst.NotOverdue);
                    helper.SetData(rowNo, 13, lst.Overdue);
                    helper.SetData(rowNo, 14, lst.Overdue60);
                    helper.SetData(rowNo, 15, lst.Overdue120);
                    helper.SetData(rowNo, 16, lst.PtpDate);
                    helper.SetData(rowNo, 17, lst.PtpAmount);
                    rowNo++;
                }

                helper.Save(tmpFile, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
        
        private void setPmtData(string templateFileName, string tmpFile, List<TaskPmtDto> lstDatas)
        {
            int rowNo = 1;

            try
            {
                NpoiHelper helper = new NpoiHelper(templateFileName);
                helper.Save(tmpFile, true);
                helper = new NpoiHelper(tmpFile);
                string sheetName = "";

                foreach (string sheet in helper.Sheets)
                {
                    sheetName = sheet;
                    break;
                }

                //设置sheet
                helper.ActiveSheetName = sheetName;

                //设置Excel的内容信息
                foreach (var lst in lstDatas)
                {
                    helper.SetData(rowNo, 0, lst.LegalEntity);
                    helper.SetData(rowNo, 1, lst.CustomerNum);
                    helper.SetData(rowNo, 2, lst.CustomerName);
                    helper.SetData(rowNo, 3, lst.SiteUseId);
                    helper.SetData(rowNo, 4, lst.Currency);
                    helper.SetData(rowNo, 5, lst.Class);
                    helper.SetData(rowNo, 6, lst.InvoiceNum);
                    helper.SetData(rowNo, 7, lst.InvoiceDate);
                    helper.SetData(rowNo, 8, lst.BalanceAmt);
                    helper.SetData(rowNo, 9, lst.Comments);
                    switch (lst.haspmt) {
                        case "0":
                            helper.SetData(rowNo, 10, "未发送");
                            break;
                        case "-":
                            helper.SetData(rowNo, 10, "已取消");
                            break;
                        case "1":
                            helper.SetData(rowNo, 10, "已发送");
                            break;
                    }
                    rowNo++;
                }

                helper.Save(tmpFile, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        private void setPtpData(string templateFileName, string tmpFile, List<TaskPtpDto> lstDatas)
        {
            int rowNo = 1;

            try
            {
                NpoiHelper helper = new NpoiHelper(templateFileName);
                helper.Save(tmpFile, true);
                helper = new NpoiHelper(tmpFile);
                string sheetName = "";

                foreach (string sheet in helper.Sheets)
                {
                    sheetName = sheet;
                    break;
                }

                //设置sheet
                helper.ActiveSheetName = sheetName;

                //设置Excel的内容信息
                foreach (var lst in lstDatas)
                {
                    helper.SetData(rowNo, 0, lst.LegalEntity);
                    helper.SetData(rowNo, 1, lst.CustomerNum);
                    helper.SetData(rowNo, 2, lst.CustomerName);
                    helper.SetData(rowNo, 3, lst.SiteUseId);
                    helper.SetData(rowNo, 4, lst.PromiseDate);
                    helper.SetData(rowNo, 5, lst.IsPartialPay);
                    helper.SetData(rowNo, 6, lst.Payer);
                    helper.SetData(rowNo, 7, lst.PromissAmount);
                    helper.SetData(rowNo, 8, lst.Comments);
                    helper.SetData(rowNo, 9, lst.CreateTime);
                    helper.SetData(rowNo, 10, lst.PTPStatusName);
                    rowNo++;
                }

                helper.Save(tmpFile, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public List<TaskSendSoaPmtList> gettaskPMTSendList(int page, int pageSize, out int total)
        {
            IEnumerable<TaskSendSoaPmtList> result = null;

            try
            {
                List<SysUser> listUser = new List<SysUser>();
                listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
                string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
                string collecotrList = "," + string.Join(",", userGroup.ToArray()) + ",";

                string sql = string.Format(@"SELECT ActionDate,
                                       TempleteLanguage,
                                       T_SYS_TYPE_DETAIL.DETAIL_NAME AS TempleteLanguageName,
                                       Region,
                                       DEAL,
                                       EID,
                                       PeriodId,
                                       AlertType,
                                       ToTitle,
                                       ToName,
                                       CCTitle,
                                       ResponseDate,
                                       CustomerNum,
                                       Comment
                                FROM V_Task_NeedPmtList
                                    LEFT JOIN dbo.T_SYS_TYPE_DETAIL WITH (NOLOCK) 
                                        ON dbo.T_SYS_TYPE_DETAIL.TYPE_CODE = '013'
                                           AND V_Task_NeedPmtList.TempleteLanguage = T_SYS_TYPE_DETAIL.DETAIL_VALUE
                            where '{0}' like '%,' + EID + ',%'
                                    Order by V_Task_NeedPmtList.EID,
                                             V_Task_NeedPmtList.Region,
                                             V_Task_NeedPmtList.ToTitle,
                                             V_Task_NeedPmtList.ToName,
                                             V_Task_NeedPmtList.CustomerNum", collecotrList);

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<TaskSendSoaPmtList>(sql, parameters).OrderBy(o => o.Region).ThenBy(o => o.Eid);
                total = result.Count();
                result = result.Skip((page - 1) * pageSize).Take(pageSize);
            }
            catch (Exception ex)
            {
                result = new List<TaskSendSoaPmtList>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Task_NeedPmtList 异常!");
            }

            return result.ToList();
        }

        public List<TaskSendSoaPmtList> gettaskSOASendList(int page, int pageSize, out int total)
        {
            IEnumerable<TaskSendSoaPmtList> result = null;

            try
            {
                List<SysUser> listUser = new List<SysUser>();
                listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
                string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
                string collecotrList = "," + string.Join(",", userGroup.ToArray()) + ",";

                string sql = string.Format(@"SELECT distinct V_Task_NeedSoaList.ActionDate,
                                       V_Task_NeedSoaList.TempleteLanguage,
                                       T_SYS_TYPE_DETAIL.DETAIL_NAME AS TempleteLanguageName,
                                       V_Task_NeedSoaList.Region,
                                       V_Task_NeedSoaList.DEAL,
                                       V_Task_NeedSoaList.EID,
                                       V_Task_NeedSoaList.PeriodId,
                                       V_Task_NeedSoaList.AlertType,
                                       V_Task_NeedSoaList.ToTitle,
                                       V_Task_NeedSoaList.ToName,
                                       V_Task_NeedSoaList.CCTitle,
                                       V_Task_NeedSoaList.ResponseDate,
                                       T_CUSTOMER.CUSTOMER_NAME as CustomerName,
                                       V_Task_NeedSoaList.CustomerNum,
                                       V_Task_NeedSoaList.Comment
                                FROM V_Task_NeedSoaList
                                  left JOIN T_CUSTOMER WITH (NOLOCK) ON T_CUSTOMER.CUSTOMER_NUM = V_Task_NeedSoaList.CustomerNum
                                    LEFT JOIN dbo.T_SYS_TYPE_DETAIL WITH (NOLOCK) 
                                        ON dbo.T_SYS_TYPE_DETAIL.TYPE_CODE = '013'
                                           AND V_Task_NeedSoaList.TempleteLanguage = T_SYS_TYPE_DETAIL.DETAIL_VALUE
                            where '{0}' like '%,' + EID + ',%'
                                    Order by V_Task_NeedSoaList.EID,
                                             V_Task_NeedSoaList.Region,
                                             V_Task_NeedSoaList.ToTitle,
                                             V_Task_NeedSoaList.ToName,
                                             V_Task_NeedSoaList.CustomerNum", collecotrList);

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<TaskSendSoaPmtList>(sql, parameters).OrderBy(o => o.Region).ThenBy(o => o.Eid);
                total = result.Count();
                result = result.Skip((page - 1) * pageSize).Take(pageSize);
            }
            catch (Exception ex)
            {
                result = new List<TaskSendSoaPmtList>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_Feedback_Detail 异常!");
            }

            return result.ToList();
        }

        public bool sendTaskByUser(string templeteLanguage, string deal, string region, string eid, int periodId, int alertType, string toTitle, string toName, string ccTitle, string customerNum, DateTime responseDate) {
            //发送之前先Build一次联系人
            int buildResult = soaService.BuildContactorByAlert(deal, region, eid, templeteLanguage, periodId, alertType, customerNum, toTitle, toName, ccTitle);
            
            //1、获取待发送 Invoice 列表
            List<int> invoiceIdList = soaService.GetAlertAutoSendInvoice(eid, deal, "", customerNum, "", alertType.ToString(), toTitle, toName, templeteLanguage);
            if (invoiceIdList == null || invoiceIdList.Count == 0)
            {
                throw new Exception("There is no invoice need send");
            }
            //2、根据客户，及发票信息，调用 Service ，生成邮件及附件,保存邮件
            AlertKey alertKey = new AlertKey();
            alertKey.Collector = eid;
            alertKey.Deal = deal;
            alertKey.LegalEntity = "";
            alertKey.CustomerNum = customerNum;
            alertKey.SiteUseId = "";
            alertKey.AlertType = alertType;
            alertKey.PeriodId = periodId;
            alertKey.ToTitle = toTitle;
            alertKey.ToName = toName;
            alertKey.CCTitle = ccTitle;
            alertKey.ResponseDate = responseDate;
            alertKey.Region = region;
            alertKey.TempleteLanguage = templeteLanguage;
            MailTmp nMail = generateMailByAlert(alertKey, invoiceIdList);
            //3、发送邮件
            if (nMail != null && !string.IsNullOrEmpty(nMail.To))
            {
                sendMail(nMail, alertKey, invoiceIdList);
            }
            else
            {
                Helper.Log.Warn("---------------Not have contactor-----------Collector:" + alertKey.Collector + ",Region:" + alertKey.Region + ",AlertType:" + alertKey.AlertType + ",ToTitle:" + alertKey.ToTitle + ",ToName:" + alertKey.ToName + ",CCTitle:" + alertKey.CCTitle);
                throw new OTCServiceException("Not have contactor");
            }
            return true;
        }

        private void sendMail(MailTmp mail, AlertKey alertKey, List<int> invoiceIdList)
        {
            int periodId = (int)alertKey.PeriodId;
            soaService.sendSoaSaveInfoToDB(mail, invoiceIdList, alertKey.AlertType, alertKey.Collector, alertKey.LegalEntity, alertKey.CustomerNum, alertKey.SiteUseId, alertKey.ToTitle, alertKey.ToName, alertKey.CCTitle, periodId, alertKey.TempleteLanguage);
        }

        /// <summary>
        /// 根据客户生成邮件
        /// </summary>
        /// <param name="customerKey"></param>
        /// <returns></returns>
        private MailTmp generateMailByAlert(AlertKey alertKey, List<int> invoiceIdList)
        {
            //每个ToTitle对应的客户只能是一种语言模板
            //根据ToTitle和ToName取客户Language唯一值

            //获得模板语言
            var disQ = from ca in CommonRep.GetDbSet<Customer>()
                       where ca.Collector == alertKey.Collector
                       && ca.Region == alertKey.Region && ca.ContactLanguage != null
                       group ca by ca.ContactLanguage into g
                       select new { g.Key };
            var nDefaultLanguage = (from a in disQ select a.Key).FirstOrDefault().ToString();

            if (string.IsNullOrEmpty(nDefaultLanguage))
            {
                Helper.Log.Info("Customer Contact Language haven't set.");
                throw new OTCServiceException("Customer Contact Language haven't set.");
            }

            // 获取联系人
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            List<ContactorDto> collectorList = service.GetContactsByAlert(alertKey.Collector, alertKey.CustomerNum, alertKey.SiteUseId, alertKey.ToTitle, alertKey.ToName, alertKey.CCTitle, invoiceIdList).ToList();
            List<string> nTo = new List<string>();
            List<string> nCC = new List<string>();
            if (collectorList != null && collectorList.Count > 0)
            {
                foreach (var x in collectorList)
                {
                    if (string.IsNullOrEmpty(x.EmailAddress))
                        continue;

                    if (x.ToCc == "1")
                    {
                        if (!nTo.Contains(x.EmailAddress))
                        {
                            nTo.Add(x.EmailAddress);
                        }
                    }
                    else
                    {
                        if (!nCC.Contains(x.EmailAddress))
                        {
                            nCC.Add(x.EmailAddress);
                        }
                    }
                }

            }
            if (nTo.Count == 0)
            {
                return null;
            }

            MailTmp nMail = soaService.GetNewMailInstance(alertKey.CustomerNum, alertKey.SiteUseId, "00" + alertKey.AlertType.ToString(), nDefaultLanguage, invoiceIdList, alertKey.Collector, alertKey.ToTitle, alertKey.ToName, alertKey.CCTitle, (alertKey.ResponseDate == null ? "" : Convert.ToDateTime(alertKey.ResponseDate).ToString("yyyy-MM-dd")), alertKey.Region);

            nMail = RenderInstance(nMail, nDefaultLanguage, alertKey, invoiceIdList);

            nMail.To = string.Join(";", nTo);
            nMail.Cc = string.Join(";", nCC);
            if (!string.IsNullOrEmpty(nMail.Cc))
            {
                nMail.Cc += ";" + nMail.From;
            }
            else
            {
                nMail.Cc = nMail.From;
            }

            //根据Collector获得发送的组邮箱
            if (!string.IsNullOrEmpty(alertKey.Collector))
            {
                var groupMailBox = (from ca in CommonRep.GetDbSet<SysTypeDetail>()
                                    where ca.TypeCode == "045" && ca.DetailName == alertKey.Collector
                                    select ca.DetailValue2).FirstOrDefault().ToString();
                nMail.From = groupMailBox;
            }

            return nMail;
        }

        public MailTmp RenderInstance(MailTmp instance, string language, AlertKey alertKey, List<int> invoiceIdList)
        {
            //invoiceIds
            instance.invoiceIds = invoiceIdList.ToArray();
            //soaFlg
            instance.soaFlg = "1";
            instance.MailType = "00" + alertKey.AlertType.ToString() + "," + language;

            return instance;
        }

        public class AlertKey
        {
            public string Collector { get; set; }
            public string Deal { get; set; }            //deal
            public string LegalEntity { get; set; }
            public string CustomerNum { get; set; }     //CustomerNum
            public string SiteUseId { get; set; }       //SiteUseId
            public int AlertType { get; set; }   //WaveX
            public int? PeriodId { get; set; }    //账期
            public string ToTitle { get; set; }  //联系人Title
            public string ToName { get; set; }
            public string CCTitle { get; set; }
            public DateTime? ResponseDate { get; set; }  //响应日期
            public string Region { get; set; }
            public string TempleteLanguage { get; set; }    //模板语言
        }
    }
}
