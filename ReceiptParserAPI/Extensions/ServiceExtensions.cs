using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ReceiptParserAPI.Services;
using System.Text;

namespace ReceiptParserAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // --- JWT Ayarlarını Çekme ---
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secret = jwtSettings["Secret"] ?? throw new ArgumentNullException("JWT Secret (Gizli Anahtar) bulunamadı.");

            // --- Authentication Servisini Ekleme ---
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                };
            });

            // --- Authorization Servisini Ekle ---
            services.AddAuthorization(); 
        }
    }
}
