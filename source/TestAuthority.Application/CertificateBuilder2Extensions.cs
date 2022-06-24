using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using TestAuthority.Domain.Services;

namespace TestAuthority.Application;

/// <summary>
///     Extensions for certificates.
/// </summary>
public static class CertificateBuilder2Extensions
{
    /// <summary>
    ///     Get <seecref name="X509Name" /> from name components.
    /// </summary>
    /// <param name="nameComponents">Nmae components.</param>
    /// <returns>Result.</returns>
    public static X509Name GetX509Name(Dictionary<DerObjectIdentifier, string> nameComponents)
    {
        var keys = nameComponents.Keys.ToArray();
        var values = nameComponents.Values.ToArray();

        return new X509Name(keys, values);
    }

    /// <summary>
    ///     Set authority key identifer.
    /// </summary>
    /// <param name="builder"><seecref name="ICertificateBuilder" />.</param>
    /// <param name="authorityKeyPair"></param>
    /// <returns><seecref name="ICertificateBuilder" />.</returns>
    public static ICertificateBuilder WithAuthorityKeyIdentifier(this ICertificateBuilder builder,
        AsymmetricCipherKeyPair authorityKeyPair)
    {
        var subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(authorityKeyPair.Public);
        builder.AddExtension(X509Extensions.AuthorityKeyIdentifier.Id, new AuthorityKeyIdentifier(subjectPublicKeyInfo));

        return builder;
    }

    /// <summary>
    ///     Set extended key usage(EKU) extension.
    /// </summary>
    /// <param name="builder"><seecref name="ICertificateBuilder" />.</param>
    /// <returns><seecref name="ICertificateBuilder" />.</returns>
    public static ICertificateBuilder WithExtendedKeyUsage(this ICertificateBuilder builder)
    {
        var extendedKeyUsage = new ExtendedKeyUsage(KeyPurposeID.IdKPClientAuth, KeyPurposeID.IdKPServerAuth);
        builder.AddExtension(X509Extensions.ExtendedKeyUsage.Id, extendedKeyUsage);

        return builder;
    }

    /// <summary>
    ///     Set Subject Alternative Name extension.
    /// </summary>
    /// <param name="builder"><seecref name="ICertificateBuilder" />.</param>
    /// <param name="hostnames">Hostnames and domain names.</param>
    /// <param name="ipAddresses">IP addresses.</param>
    /// <returns><seecref name="ICertificateBuilder" />.</returns>
    public static ICertificateBuilder WithSubjectAlternativeName(this ICertificateBuilder builder,
        List<string>? hostnames = null,
        List<string>? ipAddresses = null)
    {
        var result = new List<Asn1Encodable>();

        hostnames?.Select(x => new GeneralName(GeneralName.DnsName, x))
            .Select(x => x as Asn1Encodable)
            .ToList()
            .ForEach(result.Add);

        ipAddresses?.Select(x => new GeneralName(GeneralName.IPAddress, x))
            .Select(x => x as Asn1Encodable)
            .ToList()
            .ForEach(result.Add);

        if (result.Any() == false) return builder;

        var extension = new DerSequence(result.ToArray());
        builder.AddExtension(X509Extensions.SubjectAlternativeName.Id, extension);
        return builder;
    }

    /// <summary>
    ///     Set subject common name.
    /// </summary>
    /// <param name="builder"><seecref name="ICertificateBuilder" />.</param>
    /// <param name="commonName">Common name</param>
    /// <returns><seecref name="ICertificateBuilder" />.</returns>
    public static ICertificateBuilder WithSubjectCommonName(this ICertificateBuilder builder, string? commonName)
    {
        ArgumentNullException.ThrowIfNull(commonName);

        var subjectComponents = new Dictionary<DerObjectIdentifier, string>
        {
            { X509Name.CN, commonName }
        };

        var subject = GetX509Name(subjectComponents);

        builder.WithSubject(subject);

        return builder;
    }

    /// <summary>
    ///     Set subject key identifier extension.
    /// </summary>
    /// <param name="builder"><seecref name="ICertificateBuilder" />.</param>
    /// <returns><seecref name="ICertificateBuilder" />.</returns>
    public static ICertificateBuilder WithSubjectKeyIdentifier(this ICertificateBuilder builder)
    {
        var subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(builder.GetPublicKeyInfo());
        builder.AddExtension(X509Extensions.SubjectKeyIdentifier.Id, new SubjectKeyIdentifier(subjectPublicKeyInfo));
        return builder;
    }
}