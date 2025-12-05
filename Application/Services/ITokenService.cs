namespace Application.Services;

public interface ITokenService
{
    string GenerateToken(string email);
}
