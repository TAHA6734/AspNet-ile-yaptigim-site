using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Site.Data;
using Site.Models;

namespace Site.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UygulamaDbContext _db;

        public AuthenticationService(UygulamaDbContext db)
        {
            _db = db;
        }

        public async Task<bool> RegisterUserAsync(Kullanici kullanici)
        {
            kullanici.Sifre = BCrypt.Net.BCrypt.HashPassword(kullanici.Sifre);
            _db.Kullanicilar.Add(kullanici);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LoginAsync(HttpContext context, string email, string sifre)
        {
            var kullanici = await _db.Kullanicilar.FirstOrDefaultAsync(k => k.Email == email);

            if (kullanici != null && BCrypt.Net.BCrypt.Verify(sifre, kullanici.Sifre))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, kullanici.Isim + " " + kullanici.Soyisim),
                    new Claim(ClaimTypes.Email, kullanici.Email),
                    new Claim("Id", kullanici.Id.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                return true;
            }

            return false;
        }

        public async Task<List<Kullanici>> GetAllUsersAsync()
        {
            return await _db.Kullanicilar.ToListAsync();
        }

        public async Task<Kullanici?> GetUserByIdAsync(int id)
        {
            return await _db.Kullanicilar.FindAsync(id);
        }

        public async Task<bool> UpdateUserAsync(Kullanici kullanici)
        {
            var mevcut = await _db.Kullanicilar.FindAsync(kullanici.Id);
            if (mevcut == null)
                return false;

            mevcut.Isim = kullanici.Isim;
            mevcut.Soyisim = kullanici.Soyisim;
            mevcut.Email = kullanici.Email;
            mevcut.Yas = kullanici.Yas;

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
