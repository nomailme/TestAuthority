using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace TestAuthority.Host;

/// <summary>
///     The one and only.
/// </summary>
public class Program
{
    /// <summary>
    ///     The mainest of them all.
    /// </summary>
    /// <param name="args">Arguments.</param>
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application start-up failed");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    ///     Create a <seecref name="IWebHostBuilder" />.
    /// </summary>
    /// <param name="args">Arguments</param>
    /// <returns>
    ///     <seecref name="IWebHostBuilder" />
    /// </returns>
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console();
            })
            .ConfigureWebHostDefaults(x => x.UseStartup<Startup>())
            .ConfigureAppConfiguration(((context, builder) =>
            {
                builder.AddJsonFile("appsettings.json", true, false);
                builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, false);
            }));
    }
}
