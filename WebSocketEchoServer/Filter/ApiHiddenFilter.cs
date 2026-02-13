using AetherCore.Utility.Filter;

namespace OpenAIProxyService.Controllers
{
    public class ApiHiddenFilter : HiddenApiDocumentFilter
    {
        protected override HashSet<string> GetHiddenList()
        {
            HashSet<string> hiddenSet = new HashSet<string>()
            {                
                "AuthController.Create",
                "AuthController.GetAll",
                "AuthController.Get",
                "AuthController.Update",
                "AuthController.Delete",

                "UserController.Create",
                "UserController.GetAll",
                "UserController.Get",
                "UserController.Update",
                "UserController.Delete",

                "AdminAuthController.Create",
                "AdminAuthController.GetAll",
                "AdminAuthController.Get",
                "AdminAuthController.Update",
                "AdminAuthController.Delete",

                "ConversationController.Create",
                "ConversationController.Update",
            };

            return hiddenSet;
        }
    }
}
