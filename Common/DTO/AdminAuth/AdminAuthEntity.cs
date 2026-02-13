using AetherCore.Entities;

namespace Common.DTO.AdminAuth
{
    public class AdminAuthEntity : IDBEntity
    {        
        public string Id { get; set; }              = string.Empty;
        public DateTime CreatedAt { get; set; }             // 建立時間
        public DateTime UpdatedAt { get; set; }             // 更新時間
        public string Account { get; set; }
        public string PasswordHash { get; set; }

        public AdminAuthEntity() 
        {
            CreatedAt       = DateTime.UtcNow;
            UpdatedAt       = DateTime.UtcNow;
            Account         = string.Empty;
            PasswordHash    = string.Empty;
        }
    }
}
