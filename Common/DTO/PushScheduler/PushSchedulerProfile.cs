using AutoMapper;
using Common.DTO.PushScheduler;

namespace Common.DTO.PushScheduler
{
    public class PushSchedulerProfile : Profile
    {
        public PushSchedulerProfile()
        {
            CreateMap<PushSchedulerRequest, PushSchedulerEntity>();
            CreateMap<PushSchedulerEntity, PushSchedulerResponse>();
            CreateMap<PushSchedulerEntity, PushSchedulerDocument>();
            CreateMap<PushSchedulerDocument, PushSchedulerEntity>();
        }
    }

}
