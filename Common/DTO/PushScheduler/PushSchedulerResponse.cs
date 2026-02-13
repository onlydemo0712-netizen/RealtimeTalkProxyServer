namespace Common.DTO.PushScheduler
{
    public class PushSchedulerResponse
    {
        public string Id { get; set; }          = string.Empty;
        public string Title { get; set; }       = string.Empty;
        public string Message { get; set; }     = string.Empty;
        public DateTime SendTime { get; set; }
        //public Dictionary<string, object>? Data { get; set; }
        public PushStatus Status { get; set; }  = PushStatus.Pending;
        public string Creater { get; set; }     = string.Empty;
        public bool IsRecurring { get; set; }   = true;
    }
}
