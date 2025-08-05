using Order.API.Dtos.Supplier;

namespace Order.API.Dtos.Buyer
{
    public class CartSupplierDto
    {
        public SupplierDto Supplier { get; set; }
        public List<CartItemDto> Items { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal MinimumOrderPrice { get; set; }
        public string MinimumOrderPriceProgress { get; set; }
        public int TotalItems { get; set; }
        public int MinimumOrderItems { get; set; }
        public string MinimumOrderItemsProgress { get; set; }
        public bool IsValid { get; set; }
    }
}
