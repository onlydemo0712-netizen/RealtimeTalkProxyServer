using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using AetherCore.Entities;

namespace Common.DTO.Conversation
{
    public class RoleInfoSnapShot
    {         
        public string RoleName { get; set; }            = string.Empty;
        public bool IsMale { get; set; }
        public string Description { get; set; }         = string.Empty;                
        public string RoleInstructions { get; set; }    = string.Empty;
        public string Language { get; set; }            = string.Empty;
        public string Locale { get; set; }              = string.Empty;
        public int MaxLength { get; set; }              = 0;
        public bool SpeakFirst { get; set; }
    }

    public class ConversationDocument : IEntity, IDBEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }                  = string.Empty;
        public string UserName { get; set; }            = string.Empty;               // 談話使用者 Name
        public string RoleId { get; set; }              = string.Empty;
        public RoleInfoSnapShot RoleInfo { get; set; }
        public DateTime CreatedAt { get; set; }                                     // 建立時間
        public DateTime UpdatedAt { get; set; }                                     // 更新時間

        // 實作 IEntity
        public object GenerateNewID()   => ObjectId.GenerateNewId().ToString()!;
        public bool HasDefaultID()      => string.IsNullOrEmpty(Id);
    }
}
