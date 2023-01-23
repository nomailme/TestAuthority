using TestAuthority.Domain.Models;

namespace TestAuthority.Application.CertificateConverters;

public interface ICertificateConverter
{
    Task<byte[]> Convert(CertificateWithKey input, ICertificateConverterOptions? options = null);
}

public interface ICertificateConverterOptions
{
}
