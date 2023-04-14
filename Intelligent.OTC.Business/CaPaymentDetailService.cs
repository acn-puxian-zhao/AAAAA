using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using System.Collections.Generic;
using System;
using Intelligent.OTC.Domain.DomainModel;
using System.Linq;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Common;
using System.Data.SqlClient;
using System.Transactions;
using Intelligent.OTC.Common.Exceptions;
using Newtonsoft.Json;
using System.Text;

namespace Intelligent.OTC.Business
{
    public class CaPaymentDetailService : ICaPaymentDetailService
    {
        public OTCRepository CommonRep { get; set; }

        public int countByBsId(string bsId)
        {
            string sql = string.Format(@"SELECT
	                    COUNT (*) AS COUNT
                    FROM
	                    T_CA_PMTBS with (nolock)
                    WHERE
	                    BANK_STATEMENT_ID = '{0}'", bsId);


            return SqlHelper.ExcuteScalar<int>(sql);
        }

        public void deletePMTById(string id)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    //先删除detail
                    string delPMTDetailsql = string.Format(@"delete from T_CA_PMTDetail where ReconId = '{0}'", id);
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(delPMTDetailsql);

                    //再删除bs
                    string delPMTBSsql = string.Format(@"delete from T_CA_PMTBS where ReconId = '{0}'", id);
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(delPMTBSsql);

                    //最后删除主表
                    string delPMTsql = string.Format(@"delete from T_CA_PMT where ID = '{0}'", id);
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(delPMTsql);

                    scope.Complete();
                }
            }
            catch (Exception ex) 
            {
                throw new OTCServiceException("Delete failure.");
            }
        }

        public CaPMTDto getPMTByBsId(string id)
        {
            string sql = string.Format(@"SELECT
	                    TOP 1 t0.*
                    FROM
	                    T_CA_PMT t0 with (nolock)
                    INNER JOIN T_CA_PMTBS t1 with (nolock) ON t0.id=t1.ReconId
                    WHERE t1.BANK_STATEMENT_ID  = '{0}'", id);

            List<CaPMTDto> list = CommonRep.ExecuteSqlQuery<CaPMTDto>(sql).ToList();
            if (null != list && list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return new CaPMTDto();
            }
        }

        public CaPMTDto getPMTById(string id)
        {
            string sql = string.Format(@"SELECT
                                        a.*,
	                                    c.CUSTOMER_NAME AS CustomerName
                                    FROM T_CA_PMT a with (nolock)
	                                LEFT JOIN V_CA_Customer c with (nolock) on a.CustomerNum = c.CUSTOMER_NUM
                                    WHERE a.ID = '{0}'", id);

            List<CaPMTDto> list = CommonRep.ExecuteSqlQuery<CaPMTDto>(sql).ToList();
            if (null != list && list.Count > 0)
            {
                CaPMTDto dto = list[0];

                string bssql = string.Format(@"SELECT
	                                            a.ID,
	                                            a.ReconId,
	                                            a.SortId,
	                                            a.BANK_STATEMENT_ID,
	                                            a.Currency,
	                                            a.Amount,
	                                            b.TRANSACTION_NUMBER AS TransactionNumber,
	                                            b.VALUE_DATE AS ValueDate,
	                                            b.Description AS Description
                                            FROM
	                                            T_CA_PMTBS a with (nolock)
	                                        LEFT JOIN T_CA_BankStatement b with (nolock) ON b.ID = a.BANK_STATEMENT_ID
                                            WHERE
	                                            a.ReconId = '{0}'", id);
                List<CaPMTBSDto> bsList = CommonRep.ExecuteSqlQuery<CaPMTBSDto>(bssql).ToList();

                string detailsql = string.Format(@"SELECT
	                                                a.ID,
	                                                a.ReconId,
	                                                a.SortId,
	                                                b.CUSTOMER_NUM,
	                                                b.SiteUseId,
	                                                a.InvoiceNum,
	                                                a.Currency,
	                                                b.INVOICE_DATE AS InvoiceDate,
	                                                b.DUE_DATE AS DueDate,
	                                                a.Amount,
                                                    b.legalEntity
                                                FROM
	                                                T_CA_PMTDetail a
	                                            LEFT JOIN V_CA_AR b ON a.InvoiceNum = b.INVOICE_NUM
                                                WHERE
	                                                a.ReconId = '{0}'", id);
                List<CaPMTDetailDto> detailList = CommonRep.ExecuteSqlQuery<CaPMTDetailDto>(detailsql).ToList();
                dto.PmtBs = bsList;
                dto.PmtDetail = detailList;
                return dto;
            }
            else
            {
                return new CaPMTDto();
            }
        }

        public string getPMTIdByBsId(string bsId)
        {
            string sql = string.Format(@"SELECT
	                        TOP 1 t0.*
                        FROM
	                        T_CA_PMTBS t0 WITH (nolock)
                        INNER JOIN T_CA_PMT t1 WITH(nolock) ON t0.ReconId=t1.ID
                        WHERE
	                        t0.BANK_STATEMENT_ID = '{0}'
                        AND t1.isClosed <> '1'
                        AND EXISTS (
	                        SELECT
		                        ID
	                        FROM
		                        T_CA_PMTDetail WITH (nolock)
	                        WHERE
		                        ReconId = t0.ReconId
                        )
                        ORDER BY t1.CREATE_DATE DESC", bsId);

            List<CaPMTBSDto> list = CommonRep.ExecuteSqlQuery<CaPMTBSDto>(sql).ToList();
            if(null != list && list.Count > 0)
            {
                return list[0].ReconId;
            }
            else
            {
                return "";
            }
        }

        public string savePMTDetail(CaPMTDto dto)
        {
            string pmtID = dto.ID;
            string taskId = "";
            CaCommonService caCommonService = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            CaTaskService caTaskService = SpringFactory.GetObjectImpl<CaTaskService>("CaTaskService");
            string successStr = "Save success.";
            try
            {
                StringBuilder bankIdsSb = new StringBuilder();
                if (dto.PmtBs.Count > 0)
                {
                    foreach (CaPMTBSDto rb in dto.PmtBs)
                    {
                        bankIdsSb.Append("," + rb.BANK_STATEMENT_ID);
                    } 
                }

                if (String.IsNullOrEmpty(pmtID))
                {
                    DateTime now = AppContext.Current.User.Now;
                    dto.CREATE_DATE = now;
                    if (bankIdsSb.Length > 0)
                    {
                        string[] bankIdArr = bankIdsSb.ToString().Substring(1).Split(',');
                        taskId = caTaskService.createTask(2, bankIdArr, "", "", now);
                        dto.TASK_ID = taskId;
                    }
                    else
                    {
                        string[] bankIdArr = "".Split(',');
                        taskId = caTaskService.createTask(2, bankIdArr, "", "", now);
                        dto.TASK_ID = taskId;
                    }
                }

                string checkResult = caCommonService.CheckAndsavePMT(dto);

                if (!string.Equals(checkResult,"OK")) {
                    throw new Exception(checkResult);
                }

                if (!String.IsNullOrEmpty(taskId))
                {
                    caTaskService.updateTaskStatusById(taskId, "2");
                }
                return successStr;
            }
            catch (Exception ex)
            {
                if (!String.IsNullOrEmpty(taskId))
                {
                    caTaskService.updateTaskStatusById(taskId, "3");//异常
                }
                throw new Exception(ex.Message);
            }
        }


        public CaBankStatementDto GetBankStatementByTranINC(string transactionNumber)
        {
            string sql = String.Format(@"select top 1 TRANSACTION_NUMBER,
	                                        CURRENT_AMOUNT,
	                                        ID,
	                                        currency,
	                                        VALUE_DATE,
	                                        description
                                        from T_CA_BankStatement with (nolock) 
                                        where TRANSACTION_NUMBER = '{0}'
                                        and DEL_FLAG = 0", transactionNumber);
            List<CaBankStatementDto> list = SqlHelper.GetList<CaBankStatementDto>(SqlHelper.ExcuteTable(sql, System.Data.CommandType.Text, null));
            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public CaPMTDetailDto GetInvoiceInfoByNum(string invoiceNum)
        {
            string sql = String.Format(@"SELECT
                                        CUSTOMER_NUM AS CUSTOMER_NUM,
	                                    SiteUseId AS SiteUseId,
	                                    INVOICE_NUM AS InvoiceNum,
	                                    INVOICE_DATE AS InvoiceDate,
	                                    DUE_DATE AS DueDate,
	                                    INV_CURRENCY AS Currency,
                                        AMT AS Amount,
                                        LegalEntity AS LegalEntity
                                    FROM
	                                    V_CA_AR
                                    WHERE
	                                    INVOICE_NUM = '{0}'", invoiceNum);
            List<CaPMTDetailDto> list = SqlHelper.GetList<CaPMTDetailDto>(SqlHelper.ExcuteTable(sql, System.Data.CommandType.Text, null));
            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public void savePMTBSByBank(CaBankStatementDto bank, string pmtId)
        {
            string id = Guid.NewGuid().ToString();
            var insertSql = string.Format(@"
                    INSERT INTO T_CA_PMTBS (
	                        ID,
	                        ReconId,
	                        SortId,
	                        BANK_STATEMENT_ID,
	                        Currency,
	                        Amount
                        )
                        VALUES
	                        (
		                        N'{0}',
		                        N'{1}',
		                        N'{2}',
		                        N'{3}',
		                        N'{4}',
		                        N'{5}'
	                        )
                ", id,
                pmtId,
                1,
                bank.ID,
                bank.CURRENCY,
                bank.CURRENT_AMOUNT);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(insertSql);
        }

        public bool checkPMTAvailable(string pmtId)
        {
            // 根据pmtId获取count
            string pmtCountSql = string.Format(@"SELECT count(*) AS COUNT FROM T_CA_PMTDetail with(nolock) WHERE ReconId='{0}'", pmtId);
            int pmtCount = CommonRep.ExecuteSqlQuery<CountDto>(pmtCountSql).ToList()[0].COUNT;

            string arCountSql = string.Format(@"SELECT
	                            count(*) AS COUNT
                            FROM
	                            T_CA_PMTDetail t0 with(nolock)
                            INNER JOIN V_CA_AR_CM t1 with(nolock) ON t0.InvoiceNum = t1.INVOICE_NUM
                            WHERE
	                            t0.ReconId = '{0}'", pmtId);
            int arCount = CommonRep.ExecuteSqlQuery<CountDto>(arCountSql).ToList()[0].COUNT;

            if (pmtCount == arCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void changePMTBsId(string bsId, string pmtId)
        {
            // 根据bsId清除未关闭的PMT的BS信息
            deletePmtBSByBsId(bsId);
            // 根据pmtId清除未关闭的PMT的BS信息
            deletePmtBSByPmtId(pmtId);
            // 根据pmtId和bsId插入PMTBS中
            CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
            CaReconService reconService = SpringFactory.GetObjectImpl<CaReconService>("CaReconService");
            savePMTBSByBank(service.getBankStatementById(bsId), pmtId);
            // 根据PMT信息修改customer
            CaPMTDto pmt = getPMTById(pmtId);
            string customerNum = pmt.CustomerNum ?? "";
            string customerName = pmt.CustomerName ?? "";
            CaBankStatementDto bank = service.getBankStatementById(bsId);
            
            if (Convert.ToInt32(bank.MATCH_STATUS) > 3)
            {
                // 抛出提示信息
                throw new OTCServiceException("This BankStatement Can't Select Payment Detail!");
            }
            // 判断是否为forward
            List<CAForwarderListDto> forwarderLst = service.getForwarderListByCustomerName(bank.REF1).FindAll(a => a.LegalEntity == bank.LegalEntity).ToList<CAForwarderListDto>();
            if(forwarderLst.Count == 0)
            {
                bank.FORWARD_NUM = customerNum;
                bank.FORWARD_NAME = customerName;
                bank.CUSTOMER_NUM = customerNum;
                bank.CUSTOMER_NAME = customerName;
                bank.MATCH_STATUS = "2";
            }
            else
            {
                bank.FORWARD_NUM = forwarderLst[0].ForwardNum;
                bank.FORWARD_NAME = forwarderLst[0].ForwardName;
                bank.CUSTOMER_NUM = customerNum;
                bank.CUSTOMER_NAME = customerName;
                bank.MATCH_STATUS = "2";
            }
            // 判断是否有AR，若存在AR则形成组合
            if (pmt.PmtDetail.Count > 0)
            {
                if (checkPMTAvailable(pmtId))
                {
                    // 生成group
                    reconService.createReconGroupByBSId(bsId, "Manual select pmt", pmtId);
                    // 修改状态为matched并解锁
                    bank.MATCH_STATUS = "4";
                    bank.ISLOCKED = false;
                    bank.Comments = "Base on PMT";
                }
            }
            service.updateBank(bank);
        }

        public void deletePmtBSByBsId(string bsId)
        {
            //删除payment中此条bs
            string deletePMTBS = string.Format(@"DELETE FROM T_CA_PMTBS WHERE BANK_STATEMENT_ID = '{0}' AND EXISTS(SELECT 1 FROM T_CA_PMT WHERE ID = T_CA_PMTBS.ReconId AND isClosed = 0)", bsId);

            CommonRep.GetDBContext().Database.ExecuteSqlCommand(deletePMTBS);
        }

        public void deletePmtBSByPmtId(string pmtId)
        {
            //删除payment中此条bs
            string deletePMTBS = string.Format(@"DELETE FROM T_CA_PMTBS WHERE ReconId = '{0}' AND EXISTS(SELECT 1 FROM T_CA_PMT WHERE ID = T_CA_PMTBS.ReconId AND isClosed = 0)", pmtId);

            CommonRep.GetDBContext().Database.ExecuteSqlCommand(deletePMTBS);
        }
    }


}