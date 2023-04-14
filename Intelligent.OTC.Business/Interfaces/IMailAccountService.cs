using Intelligent.OTC.Domain.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface IMailAccountService
    {
        string AddMailAccount(T_MailAccount mailAccount);

        string UpdateMailAccount(T_MailAccount mailAccount);

        #region Get All MailAccount
        IQueryable<T_MailAccount> GetAllMailAccount(string userId);
        IQueryable<T_MailAccount> GetMailAccountBySendMailAddress(string strUserName);
        #endregion
    }
}