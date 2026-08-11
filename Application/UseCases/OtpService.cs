using System.Security.Cryptography;
using System.Text;
using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Repositories;

namespace Application.UseCases;

public class OtpService : IOtpService
{
    private const int OtpExpirationMinutes = 10;
    private const int OtpLength = 6;
    private const int MaxFailedAttempts = 5;
    private const int BlockDurationMinutes = 15;
    private const int RateLimitRequests = 3;
    private const int RateLimitWindowMinutes = 15;

    private readonly IOtpRepository _otpRepository;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;

    public OtpService(
        IOtpRepository otpRepository,
        IEmailService emailService,
        ITokenService tokenService,
        IUserRepository userRepository)
    {
        _otpRepository = otpRepository;
        _emailService = emailService;
        _tokenService = tokenService;
        _userRepository = userRepository;
    }

    public async Task<OtpResponse> SendOtpAsync(
        string email,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return new OtpResponse
            {
                Success = true,
                Message = "Se o email estiver registado e ativo, receberá um código OTP."
            };
        }

        var latestOtp = await _otpRepository.GetLatestByEmailAsync(normalizedEmail, cancellationToken);
        if (latestOtp?.BlockedUntil > DateTime.UtcNow)
        {
            return new OtpResponse
            {
                Success = false,
                Message = "Demasiadas tentativas. Tente novamente mais tarde."
            };
        }

        var recentOtps = await _otpRepository.GetRecentByEmailAsync(
            normalizedEmail,
            DateTime.UtcNow.AddMinutes(-RateLimitWindowMinutes),
            cancellationToken);
        if (recentOtps.Count >= RateLimitRequests)
        {
            return new OtpResponse
            {
                Success = false,
                Message = "Foram efetuados demasiados pedidos. Aguarde antes de pedir outro código."
            };
        }

        var otp = new OtpCode
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            Code = GenerateOtpCode(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpirationMinutes),
            IsUsed = false,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
        await _otpRepository.CreateAsync(otp, cancellationToken);

        if (!await _emailService.SendOtpEmailAsync(normalizedEmail, otp.Code, cancellationToken))
        {
            otp.IsUsed = true;
            otp.UsedAt = DateTime.UtcNow;
            await _otpRepository.UpdateAsync(otp, cancellationToken);
            return new OtpResponse { Success = false, Message = "Não foi possível enviar o email OTP." };
        }

        return new OtpResponse
        {
            Success = true,
            Message = "OTP enviado com sucesso.",
            ExpiresAt = otp.ExpiresAt
        };
    }

    public async Task<OtpResponse> ValidateOtpAsync(
        string email,
        string code,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return InvalidOtpResponse();
        }

        var otp = await _otpRepository.GetLatestByEmailAsync(normalizedEmail, cancellationToken);
        if (otp is null)
        {
            return InvalidOtpResponse();
        }

        if (otp.BlockedUntil > DateTime.UtcNow)
        {
            return new OtpResponse { Success = false, Message = "Conta temporariamente bloqueada." };
        }

        if (otp.IsUsed)
        {
            return new OtpResponse { Success = false, Message = "O código OTP já foi utilizado." };
        }

        if (DateTime.UtcNow > otp.ExpiresAt)
        {
            return new OtpResponse { Success = false, Message = "O código OTP expirou." };
        }

        var expectedCode = Encoding.UTF8.GetBytes(otp.Code);
        var suppliedCode = Encoding.UTF8.GetBytes(code);
        if (expectedCode.Length != suppliedCode.Length ||
            !CryptographicOperations.FixedTimeEquals(expectedCode, suppliedCode))
        {
            otp.FailedAttempts++;
            if (otp.FailedAttempts >= MaxFailedAttempts)
            {
                otp.BlockedUntil = DateTime.UtcNow.AddMinutes(BlockDurationMinutes);
            }

            await _otpRepository.UpdateAsync(otp, cancellationToken);
            return InvalidOtpResponse();
        }

        otp.IsUsed = true;
        otp.UsedAt = DateTime.UtcNow;
        await _otpRepository.UpdateAsync(otp, cancellationToken);

        var permissions = await _userRepository.GetPermissionsAsync(user.Id, cancellationToken);
        return new OtpResponse
        {
            Success = true,
            Message = "Autenticação concluída com sucesso.",
            Token = _tokenService.GenerateToken(user, permissions)
        };
    }

    private static string GenerateOtpCode() =>
        RandomNumberGenerator.GetInt32(0, 1_000_000).ToString($"D{OtpLength}");

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static OtpResponse InvalidOtpResponse() => new()
    {
        Success = false,
        Message = "Email ou código OTP inválido."
    };
}
