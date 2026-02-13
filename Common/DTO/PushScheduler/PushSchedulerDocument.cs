using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using AetherCore.Entities;

namespace Common.DTO.PushScheduler
{
    public enum PushStatus
    {
        Pending,
        Sending,
        Sent,
        Failed,
        Cancelled
    }

    public class PushSchedulerDocument : IEntity, IDBEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }              = string.Empty;
        public DateTime CreatedAt { get; set; }                                     // 建立時間
        public DateTime UpdatedAt { get; set; }                                     // 更新時間

        public string Title { get; set; }           = string.Empty;
        public string Message { get; set; }         = string.Empty;
        public DateTime SendTime { get; set; }
        //public Dictionary<string, object>? Data { get; set; }
        public PushStatus Status { get; set; }      = PushStatus.Pending;
        public string Creater { get; set; }         = string.Empty;
        public bool IsRecurring { get; set; }       = true;

        // 實作 IEntity
        public object GenerateNewID()   => ObjectId.GenerateNewId().ToString()!;
        public bool HasDefaultID()      => string.IsNullOrEmpty(Id);
    }
}
