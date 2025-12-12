using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ideageek.Examiner.Api.Swagger;

public class FormFileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var formParameters = context.MethodInfo
            .GetParameters()
            .Where(IsMultipartFormParameter)
            .ToList();

        if (!formParameters.Any())
        {
            return;
        }

        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>(),
            Required = new HashSet<string>()
        };

        foreach (var parameter in formParameters)
        {
            var name = parameter.Name ?? "field";
            schema.Properties[name] = BuildSchemaForParameter(parameter);

            if (!parameter.IsOptional && !parameter.HasDefaultValue)
            {
                schema.Required.Add(name);
            }
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = schema
                }
            }
        };
    }

    private static bool IsMultipartFormParameter(ParameterInfo parameter)
    {
        if (parameter.ParameterType == typeof(IFormFile))
        {
            return true;
        }

        return parameter.GetCustomAttributes()
            .Any(attr => attr.GetType() == typeof(FromFormAttribute));
    }

    private static OpenApiSchema BuildSchemaForParameter(ParameterInfo parameter)
    {
        if (parameter.ParameterType == typeof(IFormFile))
        {
            return new OpenApiSchema
            {
                Type = "string",
                Format = "binary",
                Description = "File upload"
            };
        }

        var schema = new OpenApiSchema { Type = MapParameterType(parameter.ParameterType) };

        if (schema.Type == "integer" && parameter.ParameterType == typeof(long))
        {
            schema.Format = "int64";
        }

        if (schema.Type == "number" && parameter.ParameterType == typeof(decimal))
        {
            schema.Format = "decimal";
        }

        return schema;
    }

    private static string MapParameterType(Type type)
    {
        if (type == typeof(string))
        {
            return "string";
        }

        if (type == typeof(bool))
        {
            return "boolean";
        }

        if (type == typeof(int) || type == typeof(long) || type == typeof(short))
        {
            return "integer";
        }

        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
        {
            return "number";
        }

        return "string";
    }
}
