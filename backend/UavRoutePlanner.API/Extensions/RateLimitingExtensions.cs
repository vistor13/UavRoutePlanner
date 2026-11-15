using System.Net.Mime;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.Api.Extensions;

public static class RateLimitingExtensions
{
    public const string AuthPolicy = "auth";

    private const int AuthRequestsPerWindow = 10;
    private static readonly TimeSpan AuthWindow = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(AuthPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = AuthRequestsPerWindow,
                        Window = AuthWindow,
                        QueueLimit = 0
                    }));

            options.OnRejected = async (context, token) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                await context.HttpContext.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = Errors.TooManyRequests
                    },
                    options: null,
                    contentType: MediaTypeNames.Application.ProblemJson,
                    cancellationToken: token);
            };
        });

        return services;
    }
}
