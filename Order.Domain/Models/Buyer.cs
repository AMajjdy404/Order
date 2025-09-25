using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Models
{
    public class Buyer
    {
        public int Id { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]

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

        public string PropertyInsideImagePath { get; set; }
        [Required]

        public string PropertyOutsideImagePath { get; set; }
        public bool IsActive { get; set; } = false;
        public decimal WalletBalance { get; set; } = 0;
        public string? DeviceToken { get; set; }

        public int DeliveryStationId { get; set; }
        public DeliveryStation DeliveryStation { get; set; }

    }
}
