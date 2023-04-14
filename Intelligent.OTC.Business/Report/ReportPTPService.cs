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
    public class ReportPTPService
    {
        public OTCRepository CommonRep { get; set; }

        public List<ReportPTPSumItem> GetSum()
        {
            IEnumerable<ReportPTPSumItem> result = null;

            try
            {
                string sql = @"SELECT [Region]
                                  ,[CURRENCY]
                                  ,[Category]
                                  ,[odCustomerCount] as CustomerODCount
                                  ,[ptpCustomerCount] as CustomerPTPCount
                                  ,[brokenCustomerCount] as CustomerBrokenCount
                                  ,[odAmount] as ODAmount
                                  ,[ptpAcount] as PTPAmount
                                  ,[brokenAcount] as BrokenAmount
                              FROM [dbo].[V_Report_PTP_SUM]";

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportPTPSumItem>(sql, parameters).OrderBy(o=>o.Region).ToList();
            }
            catch (Exception ex)
            {
                result = new List<ReportPTPSumItem>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_PTP_SUM 异常!");
            }
           
            return result.ToList();
        }

        public List<ReportPTPDetailItem> GetDetails(int page, int pageSize, int category, out int total)
        {
            IEnumerable<ReportPTPDetailItem> result = null;

            try
            {
                string view = "V_Report_OD_Detail";
                if (category == 1)
                {
                    view = "V_Report_PTP_Detail";
                }
                else if (category == 2)
                {
                    view = "V_Report_Broken_Detail";
                }

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
                              FROM [dbo]." + view;

              
                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportPTPDetailItem>(sql, parameters).OrderBy(o => o.Region);
                total = result.Count();
                result = result.Skip((page - 1) * pageSize).Take(pageSize);
            }
            catch (Exception ex)
            {
                result = new List<ReportPTPDetailItem>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_PTP_Detail 异常!");
            }

            return result.ToList();
        }

        public IQueryable<ReportPTPDetailItem> GetDetails(string category)
        {
            IQueryable<ReportPTPDetailItem> result = null;

            try
            {
                string sql = string.Format(@"SELECT [Region]
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
                              FROM [dbo].[V_Report_{0}_Detail]", category);

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportPTPDetailItem>(sql, parameters).OrderBy(o => o.Region).AsQueryable();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_PTP_Detail 异常!");
            }

            return result;
        }

        public string Export()
        {
            string custPathName = "OverdueportPath";
            string tempFile = HttpContext.Current.Server.MapPath("~/Template/ReportPTPTemplate.xlsx");
            string targetFoler = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString());
            string targetFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString() + "PTPReport_" + AppContext.Current.User.EID + ".xlsx");
            if (Directory.Exists(targetFoler) == false)
            {
                Directory.CreateDirectory(targetFoler);
            }

            try
            {
                var statistics = GetSum();
                var odList = GetDetails("OD");
                var ptpList = GetDetails("PTP");
                var brokenList = GetDetails("Broken");
                var detailList = new List<List<ReportPTPDetailItem>>() { odList.ToList(), ptpList.ToList(), brokenList.ToList() };
                WritePTPToExcel(tempFile, targetFile, statistics, detailList);
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

            string virPathName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[custPathName].ToString().Trim('~') + "PTPReport_" + AppContext.Current.User.EID + ".xlsx";
            return virPathName;
        }

        private void WritePTPToExcel(string tempFile, string target, IList<ReportPTPSumItem> models, List<List<ReportPTPDetailItem>>  detailList)
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

                var brokens = models.Where(o=>o.Category == "Broken");
                foreach (var item in brokens)
                {
                    IRow row = sheet.CreateRow(rowNo);
                    //Region
                    ICell cell = row.CreateCell(0);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Region);

                    //CustomerBrokenCount
                    cell = row.CreateCell(1);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.CustomerBrokenCount);

                    //CustomerPTPCount
                    cell = row.CreateCell(2);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.CustomerPTPCount);

                    //CountRate
                    cell = row.CreateCell(3);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.CountRate + "%");

                    //Currency
                    cell = row.CreateCell(4);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Currency);

                    //BrokenAmount
                    cell = row.CreateCell(5);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue((double)item.BrokenAmount);

                    //PTPAmount
                    cell = row.CreateCell(6);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue((double)item.PTPAmount);

                    //Rate
                    cell = row.CreateCell(7);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.AmountRate + "%");

                    rowNo++;
                }

                rowNo = 1;
                var confirms = models.Where(o => o.Category == "Confirm");
                foreach (var item in confirms)
                {
                    IRow row = sheet.GetRow(rowNo);
                    if (row == null)
                    {
                        row = sheet.CreateRow(rowNo);
                    }

                    //Region
                    ICell cell = row.CreateCell(9);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Region);

                    //CustomerPTPCount
                    cell = row.CreateCell(10);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.CustomerPTPCount);

                    //CustomerODCount
                    cell = row.CreateCell(11);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.CustomerODCount);

                    //CountRate
                    cell = row.CreateCell(12);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.CountRate + "%");

                    //Currency
                    cell = row.CreateCell(13);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Currency);

                    //PTPAmount
                    cell = row.CreateCell(14);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue((double)item.PTPAmount);

                    //CustomerODCount
                    cell = row.CreateCell(15);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue((double)item.CustomerODCount);

                    //AmountRate
                    cell = row.CreateCell(16);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.AmountRate + "%");

                    rowNo++;
                }

                for (var i = 0; i < detailList.Count; i++)
                {
                    ISheet sheetDetail = helper.Book.GetSheetAt(i+1);
                    for (int rowDetailNo = 1; rowDetailNo <= detailList[i].Count(); rowDetailNo++)
                    {
                        var detail = detailList[i][rowDetailNo - 1];

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
