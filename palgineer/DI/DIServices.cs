using Microsoft.Extensions.Options;
using palgineer.services;
using palgineer.models2;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.FileProviders;


namespace palgineer.DI
{
    public static class DIServices
    {
        
        public static void AddApplicationServices( IServiceCollection services,IConfiguration configuration)
        {
            services.AddScoped<FileServices> ();
            services.Configure<MongoDBSettings>(
                configuration.GetSection("MongoDBSettings"));
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            services.AddScoped<EngineerService> ();

            var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var allowedOrigins = new[] { "http://localhost:5174","http://localhost:5173" };
            services.AddCors(o => o.AddPolicy("DefaultCorsPolicy", p =>
            {
                p.WithOrigins(allowedOrigins)
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials();
            }));


            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                                                  Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ClockSkew = TimeSpan.Zero
                };
            });
            

        }



    }
}
