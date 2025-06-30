using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Site.Middlewares
{
    public class LogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string logFolder = "Logs";
        private readonly string logFile = "Logs/requests.log";

        public LogMiddleware(RequestDelegate next)
        {
            _next = next;
            // Logs klasörü yoksa oluştur
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var logLine = $"[LOG] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {context.Connection.RemoteIpAddress} - {context.Request.Method} {context.Request.Path}";

            Console.WriteLine(logLine);

            // Logu dosyaya ekle (async)
            await File.AppendAllTextAsync(logFile, logLine + Environment.NewLine);

            await _next(context);
        }
    }
}
