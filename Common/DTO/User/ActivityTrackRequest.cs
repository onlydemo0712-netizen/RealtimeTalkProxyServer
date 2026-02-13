namespace Common.DTO.User
{
    public class ActivityTrackItem
    {
        public string Feature { get; set; } = string.Empty;
        public long Timestamp { get; set; } // Unix ms
    }


    public class ActivityTrackRequest
    {
        public List<ActivityTrackItem> Items { get; set; } = new();
    }
}
