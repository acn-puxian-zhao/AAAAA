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
        public List<AssessmentStandardsDto> GetAllAssessmentStandardsList()
        {
            try
            {
                var list = CacheSvr.GetOrSet<List<T_AssessmentStandards>>("Cache_AssessmentStandardsList", () =>
                {
                    return CommonRep.GetQueryable<T_AssessmentStandards>().ToList();
                });
                return Mapper.Map<List<AssessmentStandardsDto>>(list);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public AssessmentStandardsDto GetAssessmentStandardsById(int id)
        {
            try
            {
                var entity = CommonRep.GetQueryable<T_AssessmentStandards>().FirstOrDefault(s => s.Id == id);
                if (entity == null)
                    return null;
                return Mapper.Map<AssessmentStandardsDto>(entity);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void AddAssessmentStandards(AssessmentStandardsDto dto)
        {
            try
            {
                var entity = Mapper.Map<T_AssessmentStandards>(dto);
                if (CommonRep.GetDbSet<T_AssessmentStandards>().Count() > 0)
                {
                    var max = CommonRep.GetDbSet<T_AssessmentStandards>().Max(s => s.Id);
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

        public void UpdateAssessmentStandards(AssessmentStandardsDto dto)
        {
            try
            {
                T_AssessmentStandards entity = CommonRep.FindBy<T_AssessmentStandards>(dto.Id);
                entity.DealId = dto.DealId;
                entity.Name = dto.Name;
                entity.Excellent = dto.Excellent;
                entity.Good = dto.Good;
                entity.Issue = dto.Issue;
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

        public void DeleteAssessmentStandards(int id)
        {
            try
            {
                T_AssessmentStandards entity = CommonRep.FindBy<T_AssessmentStandards>(id);
                CommonRep.Remove(entity);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void DeleteAssessmentStandards(AssessmentStandardsDto dto)
        {
            if (dto == null) return;
            DeleteAssessmentStandards(dto.Id);
        }
    }
}
