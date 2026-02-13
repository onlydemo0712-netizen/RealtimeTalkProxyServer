using AetherCore.DataAccess;
using AetherCore.Utility.Attributes;
using AutoMapper;
using Common.DTO.Jobs;
using DataAccess.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class JobsDataAccess : MongoEntityDataAccess<JobsEntity, JobsDocument>, IJobsDataAccess
    {
        public JobsDataAccess(IMapper mapper)
            : base(mapper)
        {
        }
    }
}
