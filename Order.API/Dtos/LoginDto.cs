using System.ComponentModel.DataAnnotations;

namespace Order.API.Dtos
{
    public class LoginDto
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RememberMe { get; set; } = false;
    }
}
