using Order.Domain.Models;

namespace Order.API.Dtos.Dashboard
{
    public class ProductToReturnDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public int SectionId { get; set; }
        public int CompanyId { get; set; }
        public string Section { get; set; }
        public string Company { get; set; }
    }
}
