using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface ICaBankStatementService
    {
        CaBankStatementDtoPage getCaBankStatementList(string statusselect, string legalEntity, string transNumber, string transcurrency, string transamount, string transCustomer, string transaForward, string valueDataF, string valueDataT, string createDateF, string createDateT, string ishistory,string bsType, int page,int pageSize);

        void updateBank(CaBankStatementDto dto);

        void saveBank(CaBankStatementDto dto);

        void deleteBank(string bankId);

        void cancelCaMailAlertbyid(string id);

        int isExistedTransactionNum(string bankId, string transactionNum);

        CaBankStatementDtoPage getUnknownBankStatementList(int page, int pageSize);

        CaBankStatementDto getBankStatementById(string id);


        CustomerMenuDtoPage allAgentCustomerDataDetails(int page, int pageSize, string legalEntity);
 

        CustomerMenuDtoPage likeAgentCustomerDataDetails(int page, int pageSize, string bankid);
 

        CustomerMenuDtoPage likePaymentCustomerDataDetails(int page, int pageSize, string bankid);

        CaARViewDtoAndAmtTotal getArHisDataDetails(string customerNum, string legalEntity);
 
        CaPmtDtoPage getCaPmtDetailList(string groupNo, string legalEntity, string customerNum, string currency, string amount, string transactionNumber, string invoiceNum, string valueDateF, string valueDateT, string createDateF, string createDateT, string isClosed, string hasBS, string hasInv, string hasMatched, int page, int pageSize);
        HttpResponseMessage exporPmtDetail(string groupNo, string legalEntity, string customerNum, string currency, string amount, string transactionNumber, string invoiceNum, string valueDateF, string valueDateT, string createDateF, string createDateT, string isClosed, string hasBS, string hasInv, string hasMatched);
        CaPmtDtoPage getCaPmtDetailListByBsId(string bsId);

        List<CaPMTBSDto> getCaPmtBsListById(string pmtid);
        List<CaPMTDetailDto> GetCaPMTDetailListById(string pmtid);

        void updateBsMatchStatusById(string bsId, string matchStatus);

        void changeNeedSendMail(string id, bool needSendMail);
        void changeNeedSendMailAll(string bankStatementId, bool needSendMail);

        List<CustomerMenuDto> likePaymentCustomer(string bankid);

        CustomerMenuDtoPage allPaymentCustomerDataDetails(int page, int pageSize, string legalEntity);

        CaBankStatementDtoPage getBankHistoryListByTaskType(string taskId, string taskType, int page, int pageSize);

        List<CaBankStatementDto> GetBankByTaskId(string taskId);

        List<CaBankStatementDto> GetBankByTranc(string transactionNum);

        HttpResponseMessage exporBankStatementAll(string statusselect, string legalEntity, string transNumber, string transcurrency, string transamount, string transCustomer, string transaForward, string valueDataF, string valueDataT, string createDateF, string createDateT, string ishistory, string bsType);
        CaARViewDtoAndAmtTotal getReconArHisDataDetails(string reconId);

        void RemovePmtBs(string id); 
        FileDto getFileById(string fileId);

        FileDto doExportUnknownDataByIds(List<CaBankStatementDto> banks);
        string revert(CaBankStatementDto dto);
        CaMailAlertDtoPage getCaMailAlertListbybsid(string bsid, string alertType, int page, int pageSize);
    }

}
