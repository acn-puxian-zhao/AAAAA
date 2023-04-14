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
using System.Net.Http;
using System.Web;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using Intelligent.OTC.Domain.DataModel;
using NPOI.SS.UserModel;
using System.Data;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.XWPF.UserModel;
using log4net.Repository.Hierarchy;

namespace Intelligent.OTC.Business
{
    public class CaBankStatementService : ICaBankStatementService
    {

        public OTCRepository CommonRep { get; set; }

        public CaReconService CaReconService { get; set; }

        public CaBankStatementDtoPage getCaBankStatementList(string statusselect, string legalEntity, string transNumber, string transcurrency, string transamount, string transCustomer, string transaForward, string valueDateF, string valueDateT, string createDateF, string createDateT, string ishistory,string bsType, int page, int pageSize)
        {
            CaBankStatementDtoPage result = new CaBankStatementDtoPage();

            string collecotrList = "";
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");     //注入服务
            var userId = AppContext.Current.User.EID; //当前用户ID
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");       //获得用户List(它这里用逗号间隔拼了一下)
            if (!string.IsNullOrEmpty(collecotrList))
            {
                collecotrList = collecotrList.Substring(0, collecotrList.LastIndexOf(","));
                collecotrList = collecotrList.Replace(",", "','");
            }
            collecotrList = "'" + collecotrList + "'";

            if (string.IsNullOrEmpty(statusselect)) {
                statusselect = "";
            }
            statusselect = "'" + statusselect.Replace(",", "','") + "'";
            if (string.IsNullOrEmpty(legalEntity) || legalEntity == "undefined") {
                legalEntity = "";
            }
            if (string.IsNullOrEmpty(transNumber) || transNumber == "undefined") {
                transNumber = "";
            }
            if (string.IsNullOrEmpty(transcurrency) || transcurrency == "undefined")
            {
                transcurrency = "";
            }
            if (string.IsNullOrEmpty(transamount) || transamount == "undefined" || transamount == "null")
            {
                transamount = "";
            }
            if (string.IsNullOrEmpty(transCustomer) || transCustomer == "undefined")
            {
                transCustomer = "";
            }
            if (string.IsNullOrEmpty(transaForward) || transaForward == "undefined")
            {
                transaForward = "";
            }
            if (string.IsNullOrEmpty(valueDateF) || valueDateF == "undefined") {
                valueDateF = "";
            }
            if (string.IsNullOrEmpty(valueDateT) || valueDateT == "undefined")
            {
                valueDateT = "";
            }
            if (string.IsNullOrEmpty(createDateF) || createDateF == "undefined")
            {
                createDateF = "";
            }
            if (string.IsNullOrEmpty(createDateT) || createDateT == "undefined")
            {
                createDateT = "";
            }  
            if (string.IsNullOrEmpty(bsType) || bsType == "undefined")
            {
                bsType = "";
            }
            string sql = string.Format(@"SELECT
                                    *,
                                    ISNULL((select top 1 (case when status in ('Initialized','Processing') then 1 when status='Finish' then 2 else 0 end ) from T_CA_MailAlert where bsid = t.id and AlertType = '006' and status <> 'Canceled' order by createTime desc),0) as postMailFlag,
                                    ISNULL((select top 1 (case when status in ('Initialized','Processing') then 1 when status='Finish' then 2 else 0 end ) from T_CA_MailAlert where bsid = t.id and AlertType = '008' and status <> 'Canceled' order by createTime desc),0) as clearMailFlag,
                                    T_SYS_TYPE_DETAIL.DETAIL_NAME as BSTYPENAME,
                                    (case MATCH_STATUS when '0' then '#FF0000' when '1' then '#FF69B4' when '2' then '#B23AEE' when '4' then '#1C86EE' when '7' then '#828282' when '8' then '#8B3A62' when '9' then '#9C9C9C' end) as statuscolor,
                                    (select count(*) from T_CA_CustomerIdentify with (nolock) where BANK_STATEMENT_ID = t.ID and isnull(CUSTOMER_NUM,'')<>'') as countIdentify,
                                    (SELECT SUM(COUNT) AS COUNT FROM (
                                        SELECT COUNT(*) AS COUNT FROM T_CA_ForwarderList WITH(nolock) WHERE FORWARD_NAME=REF1
                                        UNION
                                        SELECT COUNT(*) AS COUNT FROM T_CA_CustomerMapping WITH(nolock) WHERE BankCustomerName=REF1
                                        UNION
                                        SELECT COUNT(*) AS COUNT FROM T_CUSTOMER WITH(nolock) WHERE LOCALIZE_CUSTOMER_NAME=REF1 OR CUSTOMER_NAME=REF1
                                    ) t) as countCustomer
                                FROM
                                    (
                                        SELECT
			                                ROW_NUMBER () OVER (ORDER BY t0.CREATE_DATE,t0.LegalEntity,t0.CURRENCY,t0.TRANSACTION_NUMBER DESC) AS RowNumber,
			                                t0.*,t1.DETAIL_NAME AS MATCH_STATUS_NAME,
			                                (case when ISNULL(
				                                (
					                                SELECT
						                                count(*)
					                                FROM
						                                dbo.T_CA_PMT AS PMT with (nolock)
					                                JOIN dbo.T_CA_PMTBS AS PMTBS with (nolock) ON PMTBS.ReconId = PMT.ID
                                                    JOIN T_CA_PMTDetail as pmtd with (nolock) on pmtd.ReconId = PMT.ID
					                                WHERE
						                                PMT.isClosed = 0
					                                AND PMTBS.BANK_STATEMENT_ID = t0.ID
				                                ),
				                                0
			                                ) > 0 then 2 else
                                            ISNULL(
				                                (
					                                SELECT
						                                count(*)
					                                FROM
						                                dbo.T_CA_PMT AS PMT with (nolock)
					                                JOIN dbo.T_CA_PMTBS AS PMTBS with (nolock) ON PMTBS.ReconId = PMT.ID
					                                WHERE PMTBS.BANK_STATEMENT_ID = t0.ID
				                                ),
				                                0
			                                ) end) AS HASPMTDETAIL,
                                            ISNULL(t2.FILE_COUNT, 0) AS HASFILE,
			                                (SELECT
						                                TOP 1 GroupNo
					                                FROM
						                                dbo.T_CA_PMT AS PMT WITH (nolock)
					                                JOIN dbo.T_CA_PMTBS AS PMTBS WITH (nolock) ON PMTBS.ReconId = PMT.ID
					                                WHERE PMTBS.BANK_STATEMENT_ID = t0.ID
                                                    ORDER BY GroupNo DESC) AS GroupNo,
			                                (SELECT
						                                TOP 1 FILENAME
					                                FROM
						                                dbo.T_CA_PMT AS PMT WITH (nolock)
					                                JOIN dbo.T_CA_PMTBS AS PMTBS WITH (nolock) ON PMTBS.ReconId = PMT.ID
					                                WHERE
						                                PMT.isClosed = 0
					                                AND PMTBS.BANK_STATEMENT_ID = t0.ID
                                                    ORDER BY GroupNo DESC) AS PMTFileName
		                                FROM
			                                T_CA_BankStatement t0 with(nolock)
                                        INNER JOIN T_SYS_TYPE_DETAIL t1 with(nolock) ON t0.MATCH_STATUS = t1.DETAIL_VALUE AND t1.TYPE_CODE = '088'
                                        LEFT JOIN (select count(id) AS FILE_COUNT, BSID from T_CA_BSFile with(nolock) WHERE DEL_FLAG = 0 GROUP BY BSID) t2 ON t0.ID = t2.BSID
                                        WHERE
                                            t0.DEL_FLAG = 0
                                        AND ((CREATE_USER IN ({0})) OR (CHARINDEX('{13}',(SELECT DETAIL_VALUE3 FROM T_SYS_TYPE_DETAIL with(nolock) WHERE TYPE_CODE = '087' AND DETAIL_NAME = t0.LegalEntity )) > 0))
                                        AND t0.MATCH_STATUS in ({3})
                                        AND ((t0.TRANSACTION_NUMBER like '%{4}%') OR '' = '{4}')
                                        AND ((t0.LegalEntity like '%{12}%') OR '' = '{12}')
                                        AND ((t0.CURRENCY like '%{5}%') OR '' = '{5}')
                                        AND ((t0.TRANSACTION_AMOUNT = '{6}') OR '' = '{6}' OR (t0.CURRENT_AMOUNT = '{6}'))
                                        AND ((t0.CUSTOMER_NUM like '%{7}%') OR '' = '{7}' OR (t0.CUSTOMER_NAME like '%{7}%') or '' = '{7}')
                                        AND ((t0.FORWARD_NUM like '%{8}%') OR '' = '{8}' OR (t0.FORWARD_NAME like '%{8}%') or '' = '{8}')
                                        AND (t0.VALUE_DATE >= '{9} 00:00:00' OR '' = '{9}')
                                        AND (t0.VALUE_DATE <= '{10} 23:59:59' OR '' = '{10}')
                                        AND (t0.CREATE_DATE >= '{14} 00:00:00' OR '' = '{14}')
                                        AND (t0.CREATE_DATE <= '{15} 23:59:59' OR '' = '{15}')
                                        AND (t0.ISHISTORY = '{11}' OR '' = '{11}')
                                        AND (t0.BSTYPE = '{16}' OR '' = '{16}')
                                    ) AS t
                                    LEFT JOIN T_SYS_TYPE_DETAIL ON T_SYS_TYPE_DETAIL.TYPE_CODE = '085' AND t.BSTYPE = T_SYS_TYPE_DETAIL.DETAIL_VALUE
                                WHERE
                                    RowNumber BETWEEN {1} AND {2}", collecotrList, page == 1 ? 0 : pageSize * (page - 1) + 1, pageSize * page, statusselect, transNumber.Replace("'","''"), transcurrency.Replace("'", "''"), transamount.Replace("'", "''"), transCustomer.Replace("'", "''"), transaForward.Replace("'", "''"), valueDateF, valueDateT, ishistory, legalEntity.Replace("'", "''"), userId,createDateF, createDateT, bsType);

            List<CaBankStatementDto> dto = CommonRep.ExecuteSqlQuery<CaBankStatementDto>(sql).ToList();

            foreach (CaBankStatementDto item in dto) {
                item.TRANSACTION_NUMBER = HttpUtility.HtmlDecode(item.TRANSACTION_NUMBER);
                item.Description = HttpUtility.HtmlDecode(item.Description);
                item.ReceiptsMethod = HttpUtility.HtmlDecode(item.ReceiptsMethod);
                item.BankAccountNumber = HttpUtility.HtmlDecode(item.BankAccountNumber);
                item.Comments = HttpUtility.HtmlDecode(item.Comments);
            }

            string sql1 = string.Format(@"select count(1) as count FROM
			                                T_CA_BankStatement t0 with(nolock)
                                        INNER JOIN T_SYS_TYPE_DETAIL t1 with(nolock) ON t0.MATCH_STATUS = t1.DETAIL_VALUE AND t1.TYPE_CODE = '088'
                                        LEFT JOIN (select count(id) AS FILE_COUNT, BSID from T_CA_BSFile with(nolock) WHERE DEL_FLAG = 0 GROUP BY BSID) t2 ON t0.ID = t2.BSID
                                        WHERE
                                            DEL_FLAG = 0
                                        AND ((CREATE_USER IN ({0})) OR (CHARINDEX('{13}',(SELECT DETAIL_VALUE3 FROM T_SYS_TYPE_DETAIL WHERE TYPE_CODE = '087' AND DETAIL_NAME = t0.LegalEntity )) > 0))
                                        AND t0.MATCH_STATUS in ({3})
                                        AND ((t0.TRANSACTION_NUMBER like '%{4}%') OR '' = '{4}')
                                        AND ((t0.LegalEntity like '%{12}%') OR '' = '{12}')
                                        AND ((t0.CURRENCY like '%{5}%') OR '' = '{5}')
                                        AND ((t0.TRANSACTION_AMOUNT = '{6}') OR '' = '{6}' OR (t0.CURRENT_AMOUNT = '{6}'))
                                        AND ((t0.CUSTOMER_NUM like '%{7}%') OR '' = '{7}' OR (t0.CUSTOMER_NAME like '%{7}%') or '' = '{7}')
                                        AND ((t0.FORWARD_NUM like '%{8}%') OR '' = '{8}' OR (t0.FORWARD_NAME like '%{8}%') or '' = '{8}')
                                        AND (t0.VALUE_DATE >= '{9} 00:00:00' OR '' = '{9}')
                                        AND (t0.VALUE_DATE <= '{10} 23:59:59' OR '' = '{10}')
                                        AND (t0.CREATE_DATE >= '{14} 00:00:00' OR '' = '{14}')
                                        AND (t0.CREATE_DATE <= '{15} 23:59:59' OR '' = '{15}')
                                        AND (t0.ISHISTORY = '{11}' OR '' = '{11}')
                                        AND (t0.BSTYPE = '{16}' OR '' = '{16}')", collecotrList, "", "", statusselect, transNumber.Replace("'", "''"), transcurrency.Replace("'", "''"), transamount.Replace("'", "''"), transCustomer.Replace("'", "''"), transaForward.Replace("'", "''"), valueDateF, valueDateT, ishistory, legalEntity.Replace("'", "''"), userId, createDateF, createDateT, bsType);

            result.dataRows = dto;
            result.count = SqlHelper.ExcuteScalar<int>(sql1);

            return result;
        }


        public void updateBank(CaBankStatementDto dto)
        {
            SqlParameter[] ps = new SqlParameter[33];
            ps[0] = new SqlParameter("@LegalEntity", dto.LegalEntity ?? (object)DBNull.Value);
            ps[1] = new SqlParameter("@SortId", dto.SortId);
            ps[2] = new SqlParameter("@TRANSACTION_NUMBER", dto.TRANSACTION_NUMBER ?? (object)DBNull.Value);
            ps[3] = new SqlParameter("@TRANSACTION_AMOUNT", dto.TRANSACTION_AMOUNT ?? (object)DBNull.Value);
            ps[4] = new SqlParameter("@TRANSACTION_DATE", dto.TRANSACTION_DATE ?? (object)DBNull.Value);
            ps[5] = new SqlParameter("@VALUE_DATE", dto.VALUE_DATE ?? (object)DBNull.Value);
            ps[6] = new SqlParameter("@CURRENCY", dto.CURRENCY ?? (object)DBNull.Value);
            ps[7] = new SqlParameter("@CURRENT_AMOUNT", dto.CURRENT_AMOUNT ?? (object)DBNull.Value);
            ps[8] = new SqlParameter("@Description", dto.Description ?? (object)DBNull.Value);
            ps[9] = new SqlParameter("@REFERENCE1", dto.REFERENCE1 ?? (object)DBNull.Value);
            ps[10] = new SqlParameter("@ID", dto.ID);
            ps[11] = new SqlParameter("@REFERENCE2", dto.REFERENCE2 ?? (object)DBNull.Value);
            ps[12] = new SqlParameter("@REFERENCE3", dto.REFERENCE3 ?? (object)DBNull.Value);
            ps[13] = new SqlParameter("@FORWARD_NUM", dto.FORWARD_NUM ?? (object)DBNull.Value);
            ps[14] = new SqlParameter("@FORWARD_NAME", dto.FORWARD_NAME ?? (object)DBNull.Value);
            ps[15] = new SqlParameter("@TYPE", dto.TYPE ?? (object)DBNull.Value);
            ps[16] = new SqlParameter("@CUSTOMER_NUM", dto.CUSTOMER_NUM ?? (object)DBNull.Value);
            ps[17] = new SqlParameter("@CUSTOMER_NAME", dto.CUSTOMER_NAME ?? (object)DBNull.Value);
            ps[18] = new SqlParameter("@IsFixedBankCharge", dto.IsFixedBankCharge ?? (object)DBNull.Value);
            ps[19] = new SqlParameter("@BankChargeFrom", dto.BankChargeFrom ?? (object)DBNull.Value);
            ps[20] = new SqlParameter("@BankChargeTo", dto.BankChargeTo ?? (object)DBNull.Value);
            ps[21] = new SqlParameter("@IDENTIFY_TIME", dto.IDENTIFY_TIME ?? (object)DBNull.Value);
            ps[22] = new SqlParameter("@MATCH_STATUS", dto.MATCH_STATUS ?? (object)DBNull.Value);
            ps[23] = new SqlParameter("@APPLY_STATUS", dto.APPLY_STATUS ?? (object)DBNull.Value);
            ps[24] = new SqlParameter("@APPLY_TIME", dto.APPLY_TIME ?? (object)DBNull.Value);
            ps[25] = new SqlParameter("@PMTNUMBER", dto.PMTNUMBER ?? (object)DBNull.Value);
            ps[26] = new SqlParameter("@RECON_TIME", dto.RECON_TIME ?? (object)DBNull.Value);
            ps[27] = new SqlParameter("@CLEARING_STATUS", dto.CLEARING_STATUS ?? (object)DBNull.Value);
            ps[28] = new SqlParameter("@CLEARING_TIME", dto.CLEARING_TIME ?? (object)DBNull.Value);
            ps[29] = new SqlParameter("@ISLOCKED", dto.ISLOCKED ?? (object)DBNull.Value);
            ps[30] = new SqlParameter("@UPDATE_DATE", DateTime.Now);
            ps[31] = new SqlParameter("@Comments", dto.Comments ?? (object)DBNull.Value);
            ps[32] = new SqlParameter("@SiteUseId", dto.SiteUseId ?? (object)DBNull.Value);

            string sql = @"UPDATE T_CA_BankStatement
                        SET LegalEntity = @LegalEntity,
                         SortId = @SortId,
                         TRANSACTION_NUMBER = @TRANSACTION_NUMBER,
                         TRANSACTION_AMOUNT = @TRANSACTION_AMOUNT,
                         TRANSACTION_DATE = @TRANSACTION_DATE,
                         VALUE_DATE = @VALUE_DATE,
                         CURRENCY = @CURRENCY,
                         CURRENT_AMOUNT = @CURRENT_AMOUNT,
                         Description = @Description,
                         REFERENCE1 = @REFERENCE1,
                         REFERENCE2 = @REFERENCE2,
                         REFERENCE3 = @REFERENCE3,
                         FORWARD_NUM = @FORWARD_NUM,
                         FORWARD_NAME = @FORWARD_NAME,
                         TYPE = @TYPE,
                         CUSTOMER_NUM = @CUSTOMER_NUM,
                         CUSTOMER_NAME = @CUSTOMER_NAME,
                         IsFixedBankCharge = @IsFixedBankCharge,
                         BankChargeFrom = @BankChargeFrom,
                         BankChargeTo = @BankChargeTo,
                         IDENTIFY_TIME = @IDENTIFY_TIME,
                         MATCH_STATUS = @MATCH_STATUS,
                         APPLY_STATUS = @APPLY_STATUS,
                         APPLY_TIME = @APPLY_TIME,
                         PMTNUMBER = @PMTNUMBER,
                         RECON_TIME = @RECON_TIME,
                         CLEARING_STATUS = @CLEARING_STATUS,
                         CLEARING_TIME = @CLEARING_TIME,
                         ISLOCKED = @ISLOCKED,
                         UPDATE_DATE = @UPDATE_DATE,
                         Comments = @Comments,
                         SiteUseId = @SiteUseId
                       WHERE
                            ID = @ID";

            //更新
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql, ps);
        }

        public void saveBank(CaBankStatementDto dto)
        {
            CaReconService service = SpringFactory.GetObjectImpl<CaReconService>("CaReconService");
            // 判断bank的状态是否为close
            if ("9".Equals(dto.MATCH_STATUS))
            {
                if (string.IsNullOrEmpty(dto.ID))
                {
                    dto.APPLY_STATUS = "2";
                    dto.APPLY_TIME = AppContext.Current.User.Now;
                    dto.CLEARING_STATUS = "2";
                    dto.CLEARING_TIME = AppContext.Current.User.Now;
                }
                else
                {
                    CaBankStatementDto bank = getBankStatementById(dto.ID);
                    if (!"2".Equals(bank.APPLY_STATUS))
                    {
                        dto.APPLY_STATUS = "2";
                        dto.APPLY_TIME = AppContext.Current.User.Now;
                    }
                    if (!"2".Equals(bank.CLEARING_STATUS))
                    {
                        dto.CLEARING_STATUS = "2";
                        dto.CLEARING_TIME = AppContext.Current.User.Now;
                    }
                }
                dto.CURRENT_AMOUNT = Decimal.Zero;
                dto.unClear_Amount = Decimal.Zero;
                dto.Comments = "Manual Close";
            }

            if ("2".Equals(dto.MATCH_STATUS) || "0".Equals(dto.MATCH_STATUS) || "-1".Equals(dto.MATCH_STATUS))
            {
                if (!string.IsNullOrEmpty(dto.ID))
                {
                    // 若修改为unmatch状态则将现有可用组合解绑
                    string reconId = service.getLastReconIdByBsId(dto.ID);
                    if (!string.IsNullOrEmpty(reconId))
                    {
                        service.unGroupReconGroupByReconId(reconId);
                    }
                }
            }

            StringBuilder sql = new StringBuilder();
            if (String.IsNullOrEmpty(dto.ID))
            {
                sql.Append("INSERT INTO T_CA_BankStatement (");
                sql.Append("ID,LegalEntity,SortId,BSTYPE,");
                sql.Append("MATCH_STATUS,TRANSACTION_NUMBER,");
                sql.Append("TRANSACTION_AMOUNT,CURRENT_AMOUNT,");
                sql.Append("VALUE_DATE,CURRENCY,Description,REF1,");
                if (!string.IsNullOrEmpty(dto.ReceiptsMethod))
                {
                    sql.Append("ReceiptsMethod,");
                }
                if (!string.IsNullOrEmpty(dto.BankAccountNumber))
                {
                    sql.Append("BankAccountNumber,");
                }
                if (dto.BankChargeFrom != null)
                {
                    sql.Append("BankChargeFrom, ");
                }
                if (dto.BankChargeTo != null)
                {
                    sql.Append("BankChargeTo,");
                }
                if (!string.IsNullOrEmpty(dto.APPLY_STATUS))
                {
                    sql.Append("APPLY_STATUS,");
                }
                if (dto.APPLY_TIME != null)
                {
                    sql.Append("APPLY_TIME,");
                }
                if (!string.IsNullOrEmpty(dto.CLEARING_STATUS))
                {
                    sql.Append("CLEARING_STATUS,");
                }
                if (dto.CLEARING_TIME != null)
                {
                    sql.Append("CLEARING_TIME,");
                }
                if (!string.IsNullOrEmpty(dto.Comments))
                {
                    sql.Append("Comments,");
                }
                if (!string.IsNullOrEmpty(dto.PMTDetailFileName))
                {
                    sql.Append("PMTDetailFileName,");
                }
                if (dto.PMTReceiveDate != null)
                {
                    sql.Append("PMTReceiveDate,");
                }
                sql.Append("ISHISTORY,CREATE_USER, CREATE_DATE) ");
                sql.Append("VALUES('"+Guid.NewGuid().ToString()+"',");
                sql.Append("N'" + dto.LegalEntity.Replace("'", "''") + "',1,");
                sql.Append("'" + dto.BSTYPE.Replace("'", "''") + "',");
                sql.Append("'" + dto.MATCH_STATUS.Replace("'", "''") + "',");
                sql.Append("'" + dto.TRANSACTION_NUMBER.Replace("'", "''") + "',");
                sql.Append(dto.TRANSACTION_AMOUNT + ",");
                sql.Append(dto.CURRENT_AMOUNT + ",");
                sql.Append("'" + dto.VALUE_DATE + "',");
                sql.Append("'" + dto.CURRENCY.Replace("'", "''") + "',");
                sql.Append("N'" + dto.Description.Replace("'", "''") + "',");
                sql.Append("N'" + dto.Description.Replace("'", "''") + "',");
                if (!string.IsNullOrEmpty(dto.ReceiptsMethod))
                {
                    sql.Append("'" + dto.ReceiptsMethod.Replace("'", "''") + "',");
                }
                if (!string.IsNullOrEmpty(dto.BankAccountNumber))
                {
                    sql.Append("'" + dto.BankAccountNumber.Replace("'", "''") + "',");
                }               
                if (dto.BankChargeFrom != null) 
                {
                    sql.Append(dto.BankChargeFrom + ",");
                }
                if (dto.BankChargeTo != null)
                {
                    sql.Append(dto.BankChargeTo + ",");
                }
                if (!string.IsNullOrEmpty(dto.APPLY_STATUS))
                {
                    sql.Append("'" + dto.APPLY_STATUS.Replace("'", "''") + "',");
                }
                if (dto.APPLY_TIME != null)
                {
                    sql.Append("'" + dto.APPLY_TIME + "',");
                }
                if (!string.IsNullOrEmpty(dto.CLEARING_STATUS))
                {
                    sql.Append("'" + dto.CLEARING_STATUS.Replace("'", "''") + "',");
                }
                if (dto.CLEARING_TIME != null)
                {
                    sql.Append("'" + dto.CLEARING_TIME + "',");
                }
                if (!string.IsNullOrEmpty(dto.Comments))
                {
                    sql.Append("'" + dto.Comments.Replace("'", "''") + "',");
                }
                if (!string.IsNullOrEmpty(dto.PMTDetailFileName))
                {
                    sql.Append("'" + dto.PMTDetailFileName.Replace("'", "''") + "',");
                }
                if (dto.PMTReceiveDate != null)
                {
                    sql.Append("'" + dto.PMTReceiveDate + "',");
                }
                if (dto.ISHISTORY)
                {
                    sql.Append("1,");
                }
                else
                {
                    sql.Append("0,");
                }
                
                sql.Append("N'" + AppContext.Current.User.EID + "',");
                sql.Append("'" + AppContext.Current.User.Now + "')");
                

                //更新
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql.ToString());
            }
            else
            {
                sql.Append("UPDATE T_CA_BankStatement SET ");
                sql.Append("MATCH_STATUS = '" + (dto.MATCH_STATUS ?? "").Replace("'", "''") + "',");
                sql.Append("LegalEntity = '" + (dto.LegalEntity ?? "").Replace("'", "''") + "',");
                sql.Append("BSTYPE = '" + (dto.BSTYPE ?? "").Replace("'", "''") + "',");
                sql.Append("TRANSACTION_NUMBER = '" + (dto.TRANSACTION_NUMBER ?? "").Replace("'", "''") + "',");
                sql.Append("TRANSACTION_AMOUNT = " + dto.TRANSACTION_AMOUNT + ",");
                sql.Append("CURRENT_AMOUNT = " + dto.CURRENT_AMOUNT + ",");
                sql.Append("VALUE_DATE = '" + dto.VALUE_DATE + "',");
                sql.Append("CURRENCY = '"+ (dto.CURRENCY ?? "").Replace("'", "''") + "',");
                sql.Append("Description = N'"+ (dto.Description ?? "").Replace("'", "''") + "',");
                sql.Append("ReceiptsMethod = '" + (dto.ReceiptsMethod ?? "").Replace("'", "''") + "',");
                sql.Append("BankAccountNumber = '" + (dto.BankAccountNumber ?? "").Replace("'", "''") + "',");

                if (dto.BankChargeFrom != null)
                {
                    sql.Append("BankChargeFrom = " + dto.BankChargeFrom + ",");
                }
                else
                {
                    sql.Append("BankChargeFrom = NULL,");
                }
                if (dto.BankChargeTo != null)
                {
                    sql.Append("BankChargeTo = " + dto.BankChargeTo + ",");
                }
                else 
                {
                    sql.Append("BankChargeTo = NULL,");
                }
                if (!string.IsNullOrEmpty(dto.APPLY_STATUS))
                {
                    sql.Append("APPLY_STATUS = '" + dto.APPLY_STATUS.Replace("'", "''") + "',");
                }
                if (dto.APPLY_TIME != null)
                {
                    sql.Append("APPLY_TIME = '" + dto.APPLY_TIME + "',");
                }
                if (!string.IsNullOrEmpty(dto.CLEARING_STATUS))
                {
                    sql.Append("CLEARING_STATUS = '" + dto.CLEARING_STATUS.Replace("'", "''") + "',");
                }
                if (dto.CLEARING_TIME != null)
                {
                    sql.Append("CLEARING_TIME = '" + dto.CLEARING_TIME + "',");
                }
                if (!string.IsNullOrEmpty(dto.Comments))
                {
                    sql.Append("Comments = '" + dto.Comments.Replace("'", "''") + "',");
                }
                if (!string.IsNullOrEmpty(dto.PMTDetailFileName))
                {
                    sql.Append("PMTDetailFileName = '" + dto.PMTDetailFileName.Replace("'", "''") + "',");
                }
                //if (dto.PMTReceiveDate!=null)
                {
                    sql.Append("PMTReceiveDate = '" + dto.PMTReceiveDate + "',");
                }
                sql.Append("ISHISTORY = " + (dto.ISHISTORY == false ? 0 : 1) + ",");
                sql.Append("UPDATE_DATE = '"+ AppContext.Current.User.Now + "' ");
                sql.Append("WHERE ID = '"+ dto.ID + "'");

                //更新
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql.ToString());
            }
            
        }


        public void deleteBank(string bankId)
        {
            try 
            { 
                //删除Recon组合
                CaReconService caReconService = SpringFactory.GetObjectImpl<CaReconService>("CaReconService");
                string reconId = caReconService.getReconIdByBsId(bankId);
                if (!string.IsNullOrEmpty(reconId))
                {
                    caReconService.deleteReconGroupByReconId(reconId);
                }

                //删除payment中此条bs
                string deletePMTBS = string.Format(@"delete from T_CA_PMTBS 
                                            where BANK_STATEMENT_ID = '{0}'", bankId);

                CommonRep.GetDBContext().Database.ExecuteSqlCommand(deletePMTBS);

                string sql = string.Format(@"update T_CA_BankStatement
                                    set DEL_FLAG = 1,
                                    UPDATE_DATE = '{0}'
                                    where ID = '{1}'",
                                            AppContext.Current.User.Now,
                                            bankId);
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);

                string deleteCaMail = string.Format(@"delete from T_CA_MailAlert 
                                            where bsid = '{0}'", bankId);
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(deleteCaMail);
            }
            catch (Exception ex)
            {
                throw new OTCServiceException("Delete failure.");
            }
        }

        public void identifyCustomer(List<string> unlockBankIds, int type)
        {
            ICaTaskService taskService = SpringFactory.GetObjectImpl<ICaTaskService>("CaTaskService");

            CaTaskMsg msg = new CaTaskMsg();

            DateTime now = DateTime.Now;

            var unlockBankIdStr = "";
            if (unlockBankIds != null && unlockBankIds.Count > 0)
            {

                foreach (var id in unlockBankIds)
                {
                    unlockBankIdStr += "'" + id + "',";
                }
                
                if (unlockBankIdStr.Length > 0)
                {
                    unlockBankIdStr = unlockBankIdStr.Substring(0, unlockBankIdStr.Length - 1);

                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                    {
                        string[] ids = localIdentify(unlockBankIdStr, now);
                        // 创建task
                        msg.taskId = taskService.createTask(type, unlockBankIds.ToArray(), "", "", now);

                        if(type == 3)
                        {
                            if (CaReconService.identifyCustomer(msg) != "success")
                            {
                                // 抛出提示信息
                                throw new OTCServiceException("Identify failed!");
                            }
                        }
                        else
                        {
                            if (CaReconService.autoIdentifyCustomer(msg) != "success")
                            {
                                // 抛出提示信息
                                throw new OTCServiceException("Identify failed!");
                            }
                        }

                        if (ids.Length > 0)
                        {
                            unlockBankIdStr = "";
                            foreach (var id in ids)
                            {
                                unlockBankIdStr += "'" + id + "',";
                            }
                            if (unlockBankIdStr.Length > 0)
                            {
                                unlockBankIdStr = unlockBankIdStr.Substring(0, unlockBankIdStr.Length - 1);
                            }
                        }
                        else
                        {
                            unlockBankIdStr = "''";
                        }
                        

                        // lock bank
                        string sql = string.Format(@"UPDATE T_CA_BankStatement
                                SET ISLOCKED = 1,IDENTIFY_TIME = '{0}'
                                WHERE
                                    ID IN ({1})", now, unlockBankIdStr);

                        CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);

                        scope.Complete();
                    }
                }
            }
            else
            {
                // 抛出提示信息
                throw new OTCServiceException("There is no bank statements unlock!");
            }
        }

        private string[] localIdentify(string unlockBankIdStr, DateTime now)
        {
            List<string> ids = new List<string>();

            List<CaBankStatementDto> dto = getCaBankStatementListByIds(unlockBankIdStr);
            // 获取
            List<CAForwarderListDto> allForwarderLst = getCaForwarderListByRef1("");
            List<CAForwarderListDto> allForwarderNameList = getForwarderListByCustomerName("");
            foreach (CaBankStatementDto bs in dto) {
                List<CAForwarderListDto> forwarderLst = allForwarderLst.FindAll(a => a.ForwardName == bs.REF1 && a.LegalEntity == bs.LegalEntity).ToList<CAForwarderListDto>();
                if (forwarderLst.Count == 0)
                {
                    List<CAForwarderListDto> forwarderNameList = allForwarderNameList.FindAll(a => a.ForwardName == bs.REF1 && a.LegalEntity == bs.LegalEntity).ToList<CAForwarderListDto>();
                    if (forwarderNameList.Count == 0)
                    {
                        if (!identifyByPmtBS(bs.ID, now))
                        {
                            ids.Add(bs.ID);
                        }
                    }
                    else
                    {
                        string sql = string.Format(@"UPDATE T_CA_BankStatement SET FORWARD_NUM = '{0}',FORWARD_NAME = '{1}',MATCH_STATUS='0', ISLOCKED='0',IDENTIFY_TIME = '{3}' WHERE ID = '{2}' and MATCH_STATUS in ('-1','0')",
                                           forwarderNameList[0].ForwardNum, forwarderNameList[0].ForwardName.Replace("'","''"), bs.ID, now);
                        CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);
                        identifyByPmtBS(bs.ID, now, true);
                    }
                }
                else {
                    editBankstatementAndCustomerIdentify(forwarderLst,bs.ID,now);
                    identifyByPmtBS(bs.ID, now, true);
                }
            }
            return ids.ToArray();
        }

        private void editBankstatementAndCustomerIdentify(List<CAForwarderListDto> forwarderLst,string bid, DateTime now)
        {
            string sql = string.Format(@"UPDATE T_CA_BankStatement SET FORWARD_NUM = '{0}',FORWARD_NAME = '{1}', MATCH_STATUS='0', ISLOCKED='0',IDENTIFY_TIME = '{3}' WHERE ID = '{2}' and MATCH_STATUS in ('-1','0')",
                                           forwarderLst[0].ForwardNum, forwarderLst[0].ForwardName,bid, now);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);

            foreach (CAForwarderListDto fw in forwarderLst) {
                if (fw.CustomerNum == "") {
                    continue;
                }
                string id = Guid.NewGuid().ToString();

                string addSql = string.Format(@"INSERT INTO T_CA_CustomerIdentify(ID,BANK_STATEMENT_ID,SortId,FORWARD_NUM,FORWARD_NAME,CUSTOMER_NUM,CUSTOMER_NAME,CREATE_USER,CREATE_DATE) 
                                                VALUES (N'{0}',N'{1}',1,N'{2}',N'{3}',N'{4}',N'{5}',N'{6}',getdate())",
                                               id,bid,fw.ForwardNum,fw.ForwardName,fw.CustomerNum,fw.CustomerName, AppContext.Current.User.EID);
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(addSql);
            }

        }

        private bool identifyByPmtBS(string bid, DateTime now, bool flag = false)
        {
            string sql = string.Format(@"SELECT
	                t0.*,
	                c.CUSTOMER_NAME AS CustomerName
                FROM
	                T_CA_PMT t0 with (nolock)
                INNER JOIN T_CA_PMTBS t1 with (nolock) ON t0.ID = t1.ReconId
                INNER JOIN V_CA_Customer c with (nolock) on t0.CustomerNum = c.CUSTOMER_NUM
                WHERE
	                t1.BANK_STATEMENT_ID = '{0}'", bid);
            List<CaPMTDto> pmtList = CommonRep.ExecuteSqlQuery<CaPMTDto>(sql).ToList();

            if(null != pmtList && pmtList.Count > 0)
            {
                string preCustomerNumber = "";
                foreach(CaPMTDto pmt in pmtList)
                {
                    if (string.IsNullOrEmpty(preCustomerNumber))
                    {
                        preCustomerNumber = pmt.CustomerNum ?? "";
                    }
                    if(!preCustomerNumber.Equals(pmt.CustomerNum ?? ""))
                    {
                        return false;
                    }
                }
                if(!string.IsNullOrEmpty(pmtList[0].CustomerNum) && !string.IsNullOrEmpty(pmtList[0].CustomerName))
                {
                    if (flag)
                    {
                        // 已找到forward
                        string updSql = string.Format(@"UPDATE T_CA_BankStatement SET CUSTOMER_NUM = '{0}',CUSTOMER_NAME = '{1}',MATCH_STATUS='2', ISLOCKED='0',IDENTIFY_TIME = '{3}' WHERE ID = '{2}'",
                                                   pmtList[0].CustomerNum.Replace("'", "''"), pmtList[0].CustomerName.Replace("'","''"), bid, now);
                        CommonRep.GetDBContext().Database.ExecuteSqlCommand(updSql);
                        return true;
                    }
                    else
                    {
                        // 未找到forward
                        string updSql = string.Format(@"UPDATE T_CA_BankStatement SET FORWARD_NUM = '{0}',FORWARD_NAME = '{1}',CUSTOMER_NUM = '{0}',CUSTOMER_NAME = '{1}',MATCH_STATUS='2', ISLOCKED='0',IDENTIFY_TIME = '{3}' WHERE ID = '{2}'",
                                                   pmtList[0].CustomerNum.Replace("'", "''"), pmtList[0].CustomerName.Replace("'", "''"), bid, now);
                        CommonRep.GetDBContext().Database.ExecuteSqlCommand(updSql);
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public List<CAForwarderListDto> getCaForwarderListByRef1(string rEF1)
        {
            string sql = string.Format(@"SELECT
	                                        cf.ID,
	                                        cf.LegalEntity,
	                                        cf.CUSTOMER_NUM AS CustomerNum,
	                                        cc.CUSTOMER_NAME AS CustomerName,
	                                        cf.FORWARD_NUM AS ForwardNum,
	                                        cf.FORWARD_NAME AS ForwardName 
                                        FROM
	                                        T_CA_ForwarderList cf with (nolock)
	                                        INNER JOIN T_SYS_TYPE_DETAIL st with (nolock) ON st.TYPE_CODE = '093' 
	                                        AND st.DETAIL_VALUE = cf.LegalEntity 
	                                        AND st.DETAIL_NAME = cf.FORWARD_NAME
	                                        INNER JOIN V_CA_CustomerOnly cc WITH (nolock) ON cf.CUSTOMER_NUM = cc.CUSTOMER_NUM 
	                                        AND cf.LegalEntity = cc.LegalEntity 
                                        WHERE
	                                        (st.DETAIL_NAME = '{0}' OR '' = '{0}')
                                        ORDER BY
	                                        cf.CUSTOMER_NUM", (rEF1 == null ? "" : rEF1.Replace("'","''")));
            return CommonRep.ExecuteSqlQuery<CAForwarderListDto>(sql).ToList();
        }

        public List<CAForwarderListDto> getForwarderListByCustomerName(string customerName)
        {
            string sql = string.Format(@"SELECT
	                                    cf.ID,
	                                    cf.LegalEntity,
	                                    cf.CUSTOMER_NUM AS CustomerNum,
	                                    cc.CUSTOMER_NAME AS CustomerName,
	                                    cf.FORWARD_NUM AS ForwardNum,
	                                    cf.FORWARD_NAME AS ForwardName
                                    FROM
	                                    T_CA_ForwarderList cf WITH (nolock)
                                    INNER JOIN V_CA_CustomerOnly cc WITH (nolock) ON cf.CUSTOMER_NUM = cc.CUSTOMER_NUM
                                    AND cf.LegalEntity = cc.LegalEntity
                                    WHERE
                                        (cf.FORWARD_NAME = '{0}' OR '' = '{0}')
                                    ORDER BY
	                                    cf.CUSTOMER_NUM", (customerName == null ? "" : customerName.Replace("'", "''")));
            return CommonRep.ExecuteSqlQuery<CAForwarderListDto>(sql).ToList();
        }

        private List<CaBankStatementDto> getCaBankStatementListByIds(string unlockBankIdStr)
        {
            string sql = string.Format(@"SELECT * FROM T_CA_BankStatement with (nolock) WHERE ID IN ({0})", unlockBankIdStr);
            return CommonRep.ExecuteSqlQuery<CaBankStatementDto>(sql).ToList();
        }

        public void unknownCashAdvisor(List<string> unlockBankIds, string taskId)
        {
            ICaTaskService taskService = SpringFactory.GetObjectImpl<ICaTaskService>("CaTaskService");

            DateTime now = DateTime.Now;

            var unlockBankIdStr = "";
            if (unlockBankIds != null && unlockBankIds.Count > 0)
            {

                foreach (var id in unlockBankIds)
                {
                    unlockBankIdStr += "'" + id + "',";
                }
                
                if (unlockBankIdStr.Length > 0)
                {
                    unlockBankIdStr = unlockBankIdStr.Substring(0, unlockBankIdStr.Length - 1);

                    // 更新lock状态

                    CaTaskMsg msg = new CaTaskMsg();

                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                    {

                        if (string.IsNullOrEmpty(taskId))
                        {
                            // 创建task
                            msg.taskId = taskService.createTask(4, unlockBankIds.ToArray(), "", "", now);
                        }
                        else
                        {
                            // 创建task
                            msg.taskId = taskId;
                        }
                        

                        if (CaReconService.unknownCashAdvisor(msg) != "success")
                        {
                            // 抛出提示信息
                            throw new OTCServiceException("Unknown failed!");
                        }

                        // lock bank
                        string sql = string.Format(@"UPDATE T_CA_BankStatement
                                SET ISLOCKED = 1,ADVISOR_TIME = '{0}'
                                WHERE
                                    ID IN ({1})", now, unlockBankIdStr);

                        CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);

                        scope.Complete();
                    }
                }
            }
            else
            {
                // 抛出提示信息
                throw new OTCServiceException("There is no bank statements to unknown!");
            }
        }

        public List<string> filterUnlockBankIds(string[] bankIds)
        {
            var bankIdStr = "";
            foreach (var id in bankIds)
            {
                bankIdStr += "'" + id + "',";
            }
            if (bankIdStr.Length > 0)
            {
                bankIdStr = bankIdStr.Substring(0, bankIdStr.Length - 1);
                // 过滤bank islock=1的数据
                string bankIdSql = string.Format(@"SELECT
                        ID
                    FROM
                        T_CA_BankStatement with(nolock)
                    WHERE
                        (
                            ISLOCKED <> 1
                            OR ISLOCKED IS NULL
                        )
                    AND (
		                MATCH_STATUS = '0' or match_status = '-1'
	                )
                    AND (
	                    FORWARD_NUM IS NULL
	                    OR FORWARD_NUM = ''
                    )
                    AND (
		                ISHISTORY <> 1
	                )
                    AND ID IN({0})", bankIdStr);

                return CommonRep.ExecuteSqlQuery<string>(bankIdSql).ToList();
            }
            else
            {
                // 抛出提示信息
                throw new OTCServiceException("Please at least select one Item");
            }

        }

        public List<string> filterAvailableBankIds(string[] bankIds)
        {
            var bankIdStr = "";
            foreach (var id in bankIds)
            {
                bankIdStr += "'" + id + "',";
            }
            if (bankIdStr.Length > 0)
            {
                bankIdStr = bankIdStr.Substring(0, bankIdStr.Length - 1);
                // 过滤bank islock=1的数据
                string bankIdSql = string.Format(@"SELECT
	                    ID
                    FROM
	                    T_CA_BankStatement with(nolock)
                    WHERE
	                    (
		                    ISLOCKED <> 1
		                    OR ISLOCKED IS NULL
	                    )
                    AND (MATCH_STATUS in ( 0, 2) )
                    AND (ISHISTORY <> 1)
                    AND ID IN({0})", bankIdStr);

                return CommonRep.ExecuteSqlQuery<string>(bankIdSql).ToList();
            }
            else
            {
                // 抛出提示信息
                throw new OTCServiceException("Please at least select one Item");
            }

        }

        public List<string> filterUnKnownBankIds(string[] bankIds)
        {
            var bankIdStr = "";
            foreach (var id in bankIds)
            {
                bankIdStr += "'" + id + "',";
            }
            if (bankIdStr.Length > 0)
            {
                bankIdStr = bankIdStr.Substring(0, bankIdStr.Length - 1);
                // 过滤bank islock=1的数据
                string bankIdSql = string.Format(@"SELECT
	                    ID
                    FROM
	                    T_CA_BankStatement with(nolock)
                    WHERE
	                    (
		                    ISLOCKED <> 1
		                    OR ISLOCKED IS NULL
	                    )
                    AND (MATCH_STATUS = 0)
                    AND (ISHISTORY <> 1)
                    AND (
	                    FORWARD_NUM IS NOT NULL
	                    OR FORWARD_NUM <> ''
                    )
                    AND (
	                    CUSTOMER_NUM IS NULL
	                    OR CUSTOMER_NUM = ''
                    )
					AND(ISNULL(Customer_NUM, '') <> ISNULL(FORWARD_NUM, ''))
                    AND ID IN({0})", bankIdStr);

                return CommonRep.ExecuteSqlQuery<string>(bankIdSql).ToList();
            }
            else
            {
                // 抛出提示信息
                throw new OTCServiceException("Please at least select one Item");
            }

        }

        public List<string> filterUnmatchBankIds(string[] bankIds)
        {
            var bankIdStr = "";
            foreach (var id in bankIds)
            {
                bankIdStr += "'" + id + "',";
            }
            if (bankIdStr.Length > 0)
            {
                bankIdStr = bankIdStr.Substring(0, bankIdStr.Length - 1);
                // 将没有AR的BS数据的Comments写为No AR
                string sql = string.Format(@"UPDATE T_CA_BankStatement
                                SET Comments = 'NO AR'
                                WHERE
                                    ID IN ({0}) 
                                    AND NOT EXISTS (
	                                    select INVOICE_NUM from V_CA_AR with (nolock) where CUSTOMER_NUM = T_CA_BankStatement.CUSTOMER_NUM
                                    )", bankIdStr);

                CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);
                // 过滤bank islock=1的数据
                string bankIdSql = string.Format(@"SELECT
	                                    ID
                                    FROM
	                                    T_CA_BankStatement with(nolock)
                                    WHERE
	                                    (
		                                    ISLOCKED <> 1
		                                    OR ISLOCKED IS NULL
	                                    )
                                    AND MATCH_STATUS IN (2, 3)
                                    AND (
		                                ISHISTORY <> 1
	                                )
                                    AND ID IN (
	                                    {0}
                                    ) AND EXISTS (
	                                    SELECT DISTINCT
		                                    SiteUseId
	                                    FROM
		                                    V_CA_CustomerSiteUseId with (nolock)
	                                    WHERE
		                                    CUSTOMER_NUM = T_CA_BankStatement.CUSTOMER_NUM
	                                    AND LegalEntity = T_CA_BankStatement.LegalEntity
                                    ) AND EXISTS (
	                                    select INVOICE_NUM from V_CA_AR with (nolock) where CUSTOMER_NUM = T_CA_BankStatement.CUSTOMER_NUM
                                    )
                                    ORDER BY
	                                    LegalEntity DESC,
	                                    REGION DESC,
	                                    CUSTOMER_NUM DESC,
	                                    CURRENCY DESC", bankIdStr);

                return CommonRep.ExecuteSqlQuery<string>(bankIdSql).ToList();
            }
            else
            {
                // 抛出提示信息
                throw new OTCServiceException("Please at least select one Item");
            }

        }

        public CaBankStatementDtoPage getUnknownBankStatementList(int page, int pageSize)
        {
            CaBankStatementDtoPage result = new CaBankStatementDtoPage();

            string sql = string.Format(@"SELECT
                                    *
                                FROM
                                    (
                                        SELECT
                                            ROW_NUMBER () OVER (ORDER BY t0.CREATE_DATE DESC) AS RowNumber,
                                            t0.*, t1.DETAIL_NAME AS MENUREGION_NAME
                                        FROM
                                            T_CA_BankStatement t0 with(nolock)
                                        LEFT JOIN T_SYS_TYPE_DETAIL t1 with(nolock) ON t1.DETAIL_VALUE = t0.MENUREGION
                                        AND t1.TYPE_CODE = '080'
                                        WHERE
                                            DEL_FLAG = 0
                                        AND CREATE_USER = '{0}'
                                    ) AS t
                                WHERE
                                    RowNumber BETWEEN {1} AND {2}", AppContext.Current.User.EID, page == 1 ? 0 : pageSize * (page - 1) + 1, pageSize * page);

            List<CaBankStatementDto> dto = CommonRep.ExecuteSqlQuery<CaBankStatementDto>(sql).ToList();

            string sql1 = string.Format(@"select count(1) as count from T_CA_BankStatement with(nolock) WHERE
                                            DEL_FLAG = 0
                                        AND CREATE_USER = '{0}'", AppContext.Current.User.EID);

            result.dataRows = dto;
            result.count = SqlHelper.ExcuteScalar<int>(sql1);

            return result;
        }

        public CaBankStatementDto getBankStatementById(string id)
        {
            string sql = string.Format(@"SELECT
	                                        t0.*
                                        FROM
	                                        T_CA_BankStatement t0 with(nolock)
                                        WHERE
                                            t0.ID = '{0}'", id);

            List<CaBankStatementDto> list = CommonRep.ExecuteSqlQuery<CaBankStatementDto>(sql).ToList();
            if (null != list && list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return new CaBankStatementDto();
            }
        }

        public CaBankStatementDto getBankStatementByIdAndUserAuthor(string id)
        {
            string collecotrList = "";
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");     //注入服务
            var userId = AppContext.Current.User.EID; //当前用户ID
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");       //获得用户List(它这里用逗号间隔拼了一下)
            if (!string.IsNullOrEmpty(collecotrList))
            {
                collecotrList = collecotrList.Substring(0, collecotrList.LastIndexOf(","));
                collecotrList = collecotrList.Replace(",", "','");
            }
            collecotrList = "'" + collecotrList + "'";
            string sql = string.Format(@"SELECT
	                                        t0.*
                                        FROM
	                                        T_CA_BankStatement t0 with(nolock)
                                        WHERE
                                            t0.ID = '{0}'
                                        AND ((t0.CREATE_USER IN ({1})) OR (CHARINDEX('{2}',(SELECT DETAIL_VALUE3 FROM T_SYS_TYPE_DETAIL with(nolock) WHERE TYPE_CODE = '087' AND DETAIL_NAME = t0.LegalEntity )) > 0))
                                        ", id, collecotrList, userId);

            List<CaBankStatementDto> list = CommonRep.ExecuteSqlQuery<CaBankStatementDto>(sql).ToList();
            if (null != list && list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return new CaBankStatementDto();
            }
        }

        public CaPmtDtoPage getCaPmtDetailList(string groupNo,string legalEntity, string customerNum,string currency,string amount, string transactionNumber, string invoiceNum, string valueDateF, string valueDateT, string createDateF, string createDateT, string isClosed, string hasBS, string hasInv, string hasMatched, int page, int pageSize) {
            CaPmtDtoPage pageDto = new CaPmtDtoPage();

            if (string.IsNullOrEmpty(groupNo) || groupNo == "undefined")
            {
                groupNo = "";
            }
            if (string.IsNullOrEmpty(legalEntity) || legalEntity == "undefined")
            {
                legalEntity = "";
            }
            if (string.IsNullOrEmpty(amount) || amount == "undefined" || amount == "null")
            {
                amount = "";
            }
            if (string.IsNullOrEmpty(customerNum) || customerNum == "undefined")
            {
                customerNum = "";
            }
            if (string.IsNullOrEmpty(currency) || currency == "undefined")
            {
                currency = "";
            }
            if (string.IsNullOrEmpty(transactionNumber) || transactionNumber == "undefined")
            {
                transactionNumber = "";
            }
            if (string.IsNullOrEmpty(invoiceNum) || invoiceNum == "undefined")
            {
                invoiceNum = "";
            }
            if (string.IsNullOrEmpty(valueDateF) || valueDateF == "undefined")
            {
                valueDateF = "";
            }
            if (string.IsNullOrEmpty(valueDateT) || valueDateT == "undefined")
            {
                valueDateT = "";
            }
            if (string.IsNullOrEmpty(createDateF) || createDateF == "undefined")
            {
                createDateF = "";
            }
            if (string.IsNullOrEmpty(createDateT) || createDateT == "undefined")
            {
                createDateT = "";
            } 

            StringBuilder sbPmt = new StringBuilder();
            StringBuilder sbPmtselect = new StringBuilder();
            StringBuilder sbPmtfromwhere = new StringBuilder();
            StringBuilder sbPmtCountselect = new StringBuilder();
            sbPmtselect.Append(" SELECT DISTINCT T_CA_PMT.id, T_CA_PMT.GroupNo, T_CA_PMT.CustomerNum, cac.CUSTOMER_NAME as CustomerName,T_CA_PMT.Currency, T_CA_PMT.Amount, ");
            sbPmtselect.Append(" T_CA_PMT.TransactionAmount, T_CA_PMT.LegalEntity, T_CA_PMT.CREATE_USER, T_CA_PMT.CREATE_DATE, T_CA_PMT.isClosed, ");
            sbPmtselect.Append(" CASE WHEN PMTBS_COUNT.bsnum > 0 THEN 1 ELSE 0 END AS hasbs, T_CA_PMT.ValueDate,T_CA_PMT.ReceiveDate,");
            sbPmtselect.Append(" CASE WHEN PMTINV_COUNT.invnum > 0 THEN 1 ELSE 0 END AS hasinv, ");
            sbPmtselect.Append("(SELECT COUNT(*) FROM T_CA_Recon with (nolock) WHERE PMT_ID = T_CA_PMT.id ) AS hasMatched");
            sbPmtselect.Append(" ,isNull(T_CA_PMT.filename,'') AS filename");
            sbPmtCountselect.Append("select count(distinct T_CA_PMT.id) ");
            sbPmtfromwhere.Append(" FROM T_CA_PMT with (nolock) ");
            sbPmtfromwhere.Append(" LEFT JOIN  (select distinct CUSTOMER_NUM, CUSTOMER_NAME from V_CA_Customer with (nolock)) as cac ON cac.CUSTOMER_NUM = T_CA_PMT.CustomerNum ");
            sbPmtfromwhere.Append(" LEFT JOIN  T_CA_PMTBS with (nolock) ON  T_CA_PMT.id =T_CA_PMTBS.ReconId ");
            sbPmtfromwhere.Append(" LEFT JOIN  T_CA_BankStatement with (nolock) ON  T_CA_PMTBS.BANK_STATEMENT_ID = T_CA_BankStatement.ID  ");
            sbPmtfromwhere.Append(" LEFT JOIN  T_CA_PMTDetail with (nolock) ON  T_CA_PMT.id =T_CA_PMTDetail.ReconId ");
            sbPmtfromwhere.Append(" LEFT JOIN (SELECT COUNT(*) AS bsnum, ReconId FROM T_CA_PMTBS with (nolock) GROUP BY ReconId) AS PMTBS_COUNT ");
            sbPmtfromwhere.Append(" ON T_CA_PMT.id = PMTBS_COUNT.ReconId ");
            sbPmtfromwhere.Append(" LEFT JOIN (SELECT COUNT(*) AS invnum, ReconId FROM T_CA_PMTDetail with (nolock) GROUP BY ReconId) AS PMTINV_COUNT ");
            sbPmtfromwhere.Append(" ON T_CA_PMT.id = PMTINV_COUNT.ReconId ");
            sbPmtfromwhere.Append(" WHERE ((T_CA_PMT.GroupNo like '%" + groupNo + "%') OR '' = '"+ groupNo + "' )");
            sbPmtfromwhere.Append(" AND((T_CA_PMT.LegalEntity like '%" + legalEntity + "%') OR '' = '"+ legalEntity + "' )");
            sbPmtfromwhere.Append(" AND((T_CA_PMT.CustomerNum like '%" + customerNum + "%') OR '' = '"+ customerNum + "' )");
            sbPmtfromwhere.Append(" AND((T_CA_PMT.Currency like '%" + currency + "%') OR '' = '"+ currency + "' )");
            sbPmtfromwhere.Append(" AND((T_CA_PMT.Amount = '" + amount + "' OR T_CA_PMT.TransactionAmount = '" + amount + "') OR '' = '" + amount + "' )");
            sbPmtfromwhere.Append(" AND((T_CA_BankStatement.TRANSACTION_NUMBER like '%" + transactionNumber + "%') OR '' = '" + transactionNumber + "' )");
            sbPmtfromwhere.Append(" AND(T_CA_PMT.ValueDate >= '" + valueDateF + " 00:00:00' OR '' = '" + valueDateF + "' )");
            sbPmtfromwhere.Append(" AND(T_CA_PMT.ValueDate <= '" + valueDateT + " 23:59:59' OR '' = '" + valueDateT + "' )");
            sbPmtfromwhere.Append(" AND(T_CA_PMT.CREATE_DATE >= '" + createDateF + " 00:00:00' OR '' = '" + createDateF + "' )");
            sbPmtfromwhere.Append(" AND(T_CA_PMT.CREATE_DATE <= '" + createDateT + " 23:59:59' OR '' = '" + createDateT + "' )");
            sbPmtfromwhere.Append(" AND((T_CA_PMTDetail.invoiceNum like '%" + invoiceNum + "%') OR '' = '" + invoiceNum + "' )");
            //增加BS是否已关闭的判断
            if (!string.IsNullOrEmpty(isClosed) && string.Equals(isClosed, "1"))
            {
                sbPmtfromwhere.Append(" AND T_CA_PMT.isClosed = 1 ");
            }

            //增加是否关联BS的判断
            if (!string.IsNullOrEmpty(hasBS))
            {
                if (string.Equals(hasBS,"0"))
                {
                    sbPmtfromwhere.Append(" AND PMTBS_COUNT.bsnum IS NULL ");
                }
                else if(string.Equals(hasBS, "1"))
                {
                    sbPmtfromwhere.Append(" AND PMTBS_COUNT.bsnum > 0 ");
                }
            }

            //增加是否Matched的判断
            if (!string.IsNullOrEmpty(hasMatched))
            {
                if (string.Equals(hasMatched, "0"))
                {
                    sbPmtfromwhere.Append(" AND (SELECT COUNT(*) FROM T_CA_Recon with (nolock) WHERE PMT_ID = T_CA_PMT.id) <= 0 ");
                }
                else if(string.Equals(hasMatched, "1"))
                {
                    sbPmtfromwhere.Append(" AND (SELECT COUNT(*) FROM T_CA_Recon with (nolock) WHERE PMT_ID = T_CA_PMT.id) > 0 ");
                }
            }

            //增加是否关联Invoice的判断
            if (!string.IsNullOrEmpty(hasInv))
            {
                if (string.Equals(hasInv, "0"))
                {
                    sbPmtfromwhere.Append(" AND PMTINV_COUNT.invnum IS NULL ");
                }
                else if (string.Equals(hasInv, "1"))
                {
                    sbPmtfromwhere.Append(" AND PMTINV_COUNT.invnum > 0 ");
                }
            }

            //sbPmtfromwhere.Append(" Order by T_CA_PMT.CREATE_DATE desc ");

            sbPmt.Append(string.Format(@"SELECT t1.* from (select ROW_NUMBER () OVER (ORDER BY t0.CREATE_DATE desc,t0.LegalEntity,t0.CURRENCY,t0.GroupNo DESC) AS RowNumber,
                                                t0.* from (
                                            " + sbPmtselect.ToString() + sbPmtfromwhere.ToString() + ") as t0) as t1 where t1.RowNumber BETWEEN {0} AND {1}", page == 1 ? 0 : pageSize * (page - 1) + 1, pageSize * page));


            List <CaPMTDto> dto = CommonRep.ExecuteSqlQuery<CaPMTDto>(sbPmt.ToString()).ToList();
            if (dto.Count > 0)
            {
                dto[0].PmtBs = getCaPmtBsListById(dto[0].ID);
                dto[0].PmtDetail = GetCaPMTDetailListById(dto[0].ID);
            }
            pageDto.pmt = dto;


            string sql1 = sbPmtCountselect.ToString() + sbPmtfromwhere.ToString();

            pageDto.count = SqlHelper.ExcuteScalar<int>(sql1);
            return pageDto;
        }

        public CaPmtDtoPage getCaPmtDetailListByBsId(string bsId)
        {
            CaPmtDtoPage pageDto = new CaPmtDtoPage();

            string sql = string.Format(@" SELECT DISTINCT T_CA_PMT.id, T_CA_PMT.GroupNo, T_CA_PMT.CustomerNum, cac.CUSTOMER_NAME as CustomerName,T_CA_PMT.Currency, T_CA_PMT.Amount, 
                 T_CA_PMT.TransactionAmount, T_CA_PMT.LegalEntity, T_CA_PMT.CREATE_USER, T_CA_PMT.CREATE_DATE, T_CA_PMT.isClosed, 
                 CASE WHEN PMTBS_COUNT.bsnum > 0 THEN 1 ELSE 0 END AS hasbs, T_CA_PMT.ValueDate,T_CA_PMT.ReceiveDate,
                 CASE WHEN PMTINV_COUNT.invnum > 0 THEN 1 ELSE 0 END AS hasinv, 
                (SELECT COUNT(*) FROM T_CA_Recon with (nolock) WHERE PMT_ID = T_CA_PMT.id ) AS hasMatched
                 FROM T_CA_PMT with (nolock) 
                 LEFT JOIN  (select distinct CUSTOMER_NUM, CUSTOMER_NAME from V_CA_Customer with (nolock)) as cac ON cac.CUSTOMER_NUM = T_CA_PMT.CustomerNum 
                 LEFT JOIN (SELECT COUNT(*) AS bsnum, ReconId FROM T_CA_PMTBS with (nolock) GROUP BY ReconId) AS PMTBS_COUNT 
                 ON T_CA_PMT.id = PMTBS_COUNT.ReconId 
                 LEFT JOIN (SELECT COUNT(*) AS invnum, ReconId FROM T_CA_PMTDetail with (nolock) GROUP BY ReconId) AS PMTINV_COUNT 
                 ON T_CA_PMT.id = PMTINV_COUNT.ReconId 
                 INNER JOIN T_CA_BankStatement with(nolock) ON T_CA_BankStatement.ID='{0}' AND T_CA_PMT.LegalEntity= T_CA_BankStatement.LegalEntity AND T_CA_PMT.Currency = T_CA_BankStatement.CURRENCY AND T_CA_PMT.ValueDate = T_CA_BankStatement.VALUE_DATE AND (T_CA_BankStatement.CURRENT_AMOUNT + ISNULL(T_CA_BankStatement.BankChargeTo, 0)) >= T_CA_PMT.Amount
                WHERE T_CA_PMT.isClosed=0 AND NOT EXISTS(SELECT 1 FROM T_CA_Recon with(nolock) WHERE PMT_ID = T_CA_PMT.id)                 
                Order by T_CA_PMT.CREATE_DATE desc", bsId);

            List<CaPMTDto> dto = CommonRep.ExecuteSqlQuery<CaPMTDto>(sql).ToList();
            if (dto.Count > 0)
            {
                dto[0].PmtBs = getCaPmtBsListById(dto[0].ID);
                dto[0].PmtDetail = GetCaPMTDetailListById(dto[0].ID);
            }
            pageDto.pmt = dto;
            pageDto.count = dto.Count();
            return pageDto;
        }

        public List<CaPMTBSDto> getCaPmtBsListById(string pmtid) {
            StringBuilder sbPmt = new StringBuilder();
            sbPmt.Append(" SELECT T_CA_BankStatement.TRANSACTION_NUMBER as TransactionNumber,T_CA_BankStatement.VALUE_DATE as ValueDate, T_CA_PMTBS.Currency,T_CA_PMTBS.Amount, T_CA_BankStatement.Description, T_CA_BankStatement.REF1 ");
            sbPmt.Append("  FROM dbo.T_CA_PMTBS with (nolock) JOIN dbo.T_CA_BankStatement with (nolock) ON T_CA_PMTBS.BANK_STATEMENT_ID = T_CA_BankStatement.ID ");
            sbPmt.Append("  WHERE ReconId = '" + pmtid + "'");
            List<CaPMTBSDto> pmtBsList = CommonRep.ExecuteSqlQuery<CaPMTBSDto>(sbPmt.ToString()).ToList();
            return pmtBsList;
        }

        public List<CaPMTDetailDto> GetCaPMTDetailListById(string pmtid) {
            StringBuilder sbPmt = new StringBuilder();
            sbPmt.Append(" SELECT T_CA_PMTDetail.SiteUseId, T_CA_PMTDetail.InvoiceNum,T_INVOICE_AGING.INVOICE_DATE AS InvoiceDate,T_INVOICE_AGING.DUE_DATE AS DueDate, ");
            sbPmt.Append(" CASE ISNULL(V_CA_AR_CM.INVOICE_NUM, '') WHEN '' THEN 1 ELSE 0 END AS invIsClosed, ");
            sbPmt.Append(" T_CA_PMTDetail.Currency,T_CA_PMTDetail.Amount FROM T_CA_PMTDetail with (nolock) ");
            sbPmt.Append(" LEFT JOIN T_INVOICE_AGING with (nolock) ON T_CA_PMTDetail.SiteUseId = T_INVOICE_AGING.SiteUseId AND T_CA_PMTDetail.InvoiceNum = T_INVOICE_AGING.INVOICE_NUM ");
            sbPmt.Append(" LEFT JOIN V_CA_AR_CM WITH (nolock) ON V_CA_AR_CM.INVOICE_NUM = T_CA_PMTDetail.InvoiceNum ");
            sbPmt.Append(" WHERE T_CA_PMTDetail.ReconId = '" + pmtid + "'");
            List<CaPMTDetailDto> pmtDetailList = CommonRep.ExecuteSqlQuery<CaPMTDetailDto>(sbPmt.ToString()).ToList();
            return pmtDetailList;
        }

        public void recon(List<string> unlockBankIds,string taskId,string userId)
        {
            ICaTaskService taskService = SpringFactory.GetObjectImpl<ICaTaskService>("CaTaskService");
            ICaPaymentDetailService paymentDetailService = SpringFactory.GetObjectImpl<ICaPaymentDetailService>("CaPaymentDetailService");
            CaReconService reconService = SpringFactory.GetObjectImpl<CaReconService>("CaReconService");
            CaCommonService commonService = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");

            DateTime now = DateTime.Now;
            var unlockBankIdStr = "";
            if (unlockBankIds != null && unlockBankIds.Count > 0)
            {
                foreach (var id in unlockBankIds)
                {
                    unlockBankIdStr += "'" + id + "',";
                }

                if (unlockBankIdStr.Length > 0)
                {
                    unlockBankIdStr = unlockBankIdStr.Substring(0, unlockBankIdStr.Length - 1);

                    //获得所有CA客户，避免多次连接DB
                    List<CaAllCustomerDto> customerAll = GetCaAllCustomersByBSids(unlockBankIdStr);
                    //获得所有BS客户相关的PTP
                    List<CaReconMsgDetailAllDto> allPTPList = getPTPByBSids(unlockBankIdStr);
                    //获得所有BS客户相关的AR
                    List<CaReconMsgDetailAllDto> allARList = getARByBSids(unlockBankIdStr);
                    List<CaBankStatementDto> allBank = getAllBankStatement(unlockBankIdStr);
                    List<SysTypeDetail> systype095 = getAllTypeDetail095();
                    List<CaPMTBSDto> allPMT = getAllPMTByBsId(unlockBankIdStr);
                    List<CaPMTDto> allNoBsPmt = getAllNoBSPMT();

                    CaTaskMsg msg = new CaTaskMsg();
                    int sortId = 0;

                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                    {

                        // 创建task
                        if(string.IsNullOrEmpty(taskId))
                        {
                            msg.taskId = taskService.createTask(5, unlockBankIds.ToArray(), "", "", now);
                        }
                        else
                        {
                            msg.taskId = taskId;
                        }


                        // lock bank
                        string sql = string.Format(@"UPDATE T_CA_BankStatement
                                SET ISLOCKED = 1,RECON_TIME = '{0}'
                                WHERE
                                    ID IN ({1})", now, unlockBankIdStr);

                        CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);

                        // 用于判断是否有需要正常recon的数据
                        int flagCount = 0;

                        var jumpBankMap = new Dictionary<string, List<string>>();
                        try
                        {
                            // 循环遍历BS
                            for (int i = unlockBankIds.Count - 1; i >= 0; i--)
                            {
                                try
                                {
                                    var id = unlockBankIds[i];
                                    // 获取bank
                                    CaBankStatementDto bank = allBank.Where(O => O.ID == id).FirstOrDefault();

                                    string customerNum = bank.CUSTOMER_NUM == null ? "" : bank.CUSTOMER_NUM;
                                    string forwardNum = bank.FORWARD_NUM == null ? "" : bank.FORWARD_NUM;
                                    string forwardName = bank.FORWARD_NAME == null ? "" : bank.FORWARD_NAME;
                                    string customerNameInBank = bank.REF1 == null ? "" : bank.REF1;
                                    string legalEntity = bank.LegalEntity == null ? "" : bank.LegalEntity;

                                    // 判断是否需要清除bankCharge
                                    SysTypeDetail curSys = systype095.Where(o => o.DetailName == customerNameInBank && o.DetailValue == legalEntity).FirstOrDefault();

                                    if (curSys!= null && !string.IsNullOrEmpty(curSys.TypeCode))
                                    {
                                        bank.BankChargeFrom = 0;
                                        bank.BankChargeTo = 0;
                                        updateBank(bank);
                                    }
                                    
                                    // 判断是否在PMT中存在
                                    //string pmtId = paymentDetailService.getPMTIdByBsId(id);
                                    string pmtId = allPMT.Where(o=>o.BANK_STATEMENT_ID == id).Select(o=>o.ReconId).FirstOrDefault();
                                    // 判断PMT是否可用，校验组合中包含AR的状态
                                    
                                    if (null != pmtId && pmtId.Length > 0)
                                    {
                                        // 若存在则解绑原有组合并生成新的recon group
                                        string reconId = reconService.getLastReconIdByBsId(id);
                                        if (paymentDetailService.checkPMTAvailable(pmtId))
                                        {
                                            reconService.deleteReconGroupByReconId(reconId);
                                            // 生成group
                                            reconService.createReconGroupByBSId(id, msg.taskId, pmtId);
                                            // 修改状态为matched并解锁
                                            bank.MATCH_STATUS = "4";
                                            bank.ISLOCKED = false;
                                            bank.Comments = "Base on PMT";
                                            updateBank(bank);
                                        }
                                        else
                                        {
                                            CaPMTDto pmt = paymentDetailService.getPMTById(pmtId);
                                            // 修改状态为matched并解锁
                                            if (null != bank.FORWARD_NUM && null != bank.CUSTOMER_NUM)
                                            {
                                                bank.MATCH_STATUS = "2";
                                            }
                                            else
                                            {
                                                //应该为unknow，状态不变
                                            }
                                            bank.ISLOCKED = false;
                                            bank.Comments = "AR in PMT is out of date, PMT group No: " + pmt.GroupNo;
                                            updateBank(bank);
                                        }
                                    }
                                    else
                                    {
                                        // 查找customer
                                        string currency = bank.CURRENCY;
                                        
                                        // 根据legalEntity和customer no查找对应attr信息
                                        CaCustomerAttributeDto cuAttr = getCustomerAttrByCustomerNum(customerNum, legalEntity);
                                        bool isJumpBankStatement = cuAttr.IsJumpBankStatement ?? false;
                                        bool isJumpSiteUseId = cuAttr.IsJumpSiteUseId ?? false;
                                        string funcCurrency = cuAttr.Func_Currency;
                                        bool isFactoring = cuAttr.IsFactoring ?? false;

                                        decimal bankChargeFrom = bank.BankChargeFrom ?? Decimal.Zero;
                                        decimal bankChargeTo = bank.BankChargeTo ?? Decimal.Zero;

                                        string flag = "1";
                                        if (!currency.Equals(funcCurrency))
                                        {
                                            flag = "0";
                                        }
                                        
                                        bool pmtFlag = false;

                                        // 若不存在则尝试与PMT进行match
                                        // 查找PMT
                                        //List<CaPMTDto> pmtList = getNoBSPMT(legalEntity, customerNum, currency, bank.TRANSACTION_AMOUNT, bank.VALUE_DATE);
                                        List<CaPMTDto> pmtList = allNoBsPmt.Where(o=>o.LegalEntity == legalEntity && o.CustomerNum == customerNum && o.Currency == currency && o.TransactionAmount == bank.TRANSACTION_AMOUNT && o.ValueDate == bank.VALUE_DATE).ToList();
                                        foreach (var pmt in pmtList)
                                        {
                                            // 根据bank的currency判断pmt的amount是否在charge范围内
                                            if (
                                                    (
                                                        (pmt.Currency ?? "").Equals(bank.CURRENCY) &&
                                                        Decimal.Compare(pmt.Amount ?? Decimal.Zero, Decimal.Add(bank.CURRENT_AMOUNT ?? Decimal.Zero, bank.BankChargeFrom ?? Decimal.Zero)) > -1 &&
                                                        Decimal.Compare(pmt.Amount ?? Decimal.Zero, Decimal.Add(bank.CURRENT_AMOUNT ?? Decimal.Zero, bank.BankChargeTo ?? Decimal.Zero)) < 1
                                                    ) || (
                                                        (pmt.LocalCurrency ?? "").Equals(bank.CURRENCY) &&
                                                        Decimal.Compare(pmt.LocalCurrencyAmount ?? Decimal.Zero, Decimal.Add(bank.CURRENT_AMOUNT ?? Decimal.Zero, bank.BankChargeFrom ?? Decimal.Zero)) > -1 &&
                                                        Decimal.Compare(pmt.LocalCurrencyAmount ?? Decimal.Zero, Decimal.Add(bank.CURRENT_AMOUNT ?? Decimal.Zero, bank.BankChargeTo ?? Decimal.Zero)) < 1
                                                    )
                                                )
                                            {
                                                
                                                // 在范围内则将BS添加到PMT中
                                                paymentDetailService.savePMTBSByBank(bank, pmt.ID);
                                                // 按BS组成Recon
                                                //pmtId = paymentDetailService.getPMTIdByBsId(id);
                                                pmtId = allPMT.Where(o => o.BANK_STATEMENT_ID == id).Select(o => o.ReconId).FirstOrDefault();
                                                if (null != pmtId && pmtId.Length > 0 && paymentDetailService.checkPMTAvailable(pmtId))
                                                {
                                                    // 若存在则解绑原有组合并生成新的recon group
                                                    string reconId = reconService.getLastReconIdByBsId(id);
                                                    if (paymentDetailService.checkPMTAvailable(pmtId))
                                                    {
                                                        reconService.deleteReconGroupByReconId(reconId);
                                                        // 生成group
                                                        reconService.createReconGroupByBSId(id, msg.taskId, pmtId);
                                                        // 修改状态为matched并解锁
                                                        bank.MATCH_STATUS = "4";
                                                        bank.ISLOCKED = false;
                                                        bank.Comments = "Base on PMT";
                                                        updateBank(bank);

                                                        pmtFlag = true;
                                                        // 跳出本次循环
                                                        break;
                                                    }
                                                }
                                                
                                            }
                                        }

                                        // 若成功找到PMT组合则跳出本次循环
                                        if (pmtFlag)
                                        {
                                            bank.ISLOCKED = false;
                                            updateBank(bank);
                                            continue;
                                        }
                                        
                                        //Factoring客户不参加Recon，只通过PMT Detail形成组合
                                        if (isFactoring)
                                        {
                                            bank.ISLOCKED = false;
                                            updateBank(bank);
                                            continue;
                                        }
                                        
                                        flagCount++;

                                        // 若不存在则正常计算
                                        if (isJumpBankStatement)
                                        {
                                            if (!jumpBankMap.ContainsKey(customerNum + legalEntity + currency))
                                            {
                                                jumpBankMap.Add(customerNum + legalEntity + currency, new List<string>());
                                            }
                                            List<string> idList = jumpBankMap[customerNum + legalEntity + currency];
                                            idList.Add(id);
                                            jumpBankMap[customerNum + legalEntity + currency] = idList;
                                        }
                                        
                                        // 根据customer查找site
                                        //List<string> siteUseIds = getSiteByCustomerNum(customerNum, legalEntity);
                                        List<string> siteUseIds = customerAll.Where(o => o.LegalEntity == legalEntity && o.CustomerNum == customerNum).OrderBy(o => o.SiteUseId).Select(o => o.SiteUseId).ToList();
                                        
                                        if (null != siteUseIds && siteUseIds.Count > 0)
                                        {
                                            List<CaReconMsgDetailDto> jumpSite_ptpList = new List<CaReconMsgDetailDto>();
                                            List<CaReconMsgDetailDto> jumpSite_arList = new List<CaReconMsgDetailDto>();
                                            foreach (var siteUseId in siteUseIds)
                                            {
                                                CaReconMsgDto reconMsgDto = new CaReconMsgDto();
                                                // 根据site查找PTP
                                                //List<CaReconMsgDetailDto> ptpList = getPTPBySiteUseId(legalEntity, customerNum, siteUseId, flag);
                                                List<CaReconMsgDetailDto> ptpList = allPTPList.Where(o => o.LegalEntity == legalEntity && o.CUSTOMER_NUM == customerNum && o.SiteUseId == siteUseId)
                                                                                              .Select(p => new CaReconMsgDetailDto
                                                                                              {
                                                                                                  ID = p.ID,
                                                                                                  DUE_DATE = p.DUE_DATE,
                                                                                                  AMT = (flag == "1" ? p.AMT : p.Local_AMT)
                                                                                              }).OrderBy(o => o.DUE_DATE).ToList();
                                                // 根据site查找AR
                                                //List<CaReconMsgDetailDto> arList = getARBySiteUseId(legalEntity, customerNum, siteUseId, flag);
                                                List<CaReconMsgDetailDto> arList = allARList.Where(o => o.LegalEntity == legalEntity && o.CUSTOMER_NUM == customerNum && o.SiteUseId == siteUseId)
                                                                                              .Select(p => new CaReconMsgDetailDto
                                                                                              {
                                                                                                  ID = p.ID,
                                                                                                  DUE_DATE = p.DUE_DATE,
                                                                                                  AMT = (flag == "1" ? p.AMT : p.Local_AMT)
                                                                                              }).OrderBy(o => o.DUE_DATE).ToList();

                                                // 添加数据
                                                reconMsgDto.taskId = msg.taskId;
                                                reconMsgDto.REGION = bank.REGION;
                                                reconMsgDto.FUNC_CURRENCY = funcCurrency;
                                                reconMsgDto.BANK_CURRENCY = currency;
                                                reconMsgDto.Total_AMT = bank.CURRENT_AMOUNT;
                                                reconMsgDto.BankChargeFrom = bank.BankChargeFrom ?? Decimal.Zero;
                                                reconMsgDto.BankChargeTo = bank.BankChargeTo ?? Decimal.Zero;
                                                reconMsgDto.isJumpBankStatement = isJumpBankStatement;
                                                reconMsgDto.isJumpSiteUseId = isJumpSiteUseId;

                                                List<CaBankStatementDto> bankList = new List<CaBankStatementDto>();
                                                bankList.Add(bank);
                                                reconMsgDto.bankList = bankList;
                                                reconMsgDto.ptpList = ptpList;
                                                reconMsgDto.arList = arList;
                                                // 添加到数据库中
                                                insertReconTask(reconMsgDto.taskId, JsonConvert.SerializeObject(reconMsgDto), sortId++, userId);
                                                // 判断是否跨site
                                                if (isJumpSiteUseId)
                                                {
                                                    jumpSite_ptpList.AddRange(ptpList);
                                                    jumpSite_arList.AddRange(arList);
                                                }
                                            }
                                            // 跨site
                                            if (isJumpSiteUseId)
                                            {
                                                CaReconMsgDto jumpSite_reconMsgDto = new CaReconMsgDto();
                                                // 添加数据
                                                jumpSite_reconMsgDto.taskId = msg.taskId;
                                                jumpSite_reconMsgDto.REGION = bank.REGION;
                                                jumpSite_reconMsgDto.FUNC_CURRENCY = funcCurrency;
                                                jumpSite_reconMsgDto.BANK_CURRENCY = currency;
                                                jumpSite_reconMsgDto.Total_AMT = bank.CURRENT_AMOUNT;
                                                jumpSite_reconMsgDto.BankChargeFrom = bank.BankChargeFrom;
                                                jumpSite_reconMsgDto.BankChargeTo = bank.BankChargeTo;
                                                jumpSite_reconMsgDto.isJumpBankStatement = isJumpBankStatement;
                                                jumpSite_reconMsgDto.isJumpSiteUseId = isJumpSiteUseId;
                                                List<CaBankStatementDto> bankList = new List<CaBankStatementDto>();
                                                bankList.Add(bank);
                                                jumpSite_reconMsgDto.bankList = bankList;
                                                jumpSite_reconMsgDto.ptpList = jumpSite_ptpList;
                                                jumpSite_reconMsgDto.arList = jumpSite_arList;
                                                insertReconTask(jumpSite_reconMsgDto.taskId, JsonConvert.SerializeObject(jumpSite_reconMsgDto), sortId++, userId);
                                            }
                                            
                                        }
                                        else
                                        {
                                            // 未找到AR/SiteUseId则将数据置为unmatch并解锁
                                            if (null != bank.FORWARD_NUM && null != bank.CUSTOMER_NUM)
                                            {
                                                bank.MATCH_STATUS = "2";
                                            }
                                            else
                                            {
                                                //应该是unknow,不变
                                            }
                                            bank.ISLOCKED = false;
                                            updateBank(bank);
                                        }
                                    }
                                    
                                }
                                catch (Exception ex)
                                {
                                    Helper.Log.Info("******************************* Recon Error：" + ex.Message);
                                }
                            }
                            // 跨bank
                            foreach (var map in jumpBankMap)
                            {
                                try
                                {
                                    List<string> bsIds = map.Value;

                                    List<CaBankStatementDto> bankList = new List<CaBankStatementDto>();

                                    List<CaReconMsgDetailDto> ptpList = new List<CaReconMsgDetailDto>();
                                    List<CaReconMsgDetailDto> arList = new List<CaReconMsgDetailDto>();

                                    List<CaReconMsgDetailDto> jumpSite_ptpList = new List<CaReconMsgDetailDto>();
                                    List<CaReconMsgDetailDto> jumpSite_arList = new List<CaReconMsgDetailDto>();

                                    // 查找customer
                                    string legalEntity = "";
                                    string customerNum = "";
                                    string region = "";
                                    string menuRegionName = "";
                                    string currency = "";
                                    decimal totalAmt = Decimal.Zero;
                                    decimal bankChargeFrom = Decimal.Zero;
                                    decimal bankChargeTo = Decimal.Zero;

                                    // 根据region和customer no查找对应attr信息
                                    bool isJumpBankStatement = false;
                                    bool isJumpSiteUseId = false;
                                    string funcCurrency = "";

                                    string flag = "1";

                                    
                                    // 拼装banklist
                                    for (int i = 0; i < bsIds.Count(); i++)
                                    {
                                        // 获取bank
                                        CaBankStatementDto bank = allBank.Where(o=>o.ID == bsIds[i]).FirstOrDefault();
                                        bankList.Add(bank);
                                        totalAmt = Decimal.Add(totalAmt, bank.CURRENT_AMOUNT ?? Decimal.Zero);
                                        bankChargeFrom = Decimal.Add(bankChargeFrom, bank.BankChargeFrom ?? Decimal.Zero);
                                        bankChargeTo = Decimal.Add(bankChargeTo, bank.BankChargeTo ?? Decimal.Zero);
                                        
                                        if (i == 0)
                                        {
                                            // 查找customer
                                            legalEntity = bank.LegalEntity;
                                            customerNum = bank.CUSTOMER_NUM;
                                            region = bank.REGION;
                                            menuRegionName = bank.REGION;
                                            currency = bank.CURRENCY;

                                            // 根据region和customer no查找对应attr信息
                                            CaCustomerAttributeDto cuAttr = getCustomerAttrByCustomerNum(customerNum, legalEntity);
                                            isJumpBankStatement = cuAttr.IsJumpBankStatement ?? false;
                                            isJumpSiteUseId = cuAttr.IsJumpSiteUseId ?? false;
                                            funcCurrency = cuAttr.Func_Currency;

                                            if (!currency.Equals(funcCurrency))
                                            {
                                                flag = "0";
                                            }
                                        }
                                    }
                                    
                                    // 根据customer查找site
                                    //List<string> siteUseIds = getSiteByCustomerNum(customerNum, legalEntity);
                                    List<string> siteUseIds = customerAll.Where(o => o.LegalEntity == legalEntity && o.CustomerNum == customerNum).Select(o => o.SiteUseId).ToList();
                                    if (null != siteUseIds && siteUseIds.Count > 0)
                                    {
                                        foreach (var siteUseId in siteUseIds)
                                        {
                                            CaReconMsgDto reconMsgDto = new CaReconMsgDto();
                                            // 根据site查找PTP
                                            //ptpList = getPTPBySiteUseId(legalEntity, customerNum, siteUseId, flag);
                                            ptpList = allPTPList.Where(o => o.LegalEntity == legalEntity && o.CUSTOMER_NUM == customerNum && o.SiteUseId == siteUseId)
                                                                                          .Select(p => new CaReconMsgDetailDto
                                                                                          {
                                                                                              ID = p.ID,
                                                                                              DUE_DATE = p.DUE_DATE,
                                                                                              AMT = (flag == "1" ? p.AMT : p.Local_AMT)
                                                                                          }).OrderBy(o => o.DUE_DATE).ToList();
                                            // 根据site查找AR
                                            //arList = getARBySiteUseId(legalEntity, customerNum, siteUseId, flag);
                                            arList = allARList.Where(o => o.LegalEntity == legalEntity && o.CUSTOMER_NUM == customerNum && o.SiteUseId == siteUseId)
                                                                                              .Select(p => new CaReconMsgDetailDto
                                                                                              {
                                                                                                  ID = p.ID,
                                                                                                  DUE_DATE = p.DUE_DATE,
                                                                                                  AMT = (flag == "1" ? p.AMT : p.Local_AMT)
                                                                                              }).OrderBy(o => o.DUE_DATE).ToList();

                                            // 添加数据
                                            reconMsgDto.taskId = msg.taskId;
                                            reconMsgDto.REGION = region;
                                            reconMsgDto.FUNC_CURRENCY = funcCurrency;
                                            reconMsgDto.BANK_CURRENCY = currency;
                                            reconMsgDto.Total_AMT = totalAmt;
                                            reconMsgDto.BankChargeFrom = bankChargeFrom;
                                            reconMsgDto.BankChargeTo = bankChargeTo;
                                            reconMsgDto.isJumpBankStatement = isJumpBankStatement;
                                            reconMsgDto.isJumpSiteUseId = isJumpSiteUseId;



                                            reconMsgDto.bankList = bankList;
                                            reconMsgDto.ptpList = ptpList;
                                            reconMsgDto.arList = arList;
                                            // 添加到数据库中
                                            insertReconTask(reconMsgDto.taskId, JsonConvert.SerializeObject(reconMsgDto), sortId++, userId);
                                            // 判断是否跨site
                                            if (isJumpSiteUseId)
                                            {
                                                jumpSite_ptpList.AddRange(ptpList);
                                                jumpSite_arList.AddRange(arList);
                                            }
                                        }
                                        // 跨site
                                        if (isJumpSiteUseId)
                                        {
                                            CaReconMsgDto jumpSite_reconMsgDto = new CaReconMsgDto();
                                            // 添加数据
                                            jumpSite_reconMsgDto.taskId = msg.taskId;
                                            jumpSite_reconMsgDto.REGION = region;
                                            jumpSite_reconMsgDto.FUNC_CURRENCY = funcCurrency;
                                            jumpSite_reconMsgDto.BANK_CURRENCY = currency;
                                            jumpSite_reconMsgDto.Total_AMT = totalAmt;
                                            jumpSite_reconMsgDto.BankChargeFrom = bankChargeFrom;
                                            jumpSite_reconMsgDto.BankChargeTo = bankChargeTo;
                                            jumpSite_reconMsgDto.isJumpBankStatement = isJumpBankStatement;
                                            jumpSite_reconMsgDto.isJumpSiteUseId = isJumpSiteUseId;

                                            jumpSite_reconMsgDto.bankList = bankList;
                                            jumpSite_reconMsgDto.ptpList = jumpSite_ptpList;
                                            jumpSite_reconMsgDto.arList = jumpSite_arList;
                                            insertReconTask(jumpSite_reconMsgDto.taskId, JsonConvert.SerializeObject(jumpSite_reconMsgDto), sortId++, userId);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Helper.Log.Info("*********************** Recon justbank Error: " + ex.Message);
                                }
                            }
                        }
                        catch (Exception ex) {
                            Helper.Log.Info("***************************** Recon Error:" + ex.Message);
                        }
                        finally { 
                            if (string.IsNullOrEmpty(taskId))
                            {
                                CaReconService.recon(msg);
                            }
                            else
                            {
                                CaReconService.autoRecon(msg);
                            }

                            // 若无正常recon数据则将任务置为完成
                            if (flagCount == 0)
                            {
                                taskService.updateTaskStatusById(msg.taskId, "2");
                            }

                            scope.Complete();
                        }
                    }
                }
            }
            else
            {
                // 抛出提示信息
                throw new OTCServiceException("There is no bank statement need to recon!");
            }
        }

        public List<CaAllCustomerDto> GetCaAllCustomersByBSids(string bsids) {
            string sql = string.Format(@"SELECT DISTINCT LegalEntity, CUSTOMER_NUM as CustomerNum, SiteUseId
                                FROM
	                                V_CA_CustomerSiteUseId with (nolock) 
                                WHERE
	                                EXISTS(SELECT 1 FROM T_CA_BankStatement
									        WHERE ID IN ({0}) 
									          AND T_CA_BankStatement.LegalEntity = V_CA_CustomerSiteUseId.LegalEntity
                                              AND T_CA_BankStatement.CUSTOMER_NUM = V_CA_CustomerSiteUseId.CUSTOMER_NUM)", bsids);
            return CommonRep.ExecuteSqlQuery<CaAllCustomerDto>(sql).ToList();
        }

        public List<CaBankStatementDto> getAllBankStatement(string bsids)
        {
            string sql = string.Format(@"SELECT *
                                FROM
	                                t_ca_bankstatement with (nolock) 
                                WHERE ID IN ({0}) ", bsids);
            return CommonRep.ExecuteSqlQuery<CaBankStatementDto>(sql).ToList();
        }

        public List<CaPMTBSDto> getAllPMTByBsId(string bsids) {

            string sql = string.Format(@"SELECT
	                        TOP 1 t0.*
                        FROM
	                        T_CA_PMTBS t0 WITH (nolock)
                        INNER JOIN T_CA_PMT t1 WITH(nolock) ON t0.ReconId=t1.ID
                        WHERE
	                        t0.BANK_STATEMENT_ID in ({0})
                        AND t1.isClosed <> '1'
                        AND EXISTS (
	                        SELECT
		                        ID
	                        FROM
		                        T_CA_PMTDetail WITH (nolock)
	                        WHERE
		                        ReconId = t0.ReconId
                        )
                        ORDER BY t1.CREATE_DATE DESC", bsids);

            List<CaPMTBSDto> list = CommonRep.ExecuteSqlQuery<CaPMTBSDto>(sql).ToList();
            return list;
        }

        public List<SysTypeDetail> getAllTypeDetail095() {

            string sql = string.Format(@"SELECT Id,Seq,type_code as TypeCode,Detail_Name as DetailName,Detail_Value as DetailValue,Detail_Value2 as DetailValue2,Detail_Value3 as DetailValue3,Description
                                FROM t_sys_type_detail with (nolock) 
                                WHERE Type_Code = '095' ");
            return CommonRep.ExecuteSqlQuery<SysTypeDetail>(sql).ToList();
        }

        public List<string> getSiteByCustomerNum(string customerNum, string legalEntity)
        {
            string sql = string.Format(@"SELECT DISTINCT
	                                SiteUseId
                                FROM
	                                V_CA_CustomerSiteUseId with (nolock) 
                                WHERE
	                                CUSTOMER_NUM = '{0}'
                                AND LegalEntity = '{1}'", customerNum, legalEntity);

            return CommonRep.ExecuteSqlQuery<string>(sql).ToList();
        }

        public List<CaPMTDto> getNoBSPMT(string legalEntity, string customerNum, string currency, decimal? transactionAmount, DateTime? valueTime)
        {
            string sql = string.Format(@"SELECT
	                            t0.*
                            FROM
	                            T_CA_PMT t0 with (nolock)
                            WHERE isClosed = 0 and
	                            NOT EXISTS (
		                            SELECT
			                            ReconId
		                            FROM
			                            T_CA_PMTBS with (nolock)
		                            WHERE
			                            ReconId = t0.ID
	                            )
                            AND EXISTS (
	                            SELECT
		                            ID
	                            FROM
		                            T_CA_PMTDetail WITH (nolock)
	                            WHERE
		                            ReconId = t0.ID
                            )
                            AND t0.LegalEntity = '{0}'
                            AND t0.CustomerNum = '{1}'
                            AND t0.Currency = '{2}'
                            AND t0.TransactionAmount = {3}
                            AND t0.ValueDate='{4}'", legalEntity, customerNum, currency, transactionAmount??decimal.Zero, valueTime ?? (object)DBNull.Value);

            return CommonRep.ExecuteSqlQuery<CaPMTDto>(sql).ToList();
        }

        public List<CaPMTDto> getAllNoBSPMT()
        {
            string sql = string.Format(@"SELECT
	                            t0.*
                            FROM
	                            T_CA_PMT t0 with (nolock)
                            WHERE isClosed = 0 and
	                            NOT EXISTS (
		                            SELECT
			                            ReconId
		                            FROM
			                            T_CA_PMTBS with (nolock)
		                            WHERE
			                            ReconId = t0.ID
	                            )
                            AND EXISTS (
	                            SELECT
		                            ID
	                            FROM
		                            T_CA_PMTDetail WITH (nolock)
	                            WHERE
		                            ReconId = t0.ID
                            )");

            return CommonRep.ExecuteSqlQuery<CaPMTDto>(sql).ToList();
        }

        public List<CaReconMsgDetailAllDto> getPTPByBSids(string bsids) {

            string sql = string.Format(@"SELECT
                                    LegalEntity,
							        CUSTOMER_NUM,
							        SiteUseId,
	                                INVOICE_NUM AS ID,
	                                PTP_DATE AS DUE_DATE,
	                                ISNULL(AMT, 0) as AMT,
                                    ISNULL(Local_AMT, 0) as Local_AMT
                                FROM
	                                V_CA_PTP with (nolock)
                                WHERE EXISTS(SELECT 1 FROM T_CA_BankStatement
									          WHERE ID IN ({0}) 
									AND T_CA_BankStatement.LegalEntity = V_CA_PTP.LegalEntity
                                    AND T_CA_BankStatement.CUSTOMER_NUM = V_CA_PTP.CUSTOMER_NUM)
                                ORDER BY LegalEntity, CUSTOMER_NUM, SiteUseId, PTP_DATE", bsids);
            return CommonRep.ExecuteSqlQuery<CaReconMsgDetailAllDto>(sql).ToList();
        }

        public List<CaReconMsgDetailDto> getPTPBySiteUseId(string legalEntity, string customerNum, string siteUseId, string flag)
        {
            string sql = string.Format(@"SELECT
	                                INVOICE_NUM AS ID,
	                                PTP_DATE AS DUE_DATE,
	                                CASE
                                WHEN {3}= 1 THEN
	                                ISNULL(AMT, 0)
                                ELSE
	                                ISNULL(Local_AMT, 0)
                                END AS AMT
                                FROM
	                                V_CA_PTP with (nolock)
                                WHERE
	                                LegalEntity = '{0}'
                                AND CUSTOMER_NUM = '{1}'
                                AND SiteUseId = '{2}'
                                ORDER BY
	                                PTP_DATE", legalEntity, customerNum, siteUseId, flag);

            return CommonRep.ExecuteSqlQuery<CaReconMsgDetailDto>(sql).ToList();
        }

        public List<CaReconMsgDetailAllDto> getARByBSids(string bsids)
        {
            string sql = string.Format(@"SELECT
                                        LegalEntity,
							            CUSTOMER_NUM,
							            SiteUseId,
	                                    INVOICE_NUM AS ID,
	                                    DUE_DATE,
	                                    ISNULL(VAT_AMT,ISNULL(AMT, 0)) AS AMT,
                                        ISNULL(VAT_AMT,ISNULL(Local_AMT, 0)) as Local_AMT
                                    FROM
	                                    V_CA_AR with(nolock)
                                WHERE EXISTS(SELECT 1 FROM T_CA_BankStatement
									          WHERE ID IN ({0}) 
									AND T_CA_BankStatement.LegalEntity = V_CA_AR.LegalEntity
                                    AND T_CA_BankStatement.CUSTOMER_NUM = V_CA_AR.CUSTOMER_NUM)
                                ORDER BY LegalEntity, CUSTOMER_NUM, SiteUseId, DUE_DATE", bsids);

            return CommonRep.ExecuteSqlQuery<CaReconMsgDetailAllDto>(sql).ToList();
        }

        public List<CaReconMsgDetailDto> getARBySiteUseId(string legalEntity, string customerNum, string siteUseId, string flag)
        {
            string sql = string.Format(@"SELECT
	                            INVOICE_NUM AS ID,
	                            DUE_DATE,
	                            ISNULL(VAT_AMT,CASE
		                            WHEN {3} = 1 THEN
			                            ISNULL(AMT, 0)
		                            ELSE
			                            ISNULL(Local_AMT, 0)
		                            END) AS AMT
                            FROM
	                            V_CA_AR with(nolock)
                            WHERE
	                            LegalEntity = '{0}'
                            AND CUSTOMER_NUM = '{1}'
                            AND SiteUseId = '{2}'
                            ORDER BY
	                            DUE_DATE ASC,INVOICE_DATE ASC,INVOICE_NUM ASC", legalEntity, customerNum, siteUseId, flag);

            return CommonRep.ExecuteSqlQuery<CaReconMsgDetailDto>(sql).ToList();
        }

        public CaCustomerAttributeDto getCustomerAttrByCustomerNum(string customerNum, string legalEntity)
        {
            string sql = string.Format(@"SELECT
	                            *
                            FROM
	                            T_CA_CustomerAttribute with(nolock)
                            WHERE
	                            LegalEntity = '{0}'
                            AND CUSTOMER_NUM = '{1}'", legalEntity, customerNum);

            List<CaCustomerAttributeDto> list = CommonRep.ExecuteSqlQuery<CaCustomerAttributeDto>(sql).ToList();
            if (null != list && list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return new CaCustomerAttributeDto();
            }
        }

        public void insertReconTask(string taskId, string reconMsgStr, int sortId, string userId)
        {
            // 向reconTask中添加数据
            var insertSql = string.Format(@"INSERT INTO T_CA_ReconTask (
                                                 ID,
                                                 TASK_ID,
                                                 INPUT,
                                                 SORT_ID,
                                                 CREATE_USER,
                                                 CREATE_DATE
                                                )
                                                select 
		                                                N'{0}',
		                                                N'{1}',
		                                                N'{2}',
		                                                {3},
		                                                N'{4}',
		                                                '{5}'
	                                               where not exists (select 1 from t_ca_recontask where TASK_ID = N'{1}' AND CONVERT(NVARCHAR(max),INPUT) = N'{2}') ", Guid.NewGuid().ToString(),
                                            taskId,
                                            reconMsgStr.Replace("'","''"),
                                            sortId,
                                            userId,
                                            DateTime.Now);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(insertSql);
        }

        public CustomerMenuDtoPage allPaymentCustomerDataDetails(int page, int pageSize, string legalEntity)
        {
            CustomerMenuDtoPage result = new CustomerMenuDtoPage();
            string sql = string.Format(@"SELECT
                                    *
                                FROM
                                    (
                                        SELECT
	                                        ROW_NUMBER () OVER ( ORDER BY customerNum ASC ) AS RowNumber,* 
                                        FROM
	                                        (
	                                        SELECT
		                                        vm.CUSTOMER_NUM AS customerNum,
		                                        vm.CUSTOMER_NAME AS customerName 
	                                        FROM
		                                        V_CA_CustomerOnly vm WITH ( nolock ) 
	                                        WHERE
		                                        vm.LegalEntity = '{0}' UNION
	                                        SELECT
		                                        '' AS customerNum,
	                                        '' AS customerName 
	                                        ) a
                                    ) AS t
                                WHERE
                                    RowNumber BETWEEN {1} AND {2}", legalEntity, page == 1 ? 0 : pageSize * (page - 1) + 1, pageSize * page);
            List<CustomerMenuDto> dto = CommonRep.ExecuteSqlQuery<CustomerMenuDto>(sql).ToList();
            string sql1 = string.Format(@"select count(1)
                                        from V_CA_CustomerOnly vm with (nolock)
                                        where vm.LegalEntity = '{0}'", legalEntity);
            result.list = dto;
            result.listCount = SqlHelper.ExcuteScalar<int>(sql1);
            return result;
        }


        public CustomerMenuDtoPage likeAgentCustomerDataDetails(int page, int pageSize, string bankid)
        {
            CustomerMenuDtoPage result = new CustomerMenuDtoPage();
            string sql = string.Format(@"SELECT
                                    *
                                FROM
                                    (
                                select ROW_NUMBER() OVER (ORDER BY a.SortId) AS RowNumber,
                                  a.FORWARD_NUM  as customerNum,
                                   a.FORWARD_NAME as customerName
                                    from (
                                           SELECT DISTINCT SortId, FORWARD_NUM, FORWARD_NAME ,REGION
                                             FROM T_CA_CustomerIdentify with (nolock)
                                            where BANK_STATEMENT_ID = '{0}' and isNull(FORWARD_NUM,'') <> ''
                                        ) as a
                                    ) AS t
                                WHERE
                                    RowNumber BETWEEN {1} AND {2}", bankid, page == 1 ? 0 : pageSize * (page - 1) + 1, pageSize * page);

            List<CustomerMenuDto> dto = CommonRep.ExecuteSqlQuery<CustomerMenuDto>(sql).ToList();

            string sql1 = string.Format(@"select count(1)
                                    from T_CA_CustomerIdentify with (nolock)
                                    where BANK_STATEMENT_ID = '{0}'", bankid);
            result.list = dto;
            result.listCount = SqlHelper.ExcuteScalar<int>(sql1);

            return result;
        }

        public CustomerMenuDtoPage likePaymentCustomerDataDetails(int page, int pageSize, string bankid)
        {
            CustomerMenuDtoPage result = new CustomerMenuDtoPage();

            string sql = string.Format(@"SELECT
                                    *
                                FROM
                                    (
                            select ROW_NUMBER() OVER (ORDER BY tc.CUSTOMER_NUM DESC) AS RowNumber,
                                     tc.ID  as id,
                                     tc.CUSTOMER_NUM  as customerNum,
                                    tc.CUSTOMER_NAME as customerName,
                                    tc.NeedSendMail as needSendMail,
                                    tc.mailId as mailId,
                                    tc.mailDate as mailDate,
                                    tc.ReconId as reconId,
                                    tc.SortId
                                    from T_CA_CustomerIdentify tc with (nolock)
                                    where isnull(tc.CUSTOMER_NUM,'') <> ''
                                      and tc.BANK_STATEMENT_ID = '{0}'                                   
                                    ) AS t
                                WHERE
                                    RowNumber BETWEEN {1} AND {2}
                                ORDER BY SortId", bankid, page == 1 ? 0 : pageSize * (page - 1) + 1, pageSize * page);

            List<CustomerMenuDto> dto = CommonRep.ExecuteSqlQuery<CustomerMenuDto>(sql).ToList();

            string sql1 = string.Format(@"select count(1) from T_CA_CustomerIdentify tc with (nolock)
                                    where isnull(tc.CUSTOMER_NUM,'') <> '' and tc.BANK_STATEMENT_ID = '{0}'", bankid);

            result.list = dto;
            result.listCount = SqlHelper.ExcuteScalar<int>(sql1);

            return result;
        }

        public CaMailAlertDtoPage getCaMailAlertListbybsid(string bsid, string alertType, int page, int pageSize) {
            CaMailAlertDtoPage result = new CaMailAlertDtoPage();

            string sql = string.Format(@"SELECT
                                    *
                                FROM
                                    (
                            select ROW_NUMBER() OVER (ORDER BY m.CreateTime DESC) AS RowNumber,
                                    m.ID  as id,
                                    m.BSID  as BSID,
                                    m.AlertType as AlertType,
                                    m.EID as EID,
                                    m.TransNumber as TransNumber,
                                    m.LegalEntity as LegalEntity,
                                    m.CustomerNum as CustomerNum,
                                    m.SiteUseId as SiteUseId,
                                    m.ToTitle  as ToTitle,
                                    m.CCTitle  as CCTitle,
                                    m.CreateTime as CreateTime,
                                    m.MessageId as MessageId,
                                    m.SendTime as SendTime,
                                    m.status as status,
                                    m.comment as comment,
                                    z.[to]  as mailto,
                                    z.[cc]  as mailcc,
                                    z.subject as subject
                                    from T_CA_MailAlert m with (nolock)
                                    left join t_mail_tmp z on z.message_id = m.MessageId
                                    where m.bsid = '{0}'  and m.AlertType = '{1}'                               
                                    ) AS t
                                WHERE
                                    RowNumber BETWEEN {2} AND {3}
                                ORDER BY CreateTime desc", bsid, alertType, (page == 1 ? 0 : pageSize * (page - 1) + 1), pageSize * page);

            List<CaMailAlertDto> dto = CommonRep.ExecuteSqlQuery<CaMailAlertDto>(sql).ToList();

            string sql1 = string.Format(@"select count(1) from T_CA_MailAlert m with (nolock)
                                    where m.bsid = '{0}' and m.AlertType = '{1}'", bsid, alertType);

            result.dataRows = dto;
            result.count = SqlHelper.ExcuteScalar<int>(sql1);

            return result;
        }

        public CaARViewDtoAndAmtTotal getArHisDataDetails(string customerNum,string legalEntity)
        {
            CaARViewDtoAndAmtTotal result = new CaARViewDtoAndAmtTotal();
            string sql = string.Format(@"
                                select ROW_NUMBER() OVER (ORDER BY DUE_DATE DESC) AS RowNumber,
                                   LegalEntity as legalEntity,
                                   CUSTOMER_NUM as customerNum,
                                   SiteUseId as siteUseId,
                                   INVOICE_NUM as invoiceNum,
                                   INVOICE_DATE as invoiceDate,
                                   DUE_DATE as dueDate,
                                   FUNC_CURRENCY as funcCurrency,
                                   INV_CURRENCY as invCurrency,
                                   AMT as amt,
                                   Local_AMT as localAmt,
                                   EBName as Ebname
                            from V_CA_AR
                            where CUSTOMER_NUM = '{0}' and LegalEntity = '{1}'
                            ORDER BY DUE_DATE
                                   ", customerNum, legalEntity);

            List<CaARViewDto> dto = CommonRep.ExecuteSqlQuery<CaARViewDto>(sql).ToList();

            string sql1 = string.Format(@"select isnull(sum(isnull(AMT, 0)),0)
                                            from V_CA_AR
                                            where CUSTOMER_NUM = '{0}' and LegalEntity = '{1}'", customerNum, legalEntity);
            result.list = dto;
            result.amtTotal = SqlHelper.ExcuteScalar<decimal>(sql1);

            return result;
        }

        public void updateBsMatchStatusById(string bsId, string matchStatus)
        {
            SqlParameter[] ps = new SqlParameter[2];
            ps[0] = new SqlParameter("@MATCH_STATUS", matchStatus);
            ps[1] = new SqlParameter("@ID", bsId);

            string sql = @"UPDATE T_CA_BankStatement
                        SET 
                         MATCH_STATUS = @MATCH_STATUS
                        WHERE
                            ID = @ID";

            //更新
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql, ps);
        }


        public void changeNeedSendMail(string id, bool needSendMail)
        {
            try
            {
                string sql = string.Format(@"update T_CA_CustomerIdentify set NeedSendMail = (case when lower('{0}') = 'true' then 1 else 0 end) where ID='{1}'", needSendMail, id);

                SqlHelper.ExcuteSql(sql);

            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);

            }
        }

        public void changeNeedSendMailAll(string bankStatementId, bool needSendMail)
        {
            try
            {
                string sql = string.Format(@"update T_CA_CustomerIdentify set NeedSendMail = (case when lower('{0}') = 'true' then 1 else 0 end) where BANK_STATEMENT_ID='{1}'", needSendMail, bankStatementId);

                SqlHelper.ExcuteSql(sql);

            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);

            }
        }

        public List<CustomerMenuDto> likePaymentCustomer(string bankid)
        {
            CustomerMenuDtoPage result = new CustomerMenuDtoPage();

            string sql = string.Format(@"SELECT *
                                        FROM (
                                                 select ROW_NUMBER() OVER (partition by tc.CUSTOMER_NUM ORDER BY tc.CUSTOMER_NUM DESC) AS rn,
                                                        tc.ID                                                                          as id,
                                                        tc.CUSTOMER_NUM                                                                as customerNum,
                                                        tc.CUSTOMER_NAME                                                               as customerName,
                                                        tc.NeedSendMail                                                                as needSendMail,
                                                        tc.ReconId                                                                     as reconId,
                                                        tc.SortId                                                                      as sortId,
                                                        tc.OnlyReconResult                                                             as onlyReconResult,
                                                        tc.mailId                                                                      as mailId,
                                                        tc.mailDate                                                                    as mailDate,
                                                 from T_CA_CustomerIdentify tc with (nolock)
                                                 where isnull(tc.CUSTOMER_NUM, '') <> ''
                                                   and tc.NeedSendMail =1
                                                   and tc.BANK_STATEMENT_ID = '{0}') as u
                                        WHERE u.rn = 1 ORDER BY SortId", bankid);

            return CommonRep.ExecuteSqlQuery<CustomerMenuDto>(sql).ToList();

        }

        public int countCustomerMappingByCustomerNumAndName(string customerNum, string customerNameInBank, string legalEntity)
        {
            customerNum = customerNum == null ? "" : customerNum.Replace("'", "''");
            customerNameInBank = customerNameInBank == null ? "" : customerNameInBank.Replace("'", "''");
            legalEntity = legalEntity == null ? "" : legalEntity.Replace("'", "''");

            string sql = string.Format(@"SELECT
	                    COUNT (*) AS COUNT
                    FROM
	                    T_CA_CustomerMapping
                    WHERE
	                    CUSTOMER_NUM = '{0}'
                    AND BankCustomerName = '{1}'
                    AND LegalEntity = '{2}'", customerNum, customerNameInBank, legalEntity);

            return CommonRep.ExecuteSqlQuery<CountDto>(sql).ToList()[0].COUNT;
        }

        public void createCustomerMappingByCustomerNumAndName(string customerNum, string customerNameInBank, string legalEntity)
        {
            legalEntity = legalEntity == null ? "" : legalEntity.Replace("'", "''");
            customerNum = customerNum == null ? "" : customerNum.Replace("'", "''");
            customerNameInBank = customerNameInBank == null ? "" : customerNameInBank.Replace("'", "''");
            string sql = string.Format(@"IF not exists (select 1 from T_CA_CustomerMapping where LegalEntity =N'{1}' and CUSTOMER_NUM = N'{2}' and BankCustomerName = N'{3}')
                INSERT INTO T_CA_CustomerMapping (
	                ID,
	                LegalEntity,
	                CUSTOMER_NUM,
	                BankCustomerName,
	                Status,
	                CREATE_User,
	                CREATE_Date
                )
                VALUES
	                (
		                N'{0}',
		                N'{1}',
		                N'{2}',
		                N'{3}',
		                '1',
		                N'{4}',
		                '{5}'
	                )", Guid.NewGuid(), legalEntity, customerNum, customerNameInBank, "", DateTime.Now);

            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);
        }

        public int countForwarderListByCustomerNumAndName(string forwardNum, string forwardName, string legalEntity)
        {
            forwardNum = forwardNum == null ? "" : forwardNum.Replace("'", "''");
            forwardName = forwardName == null ? "" : forwardName.Replace("'", "''");
            legalEntity = legalEntity == null ? "" : legalEntity.Replace("'", "''");
            string sql = string.Format(@"SELECT
	                                COUNT (*) AS COUNT
                                FROM
	                                T_CA_ForwarderList
                                WHERE
	                                FORWARD_NUM = '{0}'
                                AND FORWARD_NAME = '{1}'
                                AND LegalEntity = '{2}'", forwardNum, forwardName, legalEntity);

            return CommonRep.ExecuteSqlQuery<CountDto>(sql).ToList()[0].COUNT;
        }

        public int countForwarderListByCustomerNumAndName(string forwardNum, string forwardName, string customerNum, string legalEntity)
        {
            forwardNum = forwardNum == null ? "" : forwardNum.Replace("'", "''");
            forwardName = forwardName == null ? "" : forwardName.Replace("'", "''");
            customerNum = customerNum == null ? "" : customerNum.Replace("'", "''");
            legalEntity = legalEntity == null ? "" : legalEntity.Replace("'", "''");
            string sql = string.Format(@"SELECT
	                    COUNT (*) AS count
                    FROM
	                    T_CA_ForwarderList
                    WHERE
	                    FORWARD_NUM = '{0}'
                    AND FORWARD_NAME = '{1}'
                    AND CUSTOMER_NUM = '{2}'
                    AND LegalEntity = '{3}'", forwardNum, forwardName, customerNum, legalEntity);

            return CommonRep.ExecuteSqlQuery<CountDto>(sql).ToList()[0].COUNT;
        }

        public void createForwarderListByCustomerNumAndName(string customerNum, string forwardNum, string forwardName, string legalEntity)
        {
            customerNum = customerNum == null ? "" : customerNum.Replace("'", "''");
            forwardNum = forwardNum == null ? "" : forwardNum.Replace("'", "''");
            forwardName = forwardName == null ? "" : forwardName.Replace("'", "''");
            legalEntity = legalEntity == null ? "" : legalEntity.Replace("'", "''");

            string sql = string.Format(@"INSERT INTO T_CA_ForwarderList( ID,
                     LegalEntity,
                     CUSTOMER_NUM,
                     FORWARD_NUM,
                     FORWARD_NAME,
                     Status,
                     CREATE_User,
                     CREATE_Date
                    )
                    VALUES
	                    (
		                    N'{0}',
		                    N'{1}',
		                    N'{2}',
		                    N'{3}',
		                    N'{4}',
		                    '1',
		                    N'{5}',
		                    '{6}'
	                    )", Guid.NewGuid(), legalEntity, customerNum, forwardNum, forwardName, "Auto", DateTime.Now);

            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);
        }

        public CustomerMenuDtoPage allAgentCustomerDataDetails(int page, int pageSize, string legalEntity)
        {
            CustomerMenuDtoPage result = new CustomerMenuDtoPage();
            string sql = string.Format(@"SELECT
                                    *
                                FROM
                                    (
                                        select ROW_NUMBER() OVER (ORDER BY customerNum ASC) AS RowNumber,* from (
                                            select
                                                   vm.CUSTOMER_NUM                                   as customerNum,
                                                   vm.CUSTOMER_NAME                                  as customerName
                                            from V_CA_CustomerOnly vm with (nolock)
                                            where vm.LegalEntity = '{0}'
                                            union
                                            select
                                                   FORWARD_NUM                                   as customerNum,
                                                   FORWARD_NAME                                  as customerName      
                                            from T_CA_ForwarderList with (nolock)  where  LegalEntity = '{0}'
                                            union
											select
                                                   ''                                   as customerNum,
                                                   ''                                  as customerName    )  a
                                    ) AS t
                                WHERE
                                    RowNumber BETWEEN {1} AND {2}", legalEntity, page == 1 ? 0 : pageSize * (page - 1) + 1, pageSize * page);
            List<CustomerMenuDto> dto = CommonRep.ExecuteSqlQuery<CustomerMenuDto>(sql).ToList();
            string sql1 = string.Format(@"select count(1)
                                            from (
                                                     select vm.CUSTOMER_NUM  as customerNum,
                                                            vm.CUSTOMER_NAME as customerName
                                                     from V_CA_CustomerOnly vm with (nolock)
                                                     where vm.LegalEntity = '{0}'
                                                     union
                                                     select FORWARD_NUM  as customerNum,
                                                            FORWARD_NAME as customerName
                                                     from T_CA_ForwarderList with (nolock)
                                                     where LegalEntity = '{0}') a", legalEntity);
            result.list = dto;
            result.listCount = SqlHelper.ExcuteScalar<int>(sql1);
            return result;
        }

        
        public CaBankStatementDtoPage getBankHistoryListByTaskType(string taskId, string taskType, int page, int pageSize)
        {
            CaBankStatementDtoPage result = new CaBankStatementDtoPage();

            string sql = string.Format(@"SELECT
	                                        *
                                        FROM
	                                        (
		                                        SELECT
			                                        ROW_NUMBER () OVER (ORDER BY t0.CREATE_DATE DESC) AS RowNumber,
			                                        t0.*
		                                        FROM
			                                        T_CA_BankStatement t0
		                                        WHERE
			                                        DEL_FLAG = 0
		                                        AND t0.ID IN (
			                                        SELECT
				                                        BSID
			                                        FROM
				                                        T_CA_TaskBS b
			                                        WHERE
				                                        b.TASKID = '{0}'
		                                        )
	                                        ) AS t
                                        WHERE
	                                        RowNumber BETWEEN {1}
                                        AND {2}", taskId, page == 1 ? 0 : pageSize * (page - 1) + 1, pageSize * page);

            List<CaBankStatementDto> dto = CommonRep.ExecuteSqlQuery<CaBankStatementDto>(sql).ToList();

            string sql1 = string.Format(@"select count(1) as count from T_CA_BankStatement 
                                          WHERE
                                            DEL_FLAG = 0
		                                  AND ID IN (
			                                 SELECT BSID
			                                 FROM T_CA_TaskBS b
			                                 WHERE b.TASKID = '{0}')", taskId);

            result.dataRows = dto;
            result.count = SqlHelper.ExcuteScalar<int>(sql1);

            return result;
        }

        public List<CaBankStatementDto> GetBankByTaskId(string taskId)
        {
            string sql = string.Format(@"select s.BSID as ID 
                                         from T_CA_TaskBS s 
                                        where s.TASKID = '{0}'", taskId);
            List<CaBankStatementDto> list = CommonRep.ExecuteSqlQuery<CaBankStatementDto>(sql).ToList();
            return list;

        }


        public HttpResponseMessage exporBankStatementAll(string statusselect, string legalEntity, string transNumber, string transcurrency, string transamount, string transCustomer, string transaForward, string valueDataF, string valueDataT, string createDateF, string createDateT, string ishistory, string bsType)
        {
            string templateFile = "";
            string fileName = "";
            string tmpFile = "";

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportBankStatementTemplate"].ToString());
                fileName = "BankStatement_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                string collecotrList = "";
                XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");     //注入服务
                var userId = AppContext.Current.User.EID; //当前用户ID
                collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");       //获得用户List(它这里用逗号间隔拼了一下)
                if (!string.IsNullOrEmpty(collecotrList))
                {
                    collecotrList = collecotrList.Substring(0, collecotrList.LastIndexOf(","));
                    collecotrList = collecotrList.Replace(",", "','");
                }
                collecotrList = "'" + collecotrList + "'";

                if (string.IsNullOrEmpty(statusselect))
                {
                    statusselect = "";
                }
                statusselect = "'" + statusselect.Replace(",", "','") + "'";
                if (string.IsNullOrEmpty(legalEntity) || legalEntity == "undefined")
                {
                    legalEntity = "";
                }
                if (string.IsNullOrEmpty(transNumber) || transNumber == "undefined")
                {
                    transNumber = "";
                }
                if (string.IsNullOrEmpty(transcurrency) || transcurrency == "undefined")
                {
                    transcurrency = "";
                }
                if (string.IsNullOrEmpty(transamount) || transamount == "undefined" || transamount == "null")
                {
                    transamount = "";
                }
                if (string.IsNullOrEmpty(transCustomer) || transCustomer == "undefined")
                {
                    transCustomer = "";
                }
                if (string.IsNullOrEmpty(transaForward) || transaForward == "undefined")
                {
                    transaForward = "";
                }
                if (string.IsNullOrEmpty(valueDataF) || valueDataF == "undefined")
                {
                    valueDataF = "";
                }
                if (string.IsNullOrEmpty(valueDataT) || valueDataT == "undefined")
                {
                    valueDataT = "";
                }
                if (string.IsNullOrEmpty(createDateF) || createDateF == "undefined")
                {
                    createDateF = "";
                }
                if (string.IsNullOrEmpty(createDateT) || createDateT == "undefined")
                {
                    createDateT = "";
                }
                if (string.IsNullOrEmpty(bsType) || bsType == "undefined")
                {
                    bsType = "";
                }
                string sql = string.Format(@"SELECT
                                    *,
                                    T_SYS_TYPE_DETAIL.DETAIL_NAME as BSTYPENAME,
                                    (case MATCH_STATUS when '0' then '#FF0000' when '1' then '#FF69B4' when '2' then '#B23AEE' when '4' then '#1C86EE' when '7' then '#828282' when '8' then '#8B3A62' when '9' then '#9C9C9C' end) as statuscolor,
                                    (select count(*) from T_CA_CustomerIdentify with (nolock) where BANK_STATEMENT_ID = t.ID and isnull(CUSTOMER_NUM,'')<>'') as countIdentify,
                                    (SELECT SUM(COUNT) AS COUNT FROM (
                                        SELECT COUNT(*) AS COUNT FROM T_CA_ForwarderList WITH(nolock) WHERE FORWARD_NAME=REF1
                                        UNION
                                        SELECT COUNT(*) AS COUNT FROM T_CA_CustomerMapping WITH(nolock) WHERE BankCustomerName=REF1
                                        UNION
                                        SELECT COUNT(*) AS COUNT FROM T_CUSTOMER WITH(nolock) WHERE LOCALIZE_CUSTOMER_NAME=REF1 OR CUSTOMER_NAME=REF1
                                    ) t) as countCustomer
                                FROM
                                    (
                                        SELECT
			                                ROW_NUMBER () OVER (ORDER BY t0.CREATE_DATE,t0.LegalEntity,t0.CURRENCY,t0.TRANSACTION_NUMBER DESC) AS RowNumber,
			                                t0.*,t1.DETAIL_NAME AS MATCH_STATUS_NAME,
			                                ISNULL(
				                                (
					                                SELECT
						                                SUM (1)
					                                FROM
						                                dbo.T_CA_PMT AS PMT with (nolock)
					                                JOIN dbo.T_CA_PMTBS AS PMTBS with (nolock) ON PMTBS.ReconId = PMT.ID
					                                WHERE
						                                PMT.isClosed = 0
					                                AND PMTBS.BANK_STATEMENT_ID = t0.ID
				                                ),
				                                0
			                                ) AS HASPMTDETAIL,
                                            ISNULL(t2.FILE_COUNT, 0) AS HASFILE,
			                                (SELECT
						                                TOP 1 GroupNo
					                                FROM
						                                dbo.T_CA_PMT AS PMT WITH (nolock)
					                                JOIN dbo.T_CA_PMTBS AS PMTBS WITH (nolock) ON PMTBS.ReconId = PMT.ID
					                                WHERE
						                                PMT.isClosed = 0
					                                AND PMTBS.BANK_STATEMENT_ID = t0.ID) AS GroupNo,
			                                (SELECT
						                                TOP 1 FILENAME
					                                FROM
						                                dbo.T_CA_PMT AS PMT WITH (nolock)
					                                JOIN dbo.T_CA_PMTBS AS PMTBS WITH (nolock) ON PMTBS.ReconId = PMT.ID
					                                WHERE
						                                PMT.isClosed = 0
					                                AND PMTBS.BANK_STATEMENT_ID = t0.ID) AS PMTFileName
		                                FROM
			                                T_CA_BankStatement t0 with(nolock)
                                        INNER JOIN T_SYS_TYPE_DETAIL t1 with(nolock) ON t0.MATCH_STATUS = t1.DETAIL_VALUE AND t1.TYPE_CODE = '088'
                                        LEFT JOIN (select count(id) AS FILE_COUNT, BSID from T_CA_BSFile with(nolock) WHERE DEL_FLAG = 0 GROUP BY BSID) t2 ON t0.ID = t2.BSID
                                        WHERE
                                            t0.DEL_FLAG = 0
                                        AND ((CREATE_USER IN ({0})) OR (CHARINDEX('{13}',(SELECT DETAIL_VALUE3 FROM T_SYS_TYPE_DETAIL WHERE TYPE_CODE = '087' AND DETAIL_NAME = t0.LegalEntity )) > 0))
                                        AND t0.MATCH_STATUS in ({3})
                                        AND ((t0.TRANSACTION_NUMBER like '%{4}%') OR '' = '{4}')
                                        AND ((t0.LegalEntity like '%{12}%') OR '' = '{12}')
                                        AND ((t0.CURRENCY like '%{5}%') OR '' = '{5}')
                                        AND ((t0.TRANSACTION_AMOUNT = '{6}') OR '' = '{6}' OR (t0.CURRENT_AMOUNT = '{6}'))
                                        AND ((t0.CUSTOMER_NUM like '%{7}%') OR '' = '{7}' OR (t0.CUSTOMER_NAME like '%{7}%') or '' = '{7}')
                                        AND ((t0.FORWARD_NUM like '%{8}%') OR '' = '{8}' OR (t0.FORWARD_NAME like '%{8}%') or '' = '{8}')
                                        AND (t0.VALUE_DATE >= '{9} 00:00:00' OR '' = '{9}')
                                        AND (t0.VALUE_DATE <= '{10} 23:59:59' OR '' = '{10}')
                                        AND (t0.CREATE_DATE >= '{14} 00:00:00' OR '' = '{14}')
                                        AND (t0.CREATE_DATE <= '{15} 23:59:59' OR '' = '{15}')
                                        AND (t0.ISHISTORY = '{11}' OR '' = '{11}')
                                        AND (t0.BSTYPE = '{16}' OR '' = '{16}')
                                    ) AS t
                                    LEFT JOIN T_SYS_TYPE_DETAIL ON T_SYS_TYPE_DETAIL.TYPE_CODE = '085' AND t.BSTYPE = T_SYS_TYPE_DETAIL.DETAIL_VALUE
                                ", collecotrList, null, null, statusselect, transNumber.Replace("'", "''"), transcurrency.Replace("'", "''"), transamount.Replace("'", "''"), transCustomer.Replace("'", "''"), transaForward.Replace("'", "''"), valueDataF, valueDataT, ishistory, legalEntity.Replace("'", "''"), userId, createDateF, createDateT, bsType);

                List<CaBankStatementDto> bankStatementList = CommonRep.ExecuteSqlQuery<CaBankStatementDto>(sql).ToList();

                this.SetData(templateFile, tmpFile, bankStatementList);

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


        public HttpResponseMessage exporPmtDetail(string groupNo, string legalEntity, string customerNum, string currency, string amount, string transactionNumber, string invoiceNum, string valueDateF, string valueDateT, string createDateF, string createDateT, string isClosed, string hasBS, string hasMatched, string hasInv)
        {
            string templateFile = "";
            string fileName = "";
            string tmpFile = "";

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportPmtDetailTemplate"].ToString());
                fileName = "PmtDetail_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                CaPmtDtoPage pmtList = getCaPmtDetailList(groupNo, legalEntity, customerNum, currency, amount, transactionNumber, invoiceNum, valueDateF, valueDateT, createDateF, createDateT, isClosed, hasBS, hasInv, hasMatched, 1, 9999999);

                this.SetPmtData(templateFile, tmpFile, pmtList.pmt);

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
        

        private void SetData(string templateFileName, string tmpFile, List<CaBankStatementDto> bankStatementList)
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
                foreach (var lst in bankStatementList)
                {
                    helper.SetData(rowNo, 0, lst.ID);
                    helper.SetData(rowNo, 1, lst.MATCH_STATUS_NAME);
                    helper.SetData(rowNo, 2, lst.LegalEntity);
                    helper.SetData(rowNo, 3, lst.TRANSACTION_NUMBER);
                    helper.SetData(rowNo, 4, lst.CURRENCY);
                    helper.SetData(rowNo, 5, lst.TRANSACTION_AMOUNT);
                    helper.SetData(rowNo, 6, lst.CURRENT_AMOUNT);
                    helper.SetData(rowNo, 7, lst.REF1);
                    helper.SetData(rowNo, 8, lst.VALUE_DATE);
                    helper.SetData(rowNo, 9, lst.FORWARD_NUM);
                    helper.SetData(rowNo, 10, lst.FORWARD_NAME);
                    helper.SetData(rowNo, 11, lst.CUSTOMER_NUM);
                    helper.SetData(rowNo, 12, lst.CUSTOMER_NAME);
                    helper.SetData(rowNo, 13, lst.IsFixedBankCharge);
                    helper.SetData(rowNo, 14, lst.BankChargeFrom);
                    helper.SetData(rowNo, 15, lst.BankChargeTo);
                    helper.SetData(rowNo, 16, lst.PMTFileName);
                    helper.SetData(rowNo, 17, lst.CREATE_DATE);
                    helper.SetData(rowNo, 18, lst.UPDATE_DATE);

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


        private void SetPmtData(string templateFileName, string tmpFile, List<CaPMTDto> pmtDetailList)
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
                foreach (var lst in pmtDetailList)
                {
                    helper.SetData(rowNo, 0, rowNo);
                    helper.SetData(rowNo, 1, lst.LegalEntity);
                    helper.SetData(rowNo, 2, lst.ValueDate);
                    helper.SetData(rowNo, 3, lst.ReceiveDate);
                    helper.SetData(rowNo, 4, lst.CustomerNum);
                    helper.SetData(rowNo, 5, lst.CustomerName);
                    helper.SetData(rowNo, 6, lst.Currency);
                    helper.SetData(rowNo, 7, lst.TransactionAmount);
                    helper.SetData(rowNo, 8, lst.Amount);
                    helper.SetData(rowNo, 9, lst.GroupNo);
                    helper.SetData(rowNo, 10, lst.filename);
                    helper.SetData(rowNo, 11, lst.CREATE_USER);
                    helper.SetData(rowNo, 12, lst.CREATE_DATE);

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

        private List<CaBankStatementDto> GetBankStatementAll()
        {
            string sql = string.Format(@"SELECT
			                                ROW_NUMBER () OVER (ORDER BY t0.CREATE_DATE DESC) AS RowNumber,
                                            PMT.filename as PMTFileName,
			                                t0.*,
			                                ISNULL(
				                                (
					                                SELECT
						                                SUM (1)
					                                FROM
						                                dbo.T_CA_PMT AS PMT with (nolock)
					                                JOIN dbo.T_CA_PMTBS AS PMTBS with (nolock) ON PMTBS.ReconId = PMT.ID
					                                WHERE
						                                PMT.isClosed = 0
					                                AND PMTBS.BANK_STATEMENT_ID = t0.ID
				                                ),
				                                0
			                                ) AS HASPMTDETAIL
		                                FROM
			                                T_CA_BankStatement t0 with (nolock)
                                            left join dbo.T_CA_PMTBS AS PMTBS with (nolock) ON t0.ID = PMTBS.BANK_STATEMENT_ID
                                            left join dbo.T_CA_PMT AS PMT with (nolock) on PMTBS.ReconId = PMT.ID
                                        WHERE
                                            t0.DEL_FLAG = 0
                                        AND t0.CREATE_USER = '{0}'
                                        ", AppContext.Current.User.EID);


            return CommonRep.ExecuteSqlQuery<CaBankStatementDto>(sql).ToList();
        }

        public int isExistedTransactionNum(string bankId, string transactionNum)
        {
            string sql = String.Empty;
            if (String.IsNullOrEmpty(bankId) || bankId == "undefined")
            {
                sql = string.Format(@"select count(0) 
                                    from T_CA_BankStatement  with(nolock)
                                    where TRANSACTION_NUMBER = '{0}'
                                    and DEL_FLAG = '0'", transactionNum);
            }
            else
            {
                sql = string.Format(@"select count(0) 
                                    from T_CA_BankStatement  with(nolock)
                                    where ID != '{0}'
                                    and TRANSACTION_NUMBER = '{1}'
                                    and DEL_FLAG = '0'", bankId, transactionNum);
            }

            return SqlHelper.ExcuteScalar<int>(sql);
        }

        public List<CaBankStatementDto> GetBankByTranc(string transactionNum)
        {
            string sql = string.Format(@"select ID 
                                        from T_CA_BankStatement with(nolock)
                                        where TRANSACTION_NUMBER = '{0}'
                                        and DEL_FLAG = '0'", transactionNum);
            List<CaBankStatementDto> list = CommonRep.ExecuteSqlQuery<CaBankStatementDto>(sql).ToList();
            return list;
        }

        public List<CaBankStatementDto> GetBankListByTranNum(string transactionNum)
        {
            string sql = string.Format(@"select * 
                                        from T_CA_BankStatement with(nolock)
                                        where TRANSACTION_NUMBER = '{0}'
                                        and DEL_FLAG = '0'", transactionNum);
            List<CaBankStatementDto> list = CommonRep.ExecuteSqlQuery<CaBankStatementDto>(sql).ToList();
            return list;
        }

        public CaARViewDtoAndAmtTotal getReconArHisDataDetails(string reconId)
        {
            CaARViewDtoAndAmtTotal result = new CaARViewDtoAndAmtTotal();
            string sql = string.Format(@"select a.LegalEntity                   as legalEntity,
                                               REPLACE(c.GroupType,'UN-','')    as groupType ,
                                               r.CUSTOMER_NUM                   as customerNum,
                                               r.SiteUseId                      as siteUseId,
                                               r.InvoiceNum                     as invoiceNum,
                                               a.INVOICE_DATE                   as invoiceDate,
                                               r.DueDate                        as dueDate,
                                               a.FUNC_CURRENCY                  as funcCurrency,
                                               a.INV_CURRENCY                   as invCurrency,
                                               r.Amount                         as amt,
                                               r.LocalCurrencyAmount            as localAmt,
                                               a.Ebname                         as Ebname
                                        from T_CA_Recon c with (nolock)
                                                 join T_CA_ReconDetail r with (nolock) on c.ID = r.ReconId
                                                 join V_CA_AR_ALL a with (nolock) on r.SiteUseId = a.SiteUseId and r.InvoiceNum = a.INVOICE_NUM
                                        where r.ReconId = '{0}'
                                        order by r.SortId", reconId);

            List<CaARViewDto> dto = CommonRep.ExecuteSqlQuery<CaARViewDto>(sql).ToList();

            string sql1 = string.Format(@"select isnull(sum(isnull(r.Amount, 0)),0)
                                        from T_CA_ReconDetail r with (nolock)
                                                 inner join V_CA_AR_ALL a  with (nolock) on r.SiteUseId = a.SiteUseId and r.InvoiceNum = a.INVOICE_NUM
                                        where r.ReconId = '{0}'", reconId);
            result.list = dto;
            result.amtTotal = SqlHelper.ExcuteScalar<decimal>(sql1);

            return result;

        }


        public void RemovePmtBs(string id)
        {
            try
            {
                string sql = string.Format(@"delete  from T_CA_PMTBS where BANK_STATEMENT_ID = '{0}'", id);

                SqlHelper.ExcuteSql(sql);

            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);

            }
        }

        public FileDto getFileById(string fileId)
        {
            string sql = string.Format(@"SELECT FILE_ID as FileId, FILE_NAME as FileName,PHYSICAL_PATH as PhysicalPath 
                                         FROM T_FILE with (nolock) WHERE FILE_ID = '{0}'", fileId);
            List<FileDto> list = CommonRep.ExecuteSqlQuery<FileDto>(sql).ToList();
            if (list != null && list.Count > 0) 
            {
                return list[0];
            }
            else
            {
                return null;
            }
        }
        public List<string> getAllAvailableBSIds(string statusselect, string legalEntity, string transNumber, string transcurrency, string transamount, string transCustomer, string transaForward, string valueDateF, string valueDateT, string ishistory, string createDateF, string createDateT, string bsType)
        {
            string collecotrList = "";
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");     //注入服务
            var userId = AppContext.Current.User.EID; //当前用户ID
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");       //获得用户List(它这里用逗号间隔拼了一下)
            if (!string.IsNullOrEmpty(collecotrList))
            {
                collecotrList = collecotrList.Substring(0, collecotrList.LastIndexOf(","));
                collecotrList = collecotrList.Replace(",", "','");
            }
            collecotrList = "'" + collecotrList + "'";

            if (string.IsNullOrEmpty(statusselect))
            {
                statusselect = "";
            }
            statusselect = "'" + statusselect.Replace(",", "','") + "'";
            if (string.IsNullOrEmpty(legalEntity) || legalEntity == "undefined")
            {
                legalEntity = "";
            }
            if (string.IsNullOrEmpty(transNumber) || transNumber == "undefined")
            {
                transNumber = "";
            }
            if (string.IsNullOrEmpty(transcurrency) || transcurrency == "undefined")
            {
                transcurrency = "";
            }
            if (string.IsNullOrEmpty(transamount) || transamount == "undefined" || transamount == "null")
            {
                transamount = "";
            }
            if (string.IsNullOrEmpty(transCustomer) || transCustomer == "undefined")
            {
                transCustomer = "";
            }
            if (string.IsNullOrEmpty(transaForward) || transaForward == "undefined")
            {
                transaForward = "";
            }
            if (string.IsNullOrEmpty(valueDateF) || valueDateF == "undefined")
            {
                valueDateF = "";
            }
            if (string.IsNullOrEmpty(valueDateT) || valueDateT == "undefined")
            {
                valueDateT = "";
            }
            if (string.IsNullOrEmpty(createDateF) || createDateF == "undefined")
            {
                createDateF = "";
            }
            if (string.IsNullOrEmpty(createDateT) || createDateT == "undefined")
            {
                createDateT = "";
            }
            if (string.IsNullOrEmpty(bsType) || bsType == "undefined")
            {
                bsType = "";
            }

            string bankIdSql = string.Format(@"SELECT
                        ID
                    FROM
                        T_CA_BankStatement with (nolock)
                    WHERE
                        (
                            isnull(ISLOCKED,0) <> 1
                        )
                    AND (
		                MATCH_STATUS NOT IN ('4','7','8','9')
	                )
                    AND DEL_FLAG = 0
                    AND MATCH_STATUS in ({2})
                    AND ((TRANSACTION_NUMBER like '%{3}%') OR '' = '{3}')
                    AND ((CURRENCY like '%{4}%') OR '' = '{4}')
                    AND ((TRANSACTION_AMOUNT = '{5}') OR '' = '{5}' OR (CURRENT_AMOUNT = '{5}'))
                    AND ((CUSTOMER_NUM like '%{6}%') OR '' = '{6}' OR (CUSTOMER_NAME like '%{6}%') or '' = '{6}')
                    AND ((FORWARD_NUM like '%{7}%') OR '' = '{7}' OR (FORWARD_NAME like '%{7}%') or '' = '{7}')
                    AND (VALUE_DATE >= '{8} 00:00:00' OR '' = '{8}')
                    AND (VALUE_DATE <= '{9} 23:59:59' OR '' = '{9}')
                    AND (CREATE_DATE >= '{12} 00:00:00' OR '' = '{12}')
                    AND (CREATE_DATE <= '{13} 23:59:59' OR '' = '{13}')
                    AND (ISHISTORY = '{10}' OR '' = '{10}')
                    AND ((LegalEntity like '%{11}%') OR '' = '{11}')
                    AND (BSTYPE = '{14}' OR '' = '{14}')
                    AND (CREATE_USER IN ({0}) OR (CHARINDEX('{1}',(SELECT DETAIL_VALUE3 FROM T_SYS_TYPE_DETAIL with(nolock) WHERE TYPE_CODE = '087' AND DETAIL_NAME = LegalEntity )) > 0)) ",
                    collecotrList, userId, statusselect, transNumber, transcurrency, transamount, transCustomer, transaForward, valueDateF, valueDateT, ishistory, legalEntity, createDateF, createDateT,bsType);
            return CommonRep.ExecuteSqlQuery<string>(bankIdSql).ToList();
        }

        public List<string> getAllAvailableBSIds(string userId, string taskId)
        {
            string collecotrList = "";
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");     //注入服务
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");       //获得用户List(它这里用逗号间隔拼了一下)
            if (!string.IsNullOrEmpty(collecotrList))
            {
                collecotrList = collecotrList.Substring(0, collecotrList.LastIndexOf(","));
                collecotrList = collecotrList.Replace(",", "','");
            }
            collecotrList = "'" + collecotrList + "'";

            //fujie.wan 2020-06-22
            //当同一天相同金额相同币制，有2笔PMT导入，一笔没有销账明细，一笔有;但BS已经识别了没有销账明细的那笔，这时侯系统永远无法识别到有PMT Detail那笔
            //现修改为，当发现有相同的PMT且当前
            string bankIdSql = string.Format(@"SELECT
	                    ID
                    FROM
	                    T_CA_BankStatement with (nolock)
                    WHERE
	                    (
		                    ISLOCKED <> 1
		                    OR ISLOCKED IS NULL
	                    )
                    AND ((MATCH_STATUS = 0) or 
                         (MATCH_STATUS = 2 
                          AND EXISTS(SELECT 1 FROM T_CA_PMT
                                WHERE ValueDate = VALUE_DATE and Currency = CURRENCY and TransactionAmount = TRANSACTION_AMOUNT and CustomerNum = CUSTOMER_NUM
                                and isClosed = 0
                                and not exists (select 1 from T_CA_PMTBS where T_CA_PMT.id = T_CA_PMTBS.ReconId) 
                                and exists (select 1 from T_CA_PMTDetail where ReconId = T_CA_PMT.id)
                                ) 
                         AND NOT EXISTS(SELECT 1 FROM T_CA_PMTDetail join 
											 T_CA_PMTBS on T_CA_PMTDetail.ReconId = T_CA_PMTBS.ReconId WHERE T_CA_PMTBS.BANK_STATEMENT_ID = T_CA_BankStatement.ID )
                        ))
                    AND (ISHISTORY <> 1)
                    AND ID IN (SELECT BSID FROM T_CA_TaskBS with(nolock) WHERE TASKID='{2}')
                    AND (CREATE_USER IN ({0}) OR (CHARINDEX('{1}',(SELECT DETAIL_VALUE3 FROM T_SYS_TYPE_DETAIL with(nolock) WHERE TYPE_CODE = '087' AND DETAIL_NAME = LegalEntity )) > 0))", collecotrList, userId, taskId);

            return CommonRep.ExecuteSqlQuery<string>(bankIdSql).ToList();
        }

        public List<string> getAllAvailableUnknownBSIds(string userId, string taskId)
        {
            string collecotrList = "";
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");     //注入服务
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");       //获得用户List(它这里用逗号间隔拼了一下)
            if (!string.IsNullOrEmpty(collecotrList))
            {
                collecotrList = collecotrList.Substring(0, collecotrList.LastIndexOf(","));
                collecotrList = collecotrList.Replace(",", "','");
            }
            collecotrList = "'" + collecotrList + "'";

            string bankIdSql = string.Format(@"SELECT
	                    ID
                    FROM
	                    T_CA_BankStatement with (nolock)
                    WHERE
	                    (
		                    ISLOCKED <> 1
		                    OR ISLOCKED IS NULL
	                    )
                    AND (MATCH_STATUS = 0)
                    AND (ISHISTORY <> 1)
                    AND (
	                    FORWARD_NUM IS NOT NULL
	                    OR FORWARD_NUM <> ''
                    )
                    AND (
	                    CUSTOMER_NUM IS NULL
	                    OR CUSTOMER_NUM = ''
                    )
					AND(ISNULL(Customer_NUM, '') <> ISNULL(FORWARD_NUM, ''))
                    AND ID IN (SELECT BSID FROM T_CA_TaskBS with(nolock) WHERE TASKID='{2}')
                    AND (CREATE_USER IN ({0}) OR (CHARINDEX('{1}',(SELECT DETAIL_VALUE3 FROM T_SYS_TYPE_DETAIL with(nolock) WHERE TYPE_CODE = '087' AND DETAIL_NAME = LegalEntity )) > 0))", collecotrList, userId, taskId);

            return CommonRep.ExecuteSqlQuery<string>(bankIdSql).ToList();
        }

        public List<string> getAllUnmatchAvailableBSIds(string userId,string taskId)
        {
            string collecotrList = "";
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");     //注入服务
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");       //获得用户List(它这里用逗号间隔拼了一下)
            if (!string.IsNullOrEmpty(collecotrList))
            {
                collecotrList = collecotrList.Substring(0, collecotrList.LastIndexOf(","));
                collecotrList = collecotrList.Replace(",", "','");
            }
            collecotrList = "'" + collecotrList + "'";

            string bankIdSql = string.Format(@"SELECT
                                        ID
                                    FROM
                                        T_CA_BankStatement with (nolock)
                                    WHERE
                                        (
                                            ISLOCKED <> 1
                                            OR ISLOCKED IS NULL
                                        )
                                    AND MATCH_STATUS IN (2, 3)
                                    AND (ISHISTORY <> 1)
                                    AND ID IN (SELECT BSID FROM T_CA_TaskBS with(nolock) WHERE TASKID='{2}')
                                    AND EXISTS (
                                        SELECT DISTINCT
                                            SiteUseId
                                        FROM
                                            V_CA_CustomerSiteUseId with (nolock)
                                        WHERE
                                            CUSTOMER_NUM = T_CA_BankStatement.CUSTOMER_NUM
                                        AND LegalEntity = T_CA_BankStatement.LegalEntity
                                    ) AND EXISTS (
                                        select INVOICE_NUM from V_CA_AR with (nolock) where CUSTOMER_NUM = T_CA_BankStatement.CUSTOMER_NUM
                                    )
                                    AND (CREATE_USER IN ({0}) OR (CHARINDEX('{1}',(SELECT DETAIL_VALUE3 FROM T_SYS_TYPE_DETAIL with(nolock) WHERE TYPE_CODE = '087' AND DETAIL_NAME = LegalEntity )) > 0))", collecotrList, userId, taskId);

            return CommonRep.ExecuteSqlQuery<string>(bankIdSql).ToList();
        }

        public string UploadBatchChangeINC()
        {
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            string strArchiveVATKey = "ArchiveVATPath";

            string resultFileId = Guid.NewGuid().ToString();

            List<string> msgList = new List<string>();

            HttpFileCollection files = HttpContext.Current.Request.Files;

            archivePath = ConfigurationManager.AppSettings[strArchiveVATKey].ToString();
            archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
            if (Directory.Exists(archivePath) == false)
            {
                Directory.CreateDirectory(archivePath);
                //   return "Path doesn't exsit！";
            }
            archiveFileName = files[0].FileName;
            int splitNum = archiveFileName.LastIndexOf(".");
            string strFileName = archivePath + "\\" + archiveFileName.Substring(0, splitNum) + DateTime.Now.ToString("yyyyMMddHHmmssfff") + archiveFileName.Substring(splitNum);
            files[0].SaveAs(strFileName);

            try
            {
                NpoiHelper helper = new NpoiHelper(strFileName);
                helper.ActiveSheet = 0;
                int maxRowNumber = helper.GetLastRowNum();  //获得数据总行数

                List<string> listSQL = new List<string>();
                for (int row = 1; row <= maxRowNumber; row++)
                {
                    var oldINC = helper.GetValue(row, 0).ToString();
                    var newINC = helper.GetCell(row, 1).ToString();

                    // 根据原INC查看BS状态
                    List<CaBankStatementDto> list = GetBankListByTranNum(oldINC);

                    if (null != list && list.Count > 0)
                    {
                        CaBankStatementDto bank = list[0];
                        if (bank.MATCH_STATUS.Equals("9"))
                        {
                            // 若已销账则给出相应提示
                            msgList.Add("Closed");
                            continue;
                        }
                    }
                    else
                    {
                        // 若未找到则给出相应提示
                        msgList.Add("Not Found");
                        continue;
                    }

                    // 若正常则变更INC
                    string sql = string.Format(@"UPDATE T_CA_BankStatement SET TRANSACTION_NUMBER='{0}' WHERE TRANSACTION_NUMBER='{1}'", newINC,oldINC);
                    msgList.Add("OK");
                    listSQL.Add(sql);
                }

                SqlHelper.ExcuteListSql(listSQL);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!");
            }

            try
            {
                var resultFileName = Path.GetFileName(strFileName).Replace("'", "''");
                //保存文件记录
                StringBuilder sqlFile = new StringBuilder();
                sqlFile.Append("INSERT INTO T_FILE (FILE_ID,FILE_NAME,PHYSICAL_PATH,OPERATOR,CREATE_TIME)");
                sqlFile.Append(" VALUES ('" + resultFileId + "',");
                sqlFile.Append("         '" + resultFileName.Replace("'", "''") + "',");
                sqlFile.Append("         '" + strFileName + "',");
                sqlFile.Append("         '" + AppContext.Current.User.EID + "',GETDATE());");
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(sqlFile.ToString());

                NpoiHelper writeHelper = new NpoiHelper(strFileName);
                int sheetCount = writeHelper.Sheets.Count();
                for (int i_sheet = 0; i_sheet < sheetCount; i_sheet++)
                {
                    writeHelper.ActiveSheet = i_sheet;
                    int rownum = writeHelper.GetLastRowNum();
                    int dataRow = 1;
                    if (rownum > dataRow)
                    {
                        writeHelper = insertColumns(writeHelper, dataRow - 1);
                        writeHelper.SetData(dataRow - 1, 0, "Status");

                        rownum = rownum + 1;
                        int i = dataRow;
                        foreach (string str in msgList)
                        {
                            writeHelper.SetData(i, 0, str);
                            i++;
                        }
                    }

                }
                writeHelper.Save();
            }
            catch (Exception wbe)
            {
                Helper.Log.Error(wbe.Message, wbe);
            }
            return resultFileId;
        }



        public string UploadBatchManualClose()
        {
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            string strArchiveVATKey = "ArchiveVATPath";

            string resultFileId = Guid.NewGuid().ToString();

            List<string> msgList = new List<string>();

            HttpFileCollection files = HttpContext.Current.Request.Files;

            archivePath = ConfigurationManager.AppSettings[strArchiveVATKey].ToString();
            archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
            if (Directory.Exists(archivePath) == false)
            {
                Directory.CreateDirectory(archivePath);
                //   return "Path doesn't exsit！";
            }
            archiveFileName = files[0].FileName;
            int splitNum = archiveFileName.LastIndexOf(".");
            string strFileName = archivePath + "\\" + archiveFileName.Substring(0, splitNum) + DateTime.Now.ToString("yyyyMMddHHmmssfff") + archiveFileName.Substring(splitNum);
            files[0].SaveAs(strFileName);

            try
            {
                NpoiHelper helper = new NpoiHelper(strFileName);
                helper.ActiveSheet = 0;
                int maxRowNumber = helper.GetLastRowNum();  //获得数据总行数

                List<string> listSQL = new List<string>();
                for (int row = 0; row <= maxRowNumber; row++)
                {
                    var closeINC = helper.GetValue(row, 0).ToString();

                    // 根据原INC查看BS状态
                    List<CaBankStatementDto> list = GetBankListByTranNum(closeINC);

                    if (null != list && list.Count > 0)
                    {
                        CaBankStatementDto bank = list[0];
                        if (bank.MATCH_STATUS.Equals("9"))
                        {
                            // 若已销账则给出相应提示
                            msgList.Add("Already Closed");
                            continue;
                        }
                    }
                    else
                    {
                        // 若未找到则给出相应提示
                        msgList.Add("Not Found");
                        continue;
                    }

                    // 若正常则变更INC
                    string sql = string.Format(@"UPDATE T_CA_BankStatement SET Match_Status = '9', UPDATE_DATE = getdate(), Comments = 'Manual Closed by " + AppContext.Current.User.EID + "' WHERE TRANSACTION_NUMBER='{0}'", closeINC);
                    msgList.Add("OK");
                    listSQL.Add(sql);
                }

                SqlHelper.ExcuteListSql(listSQL);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!");
            }

            try
            {
                var resultFileName = Path.GetFileName(strFileName).Replace("'", "''");
                //保存文件记录
                StringBuilder sqlFile = new StringBuilder();
                sqlFile.Append("INSERT INTO T_FILE (FILE_ID,FILE_NAME,PHYSICAL_PATH,OPERATOR,CREATE_TIME)");
                sqlFile.Append(" VALUES ('" + resultFileId + "',");
                sqlFile.Append("         '" + resultFileName.Replace("'", "''") + "',");
                sqlFile.Append("         '" + strFileName + "',");
                sqlFile.Append("         '" + AppContext.Current.User.EID + "',GETDATE());");
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(sqlFile.ToString());

                NpoiHelper writeHelper = new NpoiHelper(strFileName);
                int sheetCount = writeHelper.Sheets.Count();
                for (int i_sheet = 0; i_sheet < sheetCount; i_sheet++)
                {
                    writeHelper.ActiveSheet = i_sheet;
                    int rownum = writeHelper.GetLastRowNum();
                    int dataRow = 0;
                    if (rownum >= dataRow)
                    {
                        writeHelper = insertColumns(writeHelper, dataRow);
                        //writeHelper.SetData(dataRow - 1, 0, "Status");

                        //rownum = rownum + 1;
                        int i = dataRow;
                        foreach (string str in msgList)
                        {
                            writeHelper.SetData(i, 0, str);
                            i++;
                        }
                    }

                }
                writeHelper.Save();
            }
            catch (Exception wbe)
            {
                Helper.Log.Error(wbe.Message, wbe);
            }
            return resultFileId;
        }
        private NpoiHelper insertColumns(NpoiHelper poihelper, int startIndex)
        {
            int rowNum = poihelper.GetLastRowNum();
            int colNum = 0;
            for (; colNum < 100; colNum++)
            {
                if (poihelper.GetCell(0, colNum) == null)
                {
                    break;
                }
            }

            for (int n = startIndex; n <= rowNum; n++)
            {
                for (int i = colNum; i >= 0; i--)
                {
                    poihelper.CopyCell(n, i + 1, n, i, true, true);
                }
            }
            return poihelper;
        }

        public string reuploadPost()
        {
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            string strArchiveVATKey = "ArchiveVATPath";

            string errorMsg = "";

            HttpFileCollection files = HttpContext.Current.Request.Files;

            archivePath = ConfigurationManager.AppSettings[strArchiveVATKey].ToString();
            archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
            if (Directory.Exists(archivePath) == false)
            {
                Directory.CreateDirectory(archivePath);
            }
            archiveFileName = files[0].FileName;
            string strFileName = archivePath + "\\" + archiveFileName;
            files[0].SaveAs(strFileName);

            try
            {
                NpoiHelper helper = new NpoiHelper(strFileName);
                helper.ActiveSheet = 0;
                int maxRowNumber = helper.GetLastRowNum();  //获得数据总行数

                if(maxRowNumber > 0)
                {
                    var firstCol = helper.GetValue(0, 0) == null ? "" : helper.GetValue(0, 0).ToString();
                    var rvCol = helper.GetCell(0, 6) == null ? "" : helper.GetValue(0, 6).ToString();
                    var lastCol = helper.GetCell(0, 14) == null ? "" : helper.GetValue(0, 14).ToString();
                    if (!firstCol.ToLower().Equals("status") || !rvCol.ToLower().Equals("rv number") || !lastCol.ToLower().Equals("comments"))
                    {
                        throw new OTCServiceException("Please upload post confirm file!");
                    }
                }
                else
                {
                    // 无数据抛出异常
                    throw new OTCServiceException("No data in Excel!");
                }

                List<string> listSQL = new List<string>();
                for (int row = 1; row <= maxRowNumber; row++)
                {
                    var status = helper.GetValue(row, 0) == null ? "" : helper.GetValue(row, 0).ToString();
                    var rvNum = helper.GetValue(row, 6) == null ? "" : helper.GetCell(row, 6).ToString();
                    var legalEntity = helper.GetValue(row, 2) == null ? "" : helper.GetCell(row, 2).ToString();

                    // 根据原INC查看BS状态
                    List<CaBankStatementDto> list = GetBankListByTranNum(rvNum);

                    if (null != list && list.Count > 0)
                    {
                        CaBankStatementDto bank = list[0];
                        if (!bank.APPLY_STATUS.Equals("1") || bank.MATCH_STATUS.Equals("8") || bank.MATCH_STATUS.Equals("9"))
                        {
                            // 若状态不为1则无法入账
                            errorMsg += "Can't post the bankstatement which RV num is " + rvNum + " at row " + (row + 1) + "\r\n";
                            continue;
                        }
                    }
                    else
                    {
                        // 若未找到则给出相应提示
                        errorMsg += "Can't find the bankstatement which RV num is " + rvNum + " at row " + (row + 1) + "\r\n";
                        continue;
                    }

                    string sql = "";
                    string sqlMail = "";
                    if (status.ToLower().Equals("ok"))
                    {
                        sql = string.Format(@"UPDATE T_CA_BankStatement SET APPLY_STATUS='2' , APPLY_CONFIRM_TIME='{0}' WHERE TRANSACTION_NUMBER='{1}'", DateTime.Now, rvNum);

                        //入账是否为RMB,如果是RMB即使是PrePayment也要发邮件
                        bool lb_RMB = false;
                        if (list[0].CURRENCY == "CNY") { lb_RMB = true; }

                        //只有CA Team启用Confirm send mail
                        string strCurrentUser = AppContext.Current.User.EID;
                        SysTypeDetail caMember = (from ca in CommonRep.GetDbSet<SysTypeDetail>()
                                              where ca.TypeCode == "100" && ca.DetailName == strCurrentUser
                                                  select ca).FirstOrDefault();
                        string strSiteUseId = list[0].SiteUseId == null ? "" : list[0].SiteUseId;
                        var notCod = CommonRep.GetQueryable<SysTypeDetail>().Where(o => o.TypeCode == "048").Select(o => o.DetailName).DefaultIfEmpty().ToList();
                        var customerPmt = (from c in CommonRep.GetQueryable<Customer>()
                                           where c.SiteUseId == strSiteUseId && (!notCod.Contains(c.CreditTrem) || lb_RMB)
                                           select c).ToList();
                        if (customerPmt == null)
                        {
                            Helper.Log.Info("************************************ COD ********************");
                        }
                        if (list[0].CURRENT_AMOUNT > 10 && customerPmt != null && customerPmt.Count > 0 && caMember != null && !string.IsNullOrEmpty(caMember.DetailValue))
                        { 
                            //插入PMT Mail发送记录
                            string strSql = string.Format(@"SELECT ID,
                                                        LegalEntity,
                                                        TRANSACTION_NUMBER,
                                                        CURRENT_AMOUNT,
                                                        CUSTOMER_NUM,
                                                        SiteUseId,
                                                        MATCH_STATUS
                                   FROM t_ca_bankstatement 
                                  WHERE TRANSACTION_NUMBER = '{0}' and LegalEntity = '{1}'", rvNum, list[0].LegalEntity);

                            List<CaBankStatementDto> listBS = SqlHelper.GetList<CaBankStatementDto>(SqlHelper.ExcuteTable(strSql, System.Data.CommandType.Text, null));
                            if (listBS.Count > 0 && (listBS[0].MATCH_STATUS == "-1" || listBS[0].MATCH_STATUS == "0" || listBS[0].MATCH_STATUS == "2"))
                            {
                                string strId = Guid.NewGuid().ToString();
                                string strBSId = listBS[0].ID;
                                string strLegalEntity = listBS[0].LegalEntity;
                                string strCUSTOMER_NUM = listBS[0].CUSTOMER_NUM;
                                string strTRANSACTION_NUMBER = listBS[0].TRANSACTION_NUMBER;
                                decimal? decCURRENT_AMOUNT = listBS[0].CURRENT_AMOUNT == null ? 0 : Convert.ToDecimal(listBS[0].CURRENT_AMOUNT);
                                SysTypeDetail toCc = (from ca in CommonRep.GetDbSet<SysTypeDetail>()
                                                        where ca.TypeCode == "086" && ca.DetailName == "CAPMT"
                                                      select ca).FirstOrDefault();
                                string strTo = "";
                                string strCc = "";
                                if (toCc != null) {
                                    strTo = toCc.DetailValue;
                                    strCc = toCc.DetailValue2;
                                }
                                sqlMail = string.Format(@"IF NOT EXISTS (SELECT 1 FROM T_CA_MailAlert WHERE BSID = '{1}' AND AlertType = '006') INSERT INTO t_ca_mailalert (ID, BSID, AlertType, EID, TransNumber, LegalEntity, CustomerNum, SiteUseId, Amount, ToTitle, CCTitle ) values ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', {8}, '{9}', '{10}')", strId, strBSId, "006", AppContext.Current.User.EID, strTRANSACTION_NUMBER, strLegalEntity, strCUSTOMER_NUM, strSiteUseId, decCURRENT_AMOUNT, strTo, strCc);
                            }
                        }
                    }
                    else
                    {
                        sql = string.Format(@"UPDATE T_CA_BankStatement SET APPLY_STATUS='0' , APPLY_TIME=NULL , APPLY_CONFIRM_TIME=NULL, CUSTOMER_NUM=NULL, CUSTOMER_NAME=NULL, FORWARD_NUM=Null,FORWARD_NAME=NULL,MATCH_STATUS=0 WHERE TRANSACTION_NUMBER='{0}'", rvNum);
                    }
                    listSQL.Add(sql);
                    Helper.Log.Info("************************************ 777777777777 ********************");
                    if (!string.IsNullOrEmpty(sqlMail))
                    {
                        Helper.Log.Info("************************************ 888888888888 ********************");
                        listSQL.Add(sqlMail);
                    }
                }

                SqlHelper.ExcuteListSql(listSQL);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException(ex.Message);
            }
            return errorMsg;
        }

        public string reuploadPostClear()
        {
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            string strArchiveVATKey = "ArchiveVATPath";

            string errorMsg = "";

            bool nodataFlag = true;
            string nodataMsg = "";

            var bankStatusMap = new Dictionary<string, bool>();

            HttpFileCollection files = HttpContext.Current.Request.Files;

            archivePath = ConfigurationManager.AppSettings[strArchiveVATKey].ToString();
            archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
            if (Directory.Exists(archivePath) == false)
            {
                Directory.CreateDirectory(archivePath);
            }
            archiveFileName = files[0].FileName;
            string strFileName = archivePath + "\\" + archiveFileName;
            files[0].SaveAs(strFileName);

            try
            {
                List<string> listSQL = new List<string>();
                NpoiHelper helper = new NpoiHelper(strFileName);
                int sheetCount = helper.Sheets.Count();
                for (int i_sheet = 0; i_sheet < sheetCount; i_sheet++)
                {
                    helper.ActiveSheet = i_sheet;
                    int maxRowNumber = helper.GetLastRowNum();  //获得数据总行数

                    string type = "";
                    int statusIndex = 0;
                    int rvNumIndex = 0;
                    int invoiceNumberIndex = 0;
                    int amountAppliedIndex = 0;

                    if (maxRowNumber > 0)
                    {

                        var C_firstCol = helper.GetValue(0, 0) == null ? "" : helper.GetValue(0, 0).ToString();
                        var C_rvCol = helper.GetValue(0, 1) == null ? "" : helper.GetCell(0, 1).ToString();
                        var C_invoiceNumberCol = helper.GetValue(0, 4) == null ? "" : helper.GetCell(0, 4).ToString();
                        var C_lastCol = helper.GetValue(0, 11) == null ? "" : helper.GetCell(0, 11).ToString();

                        //clear 模板修改了
                        if (C_firstCol.ToLower().Equals("customer account number") && C_rvCol.ToLower().Equals("receipt number") && C_invoiceNumberCol.ToLower().Equals("trx number") && C_lastCol.ToLower().Equals("result"))
                        {
                            type = "clear";
                            statusIndex = 11;
                            rvNumIndex = 1;
                            invoiceNumberIndex = 4;
                            amountAppliedIndex = 5;
                        }
                        else
                        {
                            // 无数据抛出异常
                            throw new OTCServiceException("Please upload clear confirm file!");
                        }
                    }
                    else
                    {
                        // 无数据抛出异常
                        nodataMsg += "No data at sheet " + (i_sheet + 1) + " !";
                    }
                    //读取所有RV的状态，有一条失败，则整个组合失败
                    List<CaClearResultMedian> listResult = new List<CaClearResultMedian>();
                    for (int row = 1; row <= maxRowNumber; row++)
                    {
                        var rvNum = helper.GetValue(row, rvNumIndex) == null ? "" : helper.GetCell(row, rvNumIndex).ToString();
                        var status = helper.GetValue(row, statusIndex) == null ? "" : helper.GetValue(row, statusIndex).ToString();
                        
                        CaClearResultMedian find = listResult.Find(o => o.rvNumber == rvNum);
                        if (find == null)
                        {
                            CaClearResultMedian newItem = new CaClearResultMedian();
                            newItem.rvNumber = rvNum;
                            newItem.rvResult = status;
                            listResult.Add(newItem);
                        }
                        else
                        {
                            if (!status.ToLower().Equals("success"))
                            {
                                find.rvResult = "error";
                            }
                        }
                    }
                    for (int row = 1; row <= maxRowNumber; row++)
                    {
                        // 若存在数据则将状态置为false
                        nodataFlag = false;

                        var rvNum = helper.GetValue(row, rvNumIndex) == null ? "" : helper.GetCell(row, rvNumIndex).ToString();
                        var invoiceNumber = helper.GetValue(row, invoiceNumberIndex) == null ? "" : helper.GetCell(row, invoiceNumberIndex).ToString();

                        CaClearResultMedian find = listResult.Find(o => o.rvNumber == rvNum);

                        if (string.IsNullOrEmpty(find.rvResult) || string.IsNullOrEmpty(rvNum))
                        {
                            continue;
                        }
                        if (find.rvResult.ToLower().Equals("success"))
                        {
                            List<CaBankStatementDto> list = GetBankListByTranNum(find.rvNumber);
                            if (null != list && list.Count > 0)
                            {
                                CaBankStatementDto bank = list[0];
                                listSQL.Add("UPDATE T_CA_ReconDetail SET isCleared = 1 WHERE ReconId IN (SELECT ID FROM T_CA_Recon with(nolock) WHERE ID IN ( SELECT ReconId FROM T_CA_ReconBS WHERE bank_statement_id = '" + bank.ID + "' ) AND GroupType NOT LIKE 'NM%' AND GroupType NOT LIKE 'UN%') AND InvoiceNum = '" + invoiceNumber + "'");

                                //只有CA Team启用Confirm send mail
                                string strCurrentUser = AppContext.Current.User.EID;
                                SysTypeDetail caMember = (from ca in CommonRep.GetDbSet<SysTypeDetail>()
                                                          where ca.TypeCode == "100" && ca.DetailName == strCurrentUser
                                                          select ca).FirstOrDefault();

                                string siteSQL ="select top 1 siteuseid from T_CA_ReconDetail WHERE ReconId IN (SELECT ID FROM T_CA_Recon with(nolock) WHERE ID IN ( SELECT ReconId FROM T_CA_ReconBS WHERE bank_statement_id = '" + bank.ID + "' ) AND GroupType NOT LIKE 'NM%' AND GroupType NOT LIKE 'UN%')";
                                string strSiteUseIdCod = SqlHelper.ExcuteScalar<string>(siteSQL, null);

                                var notCod = CommonRep.GetQueryable<SysTypeDetail>().Where(o => o.TypeCode == "048").Select(o => o.DetailName).DefaultIfEmpty().ToList();
                                var customerPmt = (from c in CommonRep.GetQueryable<Customer>() 
                                            where c.SiteUseId == strSiteUseIdCod && !notCod.Contains(c.CreditTrem)
                                            select c).ToList();
                                if (customerPmt == null) {
                                    Helper.Log.Info("************************************ COD ********************");
                                }

                                if (customerPmt != null && customerPmt.Count > 0 && caMember != null && !string.IsNullOrEmpty(caMember.DetailValue))
                                {
                                    //插入PMT Mail发送记录
                                    string strId = Guid.NewGuid().ToString();
                                    string strBSId = list[0].ID;
                                    string strLegalEntity = list[0].LegalEntity;
                                    string strCUSTOMER_NUM = list[0].CUSTOMER_NUM == null ? "" : list[0].CUSTOMER_NUM;
                                    string strSiteUseId = list[0].SiteUseId == null ? "" : list[0].SiteUseId;
                                    string strTRANSACTION_NUMBER = list[0].TRANSACTION_NUMBER;
                                    decimal? decCURRENT_AMOUNT = list[0].CURRENT_AMOUNT == null ? 0 : Convert.ToDecimal(list[0].CURRENT_AMOUNT);
                                    SysTypeDetail toCc = (from ca in CommonRep.GetDbSet<SysTypeDetail>()
                                                          where ca.TypeCode == "086" && ca.DetailName == "CAClearConfirm"
                                                          select ca).FirstOrDefault();
                                    //如果是factory客户，To和CC与普通客户不同
                                    string customerAttribute = string.Format("select * from T_CA_CustomerAttribute where CUSTOMER_NUM = '{0}'", strCUSTOMER_NUM);
                                    List<CaCustomerAttributeDto> customerAttriList = SqlHelper.GetList<CaCustomerAttributeDto>(SqlHelper.ExcuteTable(customerAttribute, CommandType.Text, null));
                                    if (customerAttriList != null && customerAttriList.Count > 0 && (customerAttriList[0].IsFactoring == null ? false : customerAttriList[0].IsFactoring) == true)
                                    {
                                        Helper.Log.Info("********************************** Factory客户 ****************************" + strCUSTOMER_NUM);
                                        toCc = (from ca in CommonRep.GetDbSet<SysTypeDetail>()
                                                where ca.TypeCode == "086" && ca.DetailName == "CAClearConfirmFactory"
                                                select ca).FirstOrDefault();
                                    }
                                    string strTo = "";
                                    string strCc = "";
                                    if (toCc != null)
                                    {
                                        strTo = toCc.DetailValue;
                                        strCc = toCc.DetailValue2;
                                    }
                                    string sqlMail = string.Format(@"IF NOT EXISTS (SELECT 1 FROM T_CA_MailAlert WHERE BSID = '{1}' AND AlertType = '008' AND Status = 'Initialized') INSERT INTO t_ca_mailalert (ID, BSID, AlertType, EID, TransNumber, LegalEntity, CustomerNum, SiteUseId, Amount, ToTitle, CCTitle ) values ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', {8}, '{9}', '{10}')", strId, strBSId, "008", AppContext.Current.User.EID, strTRANSACTION_NUMBER, strLegalEntity, strCUSTOMER_NUM, strSiteUseId, decCURRENT_AMOUNT, strTo, strCc);
                                    listSQL.Add(sqlMail);
                                }
                            }
                        }
                    }

                    foreach (CaClearResultMedian rv in listResult)
                    {
                        string sql = "";
                        // 根据原INC查看BS状态
                        List<CaBankStatementDto> list = GetBankListByTranNum(rv.rvNumber);
                        if (null != list && list.Count > 0)
                        {
                            CaBankStatementDto bank = list[0];
                            // 上传clear文件
                            // 判断bank是否满足clear要求
                            if (!bank.CLEARING_STATUS.Equals("1") || bank.MATCH_STATUS.Equals("8") || bank.MATCH_STATUS.Equals("9"))
                            {
                                // 若状态不为1则无法入账
                                errorMsg += "Can't clear the bankstatement which RV num is " + rv.rvNumber + "\r\n";
                                continue;
                            }

                            //clear模板修改后，status为success
                            if (rv.rvResult.ToLower().Equals("success"))
                            {
                                sql = string.Format(@"UPDATE T_CA_BankStatement SET CLEARING_STATUS='0' , CLEARING_CONFIRM_TIME='{0}' WHERE ID='{1}'", DateTime.Now, bank.ID);
                            }
                            else
                            {
                                sql = string.Format(@"UPDATE T_CA_BankStatement SET CLEARING_STATUS='0' WHERE ID='{0}'", bank.ID);
                            }

                            listSQL.Add(sql);
                            //clear模板修改后，status为success
                            if (rv.rvResult.ToLower().Equals("success"))
                            {
                                listSQL.Add("UPDATE T_CA_Recon SET isClosed = 1 WHERE ID IN( SELECT ReconId FROM T_CA_ReconBS with(nolock) WHERE bank_statement_id = '" + bank.ID + "' ) AND GroupType NOT LIKE 'NM%' AND GroupType NOT LIKE 'UN%' AND isClosed = 0");
                                //置BS UnClear Amount金额
                                listSQL.Add(string.Format(@"UPDATE dbo.T_CA_BankStatement 
                                                           SET CURRENT_AMOUNT = TRANSACTION_AMOUNT - isNull((SELECT SUM(ISNULL(T_CA_ReconDetail.Amount,0)) FROM T_CA_ReconBS with (nolock)
                                                                                   JOIN T_CA_Recon with (nolock) ON T_CA_ReconBS.ReconId = T_CA_Recon.ID
                                                                                   JOIN T_CA_ReconDetail with (nolock) ON T_CA_ReconBS.ReconId = T_CA_ReconDetail.ReconId
                                                                                  WHERE T_CA_ReconBS.BANK_STATEMENT_ID = '{0}' AND T_CA_Recon.isClosed = 1 AND
                                                                                        T_CA_Recon.GroupType NOT LIKE 'NM%' AND T_CA_Recon.GroupType NOT LIKE 'UN%' AND 
                                                                                        T_CA_ReconDetail.isCleared <> 0),0)
                                                        WHERE id = '{0}'", bank.ID));
                                listSQL.Add(string.Format(@"UPDATE dbo.T_CA_BankStatement 
                                                           SET UNCLEAR_AMOUNT = isNull((SELECT SUM(ISNULL(T_CA_ReconDetail.Amount,0)) FROM T_CA_ReconBS with (nolock)
                                                                                   JOIN T_CA_Recon with (nolock) ON T_CA_ReconBS.ReconId = T_CA_Recon.ID
                                                                                   JOIN T_CA_ReconDetail with (nolock) ON T_CA_ReconBS.ReconId = T_CA_ReconDetail.ReconId
                                                                                  WHERE T_CA_ReconBS.BANK_STATEMENT_ID = '{0}' AND T_CA_Recon.isClosed = 1 AND
                                                                                        T_CA_Recon.GroupType NOT LIKE 'NM%' AND T_CA_Recon.GroupType NOT LIKE 'UN%' AND 
                                                                                        T_CA_ReconDetail.isCleared = 0),0)
                                                        WHERE id = '{0}'", bank.ID));
                                listSQL.Add(@"UPDATE T_CA_BankStatement SET CLEARING_STATUS = '0', MATCH_STATUS = '2' WHERE ID = '" + bank.ID + "'");
                                listSQL.Add("UPDATE T_CA_BankStatement SET CURRENT_AMOUNT = (case when CURRENT_AMOUNT < 0 then 0 else CURRENT_AMOUNT end ) WHERE ID = '" + bank.ID + "'");
                                //如果消账后，CurrenAmount还有金额(Partial)，清空PMTDetail Mail已发送标志
                                listSQL.Add("UPDATE T_CA_BankStatement SET ISPMTDetailMail = 0, ISClearConfirMail = 0 WHERE ID = '" + bank.ID + "' AND CURRENT_AMOUNT > 0");
                                //当没有未Recon及没有未消账的，置整个BS Closed
                                listSQL.Add("UPDATE T_CA_BankStatement SET CLEARING_STATUS='2', MATCH_STATUS = '9' WHERE ID = '" + bank.ID + "' AND ISNULL(CURRENT_AMOUNT, 0) <= 0 AND ISNULL(UNCLEAR_AMOUNT, 0) <= 0");
                                //将使用的PMT状态置为关闭
                                listSQL.Add("UPDATE T_CA_PMT SET isClosed=1 where ID IN (SELECT reconid FROM T_CA_PMTBS WHERE BANK_STATEMENT_ID = '" + bank.ID + "' ) ");
                                string customerNum = bank.CUSTOMER_NUM;
                                string forwardNum = bank.FORWARD_NUM;
                                string forwardName = bank.FORWARD_NAME;
                                string customerNameInBank = bank.REF1;
                                string legalEntity = bank.LegalEntity;

                                if (!string.IsNullOrEmpty(customerNameInBank) && !checkInvalidCustomer(customerNameInBank) && !checkLocalCustomer(customerNameInBank))
                                {
                                    // 根据customerNum、customerNameInBank查找是否存在
                                    if (countCustomerMappingByCustomerNumAndName(customerNum, customerNameInBank, legalEntity) == 0)
                                    {
                                        // 不存在则插入
                                        createCustomerMappingByCustomerNumAndName(customerNum, customerNameInBank, legalEntity);
                                    }

                                    // 根据forwardNum、forwardName查找是否为代付公司
                                    if (countForwarderListByCustomerNumAndName(forwardNum, forwardName, legalEntity) > 0)
                                    {
                                        // 若为代付公司则根据customerNum、forwardNum、forwardName查找是否存在
                                        if (countForwarderListByCustomerNumAndName(forwardNum, forwardName, customerNum, legalEntity) == 0)
                                        {
                                            // 不存在则插入
                                            createForwarderListByCustomerNumAndName(customerNum, forwardNum, forwardName, legalEntity);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 若未找到则给出相应提示
                            errorMsg += "Can't find the bankstatement which RV num is " + rv.rvNumber + "\r\n";
                            continue;
                        }

                    }
                }
                SqlHelper.ExcuteListSql(listSQL);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException(ex.Message);
            }

            if (nodataFlag)
            {
                errorMsg += nodataMsg;
            }

            return errorMsg;
        }
        public bool checkInvalidCustomer(string customerName)
        {
            // 判断legalEntity是否属于SAP，若属于则直接置为close
            string countSql = string.Format(@"SELECT COUNT(*) AS COUNT FROM T_SYS_TYPE_DETAIL with (nolock) WHERE TYPE_CODE='091' AND DETAIL_VALUE='{0}'", customerName.Replace("'","''"));
            int count = CommonRep.ExecuteSqlQuery<CountDto>(countSql).ToList()[0].COUNT;
            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }  
        
        public bool checkLocalCustomer(string customerName)
        {
            // 判断legalEntity是否属于SAP，若属于则直接置为close
            string countSql = string.Format(@"SELECT COUNT(*) AS COUNT FROM T_SYS_TYPE_DETAIL with (nolock) WHERE TYPE_CODE='093' AND DETAIL_NAME='{0}'", customerName.Replace("'","''"));
            int count = CommonRep.ExecuteSqlQuery<CountDto>(countSql).ToList()[0].COUNT;
            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public void ignore(string[] bankIds)
        {
            var bankIdStr = "";
            foreach (var id in bankIds)
            {
                bankIdStr += "'" + id + "',";
            }
            if (bankIdStr.Length > 0)
            {
                bankIdStr = bankIdStr.Substring(0, bankIdStr.Length - 1);

                string sql = string.Format(@"UPDATE T_CA_BankStatement SET MATCH_STATUS = 7 WHERE ID in ({0});", bankIdStr);


                SqlHelper.ExcuteSql(sql);
            }
            else
            {
                // 抛出提示信息
                throw new OTCServiceException("Please at least select one Item");
            }

        } 
        
        public void unlock(string[] bankIds)
        {
            var bankIdStr = "";
            foreach (var id in bankIds)
            {
                bankIdStr += "'" + id + "',";
            }
            if (bankIdStr.Length > 0)
            {
                bankIdStr = bankIdStr.Substring(0, bankIdStr.Length - 1);

                string sql = string.Format(@"UPDATE T_CA_BankStatement SET ISLOCKED = 0 WHERE ID in ({0});", bankIdStr);


                SqlHelper.ExcuteSql(sql);
            }
            else
            {
                // 抛出提示信息
                throw new OTCServiceException("Please at least select one Item");
            }

        } 
        
        public void batchDelete(string[] bankIds)
        {
            var bankIdStr = "";
            foreach (var id in bankIds)
            {
                bankIdStr += "'" + id + "',";
            }
            if (bankIdStr.Length > 0)
            {
                bankIdStr = bankIdStr.Substring(0, bankIdStr.Length - 1);

                string sql = string.Format(@"DELETE FROM T_CA_BankStatement WHERE ID in ({0});", bankIdStr);


                SqlHelper.ExcuteSql(sql);
            }
            else
            {
                // 抛出提示信息
                throw new OTCServiceException("Please at least select one Item");
            }

        }
        
        public FileDto doExportUnknownDataByIds(List<CaBankStatementDto> banks)
		{
            FileDto fileDto = new FileDto();
            string filePath = ConfigurationManager.AppSettings["BankStatementPath"].ToString();
            filePath = filePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
            if (Directory.Exists(filePath) == false)
            {
                Directory.CreateDirectory(filePath);
            }
            string fileDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = "UnknowMatched-" + fileDate + ".xlsx";
            fileDto.FileName = fileName;
            filePath = filePath + "\\" + fileName;
            fileDto.PhysicalPath = filePath;
            try
            {
                
                IWorkbook wk = new XSSFWorkbook();

                #region 设置样式
                IFont font = wk.CreateFont();
                font.Boldweight = (short)FontBoldWeight.Bold;

                //ICellStyle headCellStyle = wk.CreateCellStyle();
                //headCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;//设置水平居中
                //headCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;//设置垂直居中
                //headCellStyle.FillPattern = FillPattern.SolidForeground;
                //headCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                //headCellStyle.SetFont(font);

                ICellStyle rowCellStyle = wk.CreateCellStyle();
                IFont delfont = wk.CreateFont();
                delfont.Color = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                rowCellStyle.SetFont(delfont);

                //var amtStyle = wk.CreateCellStyle();
                //IDataFormat format = wk.CreateDataFormat();
                //amtStyle.DataFormat = format.GetFormat("#,##0.00");

                //var amtStyleDel = wk.CreateCellStyle();
                //amtStyleDel.DataFormat = format.GetFormat("#,##0.00");
                //IFont delfontAmt = wk.CreateFont();
                //delfont.Color = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                //amtStyleDel.SetFont(delfontAmt);

                #endregion
                ISheet sheet = wk.CreateSheet("Matched");
                List<string> headersList = new List<string>();
               
                List<UnknownMatchedDto> dataRows = new List<UnknownMatchedDto>();
                int index = 1;
                int no = 1;
                foreach (CaBankStatementDto bs in banks)
                {
                    List<CaUnknowARDto> arlist = getArByBankId(bs.ID);
                    string groupNo = "U" + no.ToString().PadLeft(4).Replace(" ", "0");
                    no++;
                    foreach (CaUnknowARDto ar in arlist)
                    {
                        UnknownMatchedDto ud = new UnknownMatchedDto();
                        ud.No = index.ToString();
                        ud.TransactionNum = bs.TRANSACTION_NUMBER;
                        ud.Date = bs.VALUE_DATE;
                        ud.Description = bs.Description;
                        ud.amount = bs.TRANSACTION_AMOUNT;
                        ud.group_id = groupNo;
                        ud.ar = ar;
                        index++;
                        dataRows.Add(ud);
                    }
                }

                //header
                int columnNum = 0;
                string[] headerStr = { "No","TransactionNum", "Date", "Description", "amount", "group_id", "customer_name",
                    "accnt_number", "amt_remaining", "site_use_id", "selling_location_code", "class", "trx_num", "trx_date",
                    "due_date", "over_credit_lmt", "func_curr_code", "inv_curr_code", "due_days", "amount_wo_vat",
                    "aging_bucket", "payment_term_desc", "selling_location_code2", "ebname", "customertype", "isr",
                    "fsr", "org_id", "cmpinv", "sales_order", "cpo", "eb", "amt_remaining_tran"};
                IRow headRow = sheet.CreateRow(0);
                foreach (string headName in headerStr)
                {
                    ICell cell = headRow.CreateCell(columnNum++);
                    cell.SetCellValue(headName);
                    //cell.CellStyle = headCellStyle;
                }
                
                //data
                int rowNum = 1;
                foreach (UnknownMatchedDto dto in dataRows)
                {
                    IRow dataRow = sheet.CreateRow(rowNum++);
                    columnNum = 0;
                    dataRow.CreateCell(columnNum++).SetCellValue(dto.No);
                    dataRow.CreateCell(columnNum++).SetCellValue(dto.TransactionNum);
                    if (dto.Date == null)
                    {
                        dataRow.CreateCell(columnNum++).SetCellValue("");
                    }
                    else
                    {
                        DateTime date = dto.Date ?? DateTime.Now;
                        dataRow.CreateCell(columnNum++).SetCellValue(date.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                   
                    dataRow.CreateCell(columnNum++).SetCellValue(dto.Description);

                    if (dto.amount == null)
                    {
                        dataRow.CreateCell(columnNum++).SetCellValue("");
                    }
                    else
                    {
                        decimal amt = dto.amount ?? 0;
                        ICell amtCell = dataRow.CreateCell(columnNum++);
                        amtCell.SetCellValue(Convert.ToDouble(amt));
                        //amtCell.CellStyle = amtStyle;
                    }
                    
                    dataRow.CreateCell(columnNum++).SetCellValue(dto.group_id);
                    dataRow.CreateCell(columnNum++).SetCellValue(dto.ar.customer_name);
                    ICell araccnt_number = dataRow.CreateCell(columnNum++);
                    araccnt_number.SetCellValue(dto.ar.accnt_number);
                    if (dto.ar.isValid != 1) { araccnt_number.CellStyle = rowCellStyle; }

                    if (dto.ar.amt_remaining == null)
                    {
                        dataRow.CreateCell(columnNum++).SetCellValue("");
                    }
                    else
                    {
                        decimal amt = dto.ar.amt_remaining ?? 0;
                        ICell amtCell = dataRow.CreateCell(columnNum++);
                        amtCell.SetCellValue(Convert.ToDouble(amt));
                        if (dto.ar.isValid == 1)
                        {
                            //amtCell.CellStyle = amtStyle;
                        }
                        else
                        {
                            //amtCell.CellStyle = amtStyleDel;
                        }
                    }

                    ICell arsite_use_id = dataRow.CreateCell(columnNum++);
                    arsite_use_id.SetCellValue(dto.ar.site_use_id);
                    if (dto.ar.isValid != 1) { arsite_use_id.CellStyle = rowCellStyle; }

                    ICell arselling_location_code = dataRow.CreateCell(columnNum++);
                    arselling_location_code.SetCellValue(dto.ar.selling_location_code);
                    if (dto.ar.isValid != 1) { arselling_location_code.CellStyle = rowCellStyle; }

                    ICell arClass = dataRow.CreateCell(columnNum++);
                    arClass.SetCellValue(dto.ar.Class);
                    if (dto.ar.isValid != 1) { arClass.CellStyle = rowCellStyle; }

                    ICell artrx_num = dataRow.CreateCell(columnNum++);
                    artrx_num.SetCellValue(dto.ar.trx_num);
                    if (dto.ar.isValid != 1) { artrx_num.CellStyle = rowCellStyle; }

                    if (dto.Date == null)
                    {
                        dataRow.CreateCell(columnNum++).SetCellValue("");
                    }
                    else
                    {
                        DateTime date = dto.ar.trx_date ?? DateTime.Now;
                        ICell artrx_date = dataRow.CreateCell(columnNum++);
                        artrx_date.SetCellValue(date.ToString("yyyy-MM-dd HH:mm:ss"));
                        if (dto.ar.isValid != 1) { artrx_date.CellStyle = rowCellStyle; }
                    }
                    
                    if (dto.Date == null)
                    {
                        dataRow.CreateCell(columnNum++).SetCellValue("");
                    }
                    else
                    {
                        DateTime date = dto.ar.due_date ?? DateTime.Now;
                        ICell ardue_date = dataRow.CreateCell(columnNum++);
                        ardue_date.SetCellValue(date.ToString("yyyy-MM-dd HH:mm:ss"));
                        if (dto.ar.isValid != 1) { ardue_date.CellStyle = rowCellStyle; }
                    }

                    if (dto.ar.over_credit_lmt == null)
                    {
                        dataRow.CreateCell(columnNum++).SetCellValue("");
                    }
                    else
                    {
                        decimal amt = dto.ar.over_credit_lmt ?? 0;
                        ICell amtCell = dataRow.CreateCell(columnNum++);
                        amtCell.SetCellValue(Convert.ToDouble(amt));

                        //amtCell.CellStyle = amtStyle;
                    }
                    ICell arfunc_curr_code = dataRow.CreateCell(columnNum++);
                    arfunc_curr_code.SetCellValue(dto.ar.func_curr_code);
                    if (dto.ar.isValid != 1) { arfunc_curr_code.CellStyle = rowCellStyle; }

                    ICell arinv_curr_code = dataRow.CreateCell(columnNum++);
                    arinv_curr_code.SetCellValue(dto.ar.inv_curr_code);
                    if (dto.ar.isValid != 1) { arinv_curr_code.CellStyle = rowCellStyle; }

                    ICell ardue_days = dataRow.CreateCell(columnNum++);
                    ardue_days.SetCellValue(dto.ar.due_days.ToString());
                    if (dto.ar.isValid != 1) { ardue_days.CellStyle = rowCellStyle; }

                    ICell aramount_wo_vat = dataRow.CreateCell(columnNum++);
                    aramount_wo_vat.SetCellValue(Convert.ToDouble(dto.ar.amount_wo_vat));
                    if (dto.ar.isValid != 1) { aramount_wo_vat.CellStyle = rowCellStyle; }

                    ICell araging_bucket = dataRow.CreateCell(columnNum++);
                    araging_bucket.SetCellValue(dto.ar.aging_bucket);
                    if (dto.ar.isValid != 1) { araging_bucket.CellStyle = rowCellStyle; }

                    ICell arpayment_term_desc = dataRow.CreateCell(columnNum++);
                    arpayment_term_desc.SetCellValue(dto.ar.payment_term_desc);
                    if (dto.ar.isValid != 1) { arpayment_term_desc.CellStyle = rowCellStyle; }

                    ICell arselling_location_code2 = dataRow.CreateCell(columnNum++);
                    arselling_location_code2.SetCellValue(dto.ar.selling_location_code2);
                    if (dto.ar.isValid != 1) { arselling_location_code2.CellStyle = rowCellStyle; }

                    ICell arebname = dataRow.CreateCell(columnNum++);
                    arebname.SetCellValue(dto.ar.ebname);
                    if (dto.ar.isValid != 1) { arebname.CellStyle = rowCellStyle; }

                    ICell arcustomertype = dataRow.CreateCell(columnNum++);
                    arcustomertype.SetCellValue(dto.ar.customertype);
                    if (dto.ar.isValid != 1) { arcustomertype.CellStyle = rowCellStyle; }

                    ICell arisr = dataRow.CreateCell(columnNum++);
                    arisr.SetCellValue(dto.ar.isr);
                    if (dto.ar.isValid != 1) { arisr.CellStyle = rowCellStyle; }

                    ICell arfsr = dataRow.CreateCell(columnNum++);
                    arfsr.SetCellValue(dto.ar.fsr);
                    if (dto.ar.isValid != 1) { arfsr.CellStyle = rowCellStyle; }

                    ICell arorg_id = dataRow.CreateCell(columnNum++);
                    arorg_id.SetCellValue(dto.ar.org_id);
                    if (dto.ar.isValid != 1) { arorg_id.CellStyle = rowCellStyle; }

                    ICell arcmpinv = dataRow.CreateCell(columnNum++);
                    arcmpinv.SetCellValue(dto.ar.cmpinv);
                    if (dto.ar.isValid != 1) { arcmpinv.CellStyle = rowCellStyle; }

                    ICell arsales_order = dataRow.CreateCell(columnNum++);
                    arsales_order.SetCellValue(dto.ar.sales_order);
                    if (dto.ar.isValid != 1) { arsales_order.CellStyle = rowCellStyle; }

                    ICell arcpo = dataRow.CreateCell(columnNum++);
                    arcpo.SetCellValue(dto.ar.cpo);
                    if (dto.ar.isValid != 1) { arcpo.CellStyle = rowCellStyle; }

                    ICell areb = dataRow.CreateCell(columnNum++);
                    areb.SetCellValue(dto.ar.eb);
                    if (dto.ar.isValid != 1) { areb.CellStyle = rowCellStyle; }

                    if (dto.ar.amt_remaining_tran == null)
                    {
                        dataRow.CreateCell(columnNum++).SetCellValue("");
                    }
                    else
                    {
                        decimal amt = dto.ar.amt_remaining ?? 0;
                        ICell amtCell = dataRow.CreateCell(columnNum++);
                        amtCell.SetCellValue(Convert.ToDouble(amt));
                        if (dto.ar.isValid == 1)
                        {
                            //amtCell.CellStyle = amtStyle;
                        }
                        else
                        {
                            //amtCell.CellStyle = amtStyleDel;
                        }
                    }
                }


                using (FileStream fs = new FileStream(fileDto.PhysicalPath, FileMode.Create))
                {
                    wk.Write(fs);
                    fs.Close();
                }

                //保存文件
                fileDto.FileId = Guid.NewGuid().ToString();
                StringBuilder sqlFile = new StringBuilder();
                sqlFile.Append("INSERT INTO T_FILE (FILE_ID,FILE_NAME,PHYSICAL_PATH,OPERATOR,CREATE_TIME)");
                sqlFile.Append(" VALUES (N'" + fileDto.FileId + "',");
                sqlFile.Append("         N'" + fileDto.FileName + "',");
                sqlFile.Append("         N'" + fileDto.PhysicalPath + "',");
                sqlFile.Append("         N'" + AppContext.Current.User.EID + "',GETDATE());");
                SqlHelper.ExcuteSql(sqlFile.ToString());
            }
            catch (Exception e)
            {
                Helper.Log.Error(e.Message, e);
                throw new Exception(e.Message);
            }
            
            return fileDto;
        }

        private List<CaUnknowARDto> getArByBankId(string bankId)
        {
            string sql = string.Format(@"SELECT
	                                    v.CUSTOMER_NUM AS accnt_number,
	                                    v.AMT AS amt_remaining,
	                                    v.SiteUseId AS site_use_id,
	                                    v.sellingLocationCode AS selling_location_code,
	                                    v.Class AS class,
                                        (select top 1 customer_name from T_CUSTOMER  with (nolock)
                                            where T_CUSTOMER.CUSTOMER_NUM = v.CUSTOMER_NUM 
                                            and T_CUSTOMER.SiteUseId = v.SiteUseId) as customer_name,
	                                    v.INVOICE_NUM AS trx_num,
	                                    v.INVOICE_DATE AS trx_date,
	                                    v.DUE_DATE AS due_date,
	                                    v.CreditLimit AS over_credit_lmt,
	                                    v.func_currency AS func_curr_code,
	                                    v.INV_CURRENCY AS inv_curr_code,
	                                    v.Duedays AS due_days,
	                                    v.VAT_AMT AS amount_wo_vat,
	                                    v.AgingBucket AS aging_bucket,
	                                    v.PaymentTerm AS payment_term_desc,
	                                    v.SellingLocationCode2 AS selling_location_code2,
	                                    v.Ebname AS ebname,
	                                    v.Ebname AS eb,
	                                    v.CS AS isr,
	                                    v.Sales AS fsr,
	                                    v.LegalEntity AS org_id,
	                                    v.Local_AMT AS amt_remaining_tran,
	                                    v.Cmpinv AS cmpinv,
	                                    v.OrderNumber AS sales_order,
	                                    v.Cpo AS cpo,
										(case when exists (select 1 from V_CA_AR where V_CA_AR.SiteUseId = v.SiteUseId and  V_CA_AR.INVOICE_NUM = v.INVOICE_NUM)
										 then 1 else 0 end
										) as isValid
                                    FROM T_CA_CustomerIdentify with (nolock)
                                    JOIN T_CA_ReconDetail t with (nolock) ON t.ReconId = T_CA_CustomerIdentify.ReconId
									join  V_CA_AR_ALL v with (nolock) ON t.SiteUseId = v.SiteUseId and  t.InvoiceNum = v.INVOICE_NUM
                                    WHERE
									T_CA_CustomerIdentify.BANK_STATEMENT_ID= '{0}'
									 order by T_CA_CustomerIdentify.CUSTOMER_NUM,
									 v.SiteUseId,
									 v.DUE_DATE ,
									 v.INVOICE_DATE", bankId);
            List<CaUnknowARDto> list = SqlHelper.GetList<CaUnknowARDto>(SqlHelper.ExcuteTable(sql, CommandType.Text, null));
            return list;
        }

        public string revert(CaBankStatementDto dto)
        {
            string result = "Operation Successed!";
            CaReconService caReconService = SpringFactory.GetObjectImpl<CaReconService>("CaReconService");

            string reconId = caReconService.getLastReconIdWithOutCloseByBsId(dto.ID);
            if (dto.CLEARING_STATUS == "1" || dto.CLEARING_STATUS == "2")
            {
               
                decimal amt = caReconService.getReconAmtByReconId(reconId);

                string sql1 = string.Format(@"UPDATE T_CA_Recon SET isClosed=0 WHERE ID = '{0}'", reconId);
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql1);
                string sql2 = string.Format(@"UPDATE T_CA_ReconDetail SET isCleared=0 WHERE reconId = '{0}'", reconId);
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql2);

                if ("2".Equals(dto.CLEARING_STATUS))
                {
                    decimal revertAmt = Decimal.Add(dto.CURRENT_AMOUNT ?? Decimal.Zero, amt);
                    int compare = Decimal.Compare(revertAmt, dto.TRANSACTION_AMOUNT ?? Decimal.Zero);
                    if (compare > -1)
                    {
                        dto.CURRENT_AMOUNT = dto.TRANSACTION_AMOUNT;
                    }
                    else
                    {
                        dto.CURRENT_AMOUNT = revertAmt;
                    }
                }

                // todo unclearAmt
                string sql3 = string.Format(@"UPDATE dbo.T_CA_BankStatement
                                                           SET UnClear_Amount = isNull((SELECT SUM(ISNULL(T_CA_ReconDetail.Amount,0)) FROM T_CA_ReconBS with (nolock)
                                                                                   JOIN T_CA_Recon with (nolock) ON T_CA_ReconBS.ReconId = T_CA_Recon.ID
                                                                                   JOIN T_CA_ReconDetail with (nolock) ON T_CA_ReconBS.ReconId = T_CA_ReconDetail.ReconId
                                                                                  WHERE T_CA_ReconBS.BANK_STATEMENT_ID = '{0}' AND T_CA_Recon.isClosed = 0 AND
                                                                                        T_CA_ReconDetail.isCleared = 0),0)
                                                        WHERE id = '{0}'", dto.ID);
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql3);

                dto.CLEARING_STATUS = "0";
                dto.MATCH_STATUS = "4";
                updateBank(dto);
            }
            else if (dto.APPLY_STATUS == "2" || dto.APPLY_STATUS == "1")
            {
                dto.APPLY_STATUS = "0";
                updateBank(dto);
            }
            else if (dto.MATCH_STATUS == "2")
            {
                if (reconId == "") {
                    dto.CUSTOMER_NUM = "";
                    dto.CUSTOMER_NAME = "";
                    dto.FORWARD_NUM = "";
                    dto.FORWARD_NAME = "";
                    dto.SiteUseId = "";
                    dto.MATCH_STATUS = "0";
                    dto.MATCH_STATUS_NAME = "Unknown";
                }
                else {
                    dto.CLEARING_STATUS = "2";
                    dto.MATCH_STATUS = "4";
                }
                updateBank(dto);
            }
            else if (dto.MATCH_STATUS == "0")
            {
                dto.CUSTOMER_NUM = "";
                dto.CUSTOMER_NAME = "";
                dto.FORWARD_NUM = "";
                dto.FORWARD_NAME = "";
                dto.MATCH_STATUS = "0";
                dto.MATCH_STATUS_NAME = "Unknown";
                updateBank(dto);
            }
            else {
                result = "This operation is not available in the current state!";
            }


            return result;
        }

        public void pmtUnknownCashAdvisor(List<string> unknownBsIds,string taskId)
        {
            ICaPaymentDetailService paymentDetailService = SpringFactory.GetObjectImpl<ICaPaymentDetailService>("CaPaymentDetailService");
            CaReconService reconService = SpringFactory.GetObjectImpl<CaReconService>("CaReconService");

            ICaTaskService taskService = SpringFactory.GetObjectImpl<ICaTaskService>("CaTaskService");

            DateTime now = DateTime.Now;

            var bankIdStr = "";
            foreach (var id in unknownBsIds)
            {
                bankIdStr += "'" + id + "',";
            }
            if (bankIdStr.Length > 0)
            {
                bankIdStr = bankIdStr.Substring(0, bankIdStr.Length - 1);

                if (string.IsNullOrEmpty(taskId))
                {
                    taskId = taskService.createTask(9, unknownBsIds.ToArray(), "", "", now);
                }

                // 查找有AR无BS 能转化为Match的数据
                string pmtMatchSql = string.Format(@"SELECT
	                                t1.ID AS pmtId,
	                                t2.ID AS bsId
                                FROM
	                                (
		                                SELECT
			                                ID,
			                                LegalEntity,
			                                Currency,
			                                TransactionAmount,
			                                Amount,
			                                BankCharge,
			                                ValueDate,
			                                GroupNo
		                                FROM
			                                dbo.T_CA_PMT WITH (NOLOCK)
		                                WHERE
			                                ISNULL(T_CA_PMT.CustomerNum, '') <> ''
		                                AND NOT EXISTS (
			                                SELECT
				                                1
			                                FROM
				                                dbo.T_CA_PMTBS WITH (NOLOCK)
			                                WHERE
				                                ReconId = T_CA_PMT.ID
		                                )
		                                AND EXISTS (
			                                SELECT
				                                1
			                                FROM
				                                dbo.T_CA_PMTDetail
			                                WHERE
				                                ReconId = T_CA_PMT.ID
		                                )
		                                AND isClosed = 0
                                        AND not exists(select 1 from T_CA_PMT as a 
										where a.id <> T_CA_PMT.id
										 and a.LegalEntity = T_CA_PMT.LegalEntity
										 and a.ValueDate = T_CA_PMT.ValueDate
										 and a.Currency = T_CA_PMT.Currency
										 and a.Amount = T_CA_PMT.Amount)
	                                ) AS t1
                                INNER JOIN (
	                                SELECT
		                                ID,
		                                LegalEntity,
		                                CURRENCY,
		                                TRANSACTION_AMOUNT,
		                                CURRENT_AMOUNT,
		                                BankChargeFrom,
                                        BankChargeTo,
		                                VALUE_DATE
	                                FROM
		                                T_CA_BankStatement WITH (nolock)
	                                WHERE
		                                (
			                                ISLOCKED <> 1
			                                OR ISLOCKED IS NULL
		                                )
	                                AND (MATCH_STATUS in (0,2))
	                                AND (ISHISTORY <> 1)
	                                AND ID IN ({0})
                                ) AS t2 ON t1.Currency = t2.CURRENCY
                                AND t1.LegalEntity = t2.LegalEntity
                                AND t1.TransactionAmount = t2.TRANSACTION_AMOUNT
                                AND t1.ValueDate = t2.VALUE_DATE
                                AND (
	                                t1.Amount
                                ) <= (
	                                t2.CURRENT_AMOUNT + ISNULL(t2.BankChargeTo, 0)
                                )
                                ORDER BY
	                                t1.GroupNo DESC", bankIdStr);
                List<CaBsIdPmtIdDto> matchList = CommonRep.ExecuteSqlQuery<CaBsIdPmtIdDto>(pmtMatchSql).ToList();

                if(null != matchList && matchList.Count > 0)
                {
                    string prePmtId = "";
                    var bankMap = new Dictionary<string, string>();
                    foreach (CaBsIdPmtIdDto dto in matchList)
                    {
                        if (dto.pmtId.Equals(prePmtId))
                        {
                            continue;
                        }
                        else
                        {
                            // 查找list中若存在多个bsId的解则放弃该解
                            List<CaBsIdPmtIdDto> bsCountlist = matchList.FindAll(a => a.bsId == dto.bsId).ToList<CaBsIdPmtIdDto>();
                            // 判断当一个bank对应多个不同customer时舍弃该解
                            if (bsCountlist.Count > 1)
                            {
                                string preCustomer = "";
                                foreach (CaBsIdPmtIdDto pmtIds in bsCountlist)
                                {
                                    CaPMTDto pmt = paymentDetailService.getPMTById(pmtIds.pmtId);
                                    if (string.IsNullOrEmpty(preCustomer))
                                    {
                                        preCustomer = pmt.CustomerNum;
                                    }
                                    else
                                    {
                                        if (!pmt.CustomerNum.Equals(preCustomer))
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }

                            // 查找list中若存在多个pmtId的解则放弃该解
                            List<CaBsIdPmtIdDto> pmtCountlist = matchList.FindAll(a => a.pmtId == dto.pmtId).ToList<CaBsIdPmtIdDto>();
                            if (pmtCountlist.Count > 1)
                            {
                                continue;
                            }

                            // 查询bank
                            CaBankStatementDto bank = getBankStatementById(dto.bsId);

                            // 判断该bank是否存在相同条件的数据，若存在则舍弃该解
                            List<CaBankStatementDto> bankList = getOpenBankList(bank);
                            if (bankList.Count > 1)
                            {
                                continue;
                            }

                            // 每个bank只操作一次，若有多个解则放弃其他解
                            if (!bankMap.ContainsKey(dto.bsId))
                            {
                                bankMap.Add(dto.bsId, dto.bsId);
                            }
                            else
                            {
                                continue;
                            }

                            prePmtId = dto.pmtId;


                            if (paymentDetailService.checkPMTAvailable(dto.pmtId))
                            {
                                // 查询pmt
                                CaPMTDto pmtDto = paymentDetailService.getPMTById(dto.pmtId);

                                // 若pmt已经关联bank则舍弃该解
                                if(pmtDto.PmtBs.Count > 0)
                                {
                                    continue;
                                }
                                // 将BS插入到PMT中
                                paymentDetailService.savePMTBSByBank(bank, dto.pmtId);

                                // 将组合应用到bank中
                                // 生成group
                                reconService.createReconGroupByBSId(dto.bsId, taskId, dto.pmtId);
                                // 修改状态为matched并解锁
                                bank.MATCH_STATUS = "4";
                                bank.ISLOCKED = false;
                                bank.Comments = "Base On PMT";

                                //为customerNum、CustomerName赋值
                                if (string.IsNullOrEmpty(bank.FORWARD_NUM) && string.IsNullOrEmpty(bank.FORWARD_NAME))
                                {
                                    bank.FORWARD_NUM = pmtDto.CustomerNum;
                                    bank.FORWARD_NAME = pmtDto.CustomerName;
                                }
                                bank.CUSTOMER_NUM = pmtDto.CustomerNum;
                                bank.CUSTOMER_NAME = pmtDto.CustomerName;
                                bank.SiteUseId = pmtDto.SiteUseId;
                                updateBank(bank);
                            }
                        }
                    }
                }

                // 查找无AR无BS 能转化为Match的数据
                string pmtUnMatchSql = string.Format(@"SELECT
	                                t1.ID AS pmtId,
	                                t2.ID AS bsId
                                FROM
	                                (
		                                SELECT
			                                ID,
			                                LegalEntity,
			                                Currency,
			                                TransactionAmount,
			                                Amount,
			                                BankCharge,
			                                ValueDate,
			                                GroupNo
		                                FROM
			                                dbo.T_CA_PMT WITH (NOLOCK)
		                                WHERE
			                                ISNULL(T_CA_PMT.CustomerNum, '') <> ''
		                                AND NOT EXISTS (
			                                SELECT
				                                1
			                                FROM
				                                dbo.T_CA_PMTBS WITH (NOLOCK)
			                                WHERE
				                                ReconId = T_CA_PMT.ID
		                                )
		                                AND NOT EXISTS (
			                                SELECT
				                                1
			                                FROM
				                                dbo.T_CA_PMTDetail
			                                WHERE
				                                ReconId = T_CA_PMT.ID
		                                )
		                                AND isClosed = 0
	                                ) AS t1
                                INNER JOIN (
	                                SELECT
		                                ID,
		                                LegalEntity,
		                                CURRENCY,
		                                TRANSACTION_AMOUNT,
		                                CURRENT_AMOUNT,
		                                BankChargeFrom,
		                                VALUE_DATE
	                                FROM
		                                T_CA_BankStatement WITH (nolock)
	                                WHERE
		                                (
			                                ISLOCKED <> 1
			                                OR ISLOCKED IS NULL
		                                )
	                                AND (MATCH_STATUS = 0)
	                                AND (ISHISTORY <> 1)
	                                AND ID IN ({0})
                                ) AS t2 ON t1.Currency = t2.CURRENCY
                                AND t1.LegalEntity = t2.LegalEntity
                                AND t1.TransactionAmount = t2.TRANSACTION_AMOUNT
                                AND t1.ValueDate = t2.VALUE_DATE
                                ORDER BY
	                                t1.GroupNo DESC", bankIdStr);

                List<CaBsIdPmtIdDto> unMatchList = CommonRep.ExecuteSqlQuery<CaBsIdPmtIdDto>(pmtUnMatchSql).ToList();

                if (null != unMatchList && unMatchList.Count > 0)
                {
                    string prePmtId = "";
                    var bankMap = new Dictionary<string, string>();
                    foreach (CaBsIdPmtIdDto dto in unMatchList)
                    {
                        if (dto.pmtId.Equals(prePmtId))
                        {
                            continue;
                        }
                        else
                        {
                            // 查找list中若存在多个bsId的解则放弃该解
                            List<CaBsIdPmtIdDto> bsCountlist = unMatchList.FindAll(a => a.bsId == dto.bsId).ToList<CaBsIdPmtIdDto>();
                            // 判断当一个bank对应多个不同customer时舍弃该解
                            if (bsCountlist.Count > 1)
                            {
                                string preCustomer = "";
                                foreach(CaBsIdPmtIdDto pmtIds in bsCountlist)
                                {
                                    CaPMTDto pmt = paymentDetailService.getPMTById(pmtIds.pmtId);
                                    if (string.IsNullOrEmpty(preCustomer))
                                    {
                                        preCustomer = pmt.CustomerNum;
                                    }
                                    else
                                    {
                                        if (!pmt.CustomerNum.Equals(preCustomer))
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }

                            // 查找list中若存在多个pmtId的解则放弃该解
                            List<CaBsIdPmtIdDto> pmtCountlist = unMatchList.FindAll(a => a.pmtId == dto.pmtId).ToList<CaBsIdPmtIdDto>();
                            if (pmtCountlist.Count > 1)
                            {
                                continue;
                            }

                            // 查询bank
                            CaBankStatementDto bank = getBankStatementById(dto.bsId);

                            // 判断该bank是否存在相同条件的数据，若存在则舍弃该解
                            List<CaBankStatementDto> bankList = getOpenBankList(bank);
                            if(bankList.Count > 1)
                            {
                                continue;
                            }

                            // 每个bank只操作一次，若有多个解则放弃其他解
                            if (!bankMap.ContainsKey(dto.bsId))
                            {
                                bankMap.Add(dto.bsId, dto.bsId);
                            }
                            else
                            {
                                continue;
                            }

                            prePmtId = dto.pmtId;

                            // 查询pmt
                            CaPMTDto pmtDto = paymentDetailService.getPMTById(dto.pmtId);

                            // 将BS插入到PMT中
                            paymentDetailService.savePMTBSByBank(bank, dto.pmtId);

                            bank.ISLOCKED = false;
                            bank.Comments = "Advised by PMT("+ pmtDto.GroupNo + ")";

                            //为customerNum、CustomerName赋值
                            if (string.IsNullOrEmpty(bank.FORWARD_NUM) && string.IsNullOrEmpty(bank.FORWARD_NAME))
                            {
                                bank.FORWARD_NUM = pmtDto.CustomerNum;
                                bank.FORWARD_NAME = pmtDto.CustomerName;
                            }
                            bank.CUSTOMER_NUM = pmtDto.CustomerNum;
                            bank.CUSTOMER_NAME = pmtDto.CustomerName;
                            bank.SiteUseId = "";
                            // 修改状态为matched并解锁
                            bank.MATCH_STATUS = "2";
                            updateBank(bank);
                        }
                    }
                }
            }
        }

        public List<CaBankStatementDto> getOpenBankList(CaBankStatementDto bank)
        {
            string sql = string.Format(@"SELECT
	                                *
                                FROM
	                                T_CA_BankStatement
                                WHERE
	                                LegalEntity = N'{0}'
                                AND CURRENCY = N'{1}'
                                AND VALUE_DATE = '{2}'
                                AND TRANSACTION_AMOUNT = {3}
                                AND CURRENT_AMOUNT = {4}
                                AND BankChargeFrom={5}
                                AND BankChargeTo={6}
                                AND MATCH_STATUS IN ('-1','0','2','4')
                                AND ISHISTORY<>'1'", bank.LegalEntity,bank.CURRENCY,bank.VALUE_DATE,bank.TRANSACTION_AMOUNT,bank.CURRENT_AMOUNT,bank.BankChargeFrom,bank.BankChargeTo);

            List<CaBankStatementDto> list = CommonRep.ExecuteSqlQuery<CaBankStatementDto>(sql).ToList();
            return list ?? new List<CaBankStatementDto>();
        }

        public void cancelCaMailAlertbyid(string id) {
            string sql = string.Format("Update t_ca_mailalert set status = 'Canceled' where id = '{0}' and status = 'Initialized'", id);
            SqlHelper.ExcuteSql(sql);
        }
    }

}