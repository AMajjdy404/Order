namespace Order.API.Dtos.Supplier
{
    public class SupplierStatementDto
    {
        public int Id { get; set; }
        public int SupplierOrderId { get; set; }
        public decimal Amount { get; set; }
        public string Title { get; set; }
        public string BuyerName { get; set; }
        public string PropertyName { get; set; }
        public string DeliveryDate { get; set; }
        public string CreatedAt { get; set; }
    }

}
