using Microsoft.Extensions.Primitives;
using OpenAIProxyService.Websockets;

namespace ElderAIServer.Websocket.Utility
{
    public sealed class InstructionInfo
    {
        public required string roleId;
        public required string language;
        public required string locale;
        public required string maxLength;
    }

    public static class InstructionInfoParser
    {
        public static InstructionInfo FromHeaders(IReadOnlyDictionary<string, StringValues> dict)
        {
            return new InstructionInfo
            {
                roleId      = Get(dict, "ChatRole", ""),
                language    = Get(dict, "Language", "繁體中文"),
                locale      = Get(dict, "Locale", "zh-TW"),
                maxLength   = Get(dict, "MaxLength", "80")
            };
        }

        private static string Get(
            IReadOnlyDictionary<string, StringValues> dict,
            string key,
            string defaultValue)
        {
            return dict.TryGetValue(key, out var value)
                ? value.ToString()
                : defaultValue;
        }
    }
}
