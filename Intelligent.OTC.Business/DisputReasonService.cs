using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business
{
    public class DisputReasonService : IDisputReasonService
    {
        public OTCRepository CommonRep { get; set; }

        /// <summary>
        /// get all CustomerAgingStaging Data from Db
        /// </summary>
        /// <returns></returns>
        public IQueryable<DisputReasonDto> GetDisputReason(string region)
        {
            IQueryable<DisputReasonDto> result = null;

            if (region == "all")
            {
                var query = from dr in CommonRep.GetQueryable<V_DisputReason>()
                        group dr by dr.DETAIL_NAME into drin
                        select new 
                        {
                            blance = drin.Sum(p => p.blance),
                            dn = drin.Key
                        };

                var disputeReason = from td in CommonRep.GetQueryable<SysTypeDetail>()
                             where td.TypeCode == "049"
                                    select td;

                result = from td in disputeReason
                         join q in query
                        on td.DetailName equals q.dn
                        into qtd
                        from qs in qtd.DefaultIfEmpty()
                        select new DisputReasonDto
                        { DETAIL_NAME = td.DetailName, blance = qs == null ? 0 : qs.blance };
            }
            else
            {
                var query = from dr in CommonRep.GetQueryable<V_DisputReason>()
                        where dr.Region == region
                        select new
                        {
                            blance = dr.blance,
                            dn = dr.DETAIL_NAME
                        };

                var disputeReason = from td in CommonRep.GetQueryable<SysTypeDetail>()
                             where td.TypeCode == "049"
                                    select td;

                result = from td in disputeReason
                         join q in query
                         on td.DetailName equals q.dn
                         into qtd
                         from qs in qtd.DefaultIfEmpty()
                         select new DisputReasonDto
                         { DETAIL_NAME = td.DetailName, blance = qs == null ? 0 : qs.blance };
            }
            result.OrderBy(o => o.DETAIL_NAME);

            return result;
        }
    }
}
