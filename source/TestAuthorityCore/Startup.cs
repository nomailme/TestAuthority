using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using TestAuthorityCore.Filters;
using TestAuthorityCore.Service;
using TestAuthorityCore.Swagger;
using TestAuthorityCore.X509;

namespace TestAuthorityCore
{
    /// <summary>
    /// Startup class.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="configuration"><seecref name="IConfiguration"/>.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
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

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(x =>
            {
                x.EnableEndpointRouting = false;
                x.Filters.Add<ValidationFilter>();
            })
            .AddJsonOptions(x =>
            {
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                x.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            })
            .AddFluentValidation(config =>
            {
                config.RegisterValidatorsFromAssemblyContaining<Startup>();
            });

            services.AddSwaggerGen(config =>
            {
                config.DescribeAllParametersInCamelCase();
                config.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Personal Signing Center",
                    Version = "v1",
                    Contact = new OpenApiContact
                    {
                        Name = "Iskander Yarmukhametov",
                        Url = new Uri("https://github.com/nomailme/TestAuthority")
                    }
                });
                config.OperationFilter<AddFileParamTypesOperationFilter>();
                var filePath = Path.Combine(System.AppContext.BaseDirectory, "TestAuthorityCore.xml");
                config.IncludeXmlComments(filePath);

            });
            services.AddLogging();
            services.AddSingleton<RootCertificateService>();
            services.AddSingleton<RandomService>();
            services.AddSingleton<ICertificateConverter, CertificateConverterService>();
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
