using AetherCore.Entities;

namespace Common.DTO.User
{
    public class UserEntity : IDBEntity
    {        
        public string Id { get; set; }              = "";
        public DateTime CreatedAt { get; set; }             // 建立時間
        public DateTime UpdatedAt { get; set; }             // 更新時間
        public string UserId { get; set; }
        public int DailyLimit { get; set; }
        public Dictionary<string, int> DailySentenceCounts { get; set; }
        public Dictionary<string, DateTime> TrackItems { get; set; }

        public UserEntity() 
        {
            CreatedAt           = DateTime.UtcNow;
            UpdatedAt           = DateTime.UtcNow;
            UserId              = string.Empty;
            DailySentenceCounts = new ();
            TrackItems          = new ();
        }
    }
}
