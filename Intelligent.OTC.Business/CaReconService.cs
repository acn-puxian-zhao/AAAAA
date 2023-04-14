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
using Intelligent.OTC.Domain.DataModel;
using System.Configuration;
using Intelligent.OTC.Common.Exceptions;
using System.Text;
using System.Net.Http;
using System.Web;
using System.IO;
using System.Net.Http.Headers;
using System.Net;

namespace Intelligent.OTC.Business
{
    public class CaReconService : ICaReconService
    {
        public OTCRepository CommonRep { get; set; }

        public string cleanData(CaTaskMsg msg)
        {
            ReconServiceProxy proxy = new ReconServiceProxy(ConfigurationManager.AppSettings["CleanDataServiceUrl"]);
            string result = proxy.cleanData(msg);

            return result;
        }

        public string identifyCustomer(CaTaskMsg msg)
        {
            ReconServiceProxy proxy = new ReconServiceProxy(ConfigurationManager.AppSettings["IdentifyCustomerServiceUrl"]);
            string raws = proxy.identifyCustomer(msg);

            return raws;
        }

        public string unknownCashAdvisor(CaTaskMsg msg)
        {
            ReconServiceProxy proxy = new ReconServiceProxy(ConfigurationManager.AppSettings["UnknownCashAdvisorServiceUrl"]);
            string raws = proxy.unknownCashAdvisor(msg);

            return raws;
        }

        public string recon(CaTaskMsg msg)
        {
            ReconServiceProxy proxy = new ReconServiceProxy(ConfigurationManager.AppSettings["ReconServiceUrl"]);
            string raws = proxy.recon(msg);

            return raws;
        }

        public string paymentDetailRecon(CaTaskMsg msg)
        {
            ReconServiceProxy proxy = new ReconServiceProxy(ConfigurationManager.AppSettings["PaymentDetailReconServiceUrl"]);
            string raws = proxy.paymentDetailRecon(msg);

            return raws;
        }

        public string autoIdentifyCustomer(CaTaskMsg msg)
        {
            ReconServiceProxy proxy = new ReconServiceProxy(ConfigurationManager.AppSettings["AutoIdentifyCustomerServiceUrl"]);
            string raws = proxy.identifyCustomer(msg);

            return raws;
        }

        public string autoRecon(CaTaskMsg msg)
        {
            ReconServiceProxy proxy = new ReconServiceProxy(ConfigurationManager.AppSettings["AutoReconServiceUrl"]);
            string raws = proxy.recon(msg);

            return raws;
        }

        public CaReconMsgResultDto splitRecon(CaReconMsgDto msg)
        {
            ReconServiceProxy proxy = new ReconServiceProxy(ConfigurationManager.AppSettings["SplitReconServiceUrl"]);
            CaReconMsgResultDto result = proxy.splitRecon(msg);

            return result;
        }

        public string getNMReconIdByBsId(string bsId)
        {
            string sql = string.Format(@"SELECT
	                    TOP 1 t0.*
                    FROM
	                    T_CA_ReconBS t0 with (nolock)
                    INNER JOIN T_CA_Recon t1 with (nolock) ON t0.ReconId = t1.ID
                    WHERE
	                    t1.GroupType = 'NM'
                    AND t0.BANK_STATEMENT_ID = '{0}'", bsId);

            List<CaReconBSDto> list = CommonRep.ExecuteSqlQuery<CaReconBSDto>(sql).ToList();
            if (null != list && list.Count > 0)
            {
                return list[0].ReconId;
            }
            else
            {
                return "";
            }
        }

        public string getReconIdByBsId(string bsId)
        {
            string sql = string.Format(@"SELECT
	                    TOP 1 t0.*
                    FROM
	                    T_CA_ReconBS t0 with (nolock)
                    INNER JOIN T_CA_Recon t1 with (nolock) ON t0.ReconId = t1.ID
                    WHERE
	                    t1.GroupType NOT LIKE 'UN%'
                    AND t0.BANK_STATEMENT_ID = '{0}'", bsId);

            List<CaReconBSDto> list = CommonRep.ExecuteSqlQuery<CaReconBSDto>(sql).ToList();
            if (null != list && list.Count > 0)
            {
                return list[0].ReconId;
            }
            else
            {
                return "";
            }
        }

        public string getLastReconIdByBsId(string bsId,int isClosed=0)
        {
            string sql = string.Format(@"SELECT
	                            TOP 1 t0.*
                            FROM
	                            T_CA_ReconBS t0 WITH (nolock)
                            INNER JOIN T_CA_Recon t1 WITH (nolock) ON t0.ReconId = t1.ID
                            WHERE
	                            t1.GroupType NOT LIKE 'UN%'
                            AND t1.GroupType NOT LIKE 'NM%'
                            AND t0.BANK_STATEMENT_ID = '{0}'
                            AND t1.isClosed = {1}
                            ORDER BY t1.CREATE_DATE DESC", bsId, isClosed);

            List<CaReconBSDto> list = CommonRep.ExecuteSqlQuery<CaReconBSDto>(sql).ToList();
            if (null != list && list.Count > 0)
            {
                return list[0].ReconId;
            }
            else
            {
                return "";
            }
        }

        public string getLastReconIdWithOutCloseByBsId(string bsId)
        {
            string sql = string.Format(@"SELECT
	                            TOP 1 t0.*
                            FROM
	                            T_CA_ReconBS t0 WITH (nolock)
                            INNER JOIN T_CA_Recon t1 WITH (nolock) ON t0.ReconId = t1.ID
                            WHERE
	                            t1.GroupType NOT LIKE 'UN%'
                            AND t1.GroupType NOT LIKE 'NM%'
                            AND t0.BANK_STATEMENT_ID = '{0}'
                            ORDER BY t1.CREATE_DATE DESC", bsId);

            List<CaReconBSDto> list = CommonRep.ExecuteSqlQuery<CaReconBSDto>(sql).ToList();
            if (null != list && list.Count > 0)
            {
                return list[0].ReconId;
            }
            else
            {
                return "";
            }
        }

        public decimal getReconAmtByReconId(string reconId)
        {
            string sql = string.Format(@"SELECT
	                ISNULL(SUM(
		                CASE
		                WHEN t2.CURRENCY = t3.Func_Currency THEN
			                ISNULL(t0.Amount, 0)
		                ELSE
			                ISNULL(t0.LocalCurrencyAmount, 0)
		                END
	                ),0) AS amt
                FROM
	                T_CA_ReconDetail t0 with (nolock)
                INNER JOIN T_CA_ReconBS t1 with (nolock) ON t0.reconId=t1.reconId
                INNER JOIN T_CA_BankStatement t2 with (nolock) ON t1.BANK_STATEMENT_ID=t2.ID
                INNER JOIN T_CA_CustomerAttribute t3 with (nolock) ON t2.LegalEntity = t3.LegalEntity AND t2.CUSTOMER_NUM = t3.CUSTOMER_NUM
                WHERE
	                t0.ReconId = '{0}'", reconId);

            List<CaAmtDto> list = CommonRep.ExecuteSqlQuery<CaAmtDto>(sql).ToList();
            if (null != list && list.Count > 0)
            {
                return list[0].amt;
            }
            else
            {
                return Decimal.Zero;
            }
        }

        public string getReconIdByArId(string arId)
        {
            string sql = string.Format(@"SELECT
	                ReconId
                FROM
	                T_CA_ReconDetail with (nolock)
                WHERE
	                InvoiceNum = '{0}'", arId);

            return SqlHelper.ExcuteScalar<string>(sql);

        }

        public void deleteReconGroupByReconId(string reconId)
        {
            string sql1 = string.Format(@"DELETE FROM T_CA_Recon WHERE ID = '{0}'", reconId);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql1);

            string sql2 = string.Format(@"DELETE FROM T_CA_ReconBS WHERE ReconId = '{0}'", reconId);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql2);

            string sql3 = string.Format(@"DELETE FROM T_CA_ReconDetail WHERE ReconId = '{0}'", reconId);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql3);
        }

        /**
         * 根据bsId生成PMT group
         */
        public void createReconGroupByBSId(string bsId, string taskId, string strPmtId)
        {
            ICaPaymentDetailService paymentDetailService = SpringFactory.GetObjectImpl<ICaPaymentDetailService>("CaPaymentDetailService");
            CaCommonService commonService = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            ICaBankStatementService bankService = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            CaTaskService taskService = SpringFactory.GetObjectImpl<CaTaskService>("CaTaskService");

            CaTaskDto task = taskService.getCaTaskById(taskId);
            // 根据bsId查找pmt头信息
            CaPMTDto pmtDto = paymentDetailService.getPMTByBsId(bsId);
            if (!string.IsNullOrEmpty(strPmtId)) {
                pmtDto = paymentDetailService.getPMTById(strPmtId);
            }
            // 生成GroupName
            string key = commonService.getKey_inservice(CommonRep.GetDBContext().Database, "RECON-P");

            string reconId = Guid.NewGuid().ToString();
            CaReconDto reconDto = new CaReconDto();
            reconDto.ID = reconId;
            reconDto.GroupNo = key;
            reconDto.GroupType = "PMT";
            reconDto.TASK_ID = taskId;
            reconDto.CREATE_USER = task.CreateUser;
            reconDto.CREATE_DATE = DateTime.Now;
            reconDto.UPDATE_USER = task.CreateUser;
            reconDto.UPDATE_DATE = DateTime.Now;
            reconDto.PMT_ID = pmtDto.ID;
            try
            {
                createRecon(reconDto);
                // 根据pmtId查找bank数据并添加数据
                createReconBS(reconId, pmtDto.ID);
                // 根据pmtId查找AR数据并添加数据
                createReconDetail(reconId, pmtDto.ID);
            }
            catch (Exception ex) {
                Helper.Log.Error(ex.Message, ex);
            }
        }

        public void createRecon(CaReconDto reconDto)
        {
            var insertSql = string.Format(@"
                    INSERT INTO T_CA_Recon (
	                        ID,
	                        GroupNo,
	                        GroupType,
	                        TASK_ID,
	                        CREATE_USER,
	                        CREATE_DATE,
	                        UPDATE_USER,
	                        UPDATE_DATE,
	                        DEL_FLAG,
                            PMT_ID
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
		                        '{7}',
		                        '0',
                                N'{8}'
	                        )
                ", reconDto.ID,
                reconDto.GroupNo,
                reconDto.GroupType,
                reconDto.TASK_ID,
                reconDto.CREATE_USER,
                reconDto.CREATE_DATE,
                reconDto.UPDATE_USER,
                reconDto.UPDATE_DATE,
                reconDto.PMT_ID);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(insertSql);
        }


        /**
         * 根据pmt生成ReconBS
         */
        public void createReconBS(string reconId, string pmtReconId)
        {
            var insertSql = string.Format(@"
                    INSERT INTO T_CA_ReconBS (
                        ID,
                         ReconId,
                         SortId,
                         BANK_STATEMENT_ID,
                         Currency,
                         Amount
                        ) SELECT
	                        NEWID(),
	                        '{0}' AS ReconId,
	                        SortId,
	                        BANK_STATEMENT_ID,
	                        Currency,
	                        Amount
                        FROM
	                        T_CA_PMTBS with (nolock)
                        WHERE
	                        ReconId = '{1}'

                ", reconId, pmtReconId);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(insertSql);
        }

        /**
         * 根据bsId list生成ReconBS
         */
        public void createReconBS(string reconId, List<string> bsIds, CaReconDto reconDto, string comments="")
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            for (int i = 0; i<bsIds.Count(); i++)
            {// 查询bank数据
                CaBankStatementDto bank = service.getBankStatementById(bsIds[i]);
                bank.ISLOCKED = false;
                bank.Comments = comments;
                if (!reconDto.GroupType.Equals("NM"))
                {
                    bank.MATCH_STATUS = "4";
                }
                else
                {
                    bank.MATCH_STATUS = "2";
                }
                
                // 更新状态并解锁
                service.updateBank(bank);
                var insertSql = string.Format(@"
                    INSERT INTO T_CA_ReconBS (
                         ID,
                         ReconId,
                         SortId,
                         BANK_STATEMENT_ID,
                         Currency,
                         Amount
                        ) VALUES(
	                        NEWID(),
	                        N'{0}',
	                        N'{1}',
	                        N'{2}',
	                        N'{3}',
	                        N'{4}')
                ", reconId, i+1, bsIds[i], bank.CURRENCY,bank.CURRENT_AMOUNT);
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(insertSql);
            }
        }

        /**
         * 根据pmt生成ReconDetail
         */
        public void createReconDetail(string reconId, string pmtReconId)
        {
            var insertSql = string.Format(@"
                    INSERT INTO T_CA_ReconDetail (
	                        ID,
	                        ReconId,
	                        SortId,
	                        CUSTOMER_NUM,
	                        SiteUseId,
	                        InvoiceNum,
	                        InvoiceDate,
	                        DueDate,
	                        Currency,
	                        Amount,
	                        LocalCurrencyAmount
                        ) SELECT
	                        NEWID(),
	                        '{0}' AS ReconId,
	                        t0.SortId,
	                        t1.CUSTOMER_NUM,
	                        t1.SiteUseId,
	                        t1.INVOICE_NUM,
	                        t1.INVOICE_DATE,
	                        t1.DUE_DATE,
	                        t1.INV_CURRENCY,
	                        t0.Amount,
	                        t0.LocalCurrencyAmount
                        FROM
	                        T_CA_PMTDetail t0
                        INNER JOIN V_CA_AR_CM t1 ON t0.InvoiceNum = t1.INVOICE_NUM
                        WHERE
	                        ReconId = '{1}'
                ", reconId, pmtReconId);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(insertSql);
        }

        /**
         * 根据arId list生成ReconDetail
         */
        public void createReconDetail(string reconId, List<string> arIds)
        {
            for (int i = 0; i<arIds.Count(); i++)
            {
                CaARDto aRDto = getArByINVNum(arIds[i]);
                //在发送Recon数据前发票还未关闭，但返回结果时，发票已经Closed,此时不生成组合
                if(aRDto != null && !string.IsNullOrEmpty(aRDto.INVOICE_NUM)) { 
                    var insertSql = string.Format(@"
                        INSERT INTO T_CA_ReconDetail (
	                            ID,
	                            ReconId,
	                            SortId,
	                            CUSTOMER_NUM,
	                            SiteUseId,
	                            InvoiceNum,
	                            InvoiceDate,
	                            DueDate,
	                            Currency,
	                            Amount,
	                            LocalCurrencyAmount
                            )  VALUES(
	                            NEWID(),
	                            N'{0}',
	                            N'{1}',
	                            N'{2}',
	                            N'{3}',
	                            N'{4}',
	                            N'{5}',
	                            N'{6}',
	                            N'{7}',
	                            N'{8}',
	                            N'{9}')

                    ", reconId, i+1, aRDto.CUSTOMER_NUM, aRDto.SiteUseId,aRDto.INVOICE_NUM,aRDto.INVOICE_DATE,aRDto.DUE_DATE,aRDto.INV_CURRENCY,aRDto.AMT,aRDto.Local_AMT);
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(insertSql);
                }
            }
        }

        public CaReconTaskDto getCaReconTaskById(string id)
        {
            string sql = string.Format(@"SELECT * FROM T_CA_ReconTask with (nolock) WHERE ID = '{0}'", id);

            List<CaReconTaskDto> list = CommonRep.ExecuteSqlQuery<CaReconTaskDto>(sql).ToList();
            if (null != list && list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return new CaReconTaskDto();
            }
        }

        public CaReconTaskDto getCaReconTaskByTaskId(string id)
        {
            string sql = string.Format(@"SELECT * FROM T_CA_ReconTask with (nolock) WHERE TASK_ID = '{0}'", id);

            List<CaReconTaskDto> list = CommonRep.ExecuteSqlQuery<CaReconTaskDto>(sql).ToList();
            if (null != list && list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return new CaReconTaskDto();
            }
        }

        /**
         * 根据recon结果生成recon group
         */
        public void createReconGroupByRecon(List<string> bsIds, List<string> arIds, string comments, string menuregion, string taskId, string createUser)
        {
            CaCommonService commonService = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");

            int operateFlag = 0;

            // 判断bank是否在reconGroup中已存在
            string bsIdsStr = "";
            foreach (var id in bsIds)
            {
                bsIdsStr += "'" + id + "',";
            }
            if (bsIdsStr.Length > 0)
            {
                bsIdsStr = bsIdsStr.Substring(0, bsIdsStr.Length - 1);
                if (checkReconGroupByBsIds(bsIdsStr)>0)
                {
                    Helper.Log.Info("Bankstatement already has group");
                    // 存在则放弃该次结果
                    return;
                }
                // 判断bank状态是否为4或9
                if (checkBankStatusByBsIds(bsIdsStr,"'4','9'") > 0)
                {
                    Helper.Log.Info("Bankstatement status not 4 or 9");
                    // 存在则放弃该次结果
                    return;
                }
            }
            // 判断ar是否在reconGroup中已存在
            string arIdsStr = "";
            foreach (var id in arIds)
            {
                arIdsStr += "'" + id + "',";
            }
            if (arIdsStr.Length > 0)
            {
                if (checkReconGroupByArIdsHasClosed(arIds)) {
                    Helper.Log.Info("Recon Group has one or more closed ar.");
                    // 存在则放弃该次结果
                    string strSQL = string.Format("update T_CA_BankStatement set Comments = 'Recon Group has one or more closed ar.' where id in ({0})", bsIdsStr);
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(strSQL);
                    return;
                }
                arIdsStr = arIdsStr.Substring(0, arIdsStr.Length - 1);
                if (checkReconGroupByArIds(arIdsStr) > 0)
                {
                    Helper.Log.Info("Recon Group is in other bs.");
                    string strSQL = string.Format("update T_CA_BankStatement set Comments = N'Recon Group has one or more INV in another BS.' where id in ({0})", bsIdsStr);
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(strSQL);
                    // 存在则放弃该次结果
                    return;
                }
            }

            string reconId = Guid.NewGuid().ToString();
            CaReconDto reconDto = new CaReconDto();
            // 生成GroupName
            if ("Based on AR".Equals(comments))
            {
                reconDto.GroupType = "AR";
                reconDto.GroupNo = commonService.getKey("RECON-R");
            }
            else if("Based on Promise to Pay".Equals(comments))
            {
                reconDto.GroupType = "PTP";
                reconDto.GroupNo = commonService.getKey("RECON-T");
            }
            else if (comments.ToLower().Contains("failure"))
            {
                reconDto.GroupType = "NM";
                reconDto.GroupNo = "NM";
                operateFlag = 1;
            }
            else
            {
                reconDto.GroupType = "NM";
                reconDto.GroupNo = commonService.getKey("RECON-R");
            }

            reconDto.ID = reconId;
            reconDto.MENUREGION = menuregion;
            reconDto.TASK_ID = taskId;
            reconDto.CREATE_USER = createUser;
            reconDto.CREATE_DATE = DateTime.Now;
            reconDto.UPDATE_USER = createUser;
            reconDto.UPDATE_DATE = DateTime.Now;

            // 删除可能存在的NM数据
            foreach (var id in bsIds)
            {
                string nmReconId = getNMReconIdByBsId(id);
                deleteReconGroupByReconId(nmReconId);
            }

            createRecon(reconDto);
            createReconBS(reconId, bsIds, reconDto, comments);
            if(operateFlag == 0)
            {
                createReconDetail(reconId, arIds);
            }
            
        }

        public bool checkReconGroupByArIdsHasClosed(List<string> arIds) {
            bool lb_hasClosed = false;
            for (int i = 0; i < arIds.Count(); i++)
            {
                CaARDto aRDto = getArByINVNum(arIds[i]);
                //在发送Recon数据前发票还未关闭，但返回结果时，发票已经Closed,此时不生成组合
                if (aRDto == null || string.IsNullOrEmpty(aRDto.INVOICE_NUM))
                {
                    lb_hasClosed = true;
                    break;
                }
            }
            return lb_hasClosed;
        }
        public void createReconGroup(string taskId, List<string> bsIds, List<string> arIds)
        {
            // 判断bank是否在reconGroup中已存在
            string bsIdsStr = "";
            foreach (var id in bsIds)
            {
                bsIdsStr += "'" + id + "',";
            }
            if (bsIdsStr.Length > 0)
            {
                bsIdsStr = bsIdsStr.Substring(0, bsIdsStr.Length - 1);
                if (checkReconGroupByBsIds(bsIdsStr)>0)
                {
                    // 存在则放弃该次结果
                    throw new OTCServiceException("Recon group aready exist!");
                }
            }
            // 判断ar是否在reconGroup中已存在
            string arIdsStr = "";
            foreach (var id in arIdsStr)
            {
                arIdsStr += "'" + id + "',";
            }
            if (arIdsStr.Length > 0)
            {
                arIdsStr = arIdsStr.Substring(0, arIdsStr.Length - 1);
                if (checkReconGroupByArIds(arIdsStr) > 0)
                {
                    // 存在则放弃该次结果
                    throw new OTCServiceException("Recon group aready exist!");
                }
            }
            CaCommonService commonService = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            string reconId = Guid.NewGuid().ToString();
            CaReconDto reconDto = new CaReconDto();
            // 生成GroupName
            reconDto.GroupType = "MANUAL";
            reconDto.GroupNo = commonService.getKey("RECON-M");
            reconDto.ID = reconId;
            reconDto.TASK_ID = taskId;
            reconDto.CREATE_USER = AppContext.Current.User.EID;
            reconDto.CREATE_DATE = DateTime.Now;
            reconDto.UPDATE_USER = AppContext.Current.User.EID;
            reconDto.UPDATE_DATE = DateTime.Now;
            updateAdjustmentTimeByBsIds(bsIds);
            createRecon(reconDto);
            createReconBS(reconId, bsIds, reconDto);
            createReconDetail(reconId, arIds);
        }

        public CaARDto getArByINVNum(string invNum)
        {
            string sql = string.Format(@"SELECT
	                        TOP 1 
	                        LegalEntity,
	                        CUSTOMER_NUM,
	                        SiteUseId,
	                        INVOICE_NUM,
	                        INVOICE_DATE,
	                        DUE_DATE,
	                        FUNC_CURRENCY,
	                        INV_CURRENCY,
	                        ISNULL(AMT, 0) AS AMT,
	                        ISNULL(Local_AMT, 0) AS Local_AMT
                        FROM
	                        V_CA_AR_CM with(nolock)
                        WHERE INVOICE_NUM='{0}'", invNum);

            List<CaARDto> list = CommonRep.ExecuteSqlQuery<CaARDto>(sql).ToList();
            if (null != list && list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return new CaARDto();
            }
        }

        public CaReconGroupDtoPage getReconGroupList(string taskId)
        {
            CaReconGroupDtoPage result = new CaReconGroupDtoPage();

            List<CaReconBSDto> list = getReconBSByTaskId(taskId);
            var matchedBsIdsStr = "";
            if (list != null && list.Count > 0)
            {

                foreach (CaReconBSDto dto in list)
                {
                    matchedBsIdsStr += dto.BANK_STATEMENT_ID + ",";
                }
                if (matchedBsIdsStr.Length > 0)
                {
                    matchedBsIdsStr = matchedBsIdsStr.Substring(0, matchedBsIdsStr.Length - 1);

                    string sql = string.Format(@"SELECT
	                            *
                            FROM
	                            (
		                            SELECT
	                                    ROW_NUMBER () OVER (
		                                    ORDER BY
			                                    charindex(
				                                    t0.GROUP_TYPE,
				                                    'MANUALARPTPPMT'
			                                    ),
			                                    t0.GROUP_NO,
			                                    CASE
		                                    WHEN t0.TRANSACTION_NUMBER IS NULL THEN
			                                    1
		                                    ELSE
			                                    0
		                                    END ASC,
		                                    CASE
	                                    WHEN t0.INVOICE_NUM IS NULL THEN
		                                    1
	                                    ELSE
		                                    0
	                                    END ASC
	                                    ) AS RowNumber,
	                                    t0.RECONID,
	                                    t0.LegalEntity,
	                                    t0.TRANSACTION_NUMBER,
	                                    t0.VALUE_DATE,
	                                    t0.TRANSACTION_CURRENCY,
	                                    t0.TRANSACTION_AMOUNT,
	                                    t0.CUSTOMER_NUM,
	                                    t0.CUSTOMER_NAME,
	                                    t0.FORWARD_NUM,
	                                    t0.FORWARD_NAME,
	                                    t0.GROUP_NO,
	                                    t0.GROUP_TYPE,
	                                    t0.INVOICE_SITEUSEID,
	                                    t0.INVOICE_NUM,
	                                    t0.INVOICE_DUEDATE,
	                                    t0.INVOICE_CURRENCY,
	                                    t0.INVOICE_AMOUNT,
	                                    t2.Ebname,
	                                    isnull(t2.HasVAT,0) as HasVAT,
	                                    t3.MATCH_STATUS,
	                                    isnull(t4.isClosed,0) as isClosed
                                    FROM
	                                    fn_ReconResult ('{0}') t0
                                    LEFT JOIN V_CA_AR_ALL t2 with(nolock) ON ISNULL(t0.INVOICE_NUM,'') = t2.INVOICE_NUM
                                    LEFT JOIN T_CA_BankStatement t3 with(nolock) ON t0.bsId = t3.ID
                                    LEFT JOIN T_CA_Recon t4 WITH(nolock) ON t0.reconid=t4.id
                                    WHERE
	                                    t0.GROUP_TYPE NOT LIKE 'UN%'
                                    AND t0.GROUP_TYPE NOT LIKE 'NM%'
	                            ) AS t
                    ", matchedBsIdsStr);

                    List<CaReconGroupDto> dto = CommonRep.ExecuteSqlQuery<CaReconGroupDto>(sql).ToList();

                    if (null != dto && dto.Count > 0)
                    {
                        result.dataRows = dto;
                    }
                    else
                    {
                        result.dataRows = new List<CaReconGroupDto>();
                    }
                }
            }

            return result;
        }

        public CaBankStatementDtoPage getUnmatchBankList(string taskId)
        {
            CaBankStatementDtoPage result = new CaBankStatementDtoPage();

            string sql = string.Format(@"SELECT
	                        t2.*
                        FROM
	                        T_CA_ReconBS t0 with (nolock)
                        INNER JOIN T_CA_Recon t1 with (nolock) ON t1.ID = t0.ReconId
                        INNER JOIN T_CA_BankStatement t2 with (nolock) ON t0.BANK_STATEMENT_ID = t2.ID
                        WHERE
	                        t1.TASK_ID = '{0}' AND t1.GroupType='NM'
                        ", taskId);

            List<CaBankStatementDto> dto = CommonRep.ExecuteSqlQuery<CaBankStatementDto>(sql).ToList();

            if (null != dto && dto.Count > 0)
            {
                result.dataRows = dto;
            }
            else
            {
                result.dataRows = new List<CaBankStatementDto>();
            }

            return result;
        }

        public CaReconGroupDtoPage getReconGroupListByBSIds(string bsIds)
        {
            CaReconGroupDtoPage result = new CaReconGroupDtoPage();
            List<string> matchedBsIds = filterMatchedBankIds(bsIds);
            var matchedBsIdsStr = "";
            if (matchedBsIds != null && matchedBsIds.Count > 0)
            {

                foreach (var id in matchedBsIds)
                {
                    matchedBsIdsStr += id + ",";
                }
                if (matchedBsIdsStr.Length > 0)
                {
                    matchedBsIdsStr = matchedBsIdsStr.Substring(0, matchedBsIdsStr.Length - 1);
                    
                    string sql = string.Format(@"SELECT
	                                        *
                                        FROM
	                                        (
		                                        SELECT
	                                                ROW_NUMBER () OVER (
		                                                ORDER BY
			                                                charindex(
				                                                t0.GROUP_TYPE,
				                                                'MANUALARPTPPMT'
			                                                ),
			                                                t0.GROUP_NO,
			                                                CASE
		                                                WHEN t0.TRANSACTION_NUMBER IS NULL THEN
			                                                1
		                                                ELSE
			                                                0
		                                                END ASC,
		                                                CASE
	                                                WHEN t0.INVOICE_NUM IS NULL THEN
		                                                1
	                                                ELSE
		                                                0
	                                                END ASC
	                                                ) AS RowNumber,
	                                                t0.RECONID,
	                                                t0.LegalEntity,
	                                                t0.TRANSACTION_NUMBER,
	                                                t0.VALUE_DATE,
	                                                t0.TRANSACTION_CURRENCY,
	                                                t0.TRANSACTION_AMOUNT,
	                                                t0.CUSTOMER_NUM,
	                                                t0.CUSTOMER_NAME,
	                                                t0.FORWARD_NUM,
	                                                t0.FORWARD_NAME,
	                                                t0.GROUP_NO,
	                                                t0.GROUP_TYPE,
	                                                t0.INVOICE_SITEUSEID,
	                                                t0.INVOICE_NUM,
	                                                t0.INVOICE_DUEDATE,
	                                                t0.INVOICE_CURRENCY,
	                                                t0.INVOICE_AMOUNT,
	                                                t2.Ebname,
	                                                isnull(t2.HasVAT,0) as HasVAT,
	                                                t3.MATCH_STATUS,
	                                                isnull(t4.isClosed,0) as isClosed
                                                FROM
	                                                fn_ReconResult ('{0}') t0
                                                LEFT JOIN V_CA_AR_ALL t2 with(nolock) ON ISNULL(t0.INVOICE_SITEUSEID,'') = t2.SITEUSEID and ISNULL(t0.INVOICE_NUM,'') = t2.INVOICE_NUM
                                                LEFT JOIN T_CA_BankStatement t3 with(nolock) ON t0.bsId = t3.ID
                                                LEFT JOIN T_CA_Recon t4 WITH(nolock) ON t0.reconid=t4.id
                                                WHERE
	                                                t0.GROUP_TYPE NOT LIKE 'UN%'
                                                AND t0.GROUP_TYPE NOT LIKE 'NM%'
	                                        ) AS t
                    ", matchedBsIdsStr);

                    List<CaReconGroupDto> dto = CommonRep.ExecuteSqlQuery<CaReconGroupDto>(sql).ToList();

                    if (null != dto && dto.Count > 0)
                    {
                        result.dataRows = dto;
                    }
                    else
                    {
                        result.dataRows = new List<CaReconGroupDto>();
                    }
                }
            }

            return result;
        }

        public CaReconGroupDtoPage getReconGroupMultipleResultListByBSIds(string bsIds)
        {
            CaReconGroupDtoPage result = new CaReconGroupDtoPage();
            string sql = string.Format(@"SELECT
	                                        *
                                        FROM
	                                        (
		                                        SELECT
			                                        ROW_NUMBER () OVER (
				                                        ORDER BY
					                                        charindex(t0.GROUP_TYPE,'MANUALARPTPPMT'),t0.GROUP_NO,
					                                        CASE
				                                        WHEN t0.TRANSACTION_NUMBER IS NULL THEN
					                                        1
				                                        ELSE
					                                        0
				                                        END ASC,
				                                        CASE
			                                        WHEN t0.INVOICE_NUM IS NULL THEN
				                                        1
			                                        ELSE
				                                        0
			                                        END ASC
			                                        ) AS RowNumber,
			                                        t0.RECONID,
			                                        t0.LegalEntity,
			                                        t0.TRANSACTION_NUMBER,
			                                        t0.VALUE_DATE,
			                                        t0.TRANSACTION_CURRENCY,
			                                        t0.TRANSACTION_AMOUNT,
			                                        t0.CUSTOMER_NUM,
			                                        t0.CUSTOMER_NAME,
			                                        t0.FORWARD_NUM,
			                                        t0.FORWARD_NAME,
			                                        t0.GROUP_NO,
			                                        t0.GROUP_TYPE,
			                                        t0.INVOICE_SITEUSEID,
			                                        t0.INVOICE_NUM,
                                                    t0.INVOICE_DUEDATE,
                                                    t0.INVOICE_CURRENCY,
			                                        t0.INVOICE_AMOUNT,
                                                    t2.Ebname,
                                                    t2.HasVAT
		                                        FROM
			                                        fn_ReconResult ('{0}') t0
		                                        INNER JOIN V_CA_AR_ALL t2 ON t0.INVOICE_SITEUSEID = t2.SITEUSEID and t0.INVOICE_NUM = t2.INVOICE_NUM
		                                        WHERE
			                                        t0.GROUP_TYPE NOT LIKE 'UN%'
	                                        ) AS t
                    ", bsIds);

            List<CaReconGroupDto> dto = CommonRep.ExecuteSqlQuery<CaReconGroupDto>(sql).ToList();

            if (null != dto && dto.Count > 0)
            {
                result.dataRows = dto;
            }
            else
            {
                result.dataRows = new List<CaReconGroupDto>();
            }

            return result;
        }

        public CaBankStatementDtoPage getUnmatchBankListByBSIds(string bsIds)
        {
            CaBankStatementDtoPage result = new CaBankStatementDtoPage();

            List<string> unmatchBsIds = filterUnMatchBankIds(bsIds);
            var unmatchBsIdsStr = "";
            if (unmatchBsIds != null && unmatchBsIds.Count > 0)
            {

                foreach (var id in unmatchBsIds)
                {
                    unmatchBsIdsStr += "'" + id + "',";
                }
                if (unmatchBsIdsStr.Length > 0)
                {
                    unmatchBsIdsStr = unmatchBsIdsStr.Substring(0, unmatchBsIdsStr.Length - 1);

                    string sql = string.Format(@"SELECT * FROM T_CA_BankStatement WHERE ID IN ({0})", unmatchBsIdsStr);

                    List<CaBankStatementDto> dto = CommonRep.ExecuteSqlQuery<CaBankStatementDto>(sql).ToList();

                    if (null != dto && dto.Count > 0)
                    {
                        result.dataRows = dto;
                    }
                    else
                    {
                        result.dataRows = new List<CaBankStatementDto>();
                    }
                }
            }

            return result;
        }

        public CaARDtoPage getArListByCustomerNum(CaCustomerInputrDto[] customerList)
        {
            CaARDtoPage result = new CaARDtoPage();
            
            if (customerList != null && customerList.Length > 0)
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT ");
                sql.Append("LegalEntity,");
                sql.Append("CUSTOMER_NUM,");
                sql.Append("SiteUseId,");
                sql.Append("INVOICE_NUM,");
                sql.Append("INVOICE_DATE,");
                sql.Append("DUE_DATE,");
                sql.Append("func_currency,");
                sql.Append("INV_CURRENCY,");
                sql.Append("ISNULL(AMT, 0) AS AMT,");
                sql.Append("ISNULL(Local_AMT, 0) AS Local_AMT,");
                sql.Append("Ebname,");
                sql.Append("HasVAT,");
                sql.Append("OrderNumber, ");
                sql.Append("(SELECT count(*) from T_CA_PMTDetail with (nolock) WHERE InvoiceNum=V_CA_AR_CM.INVOICE_NUM) AS pmtCount ");
                sql.Append("FROM ");
                sql.Append("V_CA_AR_CM ");
                sql.Append("WHERE ");
                for(var i = 0; i<customerList.Length; i++)
                {
                    if(i == 0)
                    {
                        sql.Append("(LegalEntity = '" + customerList[i].LegalEntity + "' AND CUSTOMER_NUM = '" + customerList[i].customerNum + "') ");
                    }
                    else
                    {
                        sql.Append("OR (LegalEntity = '" + customerList[i].LegalEntity + "' AND CUSTOMER_NUM = '" + customerList[i].customerNum + "') ");
                    }
                }
                sql.Append("ORDER BY DUE_DATE ASC ");

                List<CaARDto> dto = CommonRep.ExecuteSqlQuery<CaARDto>(sql.ToString()).ToList();

                result.dataRows = dto;
            }

            return result;
        }

        public void unGroupReconGroupByReconId(string reconId)
        {
            string sql1 = string.Format(@"UPDATE T_CA_Recon SET GroupNo='NM',GroupType='NM' WHERE ID = '{0}'", reconId);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql1);

            string sql2 = string.Format(@"UPDATE T_CA_BankStatement
                        SET ISLOCKED = 0,
                        MATCH_STATUS = '2'
                        WHERE
	                        ID IN (
		                        SELECT
			                        BANK_STATEMENT_ID
		                        FROM
			                        T_CA_ReconBS
		                        WHERE
			                        ReconId = '{0}'
	                        )", reconId);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql2);
            
            string sql3 = string.Format(@"DELETE FROM T_CA_ReconDetail WHERE ReconId = '{0}'", reconId);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql3);


        }

        public int checkReconGroupByBsIds(string bsIds)
        {
            string sql1 = string.Format(@"SELECT
	                    COUNT (*) AS COUNT
                    FROM
	                    T_CA_Recon t0 with(nolock)
                    INNER JOIN T_CA_ReconBS t1 with(nolock) ON t0.id = t1.ReconId
                    INNER JOIN T_CA_BankStatement t2 with(nolock) ON t1.BANK_STATEMENT_ID = t2.ID
                    WHERE
	                    t1.BANK_STATEMENT_ID IN ({0})
                    AND t0.GroupType != 'NM'
                    AND  (t0.GroupType not like 'UN%')
                    AND t2.MATCH_STATUS != '9'
                    AND t0.isClosed=0", bsIds);

            return SqlHelper.ExcuteScalar<int>(sql1);
        }

        public int checkBankStatusByBsIds(string bsIds, string status)
        {
            string sql1 = string.Format(@"SELECT count(*) AS COUNT FROM T_CA_BankStatement with(nolock) WHERE ID IN ({0}) AND MATCH_STATUS IN ({1})", bsIds,status);

            return SqlHelper.ExcuteScalar<int>(sql1);
        }

        public int checkReconGroupByArIds(string arIds)
        {
            string sql1 = string.Format(@"select count(*) as count from T_CA_Recon t0 with (nolock) inner join T_CA_ReconDetail t1 with (nolock) on t0.id = t1.ReconId where t1.InvoiceNum in ({0}) and t0.GroupType != 'NM' and (t0.GroupType not like 'UN%')", arIds);

            return SqlHelper.ExcuteScalar<int>(sql1);
        }

        public List<string> filterUnMatchBankIds(string bankIds)
        {
            string[] bs = bankIds.Split(',');
            string bsStr = "";
            foreach (var id in bs)
            {
                bsStr += "'" + id + "',";
            }
            if (bsStr.Length > 0)
            {
                bsStr = bsStr.Substring(0, bsStr.Length - 1);
            }
            
            string bankIdSql = string.Format(@"SELECT
                        ID
                    FROM
                        T_CA_BankStatement
                    WHERE
                        (
                            ISLOCKED <> 1
                            OR ISLOCKED IS NULL
                        )
                    AND MATCH_STATUS IN (2,3)
                    AND ID IN({0})", bsStr);

            return CommonRep.ExecuteSqlQuery<string>(bankIdSql).ToList();

        }

        public List<string> filterMatchedBankIds(string bankIds)
        {
            string[] bs = bankIds.Split(',');
            string bsStr = "";
            foreach (var id in bs)
            {
                bsStr += "'" + id + "',";
            }
            if (bsStr.Length > 0)
            {
                bsStr = bsStr.Substring(0, bsStr.Length - 1);
            }

            string bankIdSql = string.Format(@"SELECT
                        ID
                    FROM
                        T_CA_BankStatement
                    WHERE
                        (
                            ISLOCKED <> 1
                            OR ISLOCKED IS NULL
                        )
                    AND MATCH_STATUS IN (2,4,9)
                    AND ID IN ({0})", bsStr);

            return CommonRep.ExecuteSqlQuery<string>(bankIdSql).ToList();

        }

        public void updateAdjustmentTimeByBsIds(List<string> bsIds)
        {
            string bsStr = "";
            foreach (var id in bsIds)
            {
                bsStr += "'" + id + "',";
            }
            if (bsStr.Length > 0)
            {
                bsStr = bsStr.Substring(0, bsStr.Length - 1);
                string sql = string.Format(@"UPDATE T_CA_BankStatement
                                SET ADJUSTMENT_TIME = '{0}'
                                WHERE
	                                ID IN ({1})", AppContext.Current.User.Now, bsStr);

                CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);
            }
        }

        public HttpResponseMessage exporReconResultByTaskId(string taskId)
        {
            try
            {
                //模板文件  
                string templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportReconTemplate"].ToString());
                string fileName = "Recon_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                string tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                List<CaReconGroupDto> reconList = getReconGroupList(taskId).dataRows;
                List<CaBankStatementDto> unmatchList = getUnmatchBankList(taskId).dataRows;

                this.SetData(templateFile, tmpFile, reconList, unmatchList);

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
        }

        private void SetData(string templateFileName, string tmpFile, List<CaReconGroupDto> reconList, List<CaBankStatementDto> unmatchList)
        {
            try
            {
                NpoiHelper helper = new NpoiHelper(templateFileName);
                helper.Save(tmpFile, true);
                helper = new NpoiHelper(tmpFile);

                if (null!= reconList && reconList.Count > 0)
                {
                    helper.ActiveSheet = 0;
                    int intStartRow = 1;
                    foreach (CaReconGroupDto reconGroup in reconList)
                    {
                        helper.SetData(intStartRow, 0, reconGroup.TRANSACTION_NUMBER);
                        helper.SetData(intStartRow, 1, reconGroup.TRANSACTION_AMOUNT);
                        helper.SetData(intStartRow, 2, reconGroup.TRANSACTION_CURRENCY);
                        helper.SetData(intStartRow, 3, reconGroup.CUSTOMER_NUM);
                        helper.SetData(intStartRow, 4, reconGroup.CUSTOMER_NAME);
                        helper.SetData(intStartRow, 5, reconGroup.VALUE_DATE);
                        helper.SetData(intStartRow, 6, reconGroup.LegalEntity);
                        helper.SetData(intStartRow, 7, reconGroup.GROUP_NO);
                        helper.SetData(intStartRow, 8, reconGroup.GROUP_TYPE);
                        helper.SetData(intStartRow, 9, reconGroup.INVOICE_NUM);
                        helper.SetData(intStartRow, 10, reconGroup.INVOICE_DUEDATE);
                        helper.SetData(intStartRow, 11, reconGroup.INVOICE_CURRENCY);
                        helper.SetData(intStartRow, 12, reconGroup.INVOICE_AMOUNT);
                        helper.SetData(intStartRow, 13, reconGroup.INVOICE_SITEUSEID);
                        helper.SetData(intStartRow, 14, reconGroup.Ebname);
                        intStartRow++;
                    }
                }
                if (null != unmatchList && unmatchList.Count > 0)
                {
                    helper.ActiveSheet = 1;
                    int intStartRow = 1;
                    foreach (CaBankStatementDto bank in unmatchList)
                    {
                        helper.SetData(intStartRow, 0, bank.LegalEntity);
                        helper.SetData(intStartRow, 1, bank.TRANSACTION_NUMBER);
                        helper.SetData(intStartRow, 2, bank.TRANSACTION_AMOUNT);
                        helper.SetData(intStartRow, 3, bank.CURRENCY);
                        helper.SetData(intStartRow, 4, bank.REF1);
                        helper.SetData(intStartRow, 5, bank.CUSTOMER_NUM);
                        helper.SetData(intStartRow, 6, bank.CUSTOMER_NAME);
                        helper.SetData(intStartRow, 7, bank.VALUE_DATE);
                        intStartRow++;
                    }
                }
                helper.ActiveSheet = 0;
                helper.Save(tmpFile, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public string exporReconResultByBsIds(string bsIds)
        {
            try
            {
                //模板文件  
                string templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportReconTemplate"].ToString());
                string fileName = "Recon_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                string tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                List<CaReconGroupDto> reconList = getReconGroupListByBSIds(bsIds).dataRows;
                List<CaBankStatementDto> unmatchList = getUnmatchBankListByBSIds(bsIds).dataRows;

                this.SetData(templateFile, tmpFile, reconList, unmatchList);

                //插入T_File
                List<string> listSQL = new List<string>();
                string strFileId = System.Guid.NewGuid().ToString();
                string strFileName = Path.GetFileName(tmpFile);
                StringBuilder strFileSql = new StringBuilder();
                strFileSql.Append("INSERT INTO T_FILE ( FILE_ID,FILE_NAME,PHYSICAL_PATH,OPERATOR,CREATE_TIME)");
                strFileSql.Append(" VALUES ('" + strFileId + "',");
                strFileSql.Append("         '" + strFileName + "',");
                strFileSql.Append("         '" + tmpFile + "',");
                strFileSql.Append("         '" + AppContext.Current.User.EID + "',getdate())");
                listSQL.Add(strFileSql.ToString());
                SqlHelper.ExcuteListSql(listSQL);

                return strFileId;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public void changeToMatch(string bsId)
        {
            // 根据bsId找到reconId
            string reconId = getReconIdByBsId(bsId);
            // 根据reconId修改NM --> AR
            string sql1 = string.Format(@"UPDATE T_CA_Recon SET GroupType='AR' WHERE ID = '{0}'", reconId);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql1);
            // 根据bsId修改matchStatus为4 matched
            string sql2 = string.Format(@"UPDATE T_CA_BankStatement SET MATCH_STATUS=4 WHERE ID = '{0}'", bsId);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql2);
        }

        public string groupExport(CaCustomerInputrDto[] customerList)
        {
            string templateFile = "";
            string fileName = "";
            string tmpFile = "";

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["TemplateReconAr"].ToString());
                fileName = "ReconAr" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                CaARDtoPage caARDtoPage = getArListByCustomerNum(customerList);

                this.SetData(templateFile, tmpFile, caARDtoPage.dataRows);

                string strFileId = Guid.NewGuid().ToString();
                string strFileName = Path.GetFileName(tmpFile);
                StringBuilder sqlFile = new StringBuilder();
                sqlFile.Append("INSERT INTO T_FILE (FILE_ID,FILE_NAME,PHYSICAL_PATH,OPERATOR,CREATE_TIME)");
                sqlFile.Append(" VALUES (N'" + strFileId + "',");
                sqlFile.Append("         N'" + strFileName + "',");
                sqlFile.Append("         N'" + tmpFile + "',");
                sqlFile.Append("         N'" + AppContext.Current.User.EID + "',GETDATE());");
                SqlHelper.ExcuteSql(sqlFile.ToString());

                return strFileId;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        private void SetData(string templateFileName, string tmpFile, List<CaARDto> list)
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
                foreach (var lst in list)
                {
                    helper.SetData(rowNo, 0, lst.pmtCount);
                    helper.SetData(rowNo, 1, lst.HasVAT);
                    helper.SetData(rowNo, 2, lst.SiteUseId);
                    helper.SetData(rowNo, 3, lst.INVOICE_NUM);
                    helper.SetData(rowNo, 4, lst.func_currency);
                    helper.SetData(rowNo, 5, lst.AMT);
                    helper.SetData(rowNo, 6, lst.Local_AMT);
                    helper.SetData(rowNo, 7, lst.DUE_DATE);
                    helper.SetData(rowNo, 8, lst.EbName);

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

        public void checkCloseReconGroupByReconId(string reconId)
        {
            string sql = string.Format(@"SELECT
	                        COUNT (*) AS COUNT
                        FROM
	                        T_CA_BankStatement
                        WHERE
	                        ID IN (
		                        SELECT
			                        BANK_STATEMENT_ID
		                        FROM
			                        T_CA_ReconBS
		                        WHERE
			                        ReconId = '{0}'
	                        )
                        AND (MATCH_STATUS = 9
                        OR EXISTS(SELECT ID FROM
	                        T_CA_Recon
                        WHERE
	                        GroupType NOT LIKE 'NM%'
                        AND GroupType NOT LIKE 'UN%'
                        AND isClosed=1
                        AND ID = '{0}'))", reconId);

            if (CommonRep.ExecuteSqlQuery<CountDto>(sql).ToList()[0].COUNT > 0) {
                // 存在则放弃该次结果
                throw new OTCServiceException("Can't ungroup closed recon group!");
            }
        }

        public List<CaReconBSDto> getReconBSByTaskId(string taskId)
        {
            string sql = string.Format(@"SELECT
	                        *
                        FROM
	                        T_CA_ReconBS
                        WHERE
	                        ReconId IN (
		                        SELECT
			                        ID
		                        FROM
			                        T_CA_Recon
		                        WHERE
			                        TASK_ID = '{0}'
	                        )", taskId);

            return CommonRep.ExecuteSqlQuery<CaReconBSDto>(sql).ToList();
        }

    }
}
