using System.ComponentModel.DataAnnotations;

namespace Site.Models
{
    public class Kullanici
    {
        public int Id { get; set; }

        public int Yas { get; set; }

        [MaxLength(50)]
        public string Isim { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Soyisim { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Sifre { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
    }
}
