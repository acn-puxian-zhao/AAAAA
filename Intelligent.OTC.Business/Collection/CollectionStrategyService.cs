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
        public List<CollectionStrategyDto> GetAllCollectionStrategyList()
        {
            try
            {
                var list = CacheSvr.GetOrSet<List<T_CollectionStrategy>>("Cache_CollectionStrategyList", () =>
                {
                    return CommonRep.GetQueryable<T_CollectionStrategy>().ToList();
                });
                return Mapper.Map<List<CollectionStrategyDto>>(list);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public CollectionStrategyDto GetCollectionStrategyById(int id)
        {
            try
            {
                var entity = CommonRep.GetQueryable<T_CollectionStrategy>().FirstOrDefault(s => s.Id == id);
                if (entity == null)
                    return null;
                return Mapper.Map<CollectionStrategyDto>(entity);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void AddCollectionStrategy(CollectionStrategyDto dto)
        {
            try
            {
                var entity = Mapper.Map<T_CollectionStrategy>(dto);
                if (CommonRep.GetDbSet<T_CollectionStrategy>().Count() > 0)
                {
                    var max = CommonRep.GetDbSet<T_CollectionStrategy>().Max(s => s.Id);
                    entity.Id = max + 1;
                }
                else
                {
                    entity.Id = 1;
                }
                entity.CreationTime = DateTime.Now;
                entity.CreatorUserId = CurrentUser.Id;
                entity.LastModificationTime = DateTime.Now;
                entity.LastModifierUserId = CurrentUser.Id;
                CommonRep.Add(entity);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void UpdateCollectionStrategy(CollectionStrategyDto dto)
        {
            try
            {
                T_CollectionStrategy entity = CommonRep.FindBy<T_CollectionStrategy>(dto.Id);
                entity.DealId = dto.DealId;
                entity.Name = dto.Name;
                entity.Description = dto.Description;
                entity.Confirm1CommunicationMethod = dto.Confirm1CommunicationMethod;
                entity.Confirm1Days = dto.Confirm1Days;
                entity.Confirm2CommunicationMethod = dto.Confirm2CommunicationMethod;
                entity.Confirm2Days = dto.Confirm2Days;
                entity.RemindingCommunicationMethod = dto.RemindingCommunicationMethod;
                entity.RemindingDays = dto.RemindingDays;
                entity.Dunning1CommunicationMethod = dto.Dunning1CommunicationMethod;
                entity.Dunning1Days = dto.Dunning1Days;
                entity.Dunning2CommunicationMethod = dto.Dunning2CommunicationMethod;
                entity.Dunning2Days = dto.Dunning2Days;
                entity.LastModificationTime = DateTime.Now;
                entity.LastModifierUserId = CurrentUser.Id;
                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void DeleteCollectionStrategy(int id)
        {
            try
            {
                T_CollectionStrategy entity = CommonRep.FindBy<T_CollectionStrategy>(id);
                entity.IsDeleted = true;
                entity.DeleterUserId= CurrentUser.Id;
                entity.DeletionTime = DateTime.Now;
                entity.LastModificationTime = DateTime.Now;
                entity.LastModifierUserId = CurrentUser.Id;
                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void DeleteCollectionStrategy(CollectionStrategyDto dto)
        {
            if (dto == null) return;
            DeleteCustomerScore(dto.Id);
        }
    }
}
