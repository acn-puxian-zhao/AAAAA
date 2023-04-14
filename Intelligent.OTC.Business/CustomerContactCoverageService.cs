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
    public class CustomerContactCoverageService : ICustomerContactCoverageService
    {
        public OTCRepository CommonRep { get; set; }

        /// <summary>
        /// get all CustomerAgingStaging Data from Db
        /// </summary>
        /// <returns></returns>
        public IQueryable<Object> GetCustomerContactCount(int year,int month)
        {
            var scc = from sccc in CommonRep.GetQueryable<V_STATISTICS_CUSTOMER_COUNT>()
                         where sccc.y == year && sccc.m == month
                         select new { region = sccc.Region, amt = sccc.customerCount };

            var region = from td in CommonRep.GetQueryable<SysTypeDetail>()
                         where td.TypeCode == "044"
                         select td;

            var result = from td in region
                         join q in scc
                         on td.DetailName equals q.region
                         into qtd
                         from qs in qtd.DefaultIfEmpty()
                         select new { region = td.DetailName, amt = qs == null ? 0 : qs.amt};

            return result;
        }

        /// <summary>
        /// get all CustomerAgingStaging Data from Db
        /// </summary>
        /// <returns></returns>
        public IQueryable<Object> GetCustomerCountPercent(int year, int month)
        {

            var query = from sccc in CommonRep.GetQueryable<V_STATISTICS_CUSTOMER_COUNT>()
                        where sccc.y == year && sccc.m == month
                        select new { region = sccc.Region, customerCount = sccc.customerCount };

            var query2 = from c in CommonRep.GetQueryable<CustomerMasterData>()
                         where c.STATUS == "1"
                         group c by c.Region into ci
                         select new { region = ci.Key == null ? "other" : ci.Key, customerCount = ci.Count()};

            var qq = from q1 in query
                         join q2 in query2
                         on q1.region equals q2.region
                         select new { region = q1.region, percent = ((decimal?)q1.customerCount / (decimal?)q2.customerCount) * 100 };

            var region = from td in CommonRep.GetQueryable<SysTypeDetail>()
                         where td.TypeCode == "044"
                         select td;

            var result = from td in region
                         join q in qq
                         on td.DetailName equals q.region
                         into qtd
                         from qs in qtd.DefaultIfEmpty()
                         select new { region = td.DetailName, percent = qs == null ? 0 : qs.percent };

            return result;
        }
    }
}
