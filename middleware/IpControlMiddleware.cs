using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace Site.Middlewares
{
    public class IpControlMiddleware
    {
        private readonly RequestDelegate _next;

        // Engellenmiş IP listesi
        private readonly List<string> bannedIps = new List<string>
        {
                   // localhost (isteğe bağlı engellenebilir)
        };

        public IpControlMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            if (ipAddress != null && bannedIps.Contains(ipAddress))
            {
                Console.WriteLine($"[BLOCKED] {ipAddress} erişimi engellendi.");
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Erişiminiz engellendi.");
                return;
            }

            await _next(context);
        }
    }
}
