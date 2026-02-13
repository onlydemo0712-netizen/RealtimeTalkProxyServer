using AetherCore.Exceptions;
using AetherCore.Service;
using AetherCore.Utility;
using AetherCore.Utility.Attributes;
using AetherCore.Utility.JWT;
using AutoMapper;
using Common.DTO.AdminAuth;
using Common.DTO.Auth;
using Microsoft.Extensions.DependencyInjection;
using Repository.Interface;
using Service.Interface;

namespace Service
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class AdminAuthService : GenericService<AdminAuthEntity, IdentityRequest, LoginResponse, IAdminAuthRepository>, IAdminAuthService
    {
        static private readonly string _salt = "ShowMeTheMoney";

        private ITokenService _tokenService;

        public AdminAuthService(IAdminAuthRepository repo, IMapper mapper, ITokenServiceFactory tokenServiceFactory)
            : base(repo, mapper)
        {
            this._tokenService  = tokenServiceFactory.Create("AdminJwt"); ;
        }

        public async Task<LoginResponse> Login(IdentityRequest request)
        {
            try
            {
                var entity = await _repository.GetAsync(request.Account);

                // 對外統一：避免帳號枚舉
                if (entity == null)
                    throw new InvalidCredentialsException();

                var inputHash = Utils.ComputeSha256Hash(request.Password, _salt);

                if (!string.Equals(entity.PasswordHash, inputHash, StringComparison.Ordinal))
                    throw new InvalidCredentialsException();

                var token = _tokenService.GenerateToken(entity.Id, entity.Account);

                return new LoginResponse
                {
                    Success = true,
                    Token   = token
                };
            }
            catch (InvalidCredentialsException)
            {
                throw; // 讓上層統一轉 401
            }
            catch (Exception ex)
            {
                // DB / infra 類錯誤統一轉
                throw new ServiceUnavailableException(ex.Message);
            }
        }

        public async Task<bool> CreateIdentity(IdentityRequest request)
        {
            try
            {
                // 1. 檢查輸入參數
                if (string.IsNullOrWhiteSpace(request.Account) ||
                string.IsNullOrWhiteSpace(request.Password))
                {
                    throw new InvalidCredentialsException();
                }
                // 2. 建立新使用者
                var newEntity = new AdminAuthEntity
                {
                    Account         = request.Account,
                    PasswordHash    = Utils.ComputeSha256Hash(request.Password, _salt),
                    CreatedAt       = DateTime.UtcNow,
                    UpdatedAt       = DateTime.UtcNow
                };
                var inserted    = await _repository.InsertAsync(newEntity);
                bool bIsSuccess = inserted != null;


                return bIsSuccess;
            }
            catch (InvalidCredentialsException)
            {
                throw; // 讓上層統一轉 401
            }
            catch (InvalidEntityException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // DB / infra 類錯誤統一轉
                throw new ServiceUnavailableException(ex.Message);
            }
        }

        public async Task<bool> ChangePassword(ChangePasswordRequest request)
        {
            try
            {
                // 1. 檢查輸入參數
                if (string.IsNullOrWhiteSpace(request.Account) ||
                string.IsNullOrWhiteSpace(request.OldPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    throw new InvalidCredentialsException();
                }

                // 2. 取得使用者資料
                var staffData = await _repository.GetAsync(request.Account);

                if (staffData == null)
                {
                    throw new InvalidCredentialsException();
                }

                // 3. 驗證舊密碼
                if (staffData.PasswordHash != Utils.ComputeSha256Hash(request.OldPassword, _salt))
                {
                    throw new InvalidCredentialsException();
                }

                // 4. 更新新密碼
                string newPwHash = Utils.ComputeSha256Hash(request.NewPassword, _salt);

                return await _repository.ChangePassword(staffData.Account, newPwHash);
            }
            catch (InvalidCredentialsException)
            {
                throw; // 讓上層統一轉 401
            }
            catch (Exception ex)
            {
                // DB / infra 類錯誤統一轉
                throw new ServiceUnavailableException(ex.Message);
            }
        }
    }
}
