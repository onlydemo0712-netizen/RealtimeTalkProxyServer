using Common.DTO.Message;

using AetherCore.Repository;

namespace Repository.Interface
{
    public interface IMessageRepository : IRepository<MessageEntity>
    {
        Task<List<MessagePair>> GetDetail(string conversationId);
        Task AddMessagePairsAsync(string conversatonId, List<MessagePair> messagePairs);
        Task<List<MessageEntity>> FindUnscoredAsync(int limit);
        /// <summary>
        /// 批次更新 Pair.WarningScore（建議 BulkWrite）
        /// 回傳實際更新到的筆數（ModifiedCount）
        /// </summary>
        Task<int> UpdateWarningScoresAsync(List<WarningScoreUpdate> updates);
    }
}
