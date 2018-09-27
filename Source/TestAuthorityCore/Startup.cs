using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using TestAuthorityCore.Service;
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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(config => config.SwaggerEndpoint("/swagger/v1/swagger.json", "Personal Signing Center"));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSwaggerGen(config => config.SwaggerDoc("v1", new Info { Title = "Personal Signing Center", Version = "v2" }));
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
