using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.WebApi.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "myinvoices")]
    public class MyinvoicesController : ApiController
    {
        [HttpGet]
        [PagingQueryable]
        public IEnumerable<MyinvoicesDto> Get(string closeType = "", string siteUseId = "", string filter = "")
        {
            MyinvoicesService service = SpringFactory.GetObjectImpl<MyinvoicesService>("MyinvoicesService");
            return service.GetMyinvoicesList(closeType, siteUseId);
        }

        /* 修改者: fujie.wan
         * 日 期:  2018-12-12
         * 描 述:  增加以下方法，AllInvoice查询时，使用此方法进行查询，不使用LINQ，提高查询速度
         */
        [HttpPost]
        [Route("api/myinvoice/query")]
        public PageResultDto<MyinvoicesDto> QueryMyInvoice(int pageindex, int pagesize, string custCode, string custName, string eb, string consignmentNumber,string balanceMemo, string memoExpirationDate,
            string legal, string siteUseid, string invoiceNum, string poNum, string soNum,
            string creditTerm, string docuType, string invoiceTrackStates, string memo, 
            string ptpDateF, string ptpDateT,string memoDateF, string memoDateT, string invoiceDateF, string invoiceDateT, string dueDateF, string dueDateT, string cs, string sales, string overdueReason)
        {
            MyinvoicesService service = SpringFactory.GetObjectImpl<MyinvoicesService>("MyinvoicesService");
            return service.GetMyinvoicesListNoLinq(false, pageindex, pagesize, custCode, custName, eb, consignmentNumber, balanceMemo, memoExpirationDate,legal,
                siteUseid, invoiceNum, poNum, soNum, creditTerm, docuType, invoiceTrackStates, memo, ptpDateF, ptpDateT, memoDateF, memoDateT, invoiceDateF, invoiceDateT, dueDateF, dueDateT, cs, sales, overdueReason);
        }

        [HttpGet]
        [Route("api/myinvoice/ExpoertSOA")]
        public string ExpoertSOA(string custCode, string custName, string eb, string consignmentNumber, string balanceMemo, string memoExpirationDate,
            string legal, string siteUseid, string invoiceNum, string poNum, string soNum,
            string creditTerm, string docuType, string invoiceTrackStates, string memo,
            string ptpDateF, string ptpDateT, string memoDateF, string memoDateT, string invoiceDateF, string invoiceDateT, string dueDateF, string dueDateT, string cs, string sales, string overdueReason)
        {
            
            if (invoiceTrackStates == "null" || invoiceTrackStates == "undefined" || string.IsNullOrWhiteSpace(invoiceTrackStates))
            {
                invoiceTrackStates = "000";
            }
            MyinvoicesService service = SpringFactory.GetObjectImpl<MyinvoicesService>("MyinvoicesService");
            PageResultDto<MyinvoicesDto> list =  service.GetMyinvoicesListNoLinq(false, 1, 99999999, custCode, custName, eb, consignmentNumber, balanceMemo, memoExpirationDate, legal,
            siteUseid, invoiceNum, poNum, soNum, creditTerm, docuType, invoiceTrackStates, memo, ptpDateF, ptpDateT, memoDateF, memoDateT, invoiceDateF, invoiceDateT, dueDateF, dueDateT, cs, sales, overdueReason);
            List<MyinvoicesDto> listData = list.dataRows;
            string fileid = "";
            if (listData.Count > 0)
            {
                List<int> intIds = new List<int>();
                foreach (MyinvoicesDto s in listData)
                {
                    intIds.Add(s.Id);
                };
                InvoiceService invservice = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
                fileid = invservice.exportSoafiles(intIds, "", "", "XLS");
            }
            else {
                fileid = "";
            }
            return fileid;
        }

        [HttpGet]
        public HttpResponseMessage ExpoertInvoiceList(string custCode, string custName, string eb, string consignmentNumber, string balanceMemo, string memoExpirationDate,
            string legal, string siteUseid, string invoiceNum, string poNum, string soNum,
            string creditTerm, string docuType, string invoiceTrackStates, string memo,
            string ptpDateF, string ptpDateT, string memoDateF, string memoDateT, string invoiceDateF, string invoiceDateT, string dueDateF, string dueDateT, string cs, string sales, string overdueReason)
        {
            if (invoiceTrackStates == "null" || invoiceTrackStates == "undefined" || string.IsNullOrWhiteSpace(invoiceTrackStates))
            {
                invoiceTrackStates = "000";
            }
            MyinvoicesService service = SpringFactory.GetObjectImpl<MyinvoicesService>("MyinvoicesService");
            return service.ExportInvoicesListForArrow(custCode, custName, eb, consignmentNumber, balanceMemo, memoExpirationDate, legal,
                siteUseid, invoiceNum, poNum, soNum, creditTerm, docuType, invoiceTrackStates, memo, ptpDateF, ptpDateT, memoDateF, memoDateT, invoiceDateF, invoiceDateT, dueDateF, dueDateT, cs, sales, overdueReason);
        }


        [HttpPost]
        public String UploadVat(string FileType)
        {
            HttpFileCollection files = HttpContext.Current.Request.Files;
            string strMessage = string.Empty;

            try
            {
                if (files.Count > 0)
                {
                    MyinvoicesService custService = SpringFactory.GetObjectImpl<MyinvoicesService>("MyinvoicesService");
                    string archivePath = ConfigurationManager.AppSettings["VATStatusPath"].ToString();
                    archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                    if (Directory.Exists(archivePath) == false)
                    {
                        Directory.CreateDirectory(archivePath);
                    }
                    string strTargetFileName = archivePath + "\\" + files[0].FileName;
                    strMessage = custService.UploadFile(files[0], strTargetFileName, FileType);

                }
                return strMessage;
            }
            catch (DbEntityValidationException dbex)
            {
                if (dbex.EntityValidationErrors != null)
                {
                    foreach (var error in dbex.EntityValidationErrors)
                    {
                        Helper.Log.Error(error, dbex);
                    }
                }
                throw new OTCServiceException("Uploaded file error!");
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file error!" + ex.Message);
            }
        }

        [HttpPost]
        public String UploadDSO()
        {
            HttpFileCollection files = HttpContext.Current.Request.Files;
            string strMessage = string.Empty;

            try
            {
                if (files.Count > 0)
                {
                    MyinvoicesService custService = SpringFactory.GetObjectImpl<MyinvoicesService>("MyinvoicesService");
                    string archivePath = ConfigurationManager.AppSettings["DSOFilePath"].ToString();
                    archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                    if (Directory.Exists(archivePath) == false)
                    {
                        Directory.CreateDirectory(archivePath);
                    }
                    string strTargetFileName = archivePath + "\\" + files[0].FileName;
                    strMessage = custService.UploadDSOFile(files[0], strTargetFileName);

                }
                return strMessage;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file error!" + ex.Message);
            }
        }

        [HttpPost]
        [Route("api/MyInvoice/AnalysisDSO")]
        public string AnalysisDSO(string packFileName, string monthList, string packageDays) {
            MyinvoicesService custService = SpringFactory.GetObjectImpl<MyinvoicesService>("MyinvoicesService");
            return custService.AnalysisDSO(packFileName, monthList, packageDays);
        }
    }
}