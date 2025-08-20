using System.ComponentModel.DataAnnotations;

namespace Order.API.Dtos.Dashboard
{
    public class UpdateAppOwnerDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? Password { get; set; } // اختياري لو عايز يغير الباسورد

        public string? Role { get; set; } // Editor أو Admin
    }
}
