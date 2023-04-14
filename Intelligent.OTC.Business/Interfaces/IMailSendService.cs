using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface IMailSendService
    {
        void sendTaskMail(string taskId);

        void sendTaskFinishedMail(string strBody, string strEid);
        void sendTaskFinishedMail(string strBody, string strEid, string fileId);
        void sendCustomerBankMail(CaBankStatementDto bank, CustomerMenuDto customer);
    }
}
