using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.PushScheduler
{
    public record RecurringRescheduleInfo(string Id, DateTime NextSendTimeUtc);
}
