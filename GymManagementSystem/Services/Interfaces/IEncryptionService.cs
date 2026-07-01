namespace GymManagement.Services.Interfaces
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
        string MaskSecret(string? value);
    }
}
