
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Order.API.Extensions;
using Order.API.Helpers;
using Order.API.Middlewares;
using Order.Infrastructure.Data;
using Serilog;
using Serilog.Formatting.Json;

namespace Order.API
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(Path.Combine(AppContext.BaseDirectory, "FirebaseConfig/order-481cc-firebase-adminsdk-fbsvc-054d4f2887.json"))
            });

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration
                    .WriteTo.File(
                        path: Path.Combine(context.HostingEnvironment.ContentRootPath, "logs", "error.log"),
                        formatter: new JsonFormatter(), // الـ JsonFormatter بيولّد التنسيق JSON
                        rollingInterval: RollingInterval.Day)
                    .Enrich.FromLogContext()
                    .MinimumLevel.Error();
            });

            builder.Services.AddDbContext<OrderDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddIdentityService(builder.Configuration);
            builder.Services.AddApplicationService();

           

            var app = builder.Build();

            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();

            try
            {
                var dbContext = services.GetRequiredService<OrderDbContext>();
                var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                await dbContext.Database.MigrateAsync(); // Update Database
                await seeder.SeedAsync(); // Account & Roles Seeding
                await OrderSeedDbContext.SeedAsync(dbContext);
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "This Error Happened During Applying Migration");

            }


            //app.UseSerilogRequestLogging(); // Serilog

            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseDeveloperExceptionPage();
            app.MapControllers();
            app.Run();
        }
    }
}
