using AetherCore.Exceptions;
using AetherCore.Service;
using AetherCore.Utility.Attributes;
using AutoMapper;
using Common.DTO.Auth;
using Common.DTO.User;
using Common.Setting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Repository.Interface;
using Service.Interface;

namespace Service
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class UserService : GenericService<UserEntity, UserRequest, UserResponse, IUserRepository>, IUserService
    {
        private readonly IAuthRepository _authRepository;
        private readonly QuotaSettings _quota;

        public UserService(IUserRepository repo, IAuthRepository authRepository, IMapper mapper, IOptions<QuotaSettings> quotaSetting) 
            : base(repo, mapper)
        {
            _authRepository = authRepository;
            _quota          = quotaSetting.Value;
        }

        public async Task<bool> CreateIdentity(IdentityRequest request)
        {
            try
            {                
                var insert = await _repository.InsertAsync(new UserEntity()
                {
                    UserId      = request.Account,
                    DailyLimit  = _quota.DailySentenceLimit,
                    CreatedAt   = DateTime.UtcNow,
                    UpdatedAt   = DateTime.UtcNow
                });

                return insert != null;
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

        public async Task<bool> TrackInfo(string userId, ActivityTrackRequest request)
        {
            UserEntity entity = await _repository.GetAsync(userId);

            /*=====================
             * 將相同的feature合併
             * ====================*/

            // 轉成 Dictionary：Feature -> ActivityTrackItem
            var trackMap = entity.TrackItems;

            foreach (var item in request.Items)
            {
                // 直接覆蓋（同 key 就更新 Timestamp）
                trackMap[item.Feature] = DateTimeOffset.FromUnixTimeMilliseconds(item.Timestamp).UtcDateTime;
            }

            entity.TrackItems = trackMap;

            await _repository.UpdateAsync(userId, entity);

            return true;
        }

        public async Task<bool> QuotaLimitChange(QuotaLimitChangeRequest request)
        {
            string userName = request.UserName;
            int quotaLimit  = request.QuotaLimitChange;

            UserEntity entity = await _repository.GetAsync(userName);
            entity.DailyLimit = quotaLimit;

            await _repository.UpdateAsync(userName, entity);

            return true;
        }

        public override async Task DeleteAsync(string key)
        {
            await _repository.DeleteAsync(key);
            await _authRepository.DeleteAsync(key);
        }
    }
}
