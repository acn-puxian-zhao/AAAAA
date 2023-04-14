using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using Intelligent.OTC.Common.UnitOfWork;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using System.IO;
using System.Web;
using Intelligent.OTC.Domain;
using Intelligent.OTC.Common;
using System.Configuration;
using Intelligent.OTC.Common.Repository;

namespace Intelligent.OTC.Business
{
    public class FileService : IFileService
    {
        public IRepository CommonRep { get; set; }

        public DateTime CurrentTime
        {
            get
            {
                return AppContext.Current.User.Now;
            }
        } 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file">posted file</param>
        /// <param name="filePath">temp save path</param>
        /// <param name="archiveFileName">archive file full path</param>
        /// <param name="fileType"></param>
        /// <param name="cancelUnProcessedFile"></param>
        public void UploadFile(HttpPostedFile file, string archiveFileName, FileType fileType
            , bool cancelUnProcessedFile = false
            , bool isSaved = false )
        {
            AssertUtils.ArgumentHasText(archiveFileName, "File archive path");

            // 1, backup to archive folder
            if (!isSaved)
            {
                file.SaveAs(archiveFileName);
            }
            // 2, Insert the upload file record
            if (cancelUnProcessedFile)
            {
                // cancel all unprocessed file
                string strType = Helper.EnumToCode<FileType>(fileType);
                UpdateProcessFlagCancel(strType);
            }

            //Insert the upload file record
            FileUploadHistory updFileHistory = new FileUploadHistory();
            updFileHistory.Deal = AppContext.Current.User.Deal;
            updFileHistory.OriginalFileName = file.FileName;
            updFileHistory.ArchiveFileName = archiveFileName;
            updFileHistory.FileType = Helper.EnumToCode<FileType>(fileType);
            updFileHistory.Operator = AppContext.Current.User.EID;
            updFileHistory.UploadTime = AppContext.Current.User.Now;
            if (fileType == FileType.Customer || fileType == FileType.PaymentDateCircle
                || fileType == FileType.AccountPeriod || fileType == FileType.CustLocalize
                || fileType == FileType.VarData || fileType == FileType.CustPayment || fileType == FileType.MissingContactor
                || fileType == FileType.ContactorReplace || fileType == FileType.CreditHold || fileType == FileType.CurrencyAmount || fileType == FileType.CustComment || fileType == FileType.CustEBBranch
                || fileType == FileType.CustLitigation || fileType == FileType.CustBadDebt || fileType == FileType.ConsigmentNumber || fileType == FileType.CustCommentsFromCsSales || fileType == FileType.EBSetting)
            {
                updFileHistory.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Success);
            }
            else
            {
                updFileHistory.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Untreated);
            }
            CommonRep.Add(updFileHistory);

            try
            {
                CommonRep.Commit();
            }
            catch
            {
                Exception ex = new OTCServiceException("File upload error happened, Please try again later. " + Environment.NewLine
                    + "If this error happen again, please contact system administrator.");
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Update to process flag cancel
        /// </summary>
        /// <param name="strType"></param>
        public void UpdateProcessFlagCancel(string strType)
        {
            string strUntreated = Helper.EnumToCode<UploadStates>(UploadStates.Untreated);
            var lstUntreated = CommonRep.GetQueryable<FileUploadHistory>().Where(o => o.FileType == strType
                                                    && o.Operator == AppContext.Current.User.EID
                                                    && o.ProcessFlag == strUntreated).Select(o => o);
            foreach (var item in lstUntreated)
            {
                item.ProcessFlag = Helper.EnumToCode(UploadStates.Cancel);
            }
            CommonRep.Commit();
        }

        /// <summary>
        /// Update all datas process flag cancel
        /// </summary>
        /// <param name="strType"></param>
        public void UpdateAllProcessFlagCancel()
        {
            string strUntreated = Helper.EnumToCode<UploadStates>(UploadStates.Untreated);
            var lstUntreated = CommonRep.GetQueryable<FileUploadHistory>().Where(o => o.Operator == AppContext.Current.User.EID
                                                    && o.ProcessFlag == strUntreated).Select(o => o);
            foreach (var item in lstUntreated)
            {
                item.ProcessFlag = Helper.EnumToCode(UploadStates.Cancel);
            }
            CommonRep.Commit();
        }

        /// <summary>
        /// get all untreated datas
        /// </summary>
        /// <param name="strType"></param>
        /// <returns></returns>
        public List<FileUploadHistory> GetUntreatedHistoryList()
        {
            string strUntreated = Helper.EnumToCode<UploadStates>(UploadStates.Untreated);
            return CommonRep.GetQueryable<FileUploadHistory>().Where(o => o.Operator == AppContext.Current.User.EID
                                        && o.ProcessFlag == strUntreated).Select(o => o).OrderByDescending(o => o.UploadTime).ToList();
        }

        public string getNeweastImportIdOfOneYear()
        {
            string rtn;
            PeroidService perService = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            int? perId = perService.getIdOfcurrentPeroid();

            string strSubmit = Helper.EnumToCode<UploadStates>(UploadStates.Submitted);
            string strFileType = Helper.EnumToCode<FileType>(FileType.OneYearSales);
            FileUploadHistory filehis= CommonRep.GetQueryable<FileUploadHistory>()
                .Where(o => o.SubmitFlag == strSubmit
                        && o.PeriodId == perId
                        && o.FileType == strFileType)
                        .Select(o => o).OrderByDescending(o => o.SubmitTime).FirstOrDefault();
            if (filehis == null)
            {
                rtn = null;
            }
            else
            {
                rtn = filehis.ImportId;
            }
            return rtn;
        }

        #region get all FileUploadHistory Data from Db
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<FileUploadHistory> GetFileUploadHistory()
        {
            return CommonRep.GetQueryable<FileUploadHistory>();
        }
        #endregion

        #region get filefullname from Db By ProcessFlag and FileType and Operator
        /// <summary>
        /// upload filefullname get
        /// </summary>
        /// <param name="strType"></param>
        /// <returns></returns>
        public FileUploadHistory GetNewestData(string strType)
        {
            string strDeal = AppContext.Current.User.Deal.ToString();
            string strUntreated = Helper.EnumToCode<UploadStates>(UploadStates.Untreated);
            return CommonRep.GetQueryable<FileUploadHistory>().Where(o => o.FileType == strType
                                        && o.Operator == AppContext.Current.User.EID
                                        && o.ProcessFlag == strUntreated
                                        && o.Deal == strDeal).Select(o => o).OrderByDescending(o => o.UploadTime).FirstOrDefault();
        }
        #endregion

        #region get filefullname from Db By ProcessFlag and FileType and Operator
        /// <summary>
        /// upload filefullname get
        /// </summary>
        /// <param name="strType"></param>
        /// <returns></returns>
        public FileUploadHistory GetNewestSucData(string strType)
        {
            string strDeal = AppContext.Current.User.Deal.ToString();
            string strUntreated = Helper.EnumToCode<UploadStates>(UploadStates.Success);
            return CommonRep.GetQueryable<FileUploadHistory>().Where(o => o.FileType == strType
                                        && o.Operator == AppContext.Current.User.EID
                                        && o.ProcessFlag == strUntreated
                                        && o.Deal == strDeal).Select(o => o).OrderByDescending(o => o.UploadTime).FirstOrDefault();
        }
        #endregion

        public List<FileUploadHistory> GetSucDataByImportId(List<FileUploadHistory> importIds)
        {
            string strDeal = AppContext.Current.User.Deal.ToString();
            string strUntreated = Helper.EnumToCode<UploadStates>(UploadStates.Success);
            return (from file in CommonRep.GetQueryable<FileUploadHistory>().ToList()
                    join imp in importIds
                    on file.ImportId equals imp.ImportId
                    select file).ToList();
        }

        #region Updated
        public void upLoadHisUp(FileUploadHistory accFileName,
                                    FileUploadHistory invFileName
                                    , UploadStates sts
                                    , DateTime? dt
                                    ,string strSite = null
                                    , int datasizeAcc = 0
                                    , int datasizeInv = 0
                                    , string strImportId = null)
        {
            accFileName = CommonRep.GetQueryable<FileUploadHistory>().Where(o => o.Id == accFileName.Id).FirstOrDefault();
            invFileName = CommonRep.GetQueryable<FileUploadHistory>().Where(o => o.Id == invFileName.Id).FirstOrDefault();
            int? perId = null;
            if (accFileName != null && invFileName != null)
            {
                PeroidService perService = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
                var curP = perService.getcurrentPeroid();
                if (curP != null)
                {
                    perId = curP.Id;
                }
                accFileName.ProcessFlag = Helper.EnumToCode<UploadStates>(sts);
                accFileName.ReportTime = dt;
                accFileName.ImportId = strImportId;
                accFileName.DataSize = datasizeAcc;
                accFileName.LegalEntity = strSite;
                accFileName.PeriodId = perId;

                invFileName.ProcessFlag = Helper.EnumToCode<UploadStates>(sts);
                invFileName.DataSize = datasizeInv;
                invFileName.LegalEntity = strSite;
                invFileName.ImportId = strImportId;
                invFileName.PeriodId = perId;

                CommonRep.Commit();
            }
        }
        #endregion

        public void commitHisUp(FileUploadHistory file,int? perId)
        {
            file.SubmitFlag = Helper.EnumToCode<UploadStates>(UploadStates.Submitted);
            file.SubmitTime = AppContext.Current.User.Now;
            file.PeriodId = perId;
            CommonRep.Commit();
        }

        public void commitHisUp(List<FileUploadHistory> files)
        {
            List<FileUploadHistory> newFiles = (from newlist in CommonRep.GetDbSet<FileUploadHistory>().ToList()
                                                join list in files
                                                on newlist.Id equals list.Id
                                                select newlist).ToList();

            PeroidService perService = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            int? perId = perService.getIdOfcurrentPeroid();

            foreach (FileUploadHistory file in newFiles)
            {  
                file.SubmitFlag = Helper.EnumToCode<UploadStates>(UploadStates.Submitted);
                file.SubmitTime = AppContext.Current.User.Now;
                file.PeriodId = perId;
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strReportName"></param>
        /// <param name="strReportFullname"></param>
        /// <param name="strImportId"></param>
        /// <param name="sts"></param>
        public void downloadFileInsert(string strReportName, string strReportFullname,
                                        string strImportId, UploadStates sts)
        {
            FileDownloadHistory fileDownhis = new FileDownloadHistory();
            fileDownhis.DownloadFileFullname = strReportFullname;
            fileDownhis.DownloadFileName = strReportName;
            fileDownhis.DownloadTime = CurrentTime;
            fileDownhis.FileType = Helper.EnumToCode<FileType>(FileType.ConsolidateReport);
            fileDownhis.ImportId = strImportId;
            fileDownhis.Operator = AppContext.Current.User.EID.ToString();
            fileDownhis.ProcessFlag = Helper.EnumToCode<UploadStates>(sts);
            fileDownhis.Deal = AppContext.Current.User.Deal.ToString();

            CommonRep.Add(fileDownhis);

            CommonRep.Commit();

        }

        public void DailyReportInsert(string strReportName, string strReportFullname,
                                 UploadStates sts)
        {
            PeroidService perService = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            int? perId = perService.getIdOfcurrentPeroid();

            FileDownloadHistory fileDownhis = new FileDownloadHistory();
            fileDownhis.DownloadFileFullname = strReportFullname;
            fileDownhis.DownloadFileName = strReportName;
            fileDownhis.DownloadTime = CurrentTime;
            fileDownhis.FileType = Helper.EnumToCode<FileType>(FileType.DailyReport);
            fileDownhis.Operator = AppContext.Current.User.EID.ToString();
            fileDownhis.ProcessFlag = Helper.EnumToCode<UploadStates>(sts);
            fileDownhis.Deal = AppContext.Current.User.Deal.ToString();
            fileDownhis.ImportId = "";
            fileDownhis.PeriodId = perId.Value;

            CommonRep.Add(fileDownhis);

            CommonRep.Commit();

        }

        public List<UploadInfoHis> getFileUploadHis()
        {
            List<UploadInfoHis> rtninfos = new List<UploadInfoHis>();
            UploadInfoHis rtninfo = new UploadInfoHis();
            UploadInfoHis rtninfo2 = new UploadInfoHis();
            UploadInfoHis rtninfoNew = new UploadInfoHis();
            List<UploadInfo> upInfos = new List<UploadInfo>();
            PeroidService perService = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            int iC;
            int sortCode = 1;
            string strDeal = AppContext.Current.User.Deal.ToString();
            List<FileUploadHistory> fileHis = new List<FileUploadHistory>();
            FileUploadHistory file = new FileUploadHistory();
            List<Sites> sites = new List<Sites>();
            List<PeriodControl> pers = perService.GetAllPeroids().OrderByDescending(o=>o.PeriodEnd).ToList();

            List<FileDownloadHistory> downhis = new List<FileDownloadHistory>();
            List<FileDownloadHistory> downfile = new List<FileDownloadHistory>();
            downhis = getAllDownloadInfo();
            perService.getListInfo(strDeal, out fileHis, out sites);
            
            foreach (PeriodControl per in pers)
            {
                upInfos = perService.getCurrentPeroidUploadTimes(out iC, fileHis, sites, per);
                rtninfo = new UploadInfoHis();
                rtninfo.Period = per.PeriodBegin.ToString("yyyy-MM-dd HH:mm:ss") + "~" + per.PeriodEnd.ToString("yyyy-MM-dd HH:mm:ss");
                rtninfo.FileType = "Aging Report";
                rtninfo.DownLoadShowFlg = "1";
                if (upInfos.Where(o => o.AccTimes == 0 || o.InvTimes == 0).Select(o => o).ToList().Count == 0
                    && per.PeriodBegin <= CurrentTime && per.PeriodEnd >= CurrentTime)
                {
                    rtninfo.PeriodFlg = "1";
                }
                else
                {
                    rtninfo.PeriodFlg = "0";
                }

                downfile = new List<FileDownloadHistory>();
                downfile = downhis.Where(o => o.DownloadTime >= per.PeriodBegin &&
                                            o.DownloadTime <= per.PeriodEnd
                                            && o.FileType == Helper.EnumToCode<FileType>(FileType.ConsolidateReport))
                                            .Select(o => o)
                                            .OrderByDescending(o => o.DownloadTime).ToList();

                if (downfile.Count > 0)
                {
                    foreach (FileDownloadHistory dfile in downfile)
                    {
                        rtninfoNew = new UploadInfoHis();
                        ObjectHelper.CopyObject(rtninfo,rtninfoNew);
                        rtninfoNew.sortCode = sortCode;
                        sortCode++;
                        rtninfoNew.DownLoadFlg = "1";
                        rtninfoNew.Operator = dfile.Operator;
                        rtninfoNew.OperatorDate = dfile.DownloadTime;
                        rtninfoNew.DownLoadFullName = dfile.DownloadFileFullname;
                        rtninfos.Add(rtninfoNew);
                    }
                }
                else
                {
                    rtninfo.sortCode = sortCode;
                    sortCode++;
                    rtninfo.DownLoadFlg = "0";
                    rtninfos.Add(rtninfo);
                }


                file = new FileUploadHistory();

                file = fileHis.Where(o => o.SubmitTime >= per.PeriodBegin &&
                                            o.SubmitTime <= per.PeriodEnd
                                            && o.FileType == Helper.EnumToCode<FileType>(FileType.OneYearSales))
                                            .Select(o => o)
                                            .OrderByDescending(o => o.SubmitTime).FirstOrDefault();
                rtninfo2 = new UploadInfoHis();
                rtninfo2.Period = rtninfo.Period;
                rtninfo2.FileType = "One Year Sales";
                rtninfo2.PeriodFlg = "0";
                rtninfo2.DownLoadFlg = "0";
                rtninfo2.DownLoadShowFlg = "0";
                rtninfo2.sortCode = sortCode;
                sortCode++;
                if (file != null)
                {
                    rtninfo2.Operator = file.Operator;
                    rtninfo2.OperatorDate = file.UploadTime;
                }
                else
                {
                    rtninfo2.Operator = null;
                    rtninfo2.OperatorDate = null;
                }
                rtninfos.Add(rtninfo2);
             }

            return rtninfos;
        }

        public List<FileDownloadHistory> getAllDownloadInfo()
        {
            string fileType = Helper.EnumToCode<FileType>(FileType.ConsolidateReport);
            return CommonRep.GetQueryable<FileDownloadHistory>().Where(o => o.FileType == fileType).ToList();
        }

        public IQueryable<FileDownloadHistory> getAllReportInfo()
        {
            string fileType = Helper.EnumToCode<FileType>(FileType.DailyReport);
            return CommonRep.GetQueryable<FileDownloadHistory>()
                .Where(o => o.FileType == fileType);
        }

        public List<FileDownloadHistory> getDownloadInfoByUser()
        {
            return CommonRep.GetQueryable<FileDownloadHistory>()
                        .Where(o => o.Operator == AppContext.Current.User.EID.ToString()).Select(o => o)
                        .OrderByDescending(o=>o.DownloadTime).ToList();
        }

        #region File management
        public AppFile GetAppFile(string fileId)
        {
            return CommonRep.GetDbSet<AppFile>().Where(f => f.FileId == fileId).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="file">Caller should take care the stream closing</param>
        /// <param name="type"></param>
        /// <param name="redemFileName">If 'false' is passed in, caller should ensure the file name is unique. or else, override will happen.</param>
        /// <returns></returns>
        public AppFile AddAppFile(string name, Stream file, FileType type, bool redemFileName = true, string contentType = "", string contentId = "")
        {
            string destFileName = AddPhysicalFile(name, file, type, redemFileName);

            AppFile appFile = new AppFile();
            appFile.FileId = Guid.NewGuid().ToString("N");
            appFile.CreateTime = AppContext.Current.User.Now;
            appFile.FileName = name;
            appFile.PhysicalPath = destFileName;
            appFile.UpdateTime = appFile.CreateTime;
            appFile.Operator = AppContext.Current.User.EID;
            appFile.ContentId = contentId;
            appFile.ContentType = contentType;

            CommonRep.Add(appFile);
            CommonRep.Commit();

            return appFile;
        }

        public string AddPhysicalFile(string name, Stream file, FileType type, bool redemFileName = true)
        {
            string fileRootFolder = string.Empty;
            // decide the file path and save file into it.
            switch (type)
            {
                case FileType.Account:
                    break;
                case FileType.Invoice:
                    break;
                case FileType.OneYearSales:
                    break;
                case FileType.ConsolidateReport:
                    break;
                case FileType.PaymentDateCircle:
                    break;
                case FileType.Customer:
                    break;
                case FileType.MailBodyPart:
                case FileType.MailAttachment:
                    fileRootFolder = ConfigurationManager.AppSettings["MailAttachmentPath"];
                    break;
                case FileType.SOA:
                    fileRootFolder = ConfigurationManager.AppSettings["ArchiveSOAPath"];
                    break;
                case FileType.ReceivedMail:
                    fileRootFolder = ConfigurationManager.AppSettings["ArchiveMailPath"];
                    break;
                case FileType.SentMail:
                    fileRootFolder = ConfigurationManager.AppSettings["ArchiveSentMailPath"];
                    break;
                case FileType.DailyReport:
                    fileRootFolder = ConfigurationManager.AppSettings["DailyAgingReportPath"];
                    break;
                case FileType.PMTExport:
                    fileRootFolder = ConfigurationManager.AppSettings["DailyAgingReportPath"];
                    break;
                default:
                    throw new OTCServiceException("Un-recognized file type provided.");
            }

            if (Directory.Exists(fileRootFolder) == false)
            {
                Directory.CreateDirectory(fileRootFolder);
            }

            string destFileName = string.Empty;

            try
            {
                Path.GetFileName(name);
            }
            catch (ArgumentException ex)
            {
                name = "Illegal_File_Replaced_With_Valid_Name" + AppContext.Current.User.Now.ToString("yyyyMMdd_HHmmss_fff");
                Helper.Log.Error("illegal file name provided to AddPhysicalFile method.", ex);
            }


            if (redemFileName)
            {
                destFileName = Path.Combine(
                    fileRootFolder,
                    Path.GetFileNameWithoutExtension(name) + AppContext.Current.User.Now.ToString("yyyyMMdd_HHmmss_fff") + Path.GetExtension(name));
            }
            else
            {
                destFileName = Path.Combine(fileRootFolder, name);
            }

            using (FileStream dest = File.Create(destFileName))
            {
                file.CopyTo(dest);
            }
            return destFileName;
        }


        public void DeleteAppFile(int id)
        {
            AppFile file = CommonRep.FindBy<AppFile>(id);

            deleteAppFile(file);
        }

        public void DeleteAppFile(string fileId)
        {
            AppFile file = GetAppFile(fileId);

            deleteAppFile(file);
        }

        private void deleteAppFile(AppFile file)
        {
            if (file != null)
            {
                CommonRep.GetDbSet<AppFile>().Remove(file);
                if (File.Exists(file.PhysicalPath))
                {
                    File.Delete(file.PhysicalPath);
                }
                else
                {
                    Exception ex = new OTCServiceException("Remove file failed because file not exist with physical path: " + file.PhysicalPath);
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }
                CommonRep.Commit();
            }
        }
        #endregion

        public List<AppFile> GetAppFiles(List<string> fileIds)
        {
            return CommonRep.GetDbSet<AppFile>().Where(f => fileIds.Contains(f.FileId)).ToList();
        }


        public FileUploadHistory GetSuccessData(string strType)
        {
            string strDeal = AppContext.Current.User.Deal.ToString();
            string strSuccess = Helper.EnumToCode<UploadStates>(UploadStates.Success);
            return CommonRep.GetQueryable<FileUploadHistory>().Where(o => o.FileType == strType
                                        && o.Operator == AppContext.Current.User.EID
                                        && o.ProcessFlag == strSuccess
                                        && o.Deal == strDeal).Select(o => o).OrderByDescending(o => o.UploadTime).FirstOrDefault();
        }
    }
}
