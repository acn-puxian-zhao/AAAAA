using CsvHelper;
using CsvHelper.Configuration;
using ICSharpCode.SharpZipLib.Zip;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;

namespace Intelligent.OTC.Business
{
    /// <summary>
    /// arrow add by lilanfu
    /// </summary>
    public partial class CustomerService
    {
        #region "Arrow add by lilanfu"
        /// <summary>
        /// arrow by lilanfu, move tempTbale data to RealTable，提交主要包含1.更新FileUploadHistory上传文件三个字段2.account,invoice临时表的迁移3临时表数据的删除
        /// </summary>
        /// <param name="submitStgs"></param>
        /// <returns>0成功，1失败</returns>
        public int SubmitInitialAging(string arrow)
        {
            Helper.Log.Info("Start submit-Arrow");

            #region SQL
            // Account process logic
            // customer aging staging to customer aging
            var customerAgingMergeSql = string.Format(@"
        MERGE INTO T_CUSTOMER_AGING as Target 
        USING T_CUSTOMER_AGING_STAGING as Source
        ON
	        Target.DEAL=Source.DEAL and Target.CUSTOMER_NUM = Source.CUSTOMER_NUM
            and Target.SiteUseId = Source.SiteUseId  
	        and Target.LEGAL_ENTITY = Source.LEGAL_ENTITY and Source.DEAL = '{2}'
        WHEN MATCHED THEN 
	        UPDATE SET Target.CUSTOMER_NAME	    = Source.CUSTOMER_NAME                        
				        ,Target.CUSTOMER_CLASS	    = Source.CUSTOMER_CLASS
				        ,Target.RISK_SCORE		    = Source.RISK_SCORE
				        ,Target.BILL_GROUP_CODE	    = Source.BILL_GROUP_CODE
				        ,Target.BILL_GROUP_NAME	    = Source.BILL_GROUP_NAME
				        ,Target.COUNTRY			    = Source.COUNTRY
				        ,Target.CREDIT_TREM		    = Source.CREDIT_TREM
				        ,Target.CREDIT_LIMIT		= Source.CREDIT_LIMIT
				        ,Target.COLLECTOR		    = Source.COLLECTOR
				        ,Target.COLLECTOR_SYS	    = Source.COLLECTOR_SYS
				        ,Target.SALES			    = Source.SALES
				        ,Target.TOTAL_AMT		    = Source.TOTAL_AMT
				        ,Target.CURRENT_AMT		    = Source.CURRENT_AMT
                        ,Target.DUE15_AMT   	    = Source.DUE15_AMT
				        ,Target.DUE30_AMT		    = Source.DUE30_AMT
                        ,Target.DUE45_AMT		    = Source.DUE45_AMT
				        ,Target.DUE60_AMT		    = Source.DUE60_AMT
				        ,Target.DUE90_AMT		    = Source.DUE90_AMT
				        ,Target.DUE120_AMT		    = Source.DUE120_AMT
				        ,Target.DUE150_AMT		    = Source.DUE150_AMT
				        ,Target.DUE180_AMT		    = Source.DUE180_AMT
				        ,Target.DUE210_AMT		    = Source.DUE210_AMT
				        ,Target.DUE240_AMT		    = Source.DUE240_AMT
				        ,Target.DUE270_AMT		    = Source.DUE270_AMT
				        ,Target.DUE300_AMT		    = Source.DUE300_AMT
				        ,Target.DUE330_AMT		    = Source.DUE330_AMT
				        ,Target.DUE360_AMT		    = Source.DUE360_AMT
				        ,Target.DUEOVER360_AMT	    = Source.DUEOVER360_AMT
                        ,Target.ACCOUNT_STATUS	    = Source.ACCOUNT_STATUS
				        ,Target.IS_HOLD_FLG		    = Source.IS_HOLD_FLG
				        ,Target.IMPORT_ID		    = Source.IMPORT_ID
                        ,Target.DUEOVER_TOTAL_AMT   = Source.TOTAL_AMT - Source.CURRENT_AMT
                        ,Target.UPDATE_DATE         = CAST('{1}' as datetime)
                        ,Target.REMOVE_FLG          = '0'
                        ,Target.CURRENCY            =Source.CURRENCY
                        ,Target.COUNTRY_CODE        =Source.COUNTRY_CODE
                        ,Target.OUTSTANDING_AMT     =Source.OUTSTANDING_AMT
                        ,Target.CITY_OR_STATE       =Source.CITY_OR_STATE
                        ,Target.CONTACT_NAME        =Source.CONTACT_NAME
                        ,Target.CONTACT_PHONE       =Source.CONTACT_PHONE
                        ,Target.CUSTOMER_CREDIT_MEMO=Source.CUSTOMER_CREDIT_MEMO
                        ,Target.CUSTOMER_PAYMENTS   =Source.CUSTOMER_PAYMENTS
                        ,Target.CUSTOMER_RECEIPTS_AT_RISK=Source.CUSTOMER_RECEIPTS_AT_RISK
                        ,Target.CUSTOMER_CLAIMS     =Source.CUSTOMER_CLAIMS
                        ,Target.CUSTOMER_BALANCE    =Source.CUSTOMER_BALANCE
                        ,Target.TotalFutureDue      =Source.TotalFutureDue
                        ,Target.Ebname              =Source.Ebname
                        --,Target.'OPERATOR'          = '{0}'
        WHEN not matched and Source.DEAL = '{2}' THEN
	        insert 
		        (DEAL,LEGAL_ENTITY,CUSTOMER_NUM,CUSTOMER_NAME,SiteUseId,CUSTOMER_CLASS,RISK_SCORE,BILL_GROUP_CODE
		        ,BILL_GROUP_NAME,COUNTRY,CREDIT_TREM,CREDIT_LIMIT,COLLECTOR,COLLECTOR_SYS,SALES
		        ,TOTAL_AMT,CURRENT_AMT,DUE15_AMT,DUE30_AMT,DUE45_AMT,DUE60_AMT,DUE90_AMT,DUE120_AMT,DUE150_AMT
		        ,DUE180_AMT,DUE210_AMT,DUE240_AMT,DUE270_AMT,DUE300_AMT,DUE330_AMT,DUE360_AMT,DUEOVER360_AMT
                , DUEOVER_TOTAL_AMT,CREATE_DATE,UPDATE_DATE--,OPERATOR
                ,IS_HOLD_FLG,IMPORT_ID,REMOVE_FLG
                ,CURRENCY, COUNTRY_CODE, OUTSTANDING_AMT, CITY_OR_STATE
                , CONTACT_NAME, CONTACT_PHONE, CUSTOMER_CREDIT_MEMO, CUSTOMER_PAYMENTS
                , CUSTOMER_RECEIPTS_AT_RISK, CUSTOMER_CLAIMS, CUSTOMER_BALANCE 
                , ACCOUNT_STATUS, Ebname, TotalFutureDue)
	        values (Source.DEAL,Source.LEGAL_ENTITY,Source.CUSTOMER_NUM,Source.CUSTOMER_NAME,Source.SiteUseId,Source.CUSTOMER_CLASS,Source.RISK_SCORE,Source.BILL_GROUP_CODE
			        ,Source.BILL_GROUP_NAME,Source.COUNTRY,Source.CREDIT_TREM,Source.CREDIT_LIMIT,Source.COLLECTOR,Source.COLLECTOR_SYS,Source.SALES
			        ,Source.TOTAL_AMT,Source.CURRENT_AMT,Source.DUE15_AMT,Source.DUE30_AMT,Source.DUE45_AMT,Source.DUE60_AMT,Source.DUE90_AMT,Source.DUE120_AMT,Source.DUE150_AMT
			        ,Source.DUE180_AMT,Source.DUE210_AMT,Source.DUE240_AMT,Source.DUE270_AMT,Source.DUE300_AMT,Source.DUE330_AMT,Source.DUE360_AMT,Source.DUEOVER360_AMT
                    , Source.TOTAL_AMT - Source.CURRENT_AMT,CAST('{1}' as datetime),CAST('{1}' as datetime)--,'{0}'
                    ,Source.IS_HOLD_FLG,Source.IMPORT_ID,'0'
                    ,Source.CURRENCY, Source.COUNTRY_CODE, Source.OUTSTANDING_AMT, Source.CITY_OR_STATE
                    , Source.CONTACT_NAME, Source.CONTACT_PHONE, Source.CUSTOMER_CREDIT_MEMO, Source.CUSTOMER_PAYMENTS
                    , Source.CUSTOMER_RECEIPTS_AT_RISK, Source.CUSTOMER_CLAIMS, Source.CUSTOMER_BALANCE 
                    , Source.ACCOUNT_STATUS, Source.Ebname, Source.TotalFutureDue)
        WHEN not matched by Source and Target.DEAL = '{2}' AND Target.LEGAL_ENTITY in (SELECT DISTINCT LEGAL_ENTITY FROM T_CUSTOMER_AGING_STAGING WITH (NOLOCK)) THEN
            --目标表有，临时表没有，说明之前存在，新导入的时候消失了，说明不欠钱了，可以关闭了++必须在此次导入的LEGAL_ENTITY范围内，要不就把别的LEGAL_ENTITY关闭了（每次导入以LEGAL_ENTITY为单位）
            UPDATE SET Target.REMOVE_FLG = '1'
                       ,Target.UPDATE_DATE         = CAST('{1}' as datetime) 
                       ;", AppContext.Current.User.EID, AppContext.Current.User.Now, CurrentDeal);

            var invoiceLog = string.Format(@"
                INSERT INTO T_INVOICE_LOG
                (DEAL, CUSTOMER_NUM, SiteUseId,INVOICE_ID,
                LOG_DATE, LOG_PERSON, LOG_ACTION, 
                LOG_TYPE, OLD_STATUS, NEW_STATUS, 
                OLD_TRACK,NEW_TRACK,
                CONTACT_PERSON, PROOF_ID, DISCRIPTION)
                SELECT '{0}',ISNULL(staing.CUSTOMER_NUM,aging.CUSTOMER_NUM),ISNULL(staing.SiteUseId,aging.SiteUseId),ISNULL(staing.INVOICE_NUM,aging.INVOICE_NUM),
                        CAST('{1}' as datetime),'{2}','{3}',
                        '0',CASE WHEN aging.ID is null then '{4}' else aging.STATES end,CASE WHEN staing.ID is null then '{5}' else '{4}' end,
                        CASE WHEN aging.ID is null then '{10}' else aging.TRACK_STATES end,
                        CASE WHEN staing.ID is null then '{11}' else '{10}' end,
                        '{2}',null,null
                FROM T_INVOICE_AGING_STAGING staing WITH (NOLOCK)
                FULL OUTER JOIN T_INVOICE_AGING    aging WITH (NOLOCK)
                ON staing.DEAL = aging.DEAL
                AND staing.CUSTOMER_NUM = aging.CUSTOMER_NUM
                AND staing.SiteUseId = aging.SiteUseId
                AND staing.LEGAL_ENTITY = aging.LEGAL_ENTITY
                AND staing.INVOICE_NUM = aging.INVOICE_NUM
                WHERE aging.ID is null    --这些条件下，临时表有 主表没有的,需要插入
                OR
                    (
                        staing.ID is null   --这些条件下，临时表没有 ，主表有的且STATES不是InvoiceStatus.Closed且此次上传的ACCount 有（此次上传ACCOUNT有，且invoice没有关闭，并且这次没有上传这个invoice)
                    AND
                        --aging.STATES in ('{4}','{6}','{7}','{8}','{9}')
                        aging.STATES <> '{5}'  --InvoiceStatus.Closed
                    AND
                        EXISTS (SELECT * FROM T_CUSTOMER_AGING_STAGING AS Staging WITH (NOLOCK) WHERE aging.DEAL = Staging.DEAL 
				    --    and aging.CUSTOMER_NUM = Staging.CUSTOMER_NUM 
				        and aging.LEGAL_ENTITY = Staging.LEGAL_ENTITY)
                    );
                ", CurrentDeal, AppContext.Current.User.Now   //0,1
                 , AppContext.Current.User.EID                //2
                 , "Upload", strInvStats                      //3,4(InvoiceStatus.Open)
                 , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Closed)      //5
                 , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.PTP)         //6
                 , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Paid)        //7
                 , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.PartialPay)  //8
                 , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Dispute)     //9
                 , Helper.EnumToCode<TrackStatus>(TrackStatus.Open)             //10
                 , Helper.EnumToCode<TrackStatus>(TrackStatus.Closed));          //11
            var invoiceLog2 = string.Format(@"
                INSERT INTO T_INVOICE_LOG
                (DEAL, CUSTOMER_NUM, SiteUseId,INVOICE_ID,
                LOG_DATE, LOG_PERSON, LOG_ACTION, 
                LOG_TYPE, OLD_STATUS, NEW_STATUS, 
                OLD_TRACK,NEW_TRACK,
                CONTACT_PERSON, PROOF_ID, DISCRIPTION)
                SELECT '{0}',aging.CUSTOMER_NUM,aging.SiteUseId,aging.INVOICE_NUM,
                        CAST('{1}' as datetime),'{2}','{3}',
                        '0',aging.STATES,'{4}',
                        aging.TRACK_STATES,'{6}',
                        '{2}',null,null
                FROM T_INVOICE_AGING_STAGING staing WITH (NOLOCK)
                INNER JOIN T_INVOICE_AGING    aging WITH (NOLOCK)
                ON staing.DEAL = aging.DEAL
                AND staing.CUSTOMER_NUM = aging.CUSTOMER_NUM
                AND staing.SiteUseId = aging.SiteUseId
                AND staing.LEGAL_ENTITY = aging.LEGAL_ENTITY
                AND staing.INVOICE_NUM = aging.INVOICE_NUM
                WHERE aging.STATES = '{5}';
                ", CurrentDeal, AppContext.Current.User.Now  //0,1
                 , AppContext.Current.User.EID    //2
                 , "Upload", strInvStats   //3,4
                 , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Closed) //5
                 , Helper.EnumToCode<TrackStatus>(TrackStatus.Open));    //6
            //临时表和主表匹配上了条件，且主表为close的需要重新打开InvoiceStatus.Open，TrackStatus.Open

            var invoiceAgingMergeSql = string.Format(@"
        MERGE INTO T_INVOICE_AGING as Target 
        USING (SELECT * FROM T_INVOICE_AGING_STAGING  WITH (NOLOCK)
				WHERE CLASS+INVOICE_NUM+CUSTOMER_NUM+SiteUseId+DEAL+LEGAL_ENTITY 
					NOT IN (
						SELECT CLASS+INVOICE_NUM+CUSTOMER_NUM+SiteUseId+DEAL+LEGAL_ENTITY 
						FROM T_INVOICE_AGING_STAGING WITH (NOLOCK)
						WHERE CLASS IN ('DM','PAYMENT')
						GROUP BY CLASS, INVOICE_NUM, CUSTOMER_NUM,SiteUseId, DEAL, LEGAL_ENTITY HAVING COUNT(1) > 1)
			  ) as Source
        ON
	        Target.DEAL = Source.DEAL and Target.CUSTOMER_NUM = Source.CUSTOMER_NUM and Target.SiteUseId = Source.SiteUseId 
	        and Target.LEGAL_ENTITY = Source.LEGAL_ENTITY and Target.INVOICE_NUM = Source.INVOICE_NUM and Source.DEAL = '{2}'
        WHEN MATCHED THEN 
			UPDATE SET Target.INVOICE_TYPE		= Source.INVOICE_TYPE
			,Target.CUSTOMER_NAME	= Source.CUSTOMER_NAME
			,Target.CREDIT_TREM		= Source.CREDIT_TREM
			,Target.MST_CUSTOMER		= Source.MST_CUSTOMER
			,Target.PO_MUM			= Source.PO_MUM
			,Target.SO_NUM			= Source.SO_NUM
			,Target.CLASS			= Source.CLASS
			,Target.CURRENCY			= Source.CURRENCY
			,Target.ORDER_BY			= Source.ORDER_BY
			,Target.BILL_GROUP_CODE	= Source.BILL_GROUP_CODE
			,Target.INVOICE_DATE		= Source.INVOICE_DATE
			,Target.DUE_DATE			= Source.DUE_DATE
			--,Target.ORIGINAL_AMT		= Source.ORIGINAL_AMT
			,Target.BALANCE_AMT		= Source.BALANCE_AMT
			--Target.REMARK			--= Source.REMARK
			,Target.IMPORT_ID		= Source.IMPORT_ID
            ,Target.CreditLmt		= Source.CreditLmt
            ,Target.CreditLmtAcct		= Source.CreditLmtAcct
            ,Target.SellingLocationCode		= Source.SellingLocationCode
            ,Target.CustomerService		= Source.CustomerService
            ,Target.LsrNameHist		= Source.LsrNameHist
            ,Target.Sales		= Source.Sales
            ,Target.FsrNameHist		= Source.FsrNameHist
            ,Target.FuncCurrCode		= Source.FuncCurrCode
            ,Target.WoVat_AMT		= Source.WoVat_AMT
            ,Target.AgingBucket		= Source.AgingBucket
            ,Target.CreditTremDescription		= Source.CreditTremDescription
            ,Target.SellingLocationCode2		= Source.SellingLocationCode2
            ,Target.Ebname		= Source.Ebname
            ,Target.Customertype		= Source.Customertype
            ,Target.Cmpinv		= Source.Cmpinv
            ,Target.Eb		= Source.Eb
            ,Target.Fsr		= Source.Fsr
			,Target.UPDATE_DATE		= CAST('{1}' as datetime)
            ,Target.MISS_ACCOUNT_FLG=Source.MISS_ACCOUNT_FLG
            ,Target.STATEMENT_DATE=Source.STATEMENT_DATE
            ,Target.CUSTOMER_ADDRESS_1=Source.CUSTOMER_ADDRESS_1
            ,Target.CUSTOMER_ADDRESS_2=Source.CUSTOMER_ADDRESS_2
            ,Target.CUSTOMER_ADDRESS_3=Source.CUSTOMER_ADDRESS_3
            ,Target.CUSTOMER_ADDRESS_4=Source.CUSTOMER_ADDRESS_4
            ,Target.CUSTOMER_COUNTRY=Source.CUSTOMER_COUNTRY
            ,Target.CUSTOMER_COUNTRY_DETAIL=Source.CUSTOMER_COUNTRY_DETAIL
            ,Target.ATTENTION_TO=Source.ATTENTION_TO
            ,Target.COLLECTOR_NAME=Source.COLLECTOR_NAME
            ,Target.COLLECTOR_CONTACT=Source.COLLECTOR_CONTACT
            ,Target.DAYS_LATE_SYS=Source.DAYS_LATE_SYS
            ,Target.RBO_CODE=Source.RBO_CODE
            ,Target.OUTSTANDING_ACCUMULATED_INVOICE_AMT=Source.OUTSTANDING_ACCUMULATED_INVOICE_AMT
            ,Target.CUSTOMER_BILL_TO_SITE=Source.CUSTOMER_BILL_TO_SITE
            ,Target.STATES = (CASE WHEN Target.STATES = '{4}' THEN '{5}' ELSE Target.STATES END)
            ,Target.TRACK_STATES = (CASE WHEN Target.STATES = '{4}' THEN '{3}' ELSE Target.TRACK_STATES END)
			--Target.COMMENTS		--= Source.COMMENTS
		WHEN NOT MATCHED and Source.DEAL = '{2}' THEN
			INSERT (
				DEAL,CUSTOMER_NUM,SiteUseId,LEGAL_ENTITY,CUSTOMER_NAME,INVOICE_NUM, INVOICE_TYPE,CREDIT_TREM,MST_CUSTOMER,
				PO_MUM,SO_NUM,CLASS,CURRENCY,STATES,ORDER_BY,BILL_GROUP_CODE,
				INVOICE_DATE,DUE_DATE,ORIGINAL_AMT,BALANCE_AMT,REMARK,
				IMPORT_ID,CREATE_DATE,UPDATE_DATE,COMMENTS
                , MISS_ACCOUNT_FLG, STATEMENT_DATE, CUSTOMER_ADDRESS_1, CUSTOMER_ADDRESS_2
                , CUSTOMER_ADDRESS_3, CUSTOMER_ADDRESS_4, CUSTOMER_COUNTRY
                , CUSTOMER_COUNTRY_DETAIL, ATTENTION_TO, COLLECTOR_NAME
                , COLLECTOR_CONTACT, DAYS_LATE_SYS, RBO_CODE
                , OUTSTANDING_ACCUMULATED_INVOICE_AMT, CUSTOMER_BILL_TO_SITE
                , CreditLmt, CreditLmtAcct
                , SellingLocationCode, CustomerService
                , LsrNameHist, Sales
                , FsrNameHist, FuncCurrCode
                , WoVat_AMT, AgingBucket
                , CreditTremDescription, SellingLocationCode2
                , Ebname, Customertype
                , Cmpinv, Eb
                , Fsr
                , TRACK_STATES
			)
			VALUES (
				Source.DEAL,Source.CUSTOMER_NUM,Source.SiteUseId,Source.LEGAL_ENTITY,Source.CUSTOMER_NAME,Source.INVOICE_NUM, Source.INVOICE_TYPE,Source.CREDIT_TREM,Source.MST_CUSTOMER,
				Source.PO_MUM,Source.SO_NUM,Source.CLASS,Source.CURRENCY,Source.STATES,Source.ORDER_BY,Source.BILL_GROUP_CODE,
				Source.INVOICE_DATE,Source.DUE_DATE,Source.ORIGINAL_AMT,Source.BALANCE_AMT,Source.REMARK,
				Source.IMPORT_ID,CAST('{1}' as datetime),CAST('{1}' as datetime),Source.COMMENTS
                , Source.MISS_ACCOUNT_FLG, Source.STATEMENT_DATE, Source.CUSTOMER_ADDRESS_1, Source.CUSTOMER_ADDRESS_2
                , Source.CUSTOMER_ADDRESS_3, Source.CUSTOMER_ADDRESS_4, Source.CUSTOMER_COUNTRY
                , Source.CUSTOMER_COUNTRY_DETAIL, Source.ATTENTION_TO, Source.COLLECTOR_NAME
                , Source.COLLECTOR_CONTACT, Source.DAYS_LATE_SYS, Source.RBO_CODE
                , Source.OUTSTANDING_ACCUMULATED_INVOICE_AMT, Source.CUSTOMER_BILL_TO_SITE
                 , Source.CreditLmt, Source.CreditLmtAcct
                 , Source.SellingLocationCode, Source.CustomerService
                 , Source.LsrNameHist, Source.Sales
                 , Source.FsrNameHist, Source.FuncCurrCode
                 , Source.WoVat_AMT, Source.AgingBucket
                 , Source.CreditTremDescription, Source.SellingLocationCode2
                , Source.Ebname, Source.Customertype
                , Source.Cmpinv, Source.Eb
                , Source.Fsr
                , '{3}'
			);", AppContext.Current.User.EID, AppContext.Current.User.Now, CurrentDeal   //0,1,2
               , Helper.EnumToCode<TrackStatus>(TrackStatus.Open)            //3
               , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Closed)      //4
               , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Open));      //5

            //除了class:'DM','PAYMENT'的条件下，满足on条件，1更新 ：其中状态STATES为关闭变为打开，其他状态不变 TRACK_STATES关闭变为打开，其他状态不变

            // for Payment and DM, there are duplicate records with same invoice number. They will be treat specially because of 'Merge' clause will fail on them.
            var specialPaymentAdditionSql = string.Format(@"
        INSERT INTO T_INVOICE_AGING (
			DEAL,CUSTOMER_NUM,SiteUseId,LEGAL_ENTITY,CUSTOMER_NAME,INVOICE_NUM, INVOICE_TYPE,CREDIT_TREM,MST_CUSTOMER,
			PO_MUM,SO_NUM,CLASS,CURRENCY,STATES,ORDER_BY,BILL_GROUP_CODE,
			INVOICE_DATE,DUE_DATE,ORIGINAL_AMT,BALANCE_AMT,REMARK,
			IMPORT_ID,CREATE_DATE,UPDATE_DATE,COMMENTS
            , MISS_ACCOUNT_FLG, STATEMENT_DATE, CUSTOMER_ADDRESS_1, CUSTOMER_ADDRESS_2
            , CUSTOMER_ADDRESS_3, CUSTOMER_ADDRESS_4, CUSTOMER_COUNTRY
            , CUSTOMER_COUNTRY_DETAIL, ATTENTION_TO, COLLECTOR_NAME
            , COLLECTOR_CONTACT, DAYS_LATE_SYS, RBO_CODE
            , OUTSTANDING_ACCUMULATED_INVOICE_AMT, CUSTOMER_BILL_TO_SITE
            , CreditLmt, CreditLmtAcct
            , SellingLocationCode, CustomerService
            , LsrNameHist, Sales
            , FsrNameHist, FuncCurrCode
            , WoVat_AMT, AgingBucket
            , CreditTremDescription, SellingLocationCode2
            , Ebname, Customertype
            , Cmpinv, Eb
            , Fsr
		) 
		SELECT Source.DEAL,Source.CUSTOMER_NUM,Source.SiteUseId,Source.LEGAL_ENTITY,Source.CUSTOMER_NAME,Source.INVOICE_NUM, Source.INVOICE_TYPE,Source.CREDIT_TREM,Source.MST_CUSTOMER,
			Source.PO_MUM,Source.SO_NUM,Source.CLASS,Source.CURRENCY,Source.STATES,Source.ORDER_BY,Source.BILL_GROUP_CODE,
			Source.INVOICE_DATE,Source.DUE_DATE,Source.ORIGINAL_AMT,Source.BALANCE_AMT,Source.REMARK,
			Source.IMPORT_ID,CAST('{0}' as datetime),CAST('{0}' as datetime),
			Source.COMMENTS
            , Source.MISS_ACCOUNT_FLG, Source.STATEMENT_DATE, Source.CUSTOMER_ADDRESS_1, Source.CUSTOMER_ADDRESS_2
            , Source.CUSTOMER_ADDRESS_3, Source.CUSTOMER_ADDRESS_4, Source.CUSTOMER_COUNTRY
            , Source.CUSTOMER_COUNTRY_DETAIL, Source.ATTENTION_TO, Source.COLLECTOR_NAME
            , Source.COLLECTOR_CONTACT, Source.DAYS_LATE_SYS, Source.RBO_CODE
            , Source.OUTSTANDING_ACCUMULATED_INVOICE_AMT, Source.CUSTOMER_BILL_TO_SITE
            , Source.CreditLmt, Source.CreditLmtAcct
            , Source.SellingLocationCode, Source.CustomerService
            , Source.LsrNameHist, Source.Sales
            , Source.FsrNameHist, Source.FuncCurrCode
            , Source.WoVat_AMT, Source.AgingBucket
            , Source.CreditTremDescription, Source.SellingLocationCode2
            , Source.Ebname, Source.Customertype
            , Source.Cmpinv, Source.Eb
            , Source.Fsr 
        FROM T_INVOICE_AGING_STAGING Source WITH (NOLOCK)
		WHERE CLASS+INVOICE_NUM+CUSTOMER_NUM+SiteUseId+DEAL+LEGAL_ENTITY 
			IN (
				SELECT CLASS+INVOICE_NUM+CUSTOMER_NUM+SiteUseId+DEAL+LEGAL_ENTITY 
				FROM T_INVOICE_AGING_STAGING  WITH (NOLOCK) 
				WHERE CLASS IN ('DM','PAYMENT')
				GROUP BY CLASS, INVOICE_NUM, CUSTOMER_NUM, SiteUseId,DEAL, LEGAL_ENTITY HAVING COUNT(1) > 1)", AppContext.Current.User.Now);
            // class:'DM','PAYMENT'的条件下，插入invoice

            var invoiceAgingClosingSql = string.Format(@"
        UPDATE T_INVOICE_AGING
        SET UPDATE_DATE = CAST('{1}' as datetime), REMARK = 'Missing invoice during import. Set to paied status by SYSTEM'
            ,STATES = '{3}',TRACK_STATES = '{9}',CloseDate = CAST('{1}' as datetime) 
        WHERE 
	        NOT EXISTS (SELECT * FROM T_INVOICE_AGING_STAGING AS Staging WITH (NOLOCK) WHERE T_INVOICE_AGING.DEAL = Staging.DEAL 
				        and T_INVOICE_AGING.CUSTOMER_NUM = Staging.CUSTOMER_NUM 
                        and T_INVOICE_AGING.SiteUseId = Staging.SiteUseId 
				        and T_INVOICE_AGING.LEGAL_ENTITY = Staging.LEGAL_ENTITY and T_INVOICE_AGING.INVOICE_NUM = Staging.INVOICE_NUM)
            and EXISTS (SELECT * FROM T_CUSTOMER_AGING_STAGING AS Staging WITH (NOLOCK) WHERE T_INVOICE_AGING.DEAL = Staging.DEAL 
			--	        and T_INVOICE_AGING.CUSTOMER_NUM = Staging.CUSTOMER_NUM 
				        and T_INVOICE_AGING.LEGAL_ENTITY = Staging.LEGAL_ENTITY)
            and T_INVOICE_AGING.DEAL = '{2}'
            --and STATES in ('{4}','{5}','{6}','{7}','{8}')
            and STATES <> '{3}';
            ", AppContext.Current.User.EID, CurrentTime, CurrentDeal, Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Closed)//0,1,2,3
             , strInvStats //4
             , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.PTP) //5
             , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Paid)//6
             , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.PartialPay)//7
             , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Dispute)//8
             , Helper.EnumToCode<TrackStatus>(TrackStatus.Closed));//9


            #endregion

            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    Helper.Log.Info("Transaction scope created-Arrow");

                    Helper.Log.Info("File history update started.-Arrow");
                    //Get custAgStags info
                    var custAgStags = CommonRep.GetQueryable<CustomerAgingStaging>().Where(o => o.Deal == CurrentDeal).ToList();

                    //use slelected id to search temptable not find result(this reco other user commited)
                    if (custAgStags.Count == 0) { return 1; }

                    //从account临时表里按照ImportId分组，转换为FileUploadHistory实体类
                    List<FileUploadHistory> listImport = (from cust in custAgStags
                                                          group cust by cust.ImportId
                                                              into custg
                                                          select new FileUploadHistory
                                                          {
                                                              ImportId = custg.Key
                                                          }).ToList();

                    FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
                    //account临时表里ImportId和FileUploadHistory的ImportId一样的取出来，为了更新
                    List<FileUploadHistory> file = fileService.GetSucDataByImportId(listImport);
                    //更新FileUploadHistory表中：SubmitFlag=UploadStates.Submitted，SubmitTime，PeriodId
                    fileService.commitHisUp(file);
                    Helper.Log.Info("File history update complete.-Arrow");
                    CommonRep.Commit();

                    Helper.Log.Info("Merge(insert/update) the account level aging data to database.-Arrow");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(customerAgingMergeSql);

                    Helper.Log.Info("insert into T_INVOICE_LOG whitch invoice's STATES has changed.-Arrow");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(invoiceLog);

                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(invoiceLog2);

                    Helper.Log.Info("Merge(insert/update) the invoice level aging data to database.-Arrow");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(invoiceAgingMergeSql);

                    Helper.Log.Info("Merge(insert/update) the invoice level aging data to database for special Payment.-Arrow");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(specialPaymentAdditionSql);

                    Helper.Log.Info("Apply closing logic to the missing invoice level aging data.-Arrow");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(invoiceAgingClosingSql);

                    Helper.Log.Info("Start to delete the staging data.-Arrow");

                    SqlParameter[] parms = { new SqlParameter("@DEAL", CurrentDeal)};

                    CommonRep.GetDBContext().Database.ExecuteSqlCommand("delete from T_CUSTOMER_AGING_STAGING where DEAL = @DEAL", parms);
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand("delete from T_INVOICE_AGING_STAGING WHERE DEAL = @DEAL",parms);

                    Helper.Log.Info("Completed invoice level aging process.-Arrow");

                    // finaly commit all 
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Error happended while save submit aging.", ex);
                throw;
            }
            return 0;
        }

        /// <summary>
        /// arrow by lilanfu- 数据导入（stream）
        /// </summary>
        /// <param name="arrow">arrow标识字段</param>
        /// <param name="streamold">stream</param>
        public void allFileImport(string arrow, string streamold)
        {
            //获取最新的accfile
            FileUploadHistory accFileName;
            //获取最新的invfile
            FileUploadHistory invFileName;
            //accfile文件变为流##
            StreamReader srAcc;
            //invfile文件变为流##
            StreamReader srInv;
            DateTime? dt;
            DateTime? dtReport;
            UploadStates sts;
            bool isSuc;
            string strGuid;
            string msg = "";

            string strSite;
            //init variable
            strMessage = string.Empty;
            srAcc = null;
            srInv = null;
            strSite = null;
            dt = null;
            sts = UploadStates.Failed;
            accFileName = new FileUploadHistory();
            invFileName = new FileUploadHistory();
            isSuc = false;
            strGuid = string.Empty;

            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");

            try
            {
                //get filefullname
                string strCode;
                //arrow-get new accFileName from Db By ProcessFlag and FileType and Operator and deal
                strCode = Helper.EnumToCode<FileType>(FileType.Account);
                accFileName = fileService.GetNewestData(strCode);
                //arrow-get new invFileName from Db By ProcessFlag and FileType and Operator and deal
                strCode = Helper.EnumToCode<FileType>(FileType.Invoice);
                invFileName = fileService.GetNewestData(strCode);

                if (accFileName == null || invFileName == null)
                {
                    //file not found
                    strMessage = "Both Account level And invoice level are required!" + strEnter;
                }
                else
                {
                    //file open
                    //arrow-acc 和inv 文件变为流!!!!!!!!!!!!改Excel
                    //NpoiHelper helper = new NpoiHelper(accFileName.ArchiveFileName);
                    srAcc = fileOpen(accFileName.ArchiveFileName);
                    srInv = fileOpen(invFileName.ArchiveFileName);
                    //site check
                    //rtnSite= Table-SysTypeDetail, 传入DetailName(account文件第一行（Legal Entity）)，查找DetailValue字段
                    strSite = strSiteGet(srAcc, srInv, accFileName.OriginalFileName, invFileName.OriginalFileName);
                    if (string.IsNullOrEmpty(strSite))
                    {
                        //Do nothing
                    }
                    else
                    {
                        strMessage = string.Empty;
                        //校验accountFile：1.Summary Type:^Customer Summary2.Report Date3.标题行，得到dt=Report Date:的时间
                        dt = formartCheck(srAcc, out msg);
                        if (dt == null)
                        {
                            strMessage = accFileName.OriginalFileName + " Bad file format!" + strEnter + msg + strEnter;
                        }
                        else
                        {
                            //生成一个GUID并且去掉-
                            strGuid = System.Guid.NewGuid().ToString("N");
                            //dtReport=最新时间，FileUploadHistory-通过DetailValue（LegalEntity对应的系统值）最新的ReportTime
                            dtReport = fileService.GetFileUploadHistory().Where(o => o.LegalEntity == strSite).Max(o => o.ReportTime);
                            //accountFile 里面的Report Date 1.大于当前时间 就是Date is wrong 2.小于最新时间的时间 就是is old
                            if (dt.Value > CurrentTime || dt < dtReport)
                            {
                                strMessage = accFileName.OriginalFileName + " Report is old!" + strEnter + "(Report Date is wrong!)" + strEnter;
                            }
                            //校验invoiceFile：校验标题行1第一个字段是否正确2.列是否正确
                            else if (!invoiceFormartCheck(srInv, "arrow"))
                            {
                                strMessage = invFileName.OriginalFileName + " Bad file format!" + strEnter;
                            }
                            else
                            {
                                dataImport(srAcc, strSite, strGuid, "arrow");
                                dataInvoiceImport(srInv, strSite, strGuid);
                                if (listAgingStaging.Count > 0 && invoiceAgingList.Count > 0)
                                {
                                    if (isSameCust("arrow"))
                                    {
                                        dataAddToComm(strSite);

                                        sts = UploadStates.Success;
                                        isSuc = true;
                                    }
                                    else
                                    {
                                        strMessage = "Account level's Customers and invoice level's Customers are inconformity!" + strEnter;
                                    }
                                }
                                else
                                {
                                    strMessage = "Import Data is empty!" + strEnter;
                                }
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(strMessage))
                {
                    strGuid = null;
                    throw new AgingImportException(strMessage, isSuc);
                }

                fileService.upLoadHisUp(accFileName, invFileName, sts, dt, strSite, listAgingStaging.Count, invoiceAgingList.Count, strGuid);
            }
            catch (AgingImportException ex)
            {
                fileService.upLoadHisUp(accFileName, invFileName, sts, null);
                Helper.Log.Error("Error happended while allFileImport.", ex);
                throw new AgingImportException("File import failed, please contact the administrator.\r\n", isSuc);
            }
            catch (Exception ex)
            {
                fileService.upLoadHisUp(accFileName, invFileName, sts, null);
                Helper.Log.Error("Error happended while allFileImport.", ex);
                throw new AgingImportException("File import failed, please contact the administrator.\r\n", isSuc);
            }
            finally
            {
                //无论结果如何，需要释放和关闭流StreamReader
                if (srAcc != null)
                {
                    srAcc.Dispose();
                    srAcc.Close();
                }
                if (srInv != null)
                {
                    srInv.Dispose();
                    srInv.Close();
                }
            }

        }

        public void autoBuildContactor(string deal, string legalEntity)
        {

            try
            {
                //DEAL Parameter
                var paramDEAL = new SqlParameter
                {
                    ParameterName = "@DEAL",
                    Value = deal,
                    Direction = ParameterDirection.Input
                };
                //LEGAL_ENTITY Parameter
                var paramLegalEntity = new SqlParameter
                {
                    ParameterName = "@LegalEntity",
                    Value = legalEntity,
                    Direction = ParameterDirection.Input
                };

                SqlParameter[] paramList = new SqlParameter[2];
                paramList[0] = paramDEAL;
                paramList[1] = paramLegalEntity;
                // using(tranc)

                Helper.Log.Info("Start: call spBuildContactor(procedure):@DEAL" + deal + ",@LegalEntity:" + legalEntity);
                CommonRep.GetDBContext().Database.ExecuteSqlCommand("spBuildContactor @DEAL,@LegalEntity", paramList.ToArray());
                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Error happended while build contactor.", ex);
                throw ex;
            }
        }

        /// <summary>
        ///arrow by lilanfu- 数据导入（Excel）
        /// </summary>
        /// <param name="arrow">arrow标识字段</param>
        public void allFileImportArrow(FileUploadHistory accFileName, FileUploadHistory invFileName, FileUploadHistory invDetailFileName = null, FileUploadHistory vatFileName = null)
        {
            DateTime? dt;
            DateTime? dtReport;
            UploadStates sts;
            bool isSuc;
            string strGuid;
            string msg = "";
            string strSite;
            bool haveVat;
            bool haveInvDet;

            dt = null;
            sts = UploadStates.Failed;
            isSuc = false;
            strGuid = string.Empty;
            strSite = null;
            strMessage = string.Empty;
            haveVat = false;
            haveInvDet = false;

            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");

            try
            {
                if (accFileName == null || invFileName == null)
                {
                    //file not found
                    strMessage = "Both Account level And invoice level are required!" + strEnter;
                }
                else
                {
                    string dealName = string.Empty;
                    if (accFileName.Operator == "auto" && invFileName.Operator == "auto")
                    {
                        dealName = accFileName.Deal;
                    }
                    else if (CurrentDeal != null)
                    {
                        dealName = CurrentDeal;
                    }
                    else
                    {
                    }

                    int curPer = 0;
                    curPer = getcurrentPeroidOnlyOne(dealName);
                    if (curPer == 0)
                    {
                        strMessage = "current peroid is error!" + strEnter;
                    }
                    else
                    {
                        //site check
                        //rtnSite= Table-SysTypeDetail, 传入DetailName，查找DetailValue字段
                        strSite = strSiteGetArrow(accFileName.OriginalFileName, invFileName.OriginalFileName);

                        if (string.IsNullOrEmpty(strSite))
                        {
                            //Do nothing
                        }
                        else
                        {
                            strMessage = string.Empty;
                            //此时，查完了orgid对应的系统value，也就是Legal Entity+ 对是否为一对上传文件进行了校验
                            //校验accountFile：1.Summary Type:^Customer Summary2.Report Date3.标题行，得到dt=Report Date:的时间
                            dt = formartCheckArrow(accFileName.OriginalFileName, invFileName.OriginalFileName, out msg);
                            if (dt == null)
                            {
                                strMessage = accFileName.OriginalFileName + " Bad file format!" + strEnter + msg + strEnter;
                            }
                            else
                            {
                                //此时，查完了1orgid对应的系统value，也就是Legal Entity+ 2对是否为一对上传文件进行了校验+3Report Date
                                //生成一个GUID并且去掉-
                                strGuid = System.Guid.NewGuid().ToString("N");
                                //dtReport=最新时间，FileUploadHistory-通过DetailValue（LegalEntity对应的系统值）最新的ReportTime
                                dtReport = fileService.GetFileUploadHistory().Where(o => o.LegalEntity == strSite).Max(o => o.ReportTime);
                                //accountFile 里面的Report Date 1.大于当前时间 就是Date is wrong 2.小于最新时间的时间 就是is old
                                if (dt.Value > CurrentTime || dt < dtReport)
                                {
                                    strMessage = accFileName.OriginalFileName + " Report is old!" + strEnter + "(Report Date is wrong!)" + strEnter;
                                }
                                else
                                {
                                    //生成批量插入的list -start
                                    arrowAccountDataImport(strGuid, strSite, accFileName, dealName);
                                    arrowInvoiceDataImport(strGuid, strSite, invFileName, dealName);
                                    if (invDetailFileName != null)
                                    {
                                        arrowInvoiceDetailDataImport(strGuid, strSite, invDetailFileName, dealName);
                                    }

                                    if (vatFileName != null)
                                    {
                                        arrowVatDataImport(strGuid, vatFileName);
                                    }
                                    //生成批量插入的list -end

                                    if (listAgingStaging.Count > 0 && invoiceAgingList.Count > 0)
                                    {
                                        if (invoiceDetailAgingList != null && invoiceDetailAgingList.Count > 0)
                                        {
                                            haveInvDet = true;
                                        }
                                        if (listvat != null && listvat.Count > 0)
                                        {
                                            haveVat = true;
                                        }
                                        if (isSameCust("arrow"))//??????这里要放同一个Customer下
                                        {
                                            //对数据库的操作1.删除两个临时表条件：deal和orgid下2.custList.FindAll(cust => cust.Id == 0)3.批量插入account,invoice data
                                            dataAddToComm(strSite, dealName, haveInvDet, haveVat);
                                            sts = UploadStates.Success;
                                            isSuc = true;
                                        }
                                        else
                                        {
                                            strMessage = "Account level's Customers and invoice level's Customers are inconformity!" + strEnter;
                                        }
                                    }
                                    else
                                    {
                                        strMessage = "Import Data is empty!" + strEnter;
                                    }
                                }

                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(strMessage))
                {
                    strGuid = null;
                    throw new AgingImportException(strMessage, isSuc);
                }
                int listInvDetCount = 0;
                if (invoiceDetailAgingList != null)
                {
                    listInvDetCount = invoiceDetailAgingList.Count;
                }
                int listVatCount = 0;
                if (listvat != null)
                {
                    listVatCount = listvat.Count;
                }

                upLoadHisUpArrow(accFileName, invFileName, sts, dt, invDetailFileName, vatFileName, listVatCount, strSite, listAgingStaging.Count, invoiceAgingList.Count, listInvDetCount, strGuid);
            }
            catch (AgingImportException ex)
            {
                upLoadHisUpArrow(accFileName, invFileName, sts, null, invDetailFileName, vatFileName);
                Helper.Log.Error("Error happended while allFileImportArrow.", ex);
                throw new AgingImportException(ex.Message, ex, isSuc);
            }
            catch (Exception ex)
            {
                upLoadHisUpArrow(accFileName, invFileName, sts, null, invDetailFileName, vatFileName);
                Helper.Log.Error("Error happended while allFileImportArrow.", ex);
                throw new AgingImportException(ex.Message, ex, isSuc);
            }
            finally
            {
                //无论结果如何，需要释放和关闭流StreamReader               
            }

        }

        public string ImportVatOnly() {
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            string strArchiveVATKey = "ArchiveVATPath";
            try{ 
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strArchiveVATKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                    //   return "Path doesn't exsit！";
                }
                string strFileName = archivePath + "\\" + files[0].FileName;

                files[0].SaveAs(strFileName);

                NpoiHelper helper = new NpoiHelper(strFileName);

                helper.ActiveSheet = 0;
                int maxRowNumber = helper.GetLastRowNum();  //获得数据总行数

                List<string> listSQL = new List<string>();
                for (int row = 1; row < maxRowNumber; row++) {
                   
                    InvoiceVatDto vat = new InvoiceVatDto();
                    if (helper.GetCell(row, 0) == null) { continue; }
                    if (helper.GetCell(row, 1) == null) { continue; }
                    if (helper.GetCell(row, 2) == null) { continue; }
                    var TrxNumber = helper.GetCell(row, 0).ToString();
                    var VATInvoice = helper.GetCell(row, 1).ToString();
                    var VATInvoiceTotalAmount = helper.GetCell(row, 2).NumericCellValue;

                    if (TrxNumber.Length > 20) {
                        TrxNumber = TrxNumber.Substring(0, 20);
                    }

                    if (TrxNumber == "" && VATInvoice == "" && VATInvoiceTotalAmount ==0) {
                        continue;
                    }
                     string CreatedDate = DateTime.Now.ToString("yyyy-MM-dd");

                    StringBuilder sql = new StringBuilder();
                    sql.Append("insert into T_INVOICE_VAT(");
                    sql.Append("Trx_Number,VATInvoice,VATInvoiceTotalAmount,CreatedDate");
                    sql.Append(") SELECT");
                    sql.Append("'" + TrxNumber.Replace("'","''") + "',");
                    sql.Append("'" + VATInvoice.Replace("'", "''") + "',");
                    sql.Append(VATInvoiceTotalAmount + ",");
                    sql.Append("'" + CreatedDate + "' WHERE NOT EXISTS (SELECT 1 FROM T_INVOICE_VAT with(nolock) WHERE Trx_Number = '" + TrxNumber.Replace("'", "''") + "' AND VATInvoice = '" + VATInvoice.Replace("'", "''") + "')");
                    listSQL.Add(sql.ToString());

                    }

                SqlHelper.ExcuteListSql(listSQL);
            }
                catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!");
            }
            return "Success";
        }

        public string ImportInvoiceDetailOnly()
        {
            FileType fileT = FileType.InvoiceDetail;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            bool isSaved = false;
            try
            {
                //upload file to server
                int fileId = 0;
                string strArchiveInvoiceDetailKey = "ArchiveInvoiceDetailPath";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strArchiveInvoiceDetailKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                    //   return "Path doesn't exsit！";
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf");

                if (files[0].ContentType == "application/x-gzip" || files[0].ContentType == "application/x-tar")
                {
                    string strFileName = files[0].FileName;
                    string strExtension = Path.GetExtension(files[0].FileName).ToUpper();
                    if (strExtension == ".GZ" || strExtension == ".TAR")
                    {
                        strFileName = archivePath + "\\" + files[0].FileName;
                        files[0].SaveAs(strFileName);

                        string strExtendName = "";
                        //如果有第二节扩展名，则获得第二节扩展名的文件名（.tar.gz）
                        string strInnerFileName = Path.GetFileNameWithoutExtension(files[0].FileName);
                        string strInnerExtension = Path.GetExtension(strInnerFileName).ToUpper();
                        if (strInnerExtension == ".TAR")
                        {
                            strFileName = ungzip(strFileName, archiveFileName + strInnerExtension.ToLower(), true);
                            strExtension = strInnerExtension;
                        }
                        //读取压缩包中的文件(如果是2次压缩，读取的已经是内层的文件了),判断是什么格式的内部文件
                        strExtendName = ".csv";
                        archiveFileName = archiveFileName + strExtendName;
                        if (strExtension.ToLower() == ".zip")
                        {
                            using (ZipArchive zipArchive = System.IO.Compression.ZipFile.Open(strFileName, ZipArchiveMode.Read))
                            {
                                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                                {
                                    string strExtend = getFileExtendName(entry.Name).ToUpper();
                                    if (strExtend.ToUpper() == ".XLSX" || strExtend.ToUpper() == ".XLS" || strExtend.ToUpper() == ".CSV")
                                    {
                                        strExtendName = strExtend.ToLower();
                                        break;
                                    }
                                }
                            }
                            unZip(strFileName, archiveFileName);
                        }
                        if (strExtension.ToLower() == ".gz")
                        {
                            ungzip(strFileName, archiveFileName, true);
                        }
                        if (strExtension.ToLower() == ".tar")
                        {
                            using (FileStream fr = new FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                ICSharpCode.SharpZipLib.Tar.TarInputStream s = new ICSharpCode.SharpZipLib.Tar.TarInputStream(fr);
                                ICSharpCode.SharpZipLib.Tar.TarEntry theEntry;
                                while ((theEntry = s.GetNextEntry()) != null)
                                {
                                    string fileName = Path.GetFileName(theEntry.Name);
                                    if (fileName != String.Empty)
                                    {
                                        string strExtend = getFileExtendName(fileName);
                                        if (strExtend.ToUpper() == ".XLSX" || strExtend.ToUpper() == ".XLS" || strExtend.ToUpper() == ".CSV")
                                        {
                                            strExtendName = strExtend.ToLower();
                                            break;
                                        }
                                    }
                                }
                            }
                            unTar(strFileName, archiveFileName, true);
                        }
                        isSaved = true;
                    }
                }
                else
                {
                    string strFileName = files[0].FileName;
                    string strExtension = Path.GetExtension(files[0].FileName).ToUpper();
                    archiveFileName = archiveFileName + strExtension;
                }

                UploadFile(isSaved, files[0], archiveFileName, fileT, ref fileId, true);

                return uploadInvoiceDetail(fileId.ToString());
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!");
            }
        }

        public string ImportSAPInvoiceOnly()
        {
            FileType fileT = FileType.SAPInvoice;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            bool isSaved = false;
            try
            {
                //upload file to server
                int fileId = 0;
                string strArchiveSAPInvoiceKey = "ArchiveInvoiceLevelPath";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strArchiveSAPInvoiceKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf");

                if (files[0].ContentType == "application/x-gzip" || files[0].ContentType == "application/x-tar")
                {
                    string strFileName = files[0].FileName;
                    string strExtension = Path.GetExtension(files[0].FileName).ToUpper();
                    if (strExtension == ".GZ" || strExtension == ".TAR")
                    {
                        strFileName = archivePath + "\\" + files[0].FileName;
                        files[0].SaveAs(strFileName);

                        string strExtendName = "";
                        //如果有第二节扩展名，则获得第二节扩展名的文件名（.tar.gz）
                        string strInnerFileName = Path.GetFileNameWithoutExtension(files[0].FileName);
                        string strInnerExtension = Path.GetExtension(strInnerFileName).ToUpper();
                        if (strInnerExtension == ".TAR")
                        {
                            strFileName = ungzip(strFileName, archiveFileName + strInnerExtension.ToLower(), true);
                            strExtension = strInnerExtension;
                        }
                        //读取压缩包中的文件(如果是2次压缩，读取的已经是内层的文件了),判断是什么格式的内部文件
                        strExtendName = ".csv";
                        archiveFileName = archiveFileName + strExtendName;
                        if (strExtension.ToLower() == ".zip")
                        {
                            using (ZipArchive zipArchive = System.IO.Compression.ZipFile.Open(strFileName, ZipArchiveMode.Read))
                            {
                                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                                {
                                    string strExtend = getFileExtendName(entry.Name).ToUpper();
                                    if (strExtend.ToUpper() == ".XLSX" || strExtend.ToUpper() == ".XLS" || strExtend.ToUpper() == ".CSV")
                                    {
                                        strExtendName = strExtend.ToLower();
                                        break;
                                    }
                                }
                            }
                            unZip(strFileName, archiveFileName);
                        }
                        if (strExtension.ToLower() == ".gz")
                        {
                            ungzip(strFileName, archiveFileName, true);
                        }
                        if (strExtension.ToLower() == ".tar")
                        {
                            using (FileStream fr = new FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                ICSharpCode.SharpZipLib.Tar.TarInputStream s = new ICSharpCode.SharpZipLib.Tar.TarInputStream(fr);
                                ICSharpCode.SharpZipLib.Tar.TarEntry theEntry;
                                while ((theEntry = s.GetNextEntry()) != null)
                                {
                                    string fileName = Path.GetFileName(theEntry.Name);
                                    if (fileName != String.Empty)
                                    {
                                        string strExtend = getFileExtendName(fileName);
                                        if (strExtend.ToUpper() == ".XLSX" || strExtend.ToUpper() == ".XLS" || strExtend.ToUpper() == ".CSV")
                                        {
                                            strExtendName = strExtend.ToLower();
                                            break;
                                        }
                                    }
                                }
                            }
                            unTar(strFileName, archiveFileName, true);
                        }
                        isSaved = true;
                    }
                }
                else
                {
                    string strFileName = files[0].FileName;
                    string strExtension = Path.GetExtension(files[0].FileName).ToUpper();
                    archiveFileName = archiveFileName + strExtension;
                }

                UploadFile(isSaved, files[0], archiveFileName, fileT, ref fileId, true);

                return UploadSAPInvoice(fileId.ToString());
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!");
            }
        }

        public string ImportVatOnly(FileUploadHistory fileUpHis)
        {
            string strMessage = string.Empty;
            try
            {
                string strGuid = System.Guid.NewGuid().ToString("N");
                //string dealName = string.Empty;

                //获取当前账期
                int? perId = null;
                var curP = getcurrentPeroidOnlyOne(CurrentDeal);
                if (curP != 0)
                {
                    perId = curP;
                }
                if (perId != null)
                {
                    Helper.Log.Info("vat file Upload finished! Dataimport Start!");
                    //得到一个List
                    arrowVatDataImport(strGuid, fileUpHis);

                    if (listvat.Count > 0)
                    {
                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                        {
                            string DelSql;

                            DelSql = " TRUNCATE TABLE T_INVOICE_VAT_STAGING ";
                            CommonRep.GetDBContext().Database.ExecuteSqlCommand(DelSql);

                            CommonRep.BulkInsert(listvat);
                            CommonRep.Commit();

                            scope.Complete();
                        }
                        fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Success);
                        fileUpHis.DataSize = listvat.Count;
                        fileUpHis.ImportId = strGuid;
                        //vatFileName.LegalEntity = vatLegal;
                        fileUpHis.ReportTime = CurrentTime;
                        fileUpHis.PeriodId = perId;
                        CommonRep.Commit();
                    }
                    strMessage = "VAT Upload Successful!";
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                CommonRep.Commit();
                strMessage = "vat Upload failed!";
                throw new OTCServiceException("Uploaded file error!");
            }
            return strMessage;
        }

        public string ImportInvoiceDetailOnly(FileUploadHistory fileUpHis)
        {
            string strMessage = string.Empty;
            try
            {
                string strGuid = System.Guid.NewGuid().ToString("N");
                //string dealName = string.Empty;

                //获取当前账期
                int? perId = null;
                var curP = getcurrentPeroidOnlyOne(CurrentDeal);
                if (curP != 0)
                {
                    perId = curP;
                }
                if (perId != null)
                {
                    Helper.Log.Info("Invoice Detail file Upload finished! Dataimport Start!");
                    //得到一个List
                    arrowInvoiceDetailDataImport(strGuid, null, fileUpHis, null);

                    if (invoiceDetailAgingList.Count > 0)
                    {
                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                        {
                            string DelSql;

                            DelSql = " DELETE T_Invoice_Detail_Staging";
                            CommonRep.GetDBContext().Database.ExecuteSqlCommand(DelSql);
                            CommonRep.BulkInsert(invoiceDetailAgingList);
                            CommonRep.Commit();
                            scope.Complete();
                        }
                    }
                    fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Success);
                    fileUpHis.DataSize = listvat.Count;
                    fileUpHis.ImportId = strGuid;
                    fileUpHis.ReportTime = CurrentTime;
                    fileUpHis.PeriodId = perId;
                    CommonRep.Commit();
                    strMessage = "Invoice Detail Upload Successful!";
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                CommonRep.Commit();
                strMessage = "Invoice Detail Upload failed!";
                throw new OTCServiceException("Uploaded file error!");
            }
            return strMessage;
        }

        public string getFileExtendName(string strFileName)
        {
            string strExtendName = "";
            if (Path.GetExtension(strFileName).ToUpper() == ".XLSX")
            {
                strExtendName = ".xlsx";
            }
            else if (Path.GetExtension(strFileName).ToUpper() == ".XLS")
            {
                strExtendName = ".xls";
            }
            else if (Path.GetExtension(strFileName).ToUpper() == ".CSV")
            {
                strExtendName = ".csv";
            }
            return strExtendName;
        }

        public void unZip(string zipfile, string archiveFileName)
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

                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);

                    // create directory
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    if (fileName != String.Empty)
                    {
                        using (FileStream streamWriter = File.Create(archiveFileName))
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
                    }
                }
            }
        }

        //ADD 解压gz
        public string ungzip(string path, string decomPath, bool overwrite)
        {
            //for overwriting purposes
            if (File.Exists(decomPath))
            {
                if (overwrite)
                {
                    File.Delete(decomPath);
                }
                else
                {
                    Exception ex = new IOException("The decompressed path you specified already exists and cannot be overwritten.");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }
            }
            //create our file streams
            GZipStream stream = new GZipStream(new FileStream(path, FileMode.Open, FileAccess.ReadWrite), CompressionMode.Decompress);
            FileStream decompressedFile = new FileStream(decomPath, FileMode.OpenOrCreate, FileAccess.Write);
            //data represents a byte from the compressed file
            //it's set through each iteration of the while loop
            int data;
            while ((data = stream.ReadByte()) != -1) //iterates over the data of the compressed file and writes the decompressed data
            {
                decompressedFile.WriteByte((byte)data);
            }
            //close our file streams 
            decompressedFile.Close();
            stream.Close();
            return decomPath;
        }
        //END 解压gz

        /// <summary>
        /// 解压.tar
        /// </summary>
        /// <param name="path"></param>
        /// <param name="decomPath"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public void unTar(string path, string decomPath, bool overwrite)
        {
            //for overwriting purposes
            if (File.Exists(decomPath))
            {
                if (overwrite)
                {
                    File.Delete(decomPath);
                }
                else
                {
                    Exception ex = new IOException("The decompressed path you specified already exists and cannot be overwritten.");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }
            }
            FileStream fr = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            ICSharpCode.SharpZipLib.Tar.TarInputStream s = new ICSharpCode.SharpZipLib.Tar.TarInputStream(fr);
            ICSharpCode.SharpZipLib.Tar.TarEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = Path.GetDirectoryName(theEntry.Name);
                string fileName = Path.GetFileName(theEntry.Name);

                if (fileName != String.Empty)
                {
                    FileStream streamWriter = File.Create(decomPath);
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
                    streamWriter.Close();
                }
            }
            s.Close();
            fr.Close();
        }

        /// <summary>
        /// arrow by lilanfu -日期转换
        /// </summary>
        /// <param name="strData">yyyyMMdd</param>
        /// <param name="arrow">arrow标识</param>
        /// <returns></returns>
        private DateTime dataConvertToDT(string strData, string arrow)
        {
            DateTime dt = new DateTime();
            if (!string.IsNullOrEmpty(strData.Trim()))
            {
                return DateTime.ParseExact(strData, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
            }

            return dt;
        }

        /// <summary>
        /// arrow by lilanfu- ACCOUNT,INVOICE内部CUSTOMER校验
        /// </summary>
        /// <param name="arrow">arrow标识字段</param>
        /// <returns></returns>
        private bool isSameCust(string arrow)
        {
            bool rtn;
            rtn = true;
            return rtn;
        }

        /// <summary>
        /// arrow by lilanfu-AccountData Import
        /// </summary>
        /// <param name="strGuid">SameGuid</param>
        /// <param name="strSite">orgid in system</param>
        public void arrowAccountDataImport(string strGuid, string strSite, FileUploadHistory accFileName, string deal)
        {
            string strpath = "";

            custList = new List<Customer>(); //List<Customer>
            Customer cust = new Customer(); //Entity-Customer out     

            List<Customer> allCustList = new List<Customer>(); //List<Customer>                                    
            allCustList = GetCustomer(deal).ToList();
            try
            {
                if (accFileName == null)
                {
                    Exception ex = new Exception("import account file is not found!");
                    Helper.Log.Error("Error happended while arrowAccountDataImport.", ex);
                    throw ex;
                }

                //read excel file
                strpath = accFileName.ArchiveFileName;
                //读取数据用Excel 和CSV
                var typeName = Path.GetExtension(strpath);

                if (typeName.ToUpper() == ".XLS")
                {
                    excelToListByAcc(strpath);
                }
                else if (typeName.ToUpper() == ".CSV")
                {
                    csvToListByAcc(strpath);
                }
                //数据的加工
                if (listAgingStaging.Count > 0)
                {
                    foreach (var custaging in listAgingStaging)
                    {
                        if (!ArrowtryGetCustomer(custaging.CustomerNum, custaging.CustomerName, custaging.SiteUseId, deal, allCustList, custList, out cust))
                        {

                            cust = new Customer()
                            {
                                CustomerNum = custaging.CustomerNum,
                                CustomerName = custaging.CustomerName,
                                SiteUseId = custaging.SiteUseId,
                                Deal = deal,
                                IsHoldFlg = "0",
                                AutoReminderFlg = "0",
                                ExcludeFlg = "0",                                                                   //0:Not Assign
                                RemoveFlg = "1",
                                CreateTime = CurrentTime,
                                CreditTrem = custaging.CreditTerm,
                                CreditLimit = custaging.CreditLimit,//Over Credit Lmt
                                Organization = strSite,
                                Ebname = custaging.Ebname,
                                IsAMS = true//IsAMS = false update 17/12/01
                            };
                        }
                        if (!custList.Exists(q => q.CustomerNum == cust.CustomerNum && q.SiteUseId == cust.SiteUseId))
                        {
                            custList.Add(cust);
                        }

                        custaging.Deal = deal;
                        custaging.LegalEntity = strSite;

                        custaging.BillGroupCode = "";
                        custaging.BillGroupName = "";
                        custaging.TotalAmt = (custaging.DUE15_AMT == null ? 0 : custaging.DUE15_AMT)
                            + (custaging.Due30Amt == null ? 0 : custaging.Due30Amt)
                            + (custaging.DUE45_AMT == null ? 0 : custaging.DUE45_AMT)
                            + (custaging.Due60Amt == null ? 0 : custaging.Due60Amt)
                            + (custaging.Due90Amt == null ? 0 : custaging.Due90Amt)
                            + (custaging.Due120Amt == null ? 0 : custaging.Due120Amt)
                            + (custaging.Due180Amt == null ? 0 : custaging.Due180Amt)
                            + (custaging.Due270Amt == null ? 0 : custaging.Due270Amt)
                            + (custaging.Due360Amt == null ? 0 : custaging.Due360Amt)
                            + (custaging.DueOver360Amt == null ? 0 : custaging.DueOver360Amt)
                            + (custaging.TotalFutureDue == null ? 0 : custaging.TotalFutureDue);
                        custaging.CurrentAmt = custaging.TotalFutureDue == null ? 0 : custaging.TotalFutureDue;
                        custaging.AccountStatus = strAccountStats;
                        custaging.CreateDate = AppContext.Current.User.Now;
                        custaging.Operator = AppContext.Current.User.EID;
                        custaging.ImportId = strGuid;
                        custaging.IsHoldFlg = cust.IsHoldFlg == null ? "0" : cust.IsHoldFlg;
                    }
                }

            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new AgingImportException("Failed to extract account level aging data!");
            }
            finally
            {

            }

        }

        /// <summary>
        /// arrow by lilanfu-InvoiceData Import
        /// </summary>
        /// <param name="strGuid">SameGuid</param>
        /// <param name="strSite">orgid in system</param>
        public void arrowInvoiceDataImport(string strGuid, string strSite, FileUploadHistory invFileName, string deal)
        {
            string strpath = "";
            try
            {
                if (invFileName == null)
                {
                    Exception ex = new Exception("import invoice file is not found!");
                    Helper.Log.Error("Error happended while arrowInvoiceDataImport.", ex);
                    throw ex;
                }

                //read excel file
                strpath = invFileName.ArchiveFileName;
                //读取数据用Excel 和CSV
                var typeName = Path.GetExtension(strpath);

                if (typeName.ToUpper() == ".XLS")
                {
                    excelToListByInv(strpath);
                }
                else if (typeName.ToUpper() == ".CSV")
                {
                    csvToListByInv(strpath);
                }
                //数据的加工
                if (invoiceAgingList.Count > 0)
                {
                    foreach (var invaging in invoiceAgingList)
                    {
                        invaging.Deal = deal;
                        invaging.States = strInvStats;
                        invaging.LegalEntity = strSite;
                        invaging.ImportId = strGuid;
                        invaging.BalanceAmt = invaging.OriginalAmt;
                        invaging.CreateDate = AppContext.Current.User.Now;
                        invaging.Operator = AppContext.Current.User.EID;
                        invaging.BillGroupCode = "";
                        invaging.CustomerName = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new AgingImportException("Failed to extract invoice level aging data!");
            }
            finally
            {

            }
        }

        /// <summary>
        /// arrow by lilanfu-InvoiceDetailData Import
        /// </summary>
        /// <param name="strGuid">SameGuid</param>
        /// <param name="strSite">orgid in system</param>
        public void arrowInvoiceDetailDataImport(string strGuid, string strSite, FileUploadHistory invDetailFileName, string deal)
        {

            string strpath = "";

            try
            {
                if (invDetailFileName == null)
                {
                    Exception ex = new Exception("import invoice detail file is not found!");
                    Helper.Log.Error("Error happended while arrowInvoiceDetailDataImport.", ex);
                    throw ex;
                }

                //read excel file
                strpath = invDetailFileName.ArchiveFileName;

                //读取数据用Excel 和CSV
                var typeName = Path.GetExtension(strpath);

                if (typeName.ToUpper() == ".XLS")
                {
                    excelToListByInvDet(strpath);
                }
                else if (typeName.ToUpper() == ".CSV")
                {
                    csvToListByInvDet(strpath);
                }
                //数据的加工
                if (invoiceDetailAgingList.Count > 0)
                {
                    foreach (var invDet in invoiceDetailAgingList)
                    {
                        invDet.Import_ID = strGuid;
                    }
                }
            }

            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new AgingImportException("Failed to extract invoice detail level aging data!");
            }
            finally
            {

            }

        }

        #region "excel"
        public void excelToListByVat(string strpath)
        {
            try
            {
                T_INVOICE_VAT_STAGING vat;
                listvat = new List<T_INVOICE_VAT_STAGING>();

                bool blStartData = true;
                NpoiHelper helper = new NpoiHelper(strpath);

                int i = 1;
                string strTrxNumber;
                string strLineNumber;
                string strSalesOrder;
                string strCreationDate;
                string strCustomerTrxId;
                string strAttributeCategory;
                string strOrgId;
                string strVatInvoice;
                string strVatInvDate;
                string strVatInvAmt;
                string strVatTaxAmt;
                string strVatInvTotalAmt;

                do
                {
                    strTrxNumber = helper.GetStringData(i, 0) == null ? null : helper.GetStringData(i, 0).ToString();
                    if (string.IsNullOrEmpty(strTrxNumber)) { continue; }
                    strLineNumber = helper.GetValue(i, 1) == null ? null : helper.GetValue(i, 1).ToString();
                    strSalesOrder = helper.GetValue(i, 2) == null ? null : helper.GetValue(i, 2).ToString();
                    ICell cell = helper.GetCell(i, 3);
                    if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                    {
                        strCreationDate = cell.DateCellValue == null ? "" : cell.DateCellValue.ToString("yyyy/MM/dd");
                    }
                    else
                    {
                        strCreationDate = helper.GetValue(i, 3) == null ? "" : helper.GetValue(i, 3).ToString();
                    }
                    strCustomerTrxId = helper.GetValue(i, 4) == null ? null : helper.GetValue(i, 4).ToString();
                    strAttributeCategory = helper.GetValue(i, 5) == null ? null : helper.GetValue(i, 5).ToString();
                    strOrgId = helper.GetValue(i, 6) == null ? null : helper.GetValue(i, 6).ToString();
                    strVatInvoice = helper.GetValue(i, 7) == null ? null : helper.GetValue(i, 7).ToString();
                    ICell cell1 = helper.GetCell(i, 8);
                    if (cell1.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell1))
                    {
                        strVatInvDate = cell1.DateCellValue == null ? "" : cell1.DateCellValue.ToString("yyyy/MM/dd");
                    }
                    else
                    {
                        strVatInvDate = helper.GetValue(i, 8) == null ? "" : helper.GetValue(i, 8).ToString();
                    }
                    strVatInvAmt = helper.GetValue(i, 9) == null ? null : helper.GetValue(i, 9).ToString();
                    strVatTaxAmt = helper.GetValue(i, 10) == null ? null : helper.GetValue(i, 10).ToString();
                    strVatInvTotalAmt = helper.GetValue(i, 11) == null ? null : helper.GetValue(i, 11).ToString();

                    i = i + 1;

                    //"Factory Group Name"
                    if (
                        string.IsNullOrEmpty(strTrxNumber)
                        && string.IsNullOrEmpty(strLineNumber)
                        && string.IsNullOrEmpty(strSalesOrder)
                        && string.IsNullOrEmpty(strCreationDate)
                        && string.IsNullOrEmpty(strCustomerTrxId)
                        && string.IsNullOrEmpty(strAttributeCategory)
                        && string.IsNullOrEmpty(strOrgId)
                        && string.IsNullOrEmpty(strVatInvoice)
                        && string.IsNullOrEmpty(strVatInvDate)
                        && string.IsNullOrEmpty(strVatInvAmt)
                        && string.IsNullOrEmpty(strVatTaxAmt)
                        && string.IsNullOrEmpty(strVatInvTotalAmt)
                        )
                    {
                        blStartData = false;
                        continue;
                    }
                    else if (strTrxNumber == "Trx Number" && strLineNumber == "Line Number")
                    {
                        continue;
                    }
                    else if (!string.IsNullOrEmpty(strTrxNumber))
                    {

                        vat = new T_INVOICE_VAT_STAGING();

                        vat.Trx_Number = strTrxNumber;
                        vat.LineNumber = dataConvertToInt(strLineNumber);
                        vat.SalesOrder = strSalesOrder;
                        vat.CreationDate = dataConvertToDT(strCreationDate);
                        vat.CustomerTrxId = strCustomerTrxId;
                        vat.AttributeCategory = strAttributeCategory;
                        vat.OrgId = strOrgId;
                        vat.VATInvoice = strVatInvoice;
                        vat.VATInvoiceDate = strVatInvDate;
                        vat.VATInvoiceAmount = dataConvertToDec(strVatInvAmt);
                        vat.VATTaxAmount = dataConvertToDec(strVatTaxAmt);
                        vat.VATInvoiceTotalAmount = dataConvertToDec(strVatInvTotalAmt);

                        listvat.Add(vat);
                    }

                } while (!(string.IsNullOrEmpty(strTrxNumber)) && blStartData);
            }
            catch (Exception ex)
            {
                throw new AgingImportException("Failed to extract invoice detail level aging data !");
            }
        }

        public void excelToListByInvDet(string strpath)
        {
            try
            {
                T_Invoice_Detail_Staging invoiceDetail;
                invoiceDetailAgingList = new List<T_Invoice_Detail_Staging>();

                bool blStartData = true;
                NpoiHelper helper = new NpoiHelper(strpath);

                int i = 0;
                string strInvoiceDate;
                string strCustomerPO;
                string strManufacturer;
                string strPartNumber;
                string strInvoiceNumber;
                string strInvoiceLineNumber;
                string strTransactionCurrencyCode;
                string strInvoiceQty;
                string strUnitResales;
                string strNSB;
                string strCost;
                string strGPD;
                string strCostOriginal;
                do
                {
                    strInvoiceDate = helper.GetDateData(i, 0) == null ? null : helper.GetDateData(i, 0).ToString();
                    strCustomerPO = helper.GetValue(i, 1) == null ? null : helper.GetValue(i, 1).ToString();
                    strManufacturer = helper.GetValue(i, 2) == null ? null : helper.GetValue(i, 2).ToString();
                    strPartNumber = helper.GetValue(i, 3) == null ? null : helper.GetValue(i, 3).ToString();
                    strInvoiceNumber = helper.GetValue(i, 4) == null ? null : helper.GetValue(i, 4).ToString();
                    strInvoiceLineNumber = helper.GetValue(i, 5) == null ? null : helper.GetValue(i, 5).ToString();
                    strTransactionCurrencyCode = helper.GetValue(i, 6) == null ? null : helper.GetValue(i, 6).ToString();
                    strInvoiceQty = helper.GetValue(i, 7) == null ? null : helper.GetValue(i, 7).ToString();
                    strUnitResales = helper.GetValue(i, 8) == null ? null : helper.GetValue(i, 8).ToString();
                    strNSB = helper.GetValue(i, 9) == null ? null : helper.GetValue(i, 9).ToString();
                    strCost = helper.GetValue(i, 10) == null ? null : helper.GetValue(i, 10).ToString();
                    strGPD = helper.GetValue(i, 11) == null ? null : helper.GetValue(i, 11).ToString();
                    strCostOriginal = helper.GetValue(i, 12) == null ? null : helper.GetValue(i, 12).ToString();
                    i = i + 1;

                    //"Factory Group Name"
                    if (
                        string.IsNullOrEmpty(strInvoiceDate)
                        && string.IsNullOrEmpty(strCustomerPO)
                        && string.IsNullOrEmpty(strManufacturer)
                        && string.IsNullOrEmpty(strPartNumber)
                        && string.IsNullOrEmpty(strInvoiceNumber)
                        && string.IsNullOrEmpty(strInvoiceLineNumber)
                        && string.IsNullOrEmpty(strTransactionCurrencyCode)
                        && string.IsNullOrEmpty(strInvoiceQty)
                        && string.IsNullOrEmpty(strUnitResales)
                        && string.IsNullOrEmpty(strNSB)
                        && string.IsNullOrEmpty(strCost)
                        && string.IsNullOrEmpty(strGPD)
                        && string.IsNullOrEmpty(strCostOriginal))
                    {
                        blStartData = false;
                        continue;
                    }
                    else if (strInvoiceNumber == "Invoice Number" && strCustomerPO == "Customer PO #")
                    {
                        continue;
                    }
                    else if (!string.IsNullOrEmpty(strInvoiceNumber))
                    {

                        invoiceDetail = new T_Invoice_Detail_Staging();

                        invoiceDetail.InvoiceDate = dataConvertToDT(strInvoiceDate);
                        invoiceDetail.CustomerPO = strCustomerPO;
                        invoiceDetail.Manufacturer = strManufacturer;
                        invoiceDetail.PartNumber = strPartNumber;
                        invoiceDetail.InvoiceNumber = strInvoiceNumber;
                        invoiceDetail.InvoiceLineNumber = dataConvertToInt(strInvoiceLineNumber);
                        invoiceDetail.TransactionCurrencyCode = strTransactionCurrencyCode;
                        invoiceDetail.InvoiceQty = dataConvertToDec(strInvoiceQty);
                        invoiceDetail.UnitResales = dataConvertToDec(strUnitResales);
                        invoiceDetail.NSB = dataConvertToDec(strNSB);
                        invoiceDetail.Cost = dataConvertToDec(strCost);
                        invoiceDetail.GPD = dataConvertToDec(strGPD);
                        invoiceDetail.Cost_Original = dataConvertToDec(strCostOriginal);

                        invoiceDetailAgingList.Add(invoiceDetail);
                    }

                } while (!(string.IsNullOrEmpty(strInvoiceNumber)) && blStartData);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new AgingImportException("Failed to extract invoice detail level aging data !");
            }
        }

        public void excelToListByInv(string strpath)
        {
            try
            {
                InvoiceAgingStaging invaging;
                invoiceAgingList = new List<InvoiceAgingStaging>();

                bool blStartData = true;

                NpoiHelper helper = new NpoiHelper(strpath);

                int i = 0;
                string strCustomerName;
                string strAccntNumber;
                string strSiteUseId;
                string strSellingLocationCode;
                string strClass;
                string strTrxNum;
                string strTrxDate;
                string strDueDate;
                string strPaymentTermName;
                string strOverCreditLmt;
                string strOverCreditLmtAcct;
                string strFuncCurrCode;
                string strInvCurrCode;
                string strSalesName;
                string strDueDays;
                string strAmtRemaining;
                string strAmountWoVat;
                string strAgingBucket;
                string strPaymentTermDesc;
                string strSellingLocationCode2;
                string strEbname;
                string strCustomertype;
                string strIsr;
                string strFsr;
                string strOrgId;
                string strCmpinv;
                string strSalesOrder;
                string strCpo;
                string strFsrNameHist;
                string strIsrNameHist;
                string strEb;

                do
                {
                    strCustomerName = helper.GetValue(i, 0) == null ? null : helper.GetValue(i, 0).ToString();
                    strAccntNumber = helper.GetValue(i, 1) == null ? null : helper.GetValue(i, 1).ToString();
                    strSiteUseId = helper.GetValue(i, 2) == null ? null : helper.GetValue(i, 2).ToString();
                    strSellingLocationCode = helper.GetValue(i, 3) == null ? null : helper.GetValue(i, 3).ToString();
                    strClass = helper.GetValue(i, 4) == null ? null : helper.GetValue(i, 4).ToString();
                    strTrxNum = helper.GetValue(i, 5) == null ? null : helper.GetValue(i, 5).ToString();
                    strTrxDate = helper.GetDateData(i, 6) == null ? null : helper.GetDateData(i, 6).ToString();
                    strDueDate = helper.GetDateData(i, 7) == null ? null : helper.GetDateData(i, 7).ToString();
                    strPaymentTermName = helper.GetValue(i, 8) == null ? null : helper.GetValue(i, 8).ToString();
                    strOverCreditLmt = helper.GetValue(i, 9) == null ? null : helper.GetValue(i, 9).ToString();
                    strOverCreditLmtAcct = helper.GetValue(i, 10) == null ? null : helper.GetValue(i, 10).ToString();
                    strFuncCurrCode = helper.GetValue(i, 11) == null ? null : helper.GetValue(i, 11).ToString();
                    strInvCurrCode = helper.GetValue(i, 12) == null ? null : helper.GetValue(i, 12).ToString();
                    strSalesName = helper.GetValue(i, 13) == null ? null : helper.GetValue(i, 13).ToString();
                    strDueDays = helper.GetValue(i, 14) == null ? null : helper.GetValue(i, 14).ToString();
                    strAmtRemaining = helper.GetValue(i, 15) == null ? null : helper.GetValue(i, 15).ToString();
                    strAmountWoVat = helper.GetValue(i, 16) == null ? null : helper.GetValue(i, 16).ToString();
                    strAgingBucket = helper.GetValue(i, 17) == null ? null : helper.GetValue(i, 17).ToString();
                    strPaymentTermDesc = helper.GetValue(i, 18) == null ? null : helper.GetValue(i, 18).ToString();
                    strSellingLocationCode2 = helper.GetValue(i, 19) == null ? null : helper.GetValue(i, 19).ToString();
                    strEbname = helper.GetValue(i, 20) == null ? null : helper.GetValue(i, 20).ToString();
                    strCustomertype = helper.GetValue(i, 21) == null ? null : helper.GetValue(i, 21).ToString();
                    strIsr = helper.GetValue(i, 22) == null ? null : helper.GetValue(i, 22).ToString();
                    strFsr = helper.GetValue(i, 23) == null ? null : helper.GetValue(i, 23).ToString();
                    strOrgId = helper.GetValue(i, 24) == null ? null : helper.GetValue(i, 24).ToString();
                    strCmpinv = helper.GetValue(i, 25) == null ? null : helper.GetValue(i, 25).ToString();
                    strSalesOrder = helper.GetValue(i, 26) == null ? null : helper.GetValue(i, 26).ToString();
                    strCpo = helper.GetValue(i, 27) == null ? null : helper.GetValue(i, 27).ToString();
                    strFsrNameHist = helper.GetValue(i, 28) == null ? null : helper.GetValue(i, 28).ToString();
                    strIsrNameHist = helper.GetValue(i, 29) == null ? null : helper.GetValue(i, 29).ToString();
                    strEb = helper.GetValue(i, 30) == null ? null : helper.GetValue(i, 30).ToString();

                    i = i + 1;

                    //"Factory Group Name"
                    if (
                        string.IsNullOrEmpty(strCustomerName)
                        && string.IsNullOrEmpty(strAccntNumber)
                        && string.IsNullOrEmpty(strSiteUseId)
                        && string.IsNullOrEmpty(strSellingLocationCode)
                        && string.IsNullOrEmpty(strClass)
                        && string.IsNullOrEmpty(strTrxNum)
                        && string.IsNullOrEmpty(strTrxDate)
                        && string.IsNullOrEmpty(strDueDate))
                    {
                        blStartData = false;
                        continue;
                    }
                    else if (strCustomerName == "Customer Name" && strAccntNumber == "Accnt Number")
                    {
                        continue;
                    }
                    else if (!string.IsNullOrEmpty(strCustomerName))
                    {
                        invaging = new InvoiceAgingStaging();
                        invaging.InvoiceNum = strTrxNum;
                        invaging.CustomerNum = strAccntNumber;
                        invaging.CustomerName = strCustomerName;
                        invaging.CreditTrem = strPaymentTermName;
                        invaging.PoNum = strCpo;
                        invaging.SoNum = strSalesOrder;
                        invaging.Class = strClass;
                        invaging.Currency = strInvCurrCode;
                        invaging.InvoiceDate = dataConvertToDT(strTrxDate);
                        invaging.DueDate = dataConvertToDT(strDueDate);
                        invaging.OriginalAmt = dataConvertToDec(strAmtRemaining);
                        invaging.DaysLateSys = dataConvertToInt(strDueDays);
                        invaging.SiteUseId = strSiteUseId;
                        invaging.SellingLocationCode = strSellingLocationCode;
                        invaging.Sales = strSalesName;
                        invaging.Fsr = strFsr;
                        invaging.FuncCurrCode = strFuncCurrCode;
                        invaging.WoVat_AMT = dataConvertToDec(strAmountWoVat);
                        invaging.AgingBucket = strAgingBucket;
                        invaging.CreditTremDescription = strPaymentTermDesc;
                        invaging.SellingLocationCode2 = strSellingLocationCode2;
                        invaging.Ebname = strEbname;
                        invaging.Customertype = strCustomertype;
                        invaging.Cmpinv = strCmpinv;
                        invaging.CreditLmt = dataConvertToDec(strOverCreditLmt);
                        invaging.CreditLmtAcct = dataConvertToDec(strOverCreditLmtAcct);
                        invaging.FsrNameHist = strFsrNameHist;
                        invaging.LsrNameHist = strIsrNameHist;
                        invaging.Eb = strEb;

                        invoiceAgingList.Add(invaging);
                    }

                } while (!(string.IsNullOrEmpty(strCustomerName)) && blStartData);
            }
            catch (Exception ex)
            {
                throw new AgingImportException("Failed to extract invoice level aging data!");
            }
        }

        public void excelToListByAcc(string strpath)
        {
            CustomerAgingStaging custaging;
            listAgingStaging = new List<CustomerAgingStaging>();
            bool blStartData = true;
            try
            {
                NpoiHelper helper = new NpoiHelper(strpath);

                int i = 1;
                string strOrgId;
                string strCustomerName;
                string strAccntNumber;
                string strSiteUseId;
                string strPaymentTermDesc;
                string strEbname;
                string strOverCreditLmt;
                string strFuncCurrCode;
                string strFsr;
                string strAmtRemaining1;//001-015
                string strAmtRemaining2;//016-030
                string strAmtRemaining3;//031-045
                string strAmtRemaining4;//046-060
                string strAmtRemaining5;//061-090
                string strAmtRemaining6;//091 - 120
                string strAmtRemaining7;//120+
                string strTotalFutureDue;//total_future_due
                do
                {
                    strOrgId = helper.GetValue(i, 0) == null ? null : helper.GetValue(i, 0).ToString();
                    strCustomerName = helper.GetValue(i, 1) == null ? null : helper.GetValue(i, 1).ToString();
                    strAccntNumber = helper.GetValue(i, 2) == null ? null : helper.GetValue(i, 2).ToString();
                    strSiteUseId = helper.GetValue(i, 3) == null ? null : helper.GetValue(i, 3).ToString();
                    strPaymentTermDesc = helper.GetValue(i, 4) == null ? null : helper.GetValue(i, 4).ToString();
                    strEbname = helper.GetValue(i, 5) == null ? null : helper.GetValue(i, 5).ToString();
                    strOverCreditLmt = helper.GetValue(i, 6) == null ? null : helper.GetValue(i, 6).ToString();
                    strFuncCurrCode = helper.GetValue(i, 7) == null ? null : helper.GetValue(i, 7).ToString();
                    strFsr = helper.GetValue(i, 8) == null ? null : helper.GetValue(i, 8).ToString();
                    strAmtRemaining1 = helper.GetValue(i, 9) == null ? null : helper.GetValue(i, 9).ToString();
                    strAmtRemaining2 = helper.GetValue(i, 10) == null ? null : helper.GetValue(i, 10).ToString();
                    strAmtRemaining3 = helper.GetValue(i, 11) == null ? null : helper.GetValue(i, 11).ToString();
                    strAmtRemaining4 = helper.GetValue(i, 12) == null ? null : helper.GetValue(i, 12).ToString();
                    strAmtRemaining5 = helper.GetValue(i, 13) == null ? null : helper.GetValue(i, 13).ToString();
                    strAmtRemaining6 = helper.GetValue(i, 14) == null ? null : helper.GetValue(i, 14).ToString();
                    strAmtRemaining7 = helper.GetValue(i, 15) == null ? null : helper.GetValue(i, 15).ToString();
                    strTotalFutureDue = helper.GetValue(i, 16) == null ? null : helper.GetValue(i, 16).ToString();

                    i = i + 1;

                    //"Factory Group Name"                  
                    if (
                        string.IsNullOrEmpty(strOrgId)
                        && string.IsNullOrEmpty(strCustomerName)
                        && string.IsNullOrEmpty(strAccntNumber)
                        && string.IsNullOrEmpty(strSiteUseId)
                        && string.IsNullOrEmpty(strPaymentTermDesc)
                        && string.IsNullOrEmpty(strEbname)
                        && string.IsNullOrEmpty(strOverCreditLmt)
                        && string.IsNullOrEmpty(strFuncCurrCode)
                        && string.IsNullOrEmpty(strFsr)
                        && string.IsNullOrEmpty(strAmtRemaining1)
                        && string.IsNullOrEmpty(strAmtRemaining2)
                        && string.IsNullOrEmpty(strAmtRemaining3)
                        && string.IsNullOrEmpty(strAmtRemaining4)
                        && string.IsNullOrEmpty(strAmtRemaining5)
                        && string.IsNullOrEmpty(strAmtRemaining6)
                        && string.IsNullOrEmpty(strAmtRemaining7)
                        && string.IsNullOrEmpty(strTotalFutureDue))
                    {
                        blStartData = false;
                        continue;
                    }
                    else if (strCustomerName == "Customer Name" && strAccntNumber == "Accnt Number")
                    {
                        continue;
                    }
                    else if (!string.IsNullOrEmpty(strCustomerName))
                    {
                        custaging = new CustomerAgingStaging();

                        custaging.CustomerNum = strAccntNumber;
                        custaging.CustomerName = strCustomerName;
                        custaging.SiteUseId = strSiteUseId;
                        custaging.CreditTerm = strPaymentTermDesc;
                        custaging.CreditLimit = dataConvertToDec(strOverCreditLmt);
                        custaging.Ebname = strEbname;
                        custaging.TotalFutureDue = dataConvertToDec(strTotalFutureDue);
                        custaging.Currency = strFuncCurrCode;
                        custaging.DUE15_AMT = dataConvertToDec(strAmtRemaining1);
                        custaging.Due30Amt = dataConvertToDec(strAmtRemaining2);
                        custaging.DUE45_AMT = dataConvertToDec(strAmtRemaining3);
                        custaging.Due60Amt = dataConvertToDec(strAmtRemaining4);
                        custaging.Due90Amt = dataConvertToDec(strAmtRemaining5);
                        custaging.Due120Amt = dataConvertToDec(strAmtRemaining6);
                        custaging.Due150Amt = dataConvertToDec(strAmtRemaining7);
                        custaging.Sales = strFsr;

                        listAgingStaging.Add(custaging);
                    }

                } while (!(string.IsNullOrEmpty(strCustomerName)) && blStartData);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new AgingImportException("Failed to extract account level aging data!");
            }
        }

        public void excelToListBySAPInv(string strpath)
        {
            try
            {
                InvoiceAgingStaging invaging;
                invoiceAgingList = new List<InvoiceAgingStaging>();

                NpoiHelper helper = new NpoiHelper(strpath);

                string strOrgId;
                string strCustomerName;
                string strCustomerNameLocal;
                string strCustomerNum;
                string strInvNum;
                string strAssignment;
                string strBillingNo;
                string strInvDate;
                string strDueDate;
                string strAmtRemaining;
                string strSales;
                string strCs;
                string strSiteUseId;
                string strInvCurrCode;
                string strDueDays = "";

                int i = 7;

                do
                {
                    strOrgId = helper.GetValue(i, 2) == null ? null : helper.GetValue(i, 2).ToString();
                    strCustomerNameLocal = helper.GetValue(i, 3) == null ? null : helper.GetValue(i, 3).ToString();
                    strCustomerName = helper.GetValue(i, 4) == null ? null : helper.GetValue(i, 4).ToString();
                    strCustomerNum = helper.GetValue(i, 5) == null ? null : helper.GetValue(i, 5).ToString();
                    strInvNum = helper.GetValue(i, 6) == null ? null : helper.GetValue(i, 6).ToString();
                    strAssignment = helper.GetValue(i, 8) == null ? null : helper.GetValue(i, 8).ToString();
                    strBillingNo = helper.GetValue(i, 9) == null ? null : helper.GetValue(i, 9).ToString();
                    strInvDate = helper.GetDateData(i, 10) == null ? null : helper.GetDateData(i, 10).ToString();
                    strDueDate = helper.GetDateData(i, 11) == null ? null : helper.GetDateData(i, 11).ToString();
                    strAmtRemaining = helper.GetValue(i, 12) == null ? null : helper.GetValue(i, 12).ToString();
                    strSales = helper.GetValue(i, 13) == null ? null : helper.GetValue(i, 13).ToString();
                    strCs = helper.GetValue(i, 14) == null ? null : helper.GetValue(i, 14).ToString();
                    strInvCurrCode = "USD";
                    if (!string.IsNullOrWhiteSpace(strDueDate))
                    {
                        strDueDays = (DateTime.Today - DateTime.Parse(strDueDate)).TotalDays.ToString();
                    }

                    i = i + 1;

                    //"Factory Group Name"
                    if (string.IsNullOrEmpty(strCustomerNum)
                        && string.IsNullOrEmpty(strOrgId)
                        && string.IsNullOrEmpty(strInvNum))
                    {
                        break;
                    }
                    else
                    {
                        invaging = new InvoiceAgingStaging();
                        invaging.Deal = "Arrow";
                        invaging.InvoiceNum = strInvNum;
                        invaging.CustomerNum = string.Format("{0}_{1}", strOrgId, strCustomerNum);
                        invaging.CustomerName = strCustomerName;
                        invaging.Class = "INV";
                        invaging.Currency = strInvCurrCode;
                        invaging.InvoiceDate = dataConvertToDT(strInvDate);
                        invaging.DueDate = dataConvertToDT(strDueDate);
                        invaging.OriginalAmt = dataConvertToDec(strAmtRemaining);
                        invaging.BalanceAmt = dataConvertToDec(strAmtRemaining);
                        invaging.LegalEntity = strOrgId;
                        invaging.DaysLateSys = dataConvertToInt(strDueDays);
                        invaging.SiteUseId = invaging.CustomerNum;
                        invaging.Sales = strSales;
                        invaging.Assignment = strAssignment;
                        invaging.BillingNo = strBillingNo;
                        invaging.Remark = strCustomerNameLocal;
                        invaging.LsrNameHist = strCs;
                        invaging.Fsr = strSales;
                        invaging.FsrNameHist = strSales;
                        invaging.Operator = AppContext.Current.User.EID;
                        invaging.CreateDate = AppContext.Current.User.Now;

                        invoiceAgingList.Add(invaging);
                    }

                } while (true);
            }
            catch (Exception ex)
            {
                throw new AgingImportException("Failed to extract sap invoice aging data!");
            }
        }
        #endregion

        /*
        public void excelToListBySAPInv(string strpath)
        {
            try
            {
                InvoiceAgingStaging invaging;
                invoiceAgingList = new List<InvoiceAgingStaging>();

                NpoiHelper helper = new NpoiHelper(strpath);

                string strOrgId;
                string strCustomerName;
                string strCustomerNameLocal;
                string strCustomerNum;
                string strInvNum;
                string strAssignment;
                string strBillingNo;
                string strInvDate;
                string strDueDate;
                string strAmtRemaining;
                string strSales;
                string strCs;
                string strSiteUseId;
                string strInvCurrCode;
                string strDueDays = "";
                string strInternalText = "";
                string strRemainingAmtTran = "";

                int i = 7;

                do
                {
                    strOrgId = helper.GetValue(i, 0) == null ? null : helper.GetValue(i, 0).ToString();
                    strCustomerNameLocal = helper.GetValue(i, 14) == null ? null : helper.GetValue(i, 14).ToString();
                    strCustomerName = helper.GetValue(i, 14) == null ? null : helper.GetValue(i, 14).ToString();
                    strCustomerNum = helper.GetValue(i, 2) == null ? null : helper.GetValue(i, 2).ToString();
                    strInvNum = helper.GetValue(i, 5) == null ? null : helper.GetValue(i, 5).ToString();
                    strAssignment = helper.GetValue(i, 9) == null ? null : helper.GetValue(i, 9).ToString();
                    strBillingNo = helper.GetValue(i, 22) == null ? null : helper.GetValue(i, 22).ToString();
                    strInvDate = helper.GetDateData(i, 4) == null ? null : helper.GetDateData(i, 4).ToString();
                    strDueDate = helper.GetDateData(i, 6) == null ? null : helper.GetDateData(i, 6).ToString();
                    strAmtRemaining = helper.GetValue(i, 12) == null ? null : helper.GetValue(i, 12).ToString();
                    strRemainingAmtTran = helper.GetValue(i, 13) == null ? null : helper.GetValue(i, 13).ToString();
                    strSales = helper.GetValue(i, 15) == null ? null : helper.GetValue(i, 15).ToString();
                    strCs = helper.GetValue(i, 21) == null ? null : helper.GetValue(i, 21).ToString();
                    strInternalText = helper.GetValue(i, 20) == null ? null : helper.GetValue(i, 20).ToString();
                    strInvCurrCode = helper.GetValue(i, 10) == null ? null : helper.GetValue(i, 10).ToString();
                    if (!string.IsNullOrWhiteSpace(strDueDate))
                    {
                        strDueDays = (DateTime.Today - DateTime.Parse(strDueDate)).TotalDays.ToString();
                    }

                    i = i + 1;

                    //"Factory Group Name"
                    if (string.IsNullOrEmpty(strCustomerNum)
                        && string.IsNullOrEmpty(strOrgId)
                        && string.IsNullOrEmpty(strInvNum))
                    {
                        break;
                    }
                    else
                    {
                        invaging = new InvoiceAgingStaging();
                        invaging.Deal = "Arrow";
                        invaging.InvoiceNum = strInvNum;
                        invaging.CustomerNum = string.Format("{0}_{1}", strOrgId, strCustomerNum);
                        invaging.CustomerName = string.IsNullOrEmpty(strCustomerName) ? invaging.CustomerNum : strCustomerName;
                        invaging.Class = "INV";
                        invaging.Currency = strInvCurrCode;
                        invaging.InvoiceDate = dataConvertToDT(strInvDate);
                        invaging.DueDate = dataConvertToDT(strDueDate);
                        invaging.OriginalAmt = dataConvertToDec(strAmtRemaining);
                        invaging.BalanceAmt = dataConvertToDec(strAmtRemaining);
                        invaging.LegalEntity = strOrgId;
                        invaging.DaysLateSys = dataConvertToInt(strDueDays);
                        invaging.SiteUseId = invaging.CustomerNum;
                        invaging.Sales = strSales;
                        invaging.Assignment = strAssignment;
                        invaging.BillingNo = strBillingNo;
                        invaging.Remark = strCustomerNameLocal;
                        invaging.LsrNameHist = strCs;
                        invaging.Fsr = strSales;
                        invaging.FsrNameHist = strSales;
                        invaging.Internaltext = strInternalText;
                        invaging.RemainingAmtTran = dataConvertToDec(strRemainingAmtTran);
                        invaging.Operator = AppContext.Current.User.EID;
                        invaging.CreateDate = AppContext.Current.User.Now;

                        invoiceAgingList.Add(invaging);
                    }

                } while (true);
            }
            catch (Exception ex)
            {
                throw new AgingImportException("Failed to extract sap invoice aging data!");
            }
        }
        */
        #endregion

        #region "csv"
        public void csvToListByVat(string strpath)
        {
            try
            {
                listvat = new List<T_INVOICE_VAT_STAGING>();

                using (FileStream fs = new FileStream(strpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    CsvReader reader = new CsvReader(new StreamReader(fs, System.Text.Encoding.UTF8));
                    reader.Configuration.Delimiter = ",";
                    reader.Configuration.RegisterClassMap<VatMap>();
                    reader.Read();
                    reader.ReadHeader();
                    listvat = reader.GetRecords<T_INVOICE_VAT_STAGING>().ToList();
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new AgingImportException("Failed to extract vat level aging data!");
            }
        }

        public void csvToListByInvDet(string strpath)
        {
            try
            {
                invoiceDetailAgingList = new List<T_Invoice_Detail_Staging>();

                //如果csv文件为空，则不导入
                using (FileStream fs = new FileStream(strpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    StreamReader sr = new StreamReader(fs);
                    string s;
                    int rowCount = 0;
                    while ((s = sr.ReadLine()) != null)
                    {
                        rowCount++;
                        if (rowCount == 1)
                        {
                            if (s == "The query resulted in no rows")
                            {
                                return;
                            }
                            break;
                        }
                    }
                }

                using (FileStream fs = new FileStream(strpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    CsvReader reader = new CsvReader(new StreamReader(fs, System.Text.Encoding.UTF8));
                    reader.Configuration.RegisterClassMap<InvoiceDetailMap>();
                    reader.Read();
                    reader.ReadHeader();
                    invoiceDetailAgingList = reader.GetRecords<T_Invoice_Detail_Staging>().ToList();
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new AgingImportException("Failed to extract invoice detail level aging data!");
            }
        }

        public void csvToListByInv(string strpath)
        {
            try
            {
                invoiceAgingList = new List<InvoiceAgingStaging>();

                using (FileStream fs = new FileStream(strpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    CsvReader reader = new CsvReader(new StreamReader(fs, System.Text.Encoding.UTF8));
                    reader.Configuration.RegisterClassMap<InvoiceMap>();
                    reader.Read();
                    reader.ReadHeader();
                    invoiceAgingList = reader.GetRecords<InvoiceAgingStaging>().ToList();
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new AgingImportException("Failed to extract invoice level aging data!");
            }
            finally
            {
            }
        }

        public void csvToListByAcc(string strpath)
        {
            string strNewFileName = "";
            try
            {
                #region 由于CSV文件，头2行数据格式有问题，需要先处理一下
                strNewFileName = Path.GetDirectoryName(strpath);
                strNewFileName += "\\" + Path.GetFileNameWithoutExtension(strpath) + "_Copy";
                strNewFileName += Path.GetExtension(strpath);

                listAgingStaging = new List<CustomerAgingStaging>();
                //处理CSV文件，删除第一行数据，并替换第二行Head数据
                using (FileStream fs = new FileStream(strpath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
                {
                    StreamReader sr = new StreamReader(fs);
                    string s;
                    string strFirstRow = "";
                    string strNewFile = "";
                    int rowCount = 0;
                    while ((s = sr.ReadLine()) != null)
                    {
                        rowCount++;
                        if (rowCount == 1)
                        {
                            strFirstRow = s;
                            continue;
                        }
                        if (rowCount == 2)
                        {
                            if (strFirstRow.StartsWith("\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\""))
                            {
                                s = strFirstRow.Replace("\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"", "Org Id,Customer Name,Accnt Number,Site Use Id,Payment Term Desc,Ebname,Over Credit Lmt,Func Curr Code,Fsr");
                            }
                            else if (strFirstRow.StartsWith("AGING_BUCKET,,,,,,,,")) {
                                s = strFirstRow.Replace("AGING_BUCKET,,,,,,,,", "Org Id,Customer Name,Accnt Number,Site Use Id,Payment Term Desc,Ebname,Over Credit Lmt,Func Curr Code,Fsr");
                            }
                        }
                        strNewFile += s + "\r\n";
                    }
                    byte[] myByte = System.Text.Encoding.UTF8.GetBytes(strNewFile);
                    if (File.Exists(strNewFileName))
                    {
                        File.Delete(strNewFileName);
                    }
                    using (FileStream fsNew = new FileStream(strNewFileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                    {
                        fsNew.Write(myByte, 0, myByte.Length);
                    }
                }
                #endregion
                using (FileStream fs = new FileStream(strNewFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    CsvReader reader = new CsvReader(new StreamReader(fs, System.Text.Encoding.UTF8));
                    reader.Configuration.MissingFieldFound = null;
                    reader.Configuration.RegisterClassMap<AccountMap>();
                    reader.Read();
                    reader.ReadHeader();
                    listAgingStaging = reader.GetRecords<CustomerAgingStaging>().ToList();
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new AgingImportException("Failed to extract account level aging data!");
            }
            finally
            {
                if (File.Exists(strNewFileName))
                {
                    File.Delete(strNewFileName);
                }
            }
        }

        public void csvToListBySAPInv(string strpath)
        {
            try
            {
                invoiceAgingList = new List<InvoiceAgingStaging>();

                using (FileStream fs = new FileStream(strpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    CsvReader reader = new CsvReader(new StreamReader(fs, System.Text.Encoding.UTF8));
                    reader.Configuration.RegisterClassMap<SAPInvoiceMap>();
                    reader.Read();
                    reader.ReadHeader();
                    invoiceAgingList = reader.GetRecords<InvoiceAgingStaging>().ToList();
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new AgingImportException("Failed to extract invoice level aging data!");
            }
        }
        #endregion


        /// <summary>
        /// arrow by lilanfu -Site check
        /// </summary>
        /// <param name="fileNameAcc">fileNameAcc</param>
        /// <param name="fileNameInv">fileNameInv</param>
        /// <returns></returns>
        private string strSiteGetArrow(string fileNameAcc, string fileNameInv)
        {
            string rtnSite;
            //rtnSite= Table-SysTypeDetail, 传入DetailName(account文件名（Legal Entity）)，查找DetailValue字段
            rtnSite = strSiteGetFromFileArrow(fileNameAcc);

            //判断三个文件是一个legal entity
            if (!strSiteGetFromNameArrow(fileNameAcc, fileNameInv))
            {
                rtnSite = string.Empty;
                strMessage = "Please upload reports with same legal entity!" + strEnter;
            }

            return rtnSite;
        }

        /// <summary>
        /// arrow by lilanfu -Get Legal Entity from orgid in system
        /// </summary>
        /// <param name="strfileName"></param>
        /// <returns></returns>
        private string strSiteGetFromFileArrow(string strfileName)
        {
            string rtnSite;
            string orgSite;
            orgSite = string.Empty;

            var fileName = System.IO.Path.GetFileNameWithoutExtension(strfileName);
            var fileNameSpile = fileName.Split(strSubSplit);
            rtnSite = fileNameSpile[1];

            //account文件读取（Legal Entity） 为空的情况
            if (string.IsNullOrEmpty(rtnSite))
            {
                strMessage += strfileName + ": Bad file format!" + strEnter + "(Legal Entity is empty!)" + strEnter;
                rtnSite = string.Empty;
            }
            else
            {
                orgSite = rtnSite;

                //arrow-DB Table-SysTypeDetail, 传入DetailName，查找DetailValue字段
                rtnSite = siteGet(rtnSite, "arrow");
                //siteGet方法，传进去orgSite 查不到
                if (string.IsNullOrEmpty(rtnSite))
                {
                    strMessage += strfileName + "'s Legal Entity:[" + orgSite + "] is not found in System!" + strEnter;
                }
            }
            return rtnSite;
        }

        /// <summary>
        /// arrow by lilanfu -site info get from Db
        /// </summary>
        /// <param name="strSite"></param>
        /// <param name="sites"></param>
        /// <returns></returns>
        private string siteGet(string strType, string arrow)
        {
            IJobService Service = SpringFactory.GetObjectImpl<IJobService>("JobService");
            List<SysTypeDetail> sites = Service.GetSysTypeDetail("015");
            if (sites == null)
            {
                return null;
            }
            else
            {
                //arrow-DB Table-SysTypeDetail, 传入DetailName，查找DetailValue字段
                return sites.Where(o => o.DetailName == strType).Select(o => o.DetailValue).SingleOrDefault();
            }

        }

        /// <summary>
        /// arrow by lilanfu 
        /// </summary>
        /// <param name="site">SysTypeDetail, 传入DetailName(account文件第一行（Legal Entity）)，查找DetailValue字段</param>
        /// <param name="fileName">invoiceFileName</param>
        /// <param name="arrow"></param>
        /// <returns></returns>
        private bool strSiteGetFromNameArrow(string fileNameAcc, string fileNameInv)
        {
            var fileNameA = System.IO.Path.GetFileNameWithoutExtension(fileNameAcc);
            var fileNameSpileA = fileNameA.Split(strSubSplit);
            string orgIdAcc = fileNameSpileA[1];

            var fileNameI = System.IO.Path.GetFileNameWithoutExtension(fileNameInv);
            var fileNameSpileI = fileNameI.Split(strSubSplit);
            string orgIdInv = fileNameSpileI[1];

            if (!string.IsNullOrEmpty(orgIdAcc) && !string.IsNullOrEmpty(orgIdInv))
            {
                if (orgIdAcc == orgIdInv)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                strMessage += " Bad file format!" + strEnter;
                return false;
            }
        }

        /// <summary>
        /// arrow by lilanfu-account file Format Check
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="msg"></param>
        /// <param name="arrow"></param>
        /// <returns></returns>
        private DateTime? formartCheckArrow(string fileNameAcc, string fileNameInv, out string msg)
        {
            DateTime? dt;

            dt = null;
            msg = "";

            var dateN = DateTime.Now.Date;

            var fileNameA = System.IO.Path.GetFileNameWithoutExtension(fileNameAcc);
            var fileNameSpileA = fileNameA.Split(strSubSplit);
            string dtAcc = fileNameSpileA[2].Split('-')[1];

            var fileNameI = System.IO.Path.GetFileNameWithoutExtension(fileNameInv);
            var fileNameSpileI = fileNameI.Split(strSubSplit);
            string dtInv = fileNameSpileI[2].Split('-')[1];


            if (!string.IsNullOrEmpty(dtAcc) && !string.IsNullOrEmpty(dtInv))
            {
                if (dtAcc == dtInv)
                {
                    dt = dataConvertToDT(dtAcc, "arrow");
                    if (dt.Value.Date != dateN)
                    {
                        dt = null;
                        msg += "Please upload today's reports!" + strEnter;
                    }
                }
                else
                {
                    msg += "Please upload today's reports!" + strEnter;
                    dt = null;
                }
            }
            else
            {
                msg += " Bad file format!" + strEnter;
                dt = null;
            }
            return dt;
        }

        /// <summary>
        /// arrow by lilanfu-invoice file Format Check
        /// </summary>
        /// <param name="sr">invoice file StreamReader流</param>
        /// <param name="arrow">arrow 标识</param>
        /// <returns></returns>
        private bool invoiceFormartCheck(StreamReader sr, string arrow)
        {
            string line;//数据行读取用
            int i;
            bool isHeaderCheck;

            i = 0;
            isHeaderCheck = false;
            while ((line = sr.ReadLine()) != null)
            {
                i++;
                //读取invoice File,读取到标题行，按照^分开1.第一部分为Statement Date 并且2.^分割为29份
                if (line.Split(strSplit)[0] == strInvoiceHeaderCheck && line.Split(strSplit).Length == intInvoiceCols)
                {
                    return true;
                }
            }
            return isHeaderCheck;
        }

        /// <summary>
        /// arrow by lilanfu-account data Import
        /// </summary>
        /// <param name="sr">account file流</param>
        /// <param name="strSite">Table-SysTypeDetail, 传入DetailName(account文件第一行（Legal Entity）)，查找DetailValue字段</param>
        /// <param name="strGuid">acc,inv同一批导入一个批号</param>
        /// <param name="arrow">arrow标识</param>
        private void dataImport(StreamReader sr, string strSite, string strGuid, string arrow)
        {
            string line;//数据行读取用
            string[] arrData;
            CustomerAgingStaging custaging;
            custList = new List<Customer>();
            listAgingStaging = new List<CustomerAgingStaging>();
            int intCount;

            intCount = 0;

            List<Customer> allCustList = new List<Customer>();
            CustomerTeam custTeam = new CustomerTeam();
            Customer cust = new Customer();
            List<CustomerTeam> AllCustTeamList;
            AllCustTeamList = GetCustomerTeam().ToList();
            allCustList = GetCustomer().ToList();

            while ((line = sr.ReadLine()) != null)
            {
                arrData = line.Split(strSplit);//按照^把行数据分割
                //对每行数据进行校验
                if (!dataCheck(arrData, "arrow"))
                {
                    continue;//跳出这次循环
                }
                //第一列的特殊校验
                if (arrData[0] == strEnd)
                {
                    break;//跳出循环体
                }
                intCount++;
                try
                {

                    if (!TryGetCustomer(arrData[1], arrData[0], allCustList, custList, AllCustTeamList, out cust, out custTeam))
                    {
                        cust = new Customer()
                        {
                            CustomerNum = arrData[1],
                            CustomerName = arrData[0],
                            Deal = CurrentDeal,
                            Country = arrData[6],
                            IsHoldFlg = "0",
                            AutoReminderFlg = "0",
                            ExcludeFlg = getNewICCust(arrData[0]),//11/03 update Ic logic add
                            RemoveFlg = "1",
                            CreateTime = CurrentTime //11/03 Add create time Add
                        };
                    }
                    if (cust.ExcludeFlg == "1")
                    {
                        continue;
                    }
                    custaging = new CustomerAgingStaging();
                    custaging.Deal = CurrentDeal;
                    custaging.LegalEntity = strSite;
                    custaging.CustomerNum = arrData[1];
                    custaging.CustomerName = cust.CustomerName;
                    //BillGrpCode取得
                    custaging.BillGroupCode = cust.BillGroupCode;
                    if (cust.Id > 0)
                    {
                        custaging.BillGroupName = GetAllCustomerGroup().Where(o => o.BillGroupCode == cust.BillGroupCode)
                                                    .Select(o => o.BillGroupName).FirstOrDefault();
                    }
                    else
                    {
                        custaging.BillGroupName = "";
                    }
                    //Country做成必要待确认
                    custaging.Country = arrData[6];
                    //CustomerInfo取得
                    custaging.CreditTerm = getTerm(arrData[3]);
                    custaging.CreditLimit = dataConvertToDec(arrData[4]);
                    if (custTeam != null)
                    {
                        custaging.Collector = custTeam.Collector;
                    }

                    custaging.CollectorSys = arrData[7];
                    custaging.CurrentAmt = dataConvertToDec(arrData[9]);
                    custaging.Due30Amt = dataConvertToDec(arrData[10]);
                    custaging.Due60Amt = dataConvertToDec(arrData[11]);
                    custaging.Due90Amt = dataConvertToDec(arrData[12]);
                    custaging.Due120Amt = dataConvertToDec(arrData[13]);
                    custaging.Due150Amt = dataConvertToDec(arrData[14]);
                    custaging.Due180Amt = dataConvertToDec(arrData[15]);
                    custaging.Due210Amt = dataConvertToDec(arrData[16]);
                    custaging.Due240Amt = dataConvertToDec(arrData[17]);
                    custaging.Due270Amt = dataConvertToDec(arrData[18]);
                    custaging.Due300Amt = dataConvertToDec(arrData[19]);
                    custaging.Due330Amt = dataConvertToDec(arrData[20]);
                    custaging.Due360Amt = dataConvertToDec(arrData[21]);
                    custaging.DueOver360Amt = dataConvertToDec(arrData[22]);
                    custaging.TotalAmt = custaging.CurrentAmt + custaging.Due30Amt +
                                        custaging.Due60Amt + custaging.Due90Amt +
                                        custaging.Due120Amt + custaging.Due150Amt +
                                        custaging.Due180Amt + custaging.Due210Amt +
                                        custaging.Due240Amt + custaging.Due270Amt +
                                        custaging.Due300Amt + custaging.Due330Amt +
                                        custaging.Due360Amt + custaging.DueOver360Amt;
                    custaging.AccountStatus = strAccountStats;
                    custaging.CreateDate = AppContext.Current.User.Now;
                    custaging.Operator = AppContext.Current.User.EID;
                    custaging.Sales = cust.Sales;
                    custaging.ImportId = strGuid;
                    custaging.IsHoldFlg = cust.IsHoldFlg == null ? "0" : cust.IsHoldFlg;

                    //upload all the col
                    custaging.Currency = arrData[2];
                    custaging.CountryCode = arrData[5];
                    custaging.OutstandingAmt = dataConvertToDec(arrData[8]);
                    custaging.CityOrState = arrData[23];
                    custaging.ContactName = arrData[24];
                    custaging.ContactPhone = arrData[25];
                    custaging.CustomerCreditMemo = arrData[26];
                    custaging.CustomerPayments = arrData[27];
                    custaging.CustomerReceiptsAtRisk = arrData[28];
                    custaging.CustomerClaims = arrData[29];
                    custaging.CustomerBalance = dataConvertToDec(arrData[30]);

                    //added by zhangYu NFCusFlg  upload the customer notFind in customerTable
                    if (cust.Id == 0)
                    {
                        custaging.CusExFlg = "0";
                    }
                    else
                    {
                        custaging.CusExFlg = "1";
                    }

                    if (cust.ExcludeFlg == "1")
                    {
                        continue;
                    }
                    listAgingStaging.Add(custaging);
                }
                catch (Exception ex)
                {
                    Helper.Log.Error(ex.Message, ex);
                    throw new AgingImportException("Failed to extract customer level aging data!");
                }
            }

        }

        /// <summary>
        /// arrow by lilanfu,对导入每行的account信息进行校验
        /// </summary>
        /// <param name="arrData"></param>
        /// <param name="arrow"></param>
        /// <returns></returns>
        private bool dataCheck(string[] arrData, string arrow)
        {
            string strCheck;
            //分割成31列
            if (arrData.Length != intAccountCols)
            {
                return false;
            }
            //为了确定不是标题列
            strCheck = arrData[0].Split(strSubSplit)[0];
            if (strCheck == strHeaderCheck)
            {
                return false;
            }
            //为了确定不是最后一列汇总列
            if (strCheck == strSubTatolCheck)
            {
                return false;
            }
            //保证第一列有值
            if (string.IsNullOrEmpty(strCheck.Trim()))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// CustomerInfo取得
        /// </summary>
        /// <param name="CustomerNum"></param>
        /// <param name="custs"></param>
        /// <param name="billGroupInfos"></param>
        /// <returns>false不存在，就需要添加customer</returns>
        private bool ArrowtryGetCustomer(string customerNum, string customerName, string siteUseId, string deal, List<Customer> allCustList, List<Customer> jitCustList, out Customer cust)
        {
            //根据三个条件去customer表里查看是否存在
            cust = (from o in allCustList
                    where o.CustomerNum == customerNum && o.Deal == deal && o.SiteUseId == siteUseId
                    select o).FirstOrDefault();

            if (cust != null)
            {
                if (cust.CustomerName != customerName)
                {
                    //需要改customerName
                    cust.CustomerName = customerName;
                }
                return true;
            }
            else
            {
                //直接不存在的
                return false;
            }
        }

        /// <summary>
        /// 提交数据库
        /// </summary>
        /// <param name="strSite"></param>
        /// <param name="dealName"></param>
        public void dataAddToComm(string strSite, string dealName, bool haveInvDet, bool haveVat)
        {
            try
            {
                string DelSql;
                //T_INVOICE_VAT_STAGING
                if (haveVat)
                {
                    DelSql = " TRUNCATE TABLE T_INVOICE_VAT_STAGING";
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(DelSql);
                }
                //T_Invoice_Detail_Staging
                if (haveInvDet)
                {
                    DelSql = " DELETE T_Invoice_Detail_Staging WHERE T_Invoice_Detail_Staging.InvoiceNumber " +
                            " IN ( SELECT t.InvoiceNumber " +
                            " FROM dbo.T_Invoice_Detail_Staging t WITH (NOLOCK) " +
                            " JOIN dbo.T_INVOICE_AGING i WITH (NOLOCK) " +
                            " ON   t.InvoiceNumber = i.INVOICE_NUM)";
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(DelSql);

                    DelSql = " DELETE T_Invoice_Detail_Staging WHERE T_Invoice_Detail_Staging.InvoiceNumber " +
                                " IN ( SELECT t.InvoiceNumber " +
                                " FROM dbo.T_Invoice_Detail_Staging t WITH (NOLOCK) " +
                                " JOIN dbo.T_INVOICE_AGING_STAGING i WITH (NOLOCK) " +
                                " ON   t.InvoiceNumber = i.INVOICE_NUM)";
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(DelSql);
                }

                DelSql = "delete from T_INVOICE_AGING_STAGING WHERE DEAL = '"
                      + dealName + "' AND LEGAL_ENTITY = '" + strSite + "';";
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(DelSql);

                DelSql = "delete from T_CUSTOMER_AGING_STAGING WHERE DEAL = '"
                           + dealName + "' AND LEGAL_ENTITY = '" + strSite + "';";

                CommonRep.GetDBContext().Database.ExecuteSqlCommand(DelSql);

                CommonRep.BulkInsert(custList.FindAll(cust => cust.Id == 0));
                CommonRep.Commit();

                CommonRep.BulkInsert(listAgingStaging);
                CommonRep.BulkInsert(invoiceAgingList);
                if (haveInvDet)
                {
                    CommonRep.BulkInsert(invoiceDetailAgingList);
                }
                if (haveVat)
                {
                    CommonRep.BulkInsert(listvat);
                }
                CommonRep.Commit();

            }
            catch (Exception ex)
            {
                if (ex.GetType().Name == typeof(DbEntityValidationException).Name)
                {
                    if ((ex as DbEntityValidationException).EntityValidationErrors != null)
                    {

                    }
                }
                Helper.Log.Error(ex.Message, ex);
                throw new AgingImportException("Failed to save customer level aging data!", ex);
            }
        }

        public PeriodControl getcurrentPeroid(string dealName)
        {
            string strDeal = dealName;

            PeroidService perService = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            var curP = perService.GetAllPeroids();
            PeriodControl currentPeroid = new PeriodControl();
            currentPeroid = curP.Where(o => o.Deal == strDeal && o.PeriodBegin <= CurrentTime
                                    && o.PeriodEnd >= CurrentTime).Select(o => o).FirstOrDefault();
            if (currentPeroid != null)
            {
                // is current period
                currentPeroid.IsCurrentFlg = "1";
            }
            else if (currentPeroid == null)
            {
                currentPeroid = curP.Where(o => o.Deal == strDeal).OrderByDescending(o => o.Operatedate)
                    .Select(o => o).FirstOrDefault();
                if (currentPeroid != null)
                {
                    //not current peroid
                    currentPeroid.IsCurrentFlg = "0";
                }
            }
            return currentPeroid;
        }

        public int getcurrentPeroidOnlyOne(string dealName)
        {
            int res = 0;
            string strDeal = dealName;

            PeroidService perService = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            var curP = perService.GetAllPeroids();

            var listPeroid = curP.Where(o => o.Deal == strDeal && o.PeriodBegin <= CurrentTime
                                   && o.PeriodEnd > CurrentTime).Select(o => o);
            if (listPeroid.Count() == 1)
            {
                res = listPeroid.FirstOrDefault().Id;
            }
            return res;
        }

        public void upLoadHisUpArrow(FileUploadHistory accFileName,
                                    FileUploadHistory invFileName
                                    , UploadStates sts
                                    , DateTime? dt
                                    , FileUploadHistory invDetailFileName = null
                                    , FileUploadHistory vatFileName = null
                                    , int datasizeVat = 0
                                    , string strSite = null
                                    , int datasizeAcc = 0
                                    , int datasizeInv = 0
                                    , int datasizeInvDet = 0
                                    , string strImportId = null
                                    )
        {
            accFileName = CommonRep.GetQueryable<FileUploadHistory>().Where(o => o.Id == accFileName.Id).FirstOrDefault();
            invFileName = CommonRep.GetQueryable<FileUploadHistory>().Where(o => o.Id == invFileName.Id).FirstOrDefault();

            if (invDetailFileName != null)
            {
                invDetailFileName = CommonRep.GetQueryable<FileUploadHistory>().Where(o => o.Id == invDetailFileName.Id).FirstOrDefault();
            }
            if (vatFileName != null)
            {
                vatFileName = CommonRep.GetQueryable<FileUploadHistory>().Where(o => o.Id == vatFileName.Id).FirstOrDefault();
            }
            int? perId = null;
            if (accFileName != null && invFileName != null)
            {
                string dealName = string.Empty;
                if (accFileName.Operator == "auto" && invFileName.Operator == "auto")
                {
                    dealName = accFileName.Deal;
                }
                else
                {
                    dealName = CurrentDeal;
                }
                var curP = getcurrentPeroidOnlyOne(dealName);
                if (curP != 0)
                {
                    perId = curP;
                }
                accFileName.ProcessFlag = Helper.EnumToCode<UploadStates>(sts);
                accFileName.ReportTime = dt;
                accFileName.ImportId = strImportId;
                accFileName.DataSize = datasizeAcc;
                if (!string.IsNullOrEmpty(strSite))
                {
                    accFileName.LegalEntity = strSite;
                }
                accFileName.PeriodId = perId;

                invFileName.ProcessFlag = Helper.EnumToCode<UploadStates>(sts);
                invFileName.ReportTime = dt;
                invFileName.ImportId = strImportId;
                invFileName.DataSize = datasizeInv;
                if (!string.IsNullOrEmpty(strSite))
                {
                    invFileName.LegalEntity = strSite;
                }
                invFileName.PeriodId = perId;

                if (invDetailFileName != null)
                {
                    invDetailFileName.ProcessFlag = Helper.EnumToCode<UploadStates>(sts);
                    invDetailFileName.ReportTime = dt;
                    invDetailFileName.ImportId = strImportId;
                    invDetailFileName.DataSize = datasizeInvDet;
                    if (!string.IsNullOrEmpty(strSite))
                    {
                        invDetailFileName.LegalEntity = strSite;
                    }
                    invDetailFileName.PeriodId = perId;
                }

                if (vatFileName != null)
                {
                    vatFileName.ProcessFlag = Helper.EnumToCode<UploadStates>(sts);
                    vatFileName.ReportTime = dt;
                    vatFileName.DataSize = datasizeVat;
                    vatFileName.ImportId = strImportId;
                    if (!string.IsNullOrEmpty(strSite))
                    {
                        vatFileName.LegalEntity = strSite;
                    }
                    vatFileName.PeriodId = perId;
                }

                CommonRep.Commit();
            }
        }

        public IQueryable<Customer> GetCustomer(string deal)
        {
            return CommonRep.GetQueryable<Customer>().Where(c => c.Deal == deal
                                                        && c.RemoveFlg == "1").Include<Customer, CustomerGroupCfg>(c => c.CustomerGroupCfg);
        }


        public List<UploadLegal> getLegalHisByDate(string date)
        {
            List<UploadLegal> rtnInfos = new List<UploadLegal>();
            UploadLegal rtnInfo = new UploadLegal();

            string strDeal = AppContext.Current.User.Deal.ToString();

            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            //当前deal下所有的filehis
            List<FileUploadHistory> filehis = fileService.GetFileUploadHistory().Where(o => o.Deal == AppContext.Current.User.Deal).ToList();

            List<Sites> sites = new List<Sites>();

            SiteService siteService = SpringFactory.GetObjectImpl<SiteService>("SiteService");
            sites = siteService.GetAllSites().Where(o => o.Deal == strDeal).ToList();

            foreach (Sites site in sites)
            {
                rtnInfo = new UploadLegal();

                rtnInfo.LegalEntity = site.LegalEntity;
                rtnInfo.StateAcc = false;
                rtnInfo.StateInv = false;
                rtnInfo.StateInvDet = false;
                rtnInfo.StateVat = false;
                rtnInfos.Add(rtnInfo);
            }

            var searchDate = dataConvertToDT(date, "arrow").Date;
            //这天的成功文件
            filehis = filehis.Where(o =>
                                        o.FileType != Helper.EnumToCode<FileType>(FileType.OneYearSales)
                                        && o.ProcessFlag == Helper.EnumToCode<UploadStates>(UploadStates.Success)
                                        && o.UploadTime.Date == searchDate && o.ProcessFlag == "1")
                                        .Select(o => o)
                                        .OrderBy(o => o.LegalEntity)
                                        .ThenBy(o => o.FileType)
                                        .ThenByDescending(o => o.UploadTime).ToList();

            foreach (FileUploadHistory his in filehis)
            {
                rtnInfo = new UploadLegal();
                switch (Helper.CodeToEnum<FileType>(his.FileType))
                {
                    case FileType.Account:
                        rtnInfo = rtnInfos.FindAll(o => o.LegalEntity == his.LegalEntity).FirstOrDefault();
                        if (rtnInfo != null)
                        {
                            rtnInfo.StateAcc = true;
                        }
                        break;
                    case FileType.Invoice:
                        rtnInfo = rtnInfos.FindAll(o => o.LegalEntity == his.LegalEntity).FirstOrDefault();
                        if (rtnInfo != null)
                        {
                            rtnInfo.StateInv = true;
                        }
                        break;
                    case FileType.InvoiceDetail:
                        for (int i = 0; i < rtnInfos.Count(); i++)
                        {
                            rtnInfos[i].StateInvDet = true;
                        }
                        break;
                    case FileType.VAT:
                        for (int i = 0; i < rtnInfos.Count(); i++)
                        {
                            rtnInfos[i].StateVat = true;
                        }
                        break;
                }

            }

            return rtnInfos;
        }

        public Dictionary<string, bool> getLegalByDash()
        {
            Dictionary<string, bool> dic = new Dictionary<string, bool>();
            string legal = string.Empty;
            bool bolLegal = false;

            string strDeal = AppContext.Current.User.Deal.ToString();

            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            //当前deal下所有的filehis
            List<FileUploadHistory> filehis = fileService.GetFileUploadHistory().Where(o => o.Deal == AppContext.Current.User.Deal).ToList();

            List<Sites> sites = new List<Sites>();

            SiteService siteService = SpringFactory.GetObjectImpl<SiteService>("SiteService");
            sites = siteService.GetAllSites().Where(o => o.Deal == strDeal).ToList();

            filehis = filehis.Where(o =>
                                        o.FileType != Helper.EnumToCode<FileType>(FileType.OneYearSales)
                                        && o.ProcessFlag == Helper.EnumToCode<UploadStates>(UploadStates.Success)
                                        && o.UploadTime.Date == CurrentTime.Date)
                                        .Select(o => o)
                                        .OrderBy(o => o.LegalEntity)
                                        .ThenBy(o => o.FileType)
                                        .ThenByDescending(o => o.UploadTime).ToList();

            foreach (Sites site in sites)
            {
                legal = string.Empty;
                bolLegal = false;

                legal = site.LegalEntity;

                if (filehis.Where(o => o.LegalEntity == legal && o.FileType == Helper.EnumToCode<FileType>(FileType.Account)).ToList().Count() > 0
                    && filehis.Where(o => o.LegalEntity == legal && o.FileType == Helper.EnumToCode<FileType>(FileType.Invoice)).ToList().Count() > 0)
                {
                    bolLegal = true;
                }

                dic.Add(legal, bolLegal);
            }
            return dic;
        }

        public PeriodControl getcurrentPeroid()
        {
            string strDeal = AppContext.Current.User.Deal.ToString();

            PeriodControl currentPeroid = new PeriodControl();
            currentPeroid = GetAllPeroids().Where(o => o.Deal == strDeal && o.PeriodBegin <= CurrentTime
                                    && o.PeriodEnd >= CurrentTime).Select(o => o).FirstOrDefault();
            if (currentPeroid != null)
            {
                // is current period
                currentPeroid.IsCurrentFlg = "1";
            }
            else if (currentPeroid == null)
            {
                currentPeroid = GetAllPeroids().Where(o => o.Deal == strDeal).OrderByDescending(o => o.Operatedate)
                    .Select(o => o).FirstOrDefault();
                if (currentPeroid != null)
                {
                    //not current peroid
                    currentPeroid.IsCurrentFlg = "0";
                }
            }
            return currentPeroid;
        }

        /// <summary>
        /// Get All Peroids from Db order by PeriodEnd Desc
        /// </summary>
        /// <returns></returns>
        public List<PeriodControl> GetAllPeroids()
        {
            return CommonRep.GetDbSet<PeriodControl>().Where(o => o.Deal == AppContext.Current.User.Deal).OrderByDescending(o => o.PeriodEnd).ToList();
        }

        public UploadLegalHisModel getFileHisByDate(int pageindex, int pagesize, string date)
        {
            UploadLegalHisModel model = new UploadLegalHisModel();

            List<UploadLegalHis> rtnInfos = new List<UploadLegalHis>();
            UploadLegalHis rtnInfo = new UploadLegalHis();

            string strDeal = AppContext.Current.User.Deal.ToString();

            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            var searchDate = dataConvertToDT(date, "arrow").Date;
            //当前deal下所有的filehis
            string strAccount = Helper.EnumToCode<FileType>(FileType.Account);
            string strInvoice = Helper.EnumToCode<FileType>(FileType.Invoice);
            string strInvoiceDetail = Helper.EnumToCode<FileType>(FileType.InvoiceDetail);
            string strVAT = Helper.EnumToCode<FileType>(FileType.VAT);
            string strSAP = Helper.EnumToCode<FileType>(FileType.SAPInvoice);
            string strCreditHold = Helper.EnumToCode<FileType>(FileType.CreditHold);
            string strConsigmentNumber = Helper.EnumToCode<FileType>(FileType.ConsigmentNumber);
            string strSuccess = Helper.EnumToCode<UploadStates>(UploadStates.Success);
            List<FileUploadHistory> filehis = fileService.GetFileUploadHistory().Where(o => o.Deal == AppContext.Current.User.Deal
            && (o.FileType == strAccount || o.FileType == strInvoice || o.FileType == strInvoiceDetail || o.FileType == strSAP || o.FileType == strVAT || o.FileType == strCreditHold || o.FileType == strConsigmentNumber)
                                        && o.ProcessFlag == strSuccess
                                        && o.UploadTime >= searchDate)
                                        .Select(o => o)
                                        .OrderByDescending(o => o.UploadTime)
                                        .ThenBy(o => o.LegalEntity)
                                        .ThenBy(o => o.FileType)
                                        .Skip((pageindex - 1) * pagesize).Take(pagesize).ToList()
                ;

            model.TotalItems = fileService.GetFileUploadHistory().Where(o => o.Deal == AppContext.Current.User.Deal
                                 && (o.FileType == strAccount || o.FileType == strInvoice || o.FileType == strInvoiceDetail || o.FileType == strVAT || o.FileType == strCreditHold || o.FileType == strConsigmentNumber)
                                        && o.ProcessFlag == strSuccess
                                        && o.UploadTime >= searchDate).Count();

            foreach (var his in filehis)
            {
                rtnInfo = new UploadLegalHis();
                rtnInfo.LegalEntity = his.LegalEntity;
                rtnInfo.FileType = Helper.CodeToEnum<FileType>(his.FileType).ToString();
                rtnInfo.ReportName = his.OriginalFileName;
                rtnInfo.Operator = his.Operator;
                rtnInfo.OperatorDate = his.UploadTime.ToString("yyyy-MM-dd HH:mm:ss");
                rtnInfo.DownLoadFlg = his.ArchiveFileName;
                rtnInfos.Add(rtnInfo);
            }
            model.List = rtnInfos;

            return model;
        }

        public submitWaitInvDetModel getSubmitWaitInvDet(int pageindex, int pagesize)
        {
            submitWaitInvDetModel model = new submitWaitInvDetModel();
            List<submitWaitInvDet> rtnInfos = new List<submitWaitInvDet>();
            submitWaitInvDet rtnInfo = new submitWaitInvDet();

            string strDeal = AppContext.Current.User.Deal.ToString();

            //当前deal下所有的filehis
            List<T_Invoice_Detail_Staging> listInvDet = GetInvoiceDetail().Where(o => o.Import_ID != null)
                                        .Select(o => o)
                                        .OrderBy(o => o.Id).Skip((pageindex - 1) * pagesize).Take(pagesize).ToList();
            model.TotalItems = GetInvoiceDetail().Where(o => o.Import_ID != null).Count();
            foreach (var invDet in listInvDet)
            {
                rtnInfo = new submitWaitInvDet();

                rtnInfo.InvoiceDate = invDet.InvoiceDate.ToString();
                rtnInfo.CustomerPO = invDet.CustomerPO;
                rtnInfo.Manufacturer = invDet.Manufacturer;
                rtnInfo.PartNumber = invDet.PartNumber;
                rtnInfo.InvoiceNumber = invDet.InvoiceNumber;
                rtnInfo.InvoiceLineNumber = invDet.InvoiceLineNumber;
                rtnInfo.TransactionCurrencyCode = invDet.TransactionCurrencyCode;
                rtnInfo.InvoiceQty = invDet.InvoiceQty;
                rtnInfo.UnitResales = invDet.UnitResales;
                rtnInfo.NSB = invDet.NSB;
                rtnInfos.Add(rtnInfo);
            }
            model.List = rtnInfos;
            return model;
        }

        public submitWaitVatModel getSubmitWaitVat(int pageindex, int pagesize)
        {
            submitWaitVatModel model = new submitWaitVatModel();
            List<submitWaitVat> rtnInfos = new List<submitWaitVat>();
            submitWaitVat rtnInfo = new submitWaitVat();

            string strDeal = AppContext.Current.User.Deal.ToString();

            List<T_INVOICE_VAT_STAGING> listInvVAT = GetVAT().Where(o => o.IMPORT_ID != null)
                                        .Select(o => o)
                                        .OrderBy(o => o.Id).Skip((pageindex - 1) * pagesize).Take(pagesize).ToList();
            model.TotalItems = GetVAT().Where(o => o.IMPORT_ID != null).Count();
            foreach (var VAT in listInvVAT)
            {
                rtnInfo = new submitWaitVat();

                rtnInfo.Trx_Number = VAT.Trx_Number;
                rtnInfo.LineNumber = VAT.LineNumber;
                rtnInfo.SalesOrder = VAT.SalesOrder;
                rtnInfo.CreationDate = VAT.CreationDate.ToString();
                rtnInfo.CustomerTrxId = VAT.CustomerTrxId;
                rtnInfo.AttributeCategory = VAT.AttributeCategory;
                rtnInfo.OrgId = VAT.OrgId;
                rtnInfo.VATInvoice = VAT.VATInvoice;
                rtnInfo.VATInvoiceDate = VAT.VATInvoiceDate.ToString();
                rtnInfo.VATInvoiceAmount = VAT.VATInvoiceAmount;

                rtnInfo.VATTaxAmount = VAT.VATTaxAmount;
                rtnInfo.VATInvoiceTotalAmount = VAT.VATInvoiceTotalAmount;

                rtnInfo.CreationDate = VAT.CreationDate.ToString();
                rtnInfos.Add(rtnInfo);
            }
            model.List = rtnInfos;
            return model;
        }

        public IQueryable<T_Invoice_Detail_Staging> GetInvoiceDetail()
        {
            return CommonRep.GetQueryable<T_Invoice_Detail_Staging>();
        }

        public IQueryable<T_INVOICE_VAT_STAGING> GetVAT()
        {
            return CommonRep.GetQueryable<T_INVOICE_VAT_STAGING>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file">posted file</param>
        /// <param name="filePath">temp save path</param>
        /// <param name="archiveFileName">archive file full path</param>
        /// <param name="fileType"></param>
        /// <param name="cancelUnProcessedFile"></param>
        public void UploadFile(bool isSave, HttpPostedFile file, string archiveFileName, FileType fileType, ref int id
            , bool cancelUnProcessedFile = false)
        {
            AssertUtils.ArgumentHasText(archiveFileName, "File archive path");

            // 1, backup to archive folder    
            if (!isSave)
            {
                file.SaveAs(archiveFileName);
            }

            // 2, Insert the upload file record
            if (cancelUnProcessedFile)
            {
                // cancel all unprocessed file
                string strType = Helper.EnumToCode<FileType>(fileType);
                UpdateProcessFlagCancel(strType);
            }

            //Insert the upload file record
            FileUploadHistory updFileHistory = new FileUploadHistory();
            updFileHistory.Deal = AppContext.Current.User.Deal;
            updFileHistory.OriginalFileName = file.FileName;
            updFileHistory.ArchiveFileName = archiveFileName;
            updFileHistory.FileType = Helper.EnumToCode<FileType>(fileType);
            updFileHistory.Operator = AppContext.Current.User.EID;
            updFileHistory.UploadTime = AppContext.Current.User.Now;
            if (fileType == FileType.Customer || fileType == FileType.PaymentDateCircle
                || fileType == FileType.AccountPeriod)
            {
                updFileHistory.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Success);
            }
            else
            {
                updFileHistory.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Untreated);
            }
            CommonRep.Add(updFileHistory);

            try
            {
                CommonRep.Commit();
                id = updFileHistory.Id;
            }
            catch
            {
                Exception ex = new OTCServiceException("File upload error happened, Please try again later. " + Environment.NewLine
                    + "If this error happen again, please contact system administrator.");
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Update to process flag cancel
        /// </summary>
        /// <param name="strType"></param>
        public void UpdateProcessFlagCancel(string strType)
        {
            string strUntreated = Helper.EnumToCode<UploadStates>(UploadStates.Untreated);
            var lstUntreated = CommonRep.GetQueryable<FileUploadHistory>().Where(o => o.FileType == strType
                                                    && o.Operator == AppContext.Current.User.EID
                                                    && o.ProcessFlag == strUntreated).Select(o => o);
            foreach (var item in lstUntreated)
            {
                item.ProcessFlag = Helper.EnumToCode(UploadStates.Cancel);
            }
            CommonRep.Commit();
        }

        public FileUploadHistory getHisById(int id)
        {
            return CommonRep.GetQueryable<FileUploadHistory>().Where(o => o.Id == id).FirstOrDefault();
        }

        public string uploadAg(string acc, string inv, string invdet = null, string vat = null)
        {
            string strMessage = string.Empty;
            FileUploadHistory accFileName = new FileUploadHistory();
            FileUploadHistory invFileName = new FileUploadHistory();
            FileUploadHistory invDetailFileName = new FileUploadHistory();
            FileUploadHistory vatFileName = new FileUploadHistory();
            invDetailFileName = null;
            vatFileName = null;
            try
            {
                accFileName = getHisById(Convert.ToInt32(acc));
                invFileName = getHisById(Convert.ToInt32(inv));
                if (invdet != null)
                {
                    invDetailFileName = getHisById(Convert.ToInt32(invdet));
                }
                if (vat != null)
                {
                    vatFileName = getHisById(Convert.ToInt32(vat));
                }

                if (accFileName != null && invFileName != null)
                {
                    // account file和invoice file同时上传
                    Helper.Log.Info("Aging file Upload finished! Dataimport Start!");
                    allFileImportArrow(accFileName, invFileName, invDetailFileName, vatFileName);
                    strMessage = " File Upload Successful!";

                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                strMessage = ex.Message.ToString();
            }

            return strMessage;
        }

        /// <summary>
        /// 传一个vat文件的id,读取出来，处理再插入临时表
        /// </summary>
        /// <param name="vat"></param>
        /// <returns></returns>
        public string uploadVat(string vat)
        {
            string strMessage = string.Empty;
            FileUploadHistory vatFileName = new FileUploadHistory();
            try
            {
                vatFileName = getHisById(Convert.ToInt32(vat));
                string strGuid = System.Guid.NewGuid().ToString("N");

                if (vatFileName != null && !string.IsNullOrEmpty(vatFileName.OriginalFileName))
                {
                    //获取当前账期
                    int? perId = null;
                    var curP = getcurrentPeroidOnlyOne(CurrentDeal);
                    if (curP != 0)
                    {
                        perId = curP;
                    }
                    if (perId != null)
                    {
                        Helper.Log.Info("vat file Upload finished! Dataimport Start!");
                        //得到一个List
                        arrowVatDataImport(strGuid, vatFileName);

                        if (listvat.Count > 0)
                        {
                            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                            {
                                string DelSql;

                                DelSql = " TRUNCATE TABLE T_INVOICE_VAT_STAGING";
                                CommonRep.GetDBContext().Database.ExecuteSqlCommand(DelSql);

                                CommonRep.BulkInsert(listvat);
                                CommonRep.Commit();

                                scope.Complete();
                            }
                            vatFileName.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Success);
                            vatFileName.DataSize = listvat.Count;
                            vatFileName.ImportId = strGuid;
                            vatFileName.ReportTime = CurrentTime;
                            vatFileName.PeriodId = perId;
                            CommonRep.Commit();
                        }
                        strMessage = "VAT Upload Successful!";
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);

                vatFileName.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                CommonRep.Commit();
                strMessage = "vat Upload failed!";
                throw new OTCServiceException("Uploaded file error!");
            }

            return strMessage;
        }

        /// <summary>
        /// 传一个vat文件的id,读取出来，处理再插入临时表
        /// </summary>
        /// <param name="InvoiceDetail"></param>
        /// <returns></returns>
        public string uploadInvoiceDetail(string InvoiceDetail)
        {
            string strMessage = string.Empty;
            FileUploadHistory invDetFileName = new FileUploadHistory();
            try
            {
                invDetFileName = getHisById(Convert.ToInt32(InvoiceDetail));
                string strGuid = System.Guid.NewGuid().ToString("N");

                if (invDetFileName != null && !string.IsNullOrEmpty(invDetFileName.OriginalFileName))
                {
                    //获取当前账期
                    int? perId = null;
                    var curP = getcurrentPeroidOnlyOne(CurrentDeal);
                    if (curP != 0)
                    {
                        perId = curP;
                    }
                    if (perId != null)
                    {
                        Helper.Log.Info("Invoice detail file Upload finished! Dataimport Start!");
                        //得到一个List
                        arrowinvDetDataImport(strGuid, invDetFileName);

                        if (invoiceDetailAgingList.Count > 0)
                        {
                            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                            {
                                string DelSql;

                                DelSql = " TRUNCATE TABLE T_INVOICE_DETAIL_STAGING";
                                CommonRep.GetDBContext().Database.ExecuteSqlCommand(DelSql);

                                CommonRep.BulkInsert(invoiceDetailAgingList);
                                CommonRep.Commit();

                                scope.Complete();
                            }
                        }
                        invDetFileName.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Success);
                        invDetFileName.DataSize = invoiceDetailAgingList.Count;
                        invDetFileName.ImportId = strGuid;
                        invDetFileName.ReportTime = CurrentTime;
                        invDetFileName.PeriodId = perId;
                        CommonRep.Commit();
                        strMessage = "Invoice detail Upload Successful!";
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);

                invDetFileName.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                CommonRep.Commit();
                strMessage = "Invoice detail Upload failed!";
                throw new OTCServiceException("Uploaded file error!");
            }

            return strMessage;
        }

        /// <summary>
        /// 传一个vat文件的id,读取出来，处理再插入临时表
        /// </summary>
        /// <param name="sapInvoice"></param>
        /// <returns></returns>
        public string UploadSAPInvoice(string sapInvoice)
        {
            string strMessage = string.Empty;
            FileUploadHistory fileUploadHistory = new FileUploadHistory();
            try
            {
                fileUploadHistory = getHisById(Convert.ToInt32(sapInvoice));
                string strGuid = System.Guid.NewGuid().ToString("N");

                if (fileUploadHistory != null && !string.IsNullOrEmpty(fileUploadHistory.OriginalFileName))
                {
                    //获取当前账期
                    int? perId = null;
                    var curP = getcurrentPeroidOnlyOne(CurrentDeal);
                    if (curP != 0)
                    {
                        perId = curP;
                    }
                    if (perId != null)
                    {
                        Helper.Log.Info("SAP Invoice file Upload finished! Dataimport Start!");
                        //得到一个List
                        ArrowSAPInvoiceDataImport(strGuid, fileUploadHistory);

                        if (invoiceAgingList.Count > 0)
                        {
                            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                            {
                                //T_INVOICE_STAGING
                                string delSql1 = " TRUNCATE TABLE T_INVOICE_AGING_STAGING";
                                CommonRep.GetDBContext().Database.ExecuteSqlCommand(delSql1);
                                CommonRep.BulkInsert(invoiceAgingList);

                                //T_CUSTOMER_AGING_STAGING
                                string delSql2 = " TRUNCATE TABLE T_CUSTOMER_AGING_STAGING";
                                CommonRep.GetDBContext().Database.ExecuteSqlCommand(delSql2);
                                CommonRep.BulkInsert(listAgingStaging);

                                CommonRep.Commit();
                                scope.Complete();
                            }
                        }
                        fileUploadHistory.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Success);
                        fileUploadHistory.DataSize = invoiceAgingList.Count;
                        fileUploadHistory.ImportId = strGuid;
                        fileUploadHistory.ReportTime = CurrentTime;
                        fileUploadHistory.PeriodId = perId;
                        CommonRep.Commit();
                        strMessage = "SAP Invoice Upload Successful!";
                    }
                    else {
                        throw new OTCServiceException("Period not set!");
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);

                fileUploadHistory.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                CommonRep.Commit();
                strMessage = "SAP Invoice detail Upload failed!";
                throw new OTCServiceException("Uploaded file error!");
            }

            return strMessage;
        }

        /// <summary>
        /// arrow by lilanfu-vatData Import,读取和加工
        /// </summary>
        /// <param name="strGuid">SameGuid</param>
        /// <param name="strSite">orgid in system</param>
        public void arrowVatDataImport(string strGuid, FileUploadHistory vat)
        {
            string strpath = "";

            try
            {
                if (vat == null)
                {
                    Exception ex = new Exception("import vat file is not found!");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }

                //read excel file
                strpath = vat.ArchiveFileName;

                //读取数据用Excel 和CSV
                var typeName = Path.GetExtension(strpath);

                if (typeName.ToUpper() == ".XLS")
                {
                    excelToListByVat(strpath);
                }
                else if (typeName.ToUpper() == ".CSV")
                {
                    csvToListByVat(strpath);
                }

                //数据再处理
                if (listvat != null && listvat.Count() > 0)
                {
                    foreach (var temp in listvat)
                    {
                        temp.IMPORT_ID = strGuid;
                        temp.CreatedDate = CurrentTime;
                        temp.CreatedUser = AppContext.Current.User.EID;
                    }
                }

            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new AgingImportException("Failed to extract vat level aging data!");
            }
            finally
            {
            }
        }

        /// <summary>
        /// arrow by lilanfu-vatData Import,读取和加工
        /// </summary>
        /// <param name="strGuid">SameGuid</param>
        /// <param name="strSite">orgid in system</param>
        public void arrowinvDetDataImport(string strGuid, FileUploadHistory invDet)
        {
            string strpath = "";

            try
            {
                if (invDet == null)
                {
                    Exception ex = new Exception("import invoice detail file is not found!");
                    Helper.Log.Error("Error happended while arrowInvoiceDetailDataImport.", ex);
                    throw ex;
                }

                //read excel file
                strpath = invDet.ArchiveFileName;

                //读取数据用Excel 和CSV
                var typeName = Path.GetExtension(strpath);

                if (typeName.ToUpper() == ".XLS")
                {
                    excelToListByInvDet(strpath);
                }
                else if (typeName.ToUpper() == ".CSV")
                {
                    csvToListByInvDet(strpath);
                }

                //数据再处理
                if (invoiceDetailAgingList != null && invoiceDetailAgingList.Count() > 0)
                {
                    foreach (var temp in invoiceDetailAgingList)
                    {
                        temp.Import_ID = strGuid;
                    }
                }

            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new AgingImportException("Failed to extract invoice detail level aging data!");
            }
            finally
            {
            }
        }

        /// <summary>
        /// SAP invoice 文件 数据读取到List
        /// </summary>
        /// <param name="strGuid"></param>
        /// <param name="uploadHistory"></param>
        public void ArrowSAPInvoiceDataImport(string strGuid, FileUploadHistory uploadHistory)
        {
            try
            {
                if (uploadHistory == null)
                {
                    Exception ex = new Exception("import sap invoice file is not found!");
                    Helper.Log.Error("Error happended while ArrowSAPInvoiceDataImport.", ex);
                    throw ex;
                }

                //read excel file
                string strpath = uploadHistory.ArchiveFileName;

                //读取数据用Excel 和CSV
                var typeName = Path.GetExtension(strpath);

                if (typeName.ToUpper() == ".XLS" || typeName.ToUpper() == ".XLSX")
                {
                    excelToListBySAPInv(strpath);
                }
                else if (typeName.ToUpper() == ".CSV")
                {
                    //todo 如果是csv, 需要添加实现
                    csvToListBySAPInv(strpath);
                }

                //数据再处理
                if (invoiceAgingList != null && invoiceAgingList.Count() > 0)
                {
                    foreach (var temp in invoiceAgingList)
                    {
                        temp.ImportId = strGuid;
                    }

                    //通过sap invocie aging 统计 customer aging
                    listAgingStaging = new List<CustomerAgingStaging>();
                    var groups = invoiceAgingList.GroupBy(o=> new { o.LegalEntity, o.CustomerNum });
                    foreach (var group in groups)
                    {
                        var customerAgingStaging = new CustomerAgingStaging();
                        customerAgingStaging.Deal = "Arrow";
                        customerAgingStaging.LegalEntity =group.Key.LegalEntity;
                        customerAgingStaging.CustomerNum = group.Key.CustomerNum;
                        customerAgingStaging.SiteUseId = group.Key.CustomerNum;
                        
                        customerAgingStaging.TotalAmt = group.Sum(o=>o.OriginalAmt);
                        customerAgingStaging.DUE15_AMT = group.Where(o => o.DaysLateSys > 0 && o.DaysLateSys <= 15).Sum(o => o.OriginalAmt);
                        customerAgingStaging.Due30Amt = group.Where(o => o.DaysLateSys > 15 && o.DaysLateSys <= 30).Sum(o => o.OriginalAmt);
                        customerAgingStaging.DUE45_AMT = group.Where(o => o.DaysLateSys > 30 && o.DaysLateSys <= 45).Sum(o => o.OriginalAmt);
                        customerAgingStaging.Due60Amt = group.Where(o => o.DaysLateSys > 45 && o.DaysLateSys <= 60).Sum(o => o.OriginalAmt);
                        customerAgingStaging.Due90Amt = group.Where(o => o.DaysLateSys > 60 && o.DaysLateSys <= 90).Sum(o => o.OriginalAmt);
                        customerAgingStaging.Due120Amt = group.Where(o => o.DaysLateSys > 90 && o.DaysLateSys <= 120).Sum(o => o.OriginalAmt);
                        customerAgingStaging.Due180Amt = group.Where(o => o.DaysLateSys > 120 && o.DaysLateSys <= 180).Sum(o => o.OriginalAmt);
                        customerAgingStaging.Due270Amt = group.Where(o => o.DaysLateSys > 180 && o.DaysLateSys <= 270).Sum(o => o.OriginalAmt);
                        customerAgingStaging.Due360Amt = group.Where(o => o.DaysLateSys > 270 && o.DaysLateSys <= 360).Sum(o => o.OriginalAmt);
                        customerAgingStaging.DueOver360Amt = group.Where(o => o.DaysLateSys > 360).Sum(o => o.OriginalAmt);

                        customerAgingStaging.CurrentAmt = customerAgingStaging.TotalAmt - customerAgingStaging.DUE15_AMT - customerAgingStaging.Due30Amt -customerAgingStaging.DUE45_AMT -customerAgingStaging.Due60Amt -customerAgingStaging.Due90Amt - customerAgingStaging.Due120Amt - customerAgingStaging.Due180Amt- customerAgingStaging.Due270Amt - customerAgingStaging.Due360Amt - customerAgingStaging.DueOver360Amt;

                        customerAgingStaging.AccountStatus = "Draft";

                        var firstModel = group.FirstOrDefault();
                        customerAgingStaging.CustomerName = firstModel.CustomerName;
                        customerAgingStaging.Sales = firstModel.Sales;
                        customerAgingStaging.ImportId = firstModel.ImportId;
                        customerAgingStaging.Currency = firstModel.Currency;
                        customerAgingStaging.TotalFutureDue = customerAgingStaging.CurrentAmt;
                        customerAgingStaging.CreateDate = AppContext.Current.User.Now;
                        customerAgingStaging.Operator = AppContext.Current.User.EID;
                        customerAgingStaging.CollectorSys = firstModel.LsrNameHist;
                        customerAgingStaging.CustomerClaims = firstModel.Remark;

                        listAgingStaging.Add(customerAgingStaging);
                    }
                }

            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new AgingImportException("Failed to extract invoice detail level aging data!");
            }
            finally
            {
            }
        }

        public string GetLegalNewFile(string legal, int type)
        {
            string result = string.Empty;
            string strDeal = AppContext.Current.User.Deal.ToString();
            string strType = Helper.EnumToCode<FileType>(FileType.Account);
            if (type == 1)
            {
                strType = Helper.EnumToCode<FileType>(FileType.Account);
            }
            else if (type == 2)
            {
                strType = Helper.EnumToCode<FileType>(FileType.Invoice);
            }
            else if (type == 3)
            {
                strType = Helper.EnumToCode<FileType>(FileType.InvoiceDetail);
            }
            else if (type == 4)
            {
                strType = Helper.EnumToCode<FileType>(FileType.VAT);
            }

            var fileUpload = CommonRep.GetQueryable<FileUploadHistory>().Where(o => o.FileType == strType
                                         && o.LegalEntity == legal
                                         && o.Deal == strDeal).Select(o => o).OrderByDescending(o => o.UploadTime).FirstOrDefault();
            if (fileUpload != null)
            {
                result = fileUpload.OriginalFileName + "^" + fileUpload.ArchiveFileName;
            }

            return result;
        }

        public void Batch()
        {
            List<string> paths = new List<string>();
            paths.Add(@"C:\Users\lanfu.li\Desktop\arrow_lanfu_li\AR Aging Report\AR 292 detail 20171126.csv");

            BatchDetailInvoice(paths);
        }

        public void BatchDetailInvoice(List<string> paths)
        {
            try
            {
                string deal = CurrentDeal;
                string strGuid = System.Guid.NewGuid().ToString("N");

                foreach (var path in paths)
                {
                    string strpath = "";

                    strpath = path;
                    //读取数据用Excel 和CSV
                    var typeName = Path.GetExtension(strpath);

                    if (typeName.ToUpper() == ".XLS")
                    {
                        excelToListByInv(strpath);
                    }
                    else if (typeName.ToUpper() == ".CSV")
                    {
                        csvToListByInv(strpath);
                    }
                    //数据的加工
                    if (invoiceAgingList.Count > 0)
                    {
                        string legal = string.Empty;
                        legal = strSiteGetFromFileArrow(path);

                        foreach (var invaging in invoiceAgingList)
                        {
                            invaging.Deal = deal;
                            invaging.States = strInvStats;
                            invaging.LegalEntity = legal;
                            invaging.ImportId = strGuid;
                            invaging.BalanceAmt = invaging.OriginalAmt;
                            invaging.CreateDate = AppContext.Current.User.Now;
                            invaging.Operator = AppContext.Current.User.EID;
                            invaging.BillGroupCode = "";
                            invaging.CustomerName = "";
                        }

                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                        {
                            string DelSql = "";
                            DelSql = "delete from T_INVOICE_AGING_STAGING WHERE DEAL = '"
                                  + deal + "' AND LEGAL_ENTITY = '" + legal + "';";
                            CommonRep.GetDBContext().Database.ExecuteSqlCommand(DelSql);

                            CommonRep.BulkInsert(invoiceAgingList);
                            CommonRep.Commit();

                            scope.Complete();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new AgingImportException("Failed to extract data in Batch!");
            }
            finally
            {

            }
        }

    }

    /// <summary>
    /// CSV-VAT Map
    /// </summary>
    public sealed class VatMap : ClassMap<T_INVOICE_VAT_STAGING>
    {
        public VatMap()
        {
            CultureInfo cul = new System.Globalization.CultureInfo("en-US");
            string[] formats = { "MM/dd/yy hh:mm tt", "dd-MMM-yyyy" };
            Map(m => m.Trx_Number).Name("Trx Number");
            Map(m => m.LineNumber).ConvertUsing(row =>
            {
                int? num = null;
                string lINE_NUMBER = row.GetField<string>("Line Number");
                if (!string.IsNullOrEmpty(lINE_NUMBER))
                {
                    num = Convert.ToInt32(lINE_NUMBER);
                }
                return num;
            });

            Map(m => m.SalesOrder).Name("Sales Order");
            Map(m => m.CreationDate).ConvertUsing(row =>
            {
                DateTime? dt = null;
                string cREATION_DATE = row.GetField<string>("Creation Date");
                if (!string.IsNullOrEmpty(cREATION_DATE))
                {
                    DateTime tmp;
                    if (DateTime.TryParseExact(cREATION_DATE, formats, cul, DateTimeStyles.None, out tmp))
                    {
                        dt = tmp;
                    }
                }
                return dt;
            });
            Map(m => m.CustomerTrxId).Name("Customer Trx Id");
            Map(m => m.AttributeCategory).Name("Attribute Category");
            Map(m => m.OrgId).Name("Org Id");
            Map(m => m.VATInvoice).Name("Vat Invoice");
            Map(m => m.VATInvoiceDate).ConvertUsing(row =>
            {
                string v_INV_DATE = row.GetField<string>("Vat Inv Date");
                if (!string.IsNullOrWhiteSpace(v_INV_DATE))
                {
                    if (v_INV_DATE.IndexOf(";") >= 0)
                    {
                        v_INV_DATE = v_INV_DATE.Split(';')[0];
                    }
                }
                return v_INV_DATE;
            });
            Map(m => m.VATInvoiceAmount).ConvertUsing(row =>
            {
                decimal? dl = null;
                string vAT_INV_AMT = row.GetField<string>("Vat Inv Amt");
                vAT_INV_AMT = vAT_INV_AMT.Replace(",", "");
                vAT_INV_AMT = vAT_INV_AMT.Replace("\"", "");
                if (!string.IsNullOrWhiteSpace(vAT_INV_AMT))
                {
                    if (vAT_INV_AMT.IndexOf(";") >= 0)
                    {
                        vAT_INV_AMT = vAT_INV_AMT.Split(';')[0];
                    }
                    if (vAT_INV_AMT.IndexOf("?") >= 0)
                    {
                        vAT_INV_AMT = vAT_INV_AMT.Split('?')[0];
                    }
                    try
                    {
                        dl = Convert.ToDecimal(vAT_INV_AMT);
                    }
                    catch (Exception ex)
                    {
                        Helper.Log.Error(ex.Message, ex);
                        throw ex;
                    }
                }
                return dl;
            });
            Map(m => m.VATTaxAmount).ConvertUsing(row =>
            {
                decimal? dl = 0;
                string vAT_TAX_AMT = row.GetField<string>("Vat Tax Amt");
                vAT_TAX_AMT = vAT_TAX_AMT.Replace(",", "");
                vAT_TAX_AMT = vAT_TAX_AMT.Replace("\"", "");
                if (!string.IsNullOrWhiteSpace(vAT_TAX_AMT))
                {
                    if (vAT_TAX_AMT.IndexOf(";") >= 0)
                    {
                        vAT_TAX_AMT = vAT_TAX_AMT.Split(';')[0];
                    }
                    if (vAT_TAX_AMT.IndexOf("?") >= 0)
                    {
                        vAT_TAX_AMT = vAT_TAX_AMT.Split('?')[0];
                    }
                    try
                    {
                        dl = Convert.ToDecimal(vAT_TAX_AMT);
                    }
                    catch
                    {
                        //如果错误,反算税额
                        //读取发票总额
                        decimal? totalAmt = 0;
                        string vAT_INV_TOTAL_AMT = row.GetField<string>("Vat Inv Total Amt");
                        vAT_INV_TOTAL_AMT = vAT_INV_TOTAL_AMT.Replace(",", "");
                        vAT_INV_TOTAL_AMT = vAT_INV_TOTAL_AMT.Replace("\"", "");
                        if (!string.IsNullOrWhiteSpace(vAT_INV_TOTAL_AMT))
                        {
                            if (vAT_INV_TOTAL_AMT.IndexOf(";") >= 0)
                            {
                                vAT_INV_TOTAL_AMT = vAT_INV_TOTAL_AMT.Split(';')[0];
                            }
                            if (vAT_INV_TOTAL_AMT.IndexOf("?") >= 0)
                            {
                                vAT_INV_TOTAL_AMT = vAT_INV_TOTAL_AMT.Split('?')[0];
                            }
                            try
                            {
                                totalAmt = Convert.ToDecimal(vAT_INV_TOTAL_AMT);
                            }
                            catch (Exception ex)
                            {
                                Helper.Log.Error(ex.Message, ex);
                                throw ex;
                            }
                        }
                        //读取不含税金额
                        decimal? invAmt = 0;
                        string vAT_INV_AMT = row.GetField<string>("Vat Inv Amt");
                        vAT_INV_AMT = vAT_INV_AMT.Replace(",", "");
                        vAT_INV_AMT = vAT_INV_AMT.Replace("\"", "");
                        if (!string.IsNullOrWhiteSpace(vAT_INV_AMT))
                        {
                            if (vAT_INV_AMT.IndexOf(";") >= 0)
                            {
                                vAT_INV_AMT = vAT_INV_AMT.Split(';')[0];
                            }
                            if (vAT_INV_AMT.IndexOf("?") >= 0)
                            {
                                vAT_INV_AMT = vAT_INV_AMT.Split('?')[0];
                            }
                            try
                            {
                                invAmt = Convert.ToDecimal(vAT_INV_AMT);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                        //计算税额
                        dl = totalAmt - invAmt;
                    }
                }
                return dl;
            });
            Map(m => m.VATInvoiceTotalAmount).ConvertUsing(row =>
            {
                decimal? dl = null;
                string vAT_INV_TOTAL_AMT = row.GetField<string>("Vat Inv Total Amt");
                vAT_INV_TOTAL_AMT = vAT_INV_TOTAL_AMT.Replace(",", "");
                vAT_INV_TOTAL_AMT = vAT_INV_TOTAL_AMT.Replace("\"", "");
                if (!string.IsNullOrWhiteSpace(vAT_INV_TOTAL_AMT))
                {
                    if (vAT_INV_TOTAL_AMT.IndexOf(";") >= 0)
                    {
                        vAT_INV_TOTAL_AMT = vAT_INV_TOTAL_AMT.Split(';')[0];
                    }
                    if (vAT_INV_TOTAL_AMT.IndexOf("?") >= 0)
                    {
                        vAT_INV_TOTAL_AMT = vAT_INV_TOTAL_AMT.Split('?')[0];
                    }
                    try
                    {
                        dl = Convert.ToDecimal(vAT_INV_TOTAL_AMT);
                    }
                    catch (Exception ex)
                    {
                        Helper.Log.Error(ex.Message, ex);
                        throw ex;
                    }
                }
                return dl;
            });
        }
    }

    /// <summary>
    /// csv-account map 
    /// </summary>
    public sealed class AccountMap : ClassMap<CustomerAgingStaging>
    {
        public AccountMap()
        {
            Map(m => m.LegalEntity).ConvertUsing(row => {
                string strLegalEntity = row.GetField<string>("Org Id");
                strLegalEntity = Convert.ToInt32(strLegalEntity.Replace(",", "").Split('.')[0]).ToString();
                return strLegalEntity;
            });
            Map(m => m.CustomerName).ConvertUsing(row =>
            {
                string customerName = row.GetField<string>("Customer Name");
                return customerName;
            });
            Map(m => m.CustomerNum).ConvertUsing(row =>
            {
                decimal CustomerNum = row.GetField<decimal>("Accnt Number");
                string strCustomerNum = Convert.ToInt32(CustomerNum).ToString();
                return strCustomerNum;
            });
            Map(m => m.SiteUseId).ConvertUsing(row =>
            {
                string strSiteUseId = row.GetField<string>("Site Use Id");
                strSiteUseId = Convert.ToInt32(strSiteUseId.Replace(",", "").Split('.')[0]).ToString();
                return strSiteUseId;
            });
            Map(m => m.CreditTerm).Name("Payment Term Desc");
            Map(m => m.Ebname).Name("Ebname");
            Map(m => m.CreditLimit).ConvertUsing(row =>
            {
                decimal? dl = null;
                string creditLimit = row.GetField<string>("Over Credit Lmt");
                creditLimit = creditLimit.Replace(",", "");
                if (!string.IsNullOrWhiteSpace(creditLimit))
                {
                    dl = Convert.ToDecimal(creditLimit);
                }
                return dl;
            });
            Map(m => m.Currency).Name("Func Curr Code");
            Map(m => m.Sales).Name("Fsr");
            Map(m => m.DUE15_AMT).ConvertUsing(row =>
            {
                decimal? dl = null;
                string dUE15_AMT = row.GetField<string>("001-015");
                if (!string.IsNullOrEmpty(dUE15_AMT))
                {
                    dUE15_AMT = dUE15_AMT.Replace(",", "");
                    if (!string.IsNullOrWhiteSpace(dUE15_AMT))
                    {
                        dl = Convert.ToDecimal(dUE15_AMT);
                    }
                }
                return dl;
            });
            Map(m => m.Due30Amt).ConvertUsing(row =>
            {
                decimal? dl = null;
                string due30Amt = row.GetField<string>("016-030");
                if (!string.IsNullOrEmpty(due30Amt))
                {
                    due30Amt = due30Amt.Replace(",", "");
                    if (!string.IsNullOrWhiteSpace(due30Amt))
                    {
                        dl = Convert.ToDecimal(due30Amt);
                    }
                }
                return dl;
            });
            Map(m => m.DUE45_AMT).ConvertUsing(row =>
            {
                decimal? dl = null;
                string dUE45_AMT = row.GetField<string>("031-045");
                if (!string.IsNullOrEmpty(dUE45_AMT))
                {
                    dUE45_AMT = dUE45_AMT.Replace(",", "");
                    if (!string.IsNullOrWhiteSpace(dUE45_AMT))
                    {
                        dl = Convert.ToDecimal(dUE45_AMT);
                    }
                }
                return dl;
            });
            Map(m => m.Due60Amt).ConvertUsing(row =>
            {
                decimal? dl = null;
                string due60Amt = row.GetField<string>("046-060");
                if (!string.IsNullOrEmpty(due60Amt))
                {
                    due60Amt = due60Amt.Replace(",", "");
                    if (!string.IsNullOrWhiteSpace(due60Amt))
                    {
                        dl = Convert.ToDecimal(due60Amt);
                    }
                }
                return dl;
            });
            Map(m => m.Due90Amt).ConvertUsing(row =>
            {
                decimal? dl = null;
                string due90Amt = row.GetField<string>("061-090");
                if (!string.IsNullOrEmpty(due90Amt))
                {
                    due90Amt = due90Amt.Replace(",", "");
                    if (!string.IsNullOrWhiteSpace(due90Amt))
                    {
                        dl = Convert.ToDecimal(due90Amt);
                    }
                }
                return dl;
            });
            Map(m => m.Due120Amt).ConvertUsing(row =>
            {
                decimal? dl = null;
                string due120Amt = row.GetField<string>("091-120");
                if (!string.IsNullOrEmpty(due120Amt))
                {
                    due120Amt = due120Amt.Replace(",", "");
                    if (!string.IsNullOrWhiteSpace(due120Amt))
                    {
                        dl = Convert.ToDecimal(due120Amt);
                    }
                }
                return dl;
            });
            Map(m => m.Due180Amt).ConvertUsing(row =>
            {
                decimal? dl = null;
                string due180Amt = row.GetField<string>("121-180");
                if (!string.IsNullOrEmpty(due180Amt))
                {
                    due180Amt = due180Amt.Replace(",", "");
                    if (!string.IsNullOrWhiteSpace(due180Amt))
                    {
                        dl = Convert.ToDecimal(due180Amt);
                    }
                }
                return dl;
            });
            Map(m => m.Due270Amt).ConvertUsing(row =>
            {
                decimal? dl = null;
                string due270Amt = row.GetField<string>("181-270");
                if (!string.IsNullOrEmpty(due270Amt))
                {
                    due270Amt = due270Amt.Replace(",", "");
                    if (!string.IsNullOrWhiteSpace(due270Amt))
                    {
                        dl = Convert.ToDecimal(due270Amt);
                    }
                }
                return dl;
            });
            Map(m => m.Due360Amt).ConvertUsing(row =>
            {
                decimal? dl = null;
                string due360Amt = row.GetField<string>("271-360");
                if (!string.IsNullOrEmpty(due360Amt))
                {
                    due360Amt = due360Amt.Replace(",", "");
                    if (!string.IsNullOrWhiteSpace(due360Amt))
                    {
                        dl = Convert.ToDecimal(due360Amt);
                    }
                }
                return dl;
            });
            Map(m => m.DueOver360Amt).ConvertUsing(row =>
            {
                decimal? dl = null;
                string dueOver360Amt = row.GetField<string>("360+");
                if (!string.IsNullOrEmpty(dueOver360Amt))
                {
                    dueOver360Amt = dueOver360Amt.Replace(",", "");
                    if (!string.IsNullOrWhiteSpace(dueOver360Amt))
                    {
                        dl = Convert.ToDecimal(dueOver360Amt);
                    }
                }
                return dl;
            });
            Map(m => m.TotalFutureDue).ConvertUsing(row =>
            {
                decimal? dl = null;
                string totalFeatureDue = row.GetField<string>("total_future_due");
                if (!string.IsNullOrEmpty(totalFeatureDue))
                {
                    totalFeatureDue = totalFeatureDue.Replace(",", "");
                    if (!string.IsNullOrWhiteSpace(totalFeatureDue))
                    {
                        dl = Convert.ToDecimal(totalFeatureDue);
                    }
                }
                return dl;
            });
        }
    }

    /// <summary>
    /// csv-invoice map 
    /// </summary>
    public sealed class InvoiceMap : ClassMap<InvoiceAgingStaging>
    {
        public InvoiceMap()
        {
            string[] formats = { "MM/dd/yy hh:mm tt", "MM/dd/yy HH:mm tt", "yyyy/MM/dd", "MM-dd-yyyy", "yyyy-MM-dd" };
            Map(m => m.CustomerName).Name("CUSTOMER NAME");
            Map(m => m.CustomerNum).Name("ACCNT NUMBER");
            Map(m => m.SiteUseId).ConvertUsing(row =>
            {
                string siteUseId = row.GetField<string>("SITE USE ID");
                siteUseId = siteUseId.Replace(",","").Split('.')[0];
                return Convert.ToInt32(siteUseId).ToString();
            });
            Map(m => m.SellingLocationCode).Name("SELLING LOCATION CODE");
            Map(m => m.Class).Name("CLASS");
            Map(m => m.InvoiceNum).Name("TRX NUM");
            Map(m => m.InvoiceDate).ConvertUsing(row =>
            {
                DateTime? dt = null;
                string invoiceDate = row.GetField<string>("TRX DATE");
                if (!string.IsNullOrWhiteSpace(invoiceDate))
                {
                    DateTime tmp;
                    if (DateTime.TryParseExact(invoiceDate, formats, System.Globalization.CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out tmp))
                    {
                        dt = tmp;
                    }
                }
                return dt;
            });
            Map(m => m.DueDate).ConvertUsing(row =>
            {
                DateTime? dt = null;
                string dueDate = row.GetField<string>("DUE DATE");
                if (!string.IsNullOrWhiteSpace(dueDate))
                {
                    DateTime tmp;
                    if (DateTime.TryParseExact(dueDate, formats, System.Globalization.CultureInfo.CurrentCulture, DateTimeStyles.None, out tmp))
                    {
                        dt = tmp;
                    }
                }
                return dt;
            });
            Map(m => m.CreditTrem).Name("PAYMENT TERM NAME");
            Map(m => m.CreditLmt).ConvertUsing(row =>
            {
                decimal? dl = null;
                string creditLmt = row.GetField<string>("OVER CREDIT LMT");
                creditLmt = creditLmt.Replace(",", "");
                if (!string.IsNullOrWhiteSpace(creditLmt))
                {
                    dl = Convert.ToDecimal(creditLmt);
                }
                return dl;
            });
            Map(m => m.CreditLmtAcct).ConvertUsing(row =>
            {
                decimal? dl = null;
                string creditLmtAcct = row.GetField<string>("OVER CREDIT LMT ACCT");
                creditLmtAcct = creditLmtAcct.Replace(",", "");
                if (!string.IsNullOrWhiteSpace(creditLmtAcct))
                {
                    dl = Convert.ToDecimal(creditLmtAcct);
                }
                return dl;
            });
            Map(m => m.FuncCurrCode).Name("FUNC CURR CODE");
            Map(m => m.Currency).Name("INV CURR CODE");
            Map(m => m.Sales).Name("SALES NAME");
            Map(m => m.DaysLateSys).ConvertUsing(row =>
            {
                int? num = null;
                string daysLateSys = row.GetField<string>("DUE DAYS");
                daysLateSys = daysLateSys.Replace(",", "").Split('.')[0];
                if (!string.IsNullOrEmpty(daysLateSys))
                {
                    num = (int)Convert.ToDecimal(daysLateSys);
                }
                return num;
            });
            Map(m => m.OriginalAmt).ConvertUsing(row =>
            {
                decimal? dl = null;
                string originalAmt = row.GetField<string>("AMT REMAINING");
                originalAmt = originalAmt.Replace(",", "");
                if (!string.IsNullOrWhiteSpace(originalAmt))
                {
                    dl = Convert.ToDecimal(originalAmt);
                }
                return dl;
            });
            Map(m => m.WoVat_AMT).ConvertUsing(row =>
            {
                decimal? dl = null;
                string woVat_AMT = row.GetField<string>("AMOUNT WO VAT");
                woVat_AMT = woVat_AMT.Replace(",", "");
                if (!string.IsNullOrWhiteSpace(woVat_AMT))
                {
                    dl = Convert.ToDecimal(woVat_AMT);
                }
                return dl;
            });
            Map(m => m.AgingBucket).Name("AGING BUCKET");
            Map(m => m.CreditTremDescription).Name("PAYMENT TERM DESC");
            Map(m => m.SellingLocationCode2).Name("SELLING LOCATION CODE2");
            Map(m => m.Ebname).Name("EBNAME");
            Map(m => m.Customertype).Name("CUSTOMERTYPE");
            Map(m => m.Fsr).Name("FSR");
            Map(m => m.Cmpinv).Name("CMPINV");
            Map(m => m.SoNum).Name("SALES ORDER");
            Map(m => m.PoNum).Name("CPO");
            Map(m => m.FsrNameHist).Name("FSR NAME HIST");
            Map(m => m.LsrNameHist).Name("CSR");
            Map(m => m.Eb).Name("EB");
            Map(m => m.RemainingAmtTran).ConvertUsing(row =>
            {
                decimal? dl = null;
                string RemainingAmtTran = row.GetField<string>("AMT REMAINING TRAN");
                RemainingAmtTran = RemainingAmtTran.Replace(",", "");
                if (!string.IsNullOrWhiteSpace(RemainingAmtTran))
                {
                    dl = Convert.ToDecimal(RemainingAmtTran);
                }
                return dl;
            });
        }
    }

    /// <summary>
    /// csv-invoice-detail map 
    /// </summary>
    public sealed class InvoiceDetailMap : ClassMap<T_Invoice_Detail_Staging>
    {
        public InvoiceDetailMap()
        {
            Map(m => m.InvoiceDate).ConvertUsing(row =>
            {
                string[] formats = { "MM/dd/yyyy", "yyyy/MM/dd", "MM-dd-yyyy", "yyyy-MM-dd" };
                DateTime? dt = null;
                string invoiceDate = row.GetField<string>("Invoice Date");
                if (!string.IsNullOrWhiteSpace(invoiceDate))
                {
                    DateTime tmp;
                    if (DateTime.TryParseExact(invoiceDate, formats, System.Globalization.CultureInfo.CurrentCulture, DateTimeStyles.None, out tmp))
                    {
                        dt = tmp;
                    }
                }
                return dt;
            });
            Map(m => m.CustomerPO).Name("Customer PO #");
            Map(m => m.CustomerPartNumber).Name("Customer Part Number");
            Map(m => m.Manufacturer).Name("Manufacturer");
            Map(m => m.PartNumber).Name("Part Number");
            Map(m => m.InvoiceNumber).Name("Invoice Number");
            Map(m => m.InvoiceLineNumber).ConvertUsing(row =>
            {
                int num = 0;
                string invoiceLineNumber = row.GetField<string>("Invoice Line Number");
                if (!string.IsNullOrWhiteSpace(invoiceLineNumber))
                {
                    num = Convert.ToInt32(invoiceLineNumber);
                }
                return num;
            });
            Map(m => m.TransactionCurrencyCode).Name("Transaction Currency Code");
            Map(m => m.InvoiceQty).ConvertUsing(row =>
            {
                decimal? dl = null;
                string invoiceQty = row.GetField<string>("Invoice Qty");
                invoiceQty = invoiceQty.Replace(",", "");
                if (!string.IsNullOrWhiteSpace(invoiceQty))
                {
                    dl = Convert.ToDecimal(invoiceQty);
                }
                return dl;
            });
            Map(m => m.UnitResales).ConvertUsing(row =>
            {
                decimal? dl = null;
                string unitResales = row.GetField<string>("Unit Resales");
                unitResales = unitResales.Replace(",", "");
                if (!string.IsNullOrWhiteSpace(unitResales))
                {
                    dl = Convert.ToDecimal(unitResales);
                }
                return dl;
            });
            Map(m => m.NSB).ConvertUsing(row =>
            {
                decimal? dl = null;
                string nSB = row.GetField<string>("NSB");
                nSB = nSB.Replace(",", "");
                if (!string.IsNullOrWhiteSpace(nSB))
                {
                    dl = Convert.ToDecimal(nSB);
                }
                return dl;
            });
            Map(m => m.Cost).ConvertUsing(row =>
            {
                decimal? dl = null;
                string cost = row.GetField<string>("Cost");
                cost = cost.Replace(",", "");
                if (!string.IsNullOrWhiteSpace(cost))
                {
                    dl = Convert.ToDecimal(cost);
                }
                return dl;
            });
            Map(m => m.GPD).ConvertUsing(row =>
            {
                decimal? dl = null;
                string gpd = row.GetField<string>("GPD");
                gpd = gpd.Replace(",", "");
                if (!string.IsNullOrWhiteSpace(gpd))
                {
                    dl = Convert.ToDecimal(gpd);
                }
                return dl;
            });
            Map(m => m.Cost_Original).ConvertUsing(row =>
            {
                decimal? dl = null;
                string co = row.GetField<string>("Cost_Original");
                co = co.Replace(",", "");
                if (!string.IsNullOrWhiteSpace(co))
                {
                    dl = Convert.ToDecimal(co);
                }
                return dl;
            });
        }
    }

    /// <summary>
    /// csv-invoice map 
    /// </summary>
    public sealed class SAPInvoiceMap : ClassMap<InvoiceAgingStaging>
    {
        public SAPInvoiceMap()
        {
            string[] formats = { "MM/dd/yy hh:mm tt", "MM/dd/yy HH:mm tt", "yyyy/MM/dd", "MM-dd-yyyy", "yyyy-MM-dd" };
            Map(m => m.LegalEntity).Index(0);
            Map(m => m.CustomerNum).Index(1);
            Map(m => m.InvoiceDate).ConvertUsing(row =>
            {
                DateTime? dt = null;
                string invoiceDate = row.GetField<string>(2);
                if (!string.IsNullOrWhiteSpace(invoiceDate))
                {
                    DateTime tmp;
                    if (DateTime.TryParseExact(invoiceDate, formats, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out tmp))
                    {
                        dt = tmp;
                    }
                }
                return dt;
            });
            Map(m => m.InvoiceNum).Index(4);
            Map(m => m.DueDate).ConvertUsing(row =>
            {
                DateTime? dt = null;
                string dueDate = row.GetField<string>(5);
                if (!string.IsNullOrWhiteSpace(dueDate))
                {
                    DateTime tmp;
                    if (DateTime.TryParseExact(dueDate, formats, CultureInfo.CurrentCulture, DateTimeStyles.None, out tmp))
                    {
                        dt = tmp;
                    }
                }
                return dt;
            });
            Map(m => m.DaysLateSys).ConvertUsing(row =>
            {
                int? num = null;
                string daysLateSys = row.GetField<string>(6);
                daysLateSys = daysLateSys.Replace(",", "");
                if (!string.IsNullOrEmpty(daysLateSys))
                {
                    num = Convert.ToInt32(daysLateSys);
                }
                return num;
            });
            Map(m => m.SiteUseId).ConvertUsing(row =>
            {
                string siteUseId = string.Empty;
                siteUseId = row.GetField<string>(7);
                siteUseId = siteUseId.Replace(",", "");
                return siteUseId;
            });
            Map(m => m.PoNum).Index(8);
            Map(m => m.Currency).Index(9);
            Map(m => m.OriginalAmt).ConvertUsing(row =>
            {
                decimal? dl = null;
                string originalAmt = row.GetField<string>(10);
                originalAmt = originalAmt.Replace(",", "");
                if (!string.IsNullOrWhiteSpace(originalAmt))
                {
                    dl = Convert.ToDecimal(originalAmt);
                }
                return dl;
            });
            Map(m => m.CustomerName).ConvertUsing(row =>
            {
                string name = string.Format("{0}_{1}_{2}", row.GetField<string>(0), row.GetField<string>(1), row.GetField<string>(7));
                return name;
            });
            Map(m => m.Class).Constant("INV");
            Map(m => m.Deal).Constant("Arrow");
        }
    }
}
