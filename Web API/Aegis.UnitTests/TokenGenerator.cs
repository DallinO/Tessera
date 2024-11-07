using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Aegis.UnitTests
{
    public class TokenGenerator
    {
        public static string GenerateTestJwtToken(string username)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("@Andromeda1789&Sagittarius0476&Centuarus247") ?? throw new InvalidOperationException("Secret not configured"));

            var token = new JwtSecurityToken(
                issuer: "https://localhost",
                audience: "https://localhost",
                expires: DateTime.UtcNow.AddMinutes(10),
                claims: authClaims,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
