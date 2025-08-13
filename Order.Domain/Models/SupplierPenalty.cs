using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Models
{
    public class SupplierPenalty
    {
        public int Id { get; set; }

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        public int SupplierOrderId { get; set; }
        public SupplierOrder SupplierOrder { get; set; }

        public decimal PenaltyAmount { get; set; }
        public string Reason { get; set; }

        public string BuyerName { get; set; }
        public DateOnly DeliveryDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
