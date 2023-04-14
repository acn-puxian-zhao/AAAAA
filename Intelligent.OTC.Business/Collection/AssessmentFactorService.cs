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
        public List<AssessmentFactorDto> GetAllAssessmentFactorList()
        {
            try
            {
                var list = CacheSvr.GetOrSet<List<T_AssessmentFactor>>("Cache_AssessmentFactorList", () =>
                {
                    return CommonRep.GetQueryable<T_AssessmentFactor>().ToList();
                });
                return Mapper.Map<List<AssessmentFactorDto>>(list);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public AssessmentFactorDto GetAssessmentFactorById(int id)
        {
            try
            {
                var entity = CommonRep.GetQueryable<T_AssessmentFactor>().FirstOrDefault(s => s.Id == id);
                if (entity == null)
                    return null;
                return Mapper.Map<AssessmentFactorDto>(entity);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void AddAssessmentFactor(AssessmentFactorDto dto)
        {
            try
            {
                var entity = Mapper.Map<T_AssessmentFactor>(dto);
                if (CommonRep.GetDbSet<T_AssessmentFactor>().Count() > 0)
                {
                    var max = CommonRep.GetDbSet<T_AssessmentFactor>().Max(s => s.Id);
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

        public void UpdateAssessmentFactor(AssessmentFactorDto dto)
        {
            try
            {
                T_AssessmentFactor entity = CommonRep.FindBy<T_AssessmentFactor>(dto.Id);
                entity.DealId = dto.DealId;
                entity.FactorName = dto.FactorName;
                entity.Algorithm = dto.Algorithm;
                entity.Weight = entity.Weight;
                entity.Description = dto.Description;
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

        public void DeleteAssessmentFactor(int id)
        {
            try
            {
                T_AssessmentFactor entity = CommonRep.FindBy<T_AssessmentFactor>(id);
                entity.IsDisabled = true;
                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void DeleteAssessmentFactor(AssessmentFactorDto dto)
        {
            if (dto == null) return;
            DeleteAssessmentFactor(dto.Id);
        }
    }
}
