using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Order.Application;
using Order.Domain.Models;
using Order.Domain.Services;
using Order.Infrastructure.Data;

namespace Order.API.Extensions
{
    public static class IdentityExtension
    {
        public static IServiceCollection AddIdentityService(this IServiceCollection Services, IConfiguration configuration)
        {
            Services.AddScoped<ITokenService, TokenService>();
            Services.AddScoped<RoleManager<IdentityRole>>();
            Services.AddScoped<IPasswordHasher<Buyer>, PasswordHasher<Buyer>>();
            Services.AddScoped<IPasswordHasher<Supplier>, PasswordHasher<Supplier>>();

            Services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Debug);
            });

            Services.AddIdentity<AppOwner, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
           .AddEntityFrameworkStores<OrderDbContext>()
           .AddDefaultTokenProviders();

            Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["JWT:ValidIssuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["JWT:ValidAudience"],
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]))
                };
                // Log Errors
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var errorMessage = $"Authentication failed: {context.Exception.Message}";
                        context.Response.Headers.Add("Authentication-Failed", errorMessage);
                        Console.WriteLine(errorMessage);
                        Console.WriteLine($"Token: {context.Request.Headers["Authorization"]}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated successfully");
                        Console.WriteLine($"User: {context.Principal?.Identity?.Name}");
                        return Task.CompletedTask;
                    },

                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["yourAppCookie"];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    },

                };
            });

            Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
                 
            });
            return Services;
        }
    }
}
