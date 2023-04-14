using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using log4net.Repository.Hierarchy;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Transactions;
using System.Web;

namespace Intelligent.OTC.Business
{
    public class InvoiceService
    {
        public List<InvoiceAging> lstPFE;
        public List<InvoiceAging> lstPCN;
        public List<InvoiceAging> lstADHK;
        public const string paxarUs = "Paxar - US";
        public const string paxarCn = "Paxar - CN";
        public const string strSoaPathKey = "GenerateSOAPath";//GenerateSOA路径的config保存名
        public const string strSqlTemplatepathKey = "SouceTempSql";
        bool bRCurrency = false;
        public InvoiceService()
        {
        }

        public OTCRepository CommonRep { get; set; }

        #region 
        public IList<InvoiceAging> invoiceInfoGet()
        {
            return CommonRep.GetQueryable<InvoiceAging>().ToList();
        }
        #endregion

        /// <summary>
        /// Get invoice info by customer num
        /// </summary>
        /// <param name="cusnum"></param>
        /// <returns></returns>
        public List<InvoiceAging> invoiceInfoGetByNum(string cusnum)
        {
            return CommonRep.GetQueryable<InvoiceAging>()
                .Where(m => m.CustomerNum == cusnum && m.Deal == AppContext.Current.User.Deal).ToList();
        }

        /// <summary>
        /// Get invoice info by invoice num
        /// </summary>
        /// <param name="invoiceNum"></param>
        /// <returns></returns>
        public InvoiceAging GetInvoiceInfo(string invoiceNum)
        {
            return CommonRep.GetQueryable<InvoiceAging>().FirstOrDefault(m => m.InvoiceNum == invoiceNum);
        }

        /// <summary>
        /// update invoices's overduereason
        /// </summary>
        /// <param name="invoiceNums"></param>
        /// <param name="reason"></param>
        public void UpdateOverdueReason(List<string> invoiceNums, string reason, string comments)
        {
            foreach (var invoiceNum in invoiceNums)
            {
                var invoice = CommonRep.GetQueryable<InvoiceAging>().FirstOrDefault(m => m.InvoiceNum == invoiceNum);
                invoice.OverdueReason = reason;
                invoice.TrackStates = "001";
                CommonRep.Commit();
            }
        }


        #region Set content in SOA template

        
        public List<string> setCaPmtMailContent(string strId, List<int> intids, string strLegalEntity, string strCustomerNum, string strCustomerName, string strTRANSACTION_NUMBER, DateTime? strVALUE_DATE, string strCURRENCY, decimal decCURRENT_AMOUNT, string strDescription, out System.Data.DataTable[] soaReports, string filetype = "XLS")
        {
            List<string> lstReportPath = new List<string>();
            try
            { 
                List<InvoiceAging> invoicelist = new List<InvoiceAging>();
                invoicelist = (from inv in CommonRep.GetQueryable<InvoiceAging>()
                               where intids.Contains(inv.Id)
                               select inv).ToList();
                // 判断获取的结果是否为空
                if (invoicelist == null || invoicelist.Count <= 0)
                {
                    throw new OTCServiceException("There are no existing datas, please refresh.");
                }

                string soaFileName = "";
                soaFileName = DateTime.Now.ToString("yyyyMMdd") + "_CaApplication_" + "_" + strCustomerNum + ".xlsx";

                FileService fs = SpringFactory.GetObjectImpl<FileService>("FileService");
                InvoiceAging nInvoice = invoicelist.FirstOrDefault();
                string templateFile = ConfigurationManager.AppSettings["TemplateCAPMT"].ToString().TrimStart('~').Replace("/", "\\").TrimStart('\\');
                templateFile = Path.Combine(HttpRuntime.AppDomainAppPath, templateFile);

                string tmpFile = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx");
                generateCAPMTExcelAttachment(templateFile, intids, strId, tmpFile, out soaReports, strLegalEntity, strCustomerNum, strCustomerName, strTRANSACTION_NUMBER, strVALUE_DATE, strCURRENCY, decCURRENT_AMOUNT, strDescription);

                if (filetype == "XLS")
                {
                    using (FileStream stream = File.OpenRead(tmpFile))
                    {
                        lstReportPath.Add(fs.AddAppFile(soaFileName, stream, FileType.SOA).FileId);
                    }
                    File.Delete(tmpFile);
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Failed to create SOA Excel!", ex);
                throw;
            }
            return lstReportPath;
        }

        public List<string> setCaPmtMailContentCN(string path,string strId, List<CaBankStatementDto> bslist, List<int> intids, string strLegalEntity, ref decimal decCURRENT_AMOUNT, ref List<string> attachPhysicsPathList, out System.Data.DataTable[] soaReports, string indexFile = "", string filetype = "XLS")
        {
            List<string> lstReportPath = new List<string>();
            try
            {
                List<InvoiceAging> invoicelist = new List<InvoiceAging>();
                invoicelist = (from inv in CommonRep.GetQueryable<InvoiceAging>()
                               where intids.Contains(inv.Id)
                               select inv).ToList();
                // 判断获取的结果是否为空
                if (invoicelist == null || invoicelist.Count <= 0)
                {
                    string strIdin = strId.Replace(",", "','");
                    string strSQL = string.Format("update t_ca_mailalert set comment = 'No AR', ISLOCKED = 0 where id in ('{0}')", strIdin);
                    SqlHelper.ExcuteSql(strSQL);

                    throw new OTCServiceException("There are no existing datas, please refresh.");
                }

                string soaFileName = path;
                //soaFileName = DateTime.Now.ToString("yyyyMMdd") + "_CaApplication_" + "_" + strCustomerNum + ".xlsx";

                FileService fs = SpringFactory.GetObjectImpl<FileService>("FileService");
                InvoiceAging nInvoice = invoicelist.FirstOrDefault();
                //Helper.Log.Info("***********************************TemplateCAPMT-CN: " + "TemplateCAPMT-CN" + indexFile);
                string templateFile = ConfigurationManager.AppSettings["TemplateCAPMT-CN"+indexFile].ToString().TrimStart('~').Replace("/", "\\").TrimStart('\\');
                templateFile = Path.Combine(HttpRuntime.AppDomainAppPath, templateFile);

                string tmpFile = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx");
                generateCAPMTExcelAttachmentCN(templateFile, bslist, intids, strLegalEntity, ref decCURRENT_AMOUNT, tmpFile, out soaReports);

                if (filetype == "XLS")
                {
                    using (FileStream stream = File.OpenRead(tmpFile))
                    {
                        AppFile appFile = fs.AddAppFile(soaFileName, stream, FileType.SOA);
                        lstReportPath.Add(appFile.FileId);
                        attachPhysicsPathList.Add(appFile.PhysicalPath);
                    }
                    File.Delete(tmpFile);
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Failed to create SOA Excel!", ex);
                throw;
            }
            return lstReportPath;
        }
        public List<string> setCaPmtMailContentCNClear(string path, List<CaBankStatementDto> bslist, List<int> intids, string strLegalEntity, ref decimal decCURRENT_AMOUNT, ref List<string> attachPhysicsPathList, out System.Data.DataTable[] soaReports, string indexFile = "", string filetype = "XLS")
        {
            List<string> lstReportPath = new List<string>();
            try
            {
                List<InvoiceAging> invoicelist = new List<InvoiceAging>();
                invoicelist = (from inv in CommonRep.GetQueryable<InvoiceAging>()
                               where intids.Contains(inv.Id)
                               select inv).ToList();
                // 判断获取的结果是否为空
                if (invoicelist == null || invoicelist.Count <= 0)
                {
                    throw new OTCServiceException("There are no existing datas, please refresh.");
                }

                string soaFileName = path;

                FileService fs = SpringFactory.GetObjectImpl<FileService>("FileService");
                InvoiceAging nInvoice = invoicelist.FirstOrDefault();
                string templateFile = ConfigurationManager.AppSettings["TemplateCAPMT-CN-Clear"+indexFile].ToString().TrimStart('~').Replace("/", "\\").TrimStart('\\');
                templateFile = Path.Combine(HttpRuntime.AppDomainAppPath, templateFile);

                Helper.Log.Info("************************************* generateCAPMTExcelAttachmentCNClear start *************************************");
                string tmpFile = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx");
                generateCAPMTExcelAttachmentCNClear(templateFile, bslist, intids, strLegalEntity, decCURRENT_AMOUNT, tmpFile, out soaReports);
                //Helper.Log.Info("************************************* generateCAPMTExcelAttachmentCNClear end *************************************");
                //Helper.Log.Info("****************** tmpFile:" + tmpFile);
                //Helper.Log.Info("****************** filetype:" + filetype);
                if (filetype == "XLS")
                {
                    Helper.Log.Info("******************************** 11111 ********************************");
                    using (FileStream stream = File.OpenRead(tmpFile))
                    {
                        //Helper.Log.Info("******************************** 22222 ********************************");
                        AppFile appFile = fs.AddAppFile(soaFileName, stream, FileType.SOA);
                        //Helper.Log.Info("******************************** appFile.FileId:" + appFile.FileId);
                        //Helper.Log.Info("******************************** appFile.PhysicalPath:" + appFile.PhysicalPath);
                        lstReportPath.Add(appFile.FileId);
                        attachPhysicsPathList.Add(appFile.PhysicalPath);
                    }
                    File.Delete(tmpFile);
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Failed to create SOA Excel!", ex);
                throw;
            }
            return lstReportPath;
        }

        public List<string> setContent(List<int> intids, string Type, out System.Data.DataTable[] soaReports, string CustomerNums, string siteUseId, string language, string collector = "", string ToTitle = "", string ToName = "", string CCTitle = "", string indexFile = "", string filetype = "XLS", bool isShowCommentsFrom = false)
        {
            List<string> custList = CustomerNums.Split(',').ToList();
            List<string> siteUseIdList = siteUseId.Split(',').ToList();

            //根据某一张发票获得LegalEntity(补丁)
            int invIdOne = Convert.ToInt32(intids[0]);
            string strLegalEntity = (from inv in CommonRep.GetQueryable<InvoiceAging>()
                                    where inv.Id == invIdOne
                                     select inv.LegalEntity).FirstOrDefault();

            soaReports = null;
            DateTime now = AppContext.Current.User.Now.Date;
            string deal = AppContext.Current.User.Deal.ToString();

            var TypeTem = "";
            if (Type == "000")
            {
                TypeTem = "Daily";
            }
            if (Type == "001")
            {
                TypeTem = "Wave1";
            }
            else if (Type == "002")
            {
                TypeTem = "Wave2";
            }
            else if (Type == "003")
            {
                TypeTem = "Wave3";
            }
            else if (Type == "004")
            {
                TypeTem = "Wave4";
            }
            else if (Type == "005")
            {
                TypeTem = "PMT";
            }
            List<int> intidsAll = new List<int>();
            for (int i = 0; i < intids.Count; i++) {
                intidsAll.Add(intids[i]);
            }

            List<int> intids1 = new List<int>();
            List<int> intids2 = new List<int>();
            List<int> intids3 = new List<int>();
            List<int> intids4 = new List<int>();
            List<int> intids5 = new List<int>();
            List<int> intids6 = new List<int>();
            List<int> intids7 = new List<int>();
            List<int> intids8 = new List<int>();
            List<int> intids9 = new List<int>();

            if (intidsAll.Count > 90000)
            {
                for (int i = intidsAll.Count; i >= 90001; i--)
                {
                    intids9.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }
            if (intidsAll.Count > 80000)
            {
                for (int i = intidsAll.Count; i >= 80001; i--)
                {
                    intids8.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }
            if (intidsAll.Count > 70000)
            {
                for (int i = intidsAll.Count; i >= 70001; i--)
                {
                    intids7.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }
            if (intidsAll.Count > 60000)
            {
                for (int i = intidsAll.Count; i >= 60001; i--)
                {
                    intids6.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }
            if (intidsAll.Count > 50000)
            {
                for (int i = intidsAll.Count; i >= 50001; i--)
                {
                    intids5.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }
            if (intidsAll.Count > 40000)
            {
                for (int i = intidsAll.Count; i >= 40001; i--)
                {
                    intids4.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }
            if (intidsAll.Count > 30000)
            {
                for (int i = intidsAll.Count; i >= 30001; i--)
                {
                    intids3.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }
            if (intidsAll.Count > 20000)
            {
                for (int i = intidsAll.Count; i >= 20001; i--)
                {
                    intids2.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }
            if (intidsAll.Count > 10000)
            {
                for (int i = intidsAll.Count; i >= 10001; i--)
                {
                    intids1.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }

            Helper.Log.Info("intidsAll:" + intidsAll.Count());
            Helper.Log.Info("intids1:" + intids1.Count());
            Helper.Log.Info("intids2:" + intids2.Count());
            Helper.Log.Info("intids3:" + intids3.Count());
            Helper.Log.Info("intids4:" + intids4.Count());
            Helper.Log.Info("intids5:" + intids5.Count());
            Helper.Log.Info("intids6:" + intids6.Count());
            Helper.Log.Info("intids7:" + intids7.Count());
            Helper.Log.Info("intids8:" + intids8.Count());
            Helper.Log.Info("intids9:" + intids9.Count());

            List<InvoiceAging> invoicelist = new List<InvoiceAging>();
            List<InvoiceAging> invoicelist1 = new List<InvoiceAging>();
            List<InvoiceAging> invoicelist2 = new List<InvoiceAging>();
            List<InvoiceAging> invoicelist3 = new List<InvoiceAging>();
            List<InvoiceAging> invoicelist4 = new List<InvoiceAging>();
            using (var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions()
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                invoicelist = (from inv in CommonRep.GetQueryable<InvoiceAging>()
                               where intidsAll.Contains(inv.Id) ||
                                     intids1.Contains(inv.Id)
                               select inv).ToList();
                invoicelist1 = (from inv in CommonRep.GetQueryable<InvoiceAging>()
                                where intids2.Contains(inv.Id) || 
                                      intids3.Contains(inv.Id)
                               select inv).ToList();
                invoicelist2 = (from inv in CommonRep.GetQueryable<InvoiceAging>()
                                where intids4.Contains(inv.Id) ||
                                      intids5.Contains(inv.Id)
                                select inv).ToList();
                invoicelist3 = (from inv in CommonRep.GetQueryable<InvoiceAging>()
                                where intids6.Contains(inv.Id) ||
                                      intids7.Contains(inv.Id)
                                select inv).ToList();
                invoicelist4 = (from inv in CommonRep.GetQueryable<InvoiceAging>()
                                where intids8.Contains(inv.Id) ||
                                      intids9.Contains(inv.Id)
                                select inv).ToList();
                invoicelist.AddRange(invoicelist1);
                invoicelist.AddRange(invoicelist2);
                invoicelist.AddRange(invoicelist3);
                invoicelist.AddRange(invoicelist4);
                scope.Complete();
            }

            //生成的文件路径的列表
            List<string> lstReportPath = new List<string>();

            //获取对应的site信息
            List<Sites> lstSites = CommonRep.GetDbSet<Sites>().ToList();

            try
            {
                // 判断获取的结果是否为空
                if (invoicelist == null || invoicelist.Count <= 0)
                {
                    throw new OTCServiceException("There are no existing datas, please refresh.");
                }

                foreach (var item in intids)
                {
                    if (invoicelist.Find(m => m.Id == item) == null)
                    {
                        throw new OTCServiceException("There are no existing datas, please refresh.");
                    }
                }

                string alia = "";
                string stamp = "";

                string soaFileName = "";
                string soaPdfFileName = "";
                if (language == "006" || language == "007" || language == "0071" || language == "009")  //To Customer
                {
                    if (language == "006" || language == "009")
                    {
                        var eb = (from cus in CommonRep.GetQueryable<Customer>()
                                  where custList.Contains(cus.CustomerNum)
                                  select cus.Ebname).FirstOrDefault();
                        var custItem = (from cus in CommonRep.GetQueryable<Customer>()
                                           where custList.Contains(cus.CustomerNum)
                                           select cus).FirstOrDefault();
                        string strcname = "";
                        string[] strcnameGroup = null;
                        if (custItem != null)
                        {
                            if (!string.IsNullOrEmpty(custItem.GroupName)) {
                                strcname = custItem.GroupName;
                            } else { 
                                strcnameGroup = custItem.CustomerName.Split(' ');
                            }
                        }
                        else {
                            strcname = "";
                        }
                        string strCNName = "";
                        if (strcnameGroup != null && strcnameGroup.Length > 0)
                        {
                            strCNName = strcnameGroup[0];
                            if (strcnameGroup.Length > 1 && strcnameGroup[1] != null)
                            {
                                strCNName += " " + strcnameGroup[1];
                            }
                        }
                        else {
                            strCNName = strcname;
                        }
                        if (eb == null) { eb = "          "; }
                        if (Type == "001")
                        {
                            string strprePeriodEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM") + "-01").AddDays(-1).ToString("yyyyMMdd");
                            soaFileName = strprePeriodEndDate + "_" + TypeTem + "_" + strCNName + "_" + eb.Substring(3,2) + "_" + CustomerNums + "(" + strLegalEntity + "-" + collector + ")" + ".xlsx";
                            soaPdfFileName = strprePeriodEndDate + "_" + TypeTem + "_" + strCNName + "_" + eb.Substring(3, 2) + "_" + CustomerNums + "(" + strLegalEntity + "-" + collector + ")" + ".pdf";
                        }
                        else
                        {
                            soaFileName = DateTime.Now.ToString("yyyyMMdd") + "_" + strCNName + "_" + eb.Substring(3, 2) + "_" + CustomerNums + "(" + strLegalEntity + "-" + collector + ")" + ".xlsx";
                            soaPdfFileName = DateTime.Now.ToString("yyyyMMdd") + "_" + strCNName + "_" + eb.Substring(3, 2) + "_" + CustomerNums + "(" + strLegalEntity + "-" + collector + ")" + ".pdf";
                        }
                    } else { 
                        soaFileName = DateTime.Now.ToString("yyyyMMdd") + "_" + TypeTem + "_" + strLegalEntity + "_" + collector + "_To_" + CustomerNums + ".xlsx";
                        soaPdfFileName = DateTime.Now.ToString("yyyyMMdd") + "_" + TypeTem + "_" + strLegalEntity + "_" + collector + "_To_" + CustomerNums + ".pdf";
                    }
                }
                else {
                    if (!string.IsNullOrEmpty(CustomerNums))
                    {
                        soaFileName = DateTime.Now.ToString("yyyyMMdd") + "_" + TypeTem + "_" + strLegalEntity + "_" + collector + "_" + CustomerNums + ".xlsx";
                        soaPdfFileName = DateTime.Now.ToString("yyyyMMdd") + "_" + TypeTem + "_" + strLegalEntity + "_" + collector + "_" + CustomerNums + ".pdf";
                    }
                    else
                    {
                        soaFileName = DateTime.Now.ToString("yyyyMMdd") + "_" + TypeTem + "_" + strLegalEntity + "_" + collector + "_" + ToName + ".xlsx";
                        soaPdfFileName = DateTime.Now.ToString("yyyyMMdd") + "_" + TypeTem + "_" + strLegalEntity + "_" + collector + "_" + ToName + ".pdf";
                    }
                    if (!string.IsNullOrEmpty(siteUseId))
                    {
                        soaFileName = DateTime.Now.ToString("yyyyMMdd") + "_" + TypeTem + "_" + strLegalEntity + "_" + collector + "_To_" + ToName + "(" + siteUseId + ")" + ".xlsx";
                        soaPdfFileName = DateTime.Now.ToString("yyyyMMdd") + "_" + TypeTem + "_" + strLegalEntity + "_" + collector + "_To_" + ToName + "(" + siteUseId + ")" + ".pdf";
                    }
                }

                FileService fs = SpringFactory.GetObjectImpl<FileService>("FileService");

                InvoiceAging nInvoice = invoicelist.FirstOrDefault();

                //模板文件 
                string templateFile = "";
                string templateLanguage = "CN";
                if (language == "001")
                {
                    templateLanguage = "CN";
                }
                else if (language == "002")
                {
                    templateLanguage = "TW";
                }
                else if (language == "003")
                {
                    templateLanguage = "SAP";
                }
                else if (language == "004")
                {
                    templateLanguage = "ATWSZ";
                }
                else if (language == "005")
                {
                    templateLanguage = "KR";
                }
                else if (language == "006" || language == "008")
                {
                    templateLanguage = "ASEAN";
                }
                else if (language == "007" || language == "0071")
                {
                    //如果是296的，单独一种模板
                    if ((invoicelist[0].LegalEntity == "296" || invoicelist[0].LegalEntity == "3641") && TypeTem != "PMT")
                    {
                        templateLanguage = "HK296";
                    }
                    else
                    {
                        templateLanguage = "HK";
                    }
                }
                else if (language == "009")
                {
                    templateLanguage = "AseanConsignment";
                }
                else if (language == "011") {
                    if (invoicelist[0].LegalEntity == "308")
                    {
                        templateLanguage = "AU";
                    }
                    else if (invoicelist[0].LegalEntity == "309")
                    {
                        templateLanguage = "NZ";
                    }
                }
                Helper.Log.Info("****************************:" + "TemplateSOA" + templateLanguage + TypeTem + indexFile);
                templateFile = ConfigurationManager.AppSettings["TemplateSOA" + templateLanguage + TypeTem + indexFile].ToString().TrimStart('~').Replace("/", "\\").TrimStart('\\');
                templateFile = Path.Combine(HttpRuntime.AppDomainAppPath, templateFile);
                        
                string tmpFile = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx");
                string tmpPdfFile = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".pdf");
                
                if (TypeTem != "PMT")
                {
                    generateSOAExcelAttachment(templateFile, intids, tmpFile, tmpPdfFile, out soaReports, Type, language, stamp, filetype, CustomerNums, isShowCommentsFrom);
                }
                else {
                    //PMT需要根据PMT记录数，动态生成Sheet页
                    generatePMTExcelAttachment(templateFile, intids, tmpFile, tmpPdfFile, out soaReports, Type, language, stamp, filetype);
                }

                if (filetype == "ALL" || filetype == "XLS")
                {
                    using (FileStream stream = File.OpenRead(tmpFile))
                    {
                        lstReportPath.Add(fs.AddAppFile(soaFileName, stream, FileType.SOA, false).FileId);
                    }
                    if (File.Exists(tmpFile))
                    {
                        File.Delete(tmpFile);
                    }
                }
                if (filetype == "ALL" || filetype == "PDF") {
                    using (FileStream stream = File.OpenRead(tmpPdfFile))
                    {
                        lstReportPath.Add(fs.AddAppFile(soaPdfFileName, stream, FileType.SOA, false).FileId);
                    }
                    if (File.Exists(tmpPdfFile))
                    {
                        File.Delete(tmpPdfFile);
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Failed to create SOA Excel!", ex);
                throw;
            }
            return lstReportPath;
        }
      
        private static string getVirtualFullPath(string fileName)
        {
            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }
            string outName = appUriBuilder.ToString();
            outName += (ConfigurationManager.AppSettings[strSoaPathKey].ToString().Trim('~') + fileName);
            return outName;
        }
        public string getVirtualFullPathCommon(string fileName)
        {
            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }
            string outName = appUriBuilder.ToString();
            outName += (fileName.Trim('~'));
            return outName;
        }
        #endregion

        public Customer AttachCustomer { get; set; }
        public Sites SitesInfo { get; set; }

        public string GetCurrentDate(string format)
        {
            return DateTime.Now.ToString(format);
        }

        public string GetTotalAmtmoney()
        {
            var noSOAItem = (from o in CommonRep.GetQueryable<SysTypeDetail>()
                             where o.TypeCode == "040"
                             select o.DetailValue);

            var invos = from pj in CommonRep.GetQueryable<T_Invoice_Detail>()
                        where noSOAItem.Contains(pj.PartNumber)
                        select pj.InvoiceNumber;

            var totalamount = (from p in CommonRep.GetQueryable<InvoiceAging>().Where(x => idlist.Contains(x.Id)
                                 && x.Class == "INV" && x.BalanceAmt >= 1
                                )
            select p.BalanceAmt).Sum();

            //已对账金额
            var invoiceNo = (from p in CommonRep.GetQueryable<InvoiceAging>().Where(x => idlist.Contains(x.Id)
                                 && !invos.Contains(x.InvoiceNum)
                                 && x.Class == "INV"
                                )
                             select p.InvoiceNum);
            return String.Format("{0:N}", totalamount);
        }

        public string GetDueTotalAmtmoney()
        {
            var noSOAItem = (from o in CommonRep.GetQueryable<SysTypeDetail>()
                             where o.TypeCode == "040"
                             select o.DetailValue);

            var overduetotal = (from p in CommonRep.GetQueryable<InvoiceAging>().Where(
                                    x => idlist.Contains(x.Id))
                                select p.BalanceAmt).Sum();

            return String.Format("{0:N}", overduetotal);
        }

        public string GetDueTotalAmtmoney1()
        {
            var overduetotal = (from p in CommonRep.GetQueryable<InvoiceAging>().Where(
                                    x => idlist.Contains(x.Id)
                                    && x.DueDate < DateTime.Now
                                )
                                select p.BalanceAmt).Sum();

            return String.Format("{0:N}", overduetotal);
        }

        public List<int> idlist { get; set; }
        private Dictionary<string, string> getAttachmentHeaderDict(string customerNum, string siteUseId, List<int> intids)
        {
            System.Int32[] idstrs = intids.ToArray();
            AttachCustomer = CommonRep.GetQueryable<Customer>().Where(x => x.CustomerNum == customerNum && x.SiteUseId == siteUseId).FirstOrDefault();
            if (AttachCustomer.CustomerNum == null)
                return null;
            idlist = intids;
            this.GetTotalAmtmoney();    //现改为“未对账金额”
            this.GetDueTotalAmtmoney(); //现改为“合计未付款金额”
            this.GetDueTotalAmtmoney1();    //"OverDue金额"
            var legalentity = AttachCustomer.Organization;
            SitesInfo = CommonRep.GetQueryable<Sites>().Where(x => x.Deal == AppContext.Current.User.Deal && x.LegalEntity == legalentity).FirstOrDefault();
            Dictionary<string, string> nDict = new Dictionary<string, string>();
            string typeCode = "036";
            IBaseDataService bdService = SpringFactory.GetObjectImpl<IBaseDataService>("BaseDataService");
            List<SysTypeDetail> tokens = bdService.GetSysTypeDetail(typeCode).ToList();
            foreach (var item in tokens)
            {
                nDict[item.DetailName] = getTockeValues(item.DetailValue, item.DetailValue2);
            }

            return nDict;
        }

        private string getTockeValues(string detailValue, string detailValue2)
        {
            if (!string.IsNullOrEmpty(detailValue))
            {
                return detailValue;
            }
            else
            {
                string resValue = string.Empty;
                if (!string.IsNullOrEmpty(detailValue2))
                {
                    // If resObj is null. which means no obj to evaluate on. The expression can work on itself.
                    var resObjValue = Spring.Expressions.ExpressionEvaluator.GetValue(this, detailValue2);
                    resValue = resObjValue == null ? string.Empty : resObjValue.ToString();
                }
                return resValue;
            }
        }

        public List<string> Read()
        {
            //SouceTempSql
            string path = ConfigurationManager.AppSettings[strSqlTemplatepathKey].ToString().TrimStart('~').Replace("/", "\\").TrimStart('\\');
            path = Path.Combine(HttpRuntime.AppDomainAppPath, path);
            StreamReader sr = new StreamReader(path, Encoding.UTF8);
            String line;
            List<string> sqllist = new List<string>(); 
            while ((line = sr.ReadLine()) != null)
            {
                sqllist.Add(line);
            }
            return sqllist;
        }

        private void generateSOAExcelAttachment(string templateFileName, IEnumerable<int> intids, string repfileName, string tmpPdfFile, out System.Data.DataTable[] mailEntryData, string type, string language, string stamp, string filetype, string CustomerNums, Boolean isShowCommentsFrom)
        {
            System.Data.DataTable sheet1_entryData;
            System.Data.DataTable sheet2_entryData;

            if (string.IsNullOrEmpty(CustomerNums)) { CustomerNums = ""; }

            List<string> customerNumList = CustomerNums.Split(',').ToList();

            mailEntryData = null;
            List<string> sqlsource = null;
            sqlsource = Read();
            string mailSourceSql = "";
            string mailSourceSqlSub = "";
            string customerName = "";
            if (language == "006" || language == "008")
            {
                mailSourceSql = sqlsource[1];
                mailSourceSqlSub = sqlsource[2];
                customerName = (from c in CommonRep.GetQueryable<Customer>()
                                where customerNumList.Contains(c.CustomerNum)
                                select c.CustomerName).FirstOrDefault();
            }
            else if (language == "007" || language == "0071")
            {
                mailSourceSql = sqlsource[1];
                mailSourceSqlSub = sqlsource[4];
                customerName = (from c in CommonRep.GetQueryable<Customer>()
                                where customerNumList.Contains(c.CustomerNum)
                                select c.CustomerName).FirstOrDefault();
            }
            else if (language == "009")
            {
                mailSourceSql = sqlsource[1];
                mailSourceSqlSub = sqlsource[5];
                customerName = (from c in CommonRep.GetQueryable<Customer>()
                                where customerNumList.Contains(c.CustomerNum)
                                select c.CustomerName).FirstOrDefault();
            }
            else
            {
                mailSourceSql = sqlsource[0];
            }

            string strGroupName = (from c in CommonRep.GetQueryable<Customer>()
                                   where customerNumList.Contains(c.CustomerNum)
                                   select c.GroupName).FirstOrDefault();

            if (!string.IsNullOrEmpty(strGroupName))
            {
                customerName = strGroupName;
            }

            List<int> listIntids = intids.ToList();

            //所有发票ID
            List<int> intidsAll = new List<int>();
            for (int i = 0; i < listIntids.Count; i++)
            {
                intidsAll.Add(listIntids[i]);
            }

            List<int> intids1 = new List<int>();
            intids1.Add(0);
            List<int> intids2 = new List<int>();
            intids2.Add(0);
            List<int> intids3 = new List<int>();
            intids3.Add(0);
            List<int> intids4 = new List<int>();
            intids4.Add(0);
            List<int> intids5 = new List<int>();
            intids5.Add(0);
            List<int> intids6 = new List<int>();
            intids6.Add(0);
            List<int> intids7 = new List<int>();
            intids7.Add(0);
            List<int> intids8 = new List<int>();
            intids8.Add(0);
            List<int> intids9 = new List<int>();
            intids9.Add(0);

            if (intidsAll.Count > 90000)
            {
                for (int i = intidsAll.Count; i >= 90001; i--)
                {
                    intids9.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }
            if (intidsAll.Count > 80000)
            {
                for (int i = intidsAll.Count; i >= 80001; i--)
                {
                    intids8.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }
            if (intidsAll.Count > 70000)
            {
                for (int i = intidsAll.Count; i >= 70001; i--)
                {
                    intids7.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }
            if (intidsAll.Count > 60000)
            {
                for (int i = intidsAll.Count; i >= 60001; i--)
                {
                    intids6.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }
            if (intidsAll.Count > 50000)
            {
                for (int i = intidsAll.Count; i >= 50001; i--)
                {
                    intids5.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }
            if (intidsAll.Count > 40000)
            {
                for (int i = intidsAll.Count; i >= 40001; i--)
                {
                    intids4.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }
            if (intidsAll.Count > 30000)
            {
                for (int i = intidsAll.Count; i >= 30001; i--)
                {
                    intids3.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }
            if (intidsAll.Count > 20000)
            {
                for (int i = intidsAll.Count; i >= 20001; i--)
                {
                    intids2.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }
            if (intidsAll.Count > 10000)
            {
                for (int i = intidsAll.Count; i >= 10001; i--)
                {
                    intids1.Add(intidsAll[i - 1]);
                    intidsAll.RemoveAt(i - 1);
                }
            }

            string idstrs = string.Join<int>(",", intidsAll);
            string idstrs1 = string.Join<int>(",", intids1);
            string idstrs2 = string.Join<int>(",", intids2);
            string idstrs3 = string.Join<int>(",", intids3);
            string idstrs4 = string.Join<int>(",", intids4);
            string idstrs5 = string.Join<int>(",", intids5);
            string idstrs6 = string.Join<int>(",", intids6);
            string idstrs7 = string.Join<int>(",", intids7);
            string idstrs8 = string.Join<int>(",", intids8);
            string idstrs9 = string.Join<int>(",", intids9);
            // 打开Excel
            NpoiHelper helper = new NpoiHelper(templateFileName);
            helper.Save(repfileName, true);
            try
            {
                helper = new NpoiHelper(repfileName);

                //获得每1个Sheet信息
                var str_Sheet1 = helper.GetStringData(0, 0);
                Dictionary<string, string> tempConfig_Sheet1 = getTemplateConfigDict(str_Sheet1);
                int Sheet1_configheader = int.Parse(tempConfig_Sheet1.Count == 0 || tempConfig_Sheet1["EntryTitleRow"] == null ? "0" : tempConfig_Sheet1["EntryTitleRow"]);
                int Sheet1_EntryFirstCol = int.Parse(tempConfig_Sheet1.Count == 0 || tempConfig_Sheet1["EntryFirstCol"] == null ? "0" : tempConfig_Sheet1["EntryFirstCol"]);
                int Sheet1_EntryDataRow = int.Parse(tempConfig_Sheet1.Count == 0 || tempConfig_Sheet1["EntryDataRow"] == null ? "0" : tempConfig_Sheet1["EntryDataRow"]);
                int Sheet1_lastCol = int.Parse(tempConfig_Sheet1.Count == 0 || tempConfig_Sheet1["LastColumn"] == null ? "0" : tempConfig_Sheet1["LastColumn"]);
                string Sheet1_sourceSql = tempConfig_Sheet1.Count == 0 || tempConfig_Sheet1["EntrySource"] == null ? "" : tempConfig_Sheet1["EntrySource"];
                string Sheet1_RowNumber = tempConfig_Sheet1.Count == 0 || tempConfig_Sheet1["RowNumber"] == null ? "" : tempConfig_Sheet1["RowNumber"];
                string Sheet1_OrderBy = tempConfig_Sheet1.Count == 0 || tempConfig_Sheet1["OrderBy"] == null ? "" : tempConfig_Sheet1["OrderBy"];
                string Sheet1_sourceSql1 = Sheet1_sourceSql;
                string Sheet1_sourceSql2 = Sheet1_sourceSql;
                string Sheet1_sourceSql3 = Sheet1_sourceSql;
                string Sheet1_sourceSql4 = Sheet1_sourceSql;
                string Sheet1_sourceSql5 = Sheet1_sourceSql;
                string Sheet1_sourceSql6 = Sheet1_sourceSql;
                string Sheet1_sourceSql7 = Sheet1_sourceSql;
                string Sheet1_sourceSql8 = Sheet1_sourceSql;
                string Sheet1_sourceSql9 = Sheet1_sourceSql;
                if (Sheet1_sourceSql.Trim().StartsWith("SELECT"))
                {
                    Sheet1_sourceSql = Sheet1_sourceSql.Replace("{ID}", idstrs);
                    Sheet1_sourceSql1 = Sheet1_sourceSql1.Replace("{ID}", idstrs1);
                    Sheet1_sourceSql2 = Sheet1_sourceSql2.Replace("{ID}", idstrs2);
                    Sheet1_sourceSql3 = Sheet1_sourceSql3.Replace("{ID}", idstrs3);
                    Sheet1_sourceSql4 = Sheet1_sourceSql4.Replace("{ID}", idstrs4);
                    Sheet1_sourceSql5 = Sheet1_sourceSql5.Replace("{ID}", idstrs5);
                    Sheet1_sourceSql6 = Sheet1_sourceSql6.Replace("{ID}", idstrs6);
                    Sheet1_sourceSql7 = Sheet1_sourceSql7.Replace("{ID}", idstrs7);
                    Sheet1_sourceSql8 = Sheet1_sourceSql8.Replace("{ID}", idstrs8);
                    Sheet1_sourceSql9 = Sheet1_sourceSql9.Replace("{ID}", idstrs9);
                    string targetSql = Sheet1_RowNumber + "(" + Sheet1_sourceSql + ") AS SOA " + Sheet1_OrderBy;
                    string targetSql1 = Sheet1_RowNumber + "(" + Sheet1_sourceSql1 + ") AS SOA " + Sheet1_OrderBy;
                    string targetSql2 = Sheet1_RowNumber + "(" + Sheet1_sourceSql2 + ") AS SOA " + Sheet1_OrderBy;
                    string targetSql3 = Sheet1_RowNumber + "(" + Sheet1_sourceSql3 + ") AS SOA " + Sheet1_OrderBy;
                    string targetSql4 = Sheet1_RowNumber + "(" + Sheet1_sourceSql4 + ") AS SOA " + Sheet1_OrderBy;
                    string targetSql5 = Sheet1_RowNumber + "(" + Sheet1_sourceSql5 + ") AS SOA " + Sheet1_OrderBy;
                    string targetSql6 = Sheet1_RowNumber + "(" + Sheet1_sourceSql6 + ") AS SOA " + Sheet1_OrderBy;
                    string targetSql7 = Sheet1_RowNumber + "(" + Sheet1_sourceSql7 + ") AS SOA " + Sheet1_OrderBy;
                    string targetSql8 = Sheet1_RowNumber + "(" + Sheet1_sourceSql8 + ") AS SOA " + Sheet1_OrderBy;
                    string targetSql9 = Sheet1_RowNumber + "(" + Sheet1_sourceSql9 + ") AS SOA " + Sheet1_OrderBy;
                    sheet1_entryData = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql, null);
                    System.Data.DataTable sheet1_entryData1 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql1, null);
                    System.Data.DataTable sheet1_entryData2 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql2, null);
                    System.Data.DataTable sheet1_entryData3 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql3, null);
                    System.Data.DataTable sheet1_entryData4 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql4, null);
                    System.Data.DataTable sheet1_entryData5 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql5, null);
                    System.Data.DataTable sheet1_entryData6 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql6, null);
                    System.Data.DataTable sheet1_entryData7 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql7, null);
                    System.Data.DataTable sheet1_entryData8 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql8, null);
                    System.Data.DataTable sheet1_entryData9 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql9, null);

                    object[] obj = new object[sheet1_entryData1.Columns.Count];
                    //添加DataTable1的数据
                    for (int i = 0; i < sheet1_entryData1.Rows.Count; i++)
                    {
                        sheet1_entryData1.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet1_entryData.Rows.Add(obj);
                    }
                    //添加DataTable2的数据
                    for (int i = 0; i < sheet1_entryData2.Rows.Count; i++)
                    {
                        sheet1_entryData2.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet1_entryData.Rows.Add(obj);
                    }
                    //添加DataTable3的数据
                    for (int i = 0; i < sheet1_entryData3.Rows.Count; i++)
                    {
                        sheet1_entryData3.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet1_entryData.Rows.Add(obj);
                    }
                    //添加DataTable4的数据
                    for (int i = 0; i < sheet1_entryData4.Rows.Count; i++)
                    {
                        sheet1_entryData4.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet1_entryData.Rows.Add(obj);
                    }
                    //添加DataTable5的数据
                    for (int i = 0; i < sheet1_entryData5.Rows.Count; i++)
                    {
                        sheet1_entryData5.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet1_entryData.Rows.Add(obj);
                    }
                    //添加DataTable6的数据
                    for (int i = 0; i < sheet1_entryData6.Rows.Count; i++)
                    {
                        sheet1_entryData6.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet1_entryData.Rows.Add(obj);
                    }
                    //添加DataTable7的数据
                    for (int i = 0; i < sheet1_entryData7.Rows.Count; i++)
                    {
                        sheet1_entryData7.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet1_entryData.Rows.Add(obj);
                    }
                    //添加DataTable8的数据
                    for (int i = 0; i < sheet1_entryData8.Rows.Count; i++)
                    {
                        sheet1_entryData8.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet1_entryData.Rows.Add(obj);
                    }
                    //添加DataTable9的数据
                    for (int i = 0; i < sheet1_entryData9.Rows.Count; i++)
                    {
                        sheet1_entryData9.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet1_entryData.Rows.Add(obj);
                    }

                    sheet1_entryData.DefaultView.Sort = "LegalEntity ASC,CustomerCName asc,CustomerNum asc, SiteUseId asc";
                    sheet1_entryData = sheet1_entryData.DefaultView.ToTable();

                    string strPreSiteUseId = "";
                    string strSiteUseId = "";
                    for (int i = sheet1_entryData.Rows.Count - 1; i >= 0; i--)
                    {
                        strSiteUseId = sheet1_entryData.Rows[i]["SiteUseId"].ToString();
                        if (!string.IsNullOrEmpty(strPreSiteUseId) && strSiteUseId == strPreSiteUseId)
                        {
                            sheet1_entryData.Rows.RemoveAt(i);
                        }
                        strPreSiteUseId = strSiteUseId;
                    }

                    for (int i = 0; i < sheet1_entryData.Rows.Count; i++)
                    {
                        sheet1_entryData.Rows[i]["RowNumber"] = (i + 1).ToString();
                    }
                }
                else
                {
                    sheet1_entryData = null;
                }
                //获得第2个Sheet信息
                var str_Sheet2 = helper.GetStringData(1, 0);
                Dictionary<string, string> tempConfig_Sheet2 = getTemplateConfigDict(str_Sheet2);
                int Sheet2_configheader = int.Parse(tempConfig_Sheet2["EntryTitleRow"]);
                int Sheet2_EntryFirstCol = int.Parse(tempConfig_Sheet2["EntryFirstCol"]);
                int Sheet2_EntryDataRow = int.Parse(tempConfig_Sheet2["EntryDataRow"]);
                int Sheet2_lastCol = int.Parse(tempConfig_Sheet2["LastColumn"]);
                string Sheet2_sourceSql = tempConfig_Sheet2["EntrySource"];
                string Sheet2_RowNumber = "";
                string Sheet2_OrderBy = "";
                Sheet2_RowNumber = tempConfig_Sheet1["RowNumber"];
                Sheet2_OrderBy = tempConfig_Sheet1["OrderBy"];
                string Sheet2_sourceSql1 = Sheet2_sourceSql;
                string Sheet2_sourceSql2 = Sheet2_sourceSql;
                string Sheet2_sourceSql3 = Sheet2_sourceSql;
                string Sheet2_sourceSql4 = Sheet2_sourceSql;
                string Sheet2_sourceSql5 = Sheet2_sourceSql;
                string Sheet2_sourceSql6 = Sheet2_sourceSql;
                string Sheet2_sourceSql7 = Sheet2_sourceSql;
                string Sheet2_sourceSql8 = Sheet2_sourceSql;
                string Sheet2_sourceSql9 = Sheet2_sourceSql;
                if (Sheet2_sourceSql.Trim().StartsWith("SELECT"))
                {
                    Sheet2_sourceSql = Sheet2_sourceSql.Replace("{ID}", idstrs);
                    Sheet2_sourceSql1 = Sheet2_sourceSql1.Replace("{ID}", idstrs1);
                    Sheet2_sourceSql2 = Sheet2_sourceSql2.Replace("{ID}", idstrs2);
                    Sheet2_sourceSql3 = Sheet2_sourceSql3.Replace("{ID}", idstrs3);
                    Sheet2_sourceSql4 = Sheet2_sourceSql4.Replace("{ID}", idstrs4);
                    Sheet2_sourceSql5 = Sheet2_sourceSql5.Replace("{ID}", idstrs5);
                    Sheet2_sourceSql6 = Sheet2_sourceSql6.Replace("{ID}", idstrs6);
                    Sheet2_sourceSql7 = Sheet2_sourceSql7.Replace("{ID}", idstrs7);
                    Sheet2_sourceSql8 = Sheet2_sourceSql8.Replace("{ID}", idstrs8);
                    Sheet2_sourceSql9 = Sheet2_sourceSql9.Replace("{ID}", idstrs9);
                    string targetSql2 = Sheet2_RowNumber + "(" + Sheet2_sourceSql + ") AS SOA " + Sheet2_OrderBy;
                    string targetSql21 = Sheet2_RowNumber + "(" + Sheet2_sourceSql1 + ") AS SOA " + Sheet2_OrderBy;
                    string targetSql22 = Sheet2_RowNumber + "(" + Sheet2_sourceSql2 + ") AS SOA " + Sheet2_OrderBy;
                    string targetSql23 = Sheet2_RowNumber + "(" + Sheet2_sourceSql3 + ") AS SOA " + Sheet2_OrderBy;
                    string targetSql24 = Sheet2_RowNumber + "(" + Sheet2_sourceSql4 + ") AS SOA " + Sheet2_OrderBy;
                    string targetSql25 = Sheet2_RowNumber + "(" + Sheet2_sourceSql5 + ") AS SOA " + Sheet2_OrderBy;
                    string targetSql26 = Sheet2_RowNumber + "(" + Sheet2_sourceSql6 + ") AS SOA " + Sheet2_OrderBy;
                    string targetSql27 = Sheet2_RowNumber + "(" + Sheet2_sourceSql7 + ") AS SOA " + Sheet2_OrderBy;
                    string targetSql28 = Sheet2_RowNumber + "(" + Sheet2_sourceSql8 + ") AS SOA " + Sheet2_OrderBy;
                    string targetSql29 = Sheet2_RowNumber + "(" + Sheet2_sourceSql9 + ") AS SOA " + Sheet2_OrderBy;
                    sheet2_entryData = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql2, null);
                    System.Data.DataTable sheet2_entryData21 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql21, null);
                    System.Data.DataTable sheet2_entryData22 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql22, null);
                    System.Data.DataTable sheet2_entryData23 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql23, null);
                    System.Data.DataTable sheet2_entryData24 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql24, null);
                    System.Data.DataTable sheet2_entryData25 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql25, null);
                    System.Data.DataTable sheet2_entryData26 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql26, null);
                    System.Data.DataTable sheet2_entryData27 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql27, null);
                    System.Data.DataTable sheet2_entryData28 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql28, null);
                    System.Data.DataTable sheet2_entryData29 = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, targetSql29, null);


                    object[] obj = new object[sheet2_entryData21.Columns.Count];
                    //添加DataTable1的数据
                    for (int i = 0; i < sheet2_entryData21.Rows.Count; i++)
                    {
                        sheet2_entryData21.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet2_entryData.Rows.Add(obj);
                    }
                    //添加DataTable2的数据
                    for (int i = 0; i < sheet2_entryData22.Rows.Count; i++)
                    {
                        sheet2_entryData22.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet2_entryData.Rows.Add(obj);
                    }
                    //添加DataTable3的数据
                    for (int i = 0; i < sheet2_entryData23.Rows.Count; i++)
                    {
                        sheet2_entryData23.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet2_entryData.Rows.Add(obj);
                    }
                    //添加DataTable4的数据
                    for (int i = 0; i < sheet2_entryData24.Rows.Count; i++)
                    {
                        sheet2_entryData24.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet2_entryData.Rows.Add(obj);
                    }
                    //添加DataTable5的数据
                    for (int i = 0; i < sheet2_entryData25.Rows.Count; i++)
                    {
                        sheet2_entryData25.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet2_entryData.Rows.Add(obj);
                    }
                    //添加DataTable6的数据
                    for (int i = 0; i < sheet2_entryData26.Rows.Count; i++)
                    {
                        sheet2_entryData26.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet2_entryData.Rows.Add(obj);
                    }
                    //添加DataTable7的数据
                    for (int i = 0; i < sheet2_entryData27.Rows.Count; i++)
                    {
                        sheet2_entryData27.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet2_entryData.Rows.Add(obj);
                    }
                    //添加DataTable8的数据
                    for (int i = 0; i < sheet2_entryData28.Rows.Count; i++)
                    {
                        sheet2_entryData28.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet2_entryData.Rows.Add(obj);
                    }
                    //添加DataTable9的数据
                    for (int i = 0; i < sheet2_entryData29.Rows.Count; i++)
                    {
                        sheet2_entryData29.Rows[i].ItemArray.CopyTo(obj, 0);
                        sheet2_entryData.Rows.Add(obj);
                    }
                    if (language == "011") {
                        sheet2_entryData.DefaultView.Sort = "CustomerCName asc,CustomerNum asc, DueDays desc, InvoiceDate asc";
                    } else { 
                        sheet2_entryData.DefaultView.Sort = "CustomerCName asc,CustomerNum asc, SiteUseId asc, DueDays desc, InvoiceDate asc";
                    }
                    sheet2_entryData = sheet2_entryData.DefaultView.ToTable();
                    for (int i = 0; i < sheet2_entryData.Rows.Count; i++)
                    {
                        sheet2_entryData.Rows[i]["RowNumber"] = (i + 1).ToString();
                    }
                }
                else
                {
                    sheet2_entryData = null;
                }

                //Mail正文数据
                try
                {
                    if (mailSourceSql.Trim().StartsWith("SELECT"))
                    {
                        mailSourceSql = mailSourceSql.Replace("{ID}", idstrs);
                        mailSourceSqlSub = mailSourceSqlSub.Replace("{ID}", idstrs);
                        if (language == "006" || language == "008" || language == "007" || language == "0071" || language == "009")
                        {
                            DateTime PeriodEndDate = CommonRep.GetDbSet<PeriodControl>().Where(o => o.Deal == AppContext.Current.User.Deal && o.PeriodBegin <= CurrentTime
                                                    && o.PeriodEnd >= CurrentTime && o.SoaFlg == "1").Select(o => o.EndDate).FirstOrDefault();
                            string strPeriodEndDate = PeriodEndDate.ToString("yyyy-MM-dd HH:mm:ss");
                            if (language == "007" || language == "0071")
                            {
                                strPeriodEndDate = DateTime.Now.ToString("yyyy-MM-dd" + " 23:59:59");
                            }
                            mailSourceSql = mailSourceSql.Replace("{PeriodEnd}", strPeriodEndDate);
                            mailSourceSqlSub = mailSourceSqlSub.Replace("{PeriodEnd}", strPeriodEndDate);
                            System.Data.DataTable dtCurrency = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, mailSourceSql, null);
                            mailEntryData = new System.Data.DataTable[dtCurrency.Rows.Count + 1];
                            mailEntryData[0] = dtCurrency;
                            int currencyCount = 1;
                            foreach (System.Data.DataRow dr in dtCurrency.Rows)
                            {
                                string strCurrency = dr["Currency"].ToString();
                                string mailSourceSqlSub1 = mailSourceSqlSub.Replace("{Currency}", strCurrency);
                                mailEntryData[currencyCount] = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, mailSourceSqlSub1, null);
                                if (language == "006" || language == "008" || language == "009")
                                {
                                    mailEntryData[currencyCount].Columns["Balance Amount"].ColumnName = "Balance Amount " + strCurrency;
                                    //添加合计行
                                    decimal sum = 0;
                                    foreach (System.Data.DataRow drDetail in mailEntryData[currencyCount].Rows)
                                    {
                                        sum += Convert.ToDecimal(drDetail["Balance Amount " + strCurrency]);
                                    }
                                    System.Data.DataRow sumRow = mailEntryData[currencyCount].NewRow();
                                    sumRow["Balance Amount " + strCurrency] = sum;
                                    mailEntryData[currencyCount].Rows.Add(sumRow);
                                }
                                if (language == "007" || language == "0071")
                                {
                                    mailEntryData[currencyCount].Columns["Currency"].ColumnName = "Curr";
                                    mailEntryData[currencyCount].Columns["Amount"].ColumnName = strCurrency;
                                    //添加合计行
                                    decimal sum = 0;
                                    foreach (System.Data.DataRow drDetail in mailEntryData[currencyCount].Rows)
                                    {
                                        sum += Convert.ToDecimal(drDetail[strCurrency]);
                                    }
                                    System.Data.DataRow sumRow = mailEntryData[currencyCount].NewRow();
                                    sumRow[strCurrency] = sum;
                                    mailEntryData[currencyCount].Rows.Add(sumRow);
                                }
                                currencyCount++;
                            }
                        }
                        else
                        {
                            mailEntryData = new System.Data.DataTable[1];
                            mailEntryData[0] = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, mailSourceSql, null);
                        }
                    }
                    else
                    {
                        mailEntryData = null;
                    }
                }
                catch (Exception ex) { }

                //清除第一个单元格的SQL配置信息
                helper.SetData(0, 0, "");
                helper.SetData(1, 0, "");

                //Sheet1汇兑信息
                helper.ActiveSheet = 0;
                ISheet sheet0 = helper.Book.GetSheetAt(0);
                if (language == "001" && sheet1_entryData.Rows.Count > 2)
                {
                    for (int j = 2; j < sheet1_entryData.Rows.Count; j++)
                    {
                        helper.InsertRowWithStyle(7, 1, 6);
                    }
                }
                if (language == "001")
                {

                    int startRowTotal = 0;
                    if (sheet1_entryData.Rows.Count == 1)
                    {
                        startRowTotal = 6 + sheet1_entryData.Rows.Count + 3;
                    }
                    else
                    {
                        startRowTotal = 6 + sheet1_entryData.Rows.Count + 2;
                    }

                    for (int r = 0; r < sheet1_entryData.Rows.Count; r++)
                    {
                        bool lb_flag = false;
                        string strSiteUseId = sheet1_entryData.Rows[r]["SiteUseId"] == null ? "" : sheet1_entryData.Rows[r]["SiteUseId"].ToString();
                        string strSales = sheet1_entryData.Rows[r]["Sales"] == null ? "" : sheet1_entryData.Rows[r]["Sales"].ToString();
                        List<T_Customer_Comments> listComments = new List<T_Customer_Comments>();
                        string commentSQL = string.Format("select * from T_Customer_Comments where siteuseid = '{0}' and isDeleted <> 1 ", strSiteUseId);
                        listComments = SqlHelper.GetList<T_Customer_Comments>(SqlHelper.ExcuteTable(commentSQL, CommandType.Text));
						if (sheet1_entryData.Rows[r]["TotalFutureDue"] != null && Convert.ToDecimal(sheet1_entryData.Rows[r]["TotalFutureDue"]) != 0)
                        {
                            lb_flag = true;
                            helper.InsertRowWithStyle(startRowTotal + 1, 1, startRowTotal);
                            decimal amount = sheet1_entryData.Rows[r]["TotalFutureDue"] == null ? 0 : Convert.ToDecimal(sheet1_entryData.Rows[r]["TotalFutureDue"]);
                            helper.SetData(startRowTotal, 1, strSiteUseId);
                            helper.SetData(startRowTotal, 2, strSales);
                            helper.SetData(startRowTotal, 3, amount);
                            string strAgingBucket = "TotalFutureDue";
                            helper.SetData(startRowTotal, 4, strAgingBucket);
                            if (listComments != null && listComments.Count > 0)
                            {
                                T_Customer_Comments findComments = listComments.Find(o => o.AgingBucket == strAgingBucket);
                                if (findComments != null)
                                {
                                    if (findComments.PTPAmount != null)
                                    {
                                        helper.SetData(startRowTotal, 5, findComments.PTPAmount);
                                    }
                                    if (findComments.PTPDATE != null)
                                    {
                                        helper.SetData(startRowTotal, 6, findComments.PTPDATE);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.OverdueReason))
                                    {
                                        helper.SetData(startRowTotal, 7, findComments.OverdueReason);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.Comments))
                                    {
                                        helper.SetData(startRowTotal, 8, findComments.Comments);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.CommentsFrom) && isShowCommentsFrom)
                                    {
                                        helper.SetData(startRowTotal, 9, findComments.CommentsFrom);
                                    }
                                }
                            }
                            startRowTotal++;
                        }
                        if (sheet1_entryData.Rows[r]["Over360"] != null && Convert.ToDecimal(sheet1_entryData.Rows[r]["Over360"]) != 0)
                        {
                            lb_flag = true;
                            helper.InsertRowWithStyle(startRowTotal + 1, 1, startRowTotal);
                            decimal amount = sheet1_entryData.Rows[r]["Over360"] == null ? 0 : Convert.ToDecimal(sheet1_entryData.Rows[r]["Over360"]);
                            helper.SetData(startRowTotal, 1, strSiteUseId);
                            helper.SetData(startRowTotal, 2, strSales);
                            helper.SetData(startRowTotal, 3, amount);
                            string strAgingBucket = "360+";
                            helper.SetData(startRowTotal, 4, strAgingBucket);
                            if (listComments != null && listComments.Count > 0)
                            {
                                T_Customer_Comments findComments = listComments.Find(o => o.AgingBucket == strAgingBucket);
                                if (findComments != null)
                                {
                                    if (findComments.PTPAmount != null)
                                    {
                                        helper.SetData(startRowTotal, 5, findComments.PTPAmount);
                                    }
                                    if (findComments.PTPDATE != null)
                                    {
                                        helper.SetData(startRowTotal, 6, findComments.PTPDATE);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.OverdueReason))
                                    {
                                        helper.SetData(startRowTotal, 7, findComments.OverdueReason);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.Comments))
                                    {
                                        helper.SetData(startRowTotal, 8, findComments.Comments);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.CommentsFrom) && isShowCommentsFrom)
                                    {
                                        helper.SetData(startRowTotal, 9, findComments.CommentsFrom);
                                    }
                                }
                            }
                            startRowTotal++;
                        }
                        if (sheet1_entryData.Rows[r]["Due360"] != null && Convert.ToDecimal(sheet1_entryData.Rows[r]["Due360"]) != 0)
                        {
                            lb_flag = true;
                            helper.InsertRowWithStyle(startRowTotal + 1, 1, startRowTotal);
                            decimal amount = sheet1_entryData.Rows[r]["Due360"] == null ? 0 : Convert.ToDecimal(sheet1_entryData.Rows[r]["Due360"]);
                            helper.SetData(startRowTotal, 1, strSiteUseId);
                            helper.SetData(startRowTotal, 2, strSales);
                            helper.SetData(startRowTotal, 3, amount);
                            string strAgingBucket = "271-360";
                            helper.SetData(startRowTotal, 4, strAgingBucket);
                            if (listComments != null && listComments.Count > 0)
                            {
                                T_Customer_Comments findComments = listComments.Find(o => o.AgingBucket == strAgingBucket);
                                if (findComments != null)
                                {
                                    if (findComments.PTPAmount != null)
                                    {
                                        helper.SetData(startRowTotal, 5, findComments.PTPAmount);
                                    }
                                    if (findComments.PTPDATE != null)
                                    {
                                        helper.SetData(startRowTotal, 6, findComments.PTPDATE);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.OverdueReason))
                                    {
                                        helper.SetData(startRowTotal, 7, findComments.OverdueReason);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.Comments))
                                    {
                                        helper.SetData(startRowTotal, 8, findComments.Comments);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.CommentsFrom) && isShowCommentsFrom)
                                    {
                                        helper.SetData(startRowTotal, 9, findComments.CommentsFrom);
                                    }
                                }
                            }
                            startRowTotal++;
                        }
                        if (sheet1_entryData.Rows[r]["Due270"] != null && Convert.ToDecimal(sheet1_entryData.Rows[r]["Due270"]) != 0)
                        {
                            lb_flag = true;
                            helper.InsertRowWithStyle(startRowTotal + 1, 1, startRowTotal);
                            decimal amount = sheet1_entryData.Rows[r]["Due270"] == null ? 0 : Convert.ToDecimal(sheet1_entryData.Rows[r]["Due270"]);
                            helper.SetData(startRowTotal, 1, strSiteUseId);
                            helper.SetData(startRowTotal, 2, strSales);
                            helper.SetData(startRowTotal, 3, amount);
                            string strAgingBucket = "181-270";
                            helper.SetData(startRowTotal, 4, strAgingBucket);
                            if (listComments != null && listComments.Count > 0)
                            {
                                T_Customer_Comments findComments = listComments.Find(o => o.AgingBucket == strAgingBucket);
                                if (findComments != null)
                                {
                                    if (findComments.PTPAmount != null)
                                    {
                                        helper.SetData(startRowTotal, 5, findComments.PTPAmount);
                                    }
                                    if (findComments.PTPDATE != null)
                                    {
                                        helper.SetData(startRowTotal, 6, findComments.PTPDATE);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.OverdueReason))
                                    {
                                        helper.SetData(startRowTotal, 7, findComments.OverdueReason);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.Comments))
                                    {
                                        helper.SetData(startRowTotal, 8, findComments.Comments);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.CommentsFrom) && isShowCommentsFrom)
                                    {
                                        helper.SetData(startRowTotal, 9, findComments.CommentsFrom);
                                    }
                                }
                            }
                            startRowTotal++;
                        }
                        if (sheet1_entryData.Rows[r]["Due180"] != null && Convert.ToDecimal(sheet1_entryData.Rows[r]["Due180"]) != 0)
                        {
                            lb_flag = true;
                            helper.InsertRowWithStyle(startRowTotal + 1, 1, startRowTotal);
                            decimal amount = sheet1_entryData.Rows[r]["Due180"] == null ? 0 : Convert.ToDecimal(sheet1_entryData.Rows[r]["Due180"]);
                            helper.SetData(startRowTotal, 1, strSiteUseId);
                            helper.SetData(startRowTotal, 2, strSales);
                            helper.SetData(startRowTotal, 3, amount);
                            string strAgingBucket = "121-180";
                            helper.SetData(startRowTotal, 4, strAgingBucket);
                            if (listComments != null && listComments.Count > 0)
                            {
                                T_Customer_Comments findComments = listComments.Find(o => o.AgingBucket == strAgingBucket);
                                if (findComments != null)
                                {
                                    if (findComments.PTPAmount != null)
                                    {
                                        helper.SetData(startRowTotal, 5, findComments.PTPAmount);
                                    }
                                    if (findComments.PTPDATE != null)
                                    {
                                        helper.SetData(startRowTotal, 6, findComments.PTPDATE);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.OverdueReason))
                                    {
                                        helper.SetData(startRowTotal, 7, findComments.OverdueReason);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.Comments))
                                    {
                                        helper.SetData(startRowTotal, 8, findComments.Comments);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.CommentsFrom) && isShowCommentsFrom)
                                    {
                                        helper.SetData(startRowTotal, 9, findComments.CommentsFrom);
                                    }
                                }
                            }
                            startRowTotal++;
                        }
                        if (sheet1_entryData.Rows[r]["Due120"] != null && Convert.ToDecimal(sheet1_entryData.Rows[r]["Due120"]) != 0)
                        {
                            lb_flag = true;
                            helper.InsertRowWithStyle(startRowTotal + 1, 1, startRowTotal);
                            decimal amount = sheet1_entryData.Rows[r]["Due120"] == null ? 0 : Convert.ToDecimal(sheet1_entryData.Rows[r]["Due120"]);
                            helper.SetData(startRowTotal, 1, strSiteUseId);
                            helper.SetData(startRowTotal, 2, strSales);
                            helper.SetData(startRowTotal, 3, amount);
                            string strAgingBucket = "091-120";
                            helper.SetData(startRowTotal, 4, strAgingBucket);
                            if (listComments != null && listComments.Count > 0)
                            {
                                T_Customer_Comments findComments = listComments.Find(o => o.AgingBucket == strAgingBucket);
                                if (findComments != null)
                                {
                                    if (findComments.PTPAmount != null)
                                    {
                                        helper.SetData(startRowTotal, 5, findComments.PTPAmount);
                                    }
                                    if (findComments.PTPDATE != null)
                                    {
                                        helper.SetData(startRowTotal, 6, findComments.PTPDATE);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.OverdueReason))
                                    {
                                        helper.SetData(startRowTotal, 7, findComments.OverdueReason);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.Comments))
                                    {
                                        helper.SetData(startRowTotal, 8, findComments.Comments);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.CommentsFrom) && isShowCommentsFrom)
                                    {
                                        helper.SetData(startRowTotal, 9, findComments.CommentsFrom);
                                    }
                                }
                            }
                            startRowTotal++;
                        }
                        if (sheet1_entryData.Rows[r]["Due90"] != null && Convert.ToDecimal(sheet1_entryData.Rows[r]["Due90"]) != 0)
                        {
                            lb_flag = true;
                            helper.InsertRowWithStyle(startRowTotal + 1, 1, startRowTotal);
                            decimal amount = sheet1_entryData.Rows[r]["Due90"] == null ? 0 : Convert.ToDecimal(sheet1_entryData.Rows[r]["Due90"]);
                            helper.SetData(startRowTotal, 1, strSiteUseId);
                            helper.SetData(startRowTotal, 2, strSales);
                            helper.SetData(startRowTotal, 3, amount);
                            string strAgingBucket = "061-090";
                            helper.SetData(startRowTotal, 4, strAgingBucket);
                            if (listComments != null && listComments.Count > 0)
                            {
                                T_Customer_Comments findComments = listComments.Find(o => o.AgingBucket == strAgingBucket);
                                if (findComments != null)
                                {
                                    if (findComments.PTPAmount != null)
                                    {
                                        helper.SetData(startRowTotal, 5, findComments.PTPAmount);
                                    }
                                    if (findComments.PTPDATE != null)
                                    {
                                        helper.SetData(startRowTotal, 6, findComments.PTPDATE);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.OverdueReason))
                                    {
                                        helper.SetData(startRowTotal, 7, findComments.OverdueReason);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.Comments))
                                    {
                                        helper.SetData(startRowTotal, 8, findComments.Comments);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.CommentsFrom) && isShowCommentsFrom)
                                    {
                                        helper.SetData(startRowTotal, 9, findComments.CommentsFrom);
                                    }
                                }
                            }
                            startRowTotal++;
                        }
                        if (sheet1_entryData.Rows[r]["Due60"] != null && Convert.ToDecimal(sheet1_entryData.Rows[r]["Due60"]) != 0)
                        {
                            lb_flag = true;
                            helper.InsertRowWithStyle(startRowTotal + 1, 1, startRowTotal);
                            decimal amount = sheet1_entryData.Rows[r]["Due60"] == null ? 0 : Convert.ToDecimal(sheet1_entryData.Rows[r]["Due60"]);
                            helper.SetData(startRowTotal, 1, strSiteUseId);
                            helper.SetData(startRowTotal, 2, strSales);
                            helper.SetData(startRowTotal, 3, amount);
                            string strAgingBucket = "046-060";
                            helper.SetData(startRowTotal, 4, strAgingBucket);
                            if (listComments != null && listComments.Count > 0)
                            {
                                T_Customer_Comments findComments = listComments.Find(o => o.AgingBucket == strAgingBucket);
                                if (findComments != null)
                                {
                                    if (findComments.PTPAmount != null)
                                    {
                                        helper.SetData(startRowTotal, 5, findComments.PTPAmount);
                                    }
                                    if (findComments.PTPDATE != null)
                                    {
                                        helper.SetData(startRowTotal, 6, findComments.PTPDATE);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.OverdueReason))
                                    {
                                        helper.SetData(startRowTotal, 7, findComments.OverdueReason);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.Comments))
                                    {
                                        helper.SetData(startRowTotal, 8, findComments.Comments);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.CommentsFrom) && isShowCommentsFrom)
                                    {
                                        helper.SetData(startRowTotal, 9, findComments.CommentsFrom);
                                    }
                                }
                            }
                            startRowTotal++;
                        }
                        if (sheet1_entryData.Rows[r]["Due45"] != null && Convert.ToDecimal(sheet1_entryData.Rows[r]["Due45"]) != 0)
                        {
                            lb_flag = true;
                            helper.InsertRowWithStyle(startRowTotal + 1, 1, startRowTotal);
                            decimal amount = sheet1_entryData.Rows[r]["Due45"] == null ? 0 : Convert.ToDecimal(sheet1_entryData.Rows[r]["Due45"]);
                            helper.SetData(startRowTotal, 1, strSiteUseId);
                            helper.SetData(startRowTotal, 2, strSales);
                            helper.SetData(startRowTotal, 3, amount);
                            string strAgingBucket = "031-045";
                            helper.SetData(startRowTotal, 4, strAgingBucket);
                            if (listComments != null && listComments.Count > 0)
                            {
                                T_Customer_Comments findComments = listComments.Find(o => o.AgingBucket == strAgingBucket);
                                if (findComments != null)
                                {
                                    if (findComments.PTPAmount != null)
                                    {
                                        helper.SetData(startRowTotal, 5, findComments.PTPAmount);
                                    }
                                    if (findComments.PTPDATE != null)
                                    {
                                        helper.SetData(startRowTotal, 6, findComments.PTPDATE);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.OverdueReason))
                                    {
                                        helper.SetData(startRowTotal, 7, findComments.OverdueReason);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.Comments))
                                    {
                                        helper.SetData(startRowTotal, 8, findComments.Comments);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.CommentsFrom) && isShowCommentsFrom)
                                    {
                                        helper.SetData(startRowTotal, 9, findComments.CommentsFrom);
                                    }
                                }
                            }
                            startRowTotal++;
                        }
                        if (sheet1_entryData.Rows[r]["Due30"] != null && Convert.ToDecimal(sheet1_entryData.Rows[r]["Due30"]) != 0)
                        {
                            lb_flag = true;
                            helper.InsertRowWithStyle(startRowTotal + 1, 1, startRowTotal);
                            decimal amount = sheet1_entryData.Rows[r]["Due30"] == null ? 0 : Convert.ToDecimal(sheet1_entryData.Rows[r]["Due30"]);
                            helper.SetData(startRowTotal, 1, strSiteUseId);
                            helper.SetData(startRowTotal, 2, strSales);
                            helper.SetData(startRowTotal, 3, amount);
                            string strAgingBucket = "016-030";
                            helper.SetData(startRowTotal, 4, strAgingBucket);
                            if (listComments != null && listComments.Count > 0)
                            {
                                T_Customer_Comments findComments = listComments.Find(o => o.AgingBucket == strAgingBucket);
                                if (findComments != null)
                                {
                                    if (findComments.PTPAmount != null)
                                    {
                                        helper.SetData(startRowTotal, 5, findComments.PTPAmount);
                                    }
                                    if (findComments.PTPDATE != null)
                                    {
                                        helper.SetData(startRowTotal, 6, findComments.PTPDATE);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.OverdueReason))
                                    {
                                        helper.SetData(startRowTotal, 7, findComments.OverdueReason);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.Comments))
                                    {
                                        helper.SetData(startRowTotal, 8, findComments.Comments);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.CommentsFrom) && isShowCommentsFrom)
                                    {
                                        helper.SetData(startRowTotal, 9, findComments.CommentsFrom);
                                    }
                                }
                            }
                            startRowTotal++;
                        }
                        if (sheet1_entryData.Rows[r]["Due15"] != null && Convert.ToDecimal(sheet1_entryData.Rows[r]["Due15"]) != 0)
                        {
                            lb_flag = true;
                            helper.InsertRowWithStyle(startRowTotal + 1, 1, startRowTotal);
                            decimal amount = sheet1_entryData.Rows[r]["Due15"] == null ? 0 : Convert.ToDecimal(sheet1_entryData.Rows[r]["Due15"]);
                            helper.SetData(startRowTotal, 1, strSiteUseId);
                            helper.SetData(startRowTotal, 2, strSales);
                            helper.SetData(startRowTotal, 3, amount);
                            string strAgingBucket = "001-015";
                            helper.SetData(startRowTotal, 4, strAgingBucket);
                            if (listComments != null && listComments.Count > 0)
                            {
                                T_Customer_Comments findComments = listComments.Find(o => o.AgingBucket == strAgingBucket);
                                if (findComments != null)
                                {
                                    if (findComments.PTPAmount != null)
                                    {
                                        helper.SetData(startRowTotal, 5, findComments.PTPAmount);
                                    }
                                    if (findComments.PTPDATE != null)
                                    {
                                        helper.SetData(startRowTotal, 6, findComments.PTPDATE);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.OverdueReason))
                                    {
                                        helper.SetData(startRowTotal, 7, findComments.OverdueReason);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.Comments))
                                    {
                                        helper.SetData(startRowTotal, 8, findComments.Comments);
                                    }
                                    if (!string.IsNullOrEmpty(findComments.CommentsFrom) && isShowCommentsFrom)
                                    {
                                        helper.SetData(startRowTotal, 9, findComments.CommentsFrom);
                                    }
                                }
                            }
                            startRowTotal++;
                        }
                        //if (lb_flag && r != sheet1_entryData.Rows.Count - 1)
                        //{
                        //    helper.InsertRowWithStyle(startRowTotal, 1, startRowTotal - 1);
                        //    startRowTotal++;
                        //}
                    }
                    int startRowDisputeType = startRowTotal + 2;
                    IBaseDataService bdSer1 = SpringFactory.GetObjectImpl<IBaseDataService>("BaseDataService");
                    var config1 = bdSer1.GetSysTypeDetail("049");
                    //简体
                    int i = 0;
                    foreach (SysTypeDetail s in config1)
                    {
                        i++;
                        helper.SetData(startRowDisputeType + i, 1, s.DetailName);
                        helper.SetData(startRowDisputeType + i, 3, s.DetailValue);
                    }
                }
                SetSoaFileData(0, helper, sheet1_entryData, Sheet1_configheader, Sheet1_EntryFirstCol, Sheet1_lastCol, Sheet1_EntryDataRow, sheet0, language, false);
                if (language == "006" || language == "009")
                {
                    if (type == "001")
                    {
                        string strprePeriodEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM") + "-01").AddDays(-1).ToString("yyyy-MM-dd");
                        helper.SetData(4, 2, strprePeriodEndDate);
                    }
                }

                //Sheet2明细信息
                helper.ActiveSheet = 1;
                ISheet sheet1 = helper.Book.GetSheetAt(1);
                //ANZ,只有detail页
                if (language.Equals("011")) {
                    if(sheet2_entryData != null && sheet2_entryData.Rows.Count > 0) { 
                        helper.SetData(1, 4, sheet2_entryData.Rows[0]["CustomerName"].ToString());
                    }
                    helper.SetData(3, 5, "OPEN ITEMS AS OF " + DateTime.Now.ToString("MMM", new System.Globalization.CultureInfo("en-us")) + " " + DateTime.Now.ToString("dd,yyyy"));
                    if (sheet2_entryData.Rows.Count > 0)
                    {
                        string strLegalEntity = "";
                        List<string> listCurrency = new List<string>();
                        //写入银行信息
                        for (int b = 0; b < sheet2_entryData.Rows.Count; b++) {
                            if (string.IsNullOrEmpty(strLegalEntity)) {
                                strLegalEntity = sheet2_entryData.Rows[b]["LegalEntity"] == null ? "" : sheet2_entryData.Rows[b]["LegalEntity"].ToString();
                            }
                            string strCurrency = sheet2_entryData.Rows[b]["Currency"] == null ? "" : sheet2_entryData.Rows[b]["Currency"].ToString();
                            if (listCurrency.Find(o => o.ToUpper().Equals(strCurrency.ToUpper())) == null) {
                                listCurrency.Add(strCurrency);
                            }
                        }
                        if (listCurrency.Count > 0) {
                            if (listCurrency.Count > 1)
                            {
                                for(int k = 1; k < listCurrency.Count; k++) { 
                                    helper.InsertRowWithStyle(13, 1, 12);
                                }
                            }
                            int bkStartRow = 12;
                            foreach (string cur in listCurrency) { 
                                string strBankSql = string.Format("select * from T_Customer_CurrencyBank where legalentity ='{0}' and Currency = '{1}'", strLegalEntity, cur);
                                System.Data.DataTable dtBank = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, strBankSql, null);
                                if (dtBank != null) {
                                    if (strLegalEntity.Equals("308"))
                                    {
                                        if (dtBank != null && dtBank.Rows.Count > 0)
                                        {
                                            helper.SetData(bkStartRow, 4, dtBank.Rows[0]["EntityName"] == null ? "" : dtBank.Rows[0]["EntityName"].ToString());
                                            helper.SetData(bkStartRow, 5, dtBank.Rows[0]["Currency"] == null ? "" : dtBank.Rows[0]["Currency"].ToString());
                                            helper.SetData(bkStartRow, 6, dtBank.Rows[0]["BankName"] == null ? "" : dtBank.Rows[0]["BankName"].ToString());
                                            helper.SetData(bkStartRow, 7, dtBank.Rows[0]["BranchNumber"] == null ? "" : dtBank.Rows[0]["BranchNumber"].ToString());
                                            helper.SetData(bkStartRow, 8, dtBank.Rows[0]["BankAccount"] == null ? "" : dtBank.Rows[0]["BankAccount"].ToString());
                                            helper.SetData(bkStartRow, 9, dtBank.Rows[0]["Swift"] == null ? "" : dtBank.Rows[0]["Swift"].ToString());
                                        }
                                    }
                                    if (strLegalEntity.Equals("309"))
                                    {
                                        if(dtBank != null && dtBank.Rows.Count > 0) { 
                                            helper.SetData(bkStartRow, 4, dtBank.Rows[0]["EntityName"] == null ? "" : dtBank.Rows[0]["EntityName"].ToString());
                                            helper.SetData(bkStartRow, 5, dtBank.Rows[0]["Currency"] == null ? "" : dtBank.Rows[0]["Currency"].ToString());
                                            helper.SetData(bkStartRow, 6, dtBank.Rows[0]["BankName"] == null ? "" : dtBank.Rows[0]["BankName"].ToString());
                                            helper.SetData(bkStartRow, 7, dtBank.Rows[0]["BankNumber"] == null ? "" : dtBank.Rows[0]["BankNumber"].ToString());
                                            helper.SetData(bkStartRow, 8, dtBank.Rows[0]["BranchNumber"] == null ? "" : dtBank.Rows[0]["BranchNumber"].ToString());
                                            helper.SetData(bkStartRow, 9, dtBank.Rows[0]["BankAccount"] == null ? "" : dtBank.Rows[0]["BankAccount"].ToString());
                                            helper.SetData(bkStartRow, 10, dtBank.Rows[0]["Swift"] == null ? "" : dtBank.Rows[0]["Swift"].ToString());
                                        }
                                    }
                                }
                                bkStartRow++;
                            }
                        }
                        for (int j = 2; j < sheet2_entryData.Rows.Count; j++)
                        {
                            helper.InsertRowWithStyle(7, 1, 6);
                        }
                    }
                }
                SetSoaFileData(1, helper, sheet2_entryData, Sheet2_configheader, Sheet2_EntryFirstCol, Sheet2_lastCol, Sheet2_EntryDataRow, sheet1, language, true);
                if (language == "006" || language == "009")
                {
                    helper.SetData(1, 4, customerName);
                    if (type == "001")
                    {
                        string strprePeriodEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM") + "-01").AddDays(-1).ToString("yyyy-MM-dd");
                        DateTime dt_item = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM") + "-01").AddDays(-1);
                        string strSubTitle = "OPEN ITEMS AS OF " + dt_item.ToString("MMMM", new System.Globalization.CultureInfo("en-us")).Substring(0, 3) + " " + dt_item.ToString("dd") + "," + dt_item.ToString("yyyy");
                        helper.SetData(3, 5, strSubTitle);
                        helper.SetData(4, 2, strprePeriodEndDate);
                    }
                    else
                    {
                        DateTime dt_item = DateTime.Now;
                        string strprePeriodEndDate = dt_item.ToString("yyyy-MM-dd");
                        string strSubTitle = "OPEN ITEMS AS OF " + dt_item.ToString("MMMM", new System.Globalization.CultureInfo("en-us")).Substring(0, 3) + " " + dt_item.ToString("dd") + "," + dt_item.ToString("yyyy");
                        helper.SetData(3, 5, strSubTitle);
                        helper.SetData(4, 2, strprePeriodEndDate);
                    }
                }

                IBaseDataService bdSer = SpringFactory.GetObjectImpl<IBaseDataService>("BaseDataService");
                switch (language)
                {
                    case "001":
                        //var config1 = bdSer.GetSysTypeDetail("049");
                        ////简体
                        //int i = 0;
                        //foreach (SysTypeDetail s in config1)
                        //{
                        //    i++;
                        //    helper.SetData(Sheet1_EntryDataRow + i, 26, s.DetailName);
                        //    helper.SetData(Sheet1_EntryDataRow + i, 27, s.DetailValue);
                        //}
                        break;
                    case "002":
                        var config2 = bdSer.GetSysTypeDetail("050");
                        //繁体
                        int j = 0;
                        foreach (SysTypeDetail s in config2)
                        {
                            j++;
                            helper.SetData(Sheet1_EntryDataRow + j, 26, s.DetailName);
                            helper.SetData(Sheet1_EntryDataRow + j, 27, s.DetailValue);
                        }
                        break;
                    case "003":
                        var config3 = bdSer.GetSysTypeDetail("050");
                        //繁体
                        int k = 0;
                        foreach (SysTypeDetail s in config3)
                        {
                            k++;
                            helper.SetData(Sheet1_EntryDataRow + k, 19, s.DetailName);
                            helper.SetData(Sheet1_EntryDataRow + k, 20, s.DetailValue);
                        }
                        break;
                    case "004":
                        var config4 = bdSer.GetSysTypeDetail("049");
                        //简体
                        int l = 0;
                        foreach (SysTypeDetail s in config4)
                        {
                            l++;
                            helper.SetData(Sheet1_EntryDataRow + l, 26, s.DetailName);
                            helper.SetData(Sheet1_EntryDataRow + l, 27, s.DetailValue);
                        }
                        break;
                    case "005":
                        var config5 = bdSer.GetSysTypeDetail("054");
                        //韩文
                        int m = 0;
                        foreach (SysTypeDetail s in config5)
                        {
                            m++;
                            helper.SetData(Sheet1_EntryDataRow + m, 26, s.DetailName);
                            helper.SetData(Sheet1_EntryDataRow + m, 27, s.DetailValue);
                        }
                        break;
                }

                // 保存文件
                helper.ActiveSheet = 1;
                helper.Save(repfileName, true);

                if (filetype == "ALL" || filetype == "PDF")
                {
                    XLSConvertToPDF(stamp, repfileName, tmpPdfFile, sheet2_entryData.Rows.Count);
                }
            }
            catch (Exception ex)
            {
                string messge = string.Format("generateSOAExcelAttachment error: templateFileName:{0}", templateFileName);
                Helper.Log.Error("generateSOAExcelAttachment error", ex);
                Helper.Log.Error("SOA generation failed, please contact the administrator.\r\n" + ex.Message, ex);
                if (ex.StackTrace != null)
                {
                    Helper.Log.Error("SOA generation failed, please contact the administrator.\r\n" + ex.StackTrace.ToString(), ex);
                }
                throw new Exception("SOA generation failed, please contact the administrator");
            }
            finally {
                helper.Save(repfileName, true);
            }

        }

        public DateTime CurrentTime
        {
            get
            {
                return AppContext.Current.User.Now;
            }
        }

        private void generatePMTExcelAttachment(string templateFileName, IEnumerable<int> intids, string repfileName, string tmpPdfFile, out System.Data.DataTable[] mailEntryData, string type, string language, string stamp, string filetype)
        {
            System.Data.DataTable sheet1_entryData;
            System.Data.DataTable sheetN_entryData;

            mailEntryData = null;
            List<string> sqlsource = null;
            string mailSourceSql = "";

            //所有发票ID
            string idstrs = string.Join<int>(",", intids);

            // 打开Excel
            NpoiHelper helper = new NpoiHelper(templateFileName);
            helper.Save(repfileName, true);
            helper = new NpoiHelper(repfileName);

            //获得每1个Sheet信息
            var str_Sheet1 = helper.GetStringData(0, 0);
            Dictionary<string, string> tempConfig_Sheet1 = getTemplateConfigDict(str_Sheet1);
            int Sheet1_configheader = int.Parse(tempConfig_Sheet1["EntryTitleRow"]);
            int Sheet1_EntryFirstCol = int.Parse(tempConfig_Sheet1["EntryFirstCol"]);
            int Sheet1_EntryDataRow = int.Parse(tempConfig_Sheet1["EntryDataRow"]);
            int Sheet1_lastCol = int.Parse(tempConfig_Sheet1["LastColumn"]);
            string Sheet1_sourceSql = tempConfig_Sheet1["EntrySource"];
            if (Sheet1_sourceSql.Trim().StartsWith("SELECT"))
            {
                Sheet1_sourceSql = Sheet1_sourceSql.Replace("{ID}", idstrs);
                sheet1_entryData = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, Sheet1_sourceSql, null);
            }
            else
            {
                sheet1_entryData = null;
            }
            //获得第n个Sheet信息
            var str_SheetN = helper.GetStringData(1, 0);
            Dictionary<string, string> tempConfig_SheetN = getTemplateConfigDict(str_SheetN);
            int SheetN_configheader = int.Parse(tempConfig_SheetN["EntryTitleRow"]);
            int SheetN_EntryFirstCol = int.Parse(tempConfig_SheetN["EntryFirstCol"]);
            int SheetN_EntryDataRow = int.Parse(tempConfig_SheetN["EntryDataRow"]);
            int SheetN_lastCol = int.Parse(tempConfig_SheetN["LastColumn"]);
            string SheetN_sourceSql = tempConfig_SheetN["EntrySource"];
            if (SheetN_sourceSql.Trim().StartsWith("SELECT"))
            {
                SheetN_sourceSql = SheetN_sourceSql.Replace("{ID}", idstrs);
                sheetN_entryData = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, SheetN_sourceSql, null);
            }
            else
            {
                sheetN_entryData = null;
            }

            //Mail正文数据
            if (language == "006" || language == "008" || language == "007" || language == "0071" || language == "009")
            {
                sqlsource = Read();
                mailSourceSql = sqlsource[3];

                if (mailSourceSql.Trim().StartsWith("SELECT"))
                {
                    mailSourceSql = mailSourceSql.Replace("{ID}", idstrs);
                    mailEntryData = new System.Data.DataTable[1];
                    mailEntryData[0] = CommonRep.ExecuteDataTable(System.Data.CommandType.Text, mailSourceSql, null);

                }
                else
                {
                    mailEntryData = null;
                }
            }

            //清除第一个单元格的SQL配置信息
            helper.SetData(0, 0, "");
            helper.SetData(1, 0, "");

            //Sheet1汇兑信息
            helper.ActiveSheet = 0;
            ISheet sheet0 = helper.Book.GetSheetAt(0);
            SetSoaFileData(0, helper, sheet1_entryData, Sheet1_configheader, Sheet1_EntryFirstCol, Sheet1_lastCol, Sheet1_EntryDataRow, sheet0, language, false);

            IBaseDataService bdSer = SpringFactory.GetObjectImpl<IBaseDataService>("BaseDataService");
            switch (language)
            {
                case "001":
                    var config1 = bdSer.GetSysTypeDetail("049");
                    //简体
                    int i = 0;
                    foreach (SysTypeDetail s in config1)
                    {
                        i++;
                        helper.SetData(Sheet1_EntryDataRow + i, 26, s.DetailName);
                        helper.SetData(Sheet1_EntryDataRow + i, 27, s.DetailValue);
                    }
                    break;
                case "002":
                    var config2 = bdSer.GetSysTypeDetail("050");
                    //繁体
                    int j = 0;
                    foreach (SysTypeDetail s in config2)
                    {
                        j++;
                        helper.SetData(Sheet1_EntryDataRow + j, 26, s.DetailName);
                        helper.SetData(Sheet1_EntryDataRow + j, 27, s.DetailValue);
                    }
                    break;
                case "003":
                    var config3 = bdSer.GetSysTypeDetail("050");
                    //繁体
                    int k = 0;
                    foreach (SysTypeDetail s in config3)
                    {
                        k++;
                        helper.SetData(Sheet1_EntryDataRow + k, 26, s.DetailName);
                        helper.SetData(Sheet1_EntryDataRow + k, 27, s.DetailValue);
                    }
                    break;
                case "004":
                    var config4 = bdSer.GetSysTypeDetail("049");
                    //简体
                    int l = 0;
                    foreach (SysTypeDetail s in config4)
                    {
                        l++;
                        helper.SetData(Sheet1_EntryDataRow + l, 26, s.DetailName);
                        helper.SetData(Sheet1_EntryDataRow + l, 27, s.DetailValue);
                    }
                    break;
                case "005":
                    var config5 = bdSer.GetSysTypeDetail("054");
                    //简体
                    int m = 0;
                    foreach (SysTypeDetail s in config5)
                    {
                        m++;
                        helper.SetData(Sheet1_EntryDataRow + m, 26, s.DetailName);
                        helper.SetData(Sheet1_EntryDataRow + m, 27, s.DetailValue);
                    }
                    break;
            }

            //Sheet2明细信息
            if (sheetN_entryData.Rows.Count > 0) { 
                helper.ActiveSheet = 1;
                ISheet sheet1 = helper.Book.GetSheetAt(1);
                helper.Book.SetSheetName(1, sheetN_entryData.Rows[0]["SiteUseId"].ToString() + "-" + (sheetN_entryData.Rows[0]["InvoiceNo"].ToString().Replace("/","-")) + "(" + Math.Round(Convert.ToDecimal(sheetN_entryData.Rows[0]["InvoiceAmount"]),0) + ")");
                if (sheetN_entryData.Rows.Count > 1)
                {
                    for(int i = 1; i< sheetN_entryData.Rows.Count; i++)
                    {
                        sheet1.CopySheet(sheetN_entryData.Rows[i]["SiteUseId"].ToString() + "-" + (sheetN_entryData.Rows[i]["InvoiceNo"].ToString().Replace("/", "-")) + "(" + Math.Round(Convert.ToDecimal(sheetN_entryData.Rows[i]["InvoiceAmount"]),0) + ")",true);
                    }
                }
                for (int i = 0; i < sheetN_entryData.Rows.Count; i++)
                {
                    helper.ActiveSheet = i + 1;
                    ISheet sheetN = helper.Book.GetSheetAt(i+1);
                    System.Data.DataTable CurrentPMT = new System.Data.DataTable();
                    CurrentPMT = sheetN_entryData.Clone();
                    CurrentPMT.ImportRow(sheetN_entryData.Rows[i]);
                    SetSoaFileData(i + 1, helper, CurrentPMT, SheetN_configheader, SheetN_EntryFirstCol, SheetN_lastCol, SheetN_EntryDataRow, sheetN, language, false);
                }
            }
            // 保存文件
            helper.ActiveSheet = 0;
            helper.Save(repfileName, true);

        }

        private DataTable[] createCAPmtMailContentTable(List<CaBankStatementDto> bslist, string legalEntityName) {

            DataTable[] mailEntryData = new DataTable[1];
            mailEntryData[0] = new System.Data.DataTable("Table_MailBody");
            DataColumn dc1 = new DataColumn("No", System.Type.GetType("System.String"));
            DataColumn dc2 = new DataColumn("RV#", System.Type.GetType("System.String"));
            DataColumn dc3 = new DataColumn("Value Date", System.Type.GetType("System.String"));
            DataColumn dc4 = new DataColumn("Currency", System.Type.GetType("System.String"));
            DataColumn dc5 = new DataColumn("Net Amount", System.Type.GetType("System.String"));
            DataColumn dc6 = new DataColumn("Bank Charge", System.Type.GetType("System.String"));
            DataColumn dc7 = new DataColumn("Total Amount", System.Type.GetType("System.String"));
            DataColumn dc8 = new DataColumn("Accnt Number", System.Type.GetType("System.String"));
            DataColumn dc9 = new DataColumn("SiteUseId", System.Type.GetType("System.String"));
            DataColumn dc10 = new DataColumn("Customer Name", System.Type.GetType("System.String"));
            DataColumn dc11 = new DataColumn("Entity", System.Type.GetType("System.String"));
            mailEntryData[0].Columns.Add(dc1);
            mailEntryData[0].Columns.Add(dc2);
            mailEntryData[0].Columns.Add(dc3);
            mailEntryData[0].Columns.Add(dc4);
            mailEntryData[0].Columns.Add(dc5);
            mailEntryData[0].Columns.Add(dc6);
            mailEntryData[0].Columns.Add(dc7);
            mailEntryData[0].Columns.Add(dc8);
            mailEntryData[0].Columns.Add(dc9);
            mailEntryData[0].Columns.Add(dc10);
            mailEntryData[0].Columns.Add(dc11);
            int k = 0;
            foreach (CaBankStatementDto b in bslist)
            {
                DataRow dr = mailEntryData[0].NewRow();
                dr["No"] = k + 1;
                dr["RV#"] = b.TRANSACTION_NUMBER;
                dr["Value Date"] = Convert.ToDateTime(b.VALUE_DATE).ToString("dd-MMM-yyyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"));
                dr["Currency"] = b.CURRENCY;
                dr["Net Amount"] = b.TRANSACTION_AMOUNT == null ? 0.ToString("#,##0.00") : Convert.ToDecimal(b.TRANSACTION_AMOUNT).ToString("#,##0.00");
                dr["Bank Charge"] = b.BankChargeTo == null ? 0.ToString("#,##0.00") : Convert.ToDecimal(b.BankChargeTo).ToString("#,##0.00");
                dr["Total Amount"] = (b.TRANSACTION_AMOUNT == null ? 0.ToString("#,##0.00") : (Convert.ToDecimal(b.TRANSACTION_AMOUNT) + (b.BankChargeTo == null ? 0 : Convert.ToDecimal(b.BankChargeTo))).ToString("#,##0.00"));
                dr["Accnt Number"] = b.CUSTOMER_NUM;
                dr["SiteUseId"] = b.SiteUseId;
                dr["Customer Name"] = b.CUSTOMER_NAME;
                dr["Entity"] = legalEntityName;
                mailEntryData[0].Rows.Add(dr);
                k++;
            }
            return mailEntryData;
        }

        private DataTable[] createCAClearMailContentTable(List<CaBankStatementDto> bslist, string legalEntityName)
        {

            DataTable[] mailEntryData = new DataTable[1];
            mailEntryData[0] = new System.Data.DataTable("Table_MailBody");
            DataColumn dc1 = new DataColumn("No", System.Type.GetType("System.String"));
            DataColumn dc2 = new DataColumn("RV#", System.Type.GetType("System.String"));
            DataColumn dc3 = new DataColumn("Value Date", System.Type.GetType("System.String"));
            DataColumn dc4 = new DataColumn("Currency", System.Type.GetType("System.String"));
            DataColumn dc5 = new DataColumn("Net Amount", System.Type.GetType("System.String"));
            DataColumn dc6 = new DataColumn("Bank Charge", System.Type.GetType("System.String"));
            DataColumn dc7 = new DataColumn("Total Amount", System.Type.GetType("System.String"));
            DataColumn dc8 = new DataColumn("Accnt Number", System.Type.GetType("System.String"));
            DataColumn dc9 = new DataColumn("SiteUseId", System.Type.GetType("System.String"));
            DataColumn dc10 = new DataColumn("Customer Name", System.Type.GetType("System.String"));
            DataColumn dc11 = new DataColumn("Entity", System.Type.GetType("System.String"));
            DataColumn dc12 = new DataColumn("Category", System.Type.GetType("System.String"));
            mailEntryData[0].Columns.Add(dc1);
            mailEntryData[0].Columns.Add(dc2);
            mailEntryData[0].Columns.Add(dc3);
            mailEntryData[0].Columns.Add(dc4);
            mailEntryData[0].Columns.Add(dc5);
            mailEntryData[0].Columns.Add(dc6);
            mailEntryData[0].Columns.Add(dc7);
            mailEntryData[0].Columns.Add(dc8);
            mailEntryData[0].Columns.Add(dc9);
            mailEntryData[0].Columns.Add(dc10);
            mailEntryData[0].Columns.Add(dc11);
            mailEntryData[0].Columns.Add(dc12);
            int k = 0;
            foreach (CaBankStatementDto b in bslist)
            {
                DataRow dr = mailEntryData[0].NewRow();
                dr["No"] = k + 1;
                dr["RV#"] = b.TRANSACTION_NUMBER;
                dr["Value Date"] = Convert.ToDateTime(b.VALUE_DATE).ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"));
                dr["Currency"] = b.CURRENCY;
                dr["Net Amount"] = b.TRANSACTION_AMOUNT == null ? 0.ToString("#,##0.00") : Convert.ToDecimal(b.TRANSACTION_AMOUNT).ToString("#,##0.00");
                dr["Bank Charge"] = b.BankChargeTo == null ? 0.ToString("#,##0.00") : Convert.ToDecimal(b.BankChargeTo).ToString("#,##0.00");
                dr["Total Amount"] = (b.TRANSACTION_AMOUNT == null ? 0.ToString("#,##0.00") : (Convert.ToDecimal(b.TRANSACTION_AMOUNT) + (b.BankChargeTo == null ? 0 : Convert.ToDecimal(b.BankChargeTo))).ToString("#,##0.00"));
                dr["Accnt Number"] = b.CUSTOMER_NUM;
                dr["SiteUseId"] = b.SiteUseId;
                dr["Customer Name"] = b.CUSTOMER_NAME;
                dr["Entity"] = legalEntityName;
                dr["Category"] = b.reconType == null ? "" : b.reconType;
                mailEntryData[0].Rows.Add(dr);
                k++;
            }
            return mailEntryData;
        }

        private void generateCAPMTExcelAttachmentCN(string templateFileName, List<CaBankStatementDto> bslist, IEnumerable<int> intids, string strLegalEntity, ref decimal decCURRENT_AMOUNT, string repfileName, out System.Data.DataTable[] mailEntryData)
        {
            var legalEntityName = (from site in CommonRep.GetQueryable<Sites>()
                                   where site.LegalEntity == strLegalEntity
                                   select site.LegalEntity + " " + site.SiteNameSys
                               ).FirstOrDefault();

            mailEntryData = createCAPmtMailContentTable(bslist, legalEntityName);

            List<InvoiceAging> invoicelist = new List<InvoiceAging>();
            invoicelist = (from inv in CommonRep.GetQueryable<InvoiceAging>()
                           where intids.Contains(inv.Id)
                           select inv).OrderBy(o=>o.SiteUseId).ThenBy(o=>o.DueDate).ThenBy(o=>o.InvoiceNum).ToList();
            // 打开Excel
            NpoiHelper helper = new NpoiHelper(templateFileName);
            helper.Save(repfileName, true);
            helper = new NpoiHelper(repfileName);

            string strCustomerName = "";
            int bsCount = bslist.Count;
            if (bsCount > 2)
            {
                int iRow_StartBS = 9;
                for (int i = 3; i <= bsCount; i++) { 
                    helper.InsertRowWithStyle(iRow_StartBS, 1, iRow_StartBS - 1);
                }
            }
            //if (bslist.Count > 0) {
            //    strCustomerName = bslist[0].CUSTOMER_NAME;
            //    ISheet sheet0 = helper.Book.GetSheetAt(0);
            //    int i = 0;
            //    foreach (CaBankStatementDto b in bslist)
            //    {
            //        if (i > 0)
            //        {
            //            sheet0.CopySheet(b.TRANSACTION_NUMBER, true);
            //        }
            //        //decCURRENT_AMOUNT += b.TRANSACTION_AMOUNT == null ? 0 : Convert.ToDecimal(b.TRANSACTION_AMOUNT);
            //        i++;
            //    }
            //    helper.Book.SetSheetName(0, bslist[0].TRANSACTION_NUMBER);
            //    ISheet sheet1 = helper.Book.GetSheetAt(1);
            //    if (bslist.Count > 1)
            //    {
            //        sheet1.CopySheet("Detail1", true);
            //        helper.Book.RemoveSheetAt(1);
            //        helper.Book.SetSheetName(bslist.Count + 1, "Detail");
            //    }
            //}
            int isheet = 0;
            helper.ActiveSheet = isheet;
            //Customer信息
            helper.SetData(3, 1, bslist[0].CUSTOMER_NUM);
            helper.SetData(3, 2, bslist[0].CUSTOMER_NAME);
            helper.SetData(3, 7, legalEntityName);
            string strSiteUseId = "";
            int startRow = 8;
            int ii = 0;
            foreach (CaBankStatementDto b in bslist)
            {
                ii++;
                //BankStatement信息
                helper.SetData(startRow, 0, ii);
                helper.SetData(startRow, 1, b.TRANSACTION_NUMBER);
                helper.SetData(startRow, 2, b.VALUE_DATE);
                helper.SetData(startRow, 3, b.CURRENCY);
                helper.SetData(startRow, 4, b.TRANSACTION_AMOUNT);
                helper.SetData(startRow, 7, b.Description);
                if (!string.IsNullOrEmpty(strSiteUseId))
                {
                    strSiteUseId += "/" + b.SiteUseId;
                }
                else {
                    strSiteUseId = b.SiteUseId;
                }
                startRow++;
            }
            helper.SetData(4, 1, strSiteUseId);

            //写入详细信息
            helper.ActiveSheet = 1;
            int iRow_Start = 6;
            int iRow = 0;
            helper.SetData(4, 2, DateTime.Now.Date.ToShortDateString());

            foreach (InvoiceAging aging in invoicelist)
            {
                if (iRow > 2)
                {
                    helper.InsertRowWithStyle(iRow_Start, 1, iRow_Start - 1);
                }
                helper.SetData(iRow_Start, 1, iRow + 1);// #`````
                helper.SetData(iRow_Start, 2, aging.PtpDate);// 承诺付款日
                helper.SetData(iRow_Start, 3, aging.OverdueReason); // 逾期原因````
                helper.SetData(iRow_Start, 4, aging.Comments); // 备注····
                helper.SetData(iRow_Start, 5, strCustomerName); // Customer Name····
                helper.SetData(iRow_Start, 6, aging.CustomerNum); // Accnt Number····
                helper.SetData(iRow_Start, 7, aging.SiteUseId); // Site Use Id····
                helper.SetData(iRow_Start, 8, aging.Class); // Class·····
                helper.SetData(iRow_Start, 9, aging.InvoiceNum);// Trx Num····
                helper.SetData(iRow_Start, 10, aging.InvoiceDate);   // Trx Date····
                helper.SetData(iRow_Start, 11, aging.DueDate);  // Due Date····
                helper.SetData(iRow_Start, 12, aging.FuncCurrCode);// Func Curr Code·····
                helper.SetData(iRow_Start, 13, aging.Currency); // Inv Curr Code`````
                helper.SetData(iRow_Start, 14, aging.DaysLateSys); // Due Days`````
                helper.SetData(iRow_Start, 15, aging.BalanceAmt); // Amt Remaining`````
                helper.SetData(iRow_Start, 16, aging.AgingBucket); // Aging Bucket`````
                helper.SetData(iRow_Start, 17, aging.CreditTremDescription); // Payment Term Desc
                helper.SetData(iRow_Start, 18, aging.Ebname); // Ebname`````
                helper.SetData(iRow_Start, 19, aging.LsrNameHist); // LSR
                helper.SetData(iRow_Start, 20, aging.FsrNameHist);// Sales````
                helper.SetData(iRow_Start, 21, aging.LegalEntity);   // Org Id
                helper.SetData(iRow_Start, 22, aging.Cmpinv);  // Cmpinv`````
                helper.SetData(iRow_Start, 23, aging.SoNum);// Sales Order·····
                helper.SetData(iRow_Start, 24, aging.PoNum);// Cpo
                iRow++;
                iRow_Start++;
            }

            // 保存文件
            helper.ActiveSheet = 0;
            helper.Save(repfileName, true);

        }
        private void generateCAPMTExcelAttachmentCNClear(string templateFileName, List<CaBankStatementDto> bslist, IEnumerable<int> intids, string strLegalEntity, decimal decCURRENT_AMOUNT, string repfileName, out System.Data.DataTable[] mailEntryData)
        {

            var legalEntityName = (from site in CommonRep.GetQueryable<Sites>()
                                   where site.LegalEntity == strLegalEntity
                                   select site.LegalEntity + " " + site.SiteNameSys
                               ).FirstOrDefault();

            mailEntryData = createCAClearMailContentTable(bslist, legalEntityName);

            //Helper.Log.Info("************************** createCAClearMailContentTable finish ****************");

            //Helper.Log.Info("********************************** templateFileName:" + templateFileName);
            // 打开Excel
            NpoiHelper helper = new NpoiHelper(templateFileName);
            helper.Save(repfileName, true);
            helper = new NpoiHelper(repfileName);

            int isheet = 0;
            List<CaBankStatementDto> listTotalBs = new List<CaBankStatementDto>();
            List<CaReconDetailDto> listInvoiceDetail = new List<CaReconDetailDto>();
            foreach (CaBankStatementDto bs in bslist)
            {
                string strSqlList = string.Format(@"select T_CA_ReconBS.Amount as ReconBS_Amount, T_CA_BankStatement.* from T_CA_ReconBS
													join T_CA_BankStatement on T_CA_ReconBS.BANK_STATEMENT_ID = T_CA_BankStatement.id
													 where ReconId in (
													select top 1 T_CA_Recon.ID from T_CA_ReconBS
													join T_CA_Recon on T_CA_ReconBS.ReconId = T_CA_Recon.id
													where T_CA_ReconBS.BANK_STATEMENT_ID = '{0}'
													order by T_CA_Recon.CREATE_DATE desc
													)", bs.ID);
                List<CaBankStatementDto> list = SqlHelper.GetList<CaBankStatementDto>(SqlHelper.ExcuteTable(strSqlList, System.Data.CommandType.Text, null));
                listTotalBs.AddRange(list);

                string strSqlRecon = string.Format(@"with  card as(
                                                    SELECT * FROM T_CA_ReconDetail
	                                                    WHERE ReconId = '{0}'
                                                    )
                                                    select card.*,iv.BALANCE_AMT from card
                                                    left join 
                                                    (
	                                                    SELECT rd.id as rdid,Max(tia.id) as invid FROM 
		                                                    (select * from card) as rd
	                                                    left join t_invoice_aging tia on rd.SiteUseId=tia.SiteUseId and rd.InvoiceNum=tia.INVOICE_NUM
	                                                    group by rd.id
                                                    ) as dd on card.id=dd.rdid
                                                    left join t_invoice_aging iv on iv.ID=dd.invid", bs.reconId);
                List<CaReconDetailDto> invoicelist = SqlHelper.GetList<CaReconDetailDto>(SqlHelper.ExcuteTable(strSqlRecon, System.Data.CommandType.Text, null));
                foreach (CaReconDetailDto detail in invoicelist) {
                    CaReconDetailDto find = listInvoiceDetail.Find(o => o.SiteUseId == detail.SiteUseId && o.InvoiceNum == detail.InvoiceNum);
                    if (find == null)
                    {
                        listInvoiceDetail.Add(detail);
                    }
                    else
                    {
                        find.Amount += detail.Amount;
                    }
                }
            }


            //Customer信息
            helper.SetData(3, 1, bslist[0].CUSTOMER_NUM);
            helper.SetData(3, 2, bslist[0].CUSTOMER_NAME);
            helper.SetData(3, 7, legalEntityName);

            int bsCount = bslist.Count;
            if (bsCount > 2)
            {
                int iRow_StartBS = 9;
                for (int i = 3; i <= bsCount; i++)
                {
                    helper.InsertRowWithStyle(iRow_StartBS, 1, iRow_StartBS - 1);
                }
            }

            //string strSiteUseId = "";
            int startRow = 7;
            int ii = 0;
            decimal ldec_totalBalanceAmt = 0;
            decimal ldec_totalamount = 0;
            foreach (CaBankStatementDto b in bslist)
            {
                ii++;
                //BankStatement信息
                helper.SetData(startRow, 0, ii);
                helper.SetData(startRow, 1, b.TRANSACTION_NUMBER);
                helper.SetData(startRow, 2, b.VALUE_DATE);
                helper.SetData(startRow, 3, b.CURRENCY);
                helper.SetData(startRow, 4, b.TRANSACTION_AMOUNT);
                helper.SetData(startRow, 5, b.BankChargeTo);
                helper.SetData(startRow, 6, b.ReconBS_Amount);
                decimal ldec_balanceAmt = b.TRANSACTION_AMOUNT == null ? 0 : Convert.ToDecimal(b.TRANSACTION_AMOUNT);
                decimal ldec_amount = b.ReconBS_Amount == null ? 0 : Convert.ToDecimal(b.ReconBS_Amount);
                ldec_totalBalanceAmt += ldec_balanceAmt;
                ldec_totalamount += ldec_amount;
                decimal ldec_yue = (ldec_balanceAmt - ldec_amount) < 0 ? 0 : (ldec_balanceAmt - ldec_amount);
                helper.SetData(startRow, 7, ldec_yue);
                helper.SetData(startRow, 8, b.Description);
                //if (!string.IsNullOrEmpty(strSiteUseId))
                //{
                //    strSiteUseId += "/" + b.SiteUseId;
                //}
                //else
                //{
                //    strSiteUseId = b.SiteUseId;
                //}
                startRow++;
            }
            if (bsCount < 2)
            {
                startRow++;
            }
            helper.SetData(startRow, 4, ldec_totalBalanceAmt);
            helper.SetData(startRow, 6, ldec_totalamount);
            helper.SetData(startRow, 7, ldec_totalBalanceAmt - ldec_totalamount < 0 ? 0 : ldec_totalBalanceAmt - ldec_totalamount);
            //helper.SetData(4, 1, strSiteUseId);

            if (bsCount < 2) { bsCount = 2; }
            int iRow_Start = 13 + bsCount - 2;
            int iRow = 0;
            foreach (CaReconDetailDto aging in listInvoiceDetail)
            {
                if (iRow > 2)
                {
                    helper.InsertRowWithStyle(iRow_Start, 1, iRow_Start - 1);
                }
                helper.SetData(iRow_Start, 0, iRow + 1);
                helper.SetData(iRow_Start, 1, aging.SiteUseId);
                helper.SetData(iRow_Start, 2, aging.InvoiceNum);
                helper.SetData(iRow_Start, 3, aging.DueDate);
                helper.SetData(iRow_Start, 4, aging.Currency);
                helper.SetData(iRow_Start, 5, aging.BALANCE_AMT);
                helper.SetData(iRow_Start, 6, aging.Amount);
                decimal ldec_balanceAmt = aging.BALANCE_AMT == null ? 0 : Convert.ToDecimal(aging.BALANCE_AMT);
                decimal ldec_amount = aging.Amount == null ? 0 : Convert.ToDecimal(aging.Amount);
                decimal ldec_yue = (ldec_balanceAmt - ldec_amount) < 0 ? 0 : (ldec_balanceAmt - ldec_amount);
                helper.SetData(iRow_Start, 7, ldec_yue);
                iRow++;
                iRow_Start++;
            }

            ////TODO:BankStatement信息多行
            //int headRow_Start = 7;
            //int headRow = 0;
            //foreach (var item in list)
            //{
            //    if (headRow > 2)
            //    {
            //        helper.InsertRowWithStyle(headRow_Start, 1, headRow_Start - 1);
            //    }
            //    helper.SetData(headRow_Start, 1, item.TRANSACTION_NUMBER);
            //    helper.SetData(headRow_Start, 2, item.VALUE_DATE);
            //    helper.SetData(headRow_Start, 3, item.CURRENCY);
            //    helper.SetData(headRow_Start, 4, item.CURRENT_AMOUNT == null ? item.ReconBS_Amount : Convert.ToDecimal(item.CURRENT_AMOUNT) + item.ReconBS_Amount);
            //    helper.SetData(headRow_Start, 5, item.ReconBS_Amount);
            //    helper.SetData(headRow_Start, 7, item.Description);
            //    ++headRow;
            //    ++headRow_Start;
            //}

            ////TODO:写入详细信息
            //int iRow_Start = headRow_Start + 5;
            //int iRow = 0;

            //foreach (var aging in invoicelist)
            //{
            //    if (iRow > 2)
            //    {
            //        helper.InsertRowWithStyle(iRow_Start, 1, iRow_Start - 1);
            //    }
            //    helper.SetData(iRow_Start, 0, iRow + 1);//
            //    helper.SetData(iRow_Start, 1, aging.SiteUseId);// 
            //    helper.SetData(iRow_Start, 2, aging.InvoiceNum); // InvNo./OrderNo.
            //    helper.SetData(iRow_Start, 3, aging.DueDate);//
            //    helper.SetData(iRow_Start, 4, aging.Currency);// 
            //    helper.SetData(iRow_Start, 5, aging.BALANCE_AMT); // 
            //    helper.SetData(iRow_Start, 6, aging.Amount);
            //    iRow++;
            //    iRow_Start++;
            //}

            //Helper.Log.Info("********************** repfileName:" + repfileName);
            // 保存文件
            helper.ActiveSheet = 0;
            helper.Save(repfileName, true);

        }

        private void generateCAPMTExcelAttachment(string templateFileName, IEnumerable<int> intids, string strId, string repfileName, out System.Data.DataTable[] mailEntryData, string strLegalEntity, string strCustomerNum, string strCustomerName, string strTRANSACTION_NUMBER, DateTime? strVALUE_DATE, string strCURRENCY, decimal decCURRENT_AMOUNT, string strDescription)
        {
            System.Data.DataTable sheet1_entryData;
            System.Data.DataTable sheetN_entryData;

            mailEntryData = null;

            List<InvoiceAging> invoicelist = new List<InvoiceAging>();
            invoicelist = (from inv in CommonRep.GetQueryable<InvoiceAging>()
                           where intids.Contains(inv.Id)
                           select inv).OrderBy(o => o.SiteUseId).ThenBy(o => o.DueDate).ThenBy(o => o.InvoiceNum).ToList();
            // 打开Excel
            NpoiHelper helper = new NpoiHelper(templateFileName);
            helper.Save(repfileName, true);
            helper = new NpoiHelper(repfileName);

            //Customer信息
            helper.SetData(3, 1, strCustomerNum);
            helper.SetData(3, 2, strCustomerName);
            helper.SetData(3, 7, strLegalEntity);
            //BankStatement信息
            helper.SetData(7, 1, strTRANSACTION_NUMBER);
            helper.SetData(7, 2, strVALUE_DATE);
            helper.SetData(7, 3, strCURRENCY);
            helper.SetData(7, 4, decCURRENT_AMOUNT);
            helper.SetData(7, 7, strDescription);

            //判断客户是否有外币币制，如果有，则设置外币币制和金额列
            Boolean lb_Local = false;
            string strLocalCurrency = "";
            string sql = @"select top 1 * from T_CA_CustomerAttribute WHERE LegalEntity = '" + strLegalEntity + "' AND Customer_Num = '" + strCustomerNum + "'";
            List<CaCustomerAttributeDto> listCustomerAttribute = CommonRep.ExecuteSqlQuery<CaCustomerAttributeDto>(sql).ToList();
            if (listCustomerAttribute != null && listCustomerAttribute.Count > 0)
            {
                if (strCURRENCY == listCustomerAttribute[0].Local_Currency)
                {
                    lb_Local = true;
                    strLocalCurrency = listCustomerAttribute[0].Local_Currency;
                }
            }

            int iRow_Start = 13;
            int iRow = 0;
            foreach (InvoiceAging aging in invoicelist)
            {
                if (iRow > 2)
                {
                    helper.InsertRowWithStyle(iRow_Start, 1, iRow_Start - 1);
                }
                helper.SetData(iRow_Start, 0, iRow + 1);
                helper.SetData(iRow_Start, 1, aging.SiteUseId);
                helper.SetData(iRow_Start, 2, aging.InvoiceNum);
                helper.SetData(iRow_Start, 3, aging.DueDate);
                helper.SetData(iRow_Start, 4, aging.Currency);
                helper.SetData(iRow_Start, 5, aging.BalanceAmt);
                if (lb_Local)
                {
                    helper.SetData(iRow_Start, 7, strLocalCurrency);
                    helper.SetData(iRow_Start, 8, aging.RemainingAmtTran);
                }
                iRow++;
                iRow_Start++;
            }
            if (iRow_Start < 16) { iRow_Start = 16; }
            helper.SetFormula(11, 5, "SUM(F14:F" + (iRow_Start).ToString() + ")");
            helper.SetFormula(11, 6, "SUM(G14:G" + (iRow_Start).ToString() + ")");
            helper.SetFormula(11, 8, "SUM(I14:I" + (iRow_Start).ToString() + ")");

            // 保存文件
            helper.ActiveSheet = 0;
            helper.Save(repfileName, true);

        }

        private void SetSoaFileData(int sheetId, NpoiHelper helper, System.Data.DataTable entryData, int configheader, int EntryFirstCol, int lastCol, int EntryDataRow, ISheet sheet, string language, bool isDetail) {
            
            object cellValueObj;
            string replaceFrom;
            // 循环 xxxx 体部分列
            var row = EntryDataRow;

            helper.SetData(4,2,DateTime.Now.ToString("yyyy-MM-dd"));
            if (entryData == null || entryData.Rows.Count == 0)
            {
                helper.TryClearContents(EntryDataRow, EntryDataRow);
            }
            else
            {
                //这段处理Excel样式特别慢，整个SOA性能瓶颈
                //上面屏掉的代码的样式有的无效，下面屏掉的代码的性能跟这个差不多
                List<string> columnNames = null;
                List<ICellStyle> cellStyles = helper.GetCellStyles(row, out columnNames);
                List<ICellStyle> cellHighLightStyles = new List<ICellStyle>();
                List<ICellStyle> cellBoldStyles = new List<ICellStyle>();
                foreach (var cellStyle in cellStyles)
                {
                    ICellStyle higtLightStyle = helper.Book.CreateCellStyle();
                    higtLightStyle.CloneStyleFrom(cellStyle);
                    IFont font = helper.Book.CreateFont(); 
                    font.FontName = "Arial";
                    font.FontHeight = 9;
                    font.Color = NPOI.HSSF.Util.HSSFColor.Red.Index;
                    higtLightStyle.SetFont(font);
                    higtLightStyle.FillPattern = FillPattern.SolidForeground;
                    cellHighLightStyles.Add(higtLightStyle);
                }
                foreach (var cellStyle in cellStyles)
                {
                    ICellStyle boldStyle = helper.Book.CreateCellStyle();
                    boldStyle.CloneStyleFrom(cellStyle);
                    IFont font = helper.Book.CreateFont();
                    font.FontName = "Arial";
                    font.FontHeight = 9;
                    font.Boldweight = (short)FontBoldWeight.Bold;
                    boldStyle.SetFont(font);
                    boldStyle.FillPattern = FillPattern.SolidForeground;
                    cellBoldStyles.Add(boldStyle);
                }

                if (isDetail && (language == "006" || language == "009"))
                {
                    entryData.DefaultView.Sort = " SiteUseId ASC, Currency DESC, DueDate ASC, InvoiceNo ASC ";
                    entryData = entryData.DefaultView.ToTable();
                    string strPreCurrency = "";
                    string strPreSiteUseId = "";
                    int rowNum = 1;
                    int factRow = row;
                    decimal ld_SumByCurrency = 0;
                    for (int r = 0; r < entryData.Rows.Count; r++)
                    {
                        if ((strPreCurrency != "" && strPreCurrency != entryData.Rows[r]["Currency"].ToString()) ||
                            (strPreSiteUseId != "" && strPreSiteUseId != entryData.Rows[r]["SiteUseId"].ToString())
                            )
                        {
                            //写合计行
                            System.Data.DataTable sumTable = new System.Data.DataTable();
                            sumTable = entryData.Clone();
                            System.Data.DataRow sumRow = sumTable.NewRow();
                            sumRow["InvoiceAmount"] = ld_SumByCurrency;
                            helper.SetData(factRow + r, EntryFirstCol, sumRow, cellBoldStyles, columnNames);

                            //增加新的币制Header
                            helper.CopyRows(row - 1, row - 1, factRow + r + 1);
                            helper.SetData(factRow + r + 2, 9, "Balance Amount " + entryData.Rows[r]["Currency"].ToString());
                            helper.SetData(factRow + r + 2, 1, entryData.Rows[r]["SiteUseId"].ToString());
                            factRow = factRow + 3;
                            rowNum = 1;
                            ld_SumByCurrency = 0;
                        }
                        ld_SumByCurrency += Convert.ToDecimal(entryData.Rows[r]["InvoiceAmount"]);
                        //设置币种
                        if (strPreCurrency == "")
                        {
                            helper.SetData(factRow - 1, 9, "Balance Amount " + entryData.Rows[r]["Currency"].ToString());
                        }
                        if (strPreSiteUseId == "")
                        {
                            helper.SetData(factRow - 1, 1, entryData.Rows[r]["SiteUseId"].ToString());
                        }
                        entryData.Rows[r]["RowNumber"] = rowNum;
                        if (entryData.Columns.Contains("Class") && entryData.Rows[r]["Class"] != null && entryData.Rows[r]["Class"].ToString().ToUpper() == "PMT")
                        {
                            helper.SetData(factRow + r, EntryFirstCol, entryData.Rows[r], cellHighLightStyles, columnNames);
                        }
                        else
                        {
                            helper.SetData(factRow + r, EntryFirstCol, entryData.Rows[r], cellStyles, columnNames);
                        }
                        strPreCurrency = entryData.Rows[r]["Currency"].ToString();
                        strPreSiteUseId = entryData.Rows[r]["SiteUseId"].ToString();
                        rowNum = rowNum + 1;
                        //最后一行时
                        if (r == entryData.Rows.Count - 1)
                        {
                            //写合计行
                            System.Data.DataTable sumTable = new System.Data.DataTable();
                            sumTable = entryData.Clone();
                            System.Data.DataRow sumRow = sumTable.NewRow();
                            sumRow["InvoiceAmount"] = ld_SumByCurrency;
                            helper.SetData(factRow + r + 1, EntryFirstCol, sumRow, cellBoldStyles, columnNames);
                        }
                    }
                }
                else if (isDetail && language == "008") {
                    entryData.DefaultView.Sort = " CustomerNum ASC, SiteUseId ASC, Currency DESC, DueDate ASC, InvoiceNo ASC ";
                    entryData = entryData.DefaultView.ToTable();
                    //先CopySheet
                    string strCustomerNum = "";
                    string strCustomerNumPre = "";
                    ISheet sheet1 = helper.Book.GetSheetAt(1);
                    
                    int customerCount = 1;
                    for (int i = 0; i < entryData.Rows.Count; i++)
                    {
                        strCustomerNum = entryData.Rows[i]["CustomerNum"].ToString();
                        if (i == 0)
                        {
                            helper.Book.SetSheetName(1, strCustomerNum);
                        }
                        if (strCustomerNumPre != "" && strCustomerNum != strCustomerNumPre) {
                            sheet1.CopySheet(strCustomerNum, true);
                            customerCount++;
                        } 
                        strCustomerNumPre = entryData.Rows[i]["CustomerNum"].ToString();
                    }
                    System.Data.DataTable[] entryDataByCustomer = new System.Data.DataTable[customerCount];
                    System.Data.DataTable entryDataCur = new System.Data.DataTable();
                    entryDataCur = entryData.Clone();
                    int j = 0;
                    strCustomerNum = "";
                    strCustomerNumPre = "";
                    for (int i = 0; i < entryData.Rows.Count; i++)
                    {
                        strCustomerNum = entryData.Rows[i]["CustomerNum"].ToString();
                        if (strCustomerNumPre != "" && strCustomerNum != strCustomerNumPre)
                        {
                            entryDataByCustomer[j] = entryData.Clone();
                            for (int k = 0; k < entryDataCur.Rows.Count; k++)
                            {
                                entryDataByCustomer[j].ImportRow(entryDataCur.Rows[k]);
                            }
                            entryDataCur.Clear();
                            j++;
                        }
                        entryDataCur.ImportRow(entryData.Rows[i]);
                        strCustomerNumPre = entryData.Rows[i]["CustomerNum"].ToString();
                        if (i == entryData.Rows.Count - 1)
                        {
                            entryDataByCustomer[j] = entryData.Clone();
                            for (int k = 0; k < entryDataCur.Rows.Count; k++)
                            {
                                entryDataByCustomer[j].ImportRow(entryDataCur.Rows[k]);
                            }
                        }
                    }

                    int activeSheet = 1;
                    foreach (System.Data.DataTable dtCur in entryDataByCustomer)
                    {
                        helper.ActiveSheet = activeSheet;
                        string strPreCurrency = "";
                        string strPreSiteUseId = "";
                        int rowNum = 1;
                        int factRow = row;
                        decimal ld_SumByCurrency = 0;

                        string strCustomerNumCur = dtCur.Rows[0]["CustomerNum"].ToString();
                        string CustomerNameNoSite = (from c in CommonRep.GetQueryable<Customer>()
                                                     where c.CustomerNum == strCustomerNumCur
                                                     select c.CustomerName).FirstOrDefault();

                        helper.SetData(1, 4, CustomerNameNoSite);
                        string strSubTitle = "OPEN ITEMS AS OF " + DateTime.Now.ToString("MMMM", new System.Globalization.CultureInfo("en-us")).Substring(0, 3) + " " + DateTime.Now.ToString("dd") + "," + DateTime.Now.ToString("yyyy");
                        helper.SetData(3, 5, strSubTitle);

                        dtCur.DefaultView.Sort = " SiteUseId ASC, Currency DESC, DueDate ASC, InvoiceNo ASC ";
                        System.Data.DataTable dt = dtCur.DefaultView.ToTable();
                        for (int r = 0; r < dt.Rows.Count; r++)
                        {
                            if ((strPreCurrency != "" && strPreCurrency != dt.Rows[r]["Currency"].ToString()) ||
                                (strPreSiteUseId != "" && strPreSiteUseId != dt.Rows[r]["SiteUseId"].ToString())
                                )
                            {
                                //写合计行
                                System.Data.DataTable sumTable = new System.Data.DataTable();
                                sumTable = dt.Clone();
                                System.Data.DataRow sumRow = sumTable.NewRow();
                                sumRow["InvoiceAmount"] = ld_SumByCurrency;
                                helper.SetData(factRow + r, EntryFirstCol, sumRow, cellBoldStyles, columnNames);

                                //增加新的币制Header
                                helper.CopyRows(row - 1, row - 1, factRow + r + 1);
                                helper.SetData(factRow + r + 2, 9, "Balance Amount " + dt.Rows[r]["Currency"].ToString());
                                helper.SetData(factRow + r + 2, 1, dt.Rows[r]["SiteUseId"].ToString());
                                factRow = factRow + 3;
                                rowNum = 1;
                                ld_SumByCurrency = 0;
                            }
                            ld_SumByCurrency += Convert.ToDecimal(dt.Rows[r]["InvoiceAmount"]);
                            //设置币种
                            if (strPreCurrency == "")
                            {
                                helper.SetData(factRow - 1, 9, "Balance Amount " + dt.Rows[r]["Currency"].ToString());
                            }
                            if (strPreSiteUseId == "")
                            {
                                helper.SetData(factRow - 1, 1, dt.Rows[r]["SiteUseId"].ToString());
                            }
                            dt.Rows[r]["RowNumber"] = rowNum;
                            if (dt.Columns.Contains("Class") && dt.Rows[r]["Class"] != null && dt.Rows[r]["Class"].ToString().ToUpper() == "PMT")
                            {
                                helper.SetData(factRow + r, EntryFirstCol, dt.Rows[r], cellHighLightStyles, columnNames);
                            }
                            else
                            {
                                helper.SetData(factRow + r, EntryFirstCol, dt.Rows[r], cellStyles, columnNames);
                            }
                            strPreCurrency = dt.Rows[r]["Currency"].ToString();
                            strPreSiteUseId = dt.Rows[r]["SiteUseId"].ToString();
                            rowNum = rowNum + 1;
                            //最后一行时
                            if (r == dt.Rows.Count - 1)
                            {
                                //写合计行
                                System.Data.DataTable sumTable = new System.Data.DataTable();
                                sumTable = dt.Clone();
                                System.Data.DataRow sumRow = sumTable.NewRow();
                                sumRow["InvoiceAmount"] = ld_SumByCurrency;
                                helper.SetData(factRow + r + 1, EntryFirstCol, sumRow, cellBoldStyles, columnNames);
                            }
                        }
                        activeSheet++;
                    }
                }
                else if (isDetail && (language == "007" || language == "0071"))
                {
                    int rowNum = 1;
                    for (int r = 0; r < entryData.Rows.Count; r++)
                    {
                        entryData.Rows[r]["RowNumber"] = rowNum;
                        if (entryData.Columns.Contains("Class") && entryData.Rows[r]["Class"] != null && entryData.Rows[r]["Class"].ToString().ToUpper() == "PMT")
                        {
                            helper.SetData(row + r, EntryFirstCol, entryData.Rows[r], cellHighLightStyles, columnNames);
                        }
                        else
                        {
                            helper.SetData(row + r, EntryFirstCol, entryData.Rows[r], cellStyles, columnNames);
                        }
                        rowNum = rowNum + 1;
                    }
                }
                else
                {
                    for (int r = 0; r < entryData.Rows.Count; r++)
                    {
                        if (entryData.Columns.Contains("Class") && entryData.Rows[r]["Class"] != null && entryData.Rows[r]["Class"].ToString().ToUpper() == "PMT")
                        {
                            helper.SetData(row + r, EntryFirstCol, entryData.Rows[r], cellHighLightStyles, columnNames);
                        }
                        else
                        {
                            helper.SetData(row + r, EntryFirstCol, entryData.Rows[r], cellStyles, columnNames);
                        }
                    }
                }
            }
        }
        
        private void MergeInvRegion(NpoiHelper helper,ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol)
        {
            // 清空被合并数据
            for (int iRow = firstRow+1; iRow <= lastRow; iRow++)
            {
                for (int iCol = firstCol; iCol <= lastCol; iCol++)
                {
                    helper.TryClearCellContents(iRow, iCol);
                }
            }
            sheet.AddMergedRegion(new CellRangeAddress(firstRow, lastRow, firstCol, lastCol));
        }

        private Dictionary<string, string> getTemplateConfigDict(string configString)
        {
            Dictionary<string, string> nDict = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(configString))
            {
                string[] kvs = configString.Split(';');
                for (int i = 0; i < kvs.Length; i++)
                {
                    if (!string.IsNullOrEmpty(kvs[i]))
                    {
                        string[] kv = kvs[i].Split(new string[] { ":=" }, StringSplitOptions.RemoveEmptyEntries);
                        if (kv.Length > 1)
                            nDict[kv[0]] = kv[1];
                        else
                            nDict[kv[0]] = "";
                    }
                }
            }
            return nDict;
        }

        #region Set SOA report Datas
        private void setData(string templateFileName, string repfileName, List<Sites> lstSites, List<InvoiceAging> lstDatas)
        {
            int rowNo = 15;
            int colNo = 7;
            string companyName = string.Empty;
            string address1 = string.Empty;
            string address2 = string.Empty;
            string telAndFax = string.Empty;
            string coName = string.Empty;
            string bankName = string.Empty;
            string bankAddress = string.Empty;
            string accountNoUsd = string.Empty;
            string accountNoHkd = string.Empty;
            string swiftCode = string.Empty;
            //decimal rate = 0;
            List<InvoiceAging> lstInvAging = new List<InvoiceAging>();

            try
            {
                lstInvAging = lstDatas.OrderBy(o => o.DueDate).ToList();
                NpoiHelper helper = new NpoiHelper(templateFileName);
                helper.Save(repfileName, true);
                helper = new NpoiHelper(repfileName);
                string sheetName = "";

                foreach (string sheet in helper.Sheets)
                {
                    sheetName = sheet;
                    break;
                }

                //设置sheet
                helper.ActiveSheetName = sheetName;

                //通过siteCode获取对应的site信息
                var sitesInfo = lstSites.Where(o => o.LegalEntity == lstInvAging[0].LegalEntity).ToList();

                //设置Excel的固定信息
                if (sitesInfo.Count > 0)
                {
                    companyName = sitesInfo[0].SiteNameSys;
                    address1 = sitesInfo[0].Address1;
                    address2 = sitesInfo[0].Address2;
                    telAndFax = "TEL:" + sitesInfo[0].Telephone + " FAX:" + sitesInfo[0].Fax;
                    coName = sitesInfo[0].SiteNameSys.ToUpper();
                    bankName = sitesInfo[0].BankName;
                    bankAddress = sitesInfo[0].BankAddress;
                    accountNoUsd = sitesInfo[0].AccountNoUsd;
                    accountNoHkd = sitesInfo[0].AccountNoHkd;
                    swiftCode = sitesInfo[0].SwiftCode;
                }

                //头部信息
                helper.SetData(4, 0, companyName);
                helper.SetData(6, 0, address1);
                helper.SetData(7, 0, address2);
                helper.SetData(8, 0, telAndFax);
                helper.SetData(2, 8, coName);
                helper.SetData(3, 8, bankName);
                helper.SetData(4, 8, bankAddress);
                helper.SetData(5, 8, accountNoUsd);
                helper.SetData(6, 8, accountNoHkd);
                helper.SetData(7, 8, swiftCode);
                helper.SetData(10, 1, lstInvAging[0].CustomerName.Replace("&", "and").Replace("?", "").Replace("%", "").Replace("#", "").Replace("/", "").Replace("=", "").Replace(",", "").Replace("*", "").Replace(@"\", "").Replace("<", "").Replace(">", "").Replace("|", "").Replace(":", "").Replace(@"\\", ""));
                helper.SetData(11, 1, lstInvAging[0].CustomerNum);
                helper.SetData(11, 7, "Please make payable to '" + coName + "' and send to");
                helper.SetData(12, 7, address1);

                //设置Excel的内容信息
                foreach (var lst in lstInvAging)
                {
                    int cellRowNum = rowNo + 1;
                    string cellFormula = "IF(G" + cellRowNum + "=\"\",\"\",($B$13 - G" + cellRowNum + "))";
                    helper.SetFormula(rowNo, colNo, cellFormula, false);
                    helper.SetData(rowNo, 0, lst.CustomerNum);
                    helper.SetData(rowNo, 1, lst.CustomerName.Replace("&", "and").Replace("?", "").Replace("%", "").Replace("#", "").Replace("/", "").Replace("=", "").Replace(",", "").Replace("*", "").Replace(@"\", "").Replace("<", "").Replace(">", "").Replace("|", "").Replace(":", "").Replace(@"\\", ""));
                    helper.SetData(rowNo, 2, lst.MstCustomer);
                    helper.SetData(rowNo, 3, lst.InvoiceNum);
                    helper.SetData(rowNo, 4, lst.PoNum);
                    helper.SetData(rowNo, 5, lst.InvoiceDate.Value);
                    helper.SetData(rowNo, 6, lst.DueDate.Value);
                    helper.SetData(rowNo, 8, lst.Class);

                    helper.SetData(rowNo, 9, lst.OriginalAmt);
                    helper.SetData(rowNo, 10, lst.BalanceAmt);
                    helper.SetData(rowNo, 11, lst.Currency);
                    helper.SetData(rowNo, 12, lst.SoNum);
                    helper.SetData(rowNo, 13, lst.Remark);

                    rowNo++;
                }

                //formula calcuate result
                helper.ForceFormulaRecalculation(false);
                helper.Save(repfileName, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        #endregion

        public List<InvoiceLog> LogInvoice(List<int> invIds, Action<InvoiceLog> supplymentCallBack)
        {
            string deal = AppContext.Current.User.Deal.ToString();
            string eid = AppContext.Current.User.EID.ToString();
            DateTime operDT = AppContext.Current.User.Now;

            List<InvoiceAging> invList = (from inv in CommonRep.GetDbSet<InvoiceAging>()
                                          where invIds.Contains(inv.Id)
                                          select inv).ToList<InvoiceAging>();

            List<InvoiceLog> invlogList = new List<InvoiceLog>();
            InvoiceLog invlog = new InvoiceLog();
            foreach (var inv in invList)
            {
                invlog = new InvoiceLog();
                invlog.Deal = deal;
                invlog.CustomerNum = inv.CustomerNum;
                invlog.InvoiceId = inv.InvoiceNum;
                invlog.LogDate = operDT;
                invlog.LogPerson = eid;
                invlog.LogType = "1"; //soaFlg;
                invlog.OldStatus = inv.States;
                invlog.NewStatus = inv.States;
                invlog.OldTrack = inv.TrackStates;
                invlog.Discription = "";
                supplymentCallBack(invlog);
                invlogList.Add(invlog);
            }
            return invlogList;
        }

        public class MyCurClass
        {
            public string cur { get; set; }
            public decimal? amt { get; set; }
        }

        public string GetHTMLTableByDataTable(System.Data.DataTable xDataTable)
        {
            if (xDataTable == null)
                return "";

            StringBuilder nHtml = new StringBuilder();
            nHtml.Append("<Table cellspacing=\"0\" cellpadding=\"0\" style=\"font-family:'Microsoft YaHei';font-size: 9pt;text-align:center\">" + Environment.NewLine);

            nHtml.Append("<tr>" + Environment.NewLine);
            for (int iCol = 0; iCol < xDataTable.Columns.Count; iCol++)
            {
                if (iCol != xDataTable.Columns.Count - 1)
                    nHtml.Append("<th style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:60px\">" + xDataTable.Columns[iCol] + "</th>" + Environment.NewLine);
                else
                    nHtml.Append("<th style=\"border:#000000 solid 1px;min-width:60px\">" + xDataTable.Columns[iCol] + "</th>" + Environment.NewLine);
            }
            nHtml.Append("</tr>" + Environment.NewLine);

            for (int iRow = 0; iRow < xDataTable.Rows.Count; iRow++)
            {
                nHtml.Append("<tr>" + Environment.NewLine);
                for (int iCol = 0; iCol < xDataTable.Columns.Count; iCol++)
                {
                    string strNumberAlign = "";
                    string strcolumnname = xDataTable.Columns[iCol].ColumnName;
                    if (strcolumnname == "Amount" || strcolumnname == "Balance Amount") {
                        strNumberAlign = "align = \"right\"";
                    }
                    if (strcolumnname == "Status")
                    {
                        strNumberAlign = "align = \"left\"";
                    }
                    if (iCol != xDataTable.Columns.Count - 1)
                        nHtml.Append("<td " + strNumberAlign + " style =\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + xDataTable.Rows[iRow][iCol] + "</td>" + Environment.NewLine);
                    else
                        nHtml.Append("<td " + strNumberAlign + " style =\"border-left:#000000 solid 1px;border-right:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + xDataTable.Rows[iRow][iCol] + "</td>" + Environment.NewLine);
                }
                nHtml.Append("</tr>" + Environment.NewLine);
            }

            nHtml.Append("</Table>");

            return nHtml.ToString();
        }

        public string GetHTMLTableByDataTableCa(System.Data.DataTable xDataTable)
        {
            if (xDataTable == null)
                return "";

            StringBuilder nHtml = new StringBuilder();
            nHtml.Append("<Table cellspacing=\"0\" cellpadding=\"0\" style=\"font-family:'Microsoft YaHei';font-size: 9pt;text-align:center\">" + Environment.NewLine);

            nHtml.Append("<tr>" + Environment.NewLine);
            for (int iCol = 0; iCol < xDataTable.Columns.Count; iCol++)
            {
                if (iCol != xDataTable.Columns.Count - 1)
                    nHtml.Append("<th style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;background-color:#000080;color:white;min-width:60px\">" + xDataTable.Columns[iCol] + "</th>" + Environment.NewLine);
                else
                    nHtml.Append("<th style=\"border:#000000 solid 1px;background-color:#000080;color:white;min-width:60px\">" + xDataTable.Columns[iCol] + "</th>" + Environment.NewLine);
            }
            nHtml.Append("</tr>" + Environment.NewLine);

            for (int iRow = 0; iRow < xDataTable.Rows.Count; iRow++)
            {
                nHtml.Append("<tr>" + Environment.NewLine);
                for (int iCol = 0; iCol < xDataTable.Columns.Count; iCol++)
                {
                    string strNumberAlign = "";
                    string strcolumnname = xDataTable.Columns[iCol].ColumnName;
                    if (strcolumnname == "Net Amount" || strcolumnname == "Total Amount" || strcolumnname == "Bank Charge")
                    {
                        strNumberAlign = "align = \"right\"";
                    }
                    else if (strcolumnname == "Currency" || strcolumnname == "RV#" || strcolumnname == "Value Date" || strcolumnname == "Accnt Number" || strcolumnname == "SiteUseId") {
                        strNumberAlign = "align = \"center\"";
                    }
                    else
                    {
                        strNumberAlign = "align = \"left\"";
                    }
                    if (iCol != xDataTable.Columns.Count - 1)
                        nHtml.Append("<td " + strNumberAlign + " style =\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + xDataTable.Rows[iRow][iCol] + "</td>" + Environment.NewLine);
                    else
                        nHtml.Append("<td " + strNumberAlign + " style =\"border-left:#000000 solid 1px;border-right:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + xDataTable.Rows[iRow][iCol] + "</td>" + Environment.NewLine);
                }
                nHtml.Append("</tr>" + Environment.NewLine);
            }

            nHtml.Append("</Table>");

            return nHtml.ToString();
        }
        public bool zipexports(List<string> lfiles, string tozipfiles)
        {
            string fromfiles = string.Join(";", lfiles);
            string pwd = "";
            return ZipHelper.ZipMultiFiles(fromfiles, tozipfiles, pwd);
        }

        public string exportSoafiles(List<int> intIds, string customerNum, string siteUseId, string fileType)
        {
            System.Data.DataTable[] reportItemList;
            string language = CommonRep.GetQueryable<SysTypeDetail>().Where(x => x.TypeCode == "045" && x.DetailName == AppContext.Current.User.EID).Select(x => x.DetailValue3).FirstOrDefault();
            if (string.IsNullOrEmpty(language)) {
                language = "001";
            }
            List<string> lstPath = this.setContent(intIds, "001", out reportItemList, customerNum, siteUseId, language, "", "", "", "", "", fileType, true);
            string fileid = "";
            if (lstPath.Count == 1)
            {
                fileid = lstPath.FirstOrDefault();
            }
            else
            {
                List<string> filePath = CommonRep.GetQueryable<AppFile>().Where(x => lstPath.Contains(x.FileId)).Select(x => x.PhysicalPath).ToList();
                string filename = "SOA_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".zip";
                string tmpFile = Path.Combine(Path.GetTempPath(), filename);
                this.zipexports(filePath, tmpFile);
                // File 记录
                AppFile ffiles = new AppFile();
                ffiles.Operator = AppContext.Current.User.EID;
                fileid = System.Guid.NewGuid().ToString();
                ffiles.FileId = fileid;
                ffiles.FileName = filename;
                ffiles.PhysicalPath = tmpFile;
                ffiles.CreateTime = DateTime.Now;
                ffiles.UpdateTime = DateTime.Now;
                CommonRep.Add(ffiles);
                CommonRep.Commit();
            }

            return fileid;
        }

        /// <summary>
        /// create soa info report based on the reportItemList retrieved from the setContent method. added by zhangYu
        /// </summary>
        /// <param name="reportItemList"></param>
        /// <returns></returns>
        public string GetInvoiceReport(List<SOAInfoReportItem> reportItemList)
        {
            StringBuilder htmlReport = new StringBuilder();
            Dictionary<string, decimal> overdueCurrencys = new Dictionary<string, decimal>();
            Dictionary<string, decimal> currCurrencys = new Dictionary<string, decimal>();
            Dictionary<string, decimal> overdueCurrencysTotal = new Dictionary<string, decimal>();
            Dictionary<string, decimal> currCurrencysTotal = new Dictionary<string, decimal>();
            #region ready
            foreach (SOAInfoReportItem rep in reportItemList)
            {
                //get the OverDuecharge total col 
                foreach (Tuple<string, decimal> overDue in rep.overdueCharge)
                {
                    if (!overdueCurrencys.ContainsKey(overDue.Item1))
                    {
                        overdueCurrencys.Add(overDue.Item1, overDue.Item2);
                        overdueCurrencysTotal.Add(overDue.Item1, 0);
                    }
                }
                //get the currentcharge total col
                foreach (Tuple<string, decimal> curr in rep.currentCharge)
                {
                    if (!currCurrencys.ContainsKey(curr.Item1))
                    {
                        currCurrencys.Add(curr.Item1, curr.Item2);
                        currCurrencysTotal.Add(curr.Item1, 0);
                    }
                }
            }
            #endregion
            #region header
            int colCountOverDue = overdueCurrencys.Count();
            int colCountCurrent = currCurrencys.Count();
            // table header title
            htmlReport.Append("<table cellSpacing='0' cellPadding='0' width ='100%' border='1'");
            htmlReport.Append(">");
            htmlReport.Append("<tr valign='middle' bgcolor='pink'>");
            htmlReport.Append("<th rowspan='2'>Entity</th>");
            htmlReport.Append("<th rowspan='2'>Customer Code</th>");
            htmlReport.Append("<th rowspan='2'>Customer Name</th>");
            //Overdue Charge
            if (colCountOverDue > 0)
            {
                htmlReport.Append("<th  colspan=" + colCountOverDue * 2 + "><b>Overdue</b></th>");
            }
            //Current Charge
            if (colCountCurrent > 0)
            {
                htmlReport.Append("<th  colspan=" + colCountCurrent * 2 + "><b>Current</b></th>");
            }

            htmlReport.Append("</tr>");
            htmlReport.Append("<tr valign='middle' bgcolor='pink'>");
            //Currency,Amount
            for (int i = 0; i < colCountOverDue; i++)
            {
                htmlReport.Append("<td valign='middle' align='middle'><b>Currency</b></td>");
                htmlReport.Append("<td valign='middle' align='middle'><b>Amount</b></td>");
            }
            //Currency,Amount
            for (int i = 0; i < colCountCurrent; i++)
            {
                htmlReport.Append("<td valign='middle' align='middle'><b>Currency</b></td>");
                htmlReport.Append("<td valign='middle' align='middle'><b>Amount</b></td>");
            }
            htmlReport.Append("</tr>");
            #endregion
            #region body
            Customer cus = new Customer();
            foreach (SOAInfoReportItem soaRep in reportItemList)
            {
                cus = new Customer();
                htmlReport.Append("<tr valign='middle' >");
                htmlReport.Append("<td align='middle'><b>" + soaRep.LegalEntity + "</b></td>");
                cus = CommonRep.GetQueryable<Customer>().
                    Where(c => c.CustomerNum == soaRep.CustomerNum).FirstOrDefault();
                htmlReport.Append("<td align='middle'><b>" + soaRep.CustomerNum + "</b></td>");
                htmlReport.Append("<td align='middle'><b>" + soaRep.CustomerName + "</b></td>");

                foreach (var curr in overdueCurrencys)
                {
                    var curAmt = soaRep.overdueCharge.Find(C => C.Item1 == curr.Key);
                    decimal amt = 0;
                    if (curAmt != null)
                    {
                        amt = curAmt.Item2;
                        overdueCurrencysTotal[curr.Key] += amt;
                    }
                    else
                    {
                        amt = 0;
                    }
                    htmlReport.Append("<td align='middle'><b>" + curr.Key + "</b></td>");
                    htmlReport.Append("<td align='right'><b>" + amt + "</b></td>");
                }
                foreach (var curr in currCurrencys)
                {
                    var curAmt = soaRep.currentCharge.Find(C => C.Item1 == curr.Key);
                    decimal amt = 0;
                    if (curAmt != null)
                    {
                        amt = curAmt.Item2;
                        currCurrencysTotal[curr.Key] += amt;
                    }
                    else
                    {
                        amt = 0;
                    }
                    htmlReport.Append("<td align='middle'><b>" + curr.Key + "</b></td>");
                    htmlReport.Append("<td align='right'><b>" + amt + "</b></td>");
                }
                htmlReport.Append("</tr>");
            }

            #endregion
            #region footer

            int col = colCountCurrent * 2 + colCountOverDue * 2;
            var overDueAndCurrent = overdueCurrencysTotal.Concat(currCurrencysTotal).GroupBy(g => g.Key).Select(t => new { key = t.Key, amt = t.Sum(p => p.Value) });
            foreach (var item in overDueAndCurrent)
            {
                htmlReport.Append("<tr valign='middle' align='right' >");
                htmlReport.Append("<th colspan='3'>Grand " + item.key + " Total</th>");
                htmlReport.Append("<th  colspan=" + col + "><b>" + item.amt + "</b></th>");
                htmlReport.Append("</tr>");
            }
         
            #endregion
            htmlReport.Append("</TABLE>");

            return htmlReport.ToString();
        }

        public string setNotClear(string strType, List<string> idList) {
            try
            {
                for (int i = 0; i < idList.Count; i++)
                {
                    InvoiceAging ia = new InvoiceAging();
                    int id = Int32.Parse(idList[i]);
                    ia = CommonRep.GetQueryable<InvoiceAging>().Where(o => o.Id == id).FirstOrDefault();
                    if (strType == "1")
                    {
                        ia.NotClear = true;
                    }
                    else {
                        ia.NotClear = false;
                    }
                    CommonRep.Save(ia);
                    CommonRep.Commit();
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                return "Set NotClear Failed";
            }

            return "Set NotClear Successed";
        }

        public string clearPTP(List<string> idList)
        {
            try
            {
                List<InvoiceLog> invlogList = new List<InvoiceLog>();
                InvoiceLog Ig = new InvoiceLog();
                InvoiceAging ia = new InvoiceAging();
                string invoiceNums = "";
                for (int i = 0; i < idList.Count; i++)
                {
                    string[] par = idList[i].Split('|');
                    int id = Int32.Parse(par[0]);
                    ia = CommonRep.GetQueryable<InvoiceAging>().Where(o => o.Id == id).FirstOrDefault();
                    var old = ia;
                    ia.PtpDate = null;
                    ia.TrackStates = "000";
                    ia.FinishedStatus = "0";
                    ia.MailId = null;
                    ia.CallId = null;

                    //add operate log
                    ObjectHelper.CopyObjectWithUnNeed(ia, old, new string[] { "Id" });
                    invoiceNums += par[1] + ",";
                    Ig.Deal = AppContext.Current.User.Deal;
                    Ig.LogDate = DateTime.Now;
                    Ig.LogPerson = AppContext.Current.User.EID;
                    Ig.LogAction = "ClearPTP";
                    Ig.LogType = "0";
                    Ig.InvoiceId = par[1];
                    Ig.CustomerNum = old.CustomerNum;
                    Ig.OldTrack = old.TrackStates;
                    Ig.NewTrack = "000";
                    Ig.OldStatus = old.States;
                    Ig.NewStatus = old.States;
                    Ig.ContactPerson = AppContext.Current.User.EID;
                    Ig.SiteUseId = old.SiteUseId;
                    invlogList.Add(Ig);

                    // remove data from payment and payment_invoice
                    var paymentInvoiceList = CommonRep.GetQueryable<T_PTPPayment_Invoice>().Where(o => o.InvoiceId == id).ToList();
                    CommonRep.RemoveRange(paymentInvoiceList);

                    var paymentIdList = paymentInvoiceList.Select(o => o.PTPPaymentId);
                    var paymentList = CommonRep.GetQueryable<T_PTPPayment>().Where(o => paymentIdList.Contains(o.Id));  
                    CommonRep.RemoveRange(paymentList);
                }
                CommonRep.AddRange(invlogList);
                CommonRep.Commit();

            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                return "Clear PTP Failed";
            }

            return "Clear PTP Successed";
        }

        public string clearOverdueReason(List<string> idList)
        {
            try
            {
                for (int i = 0; i < idList.Count; i++)
                {
                    string[] par = idList[i].Split('|');
                    int id = Int32.Parse(par[0]);
                    InvoiceAging ia = CommonRep.GetQueryable<InvoiceAging>().FirstOrDefault(o => o.Id == id);
                    ia.OverdueReason = string.Empty;
                    ia.TrackStates = "000";
                    CommonRep.Commit();
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                return "Clear OverdueReason Failed";
            }

            return "Clear OverdueReason Successed";
        }

        public string clearComments(List<string> idList)
        {
            try
            {
                for (int i = 0; i < idList.Count; i++)
                {
                    string[] par = idList[i].Split('|');
                    int id = Int32.Parse(par[0]);
                    InvoiceAging ia = CommonRep.GetQueryable<InvoiceAging>().FirstOrDefault(o => o.Id == id);
                    ia.BalanceMemo = string.Empty;
                    ia.MemoExpirationDate = null;
                    CommonRep.Commit();
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                return "Clear Comments Failed";
            }

            return "Clear Comments Successed";
        }

        /// <summary>
        /// 把Excel文件转换成PDF格式文件
        /// </summary>
        /// <param name="sourcePath">源文件路径</param>
        /// <param name="targetPath">目标文件路径</param>
        /// <returns>true=转换成功</returns>
        private bool XLSConvertToPDF(string stamp, string sourcePath, string targetPath, int rowCount)
        {
            Aspose.Cells.Workbook excel = new Aspose.Cells.Workbook(sourcePath);
            foreach (Aspose.Cells.Worksheet ws in excel.Worksheets)
            {
                if (ws.Index == 1 )
                {
                    continue;
                }
                ws.AutoFitRow(10);  //Head行，列头有折行
                ws.PageSetup.Orientation = Aspose.Cells.PageOrientationType.Landscape;
                ws.PageSetup.Zoom = 100;//以100%的缩放模式打开
                ws.PageSetup.PaperSize = Aspose.Cells.PaperSizeType.PaperA4;
                ws.PageSetup.FitToPagesWide = 1;
                ws.PageSetup.PrintArea = "A1:V" + (rowCount + 11).ToString();
                //添加印章
                if (!string.IsNullOrEmpty(stamp))
                {
                    string stampFile = stamp.TrimStart('~').Replace("/", "\\").TrimStart('\\');
                    stampFile = Path.Combine(HttpRuntime.AppDomainAppPath, stampFile);
                    int iIndex = ws.Pictures.Add(0, 2, stampFile);  //左边
                    Aspose.Cells.Drawing.Picture pic = ws.Pictures[iIndex];
                    pic.Placement = Aspose.Cells.Drawing.PlacementType.FreeFloating;
                    pic.Width = 150;
                    pic.Height = 150;
                }
            }
            excel.CalculateFormula(true);
            excel.Save(targetPath, Aspose.Cells.SaveFormat.Pdf);

            return true;
        }
    }

    public class FastPropertyComparer<T> : IEqualityComparer<T>
    {
        private Func<T, Object> getPropertyValueFunc = null;

        /// <summary>
        /// 通过propertyName 获取PropertyInfo对象
        /// </summary>
        /// <param name="propertyName"></param>
        public FastPropertyComparer(string propertyName)
        {
            PropertyInfo _PropertyInfo = typeof(T).GetProperty(propertyName,
            BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
            if (_PropertyInfo == null)
            {
                throw new ArgumentException(string.Format("{0} is not a property of type {1}.",
                    propertyName, typeof(T)));
            }

            ParameterExpression expPara = Expression.Parameter(typeof(T), "obj");
            MemberExpression me = Expression.Property(expPara, _PropertyInfo);
            getPropertyValueFunc = Expression.Lambda<Func<T, object>>(me, expPara).Compile();
        }

        #region IEqualityComparer<T> Members

        public bool Equals(T x, T y)
        {
            object xValue = getPropertyValueFunc(x);
            object yValue = getPropertyValueFunc(y);

            if (xValue == null)
                return yValue == null;

            return xValue.Equals(yValue);
        }

        public int GetHashCode(T obj)
        {
            object propertyValue = getPropertyValueFunc(obj);

            if (propertyValue == null)
                return 0;
            else
                return propertyValue.GetHashCode();
        }

        #endregion
    }
}
