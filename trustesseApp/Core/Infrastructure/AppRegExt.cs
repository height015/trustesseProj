using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using trustesseApp.Core.Entities;
using trustesseApp.Core.Infrastructure;
using trustesseApp.Infrastructure.Configuration;
using trustesseApp.Infrastructure.Data;
using trustesseApp.Service;
using trustesseApp.Service.Contracts;

namespace trustesseApp.Infrastructure;


public static class AppRegExt 
{
    public static IServiceCollection AddAppInfrastructure(this WebApplicationBuilder builder)
    {
        ConfigurationManager config = builder.Configuration;
        builder.Services.AddSingleton<IConfiguration>(config);


        builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));
        builder.Services.Configure<AppMailSettings>(builder.Configuration.GetSection("AppMailSettings"));

        builder.Services.AddHttpClient<IEmailApiClientService, EmailEmailApiClientService>();



        builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .EnableSensitiveDataLogging(false));

        builder.Services.AddHttpClient();
        


        builder.Services.AddAuthentication(cfg =>
        {
            cfg.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            cfg.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
            
        })
        .AddJwtBearer(options =>
        {
            var tokenSettings = config?.GetSection("TokenSettings").Get<TokenSettings>();
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Key)),
                ValidateIssuer = true,
                ValidIssuer = tokenSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = tokenSettings.JwtIssuer,
                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TokenValidationParameters
                .DefaultClockSkew,
            };
        }
     );
    builder.Services.AddAuthorization(options =>
    {
    });
    builder.Services.AddIdentityCore<AppUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.SignIn.RequireConfirmedEmail = false;
    }).AddRoles<IdentityRole>()
    .AddRoleManager<RoleManager<IdentityRole>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager<SignInManager<AppUser>>()
    .AddUserManager<UserManager<AppUser>>() 
    .AddDefaultTokenProviders();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins",
            builder =>
            {
                builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
            });
    });
   
    builder.Services.AddControllers();

        return builder.Services;
    }

}

