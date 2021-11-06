using AutoMapper;
using SPGovernmentData.Application.Entities.DTOs;
using SPGovernmentData.Domain.Entities;

namespace SPGovernmentData.Application.Entities.Mappings
{
    public class DomainMappingProfile : Profile
    {
        public DomainMappingProfile()
        {
            CreateMap<DatasetAditionalInformation, DatasetAditionalInformationDTO>().ReverseMap();
            CreateMap<Dataset, DatasetDTO>().ReverseMap();
            CreateMap<DataSourceAditionalInformation, DataSourceAditionalInformationDTO>().ReverseMap();
            CreateMap<DataSource, DataSourceDTO>().ReverseMap();
            CreateMap<Tag, TagDTO>().ReverseMap();
            CreateMap<UrlExtracted, UrlExtractedDTO>().ReverseMap();
        }
    }
}