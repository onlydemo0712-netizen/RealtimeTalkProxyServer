namespace Common.DTO.User
{
    public class UserResponse
    {
        public string Id { get; set; }          = "";
        public string UserId { get; set; }
        public int DailyLimit { get; set; }
        public Dictionary<string, int> DailySentenceCounts { get; set; }
        public Dictionary<string, DateTime> TrackItems { get; set; }
    }
}
