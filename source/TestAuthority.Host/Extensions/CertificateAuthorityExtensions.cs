using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestAuthority.Application;
using TestAuthority.Application.Random;
using TestAuthority.Host.Service;

namespace TestAuthority.Host.Extensions;

/// <summary>
///     Extension methods for CA.
/// </summary>
public static class CertificateAuthorityExtensions
{
   /// <summary>
   /// </summary>
   /// <param name="services"></param>
   public static void AddCertificateAuthorityService(this IServiceCollection services)
   {
       services.AddSingleton<ICertificateConverter, CertificateConverterService>();
       services.AddSingleton<RootCertificateService>();
       services.AddSingleton(x =>
       {
           var logger = x.GetRequiredService<ILogger<Startup>>();
           var rootCertificateService = x.GetRequiredService<RootCertificateService>();
           var randomService = x.GetRequiredService<RandomService>();
           var certificate = rootCertificateService.GetRootCertificate();

           logger.LogInformation("Using root certificate: {NewLine}{Certificate}", Environment.NewLine, certificate.Certificate);

           return new CertificateAuthorityService(certificate, randomService);
       });
   }
}