using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using TestAuthority.Host.Swagger;

namespace TestAuthority.Host.Extensions;

/// <summary>
///     Extension methods for swagger.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    ///     Register swagger.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection" />.</param>
    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(
            config =>
            {
                config.DescribeAllParametersInCamelCase();
                config.SwaggerDoc(
                    "v1", new OpenApiInfo
                    {
                        Title = "Personal Signing Center",
                        Version = "v1",
                        Contact = new OpenApiContact { Name = "Iskander Yarmukhametov", Url = new Uri("https://github.com/nomailme/TestAuthority") }
                    });
                config.OperationFilter<AddFileParamTypesOperationFilter>();
                var filePath = Path.Combine(AppContext.BaseDirectory, "TestAuthority.Host.xml");
                config.IncludeXmlComments(filePath);
            });
    }
}
