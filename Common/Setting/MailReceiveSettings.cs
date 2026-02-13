using AetherCore.Utility.Attributes;

namespace Common.Setting
{
    [AppSettings]
    public class MailReceiveSettings
    {
        public string ToMail { get; set; }          = string.Empty;
        public string Subject { get; set; }         = string.Empty;
    }
}
