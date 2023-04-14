using Common.Logging;
using FluentFTP;
using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;

namespace Intelligent.OTC.Job
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]//不允许此 Job 并发执行任务（禁止新开线程执行）
    public class DownFTPFileJob : BaseJob
    {

        public const char strSubSplit = '_';// 分割字符串
        public const char strSubSplitDate = '-';// 分割字符串
        FileType fileT = FileType.Account;

        string legalEntity = string.Empty;
        string uploadType = string.Empty;

        string archivePath = string.Empty;
        string archiveFileName = string.Empty;
        public const string strArchiveAccountKey = "ArchiveAccountLevelPath";//ArchiveAccount路径的config保存名
        public const string strArchiveInvoiceKey = "ArchiveInvoiceLevelPath";//ArchiveInvoice路径的config保存名
        public const string strArchiveInvoiceDetailKey = "ArchiveInvoiceDetailPath";//ArchiveInvoiceDetailPath路径的config保存名
        public const string strArchiveVatKey = "ArchiveVATPath";//ArchiveInvoiceDetailPath路径的config保存名
        public const string strArchiveCusKey = "ImportCustLocalize";//ArchiveCusPath路径的config保存名

        public readonly string strPendingFolder = ConfigurationManager.AppSettings["FtpPendingFolder"].ToString();//New
        public readonly string strPendingCustFolder = ConfigurationManager.AppSettings["FtpPendingCustLocalizeFolder"].ToString();//New
        public readonly string strPendingInvoiceFolder = ConfigurationManager.AppSettings["FtpPendingInvoiceFolder"].ToString();//New
        public readonly string strPendingVatFolder = ConfigurationManager.AppSettings["FtpPendingVatFolder"].ToString();//New
        
        public readonly string ftpCustLocalizeName = ConfigurationManager.AppSettings["FtpCustLocalizeName"].ToString();
        public readonly string ftpInvoiceDetailName = ConfigurationManager.AppSettings["FtpInvoiceDetailName"].ToString();
        public readonly string ftpVatName = ConfigurationManager.AppSettings["FtpVatName"].ToString();

        IJobService Service = SpringFactory.GetObjectImpl<IJobService>("JobService");
        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            FtpClient client = FtpClientInstance();
            try
            {
                List<string> listOrgId = GetLegalEntityList();
                // begin connecting to the server
                client.Connect();
                if (client.DirectoryExists("/" + strPendingFolder))
                {
                    logger.Warn(string.Format("FTP path not exists:{0}", "/" + strPendingFolder));
                }
                if (client.DirectoryExists("/" + strPendingCustFolder))
                {
                    logger.Warn(string.Format("FTP path not exists:{0}", "/" + strPendingCustFolder));
                }
                if (client.DirectoryExists("/" + strPendingInvoiceFolder))
                {
                    logger.Warn(string.Format("FTP path not exists:{0}", "/" + strPendingInvoiceFolder));
                }
                if (client.DirectoryExists("/" + strPendingVatFolder))
                {
                    logger.Warn(string.Format("FTP path not exists:{0}", "/" + strPendingVatFolder));
                }
                DownloadFile(client, listOrgId);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Execution error,Job:{0},ErrorMsg:{1}", ((JobDetailImpl)context.JobDetail).FullName,ex.Message),ex);
            }
            finally
            {
                client.Disconnect();
            }
        }

        private void DownloadFile(FtpClient client, List<string> listOrgId)
        {
            var uploadLog = Service.GetAutoDataToday();
            var fileTypeSummary = Helper.EnumToCode<FileType>(FileType.Account);
            var fileTypeDetail = Helper.EnumToCode<FileType>(FileType.Invoice);
            var fileTypeInvoiceDetail = Helper.EnumToCode<FileType>(FileType.InvoiceDetail);
            var fileTypeVat = Helper.EnumToCode<FileType>(FileType.VAT);
            var fileTypeCust = Helper.EnumToCode<FileType>(FileType.CustLocalize);
            foreach (var legal in listOrgId)
            {
                if (!uploadLog.Any(s => s.FileType == fileTypeSummary && s.LegalEntity == legal))
                {
                    archivePath = ArchiveFile(strArchiveAccountKey, FileType.Account.ToString(), ".csv");
                    var summaryFile = "AR_" + legal + "_summary-" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
                    if (client.FileExists(strPendingFolder + summaryFile))
                    {
                        client.DownloadFile(archiveFileName, strPendingFolder + summaryFile);
                        UploadHistoryLog(summaryFile, archiveFileName, FileType.Account, legal);
                    }
                    else
                    {
                        logger.Warn(string.Format("file not exists:{0}", strPendingFolder + summaryFile));
                    }
                }
                if (!uploadLog.Any(s => s.FileType == fileTypeDetail && s.LegalEntity == legal))
                {
                    archivePath = ArchiveFile(strArchiveInvoiceKey, FileType.Invoice.ToString(), ".csv");
                    var detailFile = "AR_" + legal + "_detail-" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
                    if (client.FileExists(strPendingFolder + detailFile))
                    {
                        client.DownloadFile(archiveFileName, strPendingFolder + detailFile);
                        UploadHistoryLog(detailFile, archiveFileName, FileType.Invoice, legal);
                    }
                    else
                    {
                        logger.Warn(string.Format("file not exists:{0}", strPendingFolder + detailFile));
                    }
                }
            }
            if (!uploadLog.Any(s => s.FileType == fileTypeInvoiceDetail))
            {
                fileT = FileType.InvoiceDetail;
                archivePath = ArchiveFile(strArchiveInvoiceDetailKey, fileT.ToString(), ".csv");
                if (client.FileExists(strPendingInvoiceFolder + ftpInvoiceDetailName))
                {
                    client.DownloadFile(archiveFileName, strPendingInvoiceFolder + ftpInvoiceDetailName);
                    UploadHistoryLog(ftpInvoiceDetailName, archiveFileName, fileT, "");
                }
                else
                {
                    logger.Warn(string.Format("file not exists:{0}", strPendingInvoiceFolder + ftpInvoiceDetailName));
                }
            }
            if (!uploadLog.Any(s => s.FileType == fileTypeVat))
            {
                fileT = FileType.VAT;
                archivePath = ArchiveFile(strArchiveVatKey, fileT.ToString(), ".csv");
                if (client.FileExists(strPendingVatFolder + ftpVatName))
                {
                    client.DownloadFile(archiveFileName, strPendingVatFolder + ftpVatName);
                    UploadHistoryLog(ftpVatName, archiveFileName, fileT, "");
                }
                else
                {
                    logger.Warn(string.Format("file not exists:{0}", strPendingVatFolder + ftpVatName));
                }
            }
            if (!uploadLog.Any(s => s.FileType == fileTypeCust))
            {
                fileT = FileType.CustLocalize;
                archivePath = ArchiveFile(strArchiveCusKey, fileT.ToString(), ".xlsx");
                if (client.FileExists(strPendingCustFolder + ftpCustLocalizeName))
                {
                    client.DownloadFile(archiveFileName, strPendingCustFolder + ftpCustLocalizeName);
                    UploadHistoryLog(ftpCustLocalizeName, archiveFileName, fileT, "");
                }
                else
                {
                    logger.Warn(string.Format("file not exists:{0}", strPendingCustFolder + ftpCustLocalizeName));
                }
            }
        }
        private FtpClient FtpClientInstance()
        {
            FtpClient client = new FtpClient();
            client.Host = ConfigurationManager.AppSettings["FtpClient"];
            //Credential
            var userName = AESUtil.AESDecrypt(ConfigurationManager.AppSettings["FtpUser"]);
            var password = AESUtil.AESDecrypt(ConfigurationManager.AppSettings["FtpPassword"]);
            if (userName != null && userName != string.Empty && password != null && password != string.Empty)
                client.Credentials = new NetworkCredential(userName, password);
            return client;
        }
        private List<string> GetLegalEntityList()
        {
            List<string> listOrgId = new List<string>();
            //DB-SysTypeDetail，TypeCode-015 相当于Legal Entity,查出所有Legal Entity的数据
            List<SysTypeDetail> sites = Service.GetSysTypeDetail("015");
            if (sites == null)
            {
                logger.Error("Legal Entity is null in system");
            }
            else
            {
                listOrgId = sites.Select(o => o.DetailName).ToList();
            }
            return listOrgId;
        }
        private bool UploadHistoryLog(string OriginalFileName, string ArchiveFileName, FileType filetype, string legal)
        {
            FileUploadHistory updFileHistory = new FileUploadHistory();
            updFileHistory.Deal = AppContext.Current.User.Deal;
            updFileHistory.OriginalFileName = OriginalFileName;
            updFileHistory.ArchiveFileName = ArchiveFileName;
            updFileHistory.FileType = Helper.EnumToCode<FileType>(filetype);
            updFileHistory.Operator = "auto";
            updFileHistory.UploadTime = DateTime.Now;
            updFileHistory.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Untreated);
            if (legal != null && legal != string.Empty)
                updFileHistory.LegalEntity = legal;
            var result = Service.addUpdFileHistory(updFileHistory);
            return result;
        }
        private string ArchiveFile(string archiveKey, string fileType, string extName)
        {
            var archivePath = ConfigurationManager.AppSettings[archiveKey].ToString();
            archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
            if (Directory.Exists(archivePath) == false)
            {
                Directory.CreateDirectory(archivePath);
            }
            archiveFileName = archivePath + "\\" + fileType +
                        "-" + Guid.NewGuid().ToString() + extName;
            return archivePath;
        }
    }
}
