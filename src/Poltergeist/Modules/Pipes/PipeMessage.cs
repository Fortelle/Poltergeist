using System.Text.Json;
using System.Text.Json.Nodes;

namespace Poltergeist.Modules.Pipes;

public class PipeMessage
{
    public required string Key { get; init; }
    public JsonNode? Value { get; init; }

    public T? As<T>()
    {
        if (Value is not null)
        {
            return JsonSerializer.Deserialize<T>(Value);
        }
        else
        {
            return default;
        }
    }
}
