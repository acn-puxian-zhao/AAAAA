using Intelligent.OTC.Domain.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface IMailServerService
    {
        IQueryable<T_MailServer> GetMailServer(string mailDomain);
    }
}
