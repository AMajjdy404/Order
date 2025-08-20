using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Dtos;
using Order.Domain.Services;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.DeviceToken))
                return BadRequest("Device token is required.");

            try
            {
                await _notificationService.SendNotificationAsync(dto.DeviceToken, dto.Title, dto.Body);
                _logger.LogInformation("Notification sent successfully to device token {DeviceToken}", dto.DeviceToken);

                return Ok(new { message = "Notification sent successfully ✅" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending notification to {DeviceToken}", dto.DeviceToken);
                return StatusCode(500, $"Error while sending notification: {ex.Message}");
            }
        }
    }
}
