namespace Common.DTO.PushScheduler
{
    public record PushFailUpdate
    (
        string Id,
        PushStatus NewStatus,
        string Error,
        DateTime LastTryAtUtc
    );
}
