using System;
using System.Threading.RateLimiting;
using trustesseApp.Core.Factories;
using trustesseApp.Core.Repository;
using trustesseApp.Service;
using trustesseApp.Service.Contracts;

namespace trustesseApp.Infrastructure;


public static class ServiceRegExt 
{
    public static IServiceCollection AddServiceExt(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IOtpService, OtpRecordRepository>();

        services.AddScoped<ITemplateReader, TemplateReader>();

        services.AddScoped<IAccountFactory, AccountFactory>();


        services.AddMemoryCache();
        services.AddHttpContextAccessor();

        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                if (!httpContext.Request.Path.StartsWithSegments("/api"))
                {
                    return RateLimitPartition.GetNoLimiter("not-an-api-request");
                }

                var userAgent = httpContext.Request.Headers.UserAgent.ToString();
                var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                bool isBot = !string.IsNullOrEmpty(userAgent) &&
                            (userAgent.Contains("Googlebot", StringComparison.OrdinalIgnoreCase) ||
                            userAgent.Contains("Bingbot", StringComparison.OrdinalIgnoreCase) ||
                            userAgent.Contains("Slurp", StringComparison.OrdinalIgnoreCase) ||
                            userAgent.Contains("DuckDuckBot", StringComparison.OrdinalIgnoreCase));

                int permitLimit = isBot ? 50 : 40;

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: remoteIp,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        AutoReplenishment = true
                    });
            });

            options.RejectionStatusCode = 429;

            options.OnRejected = (context, token) =>
            {
                var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();
                var ua = context.HttpContext.Request.Headers.UserAgent.ToString();
                Console.WriteLine($"⚠️ Rate limit hit: IP = {ip}, UA = {ua}");
                return ValueTask.CompletedTask;
            };
        });

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
        });



        return services;


    }

}
