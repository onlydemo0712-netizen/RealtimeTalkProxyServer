using AetherCore.Service;
using Common.DTO.Message;

namespace Service.Interface
{
    public interface IMessageService : IService<MessageRequest, MessageResponse>
    {
    }
}
