using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using exam_postly_api.CleanupServices;
using exam_postly_api.Interfaces;
using exam_postly_api.Services;
using exam_postly_api.Utilities;


namespace exam_postly_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string productionCors = "ProductionCorsPolicy";
            string developmentCors = "DevelopmentCorsPolicy";

            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    //ClockSkew = TimeSpan.Zero,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(IdentityData.AdminPolicyName, policy => policy.RequireRole("admin"));
            });

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDBContext>(options => options.UseNpgsql(connectionString));

            builder.Services.AddHostedService<RefreshTokenCleanupService>();
            builder.Services.AddHostedService<RestoreTokenCleanupService>();
            builder.Services.AddHostedService<VerifyTokenCleanupService>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<ChatService>();

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options => 
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddSignalR();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(productionCors, builder =>
                {
                    builder.WithOrigins("https://urakanow.github.io")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(developmentCors, builder =>
                {
                    builder.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            
            app.UseRouting();

            if (app.Environment.IsDevelopment())
            {
                app.UseCors(developmentCors);
                //app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            }
            else
            {
                app.UseCors(productionCors);
            }

            if (app.Environment.IsProduction())
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseWebSockets();
            app.MapHub<ChatHub>("/api/chatHub"); 

            app.MapControllers();

            app.Run();
        }
    }
}
