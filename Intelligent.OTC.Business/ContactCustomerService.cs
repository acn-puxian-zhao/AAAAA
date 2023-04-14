using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using System.Data.Entity;
using Intelligent.OTC.Business;
using Intelligent.OTC.Domain;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using System.Web;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using Intelligent.OTC.Common.Exceptions;
using System.Transactions;

namespace Intelligent.OTC.Business
{
    public class ContactCustomerService : IContactCustomerService
    {
        public OTCRepository CommonRep { get; set; }

        /// <summary>
        /// get all collector_alert
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public IEnumerable<ContactCustomerDto> GetContactCustomer(string invoiceState = "", string invoiceTrackState = "", string legalEntity = "", string invoiceNum = "", string soNum = "", string poNum = "", string invoiceMemo = "")
        {
            string deal = AppContext.Current.User.Deal.ToString();
            string eid = AppContext.Current.User.EID.ToString();
            IEnumerable<ContactCustomerDto> r = new List<ContactCustomerDto>();
            return r;
        }

        private DateTime dataConvertToDT(string strData)
        {
            DateTime dt = new DateTime();
            if (!string.IsNullOrEmpty(strData.Trim()))
            {
                return Convert.ToDateTime(strData);
            }

            return dt;
        }

        /// <summary>
        /// Get Contact History Datas
        /// </summary>
        /// <param name="strCusNum"></param>
        /// <returns></returns>
        public IEnumerable<ContactHistory> GetContactList(string strCusNum)
        {
            List<ContactHistory> list = new List<ContactHistory>();
            using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions()
                    {
                        IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                    }))
            {
                list = CommonRep.GetQueryable<ContactHistory>().Where(o => o.CustomerNum == strCusNum && o.Deal == AppContext.Current.User.Deal).Select(o => o).ToList();
                scope.Complete();
            }

            return list;
         }

        /// <summary>
        /// Get Dispute Datas
        /// </summary>
        /// <param name="strCusNum"></param>
        /// <returns></returns>
        public IEnumerable<Dispute> GetDisputeList(string strCusNum)
        {
            List<Dispute> list = new List<Dispute>();
            using (var scope = new TransactionScope(
                       TransactionScopeOption.Required,
                       new TransactionOptions()
                       {
                           IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                       }))
            {
                list = CommonRep.GetQueryable<Dispute>().Where(o => o.CustomerNum == strCusNum && o.Deal == AppContext.Current.User.Deal).Select(o => o).ToList();
                scope.Complete();
            }
            return list;
        }

        //added by zhangYu contactHistory call detail
        public Call GetCallInfoByContactId(string contactId)
        {
            Call callIns = new Call();
            callIns = CommonRep.GetQueryable<Call>().Where(ca => ca.ContactId == contactId).FirstOrDefault();
            //add by pxc 
            callIns.contacterId = CommonRep.GetDbSet<ContactHistory>().Where(o => o.ContactId == contactId).FirstOrDefault().ContacterId;
            return callIns;
        }

        //added by zhangYu  call create/ write callInfo to T_call
        public void WriteCallLog(Call callInstance)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
            {
                string strGUID = "";
                Call ca = new Call();
                ContactHistory con = new ContactHistory();
                List<ContactHistory> cons = new List<ContactHistory>();
                InvoiceLog invLog = new InvoiceLog();
                List<InvoiceLog> listInvLog = new List<InvoiceLog>();
                //GET GUID
                strGUID = System.Guid.NewGuid().ToString();

                //T_CALL
                ca.BeginTime = callInstance.BeginTime;
                ca.EndTime = callInstance.EndTime;
                ca.Comments = callInstance.Comments;
                ca.ContactId = strGUID;
                string customerNum = "";
                //T_CONTACT_HISTORY
                foreach (var cust in callInstance.customerNum.Split(','))
                {
                    con = new ContactHistory();
                    con.Deal = AppContext.Current.User.Deal;
                    con.CustomerNum = cust;
                    if (!string.IsNullOrWhiteSpace(cust))
                    {
                        customerNum = cust;
                    }
                    con.ContactType = "Call";
                    con.ContactId = strGUID;
                    con.CollectorId = AppContext.Current.User.EID;
                    //contacter
                    con.ContacterId = callInstance.contacterId;
                    con.Comments = callInstance.Comments;
                    con.LegalEntity = callInstance.LegalEntity;
                    con.LastUpdatePerson = AppContext.Current.User.EID;
                    con.LastUpdateTime = DateTime.Now;
                    con.ContactDate = AppContext.Current.User.Now;
                    con.SiteUseId = callInstance.siteuseId;
                    cons.Add(con);
                }

                string nInvoiceNums = string.Empty;
                //T_INVOICElOG
                if (callInstance.invoiceIds.Length > 0)
                {
                    bool isFollowup = false;
                    List<string> nFollowupStatusList = new List<string>() { "001", "002", "003", "004", "005", "006", "015", "010" };
                    List<InvoiceAging> listAging = new List<InvoiceAging>();
                    listAging = CommonRep.GetDbSet<InvoiceAging>().Where(q => callInstance.invoiceIds.Contains(q.Id)).ToList();
                    List<InvoiceAging> listAgingUpdate = new List<InvoiceAging>();
                    for (int i = 0; i < listAging.Count; i++)
                    {
                        int id = listAging[i].Id;
                        InvoiceAging invAging = listAging[i];

                        if (nFollowupStatusList.Contains(invAging.TrackStates))
                            isFollowup = true;

                        invLog = new InvoiceLog();
                        nInvoiceNums += invAging.InvoiceNum + ",";
                        invLog.ProofId = strGUID;
                        invLog.Deal = AppContext.Current.User.Deal;
                        invLog.InvoiceId = invAging.InvoiceNum;
                        invLog.NewStatus = invAging.States;
                        invLog.OldStatus = invAging.States;
                        invLog.CustomerNum = invAging.CustomerNum;
                        invLog.ContactPerson = callInstance.contacterId;
                        invLog.LogPerson = AppContext.Current.User.EID;
                        invLog.LogType = "2";//2:call
                        if (string.IsNullOrEmpty(callInstance.logAction))
                        { invLog.LogAction = "CONTACT"; }
                        else
                        {
                            invLog.LogAction = callInstance.logAction;
                        }
                        invLog.LogDate = AppContext.Current.User.Now;
                        invLog.SiteUseId = callInstance.siteuseId;
                        listInvLog.Add(invLog);
                        invAging.CallId = strGUID;
                        listAgingUpdate.Add(invAging);
                    }

                    CommonRep.BulkInsert(listInvLog);
                    CommonRep.BulkUpdate(listAgingUpdate);

                    // Followup 情况下，打一次电话代表所有INV都打过电话，全部绑定
                    if (isFollowup == true)
                    {
                        List<InvoiceAging> allFollowupInvList = CommonRep.GetQueryable<InvoiceAging>().Where(x => x.CustomerNum == callInstance.customerNum && x.SiteUseId == callInstance.siteuseId && nFollowupStatusList.Contains(x.TrackStates)).ToList();
                        if (allFollowupInvList != null && allFollowupInvList.Count > 0)
                            allFollowupInvList.ForEach(x => x.CallId = strGUID);

                        CommonRep.BulkUpdate(allFollowupInvList);
                    }
                    
                }
                
                CommonRep.Add(ca);//T_call
                CommonRep.BulkInsert(cons);
                CommonRep.Commit();

                CommonRep.GetDBContext().Database.ExecuteSqlCommand(string.Format("p_UpdateTaskStatus '','','','{0}','{1}'", nInvoiceNums, DateTime.Now));

                scope.Complete();

            }

        }//WriteCallLog


        public void UpdateCallLog(Call callInstance)
        {
            Call upCal = (from cal in CommonRep.GetQueryable<Call>()
                          where cal.ContactId == callInstance.ContactId
                          select cal).FirstOrDefault<Call>();

            ContactHistory conHis = (from ch in CommonRep.GetQueryable<ContactHistory>()
                                     where ch.ContactId == callInstance.ContactId
                                     select ch).FirstOrDefault<ContactHistory>();
            //T_CALL
            upCal.BeginTime = callInstance.BeginTime;
            upCal.EndTime = callInstance.EndTime;
            upCal.Comments = callInstance.Comments;


            //T_CONTACT_HISTORY
            conHis.ContacterId = callInstance.contacterId;
            conHis.Comments = callInstance.Comments;
            conHis.LastUpdatePerson = AppContext.Current.User.EID;
            conHis.LastUpdateTime = AppContext.Current.User.Now;

            CommonRep.Commit();
        }

        /// <summary>
        /// Each Collector can expoert all of its own Invoice Account
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage ExpoertInvoiceList()
        {
            List<Customer.ExpCustomerDto> lstCustomer = new List<Customer.ExpCustomerDto>();
            List<InvoiceAging> invoiceList = new List<InvoiceAging>();
            InvoiceAging invoice = new InvoiceAging();
            string templateFile = "";
            string fileName = "";
            string tmpFile = "";
            string closedStatus = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Closed);
            string cancellStatus = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Cancelled);

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportInvoiceTemplate"].ToString());
                fileName = AppContext.Current.User.EID + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                //Get all collector responsible customers
                var customerSql = string.Format(@"
                    SELECT DEAL AS Deal, CUSTOMER_NUM AS CustomerNum, CUSTOMER_NAME AS CustomerName, COLLECTOR AS Collector 
                    FROM (SELECT A.DEAL, A.CUSTOMER_NUM, A.CUSTOMER_NAME, ISNULL(B.COLLECTOR, A.COLLECTOR) AS COLLECTOR 
                    FROM T_CUSTOMER AS A LEFT JOIN WITH (NOLOCK)
                         T_CUSTOMER_GROUP_CFG AS B WITH (NOLOCK) ON A.DEAL = B.DEAL AND A.BILL_GROUP_CODE = B.BILL_GROUP_CODE
                    WHERE A.REMOVE_FLG = '1' and A.EXCLUDE_FLG = '0') C
                    WHERE COLLECTOR = '{0}' and DEAL = '{1}'
                ", AppContext.Current.User.EID, AppContext.Current.User.Deal);
                lstCustomer = CommonRep.GetDBContext().Database.SqlQuery<Customer.ExpCustomerDto>(customerSql)
                                .OrderBy(o => o.Deal).ThenBy(o => o.CustomerNum).ToList();

                //Get invoice list
                foreach (var cus in lstCustomer)
                {
                    List<InvoiceAging> invList = new List<InvoiceAging>();
                    using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions()
                    {
                        IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                    }))
                    {
                        invList = CommonRep.GetQueryable<InvoiceAging>()
                                        .Where(o => o.Deal == cus.Deal && o.CustomerNum == cus.CustomerNum
                                            && o.States != closedStatus
                                            && o.States != cancellStatus)
                                            .OrderBy(o => o.LegalEntity).ToList();
                        scope.Complete();
                    }
                    foreach (var inv in invList)
                    {
                        invoice = new InvoiceAging();
                        invoice.Id = inv.Id;
                        invoice.CustomerNum = inv.CustomerNum;
                        invoice.CustomerName = inv.CustomerName;
                        invoice.InvoiceNum = inv.InvoiceNum;
                        invoice.LegalEntity = inv.LegalEntity;
                        invoice.InvoiceDate = inv.InvoiceDate;
                        invoice.CreditTrem = inv.CreditTrem;
                        invoice.DueDate = inv.DueDate;
                        invoice.PoNum = inv.PoNum;
                        invoice.SoNum = inv.SoNum;
                        invoice.MstCustomer = inv.MstCustomer;
                        invoice.Currency = inv.Currency;
                        invoice.OriginalAmt = inv.OriginalAmt;
                        invoice.BalanceAmt = inv.BalanceAmt;
                        invoice.DaysLateSys = int.Parse(new TimeSpan(AppContext.Current.User.Now.Ticks).Subtract(new TimeSpan(Convert.ToDateTime(inv.DueDate).Ticks)).Duration().Days.ToString());
                        invoice.TrackStates = !String.IsNullOrEmpty(inv.TrackStates) ? Helper.CodeToEnum<TrackStatus>(inv.TrackStates).ToString().Replace("_", " ") : "";
                        invoice.States = !String.IsNullOrEmpty(inv.States) ? Helper.CodeToEnum<InvoiceStatus>(inv.States).ToString() : "";
                        invoice.PtpDate = inv.PtpDate;
                        invoice.Class = inv.Class;
                        invoice.Comments = inv.Comments;
                        invoiceList.Add(invoice);
                    }
                }

                this.setData(templateFile, tmpFile, invoiceList);

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

        #region Set Invoice Report Datas
        private void setData(string templateFileName, string tmpFile, List<InvoiceAging> lstDatas)
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
                    helper.SetData(rowNo, 0, lst.CustomerNum);
                    helper.SetData(rowNo, 1, lst.CustomerName);
                    helper.SetData(rowNo, 2, lst.LegalEntity);
                    helper.SetData(rowNo, 3, lst.InvoiceNum);
                    helper.SetData(rowNo, 4, string.IsNullOrEmpty(lst.InvoiceDate.ToString()) ? "" : lst.InvoiceDate.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 5, lst.CreditTrem);
                    helper.SetData(rowNo, 6, string.IsNullOrEmpty(lst.DueDate.ToString()) ? "" : lst.DueDate.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 7, lst.PoNum);
                    helper.SetData(rowNo, 8, lst.SoNum);
                    helper.SetData(rowNo, 9, lst.MstCustomer);
                    helper.SetData(rowNo, 10, lst.Currency);
                    helper.SetData(rowNo, 11, lst.OriginalAmt);
                    helper.SetData(rowNo, 12, lst.BalanceAmt);
                    helper.SetData(rowNo, 13, lst.DaysLateSys);
                    helper.SetData(rowNo, 14, lst.TrackStates);
                    helper.SetData(rowNo, 15, lst.States);
                    helper.SetData(rowNo, 16, string.IsNullOrEmpty(lst.PtpDate.ToString()) ? "" : lst.PtpDate.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 17, lst.Class);
                    helper.SetData(rowNo, 18, lst.Comments);

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
    }
}
