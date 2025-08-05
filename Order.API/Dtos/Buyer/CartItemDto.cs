namespace Order.API.Dtos.Buyer
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int SupplierProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
