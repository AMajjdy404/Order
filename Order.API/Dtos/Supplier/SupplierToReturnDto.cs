namespace Order.API.Dtos.Supplier
{
    public class SupplierToReturnDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; } 
        public string CommercialName { get; set; }
        public string PhoneNumber { get; set; }
        public string SupplierType { get; set; }
        public string WarehouseLocation { get; set; }
        public string WarehouseAddress { get; set; }
        public string WarehouseImageUrl { get; set; }
        public string DeliveryMethod { get; set; }
        public double ProfitPercentage { get; set; }
        public decimal MinimumOrderPrice { get; set; }
        public int MinimumOrderItems { get; set; }
        public int DeliveryDays { get; set; }
        public string Token { get; set; } = "";
    }
}
