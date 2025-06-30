// Services/SiteDetayGetir.cs
using Site.Data;
using Site.Models;

namespace Site.Services
{
    public class SiteDetayGetir
    {
        private readonly UygulamaDbContext _db;

        public SiteDetayGetir(UygulamaDbContext db)
        {
            _db = db;
        }

        public string DetayGetir()
        {
            return _db.SiteIsim.FirstOrDefault(s => s.Id == 1)?.Isim ?? "Site";
        }
    }
}
