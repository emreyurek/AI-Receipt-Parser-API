using Microsoft.AspNetCore.Identity;

namespace ReceiptParserAPI.Services
{
    public class PasswordHasherService : IPasswordHasherService
    {
        private readonly IPasswordHasher<object> _passwordHasher = new PasswordHasher<object>();

        public string HashPassword(string password)
        {
            //  Salt ve Hash'i tek bir string'de birleştirir
            return _passwordHasher.HashPassword(null, password);
        }
    }
}
