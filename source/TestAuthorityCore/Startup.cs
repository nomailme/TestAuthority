using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using TestAuthorityCore.Service;
using TestAuthorityCore.Swagger;
using TestAuthorityCore.X509;

namespace TestAuthorityCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "Personal Signing Center");
                config.RoutePrefix = "";
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(x => x.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo { Title = "Personal Signing Center", Version = "v1" });
                config.OperationFilter<AddFileParamTypesOperationFilter>();
            });
            services.AddLogging();
            services.AddSingleton<RootCertificateService>();
            services.AddSingleton<RandomService>();
            services.AddSingleton(x =>
            {
                var logger = x.GetRequiredService<ILogger<Startup>>();
                var rootCertificateService = x.GetRequiredService<RootCertificateService>();
                var randomService = x.GetRequiredService<RandomService>();
                CertificateWithKey certificate = rootCertificateService.GetRootCertificate();

                logger.LogInformation($"Using root certificate: {Environment.NewLine}{certificate.Certificate.ToString()}");

                return new CertificateAuthorityService(certificate, randomService);
            });
        }
    }
}
