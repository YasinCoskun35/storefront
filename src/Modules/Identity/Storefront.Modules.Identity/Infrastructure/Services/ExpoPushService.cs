using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Sends push notifications via the Expo Push API.
/// https://docs.expo.dev/push-notifications/sending-notifications/
/// </summary>
public class ExpoPushService : IExpoPushService
{
    private const string ExpoApiUrl = "https://exp.host/--/api/v2/push/send";

    private readonly HttpClient _http;
    private readonly ILogger<ExpoPushService> _logger;

    public ExpoPushService(HttpClient http, ILogger<ExpoPushService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task SendAsync(
        string expoPushToken,
        string title,
        string body,
        object? data     = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(expoPushToken))
            return;

        // Expo push tokens start with ExponentPushToken[ or ea_
        if (!expoPushToken.StartsWith("ExponentPushToken[") && !expoPushToken.StartsWith("ea_"))
        {
            _logger.LogWarning("Ignoring invalid Expo push token format: {Token}", expoPushToken[..Math.Min(20, expoPushToken.Length)]);
            return;
        }

        var payload = new
        {
            to      = expoPushToken,
            title,
            body,
            sound   = "default",
            data    = data ?? new { }
        };

        try
        {
            var response = await _http.PostAsJsonAsync(ExpoApiUrl, payload, ct);

            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Expo push API returned {Status}: {Body}", response.StatusCode, raw);
                return;
            }

            // Parse ticket to detect DeviceNotRegistered errors
            var json    = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("data", out var dataEl))
            {
                // Single message response wraps in { data: { status, id, details } }
                if (dataEl.TryGetProperty("status", out var statusEl) &&
                    statusEl.GetString() == "error")
                {
                    if (dataEl.TryGetProperty("details", out var details) &&
                        details.TryGetProperty("error", out var errorEl) &&
                        errorEl.GetString() == "DeviceNotRegistered")
                    {
                        _logger.LogInformation("Expo push token is no longer registered — token should be cleared.");
                    }
                    else
                    {
                        _logger.LogWarning("Expo push error: {Json}", json);
                    }
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Never crash order status update because of a push failure
            _logger.LogError(ex, "Failed to send Expo push notification to token {Token}",
                expoPushToken[..Math.Min(20, expoPushToken.Length)]);
        }
    }
}
