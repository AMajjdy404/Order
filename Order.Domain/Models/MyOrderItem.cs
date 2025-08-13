using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Models
{
    public class MyOrderItem
    {
        public int Id { get; set; } 
        public int MyOrderId { get; set; } 
        public int SupplierProductId { get; set; } 
        public string ProductName { get; set; } 
        public int Quantity { get; set; } 
        public decimal UnitPrice { get; set; } 
        public string SupplierName { get; set; } 
    }
}
