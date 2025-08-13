using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Models
{
    public class ReturnOrderItem
    {
        public int Id { get; set; }
        public int SupplierOrderItemId { get; set; }
        public int ReturnedQuantity { get; set; }
        public DateTime ReturnDate { get; set; }
        public SupplierOrderItem SupplierOrderItem { get; set; }
    }

}
