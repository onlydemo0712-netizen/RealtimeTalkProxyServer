namespace Common.DTO.KnowledgeGuide
{
    public class KnowledgeGuideRequest
    {
        public int InfoType { get; set; }       // health or sport
        public int BannerType { get; set; }     // Banner or Image or Text or URL
        public string Title { get; set; }       = string.Empty;
        public string Content { get; set; }     = string.Empty;
    }
}
