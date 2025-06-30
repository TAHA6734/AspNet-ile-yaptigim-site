// Services/SiteCacheService.cs
namespace Site.Services
{
    public class SiteCacheService
    {
        public string SiteAdi { get; private set; } = "Site";

        public void SiteAdiniAyarla(string isim)
        {
            if (!string.IsNullOrWhiteSpace(isim))
            {
                SiteAdi = isim;
            }
        }
    }
}
