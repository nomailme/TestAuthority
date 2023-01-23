using TestAuthority.Domain.Models;

namespace TestAuthority.Application.CertificateConverters;

public interface ICrlConverter
{
    Task<byte[]> Convert(CrlFileModel input);
}
