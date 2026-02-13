using AutoMapper;
using Common.DTO.Message;

namespace Common.DTO.Message
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            CreateMap<MessageRequest, MessageEntity>();
            CreateMap<MessageEntity, MessageResponse>();
            CreateMap<MessageEntity, MessageDocument>();
            CreateMap<MessageDocument, MessageEntity>();
        }
    }

}
