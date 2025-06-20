using Microsoft.AspNetCore.Mvc;//Web uygulamalrinda controller ve view gibi MVC icerir
using Site.Data;
using Site.Models;
using Microsoft.AspNetCore.Authentication;// 1 bu ikiside kullanici kimlik dogrulama islemlerinde kullanilir
using Microsoft.AspNetCore.Authentication.Cookies;// 2 cerz tabanli oturum yonetimi
using System.Security.Claims;//kimlik bilgilerni tasinak icin 
using Microsoft.EntityFrameworkCore;//VT islemlerini sorgular async 
using Microsoft.AspNetCore.Authorization;  // Yetkilendirme Belirli sayfalara sadece giris yapmis kullanicilar erisebilir

namespace Site.Controllers
{
    public class AccountController : Controller
    {
        private readonly UygulamaDbContext _db;//readonly sadece constructer disardan degistirilemez 

        public AccountController(UygulamaDbContext db)
        {
            _db = db;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()//Kullanicita bis redister sayfasi gosterirli 
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(Kullanici kullanici)
        {
            if (ModelState.IsValid)
            {
                kullanici.Sifre = BCrypt.Net.BCrypt.HashPassword(kullanici.Sifre);

                // Kullanıcıyı veritabanına kaydet
                _db.Kullanicilar.Add(kullanici);
                await _db.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            return View(kullanici);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Sifre)
        {
            var kullanici = await _db.Kullanicilar.FirstOrDefaultAsync(x => x.Email == Email);

            if (kullanici != null && BCrypt.Net.BCrypt.Verify(Sifre, kullanici.Sifre))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, kullanici.Isim + " " + kullanici.Soyisim),
                    new Claim(ClaimTypes.Email, kullanici.Email),
                    new Claim("Id", kullanici.Id.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Mesaj = "Hatalı e-posta ya da şifre!";
            return View();
        }

        // /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // BURAYA EKLEDİM: Kullanıcı listesini getiren action
        [Authorize]
        public async Task<IActionResult> Kullanicilar()
        {
            var kullanicilar = await _db.Kullanicilar.ToListAsync();
            return View(kullanicilar);
        }
    }
}
