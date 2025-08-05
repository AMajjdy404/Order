using System.ComponentModel.DataAnnotations;
using Order.Domain.Models;

namespace Order.API.Dtos.Supplier
{
    public class AddSupplierProductDto
    {
        [Required]
        public int ProductId { get; set; }

        public decimal? PriceBefore { get; set; } = 0;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "PriceNow must be greater than 0")]
        public decimal PriceNow { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "MaxOrderLimit must be greater than 0")]
        public int MaxOrderLimit { get; set; }
    }
}
