using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.UseCases;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class OtpController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly ILogger<OtpController> _logger;

    public OtpController(IOtpService otpService, ILogger<OtpController> logger)
    {
        _otpService = otpService;
        _logger = logger;
    }

    /// <summary>
    /// Send OTP code to user's email
    /// </summary>
    /// <param name="request">Email address to send OTP</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>OTP response with success status and expiration time</returns>
    [HttpPost("send")]
    [ProducesResponseType(typeof(OtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OtpResponse>> SendOtp([FromBody] SendOtpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers.UserAgent.ToString();

            var result = await _otpService.SendOtpAsync(request.Email, ipAddress, userAgent, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            _logger.LogInformation("OTP sent successfully to {Email}", request.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP to {Email}", request.Email);
            return StatusCode(500, new OtpResponse 
            { 
                Success = false, 
                Message = "An error occurred while sending OTP" 
            });
        }
    }

    /// <summary>
    /// Validate OTP code provided by user
    /// </summary>
    /// <param name="request">Email and OTP code to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(OtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OtpResponse>> ValidateOtp([FromBody] ValidateOtpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers.UserAgent.ToString();

            var result = await _otpService.ValidateOtpAsync(request.Email, request.Code, ipAddress, userAgent, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            _logger.LogInformation("OTP validated successfully for {Email}", request.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating OTP for {Email}", request.Email);
            return StatusCode(500, new OtpResponse 
            { 
                Success = false, 
                Message = "An error occurred while validating OTP" 
            });
        }
    }
}
