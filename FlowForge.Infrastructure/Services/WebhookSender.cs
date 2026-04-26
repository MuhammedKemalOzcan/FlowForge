using FlowForge.Domain.Models;
using FlowForge.Domain.Services;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace FlowForge.Infrastructure.Services
{
    public class WebhookSender : IWebhookSender
    {
        private readonly HttpClient _httpClient;

        public WebhookSender(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WebhookSendResult> SendAsync(string url, string payload, string signingSecret, string eventType, Guid deliveryId)
        {
            var startedAt = DateTime.UtcNow;
            var stopWatch = Stopwatch.StartNew();

            try
            {
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var signature = ComputeSignature(payload, signingSecret);
                content.Headers.Add("X-FlowForge-Signature", signature);
                content.Headers.Add("X-FlowForge-Event", eventType);
                content.Headers.Add("X-FlowForge-Delivery-Id", deliveryId.ToString());

                //Post işlemi:
                var response = await _httpClient.PostAsync(url, content);

                stopWatch.Stop();
                var completedAt = DateTime.UtcNow;
                var durationMs = stopWatch.ElapsedMilliseconds;

                //Response'un ilk 500 karakteri:
                var responseBody = await response.Content.ReadAsStringAsync();
                var snippet = responseBody.Length > 500 ? responseBody[..500] : responseBody;

                if (response.IsSuccessStatusCode)
                {
                    return WebhookSendResult.Success(response.StatusCode, durationMs, snippet, startedAt, completedAt);
                }
                else
                {
                    return WebhookSendResult.Failure(response.StatusCode, durationMs, snippet, startedAt, completedAt);
                }
            }
            catch (TaskCanceledException)
            {
                // Timeout
                stopWatch.Stop();
                return WebhookSendResult.Failure(
                    null, stopWatch.ElapsedMilliseconds,
                    "Request timed out", startedAt, DateTime.UtcNow);
            }
            catch (HttpRequestException ex)
            {
                // Connection refused, DNS failure vs.
                stopWatch.Stop();
                return WebhookSendResult.Failure(
                    null, stopWatch.ElapsedMilliseconds,
                    ex.Message, startedAt, DateTime.UtcNow);
            }
        }

        private static string ComputeSignature(string payload, string secret)
        {
            var keyBytes = Convert.FromBase64String(secret); //Byte'a döndürülüyor.
            var payloadBytes = Encoding.UTF8.GetBytes(payload); //Byte dizisi.
            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(payloadBytes); //Payload bytelarını alıp, secret anahtarıyla imzalıyor.
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}