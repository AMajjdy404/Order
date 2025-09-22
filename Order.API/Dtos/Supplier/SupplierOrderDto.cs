using Order.API.Dtos.Buyer;

namespace Order.API.Dtos.Supplier
{
    public class SupplierOrderDto
    {
        public int Id { get; set; }
        public string BuyerName { get; set; }
        public string BuyerPhone { get; set; }
        public string PropertyName { get; set; }
        public string PropertyAddress { get; set; }
        public string PropertyLocation { get; set; }
        public decimal TotalAmount { get; set; }
        public string DeliveryDate { get; set; } // formatted
        public string Status { get; set; }
        public decimal? WalletPaymentAmount { get; set; } = 0;
        public string? PaymentMethod { get; set; } = string.Empty;
        public List<SupplierOrderItemDto> Items { get; set; } = new();
    }
}
