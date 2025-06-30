using Site.Models;

namespace Site.Services
{
    public interface IAuthenticationService
    {
        Task<bool> RegisterUserAsync(Kullanici kullanici);
        Task<bool> LoginAsync(HttpContext context, string email, string sifre);
        Task<List<Kullanici>> GetAllUsersAsync();
        Task<Kullanici?> GetUserByIdAsync(int id);
        Task<bool> UpdateUserAsync(Kullanici kullanici);
    }
}
