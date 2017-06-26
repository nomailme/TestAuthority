using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proto;
using TestAuthority.Web.Actors;
using TestAuthority.Web.X509;

namespace TestAuthority.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            Log.SetLoggerFactory(loggerFactory);
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUi();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<RootCertificateManager>();
            services.AddSwaggerGen();

            RegisterActors(services);
            services.AddTransient<ActorManager>();

            // Add framework services.
            services.AddMvc();
        }

        private static void RegisterActors(IServiceCollection services)
        {
            services.AddProtoActor(x =>
            {
                x.RegisterProps<EndpointCertificateIssueActor>(props => props);
                x.RegisterProps<RootCertificateProviderActor>(props => props);
            });
        }
    }
}
