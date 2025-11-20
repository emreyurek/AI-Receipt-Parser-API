namespace ReceiptParserAPI.Services
{
    public interface IAuthService
    {
        // Kullanıcı girişini yönetir. Başarılıysa Token ve UserId döner.
        Task<(string Token, int UserId)?> LoginAsync(string username, string password);

        // Kullanıcı kaydını yönetir.
        Task<bool> RegisterAsync(string username, string email, string password);
    }
}
