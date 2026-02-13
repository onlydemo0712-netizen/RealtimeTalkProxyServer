using Common.DTO.Auth;
using Common.DTO.User;
using Microsoft.AspNetCore.Mvc;
using AetherCore.Service;

namespace Service.Interface
{
    public interface IUserService : IService<UserRequest, UserResponse>
    {
        Task<bool> CreateIdentity(IdentityRequest request);
        Task<bool> TrackInfo(string userId, ActivityTrackRequest request);
        Task<bool> QuotaLimitChange(QuotaLimitChangeRequest request);
    }
}
