namespace Common.DTO.KnowledgeGuide
{
    public class ImgUploadResponse
    {
        public string UploadUrl { get; set; }   = string.Empty; // 含 SAS
        public string BlobUrl { get; set; }     = string.Empty; // 不含 SAS
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
