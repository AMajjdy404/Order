namespace Order.API.Dtos.Buyer
{
    public class CartDto
    {
        public int OrderId { get; set; }
        public List<CartSupplierDto> Suppliers { get; set; }
        public decimal GrandTotal { get; set; }
    }
}
