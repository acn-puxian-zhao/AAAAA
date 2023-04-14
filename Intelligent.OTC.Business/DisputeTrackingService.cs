using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Domain.Repositories;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.Dtos;
using System.Transactions;
using System.Data.SqlClient;
using System.Data;

namespace Intelligent.OTC.Business
{
    public class DisputeTrackingService
    {
        public OTCRepository CommonRep { get; set; }
        public XcceleratorService XccService { get; set; }

        /// <summary>
        /// get dispute datas
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DisputeTrackingView> GetDisputeDatas(string InvoiceNum)
        {
            string deal = AppContext.Current.User.Deal.ToString();

            List<SysUser> listUser = new List<SysUser>();
            listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
            string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
            string collecotrList = "," + string.Join(",", userGroup.ToArray()) + ",";

            if (string.IsNullOrEmpty(InvoiceNum) || InvoiceNum.Equals("undefined"))
            {
                if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
                {
                    return CommonRep.GetQueryable<DisputeTrackingView>()
                        .Where(m => m.Deal == deal );
                }
                else
                {
                    return CommonRep.GetQueryable<DisputeTrackingView>()
                       .Where(m => m.Deal == deal && collecotrList.Contains("," + m.Collector + ","));
                }
            }
            else
            {
                string ids = "";
                List<string> listDispute = new List<string>();
                var disputeInvoice = CommonRep.GetQueryable<DisputeInvoice>().Where(m=>m.InvoiceId == InvoiceNum).ToList().DefaultIfEmpty();
                if (disputeInvoice != null)
                { 
                    foreach (var item in disputeInvoice )
                    {
                        if (item == null) { continue; }
                        listDispute.Add(item.DisputeId.ToString());
                    }
                    string[] idGroup = listDispute.Distinct().ToArray();
                    ids = string.Join(",", idGroup.ToArray());
                }
                if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
                {
                    return CommonRep.GetQueryable<DisputeTrackingView>()
                        .Where(m => m.Deal == deal && ids.Contains(m.Id.ToString()));
                }
                else
                {
                    return CommonRep.GetQueryable<DisputeTrackingView>()
                       .Where(m => m.Deal == deal && ids.Contains(m.Id.ToString()) && collecotrList.Contains("," + m.Collector + ","));
                }
            }
        }

        /// <summary>
        /// Get IssueReason,Status,CreateDate,Comments
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<string> GetDisputeById(int id)
        {
            List<string> lstDispute = new List<string>();

            try
            {
                string deal = AppContext.Current.User.Deal.ToString();
                List<SysUser> listUser = new List<SysUser>();
                listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
                string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
                string collecotrList = "," + string.Join(",", userGroup.ToArray()) + ",";
                var dispItem = CommonRep.GetQueryable<DisputeTrackingView>()
                       .Where(m => m.Id == id && m.Deal == deal && collecotrList.Contains("," + m.Collector + ","))
                       .ToList();
                if (dispItem == null || dispItem.Count == 0) {
                    return lstDispute;
                }
                var dis = (from disp in CommonRep.GetDbSet<Dispute>()
                            join cus in CommonRep.GetQueryable<CustomerAging>()
                            on new { disp.Deal, disp.CustomerNum, disp.SiteUseId } equals new { cus.Deal, cus.CustomerNum, cus.SiteUseId }
                            into tmp1
                            from tp1 in tmp1.DefaultIfEmpty()
                            join std in CommonRep.GetQueryable<SysTypeDetail>()
                            on new { IssueReason = disp.IssueReason, TypeCode = "025" } equals new { IssueReason = std.DetailValue, TypeCode = std.TypeCode }
                            into tmp
                            from xxx in tmp.DefaultIfEmpty()
                            join std_status in CommonRep.GetQueryable<SysTypeDetail>()
                            on new { DisputeStatus = disp.Status, TypeCode = "026" } equals new { DisputeStatus = std_status.DetailValue, TypeCode = std_status.TypeCode }
                            into tmp_status
                            from xxxStatus in tmp_status.DefaultIfEmpty()
                            join std_ActionOwnerDepartment in CommonRep.GetQueryable<SysTypeDetail>()
                            on new { ActionOwnerDepartment = disp.ActionOwnerDepartmentCode, TypeCode = "038" } equals new { ActionOwnerDepartment = std_ActionOwnerDepartment.DetailValue, TypeCode = std_ActionOwnerDepartment.TypeCode }
                            into tmp_ActionOwnerDepartment
                            from xxxActionOwnerDepartment in tmp_ActionOwnerDepartment.DefaultIfEmpty()
                            where disp.Id == id
                            select new
                            {
                                Id = disp.Id,
                                Deal = disp.Deal,
                                ContactId = disp.ContactId,
                                Eid = disp.Eid,
                                CustomerNum = disp.CustomerNum,
                                IssueReason = disp.IssueReason,
                                CreateDate = disp.CreateDate,
                                CloseDate = disp.CloseDate,
                                Status = disp.Status,
                                CreatePerson = disp.CreatePerson,
                                Comments = disp.Comments,
                                SiteUseId = disp.SiteUseId,
                                STATUS_DATE = disp.STATUS_DATE,
                                CustomerName = tp1.CustomerName,
                                LegalEntity = tp1.LegalEntity,
                                DisputeReson = xxx.DetailName,
                                StatusName = xxxStatus.DetailName,
                                ActionOwnerDepartmentCode = disp.ActionOwnerDepartmentCode,
                                ActionOwnerDepartmentName = xxxActionOwnerDepartment.DetailName,
                                DisputeResonCode = disp.IssueReason,
                            }
                                        ).FirstOrDefault();
                lstDispute.Add(Helper.CodeToEnum<DisputeReason>(dis.IssueReason).ToString().Replace("_", " "));
                lstDispute.Add(Helper.CodeToEnum<DisputeStatus>(dis.Status).ToString().Replace("_", " "));
                lstDispute.Add(dis.CreateDate.ToString("yyyy-MM-dd hh:mm:ss"));
                lstDispute.Add(dis.Comments);
                lstDispute.Add(dis.CustomerNum);
                lstDispute.Add(dis.Status);
                lstDispute.Add(dis.SiteUseId);
                lstDispute.Add(dis.LegalEntity);
                lstDispute.Add(dis.DisputeReson);
                lstDispute.Add(dis.CustomerName);
                lstDispute.Add(dis.StatusName);
                lstDispute.Add(dis.ActionOwnerDepartmentCode);
                lstDispute.Add(dis.ActionOwnerDepartmentName);
                lstDispute.Add(dis.DisputeResonCode);
                return lstDispute;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Get Dispute Tracking Datas
        /// </summary>
        /// <param name="disId"></param>
        /// <returns></returns>
        public IEnumerable<DisputeTracking> GetDisputeInvoiceDatas(int disId)
        {
            string cusNum = "";
            List<DisputeTracking> disTracking = new List<DisputeTracking>();

            try
            {
                if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") < 0)
                { 
                    string deal = AppContext.Current.User.Deal.ToString();
                    List<SysUser> listUser = new List<SysUser>();
                    listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
                    string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
                    string collecotrList = "," + string.Join(",", userGroup.ToArray()) + ",";
                    var dispItem = CommonRep.GetQueryable<DisputeTrackingView>()
                           .Where(m => m.Id == disId && m.Deal == deal && collecotrList.Contains("," + m.Collector + ","))
                           .ToList();
                    if (dispItem == null || dispItem.Count == 0)
                    {
                        return disTracking.AsQueryable();
                    }
                }
                //get dispute invoice
                List<DisputeInvoice> disInv = CommonRep.GetQueryable<DisputeInvoice>()
                                                .Where(o => o.DisputeId == disId).ToList();

                cusNum = CommonRep.GetQueryable<Dispute>()
                            .Where(o => o.Id == disId).Select(o => o.CustomerNum).FirstOrDefault();

                //invoice aging
                List<InvoiceAging> invList = CommonRep.GetQueryable<InvoiceAging>()
                                                .Where(o => o.Deal == AppContext.Current.User.Deal
                                                    && o.CustomerNum == cusNum).ToList();

                Customer customer = CommonRep.GetQueryable<Customer>()
                                                .Where(o => o.Deal == AppContext.Current.User.Deal
                                                    && o.CustomerNum == cusNum).FirstOrDefault();

                List<string> invNumList = invList.Select(x => x.InvoiceNum).ToList();
                List<int> invIdList = invList.Select(x => x.Id).ToList();
                var invVatList = CommonRep.GetDbSet<T_INVOICE_VAT>().Where(x => x.LineNumber == 1 && invNumList.Contains(x.Trx_Number)).Select(x => new { Trx_Number = x.Trx_Number, VATInvoiceDate = x.VATInvoiceDate }).ToList();

                // ptp
                var ptpQ = from ptp in CommonRep.GetQueryable<T_PTPPayment_Invoice>()
                            join pp in CommonRep.GetQueryable<T_PTPPayment>()
                            on ptp.PTPPaymentId equals pp.Id
                            where pp.PTPPaymentType == "PTP" && invIdList.Contains((int)ptp.InvoiceId)
                            group ptp by ptp.InvoiceId into g
                            select new { Key = g.Key, PTPId = g.Max(s => s.PTPPaymentId) };

                var pt = (from pp in CommonRep.GetQueryable<T_PTPPayment>()
                            join ptp in ptpQ on pp.Id equals ptp.PTPId
                            select new { ptp.Key, pp.CustomerNum, pp.SiteUseId, pp.CreateTime, pp.PromiseDate, pp.Comments }).ToList();


                foreach (var inv in disInv)
                {
                    InvoiceAging invoice = invList.Where(o => o.InvoiceNum == inv.InvoiceId).FirstOrDefault();
                    DisputeTracking distrack = new DisputeTracking();
                    distrack.InvoiceId = invoice.Id;
                    distrack.InvoiceNum = invoice.InvoiceNum;
                    distrack.InvoiceDate = invoice.InvoiceDate;
                    distrack.CreditTerm = invoice.CreditTrem;
                    distrack.DueDate = invoice.DueDate;
                    distrack.InvoiceCurrency = invoice.Currency;
                    distrack.OutstandingInvoiceAmount = invoice.BalanceAmt;
                    distrack.PtpDate = invoice.PtpDate;
                    distrack.TrackStates = !String.IsNullOrEmpty(invoice.TrackStates) ? Helper.CodeToEnum<TrackStatus>(invoice.TrackStates).ToString().Replace("_", " ") : "";
                    distrack.Comments = invoice.Comments;
                    distrack.CollectorName = customer.Collector;
                    distrack.OrderBy = invoice.OrderBy;
                    distrack.WoVat_AMT = invoice.WoVat_AMT;
                    distrack.AgingBucket = invoice.AgingBucket;
                    distrack.Eb = invoice.Eb;
                    distrack.TRACK_DATE = invoice.TRACK_DATE;

                    // Start add by albert 
                    var iVat = invVatList.Where(x => x.Trx_Number == invoice.InvoiceNum).FirstOrDefault();
                    // Start add by albert 
                    if (iVat != null)
                    {
                        distrack.VatNum = iVat.Trx_Number;
                        distrack.VatDate = iVat.VATInvoiceDate;
                    }
                    distrack.TrackDate = invoice.TRACK_DATE;

                    if (pt != null && pt.Count > 0)
                    {
                        var iPt = pt.Where(x => x.Key == invoice.Id).FirstOrDefault();
                        if (iPt != null)
                            distrack.PtpIdentifiedDate = iPt.CreateTime;
                    }

                    //End add by albert

                    distrack.DueDays = invoice.DaysLateSys;
                    disTracking.Add(distrack);
                }
                return disTracking.AsQueryable();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Get Dispute History Status Change
        /// </summary>
        /// <param name="disputeid"></param>
        /// <returns></returns>
        public IEnumerable<DisputeHis> GetDisputeStatusChange(int disputeid)
        {
            try
            {
                Dispute dis = CommonRep.GetQueryable<Dispute>()
                                    .Where(o => o.Id == disputeid).FirstOrDefault();

                List<DisputeHis> disHisList = CommonRep.GetQueryable<DisputeHis>()
                                                .Where(o => o.DisputeId == disputeid).OrderByDescending(o => o.HisDate).ToList();

                foreach (var dislist in disHisList)
                {
                    dislist.Operator = dis.CreatePerson;
                    dislist.HisType = Helper.CodeToEnum<DisputeStatus>(dislist.HisType).ToString().Replace("_", " ");
                    dislist.ISSUE_REASON = Helper.CodeToEnum<DisputeReason>(dislist.ISSUE_REASON).ToString().Replace("_", " ");
                }

                return disHisList.AsQueryable();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //save notes
        public void SaveNotes(int id, string Notes)
        {
            Dispute dis = CommonRep.GetDbSet<Dispute>()
                .Where(o => o.Id == id).FirstOrDefault();
            dis.Comments = Notes;
            CommonRep.Commit();
        }


        /// <summary>
        /// 更新发票状态
        /// </summary>
        /// <param name="status"></param>
        /// <param name="invNums"></param>
        public void UpdateInvoicesStatus(int disputeId, string status, List<int> invIds)
        {
            if (invIds == null || invIds.Count == 0)
                return;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
            {
                List<InvoiceAging> nInvList = CommonRep.GetQueryable<InvoiceAging>().Where(x => invIds.Contains(x.Id)).ToList();

                if (nInvList != null && nInvList.Count > 0)
                    nInvList.ForEach(x => x.TrackStates = status);
                if (status == "007")
                {
                    nInvList.ForEach(x =>
                    {
                        x.FinishedStatus = "0";
                        x.MailId = null;
                        x.CallId = null;
                    });
                }
                else if (status == "010" || status == "012" || status == "013")
                {
                    nInvList.ForEach(x => x.FinishedStatus = "1");
                }
                CommonRep.Commit();

                ReCalcDisputeStatus(disputeId);

                scope.Complete();
            }
        }

        /// <summary>
        /// 重新计算Dispute状态
        /// </summary>
        /// <param name="disputeid"></param>
        public void ReCalcDisputeStatus(int disputeId)
        {
            if (disputeId <= 0)
                return;

            // 2017-12-15 追加判断，如果Dispute下所有Invoice都不为下述状态，则Close Dispute 任务
            //Dispute Identified  007
            //Wait for 2nd Time Dispute contact   008
            //Wait for Dispute Responds   009
            //Wait for 2nd Time Dispute respond   011
            //Wait for Round Table    012
            List<string> nDisputeInvStatusList = new List<string>() { "007", "008", "009", "011", "012" };
            Dispute dispute = CommonRep.GetQueryable<Dispute>().Where(x => x.Id == disputeId).FirstOrDefault();
            if (dispute != null)
            {
                List<string> nInvNums = CommonRep.GetQueryable<DisputeInvoice>().Where(x => x.DisputeId == disputeId).Select(x => x.InvoiceId).ToList();
                if (nInvNums != null && nInvNums.Count > 0)
                {
                    int notDisputeInvCount = CommonRep.GetQueryable<InvoiceAging>().Where(x => nInvNums.Contains(x.InvoiceNum) && nDisputeInvStatusList.Contains(x.TrackStates)).Count();
                    if (notDisputeInvCount == 0)
                        dispute.Status = "026012";
                }

            }

            CommonRep.Commit();
        }

        /// <summary>
        /// 根据变更的Invoice ID，重新计算 Dispute 状态
        /// </summary>
        /// <param name="invIdList"></param>
        public void ReCalcDisputeStatus(List<int> invIdList)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
            {
                // 2017-12-15 追加判断，如果Dispute下所有Invoice都不为下述状态，则Close Dispute 任务
                //Dispute Identified  007
                //Wait for 2nd Time Dispute contact   008
                //Wait for Dispute Responds   009
                //Wait for 2nd Time Dispute respond   011
                //Wait for Round Table    012
                List<string> nDisputeInvStatusList = new List<string>() { "007", "008", "009", "011", "012" };
                List<string> nInvNumList = CommonRep.GetQueryable<InvoiceAging>().Where(x => invIdList.Contains(x.Id)).Select(x => x.InvoiceNum).ToList();
                List<int> nDisputeIdList = CommonRep.GetQueryable<DisputeInvoice>().Where(x => nInvNumList.Contains(x.InvoiceId)).Select(x => x.DisputeId).ToList();
                List<int> nDisputeIdList2 = CommonRep.GetQueryable<Dispute>().Where(x => nDisputeIdList.Contains(x.Id) && x.Status != "026012").Select(x => x.Id).ToList();
                if (nDisputeIdList2 != null && nDisputeIdList2.Count > 0)
                {
                    nDisputeIdList2.ForEach(x => ReCalcDisputeStatus(x));
                }

                scope.Complete();
            }
        }

        /// <summary>
        /// update t_dispute status and insert t_dispute_his his_type
        /// </summary>
        /// <param name="id">dispute id</param>
        /// <param name="status">status</param>
        public string UpdateStatus(int id, string status, string actionownerdept,string disputereason)
        {
            DisputeHis disHis = new DisputeHis();
            List<DisputeHis> listDisHis = new List<DisputeHis>();

            // 如果Dispute下存在Invoice为 下述状态,则不允许关闭或取消
            //Wait for 1st Time Dispute contact   007
            //Wait for 2nd Time Dispute contact   008
            //Wait for 1st Time Dispute respond   009
            //Wait for 2nd Time Dispute respond   011
            //Wait for Round Table    012

            //Cancelled   026011
            //Closed  026012
            if (status == "026011" || status == "026012")
            {
                List<string> nInvNumList = CommonRep.GetQueryable<DisputeInvoice>().Where(x => x.DisputeId == id).Select(x => x.InvoiceId).ToList();
                if (nInvNumList != null && nInvNumList.Count > 0)
                {
                    List<string> nDisputeStatusList = new List<string>() { "007", "008", "009", "011", "012" };
                    List<string> nInvList = CommonRep.GetQueryable<InvoiceAging>().Where(x => nInvNumList.Contains(x.InvoiceNum) && nDisputeStatusList.Contains(x.TrackStates)).Select(x => x.InvoiceNum).ToList();
                    if (nInvList != null && nInvList.Count > 0)
                    {
                        string nErr = string.Format("Can not Close/Cancel, Invoice No. : {0} have not been resolved.", string.Join(",", nInvList));
                        return nErr;
                    }
                }
            }

            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    Dispute dis = CommonRep.GetDbSet<Dispute>()
                                .Where(o => o.Id == id).FirstOrDefault();
                    dis.Status = status;
                    dis.STATUS_DATE = DateTime.Now;
                    dis.IssueReason = disputereason;
                    dis.ActionOwnerDepartmentCode = actionownerdept;

                    if (status == "026011" || status == "026012")
                        dis.CloseDate = DateTime.Now;

                    CommonRep.Commit();

                    DisputeHis oldDisputeHis = CommonRep.GetDbSet<DisputeHis>()
                                                .Where(o => o.DisputeId == id).FirstOrDefault();

                    //insert dispute_his
                    disHis.DisputeId = id;
                    disHis.HisType = status;
                    disHis.HisDate = AppContext.Current.User.Now;
                    disHis.EmailId = oldDisputeHis == null ? "" : oldDisputeHis.EmailId;
                    disHis.ISSUE_REASON = disputereason;
                    listDisHis.Add(disHis);
                    CommonRep.AddRange<DisputeHis>(listDisHis);
                    CommonRep.Commit();

                    // 更改状态为 Cancelled : 026011 , or Closed : 026012
                    if (status == "026011" || status == "026012")
                    {
                        // 需要重新计算的发票状态
                        List<string> nReCalcTrackerStatus = new List<string>() {"001","002","004","005","006","012","000",
                        "007","008","009","011"};

                        // 需要更新 Tracker Status = Responsed OverDue Reason，Tracker Date = 当前时间，  Finished Status = Initial，Mail Id 和 Call Id = Null
                        List<string> nUpdateTrackerStatus = new List<string>() { "001", "002", "004", "005", "006", "012", "000" };

                        // 获取Dispute下所有Invoice
                        List<string> nInvNumList = CommonRep.GetQueryable<DisputeInvoice>().Where(x => x.DisputeId == id).Select(x => x.InvoiceId).ToList();

                        if (nInvNumList != null && nInvNumList.Count > 0)
                        {
                            List<InvoiceAging> nInvList = CommonRep.GetQueryable<InvoiceAging>().Where(x => nInvNumList.Contains(x.InvoiceNum) && nReCalcTrackerStatus.Contains(x.TrackStates)).ToList();
                            if (nInvList != null && nInvList.Count > 0)
                            {
                                // 需要更新 Tracker Status = Responsed OverDue Reason 001，Tracker Date = 当前时间，  Finished Status = Initial 0，Mail Id 和 Call Id = Null
                                List<InvoiceAging> nUpdateInvList = nInvList.Where(x => nUpdateTrackerStatus.Contains(x.TrackStates)).ToList();

                                if (nUpdateInvList != null && nUpdateInvList.Count > 0)
                                {
                                    nUpdateInvList.ForEach(x =>
                                    {
                                        x.TrackStates = "001";
                                        x.TRACK_DATE = DateTime.Now;
                                        x.FinishedStatus = "0";
                                        x.MailId = null;
                                        x.CallId = null;
                                    });
                                }

                                CommonRep.Commit();

                            }
                        }

                    }

                    scope.Complete();
                }

                return "success";
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public void ExecRefreshInvoiceTrackerStatus(string invNums)
        {
            string deal = AppContext.Current.User.Deal;

            //DEAL Parameter
            var paramBuildDEAL = new SqlParameter
            {
                ParameterName = "@DEAL",
                Value = deal,
                Direction = ParameterDirection.Input
            };
            //LEGAL_ENTITY Parameter
            var paramBuildLegalEntity = new SqlParameter
            {
                ParameterName = "@LegalEntity",
                Value = "",
                Direction = ParameterDirection.Input
            };
            //CustomerNo Parameter
            var paramBuildCustomerNo = new SqlParameter
            {
                ParameterName = "@CustomerNo",
                Value = "",
                Direction = ParameterDirection.Input
            };
            //SiteUseId Parameter
            var paramBuildSiteUseId = new SqlParameter
            {
                ParameterName = "@SiteUseId",
                Value = "",
                Direction = ParameterDirection.Input
            };
            //InvoiceNo Parameter
            var paramBuildInvoiceNo = new SqlParameter
            {
                ParameterName = "@InvoiceNo",
                Value = invNums,
                Direction = ParameterDirection.Input
            };
            //Operator Parameter
            var paramBuildOperator = new SqlParameter
            {
                ParameterName = "@Operator",
                Value = AppContext.Current.User.EID,
                Direction = ParameterDirection.Input
            };
            //Operator Parameter
            var paramBuildSysDate = new SqlParameter
            {
                ParameterName = "@SysDate",
                Value = DateTime.Now,
                Direction = ParameterDirection.Input
            };
            //Reuslt Parameter(0:NG; 1:OK)
            var paramBuildResultStatus = new SqlParameter
            {
                ParameterName = "@ResultStatus",
                Value = 0,
                Direction = ParameterDirection.Output
            };

            object[] paramBuildList = new object[8];
            paramBuildList[0] = paramBuildDEAL;
            paramBuildList[1] = paramBuildLegalEntity;
            paramBuildList[2] = paramBuildCustomerNo;
            paramBuildList[3] = paramBuildSiteUseId;
            paramBuildList[4] = paramBuildInvoiceNo;
            paramBuildList[5] = paramBuildOperator;
            paramBuildList[6] = paramBuildSysDate;
            paramBuildList[7] = paramBuildResultStatus;

            Helper.Log.Info("Start: call spBuildInvoiceAgingStatus(procedure):@DEAL" + deal);

            CommonRep.GetDBContext().Database.ExecuteSqlCommand("spBuildInvoiceAgingStatus @DEAL,@LegalEntity,@CustomerNo,@SiteUseId,@InvoiceNo,@Operator,@SysDate,@ResultStatus OUTPUT", paramBuildList.ToArray());

            Helper.Log.Info("End: call spBuildInvoiceAgingStatus(procedure):@DEAL" + deal + ",RETURN:" + paramBuildResultStatus.Value.ToString());

        }


        public void SendDisputeMail(SendMailDto mailDto)
        {
            // 1 Add contact History
            MailTmp mail = mailDto.mailInstance;
            string[] nInvoiceArray = mailDto.invoiceNums.Split(',');
            List<ContactHistory> ConHisList = new List<ContactHistory>();
            List<CustomerKey> cus = new List<CustomerKey>();
            cus = mail.GetRelatedCustomers();
            if (cus.Count > 0)
            {
                foreach (var iCus in cus)
                {
                    string customer = iCus.CustomerNum;
                    string siteuseid = iCus.SiteUseId;

                    ContactHistory chis = new ContactHistory();
                    chis.Deal = AppContext.Current.User.Deal;
                    chis.LegalEntity = "";
                    chis.CustomerNum = customer;
                    chis.SiteUseId = siteuseid;
                    chis.CollectorId = AppContext.Current.User.EID;
                    chis.ContacterId = mail.To;
                    chis.ContactDate = AppContext.Current.User.Now;
                    chis.Comments = "";
                    chis.ContactId = mail.MessageId;
                    chis.LastUpdatePerson = AppContext.Current.User.EID;
                    chis.LastUpdateTime = DateTime.Now;
                    chis.ContactType = "Mail";
                    ConHisList.Add(chis);
                }
            }

            // 2. send mail
            MailService mailService = SpringFactory.GetObjectImpl<MailService>("MailService");
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
            {
                // Send mail logic.
                mailService.SendMail(mail);

                CommonRep.AddRange(ConHisList);

                if (nInvoiceArray != null && nInvoiceArray.Length > 0)
                {
                    foreach (var iInvoice in nInvoiceArray)
                    {
                        if (!string.IsNullOrEmpty(iInvoice))
                        {
                            var nlist = CommonRep.GetQueryable<InvoiceAging>().Where(x => x.Class == "INV" && nInvoiceArray.Contains(x.InvoiceNum)).ToList();
                            if (nlist != null && nlist.Count > 0)
                            {
                                nlist.ForEach(x =>
                                {
                                    x.MailId = mail.MessageId;
                                    CommonRep.Save(x);
                                });
                            }
                        }
                    }
                    CommonRep.Commit();
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(string.Format("p_UpdateTaskStatus '','','','{0}','{1}'", string.Join(",", nInvoiceArray), DateTime.Now));

                }

                CommonRep.Commit();
                scope.Complete();
            }
        }
    }
}
