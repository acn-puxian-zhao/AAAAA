using Intelligent.OTC.Domain.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business
{
    public interface IJobService
    {
        List<SysTypeDetail> GetSysTypeDetail(string strTypecode);
        bool addUpdFileHistory(FileUploadHistory updFileHistory);
        List<FileUploadHistory> GetAutoData(string legal);
        string GetDealByLegal(string legal);

        List<string> GetLegalEntitys();
        bool GetLegalEntityIsFinish(string legalEntity);
        List<FileUploadHistory> GetAllPendingUploadFile();
        List<FileUploadHistory> GetAutoDataCus();
        List<FileUploadHistory> GetAutoDataVAT();
        List<FileUploadHistory> GetAutoDataInvoiceDetail();
        List<FileUploadHistory> GetAutoDataToday();
        void getAllInvoiceByUserForArrow(string isPTPOverDue, string invoiceState = "", string invoiceTrackState = "", string invoiceNum = "", string soNum = "", string poNum = "", string invoiceMemo = "");
    }
}
