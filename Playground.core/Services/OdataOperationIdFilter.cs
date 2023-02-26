namespace Playground.core.Services;

public class OdataOperationIdFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.OperationId == null || !operation.OperationId.Contains("odata/"))
        {
            return;
        }

        operation.OperationId = operation.OperationId.Replace("odata/", string.Empty);
        operation.OperationId = operation.OperationId.Replace("/$count", "Count");
        operation.OperationId = operation.OperationId.Replace("({key})", "ByKey1");
        operation.OperationId = operation.OperationId.Replace("/{key}", "ByKey2");
        operation.OperationId = operation.OperationId.Replace("$metadata", "metadata");
    }
}

public class OdataResponse<T> 
{
    public T Value { get; set; }
}
