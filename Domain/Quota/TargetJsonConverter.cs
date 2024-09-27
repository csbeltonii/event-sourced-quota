using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.Quota;

public class TargetJsonConverter : JsonConverter<Target>
{
    public override Target Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var jsonObject = jsonDoc.RootElement;

        if (!jsonObject.TryGetProperty("type", out var typeElement))
        {
            throw new JsonException("Expected a type property");
        }

        var targetType = typeElement.GetString();

        return targetType switch
        {
            "Infinite" => JsonSerializer.Deserialize<InfiniteTarget>(jsonObject.GetRawText(), options),
            "Strict"   => JsonSerializer.Deserialize<StrictTarget>(jsonObject.GetRawText(), options),
            _          => throw new JsonException($"Unknown TargetType: {targetType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, Target value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, value.GetType(), options);
}