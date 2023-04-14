using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Proxy.Workflow;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Transactions;

namespace Intelligent.OTC.Business
{
    public class PeroidService
    {
        public OTCRepository CommonRep { get; set; }
        public DateTime CurrentTime
        {
            get
            {
                return AppContext.Current.User.Now;
            }
        }
        /// <summary>
        /// Get All Peroids from Db order by PeriodEnd Desc
        /// </summary>
        /// <returns></returns>
        public List<PeriodControl> GetAllPeroids()
        {
            return CommonRep.GetDbSet<PeriodControl>().Where(o => o.Deal == AppContext.Current.User.Deal&&o.SoaFlg=="1").OrderByDescending(o => o.PeriodEnd).ToList();
        }

        public List<PeroidReport> GetAllPeroidReports()
        {
            int iCount;
            UploadInfo upInfo;
            List<PeroidReport> rtnList = new List<PeroidReport>();
            iCount = 0;
            int iOneYears;
            string strDeal = AppContext.Current.User.Deal.ToString();
            List<PeriodControl> peroids = new List<PeriodControl>();

            List<UploadInfo> upInfos = new List<UploadInfo>();

            List<FileUploadHistory> fileHis = new List<FileUploadHistory>();

            List<Sites> sites = new List<Sites>();

            peroids = GetAllPeroids().Where(o => o.Deal == strDeal).ToList();

            getListInfo(strDeal, out fileHis, out sites);

            foreach (PeriodControl per in peroids)
            {
                iCount++;
                PeroidReport perReport = new PeroidReport();
                ObjectHelper.CopyObject(per, perReport);
                perReport.sortId = iCount;
                upInfos = getCurrentPeroidUploadTimes(out iOneYears, fileHis, sites, per);
                if (iOneYears == 0)
                {
                    perReport.oneYearsFlg = "0";
                }
                else
                {
                    perReport.oneYearsFlg = "1";
                }
                upInfo = upInfos.Where(o => o.InvTimes == 0 || o.AccTimes == 0).FirstOrDefault();
                if (upInfo != null)
                {
                    perReport.agingRepotFlg = "0";
                }
                else
                {
                    perReport.agingRepotFlg = "1";
                }

                if (perReport.PeriodBegin <= CurrentTime &&
                    perReport.PeriodEnd >= CurrentTime)
                {
                    perReport.statusFlg = PeriodStatus.Running.ToString();
                    if (perReport.oneYearsFlg == "0" || perReport.agingRepotFlg == "0" || per.SoaFlg == "1")
                    {
                        perReport.soaDoneFlg = "0";
                    }
                    else
                    {
                        perReport.soaDoneFlg = "1";
                    }
                }
                else
                {
                    perReport.statusFlg = PeriodStatus.Close.ToString();
                    perReport.soaDoneFlg = "0";
                }
                perReport.soaDone = getSoaStatus(perReport);
                rtnList.Add(perReport);
            }

            return rtnList;
        }

        public void getListInfo(string strDeal, out List<FileUploadHistory> fileHis, out List<Sites> sites)
        {
            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            string strSubmitted = Helper.EnumToCode<UploadStates>(UploadStates.Submitted).ToString();

            fileHis = fileService.GetFileUploadHistory()
                                            .Where(o => o.Deal == strDeal &&
                                            o.SubmitFlag == strSubmitted)
                                            .Select(o => o).ToList();

            SiteService siteService = SpringFactory.GetObjectImpl<SiteService>("SiteService");
            sites = siteService.GetAllSites().Where(o => o.Deal == strDeal).ToList();

        }

        private string getSoaStatus(PeroidReport per)
        {
            string rtnSts = string.Empty;
            if (per.SoaFlg == "1")
            {
                rtnSts = "SOA Task Started";
            }
            else
            {
                if (per.statusFlg == PeriodStatus.Running.ToString())
                {
                    rtnSts = "SOA Task Start";
                }
                else
                {
                    rtnSts = "SOA Task Not Started";
                }
            }

            return rtnSts;
        }

        public List<UploadInfo> getCurrentPeroidDateSize(string type)
        {
            List<UploadInfo> rtnInfos = new List<UploadInfo>();
            UploadInfo rtnInfo = new UploadInfo();
            PeriodControl currentPer = new PeriodControl();
            string strDeal = AppContext.Current.User.Deal.ToString();

            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");

            List<FileUploadHistory> filehis = fileService.GetFileUploadHistory().Where(o => o.Deal == AppContext.Current.User.Deal).ToList();

            currentPer = getcurrentPeroid();

            if (type == "Aging")
            {
                List<Sites> sites = new List<Sites>();

                SiteService siteService = SpringFactory.GetObjectImpl<SiteService>("SiteService");
                sites = siteService.GetAllSites().Where(o => o.Deal == strDeal).ToList();

                string currentType;
                currentType = null;
                foreach (Sites site in sites)
                {
                    rtnInfo = new UploadInfo();

                    rtnInfo.LegalEntity = site.LegalEntity;
                    rtnInfo.AccTimes = 0;
                    rtnInfo.InvTimes = 0;
                    rtnInfos.Add(rtnInfo);
                }

                if (currentPer != null)
                {
                    rtnInfo = new UploadInfo();
                    filehis = filehis.Where(o => o.UploadTime >= currentPer.PeriodBegin &&
                                                o.UploadTime <= currentPer.PeriodEnd
                                                && o.FileType != Helper.EnumToCode<FileType>(FileType.OneYearSales)
                                                && o.ProcessFlag == Helper.EnumToCode<UploadStates>(UploadStates.Success))
                                                .Select(o => o)
                                                .OrderBy(o => o.LegalEntity)
                                                .ThenBy(o => o.FileType)
                                                .ThenByDescending(o => o.UploadTime).ToList();

                    foreach (FileUploadHistory his in filehis)
                    {
                        if (currentType != his.LegalEntity + his.FileType)
                        {
                            currentType = his.LegalEntity + his.FileType;
                            rtnInfo = rtnInfos.FindAll(o => o.LegalEntity == his.LegalEntity).FirstOrDefault();
                            if (rtnInfo != null)
                            {
                                switch (Helper.CodeToEnum<FileType>(his.FileType))
                                {
                                    case FileType.Account:
                                        rtnInfo.AccTimes = his.DataSize;
                                        if (his.ReportTime != null)
                                        {
                                            rtnInfo.ReportTime = his.ReportTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                                        }
                                        break;
                                    case FileType.Invoice:
                                        rtnInfo.InvTimes = his.DataSize.Value;
                                        break;
                                }

                            }
                        }

                    }
                }
            }
            else
            {
                rtnInfo = new UploadInfo();
                rtnInfo.OneYearTimes = 0;
                rtnInfo.ReportTime = "";
                rtnInfos.Add(rtnInfo);

                if (currentPer != null)
                {
                    filehis = filehis.Where(o => o.UploadTime >= currentPer.PeriodBegin &&
                                                o.UploadTime <= currentPer.PeriodEnd
                                                && o.FileType == Helper.EnumToCode<FileType>(FileType.OneYearSales)
                                                && o.ProcessFlag == Helper.EnumToCode<UploadStates>(UploadStates.Success))
                                                .Select(o => o)
                                                .OrderByDescending(o => o.UploadTime).ToList();

                    if (filehis.Count > 0)
                    {
                        rtnInfo = new UploadInfo();
                        rtnInfo.OneYearTimes = filehis.First().DataSize.Value;
                        rtnInfo.ReportTime = filehis.First().UploadTime.ToString("yyyy-MM-dd HH:mm:ss");
                        rtnInfos.Clear();
                        rtnInfos.Add(rtnInfo);
                    }
                }

            }

            return rtnInfos;
        }
        public string getcurrentPer()
        {
            string rtn = "";
            PeriodControl per = getcurrentPeroid();

            if (per != null)
            {
                if (per.IsCurrentFlg != "0")
                {
                    rtn = per.PeriodBegin.ToString("yyyy-MM-dd HH:mm:ss") + " to "
                            + per.PeriodEnd.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            return rtn;
        }
        public int? getIdOfcurrentPeroid()
        {
            int? rtn;
            PeriodControl per = getcurrentPeroid();
            if (per == null)
            {
                rtn = 0;
            }
            else if (per.IsCurrentFlg == "1")
            {
                rtn = per.Id;
            }
            else
            {
                rtn = 0;
            }
            return rtn;
        }
        public PeriodControl getcurrentPeroid()
        {
            string strDeal = AppContext.Current.User.Deal.ToString();
            //string strDeal = AppContext.Current.Deal.ToString();

            //edit by pxc :if currentperiod is null ,get the lasted one 
            PeriodControl currentPeroid = new PeriodControl();
            currentPeroid = GetAllPeroids().Where(o => o.Deal == strDeal && o.PeriodBegin <= CurrentTime
                                    && o.PeriodEnd >= CurrentTime && o.SoaFlg == "1").Select(o => o).FirstOrDefault();
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

        public List<UploadInfo> getCurrentPeroidUploadTimes(int id)
        {
            List<UploadInfo> rtns = new List<UploadInfo>();
            int iC;
            string strDeal = AppContext.Current.User.Deal.ToString();
            List<FileUploadHistory> fileHis = new List<FileUploadHistory>();
            List<Sites> sites = new List<Sites>();
            PeriodControl per = GetAllPeroids().Where(o => o.Id == id).FirstOrDefault();

            getListInfo(strDeal, out fileHis, out sites);

            rtns = getCurrentPeroidUploadTimes(out iC, fileHis, sites, per);

            foreach (UploadInfo rtn in rtns)
            {
                rtn.AccTimes = rtn.AccTimes > 0 ? 1 : 0;
                rtn.InvTimes = rtn.AccTimes > 0 ? 1 : 0;
            }

            return rtns;
        }

        public List<UploadInfo> getCurrentPeroidUploadTimes(out int iOneYears
                        , List<FileUploadHistory> fileHis, List<Sites> sites
                        , PeriodControl per = null)
        {
            List<UploadInfo> upInfos = new List<UploadInfo>();

            UploadInfo upInfoOld;
            UploadInfo upInfoNew;

            PeriodControl currentPer = new PeriodControl();

            iOneYears = 0;

            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");

            string strSubmitted = Helper.EnumToCode<UploadStates>(UploadStates.Submitted).ToString();

            if (per != null)
            {
                currentPer = per;
            }
            else
            {
                currentPer = getcurrentPeroid();
            }

            if (currentPer != null)
            {
                if (currentPer.IsCurrentFlg != "0")
                {
                    fileHis = fileHis.Where(o => o.SubmitTime >= currentPer.PeriodBegin &&
                                                o.SubmitTime <= currentPer.PeriodEnd)
                                                .Select(o => o)
                                                .OrderByDescending(o => o.SubmitTime).ToList();
                    if (fileHis.Count > 0)
                    {
                        foreach (FileUploadHistory his in fileHis)
                        {
                            upInfoOld = new UploadInfo();
                            upInfoNew = new UploadInfo();
                            upInfoOld = upInfos.Where(o => o.LegalEntity == his.LegalEntity).FirstOrDefault();
                            if (upInfoOld == null)
                            {
                                upInfoNew.LegalEntity = his.LegalEntity;
                                upInfoNew.Operator = his.Operator;
                                if (his.ReportTime != null)
                                {
                                    upInfoNew.ReportTime = his.ReportTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                                }
                                upInfoNew.AccTimes = 0;
                                upInfoNew.InvTimes = 0;
                                upInfoNew.OneYearTimes = 0;
                            }
                            else
                            {
                                ObjectHelper.CopyObject(upInfoOld, upInfoNew);
                                if (upInfoNew.ReportTime == null && his.ReportTime != null)
                                {
                                    upInfoNew.ReportTime = his.ReportTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                                }
                                upInfos.Remove(upInfoOld);
                            }
                            switch (Helper.CodeToEnum<FileType>(his.FileType))
                            {
                                case FileType.Account:
                                    upInfoNew.AccTimes++;
                                    break;
                                case FileType.Invoice:
                                    upInfoNew.InvTimes++;
                                    break;
                                case FileType.OneYearSales:
                                    iOneYears++;
                                    continue;
                            }
                            upInfos.Add(upInfoNew);
                        }
                    }

                }

            }

            foreach (Sites site in sites)
            {
                upInfoOld = new UploadInfo();
                upInfoOld = upInfos.Where(o => o.LegalEntity == site.LegalEntity).FirstOrDefault();
                if (upInfoOld == null)
                {
                    upInfoNew = new UploadInfo();
                    upInfoNew.LegalEntity = site.LegalEntity;
                    upInfoNew.AccTimes = 0;
                    upInfoNew.InvTimes = 0;
                    upInfoNew.OneYearTimes = 0;
                    upInfos.Add(upInfoNew);
                }
            }

            return upInfos.OrderByDescending(o => o.AccTimes).ToList();
        }

        public void newperidAdd(string strEndDate)
        {
            DateTime dt = new DateTime();
            DateTime dtNow = new DateTime();
            DateTime dtPayment = new DateTime();
            dtNow = CurrentTime.Date;
            dt = Convert.ToDateTime(strEndDate + " 23:59:59");

            PeriodControl currentPer = new PeriodControl();

            try
            {
                var period = GetAllPeroids().Where(o => o.SoaFlg == "1").FirstOrDefault();
                if (period != null)
                {
                    PeriodControl old = CommonRep.FindBy<PeriodControl>(period.Id);
                    PeriodControl cp = new PeriodControl();
                    cp = old;
                    cp.SoaFlg = "0";
                    cp.EndDate = CurrentTime;
                    ObjectHelper.CopyObjectWithUnNeed(cp, old, new string[] { "Id", "Deal", "PeriodBegin", "PeriodEnd", "Operator", "Operatedate", "Description" });
                }



                currentPer = getcurrentPeroid();
                if (currentPer != null)
                {
                    if (currentPer.IsCurrentFlg == "1")
                    {
                        currentPer.PeriodEnd = dtNow;
                        currentPer.Description = "new period open ，auto close ：" + dtNow.ToShortDateString();
                    }

                }

                currentPer = new PeriodControl();
                currentPer.Deal = AppContext.Current.User.Deal.ToString();
                currentPer.PeriodBegin = dtNow;
                currentPer.EndDate = dt;
                currentPer.PeriodEnd = dt;
                currentPer.Operator = AppContext.Current.User.EID.ToString();
                currentPer.Operatedate = CurrentTime;
                currentPer.SoaFlg = "1";
                CommonRep.Add(currentPer);
                CommonRep.Commit();

                cancelAllSOATask(currentPer);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new Exception(ex.Message);
            }

        }
        public string AddOrUpdatePeriod(int id, string strStartDate, string strEndDate)
        {
            string result = "";
            if (CommonRep.GetDbSet<PeriodControl>().Any(s => s.Id != id&&s.SoaFlg=="1" && (
            (SqlFunctions.DateDiff("day", s.PeriodBegin, strStartDate) >= 0 && SqlFunctions.DateDiff("day", strStartDate, s.PeriodEnd) >= 0)
             || (SqlFunctions.DateDiff("day", s.PeriodBegin, strEndDate) >= 0 && SqlFunctions.DateDiff("day", strEndDate, s.PeriodEnd) >= 0))))
            {
                return "Date error";
            }
            if (id == 0)//new
            {
                PeriodControl pc = new PeriodControl();
                pc.Deal = AppContext.Current.User.Deal;
                pc.Operatedate = DateTime.Now;
                pc.Operator = AppContext.Current.User.EID;
                pc.SoaFlg = "1";
                pc.EndDate= DateTime.Parse(strEndDate + " 23:59:59");
                pc.PeriodBegin = DateTime.Parse(strStartDate);
                pc.PeriodEnd = DateTime.Parse(strEndDate + " 23:59:59");
                CommonRep.GetDbSet<PeriodControl>().Add(pc);
                CommonRep.Commit();
                result = "Success!";
            }
            else//update
            {
              var pc=  CommonRep.GetDbSet<PeriodControl>().Where(s => s.Id == id).FirstOrDefault();
                if (pc == null)
                {
                    return "The period does not exist";
                }
                pc.Operatedate = DateTime.Now;
                pc.Operator= AppContext.Current.User.EID;
                pc.EndDate= DateTime.Parse(strEndDate + " 23:59:59");
                pc.PeriodBegin= DateTime.Parse(strStartDate);
                pc.PeriodEnd = DateTime.Parse(strEndDate + " 23:59:59");
                CommonRep.Commit();
                result = "Success!";
            }
            return result;
        }

        public string DeletePeriod(int id)
        {
            var pc = CommonRep.GetDbSet<PeriodControl>().Where(s => s.Id == id).FirstOrDefault();
            if(pc==null)
            {
                return "The period does not exist";
            }
            pc.Operatedate = DateTime.Now;
            pc.Operator = AppContext.Current.User.EID;
            pc.SoaFlg = "0";
            CommonRep.Commit();
           var result = "Success!";
            return result;
        }

        /// <summary>
        /// Start SOA Task
        /// </summary>
        private void startSOAWorkflowTask()
        {
            // 1, start SOA workflow for each aging records
            PeriodControl currP = getcurrentPeroid();

            var flsIds = (from f in CommonRep.GetDbSet<FileUploadHistory>()
                          where f.SubmitTime >= currP.PeriodBegin && f.SubmitTime <= currP.PeriodEnd
                          && f.FileType == Helper.EnumToCode(FileType.Account)
                          select f.ImportId).ToList();

            if (flsIds != null && flsIds.Count > 0)
            {
                // get aging records
                var allaging = (from aging in CommonRep.GetDbSet<CustomerAging>()
                                where flsIds.Contains(aging.ImportId)
                                select aging).ToList();

                string wfEndpoint = ConfigurationManager.AppSettings["WorkflowEndPoint"] as string;
                if (string.IsNullOrEmpty(wfEndpoint))
                {
                    Exception ex = new OTCServiceException("No endpoint was configed for Xccelerator workflow web api!");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }

                WorkflowClient wf = new WorkflowClient(wfEndpoint);
            }

            Helper.Log.Info("All workflow task has been queued.");
        }

        private void cancelAllSOATask(PeriodControl closingPeriod)
        {
            // close uncompleted SOA, reminder flow task.

        }


        public void StartSOATask()
        {
            // 1, Risk, Value class calculation
            string strDeal = AppContext.Current.User.Deal;
           
            RiskService rs = SpringFactory.GetObjectImpl<RiskService>("RiskService");
            Helper.Log.Info("Risk calculation Start.");
            rs.GetRiskValueNoPa();

            Helper.Log.Info("Risk calculation completed.");

            CustomerService custService = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            Helper.Log.Info("Value Start.");
            custService.GetValue();

            Helper.Log.Info("Value completed.");
            custService.ArBalanceAmtPeroidSet();
        
            //3.insert into collector_alert by pxc
            CollectorAlert colalert = new CollectorAlert();
            IWorkflowService wfservice = SpringFactory.GetObjectImpl<IWorkflowService>("WorkflowService");
            string oper = AppContext.Current.User.Id.ToString();
            PeriodControl currentPc = getcurrentPeroid();
            string IsWF = ConfigurationManager.AppSettings["IsWF"].ToString();
            //get exist alert
            List<CollectorAlert> colalertList = CommonRep.GetQueryable<CollectorAlert>()
                .Where(o => o.Status != "Finish" && o.Status != "Cancelled" && o.Status != "Initialized" && o.Deal == AppContext.Current.User.Deal).ToList();
        
            if (IsWF == "true")
            {
                foreach (var al in colalertList)
                {
                    wfservice.CancelTask("4", al.CauseObjectNumber, oper, al.TaskId);
                }
            }
          
            var InsertAlertSql = string.Format(@"
                    INSERT INTO T_COLLECTOR_ALERT
                        (EID,DEAL,CUSTOMER_NUM,ACTION_DATE,CREATE_DATE,REFERENCE_NO,ALERT_TYPE,
                        STATUS,TASKID,PROCESSID,PERIOD_ID,BATCH_TYPE,FAILEDREASON,CAUSEOBJECTNUMBER)
                    SELECT COLLECTOR,DEAL,CUSTOMER_NUM,'{0}','{0}','','1',
                            'Initialized','','','{1}',BATCHTYPE,'',''
                    FROM V_STARTSOA_ALERT ;
                ", AppContext.Current.User.Now
                 , currentPc.Id);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
            {
                string UpdateSql;
                UpdateSql = "update T_COLLECTOR_ALERT SET STATUS = 'Cancelled' WHERE STATUS <> 'Finish' AND STATUS <> 'Cancelled' AND DEAL = '" + AppContext.Current.User.Deal + "';";
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(UpdateSql);
                Helper.Log.Info("collector_alert UPDATE COMPLETED.");
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(InsertAlertSql);
                Helper.Log.Info("collector_alert INSERT COMPLETED.");

                scope.Complete();
            }

            // 2,SOA Flg Update
            ChangeSOAFlg();
            Helper.Log.Info("ChangeSOAFlg completed.");
        }

        private void ChangeSOAFlg()
        {
            PeriodControl currentPer = new PeriodControl();
            currentPer = getcurrentPeroid();
            currentPer.SoaFlg = "1";
            CommonRep.Commit();
        }
    }
}
