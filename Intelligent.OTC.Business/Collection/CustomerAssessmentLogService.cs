using AutoMapper;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Intelligent.OTC.Business.Collection
{
    public partial class CollectionService
    {
        public List<CustomerAssessmentLogDto> GetAllCustomerAssessmentLogList()
        {
            try
            {
                var list = CacheSvr.GetOrSet<List<T_CustomerAssessment_Log>>("Cache_CustomerAssessmentLogList", () =>
                {
                    return CommonRep.GetQueryable<T_CustomerAssessment_Log>().ToList();
                });
                return Mapper.Map<List<CustomerAssessmentLogDto>>(list);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public int AddCustomerAssessmentLog(CustomerAssessmentLogDto dto)
        {
            try
            {
                var entity = Mapper.Map<T_CustomerAssessment_Log>(dto);
                CommonRep.Add(entity);
                CommonRep.Commit();
                return entity.Id;
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void UpdateCustomerAssessmentLog(CustomerAssessmentLogDto dto)
        {
            try
            {
                T_CustomerAssessment_Log entity = CommonRep.FindBy<T_CustomerAssessment_Log>(dto.Id);
                entity.EffectiveDate = dto.EffectiveDate;
                entity.EffectiveUser = dto.EffectiveUser;
                entity.Status = entity.Status;
                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }
    }
}
