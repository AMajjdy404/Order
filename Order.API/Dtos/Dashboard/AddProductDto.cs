namespace Order.API.Dtos.Dashboard
{
    public class AddProductDto
    {
        public string Name { get; set; }
        public IFormFile Image { get; set; }
        public int SectionId { get; set; }
        public int CompanyId { get; set; }
    }
}
