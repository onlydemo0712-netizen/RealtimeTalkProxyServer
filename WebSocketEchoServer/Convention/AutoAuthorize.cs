using AetherCore.Utility.Convention;
using AetherCore.Utility.Filter;

namespace ElderAIServer.Convention
{
    public class AutoAuthorize : ListAuthorizeConvention
    {
        protected override Dictionary<string, string> GetAuthorizeInfo()
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                // APP only
                { "AuthController.*", "AppJwt" },
  

                // Backend only
                { "ChatRoleController.*", "AdminJwt" },
                { "KnowledgeGuideController.*", "AdminJwt" },
                { "AdminAuthController.*", "AdminJwt" },
                { "ConversationController.*", "AdminJwt" },
                { "UserController.CreateIdentity", "AdminJwt" },
                { "UserController.Get", "AdminJwt" },
                { "UserController.GetAll", "AdminJwt" },
                { "UserController.QuotaLimitChange", "AdminJwt" },
                { "UserController.Delete", "AdminJwt" },
                { "PushSchedulerController.Create", "AdminJwt" },
                { "PushSchedulerController.Get", "AdminJwt" },
                { "PushSchedulerController.GetAll", "AdminJwt" },
                { "PushSchedulerController.Update", "AdminJwt" },
                { "PushSchedulerController.Delete", "AdminJwt" },
                { "PushSchedulerController.PushMsgAllImmediately", "AdminJwt" },

                // Both
                { "ChatRoleController.GetAll", "AppJwt,AdminJwt" },
                { "KnowledgeGuideController.GetAll", "AppJwt,AdminJwt" },
                { "UserController.TrackInfo", "AppJwt,AdminJwt" },
                { "UserController.GetProfile", "AppJwt,AdminJwt" },
            };

            return keyValuePairs;
        }
    }
}
