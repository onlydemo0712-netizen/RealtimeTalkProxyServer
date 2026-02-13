using AetherCore.Utility.Attributes;

namespace AetherCore.Settings
{
    [AppSettings]
    public class OpenAISettings
    {
        public string ApiKey { get; set; }              = "";
        public string Model { get; set; }               = "gpt-4o-mini-realtime-preview";
        public string Voice { get; set; }               = "alloy";
        public string BasicInstructions { get; set; }   = "You are a helpful, concise voice assistant.";
        public bool AutoCreate { get; set; }            = false;
    }
}
