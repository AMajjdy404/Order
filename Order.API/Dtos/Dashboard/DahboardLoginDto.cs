using System.ComponentModel.DataAnnotations;

namespace Order.API.Dtos.Dashboard
{
    public class DahboardLoginDto
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }
}
