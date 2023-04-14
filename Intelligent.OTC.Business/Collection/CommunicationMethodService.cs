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
        public List<CommunicationMethodDto> GetCommunicationMethodList()
        {
            try
            {
                var list = CacheSvr.GetOrSet<List<T_CommunicationMethod>>("Cache_CommunicationMethodList", () =>
                {
                    return CommonRep.GetQueryable<T_CommunicationMethod>().ToList();
                });
                return Mapper.Map<List<CommunicationMethodDto>>(list);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public CommunicationMethodDto GetCommunicationMethodById(int id)
        {
            try
            {
                var deal = CommonRep.GetQueryable<T_CommunicationMethod>().FirstOrDefault(s => s.Id == id);
                if (deal == null)
                    return null;
                return Mapper.Map<CommunicationMethodDto>(deal);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void AddCommunicationMethod(CommunicationMethodDto dto)
        {
            try
            {
                var deal = Mapper.Map<T_CommunicationMethod>(dto);
                if (CommonRep.GetDbSet<T_CommunicationMethod>().Count() > 0)
                {
                    var max = CommonRep.GetDbSet<T_CommunicationMethod>().Max(s => s.Id);
                    deal.Id = max + 1;
                }
                else
                {
                    deal.Id = 1;
                }
                deal.CreationTime = DateTime.Now;
                deal.CreatorUserId = CurrentUser.Id;
                CommonRep.Add(deal);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void UpdateCommunicationMethod(CommunicationMethodDto dto)
        {
            try
            {
                T_CommunicationMethod deal = CommonRep.FindBy<T_CommunicationMethod>(dto.Id);
                deal.Name = dto.Name;
                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void DeleteCommunicationMethod(int id)
        {
            try
            {
                T_CommunicationMethod deal = CommonRep.FindBy<T_CommunicationMethod>(id);
                deal.IsDisabled = true;
                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void DeleteCommunicationMethod(CommunicationMethodDto dto)
        {
            if (dto == null) return;
            DeleteCommunicationMethod(dto.Id);
        }
    }
}
