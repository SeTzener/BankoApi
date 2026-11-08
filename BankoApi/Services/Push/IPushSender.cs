namespace BankoApi.Services.Push;

public sealed record PushMessage(
    string Title,
    string Body,
    IReadOnlyDictionary<string, string>? Data = null);

public enum PushSendStatus
{
    Sent,
    InvalidToken,
    Failed
}

public sealed record PushSendResult(string Token, PushSendStatus Status, string? Error = null);

public interface IPushSender
{
    Task<IReadOnlyList<PushSendResult>> SendAsync(
        IReadOnlyList<string> tokens,
        PushMessage message,
        CancellationToken cancellationToken = default);
}
