namespace Application.DTOs;

public class OtpResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public string? Token { get; set; }
}
