namespace Intelligent.OTC.Common.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using NPOI.HSSF.Record.Crypto;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    using NPOI.SS.Util;
    using NPOI.XSSF.UserModel;
    using System.Collections;
    using NPOI.HSSF.Util;

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


    /// <summary>
    /// Excel file Operation helper class
    /// </summary>
    public class NpoiHelper
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
            get{return this.book;}
        }

        public string FileName
        {
            get { return _FileName; }
        }

        /// <summary>
        /// Convert column index to column name 
        /// based on 1
        /// </summary>
        /// <param name="columnindex"></param>
        /// <returns></returns>
        public static string GetColumnName(int columnindex)
        {
            if (columnindex / 27 >= 1)
            {
                return string.Empty + Convert.ToChar(64 + columnindex / 27) + Convert.ToChar(64 + columnindex % 26);
            }
            return string.Empty + Convert.ToChar(64 + columnindex);
        }

        /// <summary>
        /// Initializes a new instance of the NpoiHelper class
        /// </summary>
        public NpoiHelper()
        {
            DefaultDateTimeFormater = "yyyy/MM/dd";
            this.book = new HSSFWorkbook();
        }

        /// <summary>
        /// Initializes a new instance of the NpoiHelper class
        /// </summary>
        /// <param name="filename">file name</param>
        public NpoiHelper(string filename)
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
        public NpoiHelper(string filename, string password)
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
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new Exception(string.Format("Open the file {0} failed.", filename));
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

        public void AddNewSheet(string sheetname)
        {
            if (IsExistSheetName(sheetname))
            {
                return;
            }
            this.book.CreateSheet(sheetname);
        }

        /// <summary>
        /// check sheet name existing
        /// </summary>
        /// <param name="sheetname">sheet name</param>
        /// <returns>result</returns>
        public bool IsExistSheetName(string sheetname)
        {
            return this.Sheets.Contains(sheetname);
        }

        /// <summary>
        /// convert sheet name to sheet index
        /// </summary>
        /// <param name="sheetname">sheet name</param>
        /// <returns>sheet index</returns>
        public int GetSheetIndex(string sheetname)
        {
            int index = 0, ret = -1;
            List<string> sheets = this.Sheets;
            sheets.ForEach(
                sheet =>
                {
                    if (sheet == sheetname)
                    {
                        ret = index;
                    }

                    index++;
                });
            return ret;
        }

        /// <summary>
        /// Add a new sheet into book
        /// </summary>
        /// <param name="sheetname">new sheet name</param>
        /// <param name="deleteexist">delete existing sheet</param>
        public void AddSheet(string sheetname, bool deleteexist)
        {
            if (this.IsExistSheetName(sheetname))
            {
                if (!deleteexist)
                {
                    return;
                }

                this.book.RemoveSheetAt(this.GetSheetIndex(sheetname));
            }

            this.book.CreateSheet(sheetname);
        }

        /// <summary>
        /// change sheet name
        /// </summary>
        /// <param name="oldsheetname">old sheet name</param>
        /// <param name="sheetname">new sheet name</param>
        public void ChangeSheetName(string oldsheetname, string sheetname)
        {
            int sheetindex = this.GetSheetIndex(oldsheetname);
            this.book.SetSheetName(sheetindex, sheetname);
        }

        /// <summary>
        /// remove existing sheet
        /// </summary>
        /// <param name="sheetname">sheet name</param>
        public void RemoveSheet(string sheetname)
        {
            if (this.IsExistSheetName(sheetname))
            {
                int index = this.GetSheetIndex(sheetname);
                this.book.RemoveSheetAt(index);
            }
        }

        /// <summary>
        /// remove existing sheet
        /// </summary>
        /// <param name="sheetindex">sheet index</param>
        public void RemoveSheet(int sheetindex)
        {
            if (sheetindex >= 0 && sheetindex < this.book.NumberOfSheets)
            {
                this.book.RemoveSheetAt(sheetindex);
            }
        }

        public void CopySheet(IWorkbook destbook, string newsheetname)
        {
            CopySheet(destbook, newsheetname, this.ActiveSheet);
        }
        public void CopySheet(IWorkbook destbook, string newsheetname, string sheetname)
        {
            int index = this.GetSheetIndex(sheetname);
            CopySheet(destbook, newsheetname, index);
        }
        public void CopySheet(IWorkbook destbook, string newsheetname, string sheetname, bool copystyle = true, bool keepformula = true)
        {
            int index = this.GetSheetIndex(sheetname);
            CopySheet(destbook, newsheetname, index,copystyle,keepformula);
        }
        public void CopySheet(IWorkbook destbook, string newsheetname, int sheetindex,bool copystyle=true,bool keepformula=true)
        {
            ISheet sheet = this.book.GetSheetAt(sheetindex);
            if ((sheet is HSSFSheet) && (destbook is HSSFWorkbook))
            {
                ((HSSFSheet)sheet).CopyTo((HSSFWorkbook)destbook, newsheetname, copystyle, keepformula);
            }
            else
            {
                Exception ex = new Exception("2007 format is not supported");
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// get a string value from a cell
        /// </summary>
        /// <param name="row">row index</param>
        /// <param name="column">column index</param>
        /// <returns>cell value</returns>
        public string GetStringData(int row, int column)
        {
            return GetStringData(row, column, this.ActiveSheet);
        }
        public string GetStringData(int row, int column, string sheetname)
        {
            int index = Sheets.FindIndex(name => { return name == sheetname; });
            return GetStringData(row, column, index);
        }
        public string GetStringData(int row, int column, int sheetindex)
        {
            ISheet sheet = this.book.GetSheetAt(sheetindex);
            if (sheet == null) return string.Empty;
            if (sheet.GetRow(row) == null || sheet.GetRow(row).GetCell(column) == null) return string.Empty;
            if ((sheet.GetRow(row).GetCell(column).CellType == CellType.Formula
                && sheet.GetRow(row).GetCell(column).CachedFormulaResultType == CellType.String)
                || sheet.GetRow(row).GetCell(column).CellType == CellType.String)
            {
                return sheet.GetRow(row).GetCell(column).StringCellValue;
            }
            if ((sheet.GetRow(row).GetCell(column).CellType == CellType.Formula
                 && sheet.GetRow(row).GetCell(column).CachedFormulaResultType == CellType.Numeric)
                 || sheet.GetRow(row).GetCell(column).CellType == CellType.Numeric)
            {
                return sheet.GetRow(row).GetCell(column).NumericCellValue.ToString();
            }
            if ((sheet.GetRow(row).GetCell(column).CellType == CellType.Formula
                 && sheet.GetRow(row).GetCell(column).CachedFormulaResultType == CellType.Numeric)
                 || sheet.GetRow(row).GetCell(column).CellType == CellType.Numeric)
            {
                return sheet.GetRow(row).GetCell(column).DateCellValue.ToString("yyyy/MM/dd");
            }
            return string.Empty;
        }

        /// <summary>
        /// get a number value from a cell
        /// </summary>
        /// <param name="row">row index</param>
        /// <param name="column">column index</param>
        /// <returns>cell value</returns>
        public double GetNumericData(int row, int column)
        {
            return GetNumericData(row, column, this.ActiveSheet);
        }
        public double GetNumericData(int row, int column, string sheetname)
        {
            int index = Sheets.FindIndex(name => { return name == sheetname; });
            return GetNumericData(row, column, index);
        }
        public double GetNumericData(int row, int column, int sheetindex)
        {
            ISheet sheet = this.book.GetSheetAt(sheetindex);
            if (sheet == null) return 0;
            if (sheet.GetRow(row) == null
                || sheet.GetRow(row).GetCell(column) == null) return double.NaN;
            if ((sheet.GetRow(row).GetCell(column).CellType == CellType.Formula
                && sheet.GetRow(row).GetCell(column).CachedFormulaResultType == CellType.Numeric)
                || sheet.GetRow(row).GetCell(column).CellType == CellType.Numeric)
            {
                return sheet.GetRow(row).GetCell(column).NumericCellValue;
            }
            return double.NaN;
        }

        /// <summary>
        /// Get Active Last Row Number.
        /// </summary>
        /// <returns></returns>
        public int GetLastRowNum()
        {
            try
            {
                ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
                return sheet.LastRowNum;
            }
            catch
            {
                return 0;
            }
        }

        public int GetLastRowNum(int column, int startrow)
        {
            try
            {
                ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
                int row = startrow;
                while (!string.IsNullOrEmpty(GetStringData(row, column, this.ActiveSheet)))
                {
                    row++;
                }
                row--;
                return row;
            }
            catch
            {
                return 0;
            }
        }

        public int GetLastRowNumUp(int column, int maxrow)
        {
            try
            {
                ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
                int row = maxrow;
                while (string.IsNullOrEmpty(GetStringData(row, column, this.ActiveSheet)))
                {
                    row--;
                }
                row++;
                return row;
            }
            catch
            {
                return 0;
            }
        }

        public int GetLastRowNumUp(int column, int maxrow, string sheetname)
        {
            int index = Sheets.FindIndex(name => { return name == sheetname; });
            return GetLastRowNumUp(column, maxrow, index);
        }

        public int GetLastRowNumUp(int column, int maxrow, int sheetindex)
        {
            try
            {
                ISheet sheet = this.book.GetSheetAt(sheetindex);
                int row = maxrow;
                while (string.IsNullOrEmpty(GetStringData(row, column, sheetindex)))
                {
                    row--;
                }
                row++;
                return row;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get Cell Type
        /// </summary>
        /// <param name="cRow"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        public CellType GetCellType(int cRow, int cell)
        {
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            return sheet.GetRow(cRow).GetCell(cell).CellType;
        }

        /// <summary>
        /// ForceFormulaRecalculation
        /// </summary>
        public bool ForceFormulaRecalculation(bool status)
        {
            bool ret = true;
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            if (status)
            {
                try
                {
                    HSSFFormulaEvaluator.EvaluateAllFormulaCells(this.book);
                }
                catch (Exception ex)
                {
                    Helper.Log.Error(ex.Message, ex);
                    ret = false;
                }
            }
            sheet.ForceFormulaRecalculation = status;
            return ret;
        }

        /// <summary>
        /// get a date value from a cell
        /// </summary>
        /// <param name="row">row index</param>
        /// <param name="column">column index</param>
        /// <returns>cell value</returns>
        public DateTime GetDateData(int row, int column)
        {
            return GetDateData(row, column, this.ActiveSheet);
        }
        public DateTime GetDateData(int row, int column, string sheetname)
        {
            int index = Sheets.FindIndex(name => { return name == sheetname; });
            return GetDateData(row, column, index);
        }
        public DateTime GetDateData(int row, int column, int sheetindex)
        {
            ISheet sheet = this.book.GetSheetAt(sheetindex);
            if (sheet == null) return InvalidDateTime;
            if (sheet.GetRow(row) == null || sheet.GetRow(row).GetCell(column) == null) return InvalidDateTime;
            if ((sheet.GetRow(row).GetCell(column).CellType == CellType.Formula
                && sheet.GetRow(row).GetCell(column).CachedFormulaResultType == CellType.Numeric)
                || sheet.GetRow(row).GetCell(column).CellType == CellType.Numeric)
            {
                return sheet.GetRow(row).GetCell(column).DateCellValue;
            }
            return InvalidDateTime;
        }

        public ICell GetCell(int rowNum, int colNum)
        {
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            IRow row = sheet.GetRow(rowNum);
            if (row == null)
            {
                return null;
            }
            ICell cell = row.GetCell(colNum);
            return cell;
        }

        public void ClearContents(int startrow, int endrow)
        {
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            for (int i = startrow; i <= endrow; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null)
                {
                    continue;
                }
                for (short j = row.FirstCellNum; j <= row.LastCellNum; j++)
                {
                    ICell cell = row.GetCell(j);
                    if (cell == null)
                    {
                        continue;
                    }
                    cell.SetCellValue(string.Empty);
                    //cell.SetCellFormula(string.Empty);
                }
            }
        }

        public bool TryClearCellContents(int row, int col)
        {
            try
            {
                ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
                IRow nRow = sheet.GetRow(row);
                ICell nCell = nRow.GetCell(col);
                if (nCell != null)
                    nCell.SetCellValue(string.Empty);

                return true;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                return false;
            }

        }

        public bool TryClearContents(int startrow, int endrow)
        {
            try
            {
                ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
                for (int i = startrow; i <= endrow; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null)
                    {
                        continue;
                    }
                    for (short j = row.FirstCellNum; j <= row.LastCellNum; j++)
                    {
                        ICell cell = row.GetCell(j);
                        if (cell == null)
                        {
                            continue;
                        }
                        cell.SetCellValue(string.Empty);
                        //cell.SetCellFormula(string.Empty);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                return false;
            }
            
        }

        public object GetValue(int row, int column)
        {
            return GetValue(row, column, this.ActiveSheet);
        }
        public object GetValue(int row, int column, string sheetname)
        {
            int index = Sheets.FindIndex(name => { return name == sheetname; });
            return GetValue(row, column, index);
        }
        public object GetValue(int row, int column, int sheetindex)
        {
            ISheet sheet = this.book.GetSheetAt(sheetindex);
            if (sheet == null) return null;
            if (sheet.GetRow(row) == null || sheet.GetRow(row).GetCell(column) == null) return null;
            if ((sheet.GetRow(row).GetCell(column).CellType == CellType.Formula
                && sheet.GetRow(row).GetCell(column).CachedFormulaResultType == CellType.String)
                || sheet.GetRow(row).GetCell(column).CellType == CellType.String)
            {
                return sheet.GetRow(row).GetCell(column).StringCellValue;
            }
            if ((sheet.GetRow(row).GetCell(column).CellType == CellType.Formula
                && sheet.GetRow(row).GetCell(column).CachedFormulaResultType == CellType.Numeric)
                || sheet.GetRow(row).GetCell(column).CellType == CellType.Numeric)
            {
                return sheet.GetRow(row).GetCell(column).NumericCellValue;
            }
            return null;
        }
        private void EnsureCellExist(ISheet sheet, int row, int column)
        {
            var xRow = sheet.GetRow(row);
            if (xRow == null)
            {
                this.AddRow(row, column + 1);
            }
            else
            {
                if (xRow.LastCellNum <= column && xRow.LastCellNum > 0)
                {
                    for (int i = column - xRow.LastCellNum; i < column + 1; i++)
                    {
                        if (xRow.GetCell(i) == null)
                        {
                            xRow.CreateCell(i);
                        }
                    }
                }
                else if (sheet.GetRow(row).GetCell(column) == null)
                {
                    sheet.GetRow(row).CreateCell(column);
                }
            }
        }

        /// <summary>
        /// set a cell value
        /// </summary>
        /// <param name="row">row index</param>
        /// <param name="column">column index</param>
        /// <param name="value">new value</param>
        public void SetData(int row, int column, object value)
        {
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            if (value == null || value is System.DBNull)
            {
                return;
            }
            else
            {
                EnsureCellExist(sheet, row, column);
                if (value is DateTime)
                {
                    //sheet.GetRow(row).GetCell(column).SetCellValue((DateTime)value);
                    sheet.GetRow(row).GetCell(column).SetCellValue(((DateTime)value).ToString("yyyy-MM-dd"));
                }
                else if (value is double)
                {
                    if (!double.IsNaN((double)value))
                    {
                        sheet.GetRow(row).GetCell(column).SetCellValue((double)value);
                    }
                }
                else if (value is int)
                {
                    sheet.GetRow(row).GetCell(column).SetCellValue((int)value);
                }
                else if (value is decimal)
                {
                    sheet.GetRow(row).GetCell(column).SetCellValue(Convert.ToDouble(value));
                }
                else if (value is float)
                {
                    sheet.GetRow(row).GetCell(column).SetCellValue((float)value);
                }
                else if (value is Int64)
                {
                    sheet.GetRow(row).GetCell(column).SetCellValue(Convert.ToDouble(value));
                }
                else
                {
                    sheet.GetRow(row).GetCell(column).SetCellValue(value.ToString());
                }
            }

        }

        public void SetData(int row, int column, object value, ICellStyle cellStyless)
        {
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            if (value == null || value is System.DBNull)
            {
                value = "";
            }
            EnsureCellExist(sheet, row, column);
            if (value is DateTime)
            {
                //sheet.GetRow(row).GetCell(column).SetCellValue((DateTime)value);
                sheet.GetRow(row).GetCell(column).SetCellValue(((DateTime)value).ToString("yyyy-MM-dd"));
            }
            else if (value is double)
            {
                if (!double.IsNaN((double)value))
                {
                    sheet.GetRow(row).GetCell(column).SetCellValue((double)value);
                }
            }
            else if (value is int)
            {
                sheet.GetRow(row).GetCell(column).SetCellValue((int)value);
            }
            else if (value is decimal)
            {
                sheet.GetRow(row).GetCell(column).SetCellValue(Convert.ToDouble(value));
            }
            else if (value is float)
            {
                sheet.GetRow(row).GetCell(column).SetCellValue((float)value);
            }
            else if (value is Int64)
            {
                sheet.GetRow(row).GetCell(column).SetCellValue(Convert.ToDouble(value));
            }
            else
            {
                sheet.GetRow(row).GetCell(column).SetCellValue(value.ToString());
            }
            sheet.GetRow(row).GetCell(column).CellStyle = cellStyless;
        }
        public void SetData(int row, int column, DataRow dataRow, List<ICellStyle> cellStyles, List<string> columnNames)
        {
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            if (dataRow == null)
            {
                return;
            }
            else
            {
                IRow newRow = sheet.CreateRow(row);
                for (var i = 0; i < columnNames.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(columnNames[i])) continue;
                    string name = columnNames[i].Replace("~e.", "").Replace("~", "");
                    object value = dataRow[name];

                    ICell newCell = newRow.CreateCell(column + i);
                    if (i < cellStyles.Count)
                    {
                        newCell.CellStyle = cellStyles[i];
                    }

                    if (value is DateTime)
                    {
                        newCell.SetCellValue(((DateTime)value).ToString("yyyy-MM-dd"));
                    }
                    else if (value is double)
                    {
                        if (!double.IsNaN((double)value))
                        {
                            newCell.SetCellValue((double)value);
                        }
                    }
                    else if (value is int)
                    {
                        newCell.SetCellValue((int)value);
                    }
                    else if (value is decimal)
                    {
                        newCell.SetCellValue(Convert.ToDouble(value));
                    }
                    else if (value is float)
                    {
                        newCell.SetCellValue((float)value);
                    }
                    else if (value is Int64)
                    {
                        newCell.SetCellValue(Convert.ToDouble(value));
                    }
                    else
                    {
                        newCell.SetCellValue(value.ToString());
                    }
                }
            }
        }

        public void SetLeftHeader(string headerText)
        {
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            IHeader header = sheet.Header;
            header.Left = headerText;
        }

        public void Save(string password = null)
        {
            Save(this.FileName, true, password);
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
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }
                }
                catch
                {
                    throw new Exception("The file "+filename+" exists and delete it failed.");
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

        public void WriteStream(Stream stream, string password = null)
        {
            if (this.book == null)
            {
                return;
            }
            for (int i = 0; i < this.book.NumberOfSheets; i++)
            {
                this.book.GetSheetAt(i).ForceFormulaRecalculation = true;
            }
            if (!string.IsNullOrEmpty(password))
            {
                if (this.book is HSSFWorkbook)
                {
                    (this.book as HSSFWorkbook).WriteProtectWorkbook(password, "owner");
                }
            }
            this.book.Write(stream);

            // remove the read only recommented flag when opened.
            if (!string.IsNullOrEmpty(password) && stream.CanSeek)
            {
                stream.Seek(0x2A0, SeekOrigin.Begin);
                stream.WriteByte(0);
            }
        }

        /// <summary>
        /// Export data table to excel file
        /// </summary>
        /// <param name="datasettings">export settings</param>
        /// <param name="filename">excel file</param>
        /// <param name="exportColumnTitleWhenEmpty">whether export the title of column when the data is empty</param>
        public void Export(List<ExportDataTable> datasettings, string filename, bool exportColumnTitleWhenEmpty = false)
        {
            string extension = Path.GetExtension(filename).ToLower();
            if (extension == ".xls")
            {
                this.book = new HSSFWorkbook();
            }
            else if (extension == ".xlsx")
            {
                this.book = new XSSFWorkbook();
            }

            this.ExportDataList(datasettings, exportColumnTitleWhenEmpty);
            this.Save(filename, true);
            FormaterSet.Clear();
        }

        public void RemoveCell(int rowindex, int columnindex)
        {
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            IRow row = sheet.GetRow(rowindex);
            if (row == null) return;
            ICell cell = row.GetCell(columnindex);
            if (cell == null) return;
            row.RemoveCell(cell);
        }

        public void RemoveCell(int startrow, int startcolumn, int endrow, int endcolumn)
        {
            for (int i = startrow; i <= endrow; i++)
            {
                for (int j = startcolumn; j <= endcolumn; j++)
                {
                    RemoveCell(i, j);
                }
            }
        }

        public void RemoveRow(int row)
        {
            ISheet sheet = book.GetSheetAt(this.ActiveSheet);
            sheet.RemoveRow(sheet.GetRow(row));
            for (int i = sheet.NumMergedRegions - 1; i >= 0; i--)
            {
                if (sheet.GetMergedRegion(i).FirstRow == row && sheet.GetMergedRegion(i).LastRow == row)
                {
                    sheet.RemoveMergedRegion(i);
                }
            }
            if (row + 1 <= sheet.LastRowNum)
            {
                sheet.ShiftRows(row + 1, sheet.LastRowNum, -1);
                // adjust row height
                for (int i = row; i < sheet.LastRowNum; i++)
                {
                    IRow irow = sheet.GetRow(i + 1);
                    if (irow == null)
                    {
                        continue;
                    }
                    SetRowHeight(i, irow.Height);
                }
            }
            else
            {
                sheet.CreateRow(row + 1);
                sheet.ShiftRows(row + 1, sheet.LastRowNum, -1);
            }
        }

        public void RemoveRow(int row, int count)
        {
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            if (row > sheet.LastRowNum)
                return;
            int wnCount = Math.Min(sheet.LastRowNum - row + 1, count);
            for (int i = 0; i < count; i++)
            {
                RemoveRow(row);
            }
        }

        public void AddRow(int rowindex, int columncount)
        {
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            if (rowindex <= sheet.LastRowNum)
            {
                sheet.ShiftRows(rowindex, sheet.LastRowNum, 1);
            }
            IRow row = sheet.CreateRow(rowindex);
            for (int i = 0; i < columncount; i++)
            {
                row.CreateCell(i);
            }
        }

        public void AddRow(int rowindex, bool copyOriginRow, int originRow)
        {
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            if (rowindex <= originRow)
            {
                originRow++;
            }
            if (rowindex <= sheet.LastRowNum)
            {
                sheet.ShiftRows(rowindex, sheet.LastRowNum, 1, true, false);
            }
            IRow row = sheet.CreateRow(rowindex);
            IRow origin = sheet.GetRow(originRow);
            row.HeightInPoints = origin.HeightInPoints;
            if (copyOriginRow && originRow >= 0)
            {
                for (int i = 0; i < origin.Cells.Count; i++)
                {
                    ICell cell = row.CreateCell(i);
                    cell.CellStyle = origin.Cells[i].CellStyle;
                }
            }
        }
        /// <summary>
        /// Insert rows.
        /// </summary>
        /// <param name="rowStart">start row index(based 0,)</param>
        /// <param name="rowCount"></param>
        /// <param name="rowOriginal">copy style from original row, if &lt;-1 does not copy</param>
        public void InsertRow(int rowStart, int rowCount, int rowOriginal)
        {
            if (rowCount <= 0)
                return;
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            // move rows.
            IRow wRowOriginal = null;
            if (rowOriginal >= 0)
                wRowOriginal = sheet.GetRow(rowOriginal);
            sheet.ShiftRows(rowStart, sheet.LastRowNum, rowCount);
            // create rows.
            for (int i = rowStart; i < rowStart + rowCount - 1; i++)
            {
                IRow targetRow = null;
                ICell sourceCell = null;
                ICell targetCell = null;
                targetRow = sheet.CreateRow(i + 1);
                if (wRowOriginal == null)
                    continue;
                targetRow.HeightInPoints = wRowOriginal.HeightInPoints;
                for (int m = wRowOriginal.FirstCellNum; m < wRowOriginal.LastCellNum; m++)
                {
                    sourceCell = wRowOriginal.GetCell(m);
                    if (sourceCell == null)
                        continue;
                    targetCell = targetRow.CreateCell(m);
                    //targetCell.Encoding = sourceCell.Encoding;
                    targetCell.CellStyle = sourceCell.CellStyle;
                    targetCell.SetCellType(sourceCell.CellType);
                }
            }
            //set first row style.
            if (wRowOriginal != null)
            {
                IRow targetRow = sheet.GetRow(rowStart);
                ICell sourceCell = null;
                ICell targetCell = null;
                for (int m = wRowOriginal.FirstCellNum; m < wRowOriginal.LastCellNum; m++)
                {
                    sourceCell = wRowOriginal.GetCell(m);
                    if (sourceCell == null)
                        continue;
                    targetCell = targetRow.CreateCell(m);
                    //targetCell.Encoding = sourceCell.Encoding;
                    targetCell.CellStyle = sourceCell.CellStyle;
                    targetCell.SetCellType(sourceCell.CellType);
                }
            }
        }

        /// <summary>
        /// Insert rows.
        /// </summary>
        /// <param name="rowStart">start row index(based 0,)</param>
        /// <param name="rowCount"></param>
        /// <param name="rowOriginal">copy style from original row, if &lt;-1 does not copy</param>
        public void InsertRowWithStyle(int rowindex, int rowCount, int originRow)
        {
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            if (rowindex <= originRow)
            {
                originRow++;
            }
            if (rowindex <= sheet.LastRowNum)
            {
                sheet.ShiftRows(rowindex, sheet.LastRowNum,rowCount);
            }
            for (int i = rowindex; i < rowindex + rowCount; i++)
            {
                IRow row = sheet.CreateRow(rowindex);
                IRow origin = sheet.GetRow(originRow);
                row.HeightInPoints = origin.HeightInPoints;
                if (originRow >= 0)
                {
                    for (int j = 0; j < origin.Cells.Count; j++)
                    {
                        ICell cell = row.CreateCell(j + origin.FirstCellNum);
                        cell.CellStyle = origin.Cells[j].CellStyle;
                    }
                }
            }
        }

        public List<ICellStyle> GetCellStyles(int originRow, out List<string> cellValuess)
        {
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            IRow origin = sheet.GetRow(originRow);

            List<ICellStyle> cellStyles = new List<ICellStyle>();
            cellValuess = new List<string>();
            if (originRow >= 0)
            {
                for (int j = 0; j < origin.Cells.Count; j++)
                {
                    cellStyles.Add(origin.Cells[j].CellStyle);
                    cellValuess.Add(origin.Cells[j].StringCellValue);
                }
            }
            return cellStyles;
        } 

        public short GetRowHeight(int row)
        {
            ISheet sheet = book.GetSheetAt(this.ActiveSheet);
            IRow row1 = sheet.GetRow(row);
            return row1.Height;
        }

        public void SetRowHeight(int row, short height)
        {
            ISheet sheet = book.GetSheetAt(this.ActiveSheet);
            IRow row1 = sheet.GetRow(row);
            EnsureCellExist(sheet, row, 0);
            if (row1 is HSSFRow)
            {
                (row1 as HSSFRow).Height = height;
            }
            else if (row1 is XSSFRow)
            {
                (row1 as XSSFRow).Height = height;
            }
        }

        public void SetRowHeight(int startrow, int endrow, short height)
        {
            for (int i = startrow; i <= endrow; i++)
            {
                SetRowHeight(i, height);
            }
        }

        public void CopyCell(int destrow, int destcol, int sourcerow, int sourcecol,bool copyValue=true,bool copyStyle=true)
        {
            if (copyValue)
            {
                SetData(destrow, destcol, GetValue(sourcerow, sourcecol));
            }
            if (copyStyle)
            {
                CopyCellStyle(destrow, destcol, sourcerow, sourcecol);
            }
        }

        public void CopyCellStyle(int destrow, int destcol, int sourcerow, int sourcecol)
        {
            ISheet sheet = book.GetSheetAt(this.ActiveSheet);
            if (sheet.GetRow(sourcerow) == null || sheet.GetRow(sourcerow).GetCell(sourcecol) == null)
            {
                return;
            }
            EnsureCellExist(sheet, destrow, destcol);
            sheet.GetRow(destrow).GetCell(destcol).CellStyle = sheet.GetRow(sourcerow).GetCell(sourcecol).CellStyle;
        }

        public void CopyCellStyle(int destsheet,int destrow, int destcol, int sourcesheet,int sourcerow, int sourcecol)
        {
            ISheet dsheet = book.GetSheetAt(destsheet);
            ISheet ssheet = book.GetSheetAt(sourcesheet);
            if (ssheet.GetRow(sourcerow) == null || ssheet.GetRow(sourcerow).GetCell(sourcecol) == null)
            {
                return;
            }
            EnsureCellExist(dsheet, destrow, destcol);
            dsheet.GetRow(destrow).GetCell(destcol).CellStyle = ssheet.GetRow(sourcerow).GetCell(sourcecol).CellStyle;
        }

        public void CopyStyle(int destrow, int sourcerow)
        {
            ISheet sheet = book.GetSheetAt(this.ActiveSheet);
            IRow row1 = sheet.GetRow(destrow);
            IRow row2 = sheet.GetRow(sourcerow);
            for (int i = 0; i < row1.Cells.Count; i++)
            {
                if (row2.Cells.Count > i)
                {
                    row1.Cells[i].CellStyle = row2.Cells[i].CellStyle;
                }
            }
        }

        /// <summary>
        /// Delete rows.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="count"></param>
        public void DeleteRows(int row, int count = 1)
        {
            ISheet sheet = book.GetSheetAt(this.ActiveSheet);
            int dirRows = 0 - count;
            if (row > sheet.LastRowNum)
                return;
            int wnRow = row + count;
            if (wnRow >= sheet.LastRowNum)
            {
                IRow wRowOriginal = sheet.GetRow(row);
                for (int i = 0; i < count; i++)
                {
                    IRow wRowTarget = sheet.CreateRow(wnRow + i);
                    for (int m = wRowOriginal.FirstCellNum; m < wRowOriginal.LastCellNum; m++)
                    {
                        ICell sourceCell = wRowOriginal.GetCell(m);
                        if (sourceCell == null)
                            continue;
                        wRowTarget.CreateCell(m);
                    }
                }
                // move rows.
                sheet.ShiftRows(wnRow, sheet.LastRowNum, dirRows);
                // remove merge region.
                int regions = sheet.NumMergedRegions;
                for (int i = sheet.NumMergedRegions - 1; i >= 0; i--)
                {
                    if (sheet.GetMergedRegion(i).FirstRow >= row && sheet.GetMergedRegion(i).LastRow <= wnRow)
                    {
                        sheet.RemoveMergedRegion(i);
                    }
                }
                int wnAddRows = sheet.LastRowNum - row;
                for (int i = 1; i <= wnAddRows; i++)
                {
                    sheet.ShiftRows(row + 1, sheet.LastRowNum, -1);
                }
                sheet.RemoveRow(sheet.GetRow(row));
            }
            else
            {
                sheet.ShiftRows(wnRow, sheet.LastRowNum, dirRows);
            }
        }

        /// <summary>
        /// Add merge region
        /// </summary>
        /// <param name="firstRow"></param>
        /// <param name="lastRow"></param>
        /// <param name="firstCol"></param>
        /// <param name="lastCol"></param>
        public void MergeRegion(int firstRow, int lastRow, int firstCol, int lastCol)
        {
            ISheet sheet = book.GetSheetAt(this.ActiveSheet);
            CellRangeAddress range = new CellRangeAddress(firstRow, lastRow, firstCol, lastCol);
            sheet.AddMergedRegion(range);
        }

        /// <summary>
        /// Remove merged region
        /// </summary>
        /// <param name="firstRow"></param>
        /// <param name="lastRow"></param>
        /// <param name="firstCol"></param>
        /// <param name="lastCol"></param>
        public void RemoveMergeRegion(int firstRow, int lastRow, int firstCol, int lastCol)
        {
            ISheet sheet = book.GetSheetAt(this.ActiveSheet);
            int regions = sheet.NumMergedRegions;
            for (int i = sheet.NumMergedRegions - 1; i >= 0; i--)
            {
                CellRangeAddress range = sheet.GetMergedRegion(i);
                if (range.FirstRow >= firstRow && range.LastRow <= lastRow && range.FirstColumn >= firstCol && range.LastColumn <= lastCol)
                {
                    sheet.RemoveMergedRegion(i);
                }
            }
        }
        /// <summary>
        /// copy rows.
        /// <para>if rowS &gt; rowE, copy rows down</para>
        /// <para>if rowS &lt; rowE, copy rows up</para>
        /// </summary>
        /// <param name="rowS">start row index</param>
        /// <param name="rowE">end row index</param>
        public void CopyRows(int rowS, int rowE)
        {
            ISheet sheet = book.GetSheetAt(this.ActiveSheet);
            int wnRowS = rowS;
            int wnRowE = rowE;
            int wnCount = 1;
            int wnDir = 1;  // 1:down, -1:up.
            if (rowS > rowE)
            {
                // up.
                wnDir = -1;
                wnRowS = rowE;
                wnRowE = rowS;
            }
            wnCount = wnRowE - wnRowS + 1;
            // move rows.
            if (wnDir == 1)
            {
                if (sheet.LastRowNum > (wnRowE + 1))
                    sheet.ShiftRows(wnRowE + 1, sheet.LastRowNum, wnCount);
                else
                {
                    for (int i = 1; i <= wnCount; i++)
                    {
                        sheet.CreateRow(wnRowE + i);
                    }
                }
            }
            else
                sheet.ShiftRows(wnRowS, sheet.LastRowNum, wnCount);
            // copy style.
            for (int i = 0; i < wnCount; i++)
            {
                IRow rowSrc = null;
                IRow rowDst = null;
                if (wnDir == 1)
                {
                    rowSrc = sheet.GetRow(wnRowS + i);
                    rowDst = sheet.GetRow(wnRowS + wnCount + i);
                    if (rowDst == null)
                        rowDst = sheet.CreateRow(wnRowS + wnCount + i);
                }
                else
                {
                    rowSrc = sheet.GetRow(wnRowS + wnCount + i);
                    rowDst = sheet.GetRow(wnRowS + i);
                    if (rowDst == null)
                        rowDst = sheet.CreateRow(wnRowS + i);
                }
                rowDst.HeightInPoints = rowSrc.HeightInPoints;

                ICell sourceCell = null;
                ICell targetCell = null;
                for (int m = rowSrc.FirstCellNum; m < rowSrc.LastCellNum; m++)
                {
                    sourceCell = rowSrc.GetCell(m);
                    if (sourceCell == null)
                        continue;
                    targetCell = rowDst.CreateCell(m);
                    //targetCell.Encoding = sourceCell.Encoding;
                    targetCell.CellStyle = sourceCell.CellStyle;
                    targetCell.SetCellType(sourceCell.CellType);
                }
            }
        }
        public void CopyRows(int rowS, int rowE, int targetS)
        {
            ISheet sheet = book.GetSheetAt(this.ActiveSheet);
            int wnRowS = rowS;
            int wnRowE = rowE;
            int wnCount = 1;
            int wnDir = 1;  // 1:down, -1:up.
            if (rowS > rowE)
            {
                // up.
                wnDir = -1;
                wnRowS = rowE;
                wnRowE = rowS;
            }
            wnCount = wnRowE - wnRowS + 1;
            // copy style.
            for (int i = 0; i < wnCount; i++)
            {
                IRow rowSrc = null;
                IRow rowDst = null;
                if (wnDir == 1)
                {
                    rowSrc = sheet.GetRow(wnRowS + i);
                    rowDst = sheet.GetRow(targetS + wnCount + i);
                    if (rowDst == null)
                        rowDst = sheet.CreateRow(targetS + wnCount + i);
                }
                else
                {
                    rowSrc = sheet.GetRow(wnRowS + wnCount + i);
                    rowDst = sheet.GetRow(wnRowS + i);
                    if (rowDst == null)
                        rowDst = sheet.CreateRow(targetS + i);
                }
                rowDst.HeightInPoints = rowSrc.HeightInPoints;

                ICell sourceCell = null;
                ICell targetCell = null;
                for (int m = rowSrc.FirstCellNum; m < rowSrc.LastCellNum; m++)
                {
                    sourceCell = rowSrc.GetCell(m);
                    if (sourceCell == null)
                        continue;
                    targetCell = rowDst.CreateCell(m);
                    //targetCell.Encoding = sourceCell.Encoding;
                    targetCell.CellStyle = sourceCell.CellStyle;
                    targetCell.SetCellType(sourceCell.CellType);
                    switch (sourceCell.CellType)
                    {
                        case CellType.Blank:
                            targetCell.SetCellValue(sourceCell.StringCellValue);
                            break;
                        case CellType.Boolean:
                            targetCell.SetCellValue(sourceCell.BooleanCellValue);
                            break;
                        case CellType.Error:
                            targetCell.SetCellValue(sourceCell.ErrorCellValue);
                            break;
                        case CellType.Formula:
                            targetCell.SetCellValue(sourceCell.CellFormula);
                            break;
                        case CellType.Numeric:
                            targetCell.SetCellValue(sourceCell.NumericCellValue);
                            break;
                        case CellType.String:
                            targetCell.SetCellValue(sourceCell.StringCellValue);
                            break;
                    }
                }
            }
        }
        public int VLookUp(string sheetname, object findvalue, int startColumn, int endColumn)
        {
            int index = Sheets.FindIndex(name => { return name == sheetname; });
            return VLookUp(index, findvalue, startColumn, endColumn);
        }

        public int VLookUp(int sheetindex, object findvalue, int startColumn, int endColumn, FindType findtype = FindType.WHOLE, int startRow = 0, int endRow = -1)
        {
            CellData ret = VLookUpCell(sheetindex, findvalue, startColumn, endColumn, findtype, startRow, endRow);
            return ret.Row;
        }

        public CellData VLookUpCell(int sheetindex, object findvalue, int startColumn, int endColumn, FindType findtype = FindType.WHOLE, int startRow = 0, int endRow = -1)
        {
            CellData ret = new CellData() { Row = -1, Column = -1, Value = null };
            if (findvalue is DateTime && InvalidDateTime == (DateTime)findvalue)
            {
                return ret;
            }
            ISheet sheet = this.book.GetSheetAt(sheetindex);
            int lastrow = endRow >= 0 ? endRow : sheet.LastRowNum; //this.GetLastRowNum();

            for (int row = startRow; row <= lastrow; row++)
            {
                if (sheet.GetRow(row) == null)
                {
                    continue;
                }
                int lastcol = endColumn;
                if (lastcol < 0)
                {
                    lastcol = sheet.GetRow(row).LastCellNum;
                }
                for (int col = startColumn; col <= lastcol; col++)
                {
                    if (findvalue is DateTime)
                    {
                        if (this.GetDateData(row, col, sheetindex) == (DateTime)findvalue)
                        {
                            ret = new CellData() { Row = row, Column = col, Value = this.GetDateData(row, col, sheetindex) };
                            return ret;
                        }
                    }
                    else if (findvalue is double || findvalue is int || findvalue is decimal || findvalue is float)
                    {
                        if (!double.IsNaN(this.GetNumericData(row, col, sheetindex)) && this.GetNumericData(row, col, sheetindex) == (double)findvalue)
                        {
                            ret = new CellData() { Row = row, Column = col, Value = this.GetNumericData(row, col, sheetindex) };
                            return ret;
                        }
                    }
                    else
                    {
                        if (this.GetStringData(row, col, sheetindex) == findvalue.ToString())
                        {
                            ret = new CellData() { Row = row, Column = col, Value = this.GetStringData(row, col, sheetindex) };
                            return ret;
                        }
                        else if (findtype == FindType.PART && this.GetStringData(row, col, sheetindex).IndexOf(findvalue.ToString()) >= 0)
                        {
                            ret = new CellData() { Row = row, Column = col, Value = this.GetStringData(row, col, sheetindex) };
                            return ret;
                        }
                    }
                }
            }
            return ret;
        }

        public void SetForegroundColor(int startrow, int startcol, int endrow, int endcol, short colorIndex)
        {
            for (int i = startrow; i <= endrow; i++)
            {
                for (int j = startcol; j <= endcol; j++)
                {
                    SetForegroundColor(i, j, colorIndex);
                }
            }
        }

        public void SetForegroundColor(int row, int col, short colorIndex)
        {

            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            EnsureCellExist(sheet, row, col);
            ICell cell = sheet.GetRow(row).GetCell(col);
            var style = this.book.CreateCellStyle();

            style.FillPattern = FillPattern.SolidForeground;
            style.FillForegroundColor = colorIndex;

            cell.CellStyle = style;
        }

        public void SetBorder(int row, int col, BorderType bordertype, BorderStyle style, bool newStyle = false)
        {
            NPOI.SS.UserModel.BorderStyle temp = (NPOI.SS.UserModel.BorderStyle)style;
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            EnsureCellExist(sheet, row, col);
            ICell cell = sheet.GetRow(row).GetCell(col);

            if (newStyle)
            {
                ICellStyle cellStyle = this.book.CreateCellStyle();
                if (cell.CellStyle != null)
                {
                    cellStyle.CloneStyleFrom(cell.CellStyle);
                }
                cell.CellStyle = cellStyle;
            }
            else
            {
                if (cell.CellStyle == null)
                {
                    cell.CellStyle = this.book.CreateCellStyle();
                }
            }
            if (bordertype == BorderType.ALL || bordertype == BorderType.LEFT)
            {
                cell.CellStyle.BorderLeft = temp;
            }
            if (bordertype == BorderType.ALL || bordertype == BorderType.RIGHT)
            {
                cell.CellStyle.BorderRight = temp;
            }
            if (bordertype == BorderType.ALL || bordertype == BorderType.TOP)
            {
                cell.CellStyle.BorderTop = temp;
            }
            if (bordertype == BorderType.ALL || bordertype == BorderType.BOTTOM)
            {
                cell.CellStyle.BorderBottom = temp;
            }
            if (bordertype == BorderType.BORDER_DIAGONAL_NONE)
            {
                cell.CellStyle.BorderDiagonal = BorderDiagonal.None;
            }
            if (bordertype == BorderType.BORDER_DIAGONAL_FORWARD)
            {
                cell.CellStyle.BorderDiagonal = BorderDiagonal.Forward;
                cell.CellStyle.BorderDiagonalLineStyle = temp;
            }
            if (bordertype == BorderType.BORDER_DIAGONAL_BOTH)
            {
                cell.CellStyle.BorderDiagonal = BorderDiagonal.Both;
                cell.CellStyle.BorderDiagonalLineStyle = temp;
            }
            if (bordertype == BorderType.BORDER_DIAGONAL_BACKWARD)
            {
                cell.CellStyle.BorderDiagonal = BorderDiagonal.Backward;
                cell.CellStyle.BorderDiagonalLineStyle = temp;
            }
        }
        /// <summary>
        /// create a new font for the cell
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        public void CreateNewFont(int row, int col)
        {
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            EnsureCellExist(sheet, row, col);
            ICell cell = sheet.GetRow(row).GetCell(col);
            CreateNewFont(cell);
        }
        private void CreateNewFont(ICell cell)
        {
            IFont fontNew = book.CreateFont();
            IFont fontOld = cell.CellStyle.GetFont(book);
            CopyFont(fontOld, fontNew);
            cell.CellStyle.SetFont(fontNew);
        }
        private void CopyFont(IFont fontSrc, IFont fontDst)
        {
            fontDst.Boldweight = fontSrc.Boldweight;
            fontDst.Charset = fontSrc.Charset;
            fontDst.Color = fontSrc.Color;
            fontDst.FontHeight = fontSrc.FontHeight;
            fontDst.FontHeightInPoints = fontSrc.FontHeightInPoints;
            fontDst.FontName = fontSrc.FontName;
            fontDst.IsItalic = fontSrc.IsItalic;
            fontDst.IsStrikeout = fontSrc.IsStrikeout;
            fontDst.TypeOffset = fontSrc.TypeOffset;
            fontDst.Underline = fontSrc.Underline;
        }

        /// <summary>
        /// Set Font Weight.
        /// Should call CreateNewFont before manully.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="bold"></param>
        public void SetBold(int row, int col, bool bold)
        {
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            EnsureCellExist(sheet, row, col);
            ICell cell = sheet.GetRow(row).GetCell(col);
            IFont font = cell.CellStyle.GetFont(book);
            if (bold)
                font.Boldweight = short.MaxValue;
            else
                font.Boldweight = short.MinValue;
        }

        /// <summary>
        /// Set cell dataformat
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="format"></param>
        public void SetFormat(int row, int col, string format)
        {
            SetFormat(row, col, format, this.ActiveSheet);
        }
        public void SetFormat(int row, int col, string format, string sheetname)
        {
            int index = Sheets.FindIndex(name => { return name == sheetname; });
            SetFormat(row, col, format, index);
        }

        public void SetFormat(int row, int col, string format, int sheetindex)
        {
            ISheet sheet = this.book.GetSheetAt(sheetindex);
            EnsureCellExist(sheet, row, col);
            ICell cell = sheet.GetRow(row).GetCell(col);
            ICellStyle cellStyle = this.book.CreateCellStyle();
            if (cell.CellStyle != null)
            {
                cellStyle.CloneStyleFrom(cell.CellStyle);
            }
            IDataFormat datastyle = this.book.CreateDataFormat();
            short newDataFormat = datastyle.GetFormat(format);
            cellStyle.DataFormat = newDataFormat;
            cell.CellStyle = cellStyle;
        }

        public object SetFormula(int row, int col, string formula, bool withreturn = true)
        {
            return SetFormula(row, col, formula, this.ActiveSheet, withreturn);
        }

        public object SetFormula(int row, int col, string formula, string sheetname, bool withreturn = true)
        {
            int index = Sheets.FindIndex(name => { return name == sheetname; });
            return SetFormula(row, col, formula, index, withreturn);
        }

        public object SetFormula(int row, int col, string formula, int sheetindex, bool withreturn=true)
        {
            object ret = null;
            ISheet sheet = this.book.GetSheetAt(sheetindex);
            EnsureCellExist(sheet, row, col);
            sheet.GetRow(row).GetCell(col).SetCellFormula(formula);
            if (withreturn)
            {
                try
                {
                    if (this.book is HSSFWorkbook)
                    {
                        HSSFFormulaEvaluator e = new HSSFFormulaEvaluator(this.book);

                        ICell cell = e.EvaluateInCell(sheet.GetRow(row).GetCell(col));
                        if (cell.CellType == CellType.String)
                        {
                            ret = cell.StringCellValue;
                        }
                        else if (cell.CellType == CellType.Numeric)
                        {
                            ret = cell.NumericCellValue;
                        }
                        else
                        {
                            ret = cell.StringCellValue;
                        }
                    }
                    else if (this.book is XSSFWorkbook)
                    {
                        XSSFFormulaEvaluator e = new XSSFFormulaEvaluator(this.book);

                        ICell cell = e.EvaluateInCell(sheet.GetRow(row).GetCell(col));
                        if (cell.CellType == CellType.String)
                        {
                            ret = cell.StringCellValue;
                        }
                        else if (cell.CellType == CellType.Numeric)
                        {
                            ret = cell.NumericCellValue;
                        }
                        else
                        {
                            ret = cell.StringCellValue;
                        }
                    }
                    sheet.GetRow(row).GetCell(col).SetCellFormula(formula);
                    return ret;
                }
                catch(Exception ex)
                {
                    Helper.Log.Error(ex.Message, ex);
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        public void AutoFill(int startrow, int startcol, int endrow, int endcol)
        {
            AutoFill(startrow, startcol, endrow, endcol, this.ActiveSheet);
        }

        public void AutoFill(int startrow, int startcol, int endrow, int endcol, string sheetname)
        {
            int index = Sheets.FindIndex(name => { return name == sheetname; });
            AutoFill(startrow, startcol, endrow, endcol, index);
        }

        public void AutoFill(int startrow, int startcol, int endrow, int endcol, int sheetindex)
        {
            ISheet sheet = this.book.GetSheetAt(sheetindex);
            for (int col = startcol; col <= endcol; col++)
            {
                EnsureCellExist(sheet, startrow, col);
                ICell cell = sheet.GetRow(startrow).GetCell(col);
                if (cell.CellType != CellType.Formula)
                {
                    continue;
                }
                var formula = cell.CellFormula;
                List<int> rows = new List<int>();
                var mark = false;
                var newformula = string.Empty;
                var index = 0;
                var word = string.Empty;
                for (int i = 0; i < formula.Length; i++)
                {
                    if (formula[i] == '"')
                    {
                        mark = !mark;
                    }
                    if (!string.IsNullOrEmpty(word) && formula[i] >= '0' && formula[i] <= '9')
                    {
                        var num = string.Empty;
                        while (i < formula.Length && formula[i] >= '0' && formula[i] <= '9')
                        {
                            num += formula[i];
                            i++;
                        }
                        newformula += word;
                        if (i < formula.Length)
                        {
                            if (formula[i] != '(' && !(formula[i] >= 'A' && formula[i] <= 'Z'))
                            {
                                newformula += "{" + index + "}";
                                index++;
                                rows.Add(int.Parse(num));
                            }
                            else
                            {
                                newformula += num;
                            }
                        }
                        else
                        {
                            newformula += "{" + index + "}";
                            index++;
                            rows.Add(int.Parse(num));
                        }
                        word = string.Empty;
                        num = string.Empty;
                        i--;
                        continue;
                    }
                    if (formula[i] >= 'A' && formula[i] <= 'Z')
                    {
                        word += formula[i];
                    }
                    else
                    {
                        newformula += word + formula[i];
                        word = string.Empty;
                    }
                }

                for (int row = startrow + 1; row <= endrow; row++)
                {
                    index = 0;
                    var temp = newformula;
                    rows.ForEach(data => { temp = temp.Replace("{" + index + "}", (data + row - startrow).ToString()); index++; });
                    EnsureCellExist(sheet, row, col);
                    sheet.GetRow(row).GetCell(col).SetCellFormula(temp);
                }
            }
        }

        public bool IsFormula(int row, int col)
        {
            ISheet sheet = this.book.GetSheetAt(this.ActiveSheet);
            if (sheet.GetRow(row) == null || sheet.GetRow(row).GetCell(col) == null)
            {
                return false;
            }
            return sheet.GetRow(row).GetCell(col).CellType == CellType.Formula;
        }

        public void SetProtect(string password)
        {
            SetProtect(this.ActiveSheet, password);
        }
        public void SetProtect(string sheetname, string password)
        {
            int index = Sheets.FindIndex(name => { return name == sheetname; });
            SetProtect(index, password);
        }
        public void SetProtect(int sheetindex, string password)
        {
            ISheet sheet = this.book.GetSheetAt(sheetindex);
            sheet.ProtectSheet(password);
        }

        public void UnProtect()
        {
            UnProtect(this.ActiveSheet);
        }
        public void UnProtect(string sheetname)
        {
            int index = Sheets.FindIndex(name => { return name == sheetname; });
            UnProtect(index);
        }
        public void UnProtect(int sheetindex)
        {
            SetProtect(sheetindex, null);
        }

        /// <summary>
        /// generate excel book according by Export data table
        /// </summary>
        /// <param name="datasettings">export settings</param>
        /// <param name="exportColumnTitleWhenEmpty">whether export the title of column when the data is empty</param>
        public void ExportDataList(List<ExportDataTable> datasettings, bool exportColumnTitleWhenEmpty = false)
        {
            datasettings.ForEach(
                setting =>
                {
                    ISheet sheet = book.CreateSheet(setting.Title);
                    this.ActiveSheetName = setting.Title;
                    if (setting.Source == null)
                    {
                        return;
                    }

                    if (!exportColumnTitleWhenEmpty && setting.Source.Rows.Count == 0)
                    {
                        return;
                    }

                    int rowindex = 0;
                    IRow row = sheet.CreateRow(rowindex);
                    for (int j = 0; j < setting.Source.Columns.Count; j++)
                    {
                        ICell cell = row.CreateCell(j);
                        SetData(cell.RowIndex, cell.ColumnIndex, setting.Source.Columns[j].Caption);

                        // set default header styles
                        ICellStyle style = sheet.Workbook.CreateCellStyle();
                        style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                        style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                        style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                        style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                        style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                        style.FillPattern = FillPattern.SolidForeground;
                        style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                        cell.CellStyle = style;
                        if (setting.Source.Columns[j].DataType == typeof(DateTime))
                        {
                            string formater = DefaultDateTimeFormater;
                            if (DateTimeFormater.ContainsKey(j))
                            {
                                formater = DateTimeFormater[j].ToString();
                            }
                            ICellStyle styledate = book.CreateCellStyle();
                            IDataFormat datastyle = book.CreateDataFormat();
                            styledate.DataFormat = datastyle.GetFormat(formater);
                            sheet.SetDefaultColumnStyle(j, styledate);
                            FormaterSet.Add(j);
                        }
                    }

                    rowindex++;
                    for (int i = 0; i < setting.Source.Rows.Count; i++)
                    {
                        row = sheet.CreateRow(rowindex + i);
                        for (int j = 0; j < setting.Source.Columns.Count; j++)
                        {
                            ICell cell = row.CreateCell(j);
                            SetData(cell.RowIndex, cell.ColumnIndex, setting.Source.Rows[i][j]);
                        }
                    }
                    if (setting.Group != null)
                    {
                        setting.Group.ForEach(
                            group =>
                            {
                                int startrow = 1;
                                int endrow = 1;
                                int colindex = -1;
                                string pre_value = string.Empty;
                                for (int col = 0; col < setting.Source.Columns.Count; col++)
                                {
                                    if (setting.Source.Columns[col].Caption == group)
                                    {
                                        colindex = col;
                                        break;
                                    }
                                }

                                if (colindex < 0)
                                {
                                    return;
                                }

                                for (int i = 1; i < setting.Source.Rows.Count; i++)
                                {
                                    if (GetStringData(i - 1, colindex) != GetStringData(i, colindex))
                                    {
                                        sheet.AddMergedRegion(new CellRangeAddress(startrow, endrow, colindex, colindex));
                                        startrow = endrow + 1;
                                    }

                                    endrow++;
                                }

                                if (startrow != endrow)
                                {
                                    sheet.AddMergedRegion(new CellRangeAddress(startrow, endrow, colindex, colindex));
                                }
                            });
                    }
                    int startcol = sheet.GetRow(0).FirstCellNum;
                    int endcol = sheet.GetRow(0).LastCellNum;
                    for (int i = startcol; i < endcol; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }

                    sheet.SetAutoFilter(new CellRangeAddress(0, setting.Source.Rows.Count + 1, 0, setting.Source.Columns.Count - 1));
                    sheet.CreateFreezePane(0, 1);
                });
        }

        /// <summary>
        /// Set Sheet Data Exchange to DataTable
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        private DataTable ExportToDataTable(ISheet sheet, int row)
        {
            DataTable dt = new DataTable();

            //Set head row of sheet
            IRow headRow = sheet.GetRow(row);

            //set datatable field
            for (int i = headRow.FirstCellNum, len = headRow.LastCellNum; i < len; i++)
            {
                dt.Columns.Add(headRow.Cells[i].StringCellValue);
            }
            //Traversal data row
            //for (int i = (sheet.FirstRowNum + 1), len = sheet.LastRowNum + 1; i < len; i++)
            for (int i = (row + 1), len = sheet.LastRowNum + 1; i < len; i++)
            {
                IRow tempRow = sheet.GetRow(i);
                DataRow dataRow = dt.NewRow();

                //Traversal very cell in a row
                for (int r = 0, j = tempRow.FirstCellNum, len2 = tempRow.LastCellNum; j < len2; j++, r++)
                {

                    ICell cell = tempRow.GetCell(j);

                    if (cell != null)
                    {
                        switch (cell.CellType)
                        {
                            case CellType.String:
                                dataRow[r] = cell.StringCellValue;
                                break;
                            case CellType.Numeric:
                                //only Judge date
                                if (HSSFDateUtil.IsCellDateFormatted(cell))
                                {
                                    dataRow[r] = cell.DateCellValue;
                                    break;
                                }
                                dataRow[r] = cell.NumericCellValue;
                                break;
                            case CellType.Boolean:
                                dataRow[r] = cell.BooleanCellValue;
                                break;
                            default: dataRow[r] = "ERROR";
                                break;
                        }
                    }
                }
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// Set Sheet Data Exchange to DataTable
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        private DataTable ExportToDataTableNoColName(ISheet sheet, int row)
        {
            DataTable dt = new DataTable();

            //Set head row of sheet
            IRow headRow = sheet.GetRow(row);

            //set datatable field
            for (int i = headRow.FirstCellNum, len = headRow.LastCellNum; i < len; i++)
            {
                dt.Columns.Add("Columns" + i.ToString());
            }
            //Traversal data row
            //for (int i = (sheet.FirstRowNum + 1), len = sheet.LastRowNum + 1; i < len; i++)
            for (int i = (row + 1), len = sheet.LastRowNum + 1; i < len; i++)
            {
                IRow tempRow = sheet.GetRow(i);
                DataRow dataRow = dt.NewRow();

                //Traversal very cell in a row
                for (int r = 0, j = tempRow.FirstCellNum, len2 = tempRow.LastCellNum; j < len2; j++, r++)
                {

                    ICell cell = tempRow.GetCell(j);

                    if (cell != null)
                    {



                        switch (cell.CellType)
                        {
                            case CellType.String:
                                dataRow[r] = cell.StringCellValue;
                                break;
                            case CellType.Numeric:
                                //only Judge date
                                if (HSSFDateUtil.IsCellDateFormatted(cell))
                                {
                                    dataRow[r] = cell.DateCellValue;
                                    break;
                                }
                                dataRow[r] = cell.NumericCellValue;
                                break;
                            case CellType.Boolean:
                                dataRow[r] = cell.BooleanCellValue;
                                break;

                            default: dataRow[r] = "ERROR";
                                break;
                        }
                    }
                }
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// The First Sheet Data，Conversion DataTable
        /// </summary>
        /// <returns></returns>
        public DataTable ExportExcelToDataTable(int row)
        {
            return ExportToDataTable(book.GetSheetAt(0), row);
        }

        /// <summary>
        /// The sheetIndex Count Sheet Data，Conversion DataTable
        /// </summary>
        /// <param name="sheetIndex">第几个Sheet，从1开始</param>
        /// <returns></returns>
        public DataTable ExportExcelToDataTable(int sheetIndex, int row)
        {
            return ExportToDataTable(book.GetSheetAt(sheetIndex - 1), row);
        }

        /// <summary>
        /// The First Sheet Data，Conversion DataTable
        /// </summary>
        /// <returns></returns>
        public DataTable ExportExcelToDataTableNoColName(int row)
        {
            return ExportToDataTableNoColName(book.GetSheetAt(0), row);
        }

        /// <summary>
        /// The sheetIndex Count Sheet Data，Conversion DataTable
        /// </summary>
        /// <param name="sheetIndex">第几个Sheet，从1开始</param>
        /// <returns></returns>
        public DataTable ExportExcelToDataTableNoColName(int sheetIndex, int row)
        {
            return ExportToDataTableNoColName(book.GetSheetAt(sheetIndex - 1), row);
        }

        /// <summary>
        /// lock the input file with password
        /// </summary>
        /// <param name="password">lock password</param>
        /// <param name="filename">input file name</param>
        public void LockFile(string password, string filename)
        {
            HSSFWorkbook dest = null;
            using (FileStream source = new FileStream(filename, FileMode.Open))
            {
                dest = new HSSFWorkbook(source);
            }
            dest.WriteProtectWorkbook(password, string.Empty);
            using (FileStream savefile = new FileStream(filename, FileMode.Create))
            {
                dest.Write(savefile);
            }
        }

        /// <summary>
        /// unlock the input file with password
        /// </summary>
        /// <param name="password">unlock password</param>
        /// <param name="filename">input file name</param>
        public void UnLockFile(string password, string filename)
        {
            HSSFWorkbook dest = null;
            using (FileStream source = new FileStream(filename, FileMode.Open))
            {
                Biff8EncryptionKey.CurrentUserPassword = password;
                dest = new HSSFWorkbook(source);
            }
            dest.WriteProtectWorkbook(string.Empty, string.Empty);
            using (FileStream savefile = new FileStream(filename, FileMode.Create))
            {
                dest.Write(savefile);
            }
        }
        
        /// <summary>
        /// Adjusts the column width to fit the contents.
        /// </summary>
        /// <param name="column"> the column index.</param>
        /// <param name="useMergedCells">whether to use the contents of merged cells when calculating the width of
        ///     the column. Default is to ignore merged cells.</param>
        public void AutoColumnWidth(int column, bool useMergedCells=false) 
        {
            AutoColumnWidth(column,useMergedCells,this.ActiveSheet);
        }

        private void AutoColumnWidth(int column, bool useMergedCells, int sheetindex) 
        {
            ISheet sheet = this.book.GetSheetAt(sheetindex);
            sheet.AutoSizeColumn(column, useMergedCells);
        }

        public ICellStyle GetCellStyles(int rowNumber, out object columnNames)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Settings class of export data to excel
    /// </summary>
    public class ExportDataTable
    {
        /// <summary>
        /// Gets or sets data source
        /// </summary>
        public DataTable Source { get; set; }

        /// <summary>
        /// Gets or sets table title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets group column
        /// </summary>
        public List<string> Group { get; set; }
    }
}
