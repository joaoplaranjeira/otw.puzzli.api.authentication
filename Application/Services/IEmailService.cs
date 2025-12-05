namespace Application.Services;

public interface IEmailService
{
    Task<bool> SendOtpEmailAsync(string email, string otpCode, CancellationToken cancellationToken = default);
}
