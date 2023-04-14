using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface ICaPaymentDetailService
    {
        int countByBsId(string bsId);

        string getPMTIdByBsId(string bsId);

        CaPMTDto getPMTById(string id);

        CaPMTDto getPMTByBsId(string id);

        void deletePMTById(string id);

        string savePMTDetail(CaPMTDto dto);

        CaBankStatementDto GetBankStatementByTranINC(string transactionNumber);

        CaPMTDetailDto GetInvoiceInfoByNum(string invoiceNum);

        void savePMTBSByBank(CaBankStatementDto bank, string pmtId);
        bool checkPMTAvailable(string pmtId);
    }

}
