using Application.DTOs;

namespace Application.UseCases;

public interface IOtpService
{
    Task<OtpResponse> SendOtpAsync(string email, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<OtpResponse> ValidateOtpAsync(string email, string code, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
}
