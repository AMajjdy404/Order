namespace Order.API.Dtos.Supplier
{
    public class ReturnedOrderDto
    {
        public int Id { get; set; }
        public int SupplierOrderItemId { get; set; }
        public string ProductName { get; set; }
        public int ReturnedQuantity { get; set; }
        public DateTime ReturnDate { get; set; }

        // Buyer Info
        public string BuyerName { get; set; }
        public string BuyerPhone { get; set; }
        public string BuyerPropertyName { get; set; }


        // Supplier Info
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierType { get; set; }
        public string SupplierCommercialName { get; set; }
    }

}
