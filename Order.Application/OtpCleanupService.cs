using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.Domain.Interfaces;
using Order.Domain.Models;

namespace Order.Application
{
    public class OtpCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public OtpCleanupService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var otpRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<Otp>>();

                var expiredOtps = await otpRepo.GetAllAsync(x => x.ExpirationTime <= DateTime.Now);
                if (expiredOtps.Any())
                {
                    otpRepo.RemoveRange(expiredOtps);
                    await otpRepo.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // كل دقيقة امسح المنتهي
            }
        }
    }

}
