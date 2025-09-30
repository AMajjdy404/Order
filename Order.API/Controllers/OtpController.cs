using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Dtos;
using Order.Domain.Services;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OtpController : ControllerBase
    {
        private readonly IOtpService _otpService;

        public OtpController(IOtpService otpService)
        {
            _otpService = otpService;
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveOtp([FromBody] OtpRequest otp)
        { 
            await _otpService.SaveOtpAsync(otp.PhoneNumber,otp.OtpCode);

            return Ok(new { status = 200, message = "Otp saved" });
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            bool isValid = await _otpService.VerifyOtpAsync(request.Otp);

            if (!isValid)
                return BadRequest(new { status = 400, message = "Invalid or expired OTP" });

            return Ok(new { status = 200, message = "OTP verified successfully" });
        }
    }
}
