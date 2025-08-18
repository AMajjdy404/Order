namespace Order.API.Dtos.Dashboard
{
    public class CreateAdvertisementDto
    {
        public string Name { get; set; } = null!;
        public IFormFile Image { get; set; } = null!;
    }

}
