using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NPOI.SS.UserModel;
using Intelligent.OTC.Common.Exceptions;
using System.Transactions;
using System.Data;
using System.Globalization;
using NPOI.HSSF.UserModel;
using System.Configuration;
using Newtonsoft.Json;
using System.Drawing;
using NPOI.XSSF.UserModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

namespace Intelligent.OTC.Business
{
    public class CaCommonService : ICaCommonService
    {
        public OTCRepository CommonRep { get; set; }
        public CaReconService CaReconService { get; set; }
        public string getCARegionByCurrentUser()
        {
            string strSQL = "select detail_Value from T_SYS_TYPE_DETAIL with (nolock) where TYPE_CODE = '081' and detail_name = '" + AppContext.Current.User.EID + "';";
            string strRegionId = SqlHelper.ExcuteScalar<String>(strSQL);
            if (strRegionId == null) { strRegionId = ""; }
            return strRegionId;
        }

        private List<CaBankStatementHeadDto> getCaBankStatementHead(string strLegalEntity, string strColumnName)
        {
            SqlParameter[] para = new SqlParameter[2];
            para[0] = new SqlParameter("@legalEntity", strLegalEntity);
            para[1] = new SqlParameter("@columnName", strColumnName);
            string strSQL = @"select ID, LegalEntity, COLUMNNAME, SORTID, FILETITLE, VALUESUM
                                from T_CA_BankStatementHead with (nolock)
                               where LegalEntity = @legalEntity and COLUMNNAME = @columnName Order by SORTID;";
            List<CaBankStatementHeadDto> listCaBankStatementHead = SqlHelper.GetList<CaBankStatementHeadDto>(SqlHelper.ExcuteTable(strSQL, System.Data.CommandType.Text, para));
            return listCaBankStatementHead;
        }

        public string UploadBankStatementFile(HttpPostedFile file, string archiveFileName)
        {
            ICaTaskService taskService = SpringFactory.GetObjectImpl<ICaTaskService>("CaTaskService");

            Random ran = new Random();

            string strFileName = Path.GetFileName(archiveFileName).Replace("'", "''");
            string strFilePath = Path.GetDirectoryName(archiveFileName);
            archiveFileName = Path.Combine(strFilePath, strFileName);

            file.SaveAs(archiveFileName);
            string tempResultFileName = saveTempResultFile(archiveFileName);
            string resultFileName = saveResultFile(archiveFileName);

            string strErrMsg = "";
            int errorRow = 0;
            int rowIndex = 0;

            bool lb_Total_HasTRANSACTION_NUMBER = false;
            bool lb_Total_HasCurrency = false;
            bool lb_Total_HasTRANSACTION_AMOUNT = false;
            bool lb_Total_HasVALUE_DATE = false;
            bool lb_Total_HasDescription = false;
            bool lb_Total_HasDescription_valuesum = false;
            List<List<string>> errSheetList = new List<List<string>>();
            string taskId = "";
            string strFileId = "";
            List<int> dataRowIndexList = new List<int>();
            List<string> bsIds = new List<string>();
            List<string> listSQL = new List<string>();

            List<Dictionary<string, bool>> errorList = new List<Dictionary<string, bool>>();
            Dictionary<string, bool> errorDic = new Dictionary<string, bool>();

            bool needCleanData = true;

            try
            {
                try
                {
                    if (File.Exists(archiveFileName))
                    {
                        //从文件名读取LegalEntity
                        string strLegalEntity = "";

                        string[] LegalEntityGroup = Path.GetFileName(archiveFileName).Split('-');
                        if (LegalEntityGroup.Length > 1)
                        {
                            strLegalEntity = LegalEntityGroup[0].Trim();
                        }
                        else
                        {
                            throw new Exception("No LegalEntity");
                        }
                        //判断LegalEntity在系统中是否存在
                        SysTypeDetail sysdetail = CommonRep.GetDbSet<SysTypeDetail>().Where(c => c.TypeCode == "087" && (c.DetailName == strLegalEntity || c.DetailValue == strLegalEntity || c.DetailValue2 == strLegalEntity)).FirstOrDefault();
                        if (null == sysdetail || string.IsNullOrEmpty(sysdetail.DetailName))
                        {
                            throw new Exception("LegalEntity is valid.");
                        }
                        else
                        {
                            strLegalEntity = sysdetail.DetailName;
                        }

                        //读取相应的列头配置
                        List<CaBankStatementHeadDto> listBSHeadTRANSACTION_NUMBER = getCaBankStatementHead(strLegalEntity, "TRANSACTION_NUMBER");//必须
                        List<CaBankStatementHeadDto> listBSHeadCurrency = getCaBankStatementHead(strLegalEntity, "Currency");//必须
                        List<CaBankStatementHeadDto> listBSHeadTRANSACTION_AMOUNT = getCaBankStatementHead(strLegalEntity, "TRANSACTION_AMOUNT");//必须
                        List<CaBankStatementHeadDto> listBSHeadVALUE_DATE = getCaBankStatementHead(strLegalEntity, "VALUE_DATE");//必须
                        List<CaBankStatementHeadDto> listBSHeadDescription = getCaBankStatementHead(strLegalEntity, "Description"); //必须
                        List<CaBankStatementHeadDto> listBSHeadCustomerNum = getCaBankStatementHead(strLegalEntity, "CustomerNum");  //非必须，有的BS上直接有CustomerNumber
                        List<CaBankStatementHeadDto> listBSHeadReceiptsMethod = getCaBankStatementHead(strLegalEntity, "ReceiptsMethod");  //非必须，ReceiptsMethod
                        List<CaBankStatementHeadDto> listBSHeadBankAccountNumber = getCaBankStatementHead(strLegalEntity, "BankAccountNumber");  //非必须，BankAccountNumber
                        List<CaBankStatementHeadDto> listBSHeadMaturityDateNumber = getCaBankStatementHead(strLegalEntity, "MaturityDate");//非必须 MaturityDate
                        List<CaBankStatementHeadDto> listBSHeadCheckNumber = getCaBankStatementHead(strLegalEntity, "CheckNumber");//非必须 CheckNumber
                        List<CaBankStatementHeadDto> listBSHeadComments = getCaBankStatementHead(strLegalEntity, "Comments");//非必填 只有884才有
                        List<CaBankStatementHeadDto> listBSHeadBSTYPE = getCaBankStatementHead(strLegalEntity, "BSTYPE");//非必填 当有承兑汇票等特殊票据的时候才会有


                        CaBankStatementHeadDto curBSHeadTRANSACTION_NUMBER = new CaBankStatementHeadDto();
                        CaBankStatementHeadDto curBSHeadDescription = new CaBankStatementHeadDto();

                        List<CaBankStatementDto> listBS = new List<CaBankStatementDto>();

                        NpoiHelper helper = new NpoiHelper(archiveFileName);
                        int sheetCount = helper.Sheets.Count();
                        for (int i_sheet = 0; i_sheet < sheetCount; i_sheet++)
                        {
                            errorDic = new Dictionary<string, bool>();
                            errorDic.Add("HasHead", false);
                            errorDic.Add("HasTRANSACTION_NUMBER", false);
                            errorDic.Add("HasCurrency", false);
                            errorDic.Add("HasTRANSACTION_AMOUNT", false);
                            errorDic.Add("HasVALUE_DATE", false);
                            errorDic.Add("HasDescription", false);

                            string[] strHeadData = new string[100];
                            string[] strHeadNameData = new string[100];
                            string[] strRowData = new string[100];

                            helper.ActiveSheet = i_sheet;
                            int maxRowNumber = helper.GetLastRowNum();  //获得数据总行数
                            int maxColNumber = 100; //每条最多100列，表结构到FED100
                            int[] intTRANSACTION_NUMBER = { -1, -1, -1 }; //TRANSACTION_NUMBER列号
                            int[] intCurrency = { -1, -1, -1 };    //Currency列号
                            int[] intTRANSACTION_AMOUNT = { -1, -1, -1 };    //TRANSACTION_AMOUNT列号
                            int[] intVALUE_DATE = { -1, -1, -1 };    //VALUE_DATE列号
                            int[] intDescription = { -1, -1, -1 };    //Description列号
                            int[] intTRANSACTION_NUMBER_ValueSum = { -1, -1, -1 };  //客户编码
                            int[] intDescription_ValueSum = { -1, -1, -1 };
                            int intCustomerNum = -1;
                            int intReceiptsMethod = -1; //ReceiptsMethod
                            int intBankAccountNumber = -1;   //BankAccountNumber
                            int intBankMaturityDate = -1;
                            int intBankCheckNumber = -1;
                            int intBankComments = -1;
                            int intbstype = -1;
                            bool lb_HasTRANSACTION_NUMBER = false;
                            bool lb_HasCurrency = false;
                            bool lb_HasTRANSACTION_AMOUNT = false;
                            bool lb_HasVALUE_DATE = false;
                            bool lb_HasDescription = false;
                            bool lb_FindHead = false;
                            DateTime dtNow = DateTime.Now;
                            int dataRowStartIndex = -1;
                            List<string> errmsgList = new List<string>();
                            for (int row = 0; row <= maxRowNumber; row++)
                            {
                                string rowerr = string.Empty;
                                errorRow = row;
                                //逐行循环
                                int emptyCellCount = 0;
                                for (int col = 0; col < maxColNumber; col++)
                                {
                                    var strValue = "";
                                    if (helper.GetCell(row, col) == null)
                                    {
                                        strValue = "";
                                        emptyCellCount++;
                                    }
                                    else
                                    {

                                        CellType ct = helper.GetCellType(row, col);
                                        switch (ct)
                                        {
                                            case CellType.Blank:
                                                strValue = "";
                                                emptyCellCount++;
                                                break;
                                            case CellType.Numeric:
                                                strValue = helper.GetValue(row, col).ToString();
                                                short format = helper.GetCell(row, col).CellStyle.DataFormat;

                                                if (format == 14 || format == 15 || format == 31 || format == 57 || format == 58 || format == 20)  //|| format == 165
                                                {
                                                    try
                                                    {
                                                        strValue = helper.GetCell(row, col).DateCellValue.ToString("yyyy-MM-dd");
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        rowerr += "," + helper.GetCell(0, col).StringCellValue + " Invalid";
                                                    }
                                                }
                                                else if (format == 164 || format == 165 || format == 166 || format == 167 || format == 168 || format == 176 || format == 256)
                                                {
                                                    if (HSSFDateUtil.IsCellDateFormatted(helper.GetCell(row, col)))//日期类型
                                                    {
                                                        strValue = helper.GetCell(row, col).DateCellValue.ToString("yyyy-MM-dd");
                                                    }
                                                    else
                                                    {
                                                        strValue = helper.GetCell(row, col).NumericCellValue.ToString();
                                                    }
                                                }
                                                else
                                                    strValue = helper.GetCell(row, col).NumericCellValue.ToString();
                                                break;
                                            case CellType.String:
                                                strValue = helper.GetCell(row, col).StringCellValue;
                                                break;
                                        }
                                    }

                                    strValue = strValue.TrimStart().TrimEnd();
                                    strHeadData[col] = strValue;
                                    //存储数据
                                    if (lb_FindHead)
                                    {
                                        strRowData[col] = strValue;
                                    }
                                    else
                                    {
                                        strHeadNameData[col] = strValue;
                                    }
                                    if (!lb_FindHead)
                                    {
                                        //判断是否列头
                                        //1.判断是否有TRANSACTION_NUMBER
                                        if (!lb_HasTRANSACTION_NUMBER)
                                        {
                                            foreach (CaBankStatementHeadDto tn in listBSHeadTRANSACTION_NUMBER)
                                            {
                                                string[] strTitle = tn.FileTitle.Split(';');
                                                int j = 0;
                                                foreach (string title in strTitle)
                                                {
                                                    if (title.Trim().ToLower() == strValue.Trim().ToLower())
                                                    {
                                                        intTRANSACTION_NUMBER[j] = col;
                                                        lb_HasTRANSACTION_NUMBER = true;
                                                        lb_Total_HasTRANSACTION_NUMBER = true;
                                                        errorDic["HasTRANSACTION_NUMBER"] = true;
                                                        curBSHeadTRANSACTION_NUMBER = tn;
                                                    }
                                                    j++;
                                                }
                                            }
                                        }

                                        //2.判断是否有Currency
                                        if (!lb_HasCurrency)
                                        {
                                            foreach (CaBankStatementHeadDto tn in listBSHeadCurrency)
                                            {
                                                string[] strTitle = tn.FileTitle.Split(';');
                                                int j = 0;
                                                foreach (string title in strTitle)
                                                {
                                                    if (title.Trim().ToLower() == strValue.Trim().ToLower())
                                                    {
                                                        intCurrency[j] = col;
                                                        lb_HasCurrency = true;
                                                        lb_Total_HasCurrency = true;
                                                        errorDic["HasCurrency"] = true;
                                                    }
                                                    j++;
                                                }
                                            }
                                        }

                                        //3.判断是否有TRANSACTION_AMOUNT
                                        if (!lb_HasTRANSACTION_AMOUNT)
                                        {
                                            foreach (CaBankStatementHeadDto tn in listBSHeadTRANSACTION_AMOUNT)
                                            {
                                                string[] strTitle = tn.FileTitle.Split(';');
                                                int j = 0;
                                                foreach (string title in strTitle)
                                                {
                                                    if (title.Trim().ToLower() == strValue.Trim().ToLower())
                                                    {
                                                        intTRANSACTION_AMOUNT[j] = col;
                                                        lb_HasTRANSACTION_AMOUNT = true;
                                                        lb_Total_HasTRANSACTION_AMOUNT = true;
                                                        errorDic["HasTRANSACTION_AMOUNT"] = true;
                                                        //curBSHeadTRANSACTION_AMOUNT = tn;
                                                    }
                                                    j++;
                                                }
                                            }
                                        }

                                        //4.判断是否有VALUE_DATE
                                        if (!lb_HasVALUE_DATE)
                                        {
                                            foreach (CaBankStatementHeadDto tn in listBSHeadVALUE_DATE)
                                            {
                                                string[] strTitle = tn.FileTitle.Split(';');
                                                int j = 0;
                                                foreach (string title in strTitle)
                                                {
                                                    if (title.Trim().ToLower() == strValue.Trim().ToLower())
                                                    {
                                                        intVALUE_DATE[j] = col;
                                                        lb_HasVALUE_DATE = true;
                                                        lb_Total_HasVALUE_DATE = true;
                                                        errorDic["HasVALUE_DATE"] = true;
                                                    }
                                                    j++;
                                                }
                                            }
                                        }

                                        //5.判断是否有Description
                                        if (!lb_HasDescription)
                                        {
                                            foreach (CaBankStatementHeadDto tn in listBSHeadDescription)
                                            {
                                                string[] strTitle = tn.FileTitle.Split(';');
                                                int j = 0;
                                                foreach (string title in strTitle)
                                                {
                                                    if (title.Trim().ToLower() == strValue.Trim().ToLower())
                                                    {
                                                        intDescription[j] = col;
                                                        lb_HasDescription = true;
                                                        lb_Total_HasDescription = true;
                                                        errorDic["HasDescription"] = true;
                                                    }
                                                    j++;
                                                }
                                            }
                                        }

                                        //如果description有value sum 需要将value sum 一起加进来
                                        if (!lb_Total_HasDescription_valuesum)
                                        {
                                            foreach (CaBankStatementHeadDto tn in listBSHeadDescription)
                                            {
                                                if (!string.IsNullOrEmpty(tn.ValueSum))
                                                {
                                                    string[] strTitle = tn.ValueSum.Split(';');
                                                    int j = 0;
                                                    foreach (string title in strTitle)
                                                    {
                                                        if (title.Trim().ToLower() == strValue.Trim().ToLower())
                                                        {
                                                            intDescription_ValueSum[j] = col;
                                                        }
                                                        j++;
                                                    }
                                                }
                                            }
                                        }

                                        //6.判断是否有CustomerNum
                                        if (intCustomerNum < 0)
                                        {
                                            foreach (CaBankStatementHeadDto tn in listBSHeadCustomerNum)
                                            {
                                                string[] strTitle = tn.FileTitle.Split(';');
                                                int j = 0;
                                                foreach (string title in strTitle)
                                                {
                                                    if (title.Trim().ToLower() == strValue.Trim().ToLower())
                                                    {
                                                        intCustomerNum = col;
                                                    }
                                                    j++;
                                                }
                                            }
                                        }

                                        //7.判断是否有ReceiptsMethod
                                        if (intReceiptsMethod < 0)
                                        {
                                            foreach (CaBankStatementHeadDto tn in listBSHeadReceiptsMethod)
                                            {
                                                string[] strTitle = tn.FileTitle.Split(';');
                                                int j = 0;
                                                foreach (string title in strTitle)
                                                {
                                                    if (title.Trim().ToLower() == strValue.Trim().ToLower())
                                                    {
                                                        intReceiptsMethod = col;
                                                    }
                                                    j++;
                                                }
                                            }
                                        }


                                        //8.判断是否有BankAccountNumber
                                        if (intBankAccountNumber < 0)
                                        {
                                            foreach (CaBankStatementHeadDto tn in listBSHeadBankAccountNumber)
                                            {
                                                string[] strTitle = tn.FileTitle.Split(';');
                                                int j = 0;
                                                foreach (string title in strTitle)
                                                {
                                                    if (title.Trim().ToLower() == strValue.Trim().ToLower())
                                                    {
                                                        intBankAccountNumber = col;
                                                    }
                                                    j++;
                                                }
                                            }
                                        }

                                        //9.判断是否有Maturity Date
                                        if (intBankMaturityDate < 0)
                                        {
                                            foreach (CaBankStatementHeadDto tn in listBSHeadMaturityDateNumber)
                                            {
                                                string[] strTitle = tn.FileTitle.Split(';');
                                                int j = 0;
                                                foreach (string title in strTitle)
                                                {
                                                    if (title.Trim().ToLower() == strValue.Trim().ToLower())
                                                    {
                                                        intBankMaturityDate = col;
                                                    }
                                                    j++;
                                                }
                                            }
                                        }

                                        //10.判断是否有CheckNumber
                                        if (intBankCheckNumber < 0)
                                        {
                                            foreach (CaBankStatementHeadDto tn in listBSHeadCheckNumber)
                                            {
                                                string[] strTitle = tn.FileTitle.Split(';');
                                                int j = 0;
                                                foreach (string title in strTitle)
                                                {
                                                    if (title.Trim().ToLower() == strValue.Trim().ToLower())
                                                    {
                                                        intBankCheckNumber = col;
                                                    }
                                                    j++;
                                                }
                                            }
                                        }

                                        //11. 判断是否有intBankComments
                                        if (intBankComments < 0)
                                        {
                                            foreach (CaBankStatementHeadDto tn in listBSHeadComments)
                                            {
                                                string[] strTitle = tn.FileTitle.Split(';');
                                                int j = 0;
                                                foreach (string title in strTitle)
                                                {
                                                    if (title.Trim().ToLower() == strValue.Trim().ToLower())
                                                    {
                                                        intBankComments = col;
                                                    }
                                                    j++;
                                                }
                                            }
                                        }

                                        //12. 判断是否有intbstype
                                        if (intbstype < 0)
                                        {
                                            foreach (CaBankStatementHeadDto tn in listBSHeadBSTYPE)
                                            {
                                                string[] strTitle = tn.FileTitle.Split(';');
                                                int j = 0;
                                                foreach (string title in strTitle)
                                                {
                                                    if (title.Trim().ToLower() == strValue.Trim().ToLower())
                                                    {
                                                        intbstype = col;
                                                    }
                                                    j++;
                                                }
                                            }
                                        }

                                    }
                                }

                                if (emptyCellCount == maxColNumber)//是空行,跳过
                                {
                                    if (lb_FindHead)
                                    {
                                        errmsgList.Add("");
                                    }

                                    continue;
                                }

                                //存储数据
                                if (lb_FindHead)
                                {
                                    CaBankStatementDto bsRow = new CaBankStatementDto();
                                    bsRow.datasheetNum = i_sheet;
                                    bsRow.dataRowNum = row;
                                    bsRow.MATCH_STATUS = "-1";
                                    //TRANSACTION_NUMBER
                                    if (intTRANSACTION_NUMBER[0] != -1)
                                    {
                                        bsRow.TRANSACTION_NUMBER = strRowData[intTRANSACTION_NUMBER[0]];
                                    }
                                    else
                                    {
                                        bsRow.TRANSACTION_NUMBER = "";
                                    }
                                    if (bsRow.TRANSACTION_NUMBER.ToUpper() == "NONREF")
                                    {
                                        bsRow.TRANSACTION_NUMBER = "";
                                    }
                                    if (String.IsNullOrEmpty(bsRow.TRANSACTION_NUMBER) && intTRANSACTION_NUMBER[1] != -1)
                                    {
                                        bsRow.TRANSACTION_NUMBER = strRowData[intTRANSACTION_NUMBER[1]];
                                    }
                                    if (String.IsNullOrEmpty(bsRow.TRANSACTION_NUMBER) && intTRANSACTION_NUMBER[2] != -1)
                                    {
                                        bsRow.TRANSACTION_NUMBER = strRowData[intTRANSACTION_NUMBER[2]];
                                    }
                                    if (intTRANSACTION_NUMBER_ValueSum[0] != -1 && strRowData[intTRANSACTION_NUMBER_ValueSum[0]] != null)
                                    {
                                        bsRow.TRANSACTION_NUMBER += strRowData[intTRANSACTION_NUMBER_ValueSum[0]];
                                    }
                                    if (intTRANSACTION_NUMBER_ValueSum[1] != -1 && strRowData[intTRANSACTION_NUMBER_ValueSum[1]] != null)
                                    {
                                        bsRow.TRANSACTION_NUMBER += strRowData[intTRANSACTION_NUMBER_ValueSum[1]];
                                    }
                                    if (intTRANSACTION_NUMBER_ValueSum[2] != -1 && strRowData[intTRANSACTION_NUMBER_ValueSum[2]] != null)
                                    {
                                        bsRow.TRANSACTION_NUMBER += strRowData[intTRANSACTION_NUMBER_ValueSum[2]];
                                    }

                                    //CURRENCY
                                    string currencyHeadName = "";
                                    try
                                    {
                                        if (intCurrency[0] != -1)
                                        {
                                            currencyHeadName = strHeadNameData[intCurrency[0]];
                                            bsRow.CURRENCY = strRowData[intCurrency[0]];
                                        }
                                        if (String.IsNullOrEmpty(bsRow.CURRENCY) && intCurrency[1] != -1)
                                        {
                                            currencyHeadName = strHeadNameData[intCurrency[1]];
                                            bsRow.CURRENCY = strRowData[intCurrency[1]];
                                        }
                                        if (String.IsNullOrEmpty(bsRow.CURRENCY) && intCurrency[2] != -1)
                                        {
                                            currencyHeadName = strHeadNameData[intCurrency[2]];
                                            bsRow.CURRENCY = strRowData[intCurrency[2]];
                                        }
                                        if (string.IsNullOrEmpty(bsRow.CURRENCY))
                                        {
                                            rowerr += ",Empty " + currencyHeadName;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        rowerr += ",Invalid " + currencyHeadName;
                                    }

                                    //TRANSACTION_AMOUNT
                                    string amountHeadName = "";
                                    try
                                    {
                                        if (intTRANSACTION_AMOUNT[0] != -1)
                                        {
                                            amountHeadName = strHeadNameData[intTRANSACTION_AMOUNT[0]];
                                            if (!string.IsNullOrEmpty(strRowData[intTRANSACTION_AMOUNT[0]]))
                                            {
                                                bsRow.TRANSACTION_AMOUNT = Convert.ToDecimal(strRowData[intTRANSACTION_AMOUNT[0]]);
                                            }

                                        }
                                        if (string.IsNullOrEmpty(strRowData[intTRANSACTION_AMOUNT[0]]) && intTRANSACTION_AMOUNT[1] != -1)
                                        {
                                            amountHeadName = strHeadNameData[intTRANSACTION_AMOUNT[1]];
                                            if (!string.IsNullOrEmpty(strRowData[intTRANSACTION_AMOUNT[1]]))
                                            {
                                                bsRow.TRANSACTION_AMOUNT = Convert.ToDecimal(strRowData[intTRANSACTION_AMOUNT[1]]);
                                            }
                                        }
                                        if (String.IsNullOrEmpty(strRowData[intTRANSACTION_AMOUNT[0]]) && intTRANSACTION_AMOUNT[1] != -1 && String.IsNullOrEmpty(strRowData[intTRANSACTION_AMOUNT[1]]) && intTRANSACTION_AMOUNT[2] != -1)
                                        {
                                            amountHeadName = strHeadNameData[intTRANSACTION_AMOUNT[2]];
                                            if (!string.IsNullOrEmpty(strRowData[intTRANSACTION_AMOUNT[2]]))
                                            {
                                                bsRow.TRANSACTION_AMOUNT = Convert.ToDecimal(strRowData[intTRANSACTION_AMOUNT[2]]);
                                            }

                                        }
                                        bsRow.CURRENT_AMOUNT = bsRow.TRANSACTION_AMOUNT;
                                        if (bsRow.TRANSACTION_AMOUNT == null)
                                        {
                                            rowerr += ",Empty " + amountHeadName;
                                        }
                                        else if (bsRow.TRANSACTION_AMOUNT <= 0)
                                        {
                                            rowerr += ",Not Positive " + amountHeadName;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        rowerr += ",Invalid " + amountHeadName;
                                    }


                                    //VALUE_DATE
                                    bool valueDateErrorFlag = true;
                                    string valueDateHeadName = "";
                                    if (intVALUE_DATE[0] != -1)
                                    {
                                        valueDateHeadName = strHeadNameData[intVALUE_DATE[0]];
                                        if (!string.IsNullOrEmpty(strRowData[intVALUE_DATE[0]]))
                                        {
                                            try
                                            {
                                                bsRow.VALUE_DATE = Convert.ToDateTime(strRowData[intVALUE_DATE[0]]);
                                            }
                                            catch (Exception e)
                                            {
                                                try
                                                {
                                                    bsRow.VALUE_DATE = string2Date(strRowData[intVALUE_DATE[0]]);
                                                }
                                                catch (Exception s2dex)
                                                {
                                                    valueDateErrorFlag = false;
                                                    rowerr += ",Invalid " + valueDateHeadName;
                                                }
                                            }
                                        }

                                    }
                                    if (intVALUE_DATE[0] != -1 && String.IsNullOrEmpty(strRowData[intVALUE_DATE[0]]) && intVALUE_DATE[1] != -1)
                                    {
                                        valueDateHeadName = strHeadNameData[intVALUE_DATE[1]];
                                        if (!string.IsNullOrEmpty(strRowData[intVALUE_DATE[1]]))
                                        {
                                            try
                                            {
                                                bsRow.VALUE_DATE = Convert.ToDateTime(strRowData[intVALUE_DATE[1]]);
                                            }
                                            catch (Exception e)
                                            {
                                                try
                                                {
                                                    bsRow.VALUE_DATE = string2Date(strRowData[intVALUE_DATE[1]]);
                                                }
                                                catch (Exception s2dex)
                                                {
                                                    valueDateErrorFlag = false;
                                                    rowerr += ",Invalid " + valueDateHeadName;
                                                }
                                            }
                                        }

                                    }
                                    if (intVALUE_DATE[0] != -1 && String.IsNullOrEmpty(strRowData[intVALUE_DATE[0]]) && intVALUE_DATE[1] != -1 && String.IsNullOrEmpty(strRowData[intVALUE_DATE[1]]) && intVALUE_DATE[2] != -1)
                                    {
                                        valueDateHeadName = strHeadNameData[intVALUE_DATE[2]];
                                        if (!string.IsNullOrEmpty(strRowData[intVALUE_DATE[2]]))
                                        {
                                            try
                                            {
                                                bsRow.VALUE_DATE = Convert.ToDateTime(strRowData[intVALUE_DATE[2]]);
                                            }
                                            catch (Exception e)
                                            {
                                                try
                                                {
                                                    bsRow.VALUE_DATE = string2Date(strRowData[intVALUE_DATE[2]]);
                                                }
                                                catch (Exception s2dex)
                                                {
                                                    valueDateErrorFlag = false;
                                                    rowerr += ",Invalid " + valueDateHeadName;
                                                }
                                            }
                                        }

                                    }
                                    if (bsRow.VALUE_DATE == null && valueDateErrorFlag)
                                    {
                                        rowerr += ",Empty " + valueDateHeadName;
                                    }


                                    //Description
                                    string descHeadName = "";
                                    try
                                    {
                                        if (intDescription[0] != -1)
                                        {
                                            descHeadName = strHeadNameData[intDescription[0]];
                                            bsRow.Description = strRowData[intDescription[0]].Trim();
                                        }
                                        if (String.IsNullOrEmpty(bsRow.Description) && intDescription[1] != -1)
                                        {
                                            descHeadName = strHeadNameData[intDescription[1]];
                                            bsRow.Description = strRowData[intDescription[1]].Trim();
                                        }
                                        if (String.IsNullOrEmpty(bsRow.Description) && intDescription[2] != -1)
                                        {
                                            descHeadName = strHeadNameData[intDescription[2]];
                                            bsRow.Description = strRowData[intDescription[2]].Trim();
                                        }
                                        if (intDescription_ValueSum[0] != -1 && strRowData[intDescription_ValueSum[0]] != null)
                                        {
                                            bsRow.Description += strRowData[intDescription_ValueSum[0]].Trim();
                                        }
                                        if (intDescription_ValueSum[1] != -1 && strRowData[intDescription_ValueSum[1]] != null)
                                        {
                                            bsRow.Description += strRowData[intDescription_ValueSum[1]].Trim();
                                        }
                                        if (intDescription_ValueSum[2] != -1 && strRowData[intDescription_ValueSum[2]] != null)
                                        {
                                            bsRow.Description += strRowData[intDescription_ValueSum[2]].Trim();
                                        }
                                        if (string.IsNullOrEmpty(bsRow.Description))
                                        {
                                            rowerr += ",Empty " + descHeadName;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        rowerr += ",Invalid " + descHeadName;
                                    }


                                    //CustomerNum(Bankstatement已经)
                                    if (intCustomerNum != -1)
                                    {
                                        bsRow.CUSTOMER_NUM = strRowData[intCustomerNum];
                                        //获得CustomerName
                                        var customerName = (from cus in CommonRep.GetQueryable<Customer>()
                                                            where cus.CustomerNum == bsRow.CUSTOMER_NUM
                                                            select cus.CustomerName).FirstOrDefault();
                                        if (!string.IsNullOrEmpty(customerName))
                                        {
                                            bsRow.CUSTOMER_NAME = customerName;
                                            bsRow.FORWARD_NUM = bsRow.CUSTOMER_NUM;
                                            bsRow.FORWARD_NAME = bsRow.CUSTOMER_NAME;
                                            bsRow.DATA_STATUS = "2";
                                            bsRow.MATCH_STATUS = "2";
                                        }
                                        else
                                        {
                                            bsRow.CUSTOMER_NAME = "";
                                            bsRow.FORWARD_NUM = "";
                                            bsRow.FORWARD_NAME = "";
                                        }

                                    }

                                    // 判断legalEntity是否属于SAP，若属于则直接置为close
                                    string sapCountSql = string.Format(@"SELECT COUNT(*) AS COUNT FROM T_SYS_TYPE_DETAIL with (nolock) WHERE TYPE_CODE='090' AND DETAIL_VALUE='{0}'", strLegalEntity);
                                    int sapCount = CommonRep.ExecuteSqlQuery<CountDto>(sapCountSql).ToList()[0].COUNT;
                                    if (sapCount > 0)
                                    {
                                        bsRow.MATCH_STATUS = "9";
                                        bsRow.CURRENT_AMOUNT = 0;
                                        needCleanData = false;
                                    }

                                    // 判断币值如果为NTD则转为TWD
                                    bsRow.CURRENCY = "NTD".Equals(bsRow.CURRENCY.TrimStart().TrimEnd().ToUpper()) ? "TWD" : bsRow.CURRENCY;

                                    //ReceiptsMethod
                                    if (intReceiptsMethod != -1)
                                    {
                                        bsRow.ReceiptsMethod = strRowData[intReceiptsMethod].Trim();
                                    }
                                    //BankAccountNumber
                                    if (intBankAccountNumber != -1)
                                    {
                                        bsRow.BankAccountNumber = strRowData[intBankAccountNumber].Trim();
                                    }

                                    //Maturity Date
                                    if (intBankMaturityDate != -1 && !String.IsNullOrEmpty(strRowData[intBankMaturityDate]))
                                    {
                                        try
                                        {
                                            bsRow.MaturityDate = Convert.ToDateTime(strRowData[intBankMaturityDate]);
                                        }
                                        catch (Exception e)
                                        {
                                            try
                                            {
                                                bsRow.MaturityDate = string2Date(strRowData[intBankMaturityDate]);
                                            }
                                            catch (Exception s2dex)
                                            {
                                                rowerr += ",Invalid " + strHeadNameData[intBankMaturityDate];
                                            }
                                        }
                                    }

                                    //Check Number
                                    if (intBankCheckNumber != -1)
                                    {
                                        bsRow.checkNumber = strRowData[intBankCheckNumber];
                                    }

                                    //Comments
                                    if (intBankComments != -1)
                                    {
                                        bsRow.Comments = strRowData[intBankComments];
                                    }

                                    //bstype
                                    if (intbstype != -1)
                                    {
                                        bsRow.BSTYPE = getBstypeByDesc(strRowData[intbstype]);
                                    }
                                    bsRow.CREATE_USER = AppContext.Current.User.EID;
                                    bsRow.CREATE_DATE = dtNow;
                                    //100列原始数据存储
                                    Type t = bsRow.GetType();
                                    for (int z = 1; z <= 100; z++)
                                    {
                                        var p = t.GetProperty("FED" + z);
                                        if (!p.PropertyType.IsGenericType)
                                        {
                                            p.SetValue(bsRow, Convert.ChangeType(strRowData[z - 1], p.PropertyType), null);
                                        }
                                        else
                                        {
                                            Type genericTypeDefinition = p.PropertyType.GetGenericTypeDefinition();
                                            if (genericTypeDefinition == typeof(Nullable<>))
                                            {
                                                p.SetValue(bsRow, Convert.ChangeType(strRowData[z - 1], Nullable.GetUnderlyingType(p.PropertyType)), null);
                                            }
                                            else
                                            {
                                                rowerr += strHeadData[z - 1] + " genericTypeDefinition";
                                            }
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(bsRow.Description) && !string.IsNullOrEmpty(bsRow.CURRENCY) && bsRow.VALUE_DATE != null && bsRow.TRANSACTION_AMOUNT != null && bsRow.TRANSACTION_AMOUNT > 0)
                                    {
                                        listBS.Add(bsRow);
                                        errmsgList.Add("OK");
                                    }
                                    else
                                    {
                                        if (rowerr.Length > 1)
                                        {
                                            rowerr = rowerr.Substring(1);
                                        }
                                        else
                                        {
                                            rowerr = "Failed";
                                        }

                                        errmsgList.Add(rowerr);
                                    }
                                    rowIndex++;
                                }
                                if (!lb_HasTRANSACTION_NUMBER || !lb_HasCurrency || !lb_HasTRANSACTION_AMOUNT || !lb_HasVALUE_DATE || !lb_HasDescription)
                                {
                                    //重置变量
                                    lb_HasTRANSACTION_NUMBER = false;
                                    lb_HasCurrency = false;
                                    lb_HasTRANSACTION_AMOUNT = false;
                                    lb_HasVALUE_DATE = false;
                                    lb_HasDescription = false;
                                    intTRANSACTION_NUMBER[0] = -1;
                                    intTRANSACTION_NUMBER[1] = -1;
                                    intTRANSACTION_NUMBER[2] = -1;
                                    intCurrency[0] = -1;
                                    intCurrency[1] = -1;
                                    intCurrency[2] = -1;
                                    intTRANSACTION_AMOUNT[0] = -1;
                                    intTRANSACTION_AMOUNT[1] = -1;
                                    intTRANSACTION_AMOUNT[2] = -1;
                                    intVALUE_DATE[0] = -1;
                                    intVALUE_DATE[1] = -1;
                                    intVALUE_DATE[2] = -1;
                                    intDescription[0] = -1;
                                    intDescription[1] = -1;
                                    intDescription[2] = -1;
                                    intCustomerNum = -1;
                                    intReceiptsMethod = -1;
                                    intBankAccountNumber = -1;
                                    intBankMaturityDate = -1;
                                    intBankCheckNumber = -1;
                                    intBankComments = -1;
                                    intbstype = -1;
                                }
                                else
                                {
                                    //标记找到了Head行，后续数据开始有效
                                    lb_FindHead = true;
                                    errorDic["HasHead"] = true;
                                    //记录Head行 行号
                                    if (dataRowStartIndex < 0)
                                    {
                                        dataRowStartIndex = row + 1;
                                    }


                                    #region 判断是否有累加多列值的字段
                                    if (curBSHeadTRANSACTION_NUMBER.ValueSum != null)
                                    {
                                        string[] strTitle_BSHeadTRANSACTION_NUMBER = curBSHeadTRANSACTION_NUMBER.ValueSum.Split('+');
                                        int j = 0;
                                        foreach (string title in strTitle_BSHeadTRANSACTION_NUMBER)
                                        {
                                            for (int zz = 0; zz < 100; zz++)
                                            {
                                                if (title == strHeadData[zz])
                                                {
                                                    intTRANSACTION_NUMBER_ValueSum[j] = zz + 1;
                                                }
                                            }
                                            j++;
                                        }
                                    }
                                    if (curBSHeadDescription.ValueSum != null)
                                    {
                                        string[] strTitle_BSHeadDescription = curBSHeadDescription.ValueSum.Split('+');
                                        int jjjjj = 0;
                                        foreach (string title in strTitle_BSHeadDescription)
                                        {
                                            for (int zz = 0; zz < 100; zz++)
                                            {
                                                if (title == strHeadData[zz])
                                                {
                                                    intDescription_ValueSum[jjjjj] = zz + 1;
                                                }
                                            }
                                            jjjjj++;
                                        }
                                    }
                                    #endregion

                                    //清空临时存储的头信息
                                    strHeadData = new string[100];
                                }
                            }
                            dataRowIndexList.Add(dataRowStartIndex);
                            errSheetList.Add(errmsgList);

                            errorList.Add(errorDic);
                        }


                        if (listBS.Count > 0)
                        {
                            int sortId = 1;
                            var rvNumMap = new Dictionary<string, string>();
                            #region
                            foreach (CaBankStatementDto bs in listBS)
                            {
                                //判断是否已经存在相同日期，金额，币制，Description的数据，如果已经存在，则跳过写入数据库，并加入弹出消息
                                bs.LegalEntity = strLegalEntity;

                                if (!rvNumMap.ContainsKey(bs.TRANSACTION_NUMBER))
                                {
                                    rvNumMap.Add(bs.TRANSACTION_NUMBER, bs.TRANSACTION_NUMBER);
                                }
                                else
                                {
                                    int dataSheetNum = bs.datasheetNum ?? 0;
                                    int dataRowNum = bs.dataRowNum ?? 0;
                                    dataRowNum = dataRowNum - dataRowIndexList[dataSheetNum];
                                    errSheetList[dataSheetNum][dataRowNum] = "Duplicate Transaction Number In Excel";
                                    continue;
                                }

                                string duplitRVNumCheckSQL = "select count(*) from T_CA_BankStatement with (nolock) where TRANSACTION_NUMBER='" + bs.TRANSACTION_NUMBER + "' and DEL_FLAG = 0";
                                int duplitRVNumCount = Int32.Parse(SqlHelper.ExecuteScalar("", CommandType.Text, duplitRVNumCheckSQL, null).ToString());
                                if (duplitRVNumCount > 0)
                                {
                                    //strErrMsg += "LegalEntity:" + bs.LegalEntity + ", ValueDate:" + bs.VALUE_DATE + ",Currency:" + bs.CURRENCY + ",Amount:" + bs.TRANSACTION_AMOUNT + "\r\n";
                                    int dataSheetNum = bs.datasheetNum ?? 0;
                                    int dataRowNum = bs.dataRowNum ?? 0;
                                    dataRowNum = dataRowNum - dataRowIndexList[dataSheetNum];
                                    errSheetList[dataSheetNum][dataRowNum] = "Duplicate Transaction Number";
                                    continue;
                                }

                                string duplitCheckSQL = "select count(*) from T_CA_BankStatement with (nolock) where DEL_FLAG = 0 and LegalEntity = '" + bs.LegalEntity + "' AND VALUE_DATE = '" + bs.VALUE_DATE + "' and CURRENCY = '" + bs.CURRENCY + "' and TRANSACTION_AMOUNT = " + bs.TRANSACTION_AMOUNT + " and Description = N'" + bs.Description.Replace("'", "''") + "'";
                                int duplitCount = Int32.Parse(SqlHelper.ExecuteScalar("", CommandType.Text, duplitCheckSQL, null).ToString());
                                if (duplitCount > 0)
                                {
                                    //strErrMsg += "LegalEntity:" + bs.LegalEntity + ", ValueDate:" + bs.VALUE_DATE + ",Currency:" + bs.CURRENCY + ",Amount:" + bs.TRANSACTION_AMOUNT + "\r\n";
                                    int dataSheetNum = bs.datasheetNum ?? 0;
                                    int dataRowNum = bs.dataRowNum ?? 0;
                                    dataRowNum = dataRowNum - dataRowIndexList[dataSheetNum];
                                    errSheetList[dataSheetNum][dataRowNum] = "Duplicate data";
                                    continue;
                                }
                                string bsId = Guid.NewGuid().ToString();
                                if (string.IsNullOrEmpty(bs.TRANSACTION_NUMBER))
                                {
                                    bs.TRANSACTION_NUMBER = getKey("BS");
                                }
                                if (string.IsNullOrEmpty(bs.ReceiptsMethod))
                                {
                                    bs.ReceiptsMethod = "";
                                }
                                if (string.IsNullOrEmpty(bs.BankAccountNumber))
                                {
                                    bs.BankAccountNumber = "";
                                }
                                if (string.IsNullOrEmpty(bs.Description))
                                {
                                    bs.Description = "";
                                }
                                if (string.IsNullOrEmpty(bs.checkNumber))
                                {
                                    bs.checkNumber = "";
                                }
                                if (string.IsNullOrEmpty(bs.Comments))
                                {
                                    bs.Comments = "";
                                }
                                if (string.IsNullOrEmpty(bs.BSTYPE))
                                {
                                    bs.BSTYPE = "GE";
                                }

                                StringBuilder sql = new StringBuilder();
                                //sql.Append("IF NOT EXISTS (SELECT 1 FROM T_CA_BankStatement with (nolock) WHERE TRANSACTION_NUMBER = '" + bs.TRANSACTION_NUMBER + "' AND DEL_FLAG = 0)");
                                sql.Append("INSERT INTO T_CA_BankStatement (");
                                sql.Append("ID");
                                sql.Append(",LegalEntity");
                                sql.Append(",BSTYPE");
                                sql.Append(",REGION");
                                sql.Append(",SortId");
                                sql.Append(",TRANSACTION_NUMBER");
                                sql.Append(",TRANSACTION_AMOUNT");
                                sql.Append(",CURRENT_AMOUNT");
                                sql.Append(",VALUE_DATE");
                                sql.Append(",CURRENCY");
                                sql.Append(",Description");
                                sql.Append(",ReceiptsMethod");
                                sql.Append(",BankAccountNumber");
                                if (bs.MaturityDate != null)
                                {
                                    sql.Append(",MaturityDate");
                                }
                                sql.Append(",CheckNumber");
                                sql.Append(",Comments");

                                if (!string.IsNullOrEmpty(bs.CUSTOMER_NUM))
                                {
                                    sql.Append(",CUSTOMER_NUM");
                                }
                                if (!string.IsNullOrEmpty(bs.CUSTOMER_NAME))
                                {
                                    sql.Append(",CUSTOMER_NAME");
                                    sql.Append(",FORWARD_NUM");
                                    sql.Append(",FORWARD_NAME");
                                    sql.Append(",DATA_STATUS");
                                }
                                sql.Append(",MATCH_STATUS");
                                sql.Append(",CREATE_USER");
                                sql.Append(",CREATE_DATE");
                                sql.Append(",FED1,FED2,FED3,FED4,FED5,FED6,FED7,FED8,FED9,FED10");
                                sql.Append(",FED11,FED12,FED13,FED14,FED15,FED16,FED17,FED18,FED19,FED20");
                                sql.Append(",FED21,FED22,FED23,FED24,FED25,FED26,FED27,FED28,FED29,FED30");
                                sql.Append(",FED31,FED32,FED33,FED34,FED35,FED36,FED37,FED38,FED39,FED40");
                                sql.Append(",FED41,FED42,FED43,FED44,FED45,FED46,FED47,FED48,FED49,FED50");
                                sql.Append(",FED51,FED52,FED53,FED54,FED55,FED56,FED57,FED58,FED59,FED60");
                                sql.Append(",FED61,FED62,FED63,FED64,FED65,FED66,FED67,FED68,FED69,FED70");
                                sql.Append(",FED71,FED72,FED73,FED74,FED75,FED76,FED77,FED78,FED79,FED80");
                                sql.Append(",FED81,FED82,FED83,FED84,FED85,FED86,FED87,FED88,FED89,FED90");
                                sql.Append(",FED91,FED92,FED93,FED94,FED95,FED96,FED97,FED98,FED99,FED100");
                                sql.Append(" ) ");
                                sql.Append(" SELECT '" + bsId + "',");
                                sql.Append("'" + bs.LegalEntity + "',");
                                sql.Append("'" + bs.BSTYPE + "',");
                                sql.Append("'',");
                                sql.Append(sortId.ToString() + ",");
                                sql.Append("'" + bs.TRANSACTION_NUMBER + "',");
                                sql.Append(bs.TRANSACTION_AMOUNT.ToString() + ",");
                                sql.Append(bs.CURRENT_AMOUNT.ToString() + ",");
                                sql.Append("'" + bs.VALUE_DATE.ToString() + "',");
                                sql.Append("'" + bs.CURRENCY + "',");
                                sql.Append("N'" + bs.Description.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.ReceiptsMethod.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.BankAccountNumber.Replace("'", "''") + "',");
                                if (bs.MaturityDate != null)
                                {
                                    sql.Append("'" + bs.MaturityDate + "',");
                                }
                                sql.Append("N'" + bs.checkNumber.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.Comments.Replace("'", "''") + "',");
                                if (!string.IsNullOrEmpty(bs.CUSTOMER_NUM))
                                {
                                    sql.Append("N'" + bs.CUSTOMER_NUM + "',");
                                }
                                if (!string.IsNullOrEmpty(bs.CUSTOMER_NAME))
                                {
                                    sql.Append("N'" + bs.CUSTOMER_NAME.Replace("'", "''") + "',");
                                    sql.Append("N'" + bs.FORWARD_NUM + "',");
                                    sql.Append("N'" + bs.FORWARD_NAME.Replace("'", "''") + "',");
                                    sql.Append("'" + bs.DATA_STATUS + "',");
                                }
                                else
                                {
                                    bs.CUSTOMER_NAME = "";
                                }
                                sql.Append("'" + bs.MATCH_STATUS + "',");
                                sql.Append("'" + bs.CREATE_USER + "',");
                                sql.Append("'" + bs.CREATE_DATE.ToString() + "',");
                                sql.Append("N'" + bs.FED1.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED2.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED3.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED4.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED5.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED6.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED7.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED8.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED9.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED10.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED11.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED12.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED13.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED14.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED15.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED16.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED17.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED18.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED19.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED20.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED21.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED22.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED23.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED24.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED25.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED26.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED27.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED28.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED29.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED30.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED31.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED32.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED33.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED34.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED35.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED36.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED37.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED38.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED39.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED40.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED41.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED42.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED43.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED44.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED45.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED46.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED47.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED48.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED49.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED50.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED51.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED52.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED53.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED54.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED55.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED56.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED57.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED58.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED59.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED60.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED61.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED62.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED63.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED64.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED65.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED66.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED67.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED68.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED69.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED70.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED71.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED72.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED73.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED74.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED75.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED76.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED77.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED78.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED79.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED80.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED81.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED82.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED83.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED84.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED85.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED86.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED87.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED88.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED89.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED90.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED91.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED92.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED93.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED94.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED95.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED96.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED97.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED98.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED99.Replace("'", "''") + "',");
                                sql.Append("N'" + bs.FED100.Replace("'", "''") + "'");
                                sql.Append(" WHERE NOT EXISTS (SELECT 1 FROM T_CA_BankStatement with (nolock) WHERE TRANSACTION_NUMBER = '" + bs.TRANSACTION_NUMBER + "' AND DEL_FLAG = 0)");
                                listSQL.Add(sql.ToString());
                                if (!string.IsNullOrEmpty(bs.CUSTOMER_NUM))
                                {
                                    //插入customer identify表
                                    string insertCustomerIdentifysql = string.Format(@"INSERT INTO T_CA_CustomerIdentify (
	                                                                                    ID,
	                                                                                    BANK_STATEMENT_ID,
	                                                                                    SortId,
	                                                                                    CUSTOMER_NUM,
	                                                                                    CUSTOMER_NAME,
	                                                                                    NeedSendMail,
	                                                                                    CREATE_USER,
	                                                                                    CREATE_DATE
                                                                                    )
                                                                                    VALUES
	                                                                                    ('{0}', '{1}', 1, '{2}', N'{3}', 0, '{4}', '{5}')",
                                                                                        Guid.NewGuid().ToString(), bsId,
                                                                                        bs.CUSTOMER_NUM, bs.CUSTOMER_NAME.Replace("'", "''"),
                                                                                        bs.CREATE_USER, bs.CREATE_DATE.ToString());
                                    listSQL.Add(insertCustomerIdentifysql);
                                }
                                bsIds.Add(bsId);
                                sortId++;
                            }

                            #endregion

                            strFileId = Guid.NewGuid().ToString();
                            //保存文件记录
                            StringBuilder sqlFile = new StringBuilder();
                            sqlFile.Append("INSERT INTO T_FILE (FILE_ID,FILE_NAME,PHYSICAL_PATH,OPERATOR,CREATE_TIME)");
                            sqlFile.Append(" VALUES (N'" + strFileId + "',");
                            sqlFile.Append("         N'" + strFileName + "',");
                            sqlFile.Append("         N'" + archiveFileName.Replace("'", "''") + "',");
                            sqlFile.Append("         N'" + AppContext.Current.User.EID + "',GETDATE());");
                            listSQL.Add(sqlFile.ToString());

                            if (!string.IsNullOrEmpty(strErrMsg))
                            {
                                strErrMsg = "The following data already exists in the system and has been skipped:\r\n" + strErrMsg;
                            }

                        }

                    }
                }
                catch (Exception ex)
                {
                    Helper.Log.Error("**************************************888 " + ex.Message, ex);
                    strErrMsg = ex.Message;
                }

                string taskStatus = "2";
                string resultFileId = Guid.NewGuid().ToString();
                try
                {
                    //保存文件记录
                    StringBuilder sqlFile = new StringBuilder();

                    string strResultFileName = Path.GetFileName(resultFileName).Replace("'", "''");
                    sqlFile.Append("INSERT INTO T_FILE (FILE_ID,FILE_NAME,PHYSICAL_PATH,OPERATOR,CREATE_TIME)");
                    sqlFile.Append(" VALUES (N'" + resultFileId + "',");
                    sqlFile.Append("         N'" + strResultFileName + "',");
                    sqlFile.Append("         N'" + resultFileName.Replace("'", "''") + "',");
                    sqlFile.Append("         N'" + AppContext.Current.User.EID + "',GETDATE());");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(sqlFile.ToString());

                    using (FileStream fs = new FileStream(tempResultFileName, FileMode.Open, FileAccess.ReadWrite))
                    {
                        IWorkbook wk = WorkbookFactory.Create(fs);//使用接口，自动识别excel2003/2007格式
                        ICellStyle higtLightStyle = wk.CreateCellStyle();
                        IFont font = wk.CreateFont();
                        font.Color = NPOI.HSSF.Util.HSSFColor.Red.Index;
                        higtLightStyle.SetFont(font);

                        ICellStyle headStyle = wk.CreateCellStyle();
                        IFont bf = wk.CreateFont();
                        bf.Boldweight = (short)FontBoldWeight.Bold;
                        headStyle.SetFont(bf);

                        int sheetCount = wk.NumberOfSheets;
                        for (int i_sheet = 0; i_sheet < sheetCount; i_sheet++)
                        {
                            ISheet sheet = wk.GetSheetAt(i_sheet);
                            if (wk.IsSheetHidden(i_sheet))
                            {
                                continue;
                            }
                            int rownum = sheet.LastRowNum;
                            if (dataRowIndexList.Count == 0) { break; }
                            int dataRow = dataRowIndexList[i_sheet];
                            if ((rownum >= dataRow && dataRow >= 1) || errorList[i_sheet]["HasHead"])
                            {
                                List<string> errmsgList = errSheetList[i_sheet];
                                if (errmsgList == null || errmsgList.Count() <= 0)
                                {
                                    strErrMsg += "There's no available data in Excel at sheet " + (i_sheet + 1) + ";\r\n";
                                    continue;
                                }
                                if (dataRow < 1 || sheet.GetRow(dataRow - 1) == null)
                                {
                                    continue;
                                }

                                sheet = insertColumns(sheet, dataRow - 1);

                                ICell headCell = sheet.GetRow(dataRow - 1).GetCell(0);
                                if (headCell == null)
                                {
                                    sheet.GetRow(dataRow - 1).CreateCell(0);
                                    headCell = sheet.GetRow(dataRow - 1).GetCell(0);
                                }

                                headCell.SetCellValue("Status");
                                headCell.CellStyle = headStyle;

                                rownum = rownum + 1;
                                int i = dataRow;

                                foreach (string str in errmsgList)
                                {
                                    if (sheet.GetRow(i) == null)
                                    {
                                        i++;
                                        continue;
                                    }
                                    ICell cell = sheet.GetRow(i).GetCell(0);
                                    if (cell == null)
                                    {
                                        sheet.GetRow(i).CreateCell(0);
                                        cell = sheet.GetRow(i).GetCell(0);
                                    }
                                    if (string.IsNullOrEmpty(str))
                                    {
                                        i++;
                                        continue;
                                    }
                                    cell.SetCellValue(str);

                                    if (!string.Equals(str, "OK"))
                                    {
                                        cell.CellStyle = higtLightStyle;
                                        taskStatus = "4";
                                    }
                                    i++;
                                }
                            }
                            else
                            {
                                //把标题错误的报错信息放在写excel的这个地方，原来那个位置，如果bslist不为零的话，会跳过这块，但是如果是多sheet页，会漏掉报错信息
                                string str = "";
                                if (!errorList[i_sheet]["HasTRANSACTION_NUMBER"])
                                {
                                    str += "No Transaction Number, ";
                                }
                                if (!errorList[i_sheet]["HasCurrency"])
                                {
                                    str += "No Currency, ";
                                }
                                if (!errorList[i_sheet]["HasTRANSACTION_AMOUNT"])
                                {
                                    str += "No Transaction Amount, ";
                                }
                                if (!errorList[i_sheet]["HasVALUE_DATE"])
                                {
                                    str += "No Value Date, ";
                                }
                                if (!errorList[i_sheet]["HasDescription"])
                                {
                                    str += "No Description, ";
                                }
                                if (string.IsNullOrEmpty(str))
                                {
                                    strErrMsg += "There's no available data in Excel at sheet " + (i_sheet + 1) + ";\r\n";
                                }
                                else
                                {
                                    strErrMsg += "Match header failed: " + str + "at sheet " + (i_sheet + 1) + ", please contact the admin to resolve it!\r\n";
                                }

                            }

                        }
                        FileStream fos = null;
                        fos = new FileStream(resultFileName, FileMode.Create, FileAccess.Write);
                        wk.Write(fos);
                        fos.Close();
                    }
                }
                catch (Exception wbe)
                {
                    Helper.Log.Error(wbe.Message, wbe);
                    strErrMsg = "Error in reading and writing receipt file. \nError message:" + wbe.Message;
                }

                SqlHelper.ExcuteListSql(listSQL);
                //生成Task
                DateTime now = AppContext.Current.User.Now;
                CaTaskMsg msg = new CaTaskMsg();
                taskId = taskService.createTask(1, bsIds.ToArray(), strFileName, strFileId, now);
                msg.taskId = taskId;

                if (needCleanData && lb_Total_HasTRANSACTION_NUMBER && lb_Total_HasCurrency && lb_Total_HasTRANSACTION_AMOUNT && lb_Total_HasVALUE_DATE && lb_Total_HasDescription)
                {
                    if (CaReconService.cleanData(msg) != "success")
                    {
                        // 抛出提示信息
                        throw new OTCServiceException("Clean Data failed!");
                    }
                }

                if (string.IsNullOrEmpty(strErrMsg))
                {
                    if (string.Equals(taskStatus, "4"))
                    {
                        strErrMsg = "Upload completed with error, please get details from the Result File.";
                    }
                    else
                    {
                        strErrMsg = "Upload success.";
                        taskStatus = "2";
                    }
                }
                else
                {
                    taskStatus = "4";
                }
                string updateFileIdSql = string.Format(@"UPDATE T_CA_TASK SET 
                                                        FILE_ID = FILE_ID + ';{0}',
                                                        STATUS = '{2}'
                                                        WHERE ID = '{1}'", resultFileId, taskId, taskStatus);
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(updateFileIdSql);
            }
            catch (Exception e111)
            {
                if (!string.IsNullOrEmpty(taskId))
                {
                    taskService.updateTaskStatusById(taskId, "3");
                    deleteBankStatementByTaskId(taskId);
                }
                throw new Exception(e111.Message);
            }

            return strErrMsg;
        }

        private ISheet insertColumns(ISheet sheet, int startIndex)
        {
            int rowNum = sheet.LastRowNum;
            int colNum = 0;
            if (sheet.GetRow(startIndex) == null)
            {
                return sheet;
            }

            for (; colNum < 100; colNum++)
            {
                if (sheet.GetRow(startIndex).GetCell(colNum) == null)
                {
                    break;
                }
            }

            for (int n = startIndex; n <= rowNum; n++)
            {
                IRow row = sheet.GetRow(n);
                if (row == null || row.GetCell(0) == null)
                {
                    continue;
                }
                for (int i = colNum; i >= 0; i--)
                {
                    if (row.GetCell(i + 1) != null)
                    {
                        row.RemoveCell(row.GetCell(i + 1));
                    }

                    //解决出现公式cell无法复制的问题
                    if (row.GetCell(i) != null && row.GetCell(i).CellType.Equals(CellType.Formula))
                    {
                        if (row.GetCell(i + 1) == null)
                        {
                            row.CreateCell(i + 1);
                        }
                        row.GetCell(i + 1).SetCellValue(row.GetCell(i).ToString());
                    }
                    else
                    {
                        row.CopyCell(i, i + 1);
                    }
                }
            }
            return sheet;
        }

        private DateTime? string2Date(string strdate)
        {
            DateTime? dt_date = null;
            try
            {
                DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo();
                dtFormat.ShortDatePattern = "MM/dd/yyyy";
                dt_date = Convert.ToDateTime(strdate, dtFormat);
            }
            catch (Exception e1)
            {
                try
                {
                    DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo();
                    dtFormat.ShortDatePattern = "dd/MM/yyyy";
                    dt_date = Convert.ToDateTime(strdate, dtFormat);
                }
                catch (Exception e2)
                {

                }
            }
            return dt_date;
        }

        public string CheckAndsavePMT(CaPMTDto caPMTDto)
        {
            string str = string.Empty;
            int bslistCount = caPMTDto.PmtBs.Count();
            int detailListCount = caPMTDto.PmtDetail.Count();

            if (caPMTDto.ReceiveDate == null)
            {
                str = "Recevice date can't null";
                return str;
            }

            if (bslistCount <= 0 && detailListCount <= 0)
            {
                str = doCheckPmt(caPMTDto);
            }
            else if (bslistCount > 0 && detailListCount <= 0)//主表+BS
            {
                str = doCheckPmtAndBS(caPMTDto);
            }
            else if (detailListCount > 0 && bslistCount <= 0)//主表+Detail
            {
                str = doCheckPmtAndDetail(caPMTDto, false, "");
            }
            else if (bslistCount > 0 && detailListCount > 0)//主表+BS+Detail
            {
                str = doCheckPmtAll(caPMTDto, false, "");
            }

            if (string.IsNullOrEmpty(str))
            {
                str = "OK";
            }
            return str;
        }

        public string UploadRemittance(HttpPostedFile file, string archiveFileName, string uploadFileName)
        {
            string strPMTFileName = "";
            string strSaveFileName = Path.GetFileName(archiveFileName);
            string strFileName = Guid.NewGuid() + "_" + Path.GetFileName(archiveFileName);
            string strFilePath = Path.GetDirectoryName(archiveFileName);
            archiveFileName = Path.Combine(strFilePath, strFileName);
            file.SaveAs(archiveFileName);
            string strErrMsg = "";
            try
            {
                if (File.Exists(archiveFileName))
                {
                    //保存文件记录
                    string strFileId = Guid.NewGuid().ToString();
                    StringBuilder sqlFile = new StringBuilder();
                    sqlFile.Append("INSERT INTO T_FILE (FILE_ID,FILE_NAME,PHYSICAL_PATH,OPERATOR,CREATE_TIME)");
                    sqlFile.Append(" VALUES (N'" + strFileId + "',");
                    sqlFile.Append("         N'" + strSaveFileName + "',");
                    sqlFile.Append("         N'" + archiveFileName + "',");
                    sqlFile.Append("         N'" + AppContext.Current.User.EID + "',GETDATE());");
                    SqlHelper.ExcuteSql(sqlFile.ToString());
                    FileDto fileDto = new FileDto();
                    fileDto.FileId = strFileId;
                    fileDto.FileName = strFileName;
                    fileDto.PhysicalPath = archiveFileName;
                    strPMTFileName = strSaveFileName;
                    strErrMsg = doExportPMTDetailByFile(strPMTFileName, fileDto, false, uploadFileName);
                    strErrMsg = "Upload success." + strErrMsg;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return strErrMsg;
        }


        public string Uploadpmthis(HttpPostedFile file, string archiveFileName)
        {
            string strSaveFileName = Path.GetFileName(archiveFileName);
            string strFileName = Guid.NewGuid() + "_" + Path.GetFileName(archiveFileName);
            string strFilePath = Path.GetDirectoryName(archiveFileName);
            archiveFileName = Path.Combine(strFilePath, strFileName);
            file.SaveAs(archiveFileName);
            string strPMTFilename = strSaveFileName;
            string strErrMsg = "";
            try
            {
                if (File.Exists(archiveFileName))
                {
                    //保存文件记录
                    string strFileId = Guid.NewGuid().ToString();
                    StringBuilder sqlFile = new StringBuilder();
                    sqlFile.Append("INSERT INTO T_FILE (FILE_ID,FILE_NAME,PHYSICAL_PATH,OPERATOR,CREATE_TIME)");
                    sqlFile.Append(" VALUES (N'" + strFileId + "',");
                    sqlFile.Append("         N'" + strSaveFileName + "',");
                    sqlFile.Append("         N'" + archiveFileName + "',");
                    sqlFile.Append("         N'" + AppContext.Current.User.EID + "',GETDATE());");
                    SqlHelper.ExcuteSql(sqlFile.ToString());

                    string tempResultFileName = saveTempResultFile(archiveFileName);
                    string resultFileName = saveResultFile(archiveFileName);

                    string resultFileId = Guid.NewGuid().ToString();
                    string resultFile = Path.GetFileName(resultFileName);

                    StringBuilder sqlResultFile = new StringBuilder();
                    sqlResultFile.Append("INSERT INTO T_FILE (FILE_ID,FILE_NAME,PHYSICAL_PATH,OPERATOR,CREATE_TIME)");
                    sqlResultFile.Append(" VALUES (N'" + resultFileId + "',");
                    sqlResultFile.Append("         N'" + resultFile + "',");
                    sqlResultFile.Append("         N'" + resultFileName + "',");
                    sqlResultFile.Append("         N'" + AppContext.Current.User.EID + "',GETDATE());");
                    SqlHelper.ExcuteSql(sqlResultFile.ToString());
                    strErrMsg = doExportPMTHisDetailByFile(strPMTFilename, tempResultFileName, resultFileName, strFileId + ";" + resultFileId);
                    strErrMsg = "Upload success." + strErrMsg;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return strErrMsg;

        }


        private string doExportPMTHisDetailByFile(string strPMTFilename, string tempResultFileName, string resultFileName, string fileId)
        {
            string resultStr = "";
            NpoiHelper helper = new NpoiHelper(tempResultFileName);
            helper.ActiveSheet = 0;
            int maxRowNumber = helper.GetLastRowNum();  //获得数据总行数
            int errorRow = 0;
            string taskId = "";
            bool errorFlag = false;


            CaTaskService caTaskService = SpringFactory.GetObjectImpl<CaTaskService>("CaTaskService");
            try
            {
                //读取数据


                Dictionary<int, List<CaPMTHisDto>> dic = new Dictionary<int, List<CaPMTHisDto>>();


                for (int row = 1; row <= maxRowNumber; row++)
                {
                    if (helper.GetCell(row, 0) == null || helper.GetCellType(row, 0).CompareTo(CellType.Blank) == 0)
                    {
                        continue;
                    }
                    errorRow = row;

                    int grno = int.Parse(helper.GetCell(row, 0).ToString());
                    CaPMTHisDto caPMTHisDto = new CaPMTHisDto();
                    caPMTHisDto.GroupNo = grno;
                    if (helper.GetCell(row, 1) != null)
                    {
                        caPMTHisDto.CustomerNum = helper.GetCell(row, 1).ToString().Trim();
                    }
                    if (helper.GetCell(row, 2) != null)
                    {
                        caPMTHisDto.CustomerName = helper.GetCell(row, 2).ToString().Trim();
                    }

                    if (helper.GetCell(row, 3) != null)
                    {
                        caPMTHisDto.LegalEntity = helper.GetCell(row, 3).ToString().Trim();
                    }

                    if (helper.GetCell(row, 4) != null)
                    {
                        caPMTHisDto.SiteUseId = helper.GetCell(row, 4).ToString().Trim();
                    }

                    if (helper.GetCell(row, 5) != null && helper.GetCellType(row, 5) != CellType.Blank && helper.GetCell(row, 5).ToString() != "")
                    {
                        caPMTHisDto.BSNo = int.Parse(helper.GetCell(row, 5).ToString().Trim());
                    }

                    if (helper.GetCell(row, 6) != null)
                    {
                        caPMTHisDto.TransactionINC = helper.GetCell(row, 6).ToString().Trim();
                    }

                    if (helper.GetCell(row, 7) != null && helper.GetCellType(row, 7) != CellType.Blank && helper.GetCell(row, 7).ToString() != "")
                    {
                        caPMTHisDto.ValueDate = helper.GetCell(row, 7).DateCellValue;
                    }


                    if (helper.GetCell(row, 8) != null && helper.GetCellType(row, 8) != CellType.Blank && helper.GetCell(row, 8).ToString() != "")
                    {
                        caPMTHisDto.BSAmount = decimal.Parse(getNumericCellValue(helper, row, 8));
                    }

                    if (helper.GetCell(row, 9) != null && helper.GetCellType(row, 9) != CellType.Blank && helper.GetCell(row, 9).ToString() != "")
                    {
                        caPMTHisDto.BSClearAmount = decimal.Parse(getNumericCellValue(helper, row, 9));
                    }


                    if (helper.GetCell(row, 10) != null)
                    {
                        caPMTHisDto.BSCurrency = helper.GetCell(row, 10).ToString().Trim();
                    }


                    if (helper.GetCell(row, 11) != null)
                    {
                        caPMTHisDto.BSDescription = helper.GetCell(row, 11).ToString().Trim();
                    }

                    if (helper.GetCell(row, 12) != null && helper.GetCellType(row, 12) != CellType.Blank && helper.GetCell(row, 12).ToString() != "")
                    {
                        caPMTHisDto.No = int.Parse(helper.GetCell(row, 12).ToString().Trim());
                    }

                    if (helper.GetCell(row, 13) != null)
                    {
                        caPMTHisDto.InvNo = helper.GetCell(row, 13).ToString().Trim();
                    }

                    if (helper.GetCell(row, 14) != null)
                    {
                        caPMTHisDto.FuncCurrency = helper.GetCell(row, 14).ToString().Trim();
                    }

                    if (helper.GetCell(row, 16) != null && helper.GetCellType(row, 16) != CellType.Blank && helper.GetCell(row, 16).ToString() != "")
                    {
                        caPMTHisDto.ClearAmount = decimal.Parse(getNumericCellValue(helper, row, 16));
                    }

                    if (helper.GetCell(row, 17) != null)
                    {
                        caPMTHisDto.FxRateCurrency = helper.GetCell(row, 17).ToString().Trim();
                    }

                    if (helper.GetCell(row, 18) != null && helper.GetCellType(row, 18) != CellType.Blank && helper.GetCell(row, 18).ToString() != "")
                    {
                        caPMTHisDto.FxRateClearAmount = decimal.Parse(helper.GetCell(row, 18).ToString());
                    }

                    if (helper.GetCell(row, 19) != null)
                    {
                        caPMTHisDto.InvDescription = helper.GetCell(row, 19).ToString().Trim();
                    }

                    if (helper.GetCell(row, 20) != null && helper.GetCellType(row, 20) != CellType.Blank && helper.GetCell(row, 20).ToString() != "")
                    {
                        caPMTHisDto.ReceiveDate = helper.GetCell(row, 20).DateCellValue;
                    }

                    caPMTHisDto.row = row;
                    if (dic.ContainsKey(grno))
                    {
                        dic[grno].Add(caPMTHisDto);
                    }
                    else
                    {
                        List<CaPMTHisDto> list = new List<CaPMTHisDto>();
                        list.Add(caPMTHisDto);
                        dic[grno] = list;
                    }

                }

                if (dic.Count == 0)
                {
                    throw new Exception("No data or miss 'Group No'.");
                }

                Dictionary<int, string> resultMap = new Dictionary<int, string>();

                DateTime now = AppContext.Current.User.Now;
                string[] bankIdArr = "".Split(',');
                taskId = caTaskService.createTask(8, bankIdArr, strPMTFilename, fileId, now);

                foreach (int key in dic.Keys)

                {
                    CaPMTDto caPMTDto = new CaPMTDto();
                    List<CaPMTBSDto> pmtBSList = new List<CaPMTBSDto>();
                    List<CaPMTDetailDto> pmtDetailList = new List<CaPMTDetailDto>();

                    List<CaPMTHisDto> li = dic[key];
                    int caCount = 0;
                    foreach (CaPMTHisDto ca in li)
                    {
                        if (caCount == 0)
                        {
                            caPMTDto.CustomerNum = ca.CustomerNum;
                            caPMTDto.CustomerName = ca.CustomerName;
                            caPMTDto.LegalEntity = ca.LegalEntity;
                            caPMTDto.SiteUseId = ca.SiteUseId;
                            caPMTDto.ValueDate = ca.ValueDate;
                            caPMTDto.Amount = ca.BSClearAmount;
                            caPMTDto.Currency = ca.BSCurrency == null ? "" : ca.BSCurrency.ToUpper();
                            caPMTDto.TASK_ID = taskId;
                            caPMTDto.ReceiveDate = ca.ReceiveDate;
                            caPMTDto.TransactionAmount = ca.BSAmount;
                            caPMTDto.CREATE_DATE = now;
                        }
                        caCount++;
                        if (!string.IsNullOrEmpty(ca.TransactionINC))
                        {
                            CaPMTBSDto caPMTBS = new CaPMTBSDto();
                            caPMTBS.TransactionNumber = ca.TransactionINC;
                            caPMTBS.Currency = ca.BSCurrency == null ? "" : ca.BSCurrency.ToUpper();
                            caPMTBS.Amount = ca.BSClearAmount;
                            caPMTBS.ValueDate = ca.ValueDate;
                            caPMTBS.Description = ca.BSDescription;
                            caPMTBS.TransactionAmount = ca.BSAmount;

                            pmtBSList.Add(caPMTBS);
                        }
                        if (!string.IsNullOrEmpty(ca.InvNo))
                        {
                            CaPMTDetailDto caPMTDetailDto = new CaPMTDetailDto();
                            caPMTDetailDto.InvoiceNum = ca.InvNo;
                            caPMTDetailDto.Currency = ca.FuncCurrency == null ? "" : ca.FuncCurrency.ToUpper();
                            caPMTDetailDto.Amount = ca.ClearAmount;
                            caPMTDetailDto.LocalCurrency = ca.FxRateCurrency == null ? "" : ca.FxRateCurrency.ToUpper();
                            caPMTDetailDto.LocalCurrencyAmount = ca.FxRateClearAmount;
                            caPMTDetailDto.Description = ca.InvDescription;
                            caPMTDetailDto.row = ca.row;

                            pmtDetailList.Add(caPMTDetailDto);
                        }

                    }

                    caPMTDto.PmtBs = pmtBSList;
                    caPMTDto.PmtDetail = pmtDetailList;
                    caPMTDto.filename = strPMTFilename;

                    if (caPMTDto.PmtBs != null && caPMTDto.PmtBs.Count > 0)
                    {
                        caPMTDto.Currency = caPMTDto.PmtBs[0].Currency.ToUpper();
                        if (caPMTDto.PmtBs[0].ValueDate != null)
                        {
                            caPMTDto.ValueDate = caPMTDto.PmtBs[0].ValueDate;
                        }
                    }
                    else if (caPMTDto.PmtDetail != null && caPMTDto.PmtDetail.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(caPMTDto.PmtDetail[0].LocalCurrency))
                        {
                            caPMTDto.Currency = caPMTDto.PmtDetail[0].LocalCurrency.ToUpper();
                        }
                        else
                        {
                            caPMTDto.Currency = caPMTDto.PmtDetail[0].Currency.ToUpper();
                        }
                    }

                    string isok = CheckAndsavePMT(caPMTDto);
                    if (!"OK".Equals(isok))
                    {
                        errorFlag = true;
                    }
                    resultMap[key] = isok;
                }

                using (FileStream fs = new FileStream(tempResultFileName, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook wk = WorkbookFactory.Create(fs);//使用接口，自动识别excel2003/2007格式
                    ISheet sheet = wk.GetSheetAt(0);
                    sheet = insertColumns(sheet, 0);

                    ICell headCell = sheet.GetRow(0).GetCell(0);


                    if (headCell == null)
                    {
                        sheet.GetRow(0).CreateCell(0);
                        headCell = sheet.GetRow(0).GetCell(0);
                    }

                    headCell.SetCellValue("Status");

                    int rownum = sheet.LastRowNum;

                    for (int sRow = 1; sRow <= rownum; sRow++)
                    {
                        if (sheet.GetRow(sRow) == null)
                        {
                            continue;
                        }
                        ICell numcell = sheet.GetRow(sRow).GetCell(1);
                        if (numcell != null)
                        {
                            int gn = (int)numcell.NumericCellValue;
                            if (gn == 0)
                            {
                                continue;
                            }
                            ICell cell = sheet.GetRow(sRow).GetCell(0);
                            if (cell == null)
                            {
                                sheet.GetRow(sRow).CreateCell(0);
                                cell = sheet.GetRow(sRow).GetCell(0);
                            }
                            cell.SetCellValue(resultMap[gn]);
                        }


                    }

                    FileStream fos = null;
                    fos = new FileStream(resultFileName, FileMode.Create, FileAccess.Write);
                    wk.Write(fos);
                    fos.Close();
                }

                if (errorFlag)
                {
                    throw new OTCServiceException("Upload fail!");
                }

                caTaskService.updateTaskStatusById(taskId, "2");//完成
            }
            catch (Exception e)
            {
                int error = errorRow;
                if (!String.IsNullOrEmpty(taskId))
                {
                    caTaskService.updateTaskStatusById(taskId, "3");//异常
                }
                throw new Exception("Row:" + error + "," + e.Message);
            }

            return resultStr;
        }
        public string UploadPMTDetailByFileId(string fileId)
        {
            string strErrMsg = "";
            try
            {
                //查询文件记录
                StringBuilder sqlFile = new StringBuilder();
                sqlFile.Append("SELECT FILE_ID as FileId, FILE_NAME as FileName, PHYSICAL_PATH as PhysicalPath, OPERATOR as Operator ");
                sqlFile.Append("FROM T_FILE with (nolock) ");
                sqlFile.Append("WHERE FILE_ID = '");
                sqlFile.Append(fileId + "'");
                List<FileDto> fileList = SqlHelper.GetList<FileDto>(SqlHelper.ExcuteTable(sqlFile.ToString(), System.Data.CommandType.Text));
                if (fileList.Count > 0)
                {
                    FileDto fileDto = fileList.FirstOrDefault<FileDto>();
                    if (File.Exists(fileDto.PhysicalPath))
                    {
                        string uploadFileName = fileDto.FileName.Substring(fileDto.FileName.LastIndexOf("_"));
                        strErrMsg = doExportPMTDetailByFile("", fileDto, true, uploadFileName);
                    }
                    else
                    {
                        throw new Exception("The file does not exist, please upload the file again.");
                    }
                }
                else
                {
                    throw new Exception("The file does not exist, please upload the file again.");
                }


                strErrMsg = "Upload success." + strErrMsg;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return strErrMsg;
        }

        public string doExportPMTDetailByFile(string strPMTFileName, FileDto fileDto, bool deleteReconFlag, String uploadFileName, String businessID = "")
        {
            string resultStr = "";
            NpoiHelper helper = new NpoiHelper(fileDto.PhysicalPath);
            helper.ActiveSheet = 0;
            int maxRowNumber = helper.GetLastRowNum();  //获得数据总行数
            int errorRow = 0;
            string taskId = "";

            CaTaskService caTaskService = SpringFactory.GetObjectImpl<CaTaskService>("CaTaskService");
            try
            {
                Helper.Log.Info("*************111222222*****************");
                //读取数据
                CaPMTDto caPMTDto = new CaPMTDto();
                caPMTDto.businessId = businessID;
                Helper.Log.Info("*************111222222*****************：" + caPMTDto.businessId);
                caPMTDto.filename = strPMTFileName;
                List<CaPMTBSDto> pmtBSList = new List<CaPMTBSDto>();
                List<CaPMTDetailDto> pmtDetailList = new List<CaPMTDetailDto>();
                int currRowNumber = 0;

                for (int row = currRowNumber; row <= maxRowNumber; row++)
                {
                    if (helper.GetCell(row, 0) == null || helper.GetCellType(row, 0).CompareTo(CellType.Blank) == 0)
                    {
                        continue;
                    }
                    else
                    {
                        string headerstr = getStringCellValue(helper, row, 0);
                        if (headerstr.IndexOf("客户名称") >= 0)
                        {
                            if (helper.GetCell(row, 1) != null && helper.GetCellType(row, 1).CompareTo(CellType.Blank) != 0)
                            {
                                string customerNumber = getStringCellValue(helper, row, 1);
                                caPMTDto.CustomerNum = customerNumber;
                            }

                            if (helper.GetCell(row, 5) != null && helper.GetCellType(row, 5).CompareTo(CellType.Blank) != 0)
                            {
                                try
                                {
                                    string sladate = getDateCellValue(helper, row, 5);
                                    caPMTDto.ReceiveDate = Convert.ToDateTime(sladate);
                                }
                                catch (Exception e)
                                {
                                    throw new Exception("Receive Date is invalid.");
                                }
                            }
                            //Reserve为必填项
                            if (caPMTDto.ReceiveDate == null)
                            {
                                throw new Exception("Receive Date cannot be empty.");
                            }

                            if (helper.GetCell(row, 7) != null && helper.GetCellType(row, 7).CompareTo(CellType.Blank) != 0)
                            {
                                string legalEntity = getStringCellValue(helper, row, 7);
                                if (legalEntity.Length >= 4)
                                {
                                    legalEntity = legalEntity.Substring(0, 4).Trim();
                                }
                                caPMTDto.LegalEntity = legalEntity;
                            }
                            currRowNumber = row + 1;
                            if (helper.GetCell(currRowNumber, 1) != null && helper.GetCellType(currRowNumber, 1).CompareTo(CellType.Blank) != 0)
                            {
                                string strSiteUseId = getStringCellValue(helper, currRowNumber, 1);
                                caPMTDto.SiteUseId = strSiteUseId;
                            }
                            break;
                        }

                    }

                }

                Helper.Log.Info("*************33333333*****************");
                //开始记录付款列表信息
                bool startBSFlag = false;
                int transactionIndex = 0;
                for (int row = currRowNumber; row <= maxRowNumber; row++)
                {
                    if (!startBSFlag && (helper.GetCell(row, 0) == null || helper.GetCellType(row, 0).CompareTo(CellType.Blank) == 0))
                    {
                        if (startBSFlag)
                        {
                            //若出现SUM表示数据行已结束，则跳出循环
                            currRowNumber = row + 1;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        string headerstr = getStringCellValue(helper, row, 0);
                        if (headerstr.IndexOf("付款信息") >= 0)
                        {
                            row++;
                            if ((helper.GetCell(row, 0) == null || helper.GetCellType(row, 0).CompareTo(CellType.Blank) == 0)
                                && (helper.GetCell(row, 1) == null || helper.GetCellType(row, 1).CompareTo(CellType.Blank) == 0)
                                && (helper.GetCell(row, 2) == null || helper.GetCellType(row, 2).CompareTo(CellType.Blank) == 0)
                                && (helper.GetCell(row, 3) == null || helper.GetCellType(row, 3).CompareTo(CellType.Blank) == 0)
                                )
                            {
                                //若付款信息后面没有数据行，则跳出循环
                                currRowNumber = row + 1;
                                break;
                            }
                            else
                            {
                                //校验数据行表头
                                if (getStringCellValue(helper, row, 0) == "No."
                                    && getStringCellValue(helper, row, 1) == "Transaction INC/PMT")
                                {
                                    startBSFlag = true;
                                    continue;
                                }
                                else
                                {
                                    //若付款信息后面数据行内容不对，则跳出循环
                                    currRowNumber = row + 1;
                                    break;
                                }
                            }

                        }
                        else if (startBSFlag && (headerstr.IndexOf("SUM") >= 0 || headerstr.IndexOf("发票明细") >= 0))
                        {
                            //若出现SUM表示数据行已结束，则跳出循环
                            currRowNumber = row;
                            break;
                        }

                        if (startBSFlag)
                        {
                            CaPMTBSDto caPMTBS = new CaPMTBSDto();

                            if (helper.GetCell(row, 1) == null || helper.GetCellType(row, 1).CompareTo(CellType.Blank) == 0)
                            {
                                if (transactionIndex == 0)
                                {
                                    if (!(helper.GetCell(row, 2) == null || helper.GetCellType(row, 2).CompareTo(CellType.Blank) == 0))
                                    {
                                        try
                                        {
                                            string strValue = getDateCellValue(helper, row, 2);
                                            try
                                            {
                                                caPMTDto.ValueDate = Convert.ToDateTime(strValue);
                                            }
                                            catch (Exception e)
                                            {
                                                try
                                                {
                                                    caPMTDto.ValueDate = string2Date(strValue);
                                                }
                                                catch (Exception ex)
                                                {
                                                    errorRow = row + 1;
                                                    throw new Exception("Upload failed. Row:" + errorRow + ", the format fo Date is incorrect.");
                                                }
                                            }
                                        }
                                        catch (Exception de)
                                        {
                                            errorRow = row + 1;
                                            throw new Exception("Upload failed. Row:" + errorRow + ", the format fo Date is incorrect.");
                                        }
                                    }

                                    //Currency
                                    if (!(helper.GetCell(row, 3) == null || helper.GetCellType(row, 3).CompareTo(CellType.Blank) == 0))
                                    {
                                        string strValue = getStringCellValue(helper, row, 3);
                                        caPMTDto.Currency = strValue.ToUpper();
                                    }

                                    //Transaction Amount
                                    if (!(helper.GetCell(row, 4) == null || helper.GetCellType(row, 4).CompareTo(CellType.Blank) == 0))
                                    {
                                        try
                                        {
                                            string strValue = getNumericCellValue(helper, row, 4);

                                            try
                                            {
                                                caPMTDto.TransactionAmount = Convert.ToDecimal(strValue);
                                            }
                                            catch (Exception e)
                                            {
                                                errorRow = row + 1;
                                                throw new Exception("Upload failed. Row:" + errorRow + ", the format fo Transaction Amount is incorrect.");
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            errorRow = row + 1;
                                            throw new Exception("Upload failed. Row:" + errorRow + ", the format fo Transaction Amount is incorrect.");
                                        }
                                    }

                                    if (!(helper.GetCell(row, 5) == null || helper.GetCellType(row, 5).CompareTo(CellType.Blank) == 0))
                                    {
                                        try
                                        {
                                            string strValue = getNumericCellValue(helper, row, 5);

                                            try
                                            {
                                                caPMTDto.Amount = Convert.ToDecimal(strValue);
                                            }
                                            catch (Exception e)
                                            {
                                                errorRow = row + 1;
                                                throw new Exception("Upload failed. Row:" + errorRow + ", the format fo Amount is incorrect.");
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            errorRow = row + 1;
                                            throw new Exception("Upload failed. Row:" + errorRow + ", the format fo Amount is incorrect.");
                                        }
                                    }



                                    //Bank Charge
                                    if (!(helper.GetCell(row, 6) == null || helper.GetCellType(row, 6).CompareTo(CellType.Blank) == 0))
                                    {
                                        try
                                        {
                                            string strValue = getNumericCellValue(helper, row, 6);

                                            try
                                            {
                                                caPMTDto.BankCharge = Convert.ToDecimal(strValue);
                                            }
                                            catch (Exception e)
                                            {
                                                errorRow = row + 1;
                                                throw new Exception("Upload failed. Row:" + errorRow + ", the format fo Amount is incorrect.");
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            errorRow = row + 1;
                                            throw new Exception("Upload failed. Row:" + errorRow + ", the format fo Amount is incorrect.");
                                        }
                                    }
                                }
                                currRowNumber = row + 1;
                                break;
                            }
                            else
                            {
                                //1 Transaction INC
                                var transactionIncVal = "";
                                transactionIncVal = helper.GetCell(row, 1).ToString();
                                transactionIncVal = transactionIncVal.TrimStart().TrimEnd();
                                caPMTBS.TransactionNumber = transactionIncVal;

                                //2 Value Date
                                try
                                {
                                    string dateVal = getDateCellValue(helper, row, 2);
                                    try
                                    {
                                        caPMTBS.ValueDate = Convert.ToDateTime(dateVal);
                                    }
                                    catch (Exception e)
                                    {
                                        try
                                        {
                                            caPMTBS.ValueDate = string2Date(dateVal);
                                        }
                                        catch (Exception ex)
                                        {
                                            errorRow = row + 1;
                                            throw new Exception("Upload failed. Row:" + errorRow + ", the format fo Date is incorrect.");
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    errorRow = row + 1;
                                    throw new Exception("Upload failed. Row:" + errorRow + ", the format fo Date is incorrect.");
                                }

                                //5 Clear Amount
                                String amountVal = "";
                                try
                                {
                                    amountVal = getNumericCellValue(helper, row, 5);
                                    if (!String.IsNullOrEmpty(amountVal))
                                    {
                                        caPMTBS.Amount = Convert.ToDecimal(amountVal);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errorRow = row + 1;
                                    throw new Exception("Upload failed. Row:" + errorRow + ", Clear Amount cannot be invalid number.");
                                }

                                String bankChargeVal = "";
                                try
                                {
                                    bankChargeVal = getNumericCellValue(helper, row, 6);
                                    if (!String.IsNullOrEmpty(bankChargeVal))
                                    {
                                        caPMTBS.BankCharge = Convert.ToDecimal(bankChargeVal);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errorRow = row + 1;
                                    throw new Exception("Upload failed. Row:" + errorRow + ", Clear Amount cannot be invalid number.");
                                }

                                //4 Currency
                                var currencyVal = getStringCellValue(helper, row, 3);
                                caPMTBS.Currency = currencyVal.ToUpper();

                                //6 Description
                                var descriptionVal = getStringCellValue(helper, row, 7);
                                caPMTBS.Description = descriptionVal;

                                caPMTBS.row = row + 1;
                                pmtBSList.Add(caPMTBS);
                            }
                        }
                    }
                }

                Helper.Log.Info("*************44444444*****************");
                //跳到下方的发票明细中获取数据
                bool startDetailFlag = false;
                bool startDetailFlag1 = false;
                for (int drow = currRowNumber; drow <= maxRowNumber; drow++)
                {
                    if ((!startDetailFlag && (helper.GetCell(drow, 0) == null || helper.GetCellType(drow, 0).CompareTo(CellType.Blank) == 0))
                        || (startDetailFlag && (helper.GetCell(drow, 2) == null || helper.GetCellType(drow, 2).CompareTo(CellType.Blank) == 0))
                        ||
                        (!startDetailFlag1 && (helper.GetCell(drow, 0) == null || helper.GetCellType(drow, 0).CompareTo(CellType.Blank) == 0))
                        || (startDetailFlag1 && (helper.GetCell(drow, 1) == null || helper.GetCellType(drow, 1).CompareTo(CellType.Blank) == 0))
                    )
                    {
                        if (startDetailFlag || startDetailFlag1)
                        {
                            //若出现SUM表示数据行已结束，则跳出循环
                            currRowNumber = drow + 1;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        string headerstr = getStringCellValue(helper, drow, 0);
                        if (headerstr.IndexOf("发票明细") >= 0)
                        {
                            drow++;
                            if (helper.GetCell(drow, 0) == null || helper.GetCellType(drow, 0).CompareTo(CellType.Blank) == 0)
                            {
                                //若付款信息后面没有数据行，则跳出循环
                                currRowNumber = drow + 1;
                                break;
                            }
                            else
                            {
                                //校验数据行表头
                                if (getStringCellValue(helper, drow, 0) == "No."
                                    && (getStringCellValue(helper, drow, 2) == "InvNo./OrderNo."
                                    || getStringCellValue(helper, drow, 2) == "InvNo."))
                                {
                                    startDetailFlag = true;
                                    continue;
                                }
                                if (getStringCellValue(helper, drow, 0) == "No."
                                    && (getStringCellValue(helper, drow, 1) == "InvNo./OrderNo."
                                    || getStringCellValue(helper, drow, 1) == "InvNo."))
                                {
                                    startDetailFlag1 = true;
                                    continue;
                                }
                                else
                                {
                                    //若付款信息后面数据行内容不对，则跳出循环
                                    currRowNumber = drow + 1;
                                    break;
                                }
                            }

                        }

                        //循环发票行数据
                        if (startDetailFlag || startDetailFlag1)
                        {
                            CaPMTDetailDto caPMTDetail = new CaPMTDetailDto();
                            //完整版明细
                            if (startDetailFlag)
                            {
                                if (helper.GetCell(drow, 2) == null || helper.GetCellType(drow, 2).CompareTo(CellType.Blank) == 0)
                                {
                                    break;
                                }
                                else
                                {
                                    //1 SiteUseId
                                    var siteUseIdVal = getStringCellValue(helper, drow, 1);
                                    caPMTDetail.SiteUseId = siteUseIdVal.Trim();

                                    //2 InvNo.
                                    var invoiceNumVal = getStringCellValue(helper, drow, 2);
                                    if (string.IsNullOrEmpty(invoiceNumVal))
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        caPMTDetail.InvoiceNum = invoiceNumVal.Trim();
                                    }


                                    //3 Date
                                    try
                                    {
                                        string dateVal = getDateCellValue(helper, drow, 3);
                                        try
                                        {
                                            caPMTDetail.InvoiceDate = Convert.ToDateTime(dateVal);
                                        }
                                        catch (Exception e)
                                        {
                                            try
                                            {
                                                caPMTDetail.InvoiceDate = string2Date(dateVal);
                                            }
                                            catch (Exception ex)
                                            {
                                                errorRow = drow + 1;
                                                throw new Exception("Upload failed. Row:" + errorRow + ", the format fo Date is incorrect.");
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        errorRow = drow + 1;
                                        throw new Exception("Upload failed. Row:" + errorRow + ", the format fo Date is incorrect.");
                                    }

                                    //6 clear Amount
                                    String amountVal = "";
                                    try
                                    {
                                        amountVal = getNumericCellValue(helper, drow, 6);
                                        if (String.IsNullOrEmpty(amountVal))
                                        {
                                            errorRow = drow + 1;
                                            throw new Exception("Upload failed. Row:" + errorRow + ", Clear Amount cannot be Empty.");
                                        }
                                        else
                                        {
                                            try
                                            {
                                                caPMTDetail.Amount = Convert.ToDecimal(amountVal);
                                            }
                                            catch (Exception ex)
                                            {
                                                errorRow = drow + 1;
                                                throw new Exception("Upload failed. Row:" + errorRow + ", Clear Amount cannot be invalid number.");
                                            }

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ex.Message);
                                    }

                                    //4 func Currency
                                    string currencyVal = getStringCellValue(helper, drow, 4);
                                    if (string.IsNullOrEmpty(currencyVal))
                                    {
                                        errorRow = drow + 1;
                                        throw new Exception("Upload failed. Row:" + errorRow + ",Local Currency cannot be Empty.");
                                    }
                                    caPMTDetail.Currency = currencyVal.Trim().ToUpper();

                                    //7 Inv Currency
                                    var invCurrencyVal = getStringCellValue(helper, drow, 7);
                                    caPMTDetail.LocalCurrency = invCurrencyVal.ToUpper();

                                    //8 Inv Amount
                                    String invAmountVal = "";
                                    try
                                    {
                                        invAmountVal = getNumericCellValue(helper, drow, 8);
                                        if (!String.IsNullOrEmpty(invAmountVal))
                                        {
                                            caPMTDetail.LocalCurrencyAmount = Convert.ToDecimal(invAmountVal);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        errorRow = drow + 1;
                                        throw new Exception("Upload failed. Row:" + errorRow + ", Inv Amount cannot be invalid number.");
                                    }

                                    //9 Description
                                    var descriptionVal = getStringCellValue(helper, drow, 9);
                                    caPMTDetail.Description = descriptionVal;

                                    caPMTDetail.row = drow + 1;
                                    caPMTDetail.CUSTOMER_NUM = caPMTDto.CustomerNum;
                                    pmtDetailList.Add(caPMTDetail);
                                }
                            }
                            //精简版明细（只有行号，发票号，金额）
                            if (startDetailFlag1) { 
                                if (helper.GetCell(drow, 1) == null || helper.GetCellType(drow, 1).CompareTo(CellType.Blank) == 0)
                                {
                                    break;
                                }
                                else
                                {
                                    //2 InvNo.
                                    var invoiceNumVal = getStringCellValue(helper, drow, 1);
                                    if (string.IsNullOrEmpty(invoiceNumVal))
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        caPMTDetail.InvoiceNum = invoiceNumVal.Trim();
                                    }
                                    String invAmountVal = "";
                                    try
                                    {
                                        invAmountVal = getNumericCellValue(helper, drow, 2);
                                        if (!String.IsNullOrEmpty(invAmountVal))
                                        {
                                            caPMTDetail.Amount = Convert.ToDecimal(invAmountVal);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        errorRow = drow + 1;
                                        throw new Exception("Upload failed. Row:" + errorRow + ", Inv Amount cannot be invalid number.");
                                    }
                                    //根据发票号获得其它信息
                                    //caPMTDto.CustomerNum
                                    InvoiceAging invItem = (from a in CommonRep.GetQueryable<InvoiceAging>()
                                                            where a.InvoiceNum == invoiceNumVal
                                                               && a.CustomerNum == caPMTDto.CustomerNum
                                                               && (a.Class == "INV" || a.Class == "CM")
                                                            select a).First();
                                    if (invItem != null)
                                    {
                                        caPMTDetail.CUSTOMER_NUM = invItem.CustomerNum;
                                        caPMTDetail.SiteUseId = invItem.SiteUseId;
                                        caPMTDetail.InvoiceDate = invItem.InvoiceDate;
                                        caPMTDetail.Currency = invItem.Currency;
                                    }
                                    pmtDetailList.Add(caPMTDetail);
                                }
                            }
                        }
                    }
                }

                Helper.Log.Info("*************55555*****************");
                DateTime now = AppContext.Current.User.Now;
                caPMTDto.CREATE_DATE = now;
                string[] bankIdArr = "".Split(',');
                Helper.Log.Info("*************55551111*****************");
                taskId = caTaskService.createTask(2, bankIdArr, uploadFileName, fileDto.FileId, now);
                Helper.Log.Info("*************55552222*****************");
                caPMTDto.TASK_ID = taskId;

                Helper.Log.Info("*************55555222222*****************");
                //校验必填项
                int bslistCount = pmtBSList.Count();
                int detailListCount = pmtDetailList.Count();

                Helper.Log.Info("*************5555522222*****************");
                if (bslistCount <= 0 && detailListCount <= 0)
                {
                    string str = doCheckPmt(caPMTDto);
                    Helper.Log.Info("*************5555544444*****************");
                    if (!string.IsNullOrEmpty(str))
                    {
                        throw new Exception(str);
                    }
                }
                Helper.Log.Info("*************66666*****************");
                if (bslistCount > 0 && detailListCount <= 0)//主表+BS
                {
                    Helper.Log.Info("*************77777*****************");
                    Helper.Log.Info("*************111222222*****************：" + caPMTDto.businessId);
                    caPMTDto.PmtBs = pmtBSList;
                    string str = doCheckPmtAndBS(caPMTDto);
                    if (!string.IsNullOrEmpty(str))
                    {
                        throw new Exception(str);
                    }
                }
                else if (detailListCount > 0 && bslistCount <= 0)//主表+Detail
                {
                    Helper.Log.Info("*************88888*****************");
                    Helper.Log.Info("*************111222222*****************：" + caPMTDto.businessId);
                    caPMTDto.PmtDetail = pmtDetailList;
                    string str = doCheckPmtAndDetail(caPMTDto, deleteReconFlag, fileDto.FileId);
                    if (!string.IsNullOrEmpty(str))
                    {
                        throw new Exception(str);
                    }
                }
                else if (bslistCount > 0 && detailListCount > 0)//主表+BS+Detail
                {
                    Helper.Log.Info("*************99999*****************");
                    Helper.Log.Info("*************111222222*****************：" + caPMTDto.businessId);
                    caPMTDto.PmtBs = pmtBSList;
                    caPMTDto.PmtDetail = pmtDetailList;
                    string str = doCheckPmtAll(caPMTDto, deleteReconFlag, fileDto.FileId);
                    if (!string.IsNullOrEmpty(str))
                    {
                        throw new Exception(str);
                    }
                }
                Helper.Log.Info("*************8q2312341234*****************");

                caTaskService.updateTaskStatusById(taskId, "2");//完成
                Helper.Log.Info("*************777777777777*****************");
            }
            catch (Exception e)
            {
                if (!String.IsNullOrEmpty(taskId))
                {
                    caTaskService.updateTaskStatusById(taskId, "3");//异常
                }
                throw new Exception(e.Message);
            }

            Helper.Log.Info("*************88888*****************" + resultStr);
            return resultStr;
        }

        private string doCheckPmt(CaPMTDto pmt)
        {
            string resultStr = string.Empty;
            if (string.IsNullOrEmpty(pmt.CustomerNum))
            {
                resultStr = "The Customer Number cannot be Empty.";
                return resultStr;
            }

            if (string.IsNullOrEmpty(pmt.LegalEntity))
            {
                resultStr = "The Legal Entity cannot be Empty.";
                return resultStr;
            }

            if (isNotExistedCustomerNumber(pmt.CustomerNum, pmt.LegalEntity))
            {
                resultStr = "The Customer Number must be in the Legal Entity(" + pmt.LegalEntity + ").";
                return resultStr;
            }

            if (string.IsNullOrEmpty(pmt.Currency))
            {
                resultStr = "The Payment Currency cannot be Empty.";
                return resultStr;
            }

            if (pmt.ValueDate == null)
            {
                resultStr = "The Payment Value Date cannot be Empty.";
                return resultStr;
            }

            if (pmt.TransactionAmount == null)
            {
                resultStr = "The Transaction Amount cannot be Empty.";
                return resultStr;
            }
            Helper.Log.Info("****************doCheckPmt:" + pmt.businessId);
            string groupNo = getNewGroupNo();
            pmt.GroupNo = groupNo;
            String reconId = string.Empty;
            if (string.IsNullOrEmpty(pmt.ID))
            {
                reconId = savePMT(pmt);
            }
            else
            {
                reconId = pmt.ID;
                updatePMT(pmt);

                //删除bs和detail数据
                string delPMTBSsql = string.Format(@"delete from T_CA_PMTBS where ReconId = '{0}'", reconId);
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(delPMTBSsql);

                string delPMTDetailsql = string.Format(@"delete from T_CA_PMTDetail where ReconId = '{0}'", reconId);
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(delPMTDetailsql);
            }
            return resultStr;
        }
        private string doCheckPmtAndBS(CaPMTDto pmt)
        {
            string resultStr = string.Empty;
            List<CaPMTBSDto> bsList = pmt.PmtBs;
            StringBuilder bankIdsSb = new StringBuilder();
            StringBuilder bssql = new StringBuilder();
            List<string> bsCurrencyList = new List<string>();
            int index = 0;
            decimal pmtAmount = 0;
            string errMsgHead = string.Empty;


            bssql.Append("SELECT DISTINCT t.ID, t.GroupNo ");
            bssql.Append("FROM T_CA_PMTBS b with (nolock), T_CA_PMT t with (nolock) ");
            bssql.Append("WHERE t.ID = b.ReconId ");
            bssql.Append("AND ReconId IN(");

            foreach (CaPMTBSDto rb in bsList)
            {
                //为与页面提示信息区别开，导入提示时需要加上导入行号，页面录入则不需要
                if (!string.IsNullOrEmpty(pmt.ID))
                {
                    errMsgHead = "Upload failed. Row:" + rb.row + ", ";
                }
                if (String.IsNullOrEmpty(rb.TransactionNumber))
                {
                    resultStr = errMsgHead + "Transaction number cannot be empty.";
                    return resultStr;
                }
                else
                {
                    //检验area下的bankstatementId是否存在
                    List<CaBankStatementDto> bankList = getBanksByTransactionNum(rb.TransactionNumber, pmt.LegalEntity);

                    if (bankList == null || bankList.Count() <= 0)
                    {
                        if (string.IsNullOrEmpty(pmt.LegalEntity))
                        {
                            resultStr = errMsgHead + "Transaction number must exist.";
                        }
                        else
                        {
                            resultStr = errMsgHead + "Transaction number must exist in the legal entity(" + pmt.LegalEntity + ").";
                        }
                        return resultStr;
                    }
                    else
                    {
                        CaBankStatementDto bank = bankList[0];
                        if (bankIdsSb.ToString().IndexOf(bank.ID) < 0)
                        {
                            bankIdsSb.Append("," + bank.ID);
                            rb.BANK_STATEMENT_ID = bank.ID;
                            rb.LegalEntity = bank.LegalEntity;
                            rb.Currency = bank.CURRENCY.ToUpper();
                            rb.ValueDate = bank.VALUE_DATE;
                            rb.TransactionAmount = bank.TRANSACTION_AMOUNT;//需要插入pmt主表中
                            rb.currentAmount = bank.CURRENT_AMOUNT;
                            rb.BankCharge = rb.BankCharge ?? (bank.BankChargeTo ?? decimal.Zero);

                            pmtAmount = decimal.Add(pmtAmount, rb.Amount ?? 0);

                            //transaction amount 与 clear amount比较的时候，需要加上bankCharge
                            Decimal currentAmt = Decimal.Add(rb.currentAmount ?? 0, rb.BankCharge ?? 0);
                            if (Decimal.Compare(rb.Amount ?? 0, currentAmt) > 0)
                            {
                                resultStr = "The Current Amount(" + rb.currentAmount + ") cannot be less than the Clear Amount(" + rb.Amount + ").";
                                return resultStr;
                            }

                            //判断Bank是否被使用
                            CaReconDto recon = getBankReconResult(bank.ID);
                            //如果有bank
                            if (recon != null && !string.IsNullOrEmpty(recon.PMT_ID))
                            {

                                if (recon.isClosed)
                                {
                                    //如果被使用,分情况提示
                                    if (string.Equals(bank.CLEARING_STATUS, "1"))
                                    {
                                        resultStr = "The Bank Statement(" + bank.TRANSACTION_NUMBER + ") is clearing now, cannot import once again.";
                                        return resultStr;
                                    }
                                    else if (string.Equals(bank.CLEARING_STATUS, "2"))
                                    {
                                        resultStr = "The Bank Statement(" + bank.TRANSACTION_NUMBER + ") has been cleared, cannot import once again.";
                                        return resultStr;
                                    }
                                }
                            }

                        }
                        else
                        {
                            resultStr = errMsgHead + "There are duplication of transation number: " + rb.TransactionNumber;
                            return resultStr;
                        }
                    }

                    if (index == 0)
                    {
                        bssql.Append("(SELECT ReconId FROM T_CA_PMTBS with (nolock) WHERE BANK_STATEMENT_ID = '");
                    }
                    else
                    {
                        bssql.Append(" INTERSECT (SELECT ReconId FROM T_CA_PMTBS with (nolock) WHERE BANK_STATEMENT_ID = '");
                    }

                    bssql.Append(rb.BANK_STATEMENT_ID);
                    bssql.Append("' AND Currency = '");
                    bssql.Append(rb.Currency);
                    bssql.Append("' AND Amount = ");
                    bssql.Append(rb.Amount ?? 0);
                    bssql.Append(" AND BankCharge = ");
                    bssql.Append(rb.BankCharge ?? 0);
                    bssql.Append(") ");
                    index++;
                }
            }
            bssql.Append(") ");
            bssql.Append("AND t.ISAPPLYGROUP = 0 ");
            bssql.Append("AND t.ISPOSTGROUP = 0 ");

            if (bsCurrencyList.Distinct().ToList().Count() > 1)
            {
                resultStr = "Bank Statement Currency must be same. ";
                return resultStr;
            }
            else
            {
                pmt.Amount = pmtAmount;
                pmt.Currency = bsList[0].Currency.ToUpper();
                pmt.ValueDate = bsList[0].ValueDate;
                pmt.LegalEntity = bsList[0].LegalEntity;
                pmt.TransactionAmount = bsList[0].TransactionAmount;
            }

            if (string.IsNullOrEmpty(pmt.CustomerNum))
            {
                resultStr = "Customer Number cannot be empty when there is no AR infomation.";
                return resultStr;
            }
            else
            {
                if (isNotExistedCustomerNumber(pmt.CustomerNum, pmt.LegalEntity))
                {
                    resultStr = "The Customer Number must be in the Legal Entity(" + pmt.LegalEntity + ").";
                    return resultStr;
                }
            }

            List<CaPMTDto> list = SqlHelper.GetList<CaPMTDto>(SqlHelper.ExcuteTable(bssql.ToString(), System.Data.CommandType.Text));
            if (list != null && list.Count() > 0)
            {
                if (!string.Equals(list[0].ID, pmt.ID))
                {
                    string currGroupNo = list[0].GroupNo;
                    resultStr = "There is already unused PMT named " + currGroupNo;
                    return resultStr;
                }

            }

            //修改task
            CaTaskService caTaskService = SpringFactory.GetObjectImpl<CaTaskService>("CaTaskService");
            string[] bankIdArr = bankIdsSb.ToString().Substring(1).Split(',');
            caTaskService.createTaskBS(pmt.TASK_ID, bankIdArr);

            //插入主表
            string groupNo = getNewGroupNo();
            pmt.GroupNo = groupNo;
            String reconId = string.Empty;
            if (string.IsNullOrEmpty(pmt.ID))
            {
                reconId = savePMT(pmt);
            }
            else
            {
                reconId = pmt.ID;
                updatePMT(pmt);

                //删除bs和detail数据
                string delPMTBSsql = string.Format(@"delete from T_CA_PMTBS where ReconId = '{0}'", reconId);
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(delPMTBSsql);

                string delPMTDetailsql = string.Format(@"delete from T_CA_PMTDetail where ReconId = '{0}'", reconId);
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(delPMTDetailsql);
            }

            //插入从表
            savePMTBS(reconId, pmt.CustomerNum, bsList, pmt.SiteUseId);

            return resultStr;
        }

        private string doCheckPmtAndDetail(CaPMTDto pmt, bool deleteReconFlag, string fileId)
        {
            string resultStr = string.Empty;
            string errMsgHead = string.Empty;
            List<CaPMTDetailDto> detailList = pmt.PmtDetail;

            int index = 0;
            List<string> customerkeylist = new List<string>();
            List<string> legalEntitykeylist = new List<string>();
            List<string> currencyList = new List<string>();
            List<string> invCurrencyList = new List<string>();
            StringBuilder detailsql = new StringBuilder();
            StringBuilder invoiceNumbers = new StringBuilder();
            StringBuilder invoices = new StringBuilder();
            string detailCurrency = string.Empty;
            string detailInvCurrency = string.Empty;

            int invoiceCount = 0;
            StringBuilder errmsgSB = new StringBuilder();
            detailsql.Append("SELECT DISTINCT p.ID, p.GroupNo ");
            detailsql.Append("FROM T_CA_PMTDetail t with (nolock), T_CA_PMT p with (nolock) ");
            detailsql.Append("WHERE p.ID = t.ReconId ");
            detailsql.Append("AND t.ReconId IN(");

            if (pmt.ValueDate == null)
            {
                resultStr = "Payment Value Date cannot be empty.";
                return resultStr;
            }

            if (pmt.TransactionAmount == null)
            {
                resultStr = "The Bank Statement Transaction Amount cannot be empty.";
                return resultStr;
            }

            if (pmt.Amount == null)
            {
                resultStr = "The payment Clear Amount cannot be empty.";
                return resultStr;
            }

            Decimal transactionAmount = Decimal.Add(pmt.TransactionAmount ?? 0, pmt.BankCharge ?? 0);
            if (Decimal.Compare(pmt.Amount ?? 0, transactionAmount) > 0)
            {
                resultStr = "The Transaction Amount(" + transactionAmount + ") cannot be less than the Clear Amount(" + pmt.Amount + ").";
                return resultStr;
            }

            if (string.IsNullOrEmpty(pmt.Currency))
            {
                resultStr = "Payment Currency cannot be empty.";
                return resultStr;
            }

            foreach (CaPMTDetailDto rd in detailList)
            {
                //为与页面提示信息区别开，导入提示时需要加上导入行号，页面录入则不需要
                if (!string.IsNullOrEmpty(pmt.ID))
                {
                    errMsgHead = "Upload failed. Row:" + rd.row + ", ";
                }

                //Local Currency不能为空
                if (string.IsNullOrEmpty(rd.Currency))
                {
                    resultStr = errMsgHead + "the Local Currency cannot be empty.";
                    return resultStr;
                }
                else
                {
                    currencyList.Add(rd.Currency);
                }

                //如果inv currency不为空，记录inv currency，并判读inv currency是否相同
                if (!string.IsNullOrEmpty(rd.LocalCurrency))
                {
                    invCurrencyList.Add(rd.LocalCurrency);
                }

                if (invoiceNumbers.ToString().IndexOf(rd.InvoiceNum) < 0)
                {
                    invoiceNumbers.Append("," + rd.InvoiceNum);
                    if (string.IsNullOrEmpty(rd.Description))
                    {
                        invoices.Append("," + rd.InvoiceNum);
                    }
                }
                else
                {
                    resultStr = errMsgHead + "There are duplication of invoice numbers: " + rd.InvoiceNum;
                    return resultStr;
                }

                //校验发票号是否过期
                CaARDto arDto = getInvoiceByInvoiceNum(rd.InvoiceNum, rd.SiteUseId, pmt.CustomerNum);
                if (arDto != null)
                {
                    invoiceCount++;
                    rd.InvoiceDate = arDto.INVOICE_DATE;
                    rd.DueDate = arDto.DUE_DATE;
                    rd.LegalEntity = arDto.LegalEntity;
                    rd.EBName = arDto.EbName;
                    rd.SiteUseId = arDto.SiteUseId;
                    rd.CUSTOMER_NUM = arDto.CUSTOMER_NUM;
                }
                else
                {
                    errmsgSB.Append(errMsgHead + "Invoice number(" + rd.InvoiceNum + ") is invalid;\n");//发票已过期
                }
            }

            if (invoiceCount > 0 && invoiceCount < detailList.Count)
            {
                resultStr = errmsgSB.ToString() + "Please check whether the invoice number is correct or has been cleared.";
                return resultStr;
            }
            else if (invoiceCount == 0)
            {
                string artaskId = Guid.NewGuid().ToString();
                //调用算法,将订单号转换成发票号
                int detailSortId = 0;
                foreach (CaPMTDetailDto rd in detailList)
                {
                    List<CaReconMsgDetailDto> arList = getARListByOrderId(rd.InvoiceNum);
                    if (arList == null || arList.Count <= 0)
                    {
                        continue;
                    }
                    CaReconMsgDto reconMsgDto = new CaReconMsgDto();
                    List<CaBankStatementDto> bankList = new List<CaBankStatementDto>();
                    CaBankStatementDto bankdto = new CaBankStatementDto();
                    bankdto.AMOUNT = rd.Amount;
                    bankdto.TRANSACTION_NUMBER = rd.InvoiceNum;
                    bankList.Add(bankdto);

                    reconMsgDto.taskId = artaskId;
                    reconMsgDto.Total_AMT = rd.Amount;
                    reconMsgDto.bankList = bankList;
                    reconMsgDto.arList = arList;

                    // 添加到数据库中
                    CaBankStatementService caBankStatementService = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
                    caBankStatementService.insertReconTask(reconMsgDto.taskId, JsonConvert.SerializeObject(reconMsgDto), detailSortId++, AppContext.Current.User.EID);

                }
                CaTaskMsg msg = new CaTaskMsg();
                msg.taskId = artaskId;
                try
                {
                    string orderResult = CaReconService.paymentDetailRecon(msg);
                    if (String.Equals(orderResult, "success"))
                    {
                        ICaReconService reconService = SpringFactory.GetObjectImpl<ICaReconService>("CaReconService");
                        // 根据taskId获取实体
                        CaReconTaskDto taskDto = reconService.getCaReconTaskByTaskId(msg.taskId);
                        CaReconCallBackMsgDto callBackMsgDto = JsonConvert.DeserializeObject<CaReconCallBackMsgDto>(taskDto.OUTPUT);

                        List<string> arIds = callBackMsgDto.reconResult.ToList<string>();
                        //先判断是否为空
                        if (arIds == null || arIds.Count == 0)
                        {
                            //未能匹配任何invoice，给出错误提示
                            resultStr = "All invoice information is invalid, or all orders cannot match valid invoices.\n"
                                + "Please check whether the numbers filled are correct or have been cleared.";
                            return resultStr;
                        }
                        detailList = getARListByIds(arIds);
                    }
                    else
                    {
                        resultStr = "All invoice information is invalid, or all orders cannot match valid invoices.\n"
                                + "Please check whether the numbers filled are correct or have been cleared.";
                        return resultStr;
                    }

                }
                catch (Exception e)
                {
                    Helper.Log.Error(e.Message, e);
                    resultStr = "All invoice information is invalid, or all orders cannot match valid invoices.\n"
                                + "Please check whether the numbers filled are correct or have been cleared.";
                    return resultStr;
                }

            }

            if (currencyList.Distinct().ToList().Count() > 1)
            {
                resultStr = "Local Currency must be same one.";
                return resultStr;
            }
            else if (currencyList.Distinct().ToList().Count() == 1)
            {
                detailCurrency = currencyList.Distinct().ToList()[0];
            }

            if (invCurrencyList.Distinct().ToList().Count() > 1)
            {
                resultStr = "Inv Currency must be same one.";
                return resultStr;
            }
            else if (invCurrencyList.Distinct().ToList().Count() == 1)
            {
                detailInvCurrency = invCurrencyList.Distinct().ToList()[0];
            }

            decimal pmtAmount = 0;

            foreach (CaPMTDetailDto rd in detailList)
            {
                //为与页面提示信息区别开，导入提示时需要加上导入行号，页面录入则不需要
                if (!string.IsNullOrEmpty(pmt.ID))
                {
                    errMsgHead = "Upload failed. Row:" + rd.row + ", ";
                }
                legalEntitykeylist.Add(rd.LegalEntity);
                customerkeylist.Add(rd.CUSTOMER_NUM);
                if (index == 0)
                {
                    detailsql.Append("(SELECT ReconId FROM T_CA_PMTDetail with (nolock) WHERE SiteUseId = '");
                }
                else
                {
                    detailsql.Append(" INTERSECT (SELECT ReconId FROM T_CA_PMTDetail with (nolock) WHERE SiteUseId = '");
                }
                detailsql.Append(rd.SiteUseId + "' ");
                detailsql.Append("AND InvoiceNum = '");
                detailsql.Append(rd.InvoiceNum + "' ");
                detailsql.Append("AND Currency = '");
                detailsql.Append(rd.Currency + "' ");
                detailsql.Append("AND Amount = ");
                detailsql.Append(rd.Amount);//比较的是clear admount
                if (string.Equals(pmt.Currency, detailCurrency))
                {
                    pmtAmount = decimal.Add(pmtAmount, Convert.ToDecimal(rd.Amount));
                }
                else if (string.Equals(pmt.Currency, detailInvCurrency))
                {
                    if (string.IsNullOrEmpty(rd.Currency))
                    {
                        resultStr = errMsgHead + "The Inv Currency cannot be empty when the Bank Statement Currency and the Local Currency do not match.";
                        return resultStr;
                    }

                    if (rd.Amount == null)
                    {
                        resultStr = errMsgHead + "The Inv Amount cannot be empty when the Bank Statement Currency and the Local Currency do not match.";
                        return resultStr;
                    }
                    pmtAmount = decimal.Add(pmtAmount, Convert.ToDecimal(rd.LocalCurrencyAmount));
                }

                detailsql.Append(") ");
                index++;
            }
            detailsql.Append(") ");
            detailsql.Append("AND p.ISAPPLYGROUP = 0 ");
            detailsql.Append("AND p.ISPOSTGROUP = 0 ");

            //如果外币比较localAmount
            if (decimal.Compare(pmt.Amount ?? 0, pmtAmount) != 0)
            {
                resultStr = "The Payment Amount(" + pmt.Amount + ") and Invoice Amount(" + pmtAmount + ") must be equal.";
                return resultStr;
            }

            List<string> resultList = legalEntitykeylist.Distinct().ToList();
            if (resultList.Count > 1)
            {
                resultStr = "These invoice numbers must be the same Legal Entity.";
                return resultStr;
            }
            else if (resultList.Count == 1)
            {
                if (string.IsNullOrEmpty(pmt.LegalEntity))
                {
                    pmt.LegalEntity = resultList[0];
                }
                else if (!string.Equals(pmt.LegalEntity, resultList[0]))
                {
                    resultStr = "Payment Legal Entity(" + pmt.LegalEntity + ") and Invoice Legal Entity(" + resultList[0] + ") must be equal.";
                    return resultStr;
                }

            }

            List<string> customerResultList = customerkeylist.Distinct().ToList();
            if (customerResultList.Count > 1)
            {
                resultStr = "Please fill in the Customer Number, or check the invoices filled in to make sure they are all invoices of one customer.";
                return resultStr;
            }
            else if (customerResultList.Count == 1)
            {
                if (String.IsNullOrEmpty(pmt.CustomerNum))
                {
                    pmt.CustomerNum = customerResultList[0];
                }
                else
                {
                    if (!string.Equals(pmt.CustomerNum, customerResultList[0]))
                    {
                        resultStr = "Payment Customer(" + pmt.CustomerNum + ") and Invoice Customer(" + customerResultList[0] + ") must be equal.";
                        return resultStr;
                    }
                }

                //校验customerNumber是否正确
                if (isNotExistedCustomerNumber(pmt.CustomerNum, pmt.LegalEntity))
                {
                    resultStr = "The customer number dose not exist，or the customer and invoices are not in the same legal entity.\n"
                            + "Please confirm that the information filled in is correct.";
                    return resultStr;
                }
            }

            //检验发票是否已参与Recon            
            string invoiceNumStr = invoiceNumbers.ToString().Substring(1);
            invoiceNumStr = invoiceNumStr.Replace(",", "','");
            string queryReconSql = string.Format(@"select d.*, r.GroupNo
                                                from T_CA_ReconDetail d with (nolock), T_CA_Recon r with (nolock)
                                                where d.ReconId = r.ID
                                                and (r.GroupType not like 'UN-%')
                                                and d.InvoiceNum in ('{0}')
                                                order by r.GroupNo", invoiceNumStr);
            List<CaReconDetailDto> reconDetailList = SqlHelper.GetList<CaReconDetailDto>(SqlHelper.ExcuteTable(queryReconSql, System.Data.CommandType.Text));
            if (reconDetailList.Count > 0)
            {
                if (!deleteReconFlag)
                {
                    foreach (CaReconDetailDto rdDto in reconDetailList)
                    {
                        //如果有description信息，表明已确认，则直接跳过,反之给出提示
                        if (invoiceCount == 0 || invoices.ToString().IndexOf(rdDto.InvoiceNum) >= 0)
                        {
                            string bsID = getBankIDByRecon(rdDto.ReconId);
                            resultStr = "\nThe invoice(" + rdDto.InvoiceNum + ") has been settledd, the Transaction number is " + bsID + ".";
                        }

                    }

                    if (!string.IsNullOrEmpty(resultStr))
                    {
                        if (!string.IsNullOrEmpty(fileId))
                        {
                            resultStr = "fileId:" + fileId + ";;" + resultStr;
                        }
                        return resultStr;
                    }
                }

            }

            //检验是否有发票在别的pmt已存在
            string queryPMTDetailSql = string.Format(@"SELECT DISTINCT d.ReconId, t.GroupNo, d.InvoiceNum
                                                        FROM T_CA_PMTDetail d with (nolock)
                                                        INNER JOIN T_CA_PMT t with (nolock) ON t.ID = d.ReconId");

            if (!string.IsNullOrEmpty(pmt.ID))
            {
                queryPMTDetailSql += string.Format(@" WHERE InvoiceNum IN ('{0}')
                                                  AND t.ID != '{1}'
                                                 ORDER BY ReconId", invoiceNumStr, pmt.ID);
            }
            else
            {
                queryPMTDetailSql += string.Format(@" WHERE InvoiceNum IN ('{0}')
                                                    ORDER BY ReconId", invoiceNumStr);
            }
            List<CaPMTDetailDto> pmtDetaillist = SqlHelper.GetList<CaPMTDetailDto>(SqlHelper.ExcuteTable(queryPMTDetailSql, System.Data.CommandType.Text));

            if (pmtDetaillist.Count > 0)
            {
                //有就提醒用户
                CaPaymentDetailService caPaymentDetailService = SpringFactory.GetObjectImpl<CaPaymentDetailService>("CaPaymentDetailService");
                if (!deleteReconFlag)
                {
                    foreach (CaPMTDetailDto pMTDetailDto in pmtDetaillist)
                    {
                        //如果有description信息，表明已确认，则直接跳过,反之给出提示
                        if (invoiceCount == 0 || invoices.ToString().IndexOf(pMTDetailDto.InvoiceNum) >= 0)
                        {
                            resultStr = resultStr + "\nThe Invoice Number(" + pMTDetailDto.InvoiceNum + ")  has already be in an other Payment named "
                                + pMTDetailDto.GroupNo + ", plase pay attention and follow up the proccess.";
                        }
                    }
                }

                if (!string.IsNullOrEmpty(resultStr))
                {
                    return resultStr;
                }
            }

            //插入主表
            string groupNo = getNewGroupNo();
            pmt.GroupNo = groupNo;
            String reconId = string.Empty;
            if (string.IsNullOrEmpty(pmt.ID))
            {
                reconId = savePMT(pmt);
            }
            else
            {
                reconId = pmt.ID;
                updatePMT(pmt);

                //删除bs和detail数据
                string delPMTBSsql = string.Format(@"delete from T_CA_PMTBS where ReconId = '{0}'", reconId);
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(delPMTBSsql);

                string delPMTDetailsql = string.Format(@"delete from T_CA_PMTDetail where ReconId = '{0}'", reconId);
                CommonRep.GetDBContext().Database.ExecuteSqlCommand(delPMTDetailsql);
            }

            //插入Detail从表
            savePMTDetail(reconId, detailList);

            return resultStr;
        }

        private string doCheckPmtAll(CaPMTDto pmt, bool deleteReconFlag, string fileId)
        {
            string resultStr = string.Empty;
            string errMsgHead = string.Empty;
            List<CaPMTDto> pmtlist = new List<CaPMTDto>();
            List<CaPMTBSDto> bsList = pmt.PmtBs;
            List<CaPMTDetailDto> detailList = pmt.PmtDetail;

            int index = 0;
            List<string> customerkeylist = new List<string>();
            List<string> legalEntitykeylist = new List<string>();
            StringBuilder detailsql = new StringBuilder();
            StringBuilder invoiceNumbers = new StringBuilder();
            StringBuilder invoices = new StringBuilder();
            Dictionary<string, CaPMTDetailDto> pmtDetailMap = new Dictionary<string, CaPMTDetailDto>();

            string bsCurrency = string.Empty;
            string detailCurrency = string.Empty;
            string detailInvCurrency = string.Empty;
            List<string> currencyList = new List<string>();
            List<string> invCurrencyList = new List<string>();
            decimal bsAmount = 0;
            decimal detailAmount = 0;

            int invoiceCount = 0;
            StringBuilder errmsgSB = new StringBuilder();
            detailsql.Append("SELECT DISTINCT p.ID, p.GroupNo ");
            detailsql.Append("FROM T_CA_PMTDetail t with (nolock), T_CA_PMT p with (nolock) ");
            detailsql.Append("WHERE p.ID = t.ReconId ");
            detailsql.Append("AND t.ReconId IN(");

            CaBankStatementService caBankStatementService = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");

            foreach (CaPMTDetailDto rd in detailList)
            {
                if (!string.IsNullOrEmpty(pmt.ID))
                {
                    errMsgHead = "Upload failed. Row:" + rd.row + ", ";
                }

                //Local Currency不能为空
                if (string.IsNullOrEmpty(rd.Currency))
                {
                    resultStr = errMsgHead + "the Local Currency cannot be empty.";
                    return resultStr;
                }
                else
                {
                    currencyList.Add(rd.Currency);
                }

                //如果inv currency不为空，记录inv currency，并判读inv currency是否相同
                if (!string.IsNullOrEmpty(rd.LocalCurrency))
                {
                    invCurrencyList.Add(rd.LocalCurrency);
                }

                //记录哪些invoice有description，拆分payment的时候会用到
                if (invoiceNumbers.ToString().IndexOf(rd.InvoiceNum) < 0)
                {
                    invoiceNumbers.Append("," + rd.InvoiceNum);
                    if (string.IsNullOrEmpty(rd.Description))
                    {
                        invoices.Append("," + rd.InvoiceNum);
                    }
                }
                else
                {
                    resultStr = errMsgHead + "There are duplication of invoice numbers: " + rd.InvoiceNum;
                    return resultStr;
                }

                //校验发票号是否过期
                CaARDto arDto = getInvoiceByInvoiceNum(rd.InvoiceNum, rd.SiteUseId, pmt.CustomerNum);
                if (arDto != null)//如果未过期，则补全发票相关信息
                {
                    invoiceCount++;
                    rd.InvoiceDate = arDto.INVOICE_DATE;
                    rd.DueDate = arDto.DUE_DATE;
                    rd.LegalEntity = arDto.LegalEntity;
                    rd.SiteUseId = arDto.SiteUseId;
                    rd.CUSTOMER_NUM = arDto.CUSTOMER_NUM;
                    rd.EBName = arDto.EbName;
                }
                else
                {
                    errmsgSB.Append(errMsgHead + "Invoice number is invalid;\n");//发票已过期
                }
            }

            //校验发票是否重复
            if (invoiceCount > 0 && invoiceCount < detailList.Count)
            {
                errmsgSB.Append("Please check whether the invoice number is correct or has been cleared.");
                resultStr = errmsgSB.ToString();
                return resultStr;
            }
            else if (invoiceCount == 0)
            {
                string artaskId = Guid.NewGuid().ToString();
                //没有任何有效发票，则初步认定录入的是订单号，调用算法,将订单号转换成发票号
                int detailSortId = 0;
                foreach (CaPMTDetailDto rd in detailList)
                {
                    List<CaReconMsgDetailDto> arList = getARListByOrderId(rd.InvoiceNum);
                    if (arList == null || arList.Count <= 0)
                    {
                        continue;
                    }
                    CaReconMsgDto reconMsgDto = new CaReconMsgDto();
                    List<CaBankStatementDto> bankList = new List<CaBankStatementDto>();
                    CaBankStatementDto bankdto = new CaBankStatementDto();
                    bankdto.AMOUNT = rd.Amount;
                    bankdto.TRANSACTION_NUMBER = rd.InvoiceNum;

                    bankList.Add(bankdto);

                    reconMsgDto.taskId = artaskId;
                    reconMsgDto.Total_AMT = rd.Amount;
                    reconMsgDto.bankList = bankList;
                    reconMsgDto.arList = arList;

                    // 添加到数据库中
                    caBankStatementService.insertReconTask(reconMsgDto.taskId, JsonConvert.SerializeObject(reconMsgDto), detailSortId++, AppContext.Current.User.EID);
                }
                CaTaskMsg msg = new CaTaskMsg();
                msg.taskId = artaskId;
                try
                {
                    string orderResult = CaReconService.paymentDetailRecon(msg);
                    if (String.Equals(orderResult, "success"))
                    {
                        ICaReconService reconService = SpringFactory.GetObjectImpl<ICaReconService>("CaReconService");
                        // 根据taskId获取实体
                        CaReconTaskDto taskDto = reconService.getCaReconTaskByTaskId(msg.taskId);
                        CaReconCallBackMsgDto callBackMsgDto = JsonConvert.DeserializeObject<CaReconCallBackMsgDto>(taskDto.OUTPUT);

                        List<string> arIds = callBackMsgDto.reconResult.ToList<string>();
                        //先判断是否为空
                        if (arIds == null || arIds.Count == 0)
                        {
                            //未能匹配任何invoice，给出错误提示
                            resultStr = "All invoice information is invalid, or all orders cannot match valid invoices.\n"
                                + "Please check whether the numbers filled are correct or have been cleared.";
                            return resultStr;
                        }
                        detailList = getARListByIds(arIds);
                    }
                    else
                    {
                        //未能匹配任何invoice，给出错误提示
                        resultStr = "All invoice information is invalid, or all orders cannot match valid invoices.\n"
                            + "Please check whether the numbers filled are correct or have been cleared.";
                        return resultStr;
                    }
                }
                catch (Exception e)
                {
                    Helper.Log.Error(e.Message, e);
                    //未能匹配任何invoice，给出错误提示
                    resultStr = "All invoice information is invalid, or all orders cannot match valid invoices.\n"
                        + "Please check whether the numbers filled are correct or have been cleared.";
                    return resultStr;
                }
            }

            //判断录入的币制是否一致，若不一致则给出提示
            if (currencyList.Distinct().ToList().Count() > 1)
            {
                resultStr = "Local Currency must be same one.";
                return resultStr;
            }
            else if (currencyList.Distinct().ToList().Count() == 1)
            {
                detailCurrency = currencyList.Distinct().ToList()[0];
            }

            if (invCurrencyList.Distinct().ToList().Count() > 1)
            {
                resultStr = "Inv Currency must be same one.";
                return resultStr;
            }
            else if (invCurrencyList.Distinct().ToList().Count() == 1)
            {
                detailInvCurrency = invCurrencyList.Distinct().ToList()[0];
            }

            foreach (CaPMTDetailDto rd in detailList)
            {
                if (!string.IsNullOrEmpty(pmt.ID))
                {
                    errMsgHead = "Upload failed. Row:" + rd.row + ", ";
                }
                pmtDetailMap.Add(rd.InvoiceNum, rd);
                legalEntitykeylist.Add(rd.LegalEntity);
                customerkeylist.Add(rd.CUSTOMER_NUM);
                if (index == 0)
                {
                    detailsql.Append("(SELECT ReconId FROM T_CA_PMTDetail with (nolock) WHERE SiteUseId = '");
                }
                else
                {
                    detailsql.Append(" INTERSECT (SELECT ReconId FROM T_CA_PMTDetail with (nolock) WHERE SiteUseId = '");
                }
                detailsql.Append(rd.SiteUseId + "' ");
                detailsql.Append("AND InvoiceNum = '");
                detailsql.Append(rd.InvoiceNum + "' ");
                detailsql.Append("AND Currency = '");
                detailsql.Append(rd.Currency + "' ");
                detailsql.Append("AND Amount = ");
                detailsql.Append(rd.Amount);//比较的是clear admount

                detailsql.Append(") ");
                index++;
            }
            detailsql.Append(") ");
            detailsql.Append("AND p.ISAPPLYGROUP = 0 ");
            detailsql.Append("AND p.ISPOSTGROUP = 0 ");

            List<string> resultList = legalEntitykeylist.Distinct().ToList();

            //判断录入发票的legal Entity是否一致
            if (resultList.Count > 1)
            {
                resultStr = "These invoice numbers must be the same Legal Entity.";
                return resultStr;
            }
            else if (resultList.Count == 1)
            {
                //若excel未录入legal Entity， 则将发票的legal Entity赋值给pmt.LegalEntity
                if (string.IsNullOrEmpty(pmt.LegalEntity))
                {
                    pmt.LegalEntity = resultList[0];
                }
                else if (!string.Equals(pmt.LegalEntity, resultList[0]))
                {
                    //若excel录入了legal Entity，则比较录入的legal Entity和发票解析出来的legal Entity是否一致，不一致给出提示
                    resultStr = "Payment Legal Entity(" + pmt.LegalEntity + ") and Invoice Legal Entity(" + resultList[0] + ") must be equal.";
                    return resultStr;
                }

            }

            //校验customer number
            List<string> customerResultList = customerkeylist.Distinct().ToList();
            //若发票解析出来的customer不唯一，给出提示
            if (customerResultList.Count > 1)
            {
                resultStr = "Please fill in the Customer Number, or check the invoices filled in to make sure they are all invoices of one customer.";
                return resultStr;
            }
            else if (customerResultList.Count == 1)
            {
                //若excel未录入customer，则从发票中获取customer number
                if (String.IsNullOrEmpty(pmt.CustomerNum))
                {
                    pmt.CustomerNum = customerResultList[0];
                }
                else
                {
                    //若excel录入了customer，则判断录入的customer与发票解析出来的customer是否一致，不一致给出提示
                    if (!string.Equals(pmt.CustomerNum, customerResultList[0]))
                    {
                        resultStr = "Payment Customer(" + pmt.CustomerNum + ") and Invoice Customer(" + customerResultList[0] + ") must be equal.";
                        return resultStr;
                    }
                }

                //校验customerNumber是否正确
                if (isNotExistedCustomerNumber(pmt.CustomerNum, pmt.LegalEntity))
                {
                    resultStr = "The customer number dose not exist，or the customer and invoices are not in the same legal entity.\n"
                            + "Please confirm that the information filled in is correct.";
                    return resultStr;
                }
            }

            StringBuilder bankIdsSb = new StringBuilder();
            StringBuilder bssql = new StringBuilder();
            List<string> bsCurrencyList = new List<string>();
            int bsIndex = 0;

            bssql.Append("SELECT DISTINCT t.ID, t.GroupNo ");
            bssql.Append("FROM T_CA_PMTBS b with (nolock), T_CA_PMT t with (nolock) ");
            bssql.Append("WHERE t.ID = b.ReconId ");
            bssql.Append("AND ReconId IN(");

            foreach (CaPMTBSDto rb in bsList)
            {
                //为与页面提示信息区别开，导入提示时需要加上导入行号，页面录入则不需要
                if (!string.IsNullOrEmpty(pmt.ID))
                {
                    errMsgHead = "Upload failed. Row:" + rb.row + ", ";
                }
                if (String.IsNullOrEmpty(rb.TransactionNumber))
                {
                    resultStr = errMsgHead + "Transaction number cannot be empty.";
                    return resultStr;
                }
                else
                {
                    //检验bankstatementId是否存在
                    List<CaBankStatementDto> bankList = getBanksByTransactionNum(rb.TransactionNumber, pmt.LegalEntity);

                    if (bankList == null || bankList.Count() <= 0)
                    {
                        resultStr = errMsgHead + "Transaction number must exists in the legal entity(" + pmt.LegalEntity + ")";
                        return resultStr;
                    }
                    else
                    {
                        //若存在，则补全bank信息
                        CaBankStatementDto bank = bankList[0];
                        if (bankIdsSb.ToString().IndexOf(bank.ID) < 0)
                        {
                            bankIdsSb.Append("," + bank.ID);
                            rb.BANK_STATEMENT_ID = bank.ID;
                            rb.LegalEntity = bank.LegalEntity;
                            rb.ValueDate = bank.VALUE_DATE;
                            rb.Currency = bank.CURRENCY.ToUpper();
                            rb.currentAmount = bank.CURRENT_AMOUNT;
                            rb.BankCharge = rb.BankCharge ?? (bank.BankChargeTo ?? decimal.Zero);
                            rb.BankCharge = Math.Round(Convert.ToDecimal(rb.BankCharge), 5);
                            rb.currentAmount = Math.Round(Convert.ToDecimal(rb.currentAmount), 5);
                            //transaction amount 与 clear amount比较的时候，需要加上bankCharge
                            Decimal currentAmt = Decimal.Add(rb.currentAmount ?? 0, rb.BankCharge ?? 0);
                            if (Decimal.Compare(rb.Amount ?? 0, currentAmt) > 0)
                            {
                                resultStr = "The Current Amount(" + rb.currentAmount + ") cannot be less than the Clear Amount(" + rb.Amount + ").";
                                return resultStr;
                            }
                            if (bank.TRANSACTION_AMOUNT != null)
                            {
                                rb.TransactionAmount = bank.TRANSACTION_AMOUNT;//记录一下transaction amount,拆分payment后要插入到主表中去
                            }

                            //判断Bank是否被使用
                            CaReconDto recon = getBankReconResult(bank.ID);
                            //如果有bank
                            if (recon != null && !string.IsNullOrEmpty(recon.PMT_ID))
                            {
                                if (recon.isClosed)
                                {
                                    //如果被使用,分情况提示
                                    if (string.Equals(bank.CLEARING_STATUS, "1"))
                                    {
                                        resultStr = "The Bank Statement(" + bank.TRANSACTION_NUMBER + ") is clearing now, cannot import once again.";
                                        return resultStr;
                                    }
                                    else if (string.Equals(bank.CLEARING_STATUS, "2"))
                                    {
                                        resultStr = "The Bank Statement(" + bank.TRANSACTION_NUMBER + ") has been cleared, cannot import once again.";
                                        return resultStr;
                                    }
                                }
                            }
                        }
                        else
                        {
                            resultStr = errMsgHead + "There are duplication of transation number: " + rb.TransactionNumber;
                            return resultStr;
                        }
                    }

                    if (bsIndex == 0)
                    {
                        bssql.Append("(SELECT ReconId FROM T_CA_PMTBS with (nolock) WHERE BANK_STATEMENT_ID = '");
                    }
                    else
                    {
                        bssql.Append(" INTERSECT (SELECT ReconId FROM T_CA_PMTBS with (nolock) WHERE BANK_STATEMENT_ID = '");
                    }

                    bssql.Append(rb.BANK_STATEMENT_ID);
                    bssql.Append("' AND Currency = '");
                    bssql.Append(rb.Currency);
                    bssql.Append("' AND Amount = ");
                    bssql.Append(rb.Amount);
                    bssql.Append(" AND BankCharge = ");
                    bssql.Append(rb.BankCharge ?? 0);
                    bssql.Append(") ");
                    bsAmount = decimal.Add(bsAmount, rb.Amount ?? 0);
                    bsCurrencyList.Add(rb.Currency);
                    bsIndex++;
                }
            }
            bssql.Append(") ");
            bssql.Append("AND t.ISAPPLYGROUP = 0 ");
            bssql.Append("AND t.ISPOSTGROUP = 0 ");

            //校验录入的bank的币制信息是否唯一
            if (bsCurrencyList.Distinct().ToList().Count() > 1)
            {
                resultStr = "Bank Statements must be same currency. ";
                return resultStr;
            }
            else
            {
                bsCurrency = bsList[0].Currency;
                //判读当bs currency不等于local currency时 需要校验后面的 Inv currency必填
                if (!string.Equals(bsCurrency.ToUpper(), detailCurrency.ToUpper()))
                {
                    if (string.IsNullOrEmpty(detailInvCurrency))
                    {
                        resultStr = "The Inv Currency cannot be Empty when the Bank Statement Currency and the Local Currency aren`t equal.";
                        return resultStr;
                    }
                }
            }


            //验证是否需要拆分recon
            string invoiceNumStr = invoiceNumbers.ToString().Substring(1);
            invoiceNumStr = invoiceNumStr.Replace(",", "','");
            string queryReconSql = string.Format(@"select d.*, r.GroupNo
                                                from T_CA_ReconDetail d with (nolock), T_CA_Recon r with (nolock)
                                                where d.ReconId = r.ID
                                                and (r.GroupType not like 'UN-%')
                                                and d.InvoiceNum in ('{0}')
                                                and r.GroupType != 'NM' 
                                                and r.GroupType not like 'UN%'
                                                order by r.GroupNo", invoiceNumStr);
            List<CaReconDetailDto> reconDetailList = SqlHelper.GetList<CaReconDetailDto>(SqlHelper.ExcuteTable(queryReconSql, System.Data.CommandType.Text));
            if (reconDetailList.Count > 0)
            {
                if (!deleteReconFlag)
                {
                    foreach (CaReconDetailDto rdDto in reconDetailList)
                    {
                        //如果有description信息，表明已确认，则直接跳过,反之给出提示
                        if (invoiceCount == 0 || invoices.ToString().IndexOf(rdDto.InvoiceNum) >= 0)
                        {
                            string bsID = getBankIDByRecon(rdDto.ReconId);
                            resultStr = "\nThe invoice(" + rdDto.InvoiceNum + ") has been settledd, the Transaction number is " + bsID + ".";
                        }

                    }

                    if (!string.IsNullOrEmpty(resultStr))
                    {
                        if (!string.IsNullOrEmpty(fileId))
                        {
                            resultStr = "fileId:" + fileId + ";;" + resultStr;
                        }
                        return resultStr;
                    }
                }

            }

            //检验是否有发票在别的pmt已存在
            string queryPMTDetailSql = string.Format(@"SELECT DISTINCT d.ReconId, t.GroupNo, d.InvoiceNum
                                                        FROM T_CA_PMTDetail d with (nolock)
                                                        INNER JOIN T_CA_PMT t with (nolock) ON t.ID = d.ReconId");

            if (!string.IsNullOrEmpty(pmt.ID))
            {
                queryPMTDetailSql += string.Format(@" WHERE InvoiceNum IN ('{0}')
                                                  AND t.ID != '{1}'
                                                 ORDER BY ReconId", invoiceNumStr, pmt.ID);
            }
            else
            {
                queryPMTDetailSql += string.Format(@" WHERE InvoiceNum IN ('{0}')
                                                    ORDER BY ReconId", invoiceNumStr);
            }
            List<CaPMTDetailDto> pmtDetaillist = SqlHelper.GetList<CaPMTDetailDto>(SqlHelper.ExcuteTable(queryPMTDetailSql, System.Data.CommandType.Text));
            if (pmtDetaillist.Count > 0)
            {
                //有就提醒用户
                CaPaymentDetailService caPaymentDetailService = SpringFactory.GetObjectImpl<CaPaymentDetailService>("CaPaymentDetailService");
                if (!deleteReconFlag)
                {
                    foreach (CaPMTDetailDto pMTDetailDto in pmtDetaillist)
                    {
                        //有description的invoice，表明已确认，直接插入payment，若没有则给出提示
                        if (invoiceCount == 0 || invoices.ToString().IndexOf(pMTDetailDto.InvoiceNum) >= 0)
                        {
                            resultStr = resultStr + "\nThe Invoice Number(" + pMTDetailDto.InvoiceNum + ")  has already be in an other Payment named "
                                + pMTDetailDto.GroupNo + ", plase pay attention and follow up the proccess.";
                        }
                    }
                }


                if (!string.IsNullOrEmpty(resultStr))
                {
                    return resultStr;
                }
            }

            //调用算法，匹配bank和invoice
            string taskId = Guid.NewGuid().ToString();
            CaReconMsgDto splitMsgDto = new CaReconMsgDto();
            List<CaReconMsgDetailDto> invoiceList = getARListByInvoiceNum(invoiceNumbers.ToString().Substring(1));
            Dictionary<string, int> pmtbsIndex = new Dictionary<string, int>();

            //bank的币制与本币相同
            if (string.Equals(bsCurrency, detailCurrency))
            {
                foreach (CaReconMsgDetailDto dto in invoiceList)
                {
                    dto.AMT = pmtDetailMap[dto.ID].Amount ?? 0;
                    detailAmount = decimal.Add(detailAmount, dto.AMT);
                }
            }
            else if (string.Equals(bsCurrency, detailInvCurrency))//bank的币制与外币相同
            {
                List<CaReconMsgDetailDto> functinvoiceList = new List<CaReconMsgDetailDto>();
                foreach (CaReconMsgDetailDto dto in invoiceList)
                {
                    if (string.IsNullOrEmpty(pmt.ID))
                    {
                        errMsgHead = "Upload failed. Row:" + pmtDetailMap[dto.ID].row + ", ";
                    }

                    if (pmtDetailMap[dto.ID].LocalCurrencyAmount != null)
                    {
                        dto.AMT = pmtDetailMap[dto.ID].LocalCurrencyAmount ?? 0;
                    }
                    else
                    {
                        resultStr = errMsgHead + "Inv Amount cannot be Empty.";
                        return resultStr;
                    }
                    detailAmount = decimal.Add(detailAmount, dto.AMT);

                    //本币金额也要传给算法
                    CaReconMsgDetailDto functdto = new CaReconMsgDetailDto();
                    functdto.DUE_DATE = dto.DUE_DATE;
                    functdto.ID = dto.ID;
                    functdto.AMT = pmtDetailMap[dto.ID].Amount ?? 0;
                    functinvoiceList.Add(functdto);
                }
                splitMsgDto.funcArList = functinvoiceList;
            }
            else
            {
                resultStr = "The Currency of Bank Statements and the Currency of Invoices must be the same one.";
                return resultStr;
            }

            if (decimal.Compare(bsAmount, detailAmount) != 0)
            {
                resultStr = "Bank Statement Amount(" + bsAmount + ") and Invoice Amount(" + detailAmount + ") must be equal!";
                return resultStr;
            }
            List<CaBankStatementDto> bankStatmentList = new List<CaBankStatementDto>();
            int n = 0;
            foreach (CaPMTBSDto bsp in bsList)
            {
                CaBankStatementDto bankDto = new CaBankStatementDto();
                // clear amt目前为最终销账的金额，无需再加bankcharge
                bankDto.AMOUNT = bsp.Amount ?? 0;
                bankDto.VALUE_DATE = bsp.ValueDate;
                bankDto.ID = bsp.BANK_STATEMENT_ID;
                bankStatmentList.Add(bankDto);
                pmtbsIndex.Add(bsp.BANK_STATEMENT_ID, n++);
            }
            splitMsgDto.taskId = taskId;
            splitMsgDto.bankList = bankStatmentList;
            splitMsgDto.arList = invoiceList;

            // 添加到数据库中
            caBankStatementService.insertReconTask(splitMsgDto.taskId, JsonConvert.SerializeObject(splitMsgDto), 1, AppContext.Current.User.EID);

            try
            {
                CaReconMsgResultDto result = CaReconService.splitRecon(splitMsgDto);
                if (!String.Equals(result.status, "-1"))
                {

                    reconResultDto[] reconResultList = result.reconResult;

                    //先判断是否为空
                    if (reconResultList == null || reconResultList.Count() <= 0)
                    {
                        resultStr = "Cannot match any Invoice Numbers.";
                        return resultStr;
                    }
                    else
                    {
                        foreach (reconResultDto dto in reconResultList)
                        {
                            CaPMTDto pmtdto = new CaPMTDto();
                            pmtdto.CustomerNum = pmt.CustomerNum;
                            pmtdto.LegalEntity = pmt.LegalEntity;
                            pmtdto.Currency = detailCurrency;
                            pmtdto.ValueDate = pmt.ValueDate;
                            pmtdto.TASK_ID = pmt.TASK_ID;
                            pmtdto.ReceiveDate = pmt.ReceiveDate;
                            pmtdto.businessId = pmt.businessId;
                            pmtdto.CREATE_DATE = AppContext.Current.User.Now;

                            List<CaPMTBSDto> blist = new List<CaPMTBSDto>();
                            CaPMTBSDto bs = bsList[pmtbsIndex[dto.KEY]];
                            pmtdto.TransactionAmount = bs.TransactionAmount;//查询出transaction amount需要插入主表
                            blist.Add(bs);
                            pmtdto.PmtBs = blist;
                            pmtdto.ValueDate = bs.ValueDate;

                            decimal sumAmount = 0;
                            decimal sumLocalAmount = 0;
                            List<CaPMTDetailDto> dlist = new List<CaPMTDetailDto>();
                            if (string.Equals(bsCurrency, detailCurrency))
                            {
                                foreach (reconResultGroupDto g in dto.GROUP)
                                {
                                    if (Decimal.Compare(g.AMOUNT ?? 0, 0) == 0)
                                    {
                                        continue;
                                    }
                                    CaPMTDetailDto d = new CaPMTDetailDto();
                                    d.CUSTOMER_NUM = pmtDetailMap[g.INV].CUSTOMER_NUM;
                                    d.SiteUseId = pmtDetailMap[g.INV].SiteUseId;
                                    d.Description = pmtDetailMap[g.INV].Description;
                                    d.DueDate = pmtDetailMap[g.INV].DueDate;
                                    d.EBName = pmtDetailMap[g.INV].EBName;
                                    d.InvoiceNum = pmtDetailMap[g.INV].InvoiceNum;
                                    d.Currency = detailCurrency;
                                    d.Amount = g.AMOUNT;
                                    d.LocalCurrencyAmount = null;
                                    d.LocalCurrency = "";
                                    sumAmount = decimal.Add(sumAmount, d.Amount ?? 0);
                                    dlist.Add(d);
                                }
                            }
                            else
                            {
                                foreach (reconResultGroupDto g in dto.GROUP)
                                {
                                    if (Decimal.Compare(g.AMOUNT ?? 0, 0) == 0)
                                    {
                                        continue;
                                    }
                                    CaPMTDetailDto d = new CaPMTDetailDto();
                                    d.CUSTOMER_NUM = pmtDetailMap[g.INV].CUSTOMER_NUM;
                                    d.SiteUseId = pmtDetailMap[g.INV].SiteUseId;
                                    d.Description = pmtDetailMap[g.INV].Description;
                                    d.DueDate = pmtDetailMap[g.INV].DueDate;
                                    d.EBName = pmtDetailMap[g.INV].EBName;
                                    d.InvoiceNum = pmtDetailMap[g.INV].InvoiceNum;
                                    d.Currency = detailCurrency;
                                    d.Amount = g.FUNCAMOUNT;
                                    d.LocalCurrencyAmount = g.AMOUNT;
                                    d.LocalCurrency = detailInvCurrency;
                                    sumLocalAmount = decimal.Add(sumLocalAmount, g.AMOUNT ?? 0);
                                    sumAmount = decimal.Add(sumAmount, d.Amount ?? 0);
                                    dlist.Add(d);
                                }
                                pmtdto.LocalCurrencyAmount = sumLocalAmount;
                                pmtdto.LocalCurrency = detailInvCurrency;
                            }
                            pmtdto.Amount = sumAmount;
                            pmtdto.PmtDetail = dlist;
                            pmtdto.filename = pmt.filename;
                            pmtlist.Add(pmtdto);
                        }
                    }

                }
                else
                {
                    resultStr = "Cannot match any Invoice Numbers.";
                    return resultStr;
                }
            }
            catch (Exception e)
            {
                Helper.Log.Error(e.Message, e);
                resultStr = "Cannot match any Invoice Numbers.";
                return resultStr;
            }

            //修改task
            //将解析出来的bs.id 插入 taskBS表
            if (!string.IsNullOrEmpty(pmt.TASK_ID))
            {
                CaTaskService caTaskService = SpringFactory.GetObjectImpl<CaTaskService>("CaTaskService");
                string[] bankIdArr = bankIdsSb.ToString().Split(',');
                caTaskService.createTaskBS(pmt.TASK_ID, bankIdArr);
            }

            //Create Recon
            bool lbCreateRecon = true;
            foreach (CaPMTDto pdto in pmtlist)
            {
                //插入主表
                string groupNo = getNewGroupNo();
                pdto.GroupNo = groupNo;
                Helper.Log.Info("*********************** savepmtall:" + pdto.businessId);
                String reconId = savePMT(pdto);

                //插入从表
                savePMTBS(reconId, pdto.CustomerNum, pdto.PmtBs, pdto.SiteUseId);

                //插入Detail从表
                savePMTDetail(reconId, pdto.PmtDetail);

                //将状态为matched的bank statement，状态应该变为unmatched
                updateBankMatchStatus(pdto.PmtBs,"2");

                //判断是否可以直接形成Recon组合
                if (pdto.PmtBs == null || pdto.PmtBs.Count == 0) { lbCreateRecon = false; }
                foreach (CaPMTBSDto pb in pdto.PmtBs) {
                    if (string.IsNullOrEmpty(pb.TransactionNumber)) {
                        lbCreateRecon = false;
                    }
                }
                string strBSSiteUseId = "";
                if (pdto.PmtDetail == null || pdto.PmtDetail.Count == 0)
                {
                    lbCreateRecon = false;
                }
                else if(!string.IsNullOrEmpty(pdto.PmtDetail[0].SiteUseId))
                {
                    strBSSiteUseId = pdto.PmtDetail[0].SiteUseId;
                } else {
                    strBSSiteUseId = pdto.SiteUseId;
                }
                
                //可以直接形成Recon组合
                if (lbCreateRecon)
                {
                    List<string> bsIdsList = new List<string>();
                    foreach (CaPMTBSDto pb in pdto.PmtBs) {
                        bsIdsList.Add(pb.TransactionNumber);
                    }
                    DateTime now = DateTime.Now;
                    CaReconService reconService = SpringFactory.GetObjectImpl<CaReconService>("CaReconService");
                    ICaTaskService taskService = SpringFactory.GetObjectImpl<ICaTaskService>("CaTaskService");
                    string reconTaskId = taskService.createTask(9, bsIdsList.ToArray(), "", "", now);
                    reconService.createReconGroupByBSId("", reconTaskId, reconId);
                    updateBankMatchStatus1(pdto.PmtBs, "4", "Base on PMT", strBSSiteUseId);
                }
            }


            return resultStr;
        }

        public string savePMT(CaPMTDto caPMTDto)
        {
            Helper.Log.Info("savepmt*****************:" + caPMTDto.businessId);
            string reconId = Guid.NewGuid().ToString();
            StringBuilder insertSql = new StringBuilder();
            insertSql.Append("INSERT INTO T_CA_PMT (");
            insertSql.Append("[ID], [LegalEntity], [GroupNo],[ValueDate],[filename],");
            if (!String.IsNullOrEmpty(caPMTDto.CustomerNum))
            {
                insertSql.Append("[CustomerNum],");
            }
            if (!String.IsNullOrEmpty(caPMTDto.Currency))
            {
                insertSql.Append("[Currency],");
            }
            if (caPMTDto.TransactionAmount != null)
            {
                insertSql.Append("[TransactionAmount],");
            }
            if (caPMTDto.Amount != null)
            {
                insertSql.Append("[Amount],");
            }
            if (!String.IsNullOrEmpty(caPMTDto.LocalCurrency))
            {
                insertSql.Append("[LocalCurrency],");
            }
            if (caPMTDto.LocalCurrencyAmount != null)
            {
                insertSql.Append("[LocalCurrencyAmount],");
            }
            if (caPMTDto.BankCharge != null)
            {
                insertSql.Append("[BankCharge],");
            }
            insertSql.Append("[ReceiveDate],");
            insertSql.Append("[TASK_ID], [CREATE_USER], [CREATE_DATE], [SiteUseId], [BusinessId]) ");
            insertSql.Append("VALUES");
            insertSql.Append("(N'" + reconId + "', ");
            insertSql.Append("N'" + caPMTDto.LegalEntity + "', ");
            insertSql.Append("N'" + caPMTDto.GroupNo + "', ");
            insertSql.Append("'" + caPMTDto.ValueDate + "', ");
            insertSql.Append("'" + caPMTDto.filename.Replace("'", "''") + "', ");
            if (!String.IsNullOrEmpty(caPMTDto.CustomerNum))
            {
                insertSql.Append("N'" + caPMTDto.CustomerNum + "', ");
            }
            if (!String.IsNullOrEmpty(caPMTDto.Currency))
            {
                insertSql.Append("N'" + caPMTDto.Currency + "', ");
            }
            if (caPMTDto.TransactionAmount != null)
            {
                insertSql.Append(caPMTDto.TransactionAmount + ", ");
            }
            if (caPMTDto.Amount != null)
            {
                insertSql.Append(caPMTDto.Amount + ", ");
            }
            if (!String.IsNullOrEmpty(caPMTDto.LocalCurrency))
            {
                insertSql.Append("N'" + caPMTDto.LocalCurrency + "', ");
            }
            if (caPMTDto.LocalCurrencyAmount != null)
            {
                insertSql.Append(caPMTDto.LocalCurrencyAmount + ", ");
            }
            if (caPMTDto.BankCharge != null)
            {
                insertSql.Append(caPMTDto.BankCharge + ", ");
            }
            insertSql.Append("'" + caPMTDto.ReceiveDate + "',");
            insertSql.Append("N'" + caPMTDto.TASK_ID + "', ");
            insertSql.Append("N'" + AppContext.Current.User.EID + "', ");
            insertSql.Append("'" + caPMTDto.CREATE_DATE + "','" + caPMTDto.SiteUseId + "',");
            insertSql.Append("N'" + caPMTDto.businessId + "' ) ");

            Helper.Log.Info("*****************:" + insertSql.ToString());
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(insertSql.ToString());
            return reconId;
        }

        public void savePMTBS(string reconId, string customerNumber, List<CaPMTBSDto> pmtBSList, string siteUseId)
        {
            List<string> listSQL = new List<string>();
            List<SqlParameter[]> listSQLParams = new List<SqlParameter[]>();
            int sortId = 1;
            foreach (CaPMTBSDto pb in pmtBSList)
            {
                decimal bankCharge = pb.BankCharge ?? 0;
                decimal amount = pb.Amount ?? 0;
                //无论bank是否被其他操作使用，都不能影响payment 修改bank的customer和状态信息
                string queryNoCustomerSql = string.Format(@"select count(*) from T_CA_BankStatement with (nolock)  
                                                   where ID = '{0}' and MATCH_STATUS in ('-1', '0', '2')", pb.BANK_STATEMENT_ID);
                int noCustomercount = SqlHelper.ExcuteScalar<int>(queryNoCustomerSql);

                string queryNoForwardSql = string.Format(@"SELECT
	                                                            COUNT (*)
                                                            FROM
	                                                            T_CA_BankStatement with (nolock) 
                                                            WHERE
	                                                            ID = '{0}'
                                                            AND MATCH_STATUS IN ('-1', '0', '2')
                                                            AND ISNULL(FORWARD_NUM, '') = ISNULL(CUSTOMER_NUM, '')", pb.BANK_STATEMENT_ID);
                int noForwardcount = SqlHelper.ExcuteScalar<int>(queryNoForwardSql);

                StringBuilder sql = new StringBuilder();
                StringBuilder updatebanksql = new StringBuilder();
                sql.Append("INSERT INTO T_CA_PMTBS(");
                sql.Append("ID, ReconId, SortId, BANK_STATEMENT_ID, Currency, Amount, BankCharge");
                sql.Append(") VALUES ( ");
                sql.Append("@ID,");
                sql.Append("@ReconId,");
                sql.Append("@SortId,");
                sql.Append("@BANK_STATEMENT_ID,");
                sql.Append("@Currency,");
                sql.Append("@Amount, ");
                sql.Append("@BankCharge)");
                listSQL.Add(sql.ToString());
                SqlParameter[] parms1 = { new SqlParameter("@ID", Guid.NewGuid()),
                                         new SqlParameter("@ReconId", reconId),
                                         new SqlParameter("@SortId", sortId),
                                         new SqlParameter("@BANK_STATEMENT_ID", pb.BANK_STATEMENT_ID),
                                         new SqlParameter("@Currency", pb.Currency),
                                         new SqlParameter("@Amount", amount),
                                         new SqlParameter("@BankCharge", bankCharge) };
                listSQLParams.Add(parms1);

                updatebanksql.Append("update T_CA_BankStatement ");
                updatebanksql.Append("set ISHISTORY = 0");

                if (noCustomercount > 0)
                {
                    updatebanksql.Append(", MATCH_STATUS = '2'");
                    updatebanksql.Append(", SiteUseId = @SiteUseId");
                    updatebanksql.Append(", CUSTOMER_NUM = @CUSTOMER_NUM");
                    updatebanksql.Append(", CUSTOMER_NAME = ");
                    updatebanksql.Append("(SELECT TOP 1 CUSTOMER_NAME FROM V_CA_Customer with (nolock) WHERE CUSTOMER_NUM = @CUSTOMER_NUM)");

                    if (noForwardcount > 0)
                    {
                        updatebanksql.Append(", FORWARD_NUM = @CUSTOMER_NUM");
                        updatebanksql.Append(", FORWARD_NAME = ");
                        updatebanksql.Append("(SELECT TOP 1 CUSTOMER_NAME FROM V_CA_Customer with (nolock) WHERE CUSTOMER_NUM = @CUSTOMER_NUM)");
                    }
                }

                updatebanksql.Append(" where ID = '");
                updatebanksql.Append(pb.BANK_STATEMENT_ID + "'");
                listSQL.Add(updatebanksql.ToString());

                if (siteUseId == null) { siteUseId = ""; }
                SqlParameter[] parms2 = { new SqlParameter("@CUSTOMER_NUM", customerNumber), new SqlParameter("@SiteUseId", siteUseId) };
                listSQLParams.Add(parms2);

                //如果有BankCharge，回写BankStatement表
                if (bankCharge > 0)
                {
                    string updateBankChargeSql = string.Format(@"update T_CA_BankStatement 
                                    set BankChargeFrom = @BankChargeFrom, BankChargeTo = @BankChargeFrom
                                    where ID = @ID");
                    listSQL.Add(updateBankChargeSql);
                    SqlParameter[] parms3 = { new SqlParameter("@BankChargeFrom", bankCharge),
                                              new SqlParameter("@ID", pb.BANK_STATEMENT_ID)
                                            };
                    listSQLParams.Add(parms3);
                }
                sortId++;
            }
            SqlHelper.ExcuteListSql(listSQL, listSQLParams);
        }

        public void savePMTDetail(string reconId, List<CaPMTDetailDto> pmtDetailList)
        {
            List<string> listSQL = new List<string>();
            int sortId = 1;
            foreach (CaPMTDetailDto pd in pmtDetailList)
            {
                StringBuilder sql = new StringBuilder();

                sql.Append("INSERT INTO T_CA_PMTDetail(");
                sql.Append("ID, ReconId, SortId, CUSTOMER_NUM, SiteUseId, InvoiceNum,");
                sql.Append("InvoiceDate, DueDate,");
                if (!string.IsNullOrEmpty(pd.Currency))
                {
                    sql.Append("Currency,");
                }

                if (pd.Amount != null)
                {
                    sql.Append("Amount,");
                }

                if (!string.IsNullOrEmpty(pd.LocalCurrency))
                {
                    sql.Append("LocalCurrency,");
                }

                if (pd.LocalCurrencyAmount != null)
                {
                    sql.Append("LocalCurrencyAmount, ");
                }
                sql.Append("EBName");
                sql.Append(") VALUES ( ");
                sql.Append("'" + Guid.NewGuid() + "',");
                sql.Append("'" + reconId + "',");
                sql.Append(sortId + ",");
                sql.Append("'" + pd.CUSTOMER_NUM + "',");
                sql.Append("'" + pd.SiteUseId + "',");
                sql.Append("'" + pd.InvoiceNum + "',");
                sql.Append("'" + pd.InvoiceDate + "',");
                sql.Append("'" + pd.DueDate + "',");
                if (!string.IsNullOrEmpty(pd.Currency))
                {
                    sql.Append("'" + pd.Currency + "',");
                }

                if (pd.Amount != null)
                {
                    sql.Append(pd.Amount + ",");
                }

                if (!string.IsNullOrEmpty(pd.LocalCurrency))
                {
                    sql.Append("'" + pd.LocalCurrency + "',");
                }

                if (pd.LocalCurrencyAmount != null)
                {
                    sql.Append(pd.LocalCurrencyAmount + ",");
                }

                sql.Append("'" + pd.EBName + "')");
                listSQL.Add(sql.ToString());
                sortId++;
            }

            SqlHelper.ExcuteListSql(listSQL);
        }

        private string getNewGroupNo()
        {
            string groupNo = getKey("PMT");
            return groupNo;
        }

        private List<CaBankStatementDto> getBanksByTransactionNum(string transactionInc, string legalEntity)
        {
            if (string.IsNullOrEmpty(legalEntity) || legalEntity == "undefined" || legalEntity == "null")
            {
                legalEntity = "";
            }
            string sql = String.Format(@"select *
                                        from T_CA_BankStatement with (nolock) 
                                        where TRANSACTION_NUMBER = '{0}'
                                        and (LegalEntity = '{1}' OR '' = '{1}')
                                        and DEL_FLAG = 0", transactionInc, legalEntity);
            List<CaBankStatementDto> list = SqlHelper.GetList<CaBankStatementDto>(SqlHelper.ExcuteTable(sql, System.Data.CommandType.Text));
            return list;
        }

        private CaARDto getInvoiceByInvoiceNum(string invoiceNum, string siteUseId, string customerNum)
        {
            string sql = String.Format(@"SELECT DISTINCT
	                                        t0.INVOICE_NUM,
	                                        t0.INVOICE_DATE,
	                                        t0.INV_CURRENCY,
	                                        t0.LegalEntity,
	                                        t0.SiteUseId,
	                                        t0.DUE_DATE,
	                                        t0.CUSTOMER_NUM,
                                            t0.Local_AMT,
                                            t0.Ebname,
                                            t0.OrderNumber
                                        FROM V_CA_AR_CM t0                                         
                                        WHERE t0.INVOICE_NUM = '{0}'
                                        ORDER BY t0.DUE_DATE DESC",
                                            invoiceNum);


            List<CaARDto> list = SqlHelper.GetList<CaARDto>(SqlHelper.ExcuteTable(sql, System.Data.CommandType.Text));
            if (list != null && list.Count > 0)
            {
                return list.FirstOrDefault<CaARDto>();
            }
            else
            {
                return null;
            }

        }

        private bool isNotExistedCustomerNumber(string customerNumber, string legalEntity)
        {
            bool isNotExisted = true;
            if (string.IsNullOrEmpty(legalEntity) || legalEntity == "undefined" || legalEntity == "null")
            {
                legalEntity = "";
            }
            string sql = String.Format(@"select count(*) as NUM
                                        from V_CA_Customer 
                                        where CUSTOMER_NUM = '{0}'
                                        and (LegalEntity = '{1}' OR '' = '{1}')",
                                        customerNumber,
                                        legalEntity);
            int count = SqlHelper.ExcuteScalar<int>(sql);
            if (count > 0)
            {
                isNotExisted = false;
            }
            return isNotExisted;
        }

        private List<CaPMTDto> queryPMT(CaPMTDto caPMT)
        {
            if (String.IsNullOrEmpty(caPMT.CustomerNum))
            {
                caPMT.CustomerNum = "";
            }
            if (caPMT.Amount == null)
            {
                caPMT.Amount = 0;
            }

            string sql = String.Format(@"select ID, GroupNo
                                        from T_CA_PMT with (nolock)
                                        where LegalEntity = '{0}'
                                        and (CustomerNum = '{1}' OR '' = '{1}')
                                        and Currency = '{2}'
                                        AND(Amount = {3} OR 0 = {3} )
                                        and ISAPPLYGROUP = 0
                                        and ISPOSTGROUP = 0", caPMT.LegalEntity, caPMT.CustomerNum, caPMT.Currency, caPMT.Amount);

            List<CaPMTDto> pmtList = SqlHelper.GetList<CaPMTDto>(SqlHelper.ExcuteTable(sql, System.Data.CommandType.Text));
            return pmtList;
        }

        public string getKey_inservice(System.Data.Entity.Database db, string type)
        {
            SqlParameter[] paramList = new SqlParameter[2];
            var paramBuildType = new SqlParameter
            {
                ParameterName = "@sCode",
                Value = type,
                Direction = ParameterDirection.Input
            };
            var paramBuildResultValue = new SqlParameter
            {
                ParameterName = "@return_value",
                Size = 30,
                Value = "".PadLeft(30, ' '),
                Direction = ParameterDirection.Output
            };
            paramList[0] = paramBuildType;
            paramList[1] = paramBuildResultValue;
            db.ExecuteSqlCommand("p_CA_GetSerialNo @sCode, @return_value out", paramList);
            return paramList[1].Value.ToString();
        }

        public string getKey(string type)
        {
            SqlParameter[] paramList = new SqlParameter[2];
            var paramBuildType = new SqlParameter
            {
                ParameterName = "@sCode",
                Value = type,
                Direction = ParameterDirection.Input
            };
            var paramBuildResultValue = new SqlParameter
            {
                ParameterName = "@return_value",
                Size = 30,
                Value = "".PadLeft(30, ' '),
                Direction = ParameterDirection.Output
            };
            paramList[0] = paramBuildType;
            paramList[1] = paramBuildResultValue;
            SqlHelper.ExecuteNonQuery(CommandType.StoredProcedure, "p_CA_GetSerialNo", paramList);
            return paramList[1].Value.ToString();
        }

        public CaActionTaskPage getActionTaskList(string transactionNumber, string status, string currency, string dateF, string dateT, int page, int pageSize)
        {

            CaActionTaskPage result = new CaActionTaskPage();
            var userId = AppContext.Current.User.EID; //当前用户ID
            string collecotrList = "";
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");
            if (!string.IsNullOrEmpty(collecotrList))
            {
                collecotrList = collecotrList.Substring(0, collecotrList.LastIndexOf(","));
                collecotrList = collecotrList.Replace(",", "','");

            }
            collecotrList = "'" + collecotrList + "'";

            string sql = string.Format(@"SELECT
	                                *
                                FROM
	                                (
		                                SELECT
			                                ROW_NUMBER () OVER (ORDER BY t0.CREATE_DATE DESC, t0.VALUE_DATE DESC) AS RowNumber,                                            
                                            t0.LegalEntity,
                                            t0.TRANSACTION_NUMBER,
                                            t0.VALUE_DATE,
                                            t0.CURRENCY,
                                            t0.TRANSACTION_AMOUNT,
                                            t0.CURRENT_AMOUNT,
                                            t0.Description,
                                            t0.MATCH_STATUS,
                                            t0.CREATE_DATE,
                                            t0.UPLOADTIME,
                                            t0.UPLOADFILEID,
                                            t0.UPLOADFILENAME,
                                            t0.UPLOADFILEPATH,
                                            t0.HASUPLOADFILE,
                                            t0.IDENTIFY_TIME,
                                            t0.IDENTIFY_TASKID,
                                            t0.ADVISOR_TIME,
                                            t0.ADVISOR_TASKID,
                                            t0.ADVISOR_MailId,
                                            t0.ADVISOR_MailDate,
                                            t0.HASADVISORMAIL,
                                            t0.RECON_TIME,
                                            t0.RECON_TASKID,
                                            t0.Adjustment_time,
                                            t0.PMTMAIL_DATE,
                                            t0.PMTMAIL_MESSAGEID,
                                            t0.APPLY_TIME,
                                            t0.CLEARING_TIME,
                                            t0.POSTFILENAME,
                                            t0.POSTFILEPATH,
                                            t0.haspostfile,
                                            t0.CLEARFILENAME,
                                            t0.CLEARFILEPATH,
                                            t0.hasclearfile,
                                            t0.REF1,
                                            t1.DETAIL_NAME AS MATCH_STATUS_NAME
		                                FROM
			                               V_CA_ACTIONTASK as t0
                                        INNER JOIN T_SYS_TYPE_DETAIL t1 with (nolock)  ON t0.MATCH_STATUS = t1.DETAIL_VALUE AND T1.TYPE_CODE='088'
                                        WHERE t0.CREATE_USER IN ({7})
                                        AND t0.DEL_FLAG = '0'
                                        AND ((t0.TRANSACTION_NUMBER like '%{2}%') OR '' = '{2}')
                                        AND (t0.MATCH_STATUS = '{3}' OR '' = '{3}')
                                        AND (t0.CURRENCY = '{4}' OR '' = '{4}')                                                                             
                                        AND (t0.VALUE_DATE >= '{5} 00:00:00' OR '' = '{5}')
                                        AND (t0.VALUE_DATE <= '{6} 23:59:59' OR '' = '{6}')
	                                ) AS t
                                WHERE
	                                RowNumber BETWEEN {0} AND {1}", page == 1 ? 0 : pageSize * (page - 1) + 1, pageSize * page, transactionNumber, status, currency, dateF, dateT, collecotrList);


            List<CaActionTaskDto> dto = CommonRep.ExecuteSqlQuery<CaActionTaskDto>(sql).ToList();

            string sql1 = string.Format(@"select count(1) as count from V_CA_ACTIONTASK 
                                        where CREATE_USER IN ({5})
                                        AND ((TRANSACTION_NUMBER like '%{0}%') OR '' = '{0}')
                                        AND (MATCH_STATUS = '{1}' OR '' = '{1}')
                                        AND (CURRENCY = '{2}' OR '' = '{2}')                                                                             
                                        AND (VALUE_DATE >= '{3} 00:00:00' OR '' = '{3}')
                                        AND (VALUE_DATE <= '{4} 23:59:59' OR '' = '{4}')", transactionNumber, status, currency, dateF, dateT, collecotrList);

            result.dataRows = dto;
            result.count = SqlHelper.ExcuteScalar<int>(sql1);

            return result;
        }

        public string postAndClear(string type, string[] bsid, string taskId, string userId, int isAutoRecon = 0)
        {
            List<CaBankStatementPostDto> listApply = new List<CaBankStatementPostDto>();
            List<CaBankStatementClearDto> listClear = new List<CaBankStatementClearDto>();
            List<CaBankStatementClearDto> listClearNoVAT = new List<CaBankStatementClearDto>();
            List<CaBankStatementDto> listNoPost = new List<CaBankStatementDto>();

            DateTime dt_Now = DateTime.Now;
            List<string> listSQL = new List<string>();
            StringBuilder errmsg = new StringBuilder();

            //生成文件
            string strTotalFileId = "";
            string[] bsidold = bsid;

            try
            {
                List<string> listLockSql = new List<string>();
                foreach (string bsiditem in bsid)
                {
                    listLockSql.Add("Update T_CA_BankStatement set ISLOCKED = 1 where ID = '" + bsiditem + "'");
                }
                SqlHelper.ExcuteListSql(listLockSql);

                if (type == "1")
                {
                    CaBankStatementService bankService = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
                    CaReconService reconService = SpringFactory.GetObjectImpl<CaReconService>("CaReconService");

                    List<string> bsIdList = new List<string>();

                    foreach (string bankId in bsid)
                    {
                        CaBankStatementDto bank = bankService.getBankStatementById(bankId);

                        string customerNum = bank.CUSTOMER_NUM == null ? "" : bank.CUSTOMER_NUM;
                        string forwardName = bank.FORWARD_NAME == null ? "" : bank.FORWARD_NAME;
                        string legalEntity = bank.LegalEntity == null ? "" : bank.LegalEntity;

                        // 查询cuAttr.IsFactoring是否为true，若为true则查询bs是否有组合，若无组合则无法post
                        CaCustomerAttributeDto cuAttr = bankService.getCustomerAttrByCustomerNum(customerNum, legalEntity) ?? new CaCustomerAttributeDto();
                        bool isFactoring = cuAttr.IsFactoring ?? false;

                        if (isFactoring)
                        {
                            string reconId = reconService.getLastReconIdByBsId(bankId);
                            if (!string.IsNullOrEmpty(reconId))
                            {
                                bsIdList.Add(bankId);
                            }
                            else
                            {
                                //Factory的，如果没有AR销账明细，则Post时生成Request PMT Mail
                                string checkSQL = string.Format("select COUNT(*) from t_ca_mailalert where bsid = '{0}' and AlertType = '006'", bank.ID);
                                int hasSend = SqlHelper.ExcuteScalar<int>(checkSQL);

                                //只有CA Team启用Confirm send mail
                                string strCurrentUser = AppContext.Current.User.EID;
                                SysTypeDetail caMember = (from ca in CommonRep.GetDbSet<SysTypeDetail>()
                                                          where ca.TypeCode == "100" && ca.DetailName == strCurrentUser
                                                          select ca).FirstOrDefault();

                                if (hasSend == 0 && caMember != null && !string.IsNullOrEmpty(caMember.DetailValue) 
                                     && (bank.MATCH_STATUS == "-1" || bank.MATCH_STATUS == "0" || bank.MATCH_STATUS == "2"))
                                {
                                    //插入PMT Mail发送记录
                                    string strId = Guid.NewGuid().ToString();
                                    string strBSId = bank.ID;
                                    string strLegalEntity = bank.LegalEntity;
                                    string strCUSTOMER_NUM = bank.CUSTOMER_NUM == null ? "" : bank.CUSTOMER_NUM;
                                    string strSiteUseId = bank.SiteUseId == null ? "" : bank.SiteUseId;
                                    string strTRANSACTION_NUMBER = bank.TRANSACTION_NUMBER;
                                    decimal? decCURRENT_AMOUNT = bank.CURRENT_AMOUNT == null ? 0 : Convert.ToDecimal(bank.CURRENT_AMOUNT);
                                    SysTypeDetail toCc = (from ca in CommonRep.GetDbSet<SysTypeDetail>()
                                                          where ca.TypeCode == "086" && ca.DetailName == "CAPMTFactory"
                                                          select ca).FirstOrDefault();
                                    string strTo = "";
                                    string strCc = "";
                                    if (toCc != null)
                                    {
                                        strTo = toCc.DetailValue;
                                        strCc = toCc.DetailValue2;
                                    }
                                    string sqlMail = string.Format(@"IF NOT EXISTS (SELECT 1 FROM T_CA_MailAlert WHERE BSID = '{1}' AND AlertType = '006') INSERT INTO t_ca_mailalert (ID, BSID, AlertType, EID, TransNumber, LegalEntity, CustomerNum, SiteUseId, Amount, ToTitle, CCTitle ) values ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', {8}, '{9}', '{10}')", strId, strBSId, "006", AppContext.Current.User.EID, strTRANSACTION_NUMBER, strLegalEntity, strCUSTOMER_NUM, strSiteUseId, decCURRENT_AMOUNT, strTo, strCc);
                                    listSQL.Add(sqlMail);
                                }
                            }
                        }
                        else
                        {
                            bsIdList.Add(bankId);
                        }

                        // 判断是否需要清除bankCharge
                        if (checkDetailNameExists(forwardName, legalEntity, "095"))
                        {
                            bank.BankChargeFrom = 0;
                            bank.BankChargeTo = 0;
                            bankService.updateBank(bank);
                        }
                    }

                    bsid = bsIdList.ToArray();

                    if (bsid.Length == 0)
                    {
                        errmsg.Append("No Bank Statement Can Be Post!");
                    }
                    else
                    {
                        //仅入账
                        listApply = getPostList(bsid, type, isAutoRecon);
                        // 入账后将siteUseId写到BS上
                        if (listApply.Count > 0)
                        {
                            foreach (CaBankStatementPostDto post in listApply)
                            {
                                listSQL.Add("UPDATE T_CA_BankStatement SET APPLY_STATUS = '1', APPLY_TIME='" + dt_Now + "',SiteUseId='" + post.SiteUseId + "' WHERE ID = '" + post.ID + "'");
                            }
                            SqlHelper.ExcuteListSql(listSQL);
                            listSQL = new List<string>();
                        }
                    }
                }
                else if (type == "2")
                {
                    //只查销账
                    listClear = getClearList(bsid, type, listApply, true);
                    listClearNoVAT = getClearList(bsid, type, listApply, false);
                    //将销账中混入的未post的bank挑出来，并给出提示信息
                    listNoPost = getNoPostlist(bsid);
                    if (listNoPost.Count > 0)
                    {
                        errmsg.Append("Clear success but with error:");
                    }
                    foreach (CaBankStatementDto bank in listNoPost)
                    {
                        errmsg.Append("\nThe Bank Statement(" + bank.TRANSACTION_NUMBER + ") has not been posted yet, please post it first.");
                    }
                }

                //File2-仅入账

                if (listApply.Count > 0)
                {
                    string templateFile = ConfigurationManager.AppSettings["TemplateCAPost"].ToString().TrimStart('~').Replace("/", "\\").TrimStart('\\');
                    templateFile = Path.Combine(HttpRuntime.AppDomainAppPath, templateFile);
                    string tmpFile = Path.Combine(Path.GetTempPath(), "Post_" + userId + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx");
                    NpoiHelper helper = new NpoiHelper(templateFile);
                    helper.Save(tmpFile, true);
                    helper = new NpoiHelper(tmpFile);
                    int intStartRow = 1;
                    int rowcount = 0;
                    foreach (CaBankStatementPostDto applyPost in listApply)
                    {
                        int colnum = 1;
                        helper.SetData(intStartRow + rowcount, colnum++, rowcount + 1);
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.LegalEntity);
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.Currency);
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.ReceiptsMethod);
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.BankAccountNumber);
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.RVNumber);
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.ReceiptsDate == null ? "" : ((DateTime)applyPost.ReceiptsDate).ToString("dd-MMM-yyyy"));
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.MaturityDate == null ? "" : ((DateTime)applyPost.MaturityDate).ToString("dd-MMM-yyyy"));//增加maturity date
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.CheckNumber);//增加check number
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.NetReceiptsAmount);
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.BankCharge);
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.CustomerNumber);
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.SiteUseId);

                        //listSQL.Add(string.Format("upate t_ca_bankstatement set siteuseid = '{0}' where id='{1}'", applyPost.SiteUseId, applyPost.ID));

                        string dateStr = "";
                        //2.payment details中添加一个字段，填写收 到 payment details的日期；
                        CaBankStatementService bankService = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
                        CaPaymentDetailService pmtService = SpringFactory.GetObjectImpl<CaPaymentDetailService>("CaPaymentDetailService");
                        List<CaBankStatementDto> bankList = bankService.GetBankListByTranNum(applyPost.RVNumber);
                        if (null != bankList && bankList.Count > 0)
                        {
                            string bsId = bankList[0].ID;
                            if (applyPost.PMTReceiveDate != null)
                            {
                                dateStr = ((DateTime)applyPost.PMTReceiveDate).ToString("dd-MMM-yyyy");
                            }
                            else
                            {
                                dateStr = ((DateTime)bankList[0].CREATE_DATE).ToString("dd-MMM-yyyy");
                            }
                            //1. 如果存在代付关系，需要输出Forwarder 名称
                            if (!(bankList[0].CUSTOMER_NUM ?? "").Equals(bankList[0].FORWARD_NUM ?? ""))
                            {
                                dateStr += "; " + bankList[0].FORWARD_NAME ?? "";
                            }
                        }

                        //根据siteuseid，获得creditterm
                        var strCreditterm = (from x in CommonRep.GetDbSet<Customer>()
                                       .Where(o => o.SiteUseId == applyPost.SiteUseId)
                                             select x.CreditTrem).FirstOrDefault();

                        helper.SetData(intStartRow + rowcount, colnum++, dateStr);
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.Ref1);
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.CustomerName);
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.EBName);
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.BSComments);
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.PMTGroupNo);
                        helper.SetData(intStartRow + rowcount, colnum++, strCreditterm);
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.PMTFileName);
                        helper.SetData(intStartRow + rowcount, colnum++, applyPost.PMTReceiveDate);

                        if (rowcount % 2 > 0)
                        {
                            helper.SetForegroundColor(intStartRow + rowcount, 0, intStartRow + rowcount, colnum, NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index);
                        }

                        //3.没有payment details的，按照上传bank statement的日期
                        //入账历史记录
                        StringBuilder strPostHis = new StringBuilder();
                        strPostHis.Append("INSERT INTO T_CA_PostClearHistory ( Date,Type,LegalEntity,BSTransactionInc,BSCurrency,BSAmount,CustomerNum)");
                        strPostHis.Append(" VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd") + "',");
                        strPostHis.Append("         'Post',");
                        strPostHis.Append("         '" + applyPost.IOTCLegalEntity + "',");
                        strPostHis.Append("         '" + applyPost.RVNumber + "',");
                        strPostHis.Append("         '" + applyPost.Currency + "',");
                        strPostHis.Append("          " + applyPost.NetReceiptsAmount + ",");
                        strPostHis.Append("         '" + applyPost.CustomerNumber + "')");
                        listSQL.Add(strPostHis.ToString());
                        rowcount++;
                    }
                    helper.Save(tmpFile, true);
                    //插入T_File
                    string strFileId = System.Guid.NewGuid().ToString();
                    string strFileName = Path.GetFileName(tmpFile);
                    StringBuilder strFileSql = new StringBuilder();
                    strFileSql.Append("INSERT INTO T_FILE ( FILE_ID,FILE_NAME,PHYSICAL_PATH,OPERATOR,CREATE_TIME)");
                    strFileSql.Append(" VALUES (N'" + strFileId + "',");
                    strFileSql.Append("         N'" + strFileName + "',");
                    strFileSql.Append("         N'" + tmpFile + "',");
                    strFileSql.Append("         N'" + userId + "',getdate())");
                    listSQL.Add(strFileSql.ToString());
                    if (!string.IsNullOrEmpty(strTotalFileId))
                    {
                        strTotalFileId += ";" + strFileId;
                    }
                    else
                    {
                        strTotalFileId = strFileId;
                    }
                }
                //File3-仅销账
                if (listClear.Count > 0)
                {
                    //取消入销同时，不用区分仅销账，查询出来的都是仅销账的
                    listClear = listClear.OrderBy(a => a.RVNumber).ThenBy(a => a.InvoiceNumber).ThenBy(a => a.InvoiceSiteUseId).ToList<CaBankStatementClearDto>();

                    List<List<CaBankStatementClearDto>> listGroup = new List<List<CaBankStatementClearDto>>();
                    int maxRowNum = 5000;//超过单文件最大行数，就需要再增加一个文件
                    int groupRowNum = maxRowNum;
                    int fileNumber = 1;
                    string fileType = ".xlsx";
                    string tmpFilePath = Path.Combine(Path.GetTempPath(), "Clear_" + userId + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                    for (int i = 0; i < listClear.Count; i += maxRowNum)//以maxRowNum为一组分组
                    {
                        List<CaBankStatementClearDto> clearList = new List<CaBankStatementClearDto>();
                        clearList = listClear.Take(groupRowNum).Skip(i).ToList();
                        groupRowNum += maxRowNum;
                        listGroup.Add(clearList);
                    }

                    foreach (List<CaBankStatementClearDto> list in listGroup)
                    {
                        string templateFile = ConfigurationManager.AppSettings["TemplateCAClear"].ToString().TrimStart('~').Replace("/", "\\").TrimStart('\\');
                        templateFile = Path.Combine(HttpRuntime.AppDomainAppPath, templateFile);
                        NpoiHelper helper = new NpoiHelper(templateFile);
                        string tmpFile = tmpFilePath + "_" + fileNumber + fileType;
                        fileNumber++;
                        helper.Save(tmpFile, true);
                        helper = new NpoiHelper(tmpFile);
                        int intStartRow = 1;
                        int rowcount = 0;
                        //取消入销同时，不用区分仅销账，查询出来的都是仅销账的
                        List<CaBankStatementClearDto> clearDtoList = new List<CaBankStatementClearDto>();
                        List<CaBankStatementClearDto> clearNeedRepostDtoList = new List<CaBankStatementClearDto>();
                        helper.ActiveSheet = 0;

                        // 拆分list
                        foreach (CaBankStatementClearDto applyClear in list)
                        {

                            List<CaBankStatementClearDto> rvlist = listClear.FindAll(a => a.RVNumber == applyClear.RVNumber).ToList<CaBankStatementClearDto>();
                            int siteUseIdCount = rvlist.GroupBy(s => s.InvoiceSiteUseId, s => s).ToList().Count;

                            //RV Number 下 只有一个siteUseId 再判断siteUseId与invoice siteUseId是否相等，若不同，则需要repost
                            if (string.IsNullOrEmpty(applyClear.RVSiteUseId))
                            {
                                applyClear.RVSiteUseId = "";
                            }

                            if (siteUseIdCount == 1)
                            {
                                if (applyClear.RVSiteUseId.Equals(applyClear.InvoiceSiteUseId))
                                {
                                    clearDtoList.Add(applyClear);
                                }
                                else
                                {
                                    clearNeedRepostDtoList.Add(applyClear);
                                }
                            }
                            else
                            {
                                //RV Number 下 若有多个siteUseId，需要repost
                                clearNeedRepostDtoList.Add(applyClear);
                            }

                        }

                        ICellStyle cellHighLightStyles1 = helper.Book.CreateCellStyle();
                        cellHighLightStyles1.FillPattern = FillPattern.SolidForeground;
                        cellHighLightStyles1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.DarkYellow.Index;

                        ICellStyle cellHighLightStyles2 = helper.Book.CreateCellStyle();
                        cellHighLightStyles2.FillPattern = FillPattern.SolidForeground;
                        cellHighLightStyles2.FillForegroundColor = 10;

                        ICellStyle cellHighLightStyles3 = helper.Book.CreateCellStyle();
                        cellHighLightStyles3.FillPattern = FillPattern.SolidForeground;
                        cellHighLightStyles3.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                        
                        foreach (CaBankStatementClearDto applyClear in clearDtoList)
                        {
                            int nextcol = 0;
                            string rvSiteUseId = (applyClear.RVSiteUseId ?? "").Equals(applyClear.InvoiceSiteUseId ?? "") ? "TRUE" : "FALSE";
                            string dateStr = "";
                            //2.payment details中添加一个字段，填写收 到 payment details的日期；
                            CaBankStatementService bankService = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
                            CaPaymentDetailService pmtService = SpringFactory.GetObjectImpl<CaPaymentDetailService>("CaPaymentDetailService");
                            List<CaBankStatementDto> bankList = bankService.GetBankListByTranNum(applyClear.RVNumber);
                            if (null != bankList && bankList.Count > 0)
                            {
                                string bsId = bankList[0].ID;
                                CaPMTDto pmt = pmtService.getPMTByBsId(bsId);
                                if (null != pmt && pmt.ReceiveDate != null)
                                {
                                    dateStr = ((DateTime)pmt.ReceiveDate).ToString("dd-MMM-yyyy");
                                }
                                else
                                {
                                    dateStr = ((DateTime)bankList[0].CREATE_DATE).ToString("dd-MMM-yyyy");
                                }
                                //1. 如果存在代付关系，需要输出Forwarder 名称  且只有需要show forward customer的才会显示
                                if (checkShowForwardByLegalEntity(bankList[0].LegalEntity) && !bankList[0].CUSTOMER_NUM.Equals(bankList[0].FORWARD_NUM))
                                {
                                    dateStr += " " + bankList[0].FORWARD_NAME;
                                }
                            }

                            if (string.Equals(rvSiteUseId, "FALSE"))
                            {
                                if (rowcount % 2 > 0)
                                {
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.CustomerNum, cellHighLightStyles1);//customer Account Number
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.RVNumber, cellHighLightStyles1);//Receipt Number 
                                    helper.SetData(intStartRow + rowcount, nextcol++, ((DateTime)applyClear.RVDate).ToString("dd-MMM-yyyy"), cellHighLightStyles1);//Receipt Date
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.RVAmount, cellHighLightStyles1);//Receipt Amount
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.InvoiceNumber, cellHighLightStyles1);//TRX Number
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.AmountApplied, cellHighLightStyles1);//Amount APPlied
                                    helper.SetData(intStartRow + rowcount, nextcol++, ((DateTime)applyClear.InvoiceDate).ToString("dd-MMM-yyyy"), cellHighLightStyles1);//TRX Date
                                    helper.SetData(intStartRow + rowcount, nextcol++, dateStr, cellHighLightStyles1);//Comments
                                    helper.SetData(intStartRow + rowcount, nextcol++, dt_Now.ToString("dd-MMM-yyyy"), cellHighLightStyles1);//Apply Date
                                    helper.SetData(intStartRow + rowcount, nextcol++, "", cellHighLightStyles1);//GL Date
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.Country, cellHighLightStyles1);//Country
                                    nextcol = nextcol + 2;//跳过result列和空列
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.Ebname, cellHighLightStyles1);//EB
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.BSComments, cellHighLightStyles1);//BS Comments
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PMTGroupNo, cellHighLightStyles1);//GroupNo
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PaymentTerm, cellHighLightStyles1);//PaymentTerm
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PMTFileName, cellHighLightStyles1);//PMTFileName
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PMTReceiveDate, cellHighLightStyles1);//PMTReceiveDate
                                }
                                else
                                {
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.CustomerNum, cellHighLightStyles2);//customer Account Number
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.RVNumber, cellHighLightStyles2);//Receipt Number 
                                    helper.SetData(intStartRow + rowcount, nextcol++, ((DateTime)applyClear.RVDate).ToString("dd-MMM-yyyy"), cellHighLightStyles2);//Receipt Date
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.RVAmount, cellHighLightStyles2);//Receipt Amount
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.InvoiceNumber, cellHighLightStyles2);//TRX Number
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.AmountApplied, cellHighLightStyles2);//Amount APPlied
                                    helper.SetData(intStartRow + rowcount, nextcol++, ((DateTime)applyClear.InvoiceDate).ToString("dd-MMM-yyyy"), cellHighLightStyles2);//TRX Date
                                    helper.SetData(intStartRow + rowcount, nextcol++, dateStr, cellHighLightStyles2);//Comments
                                    helper.SetData(intStartRow + rowcount, nextcol++, dt_Now.ToString("dd-MMM-yyyy"), cellHighLightStyles2);//Apply Date
                                    helper.SetData(intStartRow + rowcount, nextcol++, "", cellHighLightStyles2);//GL Date
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.Country, cellHighLightStyles2);//Country
                                    nextcol = nextcol + 2;//跳过result列和空列
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.Ebname, cellHighLightStyles2);//EB
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.BSComments, cellHighLightStyles2);//BS Comments
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PMTGroupNo, cellHighLightStyles2);//GroupNo
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PaymentTerm, cellHighLightStyles2);//PaymentTerm
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PMTFileName, cellHighLightStyles2);//PMTFileName
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PMTReceiveDate, cellHighLightStyles2);//PMTReceiveDate
                                }
                            }
                            else
                            {
                                if (rowcount % 2 > 0)
                                {
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.CustomerNum, cellHighLightStyles3);//customer Account Number
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.RVNumber, cellHighLightStyles3);//Receipt Number 
                                    helper.SetData(intStartRow + rowcount, nextcol++, ((DateTime)applyClear.RVDate).ToString("dd-MMM-yyyy"), cellHighLightStyles3);//Receipt Date
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.RVAmount, cellHighLightStyles3);//Receipt Amount
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.InvoiceNumber, cellHighLightStyles3);//TRX Number
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.AmountApplied, cellHighLightStyles3);//Amount APPlied
                                    helper.SetData(intStartRow + rowcount, nextcol++, ((DateTime)applyClear.InvoiceDate).ToString("dd-MMM-yyyy"), cellHighLightStyles3);//TRX Date
                                    helper.SetData(intStartRow + rowcount, nextcol++, dateStr, cellHighLightStyles3);//Comments
                                    helper.SetData(intStartRow + rowcount, nextcol++, dt_Now.ToString("dd-MMM-yyyy"), cellHighLightStyles3);//Apply Date
                                    helper.SetData(intStartRow + rowcount, nextcol++, "", cellHighLightStyles3);//GL Date
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.Country, cellHighLightStyles3);//Country
                                    nextcol = nextcol + 2;//跳过result列和空列
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.Ebname, cellHighLightStyles3);//EB
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.BSComments, cellHighLightStyles3);//BS Comments
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PMTGroupNo, cellHighLightStyles3);//GroupNo
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PaymentTerm, cellHighLightStyles3);//PaymentTerm
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PMTFileName, cellHighLightStyles3);//PMTFileName
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PMTReceiveDate, cellHighLightStyles3);//PMTReceiveDate
                                }
                                else
                                {
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.CustomerNum);//customer Account Number
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.RVNumber);//Receipt Number 
                                    helper.SetData(intStartRow + rowcount, nextcol++, ((DateTime)applyClear.RVDate).ToString("dd-MMM-yyyy"));//Receipt Date
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.RVAmount);//Receipt Amount
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.InvoiceNumber);//TRX Number
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.AmountApplied);//Amount APPlied
                                    helper.SetData(intStartRow + rowcount, nextcol++, ((DateTime)applyClear.InvoiceDate).ToString("dd-MMM-yyyy"));//TRX Date
                                    helper.SetData(intStartRow + rowcount, nextcol++, dateStr);//Comments
                                    helper.SetData(intStartRow + rowcount, nextcol++, dt_Now.ToString("dd-MMM-yyyy"));//Apply Date
                                    helper.SetData(intStartRow + rowcount, nextcol++, "");//GL Date
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.Country);//Country
                                    nextcol = nextcol + 2;//跳过result列和空列
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.Ebname);//EB
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.BSComments);//BS Comments
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PMTGroupNo);//GroupNo
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PaymentTerm);//PaymentTerm
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PMTFileName);//PMTFileName
                                    helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PMTReceiveDate);//PMTReceiveDate
                                }
                            }

                            //销账历史记录
                            StringBuilder strClearHis = new StringBuilder();
                            strClearHis.Append("INSERT INTO T_CA_PostClearHistory ( Date,Type,LegalEntity,BSTransactionInc,BSCurrency,CustomerNum,SiteUseId,InvoiceNum,ClearAmount)");
                            strClearHis.Append(" VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd") + "',");
                            strClearHis.Append("         'Clear',");
                            strClearHis.Append("         '" + applyClear.IOTCLegalEntity + "',");
                            strClearHis.Append("         '" + applyClear.RVNumber + "',");
                            strClearHis.Append("         '" + applyClear.Currency + "',");
                            strClearHis.Append("         '" + applyClear.CustomerNum + "',");
                            strClearHis.Append("         '" + applyClear.InvoiceSiteUseId + "',");
                            strClearHis.Append("         '" + applyClear.InvoiceNumber + "',");
                            strClearHis.Append("         " + applyClear.AmountApplied + ")");
                            listSQL.Add(strClearHis.ToString());

                            rowcount++;
                        }

                        intStartRow = 1;
                        rowcount = 0;
                        helper.ActiveSheet = 1;

                        foreach (CaBankStatementClearDto applyClear in clearNeedRepostDtoList)
                        {
                            int nextcol = 0;
                            string rvSiteUseId = applyClear.RVSiteUseId.Equals(applyClear.InvoiceSiteUseId) ? "TRUE" : "FALSE";
                            
                            string dateStr = "";
                            //2.payment details中添加一个字段，填写收 到 payment details的日期；
                            CaBankStatementService bankService = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
                            CaPaymentDetailService pmtService = SpringFactory.GetObjectImpl<CaPaymentDetailService>("CaPaymentDetailService");
                            List<CaBankStatementDto> bankList = bankService.GetBankListByTranNum(applyClear.RVNumber);
                            if (null != bankList && bankList.Count > 0)
                            {
                                string bsId = bankList[0].ID;
                                CaPMTDto pmt = pmtService.getPMTByBsId(bsId);
                                if (null != pmt && pmt.ReceiveDate != null)
                                {
                                    dateStr = ((DateTime)pmt.ReceiveDate).ToString("dd-MMM-yyyy");
                                    if (bankList[0].CREATE_DATE > pmt.ReceiveDate)
                                    {
                                        dateStr = ((DateTime)bankList[0].CREATE_DATE).ToString("dd-MMM-yyyy");
                                    }
                                }
                                else
                                {
                                    dateStr = ((DateTime)bankList[0].CREATE_DATE).ToString("dd-MMM-yyyy");
                                }
                                //1. 如果存在代付关系，需要输出Forwarder 名称 且只有需要show forward customer的才会显示
                                if (checkShowForwardByLegalEntity(bankList[0].LegalEntity) && !(bankList[0].CUSTOMER_NUM ?? "").Equals(bankList[0].FORWARD_NUM ?? ""))
                                {
                                    dateStr += " " + bankList[0].FORWARD_NAME ?? "";
                                }
                            }
                            if (string.Equals(rvSiteUseId, "FALSE"))
                            {
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.CustomerNum);//customer Account Number
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.RVNumber);//Receipt Number 
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.RVDate == null ? "" : ((DateTime)applyClear.RVDate).ToString("dd-MMM-yyyy"));//Receipt Date
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.RVAmount);//Receipt Amount
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.InvoiceNumber);//TRX Number
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.AmountApplied);//Amount APPlied
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.InvoiceDate == null ? "" : ((DateTime)applyClear.InvoiceDate).ToString("dd-MMM-yyyy"));//TRX Date
                                helper.SetData(intStartRow + rowcount, nextcol++, dateStr);//Comments
                                helper.SetData(intStartRow + rowcount, nextcol++, dt_Now.ToString("dd-MMM-yyyy"));//Apply Date
                                helper.SetData(intStartRow + rowcount, nextcol++, "");//GL Date
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.Country);//Country
                                nextcol = nextcol + 2;//跳过result列和空列
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.RVSiteUseId);//Post SiteUseId
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.InvoiceSiteUseId);//Clear SiteUseId
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.Ebname);//EB
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.BSComments);//BS Comments
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PMTGroupNo);//GroupNo
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PaymentTerm);//PaymentTerm
                            }
                            else {
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.CustomerNum, cellHighLightStyles2);//customer Account Number
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.RVNumber, cellHighLightStyles2);//Receipt Number 
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.RVDate == null ? "" : ((DateTime)applyClear.RVDate).ToString("dd-MMM-yyyy"), cellHighLightStyles2);//Receipt Date
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.RVAmount, cellHighLightStyles2);//Receipt Amount
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.InvoiceNumber, cellHighLightStyles2);//TRX Number
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.AmountApplied, cellHighLightStyles2);//Amount APPlied
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.InvoiceDate == null ? "" : ((DateTime)applyClear.InvoiceDate).ToString("dd-MMM-yyyy"), cellHighLightStyles2);//TRX Date
                                helper.SetData(intStartRow + rowcount, nextcol++, dateStr, cellHighLightStyles2);//Comments
                                helper.SetData(intStartRow + rowcount, nextcol++, dt_Now.ToString("dd-MMM-yyyy"), cellHighLightStyles2);//Apply Date
                                helper.SetData(intStartRow + rowcount, nextcol++, "", cellHighLightStyles2);//GL Date
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.Country, cellHighLightStyles2);//Country
                                nextcol = nextcol + 2;//跳过result列和空列
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.RVSiteUseId, cellHighLightStyles2);//Post SiteUseId
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.InvoiceSiteUseId, cellHighLightStyles2);//Clear SiteUseId
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.Ebname, cellHighLightStyles2);//EB
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.BSComments, cellHighLightStyles2);//BS Comments
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PMTGroupNo, cellHighLightStyles2);//GroupNo
                                helper.SetData(intStartRow + rowcount, nextcol++, applyClear.PaymentTerm, cellHighLightStyles2);//PaymentTerm
                            }

                            

                            //销账历史记录
                            StringBuilder strClearHis = new StringBuilder();
                            strClearHis.Append("INSERT INTO T_CA_PostClearHistory ( Date,Type,LegalEntity,BSTransactionInc,BSCurrency,CustomerNum,SiteUseId,InvoiceNum,ClearAmount)");
                            strClearHis.Append(" VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd") + "',");
                            strClearHis.Append("         'Clear',");
                            strClearHis.Append("         '" + applyClear.IOTCLegalEntity + "',");
                            strClearHis.Append("         '" + applyClear.RVNumber + "',");
                            strClearHis.Append("         '" + applyClear.Currency + "',");
                            strClearHis.Append("         '" + applyClear.CustomerNum + "',");
                            strClearHis.Append("         '" + applyClear.InvoiceSiteUseId + "',");
                            strClearHis.Append("         '" + applyClear.InvoiceNumber + "',");
                            strClearHis.Append("         " + applyClear.AmountApplied + ")");
                            listSQL.Add(strClearHis.ToString());

                            rowcount++;
                        }

                        helper.Save(tmpFile, true);

                        //插入T_File
                        string strFileId = System.Guid.NewGuid().ToString();
                        string strFileName = Path.GetFileName(tmpFile);
                        StringBuilder strFileSql = new StringBuilder();
                        strFileSql.Append("INSERT INTO T_FILE ( FILE_ID,FILE_NAME,PHYSICAL_PATH,OPERATOR,CREATE_TIME)");
                        strFileSql.Append(" VALUES (N'" + strFileId + "',");
                        strFileSql.Append("         N'" + strFileName + "',");
                        strFileSql.Append("         N'" + tmpFile + "',");
                        strFileSql.Append("         N'" + userId + "',getdate())");
                        listSQL.Add(strFileSql.ToString());
                        if (!string.IsNullOrEmpty(strTotalFileId))
                        {
                            strTotalFileId += ";" + strFileId;
                        }
                        else
                        {
                            strTotalFileId = strFileId;
                        }
                    }
                }

                if (listClearNoVAT.Count > 0)
                {
                    foreach (CaBankStatementClearDto clear in listClearNoVAT)
                    {
                        //置Recon Detail isCleared标志
                        listSQL.Add("UPDATE T_CA_ReconDetail SET isCleared = 0 WHERE ID = '" + clear.ReconDetailId + "'");
                    }
                }

                if (listClear.Count > 0)
                {
                    string strPreBSId = "";
                    string strPreReconId = "";
                    decimal ldec_Sum = 0;
                    foreach (CaBankStatementClearDto clear in listClear)
                    {
                        //置Recon Detail isCleared标志
                        if (!string.IsNullOrEmpty(strPreBSId) && strPreBSId != clear.ID)
                        {
                            //置BS各状态（默认为UnMatch）
                            listSQL.Add("UPDATE T_CA_BankStatement SET CLEARING_TIME='" + dt_Now + "' WHERE ID = '" + strPreBSId + "'");
                            //当没有未Recon及没有未消账的，置整个BS Closed
                            listSQL.Add("UPDATE T_CA_BankStatement SET CLEARING_STATUS = '1' WHERE ID = '" + strPreBSId + "'");

                            ldec_Sum = 0;
                        }
                        ldec_Sum += Convert.ToDecimal(clear.AmountApplied == null ? 0 : clear.AmountApplied);
                        strPreBSId = clear.ID;
                        strPreReconId = clear.ReconId;
                    }
                    //置BS各状态（默认为UnMatch）
                    listSQL.Add("UPDATE T_CA_BankStatement SET CLEARING_TIME='" + dt_Now + "' WHERE ID = '" + strPreBSId + "'");
                    //当没有未Recon及没有未消账的，置整个BS Closed
                    listSQL.Add("UPDATE T_CA_BankStatement SET CLEARING_STATUS = '1' WHERE ID = '" + strPreBSId + "'");

                }
                ICaTaskService taskService = SpringFactory.GetObjectImpl<ICaTaskService>("CaTaskService");
                SqlHelper.ExcuteListSql(listSQL);
                string strTaskFileName = "";
                if (type == "1")
                {
                    strTaskFileName = "Post File";
                }
                else if (type == "2")
                {
                    strTaskFileName = "Clear File";
                }

                //取消入销同时
                if (listApply.Count > 0 || listClear.Count > 0)
                {
                    if (string.IsNullOrEmpty(taskId))
                    {
                        taskId = taskService.createTask(6, bsid.ToArray(), strTaskFileName, strTotalFileId, dt_Now, 2);
                    }
                }
                if (errmsg.Length > 0)
                {
                    strTotalFileId = strTotalFileId + "&" + errmsg.ToString();//销账时，若混入尚未post的bank给出提示信息
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex.Message);
            }
            finally
            {
                List<string> listLockSql = new List<string>();
                foreach (string bsiditem in bsidold)
                {
                    listLockSql.Add("Update T_CA_BankStatement set ISLOCKED = 0 where ID = '" + bsiditem + "'");
                }
                SqlHelper.ExcuteListSql(listLockSql);
            }
            return strTotalFileId;
        }

        private List<CaBankStatementPostDto> getPostList(string[] bsid, string type, int isAutoRecon = 0)
        {
            List<CaBankStatementPostDto> list = new List<CaBankStatementPostDto>();
            var bankIdStr = "";
            foreach (var id in bsid)
            {
                bankIdStr += "'" + id + "',";
            }
            bankIdStr = bankIdStr.Substring(0, bankIdStr.Length - 1);
            if (bankIdStr.Length > 0)
            {
                // 过滤bank islock=1的数据
                string bankIdSql = string.Format(@"SELECT distinct T_CA_BankStatement.ID                 AS ID,
                                                          T_CA_BankStatement.LegalEntity        AS IOTCLegalEntity,
                                                          T_SYS_TYPE_DETAIL.DETAIL_VALUE        AS LegalEntity,
                                                          T_CA_BankStatement.TRANSACTION_NUMBER AS RVNumber, 
                                                          T_CA_BankStatement.VALUE_DATE         AS ReceiptsDate,
                                                          T_CA_BankStatement.MaturityDate AS MaturityDate,
                                                          T_CA_BankStatement.CheckNumber AS CheckNumber,
                                                          T_CA_BankStatement.CURRENCY           AS Currency,
                                                          T_CA_BankStatement.CURRENT_AMOUNT     AS NetReceiptsAmount,
                                                          T_CA_BankStatement.REF1               AS Ref1,
                                                          T_CA_BankStatement.CUSTOMER_NAME      AS CustomerName,
                                                          (select top 1 T_CUSTOMER.Ebname  from  T_CUSTOMER with(nolock)  where T_CUSTOMER.Organization = T_CA_BankStatement.LegalEntity and T_CUSTOMER.CUSTOMER_NUM = T_CA_BankStatement.CUSTOMER_NUM AND (T_CUSTOMER.SiteUseId = T_CA_BankStatement.SiteUseId OR isnull(T_CA_BankStatement.SiteUseId,'') = ''))                 AS Ebname,
                                                          (CASE WHEN ISNULL(T_CA_BankStatement.ReceiptsMethod,'') = '' THEN (SELECT TOP 1 ReceiptsMethod FROM T_CA_PostAttribute with (nolock) where T_CA_BankStatement.LegalEntity = T_CA_PostAttribute.LEGALENTITY AND T_CA_BankStatement.BSTYPE = T_CA_PostAttribute.BSTYPE AND T_CA_BankStatement.CURRENCY = T_CA_PostAttribute.CURRENCY AND
													        RIGHT(T_CA_PostAttribute.BankAccountNumber,4) = RIGHT(T_CA_BankStatement.BankAccountNumber,4))
													        ELSE T_CA_BankStatement.ReceiptsMethod END) AS ReceiptsMethod, 
                                                          T_CA_BankStatement.BankAccountNumber  AS BankAccountNumber, 
                                                          T_CA_BankStatement.BankChargeTo       AS BankCharge,
                                                          T_CA_BankStatement.CUSTOMER_NUM       AS CustomerNumber, 
                                                          T_CA_BankStatement.FORWARD_NUM        AS ForwardNumber, 
                                                            (case when isnull((SELECT
						                                        TOP 1 SiteUseId
					                                        FROM
						                                        dbo.T_CA_PMT AS PMT WITH (nolock)
					                                        JOIN dbo.T_CA_PMTBS AS PMTBS WITH (nolock) ON PMTBS.ReconId = PMT.ID
					                                        WHERE
						                                        PMTBS.BANK_STATEMENT_ID = T_CA_BankStatement.ID
                                                            ORDER BY GroupNo DESC),'') = '' then 
                                                                   (case when isnull(T_CA_BankStatement.SiteUseId,'') ='' then isnull(isnull((select top 1 V_CA_CustomerSiteUseId.SiteUseId FROM dbo.V_CA_CustomerSiteUseId with (nolock) left join T_INVOICE_AGING with (nolock) on V_CA_CustomerSiteUseId.SiteUseId = T_INVOICE_AGING.SiteUseId WHERE V_CA_CustomerSiteUseId.LegalEntity = T_CA_BankStatement.LegalEntity AND V_CA_CustomerSiteUseId.CUSTOMER_NUM = T_CA_BankStatement.CUSTOMER_NUM
                                                                            and T_INVOICE_AGING.TRACK_STATES not in ('014') order by (CASE WHEN T_INVOICE_AGING.CLASS = 'INV' THEN 1 ELSE 2 END)),(select top 1 V_CA_CustomerSiteUseId.SiteUseId FROM dbo.V_CA_CustomerSiteUseId with (nolock) left join T_INVOICE_AGING with (nolock) on V_CA_CustomerSiteUseId.SiteUseId = T_INVOICE_AGING.SiteUseId WHERE V_CA_CustomerSiteUseId.LegalEntity = T_CA_BankStatement.LegalEntity AND V_CA_CustomerSiteUseId.CUSTOMER_NUM = T_CA_BankStatement.CUSTOMER_NUM)),'')
																	        else T_CA_BankStatement.SiteUseId end)
                                                            else (SELECT
						                                            TOP 1 SiteUseId
					                                            FROM
						                                            dbo.T_CA_PMT AS PMT WITH (nolock)
					                                            JOIN dbo.T_CA_PMTBS AS PMTBS WITH (nolock) ON PMTBS.ReconId = PMT.ID
					                                            WHERE
						                                            PMTBS.BANK_STATEMENT_ID = T_CA_BankStatement.ID
                                                            ORDER BY GroupNo DESC) end
                                                            ) as SiteUseId, 
                                                          ''                 as Comments,
                                                        T_CA_BankStatement.Comments                 AS BSComments,
			                                            (SELECT
						                                    TOP 1 GroupNo
					                                    FROM
						                                    dbo.T_CA_PMT AS PMT WITH (nolock)
					                                    JOIN dbo.T_CA_PMTBS AS PMTBS WITH (nolock) ON PMTBS.ReconId = PMT.ID
					                                    WHERE
						                                    PMTBS.BANK_STATEMENT_ID = T_CA_BankStatement.ID
                                                    ORDER BY GroupNo DESC) AS PMTGroupNo,
                                                    isnull( T_CA_BankStatement.PMTDetailFileName,(SELECT
						                                    TOP 1 FILENAME
					                                    FROM
						                                    dbo.T_CA_PMT AS PMT WITH (nolock)
					                                    JOIN dbo.T_CA_PMTBS AS PMTBS WITH (nolock) ON PMTBS.ReconId = PMT.ID
					                                    WHERE
						                                    PMTBS.BANK_STATEMENT_ID = T_CA_BankStatement.ID
                                                    ORDER BY GroupNo DESC)) AS PMTFileName,
                                                    isnull(PMTReceiveDate ,(SELECT
						                                    TOP 1 ReceiveDate
					                                    FROM
						                                    dbo.T_CA_PMT AS PMT WITH (nolock)
					                                    JOIN dbo.T_CA_PMTBS AS PMTBS WITH (nolock) ON PMTBS.ReconId = PMT.ID
					                                    WHERE
						                                    PMTBS.BANK_STATEMENT_ID = T_CA_BankStatement.ID
                                                    ORDER BY GroupNo DESC)) AS PMTReceiveDate
                                                     FROM T_CA_BankStatement with (nolock)
                                                     LEFT JOIN T_SYS_TYPE_DETAIL with (nolock) ON T_SYS_TYPE_DETAIL.TYPE_CODE = '087' AND T_CA_BankStatement.LegalEntity = T_SYS_TYPE_DETAIL.DETAIL_NAME
                                                    WHERE T_CA_BankStatement.ID IN({0}) AND T_CA_BankStatement.ISLOCKED = 1 AND ISNULL(APPLY_STATUS,'0') = '0' ", bankIdStr);
                if (isAutoRecon == 0)
                {
                    bankIdSql += string.Format(@"AND ((T_CA_BankStatement.MATCH_STATUS IN ('2', '4') and isnull(T_CA_BankStatement.CUSTOMER_NUM,'')<>'') OR (T_CA_BankStatement.MATCH_STATUS = 0 AND T_CA_BankStatement.LegalEntity IN (SELECT DETAIL_NAME FROM T_SYS_TYPE_DETAIL with(nolock) WHERE TYPE_CODE='096')))
                                                 ORDER BY T_CA_BankStatement.TRANSACTION_NUMBER,T_CA_BankStatement.VALUE_DATE,T_CA_BankStatement.CURRENCY,T_CA_BankStatement.CURRENT_AMOUNT");
                }
                else
                {
                    bankIdSql += string.Format(@"AND (T_CA_BankStatement.MATCH_STATUS IN ('2', '4') and isnull(T_CA_BankStatement.CUSTOMER_NUM,'')<>'')
                                                 ORDER BY T_CA_BankStatement.TRANSACTION_NUMBER,T_CA_BankStatement.VALUE_DATE,T_CA_BankStatement.CURRENCY,T_CA_BankStatement.CURRENT_AMOUNT");
                }

                List<CaBankStatementPostDto> listTmp = SqlHelper.GetList<CaBankStatementPostDto>(SqlHelper.ExcuteTable(bankIdSql, CommandType.Text, null));
                foreach (CaBankStatementPostDto item in listTmp)
                {
                    //是否必须入销同步（IsEntryAndWiteOff）
                    if (string.IsNullOrEmpty(item.CustomerNumber))
                    {
                        item.CustomerNumber = "";
                    }
                    string strIsEntryAndWiteOffSQL = string.Format(@"SELECT ID, CAOperator, LegalEntity, Func_Currency, CUSTOMER_NUM, IsFixedBankCharge, BankChargeFrom, BankChargeTo, IsNeedRemittance, IsMustPMTDetail, IsJumpBankStatement, IsJumpSiteUseId, IsMustSiteUseIdApply, IsEntryAndWiteOff, CREATE_User, CREATE_Date, MODIFY_User, MODIFY_Date 
                                                                       From T_CA_CustomerAttribute with (nolock)  WHERE LegalEntity = '{0}' AND CUSTOMER_NUM = '{1}'", item.LegalEntity, item.CustomerNumber);
                    List<CaCustomerAttributeDto> customerAttribute = SqlHelper.GetList<CaCustomerAttributeDto>(SqlHelper.ExcuteTable(strIsEntryAndWiteOffSQL, CommandType.Text, null));
                    if (customerAttribute != null && customerAttribute.Count > 0)
                    {
                        switch (customerAttribute[0].IsEntryAndWiteOff)
                        {
                            case "Yes":
                                //必须入销同时进行
                                if (type != "2")
                                {
                                    continue;
                                }
                                break;
                            case "Factoring":
                                //代付，必须入销同时进行
                                if (type != "2" && item.CustomerNumber != item.ForwardNumber)
                                {
                                    continue;
                                }
                                break;
                        }
                        //入账时必须精确到SiteUseId
                        if (customerAttribute[0].IsMustSiteUseIdApply ?? false)
                        {
                            string strIsMustSiteUseIdApplySQL = string.Format(@"SELECT TOP 1 T_CA_ReconDetail.SiteUseId FROM dbo.T_CA_ReconBS with (nolock)
                                                                                    JOIN dbo.T_CA_Recon with (nolock) ON T_CA_ReconBS.ReconId = T_CA_Recon.ID
                                                                                    JOIN dbo.T_CA_ReconDetail with (nolock) ON T_CA_ReconBS.ReconId = T_CA_ReconDetail.ReconId
                                                                                    WHERE T_CA_ReconBS.BANK_STATEMENT_ID = '{0}' AND T_CA_Recon.isClosed <> '1'
                                                                                    AND ISNULL(T_CA_ReconDetail.SiteUseId,'') <> ''", item.ID);
                            string strSiteUseId = SqlHelper.ExcuteScalar<string>(strIsMustSiteUseIdApplySQL, null);
                            if (string.IsNullOrEmpty(strSiteUseId))
                            {
                                continue;
                            }
                            else
                            {
                                item.SiteUseId = strSiteUseId;
                                string strEBSQL = string.Format(@"SELECT Ebname FROM T_CUSTOMER with (nolock)
                                                                                    WHERE SiteUseId = '{0}'", strSiteUseId);
                                string strEB = SqlHelper.ExcuteScalar<string>(strEBSQL, null);
                                if (!string.IsNullOrEmpty(strSiteUseId))
                                {
                                    item.EBName = strEB;
                                }
                            }
                        }
                        //入销同步进行时，如果销账时必须有PMT Detail
                        //if (type == "2" && customerAttribute[0].IsMustPMTDetail == "Yes")
                        //{
                        //    //判断是否有类型为PMT的Recon组合
                        //    //string strIsMustPMTDetailSQL = string.Format(@"SELECT count(*) FROM dbo.T_CA_ReconBS with (nolock)
                        //    //                                                        JOIN dbo.T_CA_Recon with (nolock) ON T_CA_ReconBS.ReconId = T_CA_Recon.ID
                        //    //                                                        JOIN dbo.T_CA_ReconDetail with (nolock) ON T_CA_ReconBS.ReconId = T_CA_ReconDetail.ReconId
                        //    //                                                        WHERE T_CA_ReconBS.BANK_STATEMENT_ID = '{0}' AND T_CA_Recon.GroupType = 'PMT' AND T_CA_Recon.isClosed <> '1'", item.ID);
                        //    string strIsMustPMTDetailSQL = string.Format(@"select count(*) from T_CA_PMT
                        //                                                        join T_CA_PMTBS on T_CA_PMT.ID = T_CA_PMTBS.ReconId
                        //                                                        where T_CA_PMTBS.BANK_STATEMENT_ID = '{0}'
                        //                                                        and exists(select 1 from T_CA_PMTDetail where T_CA_PMTDetail.ReconId = T_CA_PMT.ID)", item.ID);
                        //    int intCount = SqlHelper.ExcuteScalar<int>(strIsMustPMTDetailSQL, null);
                        //    if (intCount == 0) { continue; }
                        //}
                    }
                    list.Add(item);
                }
            }
            else
            {
                // 抛出提示信息
                throw new OTCServiceException("Please at least select one Item");
            }
            return list;
        }

        private List<CaBankStatementDto> getNoPostlist(string[] bsid)
        {
            List<CaBankStatementDto> list = new List<CaBankStatementDto>();
            var bankIdStr = "";
            foreach (var id in bsid)
            {
                bankIdStr += "'" + id + "',";
            }
            bankIdStr = bankIdStr.Substring(0, bankIdStr.Length - 1);
            string sql = string.Format(@"SELECT *
                                        FROM T_CA_BankStatement with (nolock)
                                        WHERE T_CA_BankStatement.ID IN({0}) 
                                        AND ISNULL(APPLY_STATUS,'0') = '0'
                                        AND T_CA_BankStatement.MATCH_STATUS in ('2','4')", bankIdStr);
            list = SqlHelper.GetList<CaBankStatementDto>(SqlHelper.ExcuteTable(sql, CommandType.Text, null));
            return list;
        }

        private List<CaBankStatementClearDto> getClearList(string[] bsid, string type, List<CaBankStatementPostDto> listApply, bool flag)
        {
            List<CaBankStatementClearDto> list = new List<CaBankStatementClearDto>();
            var bankIdStr = "";
            foreach (var id in bsid)
            {
                bankIdStr += "'" + id + "',";
            }
            bankIdStr = bankIdStr.Substring(0, bankIdStr.Length - 1);
            var applyedIdStr = "";
            foreach (var apply in listApply)
            {
                applyedIdStr += "'" + apply.ID + "',";
            }
            if (applyedIdStr.Length > 0)
            {
                applyedIdStr = applyedIdStr.Substring(0, applyedIdStr.Length - 1);
            }
            if (bankIdStr.Length > 0)
            {
                // 过滤bank islock=1的数据
                //RVAmount需要加上bankChargeFrom的金额
                string bankIdSql = string.Format(@"SELECT distinct T_CA_BankStatement.ID AS ID,
	                                                    T_CA_BankStatement.LegalEntity AS IOTCLegalEntity,
	                                                    T_SYS_TYPE_DETAIL.DETAIL_VALUE2 AS LegalEntity,
                                                        T_SYS_TYPE_DETAIL.DETAIL_VALUE AS Country,
	                                                    T_CA_BankStatement.TRANSACTION_NUMBER AS RVNumber,
	                                                    T_CA_Recon.ID AS ReconId,
	                                                    T_CA_ReconDetail.ID AS ReconDetailId,
	                                                    T_CA_ReconDetail.SiteUseId AS InvoiceSiteUseId,
	                                                    T_CA_BankStatement.Currency AS Currency,
	                                                    T_CA_BankStatement.CUSTOMER_NUM AS CustomerNum,
	                                                    T_CA_BankStatement.SiteUseId AS RVSiteUseId,
	                                                    T_CA_BankStatement.Value_Date AS RVDate,
	                                                    T_CA_BankStatement.transaction_amount + ISNULL(T_CA_BankStatement.bankChargeFrom,0) AS RVAmount,
	                                                    T_CA_ReconDetail.InvoiceNum AS InvoiceNumber,
	                                                    T_CA_ReconDetail.InvoiceDate AS InvoiceDate,
                                                        T_CA_ReconDetail.SortId ,
                                                        ISNULL(CASE
		                                                            WHEN T_CA_BankStatement.CURRENCY = T_CA_CustomerAttribute.Func_Currency THEN
	                                                                    ISNULL(T_CA_ReconDetail.Amount, 0)
                                                                    ELSE
	                                                                    ISNULL(T_CA_ReconDetail.LocalCurrencyAmount, 0)
		                                                            END,0) AS AmountApplied,
                                                        ''                                          AS Comments,
														V_CA_AR_ALL.HasVAT                          AS HasVAT,
                                                        V_CA_AR_ALL.Ebname                          AS Ebname,
                                                        V_CA_AR_ALL.PaymentTerm                     AS PaymentTerm,
                                                        T_CA_BankStatement.Comments                 AS BSComments,
			                                            (SELECT
						                                    TOP 1 GroupNo
					                                    FROM
						                                    dbo.T_CA_PMT AS PMT WITH (nolock)
					                                    JOIN dbo.T_CA_PMTBS AS PMTBS WITH (nolock) ON PMTBS.ReconId = PMT.ID
					                                    WHERE
						                                    PMTBS.BANK_STATEMENT_ID = T_CA_BankStatement.ID
                                                    ORDER BY GroupNo DESC) AS PMTGroupNo,
                                                    isnull( T_CA_BankStatement.PMTDetailFileName,(SELECT
						                                    TOP 1 FILENAME
					                                    FROM
						                                    dbo.T_CA_PMT AS PMT WITH (nolock)
					                                    JOIN dbo.T_CA_PMTBS AS PMTBS WITH (nolock) ON PMTBS.ReconId = PMT.ID
					                                    WHERE
						                                    PMTBS.BANK_STATEMENT_ID = T_CA_BankStatement.ID
                                                    ORDER BY GroupNo DESC)) AS PMTFileName,
                                                    isnull(PMTReceiveDate ,(SELECT
						                                    TOP 1 ReceiveDate
					                                    FROM
						                                    dbo.T_CA_PMT AS PMT WITH (nolock)
					                                    JOIN dbo.T_CA_PMTBS AS PMTBS WITH (nolock) ON PMTBS.ReconId = PMT.ID
					                                    WHERE
						                                    PMTBS.BANK_STATEMENT_ID = T_CA_BankStatement.ID
                                                    ORDER BY GroupNo DESC)) AS PMTReceiveDate
                                                     FROM T_CA_BankStatement with (nolock)
                                                LEFT JOIN T_SYS_TYPE_DETAIL with (nolock) ON T_SYS_TYPE_DETAIL.TYPE_CODE = '094' AND T_CA_BankStatement.LegalEntity = T_SYS_TYPE_DETAIL.DETAIL_NAME
													 JOIN dbo.T_CA_ReconBS with (nolock) ON T_CA_BankStatement.ID = T_CA_ReconBS.BANK_STATEMENT_ID
													 JOIN dbo.T_CA_Recon with (nolock) ON T_CA_Recon.ID = T_CA_ReconBS.ReconId
													 JOIN dbo.T_CA_ReconDetail with (nolock) ON T_CA_Recon.ID = T_CA_ReconDetail.ReconId
                                                     JOIN dbo.V_CA_AR_ALL with (nolock) ON T_CA_ReconDetail.InvoiceNum = V_CA_AR_ALL.INVOICE_NUM
                                                     JOIN dbo.T_CA_CustomerAttribute with (nolock) ON T_CA_BankStatement.LegalEntity = T_CA_CustomerAttribute.LegalEntity AND T_CA_BankStatement.CUSTOMER_NUM = T_CA_CustomerAttribute.CUSTOMER_NUM
                                                    WHERE T_CA_BankStatement.ID IN({0}) 
                                                      AND T_CA_BankStatement.ISLOCKED = 1
                                                      AND (T_CA_BankStatement.APPLY_STATUS = '2'
                                                      AND T_CA_ReconDetail.isCleared = 0", bankIdStr);
                if (!string.IsNullOrEmpty(applyedIdStr))
                {
                    bankIdSql += " OR T_CA_BankStatement.ID IN(" + applyedIdStr + ")";
                }
                bankIdSql += @") AND ISNULL(T_CA_BankStatement.CLEARING_STATUS,'0') = '0'
                             AND (T_CA_Recon.GroupType not like 'UN-%' AND T_CA_Recon.GroupType NOT LIKE 'NM%') ";
                if (flag)
                {
                    bankIdSql += @"AND (isNull(T_CA_CustomerAttribute.IsNeedVat,0)=0 OR (isNull(T_CA_CustomerAttribute.IsNeedVat,0)=1 AND V_CA_AR_ALL.HasVAT>0)) ";
                }
                else
                {
                    bankIdSql += @"AND (isNull(T_CA_CustomerAttribute.IsNeedVat,0)=1 AND V_CA_AR_ALL.HasVAT=0)";
                }

                bankIdSql += @"ORDER BY T_CA_BankStatement.TRANSACTION_NUMBER,dbo.T_CA_ReconDetail.InvoiceNum,dbo.T_CA_ReconDetail.SortId ";
                List<CaBankStatementClearDto> listTmp = SqlHelper.GetList<CaBankStatementClearDto>(SqlHelper.ExcuteTable(bankIdSql, CommandType.Text, null));
                string strIsEntryAndWiteOffSQLAll = string.Format(@"SELECT ID, CAOperator, LegalEntity, Func_Currency, CUSTOMER_NUM, IsFixedBankCharge, BankChargeFrom, BankChargeTo, IsNeedRemittance, IsMustPMTDetail, IsJumpBankStatement, IsJumpSiteUseId, IsMustSiteUseIdApply, IsEntryAndWiteOff, CREATE_User, CREATE_Date, MODIFY_User, MODIFY_Date 
                                                                       From T_CA_CustomerAttribute with (nolock)");
                //Helper.Log.Info("*************************************************** " + strIsEntryAndWiteOffSQL);
                List<CaCustomerAttributeDto> customerAttributeAll = SqlHelper.GetList<CaCustomerAttributeDto>(SqlHelper.ExcuteTable(strIsEntryAndWiteOffSQLAll, CommandType.Text, null));

                string strIsMustPMTDetailSQLAll = string.Format(@"select T_CA_PMTBS.BANK_STATEMENT_ID as Id,count(*) as nCount from T_CA_PMT
                                                                                join T_CA_PMTBS on T_CA_PMT.ID = T_CA_PMTBS.ReconId
                                                                                where T_CA_PMTBS.BANK_STATEMENT_ID in ({0})
                                                                                and exists(select 1 from T_CA_PMTDetail where T_CA_PMTDetail.ReconId = T_CA_PMT.ID)
                                                                    group by T_CA_PMTBS.BANK_STATEMENT_ID", bankIdStr);
                List<IsMustPMTDetailDto> listIsMustPMTDetailDtoAll = SqlHelper.GetList<IsMustPMTDetailDto>(SqlHelper.ExcuteTable(strIsMustPMTDetailSQLAll, CommandType.Text, null));

                foreach (CaBankStatementClearDto item in listTmp)
                {
                    //是否必须入销同步（IsEntryAndWiteOff）
                    if (string.IsNullOrEmpty(item.CustomerNum))
                    {
                        item.CustomerNum = "";
                    }
                    //string strIsEntryAndWiteOffSQL = string.Format(@"SELECT ID, CAOperator, LegalEntity, Func_Currency, CUSTOMER_NUM, IsFixedBankCharge, BankChargeFrom, BankChargeTo, IsNeedRemittance, IsMustPMTDetail, IsJumpBankStatement, IsJumpSiteUseId, IsMustSiteUseIdApply, IsEntryAndWiteOff, CREATE_User, CREATE_Date, MODIFY_User, MODIFY_Date 
                    //                                                   From T_CA_CustomerAttribute with (nolock)  WHERE LegalEntity = '{0}' AND CUSTOMER_NUM = '{1}'", item.IOTCLegalEntity, item.CustomerNum);
                    //Helper.Log.Info("*************************************************** " + strIsEntryAndWiteOffSQL);
                    List<CaCustomerAttributeDto> customerAttribute = customerAttributeAll.FindAll(o=>o.LegalEntity == item.IOTCLegalEntity && o.CUSTOMER_NUM == item.CustomerNum);
                    if (customerAttribute != null && customerAttribute.Count > 0)
                    {
                        switch (customerAttribute[0].IsMustPMTDetail == null ? "" : customerAttribute[0].IsMustPMTDetail)
                        {
                            case "Yes":
                                //判断是否存在PMT,如果不存在则不添加到最终列表中
                                //判断是否有类型为PMT的Recon组合
                                IsMustPMTDetailDto isMustPmtDetail = listIsMustPMTDetailDtoAll.Find(o => o.Id == item.ID);
                                //string strIsMustPMTDetailSQL = string.Format(@"select count(*) from T_CA_PMT
                                //                                                join T_CA_PMTBS on T_CA_PMT.ID = T_CA_PMTBS.ReconId
                                //                                                where T_CA_PMTBS.BANK_STATEMENT_ID = '{0}'
                                //                                                and exists(select 1 from T_CA_PMTDetail where T_CA_PMTDetail.ReconId = T_CA_PMT.ID)", item.ID);
                                //int intCount = SqlHelper.ExcuteScalar<int>(strIsMustPMTDetailSQL, null);
                                if (isMustPmtDetail != null && isMustPmtDetail.nCount > 0)
                                {
                                    list.Add(item);
                                }
                                break;
                            default:
                                list.Add(item);
                                break;
                        }
                    }
                    else
                    {
                        list.Add(item);
                    }
                }
            }
            else
            {
                // 抛出提示信息
                throw new OTCServiceException("Please at least select one Item");
            }
            return list;
        }

        private List<CaReconMsgDetailDto> getARListByOrderId(string orderId)
        {
            string sql = string.Format(@"SELECT INVOICE_NUM as ID, DUE_DATE, AMT 
                                        FROM V_CA_AR_CM 
                                        WHERE OrderNumber = '{0}'
                                        ORDER BY DUE_DATE", orderId);
            List<CaReconMsgDetailDto> list = SqlHelper.GetList<CaReconMsgDetailDto>(SqlHelper.ExcuteTable(sql, CommandType.Text, null));
            return list;
        }

        private List<CaReconMsgDetailDto> getARListByInvoiceNum(string invoiceNumbers)
        {
            invoiceNumbers = invoiceNumbers.Replace(",", "','");
            string sql = string.Format(@"SELECT INVOICE_NUM as ID, DUE_DATE, AMT 
                                        FROM V_CA_AR_CM 
                                        WHERE INVOICE_NUM in ('{0}')
                                        ORDER BY DUE_DATE", invoiceNumbers);
            List<CaReconMsgDetailDto> list = SqlHelper.GetList<CaReconMsgDetailDto>(SqlHelper.ExcuteTable(sql, CommandType.Text, null));
            return list;
        }
        public string SendPmtDetailMail(string type, string[] bsid)
        {
            ICaTaskService taskService = SpringFactory.GetObjectImpl<ICaTaskService>("CaTaskService");
            var bankIdStr = "";
            foreach (var id in bsid)
            {
                bankIdStr += "'" + id + "',";
            }
            bankIdStr = bankIdStr.Substring(0, bankIdStr.Length - 1);
            //检索BankStatement
            string bankIdSql = string.Format(@"SELECT T_CA_BankStatement.ID                            AS BSID,
                                                      T_CA_BankStatement.TRANSACTION_NUMBER AS TransNumber,
                                                      T_CA_BankStatement.LegalEntity               AS LegalEntity,
                                                      isNull(T_CA_BankStatement.CUSTOMER_NUM,'')   AS CustomerNum,
                                                      isNull(T_CA_BankStatement.SiteUseId,'')      AS SiteUseId,
                                                      isNull(transaction_amount,0)                 AS Amount
                                                 FROM T_CA_BankStatement with (nolock)
                                                WHERE T_CA_BankStatement.ID IN({0}) 
                                                  AND isNull(T_CA_BankStatement.CUSTOMER_NUM,'') <> ''
                                                  AND NOT Exists (SELECT 1 FROM T_CA_Recon with (nolock) JOIN T_CA_ReconBS with (nolock) ON T_CA_Recon.ID = T_CA_ReconBS.ReconId
                                                                    WHERE T_CA_Recon.GroupType <> 'NM' AND (T_CA_Recon.GroupType NOT LIKE 'UN-%') AND T_CA_Recon.isClosed = 0 AND T_CA_ReconBS.BANK_STATEMENT_ID = T_CA_BankStatement.ID)", bankIdStr);

            if (type == "1")
            {
                //不包含已经发送过的
                bankIdSql += " AND ISPMTDetailMail = 0";
            }
            bankIdSql += " ORDER BY T_CA_BankStatement.TRANSACTION_NUMBER";
            List<CaMailAlertDto> listTmp = SqlHelper.GetList<CaMailAlertDto>(SqlHelper.ExcuteTable(bankIdSql, CommandType.Text, null));
            int intCount = 0;
            if (listTmp.Count > 0)
            {
                List<string> strSQLInsertList = new List<string>();
                string strToTitle = CommonRep.GetQueryable<SysTypeDetail>().Where(o => o.TypeCode == "086" && o.DetailName == "CAPMT").Select(o => o.DetailValue).FirstOrDefault().ToString();
                string strCCTitle = CommonRep.GetQueryable<SysTypeDetail>().Where(o => o.TypeCode == "086" && o.DetailName == "CAPMT").Select(o => o.DetailValue2).FirstOrDefault().ToString();
                string strEID = AppContext.Current.User.EID.ToString();
                foreach (CaMailAlertDto tmp in listTmp)
                {
                    tmp.ID = Guid.NewGuid().ToString();
                    tmp.EID = strEID;
                    tmp.TOTITLE = strToTitle;
                    tmp.CCTITLE = strCCTitle;
                    intCount++;
                    string strSQLInsert = string.Format(@"INSERT INTO T_CA_MailAlert(ID, EID, BSID, AlertType, TransNumber, LegalEntity,CustomerNum, SiteUseId, Amount, TOTITLE,CCTITLE) VALUES (
                            '{0}','{1}','{2}','{3}','{4}','{5}',{6},'{7}',N'{8}','{9}','{10}')",
                            tmp.ID, tmp.EID, tmp.BSID, "006", tmp.TransNumber, tmp.LegalEntity, tmp.CustomerNum, tmp.SiteUseId, tmp.Amount, tmp.TOTITLE, tmp.CCTITLE);
                    strSQLInsertList.Add(strSQLInsert);
                    string strSQLUpdate = string.Format(@"Update T_CA_BankStatement SET ISPMTDetailMail = 1 WHERE ID ='{0}' ", tmp.BSID);
                    strSQLInsertList.Add(strSQLUpdate);
                }
                SqlHelper.ExcuteListSql(strSQLInsertList);
            }

            return intCount.ToString();

        }

        private List<CaPMTDetailDto> getARListByIds(List<string> arIds)
        {
            string arIdstr = string.Join("','", arIds.ToArray()); ;
            string sql = string.Format(@"SELECT
	                                    CUSTOMER_NUM AS CUSTOMER_NUM,
	                                    SiteUseId AS SiteUseId,
	                                    INVOICE_NUM AS InvoiceNum,
	                                    INV_CURRENCY AS Currency,
	                                    INVOICE_DATE AS InvoiceDate,
	                                    DUE_DATE AS DueDate,
	                                    AMT AS Amount,
	                                    Local_AMT AS LocalCurrencyAmount,
	                                    LegalEntity AS LegalEntity,
	                                    EBName AS EBName
                                    FROM
	                                    V_CA_AR
                                    WHERE
	                                    INVOICE_NUM IN ('{0}')", arIdstr);
            List<CaPMTDetailDto> list = SqlHelper.GetList<CaPMTDetailDto>(SqlHelper.ExcuteTable(sql, CommandType.Text, null));
            return list;
        }
        private string getCurrencyByCustomerNum(string customerNum)
        {
            string sql = string.Format(@"SELECT DISTINCT
	                                        CURRENCY
                                        FROM
	                                        V_CA_Customer
                                        WHERE CUSTOMER_NUM = '{0}'", customerNum);
            string currency = SqlHelper.ExcuteScalar<string>(sql);
            return currency;
        }

        private string saveResultFile(string sourceFileName)
        {
            string targetFileName = "";
            int splitNum = sourceFileName.LastIndexOf(".");
            targetFileName = sourceFileName.Substring(0, splitNum) + "-result" + sourceFileName.Substring(splitNum);
            File.Copy(sourceFileName, targetFileName, true);
            return targetFileName;
        }
        private string saveTempResultFile(string sourceFileName)
        {
            string targetFileName = "";
            int splitNum = sourceFileName.LastIndexOf(".");
            targetFileName = sourceFileName.Substring(0, splitNum) + "-temp" + sourceFileName.Substring(splitNum);
            File.Copy(sourceFileName, targetFileName, true);
            return targetFileName;
        }

        private bool isEmptyRow(NpoiHelper helper, int rownum, int maxcol)
        {
            bool flag = false;
            int emptyCount = 0;
            for (int i = 0; i < maxcol; i++)
            {
                if (helper.GetCell(rownum, i) == null)
                {
                    emptyCount++;
                }
            }
            if (emptyCount == maxcol)
            {
                flag = true;
            }
            return flag;
        }

        private string getDateCellValue(NpoiHelper helper, int row, int column)
        {
            string strValue = "";
            if (helper.GetCell(row, column) == null)
            {
                return strValue;
            }
            CellType ct = helper.GetCellType(row, column);
            switch (ct)
            {
                case CellType.Numeric:
                    strValue = helper.GetValue(row, column).ToString();
                    short format = helper.GetCell(row, column).CellStyle.DataFormat;

                    if (format == 14 || format == 15 || format == 31 || format == 57 || format == 58 || format == 20)  //|| format == 165
                    {
                        try
                        {
                            strValue = helper.GetCell(row, column).DateCellValue.ToString("yyyy-MM-dd");
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("The format fo Date is incorrect.");
                        }
                    }
                    else if (format == 164 || format == 165 || format == 166)
                    {
                        if (HSSFDateUtil.IsCellDateFormatted(helper.GetCell(row, column)))//日期类型
                        {
                            strValue = helper.GetCell(row, column).DateCellValue.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            strValue = helper.GetCell(row, column).NumericCellValue.ToString();
                        }
                    }
                    else
                        strValue = helper.GetCell(row, column).NumericCellValue.ToString();
                    break;
                case CellType.String:
                    strValue = helper.GetCell(row, column).StringCellValue;
                    break;
            }

            return strValue;
        }

        private string getNumericCellValue(NpoiHelper helper, int row, int column)
        {
            string strValue = "";
            if (helper.GetCell(row, column) == null)
            {
                return strValue;
            }
            CellType ct = helper.GetCellType(row, column);
            switch (ct)
            {
                case CellType.Numeric:
                    strValue = helper.GetCell(row, column).NumericCellValue.ToString();
                    break;
                case CellType.String:
                    strValue = helper.GetCell(row, column).StringCellValue;
                    break;
                case CellType.Formula:
                    if (helper.GetCell(row, column).CachedFormulaResultType.Equals(CellType.Numeric))
                    {
                        strValue = helper.GetCell(row, column).NumericCellValue.ToString();
                    }
                    else if (helper.GetCell(row, column).CachedFormulaResultType.Equals(CellType.String))
                    {
                        strValue = helper.GetCell(row, column).StringCellValue;
                    }
                    break;
            }
            return strValue.Trim();
        }

        private string getStringCellValue(NpoiHelper helper, int row, int column)
        {
            string strValue = "";
            if (helper.GetCell(row, column) == null)
            {
                return strValue;
            }
            CellType ct = helper.GetCellType(row, column);
            try
            {
                if (ct.Equals(CellType.String))
                {
                    strValue = helper.GetCell(row, column).StringCellValue;
                }
                else
                {
                    strValue = helper.GetCell(row, column).ToString();
                }
            }
            catch (Exception e)
            {
                strValue = "";
            }
            strValue = strValue.TrimStart().TrimEnd();
            return strValue;
        }

        private void deleteBankStatementByTaskId(string taskId)
        {
            string sql = string.Format(@"DELETE FROM T_CA_BankStatement
                                         WHERE ID in (SELECT t1.BSID FROM T_CA_TaskBS t1 with (nolock) WHERE t1.TASKID='{0}')", taskId);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql.ToString());
        }

        public List<CaPostResultCheck> getCaPostResultCheck(string fDate, string tDate)
        {
            string sql = string.Format(@"SELECT T_CA_PostClearHistory.Date as ChangeDate,
                                                T_CA_PostClearHistory.LegalEntity,
                                                T_CA_PostClearHistory.BSTransactionInc,
                                                T_CA_PostClearHistory.BSCurrency,
                                                T_CA_PostClearHistory.BSAmount AS PostAmount,
                                                T_CA_PostClearHistory.CustomerNum,
                                                b.SiteUseId,
                                                ISNULL(-b.ChangAmount,0) AS OracleChange,
                                                T_CA_PostClearHistory.BSAmount - ISNULL(-b.ChangAmount,0) as Charge,
                                                (case when -b.ChangAmount = T_CA_PostClearHistory.BSAmount then '' else 'NG' end) as status
                                           FROM dbo.T_CA_PostClearHistory with (nolock)
                                      LEFT JOIN
                                            (SELECT * FROM T_INVOICE_AGING_CHANGE AS changsub with (nolock) ) AS b
                                                ON b.LEGAL_ENTITY = T_CA_PostClearHistory.LegalEntity
                                                   AND b.ChangeDate =
                                                   (
                                                       SELECT MIN(subchang.ChangeDate)
                                                       FROM T_INVOICE_AGING_CHANGE AS subchang with (nolock) 
                                                       WHERE subchang.LEGAL_ENTITY = T_CA_PostClearHistory.LegalEntity
                                                         and subchang.ChangeDate >= '{0}'
                                                   )
                                                   AND b.INVOICE_NUM = T_CA_PostClearHistory.BSTransactionInc
                                                   AND b.CLASS = 'PMT'
                                        WHERE Type = 'post'
                                                AND Date >= '{0}'
                                                AND Date <= '{1}'
                                    ORDER BY (case when -b.ChangAmount = T_CA_PostClearHistory.BSAmount then '' else 'NG' end) desc,
                                            T_CA_PostClearHistory.Date desc,
                                            T_CA_PostClearHistory.LegalEntity asc,
                                            T_CA_PostClearHistory.BSTransactionInc asc", fDate, tDate);
            List<CaPostResultCheck> list = SqlHelper.GetList<CaPostResultCheck>(SqlHelper.ExcuteTable(sql, CommandType.Text, null));
            return list;
        }

        public List<CaClearResultCheck> getCaClearResultCheck(string fDate, string tDate)
        {
            string sql = string.Format(@"SELECT T_CA_PostClearHistory.Date as ChangeDate,
                                                T_CA_PostClearHistory.LegalEntity,
                                                T_CA_PostClearHistory.BSTransactionInc,
                                                T_CA_PostClearHistory.BSCurrency,
                                                T_CA_PostClearHistory.CustomerNum,
                                                T_CA_PostClearHistory.SiteUseId,
                                                T_CA_PostClearHistory.InvoiceNum,
                                                -T_CA_PostClearHistory.ClearAmount AS ClearAmount,
                                                ISNULL(b.ChangAmount, 0) AS OracleChange,
                                                -T_CA_PostClearHistory.ClearAmount - ISNULL(b.ChangAmount, 0) as Charge,
                                                (case when ISNULL(b.ChangAmount, 0) <= -T_CA_PostClearHistory.ClearAmount then '' else 'NG' end) as status
                                        FROM dbo.T_CA_PostClearHistory with (nolock)
                                            LEFT JOIN
                                            (SELECT * FROM T_INVOICE_AGING_CHANGE AS changsub with (nolock)) AS b
                                                ON b.LEGAL_ENTITY = T_CA_PostClearHistory.LegalEntity
                                                    AND b.ChangeDate =
                                                    (
                                                        SELECT MIN(subchang.ChangeDate)
                                                        FROM T_INVOICE_AGING_CHANGE AS subchang with (nolock)
                                                        WHERE subchang.LEGAL_ENTITY = T_CA_PostClearHistory.LegalEntity
                                                          and subchang.ChangeDate >= '{0}'
                                                    )
                                                    AND b.SiteUseId = T_CA_PostClearHistory.SiteUseId
                                                    AND b.INVOICE_NUM = T_CA_PostClearHistory.InvoiceNum
                                        WHERE Type = 'Clear'
                                          AND Date >= '{0}'
                                          AND Date <= '{1}'
                                        ORDER BY (case when ISNULL(b.ChangAmount, 0) <= -T_CA_PostClearHistory.ClearAmount then '' else 'NG' end) desc,
                                            T_CA_PostClearHistory.Date desc,
                                            T_CA_PostClearHistory.LegalEntity asc,
                                            T_CA_PostClearHistory.BSTransactionInc asc", fDate, tDate);
            List<CaClearResultCheck> list = SqlHelper.GetList<CaClearResultCheck>(SqlHelper.ExcuteTable(sql, CommandType.Text, null));
            return list;
        }
        public HttpResponseMessage exportPostClearResult(string fDate, string tDate)
        {
            string templateFile = "";
            string fileName = "";
            string tmpFile = "";

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportPostClearResultTemplate"].ToString());
                fileName = "PostClearResult_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                List<CaPostResultCheck> postResultList = getCaPostResultCheck(fDate, tDate);
                List<CaClearResultCheck> clearResultList = getCaClearResultCheck(fDate, tDate);

                NpoiHelper helper = new NpoiHelper(templateFile);
                helper.Save(tmpFile, true);
                helper = new NpoiHelper(tmpFile);

                //Sheet0
                int rowNo = 1;
                helper.ActiveSheet = 0;
                //设置Excel的内容信息
                foreach (var lst in postResultList)
                {
                    helper.SetData(rowNo, 0, rowNo);
                    helper.SetData(rowNo, 1, lst.status);
                    helper.SetData(rowNo, 2, lst.ChangeDate);
                    helper.SetData(rowNo, 3, lst.LegalEntity);
                    helper.SetData(rowNo, 4, lst.BSTransactionInc);
                    helper.SetData(rowNo, 5, lst.BSCurrency);
                    helper.SetData(rowNo, 6, lst.PostAmount);
                    helper.SetData(rowNo, 7, lst.CustomerNum);
                    helper.SetData(rowNo, 8, lst.SiteUseId);
                    helper.SetData(rowNo, 9, lst.OracleChange);
                    helper.SetData(rowNo, 10, lst.Charge);

                    rowNo++;
                }

                //Sheet1
                rowNo = 1;
                helper.ActiveSheet = 1;
                foreach (var lst in clearResultList)
                {
                    helper.SetData(rowNo, 0, rowNo);
                    helper.SetData(rowNo, 1, lst.status);
                    helper.SetData(rowNo, 2, lst.ChangeDate);
                    helper.SetData(rowNo, 3, lst.LegalEntity);
                    helper.SetData(rowNo, 4, lst.BSTransactionInc);
                    helper.SetData(rowNo, 5, lst.BSCurrency);
                    helper.SetData(rowNo, 6, lst.CustomerNum);
                    helper.SetData(rowNo, 7, lst.SiteUseId);
                    helper.SetData(rowNo, 8, lst.InvoiceNum);
                    helper.SetData(rowNo, 9, lst.ClearAmount);
                    helper.SetData(rowNo, 10, lst.OracleChange);
                    helper.SetData(rowNo, 11, lst.Charge);

                    rowNo++;
                }
                helper.ActiveSheet = 0;
                helper.Save(tmpFile, true);

                HttpResponseMessage response = new HttpResponseMessage();
                response.StatusCode = HttpStatusCode.OK;
                MemoryStream fileStream = new MemoryStream();
                if (File.Exists(tmpFile))
                {
                    using (FileStream fs = File.OpenRead(tmpFile))
                    {
                        fs.CopyTo(fileStream);
                    }
                }
                else
                {
                    throw new OTCServiceException("Get file failed because file not exist with physical path: " + tmpFile);
                }
                Stream ms = fileStream;
                ms.Position = 0;
                response.Content = new StreamContent(ms);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentLength = ms.Length;

                return response;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public HttpResponseMessage downloadtemplete(string fileType)
        {

            string templateFile = "";
            string fileName = "";
            string tmpFile = "";
            try
            {
                Helper.Log.Info("************************ downloadtemplete **************************:" + fileType);
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[fileType].ToString());
                fileName = Path.GetFileName(templateFile);
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                NpoiHelper helper = new NpoiHelper(templateFile);
                helper.Save(tmpFile, true);

                HttpResponseMessage response = new HttpResponseMessage();
                response.StatusCode = HttpStatusCode.OK;
                MemoryStream fileStream = new MemoryStream();
                if (File.Exists(tmpFile))
                {
                    using (FileStream fs = File.OpenRead(tmpFile))
                    {
                        fs.CopyTo(fileStream);
                    }
                }
                else
                {
                    throw new OTCServiceException("Get file failed because file not exist with physical path: " + tmpFile);
                }
                Stream ms = fileStream;
                ms.Position = 0;
                response.Content = new StreamContent(ms);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentLength = ms.Length;

                return response;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        private void updatePMT(CaPMTDto pmt)
        {
            //params
            SqlParameter[] ps = new SqlParameter[12];
            ps[0] = new SqlParameter("@LegalEntity", pmt.LegalEntity ?? (object)DBNull.Value);
            ps[1] = new SqlParameter("@CustomerNum", pmt.CustomerNum ?? (object)DBNull.Value);
            ps[2] = new SqlParameter("@ValueDate", pmt.ValueDate ?? (object)DBNull.Value);
            ps[3] = new SqlParameter("@Amount", pmt.Amount ?? (object)DBNull.Value);
            ps[4] = new SqlParameter("@BankCharge", pmt.BankCharge ?? (object)DBNull.Value);
            ps[5] = new SqlParameter("@LocalCurrencyAmount", pmt.LocalCurrencyAmount ?? (object)DBNull.Value);
            ps[6] = new SqlParameter("@LocalCurrency", pmt.LocalCurrency ?? (object)DBNull.Value);
            ps[7] = new SqlParameter("@UPDATE_USER", AppContext.Current.User.EID);
            ps[8] = new SqlParameter("@UPDATE_DATE", AppContext.Current.User.Now);
            ps[9] = new SqlParameter("@ID", pmt.ID);
            ps[10] = new SqlParameter("@ReceiveDate", pmt.ReceiveDate ?? (object)DBNull.Value);
            ps[11] = new SqlParameter("@TransactionAmount", pmt.TransactionAmount ?? (object)DBNull.Value);
            ps[12] = new SqlParameter("@BusinessId", pmt.businessId);

            //updatePMT
            string updatesql = @"update T_CA_PMT set 
                            LegalEntity = @LegalEntity,
                            CustomerNum =@CustomerNum,
                            ValueDate = @ValueDate, 
                            Amount = @Amount, 
                            TransactionAmount = @TransactionAmount,
                            BankCharge = @BankCharge,
                            LocalCurrencyAmount = @LocalCurrencyAmount,
                            LocalCurrency = @LocalCurrency,
                            ReceiveDate = @ReceiveDate,
                            UPDATE_USER = @UPDATE_USER,
                            UPDATE_DATE = @UPDATE_DATE,
                            BusinessId = @BusinessId
                            WHERE ID = @ID";
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(updatesql, ps);
        }

        public bool checkShowForwardCustomerLegalEntity(string legalEntity)
        {
            // 判断legalEntity是否属于SAP，若属于则直接置为close
            string countSql = string.Format(@"SELECT COUNT(*) AS COUNT FROM T_SYS_TYPE_DETAIL with (nolock)  WHERE TYPE_CODE='091' AND DETAIL_VALUE='{0}'", legalEntity);
            int count = CommonRep.ExecuteSqlQuery<CountDto>(countSql).ToList()[0].COUNT;
            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool checkShowForwardByLegalEntity(string legalEntity)
        {
            // 判断是否需要显示forward customer
            string countSql = string.Format(@"SELECT COUNT(*) AS COUNT FROM T_SYS_TYPE_DETAIL with (nolock)  WHERE TYPE_CODE='092' AND DETAIL_VALUE='{0}'", legalEntity);
            int count = CommonRep.ExecuteSqlQuery<CountDto>(countSql).ToList()[0].COUNT;
            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private CaReconDto getBankReconResult(string bankId)
        {
            string sql = string.Format(@"SELECT
                                        r.ID,
	                                    r.PMT_ID,
	                                    r.isClosed
                                    FROM
	                                    T_CA_ReconBS b with (nolock)
                                    INNER JOIN T_CA_Recon r with (nolock) ON r.ID = b.ReconId
                                    WHERE
	                                    b.BANK_STATEMENT_ID = '{0}'
                                    AND r.GroupType NOT LIKE 'NM%'
                                    AND r.GroupType NOT LIKE 'UN%'", bankId);
            List<CaReconDto> list = SqlHelper.GetList<CaReconDto>(SqlHelper.ExcuteTable(sql, CommandType.Text, null));
            if (list == null || list.Count() < 1)
            {
                return null;
            }
            else
            {
                string pmtsql = string.Format(@"select p.ID, p.GroupNo
                                        from T_CA_PMTBS s with (nolock)
                                        INNER JOIN T_CA_PMT p with (nolock) ON p.ID = s.ReconId
                                        where BANK_STATEMENT_ID = '{0}'", bankId);
                List<CaPMTDto> pmtlist = SqlHelper.GetList<CaPMTDto>(SqlHelper.ExcuteTable(pmtsql, CommandType.Text, null));
                if (pmtlist != null && pmtlist.Count() > 0)
                {
                    list[0].PMT_ID = pmtlist[0].GroupNo;//把pmt的groupNo传过来，方便提示
                }

                return list[0];
            }
        }

        private void updateBankMatchStatus(List<CaPMTBSDto> pmtbslist, string status)
        {
//<<<<<<< Updated upstream
            foreach (CaPMTBSDto pmtbs in pmtbslist)
            {
                CaReconDto recon = getBankReconResult(pmtbs.BANK_STATEMENT_ID);
                if (recon != null)
                {
                    CaReconService caReconService = SpringFactory.GetObjectImpl<CaReconService>("CaReconService");
                    caReconService.deleteReconGroupByReconId(recon.ID);
                    string updatesql = string.Format(@"UPDATE T_CA_BankStatement
                                                SET match_status = '{1}'
                                                WHERE ID = '{0}'", pmtbs.BANK_STATEMENT_ID, status);
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(updatesql);
                }
            //CaReconDto recon = getBankReconResult(pmtbslist[0].BANK_STATEMENT_ID);
            //if (recon != null)
            //{
            //    CaReconService caReconService = SpringFactory.GetObjectImpl<CaReconService>("CaReconService");
            //    caReconService.deleteReconGroupByReconId(recon.ID);
            //    string updatesql = string.Format(@"UPDATE T_CA_BankStatement
            //                                    SET match_status = '2'
            //                                    WHERE ID = '{0}'", pmtbslist[0].BANK_STATEMENT_ID);
            //    CommonRep.GetDBContext().Database.ExecuteSqlCommand(updatesql);
            }
        }

        private void updateBankMatchStatus1(List<CaPMTBSDto> pmtbslist, string status, string strComment, string strSiteUseId)
        {
            foreach (CaPMTBSDto pmtbs in pmtbslist)
            {
                CaReconDto recon = getBankReconResult(pmtbs.BANK_STATEMENT_ID);
                if (recon != null)
                {
                    CaReconService caReconService = SpringFactory.GetObjectImpl<CaReconService>("CaReconService");
                    string updatesql = string.Format(@"UPDATE T_CA_BankStatement
                                                SET match_status = '{1}', Comments = '{2}', SiteUseId = '{3}'
                                                WHERE ID = '{0}'", pmtbs.BANK_STATEMENT_ID, status, strComment, strSiteUseId);
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(updatesql);
                }
            }
        }
        private string getBankIDByRecon(string reconId)
        {
            string sql = string.Format(@"SELECT
	                                    TRANSACTION_NUMBER
                                    FROM
	                                    T_CA_BankStatement with (nolock) 
                                    WHERE
	                                    ID = (
		                                    SELECT
			                                    top 1 BANK_STATEMENT_ID
		                                    FROM
			                                    T_CA_ReconBS with (nolock)
		                                    WHERE
			                                    ReconId = '{0}'
	                                    ) ", reconId);
            string bsID = SqlHelper.ExcuteScalar<string>(sql);
            return bsID;
        }

        public bool checkDetailNameExists(string customerName, string legalEntity, string typeCode)
        {
            if (string.IsNullOrEmpty(customerName)) { customerName = ""; }
            if (string.IsNullOrEmpty(legalEntity)) { legalEntity = ""; }
            if (string.IsNullOrEmpty(typeCode)) { typeCode = ""; }

            // 判断legalEntity是否属于SAP，若属于则直接置为close
            string countSql = string.Format(@"SELECT COUNT(*) AS COUNT FROM T_SYS_TYPE_DETAIL with (nolock) WHERE TYPE_CODE='{2}' AND DETAIL_NAME='{0}' AND DETAIL_VALUE='{1}'", customerName.Replace("'", "''"), legalEntity.Replace("'", "''"), typeCode.Replace("'", "''"));
            int count = CommonRep.ExecuteSqlQuery<CountDto>(countSql).ToList()[0].COUNT;
            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private string getBstypeByDesc(string desc)
        {
            string sql = string.Format(@"SELECT
	                                        DETAIL_VALUE
                                        FROM
	                                        T_SYS_TYPE_DETAIL with (nolock) 
                                        WHERE
	                                        TYPE_CODE = '085'
                                        AND (
	                                        DETAIL_NAME = N'{0}'
	                                        OR DETAIL_VALUE = N'{0}'
                                        )", desc);
            string bsType = SqlHelper.ExcuteScalar<string>(sql);
            if (string.IsNullOrEmpty(bsType))
            {
                bsType = "GE";
            }
            return bsType;
        }


        public CaBsReportPage getbsReport(string dateF, string dateT, int page, int pageSize)
        {

            CaBsReportPage result = new CaBsReportPage();

            string sql = string.Format(@"SELECT
	                                *
                                FROM
	                                (
		                                SELECT
			                                ROW_NUMBER () OVER (ORDER BY t0.LegalEntity asc, t0.BSCurrency ASC, t0.VALUE_DATE ASC, t0.SortId asc) AS RowNumber,                                            
                                            t0.LegalEntity,
                                            t0.TRANSACTION_NUMBER,
                                            t0.VALUE_DATE,
                                            t0.BSCurrency,
                                            t0.BSCustomerNum,
                                            t0.TransactionINC,
                                            t0.BankID,
                                            t0.AccountNumber,
                                            t0.AccountID,
                                            t0.AccountName,
                                            t0.AccountOwnerID,
                                            t0.AccountCountry,
                                            t0.TransactionDate,
                                            t0.ValueDate,
                                            t0.Currency,
                                            t0.Amount,
                                            t0.ReferenceDRNM,
                                            t0.ReferenceBRCR,
                                            t0.ReferenceDESCR,
                                            t0.ReferenceENDTOEND,
                                            t0.Description,
                                            t0.ReferenceBRTN,
                                            t0.ReferenceDRBNK,
                                            t0.UserCode,
                                            t0.UserCodeDescription,
                                            t0.ItemType,
                                            t0.Owner,
                                            t0.Cheque,
                                            t0.needchecking,
                                            t0.Area,
                                            t0.Week,
                                            t0.Type,
                                            t0.UnknownType,
                                            t0.CustomerName,
                                            t0.Account,
                                            t0.SiteUseId,
                                            t0.EB_Name,
                                            t0.OperateAmount,
                                            t0.Term,
                                            t0.Comments
		                                FROM
			                               V_CA_BSReport as t0
                                        WHERE (t0.VALUE_DATE >= '{2} 00:00:00' OR '' = '{2}')
                                        AND (t0.VALUE_DATE <= '{3} 23:59:59' OR '' = '{3}')
	                                ) AS t
                                WHERE
	                                RowNumber BETWEEN {0} AND {1}", page == 1 ? 0 : pageSize * (page - 1) + 1, pageSize * page, dateF, dateT);


            List<CaBsReportDto> dto = CommonRep.ExecuteSqlQuery<CaBsReportDto>(sql).ToList();

            string sql1 = string.Format(@"select count(1) as count from V_CA_BSReport 
                                        where (VALUE_DATE >= '{0} 00:00:00' OR '' = '{0}')
                                        AND (VALUE_DATE <= '{1} 23:59:59' OR '' = '{1}')", dateF, dateT);

            result.dataRows = dto;
            result.count = SqlHelper.ExcuteScalar<int>(sql1);

            return result;
        }

        public string exportbsReport(string fDate, string tDate)
        {
            try
            {
                //模板文件
                string templateName = "ExportBsReportTemplate";
                string outputPath = "ExportBsReportPath";
                string datetimeString = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                var tplName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[templateName].ToString());
                var fileName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString());
                var pathName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString() + "BS Report_" + datetimeString + "_" + AppContext.Current.User.EID + ".xlsx");

                if (Directory.Exists(fileName) == false)
                {
                    Directory.CreateDirectory(fileName);
                }
                File.Copy(tplName, pathName, true);

                CaBsReportPage bsReportList = getbsReport(fDate, tDate, 1, 9999999);

                ExcelPackage package = new ExcelPackage(new FileInfo(pathName));
                //判断一共有几种不同LegalEntity&Currency，以确定要生成几个Sheet
                string strPreLegalEntity = "";
                string strPreCurrency = "";
                int intSheetCount = 0;
                foreach (var lst in bsReportList.dataRows)
                {
                    if (strPreLegalEntity != lst.LegalEntity || strPreCurrency != lst.Currency)
                    {
                        intSheetCount++;
                    }
                    strPreLegalEntity = lst.LegalEntity;
                    strPreCurrency = lst.Currency;
                }

                //Copy Sheet
                if (intSheetCount > 1)
                {
                    for (int i = 2; i <= intSheetCount; i++)
                    {
                        package.Workbook.Worksheets.Copy("Sheet1", "Sheet" + i);
                    }
                }

                //Sheet0
                int rowNo = 2;
                strPreLegalEntity = "";
                strPreCurrency = "";
                intSheetCount = 0;
                ExcelWorksheet worksheet = null;
                //设置Excel的内容信息
                foreach (var lst in bsReportList.dataRows)
                {
                    if (strPreLegalEntity != lst.LegalEntity || strPreCurrency != lst.Currency)
                    {
                        rowNo = 2;
                        intSheetCount++;
                        package.Workbook.Worksheets[intSheetCount].Name = lst.LegalEntity + "(" + lst.Currency + ")";
                        worksheet = package.Workbook.Worksheets[intSheetCount];
                    }
                    worksheet.Cells[rowNo, 1].Value = lst.TransactionINC;
                    worksheet.Cells[rowNo, 2].Value = lst.BankID;
                    worksheet.Cells[rowNo, 3].Value = lst.AccountNumber;
                    worksheet.Cells[rowNo, 4].Value = lst.AccountID;
                    worksheet.Cells[rowNo, 5].Value = lst.AccountName;
                    worksheet.Cells[rowNo, 6].Value = lst.AccountOwnerID;
                    worksheet.Cells[rowNo, 7].Value = lst.AccountCountry;
                    worksheet.Cells[rowNo, 8].Value = lst.TransactionDate;
                    worksheet.Cells[rowNo, 9].Value = lst.ValueDate;
                    worksheet.Cells[rowNo, 10].Value = lst.Currency;
                    worksheet.Cells[rowNo, 11].Value = lst.Amount;
                    worksheet.Cells[rowNo, 12].Value = lst.ReferenceDRNM;
                    worksheet.Cells[rowNo, 13].Value = lst.ReferenceBRCR;
                    worksheet.Cells[rowNo, 14].Value = lst.ReferenceDESCR;
                    worksheet.Cells[rowNo, 15].Value = lst.ReferenceENDTOEND;
                    worksheet.Cells[rowNo, 16].Value = lst.Description;
                    worksheet.Cells[rowNo, 17].Value = lst.ReferenceBRTN;
                    worksheet.Cells[rowNo, 18].Value = lst.ReferenceDRBNK;
                    worksheet.Cells[rowNo, 19].Value = lst.UserCode;
                    worksheet.Cells[rowNo, 20].Value = lst.UserCodeDescription;
                    worksheet.Cells[rowNo, 21].Value = lst.ItemType;
                    worksheet.Cells[rowNo, 22].Value = lst.Owner;
                    worksheet.Cells[rowNo, 23].Value = lst.Cheque;
                    worksheet.Cells[rowNo, 24].Value = lst.needchecking;
                    worksheet.Cells[rowNo, 25].Value = lst.Area;
                    worksheet.Cells[rowNo, 26].Value = lst.Week;
                    worksheet.Cells[rowNo, 27].Value = lst.Type;
                    worksheet.Cells[rowNo, 28].Value = lst.UnknownType;
                    worksheet.Cells[rowNo, 29].Value = lst.CustomerName;
                    worksheet.Cells[rowNo, 30].Value = lst.Account;
                    worksheet.Cells[rowNo, 31].Value = lst.SiteUseId;
                    worksheet.Cells[rowNo, 32].Value = lst.EB_Name;
                    worksheet.Cells[rowNo, 33].Value = lst.OperateAmount;
                    worksheet.Cells[rowNo, 34].Value = lst.Term;
                    worksheet.Cells[rowNo, 35].Value = lst.Comments;

                    strPreLegalEntity = lst.LegalEntity;
                    strPreCurrency = lst.Currency;
                    rowNo++;
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
                var virPatnName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[outputPath].ToString().Trim('~') + "BS Report_" + datetimeString + "_" + AppContext.Current.User.EID + ".xlsx";
                return virPatnName;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public CashApplicationCountReportDto queryCashApplicationCountReport(string legalentity, string fDate, string tDate)
        {
            CashApplicationCountReportDto result = new CashApplicationCountReportDto();
            result.total = new List<CashApplicationCountReportTotalDto>();
            string[] legalEntityGroup = legalentity.Split(',');

            //获得要查询的LegalEntity
            foreach (string le in legalEntityGroup)
            {
                string lecurrent = "'" + le + "'";
                CashApplicationCountReportTotalDto total = new CashApplicationCountReportTotalDto();

                string sqlTotal = string.Format(@"select count(*) from T_CA_BankStatement with(nolock) where  LegalEntity in ({0}) and VALUE_DATE >= '{1} 00:00:00'", lecurrent, fDate);
                if (!string.IsNullOrEmpty(tDate))
                {
                    sqlTotal += " and VALUE_DATE <= '" + tDate + " 23:59:59'";
                }
                int totalCount = SqlHelper.ExcuteScalar<int>(sqlTotal);
                total.TotalBS = totalCount;
                string sqlClosed = string.Format(@"select count(*) from T_CA_BankStatement with(nolock) where MATCH_STATUS = '9' and LegalEntity in ({0}) and VALUE_DATE >= '{1} 00:00:00'", lecurrent, fDate);
                if (!string.IsNullOrEmpty(tDate))
                {
                    sqlClosed += " and VALUE_DATE <= '" + tDate + " 23:59:59'";
                }
                int closedCount = SqlHelper.ExcuteScalar<int>(sqlClosed);
                total.ClosedBS = closedCount;
                string strTypeCount = @"select count(distinct T_CA_ReconBS.BANK_STATEMENT_ID)
                                                     from T_CA_ReconBS with(nolock)
                                                    JOIN T_CA_RECON with(nolock) ON T_CA_ReconBS.ReconId = T_CA_RECON.ID
                                                    JOIN T_CA_BankStatement with(nolock) ON T_CA_ReconBS.BANK_STATEMENT_ID = T_CA_BankStatement.ID
                                                    where BANK_STATEMENT_ID in (
                                                    select id from T_CA_BankStatement with(nolock) 
                                                    where T_CA_BankStatement.LegalEntity in ({0}) and T_CA_BankStatement.VALUE_DATE >= '{1} 00:00:00'
                                                    ) AND T_CA_RECON.GroupType IN ({2})
                                                    AND ISNULL(T_CA_BankStatement.CUSTOMER_NUM,'') <> ''";
                if (!string.IsNullOrEmpty(tDate))
                {
                    strTypeCount += " and T_CA_BankStatement.VALUE_DATE <= '" + tDate + " 23:59:59'";
                }
                string sqlIsReconedCount = string.Format(strTypeCount, lecurrent, fDate, "'AR', 'PMT','PTP','MANUAL'");
                int IsReconedCount = SqlHelper.ExcuteScalar<int>(sqlIsReconedCount);
                total.IsReconed = IsReconedCount;

                string sqlPmtDetailCount = string.Format(strTypeCount, lecurrent, fDate, "'PMT'");
                int PmtDetailCount = SqlHelper.ExcuteScalar<int>(sqlPmtDetailCount);
                total.PmtDetail = PmtDetailCount;
                if (total.IsReconed == 0)
                {
                    total.PmtDetailPersent = "0%";
                }
                else
                {
                    total.PmtDetailPersent = (Math.Round(Convert.ToDecimal(Convert.ToDecimal(total.PmtDetail) / Convert.ToDecimal(total.IsReconed) * 100), 2)).ToString() + "%";
                }
                string sqlARCount = string.Format(strTypeCount, lecurrent, fDate, "'AR'");
                int arCount = SqlHelper.ExcuteScalar<int>(sqlARCount);
                total.AR = arCount;
                if (total.IsReconed == 0)
                {
                    total.ARPersent = "0%";
                }
                else
                {
                    total.ARPersent = (Math.Round(Convert.ToDecimal(Convert.ToDecimal(total.AR) / Convert.ToDecimal(total.IsReconed) * 100), 2)).ToString() + "%";
                }
                string sqlPTPCount = string.Format(strTypeCount, lecurrent, fDate, "'PTP'");
                int PTPCount = SqlHelper.ExcuteScalar<int>(sqlPTPCount);
                total.PTP = PTPCount;
                string sqlManualCount = string.Format(strTypeCount, lecurrent, fDate, "'MANUAL'");
                int ManualCount = SqlHelper.ExcuteScalar<int>(sqlManualCount);
                total.Manual = ManualCount;
                total.LegalEntity = le;
                result.total.Add(total);
            }

            legalentity = "'" + legalentity.Replace(",", "','") + "'";
            string strSQLList = @"select distinct T_CA_RECON.GroupType as GroupType, T_CA_BankStatement.LegalEntity as LegalEntity, T_CA_BankStatement.BSTYPE as BSTYPE, T_CA_BankStatement.TRANSACTION_Number as TRANSACTION_Number, 
                                                    T_CA_BankStatement.TRANSACTION_AMOUNT as TRANSACTION_AMOUNT, T_CA_BankStatement.VALUE_DATE as VALUE_DATE, T_CA_BankStatement.CURRENCY as CURRENCY, 
                                                    T_CA_BankStatement.FORWARD_NUM as FORWARD_NUM, T_CA_BankStatement.FORWARD_NAME as FORWARD_NAME, T_CA_BankStatement.CUSTOMER_NUM as CUSTOMER_NUM, T_CA_BankStatement.CUSTOMER_NAME as CUSTOMER_NAME, 
                                                    T_CA_PMT.GroupNo AS PMTNUMBER, T_CA_RECON.GroupNo as GroupNo
                                                     from T_CA_ReconBS with(nolock)
                                                    JOIN T_CA_RECON with(nolock) ON T_CA_ReconBS.ReconId = T_CA_RECON.ID
                                                    JOIN T_CA_BankStatement with(nolock) ON T_CA_ReconBS.BANK_STATEMENT_ID = T_CA_BankStatement.ID
													LEFT JOIN T_CA_PMT with(nolock) ON T_CA_RECON.PMT_ID = T_CA_PMT.ID
                                                    where BANK_STATEMENT_ID in (
                                                    select id from T_CA_BankStatement with(nolock) 
                                                    where T_CA_BankStatement.LegalEntity in ({0}) and T_CA_BankStatement.VALUE_DATE >= '{1} 00:00:00'
                                                    ) AND T_CA_RECON.GroupType IN ({2})
                                                    AND ISNULL(T_CA_BankStatement.CUSTOMER_NUM,'') <> ''";
            if (!string.IsNullOrEmpty(tDate))
            {
                strSQLList += " and T_CA_BankStatement.VALUE_DATE <= '" + tDate + " 23:59:59'";
            }
            string sqlListPMT = string.Format(strSQLList, legalentity, fDate, "'PMT'");
            List<CashApplicationCountReportDetailDto> PMTList = SqlHelper.GetList<CashApplicationCountReportDetailDto>(SqlHelper.ExcuteTable(sqlListPMT, CommandType.Text, null));
            result.pmtList = PMTList;
            string sqlListAR = string.Format(strSQLList, legalentity, fDate, "'AR'");
            List<CashApplicationCountReportDetailDto> ARList = SqlHelper.GetList<CashApplicationCountReportDetailDto>(SqlHelper.ExcuteTable(sqlListAR, CommandType.Text, null));
            result.arList = ARList;
            string sqlListPTP = string.Format(strSQLList, legalentity, fDate, "'PTP'");
            List<CashApplicationCountReportDetailDto> PTPList = SqlHelper.GetList<CashApplicationCountReportDetailDto>(SqlHelper.ExcuteTable(sqlListPTP, CommandType.Text, null));
            result.ptpList = PTPList;
            string sqlListMANUAL = string.Format(strSQLList, legalentity, fDate, "'MANUAL'");
            List<CashApplicationCountReportDetailDto> ManualList = SqlHelper.GetList<CashApplicationCountReportDetailDto>(SqlHelper.ExcuteTable(sqlListMANUAL, CommandType.Text, null));
            result.manualList = ManualList;

            return result;
        }

        public HttpResponseMessage exportCashApplicationCountReport(string legalentity, string fDate, string tDate)
        {
            string templateFile = "";
            string fileName = "";
            string tmpFile = "";

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportCashApplicationCountResultTemplate"].ToString());
                fileName = "CashApplicationCountResult_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                CashApplicationCountReportDto result = queryCashApplicationCountReport(legalentity, fDate, tDate);

                NpoiHelper helper = new NpoiHelper(templateFile);
                helper.Save(tmpFile, true);
                helper = new NpoiHelper(tmpFile);

                //Sheet0
                int rowNo = 1;
                helper.ActiveSheet = 0;
                //设置Excel的内容信息
                foreach (var lst in result.total)
                {
                    helper.SetData(rowNo, 0, lst.LegalEntity);
                    helper.SetData(rowNo, 1, lst.TotalBS);
                    helper.SetData(rowNo, 2, lst.ClosedBS);
                    helper.SetData(rowNo, 3, lst.IsReconed);
                    helper.SetData(rowNo, 4, lst.PmtDetail);
                    helper.SetData(rowNo, 5, lst.PmtDetailPersent);
                    helper.SetData(rowNo, 6, lst.AR);
                    helper.SetData(rowNo, 7, lst.ARPersent);
                    helper.SetData(rowNo, 8, lst.PTP);
                    helper.SetData(rowNo, 9, lst.Manual);
                    rowNo++;
                }

                //Sheet1
                rowNo = 1;
                helper.ActiveSheet = 1;
                foreach (var lst in result.pmtList)
                {
                    helper.SetData(rowNo, 0, lst.GroupType);
                    helper.SetData(rowNo, 1, lst.LegalEntity);
                    helper.SetData(rowNo, 2, lst.BSTYPE);
                    helper.SetData(rowNo, 3, lst.TRANSACTION_Number);
                    helper.SetData(rowNo, 4, lst.TRANSACTION_AMOUNT);
                    helper.SetData(rowNo, 5, lst.VALUE_DATE);
                    helper.SetData(rowNo, 6, lst.CURRENCY);
                    helper.SetData(rowNo, 7, lst.FORWARD_NUM);
                    helper.SetData(rowNo, 8, lst.FORWARD_NAME);
                    helper.SetData(rowNo, 9, lst.CUSTOMER_NUM);
                    helper.SetData(rowNo, 10, lst.CUSTOMER_NAME);
                    helper.SetData(rowNo, 11, lst.PMTNUMBER);
                    helper.SetData(rowNo, 12, lst.GroupNo);
                    rowNo++;
                }
                //Sheet2
                rowNo = 1;
                helper.ActiveSheet = 2;
                foreach (var lst in result.arList)
                {
                    helper.SetData(rowNo, 0, lst.GroupType);
                    helper.SetData(rowNo, 1, lst.LegalEntity);
                    helper.SetData(rowNo, 2, lst.BSTYPE);
                    helper.SetData(rowNo, 3, lst.TRANSACTION_Number);
                    helper.SetData(rowNo, 4, lst.TRANSACTION_AMOUNT);
                    helper.SetData(rowNo, 5, lst.VALUE_DATE);
                    helper.SetData(rowNo, 6, lst.CURRENCY);
                    helper.SetData(rowNo, 7, lst.FORWARD_NUM);
                    helper.SetData(rowNo, 8, lst.FORWARD_NAME);
                    helper.SetData(rowNo, 9, lst.CUSTOMER_NUM);
                    helper.SetData(rowNo, 10, lst.CUSTOMER_NAME);
                    helper.SetData(rowNo, 11, lst.PMTNUMBER);
                    helper.SetData(rowNo, 12, lst.GroupNo);
                    rowNo++;
                }
                //Sheet3
                rowNo = 1;
                helper.ActiveSheet = 3;
                foreach (var lst in result.ptpList)
                {
                    helper.SetData(rowNo, 0, lst.GroupType);
                    helper.SetData(rowNo, 1, lst.LegalEntity);
                    helper.SetData(rowNo, 2, lst.BSTYPE);
                    helper.SetData(rowNo, 3, lst.TRANSACTION_Number);
                    helper.SetData(rowNo, 4, lst.TRANSACTION_AMOUNT);
                    helper.SetData(rowNo, 5, lst.VALUE_DATE);
                    helper.SetData(rowNo, 6, lst.CURRENCY);
                    helper.SetData(rowNo, 7, lst.FORWARD_NUM);
                    helper.SetData(rowNo, 8, lst.FORWARD_NAME);
                    helper.SetData(rowNo, 9, lst.CUSTOMER_NUM);
                    helper.SetData(rowNo, 10, lst.CUSTOMER_NAME);
                    helper.SetData(rowNo, 11, lst.PMTNUMBER);
                    helper.SetData(rowNo, 12, lst.GroupNo);
                    rowNo++;
                }
                //Sheet4
                rowNo = 1;
                helper.ActiveSheet = 4;
                foreach (var lst in result.manualList)
                {
                    helper.SetData(rowNo, 0, lst.GroupType);
                    helper.SetData(rowNo, 1, lst.LegalEntity);
                    helper.SetData(rowNo, 2, lst.BSTYPE);
                    helper.SetData(rowNo, 3, lst.TRANSACTION_Number);
                    helper.SetData(rowNo, 4, lst.TRANSACTION_AMOUNT);
                    helper.SetData(rowNo, 5, lst.VALUE_DATE);
                    helper.SetData(rowNo, 6, lst.CURRENCY);
                    helper.SetData(rowNo, 7, lst.FORWARD_NUM);
                    helper.SetData(rowNo, 8, lst.FORWARD_NAME);
                    helper.SetData(rowNo, 9, lst.CUSTOMER_NUM);
                    helper.SetData(rowNo, 10, lst.CUSTOMER_NAME);
                    helper.SetData(rowNo, 11, lst.PMTNUMBER);
                    helper.SetData(rowNo, 12, lst.GroupNo);
                    rowNo++;
                }
                helper.ActiveSheet = 0;
                helper.Save(tmpFile, true);

                HttpResponseMessage response = new HttpResponseMessage();
                response.StatusCode = HttpStatusCode.OK;
                MemoryStream fileStream = new MemoryStream();
                if (File.Exists(tmpFile))
                {
                    using (FileStream fs = File.OpenRead(tmpFile))
                    {
                        fs.CopyTo(fileStream);
                    }
                }
                else
                {
                    throw new OTCServiceException("Get file failed because file not exist with physical path: " + tmpFile);
                }
                Stream ms = fileStream;
                ms.Position = 0;
                response.Content = new StreamContent(ms);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentLength = ms.Length;

                return response;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public List<ExportCadaliyReportDto> queryCadaliyReport(string legalEntity, string bsType, string CreateDateFrom, string CreateDateTo,
string transNumber, string transAmount, string ValueDateFrom, string ValueDateTo, string enter, string enterMail, string crossOff, string crossOffMail)
        {
            StringBuilder sbsql = new StringBuilder();
            StringBuilder sqlWhere = new StringBuilder();
            sbsql.Append(@" select * from [V_CA_DaliyReport] ");

            if (!string.IsNullOrEmpty(legalEntity) && legalEntity != "undefined")
            {
                sqlWhere.Append(isStringEmpty(sqlWhere.ToString(), "LegalEntity", legalEntity, "="));
            }
            if (!string.IsNullOrEmpty(bsType) && bsType != "undefined")
            {
                sqlWhere.Append(isStringEmpty(sqlWhere.ToString(), "BSTYPE", bsType, "="));
            }
            if (!string.IsNullOrEmpty(CreateDateFrom) && CreateDateFrom != "undefined")
            {
                sqlWhere.Append(isStringEmpty(sqlWhere.ToString(), "CREATE_DATE", CreateDateFrom + " 00:00:00", ">=", true, false));
            }
            if (!string.IsNullOrEmpty(CreateDateTo) && CreateDateTo != "undefined")
            {
                sqlWhere.Append(isStringEmpty(sqlWhere.ToString(), "CREATE_DATE", CreateDateTo + " 23:59:59", "<=", true, false));
            }
            if (!string.IsNullOrEmpty(transNumber) && transNumber != "undefined")
            {
                sqlWhere.Append(isStringEmpty(sqlWhere.ToString(), "TRANSACTION_NUMBER", transNumber, "="));
            }
            if (!string.IsNullOrEmpty(transAmount) && transAmount != "undefined")
            {
                sqlWhere.Append(isStringEmpty(sqlWhere.ToString(), "TRANSACTION_AMOUNT", transAmount, "="));
            }
            if (!string.IsNullOrEmpty(ValueDateFrom) && ValueDateFrom != "undefined")
            {
                sqlWhere.Append(isStringEmpty(sqlWhere.ToString(), "VALUE_DATE", ValueDateFrom + " 00:00:00", ">=", true, false));
            }
            if (!string.IsNullOrEmpty(ValueDateTo) && ValueDateTo != "undefined")
            {
                sqlWhere.Append(isStringEmpty(sqlWhere.ToString(), "VALUE_DATE", ValueDateTo + " 23:59:59", "<=", true, false));
            }
            if (!string.IsNullOrEmpty(enter) && enter != "undefined")
            {
                sqlWhere.Append(isStringEmpty(sqlWhere.ToString(), "APPLY_STATUS", enter, "="));
            }
            if (!string.IsNullOrEmpty(enterMail) && enterMail != "undefined")
            {
                sqlWhere.Append(isStringEmpty(sqlWhere.ToString(), "ClearMailStatus", enterMail, "="));
            }
            if (!string.IsNullOrEmpty(crossOff) && crossOff != "undefined")
            {
                sqlWhere.Append(isStringEmpty(sqlWhere.ToString(), "CLEARING_STATUS", crossOff, "="));
            }
            if (!string.IsNullOrEmpty(crossOffMail) && crossOffMail != "undefined")
            {
                sqlWhere.Append(isStringEmpty(sqlWhere.ToString(), "PostMailStatus", crossOffMail, "="));
            }
            sbsql.Append(sqlWhere.ToString());
            List<ExportCadaliyReportDto> list = SqlHelper.GetList<ExportCadaliyReportDto>(SqlHelper.ExcuteTable(sbsql.ToString(), CommandType.Text, null));

            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlWhere"></param>
        /// <param name="itemField"></param>
        /// <param name="itemValue"></param>
        /// <param name="opertion"></param>
        /// <returns></returns>
        private string isStringEmpty(string sqlWhere, string itemField, string itemValue, string opertion, bool isString = true, bool isN = true)
        {
            StringBuilder ret = new StringBuilder();
            if (itemValue.ToLower() != "undefined")
            {
                string temValue = string.Empty;
                if (isString)
                {
                    temValue = @"'" + itemValue + "'";
                }
                else
                {
                    temValue = itemValue;
                }
                if (string.IsNullOrEmpty(sqlWhere) && !string.IsNullOrEmpty(itemValue))
                {
                    ret.Append(@" where " + itemField + " " + opertion + (isString && isN ? " N" : " ") + temValue);
                }
                else if (!string.IsNullOrEmpty(sqlWhere) && !string.IsNullOrEmpty(itemValue))
                {
                    ret.Append(@" and " + itemField + " " + opertion + (isString && isN ? " N" : " ") + temValue);
                }
            }
            return ret.ToString();
        }

        public HttpResponseMessage exportCadaliyReport(string legalEntity, string bsType, string CreateDateFrom, string CreateDateTo,
        string transNumber, string transAmount, string ValueDateFrom, string ValueDateTo, string enter, string enterMail, string crossOff, string crossOffMail)
        {
            string templateFile = "";
            string fileName = "";
            string tmpFile = "";

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportCadaliyTemplate"].ToString());
                fileName = "CadaliyResult_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                List<ExportCadaliyReportDto> result = queryCadaliyReport(legalEntity, bsType, CreateDateFrom, CreateDateTo, transNumber, transAmount, ValueDateFrom, ValueDateTo, enter, enterMail, crossOff, crossOffMail);

                NpoiHelper helper = new NpoiHelper(templateFile);
                helper.Save(tmpFile, true);
                helper = new NpoiHelper(tmpFile);

                //Sheet0
                int rowNo = 1;
                helper.ActiveSheet = 0;
                //设置Excel的内容信息
                foreach (var lst in result)
                {
                    helper.SetData(rowNo, 0, lst.LegalEntity);
                    helper.SetData(rowNo, 1, lst.BSTYPE);
                    helper.SetData(rowNo, 2, lst.TRANSACTION_NUMBER);
                    helper.SetData(rowNo, 3, lst.TRANSACTION_AMOUNT);
                    helper.SetData(rowNo, 4, lst.VALUE_DATE);
                    helper.SetData(rowNo, 5, lst.CURRENCY);
                    helper.SetData(rowNo, 6, lst.CURRENT_AMOUNT);
                    helper.SetData(rowNo, 7, lst.UNCLEAR_AMOUNT);
                    helper.SetData(rowNo, 8, lst.CREATE_DATE);
                    helper.SetData(rowNo, 9, lst.APPLY_STATUS);
                    helper.SetData(rowNo, 10, lst.APPLY_TIME);
                    helper.SetData(rowNo, 11, lst.CLEARING_STATUS);
                    helper.SetData(rowNo, 12, lst.CLEARING_TIME);
                    helper.SetData(rowNo, 13, lst.PostMailStatus);
                    helper.SetData(rowNo, 14, lst.PostMailSendTime);
                    helper.SetData(rowNo, 15, lst.PostMailSubject);
                    helper.SetData(rowNo, 16, lst.PostMailTo);
                    helper.SetData(rowNo, 17, lst.PostMailCc);
                    helper.SetData(rowNo, 18, lst.ClearMailStatus);
                    helper.SetData(rowNo, 19, lst.ClearSendTime);
                    helper.SetData(rowNo, 20, lst.ClearMailSubject);
                    helper.SetData(rowNo, 21, lst.ClearMailTo);
                    helper.SetData(rowNo, 22, lst.ClearMailCc);
                    rowNo++;
                }
                helper.ActiveSheet = 0;
                helper.Save(tmpFile, true);

                HttpResponseMessage response = new HttpResponseMessage();
                response.StatusCode = HttpStatusCode.OK;
                MemoryStream fileStream = new MemoryStream();
                if (File.Exists(tmpFile))
                {
                    using (FileStream fs = File.OpenRead(tmpFile))
                    {
                        fs.CopyTo(fileStream);
                    }
                }
                else
                {
                    throw new OTCServiceException("Get file failed because file not exist with physical path: " + tmpFile);
                }
                Stream ms = fileStream;
                ms.Position = 0;
                response.Content = new StreamContent(ms);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentLength = ms.Length;

                return response;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
