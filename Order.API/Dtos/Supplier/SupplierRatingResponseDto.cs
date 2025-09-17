namespace Order.API.Dtos.Supplier
{
    public class SupplierRatingResponseDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierWarehouseImage { get; set; }
        public double AverageRate { get; set; }
        public int TotalRates { get; set; }
        public List<SupplierRatingDto> Ratings { get; set; }
    }
}
