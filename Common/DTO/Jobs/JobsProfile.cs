using AutoMapper;
using Common.DTO.Jobs;

namespace Common.DTO.Jobs
{
    public class JobsProfile : Profile
    {
        public JobsProfile()
        {
            CreateMap<JobsRequest, JobsEntity>();
            CreateMap<JobsEntity, JobsResponse>();
            CreateMap<JobsEntity, JobsDocument>();
            CreateMap<JobsDocument, JobsEntity>();
        }
    }

}
