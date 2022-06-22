using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using TestAuthorityCore.Swagger;

namespace TestAuthorityCore.Extensions;

/// <summary>
/// Extension methods for swagger.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Register swagger.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/>.</param>
    public static void AddSwagger(this IServiceCollection services)
    {
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
            var filePath = Path.Combine(AppContext.BaseDirectory, "TestAuthorityCore.xml");
            config.IncludeXmlComments(filePath);
        });
    }
}