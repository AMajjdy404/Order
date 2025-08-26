using System.ComponentModel.DataAnnotations;

namespace Order.API.Dtos.Dashboard
{
    public class AddToSupplierWalletDto
    {
        [Required(ErrorMessage = "SupplierId is required")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }
    }

}
