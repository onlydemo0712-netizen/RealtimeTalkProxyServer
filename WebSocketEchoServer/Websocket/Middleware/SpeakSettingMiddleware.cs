using Common.Setting;
using Microsoft.Extensions.Options;
using Repository.Interface;
using AetherCore.WebSockets;

namespace ElderAIServer.Websocket.Middleware
{
    public class SpeakSettingMiddleware : IWebSocketMiddleware
    {
        private readonly SpeakSettings _speakSettings;

        public SpeakSettingMiddleware(IOptions<SpeakSettings> speakSetting)
        {
            _speakSettings = speakSetting.Value;
        }

        /*=====================================
       * 實作 IWebSocketMiddleware 介面
       * =====================================*/
        public async Task OnConnectedAsync(WebSocketContext ctx, Func<Task> next)
        {
            object msg = await CreateInitMessage();

            await ctx.SendJsonAsync(msg); // for init message
            await next();
        }

        public async Task OnDisconnectedAsync(WebSocketContext ctx, Func<Task> next)
        {
            await next();
        }

        public async Task OnTextAsync(WebSocketContext ctx, string text, Func<Task> next)
        {
            await next();
        }

        public async Task OnBinaryAsync(WebSocketContext ctx, byte[] bytes, Func<Task> next)
        {            
            await next();
        }

        /*============================================
         * 私有方法
         * ==========================================*/
        private async Task<object> CreateInitMessage()
        {
            if (_speakSettings == null)
                return new { Type = "SpeakInit" };

            return new
            {
                Type    = "SpeakInit",
                Payload = new
                {
                    speak = new
                    {
                        duration = _speakSettings.Duration
                    }
                }
            };
        }
    }
}
