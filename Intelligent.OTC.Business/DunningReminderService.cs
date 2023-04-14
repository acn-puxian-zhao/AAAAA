using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.UnitOfWork;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using System.Configuration;
using System.IO;
using Intelligent.OTC.Domain;
using Intelligent.OTC.Common;
using System.Data.Entity.Validation;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Common.Exceptions;
using System.Data.Entity;
using System.Web;
using System.Text.RegularExpressions;
using Intelligent.OTC.Common.Repository;
using System.Transactions;
using System.Data.SqlClient;
using System.Data;
using EntityFramework.BulkInsert.Extensions;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Metadata.Edm;
using System.Collections.ObjectModel;
using EntityFramework.MappingAPI.Mappings;
using System.Reflection;
using EntityFramework.MappingAPI.Exceptions;

namespace Intelligent.OTC.Business
{
    public class DunningReminderService : IDunningService
    {
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

        //WorkFlow use this parameter
        public string CurrentOper 
        {
            get
            {
                return AppContext.Current.User.Id.ToString();
            }

        }

        public int CurrentPeriod
        {
            get
            {
                return CommonRep.GetDbSet<PeriodControl>()
                    .Where(o => o.Deal == AppContext.Current.User.Deal).Max(o => o.Id);
            }
        }
        public OTCRepository CommonRep { get; set; }
        public XcceleratorRepository XRep { get; set; }
        private string IsWF = ConfigurationManager.AppSettings["IsWF"].ToString();

        public List<CollectorAlert> listget()
        {
            return CommonRep.GetQueryable<CollectorAlert>()
                                                 .Where(o => o.Deal == CurrentDeal && o.Eid == CurrentUser && o.Status == "Initialized" && o.AlertType == 1).ToList();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strCondition"></param>
        /// <param name="strType">"1" customerNum "2" taskid</param>
        /// <param name="strReminderType">1:2ndReminder;2:FinalReminder;3:Hold</param>
        public void insertDunningReminder(List<string> strCondition, string strType, string strReminderType,DateTime? dtBase = null)
        {
            Helper.Log.Info("DunningReminder's Add/Update Start!");
            List<CollectorAlert> alertsNew = new List<CollectorAlert>();
            CollectorAlert alertNew = new CollectorAlert();
            List<CollectorAlert> alertsOld = new List<CollectorAlert>();
            List<CustomerPaymentCircle> custPayments = new List<CustomerPaymentCircle>();
            string defaultCust = "Default999";
            List<CollectorAlert> alertList = new List<CollectorAlert>();
            DunningReminderConfig dun = new DunningReminderConfig();

            CustomerPaymentCircle custPayment = new CustomerPaymentCircle();

            List<CustomerLevelView> custLevels;
            custLevels = new List<CustomerLevelView>();

            List<DunningReminderConfig> duns = new List<DunningReminderConfig>();
            CustomerLevelView custLevel = new CustomerLevelView();

            CollectorAlert alertUpd = new CollectorAlert();

            List<InvoiceAging> invs = new List<InvoiceAging>();

            List<T_MD_EXCEPTIONS> excepts = new List<T_MD_EXCEPTIONS>();

            List<Sites> sites = new List<Sites>();

            List<string> siteList = new List<string>();

            List<CollectorAlert> alertsAllCustomer = new List<CollectorAlert>();

            if (dtBase == null)
            {
                dtBase = CurrentTime.Date;
            }
            else
            {
                dtBase = dtBase.Value.Date;
            }

            string strClass;
            List<string> Custs = new List<string>();
            try
            {
                PeroidService perService = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");

                //取得当前peroid
                PeriodControl per = perService.getcurrentPeroid();

                //取得alertList
                if (strType == "1")
                {
                    alertList = CommonRep.GetQueryable<CollectorAlert>()
                                                 .Where(o => o.Deal == CurrentDeal && o.Eid == CurrentUser && o.PeriodId == per.Id && strCondition.Contains(o.CustomerNum)).ToList();
                }
                else if (strType == "2")
                {
                    alertList = CommonRep.GetQueryable<CollectorAlert>()
                                                 .Where(o => o.Deal == CurrentDeal && o.Eid == CurrentUser && o.PeriodId == per.Id && strCondition.Contains(o.TaskId)).ToList();

                }
                //取得所有CustomerNum
                Custs = alertList.Select(o => o.CustomerNum).Distinct().ToList<string>();
                Custs.Add(defaultCust);

                alertsAllCustomer = CommonRep.GetQueryable<CollectorAlert>()
                                                 .Where(o => o.Deal == CurrentDeal && o.Eid == CurrentUser && o.PeriodId == per.Id && Custs.Contains(o.CustomerNum)).ToList();

                //取得所有CustomerPaymentCircle by CustomerNum
                custPayments =CommonRep.GetQueryable<CustomerPaymentCircle>()
                                                     .Where(o => o.Deal == CurrentDeal 
                                                         && o.PaymentDay >= CurrentTime 
                                                         && o.PaymentDay <= per.PeriodEnd
                                                         && Custs.Contains(o.CustomerNum)).ToList();
                //取得所有DunningReminderConfig by CustomerNum
                duns = CommonRep.GetQueryable<DunningReminderConfig>()
                                                    .Where(o => (o.Deal == CurrentDeal
                                                    && Custs.Contains(o.CustomerNum)) ||
                                                    o.VRClass != null).ToList();
                //取得所有CustomerLevel by CustomerNum
                custLevels = CommonRep.GetQueryable<CustomerLevelView>()
                            .Where(o => o.Deal == CurrentDeal
                            && Custs.Contains(o.CustomerNum)).ToList();

                //取得所有Holidy
                excepts = XRep.GetQueryable<T_MD_EXCEPTIONS>().ToList();

                //取得所有LegalEntity
                SiteService siteService = SpringFactory.GetObjectImpl<SiteService>("SiteService");
                sites = siteService.GetAllSites().Where(o => o.Deal == CurrentDeal).ToList();

                foreach (CollectorAlert alertOld in alertList.Where(o => o.AlertType == Convert.ToInt16(strReminderType)).Select(o => o))
                {
                    if (Convert.ToInt16(strReminderType) == 1)
                    {
                        siteList = new List<string>();
                        foreach (Sites site in sites)
                        {
                            siteList.Add(site.LegalEntity);
                        }
                    }
                    else if (Convert.ToInt16(strReminderType) == 2 || Convert.ToInt16(strReminderType) == 3)
                    {
                        siteList = new List<string>();
                        siteList.Add(alertOld.LegalEntity);
                    }

                    foreach (string strSite in siteList)
                    {
                        //取得对应DunningReminderConfig by CustomerNum && LegalEntity
                        dun = duns.Where(o => o.CustomerNum == alertOld.CustomerNum && o.LegalEntity == strSite).Select(o => o).FirstOrDefault();

                        //没有得到DunningReminderConfig 的时候，取得customer的level对应的DunningReminderConfig
                        if (dun == null)
                        {
                            custLevel = custLevels.Where(o => o.CustomerNum == alertOld.CustomerNum).ToList().FirstOrDefault();
                            if (custLevel == null)//customer的level没有取到的时候，取得defult DunningReminderConfig
                            {
                                dun = duns.Where(o => o.CustomerNum == defaultCust).Select(o => o).FirstOrDefault();
                            }
                            else
                            {
                                strClass = custLevel.ClassLevel + custLevel.RiskLevel;
                                dun = duns.Where(o => o.VRClass == strClass).Select(o => o).FirstOrDefault();
                            }
                        }

                        custPayment = new CustomerPaymentCircle();

                        //custPayment取得 by CustomerNum && LegalEntity
                        if (custPayments.Count > 0)
                        {
                            custPayment = custPayments.Where(o => o.CustomerNum == alertOld.CustomerNum && o.LegalEntity == strSite).Select(o => o).OrderByDescending(o => o.PaymentDay).FirstOrDefault();
                        }
                        //2nd 计算
                        if (Convert.ToInt16(strReminderType) == 1)
                        {
                            alertUpd = alertsAllCustomer.Where(o => o.AlertType == 2 && o.CustomerNum == alertOld.CustomerNum && o.LegalEntity == strSite).Select(o => o).FirstOrDefault();
                            if (alertUpd == null)
                            {
                                alertNew = new CollectorAlert();
                                alertNew.Eid = CurrentUser;
                                alertNew.Deal = CurrentDeal;
                                alertNew.CustomerNum = alertOld.CustomerNum;
                                alertNew.ActionDate = reminderDateGet(dtBase.Value, excepts, dun, strReminderType, per, "1", custPayment);
                                alertNew.CreateDate = CurrentTime;
                                alertNew.AlertType = 2;//1,2,3
                                alertNew.Status = "Initialized";
                                alertNew.PeriodId = per.Id;
                                alertNew.BatchType = alertOld.BatchType;
                                alertNew.ReferenceNo = alertOld.ReferenceNo;
                                alertNew.TaskId = string.Empty;
                                alertNew.ProcessId = string.Empty;
                                alertNew.FailedReason = string.Empty;
                                alertNew.CauseObjectNumber = string.Empty;
                                alertNew.LegalEntity = strSite;
                                alertsNew.Add(alertNew);
                            }
                            else
                            {
                                alertUpd.ActionDate = reminderDateGet(dtBase.Value, excepts, dun, strReminderType, per, "1", custPayment);
                            }
                        }

                        //final 计算
                        if (Convert.ToInt16(strReminderType) < 3)
                        {
                            alertUpd = alertsAllCustomer.Where(o => o.AlertType == 3 && o.CustomerNum == alertOld.CustomerNum && o.LegalEntity == strSite).Select(o => o).FirstOrDefault();
                            if (alertUpd == null)
                            {
                                alertNew = new CollectorAlert();
                                alertNew.Eid = CurrentUser;
                                alertNew.Deal = CurrentDeal;
                                alertNew.CustomerNum = alertOld.CustomerNum;
                                alertNew.ActionDate = reminderDateGet(dtBase.Value, excepts, dun, strReminderType, per, "2", custPayment);
                                alertNew.CreateDate = CurrentTime;
                                alertNew.AlertType = 3;//1,2,3
                                alertNew.Status = "Initialized";
                                alertNew.PeriodId = per.Id;
                                alertNew.BatchType = alertOld.BatchType;
                                alertNew.ReferenceNo = alertOld.ReferenceNo;
                                alertNew.TaskId = string.Empty;
                                alertNew.ProcessId = string.Empty;
                                alertNew.FailedReason = string.Empty;
                                alertNew.CauseObjectNumber = string.Empty;
                                alertNew.LegalEntity = strSite;
                                alertsNew.Add(alertNew);
                            }
                            else
                            {
                                alertUpd.ActionDate = reminderDateGet(dtBase.Value, excepts, dun, strReminderType, per, "2", custPayment);
                            }
                        }

                        //hold time 计算
                        alertUpd = alertsAllCustomer.Where(o => o.AlertType == 4 && o.CustomerNum == alertOld.CustomerNum && o.LegalEntity == strSite).Select(o => o).FirstOrDefault();
                        if (alertUpd == null)
                        {
                            alertNew = new CollectorAlert();
                            alertNew.Eid = CurrentUser;
                            alertNew.Deal = CurrentDeal;
                            alertNew.CustomerNum = alertOld.CustomerNum;
                            alertNew.ActionDate = reminderDateGet(dtBase.Value, excepts, dun, strReminderType, per, "3", custPayment);
                            alertNew.CreateDate = CurrentTime;
                            alertNew.AlertType = 4;//1,2,3
                            alertNew.Status = "Initialized";
                            alertNew.PeriodId = per.Id;
                            alertNew.BatchType = alertOld.BatchType;
                            alertNew.ReferenceNo = alertOld.ReferenceNo;
                            alertNew.TaskId = string.Empty;
                            alertNew.ProcessId = string.Empty;
                            alertNew.FailedReason = string.Empty;
                            alertNew.CauseObjectNumber = string.Empty;
                            alertNew.LegalEntity = strSite;
                            alertsNew.Add(alertNew);
                        }
                        else
                        {
                            alertUpd.ActionDate = reminderDateGet(dtBase.Value, excepts, dun, strReminderType, per, "3", custPayment);
                        }
                    }
                }

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    CommonRep.Commit();
                    (CommonRep.GetDBContext() as OTCEntities).BulkInsert(alertsNew);
                    scope.Complete();
                }

                Helper.Log.Info("DunningReminder's Add/Update End!");
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new Exception(ex.Message);
            }

        }

        public List<CollectorAlert> GetEstimatedReminders(List<string> customerNums, string legalEntity = null, DateTime? dtBase = null)
        {
            Helper.Log.Info("DunningReminder's Add/Update Start!");
            List<CollectorAlert> res = new List<CollectorAlert>();

            CollectorAlert alertNew = new CollectorAlert();

            List<CustomerPaymentCircle> custPayments = new List<CustomerPaymentCircle>();
            string defaultCust = "Default999";

            List<CollectorAlert> alertList = new List<CollectorAlert>();
            DunningReminderConfig dun = new DunningReminderConfig();

            CustomerPaymentCircle custPayment = new CustomerPaymentCircle();

            List<CustomerLevelView> custLevels = new List<CustomerLevelView>();

            List<DunningReminderConfig> duns = new List<DunningReminderConfig>();
            CustomerLevelView custLevel = new CustomerLevelView();

            CollectorAlert alertUpd = new CollectorAlert();

            List<InvoiceAging> invs = new List<InvoiceAging>();

            List<T_MD_EXCEPTIONS> excepts = new List<T_MD_EXCEPTIONS>();

            List<Sites> sites = new List<Sites>();

            List<string> siteList = new List<string>();

            List<string> siteNoInvList = new List<string>();
            List<CollectorAlert> alertsAllCustomer = new List<CollectorAlert>();
            string strOpen = null;

            if (dtBase == null)
            {
                dtBase = CurrentTime.Date;
            }
            else
            {
                dtBase = dtBase.Value.Date;
            }

            string strClass;
            List<string> Custs = new List<string>();
            try
            {
                PeroidService perService = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");

                //取得当前peroid
                PeriodControl per = perService.getcurrentPeroid();

                //取得alertList
                alertList = CommonRep.GetQueryable<CollectorAlert>()
                                             .Where(o => o.Deal == CurrentDeal && o.Eid == CurrentUser && o.PeriodId == per.Id && customerNums.Contains(o.CustomerNum)).ToList();

                //取得所有CustomerNum
                Custs = alertList.Select(o => o.CustomerNum).Distinct().ToList<string>();
                Custs.Add(defaultCust);

                alertsAllCustomer = CommonRep.GetQueryable<CollectorAlert>()
                                                 .Where(o => o.Deal == CurrentDeal && o.Eid == CurrentUser && o.PeriodId == per.Id && Custs.Contains(o.CustomerNum)).ToList();

                //取得所有CustomerPaymentCircle by CustomerNum
                custPayments = CommonRep.GetQueryable<CustomerPaymentCircle>()
                                                     .Where(o => o.Deal == CurrentDeal
                                                         && o.PaymentDay >= CurrentTime
                                                         && o.PaymentDay <= per.PeriodEnd
                                                         && Custs.Contains(o.CustomerNum)).ToList();

                //取得所有DunningReminderConfig by CustomerNum
                duns = CommonRep.GetQueryable<DunningReminderConfig>()
                                                    .Where(o => (o.Deal == CurrentDeal
                                                    && Custs.Contains(o.CustomerNum)) ||
                                                    o.VRClass != null).ToList();

                //取得所有CustomerLevel by CustomerNum
                custLevels = CommonRep.GetQueryable<CustomerLevelView>()
                            .Where(o => o.Deal == CurrentDeal
                            && Custs.Contains(o.CustomerNum)).ToList();

                //取得所有Holidy
                excepts = XRep.GetQueryable<T_MD_EXCEPTIONS>().ToList();

                strOpen = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Open);

                //取得aging
                invs = CommonRep.GetQueryable<InvoiceAging>()
                        .Where(o => o.Deal == CurrentDeal
                            && Custs.Contains(o.CustomerNum)
                            && o.States == strOpen).ToList();

                SiteService siteService = SpringFactory.GetObjectImpl<SiteService>("SiteService");
                if (!string.IsNullOrEmpty(legalEntity))
                {
                    siteList = siteService.GetAllSites().Where(o => o.Deal == CurrentDeal && o.LegalEntity == legalEntity).Select(o => o.LegalEntity).ToList();
                }
                else
                {
                    //取得所有LegalEntity
                    siteList = siteService.GetAllSites().Where(o => o.Deal == CurrentDeal).Select(o => o.LegalEntity).ToList();
                }

                foreach (string con in customerNums)
                {
                    foreach (string strSite in siteList)
                    {
                        //取得对应DunningReminderConfig by CustomerNum && LegalEntity
                        dun = duns.Where(o => o.CustomerNum == con && o.LegalEntity == strSite).Select(o => o).FirstOrDefault();

                        //没有得到DunningReminderConfig 的时候，取得customer的level对应的DunningReminderConfig
                        if (dun == null)
                        {
                            custLevel = custLevels.Where(o => o.CustomerNum == con).ToList().FirstOrDefault();
                            if (custLevel == null)//customer的level没有取到的时候，取得defult DunningReminderConfig
                            {
                                dun = duns.Where(o => o.CustomerNum == defaultCust).Select(o => o).FirstOrDefault();
                            }
                            else
                            {
                                strClass = custLevel.ClassLevel + custLevel.RiskLevel;
                                dun = duns.Where(o => o.VRClass == strClass).Select(o => o).FirstOrDefault();
                            }
                        }

                        custPayment = new CustomerPaymentCircle();

                        //custPayment取得 by CustomerNum && LegalEntity
                        if (custPayments.Count > 0)
                        {
                            custPayment = custPayments.Where(o => o.CustomerNum == con && o.LegalEntity == strSite).Select(o => o).OrderByDescending(o => o.PaymentDay).FirstOrDefault();
                        }

                        CollectorAlert currNotStartStep = null;
                        // Try get First Reminder(SOA) time from DB
                        DateTime? soaActionDate = null;
                        alertUpd = alertsAllCustomer.Where(o => o.AlertType == 1 && o.CustomerNum == con).Select(o => o).FirstOrDefault();
                        if (alertUpd != null)
                        {
                            var soa = new CollectorAlert();
                            ObjectHelper.CopyObjectWithUnNeed(alertUpd, soa, "Id");
                            if (soa.Status != "Finish")
                            {
                                soa.OrginalActionDate = soa.ActionDate;
                                currNotStartStep = soa;
                                setCurrentStep(currNotStartStep);
                            }

                            res.Add(soa);

                            soaActionDate = soa.ActionDate;
                        }
                        else
                        {
                            // If SOA start doesn't complete. It may arrive at this line of code. Normally, SOA alert always exist.
                            var soa = new CollectorAlert();
                            soa.CustomerNum = con;
                            soa.LegalEntity = null;
                            soa.AlertType = 1;
                            soa.ActionDate = dtBase.Value;
                            soa.OrginalActionDate = soa.ActionDate;
                            currNotStartStep = soa;
                            setCurrentStep(currNotStartStep);
                            res.Add(soa);

                            soaActionDate = soa.ActionDate;
                        }

                        // Try get Second Reminder time from DB
                        DateTime? secondReminderActionDate = null;
                        alertUpd = alertsAllCustomer.Where(o => o.AlertType == 2 && o.CustomerNum == con && o.LegalEntity == strSite).Select(o => o).FirstOrDefault();
                        if (alertUpd == null)
                        {
                            alertNew = new CollectorAlert();
                            alertNew.Eid = CurrentUser;
                            alertNew.Deal = CurrentDeal;
                            alertNew.CustomerNum = con;
                            alertNew.ActionDate = reminderDateGet(soaActionDate.Value, excepts, dun, "1", per, "1", custPayment);
                            alertNew.OrginalActionDate = alertNew.ActionDate;
                            alertNew.CreateDate = CurrentTime;
                            alertNew.AlertType = 2;//1,2,3
                            alertNew.Status = "Initialized";
                            alertNew.PeriodId = per.Id;
                            alertNew.TaskId = string.Empty;
                            alertNew.ProcessId = string.Empty;
                            alertNew.FailedReason = string.Empty;
                            alertNew.CauseObjectNumber = string.Empty;
                            alertNew.LegalEntity = strSite;
                            res.Add(alertNew);

                            if (currNotStartStep == null)
                            {
                                currNotStartStep = alertNew;
                                setCurrentStep(currNotStartStep);
                            }

                            secondReminderActionDate = alertNew.ActionDate;
                        }
                        else
                        {
                            alertUpd.OrginalActionDate = alertUpd.ActionDate;
                            res.Add(alertUpd);
                            secondReminderActionDate=alertUpd.ActionDate;
                        }

                        // Try get Final Reminder time from DB
                        DateTime? finalReminderActionDate = null;
                        alertUpd = alertsAllCustomer.Where(o => o.AlertType == 3 && o.CustomerNum == con && o.LegalEntity == strSite).Select(o => o).FirstOrDefault();
                        if (alertUpd == null)
                        {
                            alertNew = new CollectorAlert();
                            alertNew.Eid = CurrentUser;
                            alertNew.Deal = CurrentDeal;
                            alertNew.CustomerNum = con;
                            alertNew.ActionDate = reminderDateGet(secondReminderActionDate.Value, excepts, dun, "2", per, "2", custPayment);
                            alertNew.OrginalActionDate = alertNew.ActionDate;
                            alertNew.CreateDate = CurrentTime;
                            alertNew.AlertType = 3;//1,2,3
                            alertNew.Status = "Initialized";
                            alertNew.PeriodId = per.Id;
                            alertNew.TaskId = string.Empty;
                            alertNew.ProcessId = string.Empty;
                            alertNew.FailedReason = string.Empty;
                            alertNew.CauseObjectNumber = string.Empty;
                            alertNew.LegalEntity = strSite;
                            res.Add(alertNew);

                            if (currNotStartStep == null)
                            {
                                currNotStartStep = alertNew;
                                setCurrentStep(currNotStartStep);
                            }

                            finalReminderActionDate = alertNew.ActionDate;
                        }
                        else
                        {
                            alertUpd.OrginalActionDate = alertUpd.ActionDate;
                            res.Add(alertUpd);
                            finalReminderActionDate = alertUpd.ActionDate;
                        }

                        //Try get Hold time from DB
                        alertUpd = alertsAllCustomer.Where(o => o.AlertType == 4 && o.CustomerNum == con && o.LegalEntity == strSite).Select(o => o).FirstOrDefault();
                        if (alertUpd == null)
                        {
                            alertNew = new CollectorAlert();
                            alertNew.Eid = CurrentUser;
                            alertNew.Deal = CurrentDeal;
                            alertNew.CustomerNum = con;
                            alertNew.ActionDate = reminderDateGet(finalReminderActionDate.Value, excepts, dun, "3", per, "3", custPayment);
                            alertNew.OrginalActionDate = alertNew.ActionDate;
                            alertNew.CreateDate = CurrentTime;
                            alertNew.AlertType = 4;//1,2,3
                            alertNew.Status = "Initialized";
                            alertNew.PeriodId = per.Id;
                            alertNew.TaskId = string.Empty;
                            alertNew.ProcessId = string.Empty;
                            alertNew.FailedReason = string.Empty;
                            alertNew.CauseObjectNumber = string.Empty;
                            alertNew.LegalEntity = strSite;
                            res.Add(alertNew);

                            if (currNotStartStep == null)
                            {
                                currNotStartStep = alertNew;
                                setCurrentStep(currNotStartStep);
                            }
                        }
                        else
                        {
                            alertUpd.OrginalActionDate = alertUpd.ActionDate;
                            res.Add(alertUpd);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Error happened during alert date calculation", ex);
                throw;
            }

            return res;
        }

        //Start add by xuan.wu for arrow adding
        public List<CollectorAlert> GetEstimatedRemindersForArrow(List<string> customerNums, string siteUseId, string legalEntity = null, DateTime? dtBase = null)
        {
            Helper.Log.Info("DunningReminder's Add/Update Start!");
            List<CollectorAlert> res = new List<CollectorAlert>();

            CollectorAlert alertNew = new CollectorAlert();

            string defaultCust = "Default999";

            List<CollectorAlert> alertList = new List<CollectorAlert>();
            List<CustomerLevelView> custLevels = new List<CustomerLevelView>();
            CustomerLevelView custLevel = new CustomerLevelView();

            CollectorAlert alertUpd = new CollectorAlert();

            List<InvoiceAging> invs = new List<InvoiceAging>();

            List<T_MD_EXCEPTIONS> excepts = new List<T_MD_EXCEPTIONS>();

            List<Sites> sites = new List<Sites>();

            List<string> siteList = new List<string>();

            List<string> siteNoInvList = new List<string>();
            List<CollectorAlert> alertsAllCustomer = new List<CollectorAlert>();
            string strOpen = null;

            if (dtBase == null)
            {
                dtBase = CurrentTime.Date;
            }
            else
            {
                dtBase = dtBase.Value.Date;
            }

            string strClass;
            List<string> Custs = new List<string>();
            try
            {
                PeroidService perService = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");

                //取得当前peroid
                PeriodControl per = perService.getcurrentPeroid();

                //取得alertList
                alertList = CommonRep.GetQueryable<CollectorAlert>()
                                             .Where(o => o.Deal == CurrentDeal && o.Eid == CurrentUser && o.PeriodId == per.Id && customerNums.Contains(o.CustomerNum) && o.SiteUseId==siteUseId).ToList();

                //取得所有CustomerNum
                Custs = alertList.Select(o => o.CustomerNum ).Distinct().ToList<string>();
                //取得所有的
                Custs.Add(defaultCust);

                alertsAllCustomer = CommonRep.GetQueryable<CollectorAlert>()
                                                 .Where(o => o.Status != "Cancelled" && o.Deal == CurrentDeal && o.Eid == CurrentUser && o.PeriodId == per.Id && Custs.Contains(o.CustomerNum)&&o.SiteUseId==siteUseId).ToList();

                //取得所有CustomerLevel by CustomerNum
                custLevels = CommonRep.GetQueryable<CustomerLevelView>()
                            .Where(o => o.Deal == CurrentDeal
                            && Custs.Contains(o.CustomerNum)).ToList();

                //取得所有Holidy
                excepts = XRep.GetQueryable<T_MD_EXCEPTIONS>().ToList();

                strOpen = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Open);

                //取得aging
                invs = CommonRep.GetQueryable<InvoiceAging>()
                        .Where(o => o.Deal == CurrentDeal
                            && Custs.Contains(o.CustomerNum)
                            && o.States == strOpen&&o.SiteUseId==siteUseId).ToList();

                SiteService siteService = SpringFactory.GetObjectImpl<SiteService>("SiteService");

                foreach (string con in customerNums)
                {
                    //foreach (string strSite in siteList)
                    //{
                        CollectorAlert currNotStartStep = null;
                        // Try get First Reminder(SOA) time from DB
                        DateTime? soaActionDate = null;
                        alertUpd = alertsAllCustomer.Where(o => o.AlertType == 1 && o.CustomerNum == con&& o.SiteUseId==siteUseId).Select(o => o).FirstOrDefault();
                        if (alertUpd != null)
                        {
                            var soa = new CollectorAlert();
                            ObjectHelper.CopyObjectWithUnNeed(alertUpd, soa, "Id");
                            if (soa.Status != "Finish")
                            {
                                soa.OrginalActionDate = soa.ActionDate;
                                currNotStartStep = soa;
                                setCurrentStep(currNotStartStep);
                            }

                            res.Add(soa);

                            soaActionDate = soa.ActionDate;
                        }
                        else
                        {
                            // If SOA start doesn't complete. It may arrive at this line of code. Normally, SOA alert always exist.
                            var soa = new CollectorAlert();
                            soa.CustomerNum = con;
                            soa.LegalEntity = null;
                            soa.AlertType = 1;
                            soa.ActionDate = dtBase.Value;
                            soa.OrginalActionDate = soa.ActionDate;
                            currNotStartStep = soa;
                            setCurrentStep(currNotStartStep);
                            soa.SiteUseId = siteUseId;
                            res.Add(soa);

                            soaActionDate = soa.ActionDate;
                        }
                    //}
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Error happened during alert date calculation", ex);
                throw;
            }

            return res;
        }
        //End add by xuan.wu for arrow adding
        private void setCurrentStep(CollectorAlert alertNew)
        {
            alertNew.ActionDate = AppContext.Current.User.Now.Date;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="bsDate">计算时间</param>
        /// <param name="exceptions">休息日</param>
        /// <param name="dun">DunningReminderConfig</param>
        /// <param name="strReminderType">计算base ReminderType</param>
        /// <param name="per"></param>
        /// <param name="strNo">strNo：1：second reminder 2:final reminder 3:hold date</param>
        /// <param name="pay">payment date 可空</param>
        /// <returns></returns>
        public DateTime reminderDateGet(DateTime bsDate, List<T_MD_EXCEPTIONS> exceptions
                                        , DunningReminderConfig dun, string strReminderType
                                        , PeriodControl per,string strNo, CustomerPaymentCircle pay = null)
        {
            DateTime rtn;
            rtn = CurrentTime;
            DateTime endDt;
            DateTime endDtNew;

            //decimal? null=> 0
            if (!dun.FirstInterval.HasValue)
            {
                dun.FirstInterval = 0;
            }
            if (!dun.SecondInterval.HasValue)
            {
                dun.SecondInterval = 0;
            }
            if (!dun.PaymentTAT.HasValue)
            {
                dun.SecondInterval = 0;
            }
            if (!dun.RiskInterval.HasValue)
            {
                dun.RiskInterval = 0;
            }

            //end date
            if (pay == null)
            {
                endDt = per.PeriodEnd.Date; 
            }
            else if (pay.Id != 0 && pay.PaymentDay.HasValue)
            {
                endDt = pay.PaymentDay.Value.Date;
            }
            else
            {
                endDt = per.PeriodEnd.Date; 
            }
            endDt = workingDateAdd(endDt, 0, "1", exceptions);
            endDtNew = endDt;
            if (strReminderType == "1")
            {
                endDtNew = workingDateAdd(bsDate, Convert.ToInt16(dun.FirstInterval + dun.SecondInterval + dun.PaymentTAT + dun.RiskInterval * 2), "0", exceptions);
            }
            else if (strReminderType == "2")
            {
                endDtNew = workingDateAdd(bsDate, Convert.ToInt16(dun.SecondInterval + dun.PaymentTAT + dun.RiskInterval), "0", exceptions);
            }
            else if (strReminderType == "3")
            {
                endDtNew = workingDateAdd(bsDate, Convert.ToInt16(dun.PaymentTAT), "0", exceptions); 
            }
            if (endDt > endDtNew)
            {
                if (strNo == "1")//second reminder
                {
                    rtn = workingDateAdd(endDtNew, Convert.ToInt16(dun.SecondInterval + dun.PaymentTAT + dun.RiskInterval), "1", exceptions);
                }
                else if (strNo == "2")//final reminder
                {
                    rtn = workingDateAdd(endDtNew, Convert.ToInt16(dun.PaymentTAT), "1", exceptions);
                }
                else if (strNo == "3")//hold date
                {
                    rtn = endDtNew;
                }
            }
            else
            {
                if (strNo == "1")//second reminder
                {
                    rtn = workingDateAdd(endDt, Convert.ToInt16(dun.SecondInterval + dun.PaymentTAT + dun.RiskInterval), "1", exceptions);
                }
                else if (strNo == "2")//final reminder
                {
                    rtn = workingDateAdd(endDt, Convert.ToInt16(dun.PaymentTAT), "1", exceptions);
                }
                else if (strNo == "3")//hold date
                {
                    rtn = endDt;
                }
                if (rtn < CurrentTime.Date)//时间最早是今天，并且今天不需要判断是不是工作日
                {
                    rtn = CurrentTime.Date;
                }
            }

            return rtn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bsDate">变化日期</param>
        /// <param name="days">变化天数</param>
        /// <param name="addFlg">0加1减</param>
        /// <param name="exceptions">节假日</param>
        /// <returns></returns>
        private DateTime workingDateAdd(DateTime bsDate, int days,string addFlg, List<T_MD_EXCEPTIONS> exceptions)
        {
            DateTime rtn;

            var isExceptionDate = new Func<DateTime, bool>(
                    date => exceptions.Where(o => o.EXCEPTIONS_TYPE == 1 
                                            && date >= o.START_TIME 
                                            && date <= o.END_TIME).Count() > 0);

            rtn = bsDate.Date;

                while (days > 0)
                {
                    if (
                        !isExceptionDate(rtn))
                    {
                        days--;
                    }
                    if (addFlg == "0")
                    {
                        rtn = rtn.AddDays(1);
                    }
                    else
                    {
                        rtn = rtn.AddDays(-1);
                    }
                }

            while ( isExceptionDate(rtn))
            {
                if (addFlg == "0")
                {
                    rtn = rtn.AddDays(1);
                }
                else
                {
                    rtn = rtn.AddDays(-1);
                }
            }

            return rtn;
        }

        private int getDataTimeIndex(int bsIndex, string strReminderType, int endIndex, int payIndex, DunningReminderConfig dun, string str)
        {
            int rtn;
            rtn = 0;
            int intEnd;
            intEnd = 0;
            int intNewEnd;
            intNewEnd = 0;

            if (payIndex <= bsIndex)
            {
                rtn = 0;
            }
            else if (payIndex == 0 && endIndex <= bsIndex)
            {
                rtn = 0;
            }
            else
            {
                if (payIndex == 0)
                {
                    intEnd = endIndex;
                }
                else
                {
                    intEnd = payIndex;
                }
                if (strReminderType == "1")
                {
                    intNewEnd = Convert.ToInt16(bsIndex + dun.FirstInterval + dun.SecondInterval + dun.PaymentTAT + dun.RiskInterval * 2);
                }
                else if (strReminderType == "2")
                {
                    intNewEnd = Convert.ToInt16(bsIndex + dun.SecondInterval + dun.PaymentTAT + dun.RiskInterval);
                }
                else if (strReminderType == "3")
                {
                    intNewEnd = Convert.ToInt16(bsIndex + dun.PaymentTAT);
                }
                if (intNewEnd <= intEnd)
                {
                    rtn = Convert.ToInt16(bsIndex + dun.FirstInterval + dun.RiskInterval);

                    if (str == "2")
                    {
                        rtn = Convert.ToInt16(rtn + dun.SecondInterval + dun.RiskInterval);
                    }
                    if (str == "3")
                    {
                        rtn = intNewEnd;
                    }
                }
                else
                {
                    rtn = Convert.ToInt16(intEnd - (dun.PaymentTAT));
                    if (str == "1")
                    {
                        rtn = Convert.ToInt16(rtn - (dun.SecondInterval + dun.RiskInterval));
                    }
                    if (str == "3")
                    {
                        rtn = intEnd;
                    }
                    
                }
                    
            }
            if (rtn < bsIndex)
            {
                rtn = 0;
            }
            return rtn;
        }

        private DateTime getDatetime(DateTime bsDt,string strReminderType, DateTime perEndDt, DateTime? payEndDt, DunningReminderConfig dun,string str)
        {
            DateTime rtn = new DateTime();
            DateTime endDt = new DateTime();
            DateTime endDtNew = new DateTime();
            if (perEndDt <= CurrentTime)
            {
                rtn = CurrentTime;
            }
            else
            { 
                    if (payEndDt == null)
                    {
                        endDt = perEndDt;
                    }
                    else
                    {
                        endDt = payEndDt.Value;
                    }
                    
                    if(strReminderType == "1")
                    {
                        endDtNew = bsDt.AddDays(Convert.ToInt64(dun.FirstInterval + dun.SecondInterval + dun.PaymentTAT + dun.RiskInterval * 2));
                    }
                    else if(strReminderType == "2")
                    {
                        endDtNew = bsDt.AddDays(Convert.ToInt64(dun.SecondInterval + dun.PaymentTAT + dun.RiskInterval));
                    }
                    else if (strReminderType == "3")
                    {
                        endDtNew = bsDt.AddDays(Convert.ToInt64(dun.PaymentTAT));
                    }
                    if (endDtNew > endDt)
                    {
                        rtn = endDt.AddDays(Convert.ToInt64(-(dun.PaymentTAT)));
                        if (str == "1")
                        {
                            rtn = rtn.AddDays(Convert.ToInt64(-(dun.SecondInterval + dun.RiskInterval)));
                        }
                        if (str == "3")
                        {
                            rtn = endDt;
                        }
                    }
                    else
                    {
                        rtn = bsDt.AddDays(Convert.ToInt64(dun.FirstInterval + dun.RiskInterval));
                
                        if (str == "2")
                        {
                            rtn = rtn.AddDays(Convert.ToInt64(dun.SecondInterval + dun.RiskInterval)); 
                        }
                        if (str == "3")
                        {
                            rtn = endDtNew;
                        }
                
                    }
            }
            if (rtn < bsDt)
            {
                rtn = bsDt;
            }
            return Convert.ToDateTime(rtn.ToShortDateString());
        }

        public IEnumerable<DunningReminderDto> GetDunningList(string invoiceState = "", string invoiceTrackState = "", string invoiceNum = "", string soNum = "", string poNum = "", string invoiceMemo = "")
        {
            // base table: dunning
            var r = (from final in
                         (from res in
                              (from dun in CommonRep.GetDbSet<CollectorAlert>()
                                   // left join customer level
                               join cust in SpringFactory.GetObjectImpl<CustomerService>("CustomerService").GetCustomerLevel(CurrentPeriod) on new { dun.CustomerNum, dun.Deal, dun.SiteUseId } equals new { cust.CustomerNum, cust.Deal, cust.SiteUseId }
                                  into custs
                               from cust in custs.DefaultIfEmpty()
                               where dun.Status != "Cancelled" && dun.Status != "Finish" && (dun.AlertType == 1) && !string.IsNullOrEmpty(dun.LegalEntity) && dun.ActionDate < CurrentTime && dun.PeriodId == CurrentPeriod
                               // inner join aging
                               join age in CommonRep.GetQueryable<CustomerAging>().Where(o => o.Deal == CurrentDeal) on new { cust.CustomerNum, cust.Deal, dun.LegalEntity, dun.SiteUseId } equals new { age.CustomerNum, age.Deal, age.LegalEntity, age.SiteUseId }
                               join soa in CommonRep.GetDbSet<CollectorAlert>().Where(a => a.AlertType == 1 && a.Status == "Finish" && a.PeriodId == CurrentPeriod) on new { cust.CustomerNum, cust.Deal, cust.SiteUseId } equals new { soa.CustomerNum, soa.Deal, soa.SiteUseId }
                                  into soas
                               from soa in soas.DefaultIfEmpty()
                               join inv in CommonRep.GetDbSet<InvoiceAging>().Where(inv => inv.Deal == CurrentDeal && (inv.TrackStates == "001" || inv.TrackStates == "002" || inv.TrackStates == "003" || inv.TrackStates == "004" || inv.TrackStates == "005" || inv.TrackStates == "002" || inv.TrackStates == "006" || inv.TrackStates == "015")) on new { age.CustomerNum, age.Deal, age.SiteUseId } equals new { inv.CustomerNum, inv.Deal, inv.SiteUseId }   //&& inv.DueDate <= Period.PeriodEnd此条件去掉；&& inv.States == "004001"改为track_states
                                  into invs
                               where invs.Any(
                                                i => dun.ReferenceNo.Contains(i.CustomerNum)
                                                && (i.States == invoiceState || string.IsNullOrEmpty(invoiceState))
                                                && (i.TrackStates == invoiceTrackState || string.IsNullOrEmpty(invoiceTrackState))
                                                && (i.InvoiceNum.IndexOf(invoiceNum) >= 0 || string.IsNullOrEmpty(invoiceNum))
                                                && (i.SoNum.IndexOf(soNum) >= 0 || string.IsNullOrEmpty(soNum))
                                                && (i.PoNum.IndexOf(poNum) >= 0 || string.IsNullOrEmpty(poNum))
                                                && (i.Comments.IndexOf(invoiceMemo) >= 0 || string.IsNullOrEmpty(invoiceMemo))
                                                )
                               select new
                               {
                                   Id = dun.Id,
                                   ActionDate = dun.ActionDate,
                                   Deal = cust.Deal,
                                   TaskId = dun.TaskId,
                                   ReferenceNo = dun.ReferenceNo,
                                   ProcessId = dun.ProcessId,
                                   DunStatus = dun.Status,
                                   CauseObjectNumber = dun.CauseObjectNumber,
                                   BatchType = dun.BatchType,
                                   FailedReason = dun.FailedReason,
                                   PeriodId = dun.PeriodId,
                                   AlertType = dun.AlertType,
                                   CustomerNum = cust.CustomerNum,
                                   CustomerName = cust.CustomerName,
                                   BillGroupCode = cust.CustomerName,
                                   BillGroupName = cust.CustomerName,
                                   Operator = cust.Collector,
                                   LegalEntity = dun.LegalEntity,
                                   CreditLimit = age == null ? 0 : age.CreditLimit,
                                   TotalAmt = age == null ? 0 : age.TotalAmt,
                                   CurrentAmt = age == null ? 0 : age.CurrentAmt,
                                   FDueOver90Amt = age == null ? 0 : (age.Due120Amt + age.Due150Amt + age.Due180Amt + age.Due210Amt + age.Due240Amt + age.Due270Amt + age.Due300Amt + age.Due330Amt + age.Due360Amt + age.DueOver360Amt),
                                   PastDueAmt = age == null ? 0 : age.DueoverTotalAmt,
                                   Risk = cust.Risk,
                                   Value = cust.Value,
                                   Class = cust.Class,
                                   LastRemind = 2,
                                   SiteUseId = cust.SiteUseId
                               })
                          group res by new
                          {
                              res.Id,
                              res.ActionDate,
                              res.Deal,
                              res.TaskId,
                              res.ReferenceNo,
                              res.ProcessId,
                              res.DunStatus,
                              res.CauseObjectNumber,
                              res.BatchType,
                              res.FailedReason,
                              res.PeriodId,
                              res.AlertType,
                              res.CustomerNum,
                              res.CustomerName,
                              res.BillGroupCode,
                              res.BillGroupName,
                              res.Operator,
                              res.Risk,
                              res.Value,
                              res.Class,
                              res.SiteUseId,
                              res.LegalEntity,
                              res.LastRemind
                          }
                              into reses
                          select new
                          {
                              Id = reses.Key.Id,
                              ActionDate = reses.Key.ActionDate,
                              Deal = reses.Key.Deal,
                              TaskId = reses.Key.TaskId,
                              ReferenceNo = reses.Key.ReferenceNo,
                              ProcessId = reses.Key.ProcessId,
                              DunStatus = reses.Key.DunStatus,
                              CauseObjectNumber = reses.Key.CauseObjectNumber,
                              BatchType = reses.Key.BatchType,
                              FailedReason = reses.Key.FailedReason,
                              PeriodId = reses.Key.PeriodId,
                              AlertType = reses.Key.AlertType,
                              CustomerNum = reses.Key.CustomerNum,
                              CustomerName = reses.Key.CustomerName,
                              BillGroupCode = reses.Key.BillGroupCode,
                              BillGroupName = reses.Key.BillGroupName,
                              Class = reses.Key.Class,
                              Risk = reses.Key.Risk,
                              CreditLimit = reses.Sum(age => age.CreditLimit),
                              TotalAmt = reses.Sum(age => age.TotalAmt),
                              CurrentAmt = reses.Sum(age => age.CurrentAmt),
                              FDueOver90Amt = reses.Sum(age => age.FDueOver90Amt),
                              PastDueAmt = reses.Sum(age => age.PastDueAmt),
                              Operator = reses.Key.Operator,
                              LegalEntity = reses.Key.LegalEntity,
                              LastRemind = reses.Key.LastRemind,
                              SiteUseId = reses.Key.SiteUseId
                          })

                     select new DunningReminderDto
                     {
                         Id = final.Id,
                         ActionDate = final.ActionDate,
                         Deal = final.Deal,
                         TaskId = final.TaskId,
                         ReferenceNo = final.ReferenceNo,
                         ProcessId = final.ProcessId,
                         DunStatus = final.DunStatus,
                         CauseObjectNumber = final.CauseObjectNumber,
                         BatchType = final.BatchType,
                         FailedReason = final.FailedReason,
                         PeriodId = final.PeriodId,
                         AlertType = final.AlertType,
                         CustomerNum = final.CustomerNum,
                         CustomerName = final.CustomerName,
                         BillGroupCode = string.IsNullOrEmpty(final.BillGroupCode) == true ? final.CustomerName : final.BillGroupCode,
                         BillGroupName = string.IsNullOrEmpty(final.BillGroupName) == true ? final.CustomerName : final.BillGroupName,
                         CreditLimit = final.CreditLimit,
                         TotalAmt = final.TotalAmt,
                         CurrentAmt = final.CurrentAmt,
                         FDueOver90Amt = final.FDueOver90Amt,
                         PastDueAmt = final.PastDueAmt,
                         Class = final.Class,
                         Risk = final.Risk,
                         Operator = final.Operator,
                         LegalEntity = final.LegalEntity,
                         LastRemind = final.LastRemind.ToString()
                     });

            return r;
        }

        public DateTime dataConvertToDT(string strData)
        {
            DateTime dt = new DateTime();
            if (!string.IsNullOrEmpty(strData.Trim()))
            {
                return Convert.ToDateTime(strData);
            }

            return dt;
        }
        public IEnumerable<SendSoaHead> CreateDun(string ColDun, int AlertType, int AlertId)
        {
            #region createdunlist
            string[] cusGroup = ColDun.Split(',');
            //cus
            var cusList = CommonRep.GetQueryable<Customer>()
                .Where(o => o.Deal == CurrentDeal && cusGroup.Contains(o.CustomerNum)).ToList();
            Customer cus = new Customer();
            //aging
            var cusAgingList = CommonRep.GetQueryable<CustomerAging>()
                .Where(o => o.Deal == CurrentDeal && cusGroup.Contains(o.CustomerNum)).ToList();
            //sendsoa
            List<SendSoaHead> sendsoaList = new List<SendSoaHead>();
            SendSoaHead sendsoa = new SendSoaHead();
            //SpecialNotes
            var SNList = CommonRep.GetQueryable<SpecialNote>().Where(o => o.Deal == CurrentDeal && cusGroup.Contains(o.CustomerNum)).ToList();
            //customerchangehis=>class
            var classList = CommonRep.GetQueryable<CustomerLevelView>()
                .Where(o => o.Deal == CurrentDeal && cusGroup.Contains(o.CustomerNum)).ToList();
            CustomerLevelView level = new CustomerLevelView();
            //Rate
            var rateList = CommonRep.GetQueryable<RateTran>()
                .Where(o => o.Deal == CurrentDeal && o.EffectiveDate <= CurrentTime.Date && o.ExpiredDate >= CurrentTime.Date).ToList();
            //agingDT
            DateTime agingDT = new DateTime();
            PeroidService pservice = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            PeriodControl currentP = pservice.getcurrentPeroid();
            agingDT = dataConvertToDT(currentP.PeriodEnd.ToString());
            DateTime agingDT90 = new DateTime();
            agingDT90 = agingDT.AddDays(-90);
            //invoice
            var oldinvoiceList = CommonRep.GetQueryable<InvoiceAging>()
                .Where(o => o.Deal == CurrentDeal && cusGroup.Contains(o.CustomerNum)).ToList();
            List<InvoiceAging> newinvoiceList = new List<InvoiceAging>();
            newinvoiceList = oldinvoiceList;
            try
            {
                foreach (var item in newinvoiceList)
                {
                    if (item.Currency != "USD")
                    {
                        item.StandardBalanceAmt = (rateList.Find(m => m.ForeignCurrency == item.Currency).Rate == null ? 1 : rateList.Find(m => m.ForeignCurrency == item.Currency).Rate) * item.BalanceAmt;
                    }
                    else { item.StandardBalanceAmt = item.BalanceAmt; }
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw;
            }
            foreach (var item in cusGroup)
            {
                sendsoa = new SendSoaHead();
                cus = cusList.Find(m => m.Deal == CurrentDeal && m.CustomerNum == item);
                level = classList.Find(m => m.Deal == CurrentDeal && m.CustomerNum == item);
                var newCusAgingList = cusAgingList.FindAll(m => m.Deal == CurrentDeal && m.CustomerNum == item);
                sendsoa.Deal = CurrentDeal;
                sendsoa.CustomerCode = item;
                sendsoa.CustomerName = cus.CustomerName;
                sendsoa.TotalBalance = newCusAgingList.Sum(m => m.TotalAmt);
                sendsoa.CustomerClass = (string.IsNullOrEmpty(level.ClassLevel) == true ? "LV" : level.ClassLevel)
                    + (string.IsNullOrEmpty(level.RiskLevel) == true ? "LR" : level.RiskLevel);

                //contactHistory
                List<SubContactHistory> ContactHisList = new List<SubContactHistory>();
                SubContactHistory ContactHis = new SubContactHistory();
                var OldConHisList = CommonRep.GetDbSet<ContactHistory>().Where(o => o.Deal == CurrentDeal && o.CustomerNum == item);
                int ihis = 1;
                foreach (var his in OldConHisList)
                {
                    ContactHis = new SubContactHistory();
                    ContactHis.SortId = ihis;
                    ContactHis.Deal = his.Deal;
                    ContactHis.CustomerNum = his.CustomerNum;
                    ContactHis.LegalEntity = his.LegalEntity;
                    ContactHis.ContactType = his.ContactType;
                    ContactHis.ContactDate = his.ContactDate;
                    ContactHis.ContactId = his.ContactId;
                    ContactHis.Comments = his.Comments;
                    ContactHisList.Add(ContactHis);
                    ihis++;
                }
                sendsoa.SubContactHistory = ContactHisList;

                //Legal
                List<SoaLegal> sublegalList = new List<SoaLegal>();
                SoaLegal sublegal = new SoaLegal();
                foreach (var legal in newCusAgingList)
                {
                    var invoice = newinvoiceList
                        .FindAll(m => m.CustomerNum == item && m.LegalEntity == legal.LegalEntity);
                    var inv1 = invoice.FindAll(m => m.States == "004001" || m.States == "004002" || m.States == "004004" || m.States == "004008" || m.States == "004010" || m.States == "004011" || m.States == "004012").OrderBy(m => m.DueDate).ToList();
                        sublegal = new SoaLegal();
                        sublegal.LegalEntity = legal.LegalEntity;
                        sublegal.Country = legal.Country;
                        sublegal.CreditLimit = legal.CreditLimit;
                        sublegal.TotalARBalance = legal.TotalAmt;
                        sublegal.PastDueAmount = legal.DueoverTotalAmt;
                        sublegal.CreditBalance = invoice.FindAll(m => m.BalanceAmt < 0).Sum(m => m.StandardBalanceAmt);
                        sublegal.CurrentBalance = legal.CurrentAmt;
                        sublegal.FCollectableAmount = inv1
                            .FindAll(m => m.DueDate <= agingDT && (m.Class == "DM" || m.Class == "INV")).Sum(m => m.StandardBalanceAmt);
                        sublegal.FOverdue90Amount = inv1
                            .FindAll(m => m.DueDate <= agingDT90 && (m.Class == "DM" || m.Class == "INV")).Sum(m => m.StandardBalanceAmt);
                        var SN = SNList.Find(m => m.CustomerNum == item && m.LegalEntity == legal.LegalEntity);
                        if (SN == null)
                        {
                            sublegal.SpecialNotes = "";
                        }
                        else
                        {
                            sublegal.SpecialNotes = SN.SpecialNotes;
                        }
                        sublegal.SubTracking = GetCT(AlertId);
                        List<SoaInvoice> subinvoiceList = new List<SoaInvoice>();
                        SoaInvoice subinvoice = new SoaInvoice();
                        if (inv1.Count > 0)
                        {
                            foreach (var inv in inv1)
                            {
                                subinvoice = new SoaInvoice();
                                subinvoice.InvoiceId = inv.Id;
                                subinvoice.InvoiceNum = inv.InvoiceNum;
                                subinvoice.CustomerNum = inv.CustomerNum;
                                subinvoice.CustomerName = inv.CustomerName;
                                subinvoice.LegalEntity = inv.LegalEntity;
                                subinvoice.InvoiceDate = inv.InvoiceDate;
                                subinvoice.CreditTerm = inv.CreditTrem;
                                subinvoice.DueDate = inv.DueDate;
                                subinvoice.PurchaseOrder = inv.PoNum;
                                subinvoice.SaleOrder = inv.SoNum;
                                subinvoice.RBO = inv.MstCustomer;
                                subinvoice.InvoiceCurrency = inv.Currency;
                                subinvoice.OriginalInvoiceAmount = inv.OriginalAmt.ToString();
                                subinvoice.OutstandingInvoiceAmount = inv.BalanceAmt;
                                subinvoice.DaysLate = (AppContext.Current.User.Now.Date - Convert.ToDateTime(inv.DueDate).Date).Days.ToString();
                                subinvoice.InvoiceTrack = !string.IsNullOrEmpty(inv.TrackStates) == false ? "" : Helper.CodeToEnum<TrackStatus>(inv.TrackStates).ToString().Replace("_", " ");
                                subinvoice.Status = !String.IsNullOrEmpty(inv.States) ? Helper.CodeToEnum<InvoiceStatus>(inv.States).ToString().Replace("_", " ") : "";
                                //added by zhangYu 20151205 start
                                subinvoice.PtpDate = inv.PtpDate;
                                //added by zhangYu 20151205 End
                                subinvoice.DocumentType = inv.Class;
                                subinvoice.Comments = inv.Comments;
                                subinvoice.StandardInvoiceAmount = inv.StandardBalanceAmt;
                                subinvoiceList.Add(subinvoice);
                            }
                        }
                        else
                        {
                            subinvoice = new SoaInvoice();
                            subinvoiceList.Add(subinvoice);
                        }
                        sublegal.SubInvoice = subinvoiceList;
                        sublegalList.Add(sublegal);
                    }
                sendsoa.SubLegal = sublegalList;
                sendsoaList.Add(sendsoa);
            }
            #endregion
            //**********************************WF Start ***********************************
            if (IsWF == "true")
            {
                var CauseObjectNumber = CommonRep.FindBy<CollectorAlert>(AlertId).CauseObjectNumber;
                if (string.IsNullOrEmpty(CauseObjectNumber))
                {
                    if (GetPStatus(CauseObjectNumber) == 0)
                    {
                        Wfchange("4", AlertId, "start", AlertType);
                    }
                }
            }
            return sendsoaList.AsQueryable<SendSoaHead>();
        }

        public int GetPStatus(string CauseObjectNumber)
        {
            IWorkflowService wfservice = SpringFactory.GetObjectImpl<IWorkflowService>("WorkflowService");
            if (wfservice.GetProcessStatus("4", CauseObjectNumber, CurrentOper, "Processing").Count > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            };
        }

        //Get ProcessId
        public string GetProcessId(long taskid)
        {
            IWorkflowService wfservice = SpringFactory.GetObjectImpl<IWorkflowService>("WorkflowService");
            //get processinstanceid
            List<string> status = new List<string>();
            status.Add("Processing");
            var task = wfservice.GetMyTaskList(CurrentOper, status).Find(m => m.Id == taskid);
            string processid = task.ProcessInstance_Id.ToString();

            return processid;
        }
        //WFchange
        public void Wfchange(string processDefinationId, int AlertId, string type, int AlertType)
        {
            IWorkflowService wfservice = SpringFactory.GetObjectImpl<IWorkflowService>("WorkflowService");
            var alert = new CollectorAlert();
            if (type != "start")
            {
                alert = CommonRep.FindBy<CollectorAlert>(AlertId);
            }
            if (type == "start")
            {
                string causeObjectNum = Guid.NewGuid().ToString();
                if (IsWF == "true")
                {
                    var task = wfservice.StartProcess(processDefinationId, causeObjectNum, CurrentOper);
                    wfservice.AcceptTask(task.TaskId, causeObjectNum, CurrentOper);
                    string processid = GetProcessId(task.TaskId);

                    UpdateAlert(AlertId, task.TaskId.ToString(), processid, causeObjectNum, "Processing", AlertType);
                }
            }
            else if (type == "restart")
            {
                //2016-01-12 把Finish的Task重新开启
                UpdateAlert(AlertId, "", "", "", "Restart", AlertType);
            }
            else if (type == "cancel")
            {
                if (IsWF == "true")
                {
                    wfservice.CancelTask(processDefinationId, alert.CauseObjectNumber, CurrentOper, alert.TaskId);
                    UpdateAlert(AlertId, "", "", "", "Cancel", AlertType);
                }
            }
            else if (type == "pause")
            {
                if (IsWF == "true")
                {
                    wfservice.PauseProcess(processDefinationId, alert.CauseObjectNumber, CurrentOper, alert.TaskId);
                    UpdateAlert(AlertId, "", "", "", "Pause", AlertType);
                }
            }
            else if (type == "resume")
            {
                if (IsWF == "true")
                {
                    wfservice.ResumeProcess(processDefinationId, alert.CauseObjectNumber, CurrentOper, alert.TaskId);
                    UpdateAlert(AlertId, "", "", "", "Resume", AlertType);
                }
            }
            else if (type == "finish")
            {
                if (IsWF == "true")
                {
                    wfservice.FinishProcess(processDefinationId, alert.CauseObjectNumber, CurrentOper, alert.TaskId);
                    UpdateAlert(AlertId, "", "", "", "Finish", AlertType);
                }
            }
        }

        //update alert
        /// <summary>
        /// update alert when after send soa or pause/resume/cancel/finish workflow
        /// </summary>
        /// <param name="cusnums">array custnums</param>
        /// <param name="TaskId">task id (if workflow )</param>
        /// <param name="ProcessId">processId ( if workflow )</param>
        /// <param name="status">status in alert</param>
        /// <param name="type">batch or execute: 1:batch;2:execute</param>
        public void UpdateAlert(int AlertId, string TaskId, string ProcessId, string causeObjectNum, string status, int AlertType)
        {
            //------------------------------- added by alex -----------------------
            string[] cusnums = null;
            string deal = AppContext.Current.User.Deal.ToString();
            string eid = AppContext.Current.User.EID.ToString();
            // pxc update 20160311
            List<CollectorAlert> alertList = new List<CollectorAlert>();
            //Get customer numbers
            cusnums = CommonRep.GetQueryable<CollectorAlert>()
                        .Where(o => o.Id == AlertId).Select(o => o.ReferenceNo).FirstOrDefault().Split(',');

            var colAlert = CommonRep.FindBy<CollectorAlert>(AlertId);

            //------------------------------- added by alex -----------------------

            if (status == "Processing")
            {
                foreach (var cus in cusnums)
                {
                    List<CollectorAlert> alert = new List<CollectorAlert>();
                    // pxc update 20160311
                    alert = CommonRep.GetQueryable<CollectorAlert>().Where(m => m.Deal == deal && m.AlertType == AlertType && m.Status != "Cancelled" && m.CustomerNum == cus && m.Status != "Finish" && m.PeriodId == CurrentPeriod).OrderByDescending(m => m.CreateDate).ToList();
                    foreach (var item in alert)
                    {
                        item.TaskId = TaskId;
                        item.ProcessId = ProcessId;
                        item.ReferenceNo = string.Join(",", cusnums);
                        item.CauseObjectNumber = causeObjectNum;
                        item.Status = status; 
                    }
                }
            }
            else if (status == "Pause" || status == "Resume" || status == "Finish")
            {
                List<string> strCondition = new List<string>();
                var DunTask = "";
                var DunBatchType = colAlert.BatchType.ToString();
                var DunAlertType = "";
                foreach (var cus in cusnums)
                {
                    //added by alex
                    List<CollectorAlert> alert = new List<CollectorAlert>();
                    // pxc update 20160311
                    alert = CommonRep.GetQueryable<CollectorAlert>().Where(m => m.Deal == deal && m.AlertType == AlertType && m.Status != "Cancelled" && m.CustomerNum == cus && m.Status != "Finish" && m.PeriodId == CurrentPeriod).OrderByDescending(m => m.CreateDate).ToList();
                    //added by alex
                    foreach (var item in alert)
                    {
                        item.Status = status;
                        //-----------------added by zhichao---------------
                        if (status == "Finish")
                        {
                            item.ActionDate = AppContext.Current.User.Now.Date;
                        }
                        //------------------------------------------------
                        //Dunning para
                        if (status == "Finish" && string.IsNullOrEmpty(DunTask))
                        {
                            DunTask = item.TaskId;
                        }
                        if (status == "Finish" && string.IsNullOrEmpty(DunAlertType))
                        {
                            DunAlertType = item.AlertType.ToString();
                        }
                    }
                }
                if (status == "Finish")
                {
                    //Dunning
                    strCondition.Add(DunTask);
                    insertDunningReminder(strCondition, DunBatchType, DunAlertType);
                }
            }
            else if (status == "Cancel" || status == "Restart")
            {
                foreach (var cus in cusnums)
                {
                    List<CollectorAlert> alert = new List<CollectorAlert>();
                    if (status == "Cancel")
                    {
                        // pxc update 20160311
                        alert = CommonRep.GetQueryable<CollectorAlert>().Where(m => m.Deal == deal && m.AlertType == AlertType && m.Status != "Cancelled" && m.CustomerNum == cus && m.Status != "Finish" && m.PeriodId == CurrentPeriod).OrderByDescending(m => m.CreateDate).ToList();
                    }
                    else if (status == "Restart") 
                    {
                        // pxc update 20160311
                        alert = CommonRep.GetQueryable<CollectorAlert>().Where(m => m.Deal == deal && m.AlertType == AlertType && m.Status != "Cancelled" && m.CustomerNum == cus && m.Status == "Finish" && m.PeriodId == CurrentPeriod).OrderByDescending(m => m.CreateDate).ToList();
                    }
                    foreach (var item in alert)
                    {
                        item.TaskId = "";
                        item.ProcessId = "";
                        item.CauseObjectNumber = "";
                        item.Status = "Initialized";
                    }
                }
            }
            CommonRep.Commit();
        }

        //get a dunning status
        public CollectorAlert GetStatus(int AlertId)
        {
            return CommonRep.FindBy<CollectorAlert>(AlertId);
        }

        public CurrentTracking GetCT(int AlertIdForCT)
        {
            CurrentTracking ct = new CurrentTracking();
            var Reminder = CommonRep.FindBy<CollectorAlert>(AlertIdForCT);
            if (Reminder.AlertType == 2) {
                //Reminder2th
                ct.R2Id = Reminder.Id;
                ct.TempDate = Reminder.ActionDate;
                ct.Reminder2thDate = CurrentTime.Date;
                if (Reminder.Status == "Finish")
                {
                    ct.Reminder2thStatus = 1;
                }
                else {
                    if (Reminder.ActionDate < CurrentTime)
                    {
                        ct.Reminder2thStatus = 0;
                    }
                    else {
                        ct.Reminder2thStatus = 2;
                    }
                }
                //Reminder3th
                var Reminder3th = CommonRep.GetDbSet<CollectorAlert>()
                    .Where(o => o.PeriodId == Reminder.PeriodId && o.CustomerNum == Reminder.CustomerNum && o.Deal == Reminder.Deal
                        && o.AlertType == 3 && o.LegalEntity == Reminder.LegalEntity && o.Status != "Cancelled").FirstOrDefault();
                if (Reminder3th != null)
                {
                    ct.R3Id = Reminder3th.Id;
                    ct.Reminder3thDate = Reminder3th.ActionDate;
                    if (Reminder3th.Status == "Finish")
                    {
                        ct.Reminder3thStatus = 1;
                    }
                    else
                    {
                        if (Reminder3th.ActionDate < CurrentTime)
                        {
                            ct.Reminder3thStatus = 0;
                        }
                        else
                        {
                            ct.Reminder3thStatus = 2;
                        }
                    }
                }
            }
            else if (Reminder.AlertType == 3) {
                //Reminder3th
                ct.R3Id = Reminder.Id;
                ct.TempDate = Reminder.ActionDate;
                ct.Reminder3thDate = CurrentTime.Date;
                if (Reminder.Status == "Finish")
                {
                    ct.Reminder3thStatus = 1;
                }
                else
                {
                    if (Reminder.ActionDate < CurrentTime)
                    {
                        ct.Reminder3thStatus = 0;
                    }
                    else
                    {
                        ct.Reminder3thStatus = 2;
                    }
                }
                //Reminder2th
                var Reminder2th = CommonRep.GetDbSet<CollectorAlert>()
                    .Where(o => o.PeriodId == Reminder.PeriodId && o.CustomerNum == Reminder.CustomerNum && o.Deal == Reminder.Deal
                        && o.AlertType == 2 && o.LegalEntity == Reminder.LegalEntity && o.Status != "Cancelled").FirstOrDefault();
                if (Reminder2th != null)
                {
                    ct.R2Id = Reminder2th.Id;
                    ct.Reminder2thDate = Reminder2th.ActionDate;
                    if (Reminder2th.Status == "Finish")
                    {
                        ct.Reminder2thStatus = 1;
                    }
                    else
                    {
                        if (Reminder2th.ActionDate < CurrentTime)
                        {
                            ct.Reminder2thStatus = 0;
                        }
                        else
                        {
                            ct.Reminder2thStatus = 2;
                        }
                    }
                }
            }
            //Soa
            var Soa = CommonRep.GetDbSet<CollectorAlert>()
                .Where(o => o.PeriodId == Reminder.PeriodId && o.CustomerNum == Reminder.CustomerNum && o.Deal == Reminder.Deal
                    && o.AlertType == 1 && o.Status != "Cancelled").FirstOrDefault();
            if (Soa != null)
            {
                ct.SoaId = Soa.Id;
                ct.SoaDate = Soa.ActionDate;
                if (Soa.Status == "Finish")
                {
                    ct.SoaStatus = 1;
                }
                else
                {
                    if (Soa.ActionDate < CurrentTime)
                    {
                        ct.SoaStatus = 0;
                    }
                    else
                    {
                        ct.SoaStatus = 2;
                    }
                }
            }
            //Hold
            var Hold = CommonRep.GetDbSet<CollectorAlert>()
                .Where(o => o.PeriodId == Reminder.PeriodId && o.CustomerNum == Reminder.CustomerNum && o.Deal == Reminder.Deal
                    && o.AlertType == 4 && o.LegalEntity == Reminder.LegalEntity && o.Status != "Cancelled").FirstOrDefault();
            if (Hold != null)
            {
                ct.HoldId = Hold.Id;
                ct.HoldDate = Hold.ActionDate;
                if (Hold.Status == "Finish")
                {
                    ct.HoldStatus = 1;
                }
                else
                {
                    if (Hold.ActionDate < CurrentTime)
                    {
                        ct.HoldStatus = 0;
                    }
                    else
                    {
                        ct.HoldStatus = 2;
                    }
                }
            }
            //Close
            PeroidService ps = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            var CurPeriod = ps.getcurrentPeroid();
            ct.CloseDate = CurPeriod.PeriodEnd;
            ct.CloseStatus = 1;

            ct.CurrentDate = CurrentTime;

            //DunningConfig
            var DunningConfig = CommonRep.GetDbSet<DunningReminderConfig>()
                .Where(o => o.Deal == CurrentDeal && o.CustomerNum == Reminder.CustomerNum && o.LegalEntity == Reminder.LegalEntity).FirstOrDefault();
            if(DunningConfig == null){
                var Class = CommonRep.GetDbSet<CustomerLevelView>()
                    .Where(o => o.Deal == CurrentDeal && o.CustomerNum == Reminder.CustomerNum).FirstOrDefault();
                string VRClass = Class.ClassLevel + Class.RiskLevel;
                DunningConfig = CommonRep.GetDbSet<DunningReminderConfig>()
                    .Where(o => o.Deal == CurrentDeal && o.VRClass == VRClass).FirstOrDefault();
            }
            ct.FirstInterval = Convert.ToInt32(DunningConfig.FirstInterval);
            ct.SecondInterval = Convert.ToInt32(DunningConfig.SecondInterval);
            ct.PaymentTat = Convert.ToInt32(DunningConfig.PaymentTAT);
            ct.RiskInterval = Convert.ToInt32(DunningConfig.RiskInterval);
            ct.Desc = DunningConfig.Description;
            
            return ct;
        }

        public IEnumerable<DunningReminderDto> GetNoPaging(string ListType)
        {
            #region linq dunning
            var r = (from final in
                         (from res in
                              (from dun in CommonRep.GetDbSet<CollectorAlert>()
                               // left join customer level
                               join cust in SpringFactory.GetObjectImpl<CustomerService>("CustomerService").GetCustomerLevel(CurrentPeriod) on new { dun.CustomerNum, dun.Deal } equals new { cust.CustomerNum, cust.Deal }
                                  into custs
                               from cust in custs.DefaultIfEmpty()
                               where dun.Status == "Finish" && (dun.AlertType == 2 || dun.AlertType == 3) && !string.IsNullOrEmpty(dun.LegalEntity) && dun.PeriodId == CurrentPeriod
                               // inner join aging
                               join age in CommonRep.GetQueryable<CustomerAging>().Where(o => o.Deal == CurrentDeal ) on new { cust.CustomerNum, cust.Deal, dun.LegalEntity } equals new { age.CustomerNum, age.Deal, age.LegalEntity }
                               // left join hold customer table
                               join ch in CommonRep.GetDbSet<HoldCustomer>() on new { age.CustomerNum, age.LegalEntity } equals new { ch.CustomerNum, ch.LegalEntity }
                                  into custHolds
                               from ch in custHolds.DefaultIfEmpty()
                               // left join SOA alert
                               join soa in CommonRep.GetDbSet<CollectorAlert>().Where(a => a.AlertType == 1 && a.Status == "Finish" && a.PeriodId == CurrentPeriod) on new { cust.CustomerNum, cust.Deal, } equals new { soa.CustomerNum, soa.Deal }
                                  into soas
                               from soa in soas.DefaultIfEmpty()
                               // left join second dunning alert
                               join secDun in CommonRep.GetDbSet<CollectorAlert>().Where(a => a.AlertType == 2 && a.Status == "Finish" && a.PeriodId == CurrentPeriod) on new { cust.CustomerNum, cust.Deal, dun.LegalEntity } equals new { secDun.CustomerNum, secDun.Deal, secDun.LegalEntity }
                                  into secDuns
                               from secDun in secDuns.DefaultIfEmpty()
                               select new
                               {
                                   Id = dun.Id,
                                   ActionDate = dun.ActionDate,
                                   Deal = cust.Deal,
                                   TaskId = dun.TaskId,
                                   ReferenceNo = dun.ReferenceNo,
                                   ProcessId = dun.ProcessId,
                                   DunStatus = dun.Status,
                                   CauseObjectNumber = dun.CauseObjectNumber,
                                   BatchType = dun.BatchType,
                                   FailedReason = dun.FailedReason,
                                   PeriodId = dun.PeriodId,
                                   AlertType = dun.AlertType,
                                   CustomerNum = cust.CustomerNum,
                                   CustomerName = cust.CustomerName,
                                   BillGroupCode = cust.BillGroupCode,
                                   BillGroupName = cust.BillGroupName,
                                   IsHoldFlg = custHolds.Any(c => c.IsHoldFlg == "1") ? "On Hold" : "Normal",
                                   Operator = cust.Collector,
                                   LegalEntity = dun.LegalEntity,
                                   CreditLimit = age == null ? 0 : age.CreditLimit,
                                   TotalAmt = age == null ? 0 : age.TotalAmt,
                                   CurrentAmt = age == null ? 0 : age.CurrentAmt,
                                   FDueOver90Amt = age == null ? 0 : (age.Due120Amt + age.Due150Amt + age.Due180Amt + age.Due210Amt + age.Due240Amt + age.Due270Amt + age.Due300Amt + age.Due330Amt + age.Due360Amt + age.DueOver360Amt),
                                   PastDueAmt = age == null ? 0 : age.DueoverTotalAmt,
                                   Risk = cust.Risk,
                                   Value = cust.Value,
                                   Class = (string.IsNullOrEmpty(cust.ValueLevel) == true ? "LV" : cust.ValueLevel) +
                                           (string.IsNullOrEmpty(cust.RiskLevel) == true ? "LR" : cust.RiskLevel),
                                   LastRemind = dun.AlertType == 2
                                                  ? (soa != null ? (soa.BatchType == 1 ? soa.CustomerNum : soa.ProcessId) : dun.CustomerNum)
                                                  : dun.AlertType == 3 ? (secDun != null ? secDun.ProcessId : null) : null
                               })
                          group res by new
                          {
                              res.Id,
                              res.ActionDate,
                              res.Deal,
                              res.TaskId,
                              res.ReferenceNo,
                              res.ProcessId,
                              res.DunStatus,
                              res.CauseObjectNumber,
                              res.BatchType,
                              res.FailedReason,
                              res.PeriodId,
                              res.AlertType,
                              res.CustomerNum,
                              res.CustomerName,
                              res.BillGroupCode,
                              res.BillGroupName,
                              res.Operator,
                              res.Risk,
                              res.Value,
                              res.Class,
                              res.IsHoldFlg,
                              res.LegalEntity,
                              res.LastRemind
                          }
                              into reses
                              select new
                              {
                                  Id = reses.Key.Id,
                                  ActionDate = reses.Key.ActionDate,
                                  Deal = reses.Key.Deal,
                                  TaskId = reses.Key.TaskId,
                                  ReferenceNo = reses.Key.ReferenceNo,
                                  ProcessId = reses.Key.ProcessId,
                                  DunStatus = reses.Key.DunStatus,
                                  CauseObjectNumber = reses.Key.CauseObjectNumber,
                                  BatchType = reses.Key.BatchType,
                                  FailedReason = reses.Key.FailedReason,
                                  PeriodId = reses.Key.PeriodId,
                                  AlertType = reses.Key.AlertType,
                                  CustomerNum = reses.Key.CustomerNum,
                                  CustomerName = reses.Key.CustomerName,
                                  BillGroupCode = reses.Key.BillGroupCode,
                                  BillGroupName = reses.Key.BillGroupName,
                                  Class = reses.Key.Class,
                                  Risk = reses.Key.Risk,
                                  CreditLimit = reses.Sum(age => age.CreditLimit),
                                  TotalAmt = reses.Sum(age => age.TotalAmt),
                                  CurrentAmt = reses.Sum(age => age.CurrentAmt),
                                  FDueOver90Amt = reses.Sum(age => age.FDueOver90Amt),
                                  PastDueAmt = reses.Sum(age => age.PastDueAmt),
                                  Operator = reses.Key.Operator,
                                  IsHoldFlg = reses.Key.IsHoldFlg,
                                  LegalEntity = reses.Key.LegalEntity,
                                  LastRemind = reses.Key.LastRemind
                              })
                     select new DunningReminderDto
                     {
                         Id = final.Id,
                         ActionDate = final.ActionDate,
                         Deal = final.Deal,
                         TaskId = final.TaskId,
                         ReferenceNo = final.ReferenceNo,
                         ProcessId = final.ProcessId,
                         DunStatus = final.DunStatus,
                         CauseObjectNumber = final.CauseObjectNumber,
                         BatchType = final.BatchType,
                         FailedReason = final.FailedReason,
                         PeriodId = final.PeriodId,
                         AlertType = final.AlertType,
                         CustomerNum = final.CustomerNum,
                         CustomerName = final.CustomerName,
                         BillGroupCode = string.IsNullOrEmpty(final.BillGroupCode) == true ? final.CustomerName : final.BillGroupCode,
                         BillGroupName = string.IsNullOrEmpty(final.BillGroupName) == true ? final.CustomerName : final.BillGroupName,
                         CreditLimit = final.CreditLimit,
                         TotalAmt = final.TotalAmt,
                         CurrentAmt = final.CurrentAmt,
                         FDueOver90Amt = final.FDueOver90Amt,
                         PastDueAmt = final.PastDueAmt,
                         Class = final.Class,
                         Risk = final.Risk,
                         Operator = final.Operator,
                         IsHoldFlg = final.IsHoldFlg,
                         LegalEntity = final.LegalEntity,
                         LastRemind = final.LastRemind
                     });
            #endregion
            return r;
        }

        public IEnumerable<DunningReminderDto> SelectChangePeriod(int PeriodId)
        {
            #region linq dunning
#pragma warning disable CS0472 // The result of the expression is always 'true' since a value of type 'int' is never equal to 'null' of type 'int?'
            if (PeriodId != null)
#pragma warning restore CS0472 // The result of the expression is always 'true' since a value of type 'int' is never equal to 'null' of type 'int?'
            {
                var r = (from final in
                             (from res in
                                  (from dun in CommonRep.GetDbSet<CollectorAlert>()
                                   // left join customer level
                                   join cust in SpringFactory.GetObjectImpl<CustomerService>("CustomerService").GetCustomerLevel(PeriodId) on new { dun.CustomerNum, dun.Deal } equals new { cust.CustomerNum, cust.Deal }
                                      into custs
                                   from cust in custs.DefaultIfEmpty()
                                   where dun.Status == "Finish" && (dun.AlertType == 2 || dun.AlertType == 3) && !string.IsNullOrEmpty(dun.LegalEntity) && dun.PeriodId == PeriodId
                                   // inner join aging
                                   join age in CommonRep.GetQueryable<CustomerAging>().Where(o => o.Deal == CurrentDeal) on new { cust.CustomerNum, cust.Deal, dun.LegalEntity } equals new { age.CustomerNum, age.Deal, age.LegalEntity }
                                   // left join hold customer table
                                   join ch in CommonRep.GetDbSet<HoldCustomer>() on new { age.CustomerNum, age.LegalEntity } equals new { ch.CustomerNum, ch.LegalEntity }
                                      into custHolds
                                   from ch in custHolds.DefaultIfEmpty()
                                   // left join SOA alert
                                   join soa in CommonRep.GetDbSet<CollectorAlert>().Where(a => a.AlertType == 1 && a.Status == "Finish" && a.PeriodId == PeriodId) on new { cust.CustomerNum, cust.Deal, } equals new { soa.CustomerNum, soa.Deal }
                                      into soas
                                   from soa in soas.DefaultIfEmpty()
                                   // left join second dunning alert
                                   join secDun in CommonRep.GetDbSet<CollectorAlert>().Where(a => a.AlertType == 2 && a.Status == "Finish" && a.PeriodId == PeriodId) on new { cust.CustomerNum, cust.Deal, dun.LegalEntity } equals new { secDun.CustomerNum, secDun.Deal, secDun.LegalEntity }
                                      into secDuns
                                   from secDun in secDuns.DefaultIfEmpty()
                                   select new
                                   {
                                       Id = dun.Id,
                                       ActionDate = dun.ActionDate,
                                       Deal = cust.Deal,
                                       TaskId = dun.TaskId,
                                       ReferenceNo = dun.ReferenceNo,
                                       ProcessId = dun.ProcessId,
                                       DunStatus = dun.Status,
                                       CauseObjectNumber = dun.CauseObjectNumber,
                                       BatchType = dun.BatchType,
                                       FailedReason = dun.FailedReason,
                                       PeriodId = dun.PeriodId,
                                       AlertType = dun.AlertType,
                                       CustomerNum = cust.CustomerNum,
                                       CustomerName = cust.CustomerName,
                                       BillGroupCode = cust.BillGroupCode,
                                       BillGroupName = cust.BillGroupName,
                                       IsHoldFlg = custHolds.Any(c => c.IsHoldFlg == "1") ? "On Hold" : "Normal",
                                       Operator = cust.Collector,
                                       LegalEntity = dun.LegalEntity,
                                       CreditLimit = age == null ? 0 : age.CreditLimit,
                                       TotalAmt = age == null ? 0 : age.TotalAmt,
                                       CurrentAmt = age == null ? 0 : age.CurrentAmt,
                                       FDueOver90Amt = age == null ? 0 : (age.Due120Amt + age.Due150Amt + age.Due180Amt + age.Due210Amt + age.Due240Amt + age.Due270Amt + age.Due300Amt + age.Due330Amt + age.Due360Amt + age.DueOver360Amt),
                                       PastDueAmt = age == null ? 0 : age.DueoverTotalAmt,
                                       Risk = cust.Risk,
                                       Value = cust.Value,
                                       Class = (string.IsNullOrEmpty(cust.ValueLevel) == true ? "LV" : cust.ValueLevel) +
                                               (string.IsNullOrEmpty(cust.RiskLevel) == true ? "LR" : cust.RiskLevel),
                                       // Used for dunning detail page data loading.
                                       LastRemind = dun.AlertType == 2
                                                      ? (soa != null ? (soa.BatchType == 1 ? soa.CustomerNum : soa.ProcessId) : dun.CustomerNum)
                                                      : dun.AlertType == 3 ? (secDun != null ? secDun.ProcessId : null) : null
                                   })
                              group res by new
                              {
                                  res.Id,
                                  res.ActionDate,
                                  res.Deal,
                                  res.TaskId,
                                  res.ReferenceNo,
                                  res.ProcessId,
                                  res.DunStatus,
                                  res.CauseObjectNumber,
                                  res.BatchType,
                                  res.FailedReason,
                                  res.PeriodId,
                                  res.AlertType,
                                  res.CustomerNum,
                                  res.CustomerName,
                                  res.BillGroupCode,
                                  res.BillGroupName,
                                  res.Operator,
                                  res.Risk,
                                  res.Value,
                                  res.Class,
                                  res.IsHoldFlg,
                                  res.LegalEntity,
                                  res.LastRemind
                              }
                                  into reses
                                  select new
                                  {
                                      Id = reses.Key.Id,
                                      ActionDate = reses.Key.ActionDate,
                                      Deal = reses.Key.Deal,
                                      TaskId = reses.Key.TaskId,
                                      ReferenceNo = reses.Key.ReferenceNo,
                                      ProcessId = reses.Key.ProcessId,
                                      DunStatus = reses.Key.DunStatus,
                                      CauseObjectNumber = reses.Key.CauseObjectNumber,
                                      BatchType = reses.Key.BatchType,
                                      FailedReason = reses.Key.FailedReason,
                                      PeriodId = reses.Key.PeriodId,
                                      AlertType = reses.Key.AlertType,
                                      CustomerNum = reses.Key.CustomerNum,
                                      CustomerName = reses.Key.CustomerName,
                                      BillGroupCode = reses.Key.BillGroupCode,
                                      BillGroupName = reses.Key.BillGroupName,
                                      Class = reses.Key.Class,
                                      Risk = reses.Key.Risk,
                                      CreditLimit = reses.Sum(age => age.CreditLimit),
                                      TotalAmt = reses.Sum(age => age.TotalAmt),
                                      CurrentAmt = reses.Sum(age => age.CurrentAmt),
                                      FDueOver90Amt = reses.Sum(age => age.FDueOver90Amt),
                                      PastDueAmt = reses.Sum(age => age.PastDueAmt),
                                      Operator = reses.Key.Operator,
                                      IsHoldFlg = reses.Key.IsHoldFlg,
                                      LegalEntity = reses.Key.LegalEntity,
                                      LastRemind = reses.Key.LastRemind
                                  })
                         select new DunningReminderDto
                         {
                             Id = final.Id,
                             ActionDate = final.ActionDate,
                             Deal = final.Deal,
                             TaskId = final.TaskId,
                             ReferenceNo = final.ReferenceNo,
                             ProcessId = final.ProcessId,
                             DunStatus = final.DunStatus,
                             CauseObjectNumber = final.CauseObjectNumber,
                             BatchType = final.BatchType,
                             FailedReason = final.FailedReason,
                             PeriodId = final.PeriodId,
                             AlertType = final.AlertType,
                             CustomerNum = final.CustomerNum,
                             CustomerName = final.CustomerName,
                             BillGroupCode = string.IsNullOrEmpty(final.BillGroupCode) == true ? final.CustomerName : final.BillGroupCode,
                             BillGroupName = string.IsNullOrEmpty(final.BillGroupName) == true ? final.CustomerName : final.BillGroupName,
                             CreditLimit = final.CreditLimit,
                             TotalAmt = final.TotalAmt,
                             CurrentAmt = final.CurrentAmt,
                             FDueOver90Amt = final.FDueOver90Amt,
                             PastDueAmt = final.PastDueAmt,
                             Class = final.Class,
                             Risk = final.Risk,
                             Operator = final.Operator,
                             IsHoldFlg = final.IsHoldFlg,
                             LegalEntity = final.LegalEntity,
                             LastRemind = final.LastRemind
                         });
            #endregion
                return r;
            }
            else 
            {
                return GetNoPaging("finish");
            }
        }

        public IQueryable<DunningReminderConfig> GetDunningConfig(string custNum,string siteUseId) 
        {          
            var config = CommonRep.GetQueryable<DunningReminderConfig>().Where(o => o.CustomerNum == custNum && o.Deal == AppContext.Current.User.Deal 
            && o.SiteUseId == siteUseId).AsQueryable();
            return config;         
        }
        public void SaveCustConfig(DunningReminderConfig config)
        {
            try
            {
                string custNum = config.CustomerNum;
                string siteUseId = config.SiteUseId;
                string legal = config.LegalEntity;
                var dunning=CommonRep.GetQueryable<DunningReminderConfig>().Where(o => o.CustomerNum == custNum && o.Deal == AppContext.Current.User.Deal 
                && o.LegalEntity == legal && o.SiteUseId == siteUseId).FirstOrDefault();
                if (dunning==null)
                {
                    config.Deal = AppContext.Current.User.Deal.ToString();
                    CommonRep.Add(config);
                }else{
                    ObjectHelper.CopyObjectWithUnNeed(config, dunning, new string[] { "Id", "Deal", "CustomerNum", "VRClass" });
                }

                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void SaveConfigBySingle(int AlertId, List<string> list)
        {
            var Alert = CommonRep.FindBy<CollectorAlert>(AlertId);
            var config = CommonRep.GetDbSet<DunningReminderConfig>()
                .Where(o => o.Deal == Alert.Deal && o.CustomerNum == Alert.CustomerNum && o.LegalEntity == Alert.LegalEntity).FirstOrDefault();
            if (config == null)
            {
                DunningReminderConfig newconfig = new DunningReminderConfig();
                newconfig.Deal = Alert.Deal;
                newconfig.CustomerNum = Alert.CustomerNum;
                newconfig.LegalEntity = Alert.LegalEntity;
                newconfig.FirstInterval = Convert.ToInt32(list[0]);
                newconfig.SecondInterval = Convert.ToInt32(list[1]);
                newconfig.PaymentTAT = Convert.ToInt32(list[2]);
                newconfig.RiskInterval = Convert.ToInt32(list[3]);
                newconfig.Description = list[4];
                newconfig.VRClass = "";
                CommonRep.Add(newconfig);
            }
            else
            {
                config.FirstInterval = Convert.ToInt32(list[0]);
                config.SecondInterval = Convert.ToInt32(list[1]);
                config.PaymentTAT = Convert.ToInt32(list[2]);
                config.RiskInterval = Convert.ToInt32(list[3]);
                config.Description = list[4];
            }
            CommonRep.Commit();

        }


        public void SaveActionDate(int AlertId, string Date)
        {
            var alert = CommonRep.FindBy<CollectorAlert>(AlertId);
            DateTime ActionDate = Convert.ToDateTime(Date);
            alert.ActionDate = ActionDate;
            CommonRep.Commit();
        }

        public CurrentTracking Calcu(int AlertId)
        {
            var Alert = CommonRep.FindBy<CollectorAlert>(AlertId);
            List<string> list = new List<string>();
            if (Alert.AlertType == 1)
            {
                if (Alert.BatchType == 1)
                {
                    list.Add(Alert.CustomerNum);
                    insertDunningReminder(list, "1", "1");
                }
                else
                {
                    list.Add(Alert.TaskId);
                    insertDunningReminder(list, "2", "1");
                }
            }
            else {
                list.Add(Alert.TaskId);
                insertDunningReminder(list, "2", Alert.AlertType.ToString());
            }

            return GetCT(AlertId);
        }

        public MailTmp GetSecondReminderMailInstance(string customerNums,string siteUseIds, string totalInvoiceAmount, string finalReminderDay, List<int> intIds, int templateId = 0)
        {
            MailTmp res = null;
            string attachment = "";

            // 2, retrieve template based on customer information and hint.
            IMailService ms = SpringFactory.GetObjectImpl<IMailService>("MailService");
            MailTemplate tpl = null;
            if (templateId > 0)
            {
                tpl = ms.GetMailTemplateById(templateId);
            }
            else
            {
                tpl = ms.GetMailTemplate(Helper.EnumToCode<MailTemplateType>(MailTemplateType.First_Time_Dispute));
            }

            if (tpl != null)
            {
                res = ms.GetInstanceFromTemplate(tpl, (parser) =>
                {
                    // 1, contactNames used in SOA template
                    ContactService cs = SpringFactory.GetObjectImpl<ContactService>("ContactService");
                    IList<Contactor> contactors = cs.GetContactsByCustomers(customerNums, siteUseIds);
                    string customer = cs.customerNameGet(customerNums);
                    List<string> cns = new List<string>();
                    string contactNames = string.Empty;
                    foreach (Contactor cont in contactors)
                    {
                        if (cont.ToCc == "1")
                        {
                            if (!cns.Contains(cont.Name))
                            {
                                cns.Add(cont.Name);
                                contactNames += (cont.Name + ", ");
                            }
                        }
                    }
                    contactNames = contactNames.TrimEnd(',');

                    System.Data.DataTable[] reportItemList;

                    //生成附件并取得附近名和币种的合计值
                    InvoiceService invServ = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
                    string[] attachPathList = invServ.setContent(intIds, "", out reportItemList, customerNums, null,null).ToArray();
                    attachment = string.Join(",", attachPathList);

                    string reportStr = "";
                    foreach (System.Data.DataTable dt in reportItemList) {
                        reportStr += invServ.GetHTMLTableByDataTable(dt) + "<br>";
                    }
                    

                    parser.RegistContext("attachmentInfo", reportStr);

                    parser.RegistContext("CustomerName", customer);
                    //=========added by alex body中显示附件名+Currency=====================
                    // 2, Total invoice amount
                    parser.RegistContext("totalInvoiceAmount", totalInvoiceAmount);
                    // 3, collector
                    parser.RegistContext("collector", AppContext.Current.User);
                });

                //added by alex body中显示附件名+Currency
                //附件的id
                res.Attachment = attachment;
            }
            else
            {
                Exception ex = new OTCServiceException("No matching template was found!", System.Net.HttpStatusCode.NotFound);
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

            return res;
        }

        public MailTmp GetFinalReminderMailInstance(string customerNums,string siteUseIds, string totalInvoiceAmount, string holdDay, List<int> intIds, int templateId = 0)
        {
            MailTmp res = null;
            string attachment = "";

            // 2, retrieve template based on customer information and hint.
            IMailService ms = SpringFactory.GetObjectImpl<IMailService>("MailService");
            MailTemplate tpl = null;
            if (templateId > 0)
            {
                tpl = ms.GetMailTemplateById(templateId);
            }
            else
            {
                tpl = ms.GetMailTemplate(Helper.EnumToCode<MailTemplateType>(MailTemplateType.Second_Time_Dispute));
            }

            if (tpl != null)
            {
                res = ms.GetInstanceFromTemplate(tpl, (parser) =>
                {
                    // 1, contactNames used in SOA template
                    ContactService cs = SpringFactory.GetObjectImpl<ContactService>("ContactService");
                    IList<Contactor> contactors = cs.GetContactsByCustomers(customerNums, siteUseIds);
                    List<string> cns = new List<string>();
                    string contactNames = string.Empty;
                    foreach (Contactor cont in contactors)
                    {
                        if (cont.ToCc == "1")
                        {
                            if (!cns.Contains(cont.Name))
                            {
                                cns.Add(cont.Name);
                                contactNames += (cont.Name + ", ");
                            }
                        }
                    }
                    contactNames = contactNames.TrimEnd(',');

                    System.Data.DataTable[] reportItemList; 
                    //生成附件并取得附近名和币种的合计值
                    InvoiceService invServ = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
                    string[] attachPathList = invServ.setContent(intIds, "3", out reportItemList, customerNums, null,null).ToArray();
                    attachment = string.Join(",", attachPathList);

                    string reportStr = "";
                    foreach (System.Data.DataTable dt in reportItemList)
                    {
                        reportStr += invServ.GetHTMLTableByDataTable(dt) + "<br>";
                    }

                    parser.RegistContext("attachmentInfo", reportStr);

                    parser.RegistContext("contactNames", contactNames);

                    // 2, hold date
                    DateTime hd = DateTime.Parse(holdDay);
                    parser.RegistContext("holdDay", hd);

                    int daysBeforeFinal = (hd-AppContext.Current.User.Now.Date).Days;
                    if (daysBeforeFinal <= 0)
                    {
                        daysBeforeFinal = 1;
                    }

                    parser.RegistContext("daysBeforeFinal", daysBeforeFinal);

                    // 3, collector
                    parser.RegistContext("collector", AppContext.Current.User);
                });

                //added by alex body中显示附件名+Currency
                //附件的id
                res.Attachment = attachment;
            }
            else
            {
                throw new OTCServiceException("No matching template was found!", System.Net.HttpStatusCode.NotFound);
            }

            return res;

        }

        public int CheckPermission(string ColDun)
        {
            int Check = 0;
            string CurrentUser = AppContext.Current.User.EID.ToString();
            string[] cusGroup = ColDun.Split(',');
            List<string> collectors = new List<string>();
            collectors = CommonRep.GetDbSet<CustomerTeam>()
                .Where(o => o.Deal == AppContext.Current.User.Deal && cusGroup.Contains(o.CustomerNum))
                .Select(o => o.Collector).ToList();
            foreach (var item in collectors)
            {
                if (item != CurrentUser)
                {
                    Check = 1;
                }
            }
            return Check;
        }

    }
}
