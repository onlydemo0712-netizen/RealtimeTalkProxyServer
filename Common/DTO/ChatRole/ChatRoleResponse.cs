namespace Common.DTO.ChatRole
{
    public class ChatRoleResponse
    {
        public string Id { get; set; }                  = string.Empty;
        public string RoleId { get; set; }              = string.Empty;
        public string ModelId { get; set; }             = string.Empty; 
        public string RoleName { get; set; }            = string.Empty;
        public string Description { get; set; }         = string.Empty;
        public bool IsMale { get; set; }
        public bool SpeakFirst { get; set; }
        public string RoleInstructions { get; set; }    = string.Empty;
        public string Voice { get; set; }               = string.Empty; // 例如 "alloy"、"verse"...
        public string Motion { get; set; }              = string.Empty;
    }
}
