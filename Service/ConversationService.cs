using AetherCore.Service;
using AetherCore.Utility.Attributes;
using AutoMapper;
using Common.DTO.Conversation;
using Common.DTO.Message;
using Microsoft.Extensions.DependencyInjection;
using Repository.Interface;
using Service.Interface;

namespace Service
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class ConversationService : GenericService<ConversationEntity, ConversationRequest, ConversationResponse, IConversationRepository>, IConversationService
    {
        private readonly IMessageRepository _msgRepo;

        public ConversationService(IConversationRepository repo, IMessageRepository msgRepo, IMapper mapper) 
            : base(repo, mapper)
        {
            _msgRepo = msgRepo;
        }

        public async Task<GetDetailResponse> GetDetail(GetDetailRequest request)
        {
            List<MessagePair> pairList = await _msgRepo.GetDetail(request.ConversationId);

            GetDetailResponse response = new GetDetailResponse
            {
                ConversationId  = request.ConversationId,
                PairList        = pairList
            };

            return response;
        }

        public async Task CreateConversationInfo(string userId, string roleId, RoleInfoSnapShot roleInfo, List<MessagePair> messagePairs)
        {
            ConversationEntity entity = new ConversationEntity
            {
                UserName    = userId,
                RoleId      = roleId,
                RoleInfo    = roleInfo,
                CreatedAt   = DateTime.UtcNow
            };

            var newEntity = await _repository.InsertAsync(entity);            
            await _msgRepo.AddMessagePairsAsync(newEntity.Id, messagePairs);
        }
    }
}
