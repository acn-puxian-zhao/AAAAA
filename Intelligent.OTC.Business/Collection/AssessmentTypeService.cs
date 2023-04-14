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
        public IQueryable<T_AssessmentType> GetAllAssessmentType()
        {
            return CommonRep.GetQueryable<T_AssessmentType>();
        }

        public AssessmentTypeDto GetAssessmentTypeById(int id)
        {
            try
            {
                var entity = CommonRep.GetQueryable<T_AssessmentType>().FirstOrDefault(s => s.Id == id);
                if (entity == null)
                    return null;
                return Mapper.Map<AssessmentTypeDto>(entity);
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void AddAssessmentType(AssessmentTypeDto dto)
        {
            try
            {
                var entity = Mapper.Map<T_AssessmentType>(dto);
                if (CommonRep.GetDbSet<T_AssessmentType>().Count() > 0)
                {
                    var max = CommonRep.GetDbSet<T_AssessmentType>().Max(s => s.Id);
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

        public void UpdateAssessmentType(AssessmentTypeDto dto)
        {
            try
            {
                T_AssessmentType entity = CommonRep.FindBy<T_AssessmentType>(dto.Id);
                entity.DealId = dto.DealId;
                entity.Name = dto.Name;
                entity.Description = dto.Description;
                entity.CollectionStrategyId = dto.CollectionStrategyId;
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

        public void DeleteAssessmentType(int id)
        {
            try
            {
                T_AssessmentType entity = CommonRep.FindBy<T_AssessmentType>(id);
                entity.IsDeleted = true;
                entity.LastModificationTime = DateTime.Now;
                entity.LastModifierUserId = CurrentUser.Id;
                entity.DeletionTime = DateTime.Now;
                entity.DeleterUserId = CurrentUser.Id;
                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void DeleteAssessmentType(AssessmentTypeDto dto)
        {
            if (dto == null) return;
            DeleteAssessmentType(dto.Id);
        }
    }
}
