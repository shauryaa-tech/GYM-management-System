using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GymManagement.Services.Paytm
{
    public static class PaytmChecksumHelper
    {
        public static string GenerateSignature(string bodyJson, string merchantKey)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(
                Encoding.UTF8.GetBytes(bodyJson + merchantKey));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        public static bool VerifySignature(string bodyJson, string merchantKey, string signature)
        {
            if (string.IsNullOrWhiteSpace(signature))
                return false;

            var expected = GenerateSignature(bodyJson, merchantKey);
            return string.Equals(expected, signature, StringComparison.OrdinalIgnoreCase);
        }

        public static string SerializeBody(object body) =>
            JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
    }
}
