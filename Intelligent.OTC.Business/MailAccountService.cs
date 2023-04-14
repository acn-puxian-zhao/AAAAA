using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Linq;

namespace Intelligent.OTC.Business
{
    public class MailAccountService : IMailAccountService
    {
        public OTCRepository CommonRep { get; set; }
        public string AddMailAccount(T_MailAccount mailAccount)
        {
            CommonRep.Add(mailAccount);
            CommonRep.Commit();
            return "Add Success";
        }

        public string UpdateMailAccount(T_MailAccount mailAccount)
        {
            try
            {
                Helper.Log.Info(mailAccount);
                T_MailAccount old = CommonRep.FindBy<T_MailAccount>(mailAccount.Id);
                ObjectHelper.CopyObjectWithUnNeed(mailAccount, old, new string[] { "Id" });
                CommonRep.Commit();
                return "Update Success!";
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        #region Get All MailAccount
        public IQueryable<T_MailAccount> GetAllMailAccount(string userId)
        {
            var result = CommonRep.GetQueryable<T_MailAccount>().Where(u => u.UserId == userId);
            return result;
        }
        #endregion

        public IQueryable<T_MailAccount> GetMailAccountBySendMailAddress(string strUserName)
        {
            var result = CommonRep.GetQueryable<T_MailAccount>().Where(u => u.SenderMailAddress == strUserName);
            return result;
        }
    }
}
