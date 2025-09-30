using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Order.Domain.Interfaces;
using Order.Domain.Models;
using Order.Domain.Services;

namespace Order.Application
{
    public class OtpService : IOtpService
    {
        private readonly IGenericRepository<Otp> _otpRepo;

        public OtpService(IGenericRepository<Otp> otpRepo)
        {
            _otpRepo = otpRepo;
        }

        public async Task SaveOtpAsync(string phoneNumber, string otp)
        {
            // امسح أي OTP قديم لنفس الرقم
            var existing = await _otpRepo.GetAllAsync(x => x.PhoneNumber == phoneNumber);
            if (existing.Any())
            {
                _otpRepo.RemoveRange(existing);
            }

            var entity = new Otp
            {
                PhoneNumber = phoneNumber,
                Code = otp,
                ExpirationTime = DateTime.Now.AddMinutes(3)
            };

            await _otpRepo.AddAsync(entity);
            await _otpRepo.SaveChangesAsync();
        }


        public async Task<bool> VerifyOtpAsync(string otp)
        {
            var otpRecord = await _otpRepo.GetFirstOrDefaultAsync(
                x => x.Code == otp && x.ExpirationTime > DateTime.Now
            );

            if (otpRecord != null)
            {
                _otpRepo.Delete(otpRecord);
                await _otpRepo.SaveChangesAsync();
                return true;
            }

            return false;
        }

    }
}
