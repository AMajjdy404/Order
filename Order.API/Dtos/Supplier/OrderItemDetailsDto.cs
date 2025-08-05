namespace Order.API.Dtos.Supplier
{
    public class OrderItemDetailsDto
    {
        public int SupplierProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal ItemTotal { get; set; }
        public string CompanyName { get; set; }
    }
}
