using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Models
{
    public class MyOrder
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string SupplierName { get; set; }
        public DateOnly? DeliveryDate { get; set; }
        public DateOnly OrderDate { get; set; }
        public int? BuyerOrderId { get; set; }
        public int BuyerId { get; set; }
        public int SupplierProductId { get; set; } // إضافة
        public decimal UnitPrice { get; set; } // إضافة
    }
}
