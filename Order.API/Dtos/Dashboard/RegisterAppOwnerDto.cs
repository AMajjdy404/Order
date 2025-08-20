using System.ComponentModel.DataAnnotations;

namespace Order.API.Dtos.Dashboard
{
    public class RegisterAppOwnerDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; } // Editor أو Admin
    }

}
