using Microsoft.AspNetCore.DataProtection;

namespace GymManagement.Services
{
    public class EncryptionService : Interfaces.IEncryptionService
    {
        private readonly IDataProtector _protector;

        public EncryptionService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("GymManagement.PaymentGateway.MerchantKey.v1");
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            return _protector.Protect(plainText);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            return _protector.Unprotect(cipherText);
        }

        public string MaskSecret(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "••••••••";

            if (value.Length <= 4)
                return "••••";

            return new string('•', value.Length - 4) + value[^4..];
        }
    }
}
