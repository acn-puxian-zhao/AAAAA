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
        public List<CustomerScoreDto> GetAllCustomerScoreList()
        {
            try
            {
                var list = CacheSvr.GetOrSet<List<T_CustomerScore>>("Cache_CustomerScoreList", () =>
                {
                    return CommonRep.GetQueryable<T_CustomerScore>().ToList();
                });
                return Mapper.Map<List<CustomerScoreDto>>(list);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public CustomerScoreDto GetCustomerScoreById(int id)
        {
            try
            {
                var entity = CommonRep.GetQueryable<T_CustomerScore>().FirstOrDefault(s => s.Id == id);
                if (entity == null)
                    return null;
                return Mapper.Map<CustomerScoreDto>(entity);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void AddCustomerScore(CustomerScoreDto dto)
        {
            try
            {
                var entity = Mapper.Map<T_CustomerScore>(dto);
                CommonRep.Add(entity);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void UpdateCustomerScore(CustomerScoreDto dto)
        {
            try
            {
                T_CustomerScore entity = CommonRep.FindBy<T_CustomerScore>(dto.Id);
                entity.DealId = dto.DealId;
                entity.CustomerId = dto.CustomerId;
                entity.SiteUseId = dto.SiteUseId;
                entity.FactorId = dto.FactorId;
                entity.FactorScore = dto.FactorScore;
                entity.FactorValue = dto.FactorValue;
                entity.SourceValue1 = dto.SourceValue1;
                entity.SourceValue2 = dto.SourceValue2;
                entity.SourceValue3 = dto.SourceValue3;
                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void DeleteAllCustomerScore()
        {
            try
            {
                CommonRep.GetDBContext().Database.ExecuteSqlCommand("truncate table T_CustomerScore");
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }
        public void DeleteCustomerScore(int id)
        {
            try
            {
                T_CustomerScore entity = CommonRep.FindBy<T_CustomerScore>(id);
                CommonRep.Remove(entity); 
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void DeleteCustomerScore(CustomerScoreDto dto)
        {
            if (dto == null) return;
            DeleteCustomerScore(dto.Id);
        }
    }
}
