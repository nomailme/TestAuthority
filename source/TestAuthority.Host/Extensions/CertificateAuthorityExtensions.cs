using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using TestAuthority.Application;
using TestAuthority.Application.CertificateBuilders;
using TestAuthority.Application.CertificateBuilders.CertificateBuilderSteps;
using TestAuthority.Application.CrlBuilders;
using TestAuthority.Application.CrlBuilders.CrlBuilderSteps;
using TestAuthority.Application.Random;
using TestAuthority.Application.SignatureFactoryProviders;
using TestAuthority.Application.Store;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;
using TestAuthority.Domain.Store;
using TestAuthority.Host.Service;

namespace TestAuthority.Host.Extensions;

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
        services.AddSingleton<IRandomService, RandomService>();
        services.AddSingleton<ICertificateConverter, CertificateConverterService>();
        services.AddSingleton<ISignerProvider, RootWithIntermediateCertificateProvider>();
        services.AddCertificateGenerationPipeline();
        services.AddCrlGenerationPipeline();
        services.AddSingleton<ISignatureFactoryProvider, GostSignatureFactoryProvider>();
    }

    /// <summary>
    ///     Register certificate generation pipeline.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection" />.</param>
    private static void AddCertificateGenerationPipeline(this IServiceCollection services)
    {
        services.AddScoped<IRequestPreProcessor<CertificateBuilderRequest>, KeyPairGenerationBehaviour>();

        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, CommonNameBehaviour>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, IssuerNameBehaviour>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, SerialNumberBehaviour>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, CertificateValidityBehaviour>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, SubjectAlternativeNameBehaviour>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, BasicConstraintsExtensionBehaviour>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, ExtendedKeyUsageExtensionBehaviour>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, AuthorityKeyIdentifierExtensionBehavior>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, SignCertificateBehaviour>();

        services.AddScoped<IRequestPostProcessor<CertificateBuilderRequest, CertificateWithKey>, CertificateLoggingPostProcessor>();
    }

    /// <summary>
    ///     Register Crl generation pipeline.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection" />.</param>
    private static void AddCrlGenerationPipeline(this IServiceCollection services)
    {
        services.AddScoped<IPipelineBehavior<CrlBuilderRequest, CrlFileModel>, GenerateCrlBehaviour>();
    }
}
