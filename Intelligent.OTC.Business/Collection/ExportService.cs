using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using NPOI.HSSF.Record.Crypto;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Intelligent.OTC.Business.Collection
{
    public enum FindType
    {
        WHOLE,
        PART
    }

    public enum BorderType
    {
        ALL,
        LEFT,
        RIGHT,
        TOP,
        BOTTOM,
        BORDER_DIAGONAL_NONE,
        BORDER_DIAGONAL_BACKWARD,
        BORDER_DIAGONAL_FORWARD,
        BORDER_DIAGONAL_BOTH
    }

    // Summary:
    //     The enumeration value indicating the line style of a border in a cell
    public enum BorderStyle
    {
        // Summary:
        //     No border
        None = 0,
        //
        // Summary:
        //     Thin border
        Thin = 1,
        //
        // Summary:
        //     Medium border
        Medium = 2,
        //
        // Summary:
        //     dash border
        Dashed = 3,
        //
        // Summary:
        //     dot border
        Dotted = 4,
        //
        // Summary:
        //     Thick border
        Thick = 5,
        //
        // Summary:
        //     double-line border
        Double = 6,
        //
        // Summary:
        //     hair-line border
        Hair = 7,
        //
        // Summary:
        //     Medium dashed border
        MediumDashed = 8,
        //
        // Summary:
        //     dash-dot border
        DashDot = 9,
        //
        // Summary:
        //     medium dash-dot border
        MediumDashDot = 10,
        //
        // Summary:
        //     dash-dot-dot border
        DashDotDot = 11,
        //
        // Summary:
        //     medium dash-dot-dot border
        MediumDashDotDot = 12,
        //
        // Summary:
        //     slanted dash-dot border
        SlantedDashDot = 13,
    }

    public class CellData
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public object Value { get; set; }
    }

    public class ExportService
    {
        private string _FileName;

        public HSSFColor ColorIndex = new HSSFColor();

        public static DateTime InvalidDateTime = new DateTime(1, 1, 1);
        /// <summary>
        /// workbook definition
        /// </summary>
        private IWorkbook book = null;

        /// <summary>
        /// active sheet index definition
        /// </summary>
        private int activeSheet = 0;

        /// <summary>
        /// active sheet name definition
        /// </summary>
        private string activeSheetName = string.Empty;

        private List<int> FormaterSet = new List<int>();

        public string DefaultDateTimeFormater
        {
            get;
            set;
        }

        public Hashtable DateTimeFormater = new Hashtable();

        public IWorkbook Book
        {
            get { return this.book; }
        }

        public string FileName
        {
            get { return _FileName; }
        }

        /// <summary>
        /// Initializes a new instance of the NpoiHelper class
        /// </summary>
        public ExportService()
        {
            DefaultDateTimeFormater = "yyyy/MM/dd";
            this.book = new HSSFWorkbook();
        }

        /// <summary>
        /// Initializes a new instance of the NpoiHelper class
        /// </summary>
        /// <param name="filename">file name</param>
        public ExportService(string filename)
        {
            DefaultDateTimeFormater = "yyyy/MM/dd";
            if (!File.Exists(filename))
            {
                Exception ex = new Exception(string.Format("The file {0} isn't existing.", filename));
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

            try
            {
                FileStream source = new FileStream(filename, FileMode.Open);
                this.book = WorkbookFactory.Create(source);
                source.Close();
                _FileName = filename;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new Exception(string.Format("Open the file {0} failed because of {1}", filename, ex.Message));
            }
        }

        /// <summary>
        /// Initializes a new instance of the NpoiHelper class
        /// </summary>
        /// <param name="filename">file name</param>
        /// <param name="password">open password</param>
        public ExportService(string filename, string password)
        {
            if (!File.Exists(filename))
            {
                Exception ex = new Exception(string.Format("The file {0} isn't existing.", filename));
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

            try
            {
                FileStream source = new FileStream(filename, FileMode.Open);
                Biff8EncryptionKey.CurrentUserPassword = password;
                this.book = WorkbookFactory.Create(source);
                source.Close();
                _FileName = filename;
            }
            catch
            {
                Exception ex = new Exception(string.Format("Open the file {0} failed.", filename));
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Gets the workbook
        /// </summary>
        public IWorkbook Workbook
        {
            get
            {
                return this.book;
            }
        }
        /// <summary>
        /// Gets or sets active sheet index
        /// </summary>
        public int ActiveSheet
        {
            get
            {
                return this.activeSheet;
            }

            set
            {
                if (this.book == null || value >= this.book.NumberOfSheets || value < 0)
                {
                    return;
                }

                this.activeSheet = value;
                this.activeSheetName = this.book.GetSheetName(value);
                this.book.SetActiveSheet(this.activeSheet);
            }
        }

        /// <summary>
        /// Gets or sets active sheet name
        /// </summary>
        public string ActiveSheetName
        {
            get
            {
                return this.activeSheetName;
            }

            set
            {
                if (this.book == null || string.IsNullOrEmpty(value))
                {
                    return;
                }

                List<string> sheets = this.Sheets;
                if (sheets.Contains(value))
                {
                    this.activeSheetName = value;
                    int index = 0;
                    sheets.ForEach(
                        sheet =>
                        {
                            if (sheet == value)
                            {
                                this.activeSheet = index;
                            }

                            index++;
                        });
                    this.book.SetActiveSheet(this.activeSheet);
                }
            }
        }

        /// <summary>
        /// Gets all sheet name
        /// </summary>
        public List<string> Sheets
        {
            get
            {
                List<string> sheets = new List<string>();
                for (int i = 0; this.book != null && i < this.book.NumberOfSheets; i++)
                {
                    sheets.Add(this.book.GetSheetName(i));
                }

                return sheets;
            }
        }

        /// <summary>
        /// save book to file
        /// </summary>
        /// <param name="filename">new filename</param>
        /// <param name="deleteexist">delete file when existing</param>
        public void Save(string filename, bool deleteexist, string password = null)
        {
            if (this.book == null)
            {
                return;
            }
            for (int i = 0; i < this.book.NumberOfSheets; i++)
            {
                this.book.GetSheetAt(i).ForceFormulaRecalculation = true;
            }
            if (File.Exists(filename))
            {
                if (!deleteexist)
                {
                    return;
                }

                try
                {
                    File.SetAttributes(filename, FileAttributes.Normal);
                    File.Delete(filename);
                }
                catch
                {
                    Exception ex = new Exception("The file " + filename + " exists and delete it failed.");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }
            }

            if (!string.IsNullOrEmpty(password))
            {
                if (this.book is HSSFWorkbook)
                {
                    (this.book as HSSFWorkbook).WriteProtectWorkbook(password, "owner");
                }
            }

            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    this.book.Write(fs);

                    // remove the read only recommented flag when opened.
                    if (!string.IsNullOrEmpty(password) && fs.CanSeek)
                    {
                        fs.Seek(0x2A0, SeekOrigin.Begin);
                        fs.WriteByte(0);
                    }
                    _FileName = filename;
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Export data table to excel file
        /// </summary>
        /// <param name="datasettings">export settings</param>
        /// <param name="filename">excel file</param>
        /// <param name="exportColumnTitleWhenEmpty">whether export the title of column when the data is empty</param>
        public void ExportDataList(List<CollectingReportDto> datasettings)
        {
            this.ExportDataList(datasettings, 1);
            this.Save(_FileName, true);
        }

        public void ExportAgingDataList(List<CollectingReportDto> datasettings)
        {
            this.ExportAgingDataList(datasettings, 2);
            this.Save(_FileName, true);
        }

        public void ExportCustomerStatisticDataList(List<StatisticsCollectDto> datasettings)
        {
            this.ExportCustomerStatisticDataList(datasettings, 1);
            this.Save(_FileName, true);
        }

        public void ExportCollectorStatisticDataList(List<V_STATISTICS_COLLECTOR> datasettings)
        {
            this.ExportCollectorStatisticDataList(datasettings, 1);
            this.Save(_FileName, true);
        }

        public void ExportDailyAgingDataList(List<DailyAgingDto> datasettings)
        {
            this.ExportDailyAgingDataList(datasettings, 1);
            this.Save(_FileName, true);
        }

        public void ExportDataList(List<V_CustomerAssessment> datasettings)
        {
            this.ExportDataList(datasettings, 1);
            this.Save(_FileName, true);
        }

        public void ExportDataList(List<V_CustomerAssessmentHistory> datasettings)
        {
            this.ExportDataList(datasettings, 1);
            this.Save(_FileName, true);
        }

        public void ExportDsoSheet1DataList(List<DsoAnalysisDto> datasettings)
        {
            this.ExportDSO1DataList(datasettings, 2);
            this.Save(_FileName, true);
        }

        public void ExportDsoSheet2DataList(List<DsoAnalysisDto> datasettings)
        {
            this.ExportDSO2DataList(datasettings, 2);
            this.Save(_FileName, true);
        }

        public void ExportDsoSheet3DataList(List<DsoAnalysisDto> datasettings)
        {
            this.ExportDSO3DataList(datasettings, 1);
            this.Save(_FileName, true);
        }

        public void ExportDSO1DataList(List<DsoAnalysisDto> datasettings, int startRowNo)
        {
            if (datasettings == null && datasettings.Count == 0) return;
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            foreach (var data in datasettings)
            {
                var row = sheet.CreateRow(startRowNo);
                SetCellValue(row, 0, startRowNo - 1);
                SetCellValue(row, 1, data.Legal);
                SetCellValue(row, 2, data.CusNumberNum);
                SetCellValue(row, 3, data.CusName);
                SetCellValue(row, 4, data.DSO);
                SetCellValue(row, 5, data.REV);
                startRowNo++;
            }
        }

        public void ExportDSO2DataList(List<DsoAnalysisDto> datasettings, int startRowNo)
        {
            if (datasettings == null && datasettings.Count == 0) return;
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            foreach (var data in datasettings)
            {
                var row = sheet.CreateRow(startRowNo);
                SetCellValue(row, 0, startRowNo - 1);
                SetCellValue(row, 1, data.Legal);
                SetCellValue(row, 2, data.CusNumberNum);
                SetCellValue(row, 3, data.CusName);
                SetCellValue(row, 4, data.DSO);
                SetCellValue(row, 5, data.PaymentTerm);
                SetCellValue(row, 6, data.GAP);
                startRowNo++;
            }
        }

        public void ExportDSO3DataList(List<DsoAnalysisDto> datasettings, int startRowNo)
        {
            if (datasettings == null && datasettings.Count == 0) return;
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            foreach (var data in datasettings)
            {
                var row = sheet.CreateRow(startRowNo);
                SetCellValue(row, 0, startRowNo);
                SetCellValue(row, 1, data.Legal);
                SetCellValue(row, 2, data.CusNumberNum);
                SetCellValue(row, 3, data.CusName);
                SetCellValue(row, 4, data.ARAvg);
                SetCellValue(row, 5, data.REV);
                SetCellValue(row, 6, data.DSO);
                startRowNo++;
            }
            this.book.SetActiveSheet(0);
        }

        /// <summary>
        /// generate excel book according by Export data table
        /// </summary>
        /// <param name="datasettings">export settings</param>
        /// <param name="exportColumnTitleWhenEmpty">whether export the title of column when the data is empty</param>
        public void ExportDataList(List<CollectingReportDto> datasettings, int startRowNo)
        {
            if (datasettings == null && datasettings.Count == 0) return;
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            foreach (var data in datasettings)
            {
                var row = sheet.CreateRow(startRowNo);
                SetCellValue(row, 0, data.CustomerName);
                SetCellValue(row, 1, data.AccntNumber);
                SetCellValue(row, 2, data.SiteUseId);
                SetCellValue(row, 3, data.SellingLocationCode);
                SetCellValue(row, 4, data.CLASS);
                SetCellValue(row, 5, data.TrxNum);
                SetCellValue(row, 6, data.TrxDate);
                SetCellValue(row, 7, data.DueDate);
                SetCellValue(row, 8, data.OverdueReason);
                SetCellValue(row, 9, data.PaymentTermName);
                SetCellValue(row, 10, data.OverCreditLmt);
                SetCellValue(row, 11, data.OverCreditLmtAcct);
                SetCellValue(row, 12, data.FuncCurrCode);
                SetCellValue(row, 13, data.InvCurrCode);
                SetCellValue(row, 14, data.SalesName);
                SetCellValue(row, 15, data.DueDays);
                SetCellValue(row, 16, data.AmtRemaining);
                SetCellValue(row, 17, data.AmountWoVat);
                SetCellValue(row, 18, data.AgingBucket);
                SetCellValue(row, 19, data.PaymentTermDesc);
                SetCellValue(row, 20, data.SellingLocationCode2);
                SetCellValue(row, 21, data.Ebname);
                SetCellValue(row, 22, data.Customertype);
                SetCellValue(row, 23, data.Isr);
                SetCellValue(row, 24, data.Fsr);
                SetCellValue(row, 25, data.OrgId);
                SetCellValue(row, 26, data.Cmpinv);
                SetCellValue(row, 27, data.SalesOrder);
                SetCellValue(row, 28, data.Cpo);
                SetCellValue(row, 29, data.FsrNameHist);
                SetCellValue(row, 30, data.isrNameHist);
                SetCellValue(row, 31, data.Eb);
                SetCellValue(row, 32, data.LocalName);
                SetCellValue(row, 33, data.VatNo);
                SetCellValue(row, 34, data.VatDate);
                SetCellValue(row, 35, data.Collector);
                SetCellValue(row, 36, data.CurrentStatus);
                SetCellValue(row, 37, data.Lastupdatedate);
                SetCellValue(row, 38, data.ClearingDate);
                SetCellValue(row, 39, data.PtpIdentifiedDate);
                SetCellValue(row, 40, data.PtpDate);
                SetCellValue(row, 41, data.PtpDatehis);
                SetCellValue(row, 42, data.PtpBroken);
                SetCellValue(row, 43, data.PtpComment);
                SetCellValue(row, 44, data.Dispute);
                SetCellValue(row, 45, data.DisputeIdentifiedDate);
                SetCellValue(row, 46, data.DisputeStatus);
                SetCellValue(row, 47, data.DisputeReason);
                SetCellValue(row, 48, data.DisputeComment);
                SetCellValue(row, 49, data.ActionOwnerDepartment);
                SetCellValue(row, 50, data.ActionOwnerName);
                SetCellValue(row, 51, data.NextActionDate);
                SetCellValue(row, 52, data.CommentsHelpNeeded);
                SetCellValue(row, 53, data.IsForwarder);
                SetCellValue(row, 54, data.Forwarder);
                startRowNo++;
            }
        }

        public void ExportDailyAgingDataList(List<DailyAgingDto> datasettings, int startRowNo)
        {
            if (datasettings == null && datasettings.Count == 0) return;

            ICellStyle zt16Style = this.book.CreateCellStyle();
            zt16Style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Yellow.Index;
            zt16Style.FillPattern = FillPattern.SolidForeground;
            zt16Style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            zt16Style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            zt16Style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            zt16Style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            zt16Style.TopBorderColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            zt16Style.LeftBorderColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            zt16Style.RightBorderColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            zt16Style.BottomBorderColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;

            ICellStyle zt17Style = this.book.CreateCellStyle();
            zt17Style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Lavender.Index;
            zt17Style.FillPattern = FillPattern.SolidForeground;
            zt17Style.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Top;
            zt17Style.WrapText = true;
            zt17Style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            zt17Style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            zt17Style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            zt17Style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            zt17Style.TopBorderColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            zt17Style.LeftBorderColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            zt17Style.RightBorderColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            zt17Style.BottomBorderColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;

            ICellStyle zt18Style = this.book.CreateCellStyle();
            zt18Style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Tan.Index;
            zt18Style.FillPattern = FillPattern.SolidForeground;
            zt18Style.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Top;
            zt18Style.WrapText = true;
            zt18Style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            zt18Style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            zt18Style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            zt18Style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            zt18Style.TopBorderColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            zt18Style.LeftBorderColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            zt18Style.RightBorderColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            zt18Style.BottomBorderColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;

            ICellStyle zt19Style = this.book.CreateCellStyle();
            zt19Style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightGreen.Index;
            zt19Style.FillPattern = FillPattern.SolidForeground;
            zt19Style.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Top;
            zt19Style.WrapText = true;
            zt19Style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            zt19Style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            zt19Style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            zt19Style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            zt19Style.TopBorderColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            zt19Style.LeftBorderColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            zt19Style.RightBorderColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            zt19Style.BottomBorderColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;

            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            foreach (var data in datasettings)
            {
                var row = sheet.CreateRow(startRowNo);
                SetCellValue(row, 0, data.legalEntity == null ? "" : data.Collector);
                SetCellValue(row, 1, data.legalEntity == null ? "" : data.legalEntity);
                SetCellValue(row, 2, data.CustomerName == null ? "" : data.CustomerName);
                SetCellValue(row, 3, data.CustomerName == null ? "" : data.LocalizeCustomerName);
                SetCellValue(row, 4, data.AccntNumber == null ? "" : data.AccntNumber);
                SetCellValue(row, 5, data.SiteUseId == null ? "" : data.SiteUseId);
                SetCellValue(row, 6, data.PaymentTermDesc == null ? "" : data.PaymentTermDesc);
                SetCellValue(row, 7, data.Ebname == null ? "" : data.Ebname);
                SetCellValue(row, 8, data.OverCreditLmt == null ? 0 : data.OverCreditLmt);
                SetCellValue(row, 9, data.FuncCurrCode == null ? "" : data.FuncCurrCode);
                SetCellValue(row, 10, data.TotalFutureDue == null ? 0 : data.TotalFutureDue);
                SetCellValue(row, 11, data.Due15Amt == null ? 0 : data.Due15Amt);
                SetCellValue(row, 12, data.Due30Amt == null ? 0 : data.Due30Amt);
                SetCellValue(row, 13, data.Due45Amt == null ? 0 : data.Due45Amt);
                SetCellValue(row, 14, data.Due60Amt == null ? 0 : data.Due60Amt);
                SetCellValue(row, 15, data.Due90Amt == null ? 0 : data.Due90Amt);
                SetCellValue(row, 16, data.Due120Amt == null ? 0 : data.Due120Amt);
                SetCellValue(row, 17, data.Due180Amt == null ? 0 : data.Due180Amt);
                SetCellValue(row, 18, data.Due270Amt == null ? 0 : data.Due270Amt);
                SetCellValue(row, 19, data.Due360Amt == null ? 0 : data.Due360Amt);
                SetCellValue(row, 20, data.DueOver360Amt == null ? 0 : data.DueOver360Amt);
                SetCellValue(row, 21, data.TotalAR == null ? 0 : data.TotalAR);
                SetCellValue(row, 22, data.TotalOverDue == null ? 0 : data.TotalOverDue);
                SetCellValue(row, 23, data.Lsr == null ? "" : data.Lsr);
                SetCellValue(row, 24, data.Fsr == null ? "" : data.Fsr);
                SetCellValue(row, 25, data.SpecialNote == null ? "" : data.SpecialNote);
                SetCellValue(row, 26, data.comments == null ? "" : data.comments);
                SetCellValue(row, 27, data.PTPComment == null ? "" : data.PTPComment);
                SetCellValue(row, 28, data.TotalPTPAmount == null ? 0 : data.TotalPTPAmount);
                SetCellValue(row, 29, data.DisputeAmount == null ? 0 : data.DisputeAmount);
                SetCellValue(row, 30, data.CustomerODPercent == null ? 0 : data.CustomerODPercent);
                SetCellValue(row, 31, data.DisputeODPercent == null ? 0 : data.DisputeODPercent);
                SetCellValue(row, 32, data.PtpODPercent == null ? 0 : data.PtpODPercent);
                SetCellValue(row, 33, data.OthersODPercent == null ? 0 : data.OthersODPercent);
                SetCellValue(row, 34, data.DisputeAnalysis == null ? "" : data.DisputeAnalysis);
                SetCellValue(row, 35, data.AutomaticSendMailDate == null ? "" : data.AutomaticSendMailDate);
                SetCellValue(row, 36, data.AutomaticSendMailCount == null ? 0 : data.AutomaticSendMailCount);
                SetCellValue(row, 37, data.FollowUpCallDate == null ? "" : data.FollowUpCallDate);
                SetCellValue(row, 38, data.FollowUpCallCount == null ? 0 : data.FollowUpCallCount);
                SetCellValue(row, 39, data.CurrentMonthCustomerContact == null ? "NO" : data.CurrentMonthCustomerContact);
                SetCellValue(row, 40, data.comments == null ? "" : data.comments);
                SetCellValue(row, 41, data.CommentExpirationDate == null ? "" : Convert.ToDateTime(data.CommentExpirationDate).ToString("yyyy-MM-dd"));
                SetCellValue(row, 42, data.CommentLastDate == null ? "" : Convert.ToDateTime(data.CommentLastDate).ToString("yyyy-MM-dd HH:mm:ss"));

                startRowNo++;
            }
        }

        public void ExportAgingDataList(List<CollectingReportDto> datasettings, int startRowNo)
        {
            try
            {
                if (datasettings == null && datasettings.Count == 0) return;
                ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
                ICellStyle styleCell = book.CreateCellStyle();
                IFont font = book.CreateFont();
                font.FontName = "Arial";
                font.FontHeight = 9;
                styleCell.SetFont(font);
                styleCell.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                foreach (var data in datasettings)
                {
                    var row = sheet.CreateRow(startRowNo);
                    row.Height = 12 * 20;
                    if (data.CustomerName != null)
                    {
                        SetCellValue(row, 0, data.CustomerName);
                    }
                    if (data.TrxNum != null)
                    {
                        SetCellValue(row, 1, data.TrxNum);
                    }
                    else { SetCellValue(row, 1, ""); }
                    if (data.TrxDate != null)
                    {
                        SetCellValue(row, 2, data.TrxDate);
                    }
                    else { SetCellValue(row, 2, ""); }
                    if (data.AccntNumber != null)
                    {
                        SetCellValue(row, 3, data.AccntNumber);
                    }
                    else { SetCellValue(row, 3, ""); }
                    if (data.SiteUseId != null)
                    {
                        SetCellValue(row, 4, data.SiteUseId);
                    }
                    else { SetCellValue(row, 4, ""); }
                    if (data.FuncCurrCode != null)
                    {
                        SetCellValue(row, 5, data.FuncCurrCode);
                    }
                    else { SetCellValue(row, 5, ""); }
                    if (data.AmtRemaining != null)
                    {
                        SetCellValue(row, 6, data.AmtRemaining);
                    }
                    else { SetCellValue(row, 6, ""); }
                    if (data.CLASS != null)
                    {
                        SetCellValue(row, 7, data.CLASS);
                    }
                    else { SetCellValue(row, 7, ""); }
                    if (data.DueDate != null)
                    {
                        SetCellValue(row, 8, data.DueDate);
                    }
                    else { SetCellValue(row, 8, ""); }
                    if (data.DueDays != null)
                    {
                        SetCellValue(row, 9, data.DueDays);
                    }
                    else { SetCellValue(row, 9, ""); }
                    if (data.AgingBucket != null)
                    {
                        SetCellValue(row, 10, data.AgingBucket);
                    }
                    else { SetCellValue(row, 10, ""); }
                    if (data.PtpAmount != null)
                    {
                        SetCellValue(row, 11, data.PtpAmount);
                    }
                    else { SetCellValue(row, 11, ""); }
                    if (data.DisputeReason != null)
                    {
                        SetCellValue(row, 12, data.DisputeReason);
                    }
                    else { SetCellValue(row, 12, ""); }
                    if (data.DisputeComment != null)
                    {
                        SetCellValue(row, 13, data.DisputeComment);
                    }
                    else { SetCellValue(row, 13, ""); }
                    if (data.PaymentTermName != null)
                    {
                        SetCellValue(row, 14, data.PaymentTermName);
                    }
                    else { SetCellValue(row, 14, ""); }
                    if (data.SellingLocationCode != null)
                    {
                        SetCellValue(row, 15, data.SellingLocationCode);
                    }
                    else { SetCellValue(row, 15, ""); }
                    if (data.OverCreditLmt != null)
                    {
                        SetCellValue(row, 16, data.OverCreditLmt);
                    }
                    else { SetCellValue(row, 16, ""); }
                    if (data.OverCreditLmtAcct != null)
                    {
                        SetCellValue(row, 17, data.OverCreditLmtAcct);
                    }
                    else { SetCellValue(row, 17, ""); }
                    if (data.InvCurrCode != null)
                    {
                        SetCellValue(row, 18, data.InvCurrCode);
                    }
                    else { SetCellValue(row, 18, ""); }
                    if (data.SalesName != null)
                    {
                        SetCellValue(row, 19, data.SalesName);
                    }
                    else { SetCellValue(row, 19, ""); }
                    if (data.AmountWoVat != null)
                    {
                        SetCellValue(row, 20, data.AmountWoVat);
                    }
                    else { SetCellValue(row, 20, ""); }
                    if (data.PaymentTermDesc != null)
                    {
                        SetCellValue(row, 21, data.PaymentTermDesc);
                    }
                    else { SetCellValue(row, 21, ""); }
                    if (data.SellingLocationCode2 != null)
                    {
                        SetCellValue(row, 22, data.SellingLocationCode2);
                    }
                    else { SetCellValue(row, 22, ""); }
                    if (data.Ebname != null)
                    {
                        SetCellValue(row, 23, data.Ebname);
                    }
                    else { SetCellValue(row, 23, ""); }
                    if (data.Customertype != null)
                    {
                        SetCellValue(row, 24, data.Customertype);
                    }
                    else { SetCellValue(row, 24, ""); }
                    if (data.Isr != null)
                    {
                        SetCellValue(row, 25, data.Isr);
                    }
                    else { SetCellValue(row, 25, ""); }
                    if (data.Fsr != null)
                    {
                        SetCellValue(row, 26, data.Fsr);
                    }
                    else { SetCellValue(row, 26, ""); }
                    if (data.OrgId != null)
                    {
                        SetCellValue(row, 27, data.OrgId);
                    }
                    else { SetCellValue(row, 27, ""); }
                    if (data.Cmpinv != null)
                    {
                        SetCellValue(row, 28, data.Cmpinv);
                    }
                    else { SetCellValue(row, 28, ""); }
                    if (data.SalesOrder != null)
                    {
                        SetCellValue(row, 29, data.SalesOrder);
                    }
                    else { SetCellValue(row, 29, ""); }
                    if (data.Cpo != null)
                    {
                        SetCellValue(row, 30, data.Cpo);
                    }
                    else { SetCellValue(row, 30, ""); }
                    if (data.FsrNameHist != null)
                    {
                        SetCellValue(row, 31, data.FsrNameHist);
                    }
                    else { SetCellValue(row, 31, ""); }
                    if (data.isrNameHist != null)
                    {
                        SetCellValue(row, 32, data.isrNameHist);
                    }
                    else { SetCellValue(row, 32, ""); }
                    if (data.Eb != null)
                    {
                        SetCellValue(row, 33, data.Eb);
                    }
                    else { SetCellValue(row, 33, ""); }
                    if (data.LocalName != null)
                    {
                        SetCellValue(row, 34, data.LocalName);
                    }
                    else { SetCellValue(row, 34, ""); }
                    if (data.VatNo != null)
                    {
                        SetCellValue(row, 35, data.VatNo);
                    }
                    else { SetCellValue(row, 35, ""); }
                    if (data.VatDate != null)
                    {
                        SetCellValue(row, 36, data.VatDate);
                    }
                    else { SetCellValue(row, 36, ""); }
                    if (data.Collector != null)
                    {
                        SetCellValue(row, 37, data.Collector);
                    }
                    else { SetCellValue(row, 37, ""); }
                    if (data.CurrentStatus != null)
                    {
                        SetCellValue(row, 38, data.CurrentStatus);
                    }
                    else { SetCellValue(row, 38, ""); }
                    if (data.Lastupdatedate != null)
                    {
                        SetCellValue(row, 39, data.Lastupdatedate);
                    }
                    else { SetCellValue(row, 39, ""); }
                    if (data.ClearingDate != null)
                    {
                        SetCellValue(row, 40, data.ClearingDate);
                    }
                    else { SetCellValue(row, 40, ""); }
                    if (data.PtpDate != null)
                    {
                        SetCellValue(row, 41, data.PtpDate);
                    }
                    else { SetCellValue(row, 41, ""); }
                    if (data.PtpIdentifiedDate != null)
                    {
                        SetCellValue(row, 42, data.PtpIdentifiedDate);
                    }
                    else { SetCellValue(row, 42, ""); }
                    if (data.IsPartial != null)
                    {
                        SetCellValue(row, 43, data.IsPartial);
                    }
                    else { SetCellValue(row, 43, ""); }
                    if (data.PartialAmount != null)
                    {
                        SetCellValue(row, 44, data.PartialAmount);
                    }
                    else { SetCellValue(row, 44, ""); }
                    if (data.PaymentID != null)
                    {
                        SetCellValue(row, 45, data.PaymentID);
                    }
                    else { SetCellValue(row, 45, ""); }
                    if (data.Payment_Date != null)
                    {
                        SetCellValue(row, 46, data.Payment_Date);
                    }
                    else { SetCellValue(row, 46, ""); }
                    if (data.PtpDatehis != null)
                    {
                        SetCellValue(row, 47, data.PtpDatehis);
                    }
                    else { SetCellValue(row, 47, ""); }
                    if (data.PtpBroken != null)
                    {
                        SetCellValue(row, 48, data.PtpBroken);
                    }
                    else { SetCellValue(row, 48, ""); }
                    if (data.PtpComment != null)
                    {
                        SetCellValue(row, 49, data.PtpComment);
                    }
                    else { SetCellValue(row, 49, ""); }
                    if (data.Dispute != null)
                    {
                        SetCellValue(row, 50, data.Dispute);
                    }
                    else { SetCellValue(row, 50, ""); }
                    if (data.DisputeIdentifiedDate != null)
                    {
                        SetCellValue(row, 51, data.DisputeIdentifiedDate);
                    }
                    else { SetCellValue(row, 51, ""); }
                    if (data.DisputeStatus != null)
                    {
                        SetCellValue(row, 52, data.DisputeStatus);
                    }
                    else { SetCellValue(row, 52, ""); }
                    if (data.ActionOwnerDepartment != null)
                    {
                        SetCellValue(row, 53, data.ActionOwnerDepartment);
                    }
                    else { SetCellValue(row, 53, ""); }
                    if (data.ActionOwnerName != null)
                    {
                        SetCellValue(row, 54, data.ActionOwnerName);
                    }
                    else { SetCellValue(row, 54, ""); }
                    if (data.NextActionDate != null)
                    {
                        SetCellValue(row, 55, data.NextActionDate);
                    }
                    else { SetCellValue(row, 55, ""); }
                    if (data.CommentsHelpNeeded != null)
                    {
                        SetCellValue(row, 56, data.CommentsHelpNeeded);
                    }
                    else { SetCellValue(row, 56, ""); }
                    if (data.PONum != null)
                    {
                        SetCellValue(row, 57, data.PONum);
                    }
                    else { SetCellValue(row, 57, ""); }
                    if (data.SONum != null)
                    {
                        SetCellValue(row, 58, data.SONum);
                    }
                    else { SetCellValue(row, 58, ""); }
                    if (data.IsForwarder != null)
                    {
                        SetCellValue(row, 59, data.IsForwarder);
                    }
                    else { SetCellValue(row, 59, ""); }
                    if (data.Forwarder != null)
                    {
                        SetCellValue(row, 60, data.Forwarder);
                    }
                    else { SetCellValue(row, 60, ""); }

                    for (int i = 0; i <= 60; i++)
                    {
                        row.GetCell(i).CellStyle = styleCell;
                    }

                    startRowNo++;
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);

                throw ex;
            }
        }

        public void ExportCustomerStatisticDataList(List<StatisticsCollectDto> datasettings, int startRowNo)
        {
            if (datasettings == null && datasettings.Count == 0) return;
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            foreach (var data in datasettings)
            {
                var row = sheet.CreateRow(startRowNo);
                SetCellValue(row, 0, data.CustomerNum);
                SetCellValue(row, 1, data.CustomerName);
                SetCellValue(row, 2, data.SiteUseId);
                SetCellValue(row, 3, data.Collector);
                SetCellValue(row, 4, data.openAR);
                SetCellValue(row, 5, data.overDure);
                SetCellValue(row, 6, data.dispute);
                startRowNo++;
            }
        }

        public void ExportCollectorStatisticDataList(List<V_STATISTICS_COLLECTOR> datasettings, int startRowNo)
        {
            if (datasettings == null && datasettings.Count == 0) return;
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            foreach (var data in datasettings)
            {
                var row = sheet.CreateRow(startRowNo);
                SetCellValue(row, 0, data.COLLECTOR);
                SetCellValue(row, 1, data.PTPAR_PER);
                SetCellValue(row, 2, data.PTPBROKEN_PER);
                SetCellValue(row, 3, data.OVERDUEAR_PER);
                SetCellValue(row, 4, data.NOTREFUSEREPLY_PER);
                SetCellValue(row, 5, data.AR);
                SetCellValue(row, 6, data.PTPAR);
                SetCellValue(row, 7, data.PTPBROKENAR);
                SetCellValue(row, 8, data.OVERDUEAR);
                SetCellValue(row, 9, data.NOTREFUSEREPLY);
                startRowNo++;
            }
        }
        public void ExportDataList(List<V_CustomerAssessment> datasettings, int startRowNo)
        {
            if (datasettings == null && datasettings.Count == 0) return;
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            foreach (var data in datasettings)
            {
                var row = sheet.CreateRow(startRowNo);
                SetCellValue(row, 0, data.LegalEntity);
                SetCellValue(row, 1, data.CUSTOMER_NUM);
                SetCellValue(row, 2, data.CUSTOMER_NAME);
                SetCellValue(row, 3, data.SiteUseId);
                SetCellValue(row, 4, data.IsAMS);
                SetCellValue(row, 5, data.AssessmentScore);
                SetCellValue(row, 6, data.Rank);
                SetCellValue(row, 7, data.atName);
                SetCellValue(row, 8, data.CREDIT_TREM);
                SetCellValue(row, 9, data.CREDIT_LIMIT);
                SetCellValue(row, 10, data.COLLECTOR);
                startRowNo++;
            }
        }
        public void ExportDataList(List<V_CustomerAssessmentHistory> datasettings, int startRowNo)
        {
            if (datasettings == null && datasettings.Count == 0) return;
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            foreach (var data in datasettings)
            {
                var row = sheet.CreateRow(startRowNo);
                SetCellValue(row, 0, data.LegalEntity);
                SetCellValue(row, 1, data.CUSTOMER_NUM);
                SetCellValue(row, 2, data.CUSTOMER_NAME);
                SetCellValue(row, 3, data.SiteUseId);
                SetCellValue(row, 4, data.IsAMS);
                SetCellValue(row, 5, data.AssessmentScore);
                SetCellValue(row, 6, data.Rank);
                SetCellValue(row, 7, data.atName);
                SetCellValue(row, 8, data.CREDIT_TREM);
                SetCellValue(row, 9, data.CREDIT_LIMIT);
                SetCellValue(row, 10, data.COLLECTOR);
                startRowNo++;
            }
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
    }
}
