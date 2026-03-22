using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using EHRS.Core.Interfaces;

namespace EHRS.Infrastructure.Persistence
{
    public class EncryptedStringConverter : ValueConverter<string, string>
    {
        public EncryptedStringConverter(IEncryptionService encryption)
            : base(
                v => encryption.Encrypt(v ?? string.Empty),
                v => encryption.Decrypt(v ?? string.Empty))
        {
        }
    }
}