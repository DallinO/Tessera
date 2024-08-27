using Aegis.Data;
using Tessera.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Aegis.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TesseraDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<Scribe, IdentityRole>(options =>
{
    // Configure identity options if needed
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    // Add more options as needed
})
    .AddEntityFrameworkStores<TesseraDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "http://localhost",
            ValidAudience = "http://localhost",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("@Andromeda1789&Sagittarius0476&Centuarus247")),
            ClockSkew = TimeSpan.Zero // Optional: Adjust if needed
        };
    });

builder.Services.AddScoped<DbContextFactory>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<BookService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp",
        builder =>
        {
            builder.WithOrigins("https://localhost:7021") // The URL of your Blazor app
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials(); // Include if credentials are needed
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply CORS policy
app.UseCors("AllowBlazorApp");

app.UseHttpsRedirection();

app.UseRouting();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();