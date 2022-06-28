using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestAuthority.Application;
using TestAuthority.Application.CertificateBuilders;
using TestAuthority.Application.CertificateBuilders.CertificateBuilderSteps;
using TestAuthority.Application.Random;
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
        services.AddSingleton<ISignerProvider, RootWithIntermediateCertificateService>();
        services.AddSingleton(x =>
        {
            var logger = x.GetRequiredService<ILogger<Startup>>();
            var signerProvider = x.GetRequiredService<ISignerProvider>();
            var randomService = x.GetRequiredService<IRandomService>();
            var certificate = signerProvider.GetRootCertificate();

            logger.LogInformation("Using root certificate: {NewLine}{Certificate}", Environment.NewLine, certificate.CertificateWithKey?.Certificate);

            return new CertificateAuthorityService(certificate.CertificateWithKey, randomService);
        });
        services.AddCertificateGenerationPipeline();
    }

    /// <summary>
    ///     Register certificate generation pipeline.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection" />.</param>
    public static void AddCertificateGenerationPipeline(this IServiceCollection services)
    {
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, KeyPairGenerationBehaviour>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, CommonNameBehaviour>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, IssuerNameBehaviour>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, SerialNumberBehaviour>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, CertificateValidityBehaviour>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, SubjectAlternativeNameBehaviour>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, BasicConstraintsExtensionBehaviour>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, ExtendedKeyUsageExtensionBehaviour>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, AuthorityKeyIdentifierExtensionBehavior>();
        services.AddScoped<IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>, SignCertificateBehaviour>();
    }
}
