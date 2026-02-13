using Common.DTO.AdminAuth;

using AetherCore.Repository;

namespace Repository.Interface
{
    public interface IAdminAuthRepository : IRepository<AdminAuthEntity>
    {
        Task<bool> ChangePassword(string key, string pwHash);
    }
}
