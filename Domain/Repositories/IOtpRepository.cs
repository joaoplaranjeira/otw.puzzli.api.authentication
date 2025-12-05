using Domain.Entities;

namespace Domain.Repositories;

public interface IOtpRepository
{
    Task<OtpCode> CreateAsync(OtpCode otpCode, CancellationToken cancellationToken = default);
    Task<OtpCode?> GetByEmailAndCodeAsync(string email, string code, CancellationToken cancellationToken = default);
    Task UpdateAsync(OtpCode otpCode, CancellationToken cancellationToken = default);
    Task<OtpCode?> GetLatestByEmailAsync(string email, CancellationToken cancellationToken = default);
}
