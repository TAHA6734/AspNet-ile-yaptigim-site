using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Site.Models;
using System.Text;
using AuthService = Site.Services.IAuthenticationService;

namespace Site.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;

        public AccountController(AuthService authService)
        {
            _authService = authService;
        }

        // Kayıt Ol - GET
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Kayıt Ol - POST
        [HttpPost]
        public async Task<IActionResult> Register(Kullanici kullanici)
        {
            var result = await _authService.RegisterUserAsync(kullanici);
            if (result)
                return RedirectToAction("Login");

            return View(kullanici);
        }

        // Giriş Yap - GET
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Giriş Yap - POST
        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Sifre)
        {
            var success = await _authService.LoginAsync(HttpContext, Email, Sifre);
            if (success)
                return RedirectToAction("Index", "Home");

            ViewBag.Mesaj = "Hatalı e-posta ya da şifre!";
            return View();
        }

        // Çıkış Yap
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // Kullanıcı Listesi (Giriş zorunlu)
        [Authorize]
        public async Task<IActionResult> Kullanicilar()
        {
            var list = await _authService.GetAllUsersAsync();
            return View(list);
        }

        // Kullanıcıyı Düzenle - GET
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var kullanici = await _authService.GetUserByIdAsync(id);
            if (kullanici == null)
                return NotFound();

            return View(kullanici);
        }

        // Kullanıcıyı Düzenle - POST
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(Kullanici kullanici)
        {
            if (!ModelState.IsValid)
                return View(kullanici);

            var success = await _authService.UpdateUserAsync(kullanici);
            if (!success)
                return NotFound();

            return RedirectToAction("Kullanicilar");
        }   

        // API: Kullanıcı JSON verisi al
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserJson(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Json(user);
        }

        // API: Kullanıcıyı JSON ile güncelle
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateUserJson([FromBody] Kullanici kullanici)
        {
            var result = await _authService.UpdateUserAsync(kullanici);
            if (!result)
                return BadRequest();

            return Ok();
        }

        // CSV Export
        [Authorize]
        public async Task<IActionResult> ExportCsv()
        {
            var kullanicilar = await _authService.GetAllUsersAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id,Isim,Soyisim,Email,Yas");

            foreach (var k in kullanicilar)
            {
                sb.AppendLine($"{k.Id},{k.Isim},{k.Soyisim},{k.Email},{k.Yas}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "kullanicilar.csv");
        }
    }
}
