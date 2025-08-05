using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Models
{
    public enum Status
    {
        Active,
        NotActive
    }
    public class SupplierProduct
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int SupplierId { get; set; }
        public decimal? PriceBefore { get; set; } = 0;
        public decimal PriceNow { get; set; }
        public int MaxOrderLimit { get; set; }
        public int Quantity { get; set; }
        public Status Status { get; set; }
        public bool IsAvailable { get; set; }
        public Product Product { get; set; }
        public Supplier Supplier { get; set; }
    }
}
