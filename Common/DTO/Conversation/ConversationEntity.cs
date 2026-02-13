using AetherCore.Entities;

namespace Common.DTO.Conversation
{
    public class ConversationEntity : IDBEntity
    {        
        public string Id { get; set; }                  = string.Empty;
        public string UserName { get; set; }            = string.Empty;               // 談話使用者 Name
        public string RoleId { get; set; }              = string.Empty;
        public RoleInfoSnapShot RoleInfo { get; set; }
        public DateTime CreatedAt { get; set; }             // 建立時間
        public DateTime UpdatedAt { get; set; }             // 更新時間

        public ConversationEntity() 
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
