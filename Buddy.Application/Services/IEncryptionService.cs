using System.Threading.Tasks;

namespace Buddy.Application.Services
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
