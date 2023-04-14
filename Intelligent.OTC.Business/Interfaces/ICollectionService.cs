using AutoMapper;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Interfaces
{
    interface ICollectionService
    {
        ICacheService CacheSvr { set; }
        OTCRepository CommonRep { set; }

        #region Get All AssessmentType
        IQueryable<T_AssessmentType> GetAllAssessmentType();
        #endregion

        #region Deal
        List<DealDto> GetAllDealList();
        DealDto GetDealById(int id);
        void AddDeal(DealDto dto);
        void UpdateDeal(DealDto dto);
        void DeleteDeal(int id);
        void DeleteDeal(DealDto dto);
        #endregion

        #region CustomerScore
        List<CustomerScoreDto> GetAllCustomerScoreList();
        CustomerScoreDto GetCustomerScoreById(int id);
        void AddCustomerScore(CustomerScoreDto dto);
        void UpdateCustomerScore(CustomerScoreDto dto);
        void DeleteCustomerScore(int id);
        void DeleteCustomerScore(CustomerScoreDto dto);
        #endregion

        #region  AssessmentFactor
        List<AssessmentFactorDto> GetAllAssessmentFactorList();
        AssessmentFactorDto GetAssessmentFactorById(int id);
        void AddAssessmentFactor(AssessmentFactorDto dto);
        void UpdateAssessmentFactor(AssessmentFactorDto dto);
        void DeleteAssessmentFactor(int id);
        void DeleteAssessmentFactor(AssessmentFactorDto dto);
        #endregion

        #region  AssessmentType
        AssessmentTypeDto GetAssessmentTypeById(int id);
        void AddAssessmentType(AssessmentTypeDto dto);
        void UpdateAssessmentType(AssessmentTypeDto dto);
        void DeleteAssessmentType(int id);
        void DeleteAssessmentType(AssessmentTypeDto dto);
        #endregion

        #region CustomerAssessment
        List<CustomerAssessmentDto> GetAllCustomerAssessmentList();
        CustomerAssessmentDto GetCustomerAssessmentById(int id);
        void AddCustomerAssessment(CustomerAssessmentDto dto);
        void UpdateCustomerAssessment(CustomerAssessmentDto dto);
        void DeleteCustomerAssessment(int id);
        void DeleteCustomerAssessment(CustomerAssessmentDto dto);
        #endregion

        #region AssessmentStandards
        List<AssessmentStandardsDto> GetAllAssessmentStandardsList();
        AssessmentStandardsDto GetAssessmentStandardsById(int id);
        void AddAssessmentStandards(AssessmentStandardsDto dto);
        void UpdateAssessmentStandards(AssessmentStandardsDto dto);
        void DeleteAssessmentStandards(int id);
        void DeleteAssessmentStandards(AssessmentStandardsDto dto);
        #endregion

        #region CollectionStrategy
        List<CollectionStrategyDto> GetAllCollectionStrategyList();
        CollectionStrategyDto GetCollectionStrategyById(int id);
        void AddCollectionStrategy(CollectionStrategyDto dto);
        void UpdateCollectionStrategy(CollectionStrategyDto dto);
        void DeleteCollectionStrategy(int id);
        void DeleteCollectionStrategy(CollectionStrategyDto dto);
        #endregion

        #region CommunicationMethod
        List<CommunicationMethodDto> GetCommunicationMethodList();
        CommunicationMethodDto GetCommunicationMethodById(int id);
        void AddCommunicationMethod(CommunicationMethodDto dto);
        void UpdateCommunicationMethod(CommunicationMethodDto dto);
        void DeleteCommunicationMethod(int id);
        void DeleteCommunicationMethod(CommunicationMethodDto dto);
        #endregion

        #region CustomerAssessmentHistory
        List<CustomerAssessmentHistoryDto> GetCustomerAssessmentHistoryList(int version);
        #endregion

        #region CustomerAssessmentLog
        List<CustomerAssessmentLogDto> GetAllCustomerAssessmentLogList();
        int AddCustomerAssessmentLog(CustomerAssessmentLogDto dto);
        void UpdateCustomerAssessmentLog(CustomerAssessmentLogDto dto);
        #endregion

        #region Business
        void ProcessDealCollection(string deal,string legal=null);
        DashBoardModel GetDashboardReport(string collector,string mail);
        #endregion
    }
}
