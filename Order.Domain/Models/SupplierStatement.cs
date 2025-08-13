using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Models
{
    public class SupplierStatement
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public int? SupplierOrderId { get; set; }
        public decimal Amount { get; set; }
        public string Title { get; set; }
        public string BuyerName { get; set; }
        public string PropertyName { get; set; }
        public DateOnly? DeliveryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public Supplier Supplier { get; set; }
        public SupplierOrder SupplierOrder { get; set; }
    }

}
