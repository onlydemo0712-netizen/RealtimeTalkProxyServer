namespace Common.DTO.PushScheduler
{
    public class PushSchedulerRequest
    {
        public string Title { get; set; }   = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SendTime { get; set; }
        //public Dictionary<string, object>? Data { get; set; }
    }
}
