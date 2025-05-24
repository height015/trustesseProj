using System;
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace trustesseApp.Infrastructure;



public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("Trustess", new OpenApiInfo
            {
                Title = "Trustess Volunteer Assessment",
                Version = "v1",
                Description = "Trustess Volunteer .Net C# Backend Opportunity API 😂",
                Contact = new OpenApiContact
                {
                    Name = "Joseph Efunbote (EJ)",
                    Email = "me@josephefunbote.com",
                    Url = new Uri("https://josephefunbote.com")
                },

                License = new OpenApiLicense
                {
                    Name = "Trustesse Team",
                    Url = new Uri("https://trustesse.com")
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            c.EnableAnnotations();

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
            });
        });
        return services;
    }
}

