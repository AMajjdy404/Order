using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Models
{
    public class SupplierDeliveryStation
    {
        public int Id { get; set; }

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        public int DeliveryStationId { get; set; }
        public DeliveryStation DeliveryStation { get; set; }

        public decimal MinimumOrderPrice { get; set; }
    }


}
