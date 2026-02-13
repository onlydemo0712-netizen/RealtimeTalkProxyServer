using AutoMapper;

namespace Common.DTO.Auth
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<AuthEntity, AuthDocument>();
            CreateMap<AuthDocument, AuthEntity>();
        }
    }

}
