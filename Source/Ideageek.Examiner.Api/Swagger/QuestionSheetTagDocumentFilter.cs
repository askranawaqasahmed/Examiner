using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ideageek.Examiner.Api.Swagger;

/// <summary>
/// Reorders Swagger tags so QuestionSheet appears first.
/// </summary>
public class QuestionSheetTagDocumentFilter : IDocumentFilter
{
    private const string PreferredTag = "01 - Question Sheets";

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (swaggerDoc.Tags is null || swaggerDoc.Tags.Count == 0)
        {
            swaggerDoc.Tags = new List<OpenApiTag> { new OpenApiTag { Name = PreferredTag } };
            return;
        }

        var preferred = swaggerDoc.Tags.FirstOrDefault(t =>
            string.Equals(t.Name, PreferredTag, StringComparison.OrdinalIgnoreCase))
            ?? new OpenApiTag { Name = PreferredTag };

        var remaining = swaggerDoc.Tags
            .Where(t => !string.Equals(t.Name, PreferredTag, StringComparison.OrdinalIgnoreCase))
            .ToList();

        swaggerDoc.Tags = new List<OpenApiTag> { preferred };
        foreach (var tag in remaining)
        {
            swaggerDoc.Tags.Add(tag);
        }
    }
}
