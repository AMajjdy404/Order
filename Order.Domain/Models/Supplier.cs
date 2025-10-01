using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Models
{
    public enum SupplierType
    {
        Wholesale = 1, // جملة
        BulkWholesale, // جملة الجملة
        Manufacturer, // شركة مصنعة
        RestaurantCafeSupplier // موردين المطاعم والكافيهات
    }
    public class Supplier
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        //public string Password { get; set; }
        public string CommercialName { get; set; }
        public string PhoneNumber { get; set; }
        public SupplierType SupplierType { get; set; }
        public string WarehouseLocation { get; set; }
        public string WarehouseAddress { get; set; }
        public string WarehouseImageUrl { get; set; }
        public string DeliveryMethod { get; set; }
        public double ProfitPercentage { get; set; } = 1;
        public int MinimumOrderItems { get; set; }
        public int DeliveryDays { get; set; }
        public bool IsActive { get; set; } = true; // Default = true
        public decimal? WalletBalance { get; set; } = 0;
        public string? DeviceToken { get; set; } = string.Empty;
        public ICollection<SupplierRating> Ratings { get; set; } = new List<SupplierRating>();
        public ICollection<SupplierDeliveryStation> SupplierDeliveryStations { get; set; }= new List<SupplierDeliveryStation>();

    }
}
