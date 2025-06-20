namespace Site.Models
{
    public class Kullanici
    {
        public int Id { get; set; }
        public int Yas { get; set; }
        public string Isim { get; set; } = string.Empty;

        public string Soyisim { get; set; } = string.Empty;

        public string Sifre { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

    }
}