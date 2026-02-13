using Microsoft.AspNetCore.Mvc;
using Common.DTO.Auth;

using AetherCore.Service;

namespace Service.Interface
{
    public interface IAuthService : IService<IdentityRequest, LoginResponse>
    {
        Task<LoginResponse> Login(IdentityRequest request);
        Task<bool> CreateIdentity(IdentityRequest request);
        Task<bool> ChangePassword(ChangePasswordRequest request);
    }
}
