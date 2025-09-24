namespace Order.API.Dtos.Supplier
{
    public class ReturnOrderDto
    {
        public int Id { get; set; }
        public int SupplierOrderItemId { get; set; }
        public int SupplierOrderId { get; set; }
        public int ReturnedQuantity { get; set; }
        public string ReturnDate { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
    }

}
