using Swashbuckle.AspNetCore.SwaggerGen;

namespace Playground.core.Services;

public class OdataDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (var path in swaggerDoc.Paths.Values)
        {
            foreach (var operation in path.Operations)
            {
                if (operation.Value.OperationId == null)
                {
                    continue;
                }

                if (operation.Value.OperationId.StartsWith(operation.Key.ToString()))
                {
                    continue;
                }

                operation.Value.OperationId = $"{operation.Key}{operation.Value.OperationId}"; 
            }
        }
    }
}
