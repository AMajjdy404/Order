namespace Order.API.Dtos.Supplier
{
    public class SupplierRatingDto
    {
        public int Id { get; set; }
        public string BuyerName { get; set; }
        public int Rate { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
