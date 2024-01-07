using System.Text.Json.Serialization;

namespace Playground.core.Services;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Company))]
public partial class JsonSerializerSourceGenerator : JsonSerializerContext
{
}
