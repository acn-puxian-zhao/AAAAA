using System;
using System.Web;
using Intelligent.OTC.Domain.DataModel;
using System.Collections.Generic;
using System.Linq;


namespace Intelligent.OTC.Business
{
    public interface IFileService
    {
        void UploadFile(HttpPostedFile file, string archiveFileName, Intelligent.OTC.Domain.DataModel.FileType fileType, bool updateUnProcessedFile = false,bool isSave = true);
        void UpdateProcessFlagCancel(string strType);
        List<FileUploadHistory> GetUntreatedHistoryList();
        IQueryable<FileUploadHistory> GetFileUploadHistory();
        FileUploadHistory GetNewestData(string strType);
        void upLoadHisUp(FileUploadHistory accFileName,
                                    FileUploadHistory invFileName
                                    , UploadStates sts
                                    , DateTime? dt
                                    , string strSite
                                    , int datasizeAcc
                                    , int datasizeInv
                                    , string strImportId);

        void downloadFileInsert(string strReportName, string strReportFullname,
                                        string strImportId, UploadStates sts);
        List<FileDownloadHistory> getDownloadInfoByUser();

        List<FileDownloadHistory> getAllDownloadInfo();
        List<FileUploadHistory> GetSucDataByImportId(List<FileUploadHistory> importIds);
        void commitHisUp(List<FileUploadHistory> files);
    }
}
