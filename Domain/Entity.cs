using Domain.Audit;
using System.Text.Json.Serialization;

namespace Domain;

public abstract class Entity(string userId) : SystemInformation(userId)
{
    public abstract string DocumentType { get; }

    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("_etag")]
    public string Etag { get; set; }
}