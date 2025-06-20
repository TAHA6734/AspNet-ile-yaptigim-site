using Microsoft.EntityFrameworkCore;
using Site.Models; // <-- Model dosyan buradaysa

namespace Site.Data
{
    public class UygulamaDbContext : DbContext
    {
        public UygulamaDbContext(DbContextOptions<UygulamaDbContext> options) : base(options)
        {
        }

        public DbSet<Kullanici> Kullanicilar { get; set; }
    }
}
