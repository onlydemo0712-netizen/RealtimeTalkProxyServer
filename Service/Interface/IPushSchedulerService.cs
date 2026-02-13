using AetherCore.Service;
using Common.DTO.PushScheduler;

namespace Service.Interface
{
    public interface IPushSchedulerService : IService<PushSchedulerRequest, PushSchedulerResponse>
    {        
        Task<bool> PushMsgAllImmediately(PushMsgRequest request);
        Task<bool> RunPushScheduler();
    }
}
