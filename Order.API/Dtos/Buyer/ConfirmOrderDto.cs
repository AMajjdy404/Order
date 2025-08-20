using System.ComponentModel.DataAnnotations;

namespace Order.API.Dtos.Buyer
{
    public class ConfirmOrderDto
    {
        [Required(ErrorMessage = "Delivery Date is Required")]
        public DateOnly DeliveryDate { get; set; }
        public bool UseWallet { get; set; } = false;
        public string? Notes { get; set; } = string.Empty;
    }
}
