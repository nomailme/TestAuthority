using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TestAuthorityCore.Swagger
{
    /// <summary>
    /// Filter to enable handling file upload in swagger
    /// </summary>
    public class FormFileSwaggerFilter : IOperationFilter
    {
        private const string formDataMimeType = "multipart/form-data";

        private static readonly string[] formFilePropertyNames =
            typeof(IFormFile).GetTypeInfo().DeclaredProperties.Select(p => p.Name).ToArray();

        /// <summary>
        /// Apply operation.
        /// </summary>
        /// <param name="operation">Operation.</param>
        /// <param name="context">Context.</param>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            IList<IParameter> parameters = operation.Parameters;
            if (parameters == null || parameters.Count == 0)
            {
                return;
            }

            var formFileParameterNames = new List<string>();
            var formFileSubParameterNames = new List<string>();

            foreach (ParameterDescriptor actionParameter in context.ApiDescription.ActionDescriptor.Parameters)
            {
                string[] properties =
                    actionParameter.ParameterType.GetProperties()
                        .Where(p => p.PropertyType == typeof(IFormFile))
                        .Select(p => p.Name)
                        .ToArray();

                if (properties.Length != 0)
                {
                    formFileParameterNames.AddRange(properties);
                    formFileSubParameterNames.AddRange(properties);
                    continue;
                }

                if (actionParameter.ParameterType != typeof(IFormFile))
                {
                    continue;
                }

                formFileParameterNames.Add(actionParameter.Name);
            }

            if (!formFileParameterNames.Any())
            {
                return;
            }

            IList<string> consumes = operation.Consumes;
            consumes.Clear();
            consumes.Add(formDataMimeType);

            foreach (IParameter parameter in parameters.ToArray())
            {
                if (!(parameter is NonBodyParameter) || parameter.In != "formData")
                {
                    continue;
                }

                if (formFileSubParameterNames.Any(p => parameter.Name.StartsWith(p + "."))
                    || formFilePropertyNames.Contains(parameter.Name))
                {
                    parameters.Remove(parameter);
                }
            }

            foreach (string formFileParameter in formFileParameterNames)
            {
                parameters.Add(new NonBodyParameter()
                {
                    Name = formFileParameter,
                    Type = "file",
                    In = "formData"
                });
            }
        }
    }
}
