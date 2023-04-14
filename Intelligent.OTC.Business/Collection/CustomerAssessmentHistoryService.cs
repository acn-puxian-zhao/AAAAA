using AutoMapper;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Collection
{
    public partial class CollectionService
    {
        public List<CustomerAssessmentHistoryDto> GetCustomerAssessmentHistoryList(int version)
        {
            try
            {
                var list = CacheSvr.GetOrSet<List<T_CustomerAssessment_History>>("Cache_CustomerAssessmentHistoryList", () =>
                {
                    return CommonRep.GetQueryable<T_CustomerAssessment_History>().Where(s=>s.Version==version).ToList();
                });
                return Mapper.Map<List<CustomerAssessmentHistoryDto>>(list);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }
    }
}
