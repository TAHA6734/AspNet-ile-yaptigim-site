using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Site.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {DateTime.Now} - {ex.Message}");
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Bir hata oluştu. Daha sonra tekrar deneyiniz.");
            }
        }
    }
}
