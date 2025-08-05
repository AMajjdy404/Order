namespace Order.Domain.Models
{
    //public enum OrderStatus
    //{
    //    Pending,
    //    Confirmed,
    //    Shipped,
    //    Delivered
    //}
    //public class BuyerOrder
    //{
    //    public int Id { get; set; }
    //    public int BuyerId { get; set; }
    //    public DateTime OrderDate { get; set; }
    //    public decimal TotalAmount { get; set; }
    //    public DateOnly? DeliveryDate { get; set; }
    //    public string? PaymentMethod { get; set; }
    //    public OrderStatus Status { get; set; }
    //    public Buyer Buyer { get; set; }
    //    public List<OrderItem> OrderItems { get; set; }
    //}

    public class BuyerOrder
    {
        public int Id { get; set; }
        public int BuyerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public Buyer Buyer { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public List<SupplierOrder> SupplierOrders { get; set; }
    }   
}
