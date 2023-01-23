using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using TestAuthority.Application;
using TestAuthority.Application.CertificateBuilders;
using TestAuthority.Application.CertificateBuilders.CertificateBuilderSteps;
using TestAuthority.Application.CertificateConverters;
using TestAuthority.Application.CrlBuilders;
using TestAuthority.Application.CrlBuilders.CrlBuilderSteps;
using TestAuthority.Application.SignatureFactoryProviders;
using TestAuthority.Application.Store;
using TestAuthority.Domain.CertificateConverters;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;
using TestAuthority.Domain.Store;

namespace TestAuthority.Infrastructure.Extensions;

/// <summary>
///     Extension methods for CA.
/// </summary>
public static class CertificateAuthorityExtensions
{
    /// <summary>
    ///     Register services required for CA.
    /// </summary>
    /// <param name="services"></param>
    public static void AddCertificateAuthorityService(this IServiceCollection services)
    {
        services.AddSingleton<ICertificateStore, PfxCertificateStore>();
        services.AddSingleton<ICertificateConverterService, CertificateConverterService>();
        services.AddSingleton<ISignerProvider, RootWithIntermediateCertificateProvider>();

        services.AddSingleton<ISignatureFactoryProvider, RsaSignatureFactoryProvider>();
    }

    /// <summary>
    ///     Register certificate generation pipeline.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection" />.</param>
    public static void AddCertificateGenerationPipeline(this IServiceCollection services)
    {
        services.AddTransient<IRequestPreProcessor<CertificateBuilderRequest>, KeyPairGenerationBehaviour>();

        services.AddTransient<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, CommonNameBehaviour>();
        services.AddTransient<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, IssuerNameBehaviour>();
        services.AddTransient<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, SerialNumberBehaviour>();
        services.AddTransient<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, CertificateValidityBehaviour>();
        services.AddTransient<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, SubjectAlternativeNameBehaviour>();
        services.AddTransient<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, CrlDistributionPointExtensionBehaviour>();
        services.AddTransient<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, BasicConstraintsExtensionBehaviour>();
        services.AddTransient<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, ExtendedKeyUsageExtensionBehaviour>();
        services.AddTransient<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, AuthorityKeyIdentifierExtensionBehavior>();
        services.AddTransient<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, SignCertificateBehaviour>();

        services.AddTransient<IRequestPostProcessor<CertificateBuilderRequest, CertificateWithKey>, CertificateLoggingPostProcessor>();
    }

    /// <summary>
    ///     Register Crl generation pipeline.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection" />.</param>
    public static void AddCrlGenerationPipeline(this IServiceCollection services)
    {
        services.AddTransient<IPipelineBehavior<CrlBuilderRequest, CrlFileModel>, GenerateCrlBehaviour>();
    }
}
