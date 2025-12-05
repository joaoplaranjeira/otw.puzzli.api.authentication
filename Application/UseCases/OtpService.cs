using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Repositories;

namespace Application.UseCases;

public class OtpService : IOtpService
{
    private readonly IOtpRepository _otpRepository;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;
    private const int OtpExpirationMinutes = 10;
    private const int OtpLength = 6;

    public OtpService(IOtpRepository otpRepository, IEmailService emailService, ITokenService tokenService)
    {
        _otpRepository = otpRepository;
        _emailService = emailService;
        _tokenService = tokenService;
    }

    public async Task<OtpResponse> SendOtpAsync(string email, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        // Generate OTP code
        var otpCode = GenerateOtpCode();
        
        var otp = new OtpCode
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            Code = otpCode,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpirationMinutes),
            IsUsed = false,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        await _otpRepository.CreateAsync(otp, cancellationToken);
        
        // Send email via Loops
        var emailSent = await _emailService.SendOtpEmailAsync(email, otpCode, cancellationToken);

        if (!emailSent)
        {
            return new OtpResponse
            {
                Success = false,
                Message = "Failed to send OTP email"
            };
        }

        return new OtpResponse
        {
            Success = true,
            Message = "OTP sent successfully",
            ExpiresAt = otp.ExpiresAt
        };
    }

    public async Task<OtpResponse> ValidateOtpAsync(string email, string code, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var otp = await _otpRepository.GetByEmailAndCodeAsync(email.ToLowerInvariant(), code, cancellationToken);

        if (otp == null)
        {
            return new OtpResponse
            {
                Success = false,
                Message = "Invalid OTP code"
            };
        }

        if (otp.IsUsed)
        {
            return new OtpResponse
            {
                Success = false,
                Message = "OTP code has already been used"
            };
        }

        if (DateTime.UtcNow > otp.ExpiresAt)
        {
            return new OtpResponse
            {
                Success = false,
                Message = "OTP code has expired"
            };
        }

        // Mark OTP as used
        otp.IsUsed = true;
        otp.UsedAt = DateTime.UtcNow;
        await _otpRepository.UpdateAsync(otp, cancellationToken);

        // Generate JWT token
        var token = _tokenService.GenerateToken(email);

        return new OtpResponse
        {
            Success = true,
            Message = "OTP validated successfully",
            Token = token
        };
    }

    private static string GenerateOtpCode()
    {
        var random = new Random();
        var code = random.Next(0, 999999).ToString("D6");
        return code;
    }
}
