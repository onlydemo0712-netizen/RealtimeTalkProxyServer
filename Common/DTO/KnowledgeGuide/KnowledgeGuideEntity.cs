using AetherCore.Entities;

namespace Common.DTO.KnowledgeGuide
{
    public class KnowledgeGuideEntity : IDBEntity
    {        
        public string Id { get; set; }              = string.Empty;
        public DateTime CreatedAt { get; set; }                     // 建立時間
        public DateTime UpdatedAt { get; set; }                     // 更新時間
        public KnowledgeGuideInfoType InfoType { get; set; }        // health or sport
        public KnowledgeGuideBannerType BannerType { get; set; }    // Image or Text or URL
        public string Title { get; set; }           = string.Empty;
        public string Content { get; set; }         = string.Empty;
        public KnowledgeGuideEntity() 
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
