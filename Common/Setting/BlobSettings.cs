using AetherCore.Utility.Attributes;

namespace Common.Setting
{
    [AppSettings]
    public class BlobSettings
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
    }
}
