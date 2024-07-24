#define _DEV

using Aegis.Data;
using Tessera.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Aegis.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#if _DEV
builder.Services.AddDbContext<TesseraDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TestDbConnection")));
#else
builder.Services.AddDbContext<TesseraDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
#endif

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

builder.Services.AddScoped<DbContextFactory>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<BookService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication(); // Add this to enable authentication middleware

app.UseAuthorization();

app.MapControllers();

app.Run();
