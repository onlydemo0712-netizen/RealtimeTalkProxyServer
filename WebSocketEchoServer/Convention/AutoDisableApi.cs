using System.Collections.Generic;
using AetherCore.Utility.Convention;

namespace ElderAIServer.Convention
{
    public class AutoDisableApi : ListDisableApiConvention
    {
        protected override HashSet<string> GetDisabledList()
        {
            HashSet<string> disableSet = new HashSet<string>(StringComparer.Ordinal)
            {
                "AuthController.Create",
                "AuthController.GetAll",
                "AuthController.Get",
                "AuthController.Update",
                "AuthController.Delete",

                "UserController.Create",
                "UserController.Update",

                "AdminAuthController.Create",
                "AdminAuthController.GetAll",
                "AdminAuthController.Get",
                "AdminAuthController.Update",
                //"AdminAuthController.Delete",

                "ConversationController.Create",
                "ConversationController.Update",

                "JobsController.Create",
                "JobsController.GetAll",
                "JobsController.Get",
                "JobsController.Update",
                "JobsController.Delete",
            };

            return disableSet;
        }
    }
}
