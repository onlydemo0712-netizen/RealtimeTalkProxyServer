using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using AetherCore.Entities;

namespace Common.DTO.KnowledgeGuide
{
    public enum KnowledgeGuideInfoType
    {
        SportInfo       = 0,
        NutritionInfo,
        MedicationInfo,
        OtherInfo
    }

    public enum KnowledgeGuideBannerType
    {
        Image   = 1,
        Text    = 2,
        URL     = 3
    }

    public class KnowledgeGuideDocument : IEntity, IDBEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }          = string.Empty;
        public DateTime CreatedAt { get; set; }                     // 建立時間
        public DateTime UpdatedAt { get; set; }                     // 更新時間
        public KnowledgeGuideInfoType InfoType { get; set; }        // health or sport
        public KnowledgeGuideBannerType BannerType { get; set; }    // Image or Text or URL
        public string Title { get; set; }       = string.Empty;
        public string Content{ get; set; }      = string.Empty;

        // 實作 IEntity
        public object GenerateNewID()   => ObjectId.GenerateNewId().ToString()!;
        public bool HasDefaultID()      => string.IsNullOrEmpty(Id);
    }
}
