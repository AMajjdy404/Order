using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Models
{
    public class ReferralCode
    {
        public int Id { get; set; }
        public string InvitationCode { get; set; } // الكود الدعوي
        public int BuyerId { get; set; } // ID الـ Buyer المدعو
        public Buyer Buyer { get; set; } // العلاقة مع Buyer
    }
}
