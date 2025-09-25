using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Models
{
    public class DeliveryStation
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // مثال: "فيصل - الجيزة"

        public ICollection<SupplierDeliveryStation> SupplierDeliveryStations { get; set; } = new List<SupplierDeliveryStation>();
    }

}
