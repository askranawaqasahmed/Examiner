using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ideageek.Examiner.Api.Swagger;

/// <summary>
/// Assigns a stable tag to QuestionSheet endpoints so they can be ordered first in Swagger UI.
/// </summary>
public class QuestionSheetTagOperationFilter : IOperationFilter
{
    private const string ControllerName = "QuestionSheet";
    private const string PreferredTag = "01 - Question Sheets";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!context.ApiDescription.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller) ||
            !string.Equals(controller, ControllerName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = PreferredTag } };
    }
}
