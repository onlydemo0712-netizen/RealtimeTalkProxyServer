using AutoMapper;
using Common.DTO.KnowledgeGuide;

namespace Common.DTO.KnowledgeGuide
{
    public class KnowledgeGuideProfile : Profile
    {
        public KnowledgeGuideProfile()
        {
            CreateMap<KnowledgeGuideRequest, KnowledgeGuideEntity>();
            CreateMap<KnowledgeGuideEntity, KnowledgeGuideResponse>();
            CreateMap<KnowledgeGuideEntity, KnowledgeGuideDocument>();
            CreateMap<KnowledgeGuideDocument, KnowledgeGuideEntity>();
        }
    }

}
