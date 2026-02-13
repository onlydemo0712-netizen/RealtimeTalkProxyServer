using AutoMapper;
using Common.DTO.Conversation;

namespace Common.DTO.Conversation
{
    public class ConversationProfile : Profile
    {
        public ConversationProfile()
        {
            CreateMap<ConversationRequest, ConversationEntity>();
            CreateMap<ConversationEntity, ConversationResponse>();
            CreateMap<ConversationEntity, ConversationDocument>();
            CreateMap<ConversationDocument, ConversationEntity>();
        }
    }

}
