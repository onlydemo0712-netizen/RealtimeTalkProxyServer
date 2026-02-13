using AutoMapper;

namespace Common.DTO.User
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserRequest, UserEntity>();
            CreateMap<UserEntity, UserResponse>();
            CreateMap<UserEntity, UserDocument>();
            CreateMap<UserDocument, UserEntity>();
        }
    }
}