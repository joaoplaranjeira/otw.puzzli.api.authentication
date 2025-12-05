using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;

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

    public async Task<OtpCode?> GetByEmailAndCodeAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        return await _context.OtpCodes
            .Where(o => o.Email == email && o.Code == code)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<OtpCode?> GetLatestByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.OtpCodes
            .Where(o => o.Email == email)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateAsync(OtpCode otpCode, CancellationToken cancellationToken = default)
    {
        _context.OtpCodes.Update(otpCode);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
