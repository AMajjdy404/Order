using Microsoft.AspNetCore.Identity;
using Order.Domain.Models;

namespace Order.API.Helpers
{
    public class DataSeeder
    {
        private readonly UserManager<AppOwner> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public DataSeeder(UserManager<AppOwner> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task SeedAsync()
        {
            // جلب إعدادات الإداري من appsettings.json
            var adminEmail = _configuration["AdminSettings:Email"];
            var adminPassword = _configuration["AdminSettings:Password"];

            // التحقق من وجود الأدوار وإنشاؤها إذا لم تكن موجودة
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await _roleManager.RoleExistsAsync("Editor"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Editor"));
            }

            // التحقق من وجود حساب الإداري
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var admin = new AppOwner
                {
                    UserName = adminEmail.Split("@")[0],
                    Email = adminEmail,
                    EmailConfirmed = true // لتجنب الحاجة إلى تأكيد البريد
                };

                var result = await _userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    // إضافة دور "Admin" للمستخدم
                    await _userManager.AddToRoleAsync(admin, "Admin");
                }
                else
                {
                    throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
