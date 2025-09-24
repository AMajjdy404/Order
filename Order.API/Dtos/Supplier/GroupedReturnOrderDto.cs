namespace Order.API.Dtos.Supplier
{
    public class GroupedReturnOrderDto
    {
        public int SupplierOrderId { get; set; }
        public string OrderDate { get; set; }
        public string BuyerName { get; set; }
        public int TotalReturnedQuantity { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public List<ReturnOrderDto> Items { get; set; }
    }
}
