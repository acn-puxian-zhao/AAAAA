using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Intelligent.OTC.Business
{
    public class ReportODService
    {
        public OTCRepository CommonRep { get; set; }

        public List<ReportODSumItem> GetSum()
        {
            IEnumerable<ReportODSumItem> result = null;

            try
            {
                string sql = @"SELECT [Region]
                                  ,[OverdueReason]
                                  ,[CURRENCY]
                                  ,[ODAmount]
                                  ,[TotalAmount]
                              FROM [dbo].[V_Report_OD_SUM] ";

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportODSumItem>(sql, parameters).OrderBy(o=>o.Region).ThenBy(o=>o.OverdueReason).ToList();
            }
            catch (Exception ex)
            {
                result = new List<ReportODSumItem>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_OD_SUM 异常!");
            }
           
            return result.ToList();
        }

        public List<ReportODDetailItem> GetDetails(int page, int pageSize, out int total)
        {
            IEnumerable<ReportODDetailItem> result = null;

            try
            {
                string sql = @"SELECT [Region]
                                  ,[Organization]
                                  ,[CUSTOMER_NAME] as CustomerName
                                  ,[CUSTOMER_NUM] as CustomerNum
                                  ,[SiteUseId]
                                  ,[Ebname]
                                  ,[CREDIT_TREM] as creditTrem
                                  ,[FUNC_CURRENCY] as FuncCurrency
                                  ,[INVOICE_NUM] as InvoiceNum
                                  ,[INVOICE_DATE] as InvoiceDate
                                  ,[DUE_DATE] as DueDate
                                  ,[PTP_DATE] as PtpDate
                                  ,[CLASS] as InvoiceType
                                  ,[CURRENCY] as Currency
                                  ,[DueDays]
                                  ,[AgingBucket]
                                  ,[CreditTremDescription]
                                  ,[Cmpinv]
                                  ,[PO_MUM] as PONum
                                  ,[SO_NUM] as SONum
                                  ,[OverdueReason]
                                  ,[ODAmount]
                                  ,[LsrNameHist]
                                  ,[FsrNameHist]
                                  ,[REMARK] as Remark
                              FROM [dbo].[V_Report_OD_Detail]";

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportODDetailItem>(sql, parameters).OrderBy(o => o.Region).ThenBy(o => o.OverdueReason);
                total = result.Count();
                result = result.Skip((page - 1) * pageSize).Take(pageSize);
            }
            catch (Exception ex)
            {
                result = new List<ReportODDetailItem>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_OD_Detail 异常!");
            }

            return result.ToList();
        }

        public IQueryable<ReportODDetailItem> GetDetails()
        {
            IQueryable<ReportODDetailItem> result = null;

            try
            {
                string sql = @"SELECT [Region]
                                  ,[Organization]
                                  ,[CUSTOMER_NAME] as CustomerName
                                  ,[CUSTOMER_NUM] as CustomerNum
                                  ,[SiteUseId]
                                  ,[Ebname]
                                  ,[CREDIT_TREM] as creditTrem
                                  ,[FUNC_CURRENCY] as FuncCurrency
                                  ,[INVOICE_NUM] as InvoiceNum
                                  ,[INVOICE_DATE] as InvoiceDate
                                  ,[DUE_DATE] as DueDate
                                  ,[PTP_DATE] as PtpDate
                                  ,[CLASS] as InvoiceType
                                  ,[CURRENCY] as Currency
                                  ,[DueDays]
                                  ,[AgingBucket]
                                  ,[CreditTremDescription]
                                  ,[Cmpinv]
                                  ,[PO_MUM] as PONum
                                  ,[SO_NUM] as SONum
                                  ,[OverdueReason]
                                  ,[ODAmount]
                                  ,[LsrNameHist]
                                  ,[FsrNameHist]
                                  ,[REMARK] as Remark
                              FROM [dbo].[V_Report_OD_Detail]";

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportODDetailItem>(sql, parameters).OrderBy(o => o.Region).ThenBy(o => o.OverdueReason).AsQueryable();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_OD_Detail 异常!");
            }

            return result;
        }

        public string Export()
        {
            string custPathName = "OverdueportPath";
            string tempFile = HttpContext.Current.Server.MapPath("~/Template/ReportODTemplate.xlsx");
            string targetFoler = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString());
            string targetFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString() + "ODReport_" + AppContext.Current.User.EID + ".xlsx");
            if (Directory.Exists(targetFoler) == false)
            {
                Directory.CreateDirectory(targetFoler);
            }

            try
            {
                var statistics = GetSum();
                var details = GetDetails();
                WriteODToExcel(tempFile, targetFile, statistics, details.ToList());
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Export 异常!");
            }
          
            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }

            string virPathName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[custPathName].ToString().Trim('~') + "ODReport_" + AppContext.Current.User.EID + ".xlsx";
            return virPathName;
        }

        private void WriteODToExcel(string tempFile, string target, IList<ReportODSumItem> models, List<ReportODDetailItem> details)
        {
            try
            {
                NpoiHelper helper = new NpoiHelper(tempFile);
                helper.Save(target, true);
                helper = new NpoiHelper(target);

                ICellStyle styleCell = helper.Book.CreateCellStyle();
                IFont font = helper.Book.CreateFont();
                font.FontName = "Arial";
                font.FontHeight = 9;
                styleCell.SetFont(font);
                styleCell.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;

                int rowNo = 1;
                ISheet sheet = helper.Book.GetSheetAt(0);
                foreach (var item in models)
                {
                    IRow row = sheet.CreateRow(rowNo);
                    //Region
                    ICell cell = row.CreateCell(0);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Region);

                    //TotalAmount
                    cell = row.CreateCell(1);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue((double)item.TotalAmount);

                    //OverdueReason
                    cell = row.CreateCell(2);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.OverdueReason);

                    //Currency
                    cell = row.CreateCell(3);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Currency);

                    //ODAmount
                    cell = row.CreateCell(4);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue((double)item.ODAmount);

                    //Rate
                    cell = row.CreateCell(5);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Rate + "%");

                    rowNo++;
                }

                ISheet sheetDetail = helper.Book.GetSheetAt(1);
                for(int rowDetailNo = 1 ; rowDetailNo <= details.Count(); rowDetailNo++)
                {
                    var detail = details[rowDetailNo-1];

                    IRow row = sheetDetail.CreateRow(rowDetailNo);

                    //row num
                    ICell cell = row.CreateCell(0);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(rowDetailNo);

                    //CustomerName
                    cell = row.CreateCell(1);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.CustomerName);

                    //CustomerNum
                    cell = row.CreateCell(2);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.CustomerNum);

                    //SiteUseId
                    cell = row.CreateCell(3);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.SiteUseId);

                    //InvoiceType
                    cell = row.CreateCell(4);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.InvoiceType);

                    //InvoiceNum
                    cell = row.CreateCell(5);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.InvoiceNum);

                    //InvoiceDate
                    cell = row.CreateCell(6);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.InvoiceDate.ToString("yyyy-MM-dd"));

                    //DueDate
                    cell = row.CreateCell(7);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.DueDate.ToString("yyyy-MM-dd"));

                    //FuncCurrency
                    cell = row.CreateCell(8);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.FuncCurrency);

                    //Currency
                    cell = row.CreateCell(9);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Currency);

                    //Due Days
                    cell = row.CreateCell(10);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.DueDays);

                    //ODAmount  	Amt Remaining	
                    cell = row.CreateCell(11);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue((double)detail.ODAmount);

                    // Aging Bucket  12
                    cell = row.CreateCell(12);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.AgingBucket);

                    //Payment Term Desc 13
                    cell = row.CreateCell(13);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.CreditTrem);

                    //EbName
                    cell = row.CreateCell(14);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.EbName);

                    //LsrNameHist
                    cell = row.CreateCell(15);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.LsrNameHist);

                    //FsrNameHist
                    cell = row.CreateCell(16);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.FsrNameHist);

                    //Organization
                    cell = row.CreateCell(17);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Organization);

                    //Cmpinv 18
                    cell = row.CreateCell(18);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Cmpinv);

                    //SONum
                    cell = row.CreateCell(19);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.SONum);

                    //PONum
                    cell = row.CreateCell(20);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.PONum);

                    //PtpDate
                    cell = row.CreateCell(21);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.PtpDate.HasValue ? detail.PtpDate.Value.ToString("yyyy-MM-dd") : "");


                    //OverdueReason
                    cell = row.CreateCell(22);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.OverdueReason);

                    //Remark
                    cell = row.CreateCell(23);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Remark);
             
                }

                //设置sheet
                helper.Save(target, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
