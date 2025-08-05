using Order.API.Dtos.Buyer;

namespace Order.API.Dtos.Supplier
{
    public class SupplierOrderDto
    {
        public int SupplierOrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateOnly DeliveryDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Address { get; set; }
        public List<OrderItemDetailsDto> OrderItems { get; set; }
    }
}
