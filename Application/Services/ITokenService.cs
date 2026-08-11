using Domain.Entities;

namespace Application.Services;

public interface ITokenService
{
    string GenerateToken(User user, IEnumerable<string> permissions);
}
