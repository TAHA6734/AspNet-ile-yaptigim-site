using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Site.Data;
using System.Threading.Tasks;
using Site.Models;


public class SiteNameMiddleware
{
    private readonly RequestDelegate _next;

    public SiteNameMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UygulamaDbContext db)
    {  
        try
        {
            // DbSet ismi neyse ona göre kullan
            var site = await db.SiteIsim.FirstOrDefaultAsync(s => s.Id == 1);
            if (site != null)
            {  
                context.Items["SiteAdi"] = site.Isim;
            }
        }
        catch (System.Exception ex)
        {  
            // Geliştirme aşamasında loglama için
            System.Console.WriteLine("Middleware hata: " + ex.Message);
        }

        await _next(context);
    }
}
