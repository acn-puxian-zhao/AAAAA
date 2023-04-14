using ICSharpCode.SharpZipLib.Zip;
using Intelligent.OTC.Business.Collection;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using NPOI.SS.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Intelligent.OTC.Business
{
    public class MyinvoicesService
    {
        public XcceleratorService XccService { get; set; }

        #region Parameters
        public string CurrentDeal
        {
            get
            {
                return AppContext.Current.User.Deal.ToString();
            }
        }
        public string CurrentUser
        {
            get
            {
                return AppContext.Current.User.EID.ToString();
            }
        }
        public DateTime CurrentTime
        {
            get
            {
                return AppContext.Current.User.Now;
            }
        }
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

        public OTCRepository CommonRep { get; set; }

        #endregion

        /* FuJie.Wan  2018-12-13
         * 描述：增加以下方法，替换GetMyinvoicesList，
         *      改为直接用SQL方式查询，避免LINQ慢的问题
         */
        public PageResultDto<MyinvoicesDto> GetMyinvoicesListNoLinq(bool lb_isExport, int pageindex, int pagesize, string custCode, string custName, string eb, string consignmentNumber, string balanceMemo, string memoExpirationDate,
            string legal, string siteUseid, string invoiceNum, string poNum, string soNum,
            string creditTerm, string docuType, string invoiceTrackStates, string memo,
            string ptpDateF, string ptpDateT, string memoDateF, string memoDateT, string invoiceDateF, string invoiceDateT, string dueDateF, string dueDateT, string cs, string sales, string overdueReason)
        {
            PageResultDto<MyinvoicesDto> result = new PageResultDto<MyinvoicesDto>();

            if (invoiceTrackStates == "014")
            {
                result = GetMyinvoicesListNoLinqWithHis(lb_isExport, pageindex, pagesize, custCode, custName, eb, consignmentNumber, balanceMemo, memoExpirationDate,
                legal, siteUseid, invoiceNum, poNum, soNum,
                creditTerm, docuType, invoiceTrackStates, memo,
                ptpDateF, ptpDateT, memoDateF, memoDateT, invoiceDateF, invoiceDateT, dueDateF, dueDateT, cs, sales, overdueReason);
            }
            else {
                result = GetMyinvoicesListNoLinqNoHis(lb_isExport, pageindex, pagesize, custCode, custName, eb, consignmentNumber, balanceMemo, memoExpirationDate,
                legal, siteUseid, invoiceNum, poNum, soNum,
                creditTerm, docuType, invoiceTrackStates, memo,
                ptpDateF, ptpDateT, memoDateF, memoDateT, invoiceDateF, invoiceDateT, dueDateF, dueDateT, cs, sales, overdueReason);
            }

            return result;
        }

        public PageResultDto<MyinvoicesDto> GetMyinvoicesListNoLinqNoHis(bool lb_isExport, int pageindex, int pagesize, string custCode, string custName, string eb, string consignmentNumber, string balanceMemo, string memoExpirationDate,
            string legal, string siteUseid, string invoiceNum, string poNum, string soNum,
            string creditTerm, string docuType, string invoiceTrackStates, string memo,
            string ptpDateF, string ptpDateT, string memoDateF, string memoDateT, string invoiceDateF, string invoiceDateT, string dueDateF, string dueDateT, string cs, string sales, string overdueReason) {

            PageResultDto<MyinvoicesDto> result = new PageResultDto<MyinvoicesDto>();

            //获得用户及其下级管理的所有用户
            List<SysUser> listUser = new List<SysUser>();
            listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
            listUser.Add(new SysUser { });
            string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
            string collecotrList = "";
            int i = 0;
            foreach (string u in userGroup)
            {
                if (u == null || string.IsNullOrEmpty(u)) { continue; }
                i++;
                if (i > 1)
                {
                    collecotrList += ",'" + u + "'";
                }
                else
                {
                    collecotrList += "'" + u + "'";
                }
            }

            StringBuilder sbselect = new StringBuilder();
            StringBuilder sbselectCount = new StringBuilder();
            StringBuilder sbfrom = new StringBuilder();
            StringBuilder sbCountfrom = new StringBuilder();
            StringBuilder sbOderby = new StringBuilder();
            StringBuilder sbPage = new StringBuilder();
            StringBuilder sbDetailCount = new StringBuilder();
            StringBuilder sbDetailBalanceCount = new StringBuilder();
            sbDetailCount.Append("(SELECT COUNT(*) FROM T_Invoice_Detail AS Detail WITH (NOLOCK) WHERE Detail.InvoiceNumber = invAgeing.INVOICE_NUM) ");
            sbDetailBalanceCount.Append(" (SELECT COUNT(*) FROM T_Invoice_Detail AS Detail WITH (NOLOCK) WHERE Detail.InvoiceNumber = invAgeing.INVOICE_NUM AND ISNULL(Detail.BalanceStatus,'') = N'已对账') ");
            sbselect.Append(@" SELECT invAgeing.ID AS Id,
                               invAgeing.DEAL AS Deal,
                               invAgeing.LEGAL_ENTITY AS LegalEntity,
                               invAgeing.CUSTOMER_NUM AS CustomerNum,
                               CustomerTeam.CUSTOMER_NAME AS CustomerName,
                               invAgeing.INVOICE_NUM AS InvoiceNum,
                               CustomerTeam.BILL_GROUP_CODE AS GroupCodeOld,
                               CustomerTeam.BILL_GROUP_NAME AS GroupNameOld,
                               invAgeing.MST_CUSTOMER AS MstCustomer,
                               invAgeing.PO_MUM AS PoNum,
                               invAgeing.SO_NUM AS SoNum,
                               invAgeing.CLASS AS Class,
                               invAgeing.CURRENCY AS Currency,
                               invAgeing.CUSTOMER_COUNTRY AS CustomerCountry,
                               invAgeing.STATES AS States,
                               invAgeing.ORIGINAL_AMT AS OriginalAmt,
                               invAgeing.BALANCE_AMT AS BalanceAmt,
                               invAgeing.CREDIT_TREM AS CreditTrem,
                               invAgeing.TRACK_STATES AS TrackStates,
                               invAgeing.INVOICE_DATE AS InvoiceDate,
                               invAgeing.DUE_DATE AS DueDate,
                               invAgeing.CREATE_DATE AS CreateDate,
                               invAgeing.UPDATE_DATE AS UpdateDate,
                               invAgeing.PTP_DATE AS PtpDate,
                               invAgeing.OverdueReason AS OverdueReason,
                                (case when isnull(invAgeing.NotClear,0) = 1 then 'Lock' else '' end) as NotClear,
                               invAgeing.REMARK AS Remark,
                               invAgeing.LsrNameHist AS CS,
                               invAgeing.FsrNameHist AS Sales,
                                STUFF((SELECT ';'+ A.NAME FROM  dbo.T_CONTACTOR AS a  WHERE A.SiteUseId = invAgeing.SiteUseId AND a.TITLE IN ('Branch Manager','Sales Manager','Finance Manager') for xml path('')),1,1,'') AS BranchSalesFinance,
                                CustomerTeam.COLLECTOR AS Collector,
                               CustomerTeam.COLLECTOR_NAME AS CollectorName,
                               CustomerTeam.TEAM_NAME AS TeamName,
                               invAgeing.COMMENTS AS Comments,
                               invAgeing.SiteUseId AS SiteUseId,
                               CONTACTOR.CONTACT AS COLLECTOR_CONTACT,
                               invAgeing.DAYS_LATE_SYS AS DAYS_LATE_SYS,
                               invAgeing.AgingBucket AS AgingBucket,
                               invAgeing.Ebname AS Ebname,
                               invAgeing.ConsignmentNumber AS ConsignmentNumber,
                               invAgeing.BalanceMemo AS BalanceMemo,
                               invAgeing.MemoExpirationDate AS MemoExpirationDate,
                               (CASE WHEN invAgeing.MemoExpirationDate IS NOT NULL AND invAgeing.MemoExpirationDate <= CONVERT(DATETIME,GETDATE()) THEN  1  ELSE  0 END) AS IsExp,
                               invAgeing.TRACK_DATE AS TRACK_DATE,
                               invAgeing.PTP_DATE AS PTP_DATE,
                               CustomerTeam.COLLECTOR_NAME AS COLLECTOR_NAME,
                               VAT.VATInvoice AS VAT_NO,
                               VAT.VATInvoiceDate AS VAT_DATE,
                               (CASE WHEN invAgeing.FinishedStatus = '0' THEN 'No'  ELSE  'Yes'  END) AS FinishedStatus,
                               invAgeing.Payment_Date AS Paytment_Date,
                               (CASE WHEN DISP.ID IS NULL THEN  'N'  ELSE  'Y' END) AS Dispute,
                               (CASE WHEN DISP.ID IS NULL THEN  NULL  ELSE  DISP.CREATE_DATE  END ) AS DisputeIdentifiedDate,
                               (CASE WHEN DISP.ID IS NULL THEN NULL ELSE DISPUTEREASON_TYPE.DETAIL_NAME END  ) AS DisputeReason,
                               (CASE WHEN DISP.ID IS NULL THEN NULL ELSE Department_TYPE.DETAIL_NAME END ) AS ActionOwnerDepartment,
                               (CASE WHEN DISP.ID IS NULL THEN NULL ELSE ");
            sbselect.Append(" DATEADD(\"day\", 7, DISP.CREATE_DATE)");
            sbselect.Append(@"   END) AS NextActionDate,
                           (CASE WHEN DISP.ID IS NULL THEN NULL ELSE DISPUTESTATUS_TYPE.DETAIL_NAME END) AS DisputeStatus,
                           (CASE WHEN DISP.ID IS NULL THEN NULL ELSE DISP.COMMENTS END ) AS DisputeComment,
                           (CASE WHEN PTP.Id IS NULL THEN NULL ELSE PTP.CreateTime END) AS PtpIdentifiedDate,
                           (CASE WHEN PTP.Id IS NULL THEN NULL ELSE PTP.Comments END) AS PtpComment,
                           invAgeing.Forwarder AS Forwarder,
                           (CASE WHEN invAgeing.IsForwarder = 1 THEN 'Yes' ELSE 'No' END) AS IsForwarder,");
            sbselect.Append(@" (case when isnull(invAgeing.BalanceStatus,'') = N'已对账' then isnull(invAgeing.BalanceStatus,'') else
                                (case when " + sbDetailCount.ToString() + " = 0 or " + sbDetailBalanceCount.ToString() + @" = 0
                                      then '' else ( case when " + sbDetailCount.ToString() + " = " + sbDetailBalanceCount.ToString() + @" 
                                                          then N'已对账' else N'部分对账'  end) end ) end) AS isBanlance");


            sbselectCount.Append(" SELECT COUNT(*) ");

            sbfrom.Append(@" FROM T_INVOICE_AGING AS invAgeing WITH (NOLOCK)
                        LEFT JOIN V_CUSTOMER_TEAM AS CustomerTeam WITH (NOLOCK)
                            ON invAgeing.SiteUseId = CustomerTeam.SiteUseId
                        LEFT JOIN V_PTPPayment AS PTP WITH (NOLOCK)
                            ON PTP.SiteUseId = CustomerTeam.SiteUseId
                               AND PTP.InvoiceId = invAgeing.ID
                               AND invAgeing.PTP_DATE IS NOT NULL
                        LEFT JOIN
                        (
                            SELECT DISPUTE.ID AS ID,
                                   DISPUTE.CUSTOMER_NUM AS CUSTOMER_NUM,
                                   DISPUTE.SiteUseId AS SiteUseId,
                                   DISPUTE.CREATE_DATE AS CREATE_DATE,
                                   DISPUTE.ISSUE_REASON AS ISSUE_REASON,
                                   DISPUTE.COMMENTS AS COMMENTS,
                                   DISPUTE.ActionOwnerDepartmentCode AS ActionOwnerDepartmentCode,
                                   DISPUTE.STATUS AS STATUS,
                                   DISPUTEINVOICE.INVOICE_ID AS INVOICE_ID
                            FROM dbo.T_DISPUTE AS DISPUTE WITH (NOLOCK)
                                JOIN
                                (
                                    SELECT MAX(DISPUTE_ID) AS DISPUTE_ID,
                                           INVOICE_ID
                                    FROM dbo.T_DISPUTE_INVOICE WITH (NOLOCK)
                                    GROUP BY INVOICE_ID
                                ) AS DISPUTEINVOICE
                                    ON DISPUTE.ID = DISPUTEINVOICE.DISPUTE_ID
                            WHERE DISPUTE.STATUS NOT IN('026011', '026012')
                        ) AS DISP
                            ON DISP.SiteUseId = invAgeing.SiteUseId
                               AND DISP.INVOICE_ID = invAgeing.INVOICE_NUM
                               AND invAgeing.TRACK_STATES IN( '007', '008', '009', '011', '012' )
                        LEFT JOIN V_CUSTOMER_CONTACTOR AS CONTACTOR WITH (NOLOCK)
                            ON CONTACTOR.SiteUseId = invAgeing.SiteUseId
                        LEFT JOIN T_INVOICE_VAT AS VAT WITH (NOLOCK)
                            ON VAT.Trx_Number = invAgeing.INVOICE_NUM
                               AND VAT.LineNumber = 1
                        LEFT JOIN T_SYS_TYPE_DETAIL AS TRACKSTATE_TYPE WITH (NOLOCK)
                            ON TRACKSTATE_TYPE.TYPE_CODE = '029'
                               AND TRACKSTATE_TYPE.DETAIL_VALUE = invAgeing.TRACK_STATES
                        LEFT JOIN T_SYS_TYPE_DETAIL AS DISPUTEREASON_TYPE WITH (NOLOCK)
                            ON DISPUTEREASON_TYPE.TYPE_CODE = '025'
                               AND DISPUTEREASON_TYPE.DETAIL_VALUE = DISP.ISSUE_REASON
                        LEFT JOIN T_SYS_TYPE_DETAIL AS Department_TYPE WITH (NOLOCK)
                            ON Department_TYPE.TYPE_CODE = '038'
                               AND Department_TYPE.DETAIL_VALUE = DISP.ActionOwnerDepartmentCode
                        LEFT JOIN T_SYS_TYPE_DETAIL AS DISPUTESTATUS_TYPE WITH (NOLOCK)
                            ON DISPUTESTATUS_TYPE.TYPE_CODE = '026'
                               AND DISPUTESTATUS_TYPE.DETAIL_VALUE = DISP.STATUS
                            WHERE invAgeing.DEAL = '" + CurrentDeal + "'");
            sbCountfrom.Append(@" FROM T_INVOICE_AGING AS invAgeing WITH (NOLOCK)
                        LEFT JOIN V_CUSTOMER_TEAM AS CustomerTeam WITH (NOLOCK)
                            ON invAgeing.SiteUseId = CustomerTeam.SiteUseId
                            WHERE invAgeing.DEAL = '" + CurrentDeal + "'");
            ArrayList list = new ArrayList();
            if (SupervisorPermission != 1)
            {
                sbfrom.Append(@" AND (CustomerTeam.COLLECTOR IN (" + collecotrList + "))");
                sbCountfrom.Append(@" AND (CustomerTeam.COLLECTOR IN (" + collecotrList + "))");
            }
            if (!string.IsNullOrEmpty(custCode) && custCode != "null" && custCode != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.CUSTOMER_NUM like @custCode ) ");
                sbCountfrom.Append(@" and (invAgeing.CUSTOMER_NUM like @custCode ) ");
                SqlParameter param = new SqlParameter("@custCode", "%" + custCode + "%");
                list.Add(param);

            }
            if (!string.IsNullOrEmpty(custName) && custName != "null" && custName != "undefined")
            {
                sbfrom.Append(@" and (CustomerTeam.CUSTOMER_NAME like @custName )");
                sbCountfrom.Append(@" and (CustomerTeam.CUSTOMER_NAME like @custName )");
                SqlParameter param = new SqlParameter("@custName", "%" + custName + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(eb) && eb != "null" && eb != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.Ebname = @eb) ");
                sbCountfrom.Append(@" and (invAgeing.Ebname = @eb) ");
                SqlParameter param = new SqlParameter("@eb", eb);
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(consignmentNumber) && consignmentNumber != "null" && consignmentNumber != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.ConsignmentNumber = @consignmentNumber) ");
                sbCountfrom.Append(@" and (invAgeing.ConsignmentNumber = @consignmentNumber) ");
                SqlParameter param = new SqlParameter("@consignmentNumber", consignmentNumber);
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(balanceMemo) && balanceMemo != "null" && balanceMemo != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.balanceMemo = @balanceMemo) ");
                sbCountfrom.Append(@" and (invAgeing.balanceMemo = @balanceMemo) ");
                SqlParameter param = new SqlParameter("@balanceMemo", balanceMemo);
                list.Add(param);
            }

            if (!string.IsNullOrEmpty(memoExpirationDate) && memoExpirationDate != "undefined" && memoExpirationDate != "null")
            {
                sbfrom.Append(@" and (invAgeing.MemoExpirationDate <= @memoExpirationDate) ");
                sbCountfrom.Append(@" and (invAgeing.MemoExpirationDate <= @memoExpirationDate) ");
                SqlParameter param = new SqlParameter("@memoExpirationDate", memoExpirationDate + " 23:59:59");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(legal) && legal != "null" && legal != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.LEGAL_ENTITY = @legal) ");
                sbCountfrom.Append(@" and (invAgeing.LEGAL_ENTITY = @legal) ");
                SqlParameter param = new SqlParameter("@legal", legal);
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(siteUseid) && siteUseid != "null" && siteUseid != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.SiteUseId like @siteUseid) ");
                sbCountfrom.Append(@" and (invAgeing.SiteUseId like @siteUseid) ");
                SqlParameter param = new SqlParameter("@siteUseid", "%" + siteUseid + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(invoiceNum) && invoiceNum != "null" && invoiceNum != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.INVOICE_NUM like @invoiceNum) ");
                sbCountfrom.Append(@" and (invAgeing.INVOICE_NUM like @invoiceNum) ");
                SqlParameter param = new SqlParameter("@invoiceNum", "%" + invoiceNum + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(poNum) && poNum != "null" && poNum != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.PO_MUM like @poNum) ");
                sbCountfrom.Append(@" and (invAgeing.PO_MUM like @poNum) ");
                SqlParameter param = new SqlParameter("@poNum", "%" + poNum + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(soNum) && soNum != "null" && soNum != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.SO_NUM like @soNum) ");
                sbCountfrom.Append(@" and (invAgeing.SO_NUM like @soNum) ");
                SqlParameter param = new SqlParameter("@soNum", "%" + soNum + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(creditTerm) && creditTerm != "null" && creditTerm != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.CREDIT_TREM like @creditTerm) ");
                sbCountfrom.Append(@" and (invAgeing.CREDIT_TREM like @creditTerm) ");
                SqlParameter param = new SqlParameter("@creditTerm", "%" + creditTerm + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(cs) && cs != "null" && cs != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.LsrNameHist  like @cs) ");
                sbCountfrom.Append(@" and (invAgeing.LsrNameHist  like @cs) ");
                SqlParameter param = new SqlParameter("@cs", "%" + cs + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(sales) && sales != "null" && sales != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.FsrNameHist  like @sales) ");
                sbCountfrom.Append(@" and (invAgeing.FsrNameHist  like @sales) ");
                SqlParameter param = new SqlParameter("@sales", "%" + sales + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(overdueReason) && overdueReason != "null" && overdueReason != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.overdueReason like @overdueReason) ");
                sbCountfrom.Append(@" and (invAgeing.overdueReason like @overdueReason) ");
                SqlParameter param = new SqlParameter("@overdueReason", "%" + overdueReason + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(docuType) && docuType != "null" && docuType != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.CLASS = @docuType) ");
                sbCountfrom.Append(@" and (invAgeing.CLASS = @docuType) ");
                SqlParameter param = new SqlParameter("@docuType", docuType);
                list.Add(param);
            }
            if (invoiceTrackStates == "null" || invoiceTrackStates == "undefined" || string.IsNullOrWhiteSpace(invoiceTrackStates))
            {
                invoiceTrackStates = "000";
            }
            if (invoiceTrackStates == "000")
            {
                sbfrom.Append(@" AND invAgeing.TRACK_STATES != '013'
                             AND invAgeing.TRACK_STATES != '014'
                             AND invAgeing.TRACK_STATES != '016' ");
                sbCountfrom.Append(@" AND invAgeing.TRACK_STATES != '013'
                             AND invAgeing.TRACK_STATES != '014'
                             AND invAgeing.TRACK_STATES != '016' ");
            }
            else
            {
                sbfrom.Append(@" AND invAgeing.TRACK_STATES = @invoiceTrackStates");
                sbCountfrom.Append(@" AND invAgeing.TRACK_STATES = @invoiceTrackStates");
                SqlParameter param = new SqlParameter("@invoiceTrackStates", invoiceTrackStates);
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(memo) && memo != "null" && memo != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.COMMENTS like @memo) ");
                sbCountfrom.Append(@" and (invAgeing.COMMENTS like @memo) ");
                SqlParameter param = new SqlParameter("@memo", "%" + memo + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(ptpDateF) && ptpDateF != "undefined" && ptpDateF != "null")
            {
                sbfrom.Append(@" and (invAgeing.PTP_DATE >= @ptpDateF) ");
                sbCountfrom.Append(@" and (invAgeing.PTP_DATE >= @ptpDateF) ");
                SqlParameter param = new SqlParameter("@ptpDateF", ptpDateF + " 00:00:00");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(ptpDateT) && ptpDateT != "undefined" && ptpDateT != "null")
            {
                sbfrom.Append(@" and (invAgeing.PTP_DATE <= @ptpDateT) ");
                sbCountfrom.Append(@" and (invAgeing.PTP_DATE <= @ptpDateT) ");
                SqlParameter param = new SqlParameter("@ptpDateT", ptpDateT + " 23:59:59");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(memoDateF) && memoDateF != "undefined" && memoDateF != "null")
            {
                sbfrom.Append(@" and (invAgeing.MemoExpirationDate >= @memoDateF) ");
                sbCountfrom.Append(@" and (invAgeing.MemoExpirationDate >= @memoDateF) ");
                SqlParameter param = new SqlParameter("@memoDateF", memoDateF + " 00:00:00");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(memoDateT) && memoDateT != "undefined" && memoDateT != "null")
            {
                sbfrom.Append(@" and (invAgeing.MemoExpirationDate <= @memoDateT) ");
                sbCountfrom.Append(@" and (invAgeing.MemoExpirationDate <= @memoDateT) ");
                SqlParameter param = new SqlParameter("@memoDateT", memoDateT + " 23:59:59");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(invoiceDateF) && invoiceDateF != "undefined" && invoiceDateF != "null")
            {
                sbfrom.Append(@" and (invAgeing.INVOICE_DATE >= @invoiceDateF) ");
                sbCountfrom.Append(@" and (invAgeing.INVOICE_DATE >= @invoiceDateF) ");
                SqlParameter param = new SqlParameter("@invoiceDateF", invoiceDateF + " 00:00:00");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(invoiceDateT) && invoiceDateT != "undefined" && invoiceDateT != "null")
            {
                sbfrom.Append(@" and (invAgeing.INVOICE_DATE <= @invoiceDateT) ");
                sbCountfrom.Append(@" and (invAgeing.INVOICE_DATE <= @invoiceDateT) ");
                SqlParameter param = new SqlParameter("@invoiceDateT", invoiceDateT + " 23:59:59");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(dueDateF) && dueDateF != "undefined" && dueDateF != "null")
            {
                sbfrom.Append(@" and (invAgeing.DUE_DATE >= @dueDateF) ");
                sbCountfrom.Append(@" and (invAgeing.DUE_DATE >= @dueDateF) ");
                SqlParameter param = new SqlParameter("@dueDateF", dueDateF + " 00:00:00");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(dueDateT) && dueDateT != "undefined" && dueDateT != "null")
            {
                sbfrom.Append(@" and (invAgeing.DUE_DATE <= @dueDateT) ");
                sbCountfrom.Append(@" and (invAgeing.DUE_DATE <= @dueDateT) ");
                SqlParameter param = new SqlParameter("@dueDateT", dueDateT + " 23:59:59");
                list.Add(param);
            }
            SqlParameter[] paramForSQL = new SqlParameter[list.Count];
            int intParam = 0;
            foreach (var item in list)
            {
                paramForSQL[intParam] = (SqlParameter)item;
                intParam++;
            }

            sbOderby.Append(@" order by IsExp desc ,
                                      invAgeing.DEAL,
                                      invAgeing.LEGAL_ENTITY,
                                      invAgeing.CUSTOMER_NUM,
							          invAgeing.SiteUseId,
							          invAgeing.INVOICE_NUM ");
            if (!lb_isExport)
            {
                sbPage.Append(@" offset " + (pageindex - 1) * pagesize + " rows fetch next " + pagesize + " rows only");
            }

            DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString() + sbfrom.ToString() + sbOderby.ToString() + sbPage.ToString(), System.Data.CommandType.Text, paramForSQL);
            List<MyinvoicesDto> tdd = SqlHelper.GetList<MyinvoicesDto>(dt);

            result.dataRows = tdd;
            result.count = SqlHelper.ExcuteScalar<int>(sbselectCount.ToString() + sbCountfrom.ToString(), paramForSQL);

            return result;
        }

        public PageResultDto<MyinvoicesDto> GetMyinvoicesListNoLinqWithHis(bool lb_isExport, int pageindex, int pagesize, string custCode, string custName, string eb, string consignmentNumber, string balanceMemo, string memoExpirationDate,
            string legal, string siteUseid, string invoiceNum, string poNum, string soNum,
            string creditTerm, string docuType, string invoiceTrackStates, string memo,
            string ptpDateF, string ptpDateT, string memoDateF, string memoDateT, string invoiceDateF, string invoiceDateT, string dueDateF, string dueDateT, string cs, string sales, string overdueReason)
        {
            PageResultDto<MyinvoicesDto> result = new PageResultDto<MyinvoicesDto>();

            //获得用户及其下级管理的所有用户
            List<SysUser> listUser = new List<SysUser>();
            listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
            listUser.Add(new SysUser { });
            string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
            string collecotrList = "";
            int i = 0;
            foreach (string u in userGroup)
            {
                if (u == null || string.IsNullOrEmpty(u)) { continue; }
                i++;
                if (i > 1)
                {
                    collecotrList += ",'" + u + "'";
                }
                else
                {
                    collecotrList += "'" + u + "'";
                }
            }

            StringBuilder sbselect = new StringBuilder();
            StringBuilder sbselectCount = new StringBuilder();
            StringBuilder sbfrom = new StringBuilder();
            StringBuilder sbCountfrom = new StringBuilder();
            StringBuilder sbOderby = new StringBuilder();
            StringBuilder sbPage = new StringBuilder();
            StringBuilder sbDetailCount = new StringBuilder();
            StringBuilder sbDetailBalanceCount = new StringBuilder();
            sbDetailCount.Append("(SELECT COUNT(*) FROM T_Invoice_Detail AS Detail WITH (NOLOCK) WHERE Detail.InvoiceNumber = invAgeing.INVOICE_NUM) ");
            sbDetailBalanceCount.Append(" (SELECT COUNT(*) FROM T_Invoice_Detail AS Detail WITH (NOLOCK) WHERE Detail.InvoiceNumber = invAgeing.INVOICE_NUM AND ISNULL(Detail.BalanceStatus,'') = N'已对账') ");
            sbselect.Append(@" SELECT invAgeing.ID AS Id,
                               invAgeing.DEAL AS Deal,
                               invAgeing.LEGAL_ENTITY AS LegalEntity,
                               invAgeing.CUSTOMER_NUM AS CustomerNum,
                               CustomerTeam.CUSTOMER_NAME AS CustomerName,
                               invAgeing.INVOICE_NUM AS InvoiceNum,
                               CustomerTeam.BILL_GROUP_CODE AS GroupCodeOld,
                               CustomerTeam.BILL_GROUP_NAME AS GroupNameOld,
                               invAgeing.MST_CUSTOMER AS MstCustomer,
                               invAgeing.PO_MUM AS PoNum,
                               invAgeing.SO_NUM AS SoNum,
                               invAgeing.CLASS AS Class,
                               invAgeing.CURRENCY AS Currency,
                               invAgeing.CUSTOMER_COUNTRY AS CustomerCountry,
                               invAgeing.STATES AS States,
                               invAgeing.ORIGINAL_AMT AS OriginalAmt,
                               invAgeing.BALANCE_AMT AS BalanceAmt,
                               invAgeing.CREDIT_TREM AS CreditTrem,
                               invAgeing.TRACK_STATES AS TrackStates,
                               invAgeing.INVOICE_DATE AS InvoiceDate,
                               invAgeing.DUE_DATE AS DueDate,
                               invAgeing.CREATE_DATE AS CreateDate,
                               invAgeing.UPDATE_DATE AS UpdateDate,
                               invAgeing.PTP_DATE AS PtpDate,
                               invAgeing.OverdueReason AS OverdueReason,
                                (case when isnull(invAgeing.NotClear,0) = 1 then 'Lock' else '' end) as NotClear,
                               invAgeing.REMARK AS Remark,
                               invAgeing.LsrNameHist AS CS,
                               invAgeing.FsrNameHist AS Sales,
                                STUFF((SELECT ';'+ A.NAME FROM  dbo.T_CONTACTOR AS a  WHERE A.SiteUseId = invAgeing.SiteUseId AND a.TITLE IN ('Branch Manager','Sales Manager','Finance Manager') for xml path('')),1,1,'') AS BranchSalesFinance,
                                CustomerTeam.COLLECTOR AS Collector,
                               CustomerTeam.COLLECTOR_NAME AS CollectorName,
                               CustomerTeam.TEAM_NAME AS TeamName,
                               invAgeing.COMMENTS AS Comments,
                               invAgeing.SiteUseId AS SiteUseId,
                               CONTACTOR.CONTACT AS COLLECTOR_CONTACT,
                               invAgeing.DAYS_LATE_SYS AS DAYS_LATE_SYS,
                               invAgeing.AgingBucket AS AgingBucket,
                               invAgeing.Ebname AS Ebname,
                               invAgeing.ConsignmentNumber AS ConsignmentNumber,
                               invAgeing.BalanceMemo AS BalanceMemo,
                               invAgeing.MemoExpirationDate AS MemoExpirationDate,
                               (CASE WHEN invAgeing.MemoExpirationDate IS NOT NULL AND invAgeing.MemoExpirationDate <= CONVERT(DATETIME,GETDATE()) THEN  1  ELSE  0 END) AS IsExp,
                               invAgeing.TRACK_DATE AS TRACK_DATE,
                               invAgeing.PTP_DATE AS PTP_DATE,
                               CustomerTeam.COLLECTOR_NAME AS COLLECTOR_NAME,
                               VAT.VATInvoice AS VAT_NO,
                               VAT.VATInvoiceDate AS VAT_DATE,
                               (CASE WHEN invAgeing.FinishedStatus = '0' THEN 'No'  ELSE  'Yes'  END) AS FinishedStatus,
                               invAgeing.Payment_Date AS Paytment_Date,
                               (CASE WHEN DISP.ID IS NULL THEN  'N'  ELSE  'Y' END) AS Dispute,
                               (CASE WHEN DISP.ID IS NULL THEN  NULL  ELSE  DISP.CREATE_DATE  END ) AS DisputeIdentifiedDate,
                               (CASE WHEN DISP.ID IS NULL THEN NULL ELSE DISPUTEREASON_TYPE.DETAIL_NAME END  ) AS DisputeReason,
                               (CASE WHEN DISP.ID IS NULL THEN NULL ELSE Department_TYPE.DETAIL_NAME END ) AS ActionOwnerDepartment,
                               (CASE WHEN DISP.ID IS NULL THEN NULL ELSE ");
            sbselect.Append(" DATEADD(\"day\", 7, DISP.CREATE_DATE)");
            sbselect.Append(@"   END) AS NextActionDate,
                           (CASE WHEN DISP.ID IS NULL THEN NULL ELSE DISPUTESTATUS_TYPE.DETAIL_NAME END) AS DisputeStatus,
                           (CASE WHEN DISP.ID IS NULL THEN NULL ELSE DISP.COMMENTS END ) AS DisputeComment,
                           (CASE WHEN PTP.Id IS NULL THEN NULL ELSE PTP.CreateTime END) AS PtpIdentifiedDate,
                           (CASE WHEN PTP.Id IS NULL THEN NULL ELSE PTP.Comments END) AS PtpComment,
                           invAgeing.Forwarder AS Forwarder,
                           (CASE WHEN invAgeing.IsForwarder = 1 THEN 'Yes' ELSE 'No' END) AS IsForwarder,");
            sbselect.Append(@" (case when isnull(invAgeing.BalanceStatus,'') = N'已对账' then isnull(invAgeing.BalanceStatus,'') else
                                (case when " + sbDetailCount.ToString() + " = 0 or " + sbDetailBalanceCount.ToString() + @" = 0
                                      then '' else ( case when " + sbDetailCount.ToString() + " = " + sbDetailBalanceCount.ToString() + @" 
                                                          then N'已对账' else N'部分对账'  end) end ) end) AS isBanlance");


            sbselectCount.Append(" SELECT COUNT(*) ");

            sbfrom.Append(@" FROM T_INVOICE_AGING AS invAgeing WITH (NOLOCK)
                        LEFT JOIN V_CUSTOMER_TEAM AS CustomerTeam WITH (NOLOCK)
                            ON invAgeing.SiteUseId = CustomerTeam.SiteUseId
                        LEFT JOIN V_PTPPayment AS PTP WITH (NOLOCK)
                            ON PTP.SiteUseId = CustomerTeam.SiteUseId
                               AND PTP.InvoiceId = invAgeing.ID
                               AND invAgeing.PTP_DATE IS NOT NULL
                        LEFT JOIN
                        (
                            SELECT DISPUTE.ID AS ID,
                                   DISPUTE.CUSTOMER_NUM AS CUSTOMER_NUM,
                                   DISPUTE.SiteUseId AS SiteUseId,
                                   DISPUTE.CREATE_DATE AS CREATE_DATE,
                                   DISPUTE.ISSUE_REASON AS ISSUE_REASON,
                                   DISPUTE.COMMENTS AS COMMENTS,
                                   DISPUTE.ActionOwnerDepartmentCode AS ActionOwnerDepartmentCode,
                                   DISPUTE.STATUS AS STATUS,
                                   DISPUTEINVOICE.INVOICE_ID AS INVOICE_ID
                            FROM dbo.T_DISPUTE AS DISPUTE WITH (NOLOCK)
                                JOIN
                                (
                                    SELECT MAX(DISPUTE_ID) AS DISPUTE_ID,
                                           INVOICE_ID
                                    FROM dbo.T_DISPUTE_INVOICE WITH (NOLOCK)
                                    GROUP BY INVOICE_ID
                                ) AS DISPUTEINVOICE
                                    ON DISPUTE.ID = DISPUTEINVOICE.DISPUTE_ID
                            WHERE DISPUTE.STATUS NOT IN('026011', '026012')
                        ) AS DISP
                            ON DISP.SiteUseId = invAgeing.SiteUseId
                               AND DISP.INVOICE_ID = invAgeing.INVOICE_NUM
                               AND invAgeing.TRACK_STATES IN( '007', '008', '009', '011', '012' )
                        LEFT JOIN V_CUSTOMER_CONTACTOR AS CONTACTOR WITH (NOLOCK)
                            ON CONTACTOR.SiteUseId = invAgeing.SiteUseId
                        LEFT JOIN T_INVOICE_VAT AS VAT WITH (NOLOCK)
                            ON VAT.Trx_Number = invAgeing.INVOICE_NUM
                               AND VAT.LineNumber = 1
                        LEFT JOIN T_SYS_TYPE_DETAIL AS TRACKSTATE_TYPE WITH (NOLOCK)
                            ON TRACKSTATE_TYPE.TYPE_CODE = '029'
                               AND TRACKSTATE_TYPE.DETAIL_VALUE = invAgeing.TRACK_STATES
                        LEFT JOIN T_SYS_TYPE_DETAIL AS DISPUTEREASON_TYPE WITH (NOLOCK)
                            ON DISPUTEREASON_TYPE.TYPE_CODE = '025'
                               AND DISPUTEREASON_TYPE.DETAIL_VALUE = DISP.ISSUE_REASON
                        LEFT JOIN T_SYS_TYPE_DETAIL AS Department_TYPE WITH (NOLOCK)
                            ON Department_TYPE.TYPE_CODE = '038'
                               AND Department_TYPE.DETAIL_VALUE = DISP.ActionOwnerDepartmentCode
                        LEFT JOIN T_SYS_TYPE_DETAIL AS DISPUTESTATUS_TYPE WITH (NOLOCK)
                            ON DISPUTESTATUS_TYPE.TYPE_CODE = '026'
                               AND DISPUTESTATUS_TYPE.DETAIL_VALUE = DISP.STATUS
                            WHERE invAgeing.DEAL = '" + CurrentDeal + "'");
            sbCountfrom.Append(@" FROM T_INVOICE_AGING_His AS invAgeing WITH (NOLOCK)
                        LEFT JOIN V_CUSTOMER_TEAM AS CustomerTeam WITH (NOLOCK)
                            ON invAgeing.SiteUseId = CustomerTeam.SiteUseId
                            WHERE invAgeing.DEAL = '" + CurrentDeal + "'");
            ArrayList list = new ArrayList();
            if (SupervisorPermission != 1)
            {
                sbfrom.Append(@" AND (CustomerTeam.COLLECTOR IN (" + collecotrList + "))");
                sbCountfrom.Append(@" AND (CustomerTeam.COLLECTOR IN (" + collecotrList + "))");
            }
            if (!string.IsNullOrEmpty(custCode) && custCode != "null" && custCode != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.CUSTOMER_NUM like @custCode ) ");
                sbCountfrom.Append(@" and (invAgeing.CUSTOMER_NUM like @custCode ) ");
                SqlParameter param = new SqlParameter("@custCode", "%" + custCode + "%");
                list.Add(param);

            }
            if (!string.IsNullOrEmpty(custName) && custName != "null" && custName != "undefined")
            {
                sbfrom.Append(@" and (CustomerTeam.CUSTOMER_NAME like @custName )");
                sbCountfrom.Append(@" and (CustomerTeam.CUSTOMER_NAME like @custName )");
                SqlParameter param = new SqlParameter("@custName", "%" + custName + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(eb) && eb != "null" && eb != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.Ebname = @eb) ");
                sbCountfrom.Append(@" and (invAgeing.Ebname = @eb) ");
                SqlParameter param = new SqlParameter("@eb", eb);
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(consignmentNumber) && consignmentNumber != "null" && consignmentNumber != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.ConsignmentNumber = @consignmentNumber) ");
                sbCountfrom.Append(@" and (invAgeing.ConsignmentNumber = @consignmentNumber) ");
                SqlParameter param = new SqlParameter("@consignmentNumber", consignmentNumber);
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(balanceMemo) && balanceMemo != "null" && balanceMemo != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.balanceMemo = @balanceMemo) ");
                sbCountfrom.Append(@" and (invAgeing.balanceMemo = @balanceMemo) ");
                SqlParameter param = new SqlParameter("@balanceMemo", balanceMemo);
                list.Add(param);
            }

            if (!string.IsNullOrEmpty(memoExpirationDate) && memoExpirationDate != "undefined" && memoExpirationDate != "null")
            {
                sbfrom.Append(@" and (invAgeing.MemoExpirationDate <= @memoExpirationDate) ");
                sbCountfrom.Append(@" and (invAgeing.MemoExpirationDate <= @memoExpirationDate) ");
                SqlParameter param = new SqlParameter("@memoExpirationDate", memoExpirationDate + " 23:59:59");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(legal) && legal != "null" && legal != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.LEGAL_ENTITY = @legal) ");
                sbCountfrom.Append(@" and (invAgeing.LEGAL_ENTITY = @legal) ");
                SqlParameter param = new SqlParameter("@legal", legal);
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(siteUseid) && siteUseid != "null" && siteUseid != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.SiteUseId like @siteUseid) ");
                sbCountfrom.Append(@" and (invAgeing.SiteUseId like @siteUseid) ");
                SqlParameter param = new SqlParameter("@siteUseid", "%" + siteUseid + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(invoiceNum) && invoiceNum != "null" && invoiceNum != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.INVOICE_NUM like @invoiceNum) ");
                sbCountfrom.Append(@" and (invAgeing.INVOICE_NUM like @invoiceNum) ");
                SqlParameter param = new SqlParameter("@invoiceNum", "%" + invoiceNum + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(poNum) && poNum != "null" && poNum != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.PO_MUM like @poNum) ");
                sbCountfrom.Append(@" and (invAgeing.PO_MUM like @poNum) ");
                SqlParameter param = new SqlParameter("@poNum", "%" + poNum + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(soNum) && soNum != "null" && soNum != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.SO_NUM like @soNum) ");
                sbCountfrom.Append(@" and (invAgeing.SO_NUM like @soNum) ");
                SqlParameter param = new SqlParameter("@soNum", "%" + soNum + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(creditTerm) && creditTerm != "null" && creditTerm != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.CREDIT_TREM like @creditTerm) ");
                sbCountfrom.Append(@" and (invAgeing.CREDIT_TREM like @creditTerm) ");
                SqlParameter param = new SqlParameter("@creditTerm", "%" + creditTerm + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(cs) && cs != "null" && cs != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.LsrNameHist  like @cs) ");
                sbCountfrom.Append(@" and (invAgeing.LsrNameHist  like @cs) ");
                SqlParameter param = new SqlParameter("@cs", "%" + cs + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(sales) && sales != "null" && sales != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.FsrNameHist  like @sales) ");
                sbCountfrom.Append(@" and (invAgeing.FsrNameHist  like @sales) ");
                SqlParameter param = new SqlParameter("@sales", "%" + sales + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(overdueReason) && overdueReason != "null" && overdueReason != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.overdueReason like @overdueReason) ");
                sbCountfrom.Append(@" and (invAgeing.overdueReason like @overdueReason) ");
                SqlParameter param = new SqlParameter("@overdueReason", "%" + overdueReason + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(docuType) && docuType != "null" && docuType != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.CLASS = @docuType) ");
                sbCountfrom.Append(@" and (invAgeing.CLASS = @docuType) ");
                SqlParameter param = new SqlParameter("@docuType", docuType);
                list.Add(param);
            }
            if (invoiceTrackStates == "null" || invoiceTrackStates == "undefined" || string.IsNullOrWhiteSpace(invoiceTrackStates))
            {
                invoiceTrackStates = "000";
            }
            if (invoiceTrackStates == "000")
            {
                sbfrom.Append(@" AND invAgeing.TRACK_STATES != '013'
                             AND invAgeing.TRACK_STATES != '014'
                             AND invAgeing.TRACK_STATES != '016' ");
                sbCountfrom.Append(@" AND invAgeing.TRACK_STATES != '013'
                             AND invAgeing.TRACK_STATES != '014'
                             AND invAgeing.TRACK_STATES != '016' ");
            }
            else
            {
                sbfrom.Append(@" AND invAgeing.TRACK_STATES = @invoiceTrackStates");
                sbCountfrom.Append(@" AND invAgeing.TRACK_STATES = @invoiceTrackStates");
                SqlParameter param = new SqlParameter("@invoiceTrackStates", invoiceTrackStates);
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(memo) && memo != "null" && memo != "undefined")
            {
                sbfrom.Append(@" and (invAgeing.COMMENTS like @memo) ");
                sbCountfrom.Append(@" and (invAgeing.COMMENTS like @memo) ");
                SqlParameter param = new SqlParameter("@memo", "%" + memo + "%");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(ptpDateF) && ptpDateF != "undefined" && ptpDateF != "null")
            {
                sbfrom.Append(@" and (invAgeing.PTP_DATE >= @ptpDateF) ");
                sbCountfrom.Append(@" and (invAgeing.PTP_DATE >= @ptpDateF) ");
                SqlParameter param = new SqlParameter("@ptpDateF", ptpDateF + " 00:00:00");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(ptpDateT) && ptpDateT != "undefined" && ptpDateT != "null")
            {
                sbfrom.Append(@" and (invAgeing.PTP_DATE <= @ptpDateT) ");
                sbCountfrom.Append(@" and (invAgeing.PTP_DATE <= @ptpDateT) ");
                SqlParameter param = new SqlParameter("@ptpDateT", ptpDateT + " 23:59:59");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(memoDateF) && memoDateF != "undefined" && memoDateF != "null")
            {
                sbfrom.Append(@" and (invAgeing.MemoExpirationDate >= @memoDateF) ");
                sbCountfrom.Append(@" and (invAgeing.MemoExpirationDate >= @memoDateF) ");
                SqlParameter param = new SqlParameter("@memoDateF", memoDateF + " 00:00:00");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(memoDateT) && memoDateT != "undefined" && memoDateT != "null")
            {
                sbfrom.Append(@" and (invAgeing.MemoExpirationDate <= @memoDateT) ");
                sbCountfrom.Append(@" and (invAgeing.MemoExpirationDate <= @memoDateT) ");
                SqlParameter param = new SqlParameter("@memoDateT", memoDateT + " 23:59:59");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(invoiceDateF) && invoiceDateF != "undefined" && invoiceDateF != "null")
            {
                sbfrom.Append(@" and (invAgeing.INVOICE_DATE >= @invoiceDateF) ");
                sbCountfrom.Append(@" and (invAgeing.INVOICE_DATE >= @invoiceDateF) ");
                SqlParameter param = new SqlParameter("@invoiceDateF", invoiceDateF + " 00:00:00");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(invoiceDateT) && invoiceDateT != "undefined" && invoiceDateT != "null")
            {
                sbfrom.Append(@" and (invAgeing.INVOICE_DATE <= @invoiceDateT) ");
                sbCountfrom.Append(@" and (invAgeing.INVOICE_DATE <= @invoiceDateT) ");
                SqlParameter param = new SqlParameter("@invoiceDateT", invoiceDateT + " 23:59:59");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(dueDateF) && dueDateF != "undefined" && dueDateF != "null")
            {
                sbfrom.Append(@" and (invAgeing.DUE_DATE >= @dueDateF) ");
                sbCountfrom.Append(@" and (invAgeing.DUE_DATE >= @dueDateF) ");
                SqlParameter param = new SqlParameter("@dueDateF", dueDateF + " 00:00:00");
                list.Add(param);
            }
            if (!string.IsNullOrEmpty(dueDateT) && dueDateT != "undefined" && dueDateT != "null")
            {
                sbfrom.Append(@" and (invAgeing.DUE_DATE <= @dueDateT) ");
                sbCountfrom.Append(@" and (invAgeing.DUE_DATE <= @dueDateT) ");
                SqlParameter param = new SqlParameter("@dueDateT", dueDateT + " 23:59:59");
                list.Add(param);
            }
            SqlParameter[] paramForSQL = new SqlParameter[list.Count];
            int intParam = 0;
            foreach (var item in list)
            {
                paramForSQL[intParam] = (SqlParameter)item;
                intParam++;
            }

            sbOderby.Append(@" order by IsExp desc ,
                                      invAgeing.DEAL,
                                      invAgeing.LEGAL_ENTITY,
                                      invAgeing.CUSTOMER_NUM,
							          invAgeing.SiteUseId,
							          invAgeing.INVOICE_NUM ");
            if (!lb_isExport)
            {
                sbPage.Append(@" offset " + (pageindex - 1) * pagesize + " rows fetch next " + pagesize + " rows only");
            }

            DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString() + sbfrom.ToString() + " UNION " + sbselect.ToString() + sbfrom.ToString().Replace("T_INVOICE_AGING", "T_INVOICE_AGING_HIS") + sbOderby.ToString() + sbPage.ToString(), System.Data.CommandType.Text, paramForSQL);
            List<MyinvoicesDto> tdd = SqlHelper.GetList<MyinvoicesDto>(dt);

            result.dataRows = tdd;
            result.count = SqlHelper.ExcuteScalar<int>(sbselectCount.ToString() + sbCountfrom.ToString(), paramForSQL);

            return result;
        }

        public IEnumerable<MyinvoicesDto> GetMyinvoicesList(string closeType, string siteUseId = "")
        {
            IQueryable<InvoiceAging> tempInvoices = null;
            List<MyinvoicesDto> myinvoices = new List<MyinvoicesDto>();
            MyinvoicesDto myinvoice = new MyinvoicesDto();
            IQueryable<CustomerTeam> tempcusList = null;

            List<SysUser> listUser = new List<SysUser>();
            listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
            listUser.Add(new SysUser { });
            string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
            string collecotrList = ", " + string.Join(",", userGroup.ToArray()) + ",";

            //Current User's customers
            if (SupervisorPermission == 1)
            {
                tempcusList = CommonRep.GetDbSet<CustomerTeam>().Where(o => o.Deal == CurrentDeal);

                //with closeType
                if (closeType == "000" || closeType == "undefined")
                {
                    tempInvoices = CommonRep.GetDbSet<InvoiceAging>().Where(o => o.Deal == CurrentDeal)
                                    .Where(o => o.TrackStates != "013" && o.TrackStates != "014" && o.TrackStates != "016");
                }
                else
                {
                    tempInvoices = CommonRep.GetDbSet<InvoiceAging>().Where(o => o.Deal == CurrentDeal);
                }

            }
            else
            {
                tempcusList = CommonRep.GetDbSet<CustomerTeam>().Where(o => o.Deal == CurrentDeal && collecotrList.Contains("," + o.Collector + ","));

                List<string> cusList = new List<string>();
                cusList = tempcusList.Select(o => o.CustomerNum).ToList<string>();

                //get invoiceList by custListResult
                //with closeType
                if (closeType == "000" || closeType == "undefined")
                {
                    tempInvoices = CommonRep.GetDbSet<InvoiceAging>().Where(o => o.Deal == CurrentDeal && cusList.Contains(o.CustomerNum))  // && (o.States == "004001" || o.States == "004002" || o.States == "004004" || o.States == "004008" || o.States == "004010" || o.States == "004011" || o.States == "004012")
                        .Where(o => o.TrackStates != "013" && o.TrackStates != "014" && o.TrackStates != "016");
                }
                else
                {
                    tempInvoices = CommonRep.GetDbSet<InvoiceAging>().Where(o => o.Deal == CurrentDeal && cusList.Contains(o.CustomerNum));
                }
            }

            if (!string.IsNullOrEmpty(siteUseId) && siteUseId != "undefined")
            {
                tempInvoices = from a in tempInvoices
                               where a.SiteUseId == siteUseId
                               select a;
            }

            var assType = from x in CommonRep.GetDbSet<T_CustomerAssessment>()
                          join y in CommonRep.GetDbSet<T_AssessmentType>()
                          on x.AssessmentType equals y.Id
                          into xy
                          from y in xy.DefaultIfEmpty()
                          select new { CustomerNum = x.CustomerId, DunningPirority = y.DunningPirority, SiteUseId = x.SiteUseId };

            var disQ = from di in CommonRep.GetQueryable<DisputeInvoice>()
                       group di by di.InvoiceId into g
                       select new { g.Key, DisputeID = g.Max(s => s.DisputeId) };

            var dis = from q in CommonRep.GetQueryable<Dispute>()
                      join di in disQ on q.Id equals di.DisputeID
                      select new { di.Key, q.CustomerNum, q.SiteUseId, q.CreateDate, q.IssueReason, q.Comments, q.ActionOwnerDepartmentCode, q.Status };
            //config
            var conQ = CommonRep.GetQueryable<SysTypeDetail>().Where(s => s.TypeCode == "029"); //Invoitce Track Status
            var conQ_Reason = CommonRep.GetQueryable<SysTypeDetail>().Where(s => s.TypeCode == "025"); //DisputeReason
            var conQ_Department = CommonRep.GetQueryable<SysTypeDetail>().Where(s => s.TypeCode == "038"); //ActionOwnerDepartment
            var dispute_status = CommonRep.GetQueryable<SysTypeDetail>().Where(s => s.TypeCode == "026");
            var ptpQ = from ptp in CommonRep.GetQueryable<T_PTPPayment_Invoice>()
                       join pp in CommonRep.GetQueryable<T_PTPPayment>()
                       on ptp.PTPPaymentId equals pp.Id
                       where pp.PTPPaymentType == "PTP"
                       group ptp by ptp.InvoiceId into g
                       select new { Key = g.Key, PTPId = g.Max(s => s.PTPPaymentId) };

            var pt = from pp in CommonRep.GetQueryable<T_PTPPayment>()
                     join ptp in ptpQ on pp.Id equals ptp.PTPId
                     select new { ptp.Key, pp.CustomerNum, pp.SiteUseId, pp.CreateTime, pp.PromiseDate, pp.Comments };

            var r = from invs in tempInvoices
                    join grp in tempcusList on new { invs.Deal, invs.CustomerNum, invs.SiteUseId } equals new { grp.Deal, grp.CustomerNum, grp.SiteUseId }
                    join custorder in assType on new { CustomerNum = grp.CustomerNum, SiteUseId = grp.SiteUseId } equals new { CustomerNum = custorder.CustomerNum, SiteUseId = custorder.SiteUseId }
                    into custorders
                    from custorderss in custorders.DefaultIfEmpty()
                    join vat in CommonRep.GetDbSet<T_INVOICE_VAT>()
                    on new { InvoiceNums = invs.InvoiceNum, LineNumbers = "1" } equals new { InvoiceNums = vat.Trx_Number, LineNumbers = vat.LineNumber.ToString() }
                    into vats
                    from vatss in vats.DefaultIfEmpty()
                    join ccv in CommonRep.GetQueryable<CustomerContactorView>() on new { Deal = invs.Deal, CustomerNum = invs.CustomerNum, SiteUseId = invs.SiteUseId } equals new { Deal = ccv.Deal, CustomerNum = ccv.CustomerNum, SiteUseId = ccv.SiteUseId }
                    into ccvs
                    from ccvsss in ccvs.DefaultIfEmpty()
                    join d in dis on new { InvoiceNum = invs.InvoiceNum, Class = invs.Class } equals new { InvoiceNum = d.Key, Class = "INV" } into dis_Data
                    from dis_d in dis_Data.DefaultIfEmpty()
                    join con in conQ on invs.TrackStates equals con.DetailValue into con_Data
                    from con_d in con_Data.DefaultIfEmpty()
                    join con_R in conQ_Reason on dis_d.IssueReason equals con_R.DetailValue into con_RData
                    from con_Rd in con_RData.DefaultIfEmpty()
                    join con_Q in conQ_Department on dis_d.ActionOwnerDepartmentCode equals con_Q.DetailValue into con_QData
                    from con_Qd in con_QData.DefaultIfEmpty()
                    join p in pt on invs.Id equals p.Key into ptp_Data
                    from ptp_d in ptp_Data.DefaultIfEmpty()
                    join dis_sys in dispute_status on dis_d.Status equals dis_sys.DetailValue into disSys_Data
                    from disSys in disSys_Data.DefaultIfEmpty()
                    select new
                    {
                        Id = invs.Id,
                        Deal = invs.Deal,
                        LegalEntity = invs.LegalEntity,
                        CustomerNum = invs.CustomerNum,
                        CustomerName = grp.CustomerName,
                        InvoiceNum = invs.InvoiceNum,
                        GroupCodeOld = grp.BillGroupCode,
                        GroupNameOld = grp.BillGroupName,
                        MstCustomer = invs.MstCustomer,
                        PoNum = invs.PoNum,
                        SoNum = invs.SoNum,
                        Class = invs.Class,
                        Currency = invs.Currency,
                        Country = invs.CustomerCountry,
                        States = invs.States,
                        OriginalAmt = invs.OriginalAmt,
                        BalanceAmt = invs.BalanceAmt,
                        CreditTrem = invs.CreditTrem,
                        TrackStates = invs.TrackStates,
                        InvoiceDate = invs.InvoiceDate,
                        DueDate = invs.DueDate,
                        CreateDate = invs.CreateDate,
                        UpdateDate = invs.UpdateDate,
                        PtpDate = invs.PtpDate,
                        OverdueReason = invs.OverdueReason,
                        Remark = invs.Remark,
                        Collector = grp.Collector,
                        CollectorName = grp.CollectorName,
                        TeamName = grp.TeamName,
                        Comments = invs.Comments,
                        SiteUseId = invs.SiteUseId,
                        COLLECTOR_CONTACT = ccvsss.Contactor,
                        DAYS_LATE_SYS = invs.DaysLateSys,
                        AgingBucket = invs.AgingBucket,
                        Ebname = invs.Ebname,
                        TRACK_DATE = invs.TRACK_DATE,
                        PTP_DATE = invs.PtpDate,
                        COLLECTOR_NAME = grp.CollectorName,
                        DunningPirority = custorderss != null ? custorderss.DunningPirority : 3,
                        VAT_NO = vatss.VATInvoice,
                        VAT_DATE = vatss.VATInvoiceDate,
                        FinishedStatus = invs.FinishedStatus,
                        Paytment_Date = invs.Payment_Date,
                        Dispute = dis_d == null ? "N" : "Y",
                        DisputeIdentifiedDate = dis_d == null ? default(DateTime?) : dis_d.CreateDate,
                        DisputeReason = dis_d == null ? "" : con_Rd.DetailName,
                        ActionOwnerDepartment = dis_d == null ? "" : con_Qd.DetailName,
                        NextActionDate = dis_d == null ? default(DateTime?) : SqlFunctions.DateAdd("day", 7, dis_d.CreateDate),
                        PtpIdentifiedDate = ptp_d == null ? null : ptp_d.CreateTime,
                        PtpComment = ptp_d == null ? "" : ptp_d.Comments,
                        DisputeStatus = dis_d == null ? "" : disSys.DetailName,
                        DisputeComment = dis_d == null ? "" : dis_d.Comments,
                        Forwarder = invs.Forwarder,
                        IsForwarder = invs.IsForwarder,
                        isBanlance = invs.BalanceStatus
                    }
                    into lastquery
                    orderby lastquery.InvoiceNum
                    select new MyinvoicesDto
                    {
                        Id = lastquery.Id,
                        Deal = lastquery.Deal,
                        LegalEntity = lastquery.LegalEntity,
                        CustomerNum = lastquery.CustomerNum,
                        CustomerName = lastquery.CustomerName,
                        InvoiceNum = lastquery.InvoiceNum,
                        GroupCodeOld = lastquery.GroupCodeOld,
                        GroupNameOld = lastquery.GroupNameOld,
                        MstCustomer = lastquery.MstCustomer,
                        PoNum = lastquery.PoNum,
                        SoNum = lastquery.SoNum,
                        Class = lastquery.Class,
                        Currency = lastquery.Currency,
                        Country = lastquery.Country,
                        States = lastquery.States,
                        OriginalAmt = lastquery.OriginalAmt,
                        BalanceAmt = lastquery.BalanceAmt,
                        CreditTrem = lastquery.CreditTrem,
                        TrackStates = lastquery.TrackStates,
                        InvoiceDate = lastquery.InvoiceDate,
                        DueDate = lastquery.DueDate,
                        CreateDate = lastquery.CreateDate,
                        UpdateDate = lastquery.UpdateDate,
                        PtpDate = lastquery.PtpDate,
                        OverdueReason = lastquery.OverdueReason,
                        Remark = lastquery.Remark,
                        Collector = lastquery.Collector,
                        CollectorName = lastquery.CollectorName,
                        TeamName = lastquery.TeamName,
                        Comments = lastquery.Comments,
                        SiteUseId = lastquery.SiteUseId,
                        COLLECTOR_CONTACT = lastquery.COLLECTOR_CONTACT,
                        DAYS_LATE_SYS = lastquery.DAYS_LATE_SYS,
                        AgingBucket = lastquery.AgingBucket,
                        Ebname = lastquery.Ebname,
                        TRACK_DATE = lastquery.TRACK_DATE,
                        PTP_DATE = lastquery.PtpDate,
                        COLLECTOR_NAME = lastquery.CollectorName,
                        VAT_NO = lastquery.VAT_NO,
                        VAT_DATE = lastquery.VAT_DATE,
                        FinishedStatus = lastquery.FinishedStatus == "0" ? "No" : "Yes",
                        Payment_Date = lastquery.Paytment_Date,
                        DisputeFlag = lastquery.Dispute,
                        Dispute_Identified_Date = lastquery.DisputeIdentifiedDate,
                        Dispute_Reason = lastquery.DisputeReason,
                        Owner_Department = lastquery.ActionOwnerDepartment,
                        LastUpdateDate = lastquery.UpdateDate,
                        Next_Action_Date = lastquery.NextActionDate,
                        PTP_Identified_Date = lastquery.PtpIdentifiedDate,
                        PtpComment = lastquery.PtpComment,
                        DisputeStatus = lastquery.DisputeStatus,
                        DisputeComment = lastquery.DisputeComment,
                        Forwarder = lastquery.Forwarder,
                        IsForwarder = lastquery.IsForwarder == true ? "Yes" : "No",
                        isBanlance = lastquery.isBanlance
                    };

            foreach (MyinvoicesDto item in r)
            {
                if (string.IsNullOrEmpty(item.isBanlance))
                {
                    var detail_all = CommonRep.GetQueryable<T_Invoice_Detail>()
                                .Where(o => o.InvoiceNumber == item.InvoiceNum).ToList();
                    var detail_isbalance = detail_all.FindAll(o => o.BalanceStatus == "已对账");
                    if (detail_all.Count == 0 || detail_isbalance.Count == 0)
                    {
                        item.isBanlance = "";
                    }
                    else
                    {
                        if (detail_isbalance.Count == detail_all.Count)
                        {
                            item.isBanlance = "已对账";
                        }
                        else
                        {
                            item.isBanlance = "部分对账";
                        }
                    }
                }
            }
            int count = r.Count();
            return r;
        }

        public HttpResponseMessage ExportInvoicesList(string custCode, string custName, string eb, string consignmentNumber,string balanceMemo, string memoExpirationDate,
            string legal, string siteUseid, string invoiceNum, string poNum, string soNum,
            string creditTerm, string docuType, string invoiceTrackStates, string memo,
            string ptpDateF, string ptpDateT, string memoDateF, string memoDateT, string invoiceDateF, string invoiceDateT, string dueDateF, string dueDateT, string cs, string sales, string overdueReason)
        {
            List<Customer.ExpCustomerDto> lstCustomer = new List<Customer.ExpCustomerDto>();
            List<InvoiceAging> invoiceList = new List<InvoiceAging>();
            InvoiceAging invoice = new InvoiceAging();
            string templateFile = "";
            string fileName = "";
            string tmpFile = "";

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportInvoiceTemplate"].ToString());
                fileName = AppContext.Current.User.EID + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                var tempInvoices = GetMyinvoicesListNoLinq(true, 0, 0, custCode, custName, eb, consignmentNumber, balanceMemo, memoExpirationDate, legal, siteUseid, invoiceNum,
                    poNum, soNum, creditTerm, docuType, invoiceTrackStates, memo, ptpDateF,
                    ptpDateT, memoDateF, memoDateT, invoiceDateF, invoiceDateT, dueDateF, dueDateT, cs, sales, overdueReason).dataRows;

                this.setData(templateFile, tmpFile, tempInvoices);

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
                throw ex;
            }
            finally
            {
                //File.Delete(tmpFile);
            }
        }


        public HttpResponseMessage ExportInvoicesListForArrow(string custCode, string custName, string eb, string consignmentNumber, string balanceMemo, string memoExpirationDate,
            string legal, string siteUseid, string invoiceNum, string poNum, string soNum,
            string creditTerm, string docuType, string invoiceTrackStates, string memo,
            string ptpDateF, string ptpDateT, string memoDateF, string memoDateT, string invoiceDateF, string invoiceDateT, string dueDateF, string dueDateT, string cs, string sales, string overdueReason)
        {
            List<Customer.ExpCustomerDto> lstCustomer = new List<Customer.ExpCustomerDto>();
            List<InvoiceAging> invoiceList = new List<InvoiceAging>();
            InvoiceAging invoice = new InvoiceAging();
            string templateFile = "";
            string fileName = "";
            string tmpFile = "";

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportInvoiceTemplate"].ToString());
                fileName = AppContext.Current.User.EID + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                var tempInvoices = GetMyinvoicesListNoLinq(true, 0, 0, custCode, custName, eb, consignmentNumber, balanceMemo, memoExpirationDate, legal, siteUseid, invoiceNum,
                    poNum, soNum, creditTerm, docuType, invoiceTrackStates, memo, ptpDateF,
                    ptpDateT, memoDateF, memoDateT, invoiceDateF, invoiceDateT, dueDateF, dueDateT, cs, sales, overdueReason).dataRows;

                this.setDataForArrow(templateFile, tmpFile, tempInvoices);

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
                    Exception ex = new OTCServiceException("Get file failed because file not exist with physical path: " + tmpFile);
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
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

        #region Set Invoice Report Datas
        private void setData(string templateFileName, string tmpFile, List<MyinvoicesDto> lstDatas)
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
                    helper.SetData(rowNo, 1, lst.InvoiceNum);
                    helper.SetData(rowNo, 2, lst.CustomerName);
                    helper.SetData(rowNo, 3, lst.CustomerNum);
                    helper.SetData(rowNo, 4, lst.SiteUseId);
                    helper.SetData(rowNo, 5, lst.Class);
                    helper.SetData(rowNo, 6, lst.Currency);
                    helper.SetData(rowNo, 7, lst.BalanceAmt);
                    helper.SetData(rowNo, 8, string.IsNullOrEmpty(lst.InvoiceDate.ToString()) ? "" : lst.InvoiceDate.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 9, string.IsNullOrEmpty(lst.DueDate.ToString()) ? "" : lst.DueDate.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 10, lst.DAYS_LATE_SYS);
                    helper.SetData(rowNo, 11, lst.CreditTrem);
                    helper.SetData(rowNo, 12, lst.OriginalAmt);
                    helper.SetData(rowNo, 13, lst.AgingBucket);
                    helper.SetData(rowNo, 14, lst.Ebname);
                    helper.SetData(rowNo, 15, string.IsNullOrEmpty(lst.PtpDate.ToString()) ? "" : lst.PtpDate.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 16, lst.DisputeFlag);
                    helper.SetData(rowNo, 17, lst.Dispute_Identified_Date);
                    helper.SetData(rowNo, 18, lst.Dispute_Reason);
                    helper.SetData(rowNo, 19, lst.Owner_Department);
                    helper.SetData(rowNo, 20, lst.Comments);
                    helper.SetData(rowNo, 21, lst.COLLECTOR_CONTACT);
                    helper.SetData(rowNo, 22, lst.VAT_NO);
                    helper.SetData(rowNo, 23, lst.VAT_DATE);
                    helper.SetData(rowNo, 24, !String.IsNullOrEmpty(lst.TrackStates) ? Helper.CodeToEnum<TrackStatus>(lst.TrackStates).ToString().Replace("_", " ") : "");
                    helper.SetData(rowNo, 25, lst.FinishedStatus);
                    helper.SetData(rowNo, 26, string.IsNullOrEmpty(lst.LastUpdateDate.ToString()) ? "" : lst.LastUpdateDate.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 27, string.IsNullOrEmpty(lst.PTP_Identified_Date.ToString()) ? "" : lst.PTP_Identified_Date.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 28, lst.COLLECTOR_NAME);
                    helper.SetData(rowNo, 29, lst.Next_Action_Date);
                    rowNo++;
                }
                //formula calcuate result
                helper.Save(tmpFile, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        private void setDataForArrow(string templateFileName, string tmpFile, List<MyinvoicesDto> lstDatas)
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

                ICellStyle styleCellAmount = helper.Book.CreateCellStyle();
                IDataFormat dataFormatAmount = helper.Book.CreateDataFormat();
                styleCellAmount.DataFormat = dataFormatAmount.GetFormat("#,##0.00");

                ICellStyle styleCellInt = helper.Book.CreateCellStyle();
                IDataFormat dataFormatInt = helper.Book.CreateDataFormat();
                styleCellInt.DataFormat = dataFormatInt.GetFormat("##0");

                //设置Excel的内容信息
                foreach (var lst in lstDatas)
                {
                    helper.SetData(rowNo, 0, lst.LegalEntity);
                    helper.SetData(rowNo, 1, lst.InvoiceNum);
                    helper.SetData(rowNo, 2, lst.CustomerName);
                    helper.SetData(rowNo, 3, lst.CustomerNum);
                    helper.SetData(rowNo, 4, lst.SiteUseId);
                    helper.SetData(rowNo, 5, lst.Class);
                    helper.SetData(rowNo, 6, lst.Currency);
                    helper.SetData(rowNo, 7, lst.BalanceAmt);
                    ICell cell1 = helper.GetCell(rowNo,7);
                    cell1.CellStyle = styleCellAmount;
                    helper.SetData(rowNo, 8, string.IsNullOrEmpty(lst.InvoiceDate.ToString()) ? "" : lst.InvoiceDate.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 9, string.IsNullOrEmpty(lst.DueDate.ToString()) ? "" : lst.DueDate.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 10, lst.DAYS_LATE_SYS);
                    ICell cell2 = helper.GetCell(rowNo, 10);
                    cell2.CellStyle = styleCellInt;
                    helper.SetData(rowNo, 11, lst.CreditTrem);
                    helper.SetData(rowNo, 12, lst.OriginalAmt);
                    ICell cell3 = helper.GetCell(rowNo, 12);
                    cell3.CellStyle = styleCellAmount;
                    helper.SetData(rowNo, 13, lst.AgingBucket);
                    helper.SetData(rowNo, 14, lst.Ebname);
                    helper.SetData(rowNo, 15, lst.ConsignmentNumber);
                    helper.SetData(rowNo, 16, string.IsNullOrEmpty(lst.PtpDate.ToString()) ? "" : lst.PtpDate.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 17, lst.PtpComment);
                    helper.SetData(rowNo, 18, lst.DisputeFlag);
                    helper.SetData(rowNo, 19, lst.Dispute_Identified_Date);
                    helper.SetData(rowNo, 20, lst.DisputeStatus);
                    helper.SetData(rowNo, 21, lst.Dispute_Reason);
                    helper.SetData(rowNo, 22, lst.DisputeComment);
                    helper.SetData(rowNo, 23, lst.Owner_Department);
                    helper.SetData(rowNo, 24, lst.BalanceMemo);
                    helper.SetData(rowNo, 25, string.IsNullOrEmpty(lst.MemoExpirationDate.ToString()) ? "" : lst.MemoExpirationDate.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 26, lst.COLLECTOR_CONTACT);
                    helper.SetData(rowNo, 27, lst.VAT_NO);
                    helper.SetData(rowNo, 28, lst.VAT_DATE);

                    switch (lst.TrackStates)
                    {
                        case "000":
                            helper.SetData(rowNo, 29, "Open");
                            break;
                        case "001":
                            helper.SetData(rowNo, 29, "Responsed OverDue Reason");
                            break;
                        case "002":
                            helper.SetData(rowNo, 29, "Wait for 2nd Time Confirm PTP");
                            break;
                        case "003":
                            helper.SetData(rowNo, 29, "PTP Confirmed");
                            break;
                        case "004":
                            helper.SetData(rowNo, 29, "Wait for Payment Reminding");
                            break;
                        case "005":
                            helper.SetData(rowNo, 29, "Wait for 1st Time Dunning");
                            break;
                        case "006":
                            helper.SetData(rowNo, 29, "Wait for 2nd Time Dunning");
                            break;
                        case "007":
                            helper.SetData(rowNo, 29, "Dispute Identified");
                            break;
                        case "008":
                            helper.SetData(rowNo, 29, "Wait for 2nd Time Dispute contact");
                            break;
                        case "009":
                            helper.SetData(rowNo, 29, "Wait for Dispute Responds");
                            break;
                        case "010":
                            helper.SetData(rowNo, 29, "Dispute Resolved");
                            break;
                        case "011":
                            helper.SetData(rowNo, 29, "Wait for 2nd Time Dispute respond");
                            break;
                        case "012":
                            helper.SetData(rowNo, 29, "Escalation");
                            break;
                        case "013":
                            helper.SetData(rowNo, 29, "Write off uncollectible accounts");
                            break;
                        case "014":
                            helper.SetData(rowNo, 29, "Closed");
                            break;
                        case "015":
                            helper.SetData(rowNo, 29, "Payment Notice Received");
                            break;
                        case "016":
                            helper.SetData(rowNo, 29, "Cancel");
                            break;
                        default:
                            helper.SetData(rowNo, 29, "");
                            break;
                    }
                    helper.SetData(rowNo, 30, lst.FinishedStatus);
                    helper.SetData(rowNo, 31, string.IsNullOrEmpty(lst.LastUpdateDate.ToString()) ? "" : lst.LastUpdateDate.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 32, string.IsNullOrEmpty(lst.PTP_Identified_Date.ToString()) ? "" : lst.PTP_Identified_Date.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 33, lst.COLLECTOR_NAME);
                    helper.SetData(rowNo, 34, lst.Next_Action_Date);
                    helper.SetData(rowNo, 35, lst.PoNum);
                    helper.SetData(rowNo, 36, lst.SoNum);
                    helper.SetData(rowNo, 37, lst.IsForwarder);
                    helper.SetData(rowNo, 38, lst.Forwarder);
                    rowNo++;
                }

                //formula calcuate result
                helper.Save(tmpFile, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
        #endregion

        public string UploadFile(HttpPostedFile file, string archiveFileName, String FileType)
        {
            file.SaveAs(archiveFileName);
            string strErrMsg = "";

            if (File.Exists(archiveFileName))
            {
                //读取EXCEL
                List<T_INVOICE_STATUS_STAGING> listInvoiceStatus = new List<T_INVOICE_STATUS_STAGING>();
                List<T_Customer_Comments> listComments = new List<T_Customer_Comments>();
                strErrMsg = excelToList(archiveFileName, FileType, ref listInvoiceStatus, ref listComments);
                if (string.IsNullOrEmpty(strErrMsg))
                {
                    if (FileType == "SOA-CN")
                    {
                        SqlHelper.ExcuteSql(string.Format(@"Delete from T_INVOICE_STATUS_CUSTOMER_STAGING where CREATE_USER = '{0}'", AppContext.Current.User.EID)) ;
                        foreach (T_Customer_Comments item in listComments)
                        {
                            //if(item.PTPDATE != null || item.PTPAmount != null || !string.IsNullOrEmpty(item.OverdueReason) || !string.IsNullOrEmpty(item.Comments)) { 
                                StringBuilder sqlFile = new StringBuilder();
                                sqlFile.Append("INSERT INTO T_INVOICE_STATUS_CUSTOMER_STAGING (FILETYPE, SiteUseId, AgingBucket,PTPDATE,PTPAmount,ODReason,Comments,CommentsFrom,CREATE_USER,CREATE_DATE) ");
                                sqlFile.Append(" VALUES (@FILETYPE,");
                                sqlFile.Append(" @SiteUseId,");
                                sqlFile.Append(" @AgingBucket,");
                                sqlFile.Append(" @PTPDATE,");
                                sqlFile.Append(" @PTPAmount, ");
                                sqlFile.Append(" @ODReason,");
                                sqlFile.Append(" @Comments,");
                                sqlFile.Append(" @CommentsFrom,");
                                sqlFile.Append(" @CREATE_USER, getdate()) ");
                                SqlParameter[] parms = { new SqlParameter("@FILETYPE", FileType),
                                             new SqlParameter("@SiteUseId", item.SiteUseId.Trim()),
                                             new SqlParameter("@AgingBucket", item.AgingBucket.Trim()),
                                             (item.PTPDATE == null ? new SqlParameter("@PTPDATE",  DBNull.Value) : new SqlParameter("@PTPDATE",item.PTPDATE)),
                                             (item.PTPAmount == null ? new SqlParameter("@PTPAmount", DBNull.Value) : new SqlParameter("@PTPAmount", item.PTPAmount)),
                                             new SqlParameter("@ODReason", item.OverdueReason),
                                             new SqlParameter("@Comments", item.Comments),
                                             new SqlParameter("@CommentsFrom", item.CommentsFrom),
                                             new SqlParameter("@CREATE_USER", AppContext.Current.User.EID)
                                            };
                                SqlHelper.ExcuteSql(sqlFile.ToString(), parms);
                            //}
                        }
                    }
                    //if (FileType == "SOA-CN")
                    //{
                    //    string strCustomerNum = customerNum;
                    //    SqlHelper.ExcuteSql("Update T_Customer_Comments set isDeleted = 1 where CUSTOMER_NUM = '" + strCustomerNum + "'");
                    //    SqlHelper.ExcuteSql("Update T_Customer set Comment = '' where CUSTOMER_NUM = '" + strCustomerNum + "'");
                    //    if (listComments.Count > 0)
                    //    {
                    //        string strComments = "";
                    //        int intNum = 0;
                    //        string strSiteId = "";
                    //        string strPreSiteId = "";
                    //        foreach (T_Customer_Comments item in listComments)
                    //        {
                    //            strSiteId = item.SiteUseId;
                    //            SqlHelper.ExcuteSql(string.Format(@"INSERT INTO T_Customer_Comments (ID,CUSTOMER_NUM,SiteUseId,AgingBucket,PTPDATE,PTPAmount,OverdueReason,Comments,CreateUser,CreateDate,isDeleted,SortId) " +
                    //                "                       values (NEWID(),'{0}','{1}','{2}', '{3}',{4},'{5}','{6}','{7}',getdate(), 0, {8})", strCustomerNum, item.SiteUseId.Trim(), item.AgingBucket.Trim(), item.PTPDATE, item.PTPAmount.ToString(), item.OverdueReason, item.Comments, AppContext.Current.User.EID, intNum.ToString()));
                    //            if (intNum != 0 && strSiteId != strPreSiteId)
                    //            {
                    //                SqlHelper.ExcuteSql(string.Format("Update T_Customer set Comment =  N'{0}' where CUSTOMER_NUM = '{1}' and siteuseid = '{2}'", strComments, strCustomerNum, strPreSiteId));
                    //                strComments = "";
                    //            }
                    //            strComments += item.AgingBucket + ": " + (item.PTPDATE == null ? "" : Convert.ToDateTime(item.PTPDATE).ToString("yyyy-MM-dd")) + " 计划付款" + item.PTPAmount.ToString() + "," + item.Comments + "\n\r";
                    //            strPreSiteId = strSiteId;
                    //            intNum++;
                    //        }
                    //        SqlHelper.ExcuteSql(string.Format("Update T_Customer set Comment = N'{0}' where CUSTOMER_NUM = '{1}' and siteuseid = '{2}'", strComments, strCustomerNum, strPreSiteId));
                    //    }
                    //}
                    if (listInvoiceStatus.Count > 0)
                    {
                        //保存数据(先删除该用户之前导入的临时数据)
                        List<T_INVOICE_STATUS_STAGING> listInvoiceStatus_Old = CommonRep.GetDbSet<T_INVOICE_STATUS_STAGING>().Where(o => o.CREATE_USER == AppContext.Current.User.EID).ToList();
                        CommonRep.RemoveRange(listInvoiceStatus_Old);
                        CommonRep.BulkInsert(listInvoiceStatus);
                        CommonRep.Commit();
                    }
                    if(listInvoiceStatus.Count == 0 && listComments.Count == 0)
                    {
                        strErrMsg = "No data need to upload.";
                    }
                }
            }
            return strErrMsg;
        }

        public string UploadDSOFile(HttpPostedFile file, string archiveFileName)
        {
            string strTargetFile = Path.GetDirectoryName(archiveFileName) + @"\" + Path.GetFileNameWithoutExtension(archiveFileName) + "_" + Guid.NewGuid().ToString("N") + Path.GetExtension(archiveFileName);
            file.SaveAs(strTargetFile);
            string strErrMsg = "";

            if (File.Exists(strTargetFile))
            {
                return strTargetFile;
            }
            else
            {
                strErrMsg = "Save file fail.";
            }
            return strErrMsg;
        }

        public string AnalysisDSO(string packFileName, string monthList, string packageDays)
        {
            //ZIP解压文件
            string strGuid = Guid.NewGuid().ToString("N");
            string strNoExtend = Path.GetFileNameWithoutExtension(packFileName);
            string strPath = Path.GetDirectoryName(packFileName);
            using (ZipArchive zipArchive = System.IO.Compression.ZipFile.Open(packFileName, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    string strExtend = getFileExtendName(entry.Name).ToUpper();
                    if (strExtend.ToUpper() == ".ZIP")
                    {
                        unZip(packFileName, strNoExtend + "_" + strGuid, entry.Name, strGuid);
                    }
                }
            }
            List<FileInfo> lst = new List<FileInfo>();
            lst = getFile(strPath + @"\" + strNoExtend + "_" + strGuid + @"\" + "Transaction_" + strGuid, ".txt");
            List<DsoAnalysisDto> listData = new List<DsoAnalysisDto>();
            foreach (FileInfo fileItem in lst)
            {
                string strLegal = Path.GetFileNameWithoutExtension(fileItem.Name).Split(' ')[0].Trim();
                using (StreamReader sr = new StreamReader(fileItem.FullName, Encoding.Default))
                {
                    String line = sr.ReadLine();
                    while (null != line)
                    {
                        if (line.Length != 180)
                        {
                            line = sr.ReadLine();
                            continue;
                        }
                        DsoAnalysisDto rowData = new DsoAnalysisDto();
                        rowData.Legal = strLegal;
                        rowData.InvClass = line.Substring(30, 12).Trim();
                        rowData.InvoiceNo = line.Substring(42, 20).Trim();
                        if (string.IsNullOrEmpty(rowData.InvoiceNo) || rowData.InvoiceNo == "Invoice Number" || !isNumberic(rowData.InvoiceNo))
                        {
                            line = sr.ReadLine();
                            continue;
                        }
                        rowData.CusNumberNum = line.Substring(108, 10).Trim();
                        rowData.InvoiceDate = line.Substring(118, 13).Trim();
                        rowData.EnterAmount = Convert.ToDecimal(line.Substring(144, 17).Trim().Replace(",", ""));
                        rowData.FunctionalAmount = Convert.ToDecimal(line.Substring(163, 17).Trim().Replace(",", ""));
                        listData.Add(rowData);
                        line = sr.ReadLine();
                    }
                }
            }
            var listDataGroup = (from a in listData
                                 group a by new { a.Legal, a.CusNumberNum } into g
                                 select new
                                 {
                                     LegalEntity = g.Key.Legal,
                                     CusNumberNum = g.Key.CusNumberNum,
                                     FunctionalAmount = g.Sum(c => c.FunctionalAmount)
                                 }).OrderBy(t => t.CusNumberNum).ThenByDescending(t => t.FunctionalAmount);

            List<T_CUSTOMER_AGING_DAYBACK> listAverageAR = new List<T_CUSTOMER_AGING_DAYBACK>();
            string[] strMonth = monthList.Split(',');
            for (int i = 0; i <= strMonth.Length - 1; i++)
            {
                DateTime monthDate = Convert.ToDateTime(strMonth[i]);
                var maxDate = CommonRep.GetQueryable<T_CUSTOMER_AGING_DAYBACK>()
                            .Where(o => o.BACK_DATE <= monthDate)
                            .OrderByDescending(t => t.BACK_DATE)
                            .Select(o => o.BACK_DATE).FirstOrDefault();

                if (maxDate != null && maxDate > Convert.ToDateTime("1900-01-01"))
                {
                    var averageAr = CommonRep.GetQueryable<T_CUSTOMER_AGING_DAYBACK>()
                            .Where(o => o.BACK_DATE == maxDate)
                            .Select(o => o).ToList();
                    listAverageAR.AddRange(averageAr);
                }
            }
            //按CustomerNo汇总
            var ReportData1 = (from a in listAverageAR
                               group a by new { a.LEGAL_ENTITY, a.CUSTOMER_NUM } into g
                               select new
                               {
                                   LegalEntity = g.Key.LEGAL_ENTITY,
                                   CUSTOMER_NUM = g.Key.CUSTOMER_NUM,
                                   AR_BALANCE_PERIOD = g.Sum(c => c.AR_BALANCE_PERIOD)
                               }).OrderBy(t => t.CUSTOMER_NUM).OrderBy(t => t.LegalEntity).ThenByDescending(t => t.AR_BALANCE_PERIOD).ToList();

            string templateName = "DSOReportTemplate";
            string outputPath = "DSOReportPath";
            var tplName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[templateName].ToString());
            var fileName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString());
            var pathName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString() + "DSOReport." + AppContext.Current.User.EID + ".xlsx");
            if (Directory.Exists(fileName) == false)
            {
                Directory.CreateDirectory(fileName);
            }
            List<SysTypeDetail> paymentTermDaysList = CommonRep.GetQueryable<SysTypeDetail>()
                            .Where(o => o.TypeCode == "041")
                            .Select(o => o).ToList();

            List<DsoAnalysisDto> listDso_Sheet1 = new List<DsoAnalysisDto>();
            foreach (var row in listDataGroup)
            {
                DsoAnalysisDto dsoRow = new DsoAnalysisDto();
                string strCustomerNum = row.CusNumberNum;
                var strCustomer = CommonRep.GetQueryable<Customer>()
                            .Where(o => o.CustomerNum == strCustomerNum)
                            .Select(o => new { o.CustomerName, o.CreditTrem }).FirstOrDefault();
                decimal decREV = row.FunctionalAmount;
                decimal decDSO = 0;
                var ReportData1Find = ReportData1.Find(o => o.LegalEntity == row.LegalEntity && o.CUSTOMER_NUM == strCustomerNum);
                if (ReportData1Find != null && decREV != 0)
                {
                    dsoRow.ARAvg = Convert.ToDecimal(ReportData1Find.AR_BALANCE_PERIOD == null ? 0 : ReportData1Find.AR_BALANCE_PERIOD / strMonth.Length);
                    decDSO = Convert.ToDecimal(ReportData1Find.AR_BALANCE_PERIOD / strMonth.Length / decREV * Convert.ToInt32(packageDays));
                }
                dsoRow.Legal = row.LegalEntity;
                dsoRow.CusNumberNum = strCustomerNum;
                dsoRow.DSO = decDSO;
                dsoRow.REV = decREV;
                int paymentTermDays = 0;
                dsoRow.PaymentTerm = "";
                if (strCustomer != null)
                {
                    dsoRow.CusName = strCustomer.CustomerName;
                    var paymentTerDaysFind = paymentTermDaysList.Find(o => o.DetailName.ToLower().Trim().Equals(strCustomer.CreditTrem.ToLower().Trim()));
                    if (paymentTerDaysFind != null)
                    {
                        paymentTermDays = Convert.ToInt32(paymentTerDaysFind.DetailValue);
                    }
                    dsoRow.PaymentTerm = strCustomer.CreditTrem + "(" + paymentTermDays + ")";
                }
                dsoRow.GAP = dsoRow.DSO - paymentTermDays;
                listDso_Sheet1.Add(dsoRow);
            }

            listDso_Sheet1 = listDso_Sheet1.OrderByDescending(o => o.DSO).ToList();

            WriteDsoDataToExcel(tplName, pathName, listDso_Sheet1);

            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }
            var virPatnName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[outputPath].ToString().Trim('~') + "DSOReport." + AppContext.Current.User.EID + ".xlsx";
            return virPatnName;

        }


        private void WriteDsoDataToExcel(string temp, string path, List<DsoAnalysisDto> listDso_Sheet1)
        {
            try
            {
                ExportService export = new ExportService(temp);
                export.Save(path, true);
                export = new ExportService(path);
                var sheetName1 = export.Sheets[0];
                export.ActiveSheetName = sheetName1;
                export.ExportDsoSheet1DataList(listDso_Sheet1);
                //Sheet2
                var sheetName2 = export.Sheets[1];
                export.ActiveSheetName = sheetName2;
                export.ExportDsoSheet2DataList(listDso_Sheet1);
                //Sheet3
                var sheetName3 = export.Sheets[2];
                export.ActiveSheetName = sheetName3;
                export.ExportDsoSheet3DataList(listDso_Sheet1);


            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获得目录下所有文件或指定文件类型文件(包含所有子文件夹)
        /// </summary>
        /// <param name="path">文件夹路径</param>
        /// <param name="extName">扩展名可以多个 例如 .mp3.wma.rm</param>
        /// <returns>List<FileInfo></returns>
        public List<FileInfo> getFile(string path, string extName)
        {
            List<FileInfo> lst = new List<FileInfo>();
            getdir(path, extName, ref lst);
            return lst;
        }

        public bool isNumberic(string message)
        {
            return Regex.IsMatch(message, @"^\d+$");
        }
        /// <summary>
        /// 私有方法,递归获取指定类型文件,包含子文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <param name="extName"></param>
        private static void getdir(string path, string extName, ref List<FileInfo> lst)
        {
            try
            {
                string[] dir = Directory.GetDirectories(path); //文件夹列表   
                DirectoryInfo fdir = new DirectoryInfo(path);
                FileInfo[] file = fdir.GetFiles();
                if (file.Length != 0 || dir.Length != 0) //当前目录文件或文件夹不为空     
                {
                    foreach (FileInfo f in file) //显示当前目录所有文件   
                    {
                        if (extName.ToLower().IndexOf(f.Extension.ToLower()) >= 0)
                        {
                            lst.Add(f);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }


        public string excelToList(string archiveFileName, String FileType, ref List<T_INVOICE_STATUS_STAGING> listInvoiceStatus, ref List<T_Customer_Comments> listCustomerComments)
        {
            string strErrmsg = "";
            int i = 11;
            string strINVOICE_DATE = "", strINVOICE_DATE_PRE = "";
            string strINVOICE_NO = "", strINVOICE_NO_PRE = "", strINVOICE_NO_FACT = "";
            string strINVOICE_CLASS = "", strINVOICE_CLASS_PRE = "";
            string strINVOICE_AMOUNT = "", strINVOICE_AMOUNT_PRE = "";
            string strBalance_Status = "";
            string strBalance_Memo = "";
            string strINVOICE_DUEDATE = "", strINVOICE_DUEDATE_PRE = "";
            string strINVOICE_PTPDATE = "", strINVOICE_PTPDATE_PRE = "";
            string strINVOICE_DISPUTE = "", strINVOICE_DISPUTE_PRE = "";
            string strINVOICE_OTHER = "", strINVOICE_OTHER_PRE = "";
            string strINVOICE_IsForwarder = "", strINVOICE_IsForwarder_PRE = "";
            string strINVOICE_Forwarder = "", strINVOICE_Forwarder_PRE = "";
            string strAMOUNT = "", strAMOUNT_PRE = "";
            string strLineNo = "";
            string strMaterialNo = "";
            string currencyCode = "";
            string dueReason = "";
            string consignmentNumber = "";
            string strMemoExpirationDate = "";
            decimal decMaterialAmount = 0;
            string strSiteUseId = "";
            string strLegalEntity = "";

            NpoiHelper helper = new NpoiHelper(archiveFileName);
            helper.ActiveSheet = 1;
            var maxRowNumber = helper.GetLastRowNum();

            try
            {
                switch (FileType)
                {
                    case "SOA": //SOA_NotCN_WAV
                        //明细页
                        i = 6;
                        do
                        {
                            strSiteUseId = helper.GetValue(i, 7) == null ? "" : helper.GetValue(i, 7).ToString();

                            //invoice no
                            strINVOICE_NO = helper.GetValue(i, 9) == null ? "" : helper.GetValue(i, 9).ToString();

                            //invocie data
                            ICell cell_invoiceDate = helper.GetCell(i, 10);
                            if (cell_invoiceDate != null)
                            {
                                if (cell_invoiceDate.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell_invoiceDate))
                                {
                                    strINVOICE_DATE = cell_invoiceDate.DateCellValue == null ? "" : cell_invoiceDate.DateCellValue.ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    strINVOICE_DATE = helper.GetValue(i, 10) == null ? "" : helper.GetValue(i, 10).ToString();
                                }
                            }
                            else
                            {
                                strINVOICE_DATE = helper.GetValue(i, 10) == null ? "" : helper.GetValue(i, 10).ToString();
                            }

                            ICell cell_invoiceDueDate = helper.GetCell(i, 11);
                            if (cell_invoiceDueDate != null)
                            {
                                if (cell_invoiceDate.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell_invoiceDueDate))
                                {
                                    strINVOICE_DUEDATE = cell_invoiceDueDate.DateCellValue == null ? "" : cell_invoiceDueDate.DateCellValue.ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    strINVOICE_DUEDATE = helper.GetValue(i, 11) == null ? "" : helper.GetValue(i, 11).ToString();
                                }
                            }
                            else
                            {
                                strINVOICE_DUEDATE = helper.GetValue(i, 11) == null ? "" : helper.GetValue(i, 11).ToString();
                            }



                            //invocie class
                            strINVOICE_CLASS = helper.GetValue(i, 8) == null ? "" : helper.GetValue(i, 8).ToString();

                            currencyCode = helper.GetValue(i, 13) == null ? "" : helper.GetValue(i, 13).ToString();
                            strINVOICE_AMOUNT = helper.GetValue(i, 15) == null ? "" : helper.GetValue(i, 15).ToString();

                            // ptp date
                            ICell cell = helper.GetCell(i, 2);
                            if (cell != null)
                            {
                                if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                {
                                    strINVOICE_PTPDATE = cell.DateCellValue == null ? "" : cell.DateCellValue.ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    strINVOICE_PTPDATE = helper.GetValue(i, 2) == null ? null : helper.GetValue(i, 2).ToString();
                                }
                            }
                            else
                            {
                                strINVOICE_PTPDATE = helper.GetValue(i, 2) == null ? null : helper.GetValue(i, 2).ToString();
                            }

                            dueReason = helper.GetValue(i, 3) == null ? "" : helper.GetValue(i, 3).ToString();
                            strBalance_Memo = helper.GetValue(i, 4) == null ? "" : helper.GetValue(i, 4).ToString();
                            // Memo Expiration Date
                            cell = helper.GetCell(i, 25);
                            if (cell != null && !string.IsNullOrWhiteSpace(strBalance_Memo))
                            {
                                if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                {
                                    strMemoExpirationDate = cell.DateCellValue == null ? "" : cell.DateCellValue.ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    strMemoExpirationDate = helper.GetValue(i, 25) == null ? null : helper.GetValue(i, 25).ToString();
                                }
                            }
                            else
                            {
                                strMemoExpirationDate = helper.GetValue(i, 25) == null || string.IsNullOrWhiteSpace(strBalance_Memo) ? null : helper.GetValue(i, 25).ToString();
                            }

                            //Invoice no 为空时不处理, PMT 的不处理
                            if (!string.IsNullOrEmpty(strINVOICE_NO))
                            {
                                T_INVOICE_STATUS_STAGING itemInvoiceStatus = new T_INVOICE_STATUS_STAGING();
                                itemInvoiceStatus.FILETYPE = FileType;
                                itemInvoiceStatus.INVOICE_CLASS = strINVOICE_CLASS;
                                itemInvoiceStatus.INVOICE_NO = strINVOICE_NO;
                                itemInvoiceStatus.INVOICE_DATE = Convert.ToDateTime(strINVOICE_DATE);
                                itemInvoiceStatus.INVOICE_DUEDATE = Convert.ToDateTime(strINVOICE_DATE);
                                itemInvoiceStatus.INVOICE_CurrencyCode = currencyCode;
                                itemInvoiceStatus.INVOICE_AMOUNT = Convert.ToDecimal(strINVOICE_AMOUNT);
                                if (!string.IsNullOrWhiteSpace(strINVOICE_PTPDATE))
                                {
                                    try
                                    {
                                        itemInvoiceStatus.INVOICE_PTPDATE = Convert.ToDateTime(strINVOICE_PTPDATE);
                                    }
                                    catch (Exception ex)
                                    {
                                        strErrmsg = "Row:" + (i + 1) + ",PtpDate invalid. ";
                                        return strErrmsg;
                                    }
                                }

                                itemInvoiceStatus.DueReason = dueReason;
                                itemInvoiceStatus.INVOICE_BalanceMemo = strBalance_Memo;
                                itemInvoiceStatus.CREATE_USER = AppContext.Current.User.EID;
                                itemInvoiceStatus.CREATE_DATE = AppContext.Current.User.Now;
                                itemInvoiceStatus.SiteUseId = strSiteUseId;
                                if (string.IsNullOrEmpty(strMemoExpirationDate))
                                {
                                    itemInvoiceStatus.MemoExpirationDate = null;
                                }
                                else
                                {
                                    itemInvoiceStatus.MemoExpirationDate = Convert.ToDateTime(strMemoExpirationDate);
                                }
                                listInvoiceStatus.Add(itemInvoiceStatus);
                            }
                            i++;
                        } while (i <= maxRowNumber);

                        break;
                    case "SOA-CN": //SOA_CN_WAV
                        //明细页
                        i = 6;
                        do
                        {
                            strSiteUseId = helper.GetValue(i, 7) == null ? "" : helper.GetValue(i, 7).ToString();

                            //invoice no
                            strINVOICE_NO = helper.GetValue(i, 9) == null ? "" : helper.GetValue(i, 9).ToString();

                            //invocie data
                            ICell cell_invoiceDate = helper.GetCell(i, 10);
                            if (cell_invoiceDate != null)
                            {
                                if (cell_invoiceDate.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell_invoiceDate))
                                {
                                    strINVOICE_DATE = cell_invoiceDate.DateCellValue == null ? "" : cell_invoiceDate.DateCellValue.ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    strINVOICE_DATE = helper.GetValue(i, 10) == null ? "" : helper.GetValue(i, 10).ToString();
                                }
                            }
                            else
                            {
                                strINVOICE_DATE = helper.GetValue(i, 10) == null ? "" : helper.GetValue(i, 10).ToString();
                            }

                            ICell cell_invoiceDueDate = helper.GetCell(i, 11);
                            if (cell_invoiceDueDate != null)
                            {
                                if (cell_invoiceDate.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell_invoiceDueDate))
                                {
                                    strINVOICE_DUEDATE = cell_invoiceDueDate.DateCellValue == null ? "" : cell_invoiceDueDate.DateCellValue.ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    strINVOICE_DUEDATE = helper.GetValue(i, 11) == null ? "" : helper.GetValue(i, 11).ToString();
                                }
                            }
                            else
                            {
                                strINVOICE_DUEDATE = helper.GetValue(i, 11) == null ? "" : helper.GetValue(i, 11).ToString();
                            }



                            //invocie class
                            strINVOICE_CLASS = helper.GetValue(i, 8) == null ? "" : helper.GetValue(i, 8).ToString();

                            currencyCode = helper.GetValue(i, 13) == null ? "" : helper.GetValue(i, 13).ToString();
                            strINVOICE_AMOUNT = helper.GetValue(i, 15) == null ? "" : helper.GetValue(i, 15).ToString();

                            // ptp date
                            ICell cell = helper.GetCell(i, 2);
                            if (cell != null)
                            {
                                if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                {
                                    strINVOICE_PTPDATE = cell.DateCellValue == null ? "" : cell.DateCellValue.ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    strINVOICE_PTPDATE = helper.GetValue(i, 2) == null ? null : helper.GetValue(i, 2).ToString();
                                }
                            }
                            else
                            {
                                strINVOICE_PTPDATE = helper.GetValue(i, 2) == null ? null : helper.GetValue(i, 2).ToString();
                            }

                            dueReason = helper.GetValue(i, 3) == null ? "" : helper.GetValue(i, 3).ToString();
                            strBalance_Memo = helper.GetValue(i, 4) == null ? "" : helper.GetValue(i, 4).ToString();
                            // Memo Expiration Date
                            cell = helper.GetCell(i, 25);
                            if (cell != null && !string.IsNullOrWhiteSpace(strBalance_Memo))
                            {
                                if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                {
                                    strMemoExpirationDate = cell.DateCellValue == null ? "" : cell.DateCellValue.ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    strMemoExpirationDate = helper.GetValue(i, 25) == null ? null : helper.GetValue(i, 25).ToString();
                                }
                            }
                            else
                            {
                                strMemoExpirationDate = helper.GetValue(i, 25) == null || string.IsNullOrWhiteSpace(strBalance_Memo) ? null : helper.GetValue(i, 25).ToString();
                            }

                            //Invoice no 为空时不处理, PMT 的不处理
                            if (!string.IsNullOrEmpty(strINVOICE_NO))
                            {
                                T_INVOICE_STATUS_STAGING itemInvoiceStatus = new T_INVOICE_STATUS_STAGING();
                                itemInvoiceStatus.FILETYPE = FileType;
                                itemInvoiceStatus.INVOICE_CLASS = strINVOICE_CLASS;
                                itemInvoiceStatus.INVOICE_NO = strINVOICE_NO;
                                itemInvoiceStatus.INVOICE_DATE = Convert.ToDateTime(strINVOICE_DATE);
                                itemInvoiceStatus.INVOICE_DUEDATE = Convert.ToDateTime(strINVOICE_DATE);
                                itemInvoiceStatus.INVOICE_CurrencyCode = currencyCode;
                                itemInvoiceStatus.INVOICE_AMOUNT = Convert.ToDecimal(strINVOICE_AMOUNT);
                                if (!string.IsNullOrWhiteSpace(strINVOICE_PTPDATE))
                                {
                                    try
                                    {
                                        itemInvoiceStatus.INVOICE_PTPDATE = Convert.ToDateTime(strINVOICE_PTPDATE);
                                    }
                                    catch (Exception ex)
                                    {
                                        strErrmsg = "Row:" + (i + 1) + ",PtpDate invalid. ";
                                        return strErrmsg;
                                    }
                                }

                                itemInvoiceStatus.DueReason = dueReason;
                                itemInvoiceStatus.INVOICE_BalanceMemo = strBalance_Memo;
                                itemInvoiceStatus.CREATE_USER = AppContext.Current.User.EID;
                                itemInvoiceStatus.CREATE_DATE = AppContext.Current.User.Now;
                                itemInvoiceStatus.SiteUseId = strSiteUseId;
                                if (string.IsNullOrEmpty(strMemoExpirationDate))
                                {
                                    itemInvoiceStatus.MemoExpirationDate = null;
                                }
                                else
                                {
                                    itemInvoiceStatus.MemoExpirationDate = Convert.ToDateTime(strMemoExpirationDate);
                                }
                                listInvoiceStatus.Add(itemInvoiceStatus);
                            }
                            i++;
                        } while (i <= maxRowNumber);

                        //Summary页
                        helper.ActiveSheet = 0;
                        var maxRowNumberSummary = helper.GetLastRowNum();
                        int j = 6;
                        bool lb_find = false;
                        string strCustomerNum = helper.GetValue(6, 4) == null ? "" : helper.GetValue(6, 4).ToString();
                        do
                        {
                            string strSiteUseIdHead = helper.GetValue(j, 1) == null ? "" : helper.GetValue(j, 1).ToString();
                            if (strSiteUseIdHead == "逾期原因中英文参照") { break; }
                            if (lb_find && strSiteUseIdHead != "") {
                                try
                                {
                                    T_Customer_Comments commentsItem = new T_Customer_Comments();
                                    commentsItem.CUSTOMER_NUM = strCustomerNum;
                                    commentsItem.SiteUseId = strSiteUseIdHead;
                                    string strAgingBucket = helper.GetValue(j, 4) == null ? "" : helper.GetValue(j, 4).ToString();
                                    commentsItem.AgingBucket = strAgingBucket;
                                    string strPTPDate = "";
                                    ICell cell = helper.GetCell(j, 6);
                                    if (cell != null)
                                    {
                                        if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                        {
                                            strPTPDate = cell.DateCellValue == null ? "" : cell.DateCellValue.ToString("yyyy/MM/dd");
                                        }
                                        else
                                        {
                                            strPTPDate = helper.GetValue(j, 6) == null ? null : helper.GetValue(j, 6).ToString();
                                        }
                                    }
                                    else
                                    {
                                        strPTPDate = helper.GetValue(j, 6) == null ? null : helper.GetValue(j, 6).ToString();
                                    }
                                    if(strPTPDate != null) { 
                                        DateTime dtPTPDate = Convert.ToDateTime(strPTPDate);
                                        if (dtPTPDate != Convert.ToDateTime("1900-01-01"))
                                        {
                                            commentsItem.PTPDATE = dtPTPDate;
                                        }
                                    }
                                    if (helper.GetValue(j, 5) != null) { 
                                        double decPTPAmount = helper.GetNumericData(j, 5);
                                        commentsItem.PTPAmount = Convert.ToDecimal(decPTPAmount);
                                    }
                                    string strOverDueReason = helper.GetValue(j, 7) == null ? "" : helper.GetValue(j, 7).ToString();
                                    commentsItem.OverdueReason = strOverDueReason;
                                    string strComments = helper.GetValue(j, 8) == null ? "" : helper.GetValue(j, 8).ToString();
                                    commentsItem.Comments = strComments;
                                    string strCommentsFrom = helper.GetValue(j, 9) == null ? "" : helper.GetValue(j, 9).ToString();
                                    commentsItem.CommentsFrom = strCommentsFrom;
                                    listCustomerComments.Add(commentsItem);
                                }
                                catch (Exception ex) {
                                    strErrmsg = "Row:" + (j + 1) + ",数据格式错误. ";
                                    return strErrmsg;
                                }
                            }
                            if (strSiteUseIdHead == "Site#")
                            {
                                lb_find = true;
                            }
                            j++;
                        } while (j <= maxRowNumber) ;

                        break;

                    case "SOA-SAP": //SOA_SAP_WAV
                        i = 6;
                        do
                        {
                            strLegalEntity = helper.GetValue(i, 5) == null ? "" : helper.GetValue(i, 5).ToString();

                            strSiteUseId = helper.GetValue(i, 8) == null ? "" : helper.GetValue(i, 8).ToString();

                            strSiteUseId = strLegalEntity + "_" + strSiteUseId;

                            //invoice no
                            strINVOICE_NO = helper.GetValue(i, 9) == null ? "" : helper.GetValue(i, 9).ToString();

                            //invocie data
                            ICell cell_invoiceDate = helper.GetCell(i, 12);
                            if (cell_invoiceDate != null)
                            {
                                if (cell_invoiceDate.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell_invoiceDate))
                                {
                                    strINVOICE_DATE = cell_invoiceDate.DateCellValue == null ? "" : cell_invoiceDate.DateCellValue.ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    strINVOICE_DATE = helper.GetValue(i, 12) == null ? "" : helper.GetValue(i, 12).ToString();
                                }
                            }
                            else
                            {
                                strINVOICE_DATE = helper.GetValue(i, 12) == null ? "" : helper.GetValue(i, 12).ToString();
                            }

                            ICell cell_invoiceDueDate = helper.GetCell(i, 13);
                            if (cell_invoiceDueDate != null)
                            {
                                if (cell_invoiceDate.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell_invoiceDueDate))
                                {
                                    strINVOICE_DUEDATE = cell_invoiceDueDate.DateCellValue == null ? "" : cell_invoiceDueDate.DateCellValue.ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    strINVOICE_DUEDATE = helper.GetValue(i, 13) == null ? "" : helper.GetValue(i, 13).ToString();
                                }
                            }
                            else
                            {
                                strINVOICE_DUEDATE = helper.GetValue(i, 13) == null ? "" : helper.GetValue(i, 13).ToString();
                            }

                            strINVOICE_AMOUNT = helper.GetValue(i, 15) == null ? "" : helper.GetValue(i, 15).ToString();

                            // ptp date
                            ICell cell = helper.GetCell(i, 2);
                            if (cell != null)
                            {
                                if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                {
                                    strINVOICE_PTPDATE = cell.DateCellValue == null ? "" : cell.DateCellValue.ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    strINVOICE_PTPDATE = helper.GetValue(i, 2) == null ? null : helper.GetValue(i, 2).ToString();
                                }
                            }
                            else
                            {
                                strINVOICE_PTPDATE = helper.GetValue(i, 2) == null ? null : helper.GetValue(i, 2).ToString();
                            }

                            dueReason = helper.GetValue(i, 3) == null ? "" : helper.GetValue(i, 3).ToString();
                            strBalance_Memo = helper.GetValue(i, 4) == null ? "" : helper.GetValue(i, 4).ToString();

                            // Memo Expiration Date
                            cell = helper.GetCell(i, 18);
                            if (cell != null && !string.IsNullOrWhiteSpace(strBalance_Memo))
                            {
                                if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                {
                                    strMemoExpirationDate = cell.DateCellValue == null ? "" : cell.DateCellValue.ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    strMemoExpirationDate = helper.GetValue(i, 18) == null ? null : helper.GetValue(i, 18).ToString();
                                }
                            }
                            else
                            {
                                strMemoExpirationDate = helper.GetValue(i, 18) == null || string.IsNullOrWhiteSpace(strBalance_Memo) ? null : helper.GetValue(i, 18).ToString();
                            }

                            //Invoice no 为空时不处理
                            if (!string.IsNullOrEmpty(strINVOICE_NO))
                            {
                                T_INVOICE_STATUS_STAGING itemInvoiceStatus = new T_INVOICE_STATUS_STAGING();
                                itemInvoiceStatus.FILETYPE = FileType;
                                itemInvoiceStatus.INVOICE_CLASS = strINVOICE_CLASS;
                                itemInvoiceStatus.INVOICE_NO = strINVOICE_NO;
                                itemInvoiceStatus.INVOICE_DATE = Convert.ToDateTime(strINVOICE_DATE);
                                itemInvoiceStatus.INVOICE_DUEDATE = Convert.ToDateTime(strINVOICE_DATE);
                                itemInvoiceStatus.INVOICE_AMOUNT = Convert.ToDecimal(strINVOICE_AMOUNT);
                                if (!string.IsNullOrWhiteSpace(strINVOICE_PTPDATE))
                                {
                                    try
                                    {
                                        itemInvoiceStatus.INVOICE_PTPDATE = Convert.ToDateTime(strINVOICE_PTPDATE);
                                    }
                                    catch (Exception ex) {
                                        strErrmsg = "Row:" + (i + 1) + ",PtpDate invalid. ";
                                        return strErrmsg;
                                    }
                                }

                                itemInvoiceStatus.DueReason = dueReason;
                                itemInvoiceStatus.INVOICE_BalanceMemo = strBalance_Memo;
                                itemInvoiceStatus.CREATE_USER = AppContext.Current.User.EID;
                                itemInvoiceStatus.CREATE_DATE = AppContext.Current.User.Now;
                                itemInvoiceStatus.SiteUseId = strSiteUseId;
                                if (string.IsNullOrEmpty(strMemoExpirationDate))
                                {
                                    itemInvoiceStatus.MemoExpirationDate = null;
                                }
                                else
                                {
                                    itemInvoiceStatus.MemoExpirationDate = Convert.ToDateTime(strMemoExpirationDate);
                                }
                                listInvoiceStatus.Add(itemInvoiceStatus);
                            }
                            i++;
                        } while (i <= maxRowNumber);

                        break;
                    case "SOA-India|Asean": //SOA_ASEAN_WAV
                        var hasConsignmentNumber = helper.GetValue(5, 10) == null ? false : helper.GetValue(5, 10).ToString().Contains("Consignment Number");

                         i = 6;
                        do
                        {
                            //invoice no
                            strINVOICE_NO = helper.GetValue(i, 7) == null ? "" : helper.GetValue(i, 7).ToString();
                            if(!string.IsNullOrEmpty(strINVOICE_NO) && strINVOICE_NO != "Arrow Ref.No")
                            { 
                                //invocie data
                                ICell cell_invoiceDate = helper.GetCell(i, 2);
                                if (cell_invoiceDate != null)
                                {
                                    if (cell_invoiceDate.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell_invoiceDate))
                                    {
                                        strINVOICE_DATE = cell_invoiceDate.DateCellValue == null ? "" : cell_invoiceDate.DateCellValue.ToString("yyyy/MM/dd");
                                    }
                                    else
                                    {
                                        strINVOICE_DATE = helper.GetValue(i, 2) == null ? "" : helper.GetValue(i, 2).ToString();
                                    }
                                }
                                else
                                {
                                    strINVOICE_DATE = helper.GetValue(i, 2) == null ? "" : helper.GetValue(i, 2).ToString();
                                }

                                ICell cell_invoiceDueDate = helper.GetCell(i, 3);
                                if (cell_invoiceDueDate != null)
                                {
                                    if (cell_invoiceDate.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell_invoiceDueDate))
                                    {
                                        strINVOICE_DUEDATE = cell_invoiceDueDate.DateCellValue == null ? "" : cell_invoiceDueDate.DateCellValue.ToString("yyyy/MM/dd");
                                    }
                                    else
                                    {
                                        strINVOICE_DUEDATE = helper.GetValue(i, 3) == null ? "" : helper.GetValue(i, 3).ToString();
                                    }
                                }
                                else
                                {
                                    strINVOICE_DUEDATE = helper.GetValue(i, 3) == null ? "" : helper.GetValue(i, 3).ToString();
                                }

                                //invocie class
                                strINVOICE_CLASS = helper.GetValue(i, 5) == null ? "" : helper.GetValue(i, 5).ToString();

                                strINVOICE_AMOUNT = helper.GetValue(i, 9) == null ? "" : helper.GetValue(i, 9).ToString();


                                if (hasConsignmentNumber)
                                {
                                    // Consignment Number
                                    consignmentNumber = helper.GetValue(i, 10) == null ? "" : helper.GetValue(i, 10).ToString();
                                    // ptp date
                                    ICell cell = helper.GetCell(i, 11);
                                    if (cell != null)
                                    {
                                        if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                        {
                                            strINVOICE_PTPDATE = cell.DateCellValue == null ? "" : cell.DateCellValue.ToString("yyyy/MM/dd");
                                        }
                                        else
                                        {
                                            strINVOICE_PTPDATE = helper.GetValue(i, 11) == null ? null : helper.GetValue(i, 11).ToString();
                                        }
                                    }
                                    else
                                    {
                                        strINVOICE_PTPDATE = helper.GetValue(i, 11) == null ? null : helper.GetValue(i, 11).ToString();
                                    }

                                    dueReason = helper.GetValue(i, 12) == null ? "" : helper.GetValue(i, 12).ToString();
                                    strBalance_Memo = helper.GetValue(i, 13) == null ? "" : helper.GetValue(i, 13).ToString();

                                    // Memo Expiration Date
                                    cell = helper.GetCell(i, 14);
                                    if (cell != null && !string.IsNullOrWhiteSpace(strBalance_Memo))
                                    {
                                        if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                        {
                                            strMemoExpirationDate = cell.DateCellValue == null ? "" : cell.DateCellValue.ToString("yyyy/MM/dd");
                                        }
                                        else
                                        {
                                            strMemoExpirationDate = helper.GetValue(i, 14) == null ? null : helper.GetValue(i, 14).ToString();
                                        }
                                    }
                                    else
                                    {
                                        strMemoExpirationDate = helper.GetValue(i, 14) == null || string.IsNullOrWhiteSpace(strBalance_Memo) ? null : helper.GetValue(i, 14).ToString();
                                    }
                                }
                                else
                                {
                                    // ptp date
                                    ICell cell = helper.GetCell(i, 10);
                                    if (cell != null)
                                    {
                                        if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                        {
                                            strINVOICE_PTPDATE = cell.DateCellValue == null ? "" : cell.DateCellValue.ToString("yyyy/MM/dd");
                                        }
                                        else
                                        {
                                            strINVOICE_PTPDATE = helper.GetValue(i, 10) == null ? null : helper.GetValue(i, 10).ToString();
                                        }
                                    }
                                    else
                                    {
                                        strINVOICE_PTPDATE = helper.GetValue(i, 10) == null ? null : helper.GetValue(i, 10).ToString();
                                    }

                                    dueReason = helper.GetValue(i, 11) == null ? "" : helper.GetValue(i, 11).ToString();
                                    strBalance_Memo = helper.GetValue(i, 12) == null ? "" : helper.GetValue(i, 12).ToString();
                                    // Memo Expiration Date
                                    cell = helper.GetCell(i, 13);
                                    if (cell != null && !string.IsNullOrWhiteSpace(strBalance_Memo))
                                    {
                                        if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                        {
                                            strMemoExpirationDate = cell.DateCellValue == null ? "" : cell.DateCellValue.ToString("yyyy/MM/dd");
                                        }
                                        else
                                        {
                                            strMemoExpirationDate = helper.GetValue(i, 13) == null ? null : helper.GetValue(i, 13).ToString();
                                        }
                                    }
                                    else
                                    {
                                        strMemoExpirationDate = helper.GetValue(i, 13) == null || string.IsNullOrWhiteSpace(strBalance_Memo) ? null : helper.GetValue(i, 13).ToString();
                                    }
                                }
                                //Invoice no 为空时不处理, PMT 的不处理
                                if (!string.IsNullOrEmpty(strINVOICE_NO))
                                {
                                    T_INVOICE_STATUS_STAGING itemInvoiceStatus = new T_INVOICE_STATUS_STAGING();
                                    itemInvoiceStatus.FILETYPE = FileType;
                                    itemInvoiceStatus.INVOICE_CLASS = strINVOICE_CLASS;
                                    itemInvoiceStatus.INVOICE_NO = strINVOICE_NO;
                                    itemInvoiceStatus.INVOICE_DATE = Convert.ToDateTime(strINVOICE_DATE);
                                    itemInvoiceStatus.INVOICE_DUEDATE = Convert.ToDateTime(strINVOICE_DATE);
                                    itemInvoiceStatus.INVOICE_AMOUNT = Convert.ToDecimal(strINVOICE_AMOUNT);
                                    if (!string.IsNullOrWhiteSpace(strINVOICE_PTPDATE))
                                    {
                                        try
                                        {
                                            itemInvoiceStatus.INVOICE_PTPDATE = Convert.ToDateTime(strINVOICE_PTPDATE);
                                        }
                                        catch (Exception ex)
                                        {
                                            strErrmsg = "Row:" + (i + 1) + ",PtpDate invalid. ";
                                            return strErrmsg;
                                        }
                                    }

                                    itemInvoiceStatus.DueReason = dueReason;
                                    itemInvoiceStatus.INVOICE_BalanceMemo = strBalance_Memo;
                                    itemInvoiceStatus.CREATE_USER = AppContext.Current.User.EID;
                                    itemInvoiceStatus.CREATE_DATE = AppContext.Current.User.Now;
                                    //根据INV获得SiteUseId
                                    strSiteUseId = CommonRep.GetDbSet<InvoiceAging>().Where(o => o.InvoiceNum == strINVOICE_NO).Select(o=>o.SiteUseId).FirstOrDefault();
                                    itemInvoiceStatus.SiteUseId = strSiteUseId;
                                    itemInvoiceStatus.ConsignmentNumber = consignmentNumber;
                                    if (string.IsNullOrEmpty(strMemoExpirationDate))
                                    {
                                        itemInvoiceStatus.MemoExpirationDate = null;
                                    }
                                    else
                                    {
                                        itemInvoiceStatus.MemoExpirationDate = Convert.ToDateTime(strMemoExpirationDate);
                                    }
                                    listInvoiceStatus.Add(itemInvoiceStatus);
                                }
                            }
                            i++;
                        } while (i <= maxRowNumber);
                        break;
                    case "SOA-HK":  //SOA_HK_WAV
                        i = 6;
                        do
                        {
                            strSiteUseId = helper.GetValue(i, 4) == null ? "" : helper.GetValue(i, 4).ToString();

                            //invoice no
                            strINVOICE_NO = helper.GetValue(i, 8) == null ? "" : helper.GetValue(i, 8).ToString();

                            //invocie data
                            ICell cell_invoiceDate = helper.GetCell(i, 5);
                            if (cell_invoiceDate != null)
                            {
                                if (cell_invoiceDate.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell_invoiceDate))
                                {
                                    strINVOICE_DATE = cell_invoiceDate.DateCellValue == null ? "" : cell_invoiceDate.DateCellValue.ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    strINVOICE_DATE = helper.GetValue(i, 5) == null ? "" : helper.GetValue(i, 5).ToString();
                                }
                            }
                            else
                            {
                                strINVOICE_DATE = helper.GetValue(i, 5) == null ? "" : helper.GetValue(i, 5).ToString();
                            }

                            ICell cell_invoiceDueDate = helper.GetCell(i, 6);
                            if (cell_invoiceDueDate != null)
                            {
                                if (cell_invoiceDate.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell_invoiceDueDate))
                                {
                                    strINVOICE_DUEDATE = cell_invoiceDueDate.DateCellValue == null ? "" : cell_invoiceDueDate.DateCellValue.ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    strINVOICE_DUEDATE = helper.GetValue(i, 6) == null ? "" : helper.GetValue(i, 6).ToString();
                                }
                            }
                            else
                            {
                                strINVOICE_DUEDATE = helper.GetValue(i, 6) == null ? "" : helper.GetValue(i, 6).ToString();
                            }



                            //invocie class
                            strINVOICE_CLASS = helper.GetValue(i, 7) == null ? "" : helper.GetValue(i, 7).ToString();

                            currencyCode = helper.GetValue(i, 12) == null ? "" : helper.GetValue(i, 12).ToString();
                            strINVOICE_AMOUNT = helper.GetValue(i, 13) == null ? "" : helper.GetValue(i, 13).ToString();

                            // ptp date
                            ICell cell = helper.GetCell(i, 14);
                            if (cell != null)
                            {
                                if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                {
                                    strINVOICE_PTPDATE = cell.DateCellValue == null ? "" : cell.DateCellValue.ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    strINVOICE_PTPDATE = helper.GetValue(i, 14) == null ? null : helper.GetValue(i, 14).ToString();
                                }
                            }
                            else
                            {
                                strINVOICE_PTPDATE = helper.GetValue(i, 14) == null ? null : helper.GetValue(i, 14).ToString();
                            }

                            dueReason = helper.GetValue(i, 15) == null ? "" : helper.GetValue(i, 15).ToString();
                            strBalance_Memo = helper.GetValue(i, 16) == null ? "" : helper.GetValue(i, 16).ToString();

                            // Memo Expiration Date
                            cell = helper.GetCell(i, 23);
                            if (cell != null && !string.IsNullOrWhiteSpace(strBalance_Memo))
                            {
                                if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                {
                                    strMemoExpirationDate = cell.DateCellValue == null ? "" : cell.DateCellValue.ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    strMemoExpirationDate = helper.GetValue(i, 23) == null ? null : helper.GetValue(i, 23).ToString();
                                }
                            }
                            else
                            {
                                strMemoExpirationDate = helper.GetValue(i, 23) == null || string.IsNullOrWhiteSpace(strBalance_Memo) ? null : helper.GetValue(i, 23).ToString();
                            }

                            //Invoice no 为空时不处理, PMT 的不处理
                            if (!string.IsNullOrEmpty(strINVOICE_NO))
                            {
                                T_INVOICE_STATUS_STAGING itemInvoiceStatus = new T_INVOICE_STATUS_STAGING();
                                itemInvoiceStatus.FILETYPE = FileType;
                                itemInvoiceStatus.INVOICE_CLASS = strINVOICE_CLASS;
                                itemInvoiceStatus.INVOICE_NO = strINVOICE_NO;
                                itemInvoiceStatus.INVOICE_DATE = Convert.ToDateTime(strINVOICE_DATE);
                                itemInvoiceStatus.INVOICE_DUEDATE = Convert.ToDateTime(strINVOICE_DATE);
                                itemInvoiceStatus.INVOICE_CurrencyCode = currencyCode;
                                itemInvoiceStatus.INVOICE_AMOUNT = Convert.ToDecimal(strINVOICE_AMOUNT);
                                if (!string.IsNullOrWhiteSpace(strINVOICE_PTPDATE))
                                {
                                    try
                                    {
                                        itemInvoiceStatus.INVOICE_PTPDATE = Convert.ToDateTime(strINVOICE_PTPDATE);
                                    }
                                    catch (Exception ex)
                                    {
                                        strErrmsg = "Row:" + (i + 1) + ",PtpDate invalid. ";
                                        return strErrmsg;
                                    }
                                }

                                itemInvoiceStatus.DueReason = dueReason;
                                itemInvoiceStatus.INVOICE_BalanceMemo = strBalance_Memo;
                                itemInvoiceStatus.CREATE_USER = AppContext.Current.User.EID;
                                itemInvoiceStatus.CREATE_DATE = AppContext.Current.User.Now;
                                itemInvoiceStatus.SiteUseId = strSiteUseId;
                                if (string.IsNullOrEmpty(strMemoExpirationDate))
                                {
                                    itemInvoiceStatus.MemoExpirationDate = null;
                                }
                                else
                                {
                                    itemInvoiceStatus.MemoExpirationDate = Convert.ToDateTime(strMemoExpirationDate);
                                }
                                listInvoiceStatus.Add(itemInvoiceStatus);
                            }
                            i++;
                        } while (i <= maxRowNumber);
                        break;

                    case "ANZ": //ANZ
                        i = 6;
                        do
                        {
                            //invoice no
                            strINVOICE_NO = helper.GetValue(i, 7) == null ? "" : helper.GetValue(i, 7).ToString();
                            if (string.IsNullOrEmpty(strINVOICE_NO)) { break; }
                            if (!string.IsNullOrEmpty(strINVOICE_NO) && strINVOICE_NO != "Arrow Ref.No")
                            {
                                //invocie data
                                ICell cell_invoiceDate = helper.GetCell(i, 2);
                                if (cell_invoiceDate != null)
                                {
                                    if (cell_invoiceDate.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell_invoiceDate))
                                    {
                                        strINVOICE_DATE = cell_invoiceDate.DateCellValue == null ? "" : cell_invoiceDate.DateCellValue.ToString("yyyy/MM/dd");
                                    }
                                    else
                                    {
                                        strINVOICE_DATE = helper.GetValue(i, 2) == null ? "" : helper.GetValue(i, 2).ToString();
                                    }
                                }
                                else
                                {
                                    strINVOICE_DATE = helper.GetValue(i, 2) == null ? "" : helper.GetValue(i, 2).ToString();
                                }
                                //DueDate
                                ICell cell_invoiceDueDate = helper.GetCell(i, 3);
                                if (cell_invoiceDueDate != null)
                                {
                                    if (cell_invoiceDate.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell_invoiceDueDate))
                                    {
                                        strINVOICE_DUEDATE = cell_invoiceDueDate.DateCellValue == null ? "" : cell_invoiceDueDate.DateCellValue.ToString("yyyy/MM/dd");
                                    }
                                    else
                                    {
                                        strINVOICE_DUEDATE = helper.GetValue(i, 3) == null ? "" : helper.GetValue(i, 3).ToString();
                                    }
                                }
                                else
                                {
                                    strINVOICE_DUEDATE = helper.GetValue(i, 3) == null ? "" : helper.GetValue(i, 3).ToString();
                                }

                                //invocie class
                                strINVOICE_CLASS = helper.GetValue(i, 5) == null ? "" : helper.GetValue(i, 5).ToString();

                                strINVOICE_AMOUNT = helper.GetValue(i, 10) == null ? "" : helper.GetValue(i, 10).ToString();
                                ICell cell = helper.GetCell(i, 11);
                                if (cell != null)
                                {
                                    if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                    {
                                        strINVOICE_PTPDATE = cell.DateCellValue == null ? "" : cell.DateCellValue.ToString("yyyy/MM/dd");
                                    }
                                    else
                                    {
                                        strINVOICE_PTPDATE = helper.GetValue(i, 11) == null ? null : helper.GetValue(i, 11).ToString();
                                    }
                                }
                                else
                                {
                                    strINVOICE_PTPDATE = helper.GetValue(i, 11) == null ? null : helper.GetValue(i, 11).ToString();
                                }

                                dueReason = helper.GetValue(i, 12) == null ? "" : helper.GetValue(i, 12).ToString();
                                strBalance_Memo = helper.GetValue(i, 13) == null ? "" : helper.GetValue(i, 13).ToString();
                                // Memo Expiration Date
                                cell = helper.GetCell(i, 14);
                                if (cell != null && !string.IsNullOrWhiteSpace(strBalance_Memo))
                                {
                                    if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                    {
                                        strMemoExpirationDate = cell.DateCellValue == null ? "" : cell.DateCellValue.ToString("yyyy/MM/dd");
                                    }
                                    else
                                    {
                                        strMemoExpirationDate = helper.GetValue(i, 14) == null ? null : helper.GetValue(i, 14).ToString();
                                    }
                                }
                                else
                                {
                                    strMemoExpirationDate = helper.GetValue(i, 14) == null || string.IsNullOrWhiteSpace(strBalance_Memo) ? null : helper.GetValue(i, 14).ToString();
                                }
                                //Invoice no 为空时不处理, PMT 的不处理
                                if (!string.IsNullOrEmpty(strINVOICE_NO))
                                {
                                    T_INVOICE_STATUS_STAGING itemInvoiceStatus = new T_INVOICE_STATUS_STAGING();
                                    itemInvoiceStatus.FILETYPE = FileType;
                                    itemInvoiceStatus.INVOICE_CLASS = strINVOICE_CLASS;
                                    itemInvoiceStatus.INVOICE_NO = strINVOICE_NO;
                                    itemInvoiceStatus.INVOICE_DATE = Convert.ToDateTime(strINVOICE_DATE);
                                    itemInvoiceStatus.INVOICE_DUEDATE = Convert.ToDateTime(strINVOICE_DUEDATE);
                                    itemInvoiceStatus.INVOICE_AMOUNT = Convert.ToDecimal(strINVOICE_AMOUNT);
                                    if (!string.IsNullOrWhiteSpace(strINVOICE_PTPDATE))
                                    {
                                        try
                                        {
                                            itemInvoiceStatus.INVOICE_PTPDATE = Convert.ToDateTime(strINVOICE_PTPDATE);
                                        }
                                        catch (Exception ex)
                                        {
                                            strErrmsg = "Row:" + (i + 1) + ",PtpDate invalid. ";
                                            return strErrmsg;
                                        }
                                    }

                                    itemInvoiceStatus.DueReason = dueReason;
                                    itemInvoiceStatus.INVOICE_BalanceMemo = strBalance_Memo;
                                    itemInvoiceStatus.CREATE_USER = AppContext.Current.User.EID;
                                    itemInvoiceStatus.CREATE_DATE = AppContext.Current.User.Now;
                                    //根据INV获得SiteUseId
                                    strSiteUseId = CommonRep.GetDbSet<InvoiceAging>().Where(o => o.InvoiceNum == strINVOICE_NO).Select(o => o.SiteUseId).FirstOrDefault();
                                    itemInvoiceStatus.SiteUseId = strSiteUseId;
                                    itemInvoiceStatus.ConsignmentNumber = consignmentNumber;
                                    if (string.IsNullOrEmpty(strMemoExpirationDate))
                                    {
                                        itemInvoiceStatus.MemoExpirationDate = null;
                                    }
                                    else
                                    {
                                        itemInvoiceStatus.MemoExpirationDate = Convert.ToDateTime(strMemoExpirationDate);
                                    }
                                    listInvoiceStatus.Add(itemInvoiceStatus);
                                }
                            }
                            i++;
                        } while (i <= maxRowNumber);
                        break;
                }
            }
            catch (Exception ex)
            {
                i = i;
                helper = null;
                throw ex;
            }
            finally
            {
                helper = null;
            }
            return strErrmsg;
        }

        public string getFileExtendName(string strFileName)
        {
            string strExtendName = "";
            if (Path.GetExtension(strFileName).ToUpper() == ".ZIP")
            {
                strExtendName = ".zip";
            }
            else if (Path.GetExtension(strFileName).ToUpper() == ".TXT")
            {
                strExtendName = ".txt";
            }
            return strExtendName;
        }


        public void unZip(string zipfile, string subPath, string archiveFileName, string strGuid)
        {
            if (!File.Exists(zipfile))
            {
                Helper.Log.Error(string.Format("Cannot find file '{0}'", zipfile), null);
                return;
            }
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipfile)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {

                    Helper.Log.Info(theEntry.Name);

                    string fileName = Path.GetFileName(theEntry.Name);

                    // create directory
                    string strTargetFilePath = archiveFileName;
                    string strFilePath = Path.GetDirectoryName(zipfile) + @"\" + subPath;
                    if (subPath.Length > 0)
                    {
                        Directory.CreateDirectory(strFilePath);
                        strTargetFilePath = strFilePath + @"\" + archiveFileName;
                    }

                    if (fileName != String.Empty)
                    {
                        using (FileStream streamWriter = File.Create(strTargetFilePath))
                        {

                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        if (Path.GetExtension(strTargetFilePath).ToUpper() == ".ZIP")
                        {
                            using (ZipInputStream s_sub = new ZipInputStream(File.OpenRead(strTargetFilePath)))
                            {

                                ZipEntry theEntry_sub;
                                while ((theEntry_sub = s_sub.GetNextEntry()) != null)
                                {

                                    Helper.Log.Info(theEntry_sub.Name);

                                    string fileName_sub = Path.GetFileName(theEntry_sub.Name);

                                    // create directory
                                    string strTargetFilePath_sub = archiveFileName;
                                    string strFilePath_sub = Path.GetDirectoryName(zipfile) + @"\" + subPath + @"\" + "Transaction_" + strGuid;
                                    if (strFilePath_sub.Length > 0)
                                    {
                                        Directory.CreateDirectory(strFilePath_sub);
                                        strTargetFilePath = strFilePath_sub + @"\" + fileName_sub;
                                    }

                                    if (fileName_sub != String.Empty)
                                    {
                                        using (FileStream streamWriter = File.Create(strTargetFilePath))
                                        {

                                            int size = 2048;
                                            byte[] data = new byte[2048];
                                            while (true)
                                            {
                                                size = s_sub.Read(data, 0, data.Length);
                                                if (size > 0)
                                                {
                                                    streamWriter.Write(data, 0, size);
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
