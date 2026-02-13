using AetherCore.Entities;

namespace Common.DTO.Jobs
{
    public class JobsEntity : IDBEntity
    {        
        public string Id { get; set; }              = string.Empty;
        public DateTime CreatedAt { get; set; }             // 建立時間
        public DateTime UpdatedAt { get; set; }             // 更新時間

        public JobsEntity() 
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
