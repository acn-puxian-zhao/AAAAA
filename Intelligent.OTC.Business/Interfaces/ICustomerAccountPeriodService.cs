using Intelligent.OTC.Domain.DataModel;
using System.Linq;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface ICustomerAccountPeriodService
    {
        IQueryable<T_Customer_AccountPeriod> GetByNumAndSiteUseId(T_Customer_AccountPeriod customerAccountPeriod);

        string SaveAccountPeriod(T_Customer_AccountPeriod customerAccountPeriod, string isAdd);

        void DeleteAccountPeriod(int id);

        string ImportAccountPeriod();
    }
}
