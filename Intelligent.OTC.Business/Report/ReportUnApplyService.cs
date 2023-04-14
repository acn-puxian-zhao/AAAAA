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
    public class ReportUnApplyService
    {
        public OTCRepository CommonRep { get; set; }

        public List<ReportUnApplySumItem> GetSum()
        {
            IEnumerable<ReportUnApplySumItem> result = null;

            try
            {
                string sql = @"SELECT [Region]
                                  ,[Type]
                                  ,[CURRENCY]
                                  ,[Amount]
                                  ,[TotalAR]
                              FROM [dbo].[V_Report_UnApply_SUM] ";

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportUnApplySumItem>(sql, parameters).OrderBy(o=>o.Region).ThenBy(o=>o.Type).ToList();
            }
            catch (Exception ex)
            {
                result = new List<ReportUnApplySumItem>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_UnApply_SUM 异常!");
            }
           
            return result.ToList();
        }

        public List<ReportUnApplyDetailItem> GetDetails(int page, int pageSize, out int total)
        {
            IEnumerable<ReportUnApplyDetailItem> result = null;

            try
            {
                string sql = @"SELECT [Region]
                                      ,[COLLECTOR]
                                      ,[Type]
                                      ,[CustomerName]
                                      ,[CustomerNum]
                                      ,[SiteUseId]
                                      ,[CLASS]
                                      ,[InvoiceNo] as InvoiceNum
                                      ,[InvoiceDate]
                                      ,[DueDate]
                                      ,[FuncCurrCode]
                                      ,[CURRENCY]
                                      ,[DueDays]
                                      ,[InvoiceAmount]
                                      ,[AgingBucket]
                                      ,[CREDIT_TREM] as CreditTrem
                                      ,[Ebname]
                                      ,[LSR]
                                      ,[Sales]
                                      ,[LegalEntity]
                                      ,[Cmpinv]
                                      ,[SoNum]
                                      ,[CPoNum] as PONum
                                      ,[PtpDate]
                                      ,[OverdueReason]
                                      ,[COMMENTS]
                              FROM [dbo].[V_Report_UnApply_Detail]";

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportUnApplyDetailItem>(sql, parameters).OrderBy(o => o.Region).ThenBy(o => o.OverdueReason);
                total = result.Count();
                result = result.Skip((page - 1) * pageSize).Take(pageSize);
            }
            catch (Exception ex)
            {
                result = new List<ReportUnApplyDetailItem>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_UnApply_Detail 异常!");
            }

            return result.ToList();
        }

        public IQueryable<ReportUnApplyDetailItem> GetDetails()
        {
            IQueryable<ReportUnApplyDetailItem> result = null;

            try
            {
                string sql = @"SELECT [Region]
                                      ,[COLLECTOR]
                                      ,[Type]
                                      ,[CustomerName]
                                      ,[CustomerNum] as CustomerNum
                                      ,[SiteUseId]
                                      ,[CLASS]
                                      ,[InvoiceNo] as InvoiceNum
                                      ,[InvoiceDate]
                                      ,[DueDate]
                                      ,[FuncCurrCode]
                                      ,[CURRENCY]
                                      ,[DueDays]
                                      ,[InvoiceAmount]
                                      ,[AgingBucket]
                                      ,[CREDIT_TREM] as CreditTrem
                                      ,[Ebname]
                                      ,[LSR]
                                      ,[Sales]
                                      ,[LegalEntity]
                                      ,[Cmpinv]
                                      ,[SoNum]
                                      ,[CPoNum] as PONum
                                      ,[PtpDate]
                                      ,[OverdueReason]
                                      ,[COMMENTS]
                              FROM [dbo].[V_Report_UnApply_Detail]";

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportUnApplyDetailItem>(sql, parameters).OrderBy(o => o.Region).ThenBy(o => o.OverdueReason).AsQueryable();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_UnApply_Detail 异常!");
            }

            return result;
        }

        public string Export()
        {
            string custPathName = "OverdueportPath";
            string tempFile = HttpContext.Current.Server.MapPath("~/Template/ReportUnApplyTemplate.xlsx");
            string targetFoler = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString());
            string targetFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString() + "UnApplyReport_" + AppContext.Current.User.EID + ".xlsx");
            if (Directory.Exists(targetFoler) == false)
            {
                Directory.CreateDirectory(targetFoler);
            }

            try
            {
                var statistics = GetSum();
                var details = GetDetails();
                WriteUnApplyToExcel(tempFile, targetFile, statistics, details.ToList());
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

            string virPathName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[custPathName].ToString().Trim('~') + "UnApplyReport_" + AppContext.Current.User.EID + ".xlsx";
            return virPathName;
        }

        private void WriteUnApplyToExcel(string tempFile, string target, IList<ReportUnApplySumItem> models, IList<ReportUnApplyDetailItem> details)
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
                    cell.SetCellValue((double)item.TotalAR);

                    //Type
                    cell = row.CreateCell(2);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Type);

                    //Currency
                    cell = row.CreateCell(3);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Currency);

                    //Amount
                    cell = row.CreateCell(4);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue((double)item.Amount);

                    //Rate
                    cell = row.CreateCell(5);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Rate + "%");

                    rowNo++;
                }

                ISheet sheetDetail = helper.Book.GetSheetAt(1);
                for (int rowDetailNo = 1; rowDetailNo <= details.Count(); rowDetailNo++)
                {
                    var detail = details[rowDetailNo - 1];

                    IRow row = sheetDetail.CreateRow(rowDetailNo);

                    //row num
                    ICell cell = row.CreateCell(0);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(rowDetailNo);

                    //Collector
                    cell = row.CreateCell(1);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Collector);

                    //Region
                    cell = row.CreateCell(2);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Region);

                    //CustomerName
                    cell = row.CreateCell(3);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.CustomerName);

                    //CustomerNum
                    cell = row.CreateCell(4);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.CustomerNum);

                    //SiteUseId
                    cell = row.CreateCell(5);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.SiteUseId);

                    //Class
                    cell = row.CreateCell(6);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Class);

                    //InvoiceNum
                    cell = row.CreateCell(7);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.InvoiceNum);

                    //InvoiceDate
                    cell = row.CreateCell(8);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.InvoiceDate.ToString("yyyy-MM-dd"));

                    //DueDate
                    cell = row.CreateCell(9);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.DueDate.ToString("yyyy-MM-dd"));

                    //FuncCurrCode
                    cell = row.CreateCell(10);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.FuncCurrCode);

                    //Currency
                    cell = row.CreateCell(11);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Currency);

                    //Due Days
                    cell = row.CreateCell(12);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.DueDays);

                    //InvoiceAmount
                    cell = row.CreateCell(13);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue((double)detail.InvoiceAmount);

                    // Aging Bucket  12
                    cell = row.CreateCell(14);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.AgingBucket);

                    //Payment Term Desc 13
                    cell = row.CreateCell(15);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.CreditTrem);

                    //EbName
                    cell = row.CreateCell(16);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.EbName);

                    //LsrNameHist
                    cell = row.CreateCell(17);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Lsr);

                    //Sales
                    cell = row.CreateCell(18);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Sales);

                    //LegalEntity
                    cell = row.CreateCell(19);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.LegalEntity);

                    //Cmpinv 18
                    cell = row.CreateCell(20);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Cmpinv);

                    //SONum
                    cell = row.CreateCell(21);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.SONum);

                    //PONum
                    cell = row.CreateCell(22);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.PONum);

                    //PtpDate
                    cell = row.CreateCell(23);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.PtpDate.HasValue ? detail.PtpDate.Value.ToString("yyyy-MM-dd") : "");


                    //OverdueReason
                    cell = row.CreateCell(24);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.OverdueReason);

                    //Comments
                    cell = row.CreateCell(25);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Comments);

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
