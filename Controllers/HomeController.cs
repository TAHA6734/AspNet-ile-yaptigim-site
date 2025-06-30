using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Site.Models;
using Microsoft.AspNetCore.Authorization;
using Site.Data;
using System.Security.Claims;

namespace Site.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UygulamaDbContext _db;

        public HomeController(ILogger<HomeController> logger, UygulamaDbContext db)
        {
            _logger = logger;
            _db = db;
        }

     public IActionResult Index()
{
    // Middleware'den gelen SiteAdi değerini alıyoruz
    var siteAdi = HttpContext.Items["SiteAdi"]?.ToString() ?? "Site";
    ViewBag.SiteAdi = siteAdi;

    var userId = User.FindFirstValue("Id");

    if (int.TryParse(userId, out int id))
    {
        var kullanici = _db.Kullanicilar.Find(id);
        return View(kullanici);
    }

    return RedirectToAction("Login", "Account");
}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
