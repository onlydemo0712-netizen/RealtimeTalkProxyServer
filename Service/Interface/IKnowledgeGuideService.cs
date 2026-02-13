using AetherCore.Service;
using Common.DTO.KnowledgeGuide;

namespace Service.Interface
{
    public interface IKnowledgeGuideService : IService<KnowledgeGuideRequest, KnowledgeGuideResponse>
    {
        Task<ImgUploadResponse> ImgUpload(ImgUploadRequest request);
    }
}
