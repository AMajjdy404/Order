using Order.API.Helpers;
using Order.Application;
using Order.Domain.Interfaces;
using Order.Domain.Services;
using Order.Infrastructure.Implementation;


namespace Order.API.Extensions
{
    public static class ApplicationServiceExtension
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection Services)
        {
            Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            Services.AddScoped<IUnitOfWork, UnitOfWork>();
            Services.AddScoped<INotificationService, NotificationService>();
            Services.AddScoped<DataSeeder>();
            Services.AddAutoMapper(typeof(MappingProfiles));


            return Services;
        }
    }
}
