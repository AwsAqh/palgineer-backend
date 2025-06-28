using Microsoft.Extensions.Options;
using palgineer.services;
using palgineer.models2;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.FileProviders;
using CloudinaryDotNet;


namespace palgineer.DI
{
    public static class DIServices
    {
        
        public static void AddApplicationServices( IServiceCollection services,IConfiguration configuration)
        {
            services.AddScoped<FileServices> ();
            services.Configure<MongoDBSettings>(
                configuration.GetSection("MongoDBSettings"));
            services.AddSingleton<IMongoClient>(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
                return new MongoClient(opts.ConnectionString);
            });

            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            services.AddScoped<EngineerService> ();

            var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var dev = configuration["AllowedOrigins:Dev"];
            var staging = configuration["AllowedOrigins:Staging"];
            var prod = configuration["AllowedOrigins:Production"];

            // build the list, include localhost unconditionally, then filter out blanks
            var allowedOrigins = new[]
            {
    "https://palgineer-1u9p1s7sj-awsaqhs-projects.vercel.app",
    dev,
    staging,
    prod
}
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .ToArray();

            services.AddCors(o =>
                o.AddPolicy("DefaultCorsPolicy", p =>
                    p.WithOrigins(allowedOrigins)
                     .AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials()
                )
            );


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


            services.Configure<CloudinaryConfig>(
  configuration.GetSection("Cloudinary")
);

            // register the Cloudinary client
            services.AddSingleton(sp => {
                var opts = sp.GetRequiredService<IOptions<CloudinaryConfig>>().Value;
                var account = new Account(opts.CloudName, opts.ApiKey, opts.ApiSecret);
                return new Cloudinary(account) { Api = { Secure = true } };
            });

            // register your upload service
            services.AddScoped<CloudinaryService>();


        }



    }
}
