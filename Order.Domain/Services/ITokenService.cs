using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Order.Domain.Models;

namespace Order.Domain.Services
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(AppOwner appOwner, UserManager<AppOwner> userManager, bool rememberMe = false);
        Task<string> CreateTokenAsync(Supplier supplier, bool rememberMe = false);
        Task<string> CreateTokenAsync(Buyer buyer, bool rememberMe = false);

        public void StoreTokenInCookie(string token, DateTime expiration, HttpContext context);
    }
}
