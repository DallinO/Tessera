using Aegis.Data;
using Aegis.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Aegis.SwaggerTest;
using Swashbuckle.AspNetCore.Filters;
using Tessera.Models.Authentication;

namespace Aegis.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.SetMinimumLevel(LogLevel.Information);
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
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                options.ExampleFilters(); // Register example filters

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter 'Bearer [jwt]'",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });

                var scheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                options.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, Array.Empty<string>() } });
            });

            builder.Services.AddSwaggerExamplesFromAssemblyOf<CheckInRequestExample>();

            using var loggerFactory = LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Trace).AddConsole());

            var secret = builder.Configuration["JWT:Secret"] ?? throw new InvalidOperationException("Secret not configured");
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ClockSkew = new TimeSpan(0, 0, 5)
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = ctx => LogAttempt(ctx.Request.Headers, "OnChallenge"),
                    OnTokenValidated = ctx => LogAttempt(ctx.Request.Headers, "OnTokenValidated")
                };
            });

            builder.Services.AddScoped<DbContextFactory>();
            builder.Services.AddScoped<TokenService>();
            builder.Services.AddScoped<BookService>();

            // Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowBlazorApp",
                    builder =>
                    {
                        builder.WithOrigins("https://localhost:7094") // The URL of your Blazor app
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

            app.UseHttpsRedirection();
            // Apply CORS policy
            app.UseCors("AllowBlazorApp");


            app.UseRouting();

            // Enable authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();

            Task LogAttempt(IHeaderDictionary headers, string eventType)
            {
                var logger = loggerFactory.CreateLogger<Program>();

                var authorizationHeader = headers["Authorization"].FirstOrDefault();

                if (authorizationHeader is null)
                    logger.LogInformation($"{eventType}. JWT not present");
                else
                {
                    string jwtString = authorizationHeader.Substring("Bearer ".Length);

                    var jwt = new JwtSecurityToken(jwtString);

                    logger.LogInformation($"{eventType}. Expiration: {jwt.ValidTo.ToLongTimeString()}. System time: {DateTime.UtcNow.ToLongTimeString()}");
                }

                return Task.CompletedTask;
            }
        }
    }
}