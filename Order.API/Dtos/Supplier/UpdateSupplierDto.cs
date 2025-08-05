using System.ComponentModel.DataAnnotations;

namespace Order.API.Dtos.Supplier
{
    public class UpdateSupplierDto
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string? CommercialName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? SupplierType { get; set; }
        public string? WarehouseLocation { get; set; }
        public string? WarehouseAddress { get; set; }
        public IFormFile? WarehouseImage { get; set; }
        public string? DeliveryMethod { get; set; }
        public double? ProfitPercentage { get; set; }
        public decimal? MinimumOrderPrice { get; set; } 
        [Range(1, int.MaxValue, ErrorMessage = "Minimum order items must be at least 1")]
        public int? MinimumOrderItems { get; set; }
        public int? DeliveryDays { get; set; }
    }
}
