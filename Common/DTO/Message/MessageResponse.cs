namespace Common.DTO.Message
{
    public class MessageResponse
    {
        public string Id { get; set; }      = string.Empty;
        public string ConversationId { get; set; }                  // 所屬對話ID
        public MessagePair Pair { get; set; }                       // 使用者與AI訊息
    }
}
