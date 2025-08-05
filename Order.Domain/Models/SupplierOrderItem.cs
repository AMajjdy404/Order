using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Models
{
    public class SupplierOrderItem
    {
        public int Id { get; set; }
        public int SupplierOrderId { get; set; }
        public int SupplierProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public SupplierOrder SupplierOrder { get; set; }
        public SupplierProduct SupplierProduct { get; set; }
    }

}
