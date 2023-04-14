
using CsvHelper;
using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Intelligent.OTC.Business
{
    public class CaBankFileService : ICaBankFileService
    {

        public OTCRepository CommonRep { get; set; }

        public void deleteFileById(string fileId)
        {
            string sql = string.Format(@"update T_CA_BSFile set 
                                        DEL_FLAG = 1 
                                        where ID = '{0}'", fileId);
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(sql);
        }

        public CaBsFileDtoPage GetFileList(string transactionNum, string fileName, string fileType, string createDateF, string createDateT, int page, int pageSize)
        {
            CaBsFileDtoPage result = new CaBsFileDtoPage();

            string collecotrList = "";
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");     //注入服务
            var userId = AppContext.Current.User.EID; //当前用户ID
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");       //获得用户List(它这里用逗号间隔拼了一下)
            if (!string.IsNullOrEmpty(collecotrList))
            {
                collecotrList = collecotrList.Substring(0, collecotrList.LastIndexOf(","));
                collecotrList = collecotrList.Replace(",", "','");
            }
            collecotrList = "'" + collecotrList + "'";


            if (string.IsNullOrEmpty(transactionNum) || transactionNum == "undefined")
            {
                transactionNum = "";
            }

            if (string.IsNullOrEmpty(fileName) || fileName == "undefined")
            {
                fileName = "";
            }

            if (string.IsNullOrEmpty(fileType) || fileType == "undefined")
            {
                fileType = "";
            }

            if (string.IsNullOrEmpty(createDateF) || createDateF == "undefined")
            {
                createDateF = "";
            }

            if (string.IsNullOrEmpty(createDateT) || createDateT == "undefined")
            {
                createDateT = "";
            }
            string sql = string.Format(@"SELECT
	                                        *
                                        FROM
	                                        (
		                                        SELECT
			                                        f.*, b.TRANSACTION_NUMBER as transactionNum,
                                                    ROW_NUMBER() OVER (ORDER BY b.TRANSACTION_NUMBER DESC) AS RowNumber
		                                        FROM
			                                        T_CA_BSFile f with(nolock)
		                                        INNER JOIN T_CA_BankStatement b with(nolock) ON f.BSID = b.ID
		                                        WHERE
			                                        f.DEL_FLAG = 0
                                                AND f.CREATE_USER IN ({0})
		                                        AND ((f.FILE_NAME LIKE '%{3}%') OR '' = '{3}')
		                                        AND ((f.FILETYPE = '{4}') OR '' = '{4}')
		                                        AND ((b.TRANSACTION_NUMBER LIKE '%{5}%') OR '' = '{5}')
		                                        AND (f.CREATE_TIME >= '{6} 00:00:00' OR '' = '{6}')
		                                        AND (f.CREATE_TIME <= '{7} 23:59:59' OR '' = '{7}')
	                                        ) as t WHERE RowNumber BETWEEN {1} AND {2}", collecotrList, page == 1 ? 0 : pageSize * (page - 1) + 1, pageSize * page,
                                            fileName, fileType, transactionNum, createDateF, createDateT);

            List<CaBsFileDto> list = CommonRep.ExecuteSqlQuery<CaBsFileDto>(sql).ToList();

            string countsql = string.Format(@"SELECT count(*) FROM T_CA_BSFile f
                                             INNER JOIN T_CA_BankStatement b ON f.BSID = b.ID
		                                        WHERE
			                                        f.DEL_FLAG = 0
                                                AND f.CREATE_USER IN ({0})
		                                        AND ((f.FILE_NAME LIKE '%{1}%') OR '' = '{1}')
		                                        AND ((f.FILETYPE = '{2}') OR '' = '{2}')
		                                        AND ((b.TRANSACTION_NUMBER LIKE '%{3}%') OR '' = '{3}')
		                                        AND (f.CREATE_TIME >= '{4} 00:00:00' OR '' = '{4}')
		                                        AND (f.CREATE_TIME <= '{5} 23:59:59' OR '' = '{5}')", collecotrList,
                                                fileName, fileType, transactionNum, createDateF, createDateT);
            result.dataRows = list;
            result.count = SqlHelper.ExcuteScalar<int>(countsql);
            return result;
        }

        public List<CaBsFileWithPathDto> GetFilesByBankId(string bankId)
        {
            CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");

            CaBankStatementDto bs = service.getBankStatementByIdAndUserAuthor(bankId);
            if (string.IsNullOrEmpty(bs.ID)) {
                throw new Exception("Not have bankstatement authority.");
            }
            string sql = string.Format(@"SELECT ID, BSID, FILETYPE, FILE_NAME, DEL_FLAG,
                                        CREATE_USER, CREATE_TIME
                                        FROM T_CA_BSFile with(nolock) WHERE BSID = '{0}'
                                        AND DEL_FLAG = 0", bankId);

            List<CaBsFileWithPathDto> list = CommonRep.ExecuteSqlQuery<CaBsFileWithPathDto>(sql).ToList();
            return list;
        }

        public List<CaBsFileWithPathDto> GetFilesByBankIdWithPath(string bankFileId)
        {
            CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
            //根据BS File id 获得BSID
            string sqlBS = string.Format(@"SELECT distinct BSID
                                        FROM T_CA_BSFile with(nolock) WHERE ID = '{0}'
                                        AND DEL_FLAG = 0", bankFileId);
            List<CaBsFileWithPathDto> listBS = CommonRep.ExecuteSqlQuery<CaBsFileWithPathDto>(sqlBS).ToList();
            //判断BSID是否有权限
            CaBankStatementDto bs = service.getBankStatementByIdAndUserAuthor(listBS[0].BSID);
            if (string.IsNullOrEmpty(bs.ID))
            {
                throw new Exception("Not have bankstatement authority.");
            }
            //获得带物理路径的文件信息
            string sql = string.Format(@"SELECT ID, BSID, FILETYPE, FILE_NAME, PHYSICAL_PATH, DEL_FLAG,
                                        CREATE_USER, CREATE_TIME
                                        FROM T_CA_BSFile with(nolock) WHERE ID = '{0}'
                                        AND DEL_FLAG = 0", bankFileId);

            List<CaBsFileWithPathDto> list = CommonRep.ExecuteSqlQuery<CaBsFileWithPathDto>(sql).ToList();
            return list;
        }

        public string saveBSFile(HttpPostedFile file, CaBankStatementDto bank)
        {
            string errMsgStr = string.Empty;
            try
            {
                string archivePath = ConfigurationManager.AppSettings["BankArchivePath"].ToString();
                archivePath = archivePath + bank.LegalEntity + "\\" + bank.TRANSACTION_NUMBER;

                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }

                string strFileName = file.FileName;

                string fileType = strFileName.Substring(strFileName.LastIndexOf(".")+1);

                archivePath = archivePath + "\\" + strFileName;

                file.SaveAs(archivePath);

                //如果是CSV或Excel判断里面是否有不安全的信息
                if (!checkFileContent(archivePath)) {
                    errMsgStr = "Upload Faild. Content can't start with '= + - @' and can't URL !";
                    return errMsgStr;
                }

                string strFileId = Guid.NewGuid().ToString();
                strFileName = strFileName.Replace("'", "''");
                
                //保存文件记录
                StringBuilder sqlFile = new StringBuilder();
                sqlFile.Append("INSERT INTO T_CA_BSFile (ID, BSID, FILETYPE, FILE_NAME, PHYSICAL_PATH, DEL_FLAG,");
                sqlFile.Append("CREATE_USER, CREATE_TIME)");
                sqlFile.Append(" VALUES (@ID,");
                sqlFile.Append(" @BSID,");
                sqlFile.Append(" @FILETYPE,");
                sqlFile.Append(" @FILE_NAME,");
                sqlFile.Append(" @PHYSICAL_PATH,0, ");
                sqlFile.Append(" @CREATE_USER,");
                sqlFile.Append(" @CREATE_TIME) ");

                SqlParameter[] parms = { new SqlParameter("@ID", strFileId),
                                         new SqlParameter("@BSID", bank.ID),
                                         new SqlParameter("@FILETYPE", fileType),
                                         new SqlParameter("@FILE_NAME", strFileName),
                                         new SqlParameter("@PHYSICAL_PATH", archivePath.Replace("'", "''")),
                                         new SqlParameter("@CREATE_USER", AppContext.Current.User.EID),
                                         new SqlParameter("@CREATE_TIME", AppContext.Current.User.Now)
                                        };

                CommonRep.GetDBContext().Database.ExecuteSqlCommand(sqlFile.ToString(), parms);

                errMsgStr = "Upload success.";
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return errMsgStr;
        }

        protected bool checkFileContent(string filePath) {
            string extendFileName = Path.GetExtension(filePath).ToUpper();

            if (extendFileName == ".XLS" || extendFileName == ".XLSX")
            {
                ExcelPackage package = new ExcelPackage(new FileInfo(filePath));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号
                rowStart = 1;
                for (int j = rowStart; j <= rowEnd; j++)
                {
                    for (int k = 1; k <= 100; k++) {

                        if (worksheet.Cells[j, k] != null && worksheet.Cells[j, k].Value != null)
                        {
                            string strvalue = worksheet.Cells[j, k].Value.ToString();
                            if (strvalue.StartsWith("=") || strvalue.StartsWith("+") || strvalue.StartsWith("@") || IsURL(strvalue)) {
                                return false;
                            }
                        }
                    }
                }
            }
            else if (extendFileName == ".CSV")
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    CsvReader reader = new CsvReader(new StreamReader(fs, System.Text.Encoding.UTF8));
                    while (reader.Read())
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            string strvalue = "";
                            reader.TryGetField<string>(i, out strvalue);
                            if (!string.IsNullOrEmpty(strvalue))
                            {
                                if (strvalue.StartsWith("=") || strvalue.StartsWith("+") || strvalue.StartsWith("-") || strvalue.StartsWith("@") || IsURL(strvalue))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 验证是否是URL链接
        /// </summary>
        /// <param name="str">指定字符串</param>
        /// <returns></returns>
        public static bool IsURL(string str)
        {
            if (string.IsNullOrEmpty(str)) { return false; }
            str = str.ToLower();
            string pattern = @"^(https?|ftp|file|ws)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";
            Regex reg = new Regex(pattern);
            return reg.IsMatch(str);
        }
    }
}
