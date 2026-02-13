using Common.DTO.Conversation;
using Common.DTO.Message;
using AetherCore.Service;

namespace Service.Interface
{
    public interface IConversationService : IService<ConversationRequest, ConversationResponse>
    {
        Task<GetDetailResponse> GetDetail(GetDetailRequest request);

        Task CreateConversationInfo(string userId, string roleId, RoleInfoSnapShot roleInfo, List<MessagePair> messagePairs);
    }
}
