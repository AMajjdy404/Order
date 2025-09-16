namespace Order.API.Dtos.Supplier
{
    public class SupplierProductDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public decimal? PriceBefore { get; set; }
        public decimal PriceNow { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public bool IsAvailable { get; set; }
        public int MaxOrderLimit { get; set; }
        public string ProductImageUrl { get; set; }
    }
}
