using Site.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Site.Services;
using Oracle.EntityFrameworkCore;
using Site.Middlewares;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();
        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

        builder.Services.AddDbContext<UygulamaDbContext>(options =>
            options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection"))
        );

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Login";
            });

        builder.Services.AddSingleton<SiteCacheService>(); // 🟢 Site adı için Singleton servis

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(5000);
        });

        var app = builder.Build();

        LoadSiteDetails(app.Services); // 🟢 Site adını başta belleğe al

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        // CUSTOM MIDDLEWARE’LER
        app.UseMiddleware<MaintenanceMiddleware>();
        app.UseMiddleware<IpControlMiddleware>();
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseMiddleware<LogMiddleware>();

        // 🔴 Artık kullanılmayacak:
        // app.UseMiddleware<SiteNameMiddleware>();

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }

    // 🟢 Site adını yükleyen method
    private static void LoadSiteDetails(IServiceProvider services)
    {
        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<UygulamaDbContext>();
            var cache = scope.ServiceProvider.GetRequiredService<SiteCacheService>();

            var siteAdi = db.SiteIsim.FirstOrDefault(s => s.Id == 1)?.Isim ?? "Site";
            cache.SiteAdiniAyarla(siteAdi);
            Console.WriteLine("📌 Site adı yüklendi: " + siteAdi);
        }
    }
}
