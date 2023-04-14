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
  public partial  class CollectionService
    {
        public List<DealDto> GetAllDealList()
        {
            try
            {
                var list = CacheSvr.GetOrSet<List<T_Deal>>("Cache_DealList", () =>
                {
                    return CommonRep.GetQueryable<T_Deal>().ToList();
                });
                return Mapper.Map<List<DealDto>>(list);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public DealDto GetDealById(int id)
        {
            try
            {
                var deal = CommonRep.GetQueryable<T_Deal>().FirstOrDefault(s => s.Id == id);
                if (deal == null)
                    return null;
                return Mapper.Map<DealDto>(deal);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void AddDeal(DealDto dto)
        {
            try
            {
                var deal = Mapper.Map<T_Deal>(dto);
                if (CommonRep.GetDbSet<T_Deal>().Count() > 0)
                {
                    var max = CommonRep.GetDbSet<T_Deal>().Max(s => s.Id);
                    deal.Id = max + 1;
                }
                else
                {
                    deal.Id = 1;
                }
                deal.CreationTime = DateTime.Now;
                deal.CreatorUserId = CurrentUser.Id;
                deal.LastModificationTIme = DateTime.Now;
                deal.LastModifierUserId = CurrentUser.Id;
                CommonRep.Add(deal);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void UpdateDeal(DealDto dto)
        {
            try
            {
                T_Deal deal = CommonRep.FindBy<T_Deal>(dto.Id);
                deal.Name = dto.Name;
                deal.Description = dto.Description;
                deal.LastModificationTIme = DateTime.Now;
                deal.LastModifierUserId = CurrentUser.Id;
                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void DeleteDeal(int id)
        {
            try
            {
                T_Deal deal = CommonRep.FindBy<T_Deal>(id);
                deal.IsDeleted = true;
                deal.DeletionTime = DateTime.Now;
                deal.DeleterUserId = CurrentUser.Id;
                deal.LastModificationTIme = DateTime.Now;
                deal.LastModifierUserId = CurrentUser.Id;
                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void DeleteDeal(DealDto dto)
        {
            if (dto == null) return;
            DeleteDeal(dto.Id);
        }
    }
}
