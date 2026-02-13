using DataAccess;
using DataAccess.Interface;
using OpenAIProxyService.Websockets;
using Repository;
using Repository.Interface;
using Service;
using Service.Interface;
using AetherCore.Module;
using AetherCore.Module.Interface;

namespace OpenAIProxyService.Extension
{
    public static class AddServiceExtension
    {
        static public void SlaveServices(this IServiceCollection services)
        {
            /********************************************
             * 設定 Moderation Client
             * ******************************************/
            services.AddHttpClient<IModerationClient, OpenAIModerationClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            /********************************************
             * 設定 OneSignal Client
             * ******************************************/
            services.AddHttpClient<IPushSchedulerService, PushSchedulerService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            /***********************************************************************************************
            * add Singleton Services
            ************************************************************************************************/
            services.AddSingleton<OpenAIProxyWsHub>();                  // OpenAI的 Websocket Hub（繼承 WebsocketHub）
            services.AddSingleton<ISmtpEmailSender, SmtpEmailSender>(); // Email 寄信服務

            /***********************************************************************************************
            * 讓 DI 容器可以提供 IHttpContextAccessor
            ************************************************************************************************/
            services.AddHttpContextAccessor();
        }
    }
}
