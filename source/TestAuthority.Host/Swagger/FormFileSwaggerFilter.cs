using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TestAuthority.Host.Swagger;

/// <summary>
///     AddSwaggerFileUploadButton.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AddSwaggerFileUploadButtonAttribute : Attribute
{
}

/// <summary>
///     Filter to enable handling file upload in swagger
/// </summary>
public class AddFileParamTypesOperationFilter : IOperationFilter
{
    // ReSharper disable once InconsistentNaming
    private static readonly string[] fileParameters = { "ContentType", "ContentDisposition", "Headers", "Length", "Name", "FileName" };

    /// <summary>
    ///     Apply filter.
    /// </summary>
    /// <param name="operation"><seecref name="OpenApiOperation" />.</param>
    /// <param name="context"><seecref name="OperationFilterContext" />.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var operationHasFileUploadButton = context.MethodInfo.GetCustomAttributes(true).OfType<AddSwaggerFileUploadButtonAttribute>().Any();

        if (!operationHasFileUploadButton)
        {
            return;
        }
        RemoveExistingFileParameters(operation.Parameters);
        operation.RequestBody = new OpenApiRequestBody
        {
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties =
                        {
                            ["file"] = new OpenApiSchema
                            {
                                Description = "Select file",
                                Type = "string",
                                Format = "binary"
                            }
                        }
                    }
                }
            }
        };

    }

    private static void RemoveExistingFileParameters(ICollection<OpenApiParameter> operationParameters)
    {
        foreach (var parameter in operationParameters.Where(p => p.In == 0 && fileParameters.Contains(p.Name)).ToList())
        {
            operationParameters.Remove(parameter);
        }
    }
}
