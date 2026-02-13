using AetherCore.DataAccess;
using AetherCore.Utility.Attributes;
using AutoMapper;
using Common.DTO.KnowledgeGuide;
using DataAccess.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class KnowledgeGuideDataAccess : MongoEntityDataAccess<KnowledgeGuideEntity, KnowledgeGuideDocument>, IKnowledgeGuideDataAccess
    {
        public KnowledgeGuideDataAccess(IMapper mapper)
            : base(mapper)
        {
        }
    }
}
