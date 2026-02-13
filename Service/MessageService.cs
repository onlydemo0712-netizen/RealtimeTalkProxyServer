using AetherCore.Service;
using AetherCore.Utility.Attributes;
using AutoMapper;
using Common.DTO.Message;
using Microsoft.Extensions.DependencyInjection;
using Repository.Interface;
using Service.Interface;

namespace Service
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class MessageService : GenericService<MessageEntity, MessageRequest, MessageResponse, IMessageRepository>, IMessageService
    {
        public MessageService(IMessageRepository repo, IMapper mapper) 
            : base(repo, mapper)
        {
        }
    }
}
