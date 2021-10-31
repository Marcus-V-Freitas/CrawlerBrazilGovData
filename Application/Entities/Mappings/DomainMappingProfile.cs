using Application.Entities.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Entities.Mappings
{
    public class DomainMappingProfile : Profile
    {
        public DomainMappingProfile()
        {
            CreateMap<DatasetAditionalInformation, DatasetAditionalInformationDTO>().ReverseMap();
            CreateMap<Dataset, DatasetDTO>().ReverseMap();
            CreateMap<DataSourceAditionalInformationDTO, DataSourceAditionalInformationDTO>().ReverseMap();
            CreateMap<DataSource, DataSourceDTO>().ReverseMap();
            CreateMap<Tag, TagDTO>().ReverseMap();
            CreateMap<UrlExtracted, UrlExtractedDTO>().ReverseMap();
        }
    }
}