namespace Order.API.Dtos.Buyer
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Company { get; set; }
        public string ImageUrl { get; set; }
        public decimal? LowestPriceNow { get; set; } 
    }
}
