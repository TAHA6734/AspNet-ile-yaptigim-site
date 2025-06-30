using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Site.Middlewares
{
    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public MaintenanceMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var isMaintenance = _configuration.GetValue<bool>("IsMaintenanceMode");

            if (isMaintenance && !context.Request.Path.StartsWithSegments("/Account"))
            {
                context.Response.StatusCode = 503;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Sistem bakımda. Lütfen daha sonra tekrar deneyiniz.");
                return;
            }

            await _next(context);
        }
    }
}
