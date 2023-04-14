using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Domain.Repositories;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain;
using Intelligent.OTC.Common;
using System.Data.Entity.Validation;
using Intelligent.OTC.Common.UnitOfWork;
using System.Net;
using System.IO;
using Google.Apis.Auth.OAuth2.Responses;
using Newtonsoft.Json;
using System.Configuration;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.Proxy;
using System.Data.SqlClient;
using System.Web;

namespace Intelligent.OTC.Business
{
    public class BaseDataService : IBaseDataService
    {
        public OTCRepository CommonRep { private get; set; }
        public string[] DataBaseInfo;
        public ICacheService CacheSvr { private get; set; }

        /// <summary>
        /// Get all system type details from cache
        /// </summary>
        /// <returns></returns>
        public List<SysTypeDetail> GetAllSysTypeDetail()
        {
            return CacheSvr.GetOrSet<List<SysTypeDetail>>("Cache_SysTypeDetail", () =>
            {
                HashSet<string> processed = new HashSet<string>();
                try
                {
                    List<SysTypeDetail> res = CommonRep.GetDbSet<SysTypeDetail>().OrderBy(td => td.Seq).ToList();
                    return res;
                }
                catch (Exception ex)
                {
                    return new List<SysTypeDetail>();
                }
              
            });
        }

        public List<SysTypeDetail> GetSysTypeDetail(string strTypecode)
        {
            var Result = GetAllSysTypeDetail().Where(d=>d.TypeCode == strTypecode).ToList();
            return Result;
        }

        public Dictionary<string, List<SysTypeDetail>> GetSysTypeDetails(string strTypeCodes)
        {
            AssertUtils.ArgumentHasText(strTypeCodes, "Type Codes");
            List<string> tcs = strTypeCodes.Split(',').ToList();
            Dictionary<string, List<SysTypeDetail>> res = new Dictionary<string, List<SysTypeDetail>>();

            GetAllSysTypeDetail().ForEach(td =>
            {
                if (tcs.Contains(td.TypeCode))
                {
                    if (!res.ContainsKey(td.TypeCode))
                    {
                        res.Add(td.TypeCode, new List<SysTypeDetail>() { td });
                    }
                    else
                    {
                        res[td.TypeCode].Add(td);
                    }
                }
            });
            return res;
        }

        public List<SysConfig> GetAllSysConfigs()
        {
            return CacheSvr.GetOrSet<List<SysConfig>>("Cache_SysConfig", () =>
            {
                return CommonRep.GetDbSet<SysConfig>().ToList();
            });
        }

        public SysConfig GetSysConfigByCode(string code)
        {
            return GetAllSysConfigs().Find(cfg => cfg.CfgCode == code);
        }

        public void InitialUser(string userAuthCode, string userMail)
        {
            MailServiceProxy proxy = new MailServiceProxy(ConfigurationManager.AppSettings["MailserviceEndPoint"]);
            proxy.RegistMailBox(userAuthCode, userMail);
        }

        public bool CheckAuthentication()
        {
            MailServiceProxy proxy = new MailServiceProxy(ConfigurationManager.AppSettings["MailserviceEndPoint"]);
            IMailService mailService = SpringFactory.GetObjectImpl<IMailService>("MailService");
            string nMailAddress = mailService.GetSenderMailAddress();
            return proxy.CheckMailBoxInitialized(nMailAddress);
        }

        public void SaveCollectionCalendarConfig(string customerNum, string legalEntity, List<string> calendars)
        {
            var config = CommonRep.GetDbSet<DunningReminderConfig>()
                .Where(o => o.Deal == AppContext.Current.User.Deal && o.CustomerNum == customerNum && o.LegalEntity == legalEntity).FirstOrDefault();
            if (config == null)
            {
                DunningReminderConfig newconfig = new DunningReminderConfig();
                newconfig.Deal = AppContext.Current.User.Deal;
                newconfig.CustomerNum = customerNum;
                newconfig.LegalEntity = legalEntity;
                newconfig.FirstInterval = Convert.ToInt32(calendars[0]);
                newconfig.SecondInterval = Convert.ToInt32(calendars[1]);
                newconfig.PaymentTAT = Convert.ToInt32(calendars[2]);
                newconfig.RiskInterval = Convert.ToInt32(calendars[3]);
                newconfig.Description = calendars[4];
                newconfig.VRClass = "";
                CommonRep.Add(newconfig);
            }
            else
            {
                config.FirstInterval = Convert.ToInt32(calendars[0]);
                config.SecondInterval = Convert.ToInt32(calendars[1]);
                config.PaymentTAT = Convert.ToInt32(calendars[2]);
                config.RiskInterval = Convert.ToInt32(calendars[3]);
                config.Description = calendars[4];
            }
            CommonRep.Commit();
        }


        public CurrentTracking AppendTrackingConfig(CurrentTracking tracking, string deal, string customerNum, string legalEntity)
        {
            //Close
            PeroidService ps = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            var CurPeriod = ps.getcurrentPeroid();
            tracking.CloseDate = CurPeriod.PeriodEnd;
            tracking.CloseStatus = 1;

            //DunningConfig
            var DunningConfig = CommonRep.GetDbSet<DunningReminderConfig>()
                .Where(o => o.Deal == deal && o.CustomerNum == customerNum && o.LegalEntity == legalEntity).FirstOrDefault();
            if (DunningConfig == null)
            {
                var Class = CommonRep.GetDbSet<CustomerLevelView>()
                    .Where(o => o.Deal == deal && o.CustomerNum == customerNum).FirstOrDefault();
                string VRClass = Class.ClassLevel + Class.RiskLevel;
                DunningConfig = CommonRep.GetDbSet<DunningReminderConfig>()
                    .Where(o => o.Deal == deal && o.VRClass == VRClass).FirstOrDefault();
            }

            return tracking;
        }

        public string CreateDailyReport()
        {
            var dateParam1 = new SqlParameter
            {
                ParameterName = "@para1",
                Value = 2
            };
            var dateParam2 = new SqlParameter
            {
                ParameterName = "@para1",
                Value = 3
            };
            List<DailyReportSoa> LDailySoa = CommonRep.GetDBContext().Database.SqlQuery<DailyReportSoa>
            ("P_DAILYREPORT_SOA").ToList<DailyReportSoa>();

            List<DailyReportSoa> LDailyReminder2 = CommonRep.GetDBContext().Database.SqlQuery<DailyReportSoa>
            ("P_DAILYREPORT_REMINDER @para1", dateParam1).ToList<DailyReportSoa>();

            List<DailyReportSoa> LDailyReminder3 = CommonRep.GetDBContext().Database.SqlQuery<DailyReportSoa>
            ("P_DAILYREPORT_REMINDER @para1", dateParam2).ToList<DailyReportSoa>();

            List<DailyReportAmt> LDailyAmt = CommonRep.GetDBContext().Database.SqlQuery<DailyReportAmt>
            ("P_DAILYREPORT_AMT").ToList<DailyReportAmt>();

            string strTmpPath;
            string strReportPath;
            string strReportName;
            string strTempPathKey;
            string strArchivePathKey;
            string strportPath;
            strTempPathKey = "TemplateDailyReportPath";
            strArchivePathKey = "DailyReportPath";
            strportPath = ConfigurationManager.AppSettings[strArchivePathKey].ToString();
            strReportPath = HttpContext.Current.Server.MapPath(strportPath).ToString();
            strTmpPath = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[strTempPathKey].ToString());
            if (Directory.Exists(strReportPath) == false)
            {
                Directory.CreateDirectory(strReportPath);
            }
            strReportName = "DailyCollectorReport_" + AppContext.Current.User.EID.ToString() + "_" + AppContext.Current.User.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            strReportPath += strReportName;
            NpoiHelper helper = new NpoiHelper(strTmpPath);
            helper.Save(strReportPath, true);
            helper = new NpoiHelper(strReportPath);
            string sheetName = "";
            sheetName = "Report";

            helper.ActiveSheetName = sheetName;
            int iRow;
            iRow = 2;
            DailyReportSoa re = new DailyReportSoa();
            foreach (DailyReportAmt amt in LDailyAmt)
            {
                //amt

                helper.SetData(iRow, 37, amt.PeriodAmt);
                helper.SetData(iRow, 38, amt.Soaamt);
                helper.SetData(iRow, 39, amt.SecondAmt);
                helper.SetData(iRow, 40, amt.ThirdAmt);
                helper.SetData(iRow, 41, amt.PTPAmt);
                helper.SetData(iRow, 42, amt.PayNoiceAmt);
                helper.SetData(iRow, 43, amt.DISAmt);
                helper.SetData(iRow, 44, amt.TotalAmt);
                helper.SetFormula(iRow, 45, "IF(AL" + (iRow + 1).ToString() + "=0,0,AM" + (iRow + 1).ToString() + "/AL" + (iRow + 1).ToString() + ")");
                helper.SetFormula(iRow, 46, "IF(AL" + (iRow + 1).ToString() + "=0,0,AP" + (iRow + 1).ToString() + "/AL" + (iRow + 1).ToString() + ")");
                helper.SetFormula(iRow, 47, "IF(AL" + (iRow + 1).ToString() + "=0,0,AQ" + (iRow + 1).ToString() + "/AL" + (iRow + 1).ToString() + ")");

                //SOA
                re = LDailySoa.Where(o => o.Eid == amt.Eid).FirstOrDefault();
                helper.SetData(iRow, 0, re.Eid);
                helper.SetData(iRow, 1, re.TeamName);
                helper.SetData(iRow, 2, re.TotalOpenAcc);
                helper.SetData(iRow, 3, re.WIP);
                helper.SetData(iRow, 4, re.Finish);
                helper.SetData(iRow, 5, re.normalCust);
                helper.SetData(iRow, 6, re.ClosedAcc);
                helper.SetData(iRow, 7, re.UNFinish);
                helper.SetData(iRow, 8, re.NOCON);
                helper.SetData(iRow, 9, re.NOCIAACC);
                helper.SetData(iRow, 10, re.MailAcc);
                helper.SetData(iRow, 11, re.taks);
                helper.SetData(iRow, 12, re.normaltaks);
                helper.SetData(iRow, 13, re.workTime);
                helper.SetData(iRow, 14, re.normalworkTime);
                helper.SetFormula(iRow, 15, "IF(F" + (iRow + 1).ToString() + "=0,0,O" + (iRow + 1).ToString() + "/F" + (iRow + 1).ToString() + "/60)");
                helper.SetFormula(iRow, 16, "IF(M" + (iRow + 1).ToString() + "=0,0,O" + (iRow + 1).ToString() + "/M" + (iRow + 1).ToString() + "/60)");
                helper.SetFormula(iRow, 17, "IF(C" + (iRow + 1).ToString() + "=0,0,E" + (iRow + 1).ToString() + "/C" + (iRow + 1).ToString() + ")");
                helper.SetFormula(iRow, 18, "IF(J" + (iRow + 1).ToString() + "=0,0,E" + (iRow + 1).ToString() + "/J" + (iRow + 1).ToString() + ")");

                //Remider2

                re = LDailyReminder2.Where(o => o.Eid == amt.Eid).FirstOrDefault();

                helper.SetData(iRow, 19, re.UNFinish);
                helper.SetData(iRow, 20, re.Finish);
                helper.SetData(iRow, 21, re.normalCust);
                helper.SetData(iRow, 22, re.taks);
                helper.SetData(iRow, 23, re.normaltaks);
                helper.SetData(iRow, 24, re.workTime);
                helper.SetData(iRow, 25, re.normalworkTime);
                helper.SetFormula(iRow, 26, "IF(V" + (iRow + 1).ToString() + "=0,0,Z" + (iRow + 1).ToString() + "/V" + (iRow + 1).ToString() + "/60)");
                helper.SetFormula(iRow, 27, "IF(X" + (iRow + 1).ToString() + "=0,0,Z" + (iRow + 1).ToString() + "/X" + (iRow + 1).ToString() + "/60)");

                //Remider3

                re = LDailyReminder3.Where(o => o.Eid == amt.Eid).FirstOrDefault();

                helper.SetData(iRow, 28, re.UNFinish);
                helper.SetData(iRow, 29, re.Finish);
                helper.SetData(iRow, 30, re.normalCust);
                helper.SetData(iRow, 31, re.taks);
                helper.SetData(iRow, 32, re.normaltaks);
                helper.SetData(iRow, 33, re.workTime);
                helper.SetData(iRow, 34, re.normalworkTime);
                helper.SetFormula(iRow, 35, "IF(AE" + (iRow + 1).ToString() + "=0,0,AI" + (iRow + 1).ToString() + "/AE" + (iRow + 1).ToString() + "/60)");
                helper.SetFormula(iRow, 36, "IF(AG" + (iRow + 1).ToString() + "=0,0,AI" + (iRow + 1).ToString() + "/AG" + (iRow + 1).ToString() + "/60)");

                if (iRow > 2)
                {
                    helper.CopyStyle(iRow, iRow - 1);
                }
                iRow++;
            }

            //Total
            helper.SetData(iRow, 1, "Total");
            helper.SetFormula(iRow, 2, "SUM(C3:C" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 3, "SUM(D3:D" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 4, "SUM(E3:E" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 5, "SUM(F3:F" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 6, "SUM(G3:G" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 7, "SUM(H3:H" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 8, "SUM(I3:I" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 9, "SUM(J3:J" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 10, "SUM(K3:K" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 11, "SUM(L3:L" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 12, "SUM(M3:M" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 13, "SUM(N3:N" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 14, "SUM(O3:O" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 15, "IF(F" + (iRow + 1).ToString() + "=0,0,O" + (iRow + 1).ToString() + "/F" + (iRow + 1).ToString() + "/60)");
            helper.SetFormula(iRow, 16, "IF(M" + (iRow + 1).ToString() + "=0,0,O" + (iRow + 1).ToString() + "/M" + (iRow + 1).ToString() + "/60)");
            helper.SetFormula(iRow, 17, "IF(C" + (iRow + 1).ToString() + "=0,0,E" + (iRow + 1).ToString() + "/C" + (iRow + 1).ToString() + ")");
            helper.SetFormula(iRow, 18, "IF(J" + (iRow + 1).ToString() + "=0,0,E" + (iRow + 1).ToString() + "/J" + (iRow + 1).ToString() + ")");

            helper.SetFormula(iRow, 19, "SUM(T3:T" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 20, "SUM(U3:U" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 21, "SUM(V3:V" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 22, "SUM(W3:W" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 23, "SUM(X3:X" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 24, "SUM(Y3:Y" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 25, "SUM(Z3:Z" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 26, "IF(V" + (iRow + 1).ToString() + "=0,0,Z" + (iRow + 1).ToString() + "/V" + (iRow + 1).ToString() + "/60)");
            helper.SetFormula(iRow, 27, "IF(X" + (iRow + 1).ToString() + "=0,0,Z" + (iRow + 1).ToString() + "/X" + (iRow + 1).ToString() + "/60)");

            helper.SetFormula(iRow, 28, "SUM(AC3:AC" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 29, "SUM(AD3:AD" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 30, "SUM(AE3:AE" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 31, "SUM(AF3:AF" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 32, "SUM(AG3:AG" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 33, "SUM(AH3:AH" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 34, "SUM(AI3:AI" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 35, "IF(AE" + (iRow + 1).ToString() + "=0,0,AI" + (iRow + 1).ToString() + "/AE" + (iRow + 1).ToString() + "/60)");
            helper.SetFormula(iRow, 36, "IF(AG" + (iRow + 1).ToString() + "=0,0,AI" + (iRow + 1).ToString() + "/AG" + (iRow + 1).ToString() + "/60)");

            helper.SetFormula(iRow, 37, "SUM(AL3:AL" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 38, "SUM(AM3:AM" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 39, "SUM(AN3:AN" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 40, "SUM(AO3:AO" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 41, "SUM(AP3:AP" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 42, "SUM(AQ3:AQ" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 43, "SUM(AR3:AR" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 44, "SUM(AS3:AS" + (iRow).ToString() + ")");
            helper.SetFormula(iRow, 45, "IF(AL" + (iRow + 1).ToString() + "=0,0,AM" + (iRow + 1).ToString() + "/AL" + (iRow + 1).ToString() + ")");
            helper.SetFormula(iRow, 46, "IF(AL" + (iRow + 1).ToString() + "=0,0,AP" + (iRow + 1).ToString() + "/AL" + (iRow + 1).ToString() + ")");
            helper.SetFormula(iRow, 47, "IF(AL" + (iRow + 1).ToString() + "=0,0,AQ" + (iRow + 1).ToString() + "/AL" + (iRow + 1).ToString() + ")");
            helper.CopyStyle(iRow, iRow - 1);

            helper.Save(strReportPath, true);

            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }
            strportPath = appUriBuilder.ToString() + strportPath.Trim('~') + strReportName;

            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            fileService.DailyReportInsert(strReportName, strportPath, UploadStates.Success);

            return strportPath;
        }

        public IEnumerable<CollectorReport> GetCollectorReport()
        {
            List<CollectorReport> LDailySoa = CommonRep.GetDBContext().Database.SqlQuery<CollectorReport>
            ("P_DAILYREPORT_SHOW").ToList<CollectorReport>();

            foreach (CollectorReport amt in LDailySoa) 
            {
                amt.unFinishSOAAcc = amt.TotalOpenAcc - amt.finishSOAAcc - amt.closedAcc;
                if (amt.TotalOpenAcc > 0)
                {
                    amt.coverageSOAVol = Convert.ToDecimal(amt.finishSOAAcc) / Convert.ToDecimal(amt.TotalOpenAcc);
                }
                else
                {
                    amt.coverageSOAVol = 0;
                }
                if (amt.periodInialAmt != 0)
                {
                    amt.coverageSOAAmt = amt.finishSOAAmt / amt.periodInialAmt;
                    amt.coveragePtpAmt = amt.ptpAmt / amt.periodInialAmt;
                    amt.coveragepaymentNoticeAmt = amt.paymentNoticeAmt / amt.periodInialAmt;
                }
                else
                {
                    amt.coverageSOAAmt = 0;
                    amt.coveragePtpAmt = 0;
                    amt.coveragepaymentNoticeAmt = 0;
                }
            }
            return LDailySoa.OrderByDescending(o => o.coverageSOAAmt);
        }
    }

    public class CollectorReport
    {
        public string Eid { get; set; }
        public string TeamName { get; set; }
        public int TotalOpenAcc { get; set; }
        public decimal periodInialAmt { get; set; }
        public int finishSOAAcc { get; set; }
        public decimal finishSOAAmt { get; set; }
        public int closedAcc { get; set; }
        public int unFinishSOAAcc { get; set; }
        public decimal coverageSOAAmt { get; set; }
        public decimal coverageSOAVol { get; set; }
        public int toBe2ndAcc { get; set; }
        public int finish2ndAcc { get; set; } 
        public decimal finish2ndAmt { get; set; }
        public int toBe3ndAcc { get; set; }
        public int finish3ndAcc { get; set; }
        public decimal finish3ndAmt { get; set; }
        public decimal ptpAmt { get; set; }           
        public decimal coveragePtpAmt { get; set; }      
        public decimal paymentNoticeAmt { get; set; }      
        public decimal coveragepaymentNoticeAmt { get; set; }    
        public decimal disputeAmt { get; set; }        

    }

    public class DailyReportSoa
    {
        public string Eid { get; set; }
        public string TeamName { get; set; }
        public int TotalOpenAcc { get; set; }
        public int WIP { get; set; }
        public int Finish { get; set; }
        public int normalCust { get; set; }
        public int UNFinish { get; set; }
        public int NOCON { get; set; }
        public int NOCIAACC { get; set; }
        public int MailAcc { get; set; }
        public int taks { get; set; }
        public int normaltaks { get; set; }
        public int workTime { get; set; }
        public int normalworkTime { get; set; }
        public int ClosedAcc { get; set; }
    }

    public class DailyReportAmt
    {
        public string Eid { get; set; }
        public Decimal PeriodAmt { get; set; }
        public Decimal Soaamt { get; set; }
        public Decimal SecondAmt { get; set; }
        public Decimal ThirdAmt { get; set; }
        public Decimal PTPAmt { get; set; }
        public Decimal PayNoiceAmt { get; set; }
        public Decimal DISAmt { get; set; }
        public Decimal TotalAmt { get; set; }
        public Decimal RankNum { get; set; }
    }
}