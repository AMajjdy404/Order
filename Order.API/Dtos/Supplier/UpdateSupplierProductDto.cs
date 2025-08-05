using System.ComponentModel.DataAnnotations;

namespace Order.API.Dtos.Supplier
{
    public class UpdateSupplierProductDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "PriceNow must be greater than 0")]
        public decimal? PriceNow { get; set; }

        public decimal? PriceBefore { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required]
        public string Status { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "MaxOrderLimit must be greater than 0")]
        public int? MaxOrderLimit { get; set; }
    }
}
