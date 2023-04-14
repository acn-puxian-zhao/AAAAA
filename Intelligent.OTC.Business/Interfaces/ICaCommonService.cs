using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Intelligent.OTC.Business.Interfaces
{
    interface ICaCommonService
    {
        string getCARegionByCurrentUser();
        string UploadBankStatementFile(HttpPostedFile file, string archiveFileName);
        string UploadRemittance(HttpPostedFile file, string archiveFileName, string uploadFileName);

        string UploadPMTDetailByFileId(string fileId);
        CaActionTaskPage getActionTaskList(string transactionNumber, string status, string currency, string dateF, string dateT, int page, int pageSize);
        string savePMT(CaPMTDto caPMTDto);

        void savePMTBS(string reconId, string customerNumber, List<CaPMTBSDto> pmtBSList, string siteUseId);

        void savePMTDetail(string reconId, List<CaPMTDetailDto> pmtDetailList);
        string postAndClear(string type, string[] bsid, string taskId, string userId, int isAutoRecon=1);

        string SendPmtDetailMail(string type, string[] bsid);

        string CheckAndsavePMT(CaPMTDto caPMTDto);
        List<CaPostResultCheck> getCaPostResultCheck(string fDate, string tDate);
        List<CaClearResultCheck> getCaClearResultCheck(string fDate, string tDate);
        HttpResponseMessage exportPostClearResult(string fDate, string tDate);
    }
}
