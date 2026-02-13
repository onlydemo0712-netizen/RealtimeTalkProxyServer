using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using AetherCore.Entities;

namespace Common.DTO.ChatRole
{
    public class ChatRoleDocument : IEntity, IDBEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }                  = string.Empty;
        public DateTime CreatedAt { get; set; }                                     // 建立時間
        public DateTime UpdatedAt { get; set; }                                     // 更新時間
        public string RoleId { get; set; }              = string.Empty;
        public string ModelId { get; set; }             = string.Empty;
        public string RoleName { get; set; }            = string.Empty;
        public string Description { get; set; }         = string.Empty;
        public bool IsMale { get; set; }
        public bool SpeakFirst { get; set; }
        public string RoleInstructions { get; set; }    = string.Empty;
        public string Voice { get; set; }               = string.Empty; // 例如 "alloy"、"verse"...
        public string Motion { get; set; }              = string.Empty;

        // 實作 IEntity
        public object GenerateNewID()   => ObjectId.GenerateNewId().ToString()!;
        public bool HasDefaultID()      => string.IsNullOrEmpty(Id);
    }
}
