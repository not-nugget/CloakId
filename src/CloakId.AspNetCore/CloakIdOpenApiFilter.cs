using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CloakId.AspNetCore;

/// <summary>
/// OpenAPI operation filter that modifies parameter schemas for CloakId-marked parameters.
/// This ensures that the API documentation correctly shows string parameters instead of numeric types
/// for parameters marked with the [Cloak] attribute.
/// </summary>
public class CloakIdOpenApiFilter : IOperationFilter
{
    /// <summary>
    /// Applies the CloakId parameter transformations to the OpenAPI operation specification.
    /// </summary>
    /// <param name="operation">The OpenAPI operation to modify.</param>
    /// <param name="context">The operation filter context containing method information.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null) return;

        // Get the method info from the context
        var methodInfo = context.MethodInfo;
        var parameters = methodInfo.GetParameters();

        for (int i = 0; i < parameters.Length && i < operation.Parameters.Count; i++)
        {
            var parameterInfo = parameters[i];
            var openApiParameter = operation.Parameters.FirstOrDefault(p =>
                string.Equals(p.Name, parameterInfo.Name, StringComparison.OrdinalIgnoreCase));

            if (openApiParameter == null) continue;

            // Check if the parameter has the Cloak attribute
            var cloakAttribute = parameterInfo.GetCustomAttribute<CloakAttribute>();
            if (cloakAttribute != null && parameterInfo.ParameterType.IsNumericType())
            {
                // Modify the parameter to be a string type in the OpenAPI spec
                openApiParameter.Schema = new OpenApiSchema
                {
                    Type = "string",
                    Format = null, // Remove any numeric format
                    Description = openApiParameter.Schema?.Description ??
                        $"Encoded string representation of a {parameterInfo.ParameterType.GetFriendlyTypeName()} value. " +
                        "This parameter accepts encoded string values (e.g., 'A6das1') rather than raw numeric values."
                };

                // Add example if the original had one
                if (openApiParameter.Example != null)
                {
                    openApiParameter.Example = new Microsoft.OpenApi.Any.OpenApiString("A6das1");
                }

                // Update extensions to indicate this is a CloakId parameter
                openApiParameter.Extensions["x-cloakid"] = new Microsoft.OpenApi.Any.OpenApiBoolean(true);
                openApiParameter.Extensions["x-cloakid-original-type"] =
                    new Microsoft.OpenApi.Any.OpenApiString(parameterInfo.ParameterType.GetFriendlyTypeName());
            }
        }
    }
}
