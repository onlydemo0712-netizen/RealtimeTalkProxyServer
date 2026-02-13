using AetherCore.DataAccess;
using AetherCore.Utility.Attributes;
using AutoMapper;
using Common.DTO.Conversation;
using DataAccess.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class ConversationDataAccess : MongoEntityDataAccess<ConversationEntity, ConversationDocument>, IConversationDataAccess
    {
        public ConversationDataAccess(IMapper mapper)
            : base(mapper)
        {
        }
    }
}
