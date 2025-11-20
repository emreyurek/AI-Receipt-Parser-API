namespace ReceiptParserAPI.Services
{
    public interface IPasswordHasherService
    {
        string HashPassword(string password);
    }
}
