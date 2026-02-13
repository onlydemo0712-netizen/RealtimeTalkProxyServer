using AutoMapper;
using Common.DTO.AdminAuth;

namespace Common.DTO.AdminAuth
{
    public class AdminAuthProfile : Profile
    {
        public AdminAuthProfile()
        {
            CreateMap<AdminAuthEntity, AdminAuthDocument>();
            CreateMap<AdminAuthDocument, AdminAuthEntity>();
        }
    }

}
