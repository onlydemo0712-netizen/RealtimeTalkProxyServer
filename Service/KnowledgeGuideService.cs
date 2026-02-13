using AetherCore.Service;
using AetherCore.Utility.Attributes;
using AetherCore.Utility.Blob;
using AutoMapper;
using Common.DTO.KnowledgeGuide;
using Common.Setting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver.Core.Configuration;
using Repository.Interface;
using Service.Interface;
using System.Reflection.Metadata;

namespace Service
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class KnowledgeGuideService : GenericService<KnowledgeGuideEntity, KnowledgeGuideRequest, KnowledgeGuideResponse, IKnowledgeGuideRepository>, IKnowledgeGuideService
    {
        private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".webp", ".gif"
        };

        private const long MaxBytes = 5L * 1024 * 1024;

        private readonly BlobSettings _blobSettings;

        public KnowledgeGuideService(IKnowledgeGuideRepository repo, IMapper mapper, IOptions<BlobSettings> opt)
            : base(repo, mapper)
        {
            _blobSettings = opt.Value;
        }

        public async Task<ImgUploadResponse> ImgUpload(ImgUploadRequest request)
        {
            ValidateRequest(request);

            var blobPath        = BlobHelper.BuildBlobPath(request.FileName, DateTime.UtcNow);
            var sasUri          = BlobHelper.GenerateUploadSasUri(_blobSettings.ConnectionString, 
                                                                  _blobSettings.ContainerName, 
                                                                  blobPath, 
                                                                  TimeSpan.FromMinutes(5));
            string uploadUrl    = sasUri.ToString();

            return new ImgUploadResponse
            {
                UploadUrl   = uploadUrl,                        // 含 SAS
                BlobUrl     = BlobHelper.ToBlobUrl(uploadUrl),  // 不含SAS
                ExpiresAt   = BlobHelper.GetExpiryFromSas(sasUri)   
            };
        }

        // -----------------------
        // 私有方法
        // -----------------------

        private static void ValidateRequest(ImgUploadRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (request.FileSize <= 0 || request.FileSize > MaxBytes)
                throw new InvalidOperationException("File too large");

            if (string.IsNullOrWhiteSpace(request.ContentType) ||
                !request.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Invalid content type");

            var ext = Path.GetExtension(request.FileName ?? "");
            if (string.IsNullOrWhiteSpace(ext) || !AllowedExt.Contains(ext))
                throw new InvalidOperationException("Invalid file extension");
        }
    }
}
