using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Order.Domain.Models;
using Order.Domain.Services;

namespace Order.Application
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> CreateTokenAsync(AppOwner appOwner, UserManager<AppOwner> userManager, bool rememberMe = false)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, appOwner.Id),
                new Claim(ClaimTypes.Email, appOwner.Email),
                new Claim(ClaimTypes.Name, appOwner.UserName),
                new Claim("UserType", "AppOwner")
            };

            var userRoles = await userManager.GetRolesAsync(appOwner);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            return GenerateJwtToken(authClaims, rememberMe);
        }

        public async Task<string> CreateTokenAsync(Supplier supplier, bool rememberMe = false)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, supplier.Id.ToString()),
                new Claim(ClaimTypes.Email, supplier.Email),
                new Claim(ClaimTypes.MobilePhone, supplier.PhoneNumber),
                new Claim("UserType", "Supplier")
            };

            return GenerateJwtToken(authClaims, rememberMe);
        }

        public async Task<string> CreateTokenAsync(Buyer buyer, bool rememberMe = false)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, buyer.Id.ToString()),
                new Claim(ClaimTypes.MobilePhone, buyer.PhoneNumber),
                new Claim("UserType", "Buyer")
            };

            return GenerateJwtToken(authClaims, rememberMe);
        }

        private string GenerateJwtToken(List<Claim> authClaims, bool rememberMe)
        {
            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

            var expiration = rememberMe
                ? DateTime.Now.AddDays(double.Parse(_configuration["JWT:RememberMeDurationInDays"]))
                : DateTime.Now.AddDays(double.Parse(_configuration["JWT:DurationInDays"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: expiration,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256Signature)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void StoreTokenInCookie(string token, DateTime expiration, HttpContext context)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = expiration
            };

            context.Response.Cookies.Append("yourAppCookie", token, cookieOptions);
        }
    }
}
