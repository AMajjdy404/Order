using System.ComponentModel.DataAnnotations;

namespace Order.API.Dtos.Dashboard
{
    public class AddSectionDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public IFormFile Image { get; set; }
    }
}
