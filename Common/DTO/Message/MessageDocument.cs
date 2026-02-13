using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using AetherCore.Entities;

namespace Common.DTO.Message
{
    // 小 DTO：避免整包 message 寫回去（只更新必要欄位）
    public sealed record WarningScoreUpdate(string MessageId, int WarningScore, string MsgContent, string MsgTime);

    public class MessagePair
    {
        public string UserMessage { get; set; }                         // 使用者訊息
        public string AIMessage { get; set; }                           // AI訊息
        public int? WarningScore { get; set; } = -1;                    // 警示分數
        public DateTime CreatedAt { get; set; }                         // 建立時間
    }

    public class MessageDocument : IEntity, IDBEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }              = string.Empty;
        public string ConversationId { get; set; }                      // 所屬對話ID
        public MessagePair Pair { get; set; }                           // 使用者與AI訊息
        public DateTime CreatedAt { get; set; }                         // 建立時間
        public DateTime UpdatedAt { get; set; }                         // 更新時間

        // 實作 IEntity
        public object GenerateNewID()   => ObjectId.GenerateNewId().ToString()!;
        public bool HasDefaultID()      => string.IsNullOrEmpty(Id);
    }
}
