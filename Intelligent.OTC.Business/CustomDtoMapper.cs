using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;

namespace Intelligent.OTC.Business
{
    public static class CustomDtoMapper
    {
        internal static void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<T_Deal, DealDto>().ReverseMap().ForMember(source => source.Id, options => options.Ignore());
            configuration.CreateMap<T_CustomerScore, CustomerScoreDto>().ReverseMap()
                .ForMember(source => source.Id, options => options.Ignore());
            configuration.CreateMap<T_CustomerAssessment, CustomerAssessmentDto>().ReverseMap()
                .ForMember(source => source.T_AssessmentType, options => options.Ignore())
                .ForMember(source => source.T_Deal, options => options.Ignore())
                .ForMember(source => source.Id, options => options.Ignore());
            configuration.CreateMap<T_CommunicationMethod, CommunicationMethodDto>().ReverseMap().ForMember(source => source.Id, options => options.Ignore());
            configuration.CreateMap<T_CollectionStrategy, CollectionStrategyDto>().ReverseMap().ForMember(source => source.Id, options => options.Ignore());
            configuration.CreateMap<T_AssessmentType, AssessmentTypeDto>().ReverseMap().ForMember(source => source.Id, options => options.Ignore());
            configuration.CreateMap<T_AssessmentStandards, AssessmentStandardsDto>().ReverseMap().ForMember(source => source.Id, options => options.Ignore());
            configuration.CreateMap<T_AssessmentFactor, AssessmentFactorDto>().ReverseMap().ForMember(source => source.Id, options => options.Ignore());
            configuration.CreateMap<T_CustomerAssessment_History, CustomerAssessmentHistoryDto>().ReverseMap().ForMember(source => source.Id, options => options.Ignore());
            configuration.CreateMap<CustomerAssessmentHistoryDto, CustomerAssessmentDto>()
                .ForMember(s => s.SiteUseId, option => option.MapFrom("SiteUseId"))
                .ForMember(s=>s.DealId,option=>option.Ignore())
                .ForMember(s => s.LegalEntity, option => option.Ignore())
                .ForMember(source => source.Id, option => option.Ignore());
            configuration.CreateMap<T_CustomerAssessment_Log, CustomerAssessmentLogDto>().ReverseMap()
                .ForMember(s => s.T_Deal, option => option.Ignore())
                .ForMember(s => s.T_CustomerAssessment_History, option => option.Ignore())
                .ForMember(s => s.Id, option => option.Ignore());
            configuration.CreateMap<CollectingReportDto, ReportItem>();
            configuration.CreateMap<V_CollectionReport, ReportItem>();
            configuration.CreateMap<V_CustomerAssessment, CustomerAssessmentItem>();
            configuration.CreateMap<V_CustomerAssessmentHistory, CustomerAssessmentItem>();
        }
        public static void Configure()
        {
            Mapper.Initialize(CreateMappings);
        }
    }
}
