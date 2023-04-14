using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using NPOI.SS.UserModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Intelligent.OTC.Business
{
    public class ContactReplaceService
    {
        public OTCRepository CommonRep { private get; set; }


        public IList<T_CONTACTOR_REPLACE> GetAll()
        {
            var result = CommonRep.GetQueryable<T_CONTACTOR_REPLACE>().ToList();
            return result;
        }

        public int AddOrUpdate(T_CONTACTOR_REPLACE dto)
        {
            try
            {
                var entity = CommonRep.GetQueryable<T_CONTACTOR_REPLACE>().FirstOrDefault(o => o.Id == dto.Id);
                if (entity  == null)
                {
                    var contactor = CommonRep.GetQueryable<Contactor>().FirstOrDefault(o=>o.CustomerNum == dto.CustomerNum &&o.SiteUseId == dto.SiteUseId && o.Title == dto.Title && o.Name == dto.Name);
                    if (contactor == null)
                    {
                        var customer = CommonRep.GetQueryable<Customer>().FirstOrDefault(o => o.CustomerNum == dto.CustomerNum && o.SiteUseId == dto.SiteUseId);

                        contactor = new Contactor();
                        contactor.CustomerNum = dto.CustomerNum;
                        contactor.SiteUseId = dto.SiteUseId;
                        contactor.Name = dto.Name;
                        contactor.Title = dto.Title;
                        contactor.EmailAddress = dto.Email;
                        contactor.IsDefaultFlg = "1";
                        contactor.LegalEntity = customer != null ? customer.Organization : "";
                        contactor.CommunicationLanguage = customer != null ? customer.ContactLanguage : "";
                        contactor.ToCc = "1";
                        contactor.IsCostomerContact = true;
                        contactor.Deal = AppContext.Current.User.Deal;

                        CommonRep.Add(contactor);
                    }

                    CommonRep.Add(dto);
                }
                else
                {
                    entity.Name = dto.Name;
                    entity.CustomerNum = dto.CustomerNum;
                    entity.SiteUseId = dto.SiteUseId;
                    entity.Title = dto.Title;
                    entity.Email = dto.Email;
                    entity.ChangeTo = dto.ChangeTo;
                }

                CommonRep.Commit();
                return 1;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                return -1;
            }
        }

        /// <summary>
        /// 删除所有
        /// </summary>
        /// <returns></returns>
        public int Remove()
        {
            var entities = CommonRep.GetQueryable<T_CONTACTOR_REPLACE>();
            CommonRep.RemoveRange(entities);
            CommonRep.Commit();
            return entities.Count();
        }

        /// <summary>
        /// 删除一个
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Remove(int id)
        {
            var entity = CommonRep.GetQueryable<T_CONTACTOR_REPLACE>().FirstOrDefault(o => o.Id == id);
            if (entity != null)
            {
                CommonRep.Remove(entity);
                CommonRep.Commit();
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 删除一部分
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public int Remove(int[] ids)
        {
            var entities = CommonRep.GetQueryable<T_CONTACTOR_REPLACE>().Where(o => ids.Contains(o.Id));
            CommonRep.RemoveRange(entities);
            return entities.Count();
        }

        public string Export()
        {
            string custPathName = "CustPathName";
            string tempFile = HttpContext.Current.Server.MapPath("~/Template/ContactorReplaceExport.xlsx");
            string targetFoler = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString());
            string targetFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString() + "ContactorReplaceExport." + AppContext.Current.User.EID + ".xlsx");
            if (Directory.Exists(targetFoler) == false)
            {
                Directory.CreateDirectory(targetFoler);
            }

            var entities = CommonRep.GetQueryable<T_CONTACTOR_REPLACE>();
            WriteToExcel(tempFile, targetFile, entities);

            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }

            string virPathName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[custPathName].ToString().Trim('~') + "ContactorReplaceExport." + AppContext.Current.User.EID + ".xlsx";
            return virPathName;
        }

        public string Import()
        {
            FileType fileType = FileType.ContactorReplace;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                string strMasterDataKey = "ImportMasterData";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strMasterDataKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileType.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + ".xlsx";

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileType);

                return ImportContactorReplace();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!\r\n" + ex.Message);
            }
        }

        #region Export Contactor
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void WriteToExcel(string temp, string target, IQueryable<T_CONTACTOR_REPLACE> models)
        {
            try
            {
                NpoiHelper helper = new NpoiHelper(temp);
                helper.Save(target, true);
                helper = new NpoiHelper(target);

                //向sheet为Customer的excel中写入文件

                ISheet sheet = helper.Book.GetSheetAt(0);
                ICellStyle styleCell = helper.Book.CreateCellStyle();
                IFont font = helper.Book.CreateFont();
                font.FontName = "Arial";
                font.FontHeight = 9;
                styleCell.SetFont(font);
                styleCell.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;

                int rowNo = 1;
                foreach (var item in models)
                {
                    IRow row = sheet.CreateRow(rowNo);
                    //CustomerNum
                    ICell cell = row.CreateCell(0);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.CustomerNum);

                    //SiteUseId
                    cell = row.CreateCell(1);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.SiteUseId);

                    //Name
                    cell = row.CreateCell(2);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Name);

                    //Title
                    cell = row.CreateCell(3);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Title);

                    //ChangeTo
                    cell = row.CreateCell(4);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.ChangeTo);

                    rowNo++;
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
        #endregion

        public string ImportContactorReplace()
        {
            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");

            FileUploadHistory fileUpHis = new FileUploadHistory();
            var userId = AppContext.Current.User.EID;

            try
            {
                string strCode = Helper.EnumToCode(FileType.ContactorReplace);
                fileUpHis = fileService.GetSuccessData(strCode);
                if (fileUpHis == null)
                {
                    throw new Exception("import file is not found!");
                }

                List<Contactor> contactors = new List<Contactor>();
                List<T_CONTACTOR_REPLACE> importItems = new List<T_CONTACTOR_REPLACE>();

                #region openXml
                string strpath = fileUpHis.ArchiveFileName;

                ExcelPackage package = new ExcelPackage(new FileInfo(strpath));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号

                for (int j = rowStart + 1; j <= rowEnd; j++)
                {
                    T_CONTACTOR_REPLACE item = new T_CONTACTOR_REPLACE();

                    if (worksheet.Cells[j, 1].Value != null)
                    {
                        item.CustomerNum = worksheet.Cells[j, 1].Value.ToString();
                    }

                    if (worksheet.Cells[j, 2].Value != null)
                    {
                        item.SiteUseId = worksheet.Cells[j, 2].Value.ToString();
                    }

                    if (worksheet.Cells[j, 3].Value != null)
                    {
                        item.Name = worksheet.Cells[j, 3].Value.ToString();
                    }

                    if (worksheet.Cells[j, 4].Value != null)
                    {
                        item.Title = worksheet.Cells[j, 4].Value.ToString();
                    }

                    if (worksheet.Cells[j, 5].Value != null)
                    {
                        item.Email = worksheet.Cells[j, 5].Value.ToString();
                    }
                    importItems.Add(item);
                }

                #endregion

                //CS, Sales , Credit Officer,Finance Manager,Branch Manager, CS Manager, Sales Manager, Local Finance, Collector

                List<Contactor> addContactors = new List<Contactor>();
                foreach (var item in importItems)
                {
                    var entity = CommonRep.GetQueryable<T_CONTACTOR_REPLACE>().FirstOrDefault(o => o.CustomerNum == item.CustomerNum && o.SiteUseId == item.SiteUseId&&o.Name == item.Name &&o.Title == item.Title);
                    if (entity != null)
                    {
                        entity.ChangeTo = item.ChangeTo;
                    }
                    else
                    {
                        var contactor = CommonRep.GetQueryable<Contactor>().FirstOrDefault(o => o.CustomerNum == item.CustomerNum && o.SiteUseId == item.SiteUseId && o.Title == item.Title && o.Name == item.Name);
                        if (contactor == null)
                        {
                            var customer = CommonRep.GetQueryable<Customer>().FirstOrDefault(o => o.CustomerNum == item.CustomerNum && o.SiteUseId == item.SiteUseId);

                            contactor = new Contactor();
                            contactor.CustomerNum = item.CustomerNum;
                            contactor.SiteUseId = item.SiteUseId;
                            contactor.Name = item.ChangeTo;
                            contactor.Title = item.Title;
                            contactor.EmailAddress = item.Email;
                            contactor.IsDefaultFlg = "1";
                            contactor.LegalEntity = customer != null ? customer.Organization : "";
                            contactor.CommunicationLanguage = customer != null ? customer.ContactLanguage : "";
                            contactor.ToCc = "1";
                            contactor.IsCostomerContact = true;
                            contactor.Deal = AppContext.Current.User.Deal;

                            CommonRep.Add(contactor);
                        }

                        entity = new T_CONTACTOR_REPLACE();
                        entity.CustomerNum = item.CustomerNum;
                        entity.SiteUseId = item.SiteUseId;
                        entity.Name = item.Name;
                        entity.Title = item.Title;
                        entity.Email = item.Email;
                        entity.ChangeTo = item.ChangeTo;
                        CommonRep.Add(entity);
                    }
                }

                CommonRep.Commit();

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
    }
}
