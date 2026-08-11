using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OtpRepository : IOtpRepository
{
    private readonly ApplicationDbContext _context;

    public OtpRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OtpCode> CreateAsync(OtpCode otpCode, CancellationToken cancellationToken = default)
    {
        await _context.OtpCodes.AddAsync(otpCode, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return otpCode;
    }

    public Task<OtpCode?> GetLatestByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.OtpCodes
            .Where(otp => otp.Email == email)
            .OrderByDescending(otp => otp.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<OtpCode>> GetRecentByEmailAsync(
        string email,
        DateTime sinceUtc,
        CancellationToken cancellationToken = default) =>
        await _context.OtpCodes
            .AsNoTracking()
            .Where(otp => otp.Email == email && otp.CreatedAt >= sinceUtc)
            .ToListAsync(cancellationToken);

    public async Task UpdateAsync(OtpCode otpCode, CancellationToken cancellationToken = default)
    {
        _context.OtpCodes.Update(otpCode);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
