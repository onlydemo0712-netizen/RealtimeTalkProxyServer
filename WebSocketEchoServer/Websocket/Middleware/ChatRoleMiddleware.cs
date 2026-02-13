using Repository.Interface;
using AetherCore.WebSockets;

namespace ElderAIServer.Websocket.Middleware
{
    public class ChatRoleMiddleware : IWebSocketMiddleware
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ChatRoleMiddleware(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        /*=====================================
       * 實作 IWebSocketMiddleware 介面
       * =====================================*/
        public async Task OnConnectedAsync(WebSocketContext ctx, Func<Task> next)
        {
            object msg = await CreateInitMessage(ctx.Headers["ChatRole"]);

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
        private async Task<object> CreateInitMessage(string? chatRoleId)
        {
            if (string.IsNullOrEmpty(chatRoleId))
                return new { Type = "RoleInit" };

            using var scope = _scopeFactory.CreateScope();
            var repo        = scope.ServiceProvider.GetRequiredService<IChatRoleRepository>();
            var entity      = await repo.GetAsync(chatRoleId);

            return new
            {
                Type    = "RoleInit",
                Payload = new
                {
                    aiInfo = new
                    {
                        bAIFirst = entity.SpeakFirst,
                    }
                }
            };
        }
    }
}
