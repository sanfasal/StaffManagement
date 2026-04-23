using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using StaffManagement.Application.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StaffManagement.API.Swagger;

public class StaffDtoSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(StaffDto))
        {
            return;
        }

        if (!schema.Properties.TryGetValue("gender", out var genderSchema))
        {
            return;
        }

        genderSchema.Default = new OpenApiInteger(1);
        genderSchema.Example = new OpenApiInteger(1);
    }
}
