using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Domain.Quota;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QuotaCellState
{
    [EnumMember(Value = "open")]
    Open,

    [EnumMember(Value = "closed")]
    Closed
}