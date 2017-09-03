using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;

namespace Playground.core.Services
{
    public class CustomSchemaFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Definitions.Add("Custom", new Schema()
            {
                Type = "Custom",
                Properties = new Dictionary<string, Schema>()
                {
                    { "IntProp", context.SchemaRegistry.GetOrRegister(typeof(int)) }
                }
            });
        }
    }
}
