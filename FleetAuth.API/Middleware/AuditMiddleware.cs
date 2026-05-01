using System.Security.Claims;
using FleetAuth.Core.Entities;
using FleetAuth.Infrastructure.Data;

namespace FleetAuth.API.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx, AppDbContext db)
    {
        await _next(ctx);

        var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Endpoint = ctx.Request.Path,
                Method = ctx.Request.Method,
                IpAddress = ctx.Connection.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }
}