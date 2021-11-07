using AutoMapper;
using SPNewsData.Application.Entities.DTOs;
using SPNewsData.Domain.Entities;

namespace SPNewsData.Application.Entities.Mappings
{
    public class DomainMappingProfile : Profile
    {
        public DomainMappingProfile()
        {
            CreateMap<UrlExtracted, UrlExtractedDTO>().ReverseMap();
            CreateMap<Subject, SubjectDTO>().ReverseMap();
            CreateMap<GovNews, GovNewsDTO>().ReverseMap();
        }
    }
}