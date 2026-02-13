namespace Common.DTO.KnowledgeGuide
{
    public class ImgUploadRequest
    {
        /// <summary>
        /// 原始檔名（用來取副檔名，不一定會真的用）
        /// </summary>
        public string FileName { get; set; }    = string.Empty;

        /// <summary>
        /// MIME Type，例如 image/png、image/jpeg
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// 檔案大小（byte），後端可做限制
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 上傳用途（決定 blob path）
        /// 例如：ProfileImage / KnowledgeGuideCover / Banner
        /// </summary>
        public string Purpose { get; set; }     = string.Empty;
    }
}
