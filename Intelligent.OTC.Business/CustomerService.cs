using CsvHelper;
using CsvHelper.Configuration;
using EntityFramework.BulkInsert.Extensions;
using EntityFramework.BulkInsert.Providers;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;

namespace Intelligent.OTC.Business
{
    public partial class CustomerService : ICustomerService
    {
        public XcceleratorService XccService { get; set; }

        #region constant definition
        public const string UnTouched = "001";
        public const string agingData = "002";
        public const char strSplit = '^';// 分割字符串
        public const char strSubSplit = '_';// 分割字符串
        public const char strInvSplit = '-';// 分割字符串
        public const string strAccountKey = "AccountLevelPath";//Account路径的config保存名
        public const string strInvoiceKey = "InvoiceLevelPath";//Invoice路径的config保存名
        public const string strAccountStats = "Draft";//初始stats
        public string strInvStats = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Open);//初始stats
        public const string strFormatCheck = "Summary Type:^Customer Summary";
        public const string strHeaderCheck = "Customer";
        public const string strInvoiceHeaderCheck = "Statement Date";
        public const string strSubTatolCheck = "Sub-total";
        public const string strEnd = "All Customers";
        public const int intAccountCols = 31;
        public const int intInvoiceCols = 29;
        public string strMessage = "";
        public string strEnter = "\r";
        public string strDtCheck = "Report Date:";
        public string strTempPathKey = "TemplateDownloadPath";
        public string strArchivePathKey = "ArchiveDownloadPath";
        public string CurrentDeal
        {
            get
            {
                return AppContext.Current.User.Deal.ToString();
            }
        }
        public DateTime CurrentTime
        {
            get
            {
                return AppContext.Current.User.Now;
            }
        }
        #endregion

        #region variable definition
        public OTCRepository CommonRep { get; set; }
        public ICacheService CacheSvr { get; set; }
        public List<Customer> custList;
        public IBaseDataService BDService { get; set; }

        public List<CustomerAgingStaging> listAgingStaging;

        public List<InvoiceAgingStaging> invoiceAgingList;

        public List<T_Invoice_Detail_Staging> invoiceDetailAgingList;

        public List<T_INVOICE_VAT_STAGING> listvat;//T_INVOICE_VAT_STAGING

        public List<CustomerAgingStaging> listAgingStagingEx;//导入前数据

        public List<InvoiceAgingStaging> invoiceAgingListEx;//导入前数据

        public string[] CustomerInfo;

        #endregion

        /// <summary>
        /// get all CustomerAgingStaging Data from Db
        /// </summary>
        /// <returns></returns>
        public IQueryable<CustomerAgingStaging> GetCustomerAgingStaging()
        {
            return CommonRep.GetQueryable<CustomerAgingStaging>().Where(o => o.Deal == CurrentDeal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<InvoiceAgingStaging> GetInvoiceAgingStaging()
        {
            return CommonRep.GetQueryable<InvoiceAgingStaging>().Where(o => o.Deal == CurrentDeal);
        }

        /// <summary>
        /// get all CustomerAging Data from Db
        /// </summary>
        /// <returns></returns>
        public IQueryable<CustomerAging> GetCustomerAging()
        {
            return CommonRep.GetQueryable<CustomerAging>().Where(o => o.Deal == CurrentDeal);
        }


        /// <summary>
        /// Get customer basics and level information
        /// </summary>
        /// <param name="latestPId"></param>
        /// <returns></returns>
        public IQueryable<CustomerLevelDto> GetCustomerLevel(int latestPId)
        {
            var customerLevel = from cust in CommonRep.GetDbSet<Customer>()
                                join cas in CommonRep.GetDbSet<CustomerLevelView>()
                                on new { CustomerNum = cust.CustomerNum, SiteUseId = cust.SiteUseId }
                                equals new { CustomerNum = cas.CustomerNum, SiteUseId = cas.SiteUseId }
                                into g
                                from cas in g.DefaultIfEmpty()

                                select new CustomerLevelDto
                                {
                                    Id = cust.Id,
                                    Deal = cust.Deal,
                                    CustomerNum = cust.CustomerNum,
                                    CustomerName = cust.CustomerName,
                                    // if group not exist, use customer name as group code and name.
                                    BillGroupCode = cust.CustomerName,
                                    BillGroupName = cust.CustomerName,
                                    Collector = cust.Collector,
                                    Value = 0,
                                    Risk = 0,
                                    ValueLevel = "",
                                    RiskLevel = "",
                                    Class = cas.ClassLevel,
                                    SiteUseId = cust.SiteUseId,
                                    CS = cust.CUSTOMER_SERVICE
                                };
            return customerLevel;
        }

        /// <summary>
        /// Get customer basics and level information Specially for All customers
        /// </summary>
        /// <param name="latestPId"></param>
        /// <param name="supervisor">1:isSupervisor;2:Not Supervisor</param>
        /// <returns></returns>
        public IQueryable<CustomerLevelDto> GetCustomerLevelForAllCus(int latestPId, int supervisor)
        {
            IQueryable<CustomerLevelDto> customerLevel = null;

            if (supervisor == 1)
            {
                customerLevel = from cust in CommonRep.GetDbSet<Customer>()
                                join cas in CommonRep.GetDbSet<CustomerLevelView>()
                                on new { CustomerNum = cust.CustomerNum, SiteUseId = cust.SiteUseId }
                                equals new { CustomerNum = cas.CustomerNum, SiteUseId = cas.SiteUseId }
                                into g
                                from cas in g.DefaultIfEmpty()
                                where cust.IsActive == true && cust.ExcludeFlg == "0" && cust.Deal == AppContext.Current.User.Deal
                                select new CustomerLevelDto
                                {
                                    Id = cust.Id,
                                    Deal = cust.Deal,
                                    CustomerNum = cust.CustomerNum,
                                    CustomerName = cust.CustomerName,
                                    // if group not exist, use customer name as group code and name.
                                    BillGroupCode = cust.CustomerName,
                                    BillGroupName = cust.CustomerName,
                                    Collector = cust.Collector,
                                    Value = cas.Risk,
                                    Risk = cas.Risk,
                                    ValueLevel = cas.RiskLevel,
                                    RiskLevel = cas.RiskLevel,
                                    //Start add by xuan.wu for Arrow adding
                                    CS = cust.CUSTOMER_SERVICE,
                                    SiteUseId = cust.SiteUseId,
                                    Class = cas.ClassLevel,
                                    Sales = cust.Sales
                                    //End add by xuan.wu for Arrow adding
                                };
            }
            else
            {
                List<SysUser> listUser = new List<SysUser>();
                listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
                string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
                string collecotrList = "," + string.Join(",", userGroup.ToArray()) + ",";

                customerLevel = from cust in CommonRep.GetDbSet<Customer>()
                                join cas in CommonRep.GetDbSet<CustomerLevelView>()
                                    on new { CustomerNum = cust.CustomerNum, SiteUseId = cust.SiteUseId }
                                    equals new { CustomerNum = cas.CustomerNum, SiteUseId = cas.SiteUseId }
                                    into g
                                from cas in g.DefaultIfEmpty()
                                where cust.IsActive == true && cust.ExcludeFlg == "0" && cust.Deal == AppContext.Current.User.Deal && collecotrList.Contains("," + cust.Collector + ",")
                                select new CustomerLevelDto
                                {
                                    Id = cust.Id,
                                    Deal = cust.Deal,
                                    CustomerNum = cust.CustomerNum,
                                    CustomerName = cust.CustomerName,
                                    // if group not exist, use customer name as group code and name.
                                    BillGroupCode = cust.CustomerName,
                                    BillGroupName = cust.CustomerName,
                                    Collector = cust.Collector,
                                    Value = cas.Risk,
                                    Risk = cas.Risk,
                                    ValueLevel = cas.RiskLevel,
                                    RiskLevel = cas.RiskLevel,
                                    //Start add by xuan.wu for Arrow adding
                                    CS = cust.CUSTOMER_SERVICE,
                                    SiteUseId = cust.SiteUseId,
                                    Class = cas.ClassLevel,
                                    Sales = cust.Sales
                                    //End add by xuan.wu for Arrow adding
                                };
            }
            return customerLevel;
        }

        public IQueryable<CustomerMasterDto> GetCustomerMasterForCurrentUser()
        {
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
            List<string> nEIdList = collecotr.GetUserTeamList(AppContext.Current.User.EID).Select(x => x.EID).ToList();

            return from cust in CommonRep.GetDbSet<Customer>()
                   where cust.IsActive == true && cust.ExcludeFlg == "0" && cust.Deal == AppContext.Current.User.Deal
                   && nEIdList.Contains(cust.Collector)
                   select new CustomerMasterDto
                   {
                       Id = cust.Id,
                       Deal = cust.Deal,
                       CustomerNum = cust.CustomerNum,
                       CustomerName = cust.CustomerName,
                       siteUseId = cust.SiteUseId,
                       Collector = cust.Collector
                   };
        }

        public IQueryable<CustomerMasterDto> GetCustomerMasterForAllUser()
        {
            return from cust in CommonRep.GetDbSet<Customer>()
                   join grp in CommonRep.GetDbSet<CustomerGroupCfg>()
                   on new { cust.BillGroupCode, cust.Deal } equals new { grp.BillGroupCode, grp.Deal }
                   into grps
                   from grp in grps.DefaultIfEmpty()
                   where cust.IsActive == true && cust.ExcludeFlg == "0" && cust.Deal == AppContext.Current.User.Deal
                   select new CustomerMasterDto
                   {
                       Id = cust.Id,
                       Deal = cust.Deal,
                       CustomerNum = cust.CustomerNum,
                       CustomerName = cust.CustomerName,
                       // if group not exist, use customer name as group code and name.
                       BillGroupCode = string.IsNullOrEmpty(grp.BillGroupCode) ? cust.CustomerName : grp.BillGroupCode,
                       BillGroupName = string.IsNullOrEmpty(grp.BillGroupName) ? cust.CustomerName : grp.BillGroupName,
                       Collector = string.IsNullOrEmpty(grp.Collector) ? cust.Collector : grp.Collector
                   };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<CustomerTeam> GetCustomerTeam()
        {
            return CommonRep.GetQueryable<CustomerTeam>().Where(o => o.Deal == CurrentDeal);
        }

        public CollectorTeam GetTeamByEid(string eid)
        {
            return CommonRep.GetQueryable<CollectorTeam>().Include<CollectorTeam, Team>(o => o.Team).Where(c => c.Deal == CurrentDeal && c.Collector == eid).FirstOrDefault();
        }

        #region get CustomerAgingStagingHistory Data from Db by ImportId
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<CustomerAgingStaging> GetCustomerAging(string strImportId)
        {
            return CommonRep.GetQueryable<CustomerAgingStaging>().Where(o => o.ImportId == strImportId).Select(o => o).ToList();
        }
        #endregion

        public List<CustomerGroupCfgStaging> GetGroupStaing()
        {
            return CommonRep.GetQueryable<CustomerGroupCfgStaging>().Where(o => o.Deal == CurrentDeal).ToList();
        }

        public List<CustomerAging> GetCurrentPerCustAging()
        {
            List<string> strImportId = new List<string>();
            SiteService siteService = SpringFactory.GetObjectImpl<SiteService>("SiteService");
            List<Sites> sites = siteService.GetAllSites().Where(o => o.Deal == CurrentDeal).ToList();

            PeroidService perService = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            PeriodControl currentPer = perService.getcurrentPeroid();

            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            List<FileUploadHistory> file = fileService.GetFileUploadHistory().ToList();
            string strImpId = "";
            foreach (Sites site in sites)
            {
                strImpId = file.Where(o => o.LegalEntity == site.LegalEntity
                                && o.SubmitTime >= currentPer.PeriodBegin
                                && o.SubmitTime <= currentPer.PeriodEnd).OrderByDescending(o => o.SubmitTime).Select(o => o.ImportId).FirstOrDefault();
                if (strImpId != null)
                {
                    strImportId.Add(strImpId);
                }
            }

            return CommonRep.GetQueryable<CustomerAging>().Where(o => strImportId.Contains(o.ImportId)).Select(o => o).ToList();
        }

        #region get all SysConfig Data from Db
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<SysConfig> GetSysConfig()
        {
            return CommonRep.GetQueryable<SysConfig>();
        }
        #endregion

        #region Get CustomerDetail data
        /// <summary>
        /// GetCustomerDetail 数据做成
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CustomerDetail GetCustomerDetail(int id)
        {
            DateTime agingDT = new DateTime();
            SysConfig rtnData = GetSysConfig().Where(o => o.CfgCode == agingData).Select(o => o).FirstOrDefault();
            agingDT = dataConvertToDT(rtnData.CfgValue.ToString());
            DateTime agingDT90 = new DateTime();
            agingDT90 = agingDT.AddDays(-90);
            CustomerAging cust = CommonRep.FindBy<CustomerAging>(id);
            string cusClassCode = cust.CustomerClass;
            CustomerDetail custDe = new CustomerDetail();
            ObjectHelper.CopyObject(cust, custDe);
            custDe.DueOver180Amt = cust.Due210Amt + cust.Due240Amt + cust.Due270Amt
                                    + cust.Due300Amt + cust.Due360Amt;
            custDe.DueOver90Amt = custDe.DueOver180Amt + cust.Due120Amt + cust.Due150Amt;
            custDe.DueOver60Amt = custDe.DueOver90Amt;
            custDe.DueOver30Amt = custDe.DueOver60Amt;
            custDe.DueOver0Amt = custDe.DueOver30Amt;
            custDe.CustomerClass = Helper.CodeToEnum<CustomerClass>(cusClassCode).ToString();

            IQueryable<InvoiceAging> inv = CommonRep.GetQueryable<InvoiceAging>()
                                           .Where(o => o.LegalEntity == custDe.LegalEntity  //siteCode to LegalEntity
                                               && o.CustomerNum == custDe.CustomerNum
                                               && o.Deal == custDe.Deal) //Deal condition Add 09/14
                                           .Select(o => o);
            custDe.FCollectAmt = inv.Where(o => o.InvoiceDate < agingDT && o.InvoiceDate >= AppContext.Current.User.Now).Sum(o => o.OriginalAmt);
            custDe.FDueOver90Amt = inv.Where(o => o.InvoiceDate < agingDT90).Sum(o => o.OriginalAmt);

            return custDe;
        }
        #endregion

        #region CustomerAging list to commonrep
        /// <summary>
        /// 
        /// </summary>
        /// <param name="custAgs"></param>
        public void AddCustomerAgings(List<CustomerAging> custAgs)
        {
            foreach (CustomerAging custag in custAgs)
            {
                CommonRep.Add(custag);
            }
            CommonRep.Commit();
        }
        #endregion

        #region CustomerAging to commonrep
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cust"></param>
        public void AddCustomer(Customer cust)
        {
            CommonRep.Add(cust);
            CommonRep.Commit();
        }
        #endregion

        #region Get All Customer Data
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<Customer> GetCustomer()
        {
            return CommonRep.GetQueryable<Customer>().Where(c => c.Deal == AppContext.Current.User.Deal
                                                        && c.IsActive == true);
        }
        #endregion

        #region Get All MasterData
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<CustomerMasterData> GetCustMasterData(string Contacter)
        {
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
            var collecotrList = ",";
            var userId = AppContext.Current.User.EID;
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");

            var result = CommonRep.GetQueryable<CustomerMasterData>().Where(o => o.Deal == CurrentDeal);
            if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
            {
            }
            else
            {
                result = result.Where(o => collecotrList.Contains("," + o.Collector + ","));
            }
            if (!string.IsNullOrEmpty(Contacter) && Contacter != "undefined")
            {

                result = from c in result
                         join i in CommonRep.GetQueryable<Contactor>()
                         on new { siteuseid = c.SiteUseId } equals new { siteuseid = i.SiteUseId }
                         where i.Name.Contains(Contacter) || i.EmailAddress.Contains(Contacter) || i.Number.Contains(Contacter)
                         select c;
            }

            return result;
        }
        #endregion

        /// <summary>
        /// MailDetail get ReassignCustomer add by pxc 20160203
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        public IQueryable<CustomerMasterDto> GetCustMasterDataForAssign(string customers)
        {
            if (!string.IsNullOrEmpty(customers))
            {
                List<string> cusNums = customers.Split(',').ToList<string>();
                return GetCustomerMasterForCurrentUser().Where(m => !cusNums.Contains(m.CustomerNum));
            }
            else
            {
                return GetCustomerMasterForCurrentUser();
            }
        }

        public string ExpoertComment()
        {

            string tplName = "";
            string pathName = "";
            string fileName = "";
            string virPatnName = "";
            string templateName = "MasterDataCommentTemplate";
            string custPathName = "CustPathName";
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
            var collecotrList = ",";
            var userId = AppContext.Current.User.EID;
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");

            IQueryable<CustomerMasterData> exportcust = CommonRep.GetQueryable<CustomerMasterData>().Where(m => m.Deal == CurrentDeal);

            if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
            {
            }
            else
            {
                exportcust = exportcust.Where(o => collecotrList.Contains("," + o.Collector + ","));
            }

            try
            {
                if (exportcust == null)
                {
                    Exception ex = new OTCServiceException("There are no existing datas.");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }

                tplName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[templateName].ToString());
                fileName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString());
                pathName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString() + "Customer Comment_" + DateTime.Now.ToString("yyyyMMdd") + "_" + AppContext.Current.User.EID + ".xlsx");
                if (Directory.Exists(fileName) == false)
                {
                    Directory.CreateDirectory(fileName);
                }

                File.Copy(tplName, pathName, true);

                ExcelPackage package = new ExcelPackage(new FileInfo(pathName));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int startRow = 2;
                foreach (var lst in exportcust)
                {
                    worksheet.Cells[startRow, 1].Value = lst.SiteUseId;
                    worksheet.Cells[startRow, 2].Value = lst.Comment;
                    worksheet.Cells[startRow, 3].Value = lst.CommentExpirationDate == null ? "" : Convert.ToDateTime(lst.CommentExpirationDate).ToString("yyyy-MM-dd");
                    worksheet.Cells[startRow, 4].Value = lst.CommentLastDate == null ? "" : Convert.ToDateTime(lst.CommentLastDate).ToString("yyyy-MM-dd HH:mm:ss");
                    worksheet.Cells[startRow, 5].Value = lst.CustomerNum;
                    worksheet.Cells[startRow, 6].Value = lst.CustomerName;
                    startRow++;
                }

                package.Save();
                package.Dispose();


                HttpRequest request = HttpContext.Current.Request;
                StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
                appUriBuilder.Append(Uri.SchemeDelimiter);
                appUriBuilder.Append(request.Url.Authority);
                if (String.Compare(request.ApplicationPath, @"/") != 0)
                {
                    appUriBuilder.Append(request.ApplicationPath);
                }

                virPatnName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[custPathName].ToString().Trim('~') + "Customer Comment_" + DateTime.Now.ToString("yyyyMMdd") + "_" + AppContext.Current.User.EID + ".xlsx";
                return virPatnName;
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Failed to create Customer Excel!", ex);
                throw;
            }

            return "";
        }

        public string ExpoertCommentSales()
        {

            string tplName = "";
            string pathName = "";
            string fileName = "";
            string virPatnName = "";
            string templateName = "MasterDataCommentSalesTemplate";
            string custPathName = "CustPathName";
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
            var collecotrList = ",";
            var userId = AppContext.Current.User.EID;
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");

            //IQueryable<CustomerMasterData> exportcust = CommonRep.GetQueryable<CustomerMasterData>().Where(m => m.Deal == CurrentDeal);
            List<CustomerCommentsDto> exportdata =getAllCustomerComments();
            List<CustomerAgingBucketDto> customerAging = getAllCustomerAging();
            //if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") < 0)
            //{
            //    exportcust = exportcust.Where(o => collecotrList.Contains("," + o.Collector + ","));
            //}

            try
            {
                if (exportdata == null)
                {
                    Exception ex = new OTCServiceException("There are no existing datas.");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }

                tplName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[templateName].ToString());
                fileName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString());
                pathName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString() + "CustomerCommentCsSales_" + DateTime.Now.ToString("yyyyMMdd") + "_" + AppContext.Current.User.EID + ".xlsx");
                if (Directory.Exists(fileName) == false)
                {
                    Directory.CreateDirectory(fileName);
                }

                File.Copy(tplName, pathName, true);

                ExcelPackage package = new ExcelPackage(new FileInfo(pathName));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int startRow = 2;
                foreach (var lst in exportdata)
                {
                    worksheet.Cells[startRow, 1].Value = lst.CUSTOMER_NUM;
                    worksheet.Cells[startRow, 2].Value = lst.SiteUseId;
                    worksheet.Cells[startRow, 3].Value = lst.AgingBucket;
                    CustomerAgingBucketDto findAging= customerAging.Find(o => o.CustomerNum == lst.CUSTOMER_NUM && o.SiteUseId == lst.SiteUseId);
                    decimal agingbucketAmt = 0;
                    if (findAging != null) {
                        switch (lst.AgingBucket) {
                            case "001-015":
                                agingbucketAmt = findAging.Due15Amt == null ? 0 : Convert.ToDecimal(findAging.Due15Amt);
                                break;
                            case "016-030":
                                agingbucketAmt = findAging.Due30Amt == null ? 0 : Convert.ToDecimal(findAging.Due30Amt);
                                break;
                            case "031-045":
                                agingbucketAmt = findAging.Due45Amt == null ? 0 : Convert.ToDecimal(findAging.Due45Amt);
                                break;
                            case "046-060":
                                agingbucketAmt = findAging.Due60Amt == null ? 0 : Convert.ToDecimal(findAging.Due60Amt);
                                break;
                            case "061-090":
                                agingbucketAmt = findAging.Due90Amt == null ? 0 : Convert.ToDecimal(findAging.Due90Amt);
                                break;
                            case "091-120":
                                agingbucketAmt = findAging.Due120Amt == null ? 0 : Convert.ToDecimal(findAging.Due120Amt);
                                break;
                            case "121-180":
                                agingbucketAmt = findAging.Due180Amt == null ? 0 : Convert.ToDecimal(findAging.Due180Amt) ;
                                break;
                            case "181-270":
                                agingbucketAmt = findAging.Due270Amt == null ? 0 : Convert.ToDecimal(findAging.Due270Amt);
                                break;
                            case "271-360":
                                agingbucketAmt = findAging.Due360Amt == null ? 0 : Convert.ToDecimal(findAging.Due360Amt);
                                break;
                            case "360+":
                                agingbucketAmt = findAging.DueOver360Amt == null ? 0 : Convert.ToDecimal(findAging.DueOver360Amt);
                                break;
                        }
                    }
                    worksheet.Cells[startRow, 4].Value = agingbucketAmt;
                    worksheet.Cells[startRow, 5].Value = lst.PTPAmount;
                    worksheet.Cells[startRow, 6].Value = lst.PTPDATE == null ? "" : Convert.ToDateTime(lst.PTPDATE).ToString("yyyy-MM-dd");
                    worksheet.Cells[startRow, 7].Value = lst.OverdueReason;
                    worksheet.Cells[startRow, 8].Value = lst.Comments;
                    worksheet.Cells[startRow, 9].Value = lst.CommentsFrom;
                    startRow++;
                }

                package.Save();
                package.Dispose();


                HttpRequest request = HttpContext.Current.Request;
                StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
                appUriBuilder.Append(Uri.SchemeDelimiter);
                appUriBuilder.Append(request.Url.Authority);
                if (String.Compare(request.ApplicationPath, @"/") != 0)
                {
                    appUriBuilder.Append(request.ApplicationPath);
                }

                virPatnName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[custPathName].ToString().Trim('~') + "CustomerCommentCsSales_" + DateTime.Now.ToString("yyyyMMdd") + "_" + AppContext.Current.User.EID + ".xlsx";
                return virPatnName;
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Failed to create Customer Excel!", ex);
                throw;
            }

            return "";
        }


        #region Export CustomerMasterData
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ExportCustomer(string custnum, string custname, string status, string collector, string begintime,
            string endtime, string miscollector, string misgroup, string billcode, string country, string siteUseId, string LegalEntity, string EbName)
        {
            string templateName = "MasterDataTemplate";
            string custPathName = "CustPathName";
            string tplName = "";
            string pathName = "";
            string fileName = "";
            string virPatnName = "";
            if (custnum == null)
            {
                custnum = "undefined";
            }
            if (custname == null)
            {
                custname = "undefined";
            }
            if (status == "null")
            {
                status = "undefined";
            }
            if (billcode == null)
            {
                billcode = "undefined";
            }
            if (collector == null)
            {
                collector = "undefined";
            }
            if (country == null)
            {
                country = "undefined";
            }
            if (LegalEntity == null)
            {
                LegalEntity = "undefined";
            }
            if (EbName == null)
            {
                EbName = "undefined";
            }
            if (begintime == null)
            {
                begintime = "undefined";
            }
            if (endtime == null)
            {
                endtime = "undefined";
            }

            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
            var collecotrList = ",";
            var userId = AppContext.Current.User.EID;
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");

            IQueryable<CustomerMasterData> exportcust = CommonRep.GetQueryable<CustomerMasterData>().Where(m => m.Deal == CurrentDeal);

            if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
            {
            }
            else
            {
                exportcust = exportcust.Where(o => collecotrList.Contains("," + o.Collector + ","));
            }

            if (custnum != "undefined")
            {
                exportcust = exportcust.Where(m => m.CustomerNum.Contains(custnum));
            }
            if (custname != "undefined")
            {
                exportcust = exportcust.Where(m => m.CustomerName.Contains(custname));
            }
            if (status != "undefined")
            {
                exportcust = exportcust.Where(m => m.IsHoldFlg.Contains(status));
            }
            if (billcode != "undefined")
            {
                exportcust = exportcust.Where(m => m.BillGroupCode.Contains(billcode));
            }
            if (collector != "undefined")
            {
                exportcust = exportcust.Where(m => m.Collector.Contains(collector));
            }
            if (country != "undefined")
            {
                exportcust = exportcust.Where(m => m.Country.Contains(country));
            }
            if (LegalEntity != "undefined")
            {
                exportcust = exportcust.Where(m => m.Organization == LegalEntity);
            }
            if (EbName != "undefined")
            {
                exportcust = exportcust.Where(m => m.Ebname.Contains(EbName));
            }
            if (begintime != "undefined")
            {
                DateTime et = Convert.ToDateTime(begintime);
                exportcust = exportcust.Where(m => m.CreateTime > et || m.CreateTime == et);
            }
            if (endtime != "undefined")
            {
                DateTime et = Convert.ToDateTime(endtime);
                exportcust = exportcust.Where(m => m.CreateTime < et || m.CreateTime == et);
            }
            if (miscollector == "true")
            {
                exportcust = exportcust.Where(m => m.Collector == null);
            }
            if (misgroup == "true")
            {
                exportcust = exportcust.Where(m => m.BillGroupCode == null);
            }
            if (siteUseId != "undefined")
            {
                exportcust = exportcust.Where(m => m.SiteUseId.Contains(siteUseId));
            }

            try
            {
                if (exportcust == null)
                {
                    Exception ex = new OTCServiceException("There are no existing datas.");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }

                tplName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[templateName].ToString());
                fileName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString());
                pathName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString() + "MasterDataExport." + AppContext.Current.User.EID + ".xlsx");
                if (Directory.Exists(fileName) == false)
                {
                    Directory.CreateDirectory(fileName);
                }
                WriteToExcel(tplName, pathName, exportcust);

                HttpRequest request = HttpContext.Current.Request;
                StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
                appUriBuilder.Append(Uri.SchemeDelimiter);
                appUriBuilder.Append(request.Url.Authority);
                if (String.Compare(request.ApplicationPath, @"/") != 0)
                {
                    appUriBuilder.Append(request.ApplicationPath);
                }

                virPatnName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[custPathName].ToString().Trim('~') + "MasterDataExport." + AppContext.Current.User.EID + ".xlsx";
                return virPatnName;
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Failed to create Customer Excel!", ex);
                throw;
            }
        }

        #endregion

        #region Export CustomerMasterData
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void WriteToExcel(string temp, string path, IQueryable<CustomerMasterData> custlist)
        {
            CustomerGroupCfgService service = SpringFactory.GetObjectImpl<CustomerGroupCfgService>("CustomerGroupCfgService");
            IQueryable<CustomerGroupCfg> cfglist = service.GetAllGroups();
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
            int regionId = Convert.ToInt32(AppContext.Current.User.RegionId);
            int centerId = Convert.ToInt32(AppContext.Current.User.CenterId);
            int groupId = Convert.ToInt32(AppContext.Current.User.GroupId);
            int dealId = Convert.ToInt32(AppContext.Current.User.DealId);
            int teamId = Convert.ToInt32(AppContext.Current.User.TeamId);
            string deal = AppContext.Current.User.Deal;
            IQueryable<SysUser> collectlist = collecotr.GetUsers(regionId, centerId, groupId, dealId, teamId, AppContext.Current.User.Deal).AsQueryable();
            //获得联系人信息
            var contactors = from co in CommonRep.GetQueryable<Contactor>()
                             join c in custlist
                             on new { SiteUseId = co.SiteUseId } equals new { SiteUseId = c.SiteUseId }
                             select co;



            try
            {
                NpoiHelper helper = new NpoiHelper(temp);
                helper.Save(path, true);
                helper = new NpoiHelper(path);

                string sheetName = "";
                foreach (string sheet in helper.Sheets)
                {
                    sheetName = sheet;

                    if (sheetName == "Customer")
                    {
                        //向sheet为Customer的excel中写入文件
                        helper.ActiveSheetName = sheetName;
                        ISheet curSheet = helper.Book.GetSheetAt(helper.ActiveSheet);
                        ICellStyle styleCell = helper.Book.CreateCellStyle();
                        IFont font = helper.Book.CreateFont();
                        font.FontName = "Arial";
                        font.FontHeight = 9;
                        styleCell.SetFont(font);
                        styleCell.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                        styleCell.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                        styleCell.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;

                        int rowNo = 2;
                        foreach (var cust in custlist)
                        {
                            var row = curSheet.CreateRow(rowNo);
                            row.Height = 12 * 20;

                            if (!String.IsNullOrEmpty(cust.Region))
                            {
                                SetCellValue(row, 0, cust.Region);
                            }
                            else
                            {
                                SetCellValue(row, 0, "");
                            }
                            if (!String.IsNullOrEmpty(cust.CustomerNum))
                            {
                                SetCellValue(row, 1, cust.CustomerNum);
                            }
                            else
                            {
                                SetCellValue(row, 1, "");
                            }
                            if (!String.IsNullOrEmpty(cust.CustomerName))
                            {
                                SetCellValue(row, 2, cust.CustomerName);
                            }
                            else
                            {
                                SetCellValue(row, 2, "");
                            }
                            if (!String.IsNullOrEmpty(cust.ExcludeFlg))
                            {
                                SetCellValue(row, 3, cust.ExcludeFlg);
                            }
                            else
                            {
                                SetCellValue(row, 3, 0);
                            }
                            if (!String.IsNullOrEmpty(cust.SiteUseId))
                            {
                                SetCellValue(row, 4, cust.SiteUseId);
                            }
                            else
                            {
                                SetCellValue(row, 4, "");
                            }

                            SetCellValue(row, 5, cust.STATUS == "1" ? "YES" : "");

                            if (!String.IsNullOrEmpty(cust.Country))
                            {
                                SetCellValue(row, 6, cust.Country);
                            }
                            else
                            {
                                SetCellValue(row, 6, "");
                            }
                            if (!String.IsNullOrEmpty(cust.Organization))
                            {
                                SetCellValue(row, 7, cust.Organization);
                            }
                            else
                            {
                                SetCellValue(row, 7, "");
                            }
                            if (!String.IsNullOrEmpty(cust.Ebname))
                            {
                                SetCellValue(row, 8, cust.Ebname);
                            }
                            else
                            {
                                SetCellValue(row, 8, "");
                            }

                            if (!String.IsNullOrEmpty(cust.IsHoldFlg))
                            {
                                SetCellValue(row, 9, cust.IsHoldFlg);
                            }
                            else
                            {
                                SetCellValue(row, 9, 0);
                            }
                            if (!String.IsNullOrEmpty(cust.CREDIT_TREM))
                            {
                                SetCellValue(row, 10, cust.CREDIT_TREM);
                            }
                            else
                            {
                                SetCellValue(row, 10, "");
                            }
                            if (!String.IsNullOrEmpty(cust.Collector))
                            {
                                SetCellValue(row, 11, cust.Collector);
                            }
                            else
                            {
                                SetCellValue(row, 11, "");
                            }
                            if (!String.IsNullOrEmpty(cust.SpecialNotes))
                            {
                                SetCellValue(row, 12, cust.SpecialNotes);
                            }
                            else
                            {
                                SetCellValue(row, 12, "");
                            }
                            if (!String.IsNullOrEmpty(cust.CUSTOMER_SERVICE))
                            {
                                SetCellValue(row, 13, cust.CUSTOMER_SERVICE);
                            }
                            else
                            {
                                SetCellValue(row, 13, "");
                            }
                            if (!String.IsNullOrEmpty(cust.SALES1))
                            {
                                SetCellValue(row, 14, cust.SALES1);
                            }
                            else
                            {
                                SetCellValue(row, 14, "");
                            }
                            if (!String.IsNullOrEmpty(cust.LOCALIZE_CUSTOMER_NAME))
                            {
                                SetCellValue(row, 15, cust.LOCALIZE_CUSTOMER_NAME);
                            }
                            else
                            {
                                SetCellValue(row, 15, "");
                            }
                            if (cust.AMTLimit != null)
                            {
                                SetCellValue(row, 16, cust.AMTLimit);
                            }
                            else
                            {
                                SetCellValue(row, 16, "");
                            }
                            if (cust.CREDIT_LIMIT != null)
                            {
                                SetCellValue(row, 17, cust.CREDIT_LIMIT);
                            }
                            else
                            {
                                SetCellValue(row, 17, "");
                            }
                            if (cust.BadDebt != null)
                            {
                                SetCellValue(row, 18, cust.BadDebt);
                            }
                            else
                            {
                                SetCellValue(row, 18, "");
                            }
                            if (cust.Branch != null)
                            {
                                SetCellValue(row, 19, cust.Branch);
                            }
                            else
                            {
                                SetCellValue(row, 19, "");
                            }
                            if (cust.Litigation != null)
                            {
                                SetCellValue(row, 20, cust.Litigation);
                            }
                            else
                            {
                                SetCellValue(row, 20, "");
                            }
                            if (cust.Comment != null)
                            {
                                SetCellValue(row, 21, cust.Comment);
                            }
                            else
                            {
                                SetCellValue(row, 21, "");
                            }
                            if (cust.CommentExpirationDate != null)
                            {
                                SetCellValue(row, 22, cust.CommentExpirationDate);
                            }
                            else
                            {
                                SetCellValue(row, 22, "");
                            }

                            if (cust.PTPDATE != null)
                            {
                                SetCellValue(row, 23, cust.PTPDATE);
                            }
                            else
                            {
                                SetCellValue(row, 23, "");
                            }

                            if (cust.PTPAMOUNT != null)
                            {
                                SetCellValue(row, 24, cust.PTPAMOUNT);
                            }
                            else
                            {
                                SetCellValue(row, 24, "");
                            }

                            for (int i = 0; i <= 24; i++)
                            {
                                row.GetCell(i).CellStyle = styleCell;
                            }
                            rowNo++;
                        }
                    }
                    if (sheetName == "Contactor")
                    {
                        helper.ActiveSheetName = sheetName;

                        ISheet curSheet = helper.Book.GetSheetAt(helper.ActiveSheet);
                        ICellStyle styleCell = helper.Book.CreateCellStyle();
                        IFont font = helper.Book.CreateFont();
                        font.FontName = "Arial";
                        font.FontHeight = 9;
                        styleCell.SetFont(font);
                        styleCell.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                        styleCell.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                        styleCell.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;

                        int rowNo = 2;
                        foreach (var con in contactors)
                        {
                            var row = curSheet.CreateRow(rowNo);
                            row.Height = 12 * 20;

                            if (!String.IsNullOrEmpty(con.CustomerNum))
                            {
                                SetCellValue(row, 0, con.CustomerNum);
                            }
                            else
                            {
                                SetCellValue(row, 0, "");
                            }
                            if (!String.IsNullOrEmpty(con.SiteUseId))
                            {
                                SetCellValue(row, 1, con.SiteUseId);
                            }
                            else
                            {
                                SetCellValue(row, 1, "");
                            }
                            if (!String.IsNullOrEmpty(con.Title))
                            {
                                SetCellValue(row, 2, con.Title);
                            }
                            else
                            {
                                SetCellValue(row, 2, "");
                            }
                            if (!String.IsNullOrEmpty(con.Name))
                            {
                                SetCellValue(row, 3, con.Name);
                            }
                            else
                            {
                                SetCellValue(row, 3, "");
                            }
                            if (!String.IsNullOrEmpty(con.ToCc))
                            {
                                SetCellValue(row, 4, con.ToCc == "1" ? "To" : "Cc");
                            }
                            else
                            {
                                SetCellValue(row, 4, "");
                            }
                            if (!String.IsNullOrEmpty(con.Number))
                            {
                                SetCellValue(row, 5, con.Number);
                            }
                            else
                            {
                                SetCellValue(row, 5, "");
                            }
                            if (!String.IsNullOrEmpty(con.EmailAddress))
                            {
                                SetCellValue(row, 6, con.EmailAddress);
                            }
                            else
                            {
                                SetCellValue(row, 6, "");
                            }
                            if (con.IsCostomerContact != null)
                            {
                                SetCellValue(row, 7, con.IsCostomerContact == true ? "Y" : "");
                            }
                            else
                            {
                                SetCellValue(row, 7, "");
                            }
                            if (!String.IsNullOrEmpty(con.CommunicationLanguage))
                            {
                                SetCellValue(row, 8, con.CommunicationLanguage == "002" ? "Chinese" : "English");
                            }
                            else
                            {
                                SetCellValue(row, 8, "");
                            }

                            for (int i = 0; i <= 8; i++)
                            {
                                row.GetCell(i).CellStyle = styleCell;
                            }
                            rowNo++;
                        }
                    }
                    else if (sheetName == "Collector")
                    {
                        //向sheet为Collector的excel中写入文件
                        helper.ActiveSheetName = sheetName;
                        int rowNo = 2;
                        string Team = "";
                        foreach (var collect in collectlist)
                        {
                            CollectorTeam team = GetTeamByEid(collect.EID);
                            if (team == null)
                            {
                                Team = "";
                            }
                            else
                            {
                                Team = GetTeamByEid(collect.EID).Team.TeamName;
                            }
                            helper.SetData(rowNo, 0, collect.EID);
                            helper.SetData(rowNo, 1, collect.Name);
                            helper.SetData(rowNo, 2, collect.Email);
                            helper.SetData(rowNo, 3, Team);
                            rowNo++;
                        }
                    }
                    else if (sheetName == "Group")
                    {
                        helper.ActiveSheetName = sheetName;
                        int rowNo = 1;
                        foreach (var cfg in cfglist)
                        {
                            helper.SetData(rowNo, 0, cfg.BillGroupCode);
                            helper.SetData(rowNo, 1, cfg.BillGroupName);
                            rowNo++;
                        }
                    }
                }
                helper.ActiveSheetName = "Customer";
                //设置sheet
                helper.Save(path, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
        #endregion

        public string ImportCustHistory()
        {
            FileType fileT = FileType.Customer;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                //upload file to server
                string strMasterDataKey = "ImportMasterData";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strMasterDataKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + ".xlsx";

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileT);

                return ImportCustomer();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!");
            }
        }

        public string ImportCustPayment()
        {
            FileType fileT = FileType.CustPayment;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                //upload file to server
                string strMasterDataKey = "ImportMasterPaymentData";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strMasterDataKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + ".xlsx";

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileT);

                return ImportCustomerPayment();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!\r\n" + ex.Message);
            }
        }

        public string ImportCustComment()
        {
            FileType fileT = FileType.CustComment;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                string strMasterDataKey = "ImportMasterData";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strMasterDataKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + ".xlsx";

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileT);

                return ImportCustomerComment();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!\r\n" + ex.Message);
            }
        }

        public string ImportCustCommentSales()
        {
            FileType fileT = FileType.CustCommentsFromCsSales;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                string strMasterDataKey = "ImportCustCommentSales";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strMasterDataKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + ".xlsx";

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileT);

                return ImportCustomerCommentSales();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!\r\n" + ex.Message);
            }
        }


        public string ImportEBBranch()
        {
            FileType fileT = FileType.CustEBBranch;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                string strMasterDataKey = "ImportMasterData";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strMasterDataKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + ".xlsx";

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileT);

                return ImportCustomerEBBranch();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!\r\n" + ex.Message);
            }
        }

        public string ImportLitigation()
        {
            FileType fileT = FileType.CustLitigation;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                string strMasterDataKey = "ImportMasterData";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strMasterDataKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + ".xlsx";

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileT);

                return ImportCustomerLitigation();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!\r\n" + ex.Message);
            }
        }

        public string ImportBadDebt()
        {
            FileType fileT = FileType.CustBadDebt;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                string strMasterDataKey = "ImportMasterData";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strMasterDataKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + ".xlsx";

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileT);

                return ImportCustomerBadDebt();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!\r\n" + ex.Message);
            }
        }
        public string ImportCustContactor()
        {
            FileType fileT = FileType.MissingContactor;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                string strMasterDataKey = "ImportMasterData";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strMasterDataKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + ".xlsx";

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileT);

                return ImportCustomerContactor();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!\r\n" + ex.Message);
            }
        }

        public string ImportCustLocalize()
        {
            FileType fileT = FileType.CustLocalize;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                //upload file to server
                string strMasterDataKey = "ImportCustLocalize";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strMasterDataKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + ".xlsx";

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileT);

                return ImportCustomerLocalize();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!");
            }
        }

        public string ImportCreditHold()
        {
            FileType fileT = FileType.CreditHold;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                //upload file to server
                string strMasterDataKey = "ImportCreditHold";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strMasterDataKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + ".xlsx";

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileT);

                return ImportCreditHoldDo();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!");
            }
        }

        public string ImportCreditHoldDo()
        {
            string strCode = "";
            FileUploadHistory fileUpHis = new FileUploadHistory();
            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            try
            {
                //工作区结束行号
                strCode = Helper.EnumToCode<FileType>(FileType.CreditHold);
                fileUpHis = fileService.GetSuccessData(strCode);
                string strpath = "";
                if (fileUpHis == null)
                {
                    throw new Exception("import file is not found!");
                }
                strpath = fileUpHis.ArchiveFileName;
                List<string> listcust = new List<string>();
                #region openXml
                ExcelPackage package = new ExcelPackage(new FileInfo(strpath));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号
                #endregion
                rowStart = 2;
                for (int j = rowStart; j <= rowEnd; j++)
                {
                    if (worksheet.Cells[j, 1] != null && worksheet.Cells[j, 1].Value != null)
                    {
                        string strCustomerNum = worksheet.Cells[j, 1].Value.ToString();
                        listcust.Add(strCustomerNum);
                    }
                }
                if (listcust.Count > 0)
                {
                    SqlHelper.ExcuteSql("TRUNCATE TABLE T_Customer_CreditHold", null);
                    foreach (string custNumber in listcust)
                    {
                        SqlHelper.ExcuteSql("INSERT INTO T_Customer_CreditHold(CustomerNum) VALUES('" + custNumber + "');", null);
                    }
                }
                return "Import Finished!";
            }
            catch (Exception ex)
            {
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                fileService.CommonRep.Commit();
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public string ImportCurrencyAmount()
        {
            FileType fileT = FileType.CurrencyAmount;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                //upload file to server
                string strMasterDataKey = "ImportCurrencyAmount";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strMasterDataKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + ".xlsx";

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileT);

                return ImportCurrencyAmountDo();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!");
            }
        }

        public string ImportCurrencyAmountDo()
        {
            string strCode = "";
            FileUploadHistory fileUpHis = new FileUploadHistory();
            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            try
            {
                //工作区结束行号
                strCode = Helper.EnumToCode<FileType>(FileType.CurrencyAmount);
                fileUpHis = fileService.GetSuccessData(strCode);
                string strpath = "";
                if (fileUpHis == null)
                {
                    throw new Exception("import file is not found!");
                }
                strpath = fileUpHis.ArchiveFileName;
                List<string> listcust = new List<string>();
                #region openXml
                ExcelPackage package = new ExcelPackage(new FileInfo(strpath));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号
                #endregion
                rowStart = 2;
                int icountsum = 0;
                for (int j = rowStart; j <= rowEnd; j++)
                {
                    if (worksheet.Cells[j, 1].Value == null) { continue; }
                    if (worksheet.Cells[j, 3].Value == null) { continue; }
                    if (worksheet.Cells[j, 4].Value == null) { continue; }
                    string strInvoiceNum = worksheet.Cells[j, 1].Value.ToString();
                    if (!string.IsNullOrEmpty(strInvoiceNum))
                    {
                        //判断是否为有效的发票号
                        var invRow = (from o in CommonRep.GetQueryable<InvoiceAging>()
                                      where o.InvoiceNum == strInvoiceNum
                                      select o).ToList();
                        if (invRow != null && invRow.Count > 0)
                        {
                            DateTime dueDate = Convert.ToDateTime(worksheet.Cells[j, 3].Value);
                            decimal decAmount = Convert.ToDecimal(worksheet.Cells[j, 4].Value);
                            SqlHelper.ExcuteSql("update T_INVOICE_AGING set RemainingAmtTran = " + decAmount.ToString() + ", RemainingAmtTran1 = " + decAmount.ToString() + " " +
                                "where INVOICE_NUM = '" + strInvoiceNum + "' and DUE_DATE = '" + dueDate.ToString("yyyy-MM-dd") + "'; ", null);
                            icountsum++;
                        }
                    }
                }
                return "Import Finished!" + "Updated " + icountsum + " invoices.";
            }
            catch (Exception ex)
            {
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                fileService.CommonRep.Commit();
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public string ImportVarDataOnly()
        {
            FileType fileT = FileType.VarData;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                //upload file to server
                string strVarDataKey = "ImportVarData";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strVarDataKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }

                var typeName = Path.GetExtension(files[0].FileName);

                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + typeName;

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileT);

                return ImportVarData();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!" + ex.Message);
            }
        }

        public string ImportCustomer()
        {
            string strCode = "";
            FileUploadHistory fileUpHis = new FileUploadHistory();
            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");

            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
            var collecotrList = ",";
            var userId = AppContext.Current.User.EID;
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");

            IQueryable<CustomerMasterData> exportcust = CommonRep.GetQueryable<CustomerMasterData>().Where(m => m.Deal == CurrentDeal);

            if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
            {
                //exportcust = exportcust.Where(o => o.Collector == null || o.Collector.Trim() == string.Empty || collecotrList.Contains("," + o.Collector + ","));
            }
            else
            {
                exportcust = exportcust.Where(o => collecotrList.Contains("," + o.Collector + ","));
            }

            try
            {
                strCode = Helper.EnumToCode<FileType>(FileType.Customer);
                fileUpHis = fileService.GetSuccessData(strCode);
                string strpath = "";
                IQueryable<Customer> custlist = GetCustomer();
                Customer cust = new Customer();
                if (fileUpHis == null)
                {
                    throw new Exception("import file is not found!");
                }
                strpath = fileUpHis.ArchiveFileName;
                NpoiHelper helper = new NpoiHelper(strpath);
                string sheetName = "";
                sheetName = "Customer";
                helper.ActiveSheetName = sheetName;
                int i = 1;
                //added by zhangYu 20160128
                List<Customer> listcust = new List<Customer>();

                //when excel have one row
                while (helper.GetValue(i, 1) != null)
                {
                    //get value from excel
                    var num = helper.GetValue(i, 0).ToString();
                    var siteUseId = helper.GetValue(i, 3).ToString();
                    cust = custlist.Where(o => o.CustomerNum == num && o.SiteUseId == siteUseId).FirstOrDefault();
                    if (cust == null)
                    {
                        Customer newcust = new Customer();
                        newcust.STATUS = "1";

                        if (helper.GetValue(i, 0) != null)
                        {
                            newcust.CustomerNum = helper.GetValue(i, 0).ToString();
                        }
                        else
                        {
                            newcust.CustomerNum = "";
                        }

                        if (helper.GetValue(i, 1) != null)
                        {
                            newcust.CustomerName = helper.GetValue(i, 1).ToString();
                        }
                        else
                        {
                            newcust.CustomerName = "";
                        }

                        if (helper.GetValue(i, 2) != null)
                        {
                            newcust.ExcludeFlg = helper.GetValue(i, 2).ToString();
                        }
                        else
                        {
                            newcust.ExcludeFlg = "";
                        }

                        if (helper.GetValue(i, 3) != null)
                        {
                            newcust.SiteUseId = helper.GetValue(i, 3).ToString();
                        }
                        else
                        {
                            newcust.SiteUseId = "";
                        }

                        if (helper.GetValue(i, 4) != null)
                        {
                            newcust.Country = helper.GetValue(i, 4).ToString();
                        }
                        else
                        {
                            newcust.Country = "";
                        }

                        if (helper.GetValue(i, 5) != null)
                        {
                            newcust.Organization = helper.GetValue(i, 5).ToString();
                        }
                        else
                        {
                            newcust.Organization = "";
                        }

                        if (helper.GetValue(i, 6) != null)
                        {
                            newcust.Ebname = helper.GetValue(i, 6).ToString();
                        }
                        else
                        {
                            newcust.Ebname = "";
                        }

                        if (helper.GetValue(i, 7) != null)
                        {
                            newcust.IsHoldFlg = helper.GetValue(i, 7).ToString();
                        }
                        else
                        {
                            newcust.IsHoldFlg = "";
                        }

                        if (helper.GetValue(i, 8) != null)
                        {
                            newcust.CreditTrem = helper.GetValue(i, 8).ToString();
                        }
                        else
                        {
                            newcust.CreditTrem = "";
                        }

                        if (helper.GetValue(i, 9) != null)
                        {
                            newcust.Collector = helper.GetValue(i, 9).ToString();
                        }
                        else
                        {
                            newcust.Collector = "";
                        }

                        if (helper.GetValue(i, 10) != null)
                        {
                            newcust.SpecialNotes = helper.GetValue(i, 10).ToString();
                        }
                        else
                        {
                            newcust.SpecialNotes = "";
                        }

                        if (helper.GetValue(i, 11) != null)
                        {
                            newcust.CUSTOMER_SERVICE = helper.GetValue(i, 11).ToString();
                        }
                        else
                        {
                            newcust.CUSTOMER_SERVICE = "";
                        }

                        if (helper.GetValue(i, 12) != null)
                        {
                            newcust.Sales = helper.GetValue(i, 12).ToString();
                        }
                        else
                        {
                            newcust.Sales = "";
                        }

                        if (helper.GetValue(i, 13) != null)
                        {
                            newcust.LOCALIZE_CUSTOMER_NAME = helper.GetValue(i, 13).ToString();
                        }
                        else
                        {
                            newcust.LOCALIZE_CUSTOMER_NAME = "";
                        }

                        if (helper.GetValue(i, 14) != null)
                        {
                            newcust.AMTLimit = Decimal.Parse(helper.GetValue(i, 14).ToString());
                        }
                        else
                        {
                            newcust.AMTLimit = 0;
                        }

                        if (helper.GetValue(i, 15) != null)
                        {
                            newcust.CreditLimit = Decimal.Parse(helper.GetValue(i, 15).ToString());
                        }
                        else
                        {
                            newcust.CreditLimit = 0;
                        }


                        newcust.CreateTime = AppContext.Current.User.Now;
                        newcust.Operator = AppContext.Current.User.EID;
                        newcust.Deal = AppContext.Current.User.Deal;
                        newcust.RemoveFlg = "1";
                        listcust.Add(newcust);

                    }
                    else
                    {
                        var isPower = exportcust.Where(p => p.Id == cust.Id);
                        if (isPower.Count() == 0)
                        {
                            return "Data is not within the scope of permissions";
                        }
                        var old = cust;
                        cust.UpdateTime = AppContext.Current.User.Now;
                        if (helper.GetValue(i, 0) != null)
                        {
                            cust.CustomerNum = helper.GetValue(i, 0).ToString();
                        }
                        else
                        {
                            cust.CustomerNum = "";
                        }

                        if (helper.GetValue(i, 1) != null)
                        {
                            cust.CustomerName = helper.GetValue(i, 1).ToString();
                        }
                        else
                        {
                            cust.CustomerName = "";
                        }

                        if (helper.GetValue(i, 2) != null)
                        {
                            cust.ExcludeFlg = helper.GetValue(i, 2).ToString();
                        }
                        else
                        {
                            cust.ExcludeFlg = "";
                        }

                        if (helper.GetValue(i, 3) != null)
                        {
                            cust.SiteUseId = helper.GetValue(i, 3).ToString();
                        }
                        else
                        {
                            cust.SiteUseId = "";
                        }

                        if (helper.GetValue(i, 4) != null)
                        {
                            cust.Country = helper.GetValue(i, 4).ToString();
                        }
                        else
                        {
                            cust.Country = "";
                        }

                        if (helper.GetValue(i, 5) != null)
                        {
                            cust.Organization = helper.GetValue(i, 5).ToString();
                        }
                        else
                        {
                            cust.Organization = "";
                        }

                        if (helper.GetValue(i, 6) != null)
                        {
                            cust.Ebname = helper.GetValue(i, 6).ToString();
                        }
                        else
                        {
                            cust.Ebname = "";
                        }

                        if (helper.GetValue(i, 7) != null)
                        {
                            cust.IsHoldFlg = helper.GetValue(i, 7).ToString();
                        }
                        else
                        {
                            cust.IsHoldFlg = "";
                        }

                        if (helper.GetValue(i, 8) != null)
                        {
                            cust.CreditTrem = helper.GetValue(i, 8).ToString();
                        }
                        else
                        {
                            cust.CreditTrem = "";
                        }

                        if (helper.GetValue(i, 9) != null)
                        {
                            cust.Collector = helper.GetValue(i, 9).ToString();
                        }
                        else
                        {
                            cust.Collector = "";
                        }

                        if (helper.GetValue(i, 10) != null)
                        {
                            cust.SpecialNotes = helper.GetValue(i, 10).ToString();
                        }
                        else
                        {
                            cust.SpecialNotes = "";
                        }

                        if (helper.GetValue(i, 11) != null)
                        {
                            cust.CUSTOMER_SERVICE = helper.GetValue(i, 11).ToString();
                        }
                        else
                        {
                            cust.CUSTOMER_SERVICE = "";
                        }

                        if (helper.GetValue(i, 12) != null)
                        {
                            cust.Sales = helper.GetValue(i, 12).ToString();
                        }
                        else
                        {
                            cust.Sales = "";
                        }

                        if (helper.GetValue(i, 13) != null)
                        {
                            cust.LOCALIZE_CUSTOMER_NAME = helper.GetValue(i, 13).ToString();
                        }
                        else
                        {
                            cust.LOCALIZE_CUSTOMER_NAME = "";
                        }

                        if (helper.GetValue(i, 14) != null)
                        {
                            cust.AMTLimit = Decimal.Parse(helper.GetValue(i, 14).ToString());
                        }
                        else
                        {
                            cust.AMTLimit = 0;
                        }

                        if (helper.GetValue(i, 15) != null)
                        {
                            cust.CreditLimit = Decimal.Parse(helper.GetValue(i, 15).ToString());
                        }
                        else
                        {
                            cust.CreditLimit = 0;
                        }

                        cust.Operator = AppContext.Current.User.EID;
                        ObjectHelper.CopyObjectWithUnNeed(cust, old, new string[] { "id", "CustomerNum", "SiteUseId" });
                    }
                    i = i + 1;
                }

                CommonRep.AddRange(listcust);
                CommonRep.Commit();
                //更新Task作业者
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(@"UPDATE t SET t.EID = c.COLLECTOR
                                                                        FROM dbo.T_CUSTOMER c  with (nolock) 
                                                                        JOIN dbo.T_COLLECTOR_ALERT t
                                                                        ON t.CUSTOMER_NUM = c.CUSTOMER_NUM
                                                                        AND t.SiteUseId = c.SiteUseId
                                                                        WHERE c.CUSTOMER_NUM IS NOT NULL
                                                                        AND c.COLLECTOR <> t.EID
                                                                        AND t.STATUS ='Initialized'
                                                                        AND c.REMOVE_FLG = '1'");
                //更新Dispute作业者
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(@"UPDATE t SET t.EID = c.COLLECTOR
                                                                        FROM dbo.T_CUSTOMER c  with (nolock) 
                                                                        JOIN dbo.T_DISPUTE t
                                                                        ON t.CUSTOMER_NUM = c.CUSTOMER_NUM
                                                                        AND t.SiteUseId = c.SiteUseId
                                                                        WHERE c.CUSTOMER_NUM IS NOT NULL
                                                                        AND c.COLLECTOR <> t.EID
                                                                        AND t.STATUS <> '026011'
                                                                        AND t.STATUS <> '026012'
                                                                        AND c.REMOVE_FLG = '1'");
                return "Import Finished!";
            }
            catch (DbEntityValidationException ex)
            {
                Helper.Log.Error(ex.Message, ex);

                StringBuilder errors = new StringBuilder();
                IEnumerable<DbEntityValidationResult> validationResult = ex.EntityValidationErrors;
                foreach (DbEntityValidationResult result in validationResult)
                {
                    ICollection<DbValidationError> validationError = result.ValidationErrors;
                    foreach (DbValidationError err in validationError)
                    {
                        errors.Append(err.PropertyName + ":" + err.ErrorMessage + "\r\n");
                    }
                }
                return errors.ToString();
            }
            catch (Exception ex)
            {
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                fileService.CommonRep.Commit();
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public string ImportCustomerPayment()
        {
            string strCode = "";
            FileUploadHistory fileUpHis = new FileUploadHistory();
            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");

            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
            var collecotrList = ",";
            var userId = AppContext.Current.User.EID;
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");

            IQueryable<CustomerMasterData> exportcust = CommonRep.GetQueryable<CustomerMasterData>().Where(m => m.Deal == CurrentDeal);

            if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
            {
                //exportcust = exportcust.Where(o => o.Collector == null || o.Collector.Trim() == string.Empty || collecotrList.Contains("," + o.Collector + ","));
            }
            else
            {
                exportcust = exportcust.Where(o => collecotrList.Contains("," + o.Collector + ","));
            }
            var custList = exportcust.ToList();

            try
            {
                strCode = Helper.EnumToCode<FileType>(FileType.CustPayment);
                fileUpHis = fileService.GetSuccessData(strCode);
                string strpath = "";
                IQueryable<Customer> custlist = GetCustomer();
                Customer cust = new Customer();
                if (fileUpHis == null)
                {
                    throw new Exception("import file is not found!");
                }
                strpath = fileUpHis.ArchiveFileName;
                #region openXml
                ExcelPackage package = new ExcelPackage(new FileInfo(strpath));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号
                #endregion

                List<CustomerPaymentCircle> listPaymentCircle = new List<CustomerPaymentCircle>();
                string strCustomer = "";
                string strSiteUseId = "";
                DateTime dt_PaymentDate;
                int intMonths = 1;
                CustomerMasterData findCust = new CustomerMasterData();

                for (int j = rowStart + 1; j <= rowEnd; j++)
                {
                    if (worksheet.Cells[j, 1].Value != null)
                    {
                        strCustomer = worksheet.Cells[j, 1].Value.ToString();
                    }
                    else
                    {
                        throw new Exception("第" + j.ToString() + "条记录, CustomerNum不能为空!");
                    }
                    if (worksheet.Cells[j, 2].Value != null)
                    {
                        strSiteUseId = worksheet.Cells[j, 2].Value.ToString();
                    }
                    else
                    {
                        throw new Exception("第" + j.ToString() + "条记录, SiteUseId不能为空!");
                    }
                    findCust = custList.Find(o => o.CustomerNum == strCustomer && o.SiteUseId == strSiteUseId);
                    if (findCust == null)
                    {
                        throw new Exception("第" + j.ToString() + "条记录, CustomerNum:" + strCustomer + ",SiteUseId:" + strSiteUseId + ",客户信息不存在或没权限操作!");
                    }
                    if (worksheet.Cells[j, 3].Value != null)
                    {
                        try
                        {
                            dt_PaymentDate = Convert.ToDateTime(worksheet.Cells[j, 3].Value.ToString());
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("第" + j.ToString() + "条记录, PaymentDate数据格式异常(YYYY-MM-DD)!");
                        }
                    }
                    else
                    {
                        throw new Exception("第" + j.ToString() + "条记录, PaymentDate不能为空!");
                    }
                    if (worksheet.Cells[j, 4].Value != null)
                    {
                        intMonths = Convert.ToInt32(worksheet.Cells[j, 4].Value.ToString());
                    }
                    else
                    {
                        throw new Exception("第" + j.ToString() + "条记录, Months不能为空!");
                    }

                    CustomerPaymentCircle pay = new CustomerPaymentCircle();
                    pay.Deal = CurrentDeal;
                    pay.LegalEntity = findCust.Organization;
                    pay.CustomerNum = strCustomer;
                    pay.SiteUseId = strSiteUseId;
                    pay.Reconciliation_Day = dt_PaymentDate;
                    pay.CreatePersonId = AppContext.Current.User.Name;
                    pay.CreateDate = AppContext.Current.User.Now;
                    pay.Flg = "1";
                    listPaymentCircle.Add(pay);
                    for (int ii = 1; ii < intMonths; ii++)
                    {
                        pay = new CustomerPaymentCircle();
                        pay.Deal = CurrentDeal;
                        pay.LegalEntity = findCust.Organization;
                        pay.CustomerNum = strCustomer;
                        pay.SiteUseId = strSiteUseId;
                        pay.Reconciliation_Day = dt_PaymentDate.AddMonths(ii);
                        pay.CreatePersonId = AppContext.Current.User.Name;
                        pay.CreateDate = AppContext.Current.User.Now;
                        pay.Flg = "1";
                        listPaymentCircle.Add(pay);
                    }
                }
                CommonRep.AddRange(listPaymentCircle);
                CommonRep.Commit();

                return "Import Finished!";
            }
            catch (DbEntityValidationException ex)
            {
                Helper.Log.Error(ex.Message, ex);

                StringBuilder errors = new StringBuilder();
                IEnumerable<DbEntityValidationResult> validationResult = ex.EntityValidationErrors;
                foreach (DbEntityValidationResult result in validationResult)
                {
                    ICollection<DbValidationError> validationError = result.ValidationErrors;
                    foreach (DbValidationError err in validationError)
                    {
                        errors.Append(err.PropertyName + ":" + err.ErrorMessage + "\r\n");
                    }
                }
                return errors.ToString();
            }
            catch (Exception ex)
            {
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                fileService.CommonRep.Commit();
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public string ImportCustomerContactor()
        {

            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");

            FileUploadHistory fileUpHis = new FileUploadHistory();
            var userId = AppContext.Current.User.EID;
            int count = 0;
            try
            {
                string strCode = Helper.EnumToCode(FileType.MissingContactor);
                fileUpHis = fileService.GetSuccessData(strCode);
                if (fileUpHis == null)
                {
                    throw new Exception("import file is not found!");
                }

                List<Contactor> contactors = new List<Contactor>();
                List<ContactorImportDto> importItems = new List<ContactorImportDto>();

                #region openXml
                string strpath = fileUpHis.ArchiveFileName;

                ExcelPackage package = new ExcelPackage(new FileInfo(strpath));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[2];
                int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号

                for (int j = rowStart + 1; j <= rowEnd; j++)
                {
                    ContactorImportDto item = new ContactorImportDto();

                    if (worksheet.Cells[j, 1].Value != null)
                    {
                        item.Eid = worksheet.Cells[j, 1].Value.ToString();
                    }

                    if (worksheet.Cells[j, 2].Value != null)
                    {
                        item.Region = worksheet.Cells[j, 2].Value.ToString();
                    }

                    if (worksheet.Cells[j, 3].Value != null)
                    {
                        item.EbName = worksheet.Cells[j, 3].Value.ToString();
                    }

                    if (worksheet.Cells[j, 4].Value != null)
                    {
                        item.CreditTerm = worksheet.Cells[j, 4].Value.ToString();
                    }

                    if (worksheet.Cells[j, 5].Value != null)
                    {
                        item.Legal = worksheet.Cells[j, 5].Value.ToString();
                    }

                    if (worksheet.Cells[j, 6].Value != null)
                    {
                        item.CustomerName = worksheet.Cells[j, 6].Value.ToString();
                    }

                    if (worksheet.Cells[j, 7].Value != null)
                    {
                        item.CustomerNum = worksheet.Cells[j, 7].Value.ToString();
                    }

                    if (worksheet.Cells[j, 8].Value != null)
                    {
                        item.SiteUseId = worksheet.Cells[j, 8].Value.ToString();
                    }

                    if (string.IsNullOrWhiteSpace(item.CustomerNum) && string.IsNullOrWhiteSpace(item.SiteUseId))
                        continue;

                    if (worksheet.Cells[j, 9].Value != null)
                    {
                        item.Customer = worksheet.Cells[j, 9].Value.ToString();
                    }

                    if (worksheet.Cells[j, 10].Value != null)
                    {
                        item.Cs = worksheet.Cells[j, 10].Value.ToString();
                    }

                    if (worksheet.Cells[j, 11].Value != null)
                    {
                        item.Sales = worksheet.Cells[j, 11].Value.ToString();
                    }

                    if (worksheet.Cells[j, 12].Value != null)
                    {
                        item.CustomerEmail = worksheet.Cells[j, 12].Value.ToString();
                    }

                    if (worksheet.Cells[j, 13].Value != null)
                    {
                        item.CsEmail = worksheet.Cells[j, 13].Value.ToString();
                    }

                    if (worksheet.Cells[j, 14].Value != null)
                    {
                        item.SalesEmail = worksheet.Cells[j, 14].Value.ToString();
                    }

                    if (worksheet.Cells[j, 15].Value != null)
                    {
                        item.BranchManager = worksheet.Cells[j, 15].Value.ToString();
                    }

                    if (worksheet.Cells[j, 16].Value != null)
                    {
                        item.CsManager = worksheet.Cells[j, 16].Value.ToString();
                    }

                    if (worksheet.Cells[j, 17].Value != null)
                    {
                        item.SalesManager = worksheet.Cells[j, 17].Value.ToString();
                    }

                    if (worksheet.Cells[j, 18].Value != null)
                    {
                        item.FinancialControllers = worksheet.Cells[j, 18].Value.ToString();
                    }

                    if (worksheet.Cells[j, 19].Value != null)
                    {
                        item.FinancialManagers = worksheet.Cells[j, 19].Value.ToString();
                    }

                    if (worksheet.Cells[j, 20].Value != null)
                    {
                        item.CreditOfficers = worksheet.Cells[j, 20].Value.ToString();
                    }

                    if (worksheet.Cells[j, 21].Value != null)
                    {
                        item.LocalFinance = worksheet.Cells[j, 21].Value.ToString();
                    }

                    if (worksheet.Cells[j, 22].Value != null)
                    {
                        item.FinanceLeader = worksheet.Cells[j, 22].Value.ToString();
                    }

                    if (worksheet.Cells[j, 23].Value != null)
                    {
                        item.CreditManager = worksheet.Cells[j, 23].Value.ToString();
                    }

                    importItems.Add(item);
                }

                #endregion

                //CS, Sales , Credit Officer,Finance Manager,Branch Manager, CS Manager, Sales Manager, Local Finance, Collector

                List<Contactor> addContactors = new List<Contactor>();
                importItems.Sort((a, b) => a.SiteUseId.CompareTo(b.SiteUseId));
                string strPreSiteUseId = "";
                foreach (var item in importItems)
                {
                    count++;
                    if (count == 7)
                    {
                        count = count;
                    }
                    if (item.SiteUseId == "HK88_5983")
                    {
                        item.Sales = item.Sales;
                    }
                    var groupContactors = new List<Contactor>();

                    if (strPreSiteUseId == "" || strPreSiteUseId != item.SiteUseId)
                    {
                        groupContactors = CommonRep.GetQueryable<Contactor>().Where(o => o.CustomerNum == item.CustomerNum && o.SiteUseId == item.SiteUseId).ToList();
                        CommonRep.RemoveRange(groupContactors);
                        groupContactors.Clear();
                    }

                    var customer = CommonRep.GetQueryable<Customer>().Where(o => o.CustomerNum == item.CustomerNum && o.SiteUseId == item.SiteUseId).FirstOrDefault();

                    if (customer != null)
                    {
                        //Customer
                        if (string.IsNullOrWhiteSpace(item.CustomerEmail) || item.CustomerEmail.Contains("不催") || item.CustomerEmail.Contains("無"))
                        {
                            if (!string.IsNullOrWhiteSpace(item.Customer))
                            {
                                var cust = groupContactors.FirstOrDefault(o => o.Title == "Customer" && o.Name == item.Customer);
                                if (cust == null)
                                {
                                    Contactor add = new Contactor();
                                    add.CustomerNum = item.CustomerNum;
                                    add.Name = item.Customer;
                                    add.Title = "Customer";
                                    add.IsDefaultFlg = "1";
                                    add.ToCc = "1";
                                    add.CommunicationLanguage = customer.ContactLanguage;
                                    add.SiteUseId = item.SiteUseId;
                                    add.LegalEntity = "All";
                                    add.Deal = AppContext.Current.User.Deal;
                                    add.IsCostomerContact = true;

                                    if (addContactors.Count(o => o.CustomerNum == add.CustomerNum && o.SiteUseId == add.SiteUseId && o.Title == add.Title && o.Name == add.Name) == 0)
                                    {
                                        addContactors.Add(add);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!checkEmail(item.CustomerEmail))
                            {
                                throw new Exception("【" + item.CustomerEmail + "】, mail address invalid!");
                            }
                            var cust = groupContactors.FirstOrDefault(o => o.Title == "Customer" && o.EmailAddress == item.CustomerEmail);
                            if (cust == null)
                            {
                                Contactor add = new Contactor();
                                add.CustomerNum = item.CustomerNum;
                                add.Name = item.Customer;
                                add.EmailAddress = item.CustomerEmail;
                                add.Title = "Customer";
                                add.IsDefaultFlg = "1";
                                add.ToCc = "1";
                                add.CommunicationLanguage = customer.ContactLanguage;
                                add.SiteUseId = item.SiteUseId;
                                add.LegalEntity = "All";
                                add.Deal = AppContext.Current.User.Deal;
                                if (addContactors.Count(o => o.CustomerNum == add.CustomerNum && o.SiteUseId == add.SiteUseId && o.Title == add.Title && o.EmailAddress == add.EmailAddress) == 0)
                                {
                                    addContactors.Add(add);
                                }
                            }
                            else
                            {
                                if (cust.Name != item.Customer)
                                {
                                    cust.Name = item.Customer;
                                    CommonRep.Commit();
                                }
                            }
                        }

                        //CS
                        if (string.IsNullOrWhiteSpace(item.CsEmail) || item.CsEmail.Contains("不催") || item.CsEmail.Contains("無"))
                        {
                            if (!string.IsNullOrWhiteSpace(item.Cs))
                            {
                                var cs = groupContactors.FirstOrDefault(o => o.Title == "CS" && o.Name == item.Cs);
                                if (cs == null)
                                {
                                    Contactor add = new Contactor();
                                    add.CustomerNum = item.CustomerNum;
                                    add.Name = item.Cs;
                                    add.Title = "CS";
                                    add.IsDefaultFlg = "1";
                                    add.ToCc = "1";
                                    add.CommunicationLanguage = customer.ContactLanguage;
                                    add.SiteUseId = item.SiteUseId;
                                    add.LegalEntity = "All";
                                    add.Deal = AppContext.Current.User.Deal;
                                    add.IsCostomerContact = true;

                                    if (addContactors.Count(o => o.CustomerNum == add.CustomerNum && o.SiteUseId == add.SiteUseId && o.Title == add.Title && o.Name == add.Name) == 0)
                                    {
                                        addContactors.Add(add);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!checkEmail(item.CsEmail))
                            {
                                throw new Exception("【" + item.CsEmail + "】, mail address invalid!");
                            }
                            var cs = groupContactors.FirstOrDefault(o => o.Title == "CS" && o.EmailAddress == item.CsEmail);
                            if (cs == null)
                            {
                                Contactor add = new Contactor();
                                add.CustomerNum = item.CustomerNum;
                                add.Name = item.Cs;
                                add.EmailAddress = item.CsEmail;
                                add.Title = "CS";
                                add.IsDefaultFlg = "1";
                                add.ToCc = "1";
                                add.CommunicationLanguage = customer.ContactLanguage;
                                add.SiteUseId = item.SiteUseId;
                                add.LegalEntity = "All";
                                add.Deal = AppContext.Current.User.Deal;
                                if (addContactors.Count(o => o.CustomerNum == add.CustomerNum && o.SiteUseId == add.SiteUseId && o.Title == add.Title && o.EmailAddress == add.EmailAddress) == 0)
                                {
                                    addContactors.Add(add);
                                }
                            }
                            else
                            {
                                if (cs.Name != item.Cs)
                                {
                                    cs.Name = item.Cs;
                                    CommonRep.Commit();
                                }
                            }
                        }

                        //Sales
                        if (string.IsNullOrWhiteSpace(item.SalesEmail) || item.SalesEmail.Contains("不催") || item.SalesEmail.Contains("無"))
                        {
                            if (!string.IsNullOrWhiteSpace(item.Sales))
                            {
                                var sales = groupContactors.FirstOrDefault(o => o.Title == "Sales" && o.Name == item.Sales);
                                if (sales == null)
                                {
                                    Contactor add = new Contactor();
                                    add.CustomerNum = item.CustomerNum;
                                    add.Name = item.Sales;
                                    add.Title = "Sales";
                                    add.IsDefaultFlg = "1";
                                    add.ToCc = "1";
                                    add.CommunicationLanguage = customer.ContactLanguage;
                                    add.SiteUseId = item.SiteUseId;
                                    add.LegalEntity = "All";
                                    add.Deal = AppContext.Current.User.Deal;

                                    if (addContactors.Count(o => o.CustomerNum == add.CustomerNum && o.SiteUseId == add.SiteUseId && o.Title == add.Title && o.Name == add.Name) == 0)
                                    {
                                        addContactors.Add(add);
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (!checkEmail(item.SalesEmail))
                            {
                                throw new Exception("【" + item.SalesEmail + "】, mail address invalid!");
                            }
                            var cs = groupContactors.FirstOrDefault(o => o.Title == "Sales" && o.EmailAddress == item.SalesEmail);
                            if (cs == null)
                            {
                                Contactor add = new Contactor();
                                add.CustomerNum = item.CustomerNum;
                                add.Name = item.Sales;
                                add.EmailAddress = item.SalesEmail;
                                add.Title = "Sales";
                                add.IsDefaultFlg = "1";
                                add.ToCc = "1";
                                add.CommunicationLanguage = customer.ContactLanguage;
                                add.SiteUseId = item.SiteUseId;
                                add.LegalEntity = "All";
                                add.Deal = AppContext.Current.User.Deal;

                                if (addContactors.Count(o => o.CustomerNum == add.CustomerNum && o.SiteUseId == add.SiteUseId && o.Title == add.Title && o.EmailAddress == add.EmailAddress) == 0)
                                {
                                    addContactors.Add(add);
                                }
                            }
                            else
                            {
                                if (cs.Name != item.Sales)
                                {
                                    cs.Name = item.Sales;
                                    CommonRep.Commit();
                                }
                            }
                        }

                        //Branch Manager
                        AddContactor(item.BranchManager, "Branch Manager", item, groupContactors, addContactors, customer.ContactLanguage);
                        //CS Manager
                        AddContactor(item.CsManager, "CS Manager", item, groupContactors, addContactors, customer.ContactLanguage);
                        //Sales Manager
                        AddContactor(item.SalesManager, "Sales Manager", item, groupContactors, addContactors, customer.ContactLanguage);
                        //Financial Controller
                        AddContactor(item.FinancialControllers, "Financial Controller", item, groupContactors, addContactors, customer.ContactLanguage);
                        //Financial Manager
                        AddContactor(item.FinancialManagers, "Finance Manager", item, groupContactors, addContactors, customer.ContactLanguage);
                        //Financial Manager
                        AddContactor(item.CreditOfficers, "Credit Officer", item, groupContactors, addContactors, customer.ContactLanguage);
                        //Local Finance
                        AddContactor(item.LocalFinance, "Local Finance", item, groupContactors, addContactors, customer.ContactLanguage);
                        //Finance Leader
                        AddContactor(item.FinanceLeader, "Finance Leader", item, groupContactors, addContactors, customer.ContactLanguage);
                        //Credit Manager
                        AddContactor(item.CreditManager, "Credit Manager", item, groupContactors, addContactors, customer.ContactLanguage);
                    }

                    strPreSiteUseId = item.SiteUseId;
                }
                CommonRep.BulkInsert(addContactors);
                CommonRep.Commit();

                return "Import Finished!";
            }
            catch (DbEntityValidationException ex)
            {
                Helper.Log.Error(ex.Message + count.ToString(), ex);

                StringBuilder errors = new StringBuilder();
                IEnumerable<DbEntityValidationResult> validationResult = ex.EntityValidationErrors;
                foreach (DbEntityValidationResult result in validationResult)
                {
                    ICollection<DbValidationError> validationError = result.ValidationErrors;
                    foreach (DbValidationError err in validationError)
                    {
                        errors.Append(err.PropertyName + ":" + err.ErrorMessage + "\r\n");
                    }
                }
                return errors.ToString();
            }
            catch (Exception ex)
            {
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                fileService.CommonRep.Commit();
                Helper.Log.Error(ex.Message + count.ToString(), ex);
                throw ex;
            }
        }

        public string ImportCustomerComment()
        {

            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");

            FileUploadHistory fileUpHis = new FileUploadHistory();
            var userId = AppContext.Current.User.EID;
            int count = 0;
            try
            {
                string strCode = Helper.EnumToCode(FileType.CustComment);
                fileUpHis = fileService.GetSuccessData(strCode);
                if (fileUpHis == null)
                {
                    throw new Exception("import file is not found!");
                }

                #region openXml
                string strpath = fileUpHis.ArchiveFileName;

                ExcelPackage package = new ExcelPackage(new FileInfo(strpath));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号

                List<string> listSQL = new List<string>();
                for (int j = rowStart + 1; j <= rowEnd; j++)
                {
                    count = j;
                    string strSiteUseId = worksheet.Cells[j, 1].Value == null ? "" : worksheet.Cells[j, 1].Value.ToString().Trim();
                    string strComment = worksheet.Cells[j, 2].Value == null ? "" : worksheet.Cells[j, 2].Value.ToString().Trim().Replace("'", "''");
                    string strCommentExpire = worksheet.Cells[j, 3].Value == null ? "" : worksheet.Cells[j, 3].Value.ToString().Trim();
                    DateTime? dt_CommentExpireDate = null;
                    if (!string.IsNullOrEmpty(strCommentExpire))
                    {
                        try
                        {
                            dt_CommentExpireDate = Convert.ToDateTime(strCommentExpire);
                        }
                        catch (Exception ex)
                        {
                            return "Import Failed. Row:" + count + " CommentExpirationDate invalid.";
                        }
                    }
                    if (string.IsNullOrEmpty(strSiteUseId)) { continue; }
                    if (string.IsNullOrEmpty(strComment))
                    {
                        listSQL.Add("update t_customer set comment = N'" + strComment + "', CommentExpirationDate = null, CommentLastDate = null where siteuseid = '" + strSiteUseId + "'");
                    }
                    else
                    {
                        Customer old = CommonRep.GetQueryable<Customer>().FirstOrDefault(o => o.SiteUseId == strSiteUseId);

                        // INSERT T_Customer_ExpirationDateHis
                        if (
                        dt_CommentExpireDate != null &&
                        !Convert.ToDateTime(old.CommentExpirationDate).Equals(dt_CommentExpireDate))
                        {
                            T_Customer_ExpirationDateHis cusExpDateHis = new T_Customer_ExpirationDateHis();
                            cusExpDateHis.OldCommentExpirationDate = old.CommentExpirationDate;
                            cusExpDateHis.NewCommentExpirationDate = dt_CommentExpireDate;
                            cusExpDateHis.UserId = AppContext.Current.User.EID; //当前用户ID
                            cusExpDateHis.ChangeDate = DateTime.Now;
                            cusExpDateHis.SiteUseId = strSiteUseId;
                            cusExpDateHis.CustomerNum = old.CustomerNum;
                            cusExpDateHis.Comment = strComment;
                            CommonRep.Add(cusExpDateHis);
                            CommonRep.Commit();
                        }
                        listSQL.Add("update t_customer set comment = N'" + strComment + "' where siteuseid = '" + strSiteUseId + "'");

                        string strCommentExpirationDateString = "null";
                        if (dt_CommentExpireDate != null)
                        {
                            strCommentExpirationDateString = "'" + Convert.ToDateTime(dt_CommentExpireDate).ToString("yyyy-MM-dd") + "'";
                            listSQL.Add("update t_customer set CommentExpirationDate = " + strCommentExpirationDateString + " where siteuseid = '" + strSiteUseId + "'");
                        }
                        string strCommentLastDateStrin = "null";
                        if (old.Comment != strComment)
                        {
                            strCommentLastDateStrin = "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                            listSQL.Add("update t_customer set CommentLastDate = " + strCommentLastDateStrin + " where siteuseid = '" + strSiteUseId + "'");
                        }
                    }
                }
                package.Dispose();
                if (listSQL.Count > 0)
                {
                    SqlHelper.ExcuteListSql(listSQL);
                }

                #endregion

                return "Import Finished!";
            }
            catch (DbEntityValidationException ex)
            {
                Helper.Log.Error(ex.Message + count.ToString(), ex);

                return ex.Message + count.ToString();
            }
            catch (Exception ex)
            {
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                Helper.Log.Error(ex.Message + count.ToString(), ex);
                throw ex;
            }
        }

        public string ImportCustomerCommentSales()
        {

            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");

            FileUploadHistory fileUpHis = new FileUploadHistory();
            var userId = AppContext.Current.User.EID;
            int count = 0;
            try
            {
                string strCode = Helper.EnumToCode(FileType.CustCommentsFromCsSales);
                fileUpHis = fileService.GetSuccessData(strCode);
                if (fileUpHis == null)
                {
                    throw new Exception("import file is not found!");
                }

                #region openXml
                string strpath = fileUpHis.ArchiveFileName;

                ExcelPackage package = new ExcelPackage(new FileInfo(strpath));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号

                DateTime dt_Now = DateTime.Now;
                //先删除再插入
                List<string> deleteSiteUseId = new List<string>();
                List<T_Customer_Comments> listComments = new List<T_Customer_Comments>();
                List<SqlParameter[]> parmsList = new List<SqlParameter[]>();
                List<string> listSQLDelete = new List<string>();
                List<string> listSQLInsert = new List<string>();
                for (int j = rowStart + 1; j <= rowEnd; j++)
                {
                    count = j;
                    string customerNum = worksheet.Cells[j, 1].Value == null ? "" : worksheet.Cells[j, 1].Value.ToString().Trim();
                    string strSiteUseId = worksheet.Cells[j, 2].Value == null ? "" : worksheet.Cells[j, 2].Value.ToString().Trim();
                    string strAgingBucket = worksheet.Cells[j, 3].Value == null ? "" : worksheet.Cells[j, 3].Value.ToString().Trim();
                    string strPTPAmount = worksheet.Cells[j, 5].Value == null ? "" : worksheet.Cells[j, 5].Value.ToString().Trim();
                    string strPTPDate = worksheet.Cells[j, 6].Value == null ? "" : worksheet.Cells[j, 6].Value.ToString().Trim();
                    string strOverDueReason = worksheet.Cells[j, 7].Value == null ? "" : worksheet.Cells[j, 7].Value.ToString().Trim().Replace("'", "''");
                    string strComment = worksheet.Cells[j, 8].Value == null ? "" : worksheet.Cells[j, 8].Value.ToString().Trim().Replace("'", "''");
                    string strCommentFrom = worksheet.Cells[j, 9].Value == null ? "" : worksheet.Cells[j, 9].Value.ToString().Trim().Replace("'", "''");
                    if (string.IsNullOrEmpty(customerNum)) { continue; }
                    if (string.IsNullOrEmpty(strSiteUseId)) { continue; }
                    //需要删除Comments的SiteUseId
                    deleteSiteUseId.Add(strSiteUseId);
                    if (string.IsNullOrEmpty(strAgingBucket)) { continue; }
                    //有效的Comment，PTPAmount | PTPDate | OverDueReason | Comments必须有一项信息
                    if (string.IsNullOrEmpty(strPTPAmount) && string.IsNullOrEmpty(strPTPDate) && string.IsNullOrEmpty(strOverDueReason) && string.IsNullOrEmpty(strComment))
                    {
                        continue;
                    }
                    DateTime? dt_PTPDate = null;
                    if (!string.IsNullOrEmpty(strPTPDate))
                    {
                        try
                        {
                            dt_PTPDate = Convert.ToDateTime(strPTPDate);
                            dt_PTPDate = dt_PTPDate <= new DateTime(1900,1,1)?new DateTime(1900,1,1):dt_PTPDate;
                        }
                        catch (Exception ex)
                        {
                            return "Import Failed. Row:" + count + " PTPDate invalid.";
                        }
                    }
                    decimal? dec_PTPAmount = null;
                    if (!string.IsNullOrEmpty(strPTPAmount))
                    {
                        try
                        {
                            dec_PTPAmount = Convert.ToDecimal(strPTPAmount);
                        }
                        catch (Exception ex)
                        {
                            return "Import Failed. Row:" + count + " PTPAmount invalid.";
                        }
                    }
                    //判断相应CustomerNum&SiteUseId&AgingBucket是否已经存在，如果存在则给出提示
                    T_Customer_Comments find = listComments.Find(o => o.CUSTOMER_NUM == customerNum && o.SiteUseId == strSiteUseId && o.AgingBucket == strAgingBucket);
                    if (find != null) {
                        return "Import Failed. Row:" + count + " 客户:" + strSiteUseId + ",AgingBucket:" + strAgingBucket + "重复.";
                    }
                    T_Customer_Comments comments = new T_Customer_Comments();
                    comments.CUSTOMER_NUM = customerNum;
                    comments.SiteUseId = strSiteUseId;
                    comments.AgingBucket = strAgingBucket;
                    comments.PTPAmount = dec_PTPAmount;
                    comments.PTPDATE = dt_PTPDate;
                    comments.OverdueReason = strOverDueReason;
                    comments.Comments = strComment;
                    comments.CommentsFrom = strCommentFrom;
                    listComments.Add(comments);
                }
                package.Dispose();

                if (listComments.Count == 0) {
                    return "No data need to import.";
                }

                if (deleteSiteUseId.Count > 0) {
                    foreach (string siteuseid in deleteSiteUseId)
                    {
                        listSQLDelete.Add(string.Format("update T_Customer_Comments set isDeleted = 1 where SiteUseId = '{0}'", siteuseid));
                    }
                    SqlHelper.ExcuteListSql(listSQLDelete);
                }

                foreach (T_Customer_Comments comments in listComments) {
                    StringBuilder sbsql = new StringBuilder();
                    sbsql.Append(" INSERT INTO T_Customer_Comments (ID,CUSTOMER_NUM, SiteUseId, AgingBucket, PTPAmount, PTPDATE, OverdueReason, Comments, CommentsFrom, CreateUser, CreateDate) ");
                    sbsql.Append(@" VALUES (newid(),@CUSTOMER_NUM, @SiteUseId, @AgingBucket, @PTPAmount, @PTPDATE, @OverdueReason, @Comments, @CommentsFrom, @CreateUser, @CreateDate) ");
                    SqlParameter[] parms = {
                            new SqlParameter("@CUSTOMER_NUM", comments.CUSTOMER_NUM),
                            new SqlParameter("@SiteUseId", comments.SiteUseId),
                            new SqlParameter("@AgingBucket", comments.AgingBucket),
                            (comments.PTPAmount==null ? new SqlParameter("@PTPAmount", DBNull.Value) : new SqlParameter("@PTPAmount", comments.PTPAmount)),
                            (comments.PTPDATE==null ? new SqlParameter("@PTPDATE", DBNull.Value) : new SqlParameter("@PTPDATE", comments.PTPDATE)),
                            new SqlParameter("@OverdueReason", comments.OverdueReason),
                            new SqlParameter("@Comments", comments.Comments),
                            new SqlParameter("@CommentsFrom", comments.CommentsFrom),
                            new SqlParameter("@CreateUser", AppContext.Current.User.EID),
                            new SqlParameter("@CreateDate", dt_Now)
                    };
                    parmsList.Add(parms);
                    listSQLInsert.Add(sbsql.ToString());
                }
                if (listSQLInsert.Count > 0)
                {
                    SqlHelper.ExcuteListSql(listSQLInsert, parmsList);
                }
                //Build Comments from collector, 以'Feedback'开始的后续所有文本(Collector添加的内容必须在该Tag之前，否则会被冲掉)
                List<string> listRebuildComments = reBuildComments(deleteSiteUseId);
                if (listRebuildComments.Count > 0) {
                    SqlHelper.ExcuteListSql(listRebuildComments);
                }
                #endregion

                return "Import Finished!";
            }
            catch (DbEntityValidationException ex)
            {
                Helper.Log.Error(ex.Message + count.ToString(), ex);

                return ex.Message + count.ToString();
            }
            catch (Exception ex)
            {
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                Helper.Log.Error(ex.Message + count.ToString(), ex);
                throw ex;
            }
        }

        public List<string> reBuildComments(List<string> listSiteUseId) {
            List<string> listSQL = new List<string>();
            foreach (string siteUseId in listSiteUseId)
            {
                string sql = string.Format(@"update T_CUSTOMER
                                                set CommentLastDate = getdate(),
                                                    Comment = 
                                                     (case when 
                                                    Replace(SUBSTRING(isnull(comment,''), 0, (case when (charindex('Feedback(',isnull(comment,'')) - 1) < 0 then len(isnull(comment,'')) + 1 else (charindex('Feedback(',isnull(comment,'')) - 1) end)), char(10),'') <> ''
                                                    then 
                                                    Replace(SUBSTRING(isnull(comment,''), 0, (case when (charindex('Feedback(',isnull(comment,'')) - 1) < 0 then len(isnull(comment,'')) + 1 else (charindex('Feedback(',isnull(comment,'')) - 1) end)), char(10),'') + char(10)
                                                    else '' end) +
                                                    (select (case when (select count(*) from T_Customer_Comments WITH (NOLOCK) where isDeleted = 0 and SiteUseId = '{0}') > 0 then 'Feedback(CS/Sales):' else '' end)
                                                     + CHAR(10)  +
                                                isnull((SELECT distinct  STUFF(
					                                                (
						                                                SELECT CHAR(10) + [t].[AgingBucket]
						                                                + ':' + (case when t.ptpdate is not null then ' PTPDate:' + convert(char(10),t.ptpdate,120) + ',' else '' end)
						                                                + (case when t.ptpamount is not null then ' PTPAmount:' + cast(convert(decimal(18,2),t.PTPAmount) as varchar) + ',' else ''  end)
						                                                + (case when isnull(t.OverdueReason,'') <> '' then ' OverDueReason:' + t.OverdueReason + ',' else '' end)
						                                                + (case when isnull(t.Comments,'') <> '' then ' Comments: ' + t.Comments + ',' else '' end)
						                                                FROM dbo.T_Customer_Comments t WITH (NOLOCK)
						                                                WHERE t.isDeleted = 0 and
						                                                t.SiteUseId = comments.SiteUseId
						                                                order by t.AgingBucket desc
						                                                FOR XML PATH('')
					                                                ),
					                                                1,
					                                                1,
					                                                '' ) AS TotalComments
			                                                FROM dbo.T_Customer_Comments AS comments WITH (NOLOCK) 
			                                                where isDeleted = 0 and SiteUseId = '{0}'),''))
			                                                where SiteUseId = '{0}'", siteUseId);
                listSQL.Add(sql);
                //处理Comment Expiration Date
                DateTime? dtNewExpirationDate = SqlHelper.ExcuteScalar<DateTime>(string.Format(@"select isnull(min(ptpdate), convert(datetime, '1900-01-01')) from T_Customer_Comments
			                                                                                        where ptpdate is not null
			                                                                                        and isDeleted <> 1
			                                                                                        and SiteUseId = '{0}'", siteUseId));
                if (dtNewExpirationDate != null && dtNewExpirationDate > Convert.ToDateTime("1900-01-01"))
                {
                    //记录ExpirationDate历史
                    string sqlExpirationDateHis = string.Format(@"insert into T_Customer_ExpirationDateHis (UserId, ChangeDate, CustomerNum, SiteUseId, OldCommentExpirationDate,NewCommentExpirationDate)
                                                                select '{0}', getdate(), comments.CUSTOMER_NUM, comments.SiteUseId, T_CUSTOMER.CommentExpirationDate, comments.PTPDATE
                                                                from T_Customer_Comments as comments join T_CUSTOMER on comments.SiteUseId = T_CUSTOMER.SiteUseId
                                                                 where comments.SiteUseId = '{2}'
                                                                and comments.isDeleted <> 1
                                                                and comments.ptpdate = '{1}'
                                                                and (T_CUSTOMER.CommentExpirationDate is null or T_CUSTOMER.CommentExpirationDate <> '{1}' ) ", AppContext.Current.User.EID, dtNewExpirationDate, siteUseId);
                    listSQL.Add(sqlExpirationDateHis);
                    //变更新的ExpirationDate
                    string sqlUpdateExpirationDate = string.Format("Update T_Customer set CommentExpirationDate = '{0}' where siteuseid = '{1}'", dtNewExpirationDate, siteUseId);
                    listSQL.Add(sqlUpdateExpirationDate);
                }
                else
                {
                    //记录ExpirationDate历史
                    string sqlExpirationDateHis = string.Format(@"insert into T_Customer_ExpirationDateHis (UserId, ChangeDate, CustomerNum, SiteUseId, OldCommentExpirationDate,NewCommentExpirationDate)
                                                                select '{0}', getdate(), comments.CUSTOMER_NUM, comments.SiteUseId, T_CUSTOMER.CommentExpirationDate, null
                                                                from T_Customer_Comments as comments join T_CUSTOMER on comments.SiteUseId = T_CUSTOMER.SiteUseId
                                                                 where comments.SiteUseId = '{2}'
                                                                and comments.isDeleted <> 1
                                                                and comments.ptpdate = '{1}'
                                                                and (T_CUSTOMER.CommentExpirationDate is null or T_CUSTOMER.CommentExpirationDate <> '{1}' ) ", AppContext.Current.User.EID, dtNewExpirationDate, siteUseId);
                    listSQL.Add(sqlExpirationDateHis);
                    //变更新的ExpirationDate
                    string sqlUpdateExpirationDate = string.Format("Update T_Customer set CommentExpirationDate = null where siteuseid = '{0}'", siteUseId);
                    listSQL.Add(sqlUpdateExpirationDate);
                }
            }
            return listSQL;
        }

        public string ImportCustomerEBBranch()
        {

            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");

            FileUploadHistory fileUpHis = new FileUploadHistory();
            int count = 0;
            try
            {
                string strCode = Helper.EnumToCode(FileType.CustEBBranch);
                fileUpHis = fileService.GetSuccessData(strCode);
                if (fileUpHis == null)
                {
                    throw new Exception("import file is not found!");
                }

                #region openXml
                string strpath = fileUpHis.ArchiveFileName;

                ExcelPackage package = new ExcelPackage(new FileInfo(strpath));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号

                List<string> listSQL = new List<string>();
                listSQL.Add("update t_customer set branch = null");
                for (int j = rowStart + 1; j <= rowEnd; j++)
                {
                    count = j;
                    string strEBName = worksheet.Cells[j, 1].Value == null ? "" : worksheet.Cells[j, 1].Value.ToString().Trim();
                    string strBranch = worksheet.Cells[j, 2].Value == null ? "" : worksheet.Cells[j, 2].Value.ToString().Trim().Replace("'", "''");
                    if (string.IsNullOrEmpty(strEBName)) { continue; }
                    listSQL.Add("update t_customer set branch = N'" + strBranch + "' where ebname = '" + strEBName + "'");
                }
                package.Dispose();
                if (listSQL.Count > 0)
                {
                    SqlHelper.ExcuteListSql(listSQL);
                }

                #endregion

                return "Import Finished!";
            }
            catch (DbEntityValidationException ex)
            {
                Helper.Log.Error(ex.Message + count.ToString(), ex);

                return ex.Message + count.ToString();
            }
            catch (Exception ex)
            {
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                Helper.Log.Error(ex.Message + count.ToString(), ex);
                throw ex;
            }
        }

        public string ImportCustomerLitigation()
        {

            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");

            FileUploadHistory fileUpHis = new FileUploadHistory();
            int count = 0;
            try
            {
                string strCode = Helper.EnumToCode(FileType.CustLitigation);
                fileUpHis = fileService.GetSuccessData(strCode);
                if (fileUpHis == null)
                {
                    throw new Exception("import file is not found!");
                }

                #region openXml
                string strpath = fileUpHis.ArchiveFileName;

                ExcelPackage package = new ExcelPackage(new FileInfo(strpath));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号

                List<string> listSQL = new List<string>();
                listSQL.Add("update t_customer set Litigation = null");
                for (int j = rowStart + 1; j <= rowEnd; j++)
                {
                    count = j;
                    string strSiteUseId = worksheet.Cells[j, 3].Value == null ? "" : worksheet.Cells[j, 3].Value.ToString().Trim();
                    string strLitigation = worksheet.Cells[j, 5].Value == null ? "" : worksheet.Cells[j, 5].Value.ToString().Trim().Replace("'", "''");
                    if (string.IsNullOrEmpty(strSiteUseId)) { continue; }
                    listSQL.Add("update t_customer set Litigation = N'" + strLitigation + "' where SiteUseId = '" + strSiteUseId + "'");
                }
                package.Dispose();
                if (listSQL.Count > 0)
                {
                    SqlHelper.ExcuteListSql(listSQL);
                }

                #endregion

                return "Import Finished!";
            }
            catch (DbEntityValidationException ex)
            {
                Helper.Log.Error(ex.Message + count.ToString(), ex);

                return ex.Message + count.ToString();
            }
            catch (Exception ex)
            {
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                Helper.Log.Error(ex.Message + count.ToString(), ex);
                throw ex;
            }
        }

        public string ImportCustomerBadDebt()
        {

            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");

            FileUploadHistory fileUpHis = new FileUploadHistory();
            int count = 0;
            try
            {
                string strCode = Helper.EnumToCode(FileType.CustBadDebt);
                fileUpHis = fileService.GetSuccessData(strCode);
                if (fileUpHis == null)
                {
                    throw new Exception("import file is not found!");
                }

                #region openXml
                string strpath = fileUpHis.ArchiveFileName;

                List<string> listSQL = new List<string>();
                listSQL.Add("update t_customer set BadDebt = null");
                ExcelPackage package = new ExcelPackage(new FileInfo(strpath));
                ExcelWorksheets sheets = package.Workbook.Worksheets;
                for (int i = 1; i <= sheets.Count(); i++)
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[i];
                    if (worksheet == null || worksheet.Dimension == null) { continue; }
                    int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                    int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号

                    for (int j = rowStart + 2; j <= rowEnd; j++)
                    {
                        count = j;
                        if (worksheet.Cells[j, 9] == null || worksheet.Cells[j, 59] == null) { continue; }
                        string strSiteUseId = worksheet.Cells[j, 9].Value == null ? "" : worksheet.Cells[j, 9].Value.ToString().Trim().Replace("'", "''");
                        string strBadDebt = worksheet.Cells[j, 59].Value == null ? "" : worksheet.Cells[j, 59].Value.ToString();
                        decimal ldecBadDebt = 0;
                        if (string.IsNullOrEmpty(strSiteUseId)) { continue; }
                        try
                        {
                            Decimal.TryParse(strBadDebt, out ldecBadDebt);
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                        if (ldecBadDebt == 0) { continue; }
                        listSQL.Add("update t_customer set BadDebt = " + ldecBadDebt + " where SiteUseId = '" + strSiteUseId + "'");
                    }
                }
                package.Dispose();
                if (listSQL.Count > 0)
                {
                    SqlHelper.ExcuteListSql(listSQL);
                }

                #endregion

                return "Import Finished!";
            }
            catch (DbEntityValidationException ex)
            {
                Helper.Log.Error(ex.Message + count.ToString(), ex);

                return ex.Message + count.ToString();
            }
            catch (Exception ex)
            {
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                Helper.Log.Error(ex.Message + count.ToString(), ex);
                throw ex;
            }
        }
        private bool checkEmail(string email)
        {
            Regex RegEmail = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            return RegEmail.IsMatch(email);
        }


        private void AddContactor(string content, string title, ContactorImportDto importIDto, List<Contactor> origin, List<Contactor> addContactors, string strContactLanguage)
        {
            if (!string.IsNullOrWhiteSpace(content) && !content.Contains("不催") && !content.Contains("無"))
            {
                var emails = content.Split(';');
                foreach (var email in emails)
                {
                    if (string.IsNullOrEmpty(email.Trim())) { continue; }
                    if (email.IndexOf("@") < 0) continue;
                    if (!checkEmail(email))
                    {
                        throw new Exception("【" + email + "】, mail address invalid!");
                    }

                    var name = email.Substring(0, email.IndexOf("@"));
                    var bm = origin.FirstOrDefault(o => o.Title == title && o.EmailAddress == email);
                    if (bm == null)
                    {
                        Contactor add = new Contactor();
                        add.CustomerNum = importIDto.CustomerNum;
                        add.Name = name;
                        add.EmailAddress = email;
                        add.Title = title;
                        add.IsDefaultFlg = "1";
                        add.ToCc = "1";
                        add.CommunicationLanguage = strContactLanguage;
                        add.SiteUseId = importIDto.SiteUseId;
                        add.LegalEntity = "All";
                        add.Deal = AppContext.Current.User.Deal;

                        if (addContactors.Count(o => o.CustomerNum == add.CustomerNum && o.SiteUseId == add.SiteUseId && o.Title == add.Title && o.EmailAddress == add.EmailAddress) == 0)
                        {
                            addContactors.Add(add);
                        }
                    }
                }
            }

        }


        public string ImportCustomerLocalize()
        {
            string strCode = "";
            FileUploadHistory fileUpHis = new FileUploadHistory();
            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");

            try
            {

                CommonRep.GetDBContext().Database.ExecuteSqlCommand("TRUNCATE TABLE dbo.T_CUSTOMER_STAGING");
                //工作区结束行号
                strCode = Helper.EnumToCode<FileType>(FileType.CustLocalize);
                fileUpHis = fileService.GetSuccessData(strCode);
                string strpath = "";
                IQueryable<Customer> custlist = GetCustomer();
                Customer cust = new Customer();
                if (fileUpHis == null)
                {
                    throw new Exception("import file is not found!");
                }
                strpath = fileUpHis.ArchiveFileName;
                List<T_CUSTOMER_STAGING> listcustSta = new List<T_CUSTOMER_STAGING>();
                #region openXml
                ExcelPackage package = new ExcelPackage(new FileInfo(strpath));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号
                #endregion
                rowStart = 19;
                rowEnd = rowEnd - 2;
                for (int j = rowStart; j < rowEnd; j++)
                {
                    T_CUSTOMER_STAGING custSta = new T_CUSTOMER_STAGING();

                    if (worksheet.Cells[j, 1].Value != null)
                    {
                        custSta.CUSTOMER_NAME = worksheet.Cells[j, 1].Value.ToString();
                    }

                    if (worksheet.Cells[j, 3].Value != null)
                    {
                        custSta.CUSTOMER_NUM = worksheet.Cells[j, 3].Value.ToString();
                    }

                    if (worksheet.Cells[j, 8].Value != null)
                    {
                        custSta.SiteUseId = worksheet.Cells[j, 8].Value.ToString();
                    }

                    if (worksheet.Cells[j, 9].Value != null)
                    {
                        custSta.LOCALIZE_CUSTOMER_NAME = worksheet.Cells[j, 9].Value.ToString();
                    }
                    listcustSta.Add(custSta);
                }


                EfSqlBulkInsertProviderWithMappedDataReader aaa = new EfSqlBulkInsertProviderWithMappedDataReader();
                aaa.SetContext(CommonRep.GetDBContext());
                aaa.Run(listcustSta);

                CommonRep.BulkInsert(listcustSta);
                CommonRep.Commit();

                StringBuilder updateSql = new StringBuilder();
                updateSql.Append(" UPDATE T_CUSTOMER SET T_CUSTOMER.LOCALIZE_CUSTOMER_NAME = cs.LOCALIZE_CUSTOMER_NAME, ");
                updateSql.Append(" UPDATE_TIME = getdate() ");
                updateSql.Append(" FROM dbo.T_CUSTOMER AS c ");
                updateSql.Append(" INNER join T_CUSTOMER_STAGING AS cs WITH (NOLOCK) ");
                updateSql.Append(" ON c.CUSTOMER_NUM = cs.CUSTOMER_NUM ");
                updateSql.Append(" AND c.SiteUseId = cs.SiteUseId ");
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(updateSql.ToString());
                return "Import Finished!";
            }
            catch (Exception ex)
            {
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                fileService.CommonRep.Commit();
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public string ImportVarData()
        {
            string strCode = "";
            FileUploadHistory fileUpHis = new FileUploadHistory();
            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");

            try
            {
                CommonRep.GetDBContext().Database.ExecuteSqlCommand("TRUNCATE TABLE dbo.T_INVOICE_VARDATA");
                //工作区结束行号
                strCode = Helper.EnumToCode<FileType>(FileType.VarData);
                fileUpHis = fileService.GetSuccessData(strCode);
                string strpath = "";
                if (fileUpHis == null)
                {
                    throw new Exception("import file is not found!");
                }
                strpath = fileUpHis.ArchiveFileName;

                var typeName = Path.GetExtension(strpath);

                List<T_INVOICE_VARDATA> listVar = new List<T_INVOICE_VARDATA>();

                if (typeName.ToUpper() == ".XLSX")
                {
                    #region excel

                    #region openXml
                    ExcelPackage package = new ExcelPackage(new FileInfo(strpath));
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                    int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号
                    #endregion
                    rowStart = rowStart + 3;
                    string strStartDate = DateTime.Now.ToString();

                    for (int j = rowStart; j < rowEnd; j++)
                    {
                        T_INVOICE_VARDATA varData = new T_INVOICE_VARDATA();
                        varData.IDYear = "";
                        if (worksheet.Cells[j, 1].Value != null)
                        {
                            varData.IDYear = worksheet.Cells[j, 1].Value.ToString();
                            varData.IDYear = varData.IDYear.Replace("'", "'+ '''' + '");
                        }
                        varData.IDQQ = "";
                        if (worksheet.Cells[j, 2].Value != null)
                        {
                            varData.IDQQ = worksheet.Cells[j, 2].Value.ToString();
                            varData.IDQQ = varData.IDQQ.Replace("'", "'+ '''' + '");
                        }
                        varData.IDMonth = "";
                        if (worksheet.Cells[j, 3].Value != null)
                        {
                            varData.IDMonth = worksheet.Cells[j, 3].Value.ToString();
                            varData.IDMonth = varData.IDMonth.Replace("'", "'+ '''' + '");
                        }
                        varData.BillToCustomerOperatingUnit = "";
                        if (worksheet.Cells[j, 4].Value != null)
                        {
                            varData.BillToCustomerOperatingUnit = worksheet.Cells[j, 4].Value.ToString();
                            varData.BillToCustomerOperatingUnit = varData.BillToCustomerOperatingUnit.Replace("'", "'+ '''' + '");
                        }
                        varData.BillToCustomerOUName = "";
                        if (worksheet.Cells[j, 5].Value != null)
                        {
                            varData.BillToCustomerOUName = worksheet.Cells[j, 5].Value.ToString();
                            varData.BillToCustomerOUName = varData.BillToCustomerOUName.Replace("'", "'+ '''' + '");
                        }
                        varData.OrderNumber = "";
                        if (worksheet.Cells[j, 6].Value != null)
                        {
                            varData.OrderNumber = worksheet.Cells[j, 6].Value.ToString();
                            varData.OrderNumber = varData.OrderNumber.Replace("'", "'+ '''' + '");
                        }
                        varData.LineNumber = "";
                        if (worksheet.Cells[j, 7].Value != null)
                        {
                            varData.LineNumber = worksheet.Cells[j, 7].Value.ToString();
                            varData.LineNumber = varData.LineNumber.Replace("'", "'+ '''' + '");
                        }
                        varData.VarData1 = "";
                        if (worksheet.Cells[j, 8].Value != null)
                        {
                            varData.VarData1 = worksheet.Cells[j, 8].Value.ToString();
                            varData.VarData1 = varData.VarData1.Replace("'", "'+ '''' + '");
                        }
                        varData.VarData2 = "";
                        if (worksheet.Cells[j, 9].Value != null)
                        {
                            varData.VarData2 = worksheet.Cells[j, 9].Value.ToString();
                            varData.VarData2 = varData.VarData2.Replace("'", "'+ '''' + '");
                        }
                        varData.InvoiceNumber = "";
                        if (worksheet.Cells[j, 10].Value != null)
                        {
                            varData.InvoiceNumber = worksheet.Cells[j, 10].Value.ToString();
                            varData.InvoiceNumber = varData.InvoiceNumber.Replace("'", "'+ '''' + '");
                        }
                        varData.InvoiceLineNumber = "";
                        if (worksheet.Cells[j, 11].Value != null)
                        {
                            varData.InvoiceLineNumber = worksheet.Cells[j, 11].Value.ToString();
                            varData.InvoiceLineNumber = varData.InvoiceLineNumber.Replace("'", "'+ '''' + '");
                        }
                        varData.AccountNumber = "";
                        if (worksheet.Cells[j, 12].Value != null)
                        {
                            varData.AccountNumber = worksheet.Cells[j, 12].Value.ToString();
                            varData.AccountNumber = varData.AccountNumber.Replace("'", "'+ '''' + '");
                        }
                        varData.AccountName = "";
                        if (worksheet.Cells[j, 13].Value != null)
                        {
                            varData.AccountName = worksheet.Cells[j, 13].Value.ToString();
                            varData.AccountName = varData.AccountName.Replace("'", "'+ '''' + '");
                        }
                        varData.ExtendedResaleUSD = 0;
                        if (worksheet.Cells[j, 14].Value != null)
                        {
                            varData.ExtendedResaleUSD = Convert.ToDecimal(worksheet.Cells[j, 14].Value);
                        }
                        listVar.Add(varData);
                    }
                    #endregion
                }
                else if (typeName.ToUpper() == ".CSV")
                {
                    using (FileStream fs = new FileStream(strpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        CsvReader reader = new CsvReader(new StreamReader(fs, System.Text.Encoding.UTF8));
                        reader.Configuration.RegisterClassMap<VarDataMap>();
                        reader.Read();
                        reader.ReadHeader();
                        listVar = reader.GetRecords<T_INVOICE_VARDATA>().ToList();
                    }
                }

                CommonRep.BulkInsert(listVar);
                CommonRep.Commit();

                if (listVar.Count > 0)
                {
                    StringBuilder updateSql = new StringBuilder();
                    updateSql.Append(@" UPDATE A
                                           SET A.VarData1 = V.VARDATA
                                          FROM T_INVOICE_AGING A
                                          JOIN (SELECT DISTINCT BillToCustomerOperatingUnit AS LEGALENTITY,
                                                AccountNumber AS CUSTOMERNUM, InvoiceNumber AS InvoiceNumber, VarData1 AS VARDATA
                                          FROM T_INVOICE_VARDATA WITH (NOLOCK)) AS V ON A.LEGAL_ENTITY = V.LEGALENTITY AND A.CUSTOMER_NUM = V.CUSTOMERNUM AND A.INVOICE_NUM = V.InvoiceNumber");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(updateSql.ToString());
                }

                string strEndDate = DateTime.Now.ToString();
                return "Import Finished!";
            }
            catch (Exception ex)
            {
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                fileService.CommonRep.Commit();
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public string ImportCustomerLocalize(FileUploadHistory fileUpHis)
        {
            string strCode = "";
            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");

            try
            {

                CommonRep.GetDBContext().Database.ExecuteSqlCommand("TRUNCATE TABLE dbo.T_CUSTOMER_STAGING");
                //工作区结束行号
                strCode = Helper.EnumToCode<FileType>(FileType.CustLocalize);
                string strpath = "";
                IQueryable<Customer> custlist = GetCustomer();
                Customer cust = new Customer();
                if (fileUpHis == null)
                {
                    throw new Exception("import file is not found!");
                }
                strpath = fileUpHis.ArchiveFileName;
                List<T_CUSTOMER_STAGING> listcustSta = new List<T_CUSTOMER_STAGING>();
                #region openXml
                ExcelPackage package = new ExcelPackage(new FileInfo(strpath), true);
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号
                #endregion
                rowStart = 19;
                rowEnd = rowEnd - 2;
                for (int j = rowStart; j < rowEnd; j++)
                {
                    T_CUSTOMER_STAGING custSta = new T_CUSTOMER_STAGING();

                    if (worksheet.Cells[j, 1].Value != null)
                    {
                        custSta.CUSTOMER_NAME = worksheet.Cells[j, 1].Value.ToString();
                    }

                    if (worksheet.Cells[j, 3].Value != null)
                    {
                        custSta.CUSTOMER_NUM = worksheet.Cells[j, 3].Value.ToString();
                    }

                    if (worksheet.Cells[j, 8].Value != null)
                    {
                        custSta.SiteUseId = worksheet.Cells[j, 8].Value.ToString();
                    }

                    if (worksheet.Cells[j, 9].Value != null)
                    {
                        custSta.LOCALIZE_CUSTOMER_NAME = worksheet.Cells[j, 9].Value.ToString();
                    }
                    listcustSta.Add(custSta);
                }
                EfSqlBulkInsertProviderWithMappedDataReader aaa = new EfSqlBulkInsertProviderWithMappedDataReader();
                aaa.Run(listcustSta);

                CommonRep.BulkInsert(listcustSta);
                CommonRep.Commit();

                StringBuilder updateSql = new StringBuilder();
                updateSql.Append(" UPDATE T_CUSTOMER SET T_CUSTOMER.LOCALIZE_CUSTOMER_NAME = cs.LOCALIZE_CUSTOMER_NAME, ");
                updateSql.Append(" UPDATE_TIME = getdate() ");
                updateSql.Append(" FROM dbo.T_CUSTOMER AS c ");
                updateSql.Append(" INNER join T_CUSTOMER_STAGING AS cs WITH (NOLOCK)");
                updateSql.Append(" ON c.CUSTOMER_NUM = cs.CUSTOMER_NUM ");
                updateSql.Append(" AND c.SiteUseId = cs.SiteUseId ");
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(updateSql.ToString());
                return "Import Finished!";
            }
            catch (Exception ex)
            {
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                fileService.CommonRep.Commit();
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public string AddCustMasterData(Customer cust)
        {
            try
            {
                if (string.IsNullOrEmpty(cust.CustomerName))
                {
                    return "Please Add Customer Name!";
                }
                cust.Deal = AppContext.Current.User.Deal.ToString();
                cust.CreateTime = AppContext.Current.User.Now;
                cust.RemoveFlg = "1";
                cust.IsHoldFlg = "0";
                Customer custflg = GetOneCustomer(cust.CustomerNum, cust.SiteUseId);
                if (custflg == null)
                {
                    if (cust.Id == 0)
                    {
                        CommonRep.Add(cust);
                        CommonRep.Commit();
                        return "Add Success!";
                    }
                    else
                    {
                        Customer old = CommonRep.FindBy<Customer>(cust.Id);
                        ObjectHelper.CopyObjectWithUnNeed(cust, old, new string[] { "Id" });
                        CommonRep.Commit();
                        return "Update Success！";
                    }
                }
                else
                {
                    return "This customer already exsit!";
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw ex;
            }
        }

        public string delCustMasterData(Customer cust)
        {
            try
            {
                SqlHelper.ExcuteSql("delete from t_customer where id = " + cust.Id);
                return "Delete Success!";
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public List<CustomerCommentsDto> searchCustomerCommentsByDTO(CustomerCommentsDto dto, bool isDeleted = false)
        {
            try
            {
                StringBuilder sbsql = new StringBuilder();
                sbsql.Append(@" SELECT * ");
                sbsql.Append(@" FROM T_Customer_Comments ");
                sbsql.Append(@" where CUSTOMER_NUM='{0}' and SiteUseId='{1}' and isDeleted={2} order by agingbucket desc");
                string sql = string.Format(sbsql.ToString(), dto.CUSTOMER_NUM, dto.SiteUseId, isDeleted ? 1 : 0);
                return CommonRep.ExecuteSqlQuery<CustomerCommentsDto>(sql).ToList();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }
        public List<CustomerCommentsDto> getAllCustomerComments()
        {
            try
            {
                StringBuilder sbsql = new StringBuilder();
                sbsql.Append(@" SELECT * ");
                sbsql.Append(@" FROM T_Customer_Comments where isDeleted <> 1 order by sortid ");
                return CommonRep.ExecuteSqlQuery<CustomerCommentsDto>(sbsql.ToString()).ToList();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public List<CustomerAgingBucketDto> getAllCustomerAging()
        {
            try
            {
                StringBuilder sbsql = new StringBuilder();
                sbsql.Append(@" SELECT  CUSTOMER_NUM        as	CustomerNum,
                                        SiteUseId           as	SiteUseId,
	                                    TOTAL_AMT           as	TotalAmt,
	                                    CURRENT_AMT         as	CurrentAmt,
	                                    DUEOVER_TOTAL_AMT	as  DueoverTotalAmt,
	                                    DUE15_AMT           as	Due15Amt,
	                                    DUE30_AMT           as	Due30Amt,
	                                    DUE45_AMT           as	Due45Amt,
	                                    DUE60_AMT           as	Due60Amt,
	                                    DUE90_AMT           as	Due90Amt,
	                                    DUE120_AMT          as	Due120Amt,
	                                    DUE150_AMT          as	Due150Amt,
	                                    DUE180_AMT          as	Due180Amt,
	                                    DUE210_AMT          as	Due210Amt,
	                                    DUE240_AMT          as	Due240Amt,
	                                    DUE270_AMT          as	Due270Amt,
	                                    DUE300_AMT          as	Due300Amt,
	                                    DUE330_AMT          as	Due330Amt,
	                                    DUE360_AMT          as	Due360Amt,
	                                    DUEOVER360_AMT      as	DueOver360Amt  ");
                sbsql.Append(@" FROM T_CUSTOMER_AGING ");
                return CommonRep.ExecuteSqlQuery<CustomerAgingBucketDto>(sbsql.ToString()).ToList();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }
        public CustomerCommentsDto getCustomerCommentsByID(CustomerCommentsDto dto, bool isDeleted = false)
        {
            try
            {
                StringBuilder sbsql = new StringBuilder();
                sbsql.Append(@" SELECT * ");
                sbsql.Append(@" FROM T_Customer_Comments ");
                sbsql.Append(@" where ID='{0}' and isDeleted={1} ");
                string sql = string.Format(sbsql.ToString(), dto.ID, isDeleted);
                return CommonRep.ExecuteSqlQuery<CustomerCommentsDto>(sql).ToList().FirstOrDefault();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public int updateCustomerCommentsDto(CustomerCommentsDto dto, bool isDeleted = false)
        {
            try
            {
                //判断AgingBucket是否已经存在
                int countSame = SqlHelper.ExcuteScalar<int>(string.Format("select count(*) from T_Customer_Comments where isDeleted = 0 and siteuseid ='{0}' and AgingBucket='{1}' and id <> '{2}'", dto.SiteUseId, dto.AgingBucket, dto.ID), null);
                if (countSame > 0)
                {
                    return -100;
                }
                StringBuilder sb = new StringBuilder();
                int returnValue = 0;
                if (isDeleted)
                {
                    sb.Append(@" UPDATE T_Customer_Comments SET isDeleted='{0}' WHERE ID = '{1}' ");
                    string sql1 = string.Format(sb.ToString(), isDeleted, dto.ID);
                    returnValue = CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql1);
                }
                else
                {
                    string sql1 = @"UPDATE T_Customer_Comments SET AgingBucket=@AgingBucket,PTPAmount=@PTPAmount,PTPDATE=@PTPDATE,OverdueReason=@OverdueReason,Comments=@Comments,CommentsFrom=@CommentsFrom,CreateUser=@CreateUser,CreateDate=@CreateDate WHERE ID =@Id";
                    SqlParameter[] parms = {
                            new SqlParameter("@AgingBucket", dto.AgingBucket),
                            (dto.PTPAmount==null ? new SqlParameter("@PTPAmount", DBNull.Value) : new SqlParameter("@PTPAmount", dto.PTPAmount)),
                            (dto.PTPDATE==null ? new SqlParameter("@PTPDATE", DBNull.Value) : new SqlParameter("@PTPDATE", dto.PTPDATE)),
                            new SqlParameter("@OverdueReason", dto.OverdueReason),
                            new SqlParameter("@Comments", dto.Comments),
                            new SqlParameter("@CommentsFrom", dto.CommentsFrom),
                            new SqlParameter("@CreateUser", AppContext.Current.User.EID),
                            new SqlParameter("@CreateDate", DateTime.Now),
                            new SqlParameter("@Id", dto.ID)
                    };
                    returnValue =  CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql1, parms);
                }
                //Build Comments from collector, 以'Feedback'开始的后续所有文本(Collector添加的内容必须在该Tag之前，否则会被冲掉)
                List<string> listSiteUseId = new List<string>();
                string strSiteUseId = SqlHelper.ExcuteScalar<string>(string.Format("select siteuseid from T_Customer_Comments where id = '{0}'", dto.ID), null);
                listSiteUseId.Add(strSiteUseId);
                List<string> listRebuildComments = reBuildComments(listSiteUseId);
                if (listRebuildComments.Count > 0)
                {
                    SqlHelper.ExcuteListSql(listRebuildComments);
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public int addCustomerCommentsDto(List<CustomerCommentsDto> dtos)
        {
            int ret = 0;
            try
            {
                foreach (var item in dtos)
                {
                    //判断AgingBucket是否已经存在
                    int countSame = SqlHelper.ExcuteScalar<int>(string.Format("select count(*) from T_Customer_Comments where isDeleted = 0 and siteuseid ='{0}' and AgingBucket='{1}'", item.SiteUseId, item.AgingBucket),null);
                    if (countSame > 0) {
                        return -100;
                    }

                    SqlParameter[] parms = {
                        new SqlParameter("@AgingBucket", item.AgingBucket) ,
                        new SqlParameter("@CustomerNum", item.CUSTOMER_NUM) ,
                        new SqlParameter("@SiteUseId", item.SiteUseId) ,
                        (item.PTPDATE == null ? new SqlParameter("@PTPDate", DBNull.Value) : new SqlParameter("@PTPDate", item.PTPDATE)) ,
                        (item.PTPAmount == null ? new SqlParameter("@PTPAmount", DBNull.Value) : new SqlParameter("@PTPAmount", item.PTPAmount)) ,
                        new SqlParameter("@OverDueReason", item.OverdueReason == null ? "" : item.OverdueReason) ,
                        new SqlParameter("@Comments", item.Comments == null ? "" : item.Comments.Replace("'", "''")) ,
                        new SqlParameter("@CommentsFrom", item.CommentsFrom == null ? "" : item.CommentsFrom.Replace("'", "''")) ,
                        new SqlParameter("@CreateUser",  AppContext.Current.User.EID) ,
                        new SqlParameter("@CreateDate",  DateTime.Now)
                    };

                    var insertSql = @"
                    INSERT INTO [dbo].[T_Customer_Comments]
                               (ID
                               ,[AgingBucket]
                               ,[CUSTOMER_NUM]
                               ,[SiteUseId]
                               ,[PTPDATE]
                               ,[PTPAmount]
                               ,[OverdueReason]
                               ,[Comments]
                               ,[CommentsFrom]
                               ,[CreateUser]
                               ,[CreateDate]
                               ,[isDeleted])
                         VALUES
                               (NEWID()
                               ,@AgingBucket
                               ,@CustomerNum
                               ,@SiteUseId
                               ,@PTPDate
                               ,@PTPAmount
                               ,@OverDueReason
                               ,@Comments
                               ,@CommentsFrom
                               ,@CreateUser
                               ,@CreateDate
                               ,0)";
                    ret = CommonRep.GetDBContext().Database.ExecuteSqlCommand(insertSql, parms);
                    Helper.Log.Info(insertSql);
                    //Build Comments from collector, 以'Feedback'开始的后续所有文本(Collector添加的内容必须在该Tag之前，否则会被冲掉)
                    List<string> listSiteUseId = new List<string>();
                    listSiteUseId.Add(item.SiteUseId);
                    List<string> listRebuildComments = reBuildComments(listSiteUseId);
                    Helper.Log.Info(listRebuildComments);
                    if (listRebuildComments.Count > 0)
                    {
                        SqlHelper.ExcuteListSql(listRebuildComments);
                    }
                }
                return ret;
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public string UpdateCustMasterData(Customer cust)
        {
            try
            {
                cust.Deal = AppContext.Current.User.Deal.ToString();
                cust.UpdateTime = AppContext.Current.User.Now;
                if (string.IsNullOrEmpty(cust.RemoveFlg))
                {
                    cust.RemoveFlg = "1";
                }
                if (cust.RemoveFlg.Equals("0"))
                {
                    if (checkDelCustomer(cust))
                    {
                        Helper.Log.Info(cust);
                        Customer old = CommonRep.FindBy<Customer>(cust.Id);
                        ObjectHelper.CopyObjectWithUnNeed(cust, old, new string[] { "Id", "Contacter", "CustomerGroupCfg" });
                        CommonRep.Commit();
                        return "Delete Success!";
                    }
                    return "Current Customer Exsit Some Information";
                }
                else
                {
                    Helper.Log.Info(cust);
                    if (cust.BillGroupCode == "")
                    {
                        cust.BillGroupCode = null;
                    }
                    string strType = "";
                    Customer old = CommonRep.FindBy<Customer>(cust.Id);
                    if (old == null)
                    {
                        old = CommonRep.GetDbSet<Customer>().Where(m => m.CustomerNum == cust.CustomerNum && m.SiteUseId == cust.SiteUseId).FirstOrDefault();
                        if (old == null)
                        {
                            old = new Customer();
                            strType = "new";
                        }
                    }
                    else
                    {
                        // INSERT T_Customer_ExpirationDateHis
                        if (!Convert.ToDateTime(old.CommentExpirationDate).Equals(cust.CommentExpirationDate))
                        {
                            T_Customer_ExpirationDateHis cusExpDateHis = new T_Customer_ExpirationDateHis();

                            cusExpDateHis.CustomerNum = cust.CustomerNum;
                            cusExpDateHis.OldCommentExpirationDate = old.CommentExpirationDate;
                            if (cust.CommentExpirationDate == null)
                            {
                                cusExpDateHis.NewCommentExpirationDate = null;
                            }
                            else
                            {
                                cusExpDateHis.NewCommentExpirationDate = cust.CommentExpirationDate;
                            }
                            cusExpDateHis.UserId = AppContext.Current.User.EID; //当前用户ID
                            cusExpDateHis.ChangeDate = DateTime.Now;
                            cusExpDateHis.SiteUseId = cust.SiteUseId;
                            cusExpDateHis.Comment = cust.Comment;
                            CommonRep.Add(cusExpDateHis);
                        }
                        if (string.IsNullOrEmpty(cust.Comment))
                        {
                            cust.CommentLastDate = null;
                            cust.CommentExpirationDate = null;
                        }
                        else
                        {
                            if (cust.Comment != old.Comment)
                            {
                                cust.CommentLastDate = DateTime.Now;
                            }
                        }
                    }
                    ObjectHelper.CopyObjectWithUnNeed(cust, old, new string[] { "Id", "Contacter", "CustomerGroupCfg" });
                    if (strType == "new")
                    {
                        List<Customer> listInsert = new List<Customer>();
                        if (!string.IsNullOrEmpty(old.Comment))
                        {
                            old.CommentLastDate = DateTime.Now;
                        }
                        listInsert.Add(old);
                        CommonRep.BulkInsert<Customer>(listInsert);
                    }
                    CommonRep.Commit();
                    if (strType == "new")
                    {
                        return "Add Success!";
                    }
                    else
                    {
                        return "Update Success!";
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }


        #region get one customer by customernum and sitecode
        //add by pxc 20150722
        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public Customer GetOneCustomer(string num)
        {
            return CommonRep.GetDbSet<Customer>().Where(m => m.CustomerNum == num && m.Deal == AppContext.Current.User.Deal).FirstOrDefault();
        }
        #endregion

        public Customer GetOneCustomer(string num, string siteUseId)
        {
            return CommonRep.GetDbSet<Customer>().Where(m => m.CustomerNum == num && m.SiteUseId == siteUseId && m.Deal == AppContext.Current.User.Deal).FirstOrDefault();
        }

        #region get customer by customer number
        //add by pxc 20150722
        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public List<Customer> GetCustomerByCustomerNum(string cusNum)
        {
            return CommonRep.GetDbSet<Customer>().Where(m => m.CustomerNum == cusNum && m.Deal == AppContext.Current.User.Deal).ToList();
        }
        #endregion

        #region Get All Group Data from cache
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<CustomerGroupCfg> GetAllCustomerGroup()
        {
            return CacheSvr.GetOrSet<List<CustomerGroupCfg>>("Cache_CustomerGroupCfg", () =>
            {
                return CommonRep.GetQueryable<CustomerGroupCfg>().ToList();
            });
        }
        #endregion

        public List<CustomerGroupCfg> GetAllCustomerGroupFromDb()
        {
            return CommonRep.GetQueryable<CustomerGroupCfg>().ToList();
        }

        public List<CustomerGroupCfgHistory> GetAllCustomerGroupHis()
        {
            return CommonRep.GetQueryable<CustomerGroupCfgHistory>().ToList();
        }

        #region data uploading
        /// <summary>
        /// 
        /// </summary>
        public void allFileImport()
        {
            FileUploadHistory accFileName;
            FileUploadHistory invFileName;
            StreamReader srAcc;
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
                strCode = Helper.EnumToCode<FileType>(FileType.Account);
                accFileName = fileService.GetNewestData(strCode);
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
                    srAcc = fileOpen(accFileName.ArchiveFileName);
                    srInv = fileOpen(invFileName.ArchiveFileName);
                    //site check
                    strSite = strSiteGet(srAcc, srInv, accFileName.OriginalFileName, invFileName.OriginalFileName);
                    if (string.IsNullOrEmpty(strSite))
                    {
                        //Do nothing
                    }
                    else
                    {
                        strMessage = string.Empty;
                        dt = formartCheck(srAcc, out msg);
                        if (dt == null)
                        {
                            strMessage = accFileName.OriginalFileName + " Bad file format!" + strEnter + msg + strEnter;
                        }
                        else
                        {
                            strGuid = System.Guid.NewGuid().ToString("N");
                            dtReport = fileService.GetFileUploadHistory().Where(o => o.LegalEntity == strSite).Max(o => o.ReportTime);
                            if (dt.Value > CurrentTime || dt < dtReport)
                            {
                                strMessage = accFileName.OriginalFileName + " Report is old!" + strEnter + "(Report Date is wrong!)" + strEnter;
                            }
                            else if (!invoiceFormartCheck(srInv))
                            {
                                strMessage = invFileName.OriginalFileName + " Bad file format!" + strEnter;
                            }
                            else
                            {
                                dataImport(srAcc, strSite, strGuid);
                                dataInvoiceImport(srInv, strSite, strGuid);
                                if (listAgingStaging.Count > 0 && invoiceAgingList.Count > 0)
                                {
                                    if (isSameCust())
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
                throw new AgingImportException(ex.Message, isSuc);
            }
            catch (Exception ex)
            {
                fileService.upLoadHisUp(accFileName, invFileName, sts, null);
                throw new AgingImportException(ex.Message, isSuc);
            }
            finally
            {
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
        #endregion

        #region acc inv customer check
        private bool isSameCust()
        {
            bool rtn;
            rtn = true;
            List<CustomerAgingStaging> agingStagingListNew = new List<CustomerAgingStaging>();
            List<InvoiceAgingStaging> invoiceAgingListNew = new List<InvoiceAgingStaging>();
            if (listAgingStaging == null || invoiceAgingList == null)
            {
                rtn = false;
            }

            IList<CustomerAgingStaging> custaing = getAmtNotEqualAging(out invoiceAgingListNew).ToList();


            agingStagingListNew = (from acc in listAgingStaging
                                   join inv in invoiceAgingListNew
                                   on acc.CustomerNum equals inv.CustomerNum
                                   into dft
                                   from invdft in dft.DefaultIfEmpty()
                                   where invdft == null
                                   select new CustomerAgingStaging
                                   {
                                       Deal = acc.Deal,
                                       LegalEntity = acc.LegalEntity,
                                       CustomerNum = acc.CustomerNum,
                                       TotalAmt = acc.TotalAmt
                                   }).ToList();

            listAgingStaging.ForEach(aging =>
            {
                if (custaing.Where(o => o.LegalEntity == aging.LegalEntity && o.CustomerNum == aging.CustomerNum).ToList().Count > 0)
                {
                    aging.AmtMappingFlg = "1";
                }
                else
                {
                    aging.AmtMappingFlg = "0";
                }
                if (agingStagingListNew.Where(o => o.LegalEntity == aging.LegalEntity
                                            && o.CustomerNum == aging.CustomerNum).ToList().Count > 0)
                {
                    aging.MissInvioceFlg = "1";
                }
                else
                {
                    aging.MissInvioceFlg = "0";
                }
            });

            invoiceAgingListNew = (from inv in invoiceAgingListNew
                                   join acc in listAgingStaging
                                   on inv.CustomerNum equals acc.CustomerNum
                                   into dft
                                   from accdft in dft.DefaultIfEmpty()
                                   where accdft == null
                                   select new InvoiceAgingStaging
                                   {
                                       Deal = inv.Deal,
                                       LegalEntity = inv.LegalEntity,
                                       CustomerNum = inv.CustomerNum,
                                       BalanceAmt = inv.BalanceAmt
                                   }).ToList();

            invoiceAgingList.ForEach(aging =>
            {
                if (invoiceAgingListNew.Where(o => o.LegalEntity == aging.LegalEntity
                                                && o.CustomerNum == aging.CustomerNum
                                                && o.BalanceAmt != 0).ToList().Count > 0)
                {
                    aging.MissAccountFlg = "1";
                }
                else
                {
                    aging.MissAccountFlg = "0";
                }
            });

            listAgingStaging = listAgingStaging.FindAll(o => o.MissInvioceFlg != "1");

            agingStagingListNew.Clear();
            custaing.Clear();
            invoiceAgingListNew.Clear();

            return rtn;
        }

        #endregion
        #region site check
        /// <summary>
        /// 
        /// </summary>
        /// <param name="srAcc"></param>
        /// <param name="srInv"></param>
        /// <returns></returns>
        private string strSiteGet(StreamReader srAcc, StreamReader srInv, string fileNameAcc, string fileNameInv)
        {
            string rtnSite;
            rtnSite = strSiteGetFromFile(srAcc, 0, fileNameAcc);
            if (!strSiteGetFromName(rtnSite, fileNameInv))
            {
                rtnSite = string.Empty;
                strMessage = "Please upload reports with same legal entity!" + strEnter;
            }

            return rtnSite;
        }

        private bool strSiteGetFromName(string site, string fileName)
        {
            List<SysTypeDetail> deList = BDService.GetSysTypeDetail("030");
            string strEndChar = deList.Where(o => o.DetailValue == site).Select(o => o.DetailName).FirstOrDefault().ToString();

            string fullname = System.IO.Path.GetFileNameWithoutExtension(fileName);

            if (fullname.EndsWith(strEndChar))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="iFlg"></param>
        /// <returns></returns>
        private string strSiteGetFromFile(StreamReader sr, int iFlg, string strfileName)
        {
            string rtnSite;
            string orgSite;
            orgSite = string.Empty;
            while ((rtnSite = sr.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(rtnSite))
                {
                    strMessage += strfileName + ": Bad file format!" + strEnter + "(Legal Entity is empty!)" + strEnter;
                    rtnSite = string.Empty;
                }
                else
                {
                    orgSite = rtnSite;
                    rtnSite = siteGet(rtnSite);
                    if (string.IsNullOrEmpty(rtnSite))
                    {
                        strMessage += strfileName + "'s Legal Entity:[" + orgSite + "] is not found in System!" + strEnter;
                    }
                }
                break;
            }
            return rtnSite;
        }
        #endregion

        public void dataAddToComm(string strSite)
        {
            List<InvoiceAgingStaging> list = new List<InvoiceAgingStaging>();
            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    string DelSql;
                    DelSql = "delete from T_CUSTOMER_AGING_STAGING WHERE DEAL = '"
                               + CurrentDeal + "' AND LEGAL_ENTITY = '" + strSite + "';";
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(DelSql);

                    DelSql = "delete from T_INVOICE_AGING_STAGING WHERE DEAL = '"
                               + CurrentDeal + "' AND LEGAL_ENTITY = '" + strSite + "';";
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(DelSql);

                    List<Customer> custListDelete = custList.FindAll(cust => cust.Id == 0);
                    foreach (Customer c in custListDelete)
                    {
                        DelSql = "delete from T_CUSTOMER WHERE siteuseid = '" + c.SiteUseId + "' AND CUSTOMER_NUM <> '" + c.CustomerNum + "';";
                        CommonRep.GetDBContext().Database.ExecuteSqlCommand(DelSql);
                    }

                    CommonRep.AddRange(custList.FindAll(cust => cust.Id == 0));
                    CommonRep.Commit();

                    CommonRep.AddRange(listAgingStaging);
                    CommonRep.AddRange(invoiceAgingList);
                    CommonRep.Commit();

                    scope.Complete();
                }
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

        /// <summary>
        /// file open by filefullname
        /// </summary>
        /// <param name="strFileName"></param>
        private StreamReader fileOpen(string strFileName)
        {
            return new StreamReader(strFileName, Encoding.Default);
        }

        private string getTerm(string strTerm)
        {
            string rtn;
            if (string.IsNullOrEmpty(strTerm))
            {
                rtn = strTerm;
            }
            else if (strTerm.ToUpper().IndexOf("CIA") >= 0)
            {
                rtn = "0";
            }
            else if (strTerm.ToUpper().IndexOf("N00") >= 0)
            {
                rtn = "0";
            }
            else if (strTerm.ToUpper().IndexOf("COD") >= 0)
            {
                rtn = "0";
            }
            else
            {
                rtn = strTerm.ToUpper().Replace("NET", "");
            }
            return rtn;
        }

        /// <summary>
        /// AccountLevel导入
        /// </summary>
        private void dataImport(StreamReader sr, string strSite, string strGuid)
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
                arrData = line.Split(strSplit);
                if (!dataCheck(arrData))
                {
                    continue;
                }
                if (arrData[0] == strEnd)
                {
                    break;
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
                            //0:No
                            IsHoldFlg = "0",
                            AutoReminderFlg = "0",
                            ExcludeFlg = getNewICCust(arrData[0]),//11/03 update Ic logic add
                            //0:Not Assign
                            RemoveFlg = "1",
                            CreateTime = CurrentTime //11/03 Add create time Add
                        };
                    }
                    custList.Add(cust);
                    if (cust.ExcludeFlg == "1")
                    {
                        continue;
                    }
                    custaging = new CustomerAgingStaging();
                    custaging.Deal = CurrentDeal;
                    custaging.LegalEntity = strSite;
                    custaging.CustomerNum = arrData[1];
                    //CustomerInfo取得
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
                    custaging.CreditTerm = getTerm(arrData[3]);// cust.CreditTrem;
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
        /// //11/03 Ic logic add
        /// </summary>
        /// <param name="custName"></param>
        /// <returns></returns>
        private string getNewICCust(string custName)
        {
            string rtn = "0";
            string strICs = BDService.GetSysConfigByCode("006").CfgValue;
            string[] strIC;
            strIC = strICs.Split('^');

            foreach (string str in strIC)
            {
                if (custName.ToUpper().IndexOf(str.ToUpper()) >= 0)
                {
                    rtn = "1";
                    break;
                }
            }

            return rtn;

        }

        /// <summary>
        /// Format Check
        /// </summary>
        /// <returns></returns>
        private DateTime? formartCheck(StreamReader sr, out string msg)
        {
            string line;//数据行读取用
            int i;
            DateTime? dt;
            string[] strSplited;

            i = 0;
            dt = null;

            string msg1 = "(" + strFormatCheck + " is not found!)" + strEnter;
            string msg2 = "(Report Date is not found!)" + strEnter;

            msg = "";

            while ((line = sr.ReadLine()) != null)
            {
                if (line == strFormatCheck)
                {
                    i++;
                    msg1 = "";
                    continue;
                }
                strSplited = line.Split(strSplit);
                if (strSplited[0] == strDtCheck && strSplited.Length == 2)
                {
                    i++;
                    dt = dataConvertToDT(strSplited[1]);
                    msg2 = "";
                    continue;
                }
                if (strSplited.Length == intAccountCols && strSplited[0] == strHeaderCheck)
                {
                    i++;
                    break;
                }
            }
            if (i < 3)
            {
                msg = msg1 + msg2;
                dt = null;
            }
            return dt;
        }

        /// <summary>
        /// Format Check
        /// </summary>
        /// <param name="strCheck"></param>
        /// <returns></returns>
        private bool invoiceFormartCheck(StreamReader sr)
        {
            string line;//数据行读取用
            int i;
            bool isHeaderCheck;

            i = 0;
            isHeaderCheck = false;
            while ((line = sr.ReadLine()) != null)
            {
                i++;
                if (line.Split(strSplit)[0] == strInvoiceHeaderCheck && line.Split(strSplit).Length == intInvoiceCols)
                {
                    return true;
                }
            }
            return isHeaderCheck;
        }

        /// <summary>
        /// Format Check
        /// </summary>
        /// <param name="strCheck"></param>
        /// <returns></returns>
        private bool subFormatCheck(string strCheck)
        {
            if (strCheck == strFormatCheck)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 导入必要data判断
        /// </summary>
        /// <param name="arrData"></param>
        /// <returns></returns>
        private bool dataCheck(string[] arrData)
        {
            string strCheck;

            if (arrData.Length != intAccountCols)
            {
                return false;
            }
            strCheck = arrData[0].Split(strSubSplit)[0];
            if (strCheck == strHeaderCheck)
            {
                return false;
            }
            if (strCheck == strSubTatolCheck)
            {
                return false;
            }
            if (string.IsNullOrEmpty(strCheck.Trim()))
            {
                return false;
            }

            return true;
        }

        #region site info get from Db
        /// <summary>
        /// site 取得
        /// </summary>
        /// <param name="strSite"></param>
        /// <param name="sites"></param>
        /// <returns></returns>
        private string siteGet(string strType)
        {
            IBaseDataService Service = SpringFactory.GetObjectImpl<IBaseDataService>("BaseDataService");
            List<SysTypeDetail> sites = Service.GetSysTypeDetail("015");
            if (sites == null)
            {
                return null;
            }
            else
            {
                return sites.Where(o => o.DetailName == strType).Select(o => o.DetailValue).SingleOrDefault();
            }

        }
        #endregion

        #region Customer Info Get
        /// <summary>
        /// CustomerInfo取得
        /// </summary>
        /// <param name="CustomerNum"></param>
        /// <param name="custs"></param>
        /// <param name="billGroupInfos"></param>
        /// <returns></returns>
        private bool TryGetCustomer(string customerNum, string customerName, List<Customer> allCustList, List<Customer> jitCustList, List<CustomerTeam> AllCustTeamList, out Customer cust, out CustomerTeam custTeam)
        {
            cust = (from o in allCustList
                    where o.CustomerNum == customerNum && o.Deal == CurrentDeal
                    select o).FirstOrDefault();
            custTeam = (from o in AllCustTeamList
                        where o.CustomerNum == customerNum && o.Deal == CurrentDeal
                        select o).FirstOrDefault();

            if (cust != null)
            {
                if (cust.CustomerName != customerName)
                {
                    cust.CustomerName = customerName;
                }
                return true;
            }
            else if ((cust = jitCustList.Find(c => c.CustomerNum == customerNum && c.Deal == CurrentDeal)) != null)
            {

                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 类型转换
        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        private Decimal dataConvertToDec(string strData)
        {
            decimal rtn;
            rtn = 0;
            bool isRegex = false;
            if (!string.IsNullOrEmpty(strData))
            {

                Regex reg = new Regex(@"([+-]?)(\d\.\d{1,})E([-]?)(\d+)", RegexOptions.IgnoreCase);
                MatchCollection mac = reg.Matches(strData);
                foreach (System.Text.RegularExpressions.Match m in mac)
                {
                    isRegex = true;
                    rtn = Convert.ToDecimal(Convert.ToDouble(m.Groups[0].Value));
                }
                if (!isRegex)
                {
                    rtn = Convert.ToDecimal(strData);
                }
            }
            else
            {
                rtn = 0;
            }
            return rtn;
        }
        #endregion

        /// <summary>
        /// 数据导入
        /// </summary>
        private void dataInvoiceImport(StreamReader sr, string strSite, string strGuid)
        {
            string line;//数据行读取用
            string[] arrData;
            InvoiceAgingStaging invaging;
            invoiceAgingList = new List<InvoiceAgingStaging>();
            int intCount;

            intCount = 0;

            var customerList = GetCustomer().ToList();

            while ((line = sr.ReadLine()) != null)
            {
                arrData = line.Split(strSplit);
                if (arrData.Length != intInvoiceCols)
                {
                    continue;
                }
                intCount++;

                try
                {
                    invaging = new InvoiceAgingStaging();
                    invaging.CustomerNum = arrData[1];
                    //CustomerInfo取得
                    invaging.CustomerName = arrData[2];
                    invaging.InvoiceNum = arrData[12];
                    invaging.MstCustomer = arrData[21];
                    invaging.PoNum = arrData[17];
                    invaging.SoNum = arrData[26];
                    invaging.Class = arrData[16];
                    invaging.Currency = arrData[23];
                    invaging.OrderBy = arrData[27];
                    invaging.InvoiceDate = dataConvertToDT(arrData[13]);
                    invaging.DueDate = dataConvertToDT(arrData[15]);
                    invaging.OriginalAmt = dataConvertToDec(arrData[22]);
                    invaging.BalanceAmt = dataConvertToDec(arrData[24]);
                    invaging.LegalEntity = strSite;
                    invaging.Deal = CurrentDeal; // need change
                    invaging.CreateDate = AppContext.Current.User.Now;
                    invaging.Operator = AppContext.Current.User.EID;
                    invaging.CreditTrem = getTrem(arrData[14]);
                    invaging.ImportId = strGuid;
                    invaging.States = strInvStats;

                    //upload all the col
                    invaging.StatementDate = dataConvertToDT(arrData[0]);
                    invaging.CustomerAddress1 = arrData[3];
                    invaging.CustomerAddress2 = arrData[4];
                    invaging.CustomerAddress3 = arrData[5];
                    invaging.CustomerAddress4 = arrData[6];
                    invaging.CustomerCountry = arrData[7];
                    invaging.CustomerCountryDetail = arrData[8];
                    invaging.AttentionTo = arrData[9];
                    invaging.CollectorName = arrData[10];
                    invaging.CollectorContact = arrData[11];
                    invaging.DaysLateSys = dataConvertToInt(arrData[18]);
                    invaging.RboCode = arrData[20];
                    invaging.OutstandingAccumulatedInvoiceAmt = dataConvertToDec(arrData[25]);
                    invaging.CustomerBillToSite = arrData[28];


                    Customer cust = customerList.Find(c => c.CustomerNum == invaging.CustomerNum && c.Deal == invaging.Deal);
                    if (cust != null)
                    {
                        invaging.BillGroupCode = cust.BillGroupCode;
                        invaging.CustomerName = cust.CustomerName;
                        if (cust.ExcludeFlg == "1")
                        {
                            continue;
                        }
                    }
                    invoiceAgingList.Add(invaging);
                }
                catch (Exception ex)
                {
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }
            }

        }

        private string getTrem(string tremTxt)
        {
            tremTxt = tremTxt.ToUpper().Replace("NET", "");

            if (string.IsNullOrEmpty(tremTxt))
            {
                return "0";
            }

            return tremTxt;
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        private DateTime dataConvertToDT(string strData)
        {
            DateTime dt = new DateTime();
            if (!string.IsNullOrEmpty(strData.Trim()))
            {
                return Convert.ToDateTime(strData);
            }

            return dt;
        }

        private int dataConvertToInt(string strData)
        {
            int rtn;
            rtn = 0;
            if (!string.IsNullOrEmpty(strData))
            {
                rtn = Convert.ToInt32(strData);
            }

            return rtn;
        }


        /// <summary>
        /// click submit button, move tempTbale data to RealTable
        /// </summary>
        /// <param name="submitStgs"></param>
        /// <returns></returns>
        public int SubmitInitialAging()
        {
            Helper.Log.Info("Start submit");

            #region SQL
            // Account process logic
            // customer aging staging to customer aging
            var customerAgingMergeSql = string.Format(@"
        MERGE INTO T_CUSTOMER_AGING as Target 
        USING T_CUSTOMER_AGING_STAGING as Source
        ON
	        Target.DEAL=Source.DEAL and Target.CUSTOMER_NUM = Source.CUSTOMER_NUM 
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
				        ,Target.DUE30_AMT		    = Source.DUE30_AMT
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
                        
                        --,Target.'OPERATOR'          = '{0}'
        WHEN not matched and Source.DEAL = '{2}' THEN
	        insert 
		        (DEAL,LEGAL_ENTITY,CUSTOMER_NUM,CUSTOMER_NAME,CUSTOMER_CLASS,RISK_SCORE,BILL_GROUP_CODE
		        ,BILL_GROUP_NAME,COUNTRY,CREDIT_TREM,CREDIT_LIMIT,COLLECTOR,COLLECTOR_SYS,SALES
		        ,TOTAL_AMT,CURRENT_AMT,DUE30_AMT,DUE60_AMT,DUE90_AMT,DUE120_AMT,DUE150_AMT
		        ,DUE180_AMT,DUE210_AMT,DUE240_AMT,DUE270_AMT,DUE300_AMT,DUE330_AMT,DUE360_AMT,DUEOVER360_AMT
                , DUEOVER_TOTAL_AMT,CREATE_DATE,UPDATE_DATE--,OPERATOR
                ,IS_HOLD_FLG,IMPORT_ID,REMOVE_FLG
                ,CURRENCY, COUNTRY_CODE, OUTSTANDING_AMT, CITY_OR_STATE
                , CONTACT_NAME, CONTACT_PHONE, CUSTOMER_CREDIT_MEMO, CUSTOMER_PAYMENTS
                , CUSTOMER_RECEIPTS_AT_RISK, CUSTOMER_CLAIMS, CUSTOMER_BALANCE)
	        values (Source.DEAL,Source.LEGAL_ENTITY,Source.CUSTOMER_NUM,Source.CUSTOMER_NAME,Source.CUSTOMER_CLASS,Source.RISK_SCORE,Source.BILL_GROUP_CODE
			        ,Source.BILL_GROUP_NAME,Source.COUNTRY,Source.CREDIT_TREM,Source.CREDIT_LIMIT,Source.COLLECTOR,Source.COLLECTOR_SYS,Source.SALES
			        ,Source.TOTAL_AMT,Source.CURRENT_AMT,Source.DUE30_AMT,Source.DUE60_AMT,Source.DUE90_AMT,Source.DUE120_AMT,Source.DUE150_AMT
			        ,Source.DUE180_AMT,Source.DUE210_AMT,Source.DUE240_AMT,Source.DUE270_AMT,Source.DUE300_AMT,Source.DUE330_AMT,Source.DUE360_AMT,Source.DUEOVER360_AMT
                    , Source.TOTAL_AMT - Source.CURRENT_AMT,CAST('{1}' as datetime),CAST('{1}' as datetime)--,'{0}'
                    ,Source.IS_HOLD_FLG,Source.IMPORT_ID,'0'
                    ,Source.CURRENCY, Source.COUNTRY_CODE, Source.OUTSTANDING_AMT, Source.CITY_OR_STATE
                    , Source.CONTACT_NAME, Source.CONTACT_PHONE, Source.CUSTOMER_CREDIT_MEMO, Source.CUSTOMER_PAYMENTS
                    , Source.CUSTOMER_RECEIPTS_AT_RISK, Source.CUSTOMER_CLAIMS, Source.CUSTOMER_BALANCE)
        WHEN not matched by Source and Target.DEAL = '{2}' AND Target.LEGAL_ENTITY in (SELECT DISTINCT LEGAL_ENTITY FROM T_CUSTOMER_AGING_STAGING WITH (NOLOCK)) THEN
            UPDATE SET Target.REMOVE_FLG = '1'
                       ,Target.UPDATE_DATE         = CAST('{1}' as datetime) 
                       ;", AppContext.Current.User.EID, AppContext.Current.User.Now, CurrentDeal);

            var invoiceAgingMergeSql = string.Format(@"
        MERGE INTO T_INVOICE_AGING as Target 
        USING (SELECT * FROM T_INVOICE_AGING_STAGING WITH (NOLOCK) 
				WHERE CLASS+INVOICE_NUM+CUSTOMER_NUM+DEAL+LEGAL_ENTITY 
					NOT IN (
						SELECT CLASS+INVOICE_NUM+CUSTOMER_NUM+DEAL+LEGAL_ENTITY 
						FROM T_INVOICE_AGING_STAGING WITH (NOLOCK)
						WHERE CLASS IN ('DM','PAYMENT')
						GROUP BY CLASS, INVOICE_NUM, CUSTOMER_NUM, DEAL, LEGAL_ENTITY HAVING COUNT(1) > 1)
			  ) as Source
        ON
	        Target.DEAL = Source.DEAL and Target.CUSTOMER_NUM = Source.CUSTOMER_NUM 
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
			,Target.ORIGINAL_AMT		= Source.ORIGINAL_AMT
			,Target.BALANCE_AMT		= Source.BALANCE_AMT
			--Target.REMARK			--= Source.REMARK
			,Target.IMPORT_ID		= Source.IMPORT_ID
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
				DEAL,CUSTOMER_NUM,LEGAL_ENTITY,CUSTOMER_NAME,INVOICE_NUM, INVOICE_TYPE,CREDIT_TREM,MST_CUSTOMER,
				PO_MUM,SO_NUM,CLASS,CURRENCY,STATES,ORDER_BY,BILL_GROUP_CODE,
				INVOICE_DATE,DUE_DATE,ORIGINAL_AMT,BALANCE_AMT,REMARK,
				IMPORT_ID,CREATE_DATE,UPDATE_DATE,COMMENTS
                , MISS_ACCOUNT_FLG, STATEMENT_DATE, CUSTOMER_ADDRESS_1, CUSTOMER_ADDRESS_2
                , CUSTOMER_ADDRESS_3, CUSTOMER_ADDRESS_4, CUSTOMER_COUNTRY
                , CUSTOMER_COUNTRY_DETAIL, ATTENTION_TO, COLLECTOR_NAME
                , COLLECTOR_CONTACT, DAYS_LATE_SYS, RBO_CODE
                , OUTSTANDING_ACCUMULATED_INVOICE_AMT, CUSTOMER_BILL_TO_SITE
                , TRACK_STATES
			)
			VALUES (
				Source.DEAL,Source.CUSTOMER_NUM,Source.LEGAL_ENTITY,Source.CUSTOMER_NAME,Source.INVOICE_NUM, Source.INVOICE_TYPE,Source.CREDIT_TREM,Source.MST_CUSTOMER,
				Source.PO_MUM,Source.SO_NUM,Source.CLASS,Source.CURRENCY,Source.STATES,Source.ORDER_BY,Source.BILL_GROUP_CODE,
				Source.INVOICE_DATE,Source.DUE_DATE,Source.ORIGINAL_AMT,Source.BALANCE_AMT,Source.REMARK,
				Source.IMPORT_ID,CAST('{1}' as datetime),CAST('{1}' as datetime),Source.COMMENTS
                , Source.MISS_ACCOUNT_FLG, Source.STATEMENT_DATE, Source.CUSTOMER_ADDRESS_1, Source.CUSTOMER_ADDRESS_2
                , Source.CUSTOMER_ADDRESS_3, Source.CUSTOMER_ADDRESS_4, Source.CUSTOMER_COUNTRY
                , Source.CUSTOMER_COUNTRY_DETAIL, Source.ATTENTION_TO, Source.COLLECTOR_NAME
                , Source.COLLECTOR_CONTACT, Source.DAYS_LATE_SYS, Source.RBO_CODE
                , Source.OUTSTANDING_ACCUMULATED_INVOICE_AMT, Source.CUSTOMER_BILL_TO_SITE
                , '{3}'
			);", AppContext.Current.User.EID, AppContext.Current.User.Now, CurrentDeal
               , Helper.EnumToCode<TrackStatus>(TrackStatus.Open)
               , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Closed)
               , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Open));

            // for Payment and DM, there are duplicate records with same invoice number. They will be treat specially because of 'Merge' clause will fail on them.
            var specialPaymentAdditionSql = string.Format(@"
        INSERT INTO T_INVOICE_AGING (
			DEAL,CUSTOMER_NUM,LEGAL_ENTITY,CUSTOMER_NAME,INVOICE_NUM, INVOICE_TYPE,CREDIT_TREM,MST_CUSTOMER,
			PO_MUM,SO_NUM,CLASS,CURRENCY,STATES,ORDER_BY,BILL_GROUP_CODE,
			INVOICE_DATE,DUE_DATE,ORIGINAL_AMT,BALANCE_AMT,REMARK,
			IMPORT_ID,CREATE_DATE,UPDATE_DATE,COMMENTS
            , MISS_ACCOUNT_FLG, STATEMENT_DATE, CUSTOMER_ADDRESS_1, CUSTOMER_ADDRESS_2
            , CUSTOMER_ADDRESS_3, CUSTOMER_ADDRESS_4, CUSTOMER_COUNTRY
            , CUSTOMER_COUNTRY_DETAIL, ATTENTION_TO, COLLECTOR_NAME
            , COLLECTOR_CONTACT, DAYS_LATE_SYS, RBO_CODE
            , OUTSTANDING_ACCUMULATED_INVOICE_AMT, CUSTOMER_BILL_TO_SITE
		) 
		SELECT Source.DEAL,Source.CUSTOMER_NUM,Source.LEGAL_ENTITY,Source.CUSTOMER_NAME,Source.INVOICE_NUM, Source.INVOICE_TYPE,Source.CREDIT_TREM,Source.MST_CUSTOMER,
			Source.PO_MUM,Source.SO_NUM,Source.CLASS,Source.CURRENCY,Source.STATES,Source.ORDER_BY,Source.BILL_GROUP_CODE,
			Source.INVOICE_DATE,Source.DUE_DATE,Source.ORIGINAL_AMT,Source.BALANCE_AMT,Source.REMARK,
			Source.IMPORT_ID,CAST('{0}' as datetime),CAST('{0}' as datetime),
			Source.COMMENTS
            , Source.MISS_ACCOUNT_FLG, Source.STATEMENT_DATE, Source.CUSTOMER_ADDRESS_1, Source.CUSTOMER_ADDRESS_2
            , Source.CUSTOMER_ADDRESS_3, Source.CUSTOMER_ADDRESS_4, Source.CUSTOMER_COUNTRY
            , Source.CUSTOMER_COUNTRY_DETAIL, Source.ATTENTION_TO, Source.COLLECTOR_NAME
            , Source.COLLECTOR_CONTACT, Source.DAYS_LATE_SYS, Source.RBO_CODE
            , Source.OUTSTANDING_ACCUMULATED_INVOICE_AMT, Source.CUSTOMER_BILL_TO_SITE
        FROM T_INVOICE_AGING_STAGING Source WITH (NOLOCK)
		WHERE CLASS+INVOICE_NUM+CUSTOMER_NUM+DEAL+LEGAL_ENTITY 
			IN (
				SELECT CLASS+INVOICE_NUM+CUSTOMER_NUM+DEAL+LEGAL_ENTITY 
				FROM T_INVOICE_AGING_STAGING  WITH (NOLOCK)
				WHERE CLASS IN ('DM','PAYMENT')
				GROUP BY CLASS, INVOICE_NUM, CUSTOMER_NUM, DEAL, LEGAL_ENTITY HAVING COUNT(1) > 1)", AppContext.Current.User.Now);

            var invoiceAgingClosingSql = string.Format(@"
        UPDATE T_INVOICE_AGING
        SET UPDATE_DATE = CAST('{1}' as datetime), REMARK = 'Missing invoice during import. Set to paied status by SYSTEM'
            ,STATES = '{3}',TRACK_STATES = '{9}'
        WHERE 
	        NOT EXISTS (SELECT * FROM T_INVOICE_AGING_STAGING Staging WITH (NOLOCK) WHERE T_INVOICE_AGING.DEAL = Staging.DEAL 
				        and T_INVOICE_AGING.CUSTOMER_NUM = Staging.CUSTOMER_NUM 
				        and T_INVOICE_AGING.LEGAL_ENTITY = Staging.LEGAL_ENTITY and T_INVOICE_AGING.INVOICE_NUM = Staging.INVOICE_NUM)
            and EXISTS (SELECT * FROM T_CUSTOMER_AGING_STAGING Staging WITH (NOLOCK) WHERE T_INVOICE_AGING.DEAL = Staging.DEAL 
			--	        and T_INVOICE_AGING.CUSTOMER_NUM = Staging.CUSTOMER_NUM 
				        and T_INVOICE_AGING.LEGAL_ENTITY = Staging.LEGAL_ENTITY)
            and T_INVOICE_AGING.DEAL = '{2}'
            --and STATES in ('{4}','{5}','{6}','{7}','{8}')
            and STATES <> '{3}';
            ", AppContext.Current.User.EID, CurrentTime, CurrentDeal, Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Closed)
             , strInvStats
             , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.PTP)
             , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Paid)
             , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.PartialPay)
             , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Dispute)
             , Helper.EnumToCode<TrackStatus>(TrackStatus.Closed));

            var invoiceLog = string.Format(@"
                INSERT INTO T_INVOICE_LOG
                (DEAL, CUSTOMER_NUM, INVOICE_ID,
                LOG_DATE, LOG_PERSON, LOG_ACTION, 
                LOG_TYPE, OLD_STATUS, NEW_STATUS, 
                OLD_TRACK,NEW_TRACK,
                CONTACT_PERSON, PROOF_ID, DISCRIPTION)
                SELECT '{0}',ISNULL(staing.CUSTOMER_NUM,aging.CUSTOMER_NUM),ISNULL(staing.INVOICE_NUM,aging.INVOICE_NUM),
                        CAST('{1}' as datetime),'{2}','{3}',
                        '0',CASE WHEN aging.ID is null then '{4}' else aging.STATES end,CASE WHEN staing.ID is null then '{5}' else '{4}' end,
                        CASE WHEN aging.ID is null then '{10}' else aging.TRACK_STATES end,
                        CASE WHEN staing.ID is null then '{11}' else '{10}' end,
                        '{2}',null,null
                FROM T_INVOICE_AGING_STAGING staing WITH (NOLOCK)
                FULL OUTER JOIN T_INVOICE_AGING    aging WITH (NOLOCK)
                ON staing.DEAL = aging.DEAL
                AND staing.CUSTOMER_NUM = aging.CUSTOMER_NUM
                AND staing.LEGAL_ENTITY = aging.LEGAL_ENTITY
                AND staing.INVOICE_NUM = aging.INVOICE_NUM
                WHERE aging.ID is null
                OR
                    (
                        staing.ID is null
                    AND
                        --aging.STATES in ('{4}','{6}','{7}','{8}','{9}')
                        aging.STATES <> '{5}'
                    AND
                        EXISTS (SELECT * FROM T_CUSTOMER_AGING_STAGING Staging WITH (NOLOCK) WHERE aging.DEAL = Staging.DEAL 
				    --    and aging.CUSTOMER_NUM = Staging.CUSTOMER_NUM 
				        and aging.LEGAL_ENTITY = Staging.LEGAL_ENTITY)
                    );
                ", CurrentDeal, AppContext.Current.User.Now
                 , AppContext.Current.User.EID
                 , "Upload", strInvStats
                 , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Closed)
                 , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.PTP)
                 , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Paid)
                 , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.PartialPay)
                 , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Dispute)
                 , Helper.EnumToCode<TrackStatus>(TrackStatus.Open)
                 , Helper.EnumToCode<TrackStatus>(TrackStatus.Closed));

            var invoiceLog2 = string.Format(@"
                INSERT INTO T_INVOICE_LOG
                (DEAL, CUSTOMER_NUM, INVOICE_ID,
                LOG_DATE, LOG_PERSON, LOG_ACTION, 
                LOG_TYPE, OLD_STATUS, NEW_STATUS, 
                OLD_TRACK,NEW_TRACK,
                CONTACT_PERSON, PROOF_ID, DISCRIPTION)
                SELECT '{0}',aging.CUSTOMER_NUM,aging.INVOICE_NUM,
                        CAST('{1}' as datetime),'{2}','{3}',
                        '0',aging.STATES,'{4}',
                        aging.TRACK_STATES,'{6}',
                        '{2}',null,null
                FROM T_INVOICE_AGING_STAGING staing WITH (NOLOCK)
                INNER JOIN T_INVOICE_AGING    aging WITH (NOLOCK)
                ON staing.DEAL = aging.DEAL
                AND staing.CUSTOMER_NUM = aging.CUSTOMER_NUM
                AND staing.LEGAL_ENTITY = aging.LEGAL_ENTITY
                AND staing.INVOICE_NUM = aging.INVOICE_NUM
                WHERE aging.STATES = '{5}';
                ", CurrentDeal, AppContext.Current.User.Now
                 , AppContext.Current.User.EID
                 , "Upload", strInvStats
                 , Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Closed)
                 , Helper.EnumToCode<TrackStatus>(TrackStatus.Open));
            #endregion

            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    Helper.Log.Info("Transaction scope created");

                    Helper.Log.Info("File history update started.");
                    //Get custAgStags info
                    var custAgStags = CommonRep.GetQueryable<CustomerAgingStaging>().Where(o => o.Deal == CurrentDeal).ToList();

                    //use slelected id to search temptable not find result(this reco other user commited)
                    if (custAgStags.Count == 0) { return 1; }

                    List<FileUploadHistory> listImport = (from cust in custAgStags
                                                          group cust by cust.ImportId
                                                              into custg
                                                          select new FileUploadHistory
                                                          {
                                                              ImportId = custg.Key
                                                          }).ToList();

                    FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
                    List<FileUploadHistory> file = fileService.GetSucDataByImportId(listImport);
                    fileService.commitHisUp(file);
                    Helper.Log.Info("File history update complete.");
                    CommonRep.Commit();

                    Helper.Log.Info("Merge(insert/update) the account level aging data to database.");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(customerAgingMergeSql);

                    Helper.Log.Info("insert into T_INVOICE_LOG whitch invoice's STATES has changed.");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(invoiceLog);

                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(invoiceLog2);

                    Helper.Log.Info("Merge(insert/update) the invoice level aging data to database.");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(invoiceAgingMergeSql);

                    Helper.Log.Info("Merge(insert/update) the invoice level aging data to database for special Payment.");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(specialPaymentAdditionSql);

                    Helper.Log.Info("Apply closing logic to the missing invoice level aging data.");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(invoiceAgingClosingSql);

                    Helper.Log.Info("Start to delete the staging data.");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand("delete from T_CUSTOMER_AGING_STAGING where DEAL = '" + CurrentDeal + "';");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand("delete from T_INVOICE_AGING_STAGING WHERE DEAL = '" + CurrentDeal + "';");

                    Helper.Log.Info("Completed invoice level aging process.");

                    // finaly commit all 
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Error happended while save submit aging.", ex);
                throw ex;
            }
            return 0;
        }

        /// <summary>
        /// Upload file from STAGING to Formal
        /// </summary>
        /// <param name="arrow">DEAL</param>
        /// <returns>0:fail; 1:success</returns>
        public int SubmitInitialAgingNew(string deal)
        {
            int intResult = 0;

            Helper.Log.Info("Start: CustomerService.SubmitInitialAgingNew(), deal:" + deal);

            //check arrow is null
            if (string.IsNullOrEmpty(deal))
            {
                Helper.Log.Info("Deal is null then return.");
                return intResult;
            }

            try
            {
                //Get Legal list by deal

                Helper.Log.Info("Start: Get all legalEntity by " + deal);
                var sites = CommonRep.GetQueryable<InvoiceAgingStaging>().Where(o => o.Deal == deal).Select(o => o.LegalEntity).Distinct().ToList();
                Helper.Log.Info("End: Get all legalEntity by " + deal + ", Total:" + sites.Count());
                CommonRep.Commit();


                List<Exception> listException = new List<Exception>();
                foreach (var item in sites)
                {
                    try
                    {
                        #region spSubmitAgingData
                        //Current User Parameter
                        var paramUserId = new SqlParameter
                        {
                            ParameterName = "@USERID",
                            Value = AppContext.Current.User.EID,
                            Direction = ParameterDirection.Input
                        };

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
                            ParameterName = "@LEGAL_ENTITY",
                            Value = item,
                            Direction = ParameterDirection.Input
                        };
                        //Reuslt Parameter(0:NG; 1:OK)
                        var paramResultStatus = new SqlParameter
                        {
                            ParameterName = "@ResultStatus",
                            Value = 0,
                            Direction = ParameterDirection.Output
                        };
                        //System Datetime
                        var paramSysDate = new SqlParameter
                        {
                            ParameterName = "@SysDate",
                            Value = DateTime.Now,
                            Direction = ParameterDirection.Input
                        };

                        SqlParameter[] paramList = new SqlParameter[5];
                        paramList[0] = paramDEAL;
                        paramList[1] = paramLegalEntity;
                        paramList[2] = paramUserId;
                        paramList[3] = paramSysDate;
                        paramList[4] = paramResultStatus;
                        // using(tranc)

                        Helper.Log.Info("Start: call spSubmitAgingData(procedure):@DEAL" + deal + ",@LEGAL_ENTITY:" + item);
                        CommonRep.GetDBContext().Database.ExecuteSqlCommand("spSubmitAgingData @DEAL,@LEGAL_ENTITY,@USERID,@SysDate,@ResultStatus OUTPUT", paramList.ToArray());
                        Helper.Log.Info("End: call spSubmitAgingData(procedure):@DEAL" + deal + ",@LEGAL_ENTITY:" + item + ",RETURN:" + paramResultStatus.Value.ToString());
                        CommonRep.Commit();

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
                            Value = item,
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
                            Value = "",
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

                        SqlParameter[] paramBuildList = new SqlParameter[8];
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
                        CommonRep.Commit();

                        #endregion
                    }
                    catch (Exception exLegal)
                    {
                        Helper.Log.Error("Start: call spSubmitAgingData(procedure):@DEAL" + deal + ",@LEGAL_ENTITY:" + item, exLegal);
                        listException.Add(exLegal);
                    }
                }
                //if one legalEntity is not finish, the function return 0(fail)
                if (listException.Count > 0)
                {
                    intResult = 0;  //fail
                    throw new Exception("Error happended while save submit aging.");
                }
                else
                {
                    intResult = 1;  //success
                }
                Helper.Log.Info("End: CustomerService.SubmitInitialAgingNew(), deal:" + deal);
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Error happended while save submit aging.", ex);
                throw ex;
            }

            return intResult;
        }

        public int SubmitInitialInvDet(string deal)
        {

            int intResult = 0;

            try
            {
                bool lb_hasError = false;
                try
                {
                    #region spSubmitAgingData
                    //Current User Parameter
                    var paramUserId = new SqlParameter
                    {
                        ParameterName = "@USERID",
                        Value = AppContext.Current.User.EID,
                        Direction = ParameterDirection.Input
                    };
                    //System Datetime
                    var paramSysDate = new SqlParameter
                    {
                        ParameterName = "@SysDate",
                        Value = DateTime.Now,
                        Direction = ParameterDirection.Input
                    };
                    //Reuslt Parameter(0:NG; 1:OK)
                    var paramResultStatus = new SqlParameter
                    {
                        ParameterName = "@ResultStatus",
                        Value = 0,
                        Direction = ParameterDirection.Output
                    };

                    object[] paramList = new object[3];
                    paramList[0] = paramUserId;
                    paramList[1] = paramSysDate;
                    paramList[2] = paramResultStatus;
                    // using(tranc)

                    Helper.Log.Info("Start: call spSubmitInvoiceDetailData(procedure)");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand("spSubmitInvoiceDetailData @USERID,@SysDate,@ResultStatus OUTPUT", paramList.ToArray());
                    Helper.Log.Info("End: call spSubmitInvoiceDetailData(procedure),RETURN:" + paramResultStatus.Value.ToString());

                    if (!lb_hasError && (int)paramResultStatus.Value == 0)
                    {
                        lb_hasError = true;
                    }
                    #endregion

                    if ((int)paramResultStatus.Value == 1)
                    {
                        Helper.Log.Info("Start: Get all legalEntity by " + deal);
                        var sites = CommonRep.GetQueryable<Sites>().Where(o => o.Deal == deal).ToList();
                        Helper.Log.Info("End: Get all legalEntity by " + deal + ", Total:" + sites.Count());

                        foreach (var item in sites)
                        {
                            #region spBuildInvoiceAgingStatus

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
                                Value = item.LegalEntity,
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
                                Value = "",
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

                            if (!lb_hasError && (int)paramBuildResultStatus.Value == 0)
                            {
                                lb_hasError = true;
                            }
                            #endregion
                        }

                    }
                }
                catch (Exception ex)
                {
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }

                //if one legalEntity is not finish, the function return 0(fail)
                if (lb_hasError)
                {
                    intResult = 0;  //fail
                }
                else
                {
                    intResult = 1;  //success
                }

            }
            catch (Exception ex)
            {
                Helper.Log.Error("Error happended while save submit vat.", ex);
                throw;
            }

            return intResult;
        }

        public int SubmitInitialVAT(string deal)
        {

            int intResult = 0;

            try
            {
                bool lb_hasError = false;
                try
                {
                    #region spSubmitAgingData
                    //Current User Parameter
                    var paramUserId = new SqlParameter
                    {
                        ParameterName = "@USERID",
                        Value = AppContext.Current.User.EID,
                        Direction = ParameterDirection.Input
                    };
                    //System Datetime
                    var paramSysDate = new SqlParameter
                    {
                        ParameterName = "@SysDate",
                        Value = DateTime.Now,
                        Direction = ParameterDirection.Input
                    };
                    //Reuslt Parameter(0:NG; 1:OK)
                    var paramResultStatus = new SqlParameter
                    {
                        ParameterName = "@ResultStatus",
                        Value = 0,
                        Direction = ParameterDirection.Output
                    };

                    object[] paramList = new object[3];
                    paramList[0] = paramUserId;
                    paramList[1] = paramSysDate;
                    paramList[2] = paramResultStatus;

                    Helper.Log.Info("Start: call spSubmitVATData(procedure)");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand("spSubmitVATData @USERID,@SysDate,@ResultStatus OUTPUT", paramList.ToArray());
                    Helper.Log.Info("End: call spSubmitVATData(procedure),RETURN:" + paramResultStatus.Value.ToString());

                    if (!lb_hasError && (int)paramResultStatus.Value == 0)
                    {
                        lb_hasError = true;
                    }
                    #endregion

                    if ((int)paramResultStatus.Value == 1)
                    {
                        Helper.Log.Info("Start: Get all legalEntity by " + deal);
                        var sites = CommonRep.GetQueryable<Sites>().Where(o => o.Deal == deal).ToList();
                        Helper.Log.Info("End: Get all legalEntity by " + deal + ", Total:" + sites.Count());

                        foreach (var item in sites)
                        {
                            #region spBuildInvoiceAgingStatus

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
                                Value = item.LegalEntity,
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
                                Value = "",
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

                            if (!lb_hasError && (int)paramBuildResultStatus.Value == 0)
                            {
                                lb_hasError = true;
                            }
                            #endregion
                        }

                    }
                }
                catch (Exception ex)
                {
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }
                //if one legalEntity is not finish, the function return 0(fail)
                if (lb_hasError)
                {
                    intResult = 0;  //fail
                }
                else
                {
                    intResult = 1;  //success
                }

            }
            catch (Exception ex)
            {
                Helper.Log.Error("Error happended while save submit vat.", ex);
                throw;
            }

            return intResult;
        }

        public int SubmitInitialSAPAging(string deal)
        {
            int intResult = 0;

            Helper.Log.Info("Start: CustomerService.SubmitInitialSAPAging(), deal:" + deal);

            //check arrow is null
            if (string.IsNullOrEmpty(deal))
            {
                Helper.Log.Info("Deal is null then return.");
                return intResult;
            }

            try
            {
                //Get Legal list by deal
                var history = CommonRep.GetQueryable<FileUploadHistory>().OrderByDescending(o => o.UploadTime).FirstOrDefault(o => o.Deal == deal && o.FileType == "019" && o.UploadTime > DateTime.Today);
                if (history != null)
                {
                    var legals = CommonRep.GetQueryable<InvoiceAgingStaging>().Where(o => o.ImportId == history.ImportId).Select(o => o.LegalEntity).Distinct().ToList();
                    List<Exception> listException = new List<Exception>();
                    foreach (var legal in legals)
                    {
                        try
                        {
                            #region spSubmitAgingData
                            //Current User Parameter
                            var paramUserId = new SqlParameter
                            {
                                ParameterName = "@USERID",
                                Value = AppContext.Current.User.EID,
                                Direction = ParameterDirection.Input
                            };

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
                                ParameterName = "@LEGAL_ENTITY",
                                Value = legal,
                                Direction = ParameterDirection.Input
                            };
                            //Reuslt Parameter(0:NG; 1:OK)
                            var paramResultStatus = new SqlParameter
                            {
                                ParameterName = "@ResultStatus",
                                Value = 0,
                                Direction = ParameterDirection.Output
                            };
                            //System Datetime
                            var paramSysDate = new SqlParameter
                            {
                                ParameterName = "@SysDate",
                                Value = DateTime.Now,
                                Direction = ParameterDirection.Input
                            };

                            object[] paramList = new object[5];
                            paramList[0] = paramDEAL;
                            paramList[1] = paramLegalEntity;
                            paramList[2] = paramUserId;
                            paramList[3] = paramSysDate;
                            paramList[4] = paramResultStatus;

                            Helper.Log.Info("Start: call spSubmitSAPAgingData(procedure):@DEAL" + deal + ",@LEGAL_ENTITY:" + legal);
                            CommonRep.GetDBContext().Database.ExecuteSqlCommand("spSubmitSAPAgingData @DEAL,@LEGAL_ENTITY,@USERID,@SysDate,@ResultStatus OUTPUT", paramList.ToArray());
                            Helper.Log.Info("End: call spSubmitSAPAgingData(procedure):@DEAL" + deal + ",@LEGAL_ENTITY:" + legal + ",RETURN:" + paramResultStatus.Value.ToString());

                            #endregion

                            if ((int)paramResultStatus.Value == 1)
                            {

                                #region spBuildInvoiceAgingStatus

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
                                    Value = legal,
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
                                    Value = "",
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

                                #endregion

                            }
                        }
                        catch (Exception exLegal)
                        {
                            Helper.Log.Error("Start: call spSubmitAgingData(procedure):@DEAL" + deal + ",@LEGAL_ENTITY:" + legal, exLegal);
                            listException.Add(exLegal);
                        }
                    }
                    //if one legalEntity is not finish, the function return 0(fail)
                    if (listException.Count > 0)
                    {
                        intResult = 0;  //fail
                        throw new Exception("Error happended while save submit aging.");
                    }
                    else
                    {
                        intResult = 1;  //success
                    }
                    Helper.Log.Info("End: CustomerService.SubmitInitialAgingNew(), deal:" + deal);
                }
                else
                {
                    Helper.Log.Info("Error happended while save submit aging.");
                    intResult = 0;  //fail
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Error happended while save submit aging.", ex);
                throw ex;
            }

            return intResult;
        }

        /// <summary>
        /// Build invoice status
        /// </summary>
        /// <param name="arrow">DEAL</param>
        /// <returns>0:fail; 1:success</returns>
        public int BuildInvoiceAgingStatus(string deal, string legalEntity, string customerNo, string siteUseId, string invoiceNo, string strOperator)
        {
            int intResult = 0;

            Helper.Log.Info("Start: CustomerService.BuildInvoiceAgingStatus(), deal:" + deal);

            //check arrow is null
            if (string.IsNullOrEmpty(deal))
            {
                Helper.Log.Info("Deal is null then return.");
                return intResult;
            }

            try
            {
                bool lb_hasError = false;

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
                //CustomerNo Parameter
                var paramCustomerNo = new SqlParameter
                {
                    ParameterName = "@CustomerNo",
                    Value = customerNo,
                    Direction = ParameterDirection.Input
                };
                //SiteUseId Parameter
                var paramSiteUseId = new SqlParameter
                {
                    ParameterName = "@SiteUseId",
                    Value = siteUseId,
                    Direction = ParameterDirection.Input
                };
                //InvoiceNo Parameter
                var paramInvoiceNo = new SqlParameter
                {
                    ParameterName = "@InvoiceNo",
                    Value = invoiceNo,
                    Direction = ParameterDirection.Input
                };
                //Operator Parameter
                var paramOperator = new SqlParameter
                {
                    ParameterName = "@Operator",
                    Value = strOperator,
                    Direction = ParameterDirection.Input
                };
                //Operator Parameter
                var paramSysDate = new SqlParameter
                {
                    ParameterName = "@SysDate",
                    Value = DateTime.Now,
                    Direction = ParameterDirection.Input
                };
                //Reuslt Parameter(0:NG; 1:OK)
                var paramResultStatus = new SqlParameter
                {
                    ParameterName = "@ResultStatus",
                    Value = 0,
                    Direction = ParameterDirection.Output
                };

                object[] paramList = new object[8];
                paramList[0] = paramDEAL;
                paramList[1] = paramLegalEntity;
                paramList[2] = paramCustomerNo;
                paramList[3] = paramSiteUseId;
                paramList[4] = paramInvoiceNo;
                paramList[5] = paramOperator;
                paramList[6] = paramSysDate;
                paramList[7] = paramResultStatus;

                Helper.Log.Info("Start: call spBuildInvoiceAgingStatus(procedure):@DEAL" + deal + ",Legal:" + legalEntity + ",CustomerNo:" + customerNo + ",SiteUseId:" + siteUseId + ", @InvoiceNo:" + invoiceNo + ", @Operator:" + strOperator);

                CommonRep.GetDBContext().Database.ExecuteSqlCommand("spBuildInvoiceAgingStatus @DEAL,@LegalEntity,@CustomerNo,@SiteUseId,@InvoiceNo,@Operator,@SysDate,@ResultStatus OUTPUT", paramList.ToArray());

                Helper.Log.Info("End: call spBuildInvoiceAgingStatus(procedure):@DEAL" + deal + ",RETURN:" + paramResultStatus.Value.ToString());

                if (!lb_hasError && (int)paramResultStatus.Value == 0)
                {
                    lb_hasError = true;
                }

                if (lb_hasError)
                {
                    intResult = 0;  //fail
                }
                else
                {
                    intResult = 1;  //success
                }

                Helper.Log.Info("End: CustomerService.spBuildInvoiceAgingStatus(), deal:" + deal);

            }
            catch (Exception ex)
            {
                Helper.Log.Error("Error happended while save submit aging.", ex);
                throw;
            }

            return intResult;
        }


        /// <summary>
        /// Upload file from STAGING to Formal
        /// </summary>
        /// <returns>0:fail; 1:success</returns>
        public int BuildContactor(string deal)
        {
            int intResult = 0;

            Helper.Log.Info("Start: CustomerService.spBuildContactor(), deal:" + deal);

            //check arrow is null
            if (string.IsNullOrEmpty(deal))
            {
                Helper.Log.Info("Deal is null then return.");
                return intResult;
            }

            try
            {
                //Get Legal list by deal

                Helper.Log.Info("Start: Get all legalEntity by " + deal);
                var sites = CommonRep.GetQueryable<Sites>().Where(o => o.Deal == deal).ToList();
                Helper.Log.Info("End: Get all legalEntity by " + deal + ", Total:" + sites.Count());

                List<Exception> listException = new List<Exception>();
                foreach (var item in sites)
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
                            ParameterName = "@LEGAL_ENTITY",
                            Value = item.LegalEntity,
                            Direction = ParameterDirection.Input
                        };

                        object[] paramList = new object[2];
                        paramList[0] = paramDEAL;
                        paramList[1] = paramLegalEntity;

                        Helper.Log.Info("Start: call BuildContactor(procedure):@DEAL" + deal + ",@LEGAL_ENTITY:" + item.LegalEntity);
                        CommonRep.GetDBContext().Database.ExecuteSqlCommand("spBuildContactor @DEAL,@LEGAL_ENTITY", paramList.ToArray());
                        Helper.Log.Info("End: call BuildContactor(procedure):@DEAL" + deal + ",@LEGAL_ENTITY:" + item.LegalEntity);

                    }
                    catch (Exception exLegal)
                    {
                        Helper.Log.Error("Start: call spBuildContactor(procedure):@DEAL" + deal + ",@LEGAL_ENTITY:" + item.LegalEntity, exLegal);
                        listException.Add(exLegal);
                    }
                }
                //if one legalEntity is not finish, the function return 0(fail)
                if (listException.Count > 0)
                {
                    intResult = 0;  //fail
                    throw new Exception("Error happended while save submit aging.");
                }
                else
                {
                    intResult = 1;  //success
                }
                Helper.Log.Info("End: CustomerService.spBuildContactor(), deal:" + deal);
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Error happended while save submit aging.", ex);
                throw ex;
            }

            return intResult;
        }

        public void SubmitOneYearSales()
        {
            List<CustomerGroupCfgStaging> grps = new List<CustomerGroupCfgStaging>();
            grps = GetGroupStaing();

            if (grps.Count == 0)
            {
                throw new OTCServiceException("No data needs submit!");
            }

            //2.uploadinfo update
            #region SQL

            var GroupCfgUpdateList = string.Format(@"
                UPDATE
	                cfg
                SET 
	                BILL_GROUP_NAME = ISNULL(frp.BILL_GROUP_NAME,cfg.BILL_GROUP_NAME)
                FROM 
	                T_CUSTOMER_GROUP_CFG cfg WITH (NOLOCK)
                LEFT JOIN
	                T_CUSTOMER_GROUP_CFG_STAGING frp WITH (NOLOCK)
                ON
	                cfg.DEAL = frp.DEAL
                AND
	                cfg.BILL_GROUP_CODE = frp.BILL_GROUP_CODE 
                WHERE 
                    cfg.DEAL = '{0}';
                ", CurrentDeal);

            var GroupCfgInsertList = string.Format(@"
                INSERT INTO T_CUSTOMER_GROUP_CFG
                (
	                DEAL, BILL_GROUP_CODE, BILL_GROUP_NAME
                )
                SELECT 
	                DEAL, BILL_GROUP_CODE, BILL_GROUP_NAME
                FROM 
	                T_CUSTOMER_GROUP_CFG_STAGING frp WITH (NOLOCK)
                WHERE NOT EXISTS
                (
	                SELECT * FROM 
		                T_CUSTOMER_GROUP_CFG cfg WITH (NOLOCK)
	                WHERE 
		                cfg.DEAL = frp.DEAL
	                AND
		                cfg.BILL_GROUP_CODE = frp.BILL_GROUP_CODE
                )
                AND 
                    frp.DEAL = '{0}';
                ", CurrentDeal);

            PeroidService perService = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            int? perId = perService.getIdOfcurrentPeroid();

            var hisInsertList = string.Format(@"
                INSERT INTO 
                T_CUSTOMER_GROUP_CFG_HISTORY
                (
	                PERIOD_ID, DEAL, BILL_GROUP_CODE, BILL_GROUP_NAME, ONEYEAR_SALES, IMPORT_ID, UPLOAD_TIME
                )
                SELECT 
	                {1}, DEAL, BILL_GROUP_CODE, BILL_GROUP_NAME, ONE_YEAR_SALES, IMPORT_ID, UPLOAD_TIME
                FROM
	                T_CUSTOMER_GROUP_CFG_STAGING
                WHERE 
                    DEAL = '{0}';
                ", CurrentDeal, perId);

            #endregion

            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    Helper.Log.Info("Transaction scope created");

                    Helper.Log.Info("Completed one-year-sales's submit.");

                    Helper.Log.Info("update T_CUSTOMER_GROUP_CFG whith is exists.");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(GroupCfgUpdateList);

                    Helper.Log.Info("insert into T_CUSTOMER_GROUP_CFG whith is not exists");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(GroupCfgInsertList);

                    Helper.Log.Info("insert into T_CUSTOMER_GROUP_CFG_HISTORY.");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(hisInsertList);

                    CommonRep.GetDBContext().Database.ExecuteSqlCommand("delete from T_CUSTOMER_GROUP_CFG_STAGING where DEAL = '" + CurrentDeal + "'");

                    Helper.Log.Info("Completed one-year-sales's submit.");

                    // finaly commit all 
                    scope.Complete();
                }

                //commit info write
                string strCode;
                FileUploadHistory file;
                FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
                strCode = Helper.EnumToCode<FileType>(FileType.OneYearSales);
                file = fileService.GetNewestSucData(strCode);
                fileService.commitHisUp(file, perId);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw (ex);
                throw new OTCServiceException("Submit Failed!");
            }

        }
        public void ArBalanceAmtPeroidSet()
        {
            var updateSql = string.Format(@"
                UPDATE T_CUSTOMER_AGING
                SET	
	                AR_BALANCE_PERIOD = CASE WHEN REMOVE_FLG = '0' THEN TOTAL_AMT ELSE 0 END
                WHERE
	                DEAL = '{0}'
                ", CurrentDeal);

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
            {
                Helper.Log.Info("ArBalanceAmtPeroid UPDATE Start");

                CommonRep.GetDBContext().Database.ExecuteSqlCommand(updateSql);

                Helper.Log.Info("ArBalanceAmtPeroid UPDATE End");
                // finaly commit all 
                scope.Complete();
            }
        }
        public void GetValue()
        {
            var thresholdConfig = BDService.GetSysConfigByCode("001");
            decimal dec = decimal.Parse(thresholdConfig.CfgValue);

            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            string strImportId = fileService.getNeweastImportIdOfOneYear();

            var hisInsertChangeList = string.Format(@"
                INSERT INTO
                T_CUSTOMER_CHANGE_HIS
                (
                    PERIOD_ID, DEAL, CUSTOMER_NUM, CREATE_DATE, VALUE, VALUE_LEVEL, VR_TYPE, VR_DESC
                )
                SELECT
                    frp.PERIOD_ID,
	                cust.DEAL,cust.CUSTOMER_NUM,CAST('{4}' as datetime),
                    frp.ONE_YEAR_SALES,
                    (CASE WHEN list.ID is null then 
                    (CASE WHEN frp.ONE_YEAR_SALES > {1} THEN '{2}' ELSE '{3}' END)
					ELSE list.EX_VALUE END) AS VALUE_LEVEL,
                    '1','VALUE IS :' + (CASE WHEN list.ID is null then 
                    (CASE WHEN frp.ONE_YEAR_SALES > {1} THEN '{2}' ELSE '{3}' END)
					ELSE list.EX_VALUE END)
                FROM
	                T_CUSTOMER cust
                LEFT JOIN
	                T_CUSTOMER_PRIORITIZATION_EXCEPTION_LIST list
                ON
	                cust.DEAL = list.DEAL
                AND 
	                cust.CUSTOMER_NUM = list.CUSTOMER_NUM
                AND
	                list.EX_TYPE = '1'
                AND
                    CAST('{4}' as datetime) BETWEEN list.EFFECT_DATE AND list.[EXPIRY_DATE]
                LEFT JOIN
	                (SELECT ID,DEAL,BILL_GROUP_CODE,ONEYEAR_SALES AS ONE_YEAR_SALES,PERIOD_ID FROM
	                    T_CUSTOMER_GROUP_CFG_HISTORY
                     WHERE
	                    IMPORT_ID = '{5}') frp
                ON
	                cust.DEAL = frp.DEAL
                AND
	                cust.BILL_GROUP_CODE = frp.BILL_GROUP_CODE
                WHERE 
                    cust.DEAL = '{0}'
                AND
                    (list.ID is not null
                    OR
                    frp.ID is not null);
                ", CurrentDeal
                 , dec
                 , CustomerClass.HV.ToString()
                 , CustomerClass.LV.ToString()
                 , CurrentTime
                 , strImportId);


            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
            {
                Helper.Log.Info("Transaction(Value) scope created");

                Helper.Log.Info("insert into change history.");
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(hisInsertChangeList);

                Helper.Log.Info("Transaction(Value) scope completed");
                // finaly commit all 
                scope.Complete();
            }
        }
        public IQueryable<CustomerAging> GetCustomerAgingByCollector(string eID)
        {
            var retCusAging = CommonRep.GetQueryable<CustomerAging>().Where(cusAging => cusAging.Collector == eID);
            return retCusAging;
        }

        #region Un-used
        public void DeleteCustomerAging(List<int> custIds)
        {
            AssertUtils.ArgumentHasElements(custIds, "Custome Ids");

            var res = (from c in CommonRep.GetQueryable<CustomerAgingStaging>()
                       where custIds.Contains(c.Id)
                       select c).ToList();
            if (res.Count > 0)
            {
                var keys = res.Select(c => c.Deal + c.CustomerNum + c.LegalEntity + c.Operator).ToList();

                List<InvoiceAgingStaging> invoice = CommonRep.GetQueryable<InvoiceAgingStaging>().Where(m => keys.Contains(m.Deal + m.CustomerNum + m.LegalEntity + m.Operator)).ToList();

                if (invoice.Count > 0)
                {
                    //delete invoice informations
                    CommonRep.RemoveRange(invoice as IEnumerable<InvoiceAgingStaging>);
                }

                CommonRep.RemoveRange(res as IEnumerable<CustomerAgingStaging>);

                CommonRep.Commit();
            }
        }
        #endregion

        public List<CustomerAgingStaging> getAmtNotEqualAging(out List<InvoiceAgingStaging> invlist)
        {
            //listAgingStaging invoiceAgingList
            List<CustomerAgingStaging> rtns;

            invlist = (from inv in invoiceAgingList//GetCustomerAgingStaging()
                       group inv by new { inv.LegalEntity, inv.CustomerNum } into invg
                       select new InvoiceAgingStaging
                       {
                           LegalEntity = invg.Key.LegalEntity,
                           CustomerNum = invg.Key.CustomerNum,
                           BalanceAmt = invg.Sum(o => o.BalanceAmt)
                       }).ToList();

            rtns = (from acc in listAgingStaging
                    join inv in invlist
                    on new { acc.LegalEntity, acc.CustomerNum } equals new { inv.LegalEntity, inv.CustomerNum }
                    where acc.TotalAmt != inv.BalanceAmt
                    select acc).ToList();

            return rtns;
        }

        /// <summary>
        /// Consolidate_Report
        /// </summary>
        /// <param name="strimportId"></param>
        public void createAgingReport()
        {

            Helper.Log.Info("Start create aging report!");

            string strTmpPath;
            string strReportPath;
            string strReportName;
            string strimportId;
            string strportPath;
            string strimportUploadId;
            List<CustomerGroupCfgHistory> custGrpHis = new List<CustomerGroupCfgHistory>();
            strimportId = System.Guid.NewGuid().ToString("N");
            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");

            strTmpPath = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[strTempPathKey].ToString());
            strportPath = ConfigurationManager.AppSettings[strArchivePathKey].ToString();
            strReportPath = HttpContext.Current.Server.MapPath(strportPath).ToString();
            if (Directory.Exists(strReportPath) == false)
            {
                Directory.CreateDirectory(strReportPath);
            }
            #region aging report
            strReportName = "Consolidate_Report_" + AppContext.Current.User.EID.ToString() + "_" + AppContext.Current.User.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

            strReportPath += strReportName;

            List<AgingReport> custAgingHis;
            custAgingHis = new List<AgingReport>();

            strimportUploadId = fileService.getNeweastImportIdOfOneYear();
            if (strimportUploadId != null)
            {
                custGrpHis = GetAllCustomerGroupHis().Where(o => o.ImportId == strimportUploadId && o.Deal == CurrentDeal).Select(o => o).ToList();
            }

            List<Customer> custlist = GetCustomer().Where(o => o.ExcludeFlg != "1").ToList();

            try
            {
                custAgingHis
                        = (from his in GetCurrentPerCustAging()
                           join team in GetCustomerTeam().ToList()
                           on new { his.Deal, his.CustomerNum }
                              equals new { team.Deal, team.CustomerNum }
                           join cfg in custGrpHis
                            on new { his.Deal, his.BillGroupCode } equals
                                  new { cfg.Deal, cfg.BillGroupCode }
                          into dft
                           from dftcfg in dft.DefaultIfEmpty()
                           select new AgingReport
                           {
                               Deal = his.Deal,
                               LegalEntity = his.LegalEntity,
                               Team = team.TeamName,
                               CustomerNum = his.CustomerNum,
                               CustomerName = his.CustomerName,
                               BillGroupCode = team.BillGroupCode,
                               BillGroupName = team.BillGroupName,
                               Country = his.Country,
                               CreditTrem = his.CreditTrem,
                               CreditLimit = his.CreditLimit,
                               Collector = team.CollectorName,
                               Collectorsys = his.CollectorSys,
                               Sales = team.Sales,
                               TotalAmt = his.TotalAmt,
                               CurrentAmt = his.CurrentAmt,
                               Due30Amt = his.Due30Amt,
                               Due60Amt = his.Due60Amt,
                               Due90Amt = his.Due90Amt,
                               Due180Amt = his.Due120Amt + his.Due150Amt + his.Due180Amt,
                               Due360Amt = his.Due210Amt + his.Due240Amt + his.Due270Amt +
                                           his.Due300Amt + his.Due330Amt + his.Due360Amt,
                               DueOver360Amt = his.DueOver360Amt,
                               DueOver60Amt = 0,
                               DueOver90Amt = 0,
                               DueOver180Amt = 0,
                               AdjustedDueOver60Amt = 0,
                               AdjustedDueOver90Amt = 0,
                               OneYearSales = dftcfg == null ? 0 : dftcfg.OneYearSales
                           }).ToList<AgingReport>();

                if (custAgingHis.Count == 0)
                {
                    throw new OTCServiceException("There is no aging report uploaded in current period!");
                }
                NpoiHelper helper = new NpoiHelper(strTmpPath);
                helper.Save(strReportPath, true);
                helper = new NpoiHelper(strReportPath);
                string sheetName = "";
                sheetName = "RAW";

                helper.ActiveSheetName = sheetName;

                int iRow;
                iRow = 2;
                string strInital = "";

                if (custAgingHis.Count > 0)
                {
                    foreach (AgingReport cust in custAgingHis)
                    {
                        strInital = cust.CustomerName.Substring(0, 1).ToUpper();
                        Regex reg = new Regex(@"[A-Za-z]");
                        if (!reg.IsMatch(strInital))
                        {
                            strInital = "A";
                        }
                        cust.DueOver180Amt = cust.Due360Amt + cust.DueOver360Amt;
                        cust.DueOver90Amt = cust.DueOver180Amt + cust.Due180Amt;
                        cust.DueOver60Amt = cust.DueOver90Amt + cust.Due90Amt;
                        cust.AdjustedDueOver90Amt = (cust.Due180Amt > 0 ? cust.Due180Amt : 0) +
                                                    (cust.Due360Amt > 0 ? cust.Due360Amt : 0) +
                                                    (cust.DueOver360Amt > 0 ? cust.DueOver360Amt : 0);
                        cust.AdjustedDueOver60Amt = cust.AdjustedDueOver90Amt + (cust.Due90Amt > 0 ? cust.Due90Amt : 0);
                        if (cust.OneYearSales == null)
                        {
                            cust.OneYearSales = 0;
                        }
                        helper.SetData(iRow, 0, cust.LegalEntity);
                        helper.SetData(iRow, 1, cust.Team);
                        reg = new Regex(@"^\d+$");
                        if (reg.IsMatch(cust.CustomerNum))
                        {
                            helper.SetData(iRow, 2, Convert.ToInt64(cust.CustomerNum));
                        }
                        else
                        {
                            helper.SetData(iRow, 2, cust.CustomerNum);
                        }
                        helper.SetData(iRow, 3, cust.CustomerName);
                        helper.SetData(iRow, 4, strInital);
                        helper.SetData(iRow, 5, cust.BillGroupCode);
                        helper.SetData(iRow, 6, cust.BillGroupName);
                        helper.SetData(iRow, 7, cust.Country);
                        helper.SetData(iRow, 8, cust.CreditTrem);
                        helper.SetData(iRow, 9, cust.CreditLimit);
                        helper.SetData(iRow, 10, cust.Collector);
                        helper.SetData(iRow, 11, cust.Collectorsys);
                        helper.SetData(iRow, 12, cust.Sales);
                        helper.SetData(iRow, 13, cust.TotalAmt);
                        helper.SetData(iRow, 14, cust.CurrentAmt);
                        helper.SetData(iRow, 15, cust.Due30Amt);
                        helper.SetData(iRow, 16, cust.Due60Amt);
                        helper.SetData(iRow, 17, cust.Due90Amt);
                        helper.SetData(iRow, 18, cust.Due180Amt);
                        helper.SetData(iRow, 19, cust.Due360Amt);
                        helper.SetData(iRow, 20, cust.DueOver360Amt);
                        helper.SetData(iRow, 21, cust.DueOver60Amt);
                        helper.SetData(iRow, 22, cust.AdjustedDueOver60Amt);
                        helper.SetData(iRow, 23, cust.DueOver90Amt);
                        helper.SetData(iRow, 24, cust.AdjustedDueOver90Amt);
                        helper.SetData(iRow, 25, cust.DueOver180Amt);
                        helper.SetData(iRow, 26, cust.OneYearSales);
                        helper.CopyStyle(iRow, iRow - 1);
                        iRow++;
                    }
                }
                helper.DeleteRows(1);
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

                fileService.downloadFileInsert(strReportName, strportPath, strimportId, UploadStates.Success);
                #endregion
            }
            catch (OTCServiceException ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                fileService.downloadFileInsert(strReportName, "", strimportId, UploadStates.Failed);
                throw ex;
            }

        }

        /// <summary>
        /// Upload One Year Sales
        /// </summary>
        /// <returns></returns>
        public void uploadOneYearSales()
        {
            List<CustomerGroupCfg> listTable = new List<CustomerGroupCfg>();
            Customer cusfrFile = new Customer();
            Customer updateCust = new Customer();
            string strimportId;
            strimportId = System.Guid.NewGuid().ToString("N");
            string strCode = "";
            string strpath = "";
            FileUploadHistory fileUpHis = new FileUploadHistory();
            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");

            List<CustomerGroupCfgStaging> grpStaings = new List<CustomerGroupCfgStaging>();
            CustomerGroupCfgStaging grpStaing;

            bool blStartData = false;

            try
            {

                //get T_customer
                listTable = (from cus in GetAllCustomerGroup()
                             where cus.Deal == CurrentDeal
                             select cus).ToList<CustomerGroupCfg>();

                //get the fileName from table
                strCode = Helper.EnumToCode<FileType>(FileType.OneYearSales);
                fileUpHis = fileService.GetNewestData(strCode);

                if (fileUpHis == null)
                {
                    Exception ex = new Exception("import file is not found!");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }

                //read excel file
                strpath = fileUpHis.ArchiveFileName;

                //
                NpoiHelper helper = new NpoiHelper(strpath);
                string sheetName = "";
                sheetName = "Summary";

                helper.ActiveSheetName = sheetName;

                int i = 0;
                string strImpName;
                string strImpCode;
                string strImpAmt;
                do
                {
                    strImpCode = helper.GetValue(i, 0) == null ? null : helper.GetValue(i, 0).ToString();
                    strImpName = helper.GetValue(i, 1) == null ? null : helper.GetValue(i, 1).ToString();
                    strImpAmt = helper.GetValue(i, 2) == null ? null : helper.GetValue(i, 2).ToString();

                    i = i + 1;

                    //"Factory Group Name"
                    if (!blStartData &&
                        strImpCode.ToUpper() == "FACTORY GROUP CODE"
                        && strImpName.ToUpper() == "FACTORY GROUP NAME"
                        && strImpAmt.ToUpper() == "TOTAL")
                    {
                        blStartData = true;
                        continue;
                    }
                    else if (blStartData && !string.IsNullOrEmpty(strImpName))
                    {
                        grpStaing = new CustomerGroupCfgStaging();
                        grpStaing.Deal = CurrentDeal;
                        grpStaing.BillGroupCode = strImpCode;
                        grpStaing.BillGroupName = strImpName;
                        grpStaing.OneYearSales = dataConvertToDec(strImpAmt);
                        grpStaing.UploadTime = AppContext.Current.User.Now;
                        grpStaing.ImportId = strimportId;
                        grpStaings.Add(grpStaing);
                    }

                } while (!(string.IsNullOrEmpty(strImpName) && blStartData));

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    string DelSql;

                    DelSql = "delete from T_CUSTOMER_GROUP_CFG_STAGING WHERE DEAL = '"
                               + CurrentDeal + "';";
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(DelSql);

                    (CommonRep.GetDBContext() as OTCEntities).BulkInsert(grpStaings);

                    scope.Complete();
                }

                strCode = Helper.EnumToCode<FileType>(FileType.OneYearSales);
                fileUpHis = fileService.GetNewestData(strCode);
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Success);
                fileUpHis.DataSize = grpStaings.Count;
                fileUpHis.ImportId = strimportId;
                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                CommonRep.Commit();
                throw ex;
            }
            finally
            {

            }

        }

        public List<CustomerAging> GetAgingByNum(string custNum)
        {
            return CommonRep.GetQueryable<CustomerAging>().ToList().FindAll(m => m.CustomerNum == custNum && m.Deal == CurrentDeal);
        }

        public Boolean checkDelCustomer(Customer cust)
        {
            string custNum = cust.CustomerNum;
            int contCount = CommonRep.GetQueryable<Contactor>().Where(o => o.CustomerNum == custNum && o.Deal == AppContext.Current.User.Deal).Count();
            int bankCount = CommonRep.GetQueryable<CustomerPaymentBank>().Where(o => o.CustomerNum == custNum && o.Deal == AppContext.Current.User.Deal).Count();
            int circleCount = CommonRep.GetQueryable<CustomerPaymentCircle>().Where(o => o.CustomerNum == custNum && o.Deal == AppContext.Current.User.Deal).Count();
            int agingCount = GetAgingByNum(custNum).Count();
            int invoiceCount = CommonRep.GetQueryable<InvoiceAging>().Where(o => o.CustomerNum == custNum && o.Deal == AppContext.Current.User.Deal).Count();
            if (contCount == 0 && bankCount == 0 && circleCount == 0 && agingCount == 0 && invoiceCount == 0)
            {
                return true;
            }
            return false;
        }

        private void SetCellValue(IRow row, int column, object value)
        {
            if (value == null || value is System.DBNull)
            {
                return;
            }
            else
            {
                ICell cell = row.CreateCell(column);
                if (value is DateTime)
                {
                    cell.SetCellValue(Convert.ToDateTime(value.ToString()).ToString("yyyy-MM-dd"));
                }
                else if (value is double)
                {
                    if (!double.IsNaN((double)value))
                    {
                        cell.SetCellValue((double)value);
                    }
                }
                else if (value is int)
                {
                    cell.SetCellValue((int)value);
                }
                else if (value is decimal)
                {
                    cell.SetCellValue(Convert.ToDouble(value));
                }
                else if (value is float)
                {
                    cell.SetCellValue((float)value);
                }
                else if (value is Int64)
                {
                    cell.SetCellValue(Convert.ToDouble(value));
                }
                else
                {
                    cell.SetCellValue(value.ToString());
                }
            }
        }

        public string ImportTWCurrencyAmount()
        {
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            int count = 0;
            try
            {
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings["ArchiveCurrencyTWPath"].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                string strFileName = archivePath + "\\" + files[0].FileName;

                files[0].SaveAs(strFileName);

                NpoiHelper helper = new NpoiHelper(strFileName);
                helper.ActiveSheet = 0;
                int maxRowNumber = helper.GetLastRowNum();  //获得数据总行数


                List<string> listSQL = new List<string>();
                for (int row = 1; row <= maxRowNumber; row++)
                {

                    count = row;

                    if (helper.GetCell(row, 1) == null)
                    {
                        continue;
                    }
                    CellType ct1 = helper.GetCellType(row, 1);
                    CellType ct2 = helper.GetCellType(row, 2);
                    CellType ct8 = helper.GetCellType(row, 8);
                    CellType ct5 = helper.GetCellType(row, 6);
                    CellType ct6 = helper.GetCellType(row, 7);

                    if (ct1 == CellType.Blank && ct2 == CellType.Blank && ct8 == CellType.Blank && ct5 == CellType.Blank && ct6 == CellType.Blank)
                    {
                        continue;
                    }


                    var CustomerNumber = helper.GetCell(row, 1).ToString();
                    var TransactionNumber = helper.GetCell(row, 2).ToString();
                    var OpenBalanceinTransactionalCurrency = decimal.Parse(helper.GetCell(row, 8).ToString());
                    CellType ct = helper.GetCellType(row, 4);

                    if (ct == CellType.Error)
                    {
                        continue;
                    }
                    var amt = helper.GetCell(row, 4).NumericCellValue;
                    var DueDate = helper.GetCell(row, 6).DateCellValue.ToString("yyyy-MM-dd");
                    var TransactionalCurrencyCode = helper.GetCell(row, 7).ToString();




                    StringBuilder sql = new StringBuilder();
                    sql.Append("UPDATE T_INVOICE_AGING SET RemainingAmtTran = " + amt + ",RemainingAmtTran1=" + amt);
                    sql.Append("  WHERE INVOICE_NUM =  ");
                    sql.Append("'" + TransactionNumber + "'");
                    sql.Append("  AND CUSTOMER_NUM =  ");
                    sql.Append("'" + CustomerNumber + "'");

                    sql.Append("  AND DUE_DATE >= ");
                    sql.Append("'" + DueDate + " 00:00:00'");
                    sql.Append("  AND DUE_DATE <= ");
                    sql.Append("'" + DueDate + " 23:59:59'");
                    sql.Append("  AND CURRENCY = ");
                    sql.Append("'" + TransactionalCurrencyCode + "'");
                    sql.Append("AND BALANCE_AMT =");
                    sql.Append(OpenBalanceinTransactionalCurrency);
                    listSQL.Add(sql.ToString());

                }

                SqlHelper.ExcuteListSql(listSQL);
            }
            catch (Exception ex)
            {
                count = count;
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!");
            }
            return "upload success";
        }

        public string ImportATMCurrencyAmount()
        {
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            int count = 0;
            try
            {
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings["ArchiveCurrencyTWPath"].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                string strFileName = archivePath + "\\" + files[0].FileName;

                files[0].SaveAs(strFileName);

                NpoiHelper helper = new NpoiHelper(strFileName);
                helper.ActiveSheet = 0;
                int maxRowNumber = helper.GetLastRowNum();  //获得数据总行数


                List<string> listSQL = new List<string>();
                for (int row = 1; row <= maxRowNumber; row++)
                {

                    count = row;

                    if (helper.GetCell(row, 1) == null)
                    {
                        continue;
                    }
                    CellType ct1 = helper.GetCellType(row, 1);
                    CellType ct2 = helper.GetCellType(row, 2);
                    CellType ct3 = helper.GetCellType(row, 6);
                    CellType ct5 = helper.GetCellType(row, 9);
                    CellType ct6 = helper.GetCellType(row, 10);
                    CellType ct7 = helper.GetCellType(row, 12);
                    CellType ct8 = helper.GetCellType(row, 22);

                    if (ct1 == CellType.Blank && ct2 == CellType.Blank && ct3 == CellType.Blank && ct5 == CellType.Blank && ct6 == CellType.Blank && ct7 == CellType.Blank && ct8 == CellType.Blank)
                    {
                        continue;
                    }


                    var CustomerNumber = helper.GetCell(row, 1).ToString();
                    var SiteUseId = helper.GetCell(row, 2).ToString();
                    var InvoiceNumber = helper.GetCell(row, 6).ToString();
                    var TransactionalCurrencyCode = helper.GetCell(row, 10).ToString();
                    var OpenBalanceinTransactionalCurrency = decimal.Parse(helper.GetCell(row, 12).ToString());
                    CellType ct = helper.GetCellType(row, 22);

                    if (ct == CellType.Error)
                    {
                        continue;
                    }
                    var amt = helper.GetCell(row, 22).NumericCellValue;
                    var DueDate = helper.GetCell(row, 9).DateCellValue.ToString("yyyy-MM-dd");




                    StringBuilder sql = new StringBuilder();
                    sql.Append("UPDATE T_INVOICE_AGING SET RemainingAmtTran = " + amt + ",RemainingAmtTran1=" + amt);
                    sql.Append("  WHERE INVOICE_NUM =  ");
                    sql.Append("'" + InvoiceNumber + "'");
                    sql.Append("  AND CUSTOMER_NUM =  ");
                    sql.Append("'" + CustomerNumber + "'");
                    sql.Append("  AND SiteUseId =  ");
                    sql.Append("'" + SiteUseId + "'");
                    sql.Append("  AND DUE_DATE >= ");
                    sql.Append("'" + DueDate + " 00:00:00'");
                    sql.Append("  AND DUE_DATE <= ");
                    sql.Append("'" + DueDate + " 23:59:59'");
                    sql.Append("  AND CURRENCY = ");
                    sql.Append("'" + TransactionalCurrencyCode + "'");
                    sql.Append("AND BALANCE_AMT =");
                    sql.Append(OpenBalanceinTransactionalCurrency);
                    listSQL.Add(sql.ToString());

                }

                SqlHelper.ExcuteListSql(listSQL);
            }
            catch (Exception ex)
            {
                count = count;
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!");
            }
            return "upload success";
        }

        public string ImportConsigmentNumber()
        {
            FileType fileT = FileType.ConsigmentNumber;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                //upload file to server
                string strMasterDataKey = "ImportConsigmentNumber";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strMasterDataKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + ".xlsx";

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileT);

                return ImportConsigmentNumberDo();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!");
            }
        }

        public string ImportConsigmentNumberDo()
        {
            string strCode = "";
            FileUploadHistory fileUpHis = new FileUploadHistory();
            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            try
            {
                //工作区结束行号
                strCode = Helper.EnumToCode<FileType>(FileType.ConsigmentNumber);
                fileUpHis = fileService.GetSuccessData(strCode);
                string strpath = "";
                if (fileUpHis == null)
                {
                    throw new Exception("import file is not found!");
                }
                strpath = fileUpHis.ArchiveFileName;
                List<string> listcust = new List<string>();
                #region openXml
                ExcelPackage package = new ExcelPackage(new FileInfo(strpath));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号
                #endregion
                rowStart = 2;
                int icountsum = 0;
                for (int j = rowStart; j <= rowEnd; j++)
                {
                    if (worksheet.Cells[j, 2].Value == null) { continue; }
                    if (worksheet.Cells[j, 8].Value == null) { continue; }
                    if (worksheet.Cells[j, 11].Value == null) { continue; }
                    string SiteUseId = worksheet.Cells[j, 2].Value.ToString();
                    string strInvoiceNum = worksheet.Cells[j, 8].Value.ToString();
                    if (!string.IsNullOrEmpty(strInvoiceNum))
                    {
                        //判断是否为有效的发票号
                        var invRow = (from o in CommonRep.GetQueryable<InvoiceAging>()
                                      where o.InvoiceNum == strInvoiceNum
                                      select o).ToList();
                        if (invRow != null && invRow.Count > 0)
                        {
                            DateTime dueDate = Convert.ToDateTime(worksheet.Cells[j, 4].Value);
                            string consigmentNumber = worksheet.Cells[j, 11].Value.ToString();
                            SqlHelper.ExcuteSql("update T_INVOICE_AGING set ConsignmentNumber = '" + consigmentNumber + "" +
                                "' where INVOICE_NUM = '" + strInvoiceNum + "' and SiteUseId = '" + SiteUseId + "' and DUE_DATE = '" + dueDate.ToString("yyyy-MM-dd") + "'; ", null);
                            icountsum++;
                        }
                    }
                }
                return "Import Finished!" + "Updated " + icountsum + " invoices.";
            }
            catch (Exception ex)
            {
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                fileService.CommonRep.Commit();
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
    }

    /// <summary>
    /// Include the method needed to assign the collector to customer aging automatically.
    /// </summary>
    public interface ICollectorAssignmentStratege
    {
        List<Collector> Collectors { get; set; }
        String AssignCollector(CustomerClass cclass);
    }

    public class ByValueAssignment : ICollectorAssignmentStratege
    {
        public List<Collector> Collectors { get; set; }
        public ICustomerService CustService { get; set; }

        /// <summary>
        /// Assigned tasks to collector
        /// </summary>
        /// <param name="cust"></param>
        public string AssignCollector(CustomerClass cclass)
        {
            // filter collector list by value factor.
            // filter collectors by ValueClass = cusClass
            var filtedCollectors = from sysUser in Collectors
                                   where sysUser.ValueClass == Helper.EnumToCode<CustomerClass>(cclass)
                                   select sysUser;

            // The collectors does not exist
            Boolean flag = false;

            // Get collector assignments for all filtered collectors, balance collector assignments
            foreach (var coll in filtedCollectors)
            {
                // The collectors exist
                flag = true;

                // 1, Get existing customer aging count(from Customer Aging) by collector and set "AssigmentCount" property if it is -1.
                if (coll.AssigmentCount == -1)
                {
                    var retCusAging = CustService.GetCustomerAgingByCollector(coll.EID);
                    if (retCusAging == null)
                    {
                        coll.AssigmentCount = 0;
                    }
                    else
                    {
                        coll.AssigmentCount = retCusAging.Count();
                    }
                }
            }

            // The collectors does not exist
            if (flag == false)
            {
                return string.Empty;
            }

            // 2, choose the minumum in all collectors.
            int min = filtedCollectors.Min(o => o.AssigmentCount);
            var minCol = filtedCollectors.Where(o => o.AssigmentCount == min).Select(o => o).FirstOrDefault();
            // 3, increase the "AssigmentCount" property of choosed collector.
            minCol.AssigmentCount++;

            return minCol.EID;
        }

    }

    public sealed class VarDataMap : ClassMap<T_INVOICE_VARDATA>
    {
        public VarDataMap()
        {
            Map(m => m.IDYear).Name("ID Year");
            Map(m => m.IDQQ).Name("ID QQ");
            Map(m => m.IDMonth).Name("ID Month");
            Map(m => m.BillToCustomerOperatingUnit).Name("Bill To Customer Operating Unit");
            Map(m => m.BillToCustomerOUName).Name("Bill To Customer OU Name");
            Map(m => m.OrderNumber).Name("Order Number");
            Map(m => m.LineNumber).Name("Line Number");
            Map(m => m.VarData1).Name("Var Data 1");
            Map(m => m.VarData2).Name("Var Data 2");
            Map(m => m.InvoiceNumber).Name("Invoice Number");
            Map(m => m.InvoiceLineNumber).Name("Invoice Line Number");
            Map(m => m.AccountNumber).Name("Account Number");
            Map(m => m.AccountName).Name("Account Name");
            Map(m => m.ExtendedResaleUSD).ConvertUsing(row =>
            {
                decimal? dl = null;
                string extendedResaleUSD = row.GetField<string>("Extended Resale USD");
                extendedResaleUSD = extendedResaleUSD.Replace(",", "");
                if (!string.IsNullOrWhiteSpace(extendedResaleUSD))
                {
                    dl = Convert.ToDecimal(extendedResaleUSD);
                }
                return dl;
            });
        }

    }




}
