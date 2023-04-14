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
    public class CashCollectedService : ICashCollectedService
    {
        public OTCRepository CommonRep { get; set; }

        /// <summary>
        /// get all CustomerAgingStaging Data from Db
        /// </summary>
        /// <returns></returns>
        public IQueryable<Object> GetCashCollected(int year, int month)
        {
            DateTime selectMonth = Convert.ToDateTime(year + "-" + month + "-" + "1").Date;
            DateTime selectMonth2 = selectMonth.AddMonths(1);
            var query1 = CommonRep.GetQueryable<V_CashCollected>().Where(p => p.BACK_DATE >= selectMonth && p.BACK_DATE < selectMonth2);
            var cc = from q1 in query1
                         group q1 by new { q1.Region } into b
                         select new
                         {
                             Region = b.Key.Region,
                             blance = b.Sum(c => c.balanceP)
                         };

            var region = from td in CommonRep.GetQueryable<SysTypeDetail>()
                         where td.TypeCode == "044"
                         select td;

            var result = from td in region
                         join q in cc
                         on td.DetailName equals q.Region
                         into qtd
                         from qs in qtd.DefaultIfEmpty()
                         select new { Region = td.DetailName, blance = qs == null ? 0 : qs.blance };

            return result;
        }
    }
}
