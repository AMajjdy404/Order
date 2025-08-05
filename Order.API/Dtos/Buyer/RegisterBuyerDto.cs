using System.ComponentModel.DataAnnotations;

namespace Order.API.Dtos.Buyer
{
    public class RegisterBuyerDto
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        [Required]
        public string PropertyName { get; set; }
        [Required]
        public string PropertyType { get; set; }
        [Required]
        public string PropertyLocation { get; set; }
        [Required]
        public string PropertyAddress { get; set; }
        [Required]
        public IFormFile PropertyInsideImage { get; set; }
        [Required]
        public IFormFile PropertyOutsideImage { get; set; }
        public string? ReferralCode { get; set; }
    }
}
