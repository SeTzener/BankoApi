using System.Text;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace BankoApi.Services.Push;

/// <summary>
/// Sends push notifications through Firebase Cloud Messaging.
/// The service is configured from <c>FIREBASE_SERVICE_ACCOUNT_BASE64</c> or
/// <c>FIREBASE_SERVICE_ACCOUNT_PATH</c>. When neither is set the API keeps
/// running and push sends fail gracefully instead of taking the process down.
/// </summary>
public class FirebasePushSender : IPushSender
{
    // FCM rejects multicast requests with more than 500 tokens.
    private const int MaxTokensPerRequest = 500;

    private readonly IConfiguration _configuration;
    private readonly ILogger<FirebasePushSender> _logger;
    private readonly object _initLock = new();
    private FirebaseMessaging? _messaging;
    private bool _initialized;

    public FirebasePushSender(IConfiguration configuration, ILogger<FirebasePushSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PushSendResult>> SendAsync(
        IReadOnlyList<string> tokens,
        PushMessage message,
        CancellationToken cancellationToken = default)
    {
        if (tokens.Count == 0) return Array.Empty<PushSendResult>();

        var messaging = GetMessaging();
        if (messaging == null)
        {
            return tokens
                .Select(token => new PushSendResult(token, PushSendStatus.Failed, "Push notifications are not configured"))
                .ToList();
        }

        var results = new List<PushSendResult>(tokens.Count);

        foreach (var batch in tokens.Chunk(MaxTokensPerRequest))
        {
            // Registration tokens are still supported by FCM. FIDs would require the mobile
            // clients to send Firebase Installation IDs instead, so the token contract stays.
#pragma warning disable CS0618
            var multicast = new MulticastMessage
            {
                Tokens = batch.ToList(),
#pragma warning restore CS0618
                Notification = new Notification
                {
                    Title = message.Title,
                    Body = message.Body
                },
                Data = message.Data?.ToDictionary(entry => entry.Key, entry => entry.Value),
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification { Sound = "default" }
                },
                // TODO(iOS push): iOS delivery needs an APNs authentication key uploaded in
                // Firebase Console -> Project settings -> Cloud Messaging. That requires a paid
                // Apple Developer Program membership, which is not available yet. Android works
                // without it; until the key is uploaded, iOS sends fail but their tokens stay
                // active so delivery starts automatically once the key is in place.
                Apns = new ApnsConfig
                {
                    Aps = new Aps { Sound = "default" }
                }
            };

            try
            {
                var response = await messaging.SendEachForMulticastAsync(multicast, cancellationToken);
                for (var index = 0; index < batch.Length; index++)
                {
                    var sendResponse = response.Responses[index];
                    results.Add(sendResponse.IsSuccess
                        ? new PushSendResult(batch[index], PushSendStatus.Sent)
                        : ToResult(batch[index], sendResponse.Exception));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FCM send failed for a batch of {Count} tokens", batch.Length);
                results.AddRange(batch.Select(token =>
                    new PushSendResult(token, PushSendStatus.Failed, ex.Message)));
            }
        }

        return results;
    }

    private static PushSendResult ToResult(string token, Exception? exception)
    {
        if (exception is FirebaseMessagingException messagingException)
        {
            var status = messagingException.MessagingErrorCode is MessagingErrorCode.Unregistered
                or MessagingErrorCode.InvalidArgument
                ? PushSendStatus.InvalidToken
                : PushSendStatus.Failed;

            return new PushSendResult(token, status, messagingException.MessagingErrorCode.ToString());
        }

        return new PushSendResult(token, PushSendStatus.Failed, exception?.Message);
    }

    private FirebaseMessaging? GetMessaging()
    {
        if (_initialized) return _messaging;

        lock (_initLock)
        {
            if (_initialized) return _messaging;
            _initialized = true;

            var serviceAccountJson = ReadServiceAccountJson();
            if (serviceAccountJson == null)
            {
                _logger.LogWarning(
                    "Firebase push is disabled: set FIREBASE_SERVICE_ACCOUNT_BASE64 or FIREBASE_SERVICE_ACCOUNT_PATH");
                return null;
            }

            try
            {
                var app = FirebaseApp.Create(new AppOptions
                {
                    Credential = CredentialFactory
                        .FromJson<ServiceAccountCredential>(serviceAccountJson)
                        .ToGoogleCredential()
                });

                _messaging = FirebaseMessaging.GetMessaging(app);
                _logger.LogInformation("Firebase push sender initialised for project {ProjectId}", app.Options.ProjectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialise the Firebase push sender; push notifications are disabled");
            }

            return _messaging;
        }
    }

    private string? ReadServiceAccountJson()
    {
        var base64 = _configuration["FIREBASE_SERVICE_ACCOUNT_BASE64"];

        if (!string.IsNullOrWhiteSpace(base64))
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(base64.Trim()));
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "FIREBASE_SERVICE_ACCOUNT_BASE64 is not valid base64");
                return null;
            }
        }

        var path = _configuration["FIREBASE_SERVICE_ACCOUNT_PATH"];

        if (string.IsNullOrWhiteSpace(path)) return null;

        if (!File.Exists(path))
        {
            _logger.LogError("Firebase service account file was not found at {Path}", path);
            return null;
        }

        return File.ReadAllText(path);
    }
}
