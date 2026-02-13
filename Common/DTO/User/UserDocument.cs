using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using AetherCore.Entities;

namespace Common.DTO.User
{
    public class UserDocument : IEntity, IDBEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }              = "";
        public DateTime CreatedAt { get; set; }                                     // 建立時間
        public DateTime UpdatedAt { get; set; }                                     // 更新時間

        public string UserId { get; set; }
        public int DailyLimit { get; set; }
        /// <summary>
        /// Key   : 日期 (UTC, yyyy-MM-dd)
        /// Value : 當天已使用句數
        /// </summary>
        public Dictionary<string, int> DailySentenceCounts { get; set; } = new();

        /// <summary>
        /// 行為追蹤（Feature -> 最後觸發時間 UTC）
        /// </summary>
        public Dictionary<string, DateTime> TrackItems { get; set; }

        // 實作 IEntity
        public object GenerateNewID()   => ObjectId.GenerateNewId().ToString()!;
        public bool HasDefaultID()      => string.IsNullOrEmpty(Id);
    }
}
