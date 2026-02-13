namespace Common.DTO.User
{
    public class QuotaLimitChangeRequest
    {
        public string UserName { get; set; }
        public int QuotaLimitChange { get; set; }
    }
}
