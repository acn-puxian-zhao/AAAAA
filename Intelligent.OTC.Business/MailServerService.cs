using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business
{
    public class MailServerService : IMailServerService
    {
        public OTCRepository CommonRep { get; set; }
        #region Get All MailServer
        public IQueryable<T_MailServer> GetMailServer(string mailDomain)
        {
            return CommonRep.GetQueryable<T_MailServer>().Where(o => o.MailDomain == mailDomain);
        }
        #endregion
    }
}
