using Domain.Entities;

namespace Domain.Repositories;

public interface IOtpRepository
{
    Task<OtpCode> CreateAsync(OtpCode otpCode, CancellationToken cancellationToken = default);
    Task UpdateAsync(OtpCode otpCode, CancellationToken cancellationToken = default);
    Task<OtpCode?> GetLatestByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OtpCode>> GetRecentByEmailAsync(string email, DateTime sinceUtc, CancellationToken cancellationToken = default);
}
