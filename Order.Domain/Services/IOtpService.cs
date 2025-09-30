using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Services
{
    public interface IOtpService
    {
        Task SaveOtpAsync(string phoneNumber, string otp);
        Task<bool> VerifyOtpAsync(string otp);
    }
}
