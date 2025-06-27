using palgineer.models2;
using palgineer.services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using palgineer.DI;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);


DIServices.AddApplicationServices(builder.Services, builder.Configuration);



builder.Services.AddAuthorization();

// 4) MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5) Build the app
var app = builder.Build();

// 6) Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseCors("DefaultCorsPolicy");

app.UseHttpsRedirection();

// **IMPORTANT**: run authentication *before* authorization!
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    // only in production/Render
    builder.WebHost.UseUrls($"http://*:{port}");
}
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


app.Run();
