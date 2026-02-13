using Common.DTO.Message;
using AetherCore.DataAccess;

namespace DataAccess.Interface
{
    public interface IMessageDataAccess : IDataAccess<MessageEntity>
    {
        Task<List<MessageEntity>> GetDetail(string conversationId);
        Task AddMessagePairsAsync(List<MessageEntity> messagePairs);
        Task<List<MessageEntity>> FindUnscoredAsync(int limit);
        Task<int> UpdateWarningScoresAsync(List<WarningScoreUpdate> updates);
    }
}
