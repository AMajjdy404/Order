namespace Order.API.Dtos.Supplier
{
    public class SupplierPenaltyDto
    {
        public int Id { get; set; }
        public int SupplierOrderId { get; set; }
        public decimal PenaltyAmount { get; set; }
        public string Reason { get; set; }
        public string BuyerName { get; set; }
        public string DeliveryDate { get; set; }
        public string CreatedAt { get; set; }
    }

}
