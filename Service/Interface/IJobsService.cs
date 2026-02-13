using AetherCore.Service;
using Common.DTO.Jobs;

namespace Service.Interface
{
    public interface IJobsService : IService<JobsRequest, JobsResponse>
    {
        Task<bool> DailyCheck();
    }
}
