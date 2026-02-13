using AetherCore.Utility.Filter;

namespace OpenAIProxyService.Controllers
{
    public class ControllerAddSummaryFilter : AddSummaryFilter
    {
        protected override Dictionary<string, string> GetSummaryInfo()
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>()
            {         
                { "AuthController.Login", "使用者登入" },
                { "AuthController.ChangePassword", "改變密碼" },

                { "UserController.CreateIdentity", "創建新帳號" },
                { "UserController.Delete", "刪除使用者帳號" },
                { "UserController.GetAll", "取得所有使用者資訊" },
                { "UserController.Get", "取得指定使用者資訊" },
                { "UserController.GetProfile", "取得使用者自己的資訊" },
                { "UserController.TrackInfo", "記錄使用者自己的操作" },
                { "UserController.QuotaLimitChange", "設定使用者每日可使用的句數上限" },

                { "ChatRoleController.Create", "創建聊天對象" },
                { "ChatRoleController.GetAll", "取得所有聊天對象資訊" },
                { "ChatRoleController.Get", "取得指定聊天對象資訊" },
                { "ChatRoleController.Update", "更新指定聊天對象資訊" },
                { "ChatRoleController.Delete", "刪除指定聊天對象" },

                { "KnowledgeGuideController.Create", "創建知識指南" },
                { "KnowledgeGuideController.GetAll", "取得所有創建知識" },
                { "KnowledgeGuideController.Get", "取得指定創建知識" },
                { "KnowledgeGuideController.Update", "更新指定指定創建知識" },
                { "KnowledgeGuideController.Delete", "刪除創建知識" },
                { "KnowledgeGuideController.ImgUpload", "取得上傳創建知識圖片路徑" },
                
                { "AdminAuthController.Login", "後台使用者登入" },
                { "AdminAuthController.CreateIdentity", "創建後台使用者帳號" },
                { "AdminAuthController.ChangePassword", "改變後台使用者密碼" },
                { "AdminAuthController.Delete", "刪除後台使用者帳號" },

                //{ "ConversationController.Create", "創建聊天資訊" },
                { "ConversationController.GetAll", "取得所有聊天資訊" },
                { "ConversationController.Get", "取得指定聊天資訊" },
                //{ "ConversationController.Update", "更新指定聊天資訊" },
                { "ConversationController.Delete", "刪除指定聊天資訊" },
                { "ConversationController.GetDetail", "取得指定聊天內容" },

                { "PushSchedulerController.Create", "創建推播排程" },
                { "PushSchedulerController.GetAll", "取得所有推播排程" },
                { "PushSchedulerController.Get", "取得指定推播排程" },
                { "PushSchedulerController.Update", "更新指定推播排程" },
                { "PushSchedulerController.Delete", "刪除指定推播排程" },
                { "PushSchedulerController.PushMsgAllImmediately", "立刻推播一條訊息" },
                { "PushSchedulerController.RunPushScheduler", "執行推播排程" },
                
                { "JobsController.DailyCheck", "檢查聊天訊息中是否有需要警示內容" },
            };

            return keyValuePairs;
        }
    }
}
