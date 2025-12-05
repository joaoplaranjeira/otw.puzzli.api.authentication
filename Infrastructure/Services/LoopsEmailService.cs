using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Services;

namespace Infrastructure.Services;

public class LoopsEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoopsEmailService> _logger;

    public LoopsEmailService(HttpClient httpClient, IConfiguration configuration, ILogger<LoopsEmailService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendOtpEmailAsync(string email, string otpCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var loopsApiKey = _configuration["Loops:ApiKey"];
            var transactionalId = _configuration["Loops:TransactionalId"];

            if (string.IsNullOrEmpty(loopsApiKey))
            {
                _logger.LogError("Loops API key is not configured");
                throw new InvalidOperationException("Loops API key is not configured");
            }

            if (string.IsNullOrEmpty(transactionalId))
            {
                _logger.LogError("Loops TransactionalId is not configured");
                throw new InvalidOperationException("Loops TransactionalId is not configured");
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {loopsApiKey}");

            var payload = new
            {
                transactionalId,
                email,
                dataVariables = new
                {
                    otpCode
                }
            };

            _logger.LogInformation("Sending OTP email to {Email} with code {OtpCode}", email, otpCode);

            var response = await _httpClient.PostAsJsonAsync(
                "https://app.loops.so/api/v1/transactional",
                payload,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send email via Loops. Status: {StatusCode}, Response: {Response}", 
                    response.StatusCode, errorContent);
            }
            else
            {
                _logger.LogInformation("Email sent successfully via Loops to {Email}", email);
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via Loops to {Email}", email);
            return false;
        }
    }
}
