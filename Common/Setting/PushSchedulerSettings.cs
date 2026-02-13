using AetherCore.Utility.Attributes;

namespace Common.Setting
{
    [AppSettings]
    public class PushSchedulerSettings
    {
        public string KeyID { get; set; }
        public string ApiKey { get; set; }
        public string AppId { get; set; }
    }
}
