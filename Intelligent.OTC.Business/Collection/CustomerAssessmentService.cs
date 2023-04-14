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
        public List<CustomerAssessmentDto> GetAllCustomerAssessmentList()
        {
            try
            {
                var list = CacheSvr.GetOrSet<List<T_CustomerAssessment>>("Cache_CustomerAssessmentList", () =>
                {
                    return CommonRep.GetQueryable<T_CustomerAssessment>().ToList();
                });
                return Mapper.Map<List<CustomerAssessmentDto>>(list);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public CustomerAssessmentDto GetCustomerAssessmentById(int id)
        {
            try
            {
                var entity = CommonRep.GetQueryable<T_CustomerAssessment>().FirstOrDefault(s => s.Id == id);
                if (entity == null)
                    return null;
                return Mapper.Map<CustomerAssessmentDto>(entity);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void AddCustomerAssessment(CustomerAssessmentDto dto)
        {
            try
            {
                var entity = Mapper.Map<T_CustomerAssessment>(dto);
                CommonRep.Add(entity);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }
        public void UpdateCustomerAssessment(CustomerAssessmentDto dto)
        {
            try
            {
                T_CustomerAssessment entity = CommonRep.FindBy<T_CustomerAssessment>(dto.Id);
                entity.DealId = dto.DealId;
                entity.CustomerId = dto.CustomerId;
                entity.SiteUseId = dto.SiteUseId;
                entity.AssessmentScore = dto.AssessmentScore;
                entity.AssessmentType = dto.AssessmentType;
                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void DeleteCustomerAssessment(int id)
        {
            try
            {
                T_CustomerAssessment entity = CommonRep.FindBy<T_CustomerAssessment>(id);
                CommonRep.Remove(entity);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void DeleteCustomerAssessment(CustomerAssessmentDto dto)
        {
            if (dto == null) return;
            DeleteCustomerAssessment(dto.Id);
        }
    }
}
