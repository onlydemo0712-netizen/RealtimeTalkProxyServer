namespace Common.DTO.Conversation
{
    public class ConversationResponse
    {
        public string Id { get; set; }          = string.Empty;
        public string UserName { get; set; }    = string.Empty; // 談話使用者 Name
        public string RoleId { get; set; }      = string.Empty; // AI Role Id
    }
}
