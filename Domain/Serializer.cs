using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Cosmos;

namespace Domain;

public static class MainStreetSerializer
{
    public static JsonSerializerOptions Options => new()
    {
        AllowTrailingCommas = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static CosmosLinqSerializerOptions LinqSerializerOptions => new()
    {
        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
    };

    public static string Serialize<TEntity>(this TEntity entity) =>
        JsonSerializer.Serialize(entity, Options);

    public static Task SerializeAsync<TEntity>(this TEntity entity, Stream stream) =>
        JsonSerializer.SerializeAsync(stream, entity, Options);

    public static TEntity Deserialize<TEntity>(string serializedEntity) =>
        JsonSerializer.Deserialize<TEntity>(serializedEntity, Options);

    public static TEntity Deserialize<TEntity>(Stream streamedEntity) =>
        JsonSerializer.Deserialize<TEntity>(streamedEntity, Options);

    public static ValueTask<TEntity> DeserializeAsync<TEntity>(Stream stream) =>
        JsonSerializer.DeserializeAsync<TEntity>(stream, Options);

}