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
    public class OpenARService : IOpenARService
    {
        public OTCRepository CommonRep { get; set; }

        /// <summary>
        /// get all CustomerAgingStaging Data from Db
        /// </summary>
        /// <returns></returns>
        public IQueryable<OpenARDto> GetOpenAR(string region)
        {
            IQueryable<OpenARDto> query = null;
            if (region == "all")
            {
                query = from ar in CommonRep.GetQueryable<V_Open_AR>()
                        group ar by new { gb = 1} into a
                        select new OpenARDto
                        {
                            ACurrent = a.Sum(p => p.ACurrent),
                            B30 = a.Sum(p => p.B30),
                            C60 = a.Sum(p => p.C60),
                            D90 = a.Sum(p => p.D90),
                            E120 = a.Sum(p => p.E120),
                            F150 = a.Sum(p => p.F150),
                            G180 = a.Sum(p => p.G180),
                            H360 = a.Sum(p => p.H360)
                        };
            }
            else
            {
                query = from ar in CommonRep.GetQueryable<V_Open_AR>()
                            where ar.region == region
                            select new OpenARDto
                            {
                                ACurrent = ar.ACurrent,
                                B30 = ar.B30,
                                C60 = ar.C60,
                                D90 = ar.D90,
                                E120 = ar.E120,
                                F150 = ar.F150,
                                G180 = ar.G180,
                                H360 = ar.H360,
                                I360 = ar.DUEOVER360
                            };
            }
            
            return query;
        }
    }
}
