using System.ComponentModel.DataAnnotations;

namespace Order.API.Dtos.Dashboard
{
    public class AddCompanyDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public IFormFile Image { get; set; }
    }
}
