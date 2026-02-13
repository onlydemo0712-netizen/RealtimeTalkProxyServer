using AetherCore.Entities;

namespace Common.DTO.Message
{
    public class MessageEntity : IDBEntity
    {        
        public string Id { get; set; }              = string.Empty;
        public string ConversationId { get; set; }          // 所屬對話ID
        public MessagePair Pair { get; set; }               // 使用者與AI訊息
        public DateTime CreatedAt { get; set; }             // 建立時間
        public DateTime UpdatedAt { get; set; }             // 更新時間

        public MessageEntity() 
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
