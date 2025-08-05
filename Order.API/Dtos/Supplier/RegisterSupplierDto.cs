using System.ComponentModel.DataAnnotations;

namespace Order.API.Dtos.Supplier
{
    public class RegisterSupplierDto
    {
        [Required]
        public string Name { get; set; }
        [Required]

        public string Email { get; set; }
        [Required]

        public string Password { get; set; }
        [Required]

        public string CommercialName { get; set; }
        [Required]

        public string PhoneNumber { get; set; }
        [Required]

        public string SupplierType { get; set; }
        [Required]

        public string WarehouseLocation { get; set; }
        [Required]

        public string WarehouseAddress { get; set; }
        [Required]

        public IFormFile WarehouseImage { get; set; }
        [Required]

        public string DeliveryMethod { get; set; }
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Minimum order price must be Bigger than 0")]
        public decimal MinimumOrderPrice { get; set; } 

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Minimum order items must be at least 1")]
        public int MinimumOrderItems { get; set; }
        [Required]
        public int DeliveryDays { get; set; }

    }
}
