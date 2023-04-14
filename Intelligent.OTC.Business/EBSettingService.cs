using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Intelligent.OTC.Business
{
    public class EBSettingService
    {
        public OTCRepository CommonRep { get; set; }

        public LegalEBDtoPage getEBSettinglist(string region, string legalEntity, string ebname, string collector, int page, int pagesize) {
            LegalEBDtoPage result = new LegalEBDtoPage();
            if (string.IsNullOrEmpty(legalEntity) || legalEntity.ToLower() == "null") { legalEntity = ""; }
            if (string.IsNullOrEmpty(ebname) || ebname.ToLower() == "null") { ebname = ""; }
            if (string.IsNullOrEmpty(collector) || collector.ToLower() == "null") { collector = ""; }
            if (string.IsNullOrEmpty(region) || region.ToLower() == "null") { region = ""; }

            string sql = string.Format(@"SELECT
                                    *
                                FROM
                                    (
                                        SELECT
			                                ROW_NUMBER () OVER (ORDER BY t0.Region, t0.LEGAL_ENTITY, t0.EB, t0.CREDIT_TREM,t0.Collector) AS RowNumber,
			                                t0.*, T_SYS_TYPE_DETAIL.DETAIL_NAME as CONTACT_LANGUAGENAME
		                                FROM
			                                T_LeglalEB t0 with (nolock)
                                        LEFT JOIN dbo.T_SYS_TYPE_DETAIL ON T_SYS_TYPE_DETAIL.TYPE_CODE = '013' AND t0.CONTACT_LANGUAGE = T_SYS_TYPE_DETAIL.DETAIL_VALUE 
                                        WHERE (t0.LEGAL_ENTITY = '{2}'  or '' = '{2}')
                                          AND (t0.EB like '%{3}%'  or '' = '{3}')
                                          AND (t0.Collector like '%{4}%'  or '' = '{4}')
                                          AND (t0.Region = '{5}'  or '' = '{5}')
                                    ) AS t
                                WHERE
                                    RowNumber BETWEEN {0} AND {1}", page == 1 ? 0 : pagesize * (page - 1) + 1, pagesize * page, legalEntity, ebname, collector, region);

            List<LegalEBDto> dto = CommonRep.ExecuteSqlQuery<LegalEBDto>(sql).ToList();
            string sql1 = string.Format(@"SELECT count(*) FROM T_LeglalEB 
                                        WHERE (LEGAL_ENTITY = '{0}'  or '' = '{0}')
                                          AND (EB like '%{1}%'  or '' = '{1}')
                                          AND (Collector like '%{2}%'  or '' = '{2}')
                                          AND (Region = '{3}'  or '' = '{3}')", legalEntity, ebname, collector, region);

            result.dataRows = dto;
            result.count = SqlHelper.ExcuteScalar<int>(sql1);

            return result;
        }

        public string downloadEBList(string region, string legalEntity, string ebname, string collector) {
            LegalEBDtoPage list = getEBSettinglist(region, legalEntity, ebname, collector, 1, 999999);
            string strFileId = "";
            if (list.dataRows.Count > 0)
            {
                var userId = AppContext.Current.User.EID; //当前用户ID
                string templateFile = ConfigurationManager.AppSettings["TemplateEBSetting"].ToString().TrimStart('~').Replace("/", "\\").TrimStart('\\');
                templateFile = Path.Combine(HttpRuntime.AppDomainAppPath, templateFile);
                string tmpFile = Path.Combine(Path.GetTempPath(), "EBSetting_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx");
                NpoiHelper helper = new NpoiHelper(templateFile);
                helper.Save(tmpFile, true);
                helper = new NpoiHelper(tmpFile);
                int intStartRow = 1;
                int rowcount = 0;
                foreach (LegalEBDto applyPost in list.dataRows)
                {
                    int colnum = 0;
                    helper.SetData(intStartRow + rowcount, colnum++, rowcount + 1);
                    helper.SetData(intStartRow + rowcount, colnum++, applyPost.Region);
                    helper.SetData(intStartRow + rowcount, colnum++, applyPost.LEGAL_ENTITY);
                    helper.SetData(intStartRow + rowcount, colnum++, applyPost.EB);
                    helper.SetData(intStartRow + rowcount, colnum++, applyPost.CREDIT_TREM);
                    helper.SetData(intStartRow + rowcount, colnum++, applyPost.Collector);
                    helper.SetData(intStartRow + rowcount, colnum++, applyPost.CollectorEmail);
                    helper.SetData(intStartRow + rowcount, colnum++, applyPost.CONTACT_LANGUAGENAME);
                    helper.SetData(intStartRow + rowcount, colnum++, applyPost.CreditOfficer);
                    helper.SetData(intStartRow + rowcount, colnum++, applyPost.FinancialController);
                    helper.SetData(intStartRow + rowcount, colnum++, applyPost.CSManager);
                    helper.SetData(intStartRow + rowcount, colnum++, applyPost.FinancialManagers);
                    helper.SetData(intStartRow + rowcount, colnum++, applyPost.FinanceLeader);
                    helper.SetData(intStartRow + rowcount, colnum++, applyPost.LocalFinance);
                    helper.SetData(intStartRow + rowcount, colnum++, applyPost.BranchManager);
                    rowcount++;
                }
                helper.Save(tmpFile, true);
                //插入T_File
                strFileId = System.Guid.NewGuid().ToString();
                string strFileName = Path.GetFileName(tmpFile);
                StringBuilder strFileSql = new StringBuilder();
                strFileSql.Append("INSERT INTO T_FILE ( FILE_ID,FILE_NAME,PHYSICAL_PATH,OPERATOR,CREATE_TIME)");
                strFileSql.Append(" VALUES (@FILE_ID,");
                strFileSql.Append("         @FILE_NAME,");
                strFileSql.Append("         @PHYSICAL_PATH,");
                strFileSql.Append("         @OPERATOR,getdate())");

                SqlParameter[] parms = { new SqlParameter("@FILE_ID", strFileId),
                                         new SqlParameter("@FILE_NAME", strFileName),
                                         new SqlParameter("@PHYSICAL_PATH", tmpFile),
                                         new SqlParameter("@OPERATOR", userId)
                                       };

                CommonRep.GetDBContext().Database.ExecuteSqlCommand(strFileSql.ToString(), parms);
            }
            return strFileId;
        }

        public int AddOrUpdateLegalEb(LegalEBDto model)
        {
            if (string.IsNullOrEmpty(model.LEGAL_ENTITY) || model.LEGAL_ENTITY.ToLower() == "undefined" || model.LEGAL_ENTITY.ToLower() == "null") { model.LEGAL_ENTITY = ""; }
            if (string.IsNullOrEmpty(model.EB) || model.EB.ToLower() == "undefined" || model.EB.ToLower() == "null") { model.EB = ""; }
            if (string.IsNullOrEmpty(model.CREDIT_TREM) || model.CREDIT_TREM.ToLower() == "undefined" || model.CREDIT_TREM.ToLower() == "null") { model.CREDIT_TREM = ""; }
            if (string.IsNullOrEmpty(model.Region) || model.Region.ToLower() == "undefined" || model.Region.ToLower() == "null") { model.Region = ""; }
            if (string.IsNullOrEmpty(model.Collector) || model.Collector.ToLower() == "undefined" || model.Collector.ToLower() == "null") { model.Collector = ""; }
            if (string.IsNullOrEmpty(model.CollectorEmail) || model.CollectorEmail.ToLower() == "undefined" || model.CollectorEmail.ToLower() == "null") { model.CollectorEmail = ""; }
            if (string.IsNullOrEmpty(model.CONTACT_LANGUAGE) || model.CONTACT_LANGUAGE.ToLower() == "undefined" || model.CONTACT_LANGUAGE.ToLower() == "null") { model.CONTACT_LANGUAGE = ""; }
            if (string.IsNullOrEmpty(model.CreditOfficer) || model.CreditOfficer.ToLower() == "undefined" || model.CreditOfficer.ToLower() == "null") { model.CreditOfficer = ""; }
            if (string.IsNullOrEmpty(model.FinancialController) || model.FinancialController.ToLower() == "undefined" || model.FinancialController.ToLower() == "null") { model.FinancialController = ""; }
            if (string.IsNullOrEmpty(model.CSManager) || model.CSManager.ToLower() == "undefined" || model.CSManager.ToLower() == "null") { model.CSManager = ""; }
            if (string.IsNullOrEmpty(model.FinancialManagers) || model.FinancialManagers.ToLower() == "undefined" || model.FinancialManagers.ToLower() == "null") { model.FinancialManagers = ""; }
            if (string.IsNullOrEmpty(model.FinanceLeader) || model.FinanceLeader.ToLower() == "undefined" || model.FinanceLeader.ToLower() == "null") { model.FinanceLeader = ""; }
            if (string.IsNullOrEmpty(model.LocalFinance) || model.LocalFinance.ToLower() == "undefined" || model.LocalFinance.ToLower() == "null") { model.LocalFinance = ""; }
            if (string.IsNullOrEmpty(model.BranchManager) || model.BranchManager.ToLower() == "undefined" || model.BranchManager.ToLower() == "null") { model.BranchManager = ""; }

            try
            {
                string sql = string.Format(@"select *
                                    from T_LeglalEB with (nolock) 
                                    where id = {0} ", model.Id);


                List<LegalEBDto> list = SqlHelper.GetList<LegalEBDto>(SqlHelper.ExcuteTable(sql, System.Data.CommandType.Text, null));

                string updateSql = "";
                if (list != null && list.Count > 0)
                {
                    updateSql = string.Format(@"update T_LeglalEB set LEGAL_ENTITY = N'{1}' ,EB = N'{2}' ,CREDIT_TREM = N'{3}',Region = N'{4}',Collector = N'{5}',CollectorEmail= N'{6}', CONTACT_LANGUAGE=N'{7}', CreditOfficer=N'{8}', FinancialController=N'{9}', CSManager=N'{10}', FinancialManagers=N'{11}', FinanceLeader=N'{12}', LocalFinance=N'{13}', BranchManager=N'{14}' where ID = '{0}'",
                       model.Id, model.LEGAL_ENTITY, model.EB, model.CREDIT_TREM, model.Region, model.Collector, model.CollectorEmail, model.CONTACT_LANGUAGE, model.CreditOfficer,model.FinancialController, model.CSManager, model.FinancialManagers,model.FinanceLeader, model.LocalFinance, model.BranchManager);
                }
                else
                {
                    updateSql = string.Format(@"insert into T_LeglalEB(LEGAL_ENTITY,EB,CREDIT_TREM,Region,Collector,CollectorEmail,CONTACT_LANGUAGE,CreditOfficer,FinancialController,CSManager,FinancialManagers,FinanceLeader,LocalFinance,BranchManager)
                                              values (N'{0}',N'{1}',N'{2}',N'{3}',N'{4}',N'{5}',N'{6}', N'{7}',N'{8}',N'{9}',N'{10}', N'{11}', N'{12}', N'{13}')",
                        model.LEGAL_ENTITY, model.EB, model.CREDIT_TREM, model.Region, model.Collector, model.CollectorEmail, model.CONTACT_LANGUAGE, model.CreditOfficer, model.FinancialController, model.CSManager, model.FinancialManagers, model.FinanceLeader, model.LocalFinance, model.BranchManager);
                }

                SqlHelper.ExcuteSql(updateSql);

                return 1;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex; 
            }
        }

        public void deleteLegalEB(string id) {
            try
            {
                string deleteSql = string.Format(@"delete from T_LeglalEB where id = {0}", id);
                SqlHelper.ExcuteSql(deleteSql);
            }
            catch (Exception ex) {
                throw ex;
            }
        }
        public string Import()
        {
            FileType fileType = FileType.EBSetting;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                string strEBSettingKey = "ImportEbSettingData";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strEBSettingKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileType.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + ".xlsx";

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileType);

                return ImportEBSetting();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!\r\n" + ex.Message);
            }
        }


        public string ImportEBSetting()
        {
            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");

            FileUploadHistory fileUpHis = new FileUploadHistory();
            var userId = AppContext.Current.User.EID;

            try
            {
                string strCode = Helper.EnumToCode(FileType.EBSetting);
                fileUpHis = fileService.GetSuccessData(strCode);
                if (fileUpHis == null)
                {
                    throw new Exception("import file is not found!");
                }

                List<T_LeglalEB> importItems = new List<T_LeglalEB>();
                List<SysTypeDetail> languageList = CommonRep.GetQueryable<SysTypeDetail>().Where(o=>o.TypeCode=="013").ToList();

                #region openXml
                string strpath = fileUpHis.ArchiveFileName;

                ExcelPackage package = new ExcelPackage(new FileInfo(strpath));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                int rowEnd = worksheet.Dimension.End.Row;           //工作区结束行号
                List<string> listRegion = new List<string>();

                for (int j = rowStart + 1; j <= rowEnd; j++)
                {
                    if (j == 71) {
                        j = j;
                    }
                    try
                    {
                        T_LeglalEB item = new T_LeglalEB();

                        if (worksheet.Cells[j, 2].Value != null)
                        {
                            item.Region = worksheet.Cells[j, 2].Value.ToString().Trim();
                            if (listRegion.Find(o => o == item.Region) == null)
                            {
                                listRegion.Add(item.Region);
                            }
                        }

                        if (worksheet.Cells[j, 3].Value != null)
                        {
                            item.LEGAL_ENTITY = worksheet.Cells[j, 3].Value.ToString().Trim();
                        }

                        if (worksheet.Cells[j, 4].Value != null)
                        {
                            item.EB = worksheet.Cells[j, 4].Value.ToString().Trim();
                        }

                        if (worksheet.Cells[j, 5].Value != null)
                        {
                            item.CREDIT_TREM = worksheet.Cells[j, 5].Value.ToString().Trim();
                            item.CREDIT_TREM = item.CREDIT_TREM.ToUpper();
                            if (item.CREDIT_TREM != "ALL" && item.CREDIT_TREM != "PREPAY" && item.CREDIT_TREM != "有账期")
                            {
                                return "第" + j + "行，[Credit Term]无效！";
                            }
                        }

                        if (worksheet.Cells[j, 6].Value != null)
                        {
                            item.Collector = worksheet.Cells[j, 6].Value.ToString().Trim();
                        }
                        if (worksheet.Cells[j, 7].Value != null)
                        {
                            item.CollectorEmail = worksheet.Cells[j, 7].Value.ToString().Trim();
                        }
                        if (worksheet.Cells[j, 8].Value != null)
                        {
                            item.CONTACT_LANGUAGE = worksheet.Cells[j, 8].Value.ToString().Trim();
                            SysTypeDetail lang = languageList.Find(o => o.DetailName == item.CONTACT_LANGUAGE);
                            if (lang == null)
                            {
                                return "第" + j + "行，[Language]无效！";
                            }
                            else
                            {
                                item.CONTACT_LANGUAGE = lang.DetailValue;
                            }
                        }
                        if (worksheet.Cells[j, 9].Value != null)
                        {
                            item.CreditOfficer = worksheet.Cells[j, 9].Value.ToString().Trim();
                            if (!checkContactor(item.CreditOfficer))
                            {
                                return "第" + j + "行，[Credit Officer-" + item.CreditOfficer + "]首次出现！";
                            }
                        }
                        if (worksheet.Cells[j, 10].Value != null)
                        {
                            item.FinancialController = worksheet.Cells[j, 10].Value.ToString().Trim();
                            if (!checkContactor(item.FinancialController))
                            {
                                return "第" + j + "行，[Financial Controller-" + item.FinancialController + "]首次出现！";
                            }
                        }
                        if (worksheet.Cells[j, 11].Value != null)
                        {
                            item.CSManager = worksheet.Cells[j, 11].Value.ToString().Trim();
                            if (!checkContactor(item.CSManager))
                            {
                                return "第" + j + "行，[CS Manager-" + item.CSManager + "]首次出现！";
                            }
                        }
                        if (worksheet.Cells[j, 12].Value != null)
                        {
                            item.FinancialManagers = worksheet.Cells[j, 12].Value.ToString().Trim();
                            if (!checkContactor(item.FinancialManagers))
                            {
                                return "第" + j + "行，[Financial Manager-" + item.FinancialManagers + "]首次出现！";
                            }
                        }
                        if (worksheet.Cells[j, 13].Value != null)
                        {
                            item.FinanceLeader = worksheet.Cells[j, 13].Value.ToString().Trim();
                            if (!checkContactor(item.FinanceLeader))
                            {
                                return "第" + j + "行，[Finance Leader-" + item.FinanceLeader + "]首次出现！";
                            }
                        }
                        if (worksheet.Cells[j, 14].Value != null)
                        {
                            item.LocalFinance = worksheet.Cells[j, 14].Value.ToString().Trim();
                            if (!checkContactor(item.LocalFinance))
                            {
                                return "第" + j + "行，[Local Finance-" + item.LocalFinance + "]首次出现！";
                            }
                        }
                        if (worksheet.Cells[j, 15].Value != null)
                        {
                            item.BranchManager = worksheet.Cells[j, 15].Value.ToString().Trim();
                            if (!checkContactor(item.BranchManager))
                            {
                                return "第" + j + "行，[Branch Manager-" + item.BranchManager + "]首次出现！";
                            }
                        }
                        importItems.Add(item);
                    }
                    catch (Exception ex) {
                        Helper.Log.Info("row:" + j);
                       
                    }
                }
                #endregion

                List<string> listSQL = new List<string>();
                foreach (string region in listRegion) {
                    string delsql = string.Format("delete from T_LeglalEB where region = '{0}'", region);
                    listSQL.Add(delsql);
                }
                foreach (T_LeglalEB eb in importItems) {
                    string inssql = string.Format(@"insert into T_LeglalEB (LEGAL_ENTITY
                                                                          , EB
                                                                          , CREDIT_TREM
                                                                          , Collector
                                                                          , Region
                                                                          , CONTACT_LANGUAGE
                                                                          , CreditOfficer
                                                                          , FinancialController
                                                                          , CSManager
                                                                          , FinancialManagers
                                                                          , FinanceLeader
                                                                          , LocalFinance
                                                                          , BranchManager
                                                                          , CollectorEmail)
                                                                 values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}')", 
                                                                 eb.LEGAL_ENTITY, eb.EB, eb.CREDIT_TREM, eb.Collector,
                                                                 eb.Region, eb.CONTACT_LANGUAGE, eb.CreditOfficer,
                                                                 eb.FinancialController, eb.CSManager, eb.FinancialManagers,
                                                                 eb.FinanceLeader, eb.LocalFinance, eb.BranchManager, eb.CollectorEmail);
                    listSQL.Add(inssql);
                }
                SqlHelper.ExcuteListSql(listSQL);

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

        private bool checkContactor(string strName) {
            List<string> strNameList = strName.Split(';').ToList();
            foreach (string name in strNameList) {
                Contactor cont = CommonRep.GetQueryable<Contactor>().Where(o => o.Name == name).FirstOrDefault();
                if (cont == null || string.IsNullOrEmpty(cont.Name))
                {
                    return false;
                }
            }
            return true;
        }

    }
}
